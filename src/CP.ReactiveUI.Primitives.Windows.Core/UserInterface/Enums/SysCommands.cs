// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>
/// A window receives this message when the user chooses a command from the Window menu (formerly known as the system or control menu) or when the user chooses the
/// maximize button, minimize button, restore button, or close button.
/// </summary>
public enum SysCommands
{
    /// <summary>No system command.</summary>
    None = 0,
    /// <summary>Sizes the window.</summary>
    SC_SIZE = 61_440,
    /// <summary>Moves the window.</summary>
    SC_MOVE = 61_456,
    /// <summary>Minimizes the window.</summary>
    SC_MINIMIZE = 61_472,
    /// <summary>Maximizes the window.</summary>
    SC_MAXIMIZE = 61_488,
    /// <summary>Moves to the next window.</summary>
    SC_NEXTWINDOW = 61_504,
    /// <summary>Moves to the previous window.</summary>
    SC_PREVWINDOW = 61_520,
    /// <summary>Closes the window.</summary>
    SC_CLOSE = 61_536,
    /// <summary>Scrolls vertically.</summary>
    SC_VSCROLL = 61_552,
    /// <summary>Scrolls horizontally.</summary>
    SC_HSCROLL = 61_568,
    /// <summary>Retrieves the window menu as a result of a mouse click.</summary>
    SC_MOUSEMENU = 61_584,
    /// <summary>
    /// Retrieves the window menu as a result of a keystroke. If the wordParameter is SC_KEYMENU, longParameter contains the character code of the key that is used with the ALT key
    /// to display the popup menu. For example, pressing ALT+F to display the File popup will cause a WM_SYSCOMMAND with wordParameter equal to SC_KEYMENU and longParameter equal to
    /// 'f'.
    /// </summary>
    SC_KEYMENU = 61_696,
    /// <summary>Arranges minimized windows.</summary>
    SC_ARRANGE = 61_712,
    /// <summary>Restores the window to its normal position and size.</summary>
    SC_RESTORE = 61_728,
    /// <summary>Activates the Start menu.</summary>
    SC_TASKLIST = 61_744,
    /// <summary>Executes the screen saver application specified in the [boot] section of the System.ini file.</summary>
    SC_SCREENSAVE = 61_760,
    /// <summary>Activates the window associated with the application-specified hot key. The longParameter parameter identifies the window to activate.</summary>
    SC_HOTKEY = 61_776,
    /// <summary>Selects the default item; the user double-clicked the window menu.</summary>
    SC_DEFAULT = 61_792,
    /// <summary>
    /// Sets the state of the display. This command supports devices that have power-saving features, such as a battery-powered personal computer. The longParameter parameter
    /// can have the following values: -1 (the display is powering on) 1 (the display is going to low power) 2 (the display is being shut off).
    /// </summary>
    SC_MONITORPOWER = 61_808,
    /// <summary>Changes the cursor to a question mark with a pointer. If the user then clicks a control in the dialog box, the control receives a WM_HELP message.</summary>
    SC_CONTEXTHELP = 61_824,
    /// <summary>Separates system command menu items.</summary>
    SC_SEPARATOR = 61_455,
    /// <summary>Indicates whether the screen saver is secure.</summary>
    SCF_ISSECURE = 1,
    /// <summary>Same as SC_MINIMIZE.</summary>
    SC_ICON = SC_MINIMIZE,
    /// <summary>Same as SC_MAXIMIZE.</summary>
    SC_ZOOM = SC_MAXIMIZE
}
