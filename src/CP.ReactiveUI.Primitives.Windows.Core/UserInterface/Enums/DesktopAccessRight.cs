// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>Used to open a desktop.</summary>
[Flags]
public enum DesktopAccessRight : uint
{
    /// <summary>No desktop access rights.</summary>
    None = 0U,
    /// <summary>The DESKTOP_READOBJECTS value.</summary>
    DESKTOP_READOBJECTS = 1U,
    /// <summary>The DESKTOP_CREATEWINDOW value.</summary>
    DESKTOP_CREATEWINDOW = 2U,
    /// <summary>The DESKTOP_CREATEMENU value.</summary>
    DESKTOP_CREATEMENU = 4U,
    /// <summary>The DESKTOP_HOOKCONTROL value.</summary>
    DESKTOP_HOOKCONTROL = 8U,
    /// <summary>The DESKTOP_JOURNALRECORD value.</summary>
    DESKTOP_JOURNALRECORD = 0x10U,
    /// <summary>The DESKTOP_JOURNALPLAYBACK value.</summary>
    DESKTOP_JOURNALPLAYBACK = 0x20U,
    /// <summary>The DESKTOP_ENUMERATE value.</summary>
    DESKTOP_ENUMERATE = 0x40U,
    /// <summary>The DESKTOP_WRITEOBJECTS value.</summary>
    DESKTOP_WRITEOBJECTS = 0x80U,
    /// <summary>The DESKTOP_SWITCHDESKTOP value.</summary>
    DESKTOP_SWITCHDESKTOP = 0x100U,
    /// <summary>The GENERIC_ALL value.</summary>
    GENERIC_ALL = DESKTOP_READOBJECTS | DESKTOP_CREATEWINDOW | DESKTOP_CREATEMENU | DESKTOP_HOOKCONTROL | DESKTOP_JOURNALRECORD | DESKTOP_JOURNALPLAYBACK | DESKTOP_ENUMERATE | DESKTOP_WRITEOBJECTS | DESKTOP_SWITCHDESKTOP
}
