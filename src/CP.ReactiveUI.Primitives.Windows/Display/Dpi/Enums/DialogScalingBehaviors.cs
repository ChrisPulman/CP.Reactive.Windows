// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Enums;
#endif
/// <summary>Describes per-monitor DPI scaling behavior overrides for child windows within dialogs.</summary>
[Flags]
public enum DialogScalingBehaviors
{
    /// <summary>No dialog scaling behavior override is applied.</summary>
    None = 0,
    /// <summary>Prevents the dialog manager from sending an updated font to the child window via WM_SETFONT in response to a DPI change.</summary>
    DisableFontUpdate = 1,
    /// <summary>Prevents the dialog manager from resizing and repositioning the child window in response to a DPI change.</summary>
    DisableRelayout = 2
}
