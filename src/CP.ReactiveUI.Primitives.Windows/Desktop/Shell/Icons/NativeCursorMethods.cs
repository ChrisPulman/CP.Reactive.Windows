// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif
/// <summary>Win32 native methods for cursors.</summary>
#if NETFRAMEWORK
public static class NativeCursorMethods
#else
public static partial class NativeCursorMethods
#endif
{
    /// <summary>The native cursor API used by this process.</summary>
    private static INativeCursorApi _api = WindowsNativeCursorApi.Instance;

    /// <summary>Loads an image by integer resource name.</summary>
    /// <param name="instanceHandle">The instance handle.</param>
    /// <param name="name">The image resource name.</param>
    /// <param name="type">The image type.</param>
    /// <param name="cx">The requested width.</param>
    /// <param name="cy">The requested height.</param>
    /// <param name="loadFlags">The load flags.</param>
    /// <returns>The loaded image handle.</returns>
    internal static IntPtr LoadImage(
        IntPtr instanceHandle,
        IntPtr name,
        ImageType type,
        int cx,
        int cy,
        LoadImageFlags loadFlags) =>
        _api.LoadImage(instanceHandle, name, type, cx, cy, loadFlags);

    /// <summary>Loads an image by string resource name.</summary>
    /// <param name="instanceHandle">The instance handle.</param>
    /// <param name="name">The image resource name.</param>
    /// <param name="type">The image type.</param>
    /// <param name="cx">The requested width.</param>
    /// <param name="cy">The requested height.</param>
    /// <param name="loadFlags">The load flags.</param>
    /// <returns>The loaded image handle.</returns>
    internal static IntPtr LoadImage(
        IntPtr instanceHandle,
        string name,
        ImageType type,
        int cx,
        int cy,
        LoadImageFlags loadFlags) =>
        _api.LoadImage(instanceHandle, name, type, cx, cy, loadFlags);

    /// <summary>Copies an image handle.</summary>
    /// <param name="imageHandle">The image handle.</param>
    /// <param name="type">The image type.</param>
    /// <param name="cx">The requested width.</param>
    /// <param name="cy">The requested height.</param>
    /// <param name="flags">The copy flags.</param>
    /// <returns>The copied image handle.</returns>
    internal static IntPtr CopyImage(IntPtr imageHandle, ImageType type, int cx, int cy, CopyImageFlags flags) =>
        _api.CopyImage(imageHandle, type, cx, cy, flags);

    /// <summary>Replaces the native cursor API for deterministic tests.</summary>
    /// <param name="api">The replacement cursor API.</param>
    /// <returns>The previous cursor API.</returns>
    internal static INativeCursorApi SetApiForTesting(INativeCursorApi api)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(api);
        INativeCursorApi api2 = _api;
        _api = api;
        return api2;
    }

    /// <summary>Native cursor entry points.</summary>
#if NETFRAMEWORK
    internal static class NativeMethods
#else
    internal static partial class NativeMethods
#endif
    {
        /// <summary>Invokes the native <c>LoadImage</c> entry point.</summary>
        /// <param name="instanceHandle">The native <paramref name="instanceHandle" /> value.</param>
        /// <param name="name">The native <paramref name="name" /> value.</param>
        /// <param name="type">The native <paramref name="type" /> value.</param>
        /// <param name="cx">The native <paramref name="cx" /> value.</param>
        /// <param name="cy">The native <paramref name="cy" /> value.</param>
        /// <param name="loadFlags">The native <paramref name="loadFlags" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32.dll", EntryPoint = "LoadImageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr LoadImage(
            IntPtr instanceHandle,
            IntPtr name,
            ImageType type,
            int cx,
            int cy,
            LoadImageFlags loadFlags);
#else
        [LibraryImport("user32.dll", EntryPoint = "LoadImageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr LoadImage(
            IntPtr instanceHandle,
            IntPtr name,
            ImageType type,
            int cx,
            int cy,
            LoadImageFlags loadFlags);
#endif

        /// <summary>Invokes the native <c>LoadImage</c> entry point.</summary>
        /// <param name="instanceHandle">The native <paramref name="instanceHandle" /> value.</param>
        /// <param name="name">The native <paramref name="name" /> value.</param>
        /// <param name="type">The native <paramref name="type" /> value.</param>
        /// <param name="cx">The native <paramref name="cx" /> value.</param>
        /// <param name="cy">The native <paramref name="cy" /> value.</param>
        /// <param name="loadFlags">The native <paramref name="loadFlags" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "LoadImageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr LoadImage(
            IntPtr instanceHandle,
            string name,
            ImageType type,
            int cx,
            int cy,
            LoadImageFlags loadFlags);
#else
        [LibraryImport(
            "user32.dll",
            EntryPoint = "LoadImageW",
            SetLastError = true,
            StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr LoadImage(
            IntPtr instanceHandle,
            string name,
            ImageType type,
            int cx,
            int cy,
            LoadImageFlags loadFlags);
#endif

        /// <summary>Invokes the native <c>CopyImage</c> entry point.</summary>
        /// <param name="imageHandle">The native <paramref name="imageHandle" /> value.</param>
        /// <param name="type">The native <paramref name="type" /> value.</param>
        /// <param name="cx">The native <paramref name="cx" /> value.</param>
        /// <param name="cy">The native <paramref name="cy" /> value.</param>
        /// <param name="flags">The native <paramref name="flags" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr CopyImage(
            IntPtr imageHandle,
            ImageType type,
            int cx,
            int cy,
            CopyImageFlags flags);
#else
        [LibraryImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr CopyImage(
            IntPtr imageHandle,
            ImageType type,
            int cx,
            int cy,
            CopyImageFlags flags);
#endif
    }
}
