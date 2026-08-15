// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Shell.Enums;

/// <summary>A value that specifies an edge of the screen.</summary>
[Flags]
public enum AppBarStates
{
    /// <summary>ABS_MANUAL - No automatic function.</summary>
    None = 0,

    /// <summary>ABS_AUTOHIDE - Autohides the AppBar.</summary>
    AutoHide = 1,

    /// <summary>ABS_ALWAYSONTOP - Make sure the AppBar is always on top.</summary>
    AllwaysOnTop = 2,
}
