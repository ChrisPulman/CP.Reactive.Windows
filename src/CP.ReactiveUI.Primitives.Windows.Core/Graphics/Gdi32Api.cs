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

/// <summary>Provides GDI32 helpers.</summary>
public static class Gdi32Api
{
    /// <summary>Performs a bit-block transfer from the source device context into a destination device context.</summary>
    /// <param name="destinationDeviceContext">A handle to the destination device context.</param>
    /// <param name="destination">The destination rectangle.</param>
    /// <param name="sourceDeviceContext">A handle to the source device context.</param>
    /// <param name="source">The upper-left source point.</param>
    /// <param name="rasterOperation">The raster operation to perform.</param>
    /// <returns>True if the bit-block transfer succeeds.</returns>
    public static bool BitBlt(SafeHandle destinationDeviceContext, Rectangle destination, SafeHandle sourceDeviceContext, Point source, RasterOperations rasterOperation) => NativeMethods.BitBlt(destinationDeviceContext, destination.Left, destination.Top, destination.Width, destination.Height, sourceDeviceContext, source.X, source.Y, rasterOperation);

    /// <summary>
    ///     The CreateCompatibleDC function creates a memory device context (DC) compatible with the specified device.
    ///     See
    ///     <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/dd183489(v=vs.85).aspx">
    ///         CreateCompatibleDC
    ///         function
    ///     </a>
    /// </summary>
    /// <param name="deviceContext">
    ///     A handle to an existing DC. If this handle is NULL, the function creates a memory DC compatible with
    ///     the application's current screen.
    /// </param>
    /// <returns>
    ///     If the function succeeds, the return value is the handle to a memory DC.
    ///     If the function fails, the return value is NULL.
    /// </returns>
    public static SafeCompatibleDcHandle CreateCompatibleDC(SafeHandle deviceContext) => NativeMethods.CreateCompatibleDC(deviceContext);

    /// <summary>Native GDI32 entry points.</summary>
    private static class NativeMethods
    {
        /// <summary>The GDI32 system library name.</summary>
        private const string Gdi32Dll = "gdi32.dll";

        /// <summary>Invokes the native <c>BitBlt</c> entry point.</summary>
        /// <param name="destinationDeviceContext">The native <paramref name="destinationDeviceContext" /> value.</param>
        /// <param name="destinationLeft">The native <paramref name="destinationLeft" /> value.</param>
        /// <param name="destinationTop">The native <paramref name="destinationTop" /> value.</param>
        /// <param name="width">The native <paramref name="width" /> value.</param>
        /// <param name="height">The native <paramref name="height" /> value.</param>
        /// <param name="sourceDeviceContext">The native <paramref name="sourceDeviceContext" /> value.</param>
        /// <param name="sourceLeft">The native <paramref name="sourceLeft" /> value.</param>
        /// <param name="sourceTop">The native <paramref name="sourceTop" /> value.</param>
        /// <param name="rasterOperation">The native <paramref name="rasterOperation" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BitBlt(SafeHandle destinationDeviceContext, int destinationLeft, int destinationTop, int width, int height, SafeHandle sourceDeviceContext, int sourceLeft, int sourceTop, RasterOperations rasterOperation);

        /// <summary>Invokes the native <c>CreateCompatibleDC</c> entry point.</summary>
        /// <param name="deviceContext">The native <paramref name="deviceContext" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern SafeCompatibleDcHandle CreateCompatibleDC(SafeHandle deviceContext);

        /// <summary>Invokes the native <c>CreateDIBSection</c> entry point.</summary>
        /// <param name="deviceContext">The native <paramref name="deviceContext" /> value.</param>
        /// <param name="bitmapInfo">The native <paramref name="bitmapInfo" /> value.</param>
        /// <param name="usage">The native <paramref name="usage" /> value.</param>
        /// <param name="bits">The native <paramref name="bits" /> value.</param>
        /// <param name="sectionHandle">The native <paramref name="sectionHandle" /> value.</param>
        /// <param name="offset">The native <paramref name="offset" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern SafeDibSectionHandle CreateDIBSection(SafeHandle deviceContext, ref BitmapV5Header bitmapInfo, DibColors usage, out IntPtr bits, IntPtr sectionHandle, uint offset);

