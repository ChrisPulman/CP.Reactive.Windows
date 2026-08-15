// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Deterministic coverage for raw-input native composition, structs, and reactive monitors.</summary>
public sealed class CoverageFinalRawInputTests
{
    /// <summary>The synthetic raw-input device name used by tests.</summary>
    private const string FirstDeviceName = @"\\?\HID#VID_TEST";

    /// <summary>The synthetic second raw-input device name used by tests.</summary>
    private const string SecondDeviceName = @"\\?\HID#VID_SECOND";

    /// <summary>The unknown raw-input device value used for unsupported mapping coverage.</summary>
    private const RawInputDevices UnknownRawInputDevice = (RawInputDevices)(-One);

    /// <summary>The native mouse button flags offset.</summary>
    private const int RawMouseButtonFlagsOffset = 4;

    /// <summary>The native mouse wheel data offset.</summary>
    private const int RawMouseWheelDataOffset = 6;

    /// <summary>The native mouse X delta offset.</summary>
    private const int RawMouseXOffset = 12;

    /// <summary>The native mouse Y delta offset.</summary>
    private const int RawMouseYOffset = 16;

    /// <summary>The number of bits used by the native mouse button-state field.</summary>
    private const int MouseButtonStateBits = 16;

    /// <summary>The native HID input count offset.</summary>
    private const int RawHidInputCountOffset = 4;

    /// <summary>The native HID data pointer offset.</summary>
    private const int RawHidDataPointerOffset = 8;

    /// <summary>The last-input timestamp offset.</summary>
    private const int LastInputTimeOffset = 4;

    /// <summary>The low-level keyboard virtual-key offset.</summary>
    private const int KeyboardLowLevelVirtualKeyOffset = 0;

    /// <summary>The low-level keyboard scan-code offset.</summary>
    private const int KeyboardLowLevelScanCodeOffset = 4;

    /// <summary>The low-level keyboard flags offset.</summary>
    private const int KeyboardLowLevelFlagsOffset = 8;

    /// <summary>The low-level keyboard timestamp offset.</summary>
    private const int KeyboardLowLevelTimestampOffset = 12;

    /// <summary>The low-level keyboard extra-information offset.</summary>
    private const int KeyboardLowLevelExtraInfoOffset = 16;

    /// <summary>The first raw-input device handle used by tests.</summary>
    private static readonly IntPtr FirstDeviceHandle = new(OneThousandTwoHundredThirtyFour);

    /// <summary>The second raw-input device handle used by tests.</summary>
    private static readonly IntPtr SecondDeviceHandle = new(FourThousandThreeHundredTwentyOne);

    /// <summary>The third raw-input device handle used by tests.</summary>
    private static readonly IntPtr ThirdDeviceHandle = new(SevenHundredSixtyEight);

    /// <summary>The message window handle used by tests.</summary>
    private static readonly IntPtr MessageWindowHandle = new(NineHundred);

    /// <summary>The raw-input message handle used by tests.</summary>
    private static readonly IntPtr RawInputMessageHandle = new(ThreeHundred);

    /// <summary>Tests raw-input registration mapping, remove semantics, and registration failures.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RawInputApi_RegisterRawInput_MapsDevicesAndFailuresAsync()
    {
        var fakeApi = new FakeRawInputNativeApi();
        var previousApi = RawInputApi.SetNativeApiForTesting(fakeApi);

        try
        {
            await Assert.That(static () => RawInputApi.CreateRawInputDevice(MessageWindowHandle, UnknownRawInputDevice, RawInputDeviceFlags.None)).Throws<NotSupportedException>();

            RawInputApi.RegisterRawInput(
                MessageWindowHandle,
                RawInputDeviceFlags.InputSink,
                RawInputDevices.Keyboard,
                RawInputDevices.ConsumerAudioControl);

            await Assert.That(fakeApi.Registrations.Count).IsEqualTo(One);
            await Assert.That(fakeApi.Registrations[0].Length).IsEqualTo(Two);
            await Assert.That(fakeApi.Registrations[0][0].UsagePage).IsEqualTo(HidUsagePages.Generic);
            await Assert.That(fakeApi.Registrations[0][0].Usage).IsEqualTo((ushort)HidUsagesGeneric.Keyboard);
            await Assert.That(fakeApi.Registrations[0][0].ToIntPtr()).IsEqualTo(MessageWindowHandle);
            await Assert.That(fakeApi.Registrations[0][1].UsagePage).IsEqualTo(HidUsagePages.Consumer);
            await Assert.That(fakeApi.Registrations[0][1].Usage).IsEqualTo((ushort)HidUsagesConsumer.ConsumerControl);

            RawInputApi.RegisterRawInput(MessageWindowHandle, RawInputDeviceFlags.Remove, RawInputDevices.Mouse);

            await Assert.That(fakeApi.Registrations.Count).IsEqualTo(Two);
            await Assert.That(fakeApi.Registrations[1][0].ToIntPtr()).IsEqualTo(IntPtr.Zero);

            fakeApi.RegisterResult = false;
            await Assert.That(static () => RawInputApi.RegisterRawInput(RawInputApi.CreateRawInputDevice(MessageWindowHandle, RawInputDevices.Mouse))).Throws<Win32Exception>();
        }
        finally
        {
            _ = RawInputApi.SetNativeApiForTesting(previousApi);
        }
    }

