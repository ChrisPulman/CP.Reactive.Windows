// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;

/// <summary>
///     Describes the reasons a restart of the system is needed.
///     See <a href="https://docs.microsoft.com/en-us/windows/win32/api/restartmanager/ne-restartmanager-rm_reboot_reason">RM_REBOOT_REASON enumeration</a>
/// </summary>
[Flags]
public enum RmRebootReason : uint
{
    /// <summary>A system restart is not required.</summary>
    None = 0U,
    /// <summary>The current user does not have sufficient privileges to shut down one or more processes.</summary>
    RmRebootReasonPermissionDenied = 1U,
    /// <summary>One or more processes are running in another Terminal Services session.</summary>
    RmRebootReasonSessionMismatch = 2U,
    /// <summary>A system restart is needed because one or more processes to be shut down are critical processes.</summary>
    RmRebootReasonCriticalProcess = 4U,
    /// <summary>A system restart is needed because one or more services to be shut down are critical services.</summary>
    RmRebootReasonCriticalService = 8U,
    /// <summary>A system restart is needed because the current process must be shut down.</summary>
    RmRebootReasonDetectedSelf = 0x10U
}
