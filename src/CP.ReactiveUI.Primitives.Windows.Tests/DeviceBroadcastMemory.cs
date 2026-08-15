// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Owns unmanaged memory containing a device broadcast payload.</summary>
internal sealed class DeviceBroadcastMemory : IDisposable
{
    /// <summary>The byte offset of the device type in DEV_BROADCAST_HDR-compatible structures.</summary>
    private const int DeviceTypeOffset = 4;

    /// <summary>The byte offset of the reserved value in DEV_BROADCAST_VOLUME.</summary>
    private const int VolumeReservedOffset = 8;

    /// <summary>The byte offset of the unit mask in DEV_BROADCAST_VOLUME.</summary>
    private const int VolumeUnitMaskOffset = 12;

    /// <summary>The byte offset of the flags in DEV_BROADCAST_VOLUME.</summary>
    private const int VolumeFlagsOffset = 16;

    /// <summary>Initializes a new instance of the <see cref="DeviceBroadcastMemory"/> class.</summary>
    /// <param name="pointer">The unmanaged memory pointer.</param>
    private DeviceBroadcastMemory(IntPtr pointer) => Pointer = pointer;

    /// <summary>Gets the unmanaged memory pointer.</summary>
    internal IntPtr Pointer { get; }

    /// <inheritdoc/>
    public void Dispose() => Marshal.FreeHGlobal(Pointer);

    /// <summary>Creates unmanaged memory for a volume broadcast structure.</summary>
    /// <param name="unitMask">The unit mask.</param>
    /// <param name="flags">The volume flags.</param>
    /// <returns>An owned memory block.</returns>
    internal static DeviceBroadcastMemory CreateVolume(uint unitMask, ushort flags)
    {
        var size = Marshal.SizeOf<DevBroadcastVolume>();
        var pointer = Marshal.AllocHGlobal(size);
        for (var offset = 0; offset < size; offset++)
        {
            Marshal.WriteByte(pointer, offset, 0);
        }

        Marshal.WriteInt32(pointer, 0, size);
        Marshal.WriteInt32(pointer, DeviceTypeOffset, (int)DeviceBroadcastDeviceType.Volume);
        Marshal.WriteInt32(pointer, VolumeReservedOffset, 0);
        Marshal.WriteInt32(pointer, VolumeUnitMaskOffset, (int)unitMask);
        Marshal.WriteInt16(pointer, VolumeFlagsOffset, (short)flags);
        return new(pointer);
    }

    /// <summary>Creates unmanaged memory for a device-interface broadcast structure.</summary>
    /// <param name="deviceInterface">The device interface payload.</param>
    /// <returns>An owned memory block.</returns>
    internal static DeviceBroadcastMemory CreateInterface(DevBroadcastDeviceInterface deviceInterface)
    {
        var size = Marshal.SizeOf<DevBroadcastDeviceInterface>();
        var pointer = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(deviceInterface, pointer, false);
        return new(pointer);
    }

    /// <summary>Creates a device notification event from this payload.</summary>
    /// <param name="eventType">The device change event type.</param>
    /// <returns>The decoded notification event.</returns>
    internal DeviceNotificationEvent CreateEvent(DeviceChangeEvent eventType) => new((IntPtr)(int)eventType, Pointer);
}
