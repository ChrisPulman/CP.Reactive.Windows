// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Deterministic release coverage for keyboard hooks and device-broadcast values.</summary>
public sealed class CoverageReleaseInputDeviceTests
{
    /// <summary>Defines a synthetic device description.</summary>
    private const string DeviceDescription = "device description";

    /// <summary>The key-down Windows message.</summary>
    private const int WmKeyDown = 0x0100;

    /// <summary>The key-up Windows message.</summary>
    private const int WmKeyUp = 0x0101;

    /// <summary>The allocation offset for the virtual-key code.</summary>
    private const int VirtualKeyCodeOffset = 0;

    /// <summary>The allocation offset for the scan code.</summary>
    private const int ScanCodeOffset = 4;

    /// <summary>The allocation offset for the flags.</summary>
    private const int FlagsOffset = 8;

    /// <summary>The allocation offset for the timestamp.</summary>
    private const int TimestampOffset = 12;

    /// <summary>The deterministic hook callback code.</summary>
    private const int HookCallbackCode = 0;

    /// <summary>The deterministic mouse event timestamp.</summary>
    private const uint MouseEventTimestamp = 42U;

    /// <summary>The virtual desktop coordinate representing the lower edge.</summary>
    private const int LowerVirtualDesktopCoordinate = 0;

    /// <summary>The virtual desktop coordinate representing the upper edge.</summary>
    private const int UpperVirtualDesktopCoordinate = 65_535;

    /// <summary>The virtual desktop coordinate representing the midpoint.</summary>
    private const int MidpointVirtualDesktopCoordinate = 32_767;

    /// <summary>The non-zero extent used to model an incomplete virtual desktop.</summary>
    private const int IncompleteDesktopExtent = Ten;

    /// <summary>The expected asynchronous key-state result.</summary>
    private const short ExpectedAsyncKeyState = Two;

    /// <summary>The expected synchronous key-state result.</summary>
    private const short ExpectedKeyState = Three;

    /// <summary>The unsigned zero passed to native callback operations.</summary>
    private const uint UnsignedZero = 0U;

    /// <summary>The deterministic hook handle used by adapter coverage.</summary>
    private static readonly IntPtr HookHandle = new(OneThousandTwoHundredThirtyFour);

    /// <summary>The deterministic recipient handle used by device-notification coverage.</summary>
    private static readonly IntPtr NotificationRecipient = new(2468);

    /// <summary>The deterministic registration handle returned by device-notification coverage.</summary>
    private static readonly IntPtr NotificationRegistration = new(1357);

    /// <summary>The deterministic lower-bound point used by mouse-input coverage.</summary>
    private static readonly Point LowerBoundPoint = new(Nine, 19);

    /// <summary>The deterministic upper-bound point used by mouse-input coverage.</summary>
    private static readonly Point UpperBoundPoint = new(110, 220);

    /// <summary>The deterministic midpoint used by mouse-input coverage.</summary>
    private static readonly Point Midpoint = new(Sixty, OneHundredTwenty);

    /// <summary>The deterministic screen bounds used by mouse-input coverage.</summary>
    private static readonly NativeRect ScreenBounds = new(Ten, Twenty, Hundred, TwoHundred);

