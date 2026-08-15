// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Information on RawInput device changes.</summary>
public class RawInputDeviceChangeEventArgs : EventArgs
{
    /// <summary>Gets or sets if true it was added, if false it was removed.</summary>
    public bool Added { get; set; }

    /// <summary>Gets or sets the device which was added or removed.</summary>
    public RawInputDeviceInformation DeviceInformation { get; set; }
}
