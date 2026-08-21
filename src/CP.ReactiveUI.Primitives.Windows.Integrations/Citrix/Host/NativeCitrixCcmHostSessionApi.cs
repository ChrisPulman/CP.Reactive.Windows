// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Host;

/// <summary>Production Citrix CCM host-session adapter loaded from the installed Workspace SDK.</summary>
public sealed class NativeCitrixCcmHostSessionApi : ICitrixCcmHostSessionApi, IDisposable
{
    /// <summary>Loads dependencies from the target DLL directory and the default safe locations.</summary>
    private const uint LoadLibrarySearchFlags = 0x00000100U | 0x00001000U;

    /// <summary>The Citrix installation registry key.</summary>
    private const string CitrixInstallKey = @"Software\Citrix\Install\ICA Client";

    /// <summary>Serializes native operations and disposal.</summary>
    private readonly Lock _gate = new();

    /// <summary>The CCM uninitialize export.</summary>
    private readonly CcmUninitializeDelegate _uninitialize;

    /// <summary>The CCM session-info export.</summary>
    private readonly CcmGetSessionInfoDelegate _getSessionInfo;

    /// <summary>The CCM disconnect export.</summary>
    private readonly CcmSessionOperationDelegate _disconnectSession;

    /// <summary>The CCM logoff export.</summary>
    private readonly CcmSessionOperationDelegate _logoffSession;

    /// <summary>The CCM session-memory release export.</summary>
    private readonly CcmFreeIcaSessionDelegate _freeIcaSession;

    /// <summary>The loaded CCM module.</summary>
    private IntPtr _module;

    /// <summary>Tracks whether CCM initialization completed.</summary>
    private bool _initialized;

    /// <summary>Initializes a new instance of the <see cref="NativeCitrixCcmHostSessionApi"/> class from the registered Citrix Workspace installation.</summary>
    public NativeCitrixCcmHostSessionApi()
        : this(ResolveDefaultSdkPath(), AppDomain.CurrentDomain.FriendlyName)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="NativeCitrixCcmHostSessionApi"/> class from a specific SDK module.</summary>
    /// <param name="sdkPath">The absolute path to CCMSDK.dll or CCMSDK64.dll.</param>
    /// <param name="connectionName">The application connection name supplied to CCMInitialize.</param>
    public NativeCitrixCcmHostSessionApi(string sdkPath, string connectionName)
    {
        Throw.IfNull(sdkPath);
        Throw.IfNull(connectionName);
        var fullPath = Path.GetFullPath(sdkPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("The Citrix CCM SDK module was not found.", fullPath);
        }

        _module = NativeMethods.LoadLibraryEx(fullPath, IntPtr.Zero, LoadLibrarySearchFlags);
        if (_module == IntPtr.Zero)
        {
            throw new NativeWin32Exception(Marshal.GetLastWin32Error(), "The Citrix CCM SDK module could not be loaded.");
        }

        try
        {
            var initialize = Resolve<CcmInitializeDelegate>("CCMInitialize");
            _uninitialize = Resolve<CcmUninitializeDelegate>("CCMUninitialize");
            _getSessionInfo = Resolve<CcmGetSessionInfoDelegate>("CCMGetSessionInfo");
            _disconnectSession = Resolve<CcmSessionOperationDelegate>("CCMDisconnectSession");
            _logoffSession = Resolve<CcmSessionOperationDelegate>("CCMLogoffSession");
            _freeIcaSession = Resolve<CcmFreeIcaSessionDelegate>("CCMFreeICASession");

            var result = initialize(connectionName);
            if (result != CcmHostResultCodes.Success)
            {
                throw new InvalidOperationException($"CCMInitialize failed with result code {result}.");
            }

            _initialized = true;
        }
        catch
        {
            ReleaseModule();
            throw;
        }
    }

    /// <summary>CCMInitialize export.</summary>
    /// <param name="connectionName">The application connection name.</param>
    /// <returns>The CCM result code.</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private delegate int CcmInitializeDelegate([MarshalAs(UnmanagedType.LPStr)] string connectionName);

    /// <summary>CCMUninitialize export.</summary>
    /// <returns>The CCM result code.</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int CcmUninitializeDelegate();

    /// <summary>CCMGetSessionInfo export.</summary>
    /// <param name="sessionId">The CCM session identifier.</param>
    /// <param name="session">The allocated session structure.</param>
    /// <returns>The CCM result code.</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int CcmGetSessionInfoDelegate(int sessionId, out IntPtr session);