    /// <summary>Tests raw-input device enumeration, device info extraction, and native failure branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RawInputApi_GetAllDevicesAndInformation_CoversNativeBranchesAsync()
    {
        var fakeApi = CreateDeviceApi();
        var previousApi = RawInputApi.SetNativeApiForTesting(fakeApi);

        try
        {
            var devices = ToList(RawInputApi.GetAllDevices());

            await Assert.That(devices.Count).IsEqualTo(One);
            await Assert.That(devices[0].ToIntPtr()).IsEqualTo(FirstDeviceHandle);
            await Assert.That(devices[0].DeviceName).IsEqualTo(FirstDeviceName);
            await Assert.That(devices[0].DisplayName).IsEqualTo(FirstDeviceName);
            await Assert.That(devices[0].DeviceInfo.Type).IsEqualTo(RawInputDeviceTypes.Keyboard);
        }
        finally
        {
            _ = RawInputApi.SetNativeApiForTesting(previousApi);
        }

        await AssertDeviceListReturnsEmptyOnInitialNativeFailureAsync();
        await AssertGetAllDevicesThrowsOnSecondListFailureAsync();
        await AssertGetDeviceInformationThrowsOnNativeFailuresAsync();
    }

    /// <summary>Tests raw-input data retrieval through the composed native API.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RawInputApi_GetRawInputData_UsesComposedNativeApiAsync()
    {
        var fakeApi = new FakeRawInputNativeApi { RawInputDataReturn = Marshal.SizeOf<RawInput>(), NextRawInput = CreateRawKeyboardInput(FirstDeviceHandle), };
        var previousApi = RawInputApi.SetNativeApiForTesting(fakeApi);

        try
        {
            var size = 0;
            var copied = RawInputApi.GetRawInputData(
                RawInputMessageHandle,
                RawInputDataCommands.Input,
                out var rawInput,
                ref size,
                Marshal.SizeOf<RawInputHeader>());

            await Assert.That(copied).IsEqualTo(Marshal.SizeOf<RawInput>());
            await Assert.That(fakeApi.RawInputDataCalls).IsEqualTo(One);
            await Assert.That(rawInput.Header.Type).IsEqualTo(RawInputDeviceTypes.Keyboard);
            await Assert.That(rawInput.Header.ToIntPtr()).IsEqualTo(FirstDeviceHandle);
        }
        finally
        {
            _ = RawInputApi.SetNativeApiForTesting(previousApi);
        }
    }

    /// <summary>Tests raw HID and mouse structs read native payload values.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RawInputStructs_ReadNativeMouseAndHidPayloadsAsync()
    {
        var rawData = Marshal.AllocHGlobal(Three);
        try
        {
            Marshal.Copy([(byte)One, (byte)Two, (byte)Three], 0, rawData, Three);
            var hid = CreateRawHid(One, Three, rawData);
            await Assert.That(hid.GetData()).IsEquivalentTo([(byte)One, (byte)Two, (byte)Three]);
        }
        finally
        {
            Marshal.FreeHGlobal(rawData);
        }

        const MouseButtonStates expectedButtonState = (MouseButtonStates)((OneHundredTwenty << MouseButtonStateBits) | (int)MouseButtonStates.Wheel);
        var mouse = CreateRawMouse(MouseButtonStates.Wheel, OneHundredTwenty, Ten, -Twenty);
        await Assert.That(mouse.ButtonState).IsEqualTo(expectedButtonState);
        await Assert.That(mouse.WheelData).IsEqualTo((short)OneHundredTwenty);
        await Assert.That(mouse.X).IsEqualTo(Ten);
        await Assert.That(mouse.Y).IsEqualTo(-Twenty);
    }

