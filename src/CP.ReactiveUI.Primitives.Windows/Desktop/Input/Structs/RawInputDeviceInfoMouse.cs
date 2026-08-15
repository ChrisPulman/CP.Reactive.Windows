// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     This struct defines the raw input data coming from the specified mouse.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645589.aspx">RID_DEVICE_INFO_MOUSE structure</a>
/// Remarks:
/// For the mouse, the Usage Page is 1 and the Usage is 2.
/// </summary>
public readonly record struct RawInputDeviceInfoMouse
{
    /// <summary>Gets the identifier of the mouse device.</summary>
    public int Id { get; }

    /// <summary>Gets the number of buttons for the mouse.</summary>
    public int NumberOfButtons { get; }

    /// <summary>Gets the number of data points per second. This information may not be applicable for every mouse device.</summary>
    public int SampleRate { get; }

    /// <summary>Gets a value indicating whether the mouse has a wheel for horizontal scrolling.</summary>
    public bool HasHorizontalWheel { get; }
}
