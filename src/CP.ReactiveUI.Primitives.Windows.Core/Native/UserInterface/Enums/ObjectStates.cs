// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>Defines Microsoft Active Accessibility object state flags.</summary>
[Flags]
public enum ObjectStates : uint
{
    /// <summary>Indicates that the object does not have another state assigned to it.</summary>
    None = 0U,

    /// <summary>The object is unavailable.</summary>
    STATE_SYSTEM_UNAVAILABLE = 1U,

    /// <summary>The object is selected.</summary>
    STATE_SYSTEM_SELECTED = 2U,

    /// <summary>
    /// The object has the keyboard focus. Do not confuse object focus with object selection. For more information, see Selection and Focus Properties and Methods. For
    /// objects with this object state, send the EVENT_OBJECT_SHOW or EVENT_OBJECT_HIDE WinEvents to notify client applications about state changes. Do not use
    /// EVENT_OBJECT_STATECHANGE.
    /// </summary>
    STATE_SYSTEM_FOCUSED = 4U,

    /// <summary>The object is pressed.</summary>
    STATE_SYSTEM_PRESSED = 8U,

    /// <summary>The object's check box is selected.</summary>
    STATE_SYSTEM_CHECKED = 0x10U,

    /// <summary>
    /// Indicates that the state of a three-state check box or toolbar button is not determined. The check box is neither selected nor cleared and is therefore in the
    /// third or mixed state.
    /// </summary>
    STATE_SYSTEM_MIXED = 0x20U,

    /// <summary>Same as STATE_SYSTEM_MIXED.</summary>
    STATE_SYSTEM_INDETERMINATE = STATE_SYSTEM_MIXED,

    /// <summary>The object is designated read-only.</summary>
    STATE_SYSTEM_READONLY = 0x40U,

    /// <summary>The object is hot-tracked by the mouse, which means that the object's appearance has changed to indicate that the mouse pointer is located over it.</summary>
    STATE_SYSTEM_HOTTRACKED = 0x80U,

    /// <summary>This state represents the default button in a window.</summary>
    STATE_SYSTEM_DEFAULT = 0x100U,

    /// <summary>The object's children that have the ROLE_SYSTEM_OUTLINEITEM role are displayed.</summary>
    STATE_SYSTEM_EXPANDED = 0x200U,

    /// <summary>The object's children that have the ROLE_SYSTEM_OUTLINEITEM role are hidden.</summary>
    STATE_SYSTEM_COLLAPSED = 0x400U,

    /// <summary>The control cannot accept input at this time.</summary>
    STATE_SYSTEM_BUSY = 0x800U,

    /// <summary>The object is not clipped to the boundary of its parent object, and it does not move automatically when the parent moves.</summary>
    STATE_SYSTEM_FLOATING = 0x1000U,

    /// <summary>Indicates scrolling or moving text or graphics.</summary>
    STATE_SYSTEM_MARQUEED = 0x2000U,

    /// <summary>
    /// The object's appearance changes rapidly or constantly. Graphics that are animated occasionally are described as ROLE_SYSTEM_GRAPHIC with the State property set
    /// to STATE_SYSTEM_ANIMATED. This state is used to indicate that the object's location is changing.
    /// </summary>
    STATE_SYSTEM_ANIMATED = 0x4000U,

    /// <summary>
    /// The object is programmatically hidden. For example, menu itmes are programmatically hidden until a user activates the menu. Because objects with this state are
    /// not available to users, client applications must not communicate information about the object to users. However, if client applications find an object with this
    /// state, they should check whether STATE_SYSTEM_OFFSCREEN is also set. If this second state is defined, clients can communicate the information about the object
    /// to users. For example, a list box can have both STATE_SYSTEM_INVISIBLE and STATE_SYSTEM_OFFSCREEN set. In this case, the client application can communicate all
    /// items in the list to users. If a client application is navigating through an IAccessible tree and encounters a parent object that is invisible, Microsoft Active
    /// Accessibility will not expose information about any possible children of the parent as long as the parent is invisible.
    /// </summary>
    STATE_SYSTEM_INVISIBLE = 0x8000U,

    /// <summary>
    /// The object is clipped or has scrolled out of view, but it is not programmatically hidden. If the user makes the viewport larger, more of the object will be
    /// visible on the computer screen.
    /// </summary>
    STATE_SYSTEM_OFFSCREEN = 0x10000U,

    /// <summary>The object can be resized. For example, a user could change the size of a window by dragging it by the border.</summary>
    STATE_SYSTEM_SIZEABLE = 0x20000U,

