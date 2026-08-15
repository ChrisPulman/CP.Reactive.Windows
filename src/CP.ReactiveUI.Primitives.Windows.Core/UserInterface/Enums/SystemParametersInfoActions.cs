// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>SPI_ System-wide parameter - Used in SystemParametersInfo function.</summary>
public enum SystemParametersInfoActions : uint
{
    /// <summary>No value.</summary>
    SPI_NONE = 0U,
    /// <summary>Determines whether the warning beeper is on. the parameter must point to a BOOL variable that receives TRUE if the beeper is on, or FALSE if it is off.</summary>
    SPI_GETBEEP = 1U,
    /// <summary>Turns the warning beeper on or off. the parameter specifies TRUE for on, or FALSE for off.</summary>
    SPI_SETBEEP = 2U,
    /// <summary>Retrieves the two mouse threshold values and the mouse speed.</summary>
    SPI_GETMOUSE = 3U,
    /// <summary>Sets the two mouse threshold values and the mouse speed.</summary>
    SPI_SETMOUSE = 4U,
    /// <summary>
    /// Retrieves the border multiplier factor that determines the width of a window's sizing border. the parameter must point to an integer variable that
    /// receives this value.
    /// </summary>
    SPI_GETBORDER = 5U,
    /// <summary>Sets the border multiplier factor that determines the width of a window's sizing border. the parameter specifies the new value.</summary>
    SPI_SETBORDER = 6U,
    /// <summary>
    /// Retrieves the keyboard repeat-speed setting, which is a value in the range from 0 (approximately 2.5 repetitions per second) through 31 (approximately 30
    /// repetitions per second). The actual repeat rates are hardware-dependent and may vary from a linear scale by as much as 20%. the parameter must point to
    /// a DWORD variable that receives the setting.
    /// </summary>
    SPI_GETKEYBOARDSPEED = 10U,
    /// <summary>
    /// Sets the keyboard repeat-speed setting. the parameter must specify a value in the range from 0 (approximately 2.5 repetitions per second) through 31
    /// (approximately 30 repetitions per second). The actual repeat rates are hardware-dependent and may vary from a linear scale by as much as 20%. If parameter is
    /// greater than 31, the parameter is set to 31.
    /// </summary>
    SPI_SETKEYBOARDSPEED = 11U,
    /// <summary>Not implemented.</summary>
    SPI_LANGDRIVER = 12U,
    /// <summary>
    /// Sets or retrieves the width, in pixels, of an icon cell. The system uses this rectangle to arrange icons in large icon view. To set this value, set parameter to
    /// the new value and set parameter to null. You cannot set this value to less than SM_CXICON. To retrieve this value, parameter must point to an integer that receives
    /// the current value.
    /// </summary>
    SPI_ICONHORIZONTALSPACING = 13U,
    /// <summary>Retrieves the screen saver time-out value, in seconds. the parameter must point to an integer variable that receives the value.</summary>
    SPI_GETSCREENSAVETIMEOUT = 14U,
    /// <summary>
    /// Sets the screen saver time-out value to the value of the parameter. This value is the amount of time, in seconds, that the system must be idle before
    /// the screen saver activates.
    /// </summary>
    SPI_SETSCREENSAVETIMEOUT = 15U,
    /// <summary>
    /// Determines whether screen saving is enabled. the parameter must point to a bool variable that receives TRUE if screen saving is enabled, or FALSE
    /// otherwise. Does not work for Windows 7: http://msdn.microsoft.com/en-us/library/windows/desktop/ms724947(v=vs.85).aspx.
    /// </summary>
    SPI_GETSCREENSAVEACTIVE = 16U,
    /// <summary>Sets the state of the screen saver. the parameter specifies TRUE to activate screen saving, or FALSE to deactivate it.</summary>
    SPI_SETSCREENSAVEACTIVE = 17U,
    /// <summary>Retrieves the current granularity value of the desktop sizing grid. the parameter must point to an integer variable that receives the granularity.</summary>
    SPI_GETGRIDGRANULARITY = 18U,
    /// <summary>Sets the granularity of the desktop sizing grid to the value of the parameter.</summary>
    SPI_SETGRIDGRANULARITY = 19U,
    /// <summary>
    /// Sets the desktop wallpaper. The value of the parameter determines the new wallpaper. To specify a wallpaper bitmap, set parameter to point to a
    /// null-terminated string containing the name of a bitmap file. Setting parameter to "" removes the wallpaper. Setting parameter to SETWALLPAPER_DEFAULT or null
    /// reverts to the default wallpaper.
    /// </summary>
    SPI_SETDESKWALLPAPER = 20U,
    /// <summary>Sets the current desktop pattern by causing Windows to read the Pattern= setting from the WIN.INI file.</summary>
    SPI_SETDESKPATTERN = 21U,
    /// <summary>
    /// Retrieves the keyboard repeat-delay setting, which is a value in the range from 0 (approximately 250 ms delay) through 3 (approximately 1 second delay). The
    /// actual delay associated with each value may vary depending on the hardware. the parameter must point to an integer variable that receives the setting.
    /// </summary>
    SPI_GETKEYBOARDDELAY = 22U,
    /// <summary>
    /// Sets the keyboard repeat-delay setting. the parameter must specify 0, 1, 2, or 3, where zero sets the shortest delay (approximately 250 ms) and 3 sets
    /// the longest delay (approximately 1 second). The actual delay associated with each value may vary depending on the hardware.
    /// </summary>
    SPI_SETKEYBOARDDELAY = 23U,
    /// <summary>
    /// Sets or retrieves the height, in pixels, of an icon cell. To set this value, set parameter to the new value and set parameter to null. You cannot set this value to
    /// less than SM_CYICON. To retrieve this value, parameter must point to an integer that receives the current value.
    /// </summary>
    SPI_ICONVERTICALSPACING = 24U,
    /// <summary>Determines whether icon-title wrapping is enabled. the parameter must point to a bool variable that receives TRUE if enabled, or FALSE otherwise.</summary>
    SPI_GETICONTITLEWRAP = 25U,
    /// <summary>Turns icon-title wrapping on or off. the parameter specifies TRUE for on, or FALSE for off.</summary>
    SPI_SETICONTITLEWRAP = 26U,
    /// <summary>
    /// Determines whether pop-up menus are left-aligned or right-aligned, relative to the corresponding menu-bar item. the parameter must point to a bool
    /// variable that receives TRUE if left-aligned, or FALSE otherwise.
    /// </summary>
    SPI_GETMENUDROPALIGNMENT = 27U,
    /// <summary>Sets the alignment value of pop-up menus. the parameter specifies TRUE for right alignment, or FALSE for left alignment.</summary>
    SPI_SETMENUDROPALIGNMENT = 28U,
    /// <summary>
    /// Sets the width of the double-click rectangle to the value of the parameter. The double-click rectangle is the rectangle within which the second click of
    /// a double-click must fall for it to be registered as a double-click. To retrieve the width of the double-click rectangle, call GetSystemMetrics with the
    /// SM_CXDOUBLECLK flag.
    /// </summary>
    SPI_SETDOUBLECLKWIDTH = 29U,
    /// <summary>
    /// Sets the height of the double-click rectangle to the value of the parameter. The double-click rectangle is the rectangle within which the second click
    /// of a double-click must fall for it to be registered as a double-click. To retrieve the height of the double-click rectangle, call GetSystemMetrics with the
    /// SM_CYDOUBLECLK flag.
    /// </summary>
    SPI_SETDOUBLECLKHEIGHT = 30U,
    /// <summary>
    /// Retrieves the logical font information for the current icon-title font. the parameter specifies the size of a LOGFONT structure and must point to the
    /// LOGFONT structure to fill in.
    /// </summary>
    SPI_GETICONTITLELOGFONT = 31U,
    /// <summary>
    /// Sets the double-click time for the mouse to the value of the parameter. The double-click time is the maximum number of milliseconds that can occur
    /// between the first and second clicks of a double-click. You can also call the SetDoubleClickTime function to set the double-click time. To get the current
    /// double-click time, call the GetDoubleClickTime function.
    /// </summary>
    SPI_SETDOUBLECLICKTIME = 32U,
    /// <summary>
    /// Swaps or restores the meaning of the left and right mouse buttons. the parameter specifies TRUE to swap the meanings of the buttons, or FALSE to restore
    /// their original meanings.
    /// </summary>
    SPI_SETMOUSEBUTTONSWAP = 33U,
    /// <summary>Sets the font that is used for icon titles. the parameter specifies the size of a LOGFONT structure, and the parameter must point to a LOGFONT structure.</summary>
    SPI_SETICONTITLELOGFONT = 34U,
    /// <summary>
    /// This flag is obsolete. Previous versions of the system use this flag to determine whether ALT+TAB fast task switching is enabled. For Windows 95, Windows 98,
    /// and Windows NT version 4.0 and later, fast task switching is always enabled.
    /// </summary>
    SPI_GETFASTTASKSWITCH = 35U,
    /// <summary>
    /// This flag is obsolete. Previous versions of the system use this flag to enable or disable ALT+TAB fast task switching. For Windows 95, Windows 98, and Windows
    /// NT version 4.0 and later, fast task switching is always enabled.
    /// </summary>
    SPI_SETFASTTASKSWITCH = 36U,
    /// <summary>
    /// Sets dragging of full windows either on or off. the parameter specifies TRUE for on, or FALSE for off. Windows 95: This flag is supported only if
    /// Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
    /// </summary>
    SPI_SETDRAGFULLWINDOWS = 37U,
    /// <summary>
    /// Determines whether dragging of full windows is enabled. the parameter must point to a BOOL variable that receives TRUE if enabled, or FALSE otherwise.
    /// Windows 95: This flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
    /// </summary>
    SPI_GETDRAGFULLWINDOWS = 38U,
    /// <summary>
    /// Retrieves the metrics associated with the nonclient area of nonminimized windows. the parameter must point to a NONCLIENTMETRICS structure that receives
    /// the information. Set the cbSize member of this structure and the parameter to sizeof(NONCLIENTMETRICS).
    /// </summary>
    SPI_GETNONCLIENTMETRICS = 41U,
    /// <summary>
    /// Sets the metrics associated with the nonclient area of nonminimized windows. the parameter must point to a NONCLIENTMETRICS structure that contains the
    /// new parameters. Set the cbSize member of this structure and the parameter to sizeof(NONCLIENTMETRICS). Also, the lfHeight member of the LOGFONT
    /// structure must be a negative value.
    /// </summary>
    SPI_SETNONCLIENTMETRICS = 42U,
    /// <summary>
    /// Retrieves the metrics associated with minimized windows. the parameter must point to a MINIMIZEDMETRICS structure that receives the information. Set the
    /// cbSize member of this structure and the parameter to sizeof(MINIMIZEDMETRICS).
    /// </summary>
    SPI_GETMINIMIZEDMETRICS = 43U,
    /// <summary>
    /// Sets the metrics associated with minimized windows. the parameter must point to a MINIMIZEDMETRICS structure that contains the new parameters. Set the
    /// cbSize member of this structure and the parameter to sizeof(MINIMIZEDMETRICS).
    /// </summary>
    SPI_SETMINIMIZEDMETRICS = 44U,
    /// <summary>
    /// Retrieves the metrics associated with icons. the parameter must point to an ICONMETRICS structure that receives the information. Set the cbSize member
    /// of this structure and the parameter to sizeof(ICONMETRICS).
    /// </summary>
    SPI_GETICONMETRICS = 45U,
    /// <summary>
    /// Sets the metrics associated with icons. the parameter must point to an ICONMETRICS structure that contains the new parameters. Set the cbSize member of
    /// this structure and the parameter to sizeof(ICONMETRICS).
    /// </summary>
    SPI_SETICONMETRICS = 46U,
    /// <summary>
    /// Sets the size of the work area. The work area is the portion of the screen not obscured by the system taskbar or by application desktop toolbars. the parameter
    /// is a pointer to a RECT structure that specifies the new work area rectangle, expressed in virtual screen coordinates. In a system with multiple
    /// display monitors, the function sets the work area of the monitor that contains the specified rectangle.
    /// </summary>
    SPI_SETWORKAREA = 47U,
    /// <summary>
    /// Retrieves the size of the work area on the primary display monitor. The work area is the portion of the screen not obscured by the system taskbar or by
    /// application desktop toolbars. the parameter must point to a RECT structure that receives the coordinates of the work area, expressed in virtual screen
    /// coordinates. To get the work area of a monitor other than the primary display monitor, call the GetMonitorInfo function.
    /// </summary>
    SPI_GETWORKAREA = 48U,
    /// <summary>Windows Me/98/95: Pen windows is being loaded or unloaded. the parameter is TRUE when loading and FALSE when unloading pen windows. the parameter is null.</summary>
    SPI_SETPENWINDOWS = 49U,
    /// <summary>
    /// Retrieves information about the HighContrast accessibility feature. the parameter must point to a HIGHCONTRAST structure that receives the information.
    /// Set the cbSize member of this structure and the parameter to sizeof(HIGHCONTRAST). For a general discussion, see remarks. Windows NT: This value is not
    /// supported.
    /// </summary>
    /// <remarks>
    ///     There is a difference between the High Contrast color scheme and the High Contrast Mode. The High Contrast color
    ///     scheme changes
    ///     the system colors to colors that have obvious contrast; you switch to this color scheme by using the Display
    ///     Options in the control panel.
    ///     The High Contrast Mode, which uses SPI_GETHIGHCONTRAST and SPI_SETHIGHCONTRAST, advises applications to modify
    ///     their appearance
    ///     for visually-impaired users. It involves such things as audible warning to users and customized color scheme
    ///     (using the Accessibility Options in the control panel). For more information, see HIGHCONTRAST on MSDN.
    ///     For more information on general accessibility features, see Accessibility on MSDN.
    /// </remarks>
    SPI_GETHIGHCONTRAST = 66U,
    /// <summary>
    /// Sets the parameters of the HighContrast accessibility feature. the parameter must point to a HIGHCONTRAST structure that contains the new parameters.
    /// Set the cbSize member of this structure and the parameter to sizeof(HIGHCONTRAST). Windows NT: This value is not supported.
    /// </summary>
    SPI_SETHIGHCONTRAST = 67U,
    /// <summary>
    /// Determines whether the user relies on the keyboard instead of the mouse, and wants applications to display keyboard interfaces that would otherwise be hidden.
    /// the parameter must point to a BOOL variable that receives TRUE if the user relies on the keyboard; or FALSE otherwise. Windows NT: This value is not
    /// supported.
    /// </summary>
    SPI_GETKEYBOARDPREF = 68U,
    /// <summary>
    /// Sets the keyboard preference. the parameter specifies TRUE if the user relies on the keyboard instead of the mouse, and wants applications to display
    /// keyboard interfaces that would otherwise be hidden; parameter is FALSE otherwise. Windows NT: This value is not supported.
    /// </summary>
    SPI_SETKEYBOARDPREF = 69U,
    /// <summary>
    /// Determines whether a screen reviewer utility is running. A screen reviewer utility directs textual information to an output device, such as a speech synthesizer
    /// or Braille display. When this flag is set, an application should provide textual information in situations where it would otherwise present the information
    /// graphically. the parameter is a pointer to a BOOL variable that receives TRUE if a screen reviewer utility is running, or FALSE otherwise. Windows NT:
    /// This value is not supported.
    /// </summary>
    SPI_GETSCREENREADER = 70U,
    /// <summary>Determines whether a screen review utility is running. the parameter specifies TRUE for on, or FALSE for off. Windows NT: This value is not supported.</summary>
    SPI_SETSCREENREADER = 71U,
    /// <summary>
    /// Retrieves the animation effects associated with user actions. the parameter must point to an ANIMATIONINFO structure that receives the information. Set
    /// the cbSize member of this structure and the parameter to sizeof(ANIMATIONINFO).
    /// </summary>
    SPI_GETANIMATION = 72U,
    /// <summary>
    /// Sets the animation effects associated with user actions. the parameter must point to an ANIMATIONINFO structure that contains the new parameters. Set
    /// the cbSize member of this structure and the parameter to sizeof(ANIMATIONINFO).
    /// </summary>
    SPI_SETANIMATION = 73U,
    /// <summary>
    /// Determines whether the font smoothing feature is enabled. This feature uses font antialiasing to make font curves appear smoother by painting pixels at
    /// different gray levels. the parameter must point to a BOOL variable that receives TRUE if the feature is enabled, or FALSE if it is not. Windows 95: This
    /// flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
    /// </summary>
    SPI_GETFONTSMOOTHING = 74U,
    /// <summary>
    /// Enables or disables the font smoothing feature, which uses font antialiasing to make font curves appear smoother by painting pixels at different gray levels. To
    /// enable the feature, set the parameter to TRUE. To disable the feature, set parameter to FALSE. Windows 95: This flag is supported only if Windows Plus! is
    /// installed. See SPI_GETWINDOWSEXTENSION.
    /// </summary>
    SPI_SETFONTSMOOTHING = 75U,
    /// <summary>
    /// Sets the width, in pixels, of the rectangle used to detect the start of a drag operation. Set parameter to the new value. To retrieve the drag width, call
    /// GetSystemMetrics with the SM_CXDRAG flag.
    /// </summary>
    SPI_SETDRAGWIDTH = 76U,
    /// <summary>
    /// Sets the height, in pixels, of the rectangle used to detect the start of a drag operation. Set parameter to the new value. To retrieve the drag height, call
    /// GetSystemMetrics with the SM_CYDRAG flag.
    /// </summary>
    SPI_SETDRAGHEIGHT = 77U,
    /// <summary>Used internally; applications should not use this value.</summary>
    SPI_SETHANDHELD = 78U,
    /// <summary>
    /// Retrieves the time-out value for the low-power phase of screen saving. the parameter must point to an integer variable that receives the value. This
    /// flag is supported for 32-bit applications only. Windows NT, Windows Me/98: This flag is supported for 16-bit and 32-bit applications. Windows 95: This flag is
    /// supported for 16-bit applications only.
    /// </summary>
    SPI_GETLOWPOWERTIMEOUT = 79U,
    /// <summary>
    /// Retrieves the time-out value for the power-off phase of screen saving. the parameter must point to an integer variable that receives the value. This
    /// flag is supported for 32-bit applications only. Windows NT, Windows Me/98: This flag is supported for 16-bit and 32-bit applications. Windows 95: This flag is
    /// supported for 16-bit applications only.
    /// </summary>
    SPI_GETPOWEROFFTIMEOUT = 80U,
    /// <summary>
    /// Sets the time-out value, in seconds, for the low-power phase of screen saving. the parameter specifies the new value. the parameter must be
    /// null. This flag is supported for 32-bit applications only. Windows NT, Windows Me/98: This flag is supported for 16-bit and 32-bit applications. Windows 95:
    /// This flag is supported for 16-bit applications only.
    /// </summary>
    SPI_SETLOWPOWERTIMEOUT = 81U,
    /// <summary>
    /// Sets the time-out value, in seconds, for the power-off phase of screen saving. the parameter specifies the new value. the parameter must be
    /// null. This flag is supported for 32-bit applications only. Windows NT, Windows Me/98: This flag is supported for 16-bit and 32-bit applications. Windows 95:
    /// This flag is supported for 16-bit applications only.
    /// </summary>
    SPI_SETPOWEROFFTIMEOUT = 82U,
    /// <summary>
    /// Determines whether the low-power phase of screen saving is enabled. the parameter must point to a BOOL variable that receives TRUE if enabled, or FALSE
    /// if disabled. This flag is supported for 32-bit applications only. Windows NT, Windows Me/98: This flag is supported for 16-bit and 32-bit applications. Windows
    /// 95: This flag is supported for 16-bit applications only.
    /// </summary>
    SPI_GETLOWPOWERACTIVE = 83U,
    /// <summary>
    /// Determines whether the power-off phase of screen saving is enabled. the parameter must point to a BOOL variable that receives TRUE if enabled, or FALSE
    /// if disabled. This flag is supported for 32-bit applications only. Windows NT, Windows Me/98: This flag is supported for 16-bit and 32-bit applications. Windows
    /// 95: This flag is supported for 16-bit applications only.
    /// </summary>
    SPI_GETPOWEROFFACTIVE = 84U,
    /// <summary>
    /// Activates or deactivates the low-power phase of screen saving. Set parameter to 1 to activate, or zero to deactivate. the parameter must be null. This
    /// flag is supported for 32-bit applications only. Windows NT, Windows Me/98: This flag is supported for 16-bit and 32-bit applications. Windows 95: This flag is
    /// supported for 16-bit applications only.
    /// </summary>
    SPI_SETLOWPOWERACTIVE = 85U,
    /// <summary>
    /// Activates or deactivates the power-off phase of screen saving. Set parameter to 1 to activate, or zero to deactivate. the parameter must be null. This
    /// flag is supported for 32-bit applications only. Windows NT, Windows Me/98: This flag is supported for 16-bit and 32-bit applications. Windows 95: This flag is
    /// supported for 16-bit applications only.
    /// </summary>
    SPI_SETPOWEROFFACTIVE = 86U,
    /// <summary>Reloads the system cursors. Set the parameter to zero and the parameter to null.</summary>
    SPI_SETCURSORS = 87U,
    /// <summary>Reloads the system icons. Set the parameter to zero and the parameter to null.</summary>
    SPI_SETICONS = 88U,
    /// <summary>
    /// Retrieves the input locale identifier for the system default input language. the parameter must point to an HKL variable that receives this value. For
    /// more information, see Languages, Locales, and Keyboard Layouts on MSDN.
    /// </summary>
    SPI_GETDEFAULTINPUTLANG = 89U,
    /// <summary>
    /// Sets the default input language for the system shell and applications. The specified language must be displayable using the current system character set. The
    /// parameter must point to an HKL variable that contains the input locale identifier for the default language. For more information, see Languages,
    /// Locales, and Keyboard Layouts on MSDN.
    /// </summary>
    SPI_SETDEFAULTINPUTLANG = 90U,
    /// <summary>
    /// Sets the hot key set for switching between input languages. the parameter and parameters are not used. The value sets the shortcut keys in the keyboard
    /// property sheets by reading the registry again. The registry must be set before this flag is used. the path in the registry is \HKEY_CURRENT_USER\keyboard
    /// layout\toggle. Valid values are "1" = ALT+SHIFT, "2" = CTRL+SHIFT, and "3" = none.
    /// </summary>
    SPI_SETLANGTOGGLE = 91U,
    /// <summary>
    /// Windows 95: Determines whether the Windows extension, Windows Plus!, is installed. Set the parameter to 1. the parameter is not used. The
    /// function returns TRUE if the extension is installed, or FALSE if it is not.
    /// </summary>
    SPI_GETWINDOWSEXTENSION = 92U,
    /// <summary>
    /// Enables or disables the Mouse Trails feature, which improves the visibility of mouse cursor movements by briefly showing a trail of cursors and quickly erasing
    /// them. To disable the feature, set the parameter to zero or 1. To enable the feature, set parameter to a value greater than 1 to indicate the number of
    /// cursors drawn in the trail. Windows 2000/NT: This value is not supported.
    /// </summary>
    SPI_SETMOUSETRAILS = 93U,
    /// <summary>
    /// Determines whether the Mouse Trails feature is enabled. This feature improves the visibility of mouse cursor movements by briefly showing a trail of cursors and
    /// quickly erasing them. the parameter must point to an integer variable that receives a value. If the value is zero or 1, the feature is disabled. If the
    /// value is greater than 1, the feature is enabled and the value indicates the number of cursors drawn in the trail. the parameter is not used. Windows
    /// 2000/NT: This value is not supported.
    /// </summary>
    SPI_GETMOUSETRAILS = 94U,
    /// <summary>Windows Me/98: Used internally; applications should not use this flag.</summary>
    SPI_SETSCREENSAVERRUNNING = 97U,
    /// <summary>Same as SPI_SETSCREENSAVERRUNNING.</summary>
    SPI_SCREENSAVERRUNNING = SPI_SETSCREENSAVERRUNNING,
    /// <summary>
    /// Retrieves information about the FilterKeys accessibility feature. the parameter must point to a FILTERKEYS structure that receives the information. Set
    /// the cbSize member of this structure and the parameter to sizeof(FILTERKEYS).
    /// </summary>
    SPI_GETFILTERKEYS = 50U,
    /// <summary>
    /// Sets the parameters of the FilterKeys accessibility feature. the parameter must point to a FILTERKEYS structure that contains the new parameters. Set
    /// the cbSize member of this structure and the parameter to sizeof(FILTERKEYS).
    /// </summary>
    SPI_SETFILTERKEYS = 51U,
    /// <summary>
    /// Retrieves information about the ToggleKeys accessibility feature. the parameter must point to a TOGGLEKEYS structure that receives the information. Set
    /// the cbSize member of this structure and the parameter to sizeof(TOGGLEKEYS).
    /// </summary>
    SPI_GETTOGGLEKEYS = 52U,
    /// <summary>
    /// Sets the parameters of the ToggleKeys accessibility feature. the parameter must point to a TOGGLEKEYS structure that contains the new parameters. Set
    /// the cbSize member of this structure and the parameter to sizeof(TOGGLEKEYS).
    /// </summary>
    SPI_SETTOGGLEKEYS = 53U,
    /// <summary>
    /// Retrieves information about the MouseKeys accessibility feature. the parameter must point to a MOUSEKEYS structure that receives the information. Set
    /// the cbSize member of this structure and the parameter to sizeof(MOUSEKEYS).
    /// </summary>
    SPI_GETMOUSEKEYS = 54U,
    /// <summary>
    /// Sets the parameters of the MouseKeys accessibility feature. the parameter must point to a MOUSEKEYS structure that contains the new parameters. Set the
    /// cbSize member of this structure and the parameter to sizeof(MOUSEKEYS).
    /// </summary>
    SPI_SETMOUSEKEYS = 55U,
    /// <summary>
    /// Determines whether the Show Sounds accessibility flag is on or off. If it is on, the user requires an application to present information visually in situations
    /// where it would otherwise present the information only in audible form. the parameter must point to a BOOL variable that receives TRUE if the feature is
    /// on, or FALSE if it is off. Using this value is equivalent to calling GetSystemMetrics (SM_SHOWSOUNDS). That is the recommended call.
    /// </summary>
    SPI_GETSHOWSOUNDS = 56U,
    /// <summary>
    /// Sets the parameters of the SoundSentry accessibility feature. the parameter must point to a SOUNDSENTRY structure that contains the new parameters. Set
    /// the cbSize member of this structure and the parameter to sizeof(SOUNDSENTRY).
    /// </summary>
    SPI_SETSHOWSOUNDS = 57U,
    /// <summary>
    /// Retrieves information about the StickyKeys accessibility feature. the parameter must point to a STICKYKEYS structure that receives the information. Set
    /// the cbSize member of this structure and the parameter to sizeof(STICKYKEYS).
    /// </summary>
    SPI_GETSTICKYKEYS = 58U,
    /// <summary>
    /// Sets the parameters of the StickyKeys accessibility feature. the parameter must point to a STICKYKEYS structure that contains the new parameters. Set
    /// the cbSize member of this structure and the parameter to sizeof(STICKYKEYS).
    /// </summary>
    SPI_SETSTICKYKEYS = 59U,
    /// <summary>
    /// Retrieves information about the time-out period associated with the accessibility features. the parameter must point to an ACCESSTIMEOUT structure that
    /// receives the information. Set the cbSize member of this structure and the parameter to sizeof(ACCESSTIMEOUT).
    /// </summary>
    SPI_GETACCESSTIMEOUT = 60U,
    /// <summary>
    /// Sets the time-out period associated with the accessibility features. the parameter must point to an ACCESSTIMEOUT structure that contains the new
    /// parameters. Set the cbSize member of this structure and the parameter to sizeof(ACCESSTIMEOUT).
    /// </summary>
    SPI_SETACCESSTIMEOUT = 61U,
    /// <summary>
    /// Windows Me/98/95: Retrieves information about the SerialKeys accessibility feature. the parameter must point to a SERIALKEYS structure that receives the
    /// information. Set the cbSize member of this structure and the parameter to sizeof(SERIALKEYS). Windows Server 2003, Windows XP/2000/NT: Not supported.
    /// The user controls this feature through the control panel.
    /// </summary>
    SPI_GETSERIALKEYS = 62U,
    /// <summary>
    /// Windows Me/98/95: Sets the parameters of the SerialKeys accessibility feature. the parameter must point to a SERIALKEYS structure that contains the new
    /// parameters. Set the cbSize member of this structure and the parameter to sizeof(SERIALKEYS). Windows Server 2003, Windows XP/2000/NT: Not supported. The
    /// user controls this feature through the control panel.
    /// </summary>
    SPI_SETSERIALKEYS = 63U,
    /// <summary>
    /// Retrieves information about the SoundSentry accessibility feature. the parameter must point to a SOUNDSENTRY structure that receives the information.
    /// Set the cbSize member of this structure and the parameter to sizeof(SOUNDSENTRY).
    /// </summary>
    SPI_GETSOUNDSENTRY = 64U,
    /// <summary>
    /// Sets the parameters of the SoundSentry accessibility feature. the parameter must point to a SOUNDSENTRY structure that contains the new parameters. Set
    /// the cbSize member of this structure and the parameter to sizeof(SOUNDSENTRY).
    /// </summary>
    SPI_SETSOUNDSENTRY = 65U,
    /// <summary>
    /// Determines whether the snap-to-default-button feature is enabled. If enabled, the mouse cursor automatically moves to the default button, such as OK or Apply,
    /// of a dialog box. the parameter must point to a BOOL variable that receives TRUE if the feature is on, or FALSE if it is off. Windows 95: Not supported.
    /// </summary>
    SPI_GETSNAPTODEFBUTTON = 95U,
    /// <summary>
    /// Enables or disables the snap-to-default-button feature. If enabled, the mouse cursor automatically moves to the default button, such as OK or Apply, of a dialog
    /// box. Set the parameter to TRUE to enable the feature, or FALSE to disable it. Applications should use the ShowWindow function when displaying a dialog
    /// box so the dialog manager can position the mouse cursor. Windows 95: Not supported.
    /// </summary>
    SPI_SETSNAPTODEFBUTTON = 96U,
    /// <summary>
    /// Retrieves the width, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent to generate a WM_MOUSEHOVER message. The argument
    /// parameter must point to a UINT variable that receives the width. Windows 95: Not supported.
    /// </summary>
    SPI_GETMOUSEHOVERWIDTH = 98U,
    /// <summary>
    /// Retrieves the width, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent to generate a WM_MOUSEHOVER message. The argument
    /// parameter must point to a UINT variable that receives the width. Windows 95: Not supported.
    /// </summary>
    SPI_SETMOUSEHOVERWIDTH = 99U,
    /// <summary>
    /// Retrieves the height, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent to generate a WM_MOUSEHOVER message. The
    /// parameter must point to a UINT variable that receives the height. Windows 95: Not supported.
    /// </summary>
    SPI_GETMOUSEHOVERHEIGHT = 100U,
    /// <summary>
    /// Sets the height, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent to generate a WM_MOUSEHOVER message. Set the
    /// parameter to the new height. Windows 95: Not supported.
    /// </summary>
    SPI_SETMOUSEHOVERHEIGHT = 101U,
    /// <summary>
    /// Retrieves the time, in milliseconds, that the mouse pointer has to stay in the hover rectangle for TrackMouseEvent to generate a WM_MOUSEHOVER message. The
    /// parameter must point to a UINT variable that receives the time. Windows 95: Not supported.
    /// </summary>
    SPI_GETMOUSEHOVERTIME = 102U,
    /// <summary>
    /// Sets the time, in milliseconds, that the mouse pointer has to stay in the hover rectangle for TrackMouseEvent to generate a WM_MOUSEHOVER message. This is used
    /// only if you pass HOVER_DEFAULT in the dwHoverTime parameter in the call to TrackMouseEvent. Set the parameter to the new time. Windows 95: Not
    /// supported.
    /// </summary>
    SPI_SETMOUSEHOVERTIME = 103U,
    /// <summary>
    /// Retrieves the number of lines to scroll when the mouse wheel is rotated. the parameter must point to a UINT variable that receives the number of lines.
    /// The default value is 3. Windows 95: Not supported.
    /// </summary>
    SPI_GETWHEELSCROLLLINES = 104U,
    /// <summary>
    /// Sets the number of lines to scroll when the mouse wheel is rotated. The number of lines is set from the parameter. The number of lines is the suggested
    /// number of lines to scroll when the mouse wheel is rolled without using modifier keys. If the number is 0, then no scrolling should occur. If the number of lines
    /// to scroll is greater than the number of lines viewable, and in particular if it is WHEEL_PAGESCROLL (#defined as UINT_MAX), the scroll operation should be
    /// interpreted as clicking once in the page down or page up regions of the scroll bar. Windows 95: Not supported.
    /// </summary>
    SPI_SETWHEELSCROLLLINES = 105U,
    /// <summary>
    /// Retrieves the time, in milliseconds, that the system waits before displaying a shortcut menu when the mouse cursor is over a submenu item. the parameter
    /// must point to a DWORD variable that receives the time of the delay. Windows 95: Not supported.
    /// </summary>
    SPI_GETMENUSHOWDELAY = 106U,
    /// <summary>Sets parameter to the time, in milliseconds, that the system waits before displaying a shortcut menu when the mouse cursor is over a submenu item. Windows 95: Not supported.</summary>
    SPI_SETMENUSHOWDELAY = 107U,
    /// <summary>
    /// Determines whether the IME status window is visible (on a per-user basis). the parameter must point to a BOOL variable that receives TRUE if the status
    /// window is visible, or FALSE if it is not. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_GETSHOWIMEUI = 110U,
    /// <summary>
    /// Sets whether the IME status window is visible or not on a per-user basis. the parameter specifies TRUE for on or FALSE for off. Windows NT, Windows 95:
    /// This value is not supported.
    /// </summary>
    SPI_SETSHOWIMEUI = 111U,
    /// <summary>
    /// Retrieves the current mouse speed. The mouse speed determines how far the pointer will move based on the distance the mouse moves. the parameter must
    /// point to an integer that receives a value which ranges between 1 (slowest) and 20 (fastest). A value of 10 is the default. The value can be set by an end user
    /// using the mouse control panel application or by an application using SPI_SETMOUSESPEED. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_GETMOUSESPEED = 112U,
    /// <summary>
    /// Sets the current mouse speed. the parameter is an integer between 1 (slowest) and 20 (fastest). A value of 10 is the default. This value is typically
    /// set using the mouse control panel application. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_SETMOUSESPEED = 113U,
    /// <summary>
    /// Determines whether a screen saver is currently running on the window station of the calling process. the parameter must point to a BOOL variable that
    /// receives TRUE if a screen saver is currently running, or FALSE otherwise. Note that only the interactive window station, "WinSta0", can have a screen saver
    /// running. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_GETSCREENSAVERRUNNING = 114U,
    /// <summary>
    /// Retrieves the full path of the bitmap file for the desktop wallpaper. the parameter must point to a buffer that receives a null-terminated path string.
    /// Set the parameter to the size, in characters, of the parameter buffer. The returned string will not exceed MAX_PATH characters. If there is no desktop
    /// wallpaper, the returned string is empty. Windows NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_GETDESKWALLPAPER = 115U,
    /// <summary>
    /// Determines whether active window tracking (activating the window the mouse is on) is on or off. the parameter must point to a BOOL variable that
    /// receives TRUE for on, or FALSE for off. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_GETACTIVEWINDOWTRACKING = 4096U,
    /// <summary>
    /// Sets active window tracking (activating the window the mouse is on) either on or off. Set parameter to TRUE for on or FALSE for off. Windows NT, Windows 95: This
    /// value is not supported.
    /// </summary>
    SPI_SETACTIVEWINDOWTRACKING = 4097U,
    /// <summary>
    /// Determines whether the menu animation feature is enabled. This master switch must be on to enable menu animation effects. the parameter must point to a
    /// BOOL variable that receives TRUE if animation is enabled and FALSE if it is disabled. If animation is enabled, SPI_GETMENUFADE indicates whether menus use fade
    /// or slide animation. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_GETMENUANIMATION = 4098U,
    /// <summary>
    /// Enables or disables menu animation. This master switch must be on for any menu animation to occur. the parameter is a BOOL variable; set parameter to TRUE
    /// to enable animation and FALSE to disable animation. If animation is enabled, SPI_GETMENUFADE indicates whether menus use fade or slide animation. Windows NT,
    /// Windows 95: This value is not supported.
    /// </summary>
    SPI_SETMENUANIMATION = 4099U,
    /// <summary>
    /// Determines whether the slide-open effect for combo boxes is enabled. the parameter must point to a BOOL variable that receives TRUE for enabled, or
    /// FALSE for disabled. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_GETCOMBOBOXANIMATION = 4100U,
    /// <summary>
    /// Enables or disables the slide-open effect for combo boxes. Set the parameter to TRUE to enable the gradient effect, or FALSE to disable it. Windows NT,
    /// Windows 95: This value is not supported.
    /// </summary>
    SPI_SETCOMBOBOXANIMATION = 4101U,
    /// <summary>
    /// Determines whether the smooth-scrolling effect for list boxes is enabled. the parameter must point to a BOOL variable that receives TRUE for enabled, or
    /// FALSE for disabled. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_GETLISTBOXSMOOTHSCROLLING = 4102U,
    /// <summary>
    /// Enables or disables the smooth-scrolling effect for list boxes. Set the parameter to TRUE to enable the smooth-scrolling effect, or FALSE to disable it.
    /// Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_SETLISTBOXSMOOTHSCROLLING = 4103U,
    /// <summary>
    /// Determines whether the gradient effect for window title bars is enabled. the parameter must point to a BOOL variable that receives TRUE for enabled, or
    /// FALSE for disabled. For more information about the gradient effect, see the GetSysColor function. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_GETGRADIENTCAPTIONS = 4104U,
    /// <summary>
    /// Enables or disables the gradient effect for window title bars. Set the parameter to TRUE to enable it, or FALSE to disable it. The gradient effect is
    /// possible only if the system has a color depth of more than 256 colors. For more information about the gradient effect, see the GetSysColor function. Windows NT,
    /// Windows 95: This value is not supported.
    /// </summary>
    SPI_SETGRADIENTCAPTIONS = 4105U,
    /// <summary>
    /// Determines whether menu access keys are always underlined. the parameter must point to a BOOL variable that receives TRUE if menu access keys are always
    /// underlined, and FALSE if they are underlined only when the menu is activated by the keyboard. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_GETKEYBOARDCUES = 4106U,
    /// <summary>
    /// Sets the underlining of menu access key letters. the parameter is a BOOL variable. Set parameter to TRUE to always underline menu access keys, or FALSE to
    /// underline menu access keys only when the menu is activated from the keyboard. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_SETKEYBOARDCUES = 4107U,
    /// <summary>Same as SPI_GETKEYBOARDCUES.</summary>
    SPI_GETMENUUNDERLINES = SPI_GETKEYBOARDCUES,
    /// <summary>Same as SPI_SETKEYBOARDCUES.</summary>
    SPI_SETMENUUNDERLINES = SPI_SETKEYBOARDCUES,
    /// <summary>
    /// Determines whether windows activated through active window tracking will be brought to the top. the parameter must point to a BOOL variable that
    /// receives TRUE for on, or FALSE for off. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_GETACTIVEWNDTRKZORDER = 4108U,
    /// <summary>
    /// Determines whether or not windows activated through active window tracking should be brought to the top. Set parameter to TRUE for on or FALSE for off. Windows
    /// NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_SETACTIVEWNDTRKZORDER = 4109U,
    /// <summary>
    /// Determines whether hot tracking of user-interface elements, such as menu names on menu bars, is enabled. the parameter must point to a BOOL variable
    /// that receives TRUE for enabled, or FALSE for disabled. Hot tracking means that when the cursor moves over an item, it is highlighted but not selected. You can
    /// query this value to decide whether to use hot tracking in the user interface of your application. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_GETHOTTRACKING = 4110U,
    /// <summary>
    /// Enables or disables hot tracking of user-interface elements such as menu names on menu bars. Set the parameter to TRUE to enable it, or FALSE to disable
    /// it. Hot-tracking means that when the cursor moves over an item, it is highlighted but not selected. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_SETHOTTRACKING = 4111U,
    /// <summary>
    /// Determines whether menu fade animation is enabled. the parameter must point to a BOOL variable that receives TRUE when fade animation is enabled and
    /// FALSE when it is disabled. If fade animation is disabled, menus use slide animation. This flag is ignored unless menu animation is enabled, which you can do
    /// using the SPI_SETMENUANIMATION flag. For more information, see AnimateWindow. Windows NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_GETMENUFADE = 4114U,
    /// <summary>
    /// Enables or disables menu fade animation. Set parameter to TRUE to enable the menu fade effect or FALSE to disable it. If fade animation is disabled, menus use
    /// slide animation. he The menu fade effect is possible only if the system has a color depth of more than 256 colors. This flag is ignored unless SPI_MENUANIMATION
    /// is also set. For more information, see AnimateWindow. Windows NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_SETMENUFADE = 4115U,
    /// <summary>
    /// Determines whether the selection fade effect is enabled. the parameter must point to a BOOL variable that receives TRUE if enabled or FALSE if disabled.
    /// The selection fade effect causes the menu item selected by the user to remain on the screen briefly while fading out after the menu is dismissed. Windows NT,
    /// Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_GETSELECTIONFADE = 4116U,
    /// <summary>
    /// Set parameter to TRUE to enable the selection fade effect or FALSE to disable it. The selection fade effect causes the menu item selected by the user to remain on
    /// the screen briefly while fading out after the menu is dismissed. The selection fade effect is possible only if the system has a color depth of more than 256
    /// colors. Windows NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_SETSELECTIONFADE = 4117U,
    /// <summary>
    /// Determines whether ToolTip animation is enabled. the parameter must point to a BOOL variable that receives TRUE if enabled or FALSE if disabled. If
    /// ToolTip animation is enabled, SPI_GETTOOLTIPFADE indicates whether ToolTips use fade or slide animation. Windows NT, Windows Me/98/95: This value is not
    /// supported.
    /// </summary>
    SPI_GETTOOLTIPANIMATION = 4118U,
    /// <summary>
    /// Set parameter to TRUE to enable ToolTip animation or FALSE to disable it. If enabled, you can use SPI_SETTOOLTIPFADE to specify fade or slide animation. Windows
    /// NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_SETTOOLTIPANIMATION = 4119U,
    /// <summary>
    /// If SPI_SETTOOLTIPANIMATION is enabled, SPI_GETTOOLTIPFADE indicates whether ToolTip animation uses a fade effect or a slide effect. the parameter must
    /// point to a BOOL variable that receives TRUE for fade animation or FALSE for slide animation. For more information on slide and fade effects, see AnimateWindow.
    /// Windows NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_GETTOOLTIPFADE = 4120U,
    /// <summary>
    /// If the SPI_SETTOOLTIPANIMATION flag is enabled, use SPI_SETTOOLTIPFADE to indicate whether ToolTip animation uses a fade effect or a slide effect. Set parameter
    /// to TRUE for fade animation or FALSE for slide animation. The tooltip fade effect is possible only if the system has a color depth of more than 256 colors. For
    /// more information on the slide and fade effects, see the AnimateWindow function. Windows NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_SETTOOLTIPFADE = 4121U,
    /// <summary>
    /// Determines whether the cursor has a shadow around it. the parameter must point to a BOOL variable that receives TRUE if the shadow is enabled, FALSE if
    /// it is disabled. This effect appears only if the system has a color depth of more than 256 colors. Windows NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_GETCURSORSHADOW = 4122U,
    /// <summary>
    /// Enables or disables a shadow around the cursor. the parameter is a BOOL variable. Set parameter to TRUE to enable the shadow or FALSE to disable the
    /// shadow. This effect appears only if the system has a color depth of more than 256 colors. Windows NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_SETCURSORSHADOW = 4123U,
    /// <summary>
    /// Retrieves the state of the Mouse Sonar feature. the parameter must point to a BOOL variable that receives TRUE if enabled or FALSE otherwise. For more
    /// information, see About Mouse Input on MSDN. Windows 2000/NT, Windows 98/95: This value is not supported.
    /// </summary>
    SPI_GETMOUSESONAR = 4124U,
    /// <summary>
    /// Turns the Sonar accessibility feature on or off. This feature briefly shows several concentric circles around the mouse pointer when the user presses and
    /// releases the CTRL key. the parameter specifies TRUE for on and FALSE for off. The default is off. For more information, see About Mouse Input. Windows
    /// 2000/NT, Windows 98/95: This value is not supported.
    /// </summary>
    SPI_SETMOUSESONAR = 4125U,
    /// <summary>
    /// Retrieves the state of the Mouse ClickLock feature. the parameter must point to a BOOL variable that receives TRUE if enabled, or FALSE otherwise. For
    /// more information, see About Mouse Input. Windows 2000/NT, Windows 98/95: This value is not supported.
    /// </summary>
    SPI_GETMOUSECLICKLOCK = 4126U,
    /// <summary>
    /// Turns the Mouse ClickLock accessibility feature on or off. This feature temporarily locks down the primary mouse button when that button is clicked and held
    /// down for the time specified by SPI_SETMOUSECLICKLOCKTIME. the parameter specifies TRUE for on, or FALSE for off. The default is off. For more
    /// information, see Remarks and About Mouse Input on MSDN. Windows 2000/NT, Windows 98/95: This value is not supported.
    /// </summary>
    SPI_SETMOUSECLICKLOCK = 4127U,
    /// <summary>
    /// Retrieves the state of the Mouse Vanish feature. the parameter must point to a BOOL variable that receives TRUE if enabled or FALSE otherwise. For more
    /// information, see About Mouse Input on MSDN. Windows 2000/NT, Windows 98/95: This value is not supported.
    /// </summary>
    SPI_GETMOUSEVANISH = 4128U,
    /// <summary>
    /// Turns the Vanish feature on or off. This feature hides the mouse pointer when the user types; the pointer reappears when the user moves the mouse. The argument
    /// parameter specifies TRUE for on and FALSE for off. The default is off. For more information, see About Mouse Input on MSDN. Windows 2000/NT, Windows 98/95: This
    /// value is not supported.
    /// </summary>
    SPI_SETMOUSEVANISH = 4129U,
    /// <summary>
    /// Determines whether native User menus have flat menu appearance. the parameter must point to a BOOL variable that returns TRUE if the flat menu
    /// appearance is set, or FALSE otherwise. Windows 2000/NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_GETFLATMENU = 4130U,
    /// <summary>
    /// Enables or disables flat menu appearance for native User menus. Set parameter to TRUE to enable flat menu appearance or FALSE to disable it. When enabled, the
    /// menu bar uses COLOR_MENUBAR for the menubar background, COLOR_MENU for the menu-popup background, COLOR_MENUHILIGHT for the fill of the current menu selection,
    /// and COLOR_HILIGHT for the outline of the current menu selection. If disabled, menus are drawn using the same metrics and colors as in Windows 2000 and earlier.
    /// Windows 2000/NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_SETFLATMENU = 4131U,
    /// <summary>
    /// Determines whether the drop shadow effect is enabled. the parameter must point to a BOOL variable that returns TRUE if enabled or FALSE if disabled.
    /// Windows 2000/NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_GETDROPSHADOW = 4132U,
    /// <summary>
    /// Enables or disables the drop shadow effect. Set parameter to TRUE to enable the drop shadow effect or FALSE to disable it. You must also have CS_DROPSHADOW in the
    /// window class style. Windows 2000/NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_SETDROPSHADOW = 4133U,
    /// <summary>
    /// Retrieves a BOOL indicating whether an application can reset the screensaver's timer by calling the SendInput function to simulate keyboard or mouse input. The
    /// parameter must point to a BOOL variable that receives TRUE if the simulated input will be blocked, or FALSE otherwise.
    /// </summary>
    SPI_GETBLOCKSENDINPUTRESETS = 4134U,
    /// <summary>
    /// Determines whether an application can reset the screensaver's timer by calling the SendInput function to simulate keyboard or mouse input. the parameter
    /// specifies TRUE if the screensaver will not be deactivated by simulated input, or FALSE if the screensaver will be deactivated by simulated input.
    /// </summary>
    SPI_SETBLOCKSENDINPUTRESETS = 4135U,
    /// <summary>
    /// Determines whether UI effects are enabled or disabled. the parameter must point to a BOOL variable that receives TRUE if all UI effects are enabled, or
    /// FALSE if they are disabled. Windows NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_GETUIEFFECTS = 4158U,
    /// <summary>
    /// Enables or disables UI effects. Set the parameter to TRUE to enable all UI effects or FALSE to disable all UI effects. Windows NT, Windows Me/98/95:
    /// This value is not supported.
    /// </summary>
    SPI_SETUIEFFECTS = 4159U,
    /// <summary>
    /// Retrieves the amount of time following user input, in milliseconds, during which the system will not allow applications to force themselves into the foreground.
    /// the parameter must point to a DWORD variable that receives the time. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_GETFOREGROUNDLOCKTIMEOUT = 8192U,
    /// <summary>
    /// Sets the amount of time following user input, in milliseconds, during which the system does not allow applications to force themselves into the foreground. Set
    /// parameter to the new timeout value. The calling thread must be able to change the foreground window, otherwise the call fails. Windows NT, Windows 95: This value
    /// is not supported.
    /// </summary>
    SPI_SETFOREGROUNDLOCKTIMEOUT = 8193U,
    /// <summary>
    /// Retrieves the active window tracking delay, in milliseconds. the parameter must point to a DWORD variable that receives the time. Windows NT, Windows
    /// 95: This value is not supported.
    /// </summary>
    SPI_GETACTIVEWNDTRKTIMEOUT = 8194U,
    /// <summary>
    /// Sets the active window tracking delay. Set parameter to the number of milliseconds to delay before activating the window under the mouse pointer. Windows NT,
    /// Windows 95: This value is not supported.
    /// </summary>
    SPI_SETACTIVEWNDTRKTIMEOUT = 8195U,
    /// <summary>
    /// Retrieves the number of times SetForegroundWindow will flash the taskbar button when rejecting a foreground switch request. the parameter must point to
    /// a DWORD variable that receives the value. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_GETFOREGROUNDFLASHCOUNT = 8196U,
    /// <summary>
    /// Sets the number of times SetForegroundWindow will flash the taskbar button when rejecting a foreground switch request. Set parameter to the number of times to
    /// flash. Windows NT, Windows 95: This value is not supported.
    /// </summary>
    SPI_SETFOREGROUNDFLASHCOUNT = 8197U,
    /// <summary>
    /// Retrieves the caret width in edit controls, in pixels. the parameter must point to a DWORD that receives this value. Windows NT, Windows Me/98/95: This
    /// value is not supported.
    /// </summary>
    SPI_GETCARETWIDTH = 8198U,
    /// <summary>
    /// Sets the caret width in edit controls. Set parameter to the desired width, in pixels. The default and minimum value is 1. Windows NT, Windows Me/98/95: This value
    /// is not supported.
    /// </summary>
    SPI_SETCARETWIDTH = 8199U,
    /// <summary>
    /// Retrieves the time delay before the primary mouse button is locked. the parameter must point to DWORD that receives the time delay. This is only enabled
    /// if SPI_SETMOUSECLICKLOCK is set to TRUE. For more information, see About Mouse Input on MSDN. Windows 2000/NT, Windows 98/95: This value is not supported.
    /// </summary>
    SPI_GETMOUSECLICKLOCKTIME = 8200U,
    /// <summary>
    /// Turns the Mouse ClickLock accessibility feature on or off. This feature temporarily locks down the primary mouse button when that button is clicked and held
    /// down for the time specified by SPI_SETMOUSECLICKLOCKTIME. the parameter specifies TRUE for on, or FALSE for off. The default is off. For more
    /// information, see Remarks and About Mouse Input on MSDN. Windows 2000/NT, Windows 98/95: This value is not supported.
    /// </summary>
    SPI_SETMOUSECLICKLOCKTIME = 8201U,
    /// <summary>
    /// Retrieves the type of font smoothing. the parameter must point to a UINT that receives the information. Windows 2000/NT, Windows Me/98/95: This value is
    /// not supported.
    /// </summary>
    SPI_GETFONTSMOOTHINGTYPE = 8202U,
    /// <summary>
    /// Sets the font smoothing type. the parameter points to a UINT that contains either FE_FONTSMOOTHINGSTANDARD, if standard anti-aliasing is used, or
    /// FE_FONTSMOOTHINGCLEARTYPE, if ClearType is used. The default is FE_FONTSMOOTHINGSTANDARD. When using this option, the updateProfileFlags parameter must be set to
    /// SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE; otherwise, SystemParametersInfo fails.
    /// </summary>
    SPI_SETFONTSMOOTHINGTYPE = 8203U,
    /// <summary>
    /// Retrieves a contrast value that is used in ClearType™ smoothing. the parameter must point to a UINT that receives the information. Windows 2000/NT,
    /// Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_GETFONTSMOOTHINGCONTRAST = 8204U,
    /// <summary>
    /// Sets the contrast value used in ClearType smoothing. the parameter points to a UINT that holds the contrast value. Valid contrast values are from 1000
    /// to 2200. The default value is 1400. When using this option, the updateProfileFlags parameter must be set to SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE; otherwise,
    /// SystemParametersInfo fails. SPI_SETFONTSMOOTHINGTYPE must also be set to FE_FONTSMOOTHINGCLEARTYPE. Windows 2000/NT, Windows Me/98/95: This value is not
    /// supported.
    /// </summary>
    SPI_SETFONTSMOOTHINGCONTRAST = 8205U,
    /// <summary>
    /// Retrieves the width, in pixels, of the left and right edges of the focus rectangle drawn with DrawFocusRect. the parameter must point to a UINT. Windows
    /// 2000/NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_GETFOCUSBORDERWIDTH = 8206U,
    /// <summary>
    /// Sets the height of the left and right edges of the focus rectangle drawn with DrawFocusRect to the value of the parameter. Windows 2000/NT, Windows
    /// Me/98/95: This value is not supported.
    /// </summary>
    SPI_SETFOCUSBORDERWIDTH = 8207U,
    /// <summary>
    /// Retrieves the height, in pixels, of the top and bottom edges of the focus rectangle drawn with DrawFocusRect. the parameter must point to a UINT.
    /// Windows 2000/NT, Windows Me/98/95: This value is not supported.
    /// </summary>
    SPI_GETFOCUSBORDERHEIGHT = 8208U,
    /// <summary>
    /// Sets the height of the top and bottom edges of the focus rectangle drawn with DrawFocusRect to the value of the parameter. Windows 2000/NT, Windows
    /// Me/98/95: This value is not supported.
    /// </summary>
    SPI_SETFOCUSBORDERHEIGHT = 8209U,
    /// <summary>Not implemented.</summary>
    SPI_GETFONTSMOOTHINGORIENTATION = 8210U,
    /// <summary>Not implemented.</summary>
    SPI_SETFONTSMOOTHINGORIENTATION = 8211U
}