    /// <summary>Tests remaining non-hook input structs and raw-device accessors.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InputTailStructs_CoverUnionsLowLevelAndRawDeviceAccessorsAsync()
    {
        var hardwareInput = new HardwareInput { UMsg = FortyTwo, ParamL = (short)Seven, WParamH = (short)Nine };
        await Assert.That(hardwareInput.UMsg).IsEqualTo(FortyTwo);
        await Assert.That(hardwareInput.ParamL).IsEqualTo((short)Seven);
        await Assert.That(hardwareInput.WParamH).IsEqualTo((short)Nine);

        var keyboardInput = KeyboardInput.ForKeyDown(VirtualKeyCode.KeyA);
        var keyboardUp = KeyboardInput.ForKeyUp(VirtualKeyCode.KeyB);
        var keyPress = KeyboardInput.ForKeyPress(VirtualKeyCode.KeyC);
        await Assert.That(keyboardInput.VirtualKeyCode).IsEqualTo(VirtualKeyCode.KeyA);
        await Assert.That(keyboardInput.ScanCode).IsEqualTo((ushort)0);
        await Assert.That(keyboardInput.KeyEventFlags).IsEqualTo(KeyEventFlags.None);
        await Assert.That(keyboardInput.Timestamp).IsGreaterThan(0U);
        await Assert.That(keyboardInput.ExtraInfo).IsEqualTo(UIntPtr.Zero);
        await Assert.That(keyboardUp.KeyEventFlags).IsEqualTo(KeyEventFlags.KeyUp);
        await Assert.That(keyPress.Length).IsEqualTo(Two);

        var mouseInput = MouseInput.MouseDown(MouseButtons.Left);
        var mouseUnion = new InputUnion { MouseInput = mouseInput };
        var keyboardUnion = new InputUnion { KeyboardInput = keyboardInput };
        var hardwareUnion = new InputUnion { HardwareInput = hardwareInput };
        await Assert.That(mouseUnion.MouseInput.MouseEventFlags).IsEqualTo(MouseEventFlags.LeftDown);
        await Assert.That(keyboardUnion.KeyboardInput.VirtualKeyCode).IsEqualTo(VirtualKeyCode.KeyA);
        await Assert.That(hardwareUnion.HardwareInput.UMsg).IsEqualTo(FortyTwo);

        var lowLevelKeyboard = CreateKeyboardLowLevelHookStruct(VirtualKeyCode.KeyD, UIntFour, ExtendedKeyFlags.Injected, UIntFortyTwo);
        await Assert.That(lowLevelKeyboard.VirtualKeyCode).IsEqualTo(VirtualKeyCode.KeyD);
        await Assert.That(lowLevelKeyboard.ScanCode).IsEqualTo(UIntFour);
        await Assert.That(lowLevelKeyboard.Flags).IsEqualTo(ExtendedKeyFlags.Injected);
        await Assert.That(lowLevelKeyboard.TimeStamp).IsEqualTo(UIntFortyTwo);
        await Assert.That(lowLevelKeyboard.ExtraInfo).IsEqualTo(UIntPtr.Zero);

        var lowLevelMouse = new MouseLowLevelHookStruct
        {
            Pt = new(Ten, Twenty),
            MouseData = UIntThirty,
            Flags = ExtendedMouseFlags.Injected,
            TimeStamp = UIntNinetyNine,
            ExtraInfo = (UIntPtr)UIntFour,
        };
        await Assert.That(lowLevelMouse.Pt).IsEqualTo(new(Ten, Twenty));
        await Assert.That(lowLevelMouse.MouseData).IsEqualTo(UIntThirty);
        await Assert.That(lowLevelMouse.Flags).IsEqualTo(ExtendedMouseFlags.Injected);
        await Assert.That(lowLevelMouse.TimeStamp).IsEqualTo(UIntNinetyNine);
        await Assert.That(lowLevelMouse.ExtraInfo).IsEqualTo((UIntPtr)UIntFour);

        var lastInput = CreateLastInputInfo(UIntFortyTwo);
        await Assert.That(lastInput.Size).IsEqualTo((uint)Marshal.SizeOf<LastInputInfo>());
        await Assert.That(lastInput.TickCountLastInput).IsEqualTo(UIntFortyTwo);
        await Assert.That(lastInput.LastInputTimeSpan).IsGreaterThan(TimeSpan.Zero);
        await Assert.That(lastInput.GetLastInputDateTime(TimeProvider.System) <= TimeProvider.System.GetLocalNow()).IsTrue();
        await Assert.That(LastInputInfo.Create().Size).IsEqualTo((uint)Marshal.SizeOf<LastInputInfo>());
    }

    /// <summary>Tests remaining raw device structs and raw-input device-info accessors.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RawDeviceStructs_CoverDeviceInfoAndUnionAccessorsAsync()
    {
        var rawInputDevice = RawInputApi.CreateRawInputDevice(MessageWindowHandle, RawInputDevices.Keyboard);
        await Assert.That(rawInputDevice.ToString()).Contains("Generic/6");

        var rawInputDeviceList = CreateRawInputDeviceList(SecondDeviceHandle, RawInputDeviceTypes.HID);
        await Assert.That(rawInputDeviceList.RawInputDeviceType).IsEqualTo(RawInputDeviceTypes.HID);
        await Assert.That(rawInputDeviceList.ToIntPtr()).IsEqualTo(SecondDeviceHandle);

        var rawDevice = default(RawDevice);
        await Assert.That(rawDevice.Mouse).IsEqualTo(default);
        await Assert.That(rawDevice.Keyboard).IsEqualTo(default);
        await Assert.That(rawDevice.HID).IsEqualTo(default);

        var mouseInfo = new RawInputDeviceInfo(RawInputDeviceTypes.Mouse);
        var keyboardInfo = new RawInputDeviceInfo(RawInputDeviceTypes.Keyboard);
        var hidInfo = new RawInputDeviceInfo(RawInputDeviceTypes.HID);
        await Assert.That(mouseInfo.Mouse.Id).IsEqualTo(0);
        await Assert.That(mouseInfo.Mouse.NumberOfButtons).IsEqualTo(0);
        await Assert.That(mouseInfo.Mouse.SampleRate).IsEqualTo(0);
        await Assert.That(mouseInfo.Mouse.HasHorizontalWheel).IsFalse();
        await Assert.That(keyboardInfo.Keyboard.Type).IsEqualTo(0);
        await Assert.That(keyboardInfo.Keyboard.SubType).IsEqualTo(0);
        await Assert.That(keyboardInfo.Keyboard.KeyboardMode).IsEqualTo(0);
        await Assert.That(keyboardInfo.Keyboard.NumberOfFunctionKeys).IsEqualTo(0);
        await Assert.That(keyboardInfo.Keyboard.NumberOfIndicators).IsEqualTo(0);
        await Assert.That(keyboardInfo.Keyboard.NumberOfKeysTotal).IsEqualTo(0);
        await Assert.That(hidInfo.HID.VendorId).IsEqualTo(0);
        await Assert.That(hidInfo.HID.ProductId).IsEqualTo(0);
        await Assert.That(hidInfo.HID.VersionNumber).IsEqualTo(0);
        await Assert.That(hidInfo.HID.UsagePage).IsEqualTo((ushort)0);
        await Assert.That(hidInfo.HID.Usage).IsEqualTo((ushort)0);
        await Assert.That(() => mouseInfo.Keyboard).Throws<NotSupportedException>();
        await Assert.That(() => keyboardInfo.HID).Throws<NotSupportedException>();
        await Assert.That(() => hidInfo.Mouse).Throws<NotSupportedException>();

        var rawKeyboard = default(RawKeyboard);
        await Assert.That(rawKeyboard.VirtualKey).IsEqualTo(VirtualKeyCode.None);
        await Assert.That(rawKeyboard.Flags).IsEqualTo(RawKeyboardFlags.None);
        await Assert.That(rawKeyboard.ScanCode).IsEqualTo((ushort)0);
        await Assert.That(rawKeyboard.ToString()).Contains("Rawkeyboard");
    }

