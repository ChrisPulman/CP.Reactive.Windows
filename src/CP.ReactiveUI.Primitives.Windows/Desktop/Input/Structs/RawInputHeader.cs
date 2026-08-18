// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     Contains the header information that is part of the raw input data.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645571.aspx">RAWINPUTHEADER structure</a>
/// </summary>
public readonly record struct RawInputHeader
{
    /// <summary>Gets or sets type of raw input (RIM_TYPEHID 2, RIM_TYPEKEYBOARD 1, RIM_TYPEMOUSE 0).</summary>
    public RawInputDeviceTypes Type { get; init; }

    /// <summary>Gets or sets a handle to the Device.</summary>
    internal IntPtr DeviceHandle { get; init; }

    /// <summary>Returns the raw input device handle.</summary>
    /// <returns>The raw input device handle.</returns>
    public IntPtr ToIntPtr() => DeviceHandle;
}
