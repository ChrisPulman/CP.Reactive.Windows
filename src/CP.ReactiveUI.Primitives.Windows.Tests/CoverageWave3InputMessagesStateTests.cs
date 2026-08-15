// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Coverage for input, lifecycle, messaging, multimedia, and power composition paths.</summary>
public sealed class CoverageWave3InputMessagesStateTests
{
    /// <summary>A non-owning value used when native parameters are captured by fakes.</summary>
    private const int TestNativeValue = 0x1234;

    /// <summary>An alternate message-loop handle value.</summary>
    private const long AlternateWindowHandle = 84;

    /// <summary>Small deterministic in-memory wave used by the multimedia composition test.</summary>
    private static readonly byte[] TestAudioBytes = [One, Two, Three];

    /// <summary>Covers every convenience overload of the mouse input generator without sending native input.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MouseGeneratorConvenienceOverloadsUseSharedCompositionAsync()
    {
        const uint expectedResult = Seven;
        var fake = new InputCoverage2Tests.FakeNativeInputApi { SendReturn = expectedResult };
        var previous = NativeInput.SetApiForTesting(fake);
        try
        {
            var location = new NativePoint(Ten, Twenty);
            await Assert.That(MouseInputGenerator.MouseClick(MouseButtons.Left)).IsEqualTo(expectedResult);
            await Assert.That(MouseInputGenerator.MouseClick(MouseButtons.Left, location)).IsEqualTo(expectedResult);
            await Assert.That(MouseInputGenerator.MouseDown(MouseButtons.Right)).IsEqualTo(expectedResult);
            await Assert.That(MouseInputGenerator.MouseDown(MouseButtons.Right, location)).IsEqualTo(expectedResult);
            await Assert.That(MouseInputGenerator.MouseUp(MouseButtons.Middle)).IsEqualTo(expectedResult);
            await Assert.That(MouseInputGenerator.MouseUp(MouseButtons.Middle, location)).IsEqualTo(expectedResult);
            await Assert.That(MouseInputGenerator.MoveMouse(location)).IsEqualTo(expectedResult);
            await Assert.That(MouseInputGenerator.MoveMouseWheel(OneHundredTwenty)).IsEqualTo(expectedResult);
            await Assert.That(MouseInputGenerator.MoveMouseWheel(-OneHundredTwenty, location)).IsEqualTo(expectedResult);
            await Assert.That(fake.SendCalls).IsEqualTo(Nine);
        }
        finally
        {
            _ = NativeInput.SetApiForTesting(previous);
        }
    }

    /// <summary>Covers raw-input factories, path parsing, and invalid-handle behavior.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RawInputFactoriesAndInvalidHandlesAreDeterministicAsync()
    {
        var windowHandle = new IntPtr(TestNativeValue);
        RawInputDevices[] devices =
        [
            RawInputDevices.Pointer,
            RawInputDevices.Mouse,
            RawInputDevices.Joystick,
            RawInputDevices.GamePad,
            RawInputDevices.Keyboard,
            RawInputDevices.Keypad,
            RawInputDevices.SystemControl,
            RawInputDevices.ConsumerAudioControl,
        ];

        foreach (var device in devices)
        {
            var registration = RawInputApi.CreateRawInputDevice(windowHandle, device, RawInputDeviceFlags.DeviceNotify);
            await Assert.That(registration.TargetHwnd).IsEqualTo(windowHandle);
        }

        var generic = RawInputApi.CreateRawInputDevice(windowHandle, HidUsagesGeneric.Keyboard);
        var consumer = RawInputApi.CreateRawInputDevice(windowHandle, HidUsagesConsumer.ConsumerControl);
        await Assert.That(generic.UsagePage).IsEqualTo(HidUsagePages.Generic);
        await Assert.That(consumer.UsagePage).IsEqualTo(HidUsagePages.Consumer);
        await Assert.That(() => RawInputApi.CreateRawInputDevice(
            windowHandle,
            (RawInputDevices)int.MaxValue,
            RawInputDeviceFlags.None)).Throws<NotSupportedException>();
        await Assert.That(RawInputApi.GetDisplayName(null)).IsNull();
        await Assert.That(RawInputApi.GetDisplayName("abc")).IsNull();
        await Assert.That(RawInputApi.GetDisplayName("abcd-device")).IsEqualTo("abcd-device");
        await Assert.That(RawInputApi.GetDisplayName(@"\\??\MISSING#DEVICE#INSTANCE")).IsNull();

        TryRegisterNoRawInputDevices();
        TryReadMissingRawInputDevice();
        var dataSize = 0;
        var copied = RawInputApi.GetRawInputData(
            IntPtr.Zero,
            RawInputDataCommands.Input,
            out var rawInput,
            ref dataSize,
            Marshal.SizeOf<RawInputHeader>());
        await Assert.That(copied).IsLessThanOrEqualTo(0);
        await Assert.That(rawInput).IsEqualTo(default(RawInput));
    }

