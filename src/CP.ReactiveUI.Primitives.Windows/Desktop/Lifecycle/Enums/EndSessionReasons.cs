// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Lifecycle.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Lifecycle.Enums;
#endif
/// <summary>Flags for the WM_ENDSESSION message indicating the type of session end event.</summary>
[Flags]
public enum EndSessionReasons : uint
{
    /// <summary>The system is shutting down or restarting (reason was not specified).</summary>
    None = 0U,
    /// <summary>
    ///     The application is using a file that must be replaced, the system is being serviced, or system resources are exhausted.
    ///     This flag is set when the ENDSESSION_CLOSEAPP flag is set.
    /// </summary>
    ENDSESSION_CLOSEAPP = 1U,
    /// <summary>A critical system event requires the application to close.</summary>
    ENDSESSION_CRITICAL = 0x40000000U,
    /// <summary>The user is logging off.</summary>
    ENDSESSION_LOGOFF = 0x80000000U,
}
