// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Keyboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;
#endif
/// <summary>Composes keyboard display-name native calls for production and deterministic tests.</summary>
internal interface INativeKeyboardDisplayApi
{
    /// <summary>Gets the active keyboard layout for the supplied thread.</summary>
    /// <param name="threadId">The thread id, or 0 for the current thread.</param>
    /// <returns>The keyboard layout handle.</returns>
    IntPtr GetKeyboardLayout(uint threadId);

    /// <summary>Maps a virtual key to a scan code.</summary>
    /// <param name="code">The virtual key code.</param>
    /// <param name="mapType">The map type.</param>
    /// <param name="keyboardLayout">The keyboard layout handle.</param>
    /// <returns>The mapped scan code.</returns>
    uint MapVirtualKeyEx(uint code, uint mapType, IntPtr keyboardLayout);

    /// <summary>Gets the display name for a scan code.</summary>
    /// <param name="longParameter">The scan code and modifier parameter.</param>
    /// <param name="text">The destination text buffer.</param>
    /// <returns>The number of copied characters.</returns>
    int GetKeyNameText(uint longParameter, Span<char> text);
}
