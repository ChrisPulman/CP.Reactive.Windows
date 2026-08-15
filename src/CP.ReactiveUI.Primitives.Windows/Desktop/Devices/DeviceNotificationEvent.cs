// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Devices;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Devices;
#endif
/// <summary>Information on device changes.</summary>
public class DeviceNotificationEvent
{
    /// <summary>The pointer to the device broadcast structure.</summary>
    private readonly IntPtr _deviceBroadcastPtr;

    /// <summary>The device broadcast header.</summary>
    private readonly DevBroadcastHeader _devBroadcastHeader;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Devices.DeviceNotificationEvent" /> class from a WM_DEVICECHANGE message.</summary>
    /// <param name="wordParam">The word parameter.</param>
    /// <param name="longParam">The long parameter.</param>
    public DeviceNotificationEvent(IntPtr wordParam, IntPtr longParam)
    {
        EventType = (DeviceChangeEvent)checked((uint)wordParam.ToInt32());
        _deviceBroadcastPtr = longParam;
        _devBroadcastHeader = Marshal.PtrToStructure<DevBroadcastHeader>(_deviceBroadcastPtr);
    }

    /// <summary>Gets the type of the event.</summary>
    public DeviceChangeEvent EventType { get; }

    /// <summary>Test if the message is a certain DeviceBroadcastDeviceType.</summary>
    /// <param name="deviceBroadcastDeviceType">The device broadcast device type.</param>
    /// <returns><c>true</c> if the message is for the requested type; otherwise <c>false</c>.</returns>
    public bool Is(DeviceBroadcastDeviceType deviceBroadcastDeviceType) => _devBroadcastHeader.DeviceType == deviceBroadcastDeviceType;

    /// <summary>Tries to get the DevBroadcastVolume.</summary>
    /// <param name="devBroadcastVolume">The DevBroadcastVolume value.</param>
    /// <returns><c>true</c> if the value could be converted; otherwise <c>false</c>.</returns>
    public bool TryGetDevBroadcastVolume(out DevBroadcastVolume devBroadcastVolume)
    {
        if (_devBroadcastHeader.DeviceType != DeviceBroadcastDeviceType.Volume)
        {
            devBroadcastVolume = default;
            return false;
        }

        devBroadcastVolume = Marshal.PtrToStructure<DevBroadcastVolume>(_deviceBroadcastPtr);
        return true;
    }

    /// <summary>Tries to get the DevBroadcastDeviceInterface.</summary>
    /// <param name="devBroadcastDeviceInterface">The DevBroadcastDeviceInterface value.</param>
    /// <returns><c>true</c> if the value could be converted; otherwise <c>false</c>.</returns>
    public bool TryGetDevBroadcastDeviceInterface(out DevBroadcastDeviceInterface devBroadcastDeviceInterface)
    {
        if (_devBroadcastHeader.DeviceType != DeviceBroadcastDeviceType.DeviceInterface)
        {
            devBroadcastDeviceInterface = default;
            return false;
        }

        devBroadcastDeviceInterface = Marshal.PtrToStructure<DevBroadcastDeviceInterface>(_deviceBroadcastPtr);
        return true;
    }
}
