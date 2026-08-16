// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Shell.Enums;

/// <summary>Sends an appbar message to the system. See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/bb762108.aspx">SHAppBarMessage function</a>.</summary>
public enum AppBarMessages
{
    /// <summary>ABM_NEW - Registers a new appbar and specifies the message identifier that the system should use to send notification messages to the appbar.</summary>
    New,

    /// <summary>ABM_REMOVE - Unregisters an appbar, removing the bar from the system's internal list.</summary>
    Remove,

    /// <summary>ABM_QUERYPOS - Requests a size and screen position for an appbar.</summary>
    QueryPosition,

    /// <summary>ABM_SETPOS - Sets the size and screen position of an appbar.</summary>
    SetPosition,

    /// <summary>ABM_GETSTATE - Retrieves the autohide and always-on-top states of the Windows taskbar.</summary>
    GetState,

    /// <summary>
    /// ABM_GETTASKBARPOS - Retrieves the bounding rectangle of the Windows taskbar. Note that this applies only to the
    /// system taskbar. Other objects, particularly toolbars supplied with third-party software, also can be
    /// present. As a result, some of the screen area not covered by the Windows taskbar might not be visible
    /// to the user. To retrieve the area of the screen not covered by both the taskbar and other app bars—the
    /// working area available to your application—, use the GetMonitorInfo function.
    /// </summary>
    GetTaskbarPosition,

    /// <summary>
    /// ABM_ACTIVATE - Notifies the system to activate or deactivate an appbar. The longParameter member of the APPBARDATA pointed to by pData is set to TRUE to activate or FALSE to deactivate.
    /// </summary>
    Activate,

    /// <summary>ABM_GETAUTOHIDEBAR - Retrieves the handle to the autohide appbar associated with a particular edge of the screen.</summary>
    GetAutoHideAppBar,

    /// <summary>ABM_SETAUTOHIDEBAR - Registers or unregisters an autohide appbar for an edge of the screen.</summary>
    SetAutohideAppBar,

    /// <summary>ABM_WINDOWPOSCHANGED - Notifies the system when an appbar's position has changed.</summary>
    WindowPositionChanged,

    /// <summary>ABM_SETSTATE - Windows XP and later: Sets the state of the appbar's autohide and always-on-top attributes.</summary>
    SetState,

    /// <summary>ABM_GETAUTOHIDEBAREX - Windows XP and later: Retrieves the handle to the autohide appbar associated with a particular edge of a particular monitor.</summary>
    GetAutoHideAppBarExtended,

    /// <summary>ABM_SETAUTOHIDEBAREX - Windows XP and later: Registers or unregisters an autohide appbar for an edge of a particular monitor.</summary>
    SetAutoHideAppBarExtended,
}
