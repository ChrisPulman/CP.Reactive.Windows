// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>
///     The transition state of the mouse buttons. This member can be one or more of the following values.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645578.aspx">RAWMOUSE structure</a>
/// </summary>
[Flags]
public enum MouseButtonStates
{
    /// <summary>Left button changed to down.</summary>
    None = 0,
    /// <summary>Left button changed to down.</summary>
    LeftButtonDown = 1,
    /// <summary>Left button changed to Up.</summary>
    LeftButtonUp = 2,
    /// <summary>Right button changed to down.</summary>
    RightButtonDown = 4,
    /// <summary>Right button changed to Up.</summary>
    RightButtonUp = 8,
    /// <summary>Middle button changed to down.</summary>
    MiddleButtonDown = 0x10,
    /// <summary>Middle button changed to up.</summary>
    MiddleButtonUp = 0x20,
    /// <summary>XBUTTON1 changed to down.</summary>
    ButtonX1Down = 0x40,
    /// <summary>XBUTTON1 changed to up.</summary>
    Buttonx1Up = 0x80,
    /// <summary>XBUTTON2 changed to down.</summary>
    ButtonX2Down = 0x100,
    /// <summary>XBUTTON2 changed to up.</summary>
    Buttonx2Up = 0x200,
    /// <summary>Raw input comes from a mouse wheel. The wheel delta is stored in usButtonData.</summary>
    Wheel = 0x400,
}
