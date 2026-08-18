// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif
/// <summary>Composes native icon access for production and deterministic tests.</summary>
internal interface INativeIconApi
{
    /// <summary>Copies an icon handle to a new icon handle.</summary>
    /// <param name="iconHandle">The icon handle to copy.</param>
    /// <returns>The copied icon handle.</returns>
    SafeIconHandle CopyIcon(SafeIconHandle iconHandle);

    /// <summary>Copies an icon handle to a new icon handle.</summary>
    /// <param name="iconHandle">The icon handle to copy.</param>
    /// <returns>The copied icon handle.</returns>
    SafeIconHandle CopyIcon(IntPtr iconHandle);

    /// <summary>Creates an icon from icon information.</summary>
    /// <param name="icon">Icon information.</param>
    /// <returns>The created icon handle.</returns>
    IntPtr CreateIconIndirect(ref IconInfo icon);

    /// <summary>Draws an icon or cursor.</summary>
    /// <param name="arguments">The draw operation arguments.</param>
    /// <returns><see langword="true" /> when drawing succeeded.</returns>
    bool DrawIconEx(in NativeIconMethods.DrawIconArguments arguments);

    /// <summary>Retrieves information about the specified icon or cursor.</summary>
    /// <param name="iconHandle">The icon or cursor handle.</param>
    /// <param name="iconInfo">The icon information.</param>
    /// <returns><see langword="true" /> when the call succeeds.</returns>
    bool GetIconInfo(SafeIconHandle iconHandle, out IconInfo iconInfo);

    /// <summary>Retrieves extended information about the specified icon or cursor.</summary>
    /// <param name="iconOrCursorHandle">The icon or cursor handle.</param>
    /// <param name="iconInfoEx">The extended icon information.</param>
    /// <returns><see langword="true" /> when the call succeeds.</returns>
    bool GetIconInfoEx(IntPtr iconOrCursorHandle, ref IconInfoEx iconInfoEx);

    /// <summary>Loads a metric-sized icon by integer resource identifier.</summary>
    /// <param name="instanceHandle">The module instance handle.</param>
    /// <param name="iconName">The icon resource identifier.</param>
    /// <param name="lims">The metric size.</param>
    /// <param name="iconHandle">The loaded icon handle.</param>
    /// <returns>The HRESULT from the native loader.</returns>
    int LoadIconMetric(IntPtr instanceHandle, IntPtr iconName, IconMetricSize lims, out IntPtr iconHandle);

    /// <summary>Loads a metric-sized icon by string resource name.</summary>
    /// <param name="instanceHandle">The module instance handle.</param>
    /// <param name="iconName">The icon resource name.</param>
    /// <param name="lims">The metric size.</param>
    /// <param name="iconHandle">The loaded icon handle.</param>
    /// <returns>The HRESULT from the native loader.</returns>
    int LoadIconMetric(IntPtr instanceHandle, string iconName, IconMetricSize lims, out IntPtr iconHandle);

    /// <summary>Loads a scaled icon by integer resource identifier.</summary>
    /// <param name="instanceHandle">The module instance handle.</param>
    /// <param name="iconName">The icon resource identifier.</param>
    /// <param name="cx">The desired width.</param>
    /// <param name="cy">The desired height.</param>
    /// <param name="iconHandle">The loaded icon handle.</param>
    /// <returns>The HRESULT from the native loader.</returns>
    int LoadIconWithScaleDown(IntPtr instanceHandle, IntPtr iconName, int cx, int cy, out IntPtr iconHandle);

    /// <summary>Loads a scaled icon by string resource name.</summary>
    /// <param name="instanceHandle">The module instance handle.</param>
    /// <param name="iconName">The icon resource name.</param>
    /// <param name="cx">The desired width.</param>
    /// <param name="cy">The desired height.</param>
    /// <param name="iconHandle">The loaded icon handle.</param>
    /// <returns>The HRESULT from the native loader.</returns>
    int LoadIconWithScaleDown(IntPtr instanceHandle, string iconName, int cx, int cy, out IntPtr iconHandle);
}
