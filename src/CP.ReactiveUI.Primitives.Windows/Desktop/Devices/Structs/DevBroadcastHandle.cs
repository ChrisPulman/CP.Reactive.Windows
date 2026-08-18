// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Devices.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Devices.Structs;
#endif
/// <summary>
/// Contains information about a file system handle.
/// See <a href="https://docs.microsoft.com/en-us/windows/win32/api/dbt/ns-dbt-_dev_broadcast_handle">DEV_BROADCAST_HANDLE structure</a>.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public readonly struct DevBroadcastHandle : IEquatable<DevBroadcastHandle>
{
    /// <summary>The structure size.</summary>
    private readonly int _size;

    /// <summary>The device type.</summary>
    private readonly DeviceBroadcastDeviceType _deviceType;

    /// <summary>The reserved value.</summary>
    private readonly int _reserved;

    /// <summary>The file-system handle.</summary>
    private readonly IntPtr _handle;

    /// <summary>The device notification handle returned by RegisterDeviceNotification.</summary>
    private readonly IntPtr _hdevnotify;

    /// <summary>The GUID for a custom event. Valid only for DBT_CUSTOMEVENT.</summary>
    private readonly Guid _eventguid;

    /// <summary>The name offset.</summary>
    private readonly ulong _nameoffset;

    /// <summary>The variable-length data buffer.</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.I1)]
    private readonly byte[] _data;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Devices.Structs.DevBroadcastHandle" /> struct.</summary>
    /// <param name="deviceType">The device type.</param>
    /// <param name="size">The structure size.</param>
    private DevBroadcastHandle(DeviceBroadcastDeviceType deviceType, int size)
    {
        _size = size;
        _deviceType = deviceType;
        _reserved = 0;
        _handle = IntPtr.Zero;
        _hdevnotify = IntPtr.Zero;
        _eventguid = Guid.Empty;
        _nameoffset = 0UL;
        _data = new byte[1];
    }

    /// <summary>Compares two values for equality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><c>true</c> if the values are equal; otherwise <c>false</c>.</returns>
    public static bool operator ==(DevBroadcastHandle left, DevBroadcastHandle right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two values for inequality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><c>true</c> if the values are not equal; otherwise <c>false</c>.</returns>
    public static bool operator !=(DevBroadcastHandle left, DevBroadcastHandle right)
    {
        return !left.Equals(right);
    }

    /// <summary>Factory for an empty DevBroadcastHandle.</summary>
    /// <returns>A DevBroadcastHandle value.</returns>
    public static DevBroadcastHandle Create() => new(DeviceBroadcastDeviceType.Handle, Marshal.SizeOf<DevBroadcastHandle>());

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is DevBroadcastHandle other && Equals(other);

    /// <inheritdoc />
    public bool Equals(DevBroadcastHandle other) =>
        _size == other._size
        && _deviceType == other._deviceType
        && _reserved == other._reserved
        && _handle == other._handle
        && _hdevnotify == other._hdevnotify
        && _eventguid == other._eventguid
        && _nameoffset == other._nameoffset
        && GetDataByte() == other.GetDataByte();

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(_size, _deviceType, _reserved, _handle, _hdevnotify, _eventguid, _nameoffset, GetDataByte());

    /// <summary>Gets the single marshalled data byte.</summary>
    /// <returns>The data byte.</returns>
    private byte GetDataByte()
    {
        var data = _data;
        return data is null || data.Length <= 0 ? (byte)0 : data[0];
    }
}
