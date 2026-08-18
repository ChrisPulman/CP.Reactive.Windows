// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
#endif
/// <summary>Provides convenient wrapper methods for DPI-aware Windows API calls.</summary>
public static class DpiApi
{
    /// <summary>
    /// Retrieves the value of one of the system metrics, taking into account the provided DPI value.
    /// This is a convenient wrapper around the GetSystemMetricsForDpi Win32 API.
    /// </summary>
    /// <param name="metric">The system metric or configuration setting to be retrieved.</param>
    /// <returns>The requested system metric or configuration setting scaled for the system DPI.</returns>
    public static int GetSystemMetrics(SystemMetric metric) => GetSystemMetrics(metric, NativeDpiMethods.GetDpiForSystem());

    /// <summary>
    /// Retrieves the value of one of the system metrics, taking into account the provided DPI value.
    /// This is a convenient wrapper around the GetSystemMetricsForDpi Win32 API.
    /// </summary>
    /// <param name="metric">The system metric or configuration setting to be retrieved.</param>
    /// <param name="dpi">The DPI to use for scaling the metric.</param>
    /// <returns>The requested system metric or configuration setting scaled for the specified DPI.</returns>
    public static int GetSystemMetrics(SystemMetric metric, uint dpi) => NativeDpiMethods.GetSystemMetricsForDpi(metric, dpi);

    /// <summary>
    /// Retrieves the value of one of the system metrics for a specific window, taking into account the window's DPI.
    /// This is a convenient wrapper around the GetSystemMetricsForDpi Win32 API.
    /// </summary>
    /// <param name="metric">The system metric or configuration setting to be retrieved.</param>
    /// <param name="windowHandle">Handle to the window. The DPI of this window will be used for scaling.</param>
    /// <returns>The requested system metric or configuration setting scaled for the window's DPI.</returns>
    public static int GetSystemMetricsForWindow(SystemMetric metric, IntPtr windowHandle)
    {
        var dpi = checked((uint)NativeDpiMethods.GetDpi(windowHandle));
        return NativeDpiMethods.GetSystemMetricsForDpi(metric, dpi);
    }

    /// <summary>
    /// Calculates the required size of the window rectangle, based on the desired size of the client rectangle and the provided DPI.
    /// This is a convenient wrapper around the AdjustWindowRectExForDpi Win32 API.
    /// </summary>
    /// <param name="clientRect">The desired client rectangle.</param>
    /// <param name="style">The window style of the window.</param>
    /// <returns>The calculated window rectangle, or null if the function fails.</returns>
    public static NativeRect? AdjustWindowRect(NativeRect clientRect, WindowStyleFlags style) =>
        AdjustWindowRect(
            clientRect,
            style,
            hasMenu: false,
            ExtendedWindowStyleFlags.None,
            NativeDpiMethods.GetDpiForSystem());

    /// <summary>
    /// Calculates the required size of the window rectangle, based on the desired size of the client rectangle and the
    /// provided DPI.
    /// </summary>
    /// <param name="clientRect">The desired client rectangle.</param>
    /// <param name="style">The window style of the window.</param>
    /// <param name="hasMenu">Indicates whether the window has a menu.</param>
    /// <returns>The calculated window rectangle, or null if the function fails.</returns>
    public static NativeRect? AdjustWindowRect(NativeRect clientRect, WindowStyleFlags style, bool hasMenu) =>
        AdjustWindowRect(clientRect, style, hasMenu, ExtendedWindowStyleFlags.None, NativeDpiMethods.GetDpiForSystem());

    /// <summary>
    /// Calculates the required size of the window rectangle, based on the desired size of the client rectangle and the
    /// provided DPI.
    /// </summary>
    /// <param name="clientRect">The desired client rectangle.</param>
    /// <param name="style">The window style of the window.</param>
    /// <param name="hasMenu">Indicates whether the window has a menu.</param>
    /// <param name="extendedStyle">The extended window style of the window.</param>
    /// <returns>The calculated window rectangle, or null if the function fails.</returns>
    public static NativeRect? AdjustWindowRect(
        NativeRect clientRect,
        WindowStyleFlags style,
        bool hasMenu,
        ExtendedWindowStyleFlags extendedStyle) =>
        AdjustWindowRect(clientRect, style, hasMenu, extendedStyle, NativeDpiMethods.GetDpiForSystem());

    /// <summary>
    /// Calculates the required size of the window rectangle, based on the desired size of the client rectangle and the
    /// provided DPI.
    /// </summary>
    /// <param name="clientRect">The desired client rectangle.</param>
    /// <param name="style">The window style of the window.</param>
    /// <param name="hasMenu">Indicates whether the window has a menu.</param>
    /// <param name="extendedStyle">The extended window style of the window.</param>
    /// <param name="dpi">The DPI to use for scaling.</param>
    /// <returns>The calculated window rectangle, or null if the function fails.</returns>
    public static NativeRect? AdjustWindowRect(NativeRect clientRect, WindowStyleFlags style, bool hasMenu, ExtendedWindowStyleFlags extendedStyle, uint dpi)
    {
        var rect = clientRect;
        return NativeDpiMethods.AdjustWindowRectExForDpi(ref rect, style, hasMenu, extendedStyle, dpi) ? rect : null;
    }

