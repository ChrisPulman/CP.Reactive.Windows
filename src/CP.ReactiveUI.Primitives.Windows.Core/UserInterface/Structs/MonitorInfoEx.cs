// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;

/// <summary>
/// The MONITORINFOEX structure contains information about a display monitor. The GetMonitorInfo function stores information into a MONITORINFOEX structure or a
/// MONITORINFO structure. The MONITORINFOEX structure is a superset of the MONITORINFO structure. The MONITORINFOEX structure adds a string member to contain a
/// name for the display monitor.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public readonly struct MonitorInfoEx : IEquatable<MonitorInfoEx>
{
    /// <summary>The byte size of the native <c>MONITORINFOEXW</c> structure.</summary>
    private const int NativeSize = 104;

    /// <summary>The maximum number of UTF-16 characters in the native device name buffer.</summary>
    private const int DeviceNameCharacterCount = 32;

    /// <summary>The native structure size.</summary>
    private readonly int _size;

    /// <summary>The monitor rectangle.</summary>
    private readonly NativeRect _monitor;

    /// <summary>The monitor work-area rectangle.</summary>
    private readonly NativeRect _workArea;

    /// <summary>The monitor attributes.</summary>
    private readonly MonitorInfoFlags _flags;

    /// <summary>The fixed UTF-16 display device name buffer.</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DeviceNameCharacterCount)]
    private readonly string _deviceName;

    /// <summary>Initializes a new instance of the <see cref="MonitorInfoEx" /> struct.</summary>
    /// <param name="size">The size, in bytes, of the native structure.</param>
    /// <param name="monitor">The monitor rectangle in virtual-screen coordinates.</param>
    /// <param name="workArea">The monitor work-area rectangle in virtual-screen coordinates.</param>
    /// <param name="flags">The monitor attributes.</param>
    /// <param name="deviceName">The device name.</param>
    public MonitorInfoEx(
        int size,
        NativeRect monitor,
        NativeRect workArea,
        MonitorInfoFlags flags,
        string deviceName)
    {
        _size = size;
        _monitor = monitor;
        _workArea = workArea;
        _flags = flags;
        _deviceName = deviceName ?? string.Empty;
    }

    /// <summary>Gets the size, in bytes, of the native structure.</summary>
    public int Size => _size;

    /// <summary>Gets the monitor rectangle in virtual-screen coordinates.</summary>
    public NativeRect Monitor => _monitor;

    /// <summary>Gets the monitor work-area rectangle in virtual-screen coordinates.</summary>
    public NativeRect WorkArea => _workArea;

    /// <summary>Gets the monitor attributes.</summary>
    public MonitorInfoFlags Flags => _flags;

    /// <summary>Gets the device name.</summary>
    public string DeviceName => _deviceName ?? string.Empty;

    /// <summary>Creates an empty monitor-information value.</summary>
    /// <returns>The initialized monitor information.</returns>
    public static MonitorInfoEx Create() =>
        new(NativeSize, default, default, MonitorInfoFlags.None, string.Empty);

    /// <summary>Determines whether two monitor information values are equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when both values are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(MonitorInfoEx left, MonitorInfoEx right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two monitor information values are not equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when both values are not equal; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(MonitorInfoEx left, MonitorInfoEx right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public bool Equals(MonitorInfoEx other) =>
        _size == other._size
        && _monitor.Equals(other._monitor)
        && _workArea.Equals(other._workArea)
        && _flags == other._flags
        && string.Equals(DeviceName, other.DeviceName, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is MonitorInfoEx other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => typeof(MonitorInfoEx).GetHashCode();
}