        /// <summary>Invokes the native <c>CreateRectRgn</c> entry point.</summary>
        /// <param name="left">The native <paramref name="left" /> value.</param>
        /// <param name="top">The native <paramref name="top" /> value.</param>
        /// <param name="right">The native <paramref name="right" /> value.</param>
        /// <param name="bottom">The native <paramref name="bottom" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern SafeRegionHandle CreateRectRgn(int left, int top, int right, int bottom);

        /// <summary>Invokes the native <c>GetDeviceCaps</c> entry point.</summary>
        /// <param name="deviceContext">The native <paramref name="deviceContext" /> value.</param>
        /// <param name="index">The native <paramref name="index" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetDeviceCaps(SafeHandle deviceContext, DeviceCaps index);

        /// <summary>Invokes the native <c>GetPixel</c> entry point.</summary>
        /// <param name="deviceContext">The native <paramref name="deviceContext" /> value.</param>
        /// <param name="horizontalPosition">The native <paramref name="horizontalPosition" /> value.</param>
        /// <param name="verticalPosition">The native <paramref name="verticalPosition" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern uint GetPixel(SafeHandle deviceContext, int horizontalPosition, int verticalPosition);

        /// <summary>Invokes the native <c>SelectObject</c> entry point.</summary>
        /// <param name="deviceContext">The native <paramref name="deviceContext" /> value.</param>
        /// <param name="objectHandle">The native <paramref name="objectHandle" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr SelectObject(SafeHandle deviceContext, SafeHandle objectHandle);

        /// <summary>Invokes the native <c>StretchBlt</c> entry point.</summary>
        /// <param name="destinationDeviceContext">The native <paramref name="destinationDeviceContext" /> value.</param>
        /// <param name="destinationLeft">The native <paramref name="destinationLeft" /> value.</param>
        /// <param name="destinationTop">The native <paramref name="destinationTop" /> value.</param>
        /// <param name="destinationWidth">The native <paramref name="destinationWidth" /> value.</param>
        /// <param name="destinationHeight">The native <paramref name="destinationHeight" /> value.</param>
        /// <param name="sourceDeviceContext">The native <paramref name="sourceDeviceContext" /> value.</param>
        /// <param name="sourceLeft">The native <paramref name="sourceLeft" /> value.</param>
        /// <param name="sourceTop">The native <paramref name="sourceTop" /> value.</param>
        /// <param name="sourceWidth">The native <paramref name="sourceWidth" /> value.</param>
        /// <param name="sourceHeight">The native <paramref name="sourceHeight" /> value.</param>
        /// <param name="rasterOperation">The native <paramref name="rasterOperation" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool StretchBlt(SafeHandle destinationDeviceContext, int destinationLeft, int destinationTop, int destinationWidth, int destinationHeight, SafeHandle sourceDeviceContext, int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, RasterOperations rasterOperation);

        /// <summary>Invokes the native <c>GetObject</c> entry point.</summary>
        /// <param name="graphicsObject">The native <paramref name="graphicsObject" /> value.</param>
        /// <param name="bufferSize">The native <paramref name="bufferSize" /> value.</param>
        /// <param name="destinationObject">The native <paramref name="destinationObject" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdi32.dll", EntryPoint = "GetObjectW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetObject(SafeHBitmapHandle graphicsObject, int bufferSize, ref GdiBitmap destinationObject);

        /// <summary>Invokes the native <c>DeleteObject</c> entry point.</summary>
        /// <param name="objectHandle">The native <paramref name="objectHandle" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr objectHandle);

        /// <summary>Invokes the native <c>CreateSolidBrush</c> entry point.</summary>
        /// <param name="color">The native <paramref name="color" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr CreateSolidBrush(uint color);

        /// <summary>Invokes the native <c>GetDIBits</c> entry point.</summary>
        /// <param name="deviceContext">The native <paramref name="deviceContext" /> value.</param>
        /// <param name="bitmap">The native <paramref name="bitmap" /> value.</param>
        /// <param name="start">The native <paramref name="start" /> value.</param>
        /// <param name="lineCount">The native <paramref name="lineCount" /> value.</param>
        /// <param name="bits">The native <paramref name="bits" /> value.</param>
        /// <param name="bitmapInfo">The native <paramref name="bitmapInfo" /> value.</param>
        /// <param name="usage">The native <paramref name="usage" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetDIBits(SafeWindowDcHandle deviceContext, SafeHBitmapHandle bitmap, uint start, uint lineCount, IntPtr bits, ref BitmapInfoHeader bitmapInfo, DibColors usage);
    }

