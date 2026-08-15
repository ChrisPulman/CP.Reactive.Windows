// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     Contains information about the state of the mouse.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645578.aspx">RAWMOUSE structure</a>
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly record struct RawMouse
{
    /// <summary>Gets the mouse state.</summary>
    public MouseStates State => _flags;

    /// <summary>Gets the button state.</summary>
    public MouseButtonStates ButtonState => _buttonFlags;

    /// <summary>Gets if usButtonFlags is RI_MOUSE_WHEEL, this member is a signed value that specifies the wheel delta.</summary>
    public short WheelData => _buttonData;

    /// <summary>Gets the motion in the X direction. This is signed relative motion or absolute motion, depending on the value of usFlags.</summary>
    public int X => _lastX;

    /// <summary>Gets the motion in the Y direction. This is signed relative motion or absolute motion, depending on the value of usFlags.</summary>
    public int Y => _lastY;

    /// <summary>Stores the native mouse state flags.</summary>
    [FieldOffset(0)]
    private readonly MouseStates _flags;

    /// <summary>Stores the native reserved buttons value.</summary>
    [FieldOffset(4)]
    private readonly uint _buttons;

    /// <summary>Stores the native mouse button state flags.</summary>
    [FieldOffset(4)]
    private readonly MouseButtonStates _buttonFlags;

    /// <summary>Stores the native mouse button data.</summary>
    [FieldOffset(6)]
    private readonly short _buttonData;

    /// <summary>Stores the native raw button state.</summary>
    [FieldOffset(8)]
    private readonly uint _rawButtons;

    /// <summary>Stores the native x movement value.</summary>
    [FieldOffset(12)]
    private readonly int _lastX;

    /// <summary>Stores the native y movement value.</summary>
    [FieldOffset(16)]
    private readonly int _lastY;

    /// <summary>Stores the native extra information value.</summary>
    [FieldOffset(20)]
    private readonly uint _extraInformation;
}
