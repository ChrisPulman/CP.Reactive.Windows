// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Devices;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Devices;
#endif
/// <summary>Information on a volume change.</summary>
public class VolumeInfo
{
    /// <summary>Gets the type of the event.</summary>
    public DeviceChangeEvent EventType { get; internal set; }

    /// <summary>Gets the volume that was added or removed.</summary>
    public DevBroadcastVolume Volume { get; internal set; }
}
