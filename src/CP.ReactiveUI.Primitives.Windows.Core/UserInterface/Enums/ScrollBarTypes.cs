// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>Identifies the scroll bar affected by scroll bar APIs.</summary>
public enum ScrollBarTypes
{
    /// <summary>The horizontal scroll bar of the specified window.</summary>
    Horizontal,

    /// <summary>The vertical scroll bar of the specified window.</summary>
    Vertical,

    /// <summary>A scroll bar control.</summary>
    Control,

    /// <summary>The horizontal and vertical scroll bars of the specified window.</summary>
    Both,
}
