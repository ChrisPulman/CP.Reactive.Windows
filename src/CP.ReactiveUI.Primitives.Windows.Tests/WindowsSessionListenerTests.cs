// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests for WindowsSessionListener.</summary>
public class WindowsSessionListenerTests
{
    /// <summary>Defines the TestValue123 test value.</summary>
    private const int TestValue123 = 123;

    /// <summary>Writes diagnostic messages for these tests.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(WindowsSessionListenerTests));

    /// <summary>Test that WindowsSessionListener can be created and started.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestWindowsSessionListener_CanCreateAsync()
    {
        using var listener = CreateListener(new());
        await Assert.That(listener).IsNotNull();
    }

    /// <summary>Test that WindowsSessionListener can be started and stopped.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestWindowsSessionListener_CanStartAndStopAsync()
    {
        var source = new SessionListenerTestSource();
        using var listener = CreateListener(source);
        listener.Start();
        listener.Stop();

        await Assert.That(source.RegistrationCount).IsEqualTo(1);
        await Assert.That(source.UnregistrationCount).IsEqualTo(1);
    }

    /// <summary>Test that WindowsSessionListener can be paused and resumed.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestWindowsSessionListener_CanPauseAndResumeAsync()
    {
        var source = new SessionListenerTestSource();
        using var listener = CreateListener(source);
        listener.Start();
        listener.Pause();
        listener.Resume();
        listener.Stop();

        await Assert.That(source.RegistrationCount).IsEqualTo(1);
        await Assert.That(source.UnregistrationCount).IsEqualTo(1);
    }

    /// <summary>Test that WindowsSessionListener observables can be subscribed.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestWindowsSessionListener_CanSubscribeToObservablesAsync()
    {
        var source = new SessionListenerTestSource();
        using var listener = CreateListener(source);

        var lockEventReceived = false;
        var logonEventReceived = false;

        using var lockSubscription = listener.ObserveSessionLockChanges().SubscribeOnNext(args =>
        {
            Log.Info($"Lock/Unlock event: {args.EventType}, SessionId: {args.SessionId}");
            lockEventReceived = true;
        });

        using var logonSubscription = listener.ObserveSessionLogonChanges().SubscribeOnNext(args =>
        {
            Log.Info($"Logon/Logoff event: {args.EventType}, SessionId: {args.SessionId}");
            logonEventReceived = true;
        });

        listener.Start();
        source.Publish(WtsSessionChangeEvents.WTS_SESSION_LOCK);
        source.Publish(WtsSessionChangeEvents.WTS_SESSION_LOGON);

        await Assert.That(lockEventReceived).IsTrue();
        await Assert.That(logonEventReceived).IsTrue();

        listener.Stop();
    }

    /// <summary>Test that WindowsSessionListener can be disposed multiple times safely.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestWindowsSessionListener_CanDisposeMultipleTimesAsync()
    {
        var source = new SessionListenerTestSource();
        var listener = CreateListener(source);
        listener.Start();
        listener.Dispose();
        listener.Dispose();

        await Assert.That(source.UnregistrationCount).IsEqualTo(1);
    }

    /// <summary>Test that WindowsSessionListener throws when used after disposal.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestWindowsSessionListener_ThrowsAfterDisposalAsync()
    {
        var listener = CreateListener(new());
        listener.Dispose();
        await Assert.That(() => listener.Start()).Throws<ObjectDisposedException>();
        await Assert.That(() => listener.ObserveSessionChanges()).Throws<ObjectDisposedException>();
    }

    /// <summary>Test that WtsSessionChangeEvents enum has expected values.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestWtsSessionChangeEvents_HasExpectedValuesAsync()
    {
        var hasLogon = Enum.TryParse(nameof(WtsSessionChangeEvents.WTS_SESSION_LOGON), out WtsSessionChangeEvents logon);
        var hasLogoff = Enum.TryParse(nameof(WtsSessionChangeEvents.WTS_SESSION_LOGOFF), out WtsSessionChangeEvents logoff);
        var hasLock = Enum.TryParse(nameof(WtsSessionChangeEvents.WTS_SESSION_LOCK), out WtsSessionChangeEvents locked);
        var hasUnlock = Enum.TryParse(nameof(WtsSessionChangeEvents.WTS_SESSION_UNLOCK), out WtsSessionChangeEvents unlocked);

        await Assert.That(hasLogon).IsTrue();
        await Assert.That(hasLogoff).IsTrue();
        await Assert.That(hasLock).IsTrue();
        await Assert.That(hasUnlock).IsTrue();
        await Assert.That((int)logon).IsEqualTo(0x5);
        await Assert.That((int)logoff).IsEqualTo(0x6);
        await Assert.That((int)locked).IsEqualTo(0x7);
        await Assert.That((int)unlocked).IsEqualTo(0x8);
    }

