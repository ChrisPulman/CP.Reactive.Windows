// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>
///     The mouse state. This member can be any reasonable combination of the following.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645578.aspx">RAWMOUSE structure</a>
/// </summary>
[Flags]
public enum MouseStates
{
    /// <summary>No mouse state flags are set.</summary>
    None = 0,
    /// <summary>Mouse movement data is based on absolute position.</summary>
    MoveAbsolute = 1,
    /// <summary>Mouse coordinates are mapped to the virtual desktop (for a multiple monitor system).</summary>
    VirtualDesktop = 2,
    /// <summary>The left button was released.</summary>
    AttributesChanged = 4
}