    /// <summary>Covers restart registration through reversible native-operation composition.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RestartOperationsComposeSuccessAndFailureAsync()
    {
        var registrations = new List<(string CommandLine, ApplicationRestartFlags Flags)>();
        var unregisterCalls = 0;
        using (ApplicationRestartManager.OverrideRestartOperationsForTesting(
                   (commandLine, flags) =>
                   {
                       registrations.Add((commandLine, flags));
                       return 0;
                   },
                   () =>
                   {
                       unregisterCalls++;
                       return 0;
                   }))
        {
            ApplicationRestartManager.RegisterForRestart();
            ApplicationRestartManager.RegisterForRestart("--safe");
            ApplicationRestartManager.RegisterForRestart("--safe-two", ApplicationRestartFlags.RestartNoCrash);
            ApplicationRestartManager.UnregisterForRestart();
        }

        await Assert.That(registrations.Count).IsEqualTo(Three);
        await Assert.That(unregisterCalls).IsEqualTo(One);
        using (ApplicationRestartManager.OverrideRestartOperationsForTesting(static (_, _) => Five, static () => Five))
        {
            await Assert.That(static () => ApplicationRestartManager.RegisterForRestart("--failure")).Throws<Win32Exception>();
            await Assert.That(static () => ApplicationRestartManager.UnregisterForRestart()).Throws<Win32Exception>();
        }
    }

    /// <summary>Covers end-session translation and filtering without connecting a native message window.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task EndSessionCompositionTranslatesAndFiltersMessagesAsync()
    {
        var observer = new CoreInteropCoverageTests.RecordingObserver<EndSessionMessage>();
        var subscription = ApplicationRestartManager.CreateEndSessionObserverForTesting(observer, null, null, connect: false);
        subscription.Observer.OnNext(new(0, WindowsMessages.WM_NULL, 0, 0));
        subscription.Observer.OnNext(new(
            0,
            WindowsMessages.WM_QUERYENDSESSION,
            0,
            unchecked((nint)(uint)EndSessionReasons.ENDSESSION_LOGOFF)));
        subscription.Observer.OnNext(new(
            0,
            WindowsMessages.WM_ENDSESSION,
            0,
            (nint)EndSessionReasons.ENDSESSION_CLOSEAPP));
        subscription.Observer.OnCompleted();
        subscription.Observer.OnError(new InvalidOperationException("expected"));
        subscription.Lifetime.Dispose();
        await Assert.That(observer.Values.Count).IsEqualTo(Two);
        await Assert.That(observer.Completed).IsTrue();
        await Assert.That(observer.Error).IsTypeOf<InvalidOperationException>();

        var handledObserver = new CoreInteropCoverageTests.RecordingObserver<EndSessionMessage>();
        var handled = ApplicationRestartManager.CreateEndSessionObserverForTesting(
            handledObserver,
            static _ => true,
            static _ => false,
            connect: false);
        handled.Observer.OnNext(new(0, WindowsMessages.WM_QUERYENDSESSION, 0, 0));
        handled.Observer.OnNext(new(0, WindowsMessages.WM_ENDSESSION, 0, 0));
        handled.Lifetime.Dispose();
        await Assert.That(handledObserver.Values).IsEmpty();

        var connectedObserver = new CoreInteropCoverageTests.RecordingObserver<EndSessionMessage>();
        var connected = ApplicationRestartManager.CreateEndSessionObserverForTesting(
            connectedObserver,
            null,
            null,
            connect: true);
        connected.Lifetime.Dispose();
    }