    /// <summary>Exercises every keyboard-state branch through the injected hook API.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyboardHook_ResolvesAllStateAdjustmentsWithoutInstallingAHookAsync()
    {
        var nativeApi = new InputCoverage2Tests.FakeNativeHookApi();
        INativeHookApi previousNativeApi = NativeHookMethods.SetApiForTesting(nativeApi);

        try
        {
            var events = new List<KeyboardHookEventArgs>();
            using var subscription = KeyboardHook.KeyboardHookEvents.SubscribeOnNext(events.Add);
            VirtualKeyCode[] keys =
            [
                VirtualKeyCode.LeftShift,
                VirtualKeyCode.RightShift,
                VirtualKeyCode.LeftControl,
                VirtualKeyCode.RightControl,
                VirtualKeyCode.LeftMenu,
                VirtualKeyCode.RightMenu,
                VirtualKeyCode.LeftWin,
                VirtualKeyCode.RightWin,
                VirtualKeyCode.Capital,
                VirtualKeyCode.NumLock,
                VirtualKeyCode.Scroll,
                VirtualKeyCode.KeyA,
                (VirtualKeyCode)int.MaxValue,
            ];

            foreach (VirtualKeyCode key in keys)
            {
                InvokeKeyboardHookCallback(nativeApi, key, WmKeyDown);
            }

            InvokeKeyboardHookCallback(nativeApi, VirtualKeyCode.Capital, WmKeyUp);
            InvokeKeyboardHookCallback(nativeApi, VirtualKeyCode.NumLock, WmKeyUp);
            InvokeKeyboardHookCallback(nativeApi, VirtualKeyCode.Scroll, WmKeyUp);

            await Assert.That(events.Count).IsEqualTo(keys.Length + Three);
            await Assert.That(events[HookCallbackCode].IsLeftShift).IsTrue();
            await Assert.That(events[One].IsRightShift).IsTrue();
            await Assert.That(events[Two].IsLeftControl).IsTrue();
            await Assert.That(events[Three].IsRightControl).IsTrue();
            await Assert.That(events[Four].IsLeftAlt).IsTrue();
            await Assert.That(events[Five].IsRightAlt).IsTrue();
            await Assert.That(events[Six].IsLeftWindows).IsTrue();
            await Assert.That(events[Seven].IsRightWindows).IsTrue();
            await Assert.That(events[Eight].IsCapsLockActive).IsTrue();
            await Assert.That(events[Nine].IsNumLockActive).IsTrue();
            await Assert.That(events[Ten].IsScrollLockActive).IsTrue();
            await Assert.That(events[Eleven].Key).IsEqualTo(VirtualKeyCode.KeyA);
            await Assert.That(nativeApi.CallNextHookCount).IsEqualTo(keys.Length + Three);
        }
        finally
        {
            _ = NativeHookMethods.SetApiForTesting(previousNativeApi);
        }
    }

    /// <summary>Exercises the low-level keyboard value constructor and generated value members.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyboardLowLevelHookStruct_StoresAndComparesNativeValuesAsync()
    {
        KeyboardLowLevelHookStruct value = new((uint)VirtualKeyCode.KeyB, TwentyNine, ExtendedKeyFlags.Injected, FortyTwo, (UIntPtr)OneThousandTwoHundredThirtyFour);
        KeyboardLowLevelHookStruct equivalent = new((uint)VirtualKeyCode.KeyB, TwentyNine, ExtendedKeyFlags.Injected, FortyTwo, (UIntPtr)OneThousandTwoHundredThirtyFour);
        KeyboardLowLevelHookStruct different = new((uint)VirtualKeyCode.KeyC, Thirty, ExtendedKeyFlags.Up, FortyThree, (UIntPtr)FourThousandThreeHundredTwentyOne);

        await Assert.That(value.VirtualKeyCode).IsEqualTo(VirtualKeyCode.KeyB);
        await Assert.That(value.ScanCode).IsEqualTo((uint)TwentyNine);
        await Assert.That(value.Flags).IsEqualTo(ExtendedKeyFlags.Injected);
        await Assert.That(value.TimeStamp).IsEqualTo((uint)FortyTwo);
        await Assert.That(value.ExtraInfo).IsEqualTo((UIntPtr)OneThousandTwoHundredThirtyFour);
        await Assert.That(value.Equals(equivalent)).IsTrue();
        await Assert.That(value.Equals(different)).IsFalse();
        await Assert.That(value.GetHashCode()).IsEqualTo(equivalent.GetHashCode());
    }

