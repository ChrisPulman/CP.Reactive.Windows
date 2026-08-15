// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>A "union" containing a specific input struct.</summary>
[StructLayout(LayoutKind.Explicit)]
public readonly record struct InputUnion
{
    /// <summary>Stores the mouse input value.</summary>
    [FieldOffset(0)]
    private readonly MouseInput _mouseInput;

    /// <summary>Stores the keyboard input value.</summary>
    [FieldOffset(0)]
    private readonly KeyboardInput _keyboardInput;

    /// <summary>Stores the hardware input value.</summary>
    [FieldOffset(0)]
    private readonly HardwareInput _hardwareInput;

    /// <summary>Gets or initializes the mouse input value.</summary>
    public MouseInput MouseInput
    {
        get => _mouseInput;
        init => _mouseInput = value;
    }

    /// <summary>Gets or initializes the keyboard input value.</summary>
    public KeyboardInput KeyboardInput
    {
        get => _keyboardInput;
        init => _keyboardInput = value;
    }

    /// <summary>Gets or initializes the hardware input value.</summary>
    public HardwareInput HardwareInput
    {
        get => _hardwareInput;
        init => _hardwareInput = value;
    }
}
