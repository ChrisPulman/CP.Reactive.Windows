// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Additional deterministic coverage for Input helpers, handlers, event args, structs, and monitor paths.</summary>
public class InputCoverageAdditionalTests
{
    /// <summary>The fixed timestamp used by deterministic input struct tests.</summary>
    private const uint TestTimestamp = 1234;

    /// <summary>The raw input handle used by deterministic raw input tests.</summary>
    private static readonly IntPtr RawInputHandle = new(0x123456);

    /// <summary>Tests KeyHelper parsing aliases, single characters, invalid values, and combination filtering.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyHelper_ParsesAliasesSingleCharactersAndCombinationsAsync()
    {
        await Assert.That(KeyHelper.VirtualKeyCodeFromString(string.Empty)).IsEqualTo(VirtualKeyCode.None);
        await Assert.That(KeyHelper.VirtualKeyCodeFromString("a")).IsEqualTo(VirtualKeyCode.KeyA);
        await Assert.That(KeyHelper.VirtualKeyCodeFromString("ALT")).IsEqualTo(VirtualKeyCode.Menu);
        await Assert.That(KeyHelper.VirtualKeyCodeFromString("ctrl")).IsEqualTo(VirtualKeyCode.Control);
        await Assert.That(KeyHelper.VirtualKeyCodeFromString("win")).IsEqualTo(VirtualKeyCode.LeftWin);
        await Assert.That(KeyHelper.VirtualKeyCodeFromString("unknown")).IsEqualTo(VirtualKeyCode.None);

        var parsed = ToList(KeyHelper.VirtualKeyCodesFromString(" + ctrl + shift + win + B + nope + "));
        await Assert.That(parsed.Count).IsEqualTo(Four);
        await Assert.That(parsed[0]).IsEqualTo(VirtualKeyCode.Control);
        await Assert.That(parsed[1]).IsEqualTo(VirtualKeyCode.Shift);
        await Assert.That(parsed[2]).IsEqualTo(VirtualKeyCode.LeftWin);
        await Assert.That(parsed[3]).IsEqualTo(VirtualKeyCode.KeyB);

        await Assert.That(ToList(KeyHelper.VirtualKeyCodesFromString(string.Empty)).Count).IsEqualTo(0);
    }