    /// <summary>Indicates that the object can be moved. For example, a user can click the object's title bar and drag the object to a new location.</summary>
    STATE_SYSTEM_MOVEABLE = 0x40000U,

    /// <summary>
    /// The object or child uses text-to-speech (TTS) technology for description purposes. When an object with this state has the focus, a speech-based accessibility
    /// aid does not announce information because the object automatically announces it.
    /// </summary>
    STATE_SYSTEM_SELFVOICING = 0x80000U,

    /// <summary>The object is on the active window and is ready to receive keyboard focus.</summary>
    STATE_SYSTEM_FOCUSABLE = 0x100000U,

    /// <summary>The object accepts selection.</summary>
    STATE_SYSTEM_SELECTABLE = 0x200000U,

    /// <summary>Indicates that the object is formatted as a hyperlink. The object's role will usually be ROLE_SYSTEM_TEXT.</summary>
    STATE_SYSTEM_LINKED = 0x400000U,

    /// <summary>The object is a hyperlink that has been visited (previously clicked) by a user.</summary>
    STATE_SYSTEM_TRAVERSED = 0x800000U,

    /// <summary>Indicates that the object accepts multiple selected items; that is, SELFLAG_ADDSELECTION for the IAccessible::accSelect method is valid.</summary>
    STATE_SYSTEM_MULTISELECTABLE = 0x1000000U,

    /// <summary>Indicates that an object extends its selection by using SELFLAG_EXTENDSELECTION in the IAccessible::accSelect method.</summary>
    STATE_SYSTEM_EXTSELECTABLE = 0x2000000U,

    /// <summary>
    /// Indicates low-priority information that is not important to the user. This state is used, for example, when Word changes the appearance of the TipWizard button
    /// on its toolbar to indicate that it has a hint for the user.
    /// </summary>
    STATE_SYSTEM_ALERT_LOW = 0x4000000U,

    /// <summary>
    /// Indicates important information that is not conveyed immediately to the user. For example, when a battery is starting to reach a low level, a level indicator
    /// generates a medium-level alert. A blind access tool then generates a sound to let the user know that important information is available, without actually
    /// interrupting the user's work. The user could then query the alert information when convenient.
    /// </summary>
    STATE_SYSTEM_ALERT_MEDIUM = 0x8000000U,

    /// <summary>
    /// Indicates important information to be immediately conveyed to the user. For example, when a battery reaches a critically low level, a level indicator generates
    /// a high-level alert. As a result, a blind access tool immediately announces this information to the user, and a screen magnification program scrolls the screen
    /// so that the battery indicator is in view. This state is also appropriate for any prompt or operation that must be completed before the user can continue.
    /// </summary>
    STATE_SYSTEM_ALERT_HIGH = 0x10000000U,

    /// <summary>The object is a password-protected edit control.</summary>
    STATE_SYSTEM_PROTECTED = 0x20000000U,

    /// <summary>When invoked, the object displays a pop-up menu or a window.</summary>
    STATE_SYSTEM_HASPOPUP = 0x40000000U,

    /// <summary>A bitmask representing all valid state flags.</summary>
    STATE_SYSTEM_VALID =
        STATE_SYSTEM_UNAVAILABLE
        | STATE_SYSTEM_SELECTED
        | STATE_SYSTEM_FOCUSED
        | STATE_SYSTEM_PRESSED
        | STATE_SYSTEM_CHECKED
        | STATE_SYSTEM_MIXED
        | STATE_SYSTEM_READONLY
        | STATE_SYSTEM_HOTTRACKED
        | STATE_SYSTEM_DEFAULT
        | STATE_SYSTEM_EXPANDED
        | STATE_SYSTEM_COLLAPSED
        | STATE_SYSTEM_BUSY
        | STATE_SYSTEM_FLOATING
        | STATE_SYSTEM_MARQUEED
        | STATE_SYSTEM_ANIMATED
        | STATE_SYSTEM_INVISIBLE
        | STATE_SYSTEM_OFFSCREEN
        | STATE_SYSTEM_SIZEABLE
        | STATE_SYSTEM_MOVEABLE
        | STATE_SYSTEM_SELFVOICING
        | STATE_SYSTEM_FOCUSABLE
        | STATE_SYSTEM_SELECTABLE
        | STATE_SYSTEM_LINKED
        | STATE_SYSTEM_TRAVERSED
        | STATE_SYSTEM_MULTISELECTABLE
        | STATE_SYSTEM_EXTSELECTABLE
        | STATE_SYSTEM_ALERT_LOW
        | STATE_SYSTEM_ALERT_MEDIUM
        | STATE_SYSTEM_ALERT_HIGH
        | STATE_SYSTEM_PROTECTED,
}
