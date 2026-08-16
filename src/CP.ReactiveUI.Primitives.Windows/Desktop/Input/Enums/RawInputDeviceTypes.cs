// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>
/// The type of device, using in the RAWINPUTDEVICELIST
/// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645568.aspx">RAWINPUTDEVICELIST structure</a>
/// </summary>
public enum RawInputDeviceTypes : uint
{
    /// <summary>RIM_TYPEMOUSE: Specified device is a mouse.</summary>
    Mouse,
    /// <summary>RIM_TYPEKEYBOARD: Specified device is a keyboard.</summary>
    Keyboard,
    /// <summary>RIM_TYPEHID: Specified device is not a mouse or a keyboard.</summary>
    HID,
}
