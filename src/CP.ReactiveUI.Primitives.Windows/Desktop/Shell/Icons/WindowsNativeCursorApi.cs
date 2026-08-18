// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif
/// <summary>Production Win32 cursor API implementation.</summary>
internal sealed class WindowsNativeCursorApi : INativeCursorApi
{
    /// <summary>The singleton instance.</summary>
    internal static readonly WindowsNativeCursorApi Instance = new();

    /// <inheritdoc />
    public IntPtr CopyImage(IntPtr imageHandle, ImageType type, int cx, int cy, CopyImageFlags flags) =>
        NativeCursorMethods.NativeMethods.CopyImage(imageHandle, type, cx, cy, flags);

    /// <inheritdoc />
    public IntPtr LoadImage(
        IntPtr instanceHandle,
        IntPtr name,
        ImageType type,
        int cx,
        int cy,
        LoadImageFlags loadFlags) =>
        NativeCursorMethods.NativeMethods.LoadImage(instanceHandle, name, type, cx, cy, loadFlags);

    /// <inheritdoc />
    public IntPtr LoadImage(
        IntPtr instanceHandle,
        string name,
        ImageType type,
        int cx,
        int cy,
        LoadImageFlags loadFlags) =>
        NativeCursorMethods.NativeMethods.LoadImage(instanceHandle, name, type, cx, cy, loadFlags);
}
