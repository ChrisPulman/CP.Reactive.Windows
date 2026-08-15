// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645597.aspx">GetRawInputDeviceInfo function</a></summary>
public enum RawInputDeviceInfoCommands : uint
{
    /// <summary>No raw input device information command is selected.</summary>
    None = 0U,
    /// <summary>
    /// The data pointer points to a string that contains the device name.
    /// For this uiCommand only, the value in pcbSize is the character count (not the byte count).
    /// </summary>
    DeviceName = 536_870_919U,
    /// <summary>The data pointer points to an RID_DEVICE_INFO structure.</summary>
    DeviceInfo = 536_870_923U,
    /// <summary>The data pointer points to the previously parsed data.</summary>
    PreparsedData = 536_870_917U
}