    /// <summary>Tests MouseInput factory overloads without sending any OS input.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MouseInputFactories_CoverButtonWheelMoveAndLocationPathsAsync()
    {
        var wheel = MouseInput.MoveMouseWheel(OneHundredTwenty);
        var wheelAtLocation = MouseInput.MoveMouseWheel(-OneHundredTwenty, new(Ten, Twenty));
        var mouseMove = MouseInput.MouseMove(new(Thirty, Forty));
        var down = MouseInput.MouseDown(MouseButtons.Left | MouseButtons.Middle | MouseButtons.XButton1);
        var downAtLocation = MouseInput.MouseDown(MouseButtons.XButton2, new(Ten, Twenty));
        var up = MouseInput.MouseUp(MouseButtons.Right | MouseButtons.Middle | MouseButtons.XButton2);
        var upAtLocation = MouseInput.MouseUp(MouseButtons.XButton1, new(Thirty, Forty));

        await Assert.That(wheel.MouseEventFlags).IsEqualTo(MouseEventFlags.Wheel);
        await Assert.That(wheel.MouseData).IsEqualTo(OneHundredTwenty);
        await Assert.That(wheel.ExtraInfo).IsEqualTo(UIntPtr.Zero);
        await Assert.That(wheelAtLocation.MouseEventFlags).HasFlag(MouseEventFlags.Wheel);
        await Assert.That(wheelAtLocation.MouseEventFlags).HasFlag(MouseEventFlags.Move);
        await Assert.That(wheelAtLocation.MouseData).IsEqualTo(-OneHundredTwenty);
        await Assert.That(mouseMove.MouseEventFlags).IsEqualTo(MouseEventFlags.Absolute | MouseEventFlags.Virtualdesk | MouseEventFlags.Move);
        await Assert.That(mouseMove.Timestamp).IsGreaterThan(0U);
        await Assert.That(down.MouseEventFlags).IsEqualTo(MouseEventFlags.LeftDown | MouseEventFlags.MiddleDown | MouseEventFlags.XDown);
        await Assert.That(down.MouseData).IsEqualTo(1);
        await Assert.That(downAtLocation.MouseEventFlags).HasFlag(MouseEventFlags.XDown);
        await Assert.That(downAtLocation.MouseData).IsEqualTo(Two);
        await Assert.That(up.MouseEventFlags).IsEqualTo(MouseEventFlags.RightUp | MouseEventFlags.MiddleUp | MouseEventFlags.XUp);
        await Assert.That(up.MouseData).IsEqualTo(Two);
        await Assert.That(upAtLocation.MouseEventFlags).HasFlag(MouseEventFlags.XUp);
        await Assert.That(upAtLocation.MouseData).IsEqualTo(1);
    }

    /// <summary>Tests WindowsNativeInputApi through overridden native operations without OS input side effects.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsNativeInputApi_UsesOverriddenOperationsAsync()
    {
        var recorder = new NativeInputOperationRecorder();
        using var operations = WindowsNativeInputApi.OverrideOperationsForTesting(recorder.GetLastInputInfo, recorder.SendInput);
        var lastInputInfo = LastInputInfo.Create();
        var input = Input.CreateKeyboardInputs(KeyboardInput.ForKeyDown(VirtualKeyCode.KeyA, UIntFortyTwo));

        await Assert.That(WindowsNativeInputApi.Instance.GetLastInputInfo(ref lastInputInfo)).IsTrue();
        await Assert.That(lastInputInfo.TickCountLastInput).IsEqualTo(UIntNinetyNine);
        await Assert.That(WindowsNativeInputApi.Instance.SendInput(input)).IsEqualTo(UIntFour);
        await Assert.That(recorder.LastInputInfoCalls).IsEqualTo(One);
        await Assert.That(recorder.SendInputCalls).IsEqualTo(One);
        await Assert.That(recorder.NumberOfInputs).IsEqualTo((uint)input.Length);
        await Assert.That(recorder.InputSize).IsEqualTo(Input.Size);
    }

    /// <summary>Tests the raw-input monitor transforms shared window messages into raw-input events.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RawInputMonitor_TransformsMessagesAndRegistersOnHandleAsync()
    {
        var source = new FakeRawInputMessageSource();
        var fakeApi = new FakeRawInputNativeApi { RawInputDataReturn = Marshal.SizeOf<RawInput>(), NextRawInput = CreateRawKeyboardInput(FirstDeviceHandle), };
        var previousApi = RawInputApi.SetNativeApiForTesting(fakeApi);

        try
        {
            using var sourceScope = RawInputMonitor.Infrastructure.OverrideMessageSourceForTesting(source);
            var observed = new List<RawInputEventArgs>();
            using var subscription = RawInputMonitor.ObserveRawInput(RawInputDevices.Keyboard).SubscribeOnNext(observed.Add);

            source.PublishHandle(MessageWindowHandle);

            await Assert.That(fakeApi.Registrations.Count).IsEqualTo(One);
            await Assert.That(fakeApi.Registrations[0][0].Flags).IsEqualTo(RawInputDeviceFlags.InputSink | RawInputDeviceFlags.DeviceNotify);

            var foregroundMessage = source.PublishMessage(WindowsMessages.WM_INPUT, 0, RawInputMessageHandle);
            var backgroundMessage = source.PublishMessage(WindowsMessages.WM_INPUT, One, RawInputMessageHandle);
            fakeApi.RawInputDataReturn = -One;
            var failedMessage = source.PublishMessage(WindowsMessages.WM_INPUT, One, RawInputMessageHandle);

            await Assert.That(observed.Count).IsEqualTo(Two);
            await Assert.That(observed[0].IsForeground).IsTrue();
            await Assert.That(observed[1].IsForeground).IsFalse();
            await Assert.That(observed[0].RawInput.Header.ToIntPtr()).IsEqualTo(FirstDeviceHandle);
            await Assert.That(foregroundMessage.Handled).IsTrue();
            await Assert.That(backgroundMessage.Handled).IsTrue();
            await Assert.That(failedMessage.Handled).IsTrue();
        }
        finally
        {
            _ = RawInputApi.SetNativeApiForTesting(previousApi);
            RawInputMonitor.ResetForTesting();
        }
    }