    /// <summary>Exercises device parsing, registry values, and all friendly-name fallbacks through an injected reader.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DevBroadcastDeviceInterface_UsesInjectedRegistryValuesAndValuePathsAsync()
    {
        const string deviceName = @"\?\USB#VID_1234&PID_ABCD#INSTANCE#{class}";
        const string classGuid = "a5dcbf10-6530-11d2-901f-00c04fb951ed";
        var device = DevBroadcastDeviceInterface.Test(deviceName, DeviceInterfaceClass.UsbDevice);
        var emptyNameDevice = DevBroadcastDeviceInterface.Test(string.Empty);
        var shortNameDevice = DevBroadcastDeviceInterface.Test("abc");
        device.DeviceClassGuid = Guid.Parse(classGuid);

        using (DevBroadcastDeviceInterface.OverrideRegistryValueReaderForTesting(static (_, valueName) => valueName switch
        {
            "ClassGUID" => classGuid,
            "FriendlyName" => "USB test device",
            "DeviceDesc" => $"provider;{DeviceDescription}",
            _ => null,
        }))
        {
            await Assert.That(device.DeviceSetupClassGuid).IsEqualTo(Guid.Parse(classGuid));
            await Assert.That(device.FriendlyDeviceName).IsEqualTo("USB test device");
        }

        using (DevBroadcastDeviceInterface.OverrideRegistryValueReaderForTesting(static (_, valueName) => valueName == "DeviceDesc" ? $"provider;{DeviceDescription}" : null))
        {
            await Assert.That(device.FriendlyDeviceName).IsEqualTo(DeviceDescription);
        }

        using (DevBroadcastDeviceInterface.OverrideRegistryValueReaderForTesting(static (_, valueName) => valueName == "DeviceDesc" ? DeviceDescription : null))
        {
            await Assert.That(device.FriendlyDeviceName).IsEqualTo(DeviceDescription);
        }

        await Assert.That(device.Name).IsEqualTo(deviceName);
        await Assert.That(device.DisplayName).IsEqualTo(@"USB\VID_1234&PID_ABCD\INSTANCE");
        await Assert.That(emptyNameDevice.DisplayName).IsEqualTo(string.Empty);
        await Assert.That(shortNameDevice.DisplayName).IsEqualTo("abc");
        await Assert.That(device.DeviceClass).IsEqualTo(DeviceInterfaceClass.UsbDevice);
        await Assert.That(device.IsUsb).IsTrue();
        await Assert.That(device.IsPci).IsFalse();
    }

    /// <summary>Exercises the explicit device-header constructor and equality branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DevBroadcastHeader_StoresAndComparesAllNativeFieldsAsync()
    {
        DevBroadcastHeader value = new(Twelve, DeviceBroadcastDeviceType.DeviceInterface, Seven);
        DevBroadcastHeader equivalent = new(Twelve, DeviceBroadcastDeviceType.DeviceInterface, Seven);
        DevBroadcastHeader different = new(Sixteen, DeviceBroadcastDeviceType.Volume, Eight);

        await Assert.That(value.DeviceType).IsEqualTo(DeviceBroadcastDeviceType.DeviceInterface);
        await Assert.That(value == equivalent).IsTrue();
        await Assert.That(value != equivalent).IsFalse();
        await Assert.That(value != different).IsTrue();
        await Assert.That(value.Equals((object)equivalent)).IsTrue();
        await Assert.That(value.Equals((object)different)).IsFalse();
        await Assert.That(value.GetHashCode()).IsEqualTo(equivalent.GetHashCode());
    }

