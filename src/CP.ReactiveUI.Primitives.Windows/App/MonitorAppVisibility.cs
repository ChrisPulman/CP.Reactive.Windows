// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Apps;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Apps;
#endif
/// <summary>A simple enum for the GetAppVisibilityOnMonitor method, this tells us if an App is visible on the supplied monitor.</summary>
internal enum MonitorAppVisibility
{
    /// <summary>Represents the MAV_UNKNOWN value.</summary>
    MAV_UNKNOWN,
    /// <summary>Represents the MAV_NO_APP_VISIBLE value.</summary>
    MAV_NO_APP_VISIBLE,
    /// <summary>Represents the MAV_APP_VISIBLE value.</summary>
    MAV_APP_VISIBLE,
}