    /// <summary>Tests the raw-input device monitor refreshes, registers, publishes add/remove events, and maintains its cache.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RawInputDeviceMonitor_TracksDeviceChangesAndCacheAsync()
    {
        var source = new FakeRawInputMessageSource();
        var fakeApi = CreateDeviceApi();
        fakeApi.DeviceHandles.Add(SecondDeviceHandle);
        fakeApi.DeviceNames[SecondDeviceHandle] = SecondDeviceName;
        fakeApi.DeviceTypes[SecondDeviceHandle] = RawInputDeviceTypes.Mouse;
        var previousApi = RawInputApi.SetNativeApiForTesting(fakeApi);

        try
        {
            using var sourceScope = RawInputMonitor.Infrastructure.OverrideMessageSourceForTesting(source);
            var initialSnapshot = RawInputDeviceMonitor.GetDevicesSnapshot();
            var observed = new List<RawInputDeviceChangeEventArgs>();
            using var subscription = RawInputDeviceMonitor.ObserveDeviceChanges(RawInputDevices.Keyboard).SubscribeOnNext(observed.Add);

            source.PublishHandle(MessageWindowHandle);
            _ = source.PublishMessage(WindowsMessages.WM_INPUT_DEVICE_CHANGE, One, SecondDeviceHandle);
            _ = source.PublishMessage(WindowsMessages.WM_INPUT_DEVICE_CHANGE, 0, SecondDeviceHandle);
            _ = source.PublishMessage(WindowsMessages.WM_INPUT_DEVICE_CHANGE, 0, ThirdDeviceHandle);
            var finalSnapshot = RawInputDeviceMonitor.GetDevicesSnapshot();

            await Assert.That(initialSnapshot.Count).IsEqualTo(Two);
            await Assert.That(fakeApi.Registrations.Count).IsEqualTo(One);
            await Assert.That(fakeApi.Registrations[0][0].Flags).IsEqualTo(RawInputDeviceFlags.DeviceNotify);
            await Assert.That(observed.Count).IsEqualTo(Three);
            await Assert.That(observed[0].Added).IsTrue();
            await Assert.That(observed[0].DeviceInformation.ToIntPtr()).IsEqualTo(SecondDeviceHandle);
            await Assert.That(observed[1].Added).IsFalse();
            await Assert.That(observed[2].DeviceInformation.ToIntPtr()).IsEqualTo(ThirdDeviceHandle);
            await Assert.That(finalSnapshot.ContainsKey(FirstDeviceHandle)).IsTrue();
        }
        finally
        {
            _ = RawInputApi.SetNativeApiForTesting(previousApi);
            RawInputDeviceMonitor.ResetForTesting();
        }
    }

    /// <summary>Creates a fake native API with one keyboard device.</summary>
    /// <returns>The configured fake API.</returns>
    private static FakeRawInputNativeApi CreateDeviceApi()
    {
        var fakeApi = new FakeRawInputNativeApi();
        fakeApi.DeviceHandles.Add(FirstDeviceHandle);
        fakeApi.DeviceNames[FirstDeviceHandle] = FirstDeviceName;
        fakeApi.DeviceTypes[FirstDeviceHandle] = RawInputDeviceTypes.Keyboard;
        return fakeApi;
    }

    /// <summary>Asserts device enumeration returns empty on an initial native failure.</summary>
    /// <returns>A task representing the asynchronous assertion.</returns>
    private static async Task AssertDeviceListReturnsEmptyOnInitialNativeFailureAsync()
    {
        var fakeApi = new FakeRawInputNativeApi { DeviceListInitialReturn = UIntFour };
        var previousApi = RawInputApi.SetNativeApiForTesting(fakeApi);

        try
        {
            await Assert.That(ToList(RawInputApi.GetAllDevices()).Count).IsEqualTo(0);
        }
        finally
        {
            _ = RawInputApi.SetNativeApiForTesting(previousApi);
        }
    }

    /// <summary>Asserts device enumeration throws when the second native device-list call fails.</summary>
    /// <returns>A task representing the asynchronous assertion.</returns>
    private static async Task AssertGetAllDevicesThrowsOnSecondListFailureAsync()
    {
        var fakeApi = CreateDeviceApi();
        fakeApi.FailSecondDeviceListCall = true;
        var previousApi = RawInputApi.SetNativeApiForTesting(fakeApi);

        try
        {
            await Assert.That(static () => ToList(RawInputApi.GetAllDevices())).Throws<Win32Exception>();
        }
        finally
        {
            _ = RawInputApi.SetNativeApiForTesting(previousApi);
        }
    }