    /// <summary>Tests locale display text safe fallbacks and special-key branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyHelper_DisplayText_HandlesSpecialAndFallbackKeysAsync()
    {
        await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.None)).IsEqualTo(nameof(VirtualKeyCode.None));
        await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.KeyA, false)).IsNotEmpty();
        await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.LeftShift, true)).IsNotEmpty();
        await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.RightControl, true)).IsNotEmpty();
        await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.Print, false)).IsNotEmpty();
        await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.Pause, false)).IsNotEmpty();
        await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.Multiply, false)).Contains("*");
        await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.Divide, false)).Contains("/");
    }

    /// <summary>Tests KeyCombinationHandler state, modifiers, repeat, pass-through, and injected filtering.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyCombinationHandler_CoversStateAndPolicyBranchesAsync()
    {
        var handler = new KeyCombinationHandler(VirtualKeyCode.Control, VirtualKeyCode.Control, VirtualKeyCode.KeyA);
        await Assert.That(handler.TriggerCombination.Length).IsEqualTo(Two);
        await Assert.That(handler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.LeftControl))).IsFalse();
        await Assert.That(handler.HasKeysPressed).IsTrue();
        await Assert.That(handler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyA))).IsTrue();

        handler.Configure([VirtualKeyCode.Shift]);
        await Assert.That(handler.HasKeysPressed).IsFalse();
        await Assert.That(handler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.RightShift))).IsTrue();

        var repeatHandler = new KeyCombinationHandler(VirtualKeyCode.KeyB) { CanRepeat = true, IsPassThrough = true };
        var first = KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyB);
        var second = KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyB);
        await Assert.That(repeatHandler.Handle(first)).IsTrue();
        await Assert.That(repeatHandler.Handle(second)).IsTrue();
        await Assert.That(first.Handled).IsFalse();

        var injectedHandler = new KeyCombinationHandler(VirtualKeyCode.KeyC);
        var injected = KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyC);
        injected.Flags = ExtendedKeyFlags.Injected;
        await Assert.That(injectedHandler.Handle(injected)).IsFalse();
        await Assert.That(injected.IsInjectedByProcess).IsTrue();
    }

    /// <summary>Tests key-up triggering only fires after the complete combination was held and no other key is down.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyCombinationHandler_KeyUpTrigger_CoversOtherKeyBranchesAsync()
    {
        var handler = new KeyCombinationHandler(VirtualKeyCode.Control, VirtualKeyCode.KeyA) { TriggerOnKeyUp = true };

        await Assert.That(handler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.LeftControl))).IsFalse();
        await Assert.That(handler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyA))).IsFalse();
        await Assert.That(handler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyB))).IsFalse();
        await Assert.That(handler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyA))).IsFalse();
        await Assert.That(handler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyB))).IsFalse();
        await Assert.That(handler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyA))).IsFalse();

        var keyUp = KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyA);
        await Assert.That(handler.Handle(keyUp)).IsTrue();
        await Assert.That(keyUp.Handled).IsTrue();
    }

    /// <summary>Tests sequence and either/or handlers with timeout, reset, and reconfiguration paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeySequenceAndOrHandlers_CoverProgressTimeoutAndResetAsync()
    {
        var orHandler = new KeyOrCombinationHandler(
            new KeyCombinationHandler(VirtualKeyCode.KeyA),
            new KeyCombinationHandler(VirtualKeyCode.KeyB));

        await Assert.That(orHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyA))).IsTrue();
        await Assert.That(orHandler.HasKeysPressed).IsTrue();
        await Assert.That(orHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyA))).IsFalse();

        var sequence = new KeySequenceHandler(
            new KeyCombinationHandler(VirtualKeyCode.KeyC),
            new KeyCombinationHandler(VirtualKeyCode.KeyD))
        { Timeout = TimeSpan.FromMilliseconds(1), };

        await Assert.That(sequence.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyC))).IsFalse();
        await Assert.That(sequence.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyC))).IsFalse();
        await Task.Delay(Twenty, CancellationToken.None);
        await Assert.That(sequence.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyD))).IsFalse();

        sequence.Configure([
            new KeyCombinationHandler(VirtualKeyCode.KeyC),
            new KeyCombinationHandler(VirtualKeyCode.KeyD),
        ]);
        sequence.Timeout = null;

        await Assert.That(sequence.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyC))).IsFalse();
        await Assert.That(sequence.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyC))).IsFalse();
        await Assert.That(sequence.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyD))).IsTrue();
        await Assert.That(sequence.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyD))).IsFalse();
    }

    /// <summary>Tests keyboard hook event argument flags, combined modifiers, event time, and string formatting.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyboardHookEventArgs_CoversFlagsTimeAndStringFormattingAsync()
    {
        var args = KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyA);
        args.Handled = true;
        args.IsLeftShift = true;
        args.IsRightControl = true;
        args.IsLeftAlt = true;
        args.IsRightWindows = true;
        args.IsCapsLockActive = true;
        args.IsNumLockActive = true;
        args.IsScrollLockActive = true;
        args.TimeStamp = unchecked((uint)(Environment.TickCount - Hundred));
        args.Flags = ExtendedKeyFlags.Injected | ExtendedKeyFlags.LowerIntegretyInjected;

        await Assert.That(args.IsShift).IsTrue();
        await Assert.That(args.IsControl).IsTrue();
        await Assert.That(args.IsAlt).IsTrue();
        await Assert.That(args.IsWindows).IsTrue();
        await Assert.That(args.IsInjectedByProcess).IsTrue();
        await Assert.That(args.IsInjectedByLowerIntegrityLevelProcess).IsTrue();
        await Assert.That(args.GetEventTime(TimeProvider.System) <= TimeProvider.System.GetLocalNow()).IsTrue();

        var text = args.ToString();
        await Assert.That(text).Contains("left shift +");
        await Assert.That(text).Contains("right control +");
        await Assert.That(text).Contains("with left-alt");
        await Assert.That(text).Contains("with right-windows");
        await Assert.That(text).Contains("handled");
        await Assert.That(text).Contains("ScrollLocked");
        await Assert.That(text).Contains("NumLock active");
        await Assert.That(text).Contains("CapsLock active");

        await Assert.That(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.LeftShift).IsModifier).IsTrue();
    }

    /// <summary>Tests mouse hook event args are simple mutable event data.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MouseHookEventArgs_CoversMutablePropertiesAsync()
    {
        var args = new MouseHookEventArgs { Handled = true, Point = new(Ten, Twenty), WindowsMessage = WindowsMessages.WM_MOUSEMOVE, };

        await Assert.That(args.Handled).IsTrue();
        await Assert.That(args.Point).IsEqualTo(new(Ten, Twenty));
        await Assert.That(args.WindowsMessage).IsEqualTo(WindowsMessages.WM_MOUSEMOVE);
    }

    /// <summary>Tests keyboard generator empty calls use the safe zero-input path.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyboardInputGenerator_EmptyInputs_ReturnZeroWithoutNativeSendAsync()
    {
        await Assert.That(KeyboardInputGenerator.KeyDown()).IsEqualTo(0U);
        await Assert.That(KeyboardInputGenerator.KeyUp()).IsEqualTo(0U);
        await Assert.That(KeyboardInputGenerator.KeyPresses()).IsEqualTo(0U);
        await Assert.That(KeyboardInputGenerator.KeyCombinationPress()).IsEqualTo(0U);
    }

    /// <summary>Tests keyboard and mouse input struct factory methods without sending input.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InputStructFactories_CreateExpectedKeyboardAndMouseRecordsAsync()
    {
        var keyPress = KeyboardInput.ForKeyPress(VirtualKeyCode.KeyA, TestTimestamp);
        await Assert.That(keyPress.Length).IsEqualTo(Two);
        await Assert.That(keyPress[0].VirtualKeyCode).IsEqualTo(VirtualKeyCode.KeyA);
        await Assert.That(keyPress[0].KeyEventFlags).IsEqualTo(KeyEventFlags.None);
        await Assert.That(keyPress[0].Timestamp).IsEqualTo(TestTimestamp);
        await Assert.That(keyPress[1].KeyEventFlags).IsEqualTo(KeyEventFlags.KeyUp);

        var keyboardInputs = Input.CreateKeyboardInputs(keyPress);
        await Assert.That(keyboardInputs.Length).IsEqualTo(Two);
        await Assert.That(keyboardInputs[0].InputType).IsEqualTo(InputTypes.Keyboard);
        await Assert.That(keyboardInputs[1].InputUnion.KeyboardInput.VirtualKeyCode).IsEqualTo(VirtualKeyCode.KeyA);

        var mouseDown = MouseInput.MouseDown(MouseButtons.Left | MouseButtons.XButton2, null, TestTimestamp);
        var mouseUp = MouseInput.MouseUp(MouseButtons.Right | MouseButtons.XButton1, null, TestTimestamp);
        var wheel = MouseInput.MoveMouseWheel(-OneHundredTwenty, null, TestTimestamp);
        await Assert.That(mouseDown.MouseEventFlags).IsEqualTo(MouseEventFlags.LeftDown | MouseEventFlags.XDown);
        await Assert.That(mouseDown.MouseData).IsEqualTo(Two);
        await Assert.That(mouseUp.MouseEventFlags).IsEqualTo(MouseEventFlags.RightUp | MouseEventFlags.XUp);
        await Assert.That(mouseUp.MouseData).IsEqualTo(1);
        await Assert.That(wheel.MouseEventFlags).IsEqualTo(MouseEventFlags.Wheel);
        await Assert.That(wheel.MouseData).IsEqualTo(-OneHundredTwenty);

        var mouseInputs = Input.CreateMouseInputs(mouseDown, mouseUp, wheel);
        await Assert.That(mouseInputs.Length).IsEqualTo(Three);
        await Assert.That(mouseInputs[0].InputType).IsEqualTo(InputTypes.Mouse);
        await Assert.That(mouseInputs[2].InputUnion.MouseInput.MouseEventFlags).IsEqualTo(MouseEventFlags.Wheel);
        await Assert.That(Input.Size > 0).IsTrue();
    }

    /// <summary>Tests raw-input structs, snapshots, and event args without registering native hooks.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RawInputStructsAndEventArgs_CoverSafeDataPathsAsync()
    {
        var header = new RawInputHeader { Type = RawInputDeviceTypes.Keyboard, DeviceHandle = RawInputHandle };
        await Assert.That(header.Type).IsEqualTo(RawInputDeviceTypes.Keyboard);
        await Assert.That(header.ToIntPtr()).IsEqualTo(RawInputHandle);

        var rawInput = new RawInput { Header = header, Device = default };
        var rawArgs = new RawInputEventArgs { IsForeground = true, RawInput = rawInput };
        await Assert.That(rawArgs.IsForeground).IsTrue();
        await Assert.That(rawArgs.RawInput.Header.ToIntPtr()).IsEqualTo(RawInputHandle);

        var deviceInformation = new RawInputDeviceInformation { DeviceName = @"\\?\raw-keyboard", DisplayName = "Keyboard", Handle = RawInputHandle, };
        var changeArgs = new RawInputDeviceChangeEventArgs { Added = true, DeviceInformation = deviceInformation };
        await Assert.That(changeArgs.Added).IsTrue();
        await Assert.That(changeArgs.DeviceInformation.DeviceName).Contains("raw-keyboard");
        await Assert.That(changeArgs.DeviceInformation.DisplayName).IsEqualTo("Keyboard");
        await Assert.That(changeArgs.DeviceInformation.ToIntPtr()).IsEqualTo(RawInputHandle);

        await Assert.That(default(RawHID).GetData().Length).IsEqualTo(0);
        await Assert.That(default(RawKeyboard).ToString()).Contains("Rawkeyboard");
        await Assert.That(default(RawMouse).State).IsEqualTo(MouseStates.None);
        await Assert.That(default(RawDevice).Keyboard).IsEqualTo(default);
    }

    /// <summary>Tests RawInputDeviceInfo union accessors for supported and unsupported type branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RawInputDeviceInfo_Accessors_ValidateDeviceTypeAsync()
    {
        var keyboardInfo = new RawInputDeviceInfo(RawInputDeviceTypes.Keyboard);
        await Assert.That(keyboardInfo.Type).IsEqualTo(RawInputDeviceTypes.Keyboard);
        await Assert.That(keyboardInfo.Keyboard).IsEqualTo(default);
        await Assert.That(() => keyboardInfo.Mouse).Throws<NotSupportedException>();
        await Assert.That(() => keyboardInfo.HID).Throws<NotSupportedException>();

        var hidInfo = new RawInputDeviceInfo(RawInputDeviceTypes.HID);
        await Assert.That(hidInfo.HID).IsEqualTo(default);
        await Assert.That(() => hidInfo.Keyboard).Throws<NotSupportedException>();
    }

    /// <summary>Tests enum values used by input registration, hook setup, and event generation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InputEnums_HaveExpectedNativeValuesAsync()
    {
        await Assert.That(GetEnumValue(InputTypes.Mouse)).IsEqualTo(0);
        await Assert.That(GetEnumValue(InputTypes.Keyboard)).IsEqualTo(1);
        await Assert.That(GetEnumValue(InputTypes.Hardware)).IsEqualTo(Two);
        await Assert.That(GetEnumValue(HookTypes.WH_KEYBOARD_LL)).IsEqualTo(Thirteen);
        await Assert.That(GetEnumValue(HookTypes.WH_MOUSE_LL)).IsEqualTo(Fourteen);
        await Assert.That(GetEnumValue(RawInputDevices.Keyboard)).IsEqualTo(Four);
        await Assert.That(GetEnumValue(RawInputDevices.Mouse)).IsEqualTo(1);
        await Assert.That(GetEnumValue(RawInputDeviceTypes.Mouse)).IsEqualTo(0);
        await Assert.That(GetEnumValue(RawInputDeviceTypes.Keyboard)).IsEqualTo(1);
        await Assert.That(GetEnumValue(RawInputDeviceTypes.HID)).IsEqualTo(Two);
        await Assert.That(GetEnumValue(MouseEventFlags.LeftDown)).IsEqualTo(0x0002);
        await Assert.That(GetEnumValue(MouseEventFlags.Wheel)).IsEqualTo(0x0800);
        await Assert.That(GetEnumValue(MouseButtons.XButton2)).IsEqualTo(0x01000000);
        await Assert.That(GetEnumValue(KeyEventFlags.KeyUp)).IsEqualTo(0x0002);
        await Assert.That(GetEnumValue(ExtendedKeyFlags.Injected)).IsEqualTo(0x10);
        await Assert.That(GetEnumValue(MapVkType.VkToVscEx)).IsEqualTo(Four);
    }

    /// <summary>Tests raw-input monitor methods return shared observables before native subscription side effects.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RawInputMonitors_ReturnSharedObservableInstancesBeforeSubscriptionAsync()
    {
        var keyboardStream = RawInputMonitor.ObserveRawInput(RawInputDevices.Keyboard);
        var mouseStream = RawInputMonitor.ObserveRawInput(RawInputDevices.Mouse);
        var deviceChanges = RawInputDeviceMonitor.ObserveDeviceChanges(RawInputDevices.Keyboard);
        var secondDeviceChanges = RawInputDeviceMonitor.ObserveDeviceChanges(RawInputDevices.Mouse);

        await Assert.That(keyboardStream).IsNotNull();
        await Assert.That(ReferenceEquals(keyboardStream, mouseStream)).IsTrue();
        await Assert.That(deviceChanges).IsNotNull();
        await Assert.That(ReferenceEquals(deviceChanges, secondDeviceChanges)).IsTrue();
    }

    /// <summary>Materializes an enumerable without using LINQ.</summary>
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

    /// <summary>Reads an enum's native integer value through the runtime conversion path.</summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The enum value.</param>
    /// <returns>The native integer value.</returns>
    private static int GetEnumValue<TEnum>(TEnum value)
        where TEnum : struct, Enum => Convert.ToInt32(value, CultureInfo.InvariantCulture);
}
