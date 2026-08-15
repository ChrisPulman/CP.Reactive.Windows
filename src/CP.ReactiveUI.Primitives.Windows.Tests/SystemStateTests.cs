// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests for the SystemState package: PowerManagementApi, SystemStateApi, and WaitableTimer.</summary>
public class SystemStateTests
{
    /// <summary>Defines the TestValue100 test value.</summary>
    private const int TestValue100 = 100;

    /// <summary>Defines the TestValue5 test value.</summary>
    private const int TestValue5 = 5;

    /// <summary>Writes diagnostic messages for these tests.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(SystemStateTests));

    /// <summary>Test that SetThreadExecutionState can be called and returns a valid previous state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_SetThreadExecutionState_PreventAndAllowSleepAsync()
    {
        // Prevent the system from sleeping (keep the system awake)
        var previousState = SystemStateApi.PreventSystemSleep();
        Log.Info($"Previous execution state: {previousState}");

        // Allow the system to sleep again
        var restoredState = SystemStateApi.AllowSleep();
        Log.Info($"Restored execution state: {restoredState}");

        // A successful call should return a non-zero value
        await Assert.That(restoredState != 0).IsTrue();
    }

    /// <summary>Test that SetThreadExecutionState can be called with display required flag.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_SetThreadExecutionState_PreventDisplaySleepAsync()
    {
        // Prevent the display from turning off
        var previousState = SystemStateApi.PreventSleep();
        Log.Info($"Previous execution state: {previousState}");

        // Allow the system to sleep again
        var restoredState = SystemStateApi.AllowSleep();
        Log.Info($"Restored execution state: {restoredState}");

        await Assert.That(restoredState != 0).IsTrue();
    }

    /// <summary>Test that a WaitableTimer can be created and disposed.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_WaitableTimer_CreateAndDisposeAsync()
    {
        using var timer = new WaitableTimer();
        await Assert.That(timer.IsValid).IsTrue();
    }

    /// <summary>Test that a named WaitableTimer can be created.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_WaitableTimer_CreateNamedTimerAsync()
    {
        var timerName = $"CPReactiveWindowsTestTimer_{Guid.NewGuid():N}";
        using var timer = new WaitableTimer(timerName);
        await Assert.That(timer.IsValid).IsTrue();
    }

    /// <summary>Test that a WaitableTimer fires after a short delay.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_WaitableTimer_FiresAfterDelayAsync()
    {
        using var timer = new WaitableTimer();
        var delay = TimeSpan.FromMilliseconds(TestValue100);
        var wasSet = timer.SetOnce(delay);
        await Assert.That(wasSet).IsTrue();

        // Wait for the timer to fire (with a generous timeout)
        var signaled = await timer.WaitAsync(TimeSpan.FromSeconds(TestValue5));
        await Assert.That(signaled).IsTrue();
    }

    /// <summary>Test that a WaitableTimer can be awaited asynchronously.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_WaitableTimer_WaitAsyncFiresAfterDelayAsync()
    {
        using var timer = new WaitableTimer();
        var wasSet = timer.SetOnce(TimeSpan.FromMilliseconds(TestValue100));
        await Assert.That(wasSet).IsTrue();

        var signaled = await timer.WaitAsync(TimeSpan.FromSeconds(TestValue5));
        await Assert.That(signaled).IsTrue();
    }

    /// <summary>Test that a WaitableTimer exposes signals as an observable sequence.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_WaitableTimer_ObserveSignalsAsync()
    {
        using var timer = new WaitableTimer();
        var signalCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = timer.ObserveSignals(TimeSpan.FromSeconds(TestValue5))
            .Subscribe(
                _ => signalCompletion.TrySetResult(true),
                exception => signalCompletion.TrySetException(exception));

        var wasSet = timer.SetOnce(TimeSpan.FromMilliseconds(TestValue100));
        await Assert.That(wasSet).IsTrue();

        var completedTask = await Task.WhenAny(signalCompletion.Task, Task.Delay(TimeSpan.FromSeconds(TestValue5)));
        await Assert.That(ReferenceEquals(completedTask, signalCompletion.Task)).IsTrue();
        await Assert.That(await signalCompletion.Task).IsTrue();
    }

