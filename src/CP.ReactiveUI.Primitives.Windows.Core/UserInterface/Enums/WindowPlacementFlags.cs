// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>Defines flags used by the WINDOWPLACEMENT structure.</summary>
[Flags]
public enum WindowPlacementFlags : uint
{
    /// <summary>When no flags are used.</summary>
    None = 0U,

    /// <summary>The coordinates of the minimized window may be specified. This flag must be specified if the coordinates are set in the ptMinPosition member.</summary>
    SetMinPosition = 1U,

    /// <summary>
    /// If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the
    /// window. This prevents the calling thread from blocking its execution while other threads process the request.
    /// </summary>
    AsyncWindowPlacement = 4U,

    /// <summary>
    /// The restored window will be maximized, regardless of whether it was maximized before it was minimized. This setting is only valid the next time the window is
    /// restored. It does not change the default restoration behavior. This flag is only valid when the SW_SHOWMINIMIZED value is specified for the showCmd member.
    /// </summary>
    RestoreToMaximized = 2U,
}
