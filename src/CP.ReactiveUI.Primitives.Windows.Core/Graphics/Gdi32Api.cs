// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi;

/// <summary>Provides typed wrappers for GDI32 drawing and bitmap entry points.</summary>
#if NETFRAMEWORK
public static class Gdi32Api
#else
public static partial class Gdi32Api
#endif
{
    /// <summary>Performs a bit-block transfer from a source device context to a destination device context.</summary>
    /// <param name="destinationDeviceContext">The destination device context.</param>
    /// <param name="destination">The destination rectangle.</param>
    /// <param name="sourceDeviceContext">The source device context.</param>
    /// <param name="source">The source point.</param>
    /// <param name="rasterOperation">The raster operation.</param>
    /// <returns><see langword="true" /> when the transfer succeeds; otherwise, <see langword="false" />.</returns>
    public static bool BitBlt(
        SafeHandle destinationDeviceContext,
        Rectangle destination,
        SafeHandle sourceDeviceContext,
        Point source,
        RasterOperations rasterOperation) =>
        NativeMethods.BitBlt(
            destinationDeviceContext,
            destination.Left,
            destination.Top,
            destination.Width,
            destination.Height,
            sourceDeviceContext,
            source.X,
            source.Y,
            rasterOperation);

    /// <summary>Creates a memory device context compatible with <paramref name="deviceContext" />.</summary>
    /// <param name="deviceContext">The device context to match.</param>
    /// <returns>A compatible memory device context.</returns>
    public static SafeCompatibleDcHandle CreateCompatibleDC(SafeHandle deviceContext) =>
        NativeMethods.CreateCompatibleDC(deviceContext);

    /// <summary>Creates a device-independent bitmap section.</summary>
    /// <param name="deviceContext">The device context for the bitmap.</param>
    /// <param name="bitmapInfo">The bitmap metadata.</param>
    /// <param name="usage">The color-table interpretation.</param>
    /// <param name="bits">Receives a pointer to the bitmap bits.</param>
    /// <param name="sectionHandle">The optional file-mapping handle.</param>
    /// <param name="offset">The byte offset in the file mapping.</param>
    /// <returns>A handle for the bitmap section.</returns>
    public static SafeDibSectionHandle CreateDIBSection(
        SafeHandle deviceContext,
        ref BitmapV5Header bitmapInfo,
        DibColors usage,
        out IntPtr bits,
        IntPtr sectionHandle,
        uint offset) =>
        NativeMethods.CreateDIBSection(
            deviceContext,
            ref bitmapInfo,
            usage,
            out bits,
            sectionHandle,
            offset);

    /// <summary>Creates a rectangular GDI region.</summary>
    /// <param name="left">The left coordinate.</param>
    /// <param name="top">The top coordinate.</param>
    /// <param name="right">The right coordinate.</param>
    /// <param name="bottom">The bottom coordinate.</param>
    /// <returns>A handle for the new region.</returns>
    public static SafeRegionHandle CreateRectRgn(int left, int top, int right, int bottom) =>
        NativeMethods.CreateRectRgn(left, top, right, bottom);

    /// <summary>Gets a device capability from a device context.</summary>
    /// <param name="deviceContextHandle">The device context.</param>
    /// <param name="index">The requested capability.</param>
    /// <returns>The capability value.</returns>
    public static int GetDeviceCaps(SafeHandle deviceContextHandle, DeviceCaps index) =>
        NativeMethods.GetDeviceCaps(deviceContextHandle, index);

    /// <summary>Gets the COLORREF value for a pixel in a device context.</summary>
    /// <param name="deviceContextHandle">The device context.</param>
    /// <param name="horizontalPosition">The horizontal pixel position.</param>
    /// <param name="verticalPosition">The vertical pixel position.</param>
    /// <returns>The pixel COLORREF value.</returns>
    public static uint GetPixel(
        SafeHandle deviceContextHandle,
        int horizontalPosition,
        int verticalPosition) =>
        NativeMethods.GetPixel(deviceContextHandle, horizontalPosition, verticalPosition);

    /// <summary>Selects a GDI object into a device context.</summary>
    /// <param name="deviceContext">The device context.</param>
    /// <param name="objectHandle">The GDI object to select.</param>
    /// <returns>A non-owning handle for the previously selected object.</returns>
    public static SafeNonDisposableObjectHandle SelectObject(
        SafeDcHandle deviceContext,
        SafeObjectHandle objectHandle) =>
        new(SelectObjectHandle(deviceContext, objectHandle));

    /// <summary>Performs a stretch transfer from a source device context to a destination device context.</summary>
    /// <param name="destinationDeviceContext">The destination device context.</param>
    /// <param name="destination">The destination rectangle.</param>
    /// <param name="sourceDeviceContext">The source device context.</param>
    /// <param name="source">The source rectangle.</param>
    /// <param name="rasterOperation">The raster operation.</param>
    /// <returns><see langword="true" /> when the transfer succeeds; otherwise, <see langword="false" />.</returns>
    public static bool StretchBlt(
        SafeHandle destinationDeviceContext,
        Rectangle destination,
        SafeHandle sourceDeviceContext,
        Rectangle source,
        RasterOperations rasterOperation) =>
        NativeMethods.StretchBlt(
            destinationDeviceContext,
            destination.Left,
            destination.Top,
            destination.Width,
            destination.Height,
            sourceDeviceContext,
            source.Left,
            source.Top,
            source.Width,
            source.Height,
            rasterOperation);

    /// <summary>Copies bitmap information into <paramref name="destinationObject" />.</summary>
    /// <param name="graphicsObject">The bitmap handle.</param>
    /// <param name="bufferSize">The size of the destination structure.</param>
    /// <param name="destinationObject">Receives bitmap information.</param>
    /// <returns>The number of bytes copied.</returns>
    public static int GetObject(
        SafeHBitmapHandle graphicsObject,
        int bufferSize,
        ref GdiBitmap destinationObject) =>
        NativeMethods.GetObject(graphicsObject, bufferSize, ref destinationObject);

    /// <summary>Deletes a GDI object.</summary>
    /// <param name="objectHandle">The object handle to delete.</param>
    /// <returns><see langword="true" /> when the object is deleted; otherwise, <see langword="false" />.</returns>
    public static bool DeleteObject(IntPtr objectHandle) => NativeMethods.DeleteObject(objectHandle);

    /// <summary>Creates a solid GDI brush from a COLORREF value.</summary>
    /// <param name="color">The brush color.</param>
    /// <returns>The native brush handle.</returns>
    public static IntPtr CreateSolidBrush(uint color) => NativeMethods.CreateSolidBrush(color);

    /// <summary>Copies scan lines from a DIB into unmanaged memory.</summary>
    /// <param name="deviceContextHandle">The device context.</param>
    /// <param name="bitmap">The bitmap handle.</param>
    /// <param name="start">The first scan line.</param>
    /// <param name="lineCount">The number of scan lines.</param>
    /// <param name="bits">The destination buffer.</param>
    /// <param name="bitmapInfo">The bitmap metadata.</param>
    /// <param name="usage">The color-table interpretation.</param>
    /// <returns>The number of scan lines copied.</returns>
    public static int GetDIBits(
        SafeWindowDcHandle deviceContextHandle,
        SafeHBitmapHandle bitmap,
        uint start,
        uint lineCount,
        IntPtr bits,
        ref BitmapInfoHeader bitmapInfo,
        DibColors usage) =>
        NativeMethods.GetDIBits(
            deviceContextHandle,
            bitmap,
            start,
            lineCount,
            bits,
            ref bitmapInfo,
            usage);

    /// <summary>Selects an object through the single native entry-point owner.</summary>
    /// <param name="deviceContext">The device context.</param>
    /// <param name="objectHandle">The object to select.</param>
    /// <returns>The previously selected object handle.</returns>
    internal static IntPtr SelectObjectHandle(SafeHandle deviceContext, SafeHandle objectHandle) =>
        NativeMethods.SelectObject(deviceContext, objectHandle);

    /// <summary>Restores a previously selected GDI object.</summary>
    /// <param name="deviceContext">The device context.</param>
    /// <param name="objectHandle">The object handle to restore.</param>
    /// <returns>The object handle replaced by the restored object.</returns>
    internal static IntPtr RestoreObjectHandle(SafeHandle deviceContext, IntPtr objectHandle)
    {
        using SafeNonDisposableObjectHandle objectHandleWrapper = new(objectHandle);
        return NativeMethods.SelectObject(deviceContext, objectHandleWrapper);
    }

    /// <summary>Contains the native GDI32 entry points.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>Identifies the GDI32 system library.</summary>
        private const string Gdi32Dll = "gdi32.dll";

#if NETFRAMEWORK
        /// <summary>Invokes the native <c>BitBlt</c> entry point.</summary>
        /// <param name="destinationDeviceContext">The destination device context.</param>
        /// <param name="destinationLeft">The destination x-coordinate.</param>
        /// <param name="destinationTop">The destination y-coordinate.</param>
        /// <param name="width">The transfer width.</param>
        /// <param name="height">The transfer height.</param>
        /// <param name="sourceDeviceContext">The source device context.</param>
        /// <param name="sourceLeft">The source x-coordinate.</param>
        /// <param name="sourceTop">The source y-coordinate.</param>
        /// <param name="rasterOperation">The raster operation.</param>
        /// <returns><see langword="true" /> when the transfer succeeds; otherwise, <see langword="false" />.</returns>
        [DllImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BitBlt(
            SafeHandle destinationDeviceContext,
            int destinationLeft,
            int destinationTop,
            int width,
            int height,
            SafeHandle sourceDeviceContext,
            int sourceLeft,
            int sourceTop,
            RasterOperations rasterOperation);

        /// <summary>Invokes the native <c>CreateCompatibleDC</c> entry point.</summary>
        /// <param name="deviceContext">The device context to match.</param>
        /// <returns>A compatible device-context handle.</returns>
        [DllImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern SafeCompatibleDcHandle CreateCompatibleDC(SafeHandle deviceContext);

        /// <summary>Invokes the native <c>CreateDIBSection</c> entry point.</summary>
        /// <param name="deviceContext">The device context.</param>
        /// <param name="bitmapInfo">The bitmap metadata.</param>
        /// <param name="usage">The color-table interpretation.</param>
        /// <param name="bits">Receives a pointer to the bitmap bits.</param>
        /// <param name="sectionHandle">The optional section handle.</param>
        /// <param name="offset">The section offset.</param>
        /// <returns>A bitmap-section handle.</returns>
        [DllImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern SafeDibSectionHandle CreateDIBSection(
            SafeHandle deviceContext,
            ref BitmapV5Header bitmapInfo,
            DibColors usage,
            out IntPtr bits,
            IntPtr sectionHandle,
            uint offset);

        /// <summary>Invokes the native <c>CreateRectRgn</c> entry point.</summary>
        /// <param name="left">The left coordinate.</param>
        /// <param name="top">The top coordinate.</param>
        /// <param name="right">The right coordinate.</param>
        /// <param name="bottom">The bottom coordinate.</param>
        /// <returns>A region handle.</returns>
        [DllImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern SafeRegionHandle CreateRectRgn(int left, int top, int right, int bottom);

        /// <summary>Invokes the native <c>GetDeviceCaps</c> entry point.</summary>
        /// <param name="deviceContext">The device context.</param>
        /// <param name="index">The requested capability.</param>
        /// <returns>The capability value.</returns>
        [DllImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetDeviceCaps(SafeHandle deviceContext, DeviceCaps index);

        /// <summary>Invokes the native <c>GetPixel</c> entry point.</summary>
        /// <param name="deviceContext">The device context.</param>
        /// <param name="horizontalPosition">The horizontal pixel position.</param>
        /// <param name="verticalPosition">The vertical pixel position.</param>
        /// <returns>The pixel value.</returns>
        [DllImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern uint GetPixel(
            SafeHandle deviceContext,
            int horizontalPosition,
            int verticalPosition);

        /// <summary>Invokes the native <c>SelectObject</c> entry point.</summary>
        /// <param name="deviceContext">The device context.</param>
        /// <param name="objectHandle">The object to select.</param>
        /// <returns>The previously selected object handle.</returns>
        [DllImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr SelectObject(SafeHandle deviceContext, SafeHandle objectHandle);

        /// <summary>Invokes the native <c>StretchBlt</c> entry point.</summary>
        /// <param name="destinationDeviceContext">The destination device context.</param>
        /// <param name="destinationLeft">The destination x-coordinate.</param>
        /// <param name="destinationTop">The destination y-coordinate.</param>
        /// <param name="destinationWidth">The destination width.</param>
        /// <param name="destinationHeight">The destination height.</param>
        /// <param name="sourceDeviceContext">The source device context.</param>
        /// <param name="sourceLeft">The source x-coordinate.</param>
        /// <param name="sourceTop">The source y-coordinate.</param>
        /// <param name="sourceWidth">The source width.</param>
        /// <param name="sourceHeight">The source height.</param>
        /// <param name="rasterOperation">The raster operation.</param>
        /// <returns><see langword="true" /> when the transfer succeeds; otherwise, <see langword="false" />.</returns>
        [DllImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool StretchBlt(
            SafeHandle destinationDeviceContext,
            int destinationLeft,
            int destinationTop,
            int destinationWidth,
            int destinationHeight,
            SafeHandle sourceDeviceContext,
            int sourceLeft,
            int sourceTop,
            int sourceWidth,
            int sourceHeight,
            RasterOperations rasterOperation);

        /// <summary>Invokes the native <c>GetObjectW</c> entry point.</summary>
        /// <param name="graphicsObject">The bitmap handle.</param>
        /// <param name="bufferSize">The destination buffer size.</param>
        /// <param name="destinationObject">Receives the bitmap information.</param>
        /// <returns>The number of bytes copied.</returns>
        [DllImport(Gdi32Dll, EntryPoint = "GetObjectW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetObject(
            SafeHBitmapHandle graphicsObject,
            int bufferSize,
            ref GdiBitmap destinationObject);

        /// <summary>Invokes the native <c>DeleteObject</c> entry point.</summary>
        /// <param name="objectHandle">The object handle.</param>
        /// <returns><see langword="true" /> when the object is deleted; otherwise, <see langword="false" />.</returns>
        [DllImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr objectHandle);

        /// <summary>Invokes the native <c>CreateSolidBrush</c> entry point.</summary>
        /// <param name="color">The brush color.</param>
        /// <returns>The native brush handle.</returns>
        [DllImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr CreateSolidBrush(uint color);

        /// <summary>Invokes the native <c>GetDIBits</c> entry point.</summary>
        /// <param name="deviceContext">The device context.</param>
        /// <param name="bitmap">The bitmap handle.</param>
        /// <param name="start">The first scan line.</param>
        /// <param name="lineCount">The number of scan lines.</param>
        /// <param name="bits">The destination buffer.</param>
        /// <param name="bitmapInfo">The bitmap metadata.</param>
        /// <param name="usage">The color-table interpretation.</param>
        /// <returns>The number of scan lines copied.</returns>
        [DllImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetDIBits(
            SafeWindowDcHandle deviceContext,
            SafeHBitmapHandle bitmap,
            uint start,
            uint lineCount,
            IntPtr bits,
            ref BitmapInfoHeader bitmapInfo,
            DibColors usage);
#else
        [LibraryImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool BitBlt(
            SafeHandle destinationDeviceContext,
            int destinationLeft,
            int destinationTop,
            int width,
            int height,
            SafeHandle sourceDeviceContext,
            int sourceLeft,
            int sourceTop,
            RasterOperations rasterOperation);

        [LibraryImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial SafeCompatibleDcHandle CreateCompatibleDC(SafeHandle deviceContext);

        [LibraryImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial SafeDibSectionHandle CreateDIBSection(
            SafeHandle deviceContext,
            ref BitmapV5Header bitmapInfo,
            DibColors usage,
            out IntPtr bits,
            IntPtr sectionHandle,
            uint offset);

        [LibraryImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial SafeRegionHandle CreateRectRgn(int left, int top, int right, int bottom);

        [LibraryImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int GetDeviceCaps(SafeHandle deviceContext, DeviceCaps index);

        [LibraryImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial uint GetPixel(
            SafeHandle deviceContext,
            int horizontalPosition,
            int verticalPosition);

        [LibraryImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr SelectObject(SafeHandle deviceContext, SafeHandle objectHandle);

        [LibraryImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool StretchBlt(
            SafeHandle destinationDeviceContext,
            int destinationLeft,
            int destinationTop,
            int destinationWidth,
            int destinationHeight,
            SafeHandle sourceDeviceContext,
            int sourceLeft,
            int sourceTop,
            int sourceWidth,
            int sourceHeight,
            RasterOperations rasterOperation);

        [LibraryImport(Gdi32Dll, EntryPoint = "GetObjectW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int GetObject(
            SafeHBitmapHandle graphicsObject,
            int bufferSize,
            ref GdiBitmap destinationObject);

        [LibraryImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool DeleteObject(IntPtr objectHandle);

        [LibraryImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr CreateSolidBrush(uint color);

        [LibraryImport(Gdi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int GetDIBits(
            SafeWindowDcHandle deviceContext,
            SafeHBitmapHandle bitmap,
            uint start,
            uint lineCount,
            IntPtr bits,
            ref BitmapInfoHeader bitmapInfo,
            DibColors usage);
#endif
    }
}