    /// <summary>
    ///     The CreateDIBSection function creates a DIB that applications can write to directly.
    ///     The function gives you a pointer to the location of the bitmap bit values.
    ///     You can supply a handle to a file-mapping object that the function will use to create the bitmap, or you can let
    ///     the system allocate the memory for the bitmap.
    ///     See
    ///     <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/dd183494(v=vs.85).aspx">CreateDIBSection function</a>
    /// </summary>
    /// <param name="deviceContextHandle">
    ///     A handle to a device context. If the value of iUsage is DIB_PAL_COLORS, the function uses this device
    ///     context's logical palette to initialize the DIB colors.
    /// </param>
    /// <param name="bmi">
    ///     A pointer to a BITMAPINFO structure that specifies various attributes of the DIB, including the
    ///     bitmap dimensions and colors.
    /// </param>
    /// <param name="usage">
    ///     The type of data contained in the bmiColors array member of the BITMAPINFO structure pointed to by pbmi (either
    ///     logical palette indexes or literal RGB values).
    ///     The following values are defined.
    ///     DIB_PAL_COLORS The bmiColors member is an array of 16-bit indexes into the logical palette of the device context
    ///     specified by deviceContextHandle.
    ///     DIB_RGB_COLORS The BITMAPINFO structure contains an array of literal RGB values.
    /// </param>
    /// <param name="bits">A pointer to a variable that receives a pointer to the location of the DIB bit values.</param>
    /// <param name="sectionHandle">
    ///     A handle to a file-mapping object that the function will use to create the DIB. This parameter can be NULL.
    ///     If hSection is not NULL, it must be a handle to a file-mapping object created by calling the CreateFileMapping
    ///     function with the PAGE_READWRITE or PAGE_WRITECOPY flag. Read-only DIB sections are not supported. Handles created
    ///     by other means will cause CreateDIBSection to fail.
    ///     If hSection is not NULL, the CreateDIBSection function locates the bitmap bit values at offset dwOffset in the
    ///     file-mapping object referred to by hSection. An application can later retrieve the hSection handle by calling the
    ///     GetObject function with the HBITMAP returned by CreateDIBSection.
    ///     If hSection is NULL, the system allocates memory for the DIB. In this case, the CreateDIBSection function ignores
    ///     the dwOffset parameter. An application cannot later obtain a handle to this memory. The dshSection member of the
    ///     DIBSECTION structure filled in by calling the GetObject function will be NULL.
    /// </param>
    /// <param name="offset">
    ///     The offset from the beginning of the file-mapping object referenced by hSection where storage
    ///     for the bitmap bit values is to begin. This value is ignored if hSection is NULL. The bitmap bit values are aligned
    ///     on doubleword boundaries, so dwOffset must be a multiple of the size of a DWORD.
    /// </param>
    /// <returns>
    ///     If the function succeeds, the return value is a handle to the newly created DIB, and *ppvBits points to the bitmap
    ///     bit values.
    ///     If the function fails, the return value is NULL, and *ppvBits is NULL.
    /// </returns>
    public static SafeDibSectionHandle CreateDIBSection(SafeHandle deviceContextHandle, ref BitmapV5Header bmi, DibColors usage, out IntPtr bits, IntPtr sectionHandle, uint offset) => NativeMethods.CreateDIBSection(deviceContextHandle, ref bmi, usage, out bits, sectionHandle, offset);

    /// <summary>
    ///     The CreateRectRgn function creates a rectangular region.
    ///     See
    ///     <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/dd183514(v=vs.85).aspx">CreateRectRgn function</a>
    /// </summary>
    /// <param name="left">Specifies the x-coordinate of the upper-left corner of the region in logical units.</param>
    /// <param name="top">Specifies the y-coordinate of the upper-left corner of the region in logical units.</param>
    /// <param name="right">Specifies the x-coordinate of the lower-right corner of the region in logical units.</param>
    /// <param name="bottom">Specifies the y-coordinate of the lower-right corner of the region in logical units.</param>
    /// <returns>
    ///     If the function succeeds, the return value is the handle to the region.
    ///     If the function fails, the return value is NULL.
    /// </returns>
    public static SafeRegionHandle CreateRectRgn(int left, int top, int right, int bottom) => NativeMethods.CreateRectRgn(left, top, right, bottom);