    /// <summary>Test SessionChangeEventArgs properties.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestSessionChangeEventArgs_HasCorrectPropertiesAsync()
    {
        var args = new SessionChangeEventArgs(WtsSessionChangeEvents.WTS_SESSION_LOCK, TestValue123);
        await Assert.That(args.EventType).IsEqualTo(WtsSessionChangeEvents.WTS_SESSION_LOCK);
        await Assert.That(args.SessionId).IsEqualTo(TestValue123);
    }

    /// <summary>Creates a listener with an isolated deterministic message source.</summary>
    /// <param name="source">The message source used by the listener.</param>
    /// <returns>A session listener that never calls Windows APIs.</returns>
    private static WindowsSessionListener CreateListener(SessionListenerTestSource source) =>
        new(source.Listen, source.Register, source.Unregister);

    /// <summary>Provides deterministic session-message and registration operations for a test.</summary>
    private sealed class SessionListenerTestSource
    {
        /// <summary>The deterministic listener handle.</summary>
        private const long WindowHandle = 42;

        /// <summary>The deterministic session identifier.</summary>
        private const int SessionId = 123;

        /// <summary>The observers currently listening for test messages.</summary>
        private readonly List<IObserver<WindowMessage>> _observers = [];

        /// <summary>Gets the number of registrations.</summary>
        public int RegistrationCount { get; private set; }

        /// <summary>Gets the number of unregistrations.</summary>
        public int UnregistrationCount { get; private set; }

        /// <summary>Creates a deterministic message stream.</summary>
        /// <param name="onSetup">The setup callback.</param>
        /// <param name="onTeardown">The teardown callback.</param>
        /// <returns>The deterministic message stream.</returns>
        public IObservable<WindowMessage> Listen(Action<long> onSetup, Action<long> onTeardown) =>
            new SessionMessageObservable(this, onSetup, onTeardown);

        /// <summary>Records a successful test registration.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="flags">The session registration flags.</param>
        /// <returns><c>true</c>.</returns>
        public bool Register(IntPtr windowHandle, int flags)
        {
            _ = windowHandle;
            _ = flags;
            RegistrationCount++;
            return true;
        }

        /// <summary>Records a successful test unregistration.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <returns><c>true</c>.</returns>
        public bool Unregister(IntPtr windowHandle)
        {
            _ = windowHandle;
            UnregistrationCount++;
            return true;
        }

        /// <summary>Publishes a deterministic session event.</summary>
        /// <param name="eventType">The event type to publish.</param>
        public void Publish(WtsSessionChangeEvents eventType)
        {
            var message = new WindowMessage(
                (nint)WindowHandle,
                WindowsMessages.WM_WTSSESSION_CHANGE,
                (nint)eventType,
                (nint)SessionId);
            foreach (IObserver<WindowMessage> observer in _observers.ToArray())
            {
                observer.OnNext(message);
            }
        }

        /// <summary>Represents one deterministic source subscription.</summary>
        /// <param name="source">The source that owns the subscription.</param>
        /// <param name="onSetup">The setup callback.</param>
        /// <param name="onTeardown">The teardown callback.</param>
        private sealed class SessionMessageObservable(
            SessionListenerTestSource source,
            Action<long> onSetup,
            Action<long> onTeardown) : IObservable<WindowMessage>
        {
            /// <inheritdoc/>
            public IDisposable Subscribe(IObserver<WindowMessage> observer)
            {
                onSetup(WindowHandle);
                source._observers.Add(observer);
                return new SessionMessageSubscription(source, observer, onTeardown);
            }
        }

        /// <summary>Disposes one deterministic source subscription.</summary>
        /// <param name="source">The source that owns the subscription.</param>
        /// <param name="observer">The observer to remove.</param>
        /// <param name="onTeardown">The teardown callback.</param>
        private sealed class SessionMessageSubscription(
            SessionListenerTestSource source,
            IObserver<WindowMessage> observer,
            Action<long> onTeardown) : IDisposable
        {
            /// <summary>Tracks whether this subscription has been disposed.</summary>
            private bool _isDisposed;

            /// <inheritdoc/>
            public void Dispose()
            {
                if (!_isDisposed)
                {
                    _isDisposed = true;
                    _ = source._observers.Remove(observer);
                    onTeardown(WindowHandle);
                }
            }
        }
    }
}
