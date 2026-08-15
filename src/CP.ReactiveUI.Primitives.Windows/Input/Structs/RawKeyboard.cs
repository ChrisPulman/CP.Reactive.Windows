// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     Contains information about the state of the keyboard.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645575.aspx">RAWKEYBOARD structure</a>
/// </summary>
public readonly record struct RawKeyboard
{
    /// <summary>Gets the virtual key code.</summary>
    public VirtualKeyCode VirtualKey => _vkey;

    /// <summary>Gets scan code flags.</summary>
    public RawKeyboardFlags Flags { get; }

    /// <summary>Gets the scan code.</summary>
    public ushort ScanCode { get; }

    /// <summary>The reusable raw keyboard display format.</summary>
    private const string DisplayFormatText = "Rawkeyboard\n Makecode: {0}\n Makecode(hex) : {0:X}\n Flags: {1}\n Reserved: {2}\n VKeyName: {3}\n Message: {4}\n ExtraInformation {5}\n";

    /// <summary>Stores the reserved native value.</summary>
    private readonly ushort _reserved;

    /// <summary>Stores the native virtual key code.</summary>
    private readonly VirtualKeyCode _vkey;

    /// <summary>Stores the corresponding Windows message.</summary>
    private readonly WindowsMessages _message;

    /// <summary>Stores the device-specific additional information for the event.</summary>
    private readonly uint _extraInformation;

    /// <inheritdoc />
    public override string ToString() => string.Format(null, "Rawkeyboard\n Makecode: {0}\n Makecode(hex) : {0:X}\n Flags: {1}\n Reserved: {2}\n VKeyName: {3}\n Message: {4}\n ExtraInformation {5}\n", ScanCode, Flags, _reserved, _vkey, _message, _extraInformation);

    /// <inheritdoc/>
    [CompilerGenerated]
    public override int GetHashCode() => (((((((((EqualityComparer<ushort>.Default.GetHashCode(_reserved) * -1_521_134_295) + EqualityComparer<VirtualKeyCode>.Default.GetHashCode(_vkey)) * -1_521_134_295) + EqualityComparer<WindowsMessages>.Default.GetHashCode(_message)) * -1_521_134_295) + EqualityComparer<uint>.Default.GetHashCode(_extraInformation)) * -1_521_134_295) + EqualityComparer<RawKeyboardFlags>.Default.GetHashCode(Flags)) * -1_521_134_295) + EqualityComparer<ushort>.Default.GetHashCode(ScanCode);

    /// <inheritdoc/>
    [CompilerGenerated]
    public bool Equals(RawKeyboard other)
    {
        if (EqualityComparer<ushort>.Default.Equals(_reserved, other._reserved) && EqualityComparer<VirtualKeyCode>.Default.Equals(_vkey, other._vkey) && EqualityComparer<WindowsMessages>.Default.Equals(_message, other._message) && EqualityComparer<uint>.Default.Equals(_extraInformation, other._extraInformation) && EqualityComparer<RawKeyboardFlags>.Default.Equals(Flags, other.Flags))
        {
            return EqualityComparer<ushort>.Default.Equals(ScanCode, other.ScanCode);
        }

        return false;
    }
}