    /// <summary>
    ///     See
    ///     <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/dd144877(v=vs.85).aspx">GetDeviceCaps function</a>
    ///     The GetDeviceCaps function retrieves device-specific information for the specified device.
    /// </summary>
    /// <param name="deviceContextHandle">A handle to the DC.</param>
    /// <param name="index">The item to be returned.</param>
    /// <returns>The requested device capability value.</returns>
    public static int GetDeviceCaps(SafeHandle deviceContextHandle, DeviceCaps index) => NativeMethods.GetDeviceCaps(deviceContextHandle, index);

    /// <summary>
    ///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/dd144909(v=vs.85).aspx">GetPixel function</a>
    ///     The GetPixel function retrieves the red, green, blue (RGB) color value of the pixel at the specified coordinates.
    /// </summary>
    /// <param name="deviceContextHandle">A handle to the device context.</param>
    /// <param name="horizontalPosition">The x-coordinate, in logical units, of the pixel to be examined.</param>
    /// <param name="verticalPosition">The y-coordinate, in logical units, of the pixel to be examined.</param>
    /// <returns>
    ///     The return value is the COLORREF value that specifies the RGB of the pixel. If the pixel is outside of the
    ///     current clipping region, the return value is CLR_INVALID (0xFFFFFFFF defined in Wingdi.h).
    /// </returns>
    public static uint GetPixel(SafeHandle deviceContextHandle, int horizontalPosition, int verticalPosition) => NativeMethods.GetPixel(deviceContextHandle, horizontalPosition, verticalPosition);

    /// <summary>
    ///     The SelectObject function selects an object into the specified device context (DC). The new object replaces the
    ///     previous object of the same type.
    /// </summary>
    /// <param name="deviceContext">A handle to the DC.</param>
    /// <param name="objectHandle">
    ///     A handle to the object to be selected. The specified object must have been created by using one of the following
    ///     functions.
    ///     Object    Functions
    ///     Bitmap  CreateBitmap, CreateBitmapIndirect, CreateCompatibleBitmap, CreateDIBitmap, CreateDIBSection
    ///     (Bitmaps can only be selected into memory DC's. A single bitmap cannot be selected into more than one DC at the
    ///     same time.)
    ///     Brush   CreateBrushIndirect, CreateDIBPatternBrush, CreateDIBPatternBrushPt, CreateHatchBrush, CreatePatternBrush,
    ///     CreateSolidBrush
    ///     Font    CreateFont, CreateFontIndirect
    ///     Pen     CreatePen, CreatePenIndirect
    ///     Region  CombineRgn, CreateEllipticRgn, CreateEllipticRgnIndirect, CreatePolygonRgn, CreateRectRgn,
    ///     CreateRectRgnIndirect
    /// </param>
    /// <returns>
    ///     If the selected object is not a region and the function succeeds, the return value is a handle to the object being
    ///     replaced.
    ///     If the selected object is a region and the function succeeds, the return value is one of the following values.
    ///     SIMPLEREGION    Region consists of a single rectangle.
    ///     COMPLEXREGION    Region consists of more than one rectangle.
    ///     NULLREGION    Region is empty.
    /// </returns>
    public static SafeNonDisposableObjectHandle SelectObject(SafeDcHandle deviceContext, SafeObjectHandle objectHandle) => new(SelectObjectHandle(deviceContext, objectHandle));

    /// <summary>Performs a stretch transfer from the source device context into the destination device context.</summary>
    /// <param name="destinationDeviceContext">A handle to the destination device context.</param>
    /// <param name="destination">The destination rectangle.</param>
    /// <param name="sourceDeviceContext">A handle to the source device context.</param>
    /// <param name="source">The source rectangle.</param>
    /// <param name="rasterOperation">The raster operation to perform.</param>
    /// <returns>True if the stretch transfer succeeds.</returns>
    public static bool StretchBlt(SafeHandle destinationDeviceContext, Rectangle destination, SafeHandle sourceDeviceContext, Rectangle source, RasterOperations rasterOperation) => NativeMethods.StretchBlt(destinationDeviceContext, destination.Left, destination.Top, destination.Width, destination.Height, sourceDeviceContext, source.Left, source.Top, source.Width, source.Height, rasterOperation);

