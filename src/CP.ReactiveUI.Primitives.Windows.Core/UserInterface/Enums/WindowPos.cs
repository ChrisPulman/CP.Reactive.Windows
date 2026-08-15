// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>Defines flags for SetWindowPos.</summary>
[Flags]
public enum WindowPos
{
    /// <summary>No window position flags.</summary>
    None = 0,

    /// <summary>
    /// If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the
    /// window. This prevents the calling thread from blocking its execution while other threads process the request.
    /// </summary>
    SWP_ASYNCWINDOWPOS = 0x4000,

    /// <summary>Prevents generation of the WM_SYNCPAINT message.</summary>
    SWP_DEFERERASE = 0x2000,

    /// <summary>Draws a frame (defined in the window's class description) around the window.</summary>
    SWP_DRAWFRAME = 0x20,

    /// <summary>Hides the window.</summary>
    SWP_HIDEWINDOW = 0x80,

    /// <summary>
    /// Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the
    /// setting of the insertAfterWindowHandle parameter).
    /// </summary>
    SWP_NOACTIVATE = 0x10,

    /// <summary>
    /// Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client
    /// area after the window is sized or repositioned.
    /// </summary>
    SWP_NOCOPYBITS = 0x100,

    /// <summary>Retains the current position (ignores X and Y parameters).</summary>
    SWP_NOMOVE = 2,

    /// <summary>Does not change the owner window's position in the Z order.</summary>
    SWP_NOOWNERZORDER = 0x200,

    /// <summary>
    /// Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and
    /// scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly
    /// invalidate or redraw any parts of the window and parent window that need redrawing.
    /// </summary>
    SWP_NOREDRAW = 8,

    /// <summary>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</summary>
    SWP_NOSENDCHANGING = 0x400,

    /// <summary>Retains the current size (ignores the cx and cy parameters).</summary>
    SWP_NOSIZE = 1,

    /// <summary>Retains the current Z order (ignores the insertAfterWindowHandle parameter).</summary>
    SWP_NOZORDER = 4,

    /// <summary>Displays the window.</summary>
    SWP_SHOWWINDOW = 0x40,
}
