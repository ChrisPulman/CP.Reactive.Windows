// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows.Enums;
#endif
/// <summary>These flags define which values are retrieved and if they are cached or not.</summary>
[Flags]
public enum InteropWindowRetrieveSettings : uint
{
    /// <summary>No values are retrieved.</summary>
    None = 0U,
    /// <summary>Forces an update of the specified flags.</summary>
    ForceUpdate = 1U,
    /// <summary>Retrieve the WindowInfo.</summary>
    Info = 2U,
    /// <summary>Retrieve the caption.</summary>
    Caption = 4U,
    /// <summary>Retrieve the class name.</summary>
    Classname = 8U,
    /// <summary>Retrieve the matching process id.</summary>
    ProcessId = 0x10U,
    /// <summary>Retrieve the parent.</summary>
    Parent = 0x20U,
    /// <summary>Retrieve the placement.</summary>
    Placement = 0x40U,
    /// <summary>Retrieve the is visible.</summary>
    Visible = 0x80U,
    /// <summary>Retrieve the zoom state (maximized).</summary>
    Maximized = 0x100U,
    /// <summary>Retrieve the icon state (minimized).</summary>
    Minimized = 0x200U,
    /// <summary>Retrieve the text.</summary>
    Text = 0x400U,
    /// <summary>Retrieve the scroll info.</summary>
    ScrollInfo = 0x800U,
    /// <summary>Retrieve the children.</summary>
    Children = 0x1000U,
    /// <summary>Retrieve the children by z-order.</summary>
    ZOrderedChildren = 0x2000U,
    /// <summary>Specify if values are auto corrected, e.g. the WindowInfo bounds are cropped to the parent.</summary>
    AutoCorrectValues = 0x4000U,
    /// <summary>Cache all, except children, don't force reloading.</summary>
    CacheAll = Info | Caption | Classname | ProcessId | Parent | Placement | Visible | Maximized | Minimized | Text | ScrollInfo,
    /// <summary>Cache all, except children, don't force reloading, auto correct certain values.</summary>
    CacheAllAutoCorrect = CacheAll | AutoCorrectValues,
    /// <summary>Cache all, with children, don't force reloading.</summary>
    CacheAllWithChildren = CacheAll | Children,
    /// <summary>Cache all, don't force reloading.</summary>
    CacheAllChildZorder = CacheAll | ZOrderedChildren,
}
