// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using ReactiveUI.Primitives.Disposables;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>A listener for Windows session change notifications.</summary>
public class WindowsSessionListener : IDisposable
{
    /// <summary>Composes session-listener operations.</summary>
    /// <param name="listen">The message source operation.</param>
    /// <param name="register">The registration operation.</param>
    /// <param name="unregister">The unregistration operation.</param>
    private sealed class SessionListenerOperations(SessionMessageSource listen, SessionRegistrationOperation register, SessionUnregistrationOperation unregister)
    {
        /// <summary>Creates a session-message stream.</summary>
        /// <param name="onSetup">The setup callback.</param>
        /// <param name="onTeardown">The teardown callback.</param>
        /// <returns>The session-message stream.</returns>
        public IObservable<WindowMessage> Listen(Action<long> onSetup, Action<long> onTeardown) => listen(onSetup, onTeardown);

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

    /// <summary>The flag that registers only the current session.</summary>
    private const int NotifyForThisSession = 0;

    /// <summary>Session listener operations used by this process.</summary>
    private static SessionListenerOperations _operations = new(SharedMessageWindow.ObserveWindowMessages, WindowsSessionListenerNativeMethods.WtsRegisterSessionNotification, WindowsSessionListenerNativeMethods.WtsUnRegisterSessionNotification);

    /// <summary>Synchronizes listener state changes.</summary>
    private readonly object _lock = new();

    /// <summary>The shared stream of session change events.</summary>
    private readonly IObservable<SessionChangeEventArgs> _sessionChanges;

    /// <summary>The active message subscription.</summary>
    private IDisposable _subscription;

    /// <summary>A value indicating whether event delivery is paused.</summary>
    private volatile bool _isPaused;

    /// <summary>A value indicating whether this instance has been disposed.</summary>
    private volatile bool _isDisposed;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.WindowsSessionListener" /> class.</summary>
    public WindowsSessionListener()
    {
        _sessionChanges = CreateSessionChanges().Publish().RefCount();
    }

    /// <summary>Observes all Windows session change notifications.</summary>
    /// <returns>An observable sequence of session change notifications.</returns>
    public IObservable<SessionChangeEventArgs> ObserveSessionChanges()
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfDisposed(_isDisposed, this);
        return _sessionChanges.Where((_) => !_isPaused);
    }

    /// <summary>Observes Windows session lock and unlock notifications.</summary>
    /// <returns>An observable sequence of lock and unlock notifications.</returns>
    public IObservable<SessionChangeEventArgs> ObserveSessionLockChanges() => from args in ObserveSessionChanges()
            where IsSessionLockChange(args.EventType)
            select args;

    /// <summary>Observes Windows session logon and logoff notifications.</summary>
    /// <returns>An observable sequence of logon and logoff notifications.</returns>
    public IObservable<SessionChangeEventArgs> ObserveSessionLogonChanges() => from args in ObserveSessionChanges()
            where IsSessionLogonChange(args.EventType)
            select args;

    /// <summary>Starts listening for session change events.</summary>
    public void Start()
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfDisposed(_isDisposed, this);
        lock (_lock)
        {
            if (_subscription is null)
            {
                CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfDisposed(_isDisposed, this);
                _isPaused = false;
                _subscription = ObserveSessionChanges().Subscribe(delegate
                {
                });
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
    internal static IDisposable OverrideOperationsForTesting(SessionMessageSource messageSource, SessionRegistrationOperation register, SessionUnregistrationOperation unregister)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(messageSource);
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(register);
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(unregister);
        SessionListenerOperations operations = _operations;
        _operations = new(messageSource, register, unregister);
        return Scope.Create(operations, delegate(SessionListenerOperations previous)
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
    private static IObservable<SessionChangeEventArgs> CreateSessionChanges() => from message in _operations.Listen(
        delegate (long hwnd)
            {
                if (!_operations.Register((nint)hwnd, 0))
                {
                    throw new InvalidOperationException("Failed to register for session notifications");
                }
            },
        delegate (long hwnd)
            {
                _ = _operations.Unregister((nint)hwnd);
            })
                                                                                 where message.Msg == WindowsMessages.WM_WTSSESSION_CHANGE
            select new SessionChangeEventArgs((WtsSessionChangeEvents)checked((int)message.WParam), checked((int)message.LParam));

    /// <summary>Determines whether the event is a lock-state change.</summary>
    /// <param name="eventType">The session event type.</param>
    /// <returns><c>true</c> for lock or unlock events; otherwise <c>false</c>.</returns>
    private static bool IsSessionLockChange(WtsSessionChangeEvents eventType)
    {
        if ((uint)(eventType - 7) <= 1U)
        {
            return true;
        }

        return false;
    }

    /// <summary>Determines whether the event is a logon-state change.</summary>
    /// <param name="eventType">The session event type.</param>
    /// <returns><c>true</c> for logon or logoff events; otherwise <c>false</c>.</returns>
    private static bool IsSessionLogonChange(WtsSessionChangeEvents eventType)
    {
        if ((uint)(eventType - 5) <= 1U)
        {
            return true;
        }

        return false;
    }
}
