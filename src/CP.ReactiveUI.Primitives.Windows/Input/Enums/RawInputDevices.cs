// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>This is used for mapping a device to Usage Page and Usage, to register for RawInput.</summary>
public enum RawInputDevices
{
    /// <summary>Pointer UsagePage: 0x01 Usage: 0x01 Hardware ID: HID_DEVICE_SYSTEM_MOUSE.</summary>
    Pointer,
    /// <summary>Mouse UsagePage: 0x01 Usage: 0x02 Hardware ID: HID_DEVICE_SYSTEM_MOUSE.</summary>
    Mouse,
    /// <summary>Pointer UsagePage: 0x01 Usage: 0x04 Hardware ID: HID_DEVICE_SYSTEM_GAME.</summary>
    Joystick,
    /// <summary>Pointer UsagePage: 0x01 Usage: 0x05 Hardware ID: HID_DEVICE_SYSTEM_GAME.</summary>
    GamePad,
    /// <summary>Pointer UsagePage: 0x01 Usage: 0x06 Hardware ID: HID_DEVICE_SYSTEM_KEYBOARD.</summary>
    Keyboard,
    /// <summary>Pointer UsagePage: 0x01 Usage: 0x07 Hardware ID: HID_DEVICE_SYSTEM_KEYBOARD.</summary>
    Keypad,
    /// <summary>Pointer UsagePage: 0x01 Usage: 0x80 Hardware ID: HID_DEVICE_SYSTEM_CONTROL.</summary>
    SystemControl,
    /// <summary>Pointer UsagePage: 0x0C Usage: 0x01 Hardware ID: HID_DEVICE_SYSTEM_CONSUMER.</summary>
    ConsumerAudioControl,
}
