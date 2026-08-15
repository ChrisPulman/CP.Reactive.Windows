// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Enums;
#endif
/// <summary>
///     Identifies the dots per inch (dpi) setting for a thread, process, or window.
///     Can be used everywhere ProcessDpiAwareness is passed.
/// </summary>
public enum DpiAwareness
{
    /// <summary>Invalid DPI awareness. This is an invalid DPI awareness value.</summary>
    Invalid = -1,
    /// <summary>
    ///     DPI unaware.
    ///     This process does not scale for DPI changes and is always assumed to have a scale factor of 100% (96 DPI).
    ///     It will be automatically scaled by the system on any other DPI setting.
    /// </summary>
    Unaware,
    /// <summary>
    ///     System DPI aware.
    ///     This process does not scale for DPI changes.
    ///     It will query for the DPI once and use that value for the lifetime of the process.
    ///     If the DPI changes, the process will not adjust to the new DPI value.
    ///     It will be automatically scaled up or down by the system when the DPI changes from the system value.
    /// </summary>
    SystemAware,
    /// <summary>
    ///     Per monitor DPI aware.
    ///     This process checks for the DPI when it is created and adjusts the scale factor whenever the DPI changes.
    ///     These processes are not automatically scaled by the system.
    /// </summary>
    PerMonitorAware
}
