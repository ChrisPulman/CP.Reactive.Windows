// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Keyboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;
#endif
/// <summary>Extensions for VirtualKeyCode.</summary>
public static class VirtualKeyCodeExtensions
{
    extension(VirtualKeyCode virtualKeyCode)
    {
        /// <summary>Test if the VirtualKeyCode is a modifier key.</summary>
        /// <returns>bool.</returns>
        public bool IsModifier() => ModifierKeys.Contains(virtualKeyCode);
    }

    /// <summary>The virtual keys that act as keyboard modifiers.</summary>
    private static readonly HashSet<VirtualKeyCode> ModifierKeys = new HashSet<VirtualKeyCode>
    {
        VirtualKeyCode.Capital,
        VirtualKeyCode.NumLock,
        VirtualKeyCode.Scroll,
        VirtualKeyCode.LeftShift,
        VirtualKeyCode.Shift,
        VirtualKeyCode.RightShift,
        VirtualKeyCode.Control,
        VirtualKeyCode.LeftControl,
        VirtualKeyCode.RightControl,
        VirtualKeyCode.Menu,
        VirtualKeyCode.LeftMenu,
        VirtualKeyCode.RightMenu,
        VirtualKeyCode.LeftWin,
        VirtualKeyCode.RightWin
    };
}
