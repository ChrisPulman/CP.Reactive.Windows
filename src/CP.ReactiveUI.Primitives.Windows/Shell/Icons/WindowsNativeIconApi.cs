// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif
/// <summary>Production Win32 icon API implementation.</summary>
internal sealed class WindowsNativeIconApi : INativeIconApi
{
    /// <summary>The singleton instance.</summary>
    internal static readonly WindowsNativeIconApi Instance = new();

    /// <inheritdoc />
    public SafeIconHandle CopyIcon(SafeIconHandle iconHandle) => NativeIconMethods.NativeMethods.CopyIcon(iconHandle);

    /// <inheritdoc />
    public SafeIconHandle CopyIcon(IntPtr iconHandle) => NativeIconMethods.NativeMethods.CopyIcon(iconHandle);

    /// <inheritdoc />
    public IntPtr CreateIconIndirect(ref IconInfo icon) => NativeIconMethods.NativeMethods.CreateIconIndirect(ref icon);

    /// <inheritdoc />
    public bool DrawIconEx(in NativeIconMethods.DrawIconArguments arguments) => NativeIconMethods.NativeMethods.DrawIconEx(arguments.DeviceContext, arguments.Left, arguments.Top, arguments.IconHandle, arguments.Width, arguments.Height, arguments.AnimationStep, arguments.FlickerFreeBrush, arguments.Flags);

    /// <inheritdoc />
    public bool GetIconInfo(SafeIconHandle iconHandle, out IconInfo iconInfo) => NativeIconMethods.NativeMethods.GetIconInfo(iconHandle, out iconInfo);

    /// <inheritdoc />
    public bool GetIconInfoEx(IntPtr iconOrCursorHandle, ref IconInfoEx iconInfoEx) => NativeIconMethods.NativeMethods.GetIconInfoEx(iconOrCursorHandle, ref iconInfoEx);

    /// <inheritdoc />
    public int LoadIconMetric(IntPtr instanceHandle, IntPtr iconName, IconMetricSize lims, out IntPtr iconHandle) => NativeIconMethods.NativeMethods.LoadIconMetric(instanceHandle, iconName, lims, out iconHandle);

    /// <inheritdoc />
    public int LoadIconMetric(IntPtr instanceHandle, string iconName, IconMetricSize lims, out IntPtr iconHandle) => NativeIconMethods.NativeMethods.LoadIconMetric(instanceHandle, iconName, lims, out iconHandle);

    /// <inheritdoc />
    public int LoadIconWithScaleDown(IntPtr instanceHandle, IntPtr iconName, int cx, int cy, out IntPtr iconHandle) => NativeIconMethods.NativeMethods.LoadIconWithScaleDown(instanceHandle, iconName, cx, cy, out iconHandle);

    /// <inheritdoc />
    public int LoadIconWithScaleDown(IntPtr instanceHandle, string iconName, int cx, int cy, out IntPtr iconHandle) => NativeIconMethods.NativeMethods.LoadIconWithScaleDown(instanceHandle, iconName, cx, cy, out iconHandle);

    bool INativeIconApi.DrawIconEx(in NativeIconMethods.DrawIconArguments arguments) => DrawIconEx(in arguments);
}
