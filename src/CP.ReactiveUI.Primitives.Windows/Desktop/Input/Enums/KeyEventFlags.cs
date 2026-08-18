// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>
///     This enum specifies various aspects of a keystroke. This member can be certain combinations of the following
///     values.
///     See
///     <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646271(v=vs.85).aspx">KEYBDINPUT structure</a>
/// </summary>
[Flags]
public enum KeyEventFlags : uint
{
    /// <summary>If specified, the scan code was preceded by a prefix byte that has the value 0xE0 (224).</summary>
    None = 0U,
    /// <summary>If specified, the scan code was preceded by a prefix byte that has the value 0xE0 (224).</summary>
    ExtendedKey = 1U,
    /// <summary>If specified, the key is being released. If not specified, the key is being pressed.</summary>
    KeyUp = 2U,
    /// <summary>
    ///     If specified, the system synthesizes a VK_PACKET keystroke. The VirtualKeyCode parameter must be zero.
    ///     This flag can only be combined with the KeyUp flag. For more information, see the Remarks section.
    /// </summary>
    Unicode = 4U,
    /// <summary>If specified, wScan identifies the key and VirtualKeyCode is ignored.</summary>
    Scancode = 8U,
}