    /// <summary>Covers all power convenience APIs through reversible fake operations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task PowerMethodsComposeFlagsWithoutChangingSystemStateAsync()
    {
        var suspendCalls = new List<(bool Hibernate, bool ForceCritical, bool DisableWakeEvent)>();
        var exitCalls = new List<(ExitWindowsFlags Flags, uint Reason)>();
        using var scope = PowerManagementApi.OverrideOperationsForTesting(
            (hibernate, forceCritical, disableWakeEvent) =>
            {
                suspendCalls.Add((hibernate, forceCritical, disableWakeEvent));
                return true;
            },
            (flags, reason) =>
            {
                exitCalls.Add((flags, reason));
                return true;
            });

        await Assert.That(PowerManagementApi.SetSuspendState(true, true, true)).IsTrue();
        await Assert.That(PowerManagementApi.Sleep()).IsTrue();
        await Assert.That(PowerManagementApi.Sleep(true)).IsTrue();
        await Assert.That(PowerManagementApi.Hibernate()).IsTrue();
        await Assert.That(PowerManagementApi.Hibernate(true)).IsTrue();
        await Assert.That(PowerManagementApi.ExitWindowsEx(ExitWindowsFlags.EWX_RESTARTAPPS, FortyTwo)).IsTrue();
        await Assert.That(PowerManagementApi.ExitWindowsEx(ExitWindowsFlags.EWX_RESTARTAPPS)).IsTrue();
        await Assert.That(PowerManagementApi.Shutdown()).IsTrue();
        await Assert.That(PowerManagementApi.Shutdown(true)).IsTrue();
        await Assert.That(PowerManagementApi.Restart()).IsTrue();
        await Assert.That(PowerManagementApi.Restart(true)).IsTrue();
        await Assert.That(PowerManagementApi.LogOff()).IsTrue();
        await Assert.That(PowerManagementApi.LogOff(true)).IsTrue();
        await Assert.That(suspendCalls.Count).IsEqualTo(Five);
        await Assert.That(exitCalls.Count).IsEqualTo(Eight);
        await Assert.That(exitCalls.Exists(static call =>
            call.Flags == (ExitWindowsFlags.EWX_SHUTDOWN | ExitWindowsFlags.EWX_FORCE))).IsTrue();
        await Assert.That(exitCalls.Exists(static call =>
            call.Flags == (ExitWindowsFlags.EWX_REBOOT | ExitWindowsFlags.EWX_FORCE))).IsTrue();
        await Assert.That(exitCalls.Exists(static call => call.Flags == ExitWindowsFlags.EWX_FORCE)).IsTrue();
    }

    /// <summary>Covers multimedia overload routing without playing audio.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MultimediaMethodsRouteWithoutPlayingAudioAsync()
    {
        var byteCalls = 0;
        var nameCalls = new List<(string Name, SoundSettings Settings)>();
        var pointerCalls = new List<(IntPtr Pointer, SoundSettings Settings)>();
        using var scope = WinMm.OverrideOperationsForTesting(
            (bytes, _, _) =>
            {
                byteCalls += bytes.Length;
                return true;
            },
            (name, _, settings) =>
            {
                nameCalls.Add((name, settings));
                return true;
            },
            (pointer, _, settings) =>
            {
                pointerCalls.Add((pointer, settings));
                return true;
            });

        WinMm.PlaySystemSound(SystemSounds.SystemAsterisk);
        WinMm.Play("resource");
        WinMm.Play(unchecked((nint)TestNativeValue), SoundSettings.Memory);
        WinMm.Play(TestAudioBytes);
        WinMm.StopPlaying();
        await Assert.That(byteCalls).IsEqualTo(Three);
        await Assert.That(nameCalls.Count).IsEqualTo(Three);
        await Assert.That(nameCalls[nameCalls.Count - One].Name).IsNull();
        await Assert.That(pointerCalls.Count).IsEqualTo(One);
    }