    /// <summary>Test that a WaitableTimer can be cancelled.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_WaitableTimer_CancelAsync()
    {
        using var timer = new WaitableTimer();
        var wasSet = timer.SetOnce(TimeSpan.FromHours(1));
        await Assert.That(wasSet).IsTrue();

        var wasCancelled = timer.Cancel();
        await Assert.That(wasCancelled).IsTrue();
    }

    /// <summary>Test that WaitableTimer throws ObjectDisposedException after disposal.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_WaitableTimer_ThrowsAfterDisposeAsync()
    {
        var timer = new WaitableTimer();
        timer.Dispose();
        await Assert.That(() => timer.SetOnce(TimeSpan.FromSeconds(1))).Throws<ObjectDisposedException>();
    }

    /// <summary>Test that PowerBroadcastEvent enum has expected values.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_PowerBroadcastEvent_EnumValuesAsync()
    {
        var suspend = ToUInt32(PowerBroadcastEvent.PBT_APMSUSPEND);
        var resumeSuspend = ToUInt32(PowerBroadcastEvent.PBT_APMRESUMESUSPEND);
        var powerStatusChange = ToUInt32(PowerBroadcastEvent.PBT_APMPOWERSTATUSCHANGE);
        var resumeAutomatic = ToUInt32(PowerBroadcastEvent.PBT_APMRESUMEAUTOMATIC);

        await Assert.That(suspend).IsEqualTo(0x0004U);
        await Assert.That(resumeSuspend).IsEqualTo(0x0007U);
        await Assert.That(powerStatusChange).IsEqualTo(0x000AU);
        await Assert.That(resumeAutomatic).IsEqualTo(0x0012U);
    }

    /// <summary>Test that ThreadExecutionStateFlags enum has expected values.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_ThreadExecutionStateFlags_EnumValuesAsync()
    {
        var systemRequired = ToUInt32(ThreadExecutionStateFlags.ES_SYSTEM_REQUIRED);
        var displayRequired = ToUInt32(ThreadExecutionStateFlags.ES_DISPLAY_REQUIRED);
        var continuous = ToUInt32(ThreadExecutionStateFlags.ES_CONTINUOUS);

        await Assert.That(systemRequired).IsEqualTo(0x00000001U);
        await Assert.That(displayRequired).IsEqualTo(0x00000002U);
        await Assert.That(continuous).IsEqualTo(0x80000000U);
    }

    /// <summary>Test that ExitWindowsFlags enum has expected values.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_ExitWindowsFlags_EnumValuesAsync()
    {
        var none = ToUInt32(ExitWindowsFlags.None);
        var shutdown = ToUInt32(ExitWindowsFlags.EWX_SHUTDOWN);
        var reboot = ToUInt32(ExitWindowsFlags.EWX_REBOOT);
        var powerOff = ToUInt32(ExitWindowsFlags.EWX_POWEROFF);

        await Assert.That(none).IsEqualTo(0x00000000U);
        await Assert.That(shutdown).IsEqualTo(0x00000001U);
        await Assert.That(reboot).IsEqualTo(0x00000002U);
        await Assert.That(powerOff).IsEqualTo(0x00000008U);
    }

    /// <summary>Test that SetWaitableTimer can be used for a future UTC time.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_WaitableTimer_SetAtFutureTimeAsync()
    {
        using var timer = new WaitableTimer();
        var futureTime = TimeProvider.System.GetUtcNow().AddMilliseconds(TestValue100);
        var wasSet = timer.SetAt(futureTime);
        await Assert.That(wasSet).IsTrue();

        var signaled = await timer.WaitAsync(TimeSpan.FromSeconds(TestValue5));
        await Assert.That(signaled).IsTrue();
    }

    /// <summary>Converts an enum value to its unsigned integer representation.</summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The unsigned integer representation.</returns>
    private static uint ToUInt32<TEnum>(TEnum value)
        where TEnum : struct, Enum =>
        Convert.ToUInt32(value);
}
