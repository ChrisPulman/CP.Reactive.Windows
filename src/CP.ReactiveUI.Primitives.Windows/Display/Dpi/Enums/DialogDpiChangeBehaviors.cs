// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Enums;
#endif
/// <summary>Describes dialog-level per-monitor DPI scaling behavior overrides.</summary>
[Flags]
public enum DialogDpiChangeBehaviors
{
    /// <summary>No dialog DPI change behavior override is applied.</summary>
    None = 0,
    /// <summary>Prevents the dialog manager from responding to WM_GETDPISCALEDSIZE and WM_DPICHANGED, disabling all default DPI scaling behavior.</summary>
    DisableAll = 1,
    /// <summary>Prevents the dialog manager from resizing the dialog in response to a DPI change.</summary>
    DisableResize = 2,
    /// <summary>
    /// Prevents the dialog manager from re-layouting all of the dialogue's immediate child windows in response to a DPI
    /// change.
    /// </summary>
    DisableControlRelayout = DisableAll | DisableResize
}
