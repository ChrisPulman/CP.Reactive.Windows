// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>Identifies the state entries inside a native scroll bar information structure.</summary>
public enum ScrollBarStateIndexes : uint
{
    /// <summary>The scroll bar itself.</summary>
    Scrollbar,
    /// <summary>The top or right arrow button.</summary>
    TopOrRightArrow,
    /// <summary>The page up or page right region.</summary>
    PageUpOrRightRegion,
    /// <summary>The scroll box (thumb).</summary>
    ScrollBox,
    /// <summary>The page down or page left region.</summary>
    PageDownOrLeftRegion,
    /// <summary>The bottom or left arrow button.</summary>
    ButtonOrLeftArrow
}
