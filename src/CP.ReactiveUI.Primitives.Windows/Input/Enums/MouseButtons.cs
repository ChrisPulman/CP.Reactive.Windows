// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>Defines which mouse buttons to use.</summary>
[Flags]
public enum MouseButtons
{
    /// <summary>No button.</summary>
    None = 0,
    /// <summary>Left mouse button.</summary>
    Left = 0x100000,
    /// <summary>Right mouse button.</summary>
    Right = 0x200000,
    /// <summary>Middle mouse button.</summary>
    Middle = 0x400000,
    /// <summary>Extra button 1.</summary>
    XButton1 = 0x800000,
    /// <summary>Extra button 2.</summary>
    XButton2 = 0x1000000
}
