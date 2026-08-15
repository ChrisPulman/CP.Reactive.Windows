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
///     Contains information about the state of the keyboard.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645575.aspx">RAWKEYBOARD structure</a>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct RawKeyboard
{
    /// <summary>Stores the native scan code.</summary>
    private readonly ushort _scanCode;

    /// <summary>Stores the native scan code flags.</summary>
    private readonly ushort _flags;

    /// <summary>Stores the reserved native value.</summary>
    private readonly ushort _reserved;

    /// <summary>Stores the native virtual key code.</summary>
    private readonly ushort _virtualKeyCode;

    /// <summary>Stores the corresponding Windows message.</summary>
    private readonly WindowsMessages _message;

    /// <summary>Stores the device-specific additional information for the event.</summary>
    private readonly uint _extraInformation;

    /// <summary>Gets the virtual key code.</summary>
    public VirtualKeyCode VirtualKey => ToVirtualKeyCode(_virtualKeyCode);

    /// <summary>Gets scan code flags.</summary>
    public RawKeyboardFlags Flags => (RawKeyboardFlags)_flags;

    /// <summary>Gets the scan code.</summary>
    public ushort ScanCode => GetScanCode(_scanCode);

    /// <inheritdoc />
    public override string ToString() =>
        $"Rawkeyboard\n Makecode: {ScanCode}\n Makecode(hex) : {ScanCode:X}\n Flags: {Flags}\n"
        + $" Reserved: {_reserved}\n VKeyName: {VirtualKey}\n Message: {_message}\n ExtraInformation {_extraInformation}\n";

    /// <summary>Converts the native virtual-key value to its public enumeration.</summary>
    /// <param name="virtualKeyCode">The native virtual-key value.</param>
    /// <returns>The corresponding virtual-key enumeration value.</returns>
    private static VirtualKeyCode ToVirtualKeyCode(ushort virtualKeyCode) => (VirtualKeyCode)virtualKeyCode;

    /// <summary>Returns the native scan code without changing its width.</summary>
    /// <param name="scanCode">The native scan code.</param>
    /// <returns>The scan code.</returns>
    private static ushort GetScanCode(ushort scanCode) => scanCode;
}
