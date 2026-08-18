// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>A listener for Windows session change notifications.</summary>
public class WindowsSessionListener : IDisposable
{
    /// <summary>The flag that registers only the current session.</summary>
    private const int NotifyForThisSession = 0;

    /// <summary>Session listener operations used by listeners that use the default constructor.</summary>
    private static SessionListenerOperations _operations = new(
        SharedMessageWindow.ObserveWindowMessages,
        WindowsSessionListenerNativeMethods.WtsRegisterSessionNotification,
        WindowsSessionListenerNativeMethods.WtsUnRegisterSessionNotification);

    /// <summary>Synchronizes listener state changes.</summary>
#if NET9_0_OR_GREATER
    private readonly System.Threading.Lock _lock = new();
#else
    private readonly object _lock = new();
#endif

    /// <summary>The shared stream of session change events.</summary>
    private readonly IObservable<SessionChangeEventArgs> _sessionChanges;

    /// <summary>The composed message and registration operations for this listener.</summary>
    private readonly SessionListenerOperations _listenerOperations;

    /// <summary>The active message subscription.</summary>
    private IDisposable _subscription;

    /// <summary>A value indicating whether event delivery is paused.</summary>
    private volatile bool _isPaused;

    /// <summary>A value indicating whether this instance has been disposed.</summary>
    private volatile bool _isDisposed;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.WindowsSessionListener" /> class.</summary>
    public WindowsSessionListener()
    {
        _listenerOperations = _operations;
        _sessionChanges = CreateSessionChanges().Publish().RefCount();
    }

    /// <summary>Initializes a new instance of the <see cref="WindowsSessionListener" /> class with explicitly composed native operations.</summary>
    /// <param name="messageSource">The source of window messages.</param>
    /// <param name="register">The session-notification registration operation.</param>
    /// <param name="unregister">The session-notification unregistration operation.</param>
    /// <remarks>This overload is internal so the test assembly can supply deterministic operations without invoking Windows APIs.</remarks>
    internal WindowsSessionListener(
        SessionMessageSource messageSource,
        SessionRegistrationOperation register,
        SessionUnregistrationOperation unregister)
    {
        Throw.IfNull(messageSource);
        Throw.IfNull(register);
        Throw.IfNull(unregister);
        _listenerOperations = new(messageSource, register, unregister);
        _sessionChanges = CreateSessionChanges().Publish().RefCount();
    }

    /// <summary>Observes all Windows session change notifications.</summary>
    /// <returns>An observable sequence of session change notifications.</returns>
    public IObservable<SessionChangeEventArgs> ObserveSessionChanges()
    {
        Throw.IfDisposed(_isDisposed, this);
        return _sessionChanges.Where((_) => !_isPaused);
    }

    /// <summary>Observes Windows session lock and unlock notifications.</summary>
    /// <returns>An observable sequence of lock and unlock notifications.</returns>
    public IObservable<SessionChangeEventArgs> ObserveSessionLockChanges() => from args in ObserveSessionChanges()
                                                                              where args.EventType is WtsSessionChangeEvents.WTS_SESSION_LOCK
                                                                                  or WtsSessionChangeEvents.WTS_SESSION_UNLOCK
                                                                              select args;

    /// <summary>Observes Windows session logon and logoff notifications.</summary>
    /// <returns>An observable sequence of logon and logoff notifications.</returns>
    public IObservable<SessionChangeEventArgs> ObserveSessionLogonChanges() => from args in ObserveSessionChanges()
                                                                               where args.EventType is WtsSessionChangeEvents.WTS_SESSION_LOGON
                                                                                   or WtsSessionChangeEvents.WTS_SESSION_LOGOFF
                                                                               select args;

    /// <summary>Starts listening for session change events.</summary>
    public void Start()
    {
        Throw.IfDisposed(_isDisposed, this);
        lock (_lock)
        {
            if (_subscription is null)
            {
                Throw.IfDisposed(_isDisposed, this);
                _isPaused = false;
                _subscription = ObserveSessionChanges().Subscribe(static _ => { });
            }
        }
    }

    /// <summary>Pauses listening for session change events.</summary>
    public void Pause() => _isPaused = true;

    /// <summary>Resumes listening for session change events after being paused.</summary>
    public void Resume() => _isPaused = false;

    /// <summary>Stops listening for session change events.</summary>
    public void Stop()
    {
        lock (_lock)
        {
            if (_subscription is not null)
            {
                _subscription.Dispose();
                _subscription = null;
            }

            _isPaused = false;
        }
    }

    /// <summary>Disposes the listener and stops listening for events.</summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Overrides session-listener operations for deterministic tests.</summary>
    /// <param name="messageSource">The message source operation.</param>
    /// <param name="register">The registration operation.</param>
    /// <param name="unregister">The unregistration operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(
        SessionMessageSource messageSource,
        SessionRegistrationOperation register,
        SessionUnregistrationOperation unregister)
    {
        Throw.IfNull(messageSource);
        Throw.IfNull(register);
        Throw.IfNull(unregister);
        var operations = _operations;
        _operations = new(messageSource, register, unregister);
        return Scope.Create(operations, static previous =>
        {
            _operations = previous;
        });
    }

    /// <summary>Releases managed resources used by the listener.</summary>
    /// <param name="disposing"><c>true</c> to release managed resources; otherwise <c>false</c>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            if (disposing)
            {
                Stop();
            }
        }
    }

    /// <summary>Creates the session change stream.</summary>
    /// <returns>An observable sequence of session change notifications.</returns>
    private IObservable<SessionChangeEventArgs> CreateSessionChanges() => from message in _listenerOperations.Listen(
        hwnd =>
            {
                if (!_listenerOperations.Register((nint)hwnd, NotifyForThisSession))
                {
                    throw new InvalidOperationException("Failed to register for session notifications");
                }
            },
        hwnd =>
            {
                _ = _listenerOperations.Unregister((nint)hwnd);
            })
                                                                                  where message.Msg == WindowsMessages.WM_WTSSESSION_CHANGE
                                                                                  select new SessionChangeEventArgs((WtsSessionChangeEvents)checked((int)message.WParam), checked((int)message.LParam));

    /// <summary>Composes session-listener operations.</summary>
    /// <param name="listen">The message source operation.</param>
    /// <param name="register">The registration operation.</param>
    /// <param name="unregister">The unregistration operation.</param>
    private sealed class SessionListenerOperations(
        SessionMessageSource listen,
        SessionRegistrationOperation register,
        SessionUnregistrationOperation unregister)
    {
        /// <summary>Creates a session-message stream.</summary>
        /// <param name="onSetup">The setup callback.</param>
        /// <param name="onTeardown">The teardown callback.</param>
        /// <returns>The session-message stream.</returns>
        public IObservable<WindowMessage> Listen(Action<long> onSetup, Action<long> onTeardown) =>
            listen(onSetup, onTeardown);

        /// <summary>Registers a window for session notifications.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="flags">The registration flags.</param>
        /// <returns><c>true</c> when registration succeeds; otherwise <c>false</c>.</returns>
        public bool Register(IntPtr windowHandle, int flags) => register(windowHandle, flags);

        /// <summary>Unregisters a window from session notifications.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <returns><c>true</c> when unregistration succeeds; otherwise <c>false</c>.</returns>
        public bool Unregister(IntPtr windowHandle) => unregister(windowHandle);
    }
}
