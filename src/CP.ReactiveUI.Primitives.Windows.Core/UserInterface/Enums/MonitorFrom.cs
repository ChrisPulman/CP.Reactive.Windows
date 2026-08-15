// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>
///     Flags for the MonitorFromRect / MonitorFromWindow "flags" field
///     see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/dd145063(v=vs.85).aspx">MonitorFromRect function</a>
///     or see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/dd145064(v=vs.85).aspx">MonitorFromWindow function</a>
/// </summary>
[Flags]
public enum MonitorFrom : uint
{
    /// <summary>Returns a handle to the display monitor that is nearest to the rectangle.</summary>
    None = 0U,
    /// <summary>Returns NULL. (why??).</summary>
    DefaultToNull = 1U,
    /// <summary>Returns a handle to the primary display monitor.</summary>
    DefaultToPrimary = 2U
}
