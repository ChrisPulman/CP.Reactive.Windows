// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Primitives.Disposables;
using ReactiveUI.Primitives.Signals;

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Host;

/// <summary>Reactive composition helpers for Citrix host session management.</summary>
public static class CitrixHostSessionManagement
{
    /// <summary>Retrieves session information through the installed Citrix CCM SDK.</summary>
    /// <param name="sessionId">The CCM session identifier.</param>
    /// <returns>An observable that emits one session-information result and completes.</returns>
    public static IObservable<CcmHostSessionInformationResult> CCMGetSessionInformation(int sessionId) =>
        Signal.CreateWithState<CcmHostSessionInformationResult, int>(
            sessionId,
            SubscribeNativeSessionInformation);

    /// <summary>Retrieves session information using CCMGetSessionInfo semantics.</summary>
    /// <param name="sessionId">The CCM session identifier.</param>
    /// <param name="ccmApi">The CCM host-session adapter.</param>
    /// <returns>An observable that emits one session-information result and completes.</returns>
    public static IObservable<CcmHostSessionInformationResult> CCMGetSessionInformation(
        int sessionId,
        ICitrixCcmHostSessionApi ccmApi)
    {
        Throw.IfNull(ccmApi);
        return Signal.CreateWithState(
            new(
                sessionId,
                ccmApi),
            static (
                CcmSessionInformationObservation state,
                IObserver<CcmHostSessionInformationResult> observer) =>
                state.Subscribe(observer));
    }

    /// <summary>Disconnects a running session using CCMDisconnectSession semantics.</summary>
    /// <param name="sessionId">The CCM session identifier.</param>
    /// <param name="ccmApi">The CCM host-session adapter.</param>
    /// <returns>An observable that emits one operation result and completes.</returns>
    public static IObservable<CcmHostOperationResult> CCMDisconnectSession(
        int sessionId,
        ICitrixCcmHostSessionApi ccmApi)
    {
        Throw.IfNull(ccmApi);
        return Signal.CreateWithState(
            new(
                sessionId,
                ccmApi.DisconnectSession),
            static (
                CcmOperationObservation state,
                IObserver<CcmHostOperationResult> observer) =>
                state.Subscribe(observer));
    }

    /// <summary>Disconnects a running session through the installed Citrix CCM SDK.</summary>
    /// <param name="sessionId">The CCM session identifier.</param>
    /// <returns>An observable that emits one operation result and completes.</returns>
    public static IObservable<CcmHostOperationResult> CCMDisconnectSession(int sessionId) =>
        Signal.CreateWithState<CcmHostOperationResult, int>(
            sessionId,
            static (state, observer) => SubscribeNativeOperation(state, logoff: false, observer));

    /// <summary>Logs off a running session using CCMLogoffSession semantics.</summary>
    /// <param name="sessionId">The CCM session identifier.</param>
    /// <param name="ccmApi">The CCM host-session adapter.</param>
    /// <returns>An observable that emits one operation result and completes.</returns>
    public static IObservable<CcmHostOperationResult> CCMLogoffSession(
        int sessionId,
        ICitrixCcmHostSessionApi ccmApi)
    {
        Throw.IfNull(ccmApi);
        return Signal.CreateWithState(
            new(
                sessionId,
                ccmApi.LogoffSession),
            static (
                CcmOperationObservation state,
                IObserver<CcmHostOperationResult> observer) =>
                state.Subscribe(observer));
    }

    /// <summary>Logs off a running session through the installed Citrix CCM SDK.</summary>
    /// <param name="sessionId">The CCM session identifier.</param>
    /// <returns>An observable that emits one operation result and completes.</returns>
    public static IObservable<CcmHostOperationResult> CCMLogoffSession(int sessionId) =>
        Signal.CreateWithState<CcmHostOperationResult, int>(
            sessionId,
            static (state, observer) => SubscribeNativeOperation(state, logoff: true, observer));

