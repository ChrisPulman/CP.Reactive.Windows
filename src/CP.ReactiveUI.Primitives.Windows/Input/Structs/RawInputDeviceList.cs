// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
/// Contains information about a raw input device.
/// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645568.aspx">RAWINPUTDEVICELIST structure</a>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct RawInputDeviceList
{
    /// <summary>Stores a handle to the raw input device.</summary>
    private readonly IntPtr _handle;

    /// <summary>Stores the native raw-input device type.</summary>
    private readonly RawInputDeviceTypes _rawInputDeviceType;

    /// <summary>Initializes a new instance of the <see cref="RawInputDeviceList"/> struct.</summary>
    /// <param name="handle">The raw input device handle.</param>
    /// <param name="deviceType">The raw input device type.</param>
    internal RawInputDeviceList(IntPtr handle, RawInputDeviceTypes deviceType)
    {
        _handle = handle;
        _rawInputDeviceType = deviceType;
    }

    /// <summary>Gets the type of device.</summary>
    public RawInputDeviceTypes RawInputDeviceType => _rawInputDeviceType;

    /// <summary>Gets a handle to the raw input device.</summary>
    internal IntPtr Handle => _handle;

    /// <summary>Returns the raw input device handle.</summary>
    /// <returns>The raw input device handle.</returns>
    public IntPtr ToIntPtr() => _handle;
}