    /// <summary>Asserts device information throws for each native failure point.</summary>
    /// <returns>A task representing the asynchronous assertion.</returns>
    private static async Task AssertGetDeviceInformationThrowsOnNativeFailuresAsync()
    {
        await AssertGetDeviceInformationFailureAsync(static api => api.FailDeviceNameSize = true);
        await AssertGetDeviceInformationFailureAsync(static api => api.FailDeviceNameCopy = true);
        await AssertGetDeviceInformationFailureAsync(static api => api.FailDeviceInfoSize = true);
        await AssertGetDeviceInformationFailureAsync(static api => api.FailDeviceInfoCopy = true);
    }

    /// <summary>Asserts one configured native device information failure.</summary>
    /// <param name="configure">Configures the failure branch.</param>
    /// <returns>A task representing the asynchronous assertion.</returns>
    private static async Task AssertGetDeviceInformationFailureAsync(Action<FakeRawInputNativeApi> configure)
    {
        var fakeApi = CreateDeviceApi();
        configure(fakeApi);
        var previousApi = RawInputApi.SetNativeApiForTesting(fakeApi);

        try
        {
            await Assert.That(static () => RawInputApi.GetDeviceInformation(FirstDeviceHandle)).Throws<Win32Exception>();
        }
        finally
        {
            _ = RawInputApi.SetNativeApiForTesting(previousApi);
        }
    }

    /// <summary>Creates a raw keyboard input record for the supplied handle.</summary>
    /// <param name="deviceHandle">The raw-input device handle.</param>
    /// <returns>The raw input record.</returns>
    private static RawInput CreateRawKeyboardInput(IntPtr deviceHandle) =>
        new() { Header = new RawInputHeader { Type = RawInputDeviceTypes.Keyboard, DeviceHandle = deviceHandle }, Device = default, };

