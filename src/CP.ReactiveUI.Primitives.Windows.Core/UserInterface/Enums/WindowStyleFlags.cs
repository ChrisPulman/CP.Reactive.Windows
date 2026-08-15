// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>
///     The following are the window styles. After the window has been created, these styles cannot be modified, except as
///     noted.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms632600(v=vs.85).aspx">Window Styles</a>
/// </summary>
[Flags]
public enum WindowStyleFlags : uint
{
    /// <summary>The window is an overlapped window. An overlapped window has a title bar and a border. Same as the WS_TILED style.</summary>
    None = 0U,
    /// <summary>The windows is a pop-up window. This style cannot be used with the WS_CHILD style.</summary>
    WS_POPUP = 0x80000000U,
    /// <summary>
    /// The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style. Alias WS_CHILDWINDOW: Same as
    /// the WS_CHILD style.
    /// </summary>
    WS_CHILD = 0x40000000U,
    /// <summary>The window is initially minimized. Same as the WS_ICONIC style.</summary>
    WS_MINIMIZE = 0x20000000U,
    /// <summary>The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.</summary>
    WS_VISIBLE = 0x10000000U,
    /// <summary>The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.</summary>
    WS_DISABLED = 0x8000000U,
    /// <summary>
    /// Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other
    /// overlapping child windows out of the region of the child window to be updated. If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible,
    /// when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
    /// </summary>
    WS_CLIPSIBLINGS = 0x4000000U,
    /// <summary>Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.</summary>
    WS_CLIPCHILDREN = 0x2000000U,
    /// <summary>The window is initially maximized.</summary>
    WS_MAXIMIZE = 0x1000000U,
    /// <summary>The window has a thin-line border.</summary>
    WS_BORDER = 0x800000U,
    /// <summary>The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.</summary>
    WS_DLGFRAME = 0x400000U,
    /// <summary>The window has a vertical scroll bar.</summary>
    WS_VSCROLL = 0x200000U,
    /// <summary>The window has a horizontal scroll bar.</summary>
    WS_HSCROLL = 0x100000U,
    /// <summary>The window has a window menu on its title bar. The WS_CAPTION style must also be specified.</summary>
    WS_SYSMENU = 0x80000U,
    /// <summary>The window has a sizing border. Same as the WS_SIZEBOX style.</summary>
    WS_THICKFRAME = 0x40000U,
    /// <summary>
    /// The window has a minimize button Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified. Value is the same as
    /// WS_GROUP, due to a different context.
    /// </summary>
    WS_MINIMIZEBOX = 0x20000U,
    /// <summary>
    /// The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified. Value is the same as
    /// WS_TABSTOP, due to a different context.
    /// </summary>
    WS_MAXIMIZEBOX = 0x10000U,
    /// <summary>The WS_UNK8000 value.</summary>
    WS_UNK8000 = 0x8000U,
    /// <summary>The WS_UNK4000 value.</summary>
    WS_UNK4000 = 0x4000U,
    /// <summary>The WS_UNK2000 value.</summary>
    WS_UNK2000 = 0x2000U,
    /// <summary>The WS_UNK1000 value.</summary>
    WS_UNK1000 = 0x1000U,
    /// <summary>The WS_UNK800 value.</summary>
    WS_UNK800 = 0x800U,
    /// <summary>The WS_UNK400 value.</summary>
    WS_UNK400 = 0x400U,
    /// <summary>The WS_UNK200 value.</summary>
    WS_UNK200 = 0x200U,
    /// <summary>The WS_UNK100 value.</summary>
    WS_UNK100 = 0x100U,
    /// <summary>The WS_UNK80 value.</summary>
    WS_UNK80 = 0x80U,
    /// <summary>The WS_UNK40 value.</summary>
    WS_UNK40 = 0x40U,
    /// <summary>The WS_UNK20 value.</summary>
    WS_UNK20 = 0x20U,
    /// <summary>The WS_UNK10 value.</summary>
    WS_UNK10 = 0x10U,
    /// <summary>The WS_UNK8 value.</summary>
    WS_UNK8 = 8U,
    /// <summary>The WS_UNK4 value.</summary>
    WS_UNK4 = 4U,
    /// <summary>The WS_UNK2 value.</summary>
    WS_UNK2 = 2U,
    /// <summary>The WS_UNK1 value.</summary>
    WS_UNK1 = 1U,
    /// <summary>The window has a title bar (includes the WS_BORDER style).</summary>
    WS_CAPTION = WS_BORDER | WS_DLGFRAME,
    /// <summary>The window is an overlapped window. An overlapped window has a title bar and a border. Same as the WS_OVERLAPPED style.</summary>
    /// <summary>The window has a sizing border. Same as the WS_THICKFRAME style.</summary>
    /// <summary>The window is an overlapped window. Same as the WS_OVERLAPPEDWINDOW style.</summary>
    /// <summary>The window is an overlapped window. Same as the WS_TILEDWINDOW style.</summary>
    WS_OVERLAPPEDWINDOW = WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
    /// <summary>The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.</summary>
    WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU
}