    /// <summary>Exercises the production hook adapter through injected operations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsNativeHookApi_UsesInjectedOperationsWithoutCallingWindowsAsync()
    {
        IntPtr hookHandle = HookHandle;
        using var operations = WindowsNativeHookApi.OverrideOperationsForTesting(
            static (_, _, _, _) => new(One),
            static _ => ExpectedAsyncKeyState,
            static _ => ExpectedKeyState,
            static (_, _, _, _) => new(Four),
            static _ => true);

        await Assert.That(WindowsNativeHookApi.Instance.CallNextHookEx(hookHandle, HookCallbackCode, IntPtr.Zero, IntPtr.Zero)).IsEqualTo(new(One));
        await Assert.That(WindowsNativeHookApi.Instance.GetAsyncKeyState(VirtualKeyCode.KeyA)).IsEqualTo(ExpectedAsyncKeyState);
        await Assert.That(WindowsNativeHookApi.Instance.GetKeyState(VirtualKeyCode.KeyA)).IsEqualTo(ExpectedKeyState);
        await Assert.That(WindowsNativeHookApi.Instance.SetWindowsHookEx(HookTypes.WH_KEYBOARD_LL, static (_, _, _) => IntPtr.Zero, IntPtr.Zero, UnsignedZero)).IsEqualTo(new(Four));
        await Assert.That(WindowsNativeHookApi.Instance.UnhookWindowsHookEx(hookHandle)).IsTrue();
    }

    /// <summary>Exercises the remaining input value factories without sending live input.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InputValues_UseDeterministicBoundsAndNativeValueConstructorsAsync()
    {
        using var bounds = MouseInput.OverrideScreenBoundsForTesting(static () => ScreenBounds);
        var lowerBound = MouseInput.MouseMove(LowerBoundPoint, MouseEventTimestamp);
        var upperBound = MouseInput.MouseMove(UpperBoundPoint, MouseEventTimestamp);
        var midpoint = MouseInput.MouseMove(Midpoint, MouseEventTimestamp);
        RawInputDeviceList rawDevice = new(new(FourThousandThreeHundredTwentyOne), RawInputDeviceTypes.HID);

        await Assert.That(lowerBound.Dx).IsEqualTo(LowerVirtualDesktopCoordinate);
        await Assert.That(lowerBound.Dy).IsEqualTo(LowerVirtualDesktopCoordinate);
        await Assert.That(upperBound.Dx).IsEqualTo(UpperVirtualDesktopCoordinate);
        await Assert.That(upperBound.Dy).IsEqualTo(UpperVirtualDesktopCoordinate);
        await Assert.That(midpoint.Dx).IsEqualTo(MidpointVirtualDesktopCoordinate);
        await Assert.That(midpoint.Dy).IsEqualTo(MidpointVirtualDesktopCoordinate);
        await Assert.That(rawDevice.RawInputDeviceType).IsEqualTo(RawInputDeviceTypes.HID);
        await Assert.That(rawDevice.Handle).IsEqualTo(new(FourThousandThreeHundredTwentyOne));
        await Assert.That(rawDevice.ToIntPtr()).IsEqualTo(new(FourThousandThreeHundredTwentyOne));
    }

    /// <summary>Exercises incomplete virtual desktop bounds without sending any input.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MouseInput_LeavesCoordinatesUnchangedForIncompleteDesktopBoundsAsync()
    {
        var location = new NativePoint(One, Two);

        using (MouseInput.OverrideScreenBoundsForTesting(static () => new NativeRect(0, 0, 0, IncompleteDesktopExtent)))
        {
            var input = MouseInput.MouseMove(location, MouseEventTimestamp);
            await Assert.That(input.Dx).IsEqualTo(location.X);
            await Assert.That(input.Dy).IsEqualTo(location.Y);
        }

        using (MouseInput.OverrideScreenBoundsForTesting(static () => new NativeRect(0, 0, IncompleteDesktopExtent, 0)))
        {
            var input = MouseInput.MouseMove(location, MouseEventTimestamp);
            await Assert.That(input.Dx).IsEqualTo(location.X);
            await Assert.That(input.Dy).IsEqualTo(location.Y);
        }
    }

