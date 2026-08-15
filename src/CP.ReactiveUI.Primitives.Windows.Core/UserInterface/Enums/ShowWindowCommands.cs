// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>Used by User32.ShowWindow.</summary>
public enum ShowWindowCommands : uint
{
    /// <summary>Hides the window and activates another window.</summary>
    Hide,
    /// <summary>
    /// Activates and displays a window. If the window is minimized or maximized, the system restores it to its original size and position. An application should
    /// specify this flag when displaying the window for the first time.
    /// </summary>
    Normal,
    /// <summary>Activates the window and displays it as a minimized window.</summary>
    ShowMinimized,
    /// <summary>Maximizes the specified window.</summary>
    Maximize,
    /// <summary>Displays a window in its most recent size and position without activating it.</summary>
    ShowRecentNoActivation,
    /// <summary>Activates the window and displays it in its current size and position.</summary>
    Show,
    /// <summary>Minimizes the specified window and activates the next top-level window in the Z order.</summary>
    Minimize,
    /// <summary>Displays the window as minimized without activating it.</summary>
    ShowMinNoActivation,
    /// <summary>Displays the window in its current size and position without activating it.</summary>
    ShowNoActivation,
    /// <summary>
    /// Activates and displays the window. If the window is minimized or maximized, the system restores it to its original size and position. An application should
    /// specify this flag when restoring a minimized window.
    /// </summary>
    Restore,
    /// <summary>Sets the show state based on the SW_* value specified in the STARTUPINFO structure passed to the CreateProcess function by the program that started the application.</summary>
    ShowDefault,
    /// <summary>
    ///     <b>Windows 2000/XP:</b> Minimizes a window, even if the thread
    ///     that owns the window is not responding. This flag should only be
    ///     used when minimizing windows from a different thread.
    /// </summary>
    ForceMinimize
}