    /// <summary>Covers message-loop overloads and handler branches through deterministic operations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MessageLoopUsesComposableOperationsAsync()
    {
        var responses = new Queue<sbyte>();
        var dispatchCount = 0;
        var filters = new List<(nint WindowHandle, uint Minimum, uint Maximum)>();

        sbyte GetMessage(out Msg message, nint windowHandle, uint minimumMessage, uint maximumMessage)
        {
            filters.Add((windowHandle, minimumMessage, maximumMessage));
            message = default;
            return responses.Dequeue();
        }

        void Dispatch(ref Msg message)
        {
            dispatchCount++;
            GC.KeepAlive(message);
        }

        using (MessageLoop.OverrideOperationsForTesting(GetMessage, Dispatch))
        {
            responses.Enqueue(1);
            responses.Enqueue(0);
            MessageLoop.ProcessMessages();
            responses.Enqueue(1);
            MessageLoop.ProcessMessages(StopAfterFirstMessage);
            responses.Enqueue(0);
            MessageLoop.ProcessMessages(ContinueMessages);
            responses.Enqueue(0);
            MessageLoop.ProcessMessages(ContinueMessages, FortyTwo);
            responses.Enqueue(0);
            MessageLoop.ProcessMessages(ContinueMessages, AlternateWindowHandle, Ten, Twenty);
            responses.Enqueue(0);
            await Assert.That(MessageLoop.TryGetMessage(out _)).IsFalse();
        }

        await Assert.That(dispatchCount).IsEqualTo(Two);
        await Assert.That(filters.Exists(static filter =>
            filter.WindowHandle == AlternateWindowHandle && filter.Minimum == Ten && filter.Maximum == Twenty)).IsTrue();
        SharedMessageWindow.PostQuitMessageForTesting();
        await Assert.That(MessageLoop.TryGetMessage(out _)).IsFalse();
        var nativeMessage = default(Msg);
        MessageLoop.Dispatch(ref nativeMessage);
    }

    /// <summary>Covers waitable-timer convenience overloads and repeated disposal.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WaitableTimerConvenienceOverloadsAndDisposalAreCoveredAsync()
    {
        var timer = new WaitableTimer($"CoverageWave3_{Guid.NewGuid():N}");
        await Assert.That(timer.SetOnce(TimeSpan.FromMilliseconds(Ten))).IsTrue();
        timer.Wait();
        await Assert.That(timer.SetOnce(TimeSpan.FromMilliseconds(Ten))).IsTrue();
        await Assert.That(await timer.WaitAsync()).IsTrue();
        await Assert.That(timer.SetOnce(TimeSpan.FromMilliseconds(Ten))).IsTrue();
        await Assert.That(await timer.WaitAsync(CancellationToken.None)).IsTrue();
        await Assert.That(timer.ObserveSignals()).IsNotNull();
        var excessiveTimeout = TimeSpan.FromMilliseconds((double)uint.MaxValue + 1);
        await Assert.That(() => timer.Wait(excessiveTimeout)).Throws<ArgumentOutOfRangeException>();
        timer.Dispose();
        timer.Dispose();
        await Assert.That(() => timer.Wait(TimeSpan.Zero)).Throws<ObjectDisposedException>();
    }

    /// <summary>Attempts a harmless empty raw-input registration.</summary>
    private static void TryRegisterNoRawInputDevices()
    {
        try
        {
            RawInputApi.RegisterRawInput([]);
        }
        catch (Win32Exception)
        {
        }
    }

    /// <summary>Attempts to read a non-existent raw-input device.</summary>
    private static void TryReadMissingRawInputDevice()
    {
        try
        {
            _ = RawInputApi.GetDeviceInformation(IntPtr.Zero);
        }
        catch (Win32Exception)
        {
        }
    }

    /// <summary>Stops a message loop after its first dispatched message.</summary>
    /// <param name="message">The dispatched message.</param>
    /// <returns><see langword="false"/>.</returns>
    private static bool StopAfterFirstMessage(ref Msg message)
    {
        GC.KeepAlive(message);
        return false;
    }

    /// <summary>Continues a message loop.</summary>
    /// <param name="message">The dispatched message.</param>
    /// <returns><see langword="true"/>.</returns>
    private static bool ContinueMessages(ref Msg message)
    {
        GC.KeepAlive(message);
        return true;
    }
}
