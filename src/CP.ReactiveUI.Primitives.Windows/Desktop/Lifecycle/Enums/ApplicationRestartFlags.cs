// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Lifecycle.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Lifecycle.Enums;
#endif
/// <summary>Flags for the RegisterApplicationRestart function.</summary>
[Flags]
public enum ApplicationRestartFlags : uint
{
    /// <summary>No flags set. The application will be restarted with default behavior.</summary>
    None = 0U,
    /// <summary>Do not restart the process if it terminates due to an unhandled exception.</summary>
    RestartNoCrash = 1U,
    /// <summary>Do not restart the process if it terminates due to the application not responding.</summary>
    RestartNoHang = 2U,
    /// <summary>Do not restart the process if it terminates due to the installation of an update.</summary>
    RestartNoPatch = 4U,
    /// <summary>Do not restart the process if the computer is restarted as the result of an update.</summary>
    RestartNoReboot = 8U,
}
