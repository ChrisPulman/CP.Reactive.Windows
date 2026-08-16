// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>The different known generic HID usages See <a href="http://www.usb.org/developers/hidpage/Hut1_12v2.pdf">here</a></summary>
public enum HidUsagesGeneric
{
    /// <summary>No HID usage.</summary>
    None = 0,
    /// <summary>Pointer usage.</summary>
    Pointer = 1,
    /// <summary>Mouse usage.</summary>
    Mouse = 2,
    /// <summary>Joystick usage.</summary>
    Joystick = 4,
    /// <summary>Game Pad.</summary>
    Gamepad = 5,
    /// <summary>Keyboard usage.</summary>
    Keyboard = 6,
    /// <summary>Keypad usage.</summary>
    Keypad = 7,
    /// <summary>Multi-axis usage.</summary>
    MultiAxis = 8,
    /// <summary>Tablet PC.</summary>
    Tablet = 9,
    /// <summary>Consumer usage.</summary>
    Consumer = 12,
    /// <summary>X axis usage.</summary>
    X = 48,
    /// <summary>Y axis usage.</summary>
    Y = 49,
    /// <summary>Z axis usage.</summary>
    Z = 50,
    /// <summary>X rotation usage.</summary>
    Rx = 51,
    /// <summary>Y rotation usage.</summary>
    Ry = 52,
    /// <summary>Z rotation usage.</summary>
    Rz = 53,
    /// <summary>Slider usage.</summary>
    Slider = 54,
    /// <summary>Dial usage.</summary>
    Dial = 55,
    /// <summary>Wheel usage.</summary>
    Wheel = 56,
    /// <summary>Hat switch.</summary>
    HatSwitch = 57,
    /// <summary>Counted buffer.</summary>
    CountedBuffer = 58,
    /// <summary>Byte count.</summary>
    ByteCount = 59,
    /// <summary>Motion Wakeup.</summary>
    MotionWakeup = 60,
    /// <summary>Start usage.</summary>
    Start = 61,
    /// <summary>Select usage.</summary>
    Select = 62,
    /// <summary>Muilt-axis Controller.</summary>
    SystemControl = 128,
}
