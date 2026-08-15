// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Devices;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Devices;
#endif
/// <summary>Information on device changes.</summary>
public class DeviceInterfaceChangeInfo
{
    /// <summary>Gets the type of the event.</summary>
    public DeviceChangeEvent EventType { get; internal set; }

    /// <summary>Gets the already prepared DevBroadcastDeviceInterface.</summary>
    /// <remarks>Device.DeviceClassGuid contains the Device Interface Class GUID from the notification.
    /// For the Device Setup Class GUID shown in Device Manager, use Device.DeviceSetupClassGuid.</remarks>
    public DevBroadcastDeviceInterface Device { get; internal set; }
}
