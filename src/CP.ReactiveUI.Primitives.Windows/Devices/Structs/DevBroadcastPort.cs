// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Devices.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Devices.Structs;
#endif
/// <summary>
/// Contains information about a modem, serial, or parallel port.
/// See <a href="https://docs.microsoft.com/en-us/windows/win32/api/dbt/ns-dbt-dev_broadcast_port_w">DEV_BROADCAST_PORT_W structure</a>.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public readonly struct DevBroadcastPort : IEquatable<DevBroadcastPort>
{
    /// <summary>The structure size.</summary>
    private readonly int _size;

    /// <summary>The device type.</summary>
    private readonly DeviceBroadcastDeviceType _deviceType;

    /// <summary>The port name.</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
    private readonly string _name;

    /// <summary>The reserved value.</summary>
    private readonly int _reserved;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Devices.Structs.DevBroadcastPort" /> struct.</summary>
    /// <param name="deviceType">The device type.</param>
    /// <param name="size">The structure size.</param>
    private DevBroadcastPort(DeviceBroadcastDeviceType deviceType, int size)
    {
        _size = size;
        _deviceType = deviceType;
        _name = null;
        _reserved = 0;
    }

    /// <summary>Gets the name of the device.</summary>
    public string Name => _name;

    /// <summary>Compares two values for equality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><c>true</c> if the values are equal; otherwise <c>false</c>.</returns>
    public static bool operator ==(DevBroadcastPort left, DevBroadcastPort right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two values for inequality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><c>true</c> if the values are not equal; otherwise <c>false</c>.</returns>
    public static bool operator !=(DevBroadcastPort left, DevBroadcastPort right)
    {
        return !left.Equals(right);
    }

    /// <summary>Factory for an empty DevBroadcastPort.</summary>
    /// <returns>A DevBroadcastPort value.</returns>
    public static DevBroadcastPort Create() => new(DeviceBroadcastDeviceType.Port, Marshal.SizeOf<DevBroadcastPort>());

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is DevBroadcastPort other && Equals(other);

    /// <inheritdoc />
    public bool Equals(DevBroadcastPort other) =>
        _size == other._size
        && _deviceType == other._deviceType
        && _reserved == other._reserved
        && _name == other._name;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_size, _deviceType, _reserved, _name);
}
