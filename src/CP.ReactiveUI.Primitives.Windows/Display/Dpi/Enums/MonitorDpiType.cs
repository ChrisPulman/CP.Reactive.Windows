// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Enums;
#endif
/// <summary>See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/dn280511(v=vs.85).aspx"> MONITOR_DPI_TYPE enumeration </a></summary>
public enum MonitorDpiType
{
    /// <summary>
    ///     The effective DPI.
    ///     This value should be used when determining the correct scale factor for scaling UI elements.
    ///     This incorporates the scale factor set by the user for this specific display.
    /// </summary>
    None,
    /// <summary>
    ///     The angular DPI.
    ///     This DPI ensures rendering at a compliant angular resolution on the screen.
    ///     This does not include the scale factor set by the user for this specific display.
    /// </summary>
    AngularDpi,
    /// <summary>
    ///     The raw DPI.
    ///     This value is the linear DPI of the screen as measured on the screen itself.
    ///     Use this value when you want to read the pixel density and not the recommended scaling setting.
    ///     This does not include the scale factor set by the user for this specific display and is not guaranteed to be a
    ///     supported DPI value.
    /// </summary>
    RawDpi
}
