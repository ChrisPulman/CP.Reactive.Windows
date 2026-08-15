// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Devices.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Devices.Enums;
#endif
/// <summary>See <a href="https://docs.microsoft.com/en-us/windows/win32/api/dbt/ns-dbt-_dev_broadcast_hdr">DEV_BROADCAST_HDR structure</a>.</summary>
public enum DeviceBroadcastDeviceType : uint
{
    /// <summary>DBT_DEVTYP_OEM: OEM- or IHV-defined device type. This structure is a DEV_BROADCAST_OEM structure.</summary>
    Oem = 0U,
    /// <summary>DBT_DEVTYP_VOLUME: Logical volume. This structure is a DEV_BROADCAST_VOLUME structure.</summary>
    Volume = 2U,
    /// <summary>DBT_DEVTYP_PORT: Port device (serial or parallel). This structure is a DEV_BROADCAST_PORT structure.</summary>
    Port = 3U,
    /// <summary>DBT_DEVTYP_DEVICEINTERFACE: Class of devices. This structure is a DEV_BROADCAST_DEVICEINTERFACE structure.</summary>
    DeviceInterface = 5U,
    /// <summary>DBT_DEVTYP_HANDLE: File system handle. This structure is a DEV_BROADCAST_HANDLE structure.</summary>
    Handle = 6U
}
