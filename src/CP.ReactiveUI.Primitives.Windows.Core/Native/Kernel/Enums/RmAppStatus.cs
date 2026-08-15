// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;

/// <summary>
///     Describes the current status of an application that is acted upon by the Restart Manager.
///     See <a href="https://docs.microsoft.com/en-us/windows/win32/api/restartmanager/ne-restartmanager-rm_app_status">RM_APP_STATUS enumeration</a>
/// </summary>
[Flags]
public enum RmAppStatus : uint
{
    /// <summary>The application is in a state that is not described by any other enumerated state.</summary>
    None = 0U,

    /// <summary>The application is currently running.</summary>
    RmStatusRunning = 1U,

    /// <summary>The Restart Manager has stopped the application.</summary>
    RmStatusStopped = 2U,

    /// <summary>An action outside the Restart Manager has stopped the application.</summary>
    RmStatusStoppedOther = 4U,

    /// <summary>The Restart Manager has restarted the application.</summary>
    RmStatusRestarted = 8U,

    /// <summary>The Restart Manager encountered an error when stopping the application.</summary>
    RmStatusErrorOnStop = 0x10U,

    /// <summary>The Restart Manager encountered an error when restarting the application.</summary>
    RmStatusErrorOnRestart = 0x20U,

    /// <summary>Shutdown is masked by a filter.</summary>
    RmStatusShutdownMasked = 0x40U,

    /// <summary>Restart is masked by a filter.</summary>
    RmStatusRestartMasked = 0x80U,
}