    /// <summary>
    /// Calculates the required size of the window rectangle for a specific window, based on the desired size of the client rectangle and the window's DPI.
    /// This is a convenient wrapper around the AdjustWindowRectExForDpi Win32 API.
    /// </summary>
    /// <param name="clientRect">The desired client rectangle.</param>
    /// <param name="style">The window style of the window.</param>
    /// <param name="windowHandle">Handle to the window. The DPI of this window will be used for scaling.</param>
    /// <returns>The calculated window rectangle, or null if the function fails.</returns>
    public static NativeRect? AdjustWindowRectForWindow(
        NativeRect clientRect,
        WindowStyleFlags style,
        IntPtr windowHandle) =>
        AdjustWindowRectForWindow(clientRect, style, windowHandle, hasMenu: false, ExtendedWindowStyleFlags.None);

    /// <summary>
    /// Calculates the required size of the window rectangle for a specific window, based on the desired size of the client
    /// rectangle and the window's DPI.
    /// </summary>
    /// <param name="clientRect">The desired client rectangle.</param>
    /// <param name="style">The window style of the window.</param>
    /// <param name="windowHandle">Handle to the window. The DPI of this window will be used for scaling.</param>
    /// <param name="hasMenu">Indicates whether the window has a menu.</param>
    /// <returns>The calculated window rectangle, or null if the function fails.</returns>
    public static NativeRect? AdjustWindowRectForWindow(
        NativeRect clientRect,
        WindowStyleFlags style,
        IntPtr windowHandle,
        bool hasMenu) =>
        AdjustWindowRectForWindow(clientRect, style, windowHandle, hasMenu, ExtendedWindowStyleFlags.None);

    /// <summary>
    /// Calculates the required size of the window rectangle for a specific window, based on the desired size of the client
    /// rectangle and the window's DPI.
    /// </summary>
    /// <param name="clientRect">The desired client rectangle.</param>
    /// <param name="style">The window style of the window.</param>
    /// <param name="windowHandle">Handle to the window. The DPI of this window will be used for scaling.</param>
    /// <param name="hasMenu">Indicates whether the window has a menu.</param>
    /// <param name="extendedStyle">The extended window style of the window.</param>
    /// <returns>The calculated window rectangle, or null if the function fails.</returns>
    public static NativeRect? AdjustWindowRectForWindow(
        NativeRect clientRect,
        WindowStyleFlags style,
        IntPtr windowHandle,
        bool hasMenu,
        ExtendedWindowStyleFlags extendedStyle)
    {
        var dpi = checked((uint)NativeDpiMethods.GetDpi(windowHandle));
        var rect = clientRect;
        return NativeDpiMethods.AdjustWindowRectExForDpi(ref rect, style, hasMenu, extendedStyle, dpi) ? rect : null;
    }

    /// <summary>
    /// Retrieves system parameters information for a specific DPI.
    /// This is a convenient wrapper around the SystemParametersInfoForDpi Win32 API.
    /// </summary>
    /// <typeparam name="T">The type of the parameter structure.</typeparam>
    /// <param name="action">The system parameter to query.</param>
    /// <param name="value">The requested system parameter when the function succeeds.</param>
    /// <returns><see langword="true" /> when the function succeeds.</returns>
    public static bool TryGetSystemParametersInfo<T>(SystemParametersInfoActions action, out T value)
        where T : struct => TryGetSystemParametersInfo(action, NativeDpiMethods.GetDpiForSystem(), out value);

    /// <summary>
    /// Retrieves system parameters information for a specific DPI.
    /// This is a convenient wrapper around the SystemParametersInfoForDpi Win32 API.
    /// </summary>
    /// <typeparam name="T">The type of the parameter structure.</typeparam>
    /// <param name="action">The system parameter to query.</param>
    /// <param name="dpi">The DPI to use for scaling.</param>
    /// <param name="value">The requested system parameter when the function succeeds.</param>
    /// <returns><see langword="true" /> when the function succeeds.</returns>
    public static bool TryGetSystemParametersInfo<T>(SystemParametersInfoActions action, uint dpi, out T value)
        where T : struct => TryGetSystemParametersInfoInternal(action, dpi, out value);

    /// <summary>
    /// Retrieves system parameters information for a specific window's DPI.
    /// This is a convenient wrapper around the SystemParametersInfoForDpi Win32 API.
    /// </summary>
    /// <typeparam name="T">The type of the parameter structure.</typeparam>
    /// <param name="action">The system parameter to query.</param>
    /// <param name="windowHandle">Handle to the window. The DPI of this window will be used for scaling.</param>
    /// <param name="value">The requested system parameter when the function succeeds.</param>
    /// <returns><see langword="true" /> when the function succeeds.</returns>
    public static bool TryGetSystemParametersInfoForWindow<T>(SystemParametersInfoActions action, IntPtr windowHandle, out T value)
        where T : struct
    {
        var dpi = checked((uint)NativeDpiMethods.GetDpi(windowHandle));
        return TryGetSystemParametersInfoInternal(action, dpi, out value);
    }

    /// <summary>Retrieves system parameters information.</summary>
    /// <typeparam name="T">The type of the parameter structure.</typeparam>
    /// <param name="action">The system parameter to query.</param>
    /// <param name="dpi">The DPI to use for scaling.</param>
    /// <param name="value">The requested system parameter when the function succeeds.</param>
    /// <returns><see langword="true" /> when the function succeeds.</returns>
    private static bool TryGetSystemParametersInfoInternal<T>(SystemParametersInfoActions action, uint dpi, out T value)
        where T : struct
    {
        var size = Marshal.SizeOf<T>();
        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            if (NativeDpiMethods.SystemParametersInfoForDpi(action, checked((uint)size), ptr, SystemParametersInfoBehaviors.None, dpi))
            {
                value = Marshal.PtrToStructure<T>(ptr);
                return true;
            }

            value = default;
            return false;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
