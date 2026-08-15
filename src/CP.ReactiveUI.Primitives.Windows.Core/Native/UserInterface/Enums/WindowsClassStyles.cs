// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>Specifies window class style flags used when registering a window class in the Windows API.</summary>
/// <remarks>These flags control various behaviors and characteristics of windows created with the associated
/// class, such as redraw behavior, device context allocation, and special effects. Multiple values can be combined
/// using a bitwise OR operation. These styles correspond to the CS_* constants used in native Win32
/// programming.</remarks>
[Flags]
public enum WindowsClassStyles : uint
{
    /// <summary>No window class style flags.</summary>
    None = 0U,

    /// <summary>Redraws the entire window if a movement or size adjustment changes the height of the client area.</summary>
    VREDRAW = 1U,

    /// <summary>Redraws the entire window if a movement or size adjustment changes the width of the client area.</summary>
    HREDRAW = 2U,

    /// <summary>Sends a double-click message to the window procedure when the user double-clicks the mouse while the cursor is within a window belonging to the class.</summary>
    DBLCLKS = 8U,

    /// <summary>Allocates a unique device context for each window in the class.</summary>
    OWNDC = 0x20U,

    /// <summary>Allocates one device context to be shared by all windows in the class.</summary>
    CLASSDC = 0x40U,

    /// <summary>Sets the clipping rectangle of the child window to that of the parent window so that the child can draw on the parent.</summary>
    PARENTDC = 0x80U,

    /// <summary>Disables Close on the window menu.</summary>
    NOCLOSE = 0x200U,

    /// <summary>Saves, as a bitmap, the portion of the screen image obscured by a window of this class.</summary>
    SAVEBITS = 0x800U,

    /// <summary>Aligns the window's client area on a byte boundary (in the x direction).</summary>
    BYTEALIGNCLIENT = 0x1000U,

    /// <summary>Aligns the window on a byte boundary (in the x direction).</summary>
    BYTEALIGNWINDOW = 0x2000U,

    /// <summary>Indicates that the window class is an application global class.</summary>
    GLOBALCLASS = 0x4000U,

    /// <summary>Enables the drop shadow effect on a window.</summary>
    DROPSHADOW = 0x20000U,

    /// <summary>(Internal Use) IME window class.</summary>
    IME = 0x10000U,
}
