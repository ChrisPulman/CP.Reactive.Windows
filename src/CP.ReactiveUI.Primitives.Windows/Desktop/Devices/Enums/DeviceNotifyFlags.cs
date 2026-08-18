// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Devices.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Devices.Enums;
#endif
/// <summary>See <a href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerdevicenotificationw">RegisterDeviceNotificationW function</a>.</summary>
[Flags]
public enum DeviceNotifyFlags : uint
{
    /// <summary>No additional flags are set; the recipient parameter is a window handle.</summary>
    None = 0U,
    /// <summary>The hRecipient parameter is a service status handle.</summary>
    ServiceHandle = 1U,
    /// <summary>
    /// Notifies the recipient of device interface events for all device interface classes. (The dbcc_classguid member is ignored.)
    /// This value can be used only if the dbch_devicetype member is DBT_DEVTYP_DEVICEINTERFACE.
    /// </summary>
    AllInterfaceClasses = 4U,
}
