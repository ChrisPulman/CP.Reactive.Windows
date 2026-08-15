// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>A set of flags that represent attributes of the display monitor.</summary>
[Flags]
public enum MonitorInfoFlags
{
    /// <summary>No monitor information flags.</summary>
    None = 0,
    /// <summary>This is the primary display monitor.</summary>
    Primary = 1
}
