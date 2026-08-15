// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
/// Contains information about a raw input device.
/// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645568.aspx">RAWINPUTDEVICELIST structure</a>
/// </summary>
public readonly record struct RawInputDeviceList
{
    /// <summary>Gets the type of device.</summary>
    public RawInputDeviceTypes RawInputDeviceType { get; }

    /// <summary>Gets a handle to the raw input device.</summary>
    internal IntPtr Handle => _handle;

    /// <summary>Stores a handle to the raw input device.</summary>
    private readonly IntPtr _handle;

    /// <summary>Initializes a new instance of the <see cref="RawInputDeviceList"/> struct.</summary>
	/// <param name="handle">The raw input device handle.</param>
	/// <param name="deviceType">The raw input device type.</param>
    internal RawInputDeviceList(IntPtr handle, RawInputDeviceTypes deviceType)
    {
        _handle = handle;
        RawInputDeviceType = deviceType;
    }

    /// <summary>Returns the raw input device handle.</summary>
    /// <returns>The raw input device handle.</returns>
    public IntPtr ToIntPtr() => _handle;

    /// <inheritdoc/>
    [CompilerGenerated]
    public override int GetHashCode() => (EqualityComparer<IntPtr>.Default.GetHashCode(_handle) * -1_521_134_295) + EqualityComparer<RawInputDeviceTypes>.Default.GetHashCode(RawInputDeviceType);

    /// <inheritdoc/>
    [CompilerGenerated]
    public bool Equals(RawInputDeviceList other)
    {
        if (EqualityComparer<IntPtr>.Default.Equals(_handle, other._handle))
        {
            return EqualityComparer<RawInputDeviceTypes>.Default.Equals(RawInputDeviceType, other.RawInputDeviceType);
        }

        return false;
    }
}