    /// <summary>Creates a last-input info struct with native fields populated.</summary>
    /// <param name="tickCount">The last-input tick count.</param>
    /// <returns>The populated last-input info struct.</returns>
    private static LastInputInfo CreateLastInputInfo(uint tickCount)
    {
        var buffer = Marshal.AllocHGlobal(Marshal.SizeOf<LastInputInfo>());
        try
        {
            Marshal.WriteInt32(buffer, Marshal.SizeOf<LastInputInfo>());
            Marshal.WriteInt32(buffer, LastInputTimeOffset, unchecked((int)tickCount));
            return Marshal.PtrToStructure<LastInputInfo>(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>Creates a low-level keyboard hook struct with native fields populated.</summary>
    /// <param name="virtualKeyCode">The virtual-key code.</param>
    /// <param name="scanCode">The scan code.</param>
    /// <param name="flags">The keyboard flags.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns>The populated low-level keyboard hook struct.</returns>
    private static KeyboardLowLevelHookStruct CreateKeyboardLowLevelHookStruct(
        VirtualKeyCode virtualKeyCode,
        uint scanCode,
        ExtendedKeyFlags flags,
        uint timestamp)
    {
        var buffer = Marshal.AllocHGlobal(Marshal.SizeOf<KeyboardLowLevelHookStruct>());
        try
        {
            Marshal.WriteInt32(buffer, KeyboardLowLevelVirtualKeyOffset, (int)virtualKeyCode);
            Marshal.WriteInt32(buffer, KeyboardLowLevelScanCodeOffset, unchecked((int)scanCode));
            Marshal.WriteInt32(buffer, KeyboardLowLevelFlagsOffset, (int)flags);
            Marshal.WriteInt32(buffer, KeyboardLowLevelTimestampOffset, unchecked((int)timestamp));
            Marshal.WriteIntPtr(buffer, KeyboardLowLevelExtraInfoOffset, IntPtr.Zero);
            return Marshal.PtrToStructure<KeyboardLowLevelHookStruct>(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>Creates a raw-input device-list struct with private native fields populated.</summary>
    /// <param name="deviceHandle">The raw-input device handle.</param>
    /// <param name="deviceType">The raw-input device type.</param>
    /// <returns>The populated device-list struct.</returns>
    private static RawInputDeviceList CreateRawInputDeviceList(IntPtr deviceHandle, RawInputDeviceTypes deviceType)
    {
        var buffer = Marshal.AllocHGlobal(Marshal.SizeOf<RawInputDeviceList>());
        try
        {
            Marshal.WriteIntPtr(buffer, deviceHandle);
            Marshal.WriteInt32(buffer, IntPtr.Size, (int)deviceType);
            return Marshal.PtrToStructure<RawInputDeviceList>(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>Creates a raw HID struct with native fields populated.</summary>
    /// <param name="inputSize">The HID input size.</param>
    /// <param name="inputCount">The HID input count.</param>
    /// <param name="rawData">The native data pointer.</param>
    /// <returns>The populated raw HID struct.</returns>
    private static RawHID CreateRawHid(int inputSize, int inputCount, IntPtr rawData)
    {
        var buffer = Marshal.AllocHGlobal(Marshal.SizeOf<RawHID>());
        try
        {
            Marshal.WriteInt32(buffer, inputSize);
            Marshal.WriteInt32(buffer, RawHidInputCountOffset, inputCount);
            Marshal.WriteIntPtr(buffer, RawHidDataPointerOffset, rawData);
            return Marshal.PtrToStructure<RawHID>(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>Creates a raw mouse struct with native fields populated.</summary>
    /// <param name="buttonState">The button state.</param>
    /// <param name="wheelData">The wheel data.</param>
    /// <param name="x">The X delta.</param>
    /// <param name="y">The Y delta.</param>
    /// <returns>The populated raw mouse struct.</returns>
    private static RawMouse CreateRawMouse(MouseButtonStates buttonState, int wheelData, int x, int y)
    {
        var buffer = Marshal.AllocHGlobal(Marshal.SizeOf<RawMouse>());
        try
        {
            Marshal.WriteInt32(buffer, RawMouseButtonFlagsOffset, (int)buttonState);
            Marshal.WriteInt16(buffer, RawMouseWheelDataOffset, (short)wheelData);
            Marshal.WriteInt32(buffer, RawMouseXOffset, x);
            Marshal.WriteInt32(buffer, RawMouseYOffset, y);
            return Marshal.PtrToStructure<RawMouse>(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>Materializes an enumerable without relying on LINQ.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="values">The values to materialize.</param>
    /// <returns>A list containing the values.</returns>
    private static List<T> ToList<T>(IEnumerable<T> values)
    {
        var result = new List<T>();
        foreach (var value in values)
        {
            result.Add(value);
        }

        return result;
    }

    /// <summary>Records Windows native input API calls without invoking user32.</summary>
    private sealed class NativeInputOperationRecorder
    {
        /// <summary>Gets the number of last-input calls.</summary>
        public int LastInputInfoCalls { get; private set; }

        /// <summary>Gets the number of send-input calls.</summary>
        public int SendInputCalls { get; private set; }

        /// <summary>Gets the last number of input records supplied to SendInput.</summary>
        public uint NumberOfInputs { get; private set; }

        /// <summary>Gets the last input size supplied to SendInput.</summary>
        public int InputSize { get; private set; }

        /// <summary>Fills deterministic last-input information.</summary>
        /// <param name="lastInputInfo">The last-input information to fill.</param>
        /// <returns><see langword="true"/>.</returns>
        public bool GetLastInputInfo(ref LastInputInfo lastInputInfo)
        {
            LastInputInfoCalls++;
            lastInputInfo = CreateLastInputInfo(UIntNinetyNine);
            return true;
        }

        /// <summary>Records deterministic send-input information.</summary>
        /// <param name="numberOfInputs">The supplied input count.</param>
        /// <param name="inputs">The supplied input records.</param>
        /// <param name="inputSize">The supplied input size.</param>
        /// <returns>A deterministic sent-input count.</returns>
        public uint SendInput(uint numberOfInputs, Input[] inputs, int inputSize)
        {
            _ = inputs;
            SendInputCalls++;
            NumberOfInputs = numberOfInputs;
            InputSize = inputSize;
            return UIntFour;
        }
    }

    /// <summary>Deterministic raw-input message source.</summary>
    private sealed class FakeRawInputMessageSource : IRawInputMessageSource
    {
        /// <summary>The message stream.</summary>
        private readonly ManualObservable<WindowMessage> _messages = new();

        /// <summary>The handle stream.</summary>
        private readonly ManualObservable<long> _handles = new();

        /// <inheritdoc />
        public IObservable<WindowMessage> Messages => _messages;

        /// <inheritdoc />
        public IObservable<long> ObserveHandleChanges() => _handles;

        /// <summary>Publishes a handle change.</summary>
        /// <param name="windowHandle">The message-window handle.</param>
        public void PublishHandle(IntPtr windowHandle) => _handles.OnNext(windowHandle.ToInt64());

        /// <summary>Publishes a raw-input window message.</summary>
        /// <param name="message">The Windows message.</param>
        /// <param name="wordParameter">The word parameter.</param>
        /// <param name="longParameter">The long parameter.</param>
        /// <returns>The published message instance.</returns>
        public WindowMessage PublishMessage(WindowsMessages message, int wordParameter, IntPtr longParameter)
        {
            var windowMessage = new WindowMessage(MessageWindowHandle, message, (nint)wordParameter, longParameter);
            _messages.OnNext(windowMessage);
            return windowMessage;
        }
    }

    /// <summary>Small manual observable for deterministic monitor tests.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    private sealed class ManualObservable<T> : IObservable<T>
    {
        /// <summary>The active observers.</summary>
        private readonly List<IObserver<T>> _observers = [];

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observers.Add(observer);
            return new Subscription(this, observer);
        }

        /// <summary>Publishes a value to active observers.</summary>
        /// <param name="value">The value to publish.</param>
        public void OnNext(T value)
        {
            var snapshot = _observers.ToArray();
            foreach (var observer in snapshot)
            {
                observer.OnNext(value);
            }
        }

        /// <summary>Removes an observer.</summary>
        /// <param name="observer">The observer to remove.</param>
        private void Unsubscribe(IObserver<T> observer) => _ = _observers.Remove(observer);

        /// <summary>Manual observable subscription.</summary>
        /// <param name="owner">The owning observable.</param>
        /// <param name="observer">The subscribed observer.</param>
        private sealed class Subscription(ManualObservable<T> owner, IObserver<T> observer) : IDisposable
        {
            /// <summary>Tracks disposal.</summary>
            private bool _disposed;

            /// <inheritdoc />
            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    owner.Unsubscribe(observer);
                }
            }
        }
    }

    /// <summary>Deterministic raw-input native API.</summary>
    private sealed class FakeRawInputNativeApi : IRawInputNativeApi
    {
        /// <summary>Gets configured raw-input device handles.</summary>
        public List<IntPtr> DeviceHandles { get; } = [];

        /// <summary>Gets configured raw-input device names.</summary>
        public Dictionary<IntPtr, string> DeviceNames { get; } = [];

        /// <summary>Gets configured raw-input device types.</summary>
        public Dictionary<IntPtr, RawInputDeviceTypes> DeviceTypes { get; } = [];

        /// <summary>Gets captured raw-input registrations.</summary>
        public List<RawInputDevice[]> Registrations { get; } = [];

        /// <summary>Gets or sets the first device-list call return value.</summary>
        public uint DeviceListInitialReturn { get; set; }

        /// <summary>Gets or sets a value indicating whether the second device-list call fails.</summary>
        public bool FailSecondDeviceListCall { get; set; }

        /// <summary>Gets or sets a value indicating whether the device-name size query fails.</summary>
        public bool FailDeviceNameSize { get; set; }

        /// <summary>Gets or sets a value indicating whether the device-name copy query fails.</summary>
        public bool FailDeviceNameCopy { get; set; }

        /// <summary>Gets or sets a value indicating whether the device-info size query fails.</summary>
        public bool FailDeviceInfoSize { get; set; }

        /// <summary>Gets or sets a value indicating whether the device-info copy query fails.</summary>
        public bool FailDeviceInfoCopy { get; set; }

        /// <summary>Gets or sets a value indicating whether registration succeeds.</summary>
        public bool RegisterResult { get; set; } = true;

        /// <summary>Gets or sets the raw-input data return value.</summary>
        public int RawInputDataReturn { get; set; }

        /// <summary>Gets or sets the next raw-input data record.</summary>
        public RawInput NextRawInput { get; set; }

        /// <summary>Gets the number of raw-input data calls.</summary>
        public int RawInputDataCalls { get; private set; }

        /// <inheritdoc />
        public uint GetRawInputDeviceList(RawInputDeviceList[] rawInputDeviceList, ref uint numDevices, uint size)
        {
            if (rawInputDeviceList is null)
            {
                numDevices = (uint)DeviceHandles.Count;
                return DeviceListInitialReturn;
            }

            if (FailSecondDeviceListCall)
            {
                return uint.MaxValue;
            }

            for (var index = 0; index < rawInputDeviceList.Length; index++)
            {
                var handle = DeviceHandles[index];
                rawInputDeviceList[index] = CreateRawInputDeviceList(handle, DeviceTypes[handle]);
            }

            return (uint)rawInputDeviceList.Length;
        }

        /// <inheritdoc />
        public uint GetRawInputDeviceInfo(IntPtr deviceHandle, RawInputDeviceInfoCommands command, IntPtr deviceName, ref uint dataSize) =>
            command == RawInputDeviceInfoCommands.DeviceName
                ? GetDeviceName(deviceHandle, deviceName, ref dataSize)
                : GetDeviceInfo(deviceHandle, deviceName, ref dataSize);

        /// <inheritdoc />
        public bool RegisterRawInputDevices(RawInputDevice[] rawInputDevices, int numberOfDevices, int size)
        {
            var captured = new RawInputDevice[numberOfDevices];
            Array.Copy(rawInputDevices, captured, numberOfDevices);
            Registrations.Add(captured);
            return RegisterResult;
        }

        /// <inheritdoc />
        public int GetRawInputData(IntPtr rawInputHandle, RawInputDataCommands command, ref RawInput data, ref int size, int headerSize)
        {
            RawInputDataCalls++;
            data = NextRawInput;
            size = Marshal.SizeOf<RawInput>();
            return RawInputDataReturn;
        }

        /// <summary>Creates a raw-input device-list struct with private native fields populated.</summary>
        /// <param name="deviceHandle">The raw-input device handle.</param>
        /// <param name="deviceType">The raw-input device type.</param>
        /// <returns>The populated device-list struct.</returns>
        private static RawInputDeviceList CreateRawInputDeviceList(IntPtr deviceHandle, RawInputDeviceTypes deviceType)
        {
            var buffer = Marshal.AllocHGlobal(Marshal.SizeOf<RawInputDeviceList>());
            try
            {
                Marshal.WriteIntPtr(buffer, deviceHandle);
                Marshal.WriteInt32(buffer, IntPtr.Size, (int)deviceType);
                return Marshal.PtrToStructure<RawInputDeviceList>(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>Gets a fake device name response.</summary>
        /// <param name="deviceHandle">The device handle.</param>
        /// <param name="destination">The destination buffer.</param>
        /// <param name="dataSize">The data size.</param>
        /// <returns>The native return value.</returns>
        private uint GetDeviceName(IntPtr deviceHandle, IntPtr destination, ref uint dataSize)
        {
            if (destination == IntPtr.Zero)
            {
                if (FailDeviceNameSize)
                {
                    return uint.MaxValue;
                }

                dataSize = (uint)(DeviceNames[deviceHandle].Length + One);
                return 0;
            }

            if (FailDeviceNameCopy)
            {
                return uint.MaxValue;
            }

            var deviceName = $"{DeviceNames[deviceHandle]}\0";
            var bytes = Encoding.Unicode.GetBytes(deviceName);
            Marshal.Copy(bytes, 0, destination, bytes.Length);
            dataSize = (uint)deviceName.Length;
            return dataSize;
        }

        /// <summary>Gets a fake device-info response.</summary>
        /// <param name="deviceHandle">The device handle.</param>
        /// <param name="destination">The destination buffer.</param>
        /// <param name="dataSize">The data size.</param>
        /// <returns>The native return value.</returns>
        private uint GetDeviceInfo(IntPtr deviceHandle, IntPtr destination, ref uint dataSize)
        {
            if (destination == IntPtr.Zero)
            {
                if (FailDeviceInfoSize)
                {
                    return uint.MaxValue;
                }

                dataSize = (uint)Marshal.SizeOf<RawInputDeviceInfo>();
                return 0;
            }

            if (FailDeviceInfoCopy)
            {
                return uint.MaxValue;
            }

            var deviceInfo = new RawInputDeviceInfo(DeviceTypes[deviceHandle]);
            Marshal.StructureToPtr(deviceInfo, destination, false);
            dataSize = (uint)deviceInfo.Size;
            return dataSize;
        }
    }
}
