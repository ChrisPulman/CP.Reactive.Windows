// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
#endif
/// <summary>Provides composable access to native DPI operations.</summary>
internal interface INativeDpiApi
{
    /// <summary>Creates a default thread DPI-awareness scope.</summary>
    /// <returns>A scope that restores the previous DPI-awareness context.</returns>
    IDisposable DefaultScopedThreadDpiAwarenessContext();

    /// <summary>Enables non-client DPI scaling for the specified window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The operation result.</returns>
    HResult EnableNonClientDpiScaling(IntPtr windowHandle);

    /// <summary>Gets the DPI for a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The DPI value.</returns>
    int GetDpi(IntPtr windowHandle);

    /// <summary>Gets the DPI for a screen location.</summary>
    /// <param name="location">The screen location.</param>
    /// <returns>The DPI value.</returns>
    int GetDpi(NativePoint location);

    /// <summary>Gets the system DPI.</summary>
    /// <returns>The system DPI.</returns>
    uint GetDpiForSystem();

    /// <summary>Gets a DPI-scaled system metric.</summary>
    /// <param name="index">The system metric.</param>
    /// <param name="dpi">The DPI value.</param>
    /// <returns>The metric value.</returns>
    int GetSystemMetricsForDpi(SystemMetric index, uint dpi);

    /// <summary>Adjusts a window rectangle for the supplied DPI.</summary>
    /// <param name="rect">The rectangle to adjust.</param>
    /// <param name="style">The window style.</param>
    /// <param name="hasMenu">A value indicating whether the window has a menu.</param>
    /// <param name="extendedStyle">The extended window style.</param>
    /// <param name="dpi">The DPI value.</param>
    /// <returns><see langword="true" /> when the operation succeeds.</returns>
    bool AdjustWindowRectExForDpi(ref NativeRect rect, WindowStyleFlags style, bool hasMenu, ExtendedWindowStyleFlags extendedStyle, uint dpi);

    /// <summary>Retrieves a DPI-scaled system parameter.</summary>
    /// <param name="action">The system parameter action.</param>
    /// <param name="uiParameter">The action-specific unsigned integer parameter.</param>
    /// <param name="parameter">The destination parameter buffer.</param>
    /// <param name="updateProfileFlags">Profile update flags.</param>
    /// <param name="dpi">The DPI value.</param>
    /// <returns><see langword="true" /> when the operation succeeds.</returns>
    bool SystemParametersInfoForDpi(SystemParametersInfoActions action, uint uiParameter, IntPtr parameter, SystemParametersInfoBehaviors updateProfileFlags, uint dpi);
}