    /// <summary>Registers a window for WTS session notifications using the native WTS API.</summary>
    /// <param name="windowHandle">The window handle to register.</param>
    /// <param name="scope">The notification scope.</param>
    /// <returns>An observable whose subscription owns the native registration lifetime.</returns>
    public static IObservable<WtsSessionNotificationRegistration> WTSRegisterSessionNotification(
        IntPtr windowHandle,
        WtsSessionNotificationScope scope) =>
        WTSRegisterSessionNotification(
            windowHandle,
            scope,
            NativeWtsSessionNotificationApi.Instance);

    /// <summary>Registers a window for WTS session notifications.</summary>
    /// <param name="windowHandle">The window handle to register.</param>
    /// <param name="scope">The notification scope.</param>
    /// <param name="wtsApi">The WTS session-notification adapter.</param>
    /// <returns>An observable whose subscription owns the registration lifetime.</returns>
    public static IObservable<WtsSessionNotificationRegistration> WTSRegisterSessionNotification(
        IntPtr windowHandle,
        WtsSessionNotificationScope scope,
        IWtsSessionNotificationApi wtsApi)
    {
        Throw.IfNull(wtsApi);
        return Signal.CreateWithState(
            new(
                windowHandle,
                scope,
                wtsApi),
            static (
                WtsSessionNotificationObservation state,
                IObserver<WtsSessionNotificationRegistration> observer) =>
                state.Subscribe(observer));
    }

    /// <summary>Runs one native CCM session-information operation.</summary>
    /// <param name="sessionId">The CCM session identifier.</param>
    /// <param name="observer">The result observer.</param>
    /// <returns>The completed subscription.</returns>
    private static IDisposable SubscribeNativeSessionInformation(
        int sessionId,
        IObserver<CcmHostSessionInformationResult> observer)
    {
        try
        {
            using var ccmApi = new NativeCitrixCcmHostSessionApi();
            observer.OnNext(ccmApi.GetSessionInfo(sessionId));
            observer.OnCompleted();
        }
        catch (Exception error)
        {
            observer.OnError(error);
        }

        return Scope.Create(static () => { });
    }

    /// <summary>Runs one native CCM disconnect or logoff operation.</summary>
    /// <param name="sessionId">The CCM session identifier.</param>
    /// <param name="logoff">Whether to log off instead of disconnecting.</param>
    /// <param name="observer">The result observer.</param>
    /// <returns>The completed subscription.</returns>
    private static IDisposable SubscribeNativeOperation(
        int sessionId,
        bool logoff,
        IObserver<CcmHostOperationResult> observer)
    {
        try
        {
            using var ccmApi = new NativeCitrixCcmHostSessionApi();
            observer.OnNext(logoff ? ccmApi.LogoffSession(sessionId) : ccmApi.DisconnectSession(sessionId));
            observer.OnCompleted();
        }
        catch (Exception error)
        {
            observer.OnError(error);
        }

        return Scope.Create(static () => { });
    }

    /// <summary>CCM session-information observation state.</summary>
    private sealed class CcmSessionInformationObservation
    {
        /// <summary>The CCM session identifier.</summary>
        private readonly int _sessionId;

        /// <summary>The CCM host-session adapter.</summary>
        private readonly ICitrixCcmHostSessionApi _ccmApi;

        /// <summary>Initializes a new instance of the <see cref="CcmSessionInformationObservation"/> class.</summary>
        /// <param name="sessionId">The CCM session identifier.</param>
        /// <param name="ccmApi">The CCM host-session adapter.</param>
        internal CcmSessionInformationObservation(
            int sessionId,
            ICitrixCcmHostSessionApi ccmApi)
        {
            _sessionId = sessionId;
            _ccmApi = ccmApi;
        }

        /// <summary>Subscribes to the single-result operation.</summary>
        /// <param name="observer">The observer.</param>
        /// <returns>The subscription.</returns>
        internal IDisposable Subscribe(
            IObserver<CcmHostSessionInformationResult> observer)
        {
            try
            {
                observer.OnNext(_ccmApi.GetSessionInfo(_sessionId));
                observer.OnCompleted();
            }
            catch (Exception error)
            {
                observer.OnError(error);
            }

            return Scope.Create(static () => { });
        }
    }