    /// <summary>CCM single-session operation export.</summary>
    /// <param name="sessionId">The CCM session identifier.</param>
    /// <returns>The CCM result code.</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int CcmSessionOperationDelegate(int sessionId);

    /// <summary>CCMFreeICASession export.</summary>
    /// <param name="count">The number of structures.</param>
    /// <param name="session">The native session pointer.</param>
    /// <returns>The CCM result code.</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int CcmFreeIcaSessionDelegate(uint count, IntPtr session);

    /// <summary>Resolves the installed architecture-matched Citrix CCM SDK path.</summary>
    /// <returns>The absolute SDK module path.</returns>
    public static string ResolveDefaultSdkPath()
    {
        var moduleName = Environment.Is64BitProcess ? "CCMSDK64.dll" : "CCMSDK.dll";
        var views = Environment.Is64BitOperatingSystem
            ? new[] { RegistryView.Registry64, RegistryView.Registry32 }
            : new[] { RegistryView.Registry32 };

        foreach (var hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
        {
            foreach (var view in views)
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                using var installKey = baseKey.OpenSubKey(CitrixInstallKey, writable: false);
                if (installKey?.GetValue("InstallFolder") is string installFolder && !string.IsNullOrWhiteSpace(installFolder))
                {
                    var candidate = Path.GetFullPath(Path.Combine(installFolder, moduleName));
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
            }
        }

        throw new FileNotFoundException($"The installed Citrix Workspace {moduleName} module could not be located.");
    }

    /// <inheritdoc />
    public CcmHostSessionInformationResult GetSessionInfo(int sessionId)
    {
        lock (_gate)
        {
            ThrowIfDisposed();
            var result = _getSessionInfo(sessionId, out var sessionPointer);
            if (sessionPointer == IntPtr.Zero)
            {
                return new(sessionId, result, default, HasSessionInformation: false);
            }

            try
            {
                if (result != CcmHostResultCodes.Success)
                {
                    return new(sessionId, result, default, HasSessionInformation: false);
                }

                var native = Marshal.PtrToStructure<CcmIcaSession>(sessionPointer);
                return new(sessionId, result, native.ToManaged(), HasSessionInformation: true);
            }
            finally
            {
                _ = _freeIcaSession(1U, sessionPointer);
            }
        }
    }

    /// <inheritdoc />
    public CcmHostOperationResult DisconnectSession(int sessionId)
    {
        lock (_gate)
        {
            ThrowIfDisposed();
            return new(sessionId, _disconnectSession(sessionId));
        }
    }

    /// <inheritdoc />
    public CcmHostOperationResult LogoffSession(int sessionId)
    {
        lock (_gate)
        {
            ThrowIfDisposed();
            return new(sessionId, _logoffSession(sessionId));
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_gate)
        {
            if (_module == IntPtr.Zero)
            {
                return;
            }

            if (_initialized)
            {
                _ = _uninitialize();
                _initialized = false;
            }

            ReleaseModule();
        }
    }

    /// <summary>Resolves a required CCM export.</summary>
    /// <typeparam name="TDelegate">The unmanaged delegate type.</typeparam>
    /// <param name="name">The export name.</param>
    /// <returns>The resolved delegate.</returns>
    private TDelegate Resolve<TDelegate>(string name)
        where TDelegate : Delegate
    {
        var address = NativeMethods.GetProcAddress(_module, name);
        return address == IntPtr.Zero
            ? throw new EntryPointNotFoundException($"The Citrix CCM SDK export '{name}' was not found.")
            : Marshal.GetDelegateForFunctionPointer<TDelegate>(address);
    }

    /// <summary>Releases the loaded module.</summary>
    private void ReleaseModule()
    {
        if (_module != IntPtr.Zero)
        {
            _ = NativeMethods.FreeLibrary(_module);
            _module = IntPtr.Zero;
        }
    }

    /// <summary>Throws when the adapter has been disposed.</summary>
    private void ThrowIfDisposed()
    {
#if NET8_0_OR_GREATER
        ObjectDisposedException.ThrowIf(_module == IntPtr.Zero, this);
#else
        if (_module == IntPtr.Zero)
        {
            throw new ObjectDisposedException(nameof(NativeCitrixCcmHostSessionApi));
        }
#endif
    }

    /// <summary>The native CCM_ICASession structure.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private readonly struct CcmIcaSession
    {
        /// <summary>The native session identifier.</summary>
        private readonly int _sessionId;

        /// <summary>The native connection identifier.</summary>
        private readonly int _connectionId;

        /// <summary>The native friendly-name pointer.</summary>
        private readonly IntPtr _friendlyName;

        /// <summary>The native non-seamless title pointer.</summary>
        private readonly IntPtr _nonSeamlessAppTitle;

        /// <summary>The native full-screen flag.</summary>
        private readonly uint _isFullScreen;

        /// <summary>The native SSL flag.</summary>
        private readonly uint _ssl;

        /// <summary>The native encryption-level pointer.</summary>
        private readonly IntPtr _encryptionLevel;

        /// <summary>The native engine-version pointer.</summary>
        private readonly IntPtr _engineVersion;

        /// <summary>The native server-name pointer.</summary>
        private readonly IntPtr _serverName;

        /// <summary>The native user-name pointer.</summary>
        private readonly IntPtr _userName;

        /// <summary>The native domain-name pointer.</summary>
        private readonly IntPtr _domainName;

        /// <summary>The native received frame count.</summary>
        private readonly uint _rxFrameCount;

        /// <summary>The native transmitted frame count.</summary>
        private readonly uint _transmittedFrameCount;

        /// <summary>The native received byte count.</summary>
        private readonly uint _rxByteCount;

        /// <summary>The native transmitted byte count.</summary>
        private readonly uint _transmittedByteCount;

        /// <summary>The native received frame error count.</summary>
        private readonly uint _rxFrameErrorCount;

        /// <summary>The native transmitted frame error count.</summary>
        private readonly uint _transmittedFrameErrorCount;

        /// <summary>The native seamless-mode flag.</summary>
        private readonly uint _seamlessMode;

        /// <summary>The native zero-latency flag.</summary>
        private readonly uint _zeroLatencyMode;

        /// <summary>The native Common Gateway Protocol flag.</summary>
        private readonly uint _cgp;

        /// <summary>The native SpeedBrowse flag.</summary>
        private readonly uint _speedBrowseEnabled;

        /// <summary>The native last latency.</summary>
        private readonly uint _lastLatency;

        /// <summary>The native average latency.</summary>
        private readonly uint _averageLatency;

        /// <summary>The native round-trip deviation.</summary>
        private readonly uint _roundTripDeviation;

        /// <summary>The native horizontal resolution.</summary>
        private readonly uint _horizontalResolution;

        /// <summary>The native vertical resolution.</summary>
        private readonly uint _verticalResolution;

        /// <summary>The native color depth.</summary>
        private readonly uint _colorDepth;

        /// <summary>The native audio flag.</summary>
        private readonly uint _audioEnabled;

        /// <summary>The native PDA redirection flag.</summary>
        private readonly uint _pdaEnabled;

        /// <summary>The native TWAIN redirection flag.</summary>
        private readonly uint _twnEnabled;

        /// <summary>The native Plug and Play redirection flag.</summary>
        private readonly uint _pnpEnabled;

        /// <summary>Converts the native structure to its immutable managed representation.</summary>
        /// <returns>The managed session information.</returns>
        internal CcmHostSessionInformation ToManaged() =>
            new(
                _sessionId,
                _connectionId,
                ReadString(_friendlyName),
                ReadString(_nonSeamlessAppTitle),
                _isFullScreen != 0U,
                _ssl != 0U,
                ReadString(_encryptionLevel),
                ReadString(_engineVersion),
                ReadString(_serverName),
                ReadString(_userName),
                ReadString(_domainName),
                _rxFrameCount,
                _transmittedFrameCount,
                _rxByteCount,
                _transmittedByteCount,
                _rxFrameErrorCount,
                _transmittedFrameErrorCount,
                _seamlessMode != 0U,
                _zeroLatencyMode != 0U,
                _cgp != 0U,
                _speedBrowseEnabled != 0U,
                _lastLatency,
                _averageLatency,
                _roundTripDeviation,
                _horizontalResolution,
                _verticalResolution,
                _colorDepth,
                _audioEnabled != 0U,
                _pdaEnabled != 0U,
                _twnEnabled != 0U,
                _pnpEnabled != 0U);

        /// <summary>Reads an optional native CCM string.</summary>
        /// <param name="value">The native string pointer.</param>
        /// <returns>The managed string.</returns>
        private static string ReadString(IntPtr value) =>
            value == IntPtr.Zero ? string.Empty : Marshal.PtrToStringAnsi(value) ?? string.Empty;
    }
}
