// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>This is used to similate a union in the RawInput struct, were cannot use Explicit due to 32/64 bit.</summary>
[StructLayout(LayoutKind.Explicit)]
public readonly record struct RawDevice
{
    /// <summary>Gets information on the mouse.</summary>
    public RawMouse Mouse => _mouse;

    /// <summary>Gets information on the keyboard.</summary>
    public RawKeyboard Keyboard => _keyboard;

    /// <summary>Gets information on the HID device.</summary>
    public RawHID HID => _hid;

    /// <summary>Stores raw mouse data.</summary>
    [FieldOffset(0)]
    private readonly RawMouse _mouse;

    /// <summary>Stores raw keyboard data.</summary>
    [FieldOffset(0)]
    private readonly RawKeyboard _keyboard;

    /// <summary>Stores raw HID data.</summary>
    [FieldOffset(0)]
    private readonly RawHID _hid;
}
