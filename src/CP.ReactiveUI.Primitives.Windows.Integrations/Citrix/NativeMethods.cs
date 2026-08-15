// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix;

/// <summary>Citrix WFAPI native methods.</summary>
#if NETFRAMEWORK
internal static class NativeMethods
#else
internal static partial class NativeMethods
#endif
{
    /// <summary>The WFAPI library name.</summary>
    private const string WfApiLibraryName = "WFAPI";

    /// <summary>The WFFreeMemory export pointer.</summary>
    private const string FreeMemoryExportName = nameof(WFFreeMemory);

    /// <summary>Coordinates WFAPI module and export initialization.</summary>
    private static readonly object NativeLibraryGate = new();

    /// <summary>Loads native modules and exports.</summary>
    private static INativeLibrary _nativeLibrary = NativeLibraryAdapter.Instance;

    /// <summary>The cached WFAPI module handle.</summary>
    private static IntPtr _citrixApiModule;

    /// <summary>The cached WFFreeMemory export pointer.</summary>
    private static IntPtr _freeMemoryExport;

    /// <summary>Loads native modules and retrieves their exports.</summary>
    internal interface INativeLibrary
    {
        /// <summary>Loads a native module.</summary>
        /// <param name="libraryName">The native library name.</param>
        /// <returns>The native module handle.</returns>
        IntPtr Load(string libraryName);

        /// <summary>Gets an export from a native module.</summary>
        /// <param name="module">The native module handle.</param>
        /// <param name="exportName">The export name.</param>
        /// <returns>The native export pointer.</returns>
        IntPtr GetExport(IntPtr module, string exportName);
    }

    /// <summary>Queries session information from the Citrix WFAPI.</summary>
    /// <param name="serverHandle">The server handle, or zero for the current server.</param>
    /// <param name="sessionId">The session ID, or -1 for the current session.</param>
    /// <param name="infoType">The information type to retrieve.</param>
    /// <param name="buffer">The returned buffer pointer.</param>
    /// <param name="bytesReturned">The number of bytes returned.</param>
    /// <returns><see langword="true"/> when the query succeeds.</returns>
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("WFAPI", EntryPoint = "WFQuerySessionInformationW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static
#if NETFRAMEWORK
        extern
#else
        partial
#endif
        bool WFQuerySessionInformation(
        IntPtr serverHandle,
        int sessionId,
        InfoClasses infoType,
        out IntPtr buffer,
        out int bytesReturned);

    /// <summary>Waits for a Citrix session event.</summary>
    /// <param name="serverHandle">The server handle, or zero for the current server.</param>
    /// <param name="eventMask">The event mask.</param>
    /// <param name="eventFlags">The event flags that occurred.</param>
    /// <returns><see langword="true"/> when an event is returned.</returns>
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("WFAPI")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static
#if NETFRAMEWORK
        extern
#else
        partial
#endif
        bool WFWaitSystemEvent(IntPtr serverHandle, EventMask eventMask, out EventMask eventFlags);

    /// <summary>Frees memory allocated by WFQuerySessionInformation.</summary>
    /// <param name="memory">The memory pointer to free.</param>
    internal static unsafe void WFFreeMemory(IntPtr memory)
    {
        var freeMemory = (delegate* unmanaged[Stdcall]<IntPtr, void>)GetFreeMemoryExport();
        freeMemory(memory);
    }

    /// <summary>Replaces the native-library adapter and clears cached WFAPI handles.</summary>
    /// <param name="nativeLibrary">The replacement native-library adapter.</param>
    /// <returns>The replaced native-library adapter.</returns>
    internal static INativeLibrary ExchangeNativeLibrary(INativeLibrary nativeLibrary)
    {
        Throw.IfNull(nativeLibrary);
        lock (NativeLibraryGate)
        {
            var previousNativeLibrary = _nativeLibrary;
            _nativeLibrary = nativeLibrary;
            _citrixApiModule = IntPtr.Zero;
            _freeMemoryExport = IntPtr.Zero;
            return previousNativeLibrary;
        }
    }

    /// <summary>Gets the cached WFFreeMemory export pointer.</summary>
    /// <returns>The WFFreeMemory export pointer.</returns>
    private static IntPtr GetFreeMemoryExport()
    {
        lock (NativeLibraryGate)
        {
            if (_freeMemoryExport == IntPtr.Zero)
            {
                _citrixApiModule = _nativeLibrary.Load(WfApiLibraryName);
                _freeMemoryExport = _nativeLibrary.GetExport(_citrixApiModule, FreeMemoryExportName);
            }

            return _freeMemoryExport;
        }
    }

    /// <summary>NativeLibrary-backed adapter for loading modules and exports.</summary>
    internal sealed class NativeLibraryAdapter : INativeLibrary
    {
        /// <summary>The shared native-library adapter instance.</summary>
        internal static readonly INativeLibrary Instance = new NativeLibraryAdapter();

        /// <summary>Initializes a new instance of the <see cref="NativeLibraryAdapter"/> class.</summary>
        private NativeLibraryAdapter()
        {
        }

        /// <inheritdoc />
        public IntPtr Load(string libraryName) => NativeLibrary.Load(libraryName);

        /// <inheritdoc />
        public IntPtr GetExport(IntPtr module, string exportName) => NativeLibrary.GetExport(module, exportName);
    }
}
