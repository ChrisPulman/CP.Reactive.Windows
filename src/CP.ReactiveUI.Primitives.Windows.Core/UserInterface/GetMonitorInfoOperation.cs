// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface;

/// <summary>Represents a monitor-information operation.</summary>
/// <param name="monitorHandle">The monitor handle.</param>
/// <param name="monitorInfo">The receiving monitor information.</param>
/// <returns>True when monitor information was retrieved.</returns>
internal delegate bool GetMonitorInfoOperation(IntPtr monitorHandle, ref MonitorInfoEx monitorInfo);