    /// <summary>CCM single-operation observation state.</summary>
    private sealed class CcmOperationObservation
    {
        /// <summary>The CCM session identifier.</summary>
        private readonly int _sessionId;

        /// <summary>The CCM operation.</summary>
        private readonly Func<int, CcmHostOperationResult> _operation;

        /// <summary>Initializes a new instance of the <see cref="CcmOperationObservation"/> class.</summary>
        /// <param name="sessionId">The CCM session identifier.</param>
        /// <param name="operation">The CCM operation.</param>
        internal CcmOperationObservation(
            int sessionId,
            Func<int, CcmHostOperationResult> operation)
        {
            _sessionId = sessionId;
            _operation = operation;
        }

        /// <summary>Subscribes to the single-result operation.</summary>
        /// <param name="observer">The observer.</param>
        /// <returns>The subscription.</returns>
        internal IDisposable Subscribe(
            IObserver<CcmHostOperationResult> observer)
        {
            try
            {
                observer.OnNext(_operation(_sessionId));
                observer.OnCompleted();
            }
            catch (Exception error)
            {
                observer.OnError(error);
            }

            return Scope.Create(static () => { });
        }
    }

    /// <summary>WTS registration observation state.</summary>
    private sealed class WtsSessionNotificationObservation
    {
        /// <summary>The window handle.</summary>
        private readonly IntPtr _windowHandle;

        /// <summary>The notification scope.</summary>
        private readonly WtsSessionNotificationScope _scope;

        /// <summary>The WTS adapter.</summary>
        private readonly IWtsSessionNotificationApi _wtsApi;

        /// <summary>Initializes a new instance of the <see cref="WtsSessionNotificationObservation"/> class.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="scope">The notification scope.</param>
        /// <param name="wtsApi">The WTS adapter.</param>
        internal WtsSessionNotificationObservation(
            IntPtr windowHandle,
            WtsSessionNotificationScope scope,
            IWtsSessionNotificationApi wtsApi)
        {
            _windowHandle = windowHandle;
            _scope = scope;
            _wtsApi = wtsApi;
        }

        /// <summary>Subscribes to the WTS registration.</summary>
        /// <param name="observer">The observer.</param>
        /// <returns>The subscription that unregisters when disposed.</returns>
        internal IDisposable Subscribe(
            IObserver<WtsSessionNotificationRegistration> observer)
        {
            try
            {
                if (!_wtsApi.WTSRegisterSessionNotification(
                    _windowHandle,
                    _scope))
                {
                    observer.OnNext(
                        new(
                            _windowHandle.ToInt64(),
                            _scope,
                            Succeeded: false,
                            _wtsApi.GetLastError()));
                    observer.OnCompleted();
                    return Scope.Create(static () => { });
                }

                observer.OnNext(
                    new(
                        _windowHandle.ToInt64(),
                        _scope,
                        Succeeded: true,
                        LastError: 0));
                return Scope.Create<WtsRegistrationState>(
                    new(
                        _windowHandle.ToInt64(),
                        _wtsApi),
                    static state => state.Unregister());
            }
            catch (Exception error)
            {
                observer.OnError(error);
                return Scope.Create(static () => { });
            }
        }
    }

    /// <summary>WTS registration state.</summary>
    private sealed class WtsRegistrationState
    {
        /// <summary>The registered window handle value.</summary>
        private readonly long _windowHandleValue;

        /// <summary>The WTS adapter.</summary>
        private readonly IWtsSessionNotificationApi _wtsApi;

        /// <summary>Initializes a new instance of the <see cref="WtsRegistrationState"/> class.</summary>
        /// <param name="windowHandleValue">The registered window handle value.</param>
        /// <param name="wtsApi">The WTS adapter.</param>
        internal WtsRegistrationState(
            long windowHandleValue,
            IWtsSessionNotificationApi wtsApi)
        {
            _windowHandleValue = windowHandleValue;
            _wtsApi = wtsApi;
        }

        /// <summary>Unregisters the window.</summary>
        internal void Unregister() =>
            _ = _wtsApi.WTSUnRegisterSessionNotification(new(_windowHandleValue));
    }
}
