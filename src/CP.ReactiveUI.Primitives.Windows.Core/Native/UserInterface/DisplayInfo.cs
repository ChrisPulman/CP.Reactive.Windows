// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface;

/// <summary>Describes a display returned by Windows monitor enumeration.</summary>
public class DisplayInfo
{
    /// <summary>Gets or sets the display monitor handle.</summary>
    public SafeMonitorHandle MonitorHandle { get; set; }

    /// <summary>Gets or sets index of the Display, as specified in the "control panel".</summary>
    public int? Index { get; set; }

    /// <summary>Gets or sets screen bounds.</summary>
    public NativeRect Bounds { get; set; }

    /// <summary>Gets or sets device name.</summary>
    public string DeviceName { get; set; }

    /// <summary>Gets or sets is this the primary monitor.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>Gets or sets height of the screen.</summary>
    public int ScreenHeight { get; set; }

    /// <summary>Gets or sets width of the screen.</summary>
    public int ScreenWidth { get; set; }

    /// <summary>Gets or sets desktop working area.</summary>
    public NativeRect WorkingArea { get; set; }
}
