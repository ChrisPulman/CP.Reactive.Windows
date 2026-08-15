// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;

/// <summary>
///     Configures the shut down of applications.
///     See <a href="https://docs.microsoft.com/en-us/windows/win32/api/restartmanager/ne-restartmanager-rm_shutdown_type">RM_SHUTDOWN_TYPE enumeration</a>
/// </summary>
[Flags]
public enum RmShutdownType : uint
{
    /// <summary>
    ///     Force unresponsive applications and services to shut down after the timeout period.
    ///     An application that does not respond to a shutdown request by the Restart Manager is forced to shut down after 30 seconds.
    ///     A service that does not respond to a shutdown request is forced to shut down after 20 seconds.
    /// </summary>
    None = 0U,
    /// <summary>
    ///     Force unresponsive applications and services to shut down after the timeout period.
    ///     An application that does not respond to a shutdown request by the Restart Manager is forced to shut down after 30 seconds.
    ///     A service that does not respond to a shutdown request is forced to shut down after 20 seconds.
    /// </summary>
    RmForceShutdown = 1U,
    /// <summary>Shut down applications if and only if all the applications have been registered for restart using the RegisterApplicationRestart function.</summary>
    RmShutdownOnlyRegistered = 0x10U
}
