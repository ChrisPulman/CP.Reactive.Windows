// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Power.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Power.Enums;
#endif
/// <summary>
/// Flags for the ExitWindowsEx function, which logs off the interactive user, shuts down the system,
/// or shuts down and restarts the system.
/// See <a href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-exitwindowsex">ExitWindowsEx function</a>
/// </summary>
[Flags]
public enum ExitWindowsFlags : uint
{
    /// <summary>
    /// Shuts down all processes running in the logon session of the process that called the ExitWindowsEx function.
    /// Then it logs the user off. This flag can be used only by processes running in an interactive user's logon session.
    /// </summary>
    None = 0U,
    /// <summary>
    /// Shuts down the system to a point at which it is safe to turn off the power. All file buffers have been flushed to disk, and all running processes have stopped.
    /// The calling process must have the SE_SHUTDOWN_NAME privilege.
    /// Specifying this flag will not turn off the power even if the system supports the power-off feature.
    /// </summary>
    EWX_SHUTDOWN = 1U,
    /// <summary>Shuts down the system and then restarts it. The calling process must have the SE_SHUTDOWN_NAME privilege.</summary>
    EWX_REBOOT = 2U,
    /// <summary>
    /// This flag has no effect if terminal services is enabled. Otherwise, the system does not send the WM_QUERYENDSESSION message.
    /// This can cause applications to lose data. Therefore, you should only use this flag in an emergency.
    /// </summary>
    EWX_FORCE = 4U,
    /// <summary>
    /// Shuts down the system and turns off the power. The system must support the power-off feature.
    /// The calling process must have the SE_SHUTDOWN_NAME privilege.
    /// </summary>
    EWX_POWEROFF = 8U,
    /// <summary>Forces processes to terminate if they do not respond to the WM_QUERYENDSESSION or WM_ENDSESSION message within the timeout interval.</summary>
    EWX_FORCEIFHUNG = 0x10U,
    /// <summary>The system is restarted using the ExitProcess function.</summary>
    EWX_RESTARTAPPS = 0x40U,
    /// <summary>Hybrid shutdown; on Windows 8, shuts down and begins cool boot (new fast startup feature).</summary>
    EWX_HYBRID_SHUTDOWN = 0x400000U,
    /// <summary>Restarts only the boot application, such as the Windows Boot Manager.</summary>
    EWX_BOOTOPTIONS = 0x1000000U,
}