    /// <summary>Exercises event-argument short-circuit and formatting branches using value data only.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyboardHookEventArgs_CoversEventTimeAndFalseValueBranchesAsync()
    {
        var noModifiers = KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyA);
        noModifiers.TimeStamp = unchecked((uint)Environment.TickCount);
        var rightShift = KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyB);
        rightShift.IsRightShift = true;
        rightShift.Flags = ExtendedKeyFlags.LowerIntegretyInjected;
        var modifier = KeyboardHookEventArgs.KeyDown(VirtualKeyCode.LeftShift);

        await Assert.That(noModifiers.EventTime <= TimeProvider.System.GetLocalNow()).IsTrue();
        await Assert.That(noModifiers.IsShift).IsFalse();
        await Assert.That(noModifiers.IsInjectedByLowerIntegrityLevelProcess).IsFalse();
        await Assert.That(noModifiers.ToString()).Contains("KeyA up (not handled)");
        await Assert.That(rightShift.IsShift).IsTrue();
        await Assert.That(rightShift.ToString()).Contains("right shift +");
        await Assert.That(modifier.ToString().Contains("shift +", StringComparison.Ordinal)).IsFalse();
    }

    /// <summary>Exercises the native device-registration wrapper through an injected operation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DeviceNotification_UsesInjectedRegistrationOperationAsync()
    {
        IntPtr recipient = NotificationRecipient;
        IntPtr registration = NotificationRegistration;
        var expectedFilter = DevBroadcastDeviceInterface.Create();
        var receivedRecipient = IntPtr.Zero;
        var receivedFilterPointer = IntPtr.Zero;
        var receivedFlags = DeviceNotifyFlags.None;
        var previousRegistration = DeviceNotification.SetRegisterDeviceNotificationForTesting((recipientHandle, filterPointer, flags) =>
        {
            receivedRecipient = recipientHandle;
            receivedFilterPointer = filterPointer;
            receivedFlags = flags;
            return registration;
        });

        try
        {
            await Assert.That(DeviceNotification.RegisterDeviceNotification(recipient, expectedFilter, DeviceNotifyFlags.AllInterfaceClasses)).IsEqualTo(registration);
            await Assert.That(receivedRecipient).IsEqualTo(recipient);
            await Assert.That(receivedFilterPointer).IsNotEqualTo(IntPtr.Zero);
            await Assert.That(receivedFlags).IsEqualTo(DeviceNotifyFlags.AllInterfaceClasses);
        }
        finally
        {
            _ = DeviceNotification.SetRegisterDeviceNotificationForTesting(previousRegistration);
        }
    }

    /// <summary>Allocates a native low-level keyboard payload without calling Windows.</summary>
    /// <param name="key">The virtual key code.</param>
    /// <param name="flags">The hook flags.</param>
    /// <returns>The allocated payload.</returns>
    private static IntPtr AllocateKeyboardHookData(VirtualKeyCode key, ExtendedKeyFlags flags)
    {
        IntPtr data = Marshal.AllocHGlobal(Marshal.SizeOf<KeyboardLowLevelHookStruct>());
        Marshal.WriteInt32(data, VirtualKeyCodeOffset, (int)key);
        Marshal.WriteInt32(data, ScanCodeOffset, HookCallbackCode);
        Marshal.WriteInt32(data, FlagsOffset, unchecked((int)flags));
        Marshal.WriteInt32(data, TimestampOffset, HookCallbackCode);
        return data;
    }

    /// <summary>Invokes a hook callback with a temporary native keyboard payload.</summary>
    /// <param name="nativeApi">The deterministic native hook API.</param>
    /// <param name="key">The virtual key code to include in the payload.</param>
    /// <param name="windowsMessage">The Windows keyboard message.</param>
    private static void InvokeKeyboardHookCallback(InputCoverage2Tests.FakeNativeHookApi nativeApi, VirtualKeyCode key, int windowsMessage)
    {
        IntPtr data = AllocateKeyboardHookData(key, ExtendedKeyFlags.None);
        try
        {
            _ = nativeApi.Invoke(HookCallbackCode, (IntPtr)windowsMessage, data);
        }
        finally
        {
            Marshal.FreeHGlobal(data);
        }
    }
}
