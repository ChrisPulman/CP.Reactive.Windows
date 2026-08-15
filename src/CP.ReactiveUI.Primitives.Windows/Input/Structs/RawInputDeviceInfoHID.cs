// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     This struct defines the raw input data coming from the specified keyboard.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645587.aspx">RID_DEVICE_INFO_KEYBOARD structure</a>
/// Remarks:
/// For the keyboard, the Usage Page is 1 and the Usage is 6.
/// </summary>
public readonly record struct RawInputDeviceInfoHID
{
    /// <summary>Gets the vendor identifier for the HID.</summary>
    public int VendorId { get; }

    /// <summary>Gets the product identifier for the HID.</summary>
    public int ProductId { get; }

    /// <summary>Gets the version number for the HID.</summary>
    public int VersionNumber { get; }

    /// <summary>Gets the top-level collection Usage Page for the device.</summary>
    public ushort UsagePage { get; }

    /// <summary>Gets the top-level collection Usage for the device.</summary>
    public ushort Usage { get; }
}
