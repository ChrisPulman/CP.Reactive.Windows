// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>
///     The ScrollInfoMask enum is used for retrieving the SCROLLINFO via the GetScrollInfo
///     See <a href="http://pinvoke.net/default.aspx/Enums/ScrollInfoMask.html">here</a>
/// </summary>
[Flags]
public enum ScrollInfoMask
{
    /// <summary>No scroll information mask.</summary>
    None = 0,

    /// <summary>Copies the scroll range to the Minimum and Maximum members of the SCROLLINFO structure pointed to by lpsi.</summary>
    Range = 1,

    /// <summary>Copies the scroll page to the PageSize member of the SCROLLINFO structure pointed to by lpsi.</summary>
    Page = 2,

    /// <summary>Copies the scroll position to the Position member of the SCROLLINFO structure pointed to by lpsi.</summary>
    Pos = 4,

    /// <summary>Disables the scroll bar instead of removing it.</summary>
    DisableNoScroll = 8,

    /// <summary>Copies the current scroll box tracking position to the TrackingPosition member of the SCROLLINFO structure pointed to by lpsi.</summary>
    Trackpos = 0x10,

    /// <summary>All of the above.</summary>
    All = Range | Page | Pos | Trackpos,
}
