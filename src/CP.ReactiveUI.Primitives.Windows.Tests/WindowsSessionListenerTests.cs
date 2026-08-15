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
        using var listener = new WindowsSessionListener();
        await Assert.That(listener).IsNotNull();
    }

    /// <summary>Test that WindowsSessionListener can be started and stopped.</summary>
    [Test]
    public void TestWindowsSessionListener_CanStartAndStop()
    {
        using var listener = new WindowsSessionListener();
        listener.Start();
        listener.Stop();
    }

    /// <summary>Test that WindowsSessionListener can be paused and resumed.</summary>
    [Test]
    public void TestWindowsSessionListener_CanPauseAndResume()
    {
        using var listener = new WindowsSessionListener();
        listener.Start();
        listener.Pause();
        listener.Resume();
        listener.Stop();
    }

    /// <summary>Test that WindowsSessionListener observables can be subscribed.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestWindowsSessionListener_CanSubscribeToObservablesAsync()
    {
        using var listener = new WindowsSessionListener();

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

        // Note: We can't easily trigger actual session change events in a unit test,
        // but we can verify that the events are properly wired up
        await Assert.That(lockEventReceived).IsFalse(); // No events should have been received yet
        await Assert.That(logonEventReceived).IsFalse(); // No events should have been received yet

        listener.Stop();
    }

    /// <summary>Test that WindowsSessionListener can be disposed multiple times safely.</summary>
    [Test]
    public void TestWindowsSessionListener_CanDisposeMultipleTimes()
    {
        var listener = new WindowsSessionListener();
        listener.Start();
        listener.Dispose();
        listener.Dispose(); // Should not throw
    }

    /// <summary>Test that WindowsSessionListener throws when used after disposal.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestWindowsSessionListener_ThrowsAfterDisposalAsync()
    {
        var listener = new WindowsSessionListener();
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
}
