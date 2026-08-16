// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Power.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Power.Enums;
#endif
/// <summary>
/// Flags for the SetThreadExecutionState function, which enables an application to inform
/// the system that it is in use, thereby preventing the system from entering sleep or turning off the display.
/// See <a href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-setthreadexecutionstate">SetThreadExecutionState function</a>
/// </summary>
[Flags]
public enum ThreadExecutionStateFlags : uint
{
    /// <summary>No thread execution state flags.</summary>
    None = 0U,
    /// <summary>
    /// Enables away mode. This value must be specified with <see cref="F:CP.ReactiveUI.Primitives.Windows.Desktop.Power.Enums.ThreadExecutionStateFlags.ES_CONTINUOUS" />.
    /// Away mode should be used only by media-recording and media-distribution applications that must perform
    /// critical background processing on desktop computers while the computer appears to be sleeping.
    /// </summary>
    ES_AWAYMODE_REQUIRED = 0x40U,
    /// <summary>
    /// Informs the system that the state being set should remain in effect until the next call
    /// that includes <see cref="F:CP.ReactiveUI.Primitives.Windows.Desktop.Power.Enums.ThreadExecutionStateFlags.ES_CONTINUOUS" /> and one of the other state flags is cleared.
    /// </summary>
    ES_CONTINUOUS = 0x80000000U,
    /// <summary>Forces the display to be on by resetting the display idle timer.</summary>
    ES_DISPLAY_REQUIRED = 2U,
    /// <summary>Forces the system to be in the working state by resetting the system idle timer.</summary>
    ES_SYSTEM_REQUIRED = 1U,
}
