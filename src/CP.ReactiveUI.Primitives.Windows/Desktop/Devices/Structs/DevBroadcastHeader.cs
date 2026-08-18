// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Devices.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Devices.Structs;
#endif
/// <summary>
/// Serves as a standard header for information related to a device event reported through the WM_DEVICECHANGE message.
/// The members of the DEV_BROADCAST_HDR structure are contained in each device management structure. To determine
/// which structure you have received through WM_DEVICECHANGE, treat the structure as a DEV_BROADCAST_HDR structure
/// and check its dbch_devicetype member.
/// See <a href="https://docs.microsoft.com/en-us/windows/win32/api/dbt/ns-dbt-_dev_broadcast_hdr">DEV_BROADCAST_HDR structure</a>.
/// </summary>
public readonly struct DevBroadcastHeader : IEquatable<DevBroadcastHeader>
{
    /// <summary>The structure size.</summary>
    private readonly int _size;

    /// <summary>The device type.</summary>
    private readonly DeviceBroadcastDeviceType _deviceType;

    /// <summary>The reserved value.</summary>
    private readonly int _reserved;

    /// <summary>Initializes a new instance of the <see cref="DevBroadcastHeader"/> struct.</summary>
	/// <param name="size">The native structure size.</param>
	/// <param name="deviceType">The device type.</param>
	/// <param name="reserved">The reserved value.</param>
    internal DevBroadcastHeader(int size, DeviceBroadcastDeviceType deviceType, int reserved)
    {
        _size = size;
        _deviceType = deviceType;
        _reserved = reserved;
    }

    /// <summary>Gets the device type, which determines the event-specific information that follows the first three members.</summary>
    public DeviceBroadcastDeviceType DeviceType
    {
        get
        {
            MarkFieldsAsRead();
            return _deviceType;
        }
    }

    /// <summary>Compares two values for equality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><c>true</c> if the values are equal; otherwise <c>false</c>.</returns>
    public static bool operator ==(DevBroadcastHeader left, DevBroadcastHeader right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two values for inequality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><c>true</c> if the values are not equal; otherwise <c>false</c>.</returns>
    public static bool operator !=(DevBroadcastHeader left, DevBroadcastHeader right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is DevBroadcastHeader other && Equals(other);

    /// <inheritdoc />
    public bool Equals(DevBroadcastHeader other) =>
        _size == other._size && _deviceType == other._deviceType && _reserved == other._reserved;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_size, _deviceType, _reserved);

    /// <summary>Reads marshal-only fields so analyzers do not treat them as unused.</summary>
    private void MarkFieldsAsRead()
    {
        _ = _size;
        _ = _reserved;
    }
}
