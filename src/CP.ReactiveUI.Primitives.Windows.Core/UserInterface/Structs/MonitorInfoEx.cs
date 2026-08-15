// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;

/// <summary>
/// The MONITORINFOEX structure contains information about a display monitor. The GetMonitorInfo function stores information into a MONITORINFOEX structure or a
/// MONITORINFO structure. The MONITORINFOEX structure is a superset of the MONITORINFO structure. The MONITORINFOEX structure adds a string member to contain a
/// name for the display monitor.
/// </summary>
/// <param name="Size">The size, in bytes, of the native structure.</param>
/// <param name="Monitor">The monitor rectangle in virtual-screen coordinates.</param>
/// <param name="WorkArea">The monitor work-area rectangle in virtual-screen coordinates.</param>
/// <param name="Flags">The monitor attributes.</param>
/// <param name="DeviceName">The device name.</param>
public readonly record struct MonitorInfoEx(int Size, NativeRect Monitor, NativeRect WorkArea, MonitorInfoFlags Flags, string DeviceName)
{
    /// <summary>The byte size of the native <c>MONITORINFOEXW</c> structure.</summary>
    private const int NativeSize = 104;

    /// <summary>Creates an empty monitor-information value.</summary>
    /// <returns>The initialized monitor information.</returns>
    public static MonitorInfoEx Create() => new(104, default(NativeRect), default(NativeRect), MonitorInfoFlags.None, string.Empty);
}
