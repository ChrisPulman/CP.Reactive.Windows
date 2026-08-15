// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix;

/// <summary>Helper class for the WinFrame API, which is used by Citrix XenApp and XenDesktop.</summary>
public static class WinFrame
{
    /// <summary>The current session identifier.</summary>
    private const int CurrentSession = -1;

    /// <summary>The log source for Citrix API diagnostics.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(WinFrame));

    /// <summary>The current server handle.</summary>
    private static readonly IntPtr CurrentServer = IntPtr.Zero;

    /// <summary>The native API used to perform WinFrame calls.</summary>
    private static IWinFrameApi _api = new NativeWinFrameApi();

    /// <summary>Gets a value indicating whether WinFrame, the API for Citrix, is available.</summary>
    public static bool IsAvailabe
    {
        get
        {
            try
            {
                _ = QuerySessionConnectState();
                return true;
            }
            catch (Exception ex)
            {
                Log.WarnFormat("Couldn't load WFAPI.DLL, this only means that the process is not running on Citrix and could be okay. Error: {0}", ex.Message);
            }

            return false;
        }
    }

    /// <summary>Retrieves the IP address of the client PC.</summary>
    /// <returns>The IP address.</returns>
    public static string GetClientIpAddress() => QuerySessionStructure<ClientAddress>(InfoClasses.ClientAddress).IpAddress;

    /// <summary>Retrieves the name of the client PC.</summary>
    /// <returns>The host name of the client.</returns>
    public static string GetClientName() => QuerySessionInformation(InfoClasses.ClientName);

    /// <summary>Retrieves the connection state from WFQuerySessionInformation.</summary>
    /// <returns>The optional connection state.</returns>
    public static ConnectStates? QuerySessionConnectState()
    {
        if (!_api.QuerySessionInformation(CurrentServer, CurrentSession, InfoClasses.ConnectState, out var state, out _))
        {
            return null;
        }

        try
        {
            return (ConnectStates)Marshal.ReadInt32(state);
        }
        finally
        {
            _api.FreeMemory(state);
        }
    }

    /// <summary>Retrieves a string value from WFQuerySessionInformation.</summary>
    /// <param name="infoClass">The information class to retrieve.</param>
    /// <returns>The retrieved string value.</returns>
    public static string QuerySessionInformation(InfoClasses infoClass)
    {
        if (!_api.QuerySessionInformation(CurrentServer, CurrentSession, infoClass, out var address, out _))
        {
            return null;
        }

        try
        {
            return Marshal.PtrToStringAuto(address);
        }
        finally
        {
            _api.FreeMemory(address);
        }
    }

    /// <summary>Waits for a Citrix session event before returning.</summary>
    /// <param name="eventMask">The event mask to wait for.</param>
    /// <returns>The events that happened.</returns>
    public static EventMask WaitSystemEvent(EventMask eventMask)
    {
        if (_api.WaitSystemEvent(CurrentServer, eventMask, out var result))
        {
            return result;
        }

        throw new Win32Exception();
    }

    /// <summary>Replaces the native API and returns the replaced adapter.</summary>
    /// <param name="api">The replacement native API adapter.</param>
    /// <returns>The replaced native API adapter.</returns>
    internal static IWinFrameApi ExchangeApi(IWinFrameApi api)
    {
        Throw.IfNull(api);
        var previousApi = _api;
        _api = api;
        return previousApi;
    }

    /// <summary>Retrieves the specified struct.</summary>
    /// <typeparam name="T">The type of the struct to return.</typeparam>
    /// <param name="infoClass">The information class to retrieve.</param>
    /// <returns>The struct of type <typeparamref name="T"/>.</returns>
    private static T QuerySessionStructure<T>(InfoClasses infoClass)
        where T : struct
    {
        if (!_api.QuerySessionInformation(CurrentServer, CurrentSession, infoClass, out var address, out _))
        {
            return default;
        }

        try
        {
            return Marshal.PtrToStructure<T>(address);
        }
        finally
        {
            _api.FreeMemory(address);
        }
    }

    /// <summary>Managed adapter for the WFAPI exports.</summary>
    internal sealed class NativeWinFrameApi : IWinFrameApi
    {
        /// <summary>The native session-information query.</summary>
        private readonly QuerySessionInformationDelegate _querySessionInformation;

        /// <summary>The native session-event wait.</summary>
        private readonly WaitSystemEventDelegate _waitSystemEvent;

        /// <summary>The native memory release action.</summary>
        private readonly Action<IntPtr> _freeMemory;

        /// <summary>Initializes a new instance of the <see cref="NativeWinFrameApi" /> class.</summary>
        public NativeWinFrameApi()
            : this(NativeMethods.WFQuerySessionInformation, NativeMethods.WFWaitSystemEvent, NativeMethods.WFFreeMemory)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="NativeWinFrameApi" /> class.</summary>
        /// <param name="querySessionInformation">The session-information query delegate.</param>
        /// <param name="waitSystemEvent">The session-event wait delegate.</param>
        /// <param name="freeMemory">The memory release action.</param>
        internal NativeWinFrameApi(
            QuerySessionInformationDelegate querySessionInformation,
            WaitSystemEventDelegate waitSystemEvent,
            Action<IntPtr> freeMemory)
        {
            Throw.IfNull(querySessionInformation);
            Throw.IfNull(waitSystemEvent);
            Throw.IfNull(freeMemory);
            _querySessionInformation = querySessionInformation;
            _waitSystemEvent = waitSystemEvent;
            _freeMemory = freeMemory;
        }

        /// <inheritdoc />
        public bool QuerySessionInformation(
            IntPtr serverHandle,
            int sessionId,
            InfoClasses infoType,
            out IntPtr buffer,
            out int bytesReturned) =>
            _querySessionInformation(serverHandle, sessionId, infoType, out buffer, out bytesReturned);

        /// <inheritdoc />
        public bool WaitSystemEvent(IntPtr serverHandle, EventMask eventMask, out EventMask eventFlags) =>
            _waitSystemEvent(serverHandle, eventMask, out eventFlags);

        /// <inheritdoc />
        public void FreeMemory(IntPtr memory) => _freeMemory(memory);
    }
}
