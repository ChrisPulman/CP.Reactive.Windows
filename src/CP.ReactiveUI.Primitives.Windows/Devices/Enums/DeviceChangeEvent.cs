// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Devices.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Devices.Enums;
#endif
/// <summary>These are the possible device change values, as described in the
/// <a href="https://docs.microsoft.com/en-us/windows/win32/devio/device-management-events">Device Management Events</a>.</summary>
public enum DeviceChangeEvent : uint
{
    /// <summary>No device change event.</summary>
    None = 0U,
    /// <summary>
    /// The system broadcasts the DBT_DEVNODES_CHANGED device event when a device has been added to or removed from the system.
    /// Applications that maintain lists of devices in the system should refresh their lists.
    /// </summary>
    DevNodesChanged = 7U,
    /// <summary>
    /// The system broadcasts the DBT_QUERYCHANGECONFIG device event to request permission to change the current configuration (dock or undock).
    /// Any application can deny this request and cancel the change.
    /// </summary>
    QueryChangeConfig = 23U,
    /// <summary>
    /// The system broadcasts the DBT_CONFIGCHANGED device event to indicate that the current configuration has changed, due to a dock or undock.
    /// An application or driver that stores data in the registry under the HKEY_CURRENT_CONFIG key should update the data.
    /// </summary>
    ConfigChanged = 24U,
    /// <summary>The system broadcasts the DBT_CONFIGCHANGECANCELED device event when a request to change the current configuration (dock or undock) has been canceled.</summary>
    ConfigChangeCanceled = 25U,
    /// <summary>The system broadcasts the DBT_DEVICEARRIVAL device event when a device or piece of media has been inserted and becomes available.</summary>
    DeviceArrival = 32_768U,
    /// <summary>
    /// The system broadcasts the DBT_DEVICEQUERYREMOVE device event to request permission to remove a device or piece of media.
    /// This message is the last chance for applications and drivers to prepare for this removal.
    /// However, any application can deny this request and cancel the operation.
    /// </summary>
    DeviceQueryRemove = 32_769U,
    /// <summary>The system broadcasts the DBT_DEVICEQUERYREMOVEFAILED device event when a request to remove a device or piece of media has been canceled.</summary>
    DeviceQueryRemoveFailed = 32_770U,
    /// <summary>The system broadcasts the DBT_DEVICEREMOVEPENDING device event when a device or piece of media is being removed and is no longer available for use.</summary>
    DeviceRemovePending = 32_771U,
    /// <summary>The system broadcasts the DBT_DEVICEREMOVECOMPLETE device event when a device or piece of media has been physically removed.</summary>
    DeviceRemoveComplete = 32_772U,
    /// <summary>The system broadcasts the DBT_DEVICETYPESPECIFIC device event when a device-specific event occurs.</summary>
    DeviceTypeSpecific = 32_773U,
    /// <summary>The system sends the DBT_CUSTOMEVENT device event when a driver-defined custom event has occurred.</summary>
    CustomEvent = 32_774U,
    /// <summary>The DBT_USERDEFINED device event identifies a user-defined event.</summary>
    UserDefined = 65_535U
}
