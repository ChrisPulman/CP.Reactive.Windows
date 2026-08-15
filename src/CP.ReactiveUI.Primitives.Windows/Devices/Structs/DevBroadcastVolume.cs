// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using System.Text;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Devices.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Devices.Structs;
#endif
/// <summary>
/// Contains information about a logical volume.
/// See <a href="https://docs.microsoft.com/en-us/windows/win32/api/dbt/ns-dbt-dev_broadcast_volume">DEV_BROADCAST_VOLUME structure</a>.
/// Although the dbcv_unitmask member may specify more than one volume in any message, this does not guarantee that only one message is generated for a specified event.
/// Multiple system features may independently generate messages for logical volumes at the same time.
/// Messages for media arrival and removal are sent only for media in devices that support a soft-eject mechanism.
/// For example, applications will not see media-related volume messages for floppy disks.
/// Messages for network drive arrival and removal are not sent whenever network commands are issued, but rather when network connections will disappear as the result of a hardware event.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public readonly struct DevBroadcastVolume : IEquatable<DevBroadcastVolume>
{
    /// <summary>The media-change flag.</summary>
    private const ushort MediaChangeFlag = 1;

    /// <summary>The network-volume flag.</summary>
    private const ushort NetworkVolumeFlag = 2;

    /// <summary>The number of drive letters represented by the mask.</summary>
    private const int DriveLetterCount = 26;

    /// <summary>The first drive letter.</summary>
    private const char FirstDriveLetter = 'A';

    /// <summary>The structure size.</summary>
    private readonly int _size;

    /// <summary>The device type.</summary>
    private readonly DeviceBroadcastDeviceType _deviceType;

    /// <summary>The reserved value.</summary>
    private readonly uint _reserved;

    /// <summary>The logical unit mask identifying one or more logical units.</summary>
    private readonly uint _unitMask;

    /// <summary>The volume flags.</summary>
    private readonly ushort _flags;

    /// <summary>Gets a string with the drive letters that are influenced in the message.</summary>
    public string Drives
    {
        get
        {
            StringBuilder drives = new();
            for (int letter = 0; letter < DriveLetterCount; letter = checked(letter + 1))
            {
                uint bit = (uint)(1 << letter);
                if ((_unitMask & bit) != 0)
                {
                    _ = drives.Append((char)checked((ushort)(FirstDriveLetter + letter)));
                }
            }

            return drives.ToString();
        }
    }

    /// <summary>Gets a value indicating whether the change affects media in drive.</summary>
    public bool IsMediaChange => (_flags & MediaChangeFlag) != 0;

    /// <summary>Gets a value indicating whether the indicated logical volume is a network volume.</summary>
    public bool IsNetworkVolume => (_flags & NetworkVolumeFlag) != 0;

    /// <summary>Compares two values for equality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><c>true</c> if the values are equal; otherwise <c>false</c>.</returns>
    public static bool operator ==(DevBroadcastVolume left, DevBroadcastVolume right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two values for inequality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><c>true</c> if the values are not equal; otherwise <c>false</c>.</returns>
    public static bool operator !=(DevBroadcastVolume left, DevBroadcastVolume right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is DevBroadcastVolume other && Equals(other);

    /// <inheritdoc />
    public bool Equals(DevBroadcastVolume other) =>
        _size == other._size
        && _deviceType == other._deviceType
        && _reserved == other._reserved
        && _unitMask == other._unitMask
        && _flags == other._flags;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_size, _deviceType, _reserved, _unitMask, _flags);
}
