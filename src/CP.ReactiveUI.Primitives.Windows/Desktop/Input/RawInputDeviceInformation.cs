// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Describes a RawInput device.</summary>
public class RawInputDeviceInformation
{
    /// <summary>Gets the cryptic device name.</summary>
    public string DeviceName { get; internal set; }

    /// <summary>Gets a name which can be used to display to a user.</summary>
    public string DisplayName { get; internal set; }

    /// <summary>Gets the actual device information.</summary>
    public RawInputDeviceInfo DeviceInfo { get; internal set; }

    /// <summary>Gets or sets the handle to the raw input device for same-assembly raw input tracking.</summary>
    internal IntPtr Handle { get; set; }

    /// <summary>Returns the raw input device handle.</summary>
    /// <returns>The raw input device handle.</returns>
    public IntPtr ToIntPtr() => Handle;
}