    /// <summary>Retrieves information about a specified graphics object, such as a bitmap, and copies it into a provided buffer.</summary>
    /// <remarks>This method can be used to obtain details about various types of graphics objects, such as
    /// bitmaps. Ensure that the buffer size specified in bufferSize is sufficient for the object type. If the function
    /// fails, call GetLastError to obtain extended error information.</remarks>
    /// <param name="hgdiobj">A handle to the graphics object for which information is to be retrieved. This must be a valid handle to an
    /// object created by a GDI function.</param>
    /// <param name="bufferSize">The size, in bytes, of the buffer that receives the information. The buffer must be large enough to hold the
    /// data for the object type being queried.</param>
    /// <param name="lpvObject">A reference to a Bitmap structure that receives the information about the graphics object. The structure is
    /// populated with data if the call succeeds.</param>
    /// <returns>The number of bytes copied to the buffer if successful; otherwise, zero if the function fails.</returns>
    public static int GetObject(SafeHBitmapHandle hgdiobj, int bufferSize, ref GdiBitmap lpvObject) => NativeMethods.GetObject(hgdiobj, bufferSize, ref lpvObject);

    /// <summary>Deletes a logical pen, brush, font, bitmap, region, or palette and frees its system resources.</summary>
    /// <param name="objectHandle">A handle to a logical pen, brush, font, bitmap, region, or palette.</param>
    /// <returns>True if the object is deleted.</returns>
    public static bool DeleteObject(IntPtr objectHandle) => NativeMethods.DeleteObject(objectHandle);

    /// <summary>Creates a logical brush with a solid color for use in GDI drawing operations.</summary>
    /// <param name="color">The color of the brush, specified as a COLORREF value. The low-order byte contains the red component, the next
    /// byte contains the green component, and the third byte contains the blue component.</param>
    /// <returns>A handle to the created logical brush, or IntPtr.Zero if the function fails.</returns>
    public static IntPtr CreateSolidBrush(uint color) => NativeMethods.CreateSolidBrush(color);

    /// <summary>
    /// Retrieves the bits of a device-independent bitmap (DIB) and copies them into a buffer, using the specified device context and bitmap handle.
    /// </summary>
    /// <remarks>This method is a P/Invoke wrapper for the native GDI GetDIBits function. The caller is
    /// responsible for ensuring that the buffer pointed to by lpvBits is large enough to hold the requested bitmap
    /// data. If lpvBits is null, the function fills the bmi structure with information about the bitmap without copying
    /// any bits.</remarks>
    /// <param name="deviceContextHandle">A handle to the device context used for the operation. This must be compatible with the bitmap specified by the
    /// hbm parameter.</param>
    /// <param name="hbm">A handle to the bitmap whose bits are to be retrieved.</param>
    /// <param name="start">The starting scan line index, zero-based, from which to begin retrieving bitmap data.</param>
    /// <param name="lineCount">The number of scan lines to retrieve from the bitmap, beginning at the start parameter.</param>
    /// <param name="lpvBits">A pointer to the buffer that receives the bitmap bits. If this parameter is null, the function fills the bmi
    /// structure with information about the bitmap.</param>
    /// <param name="bmi">A reference to a BitmapInfoHeader structure that specifies the desired format for the DIB and receives
    /// information about the bitmap.</param>
    /// <param name="usage">Specifies whether the bmi colors are provided as a color table or as direct RGB values. Typically set to
    /// DIB_RGB_COLORS or DIB_PAL_COLORS.</param>
    /// <returns>The number of scan lines copied into the buffer, or zero if the operation fails.</returns>
    public static int GetDIBits(SafeWindowDcHandle deviceContextHandle, SafeHBitmapHandle hbm, uint start, uint lineCount, IntPtr lpvBits, ref BitmapInfoHeader bmi, DibColors usage) => NativeMethods.GetDIBits(deviceContextHandle, hbm, start, lineCount, lpvBits, ref bmi, usage);

    /// <summary>Selects a GDI object through the single native entry-point owner.</summary>
    /// <param name="deviceContext">The device context in which to select the object.</param>
    /// <param name="objectHandle">The GDI object to select.</param>
    /// <returns>The previously selected object's handle.</returns>
    internal static IntPtr SelectObjectHandle(SafeHandle deviceContext, SafeHandle objectHandle) => NativeMethods.SelectObject(deviceContext, objectHandle);

    /// <summary>Restores a previously selected GDI object through the single native entry-point owner.</summary>
    /// <param name="deviceContext">The device context in which to restore the object.</param>
    /// <param name="objectHandle">The previous GDI object handle.</param>
    /// <returns>The replaced object's handle.</returns>
    internal static IntPtr RestoreObjectHandle(SafeHandle deviceContext, IntPtr objectHandle)
    {
        using SafeNonDisposableObjectHandle safeObjectHandle = new(objectHandle);
        return NativeMethods.SelectObject(deviceContext, safeObjectHandle);
    }
}
