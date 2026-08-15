// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using Microsoft.Win32;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif
/// <summary>Helper methods for using cursor information.</summary>
public static class CursorHelper
{
    /// <summary>The byte offset of the alpha channel in BGRA pixels.</summary>
    private const int AlphaByteOffset = 3;

    /// <summary>The default cursor size in pixels.</summary>
    private const int DefaultCursorSize = 32;

    /// <summary>The base DPI used for scaling calculations.</summary>
    private const int DpiScaleBase = 96;

    /// <summary>The alpha value for a fully opaque pixel.</summary>
    private const int FullyOpaqueAlpha = 255;

    /// <summary>The divisor for monochrome mask bitmap height.</summary>
    private const int MonochromeMaskHeightDivisor = 2;

    /// <summary>The byte count in one BGRA pixel.</summary>
    private const int PixelByteCount = 4;

    /// <summary>The bit count in a 32-bit pixel.</summary>
    private const int ThirtyTwoBitPixel = 32;

    /// <summary>Reads the configured cursor base size.</summary>
    private static Func<int?> _cursorBaseSizeProvider = ReadConfiguredCursorBaseSize;

    /// <summary>Retrieves the current cursor information.</summary>
    private static CursorInfoProvider _cursorInfoProvider = User32Api.GetCursorInfo;

    /// <summary>Retrieves extended cursor icon information.</summary>
    private static CursorIconInfoProvider _cursorIconInfoProvider = NativeIconMethods.GetIconInfoEx;

    /// <summary>Captures the current cursor layers.</summary>
    private static CursorCaptureOperation _cursorCaptureOperation = CaptureCurrentCursor;

    /// <summary>Renders native cursor handles into bitmaps.</summary>
    private static CursorBitmapRenderer _cursorBitmapRenderer = BitmapFromHIcon;

    /// <summary>Creates mask layers for alpha-less cursor color bitmaps.</summary>
    private static CursorMaskLayerProvider _cursorMaskLayerProvider = CreateCursorMaskLayer;

    /// <summary>Gets the base size of the mouse cursor, in pixels, as configured by the user in the system settings.</summary>
    /// <remarks>This method reads the 'CursorBaseSize' value from the Windows Registry under 'Control
    /// Panel\Cursors'. If the value is not found or an error occurs while accessing the registry, a default size of 32
    /// pixels is returned.</remarks>
    /// <returns>The size of the mouse cursor in pixels. Returns 32 if the value cannot be retrieved from the system settings.</returns>
    public static int GetCursorBaseSize()
    {
        try
        {
            int? num = _cursorBaseSizeProvider();
            if (num.HasValue)
            {
                return num.GetValueOrDefault();
            }
        }
        catch (SystemException)
        {
        }

        return DefaultCursorSize;
    }

    /// <summary>Attempts to retrieve information about the current cursor and capture its visual and positional properties.</summary>
    /// <remarks>This method captures both system and custom cursors, ensuring the cursor is visible before
    /// extracting its details. For system cursors, it attempts to load a high-DPI version to improve image quality. The
    /// captured information includes the cursor's size, hotspot, and image layers, which can be used for further
    /// processing or display.</remarks>
    /// <param name="result">When this method returns, contains a CapturedCursor instance populated with details about the current cursor,
    /// including its size, hotspot, and image layers. This parameter is passed uninitialized.</param>
    /// <returns>true if the current cursor information was successfully retrieved and captured; otherwise, false.</returns>
    public static bool TryGetCurrentCursor(out CapturedCursor result)
    {
        result = new();
        if (!TryGetVisibleCursor(out var cursorHandle))
        {
            return false;
        }

        IconInfoEx iconInfo = IconInfoEx.Create();
        if (!_cursorIconInfoProvider(cursorHandle, ref iconInfo))
        {
            return false;
        }

        try
        {
            _cursorCaptureOperation(result, cursorHandle, in iconInfo);
        }
        finally
        {
            iconInfo.Dispose();
        }

        return true;
    }

    /// <summary>Extracts a 32-bit color bitmap from a native handle and determines whether the bitmap contains an alpha channel.</summary>
    /// <remarks>The returned bitmap uses premultiplied alpha format to prevent color distortion. If the
    /// source bitmap does not contain an alpha channel, the method forces all pixels to be fully opaque.</remarks>
    /// <param name="colorBitmapHandle">A handle to the native color bitmap to extract. Must not be zero.</param>
    /// <param name="width">The width, in pixels, of the bitmap to extract. Must be greater than zero.</param>
    /// <param name="height">The height, in pixels, of the bitmap to extract. Must be greater than zero.</param>
    /// <param name="hasAlpha">When the method returns, contains a value indicating whether the extracted bitmap includes an alpha channel.</param>
    /// <returns>A 32-bit color bitmap representing the extracted image, or null if extraction fails or the parameters are
    /// invalid.</returns>
    public static Bitmap ExtractRawColorBitmap(SafeHBitmapHandle colorBitmapHandle, int width, int height, out bool hasAlpha)
    {
        hasAlpha = false;
        if (colorBitmapHandle.IsInvalid || width <= 0 || height <= 0)
        {
            return null;
        }

        Bitmap bmp = new(width, height, PixelFormat.Format32bppPArgb);
        BitmapData data = bmp.LockBits(new(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
        checked
        {
            BitmapInfoHeader bitmapInfoHeader = BitmapInfoHeader.Create(width, -height, ThirtyTwoBitPixel);
            bitmapInfoHeader.SizeImage = 0U;
            using SafeWindowDcHandle deviceContextHandle = SafeWindowDcHandle.FromWindowClientArea(IntPtr.Zero);
            int copiedScanLines = Gdi32Api.GetDIBits(deviceContextHandle, colorBitmapHandle, 0U, (uint)height, data.Scan0, ref bitmapInfoHeader, DibColors.RgbColors);
            return CreateRawColorBitmap(copiedScanLines, bmp, data, width, height, out hasAlpha);
        }
    }

    /// <summary>Creates a bitmap image from the specified Windows icon handle at the given size.</summary>
    /// <remarks>The resulting bitmap is initialized with a transparent background before the icon is drawn.
    /// Ensure that the provided icon handle is valid and that the size parameter is appropriate for the intended
    /// use.</remarks>
    /// <param name="iconHandle">A handle to the icon to convert. This parameter must not be zero or invalid.</param>
    /// <param name="width">The width, in pixels, of the resulting bitmap. Must be a positive integer.</param>
    /// <param name="height">The height, in pixels, of the resulting bitmap. Must be a positive integer.</param>
    /// <returns>A bitmap object that represents the icon handle rendered at the specified size.</returns>
    public static Bitmap BitmapFromHIcon(IntPtr iconHandle, int width, int height) => BitmapFromHIcon(iconHandle, width, height, DrawIconExFlags.DI_NORMAL, PixelFormat.Undefined);

    /// <summary>Creates a bitmap image from the specified Windows icon handle at the given size.</summary>
    /// <param name="iconHandle">A handle to the icon to convert. This parameter must not be zero or invalid.</param>
    /// <param name="width">The width, in pixels, of the resulting bitmap. Must be a positive integer.</param>
    /// <param name="height">The height, in pixels, of the resulting bitmap. Must be a positive integer.</param>
    /// <param name="flags">The draw flags.</param>
    /// <returns>A bitmap object that represents the icon handle rendered at the specified size.</returns>
    public static Bitmap BitmapFromHIcon(IntPtr iconHandle, int width, int height, DrawIconExFlags flags) => BitmapFromHIcon(iconHandle, width, height, flags, PixelFormat.Undefined);

    /// <summary>Creates a bitmap image from the specified Windows icon handle at the given size.</summary>
    /// <param name="iconHandle">A handle to the icon to convert. This parameter must not be zero or invalid.</param>
    /// <param name="width">The width, in pixels, of the resulting bitmap. Must be a positive integer.</param>
    /// <param name="height">The height, in pixels, of the resulting bitmap. Must be a positive integer.</param>
    /// <param name="flags">The draw flags.</param>
    /// <param name="pixelFormat">The target pixel format.</param>
    /// <returns>A bitmap object that represents the icon handle rendered at the specified size.</returns>
    public static Bitmap BitmapFromHIcon(IntPtr iconHandle, int width, int height, DrawIconExFlags flags, PixelFormat pixelFormat)
    {
        PixelFormat format = GetIconBitmapPixelFormat(flags, pixelFormat);
        Bitmap bmp = new(width, height, format);
        using Graphics g = Graphics.FromImage(bmp);
        if (format == PixelFormat.Format24bppRgb)
        {
            if (flags == DrawIconExFlags.DI_MASK)
            {
                g.Clear(Color.White);
            }
            else
            {
                g.Clear(Color.Black);
            }
        }
        else
        {
            g.Clear(Color.Transparent);
        }

        _ = NativeIconMethods.DrawIconEx(new(
            g.GetHdc(),
            (Left: 0, Top: 0),
            iconHandle,
            (Width: width, Height: height),
            0,
            IntPtr.Zero,
            flags));
        g.ReleaseHdc();
        return bmp;
    }

    /// <summary>Determines whether the specified module name corresponds to a known system cursor provider.</summary>
    /// <remarks>This method checks for common Windows system cursor sources, such as 'user32.dll', the
    /// Windows cursors directory, and 'main.cpl' (mouse settings).</remarks>
    /// <param name="moduleName">The name of the module to check. This parameter cannot be null or empty.</param>
    /// <returns>true if the module name is associated with a standard Windows system cursor provider; otherwise, false.</returns>
    public static bool IsSystemCursor(string moduleName)
    {
        if (string.IsNullOrEmpty(moduleName))
        {
            return false;
        }

        string lower = moduleName.ToLowerInvariant();
        return lower.Contains("user32")
            || lower.Contains("\\windows\\cursors\\")
            || lower.Contains("main.cpl");
    }

    /// <summary>
    /// Draws the specified cursor image onto the provided graphics context at the given position, applying appropriate
    /// blending techniques based on the cursor type.
    /// </summary>
    /// <remarks>This method supports both modern system cursors and legacy XOR/mask cursors, utilizing
    /// different drawing strategies based on the cursor's properties. It handles transparency and blending
    /// appropriately for each case.</remarks>
    /// <param name="targetGraphics">The graphics context where the cursor will be drawn. This must not be null.</param>
    /// <param name="cursor">The cursor to be drawn, represented as a CapturedCursor containing the color and mask layers. This must not be
    /// null, and the ColorLayer must be available.</param>
    /// <param name="position">The position on the graphics context where the cursor will be drawn, specified as a NativePoint. The cursor will
    /// be offset by its hot spot.</param>
    public static void DrawCursorOnGraphics(Graphics targetGraphics, CapturedCursor cursor, NativePoint position) => DrawCursorOnGraphics(targetGraphics, cursor, position, default(NativeSize));

    /// <summary>
    /// Draws the specified cursor image onto the provided graphics context at the given position, applying appropriate
    /// blending techniques based on the cursor type.
    /// </summary>
    /// <param name="targetGraphics">The graphics context where the cursor will be drawn. This must not be null.</param>
    /// <param name="cursor">The cursor to be drawn.</param>
    /// <param name="position">The position on the graphics context where the cursor will be drawn.</param>
    /// <param name="destinationSize">The destination size.</param>
    public static void DrawCursorOnGraphics(Graphics targetGraphics, CapturedCursor cursor, NativePoint position, NativeSize destinationSize)
    {
        if (cursor is null || cursor.ColorLayer is null)
        {
            return;
        }

        int x = position.X;
        int y = position.Y;
        int sourceWidth = cursor.Size.Width;
        int sourceHeight = cursor.Size.Height;
        if (destinationSize.IsEmpty)
        {
            destinationSize = new(sourceWidth, sourceHeight);
        }

        if (cursor.MaskLayer is null)
        {
            GraphicsState state = targetGraphics.Save();
            targetGraphics.SmoothingMode = SmoothingMode.HighQuality;
            targetGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            targetGraphics.CompositingQuality = CompositingQuality.HighQuality;
            targetGraphics.PixelOffsetMode = PixelOffsetMode.Half;
            using ImageAttributes wrapMode = new();
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            targetGraphics.DrawImage(
                image: cursor.ColorLayer,
                destRect: new(x, y, destinationSize.Width, destinationSize.Height),
                srcX: 0,
                srcY: 0,
                srcWidth: sourceWidth,
                srcHeight: sourceWidth,
                srcUnit: GraphicsUnit.Pixel,
                imageAttr: wrapMode);

            targetGraphics.Restore(state);
            return;
        }

        Point[] pts = [new(position.X, position.Y)];
        targetGraphics.TransformPoints(CoordinateSpace.Device, CoordinateSpace.World, pts);
        position = new(pts[0].X, pts[0].Y);
        using SafeGraphicsDcHandle hdcDest = SafeGraphicsDcHandle.FromGraphics(targetGraphics);
        using SafeCompatibleDcHandle hdcSrc = Gdi32Api.CreateCompatibleDC(hdcDest);
        using SafeHBitmapHandle hbmMask = new(cursor.MaskLayer.GetHbitmap());
        SafeNonDisposableObjectHandle hbmOld = Gdi32Api.SelectObject(hdcSrc, hbmMask);
        ThrowIfObjectSelectionFailed(hbmOld.IsInvalid, Marshal.GetLastWin32Error());

        ThrowIfRasterOperationFailed(
            Gdi32Api.StretchBlt(hdcDest, new(x, y, destinationSize.Width, destinationSize.Height), hdcSrc, new(0, 0, sourceWidth, sourceHeight), RasterOperations.SourceAnd),
            Marshal.GetLastWin32Error());

        using SafeHBitmapHandle hbmColor = new(cursor.ColorLayer.GetHbitmap());
        _ = Gdi32Api.SelectObject(hdcSrc, hbmColor);
        ThrowIfRasterOperationFailed(
            Gdi32Api.StretchBlt(hdcDest, new(x, y, destinationSize.Width, destinationSize.Height), hdcSrc, new(0, 0, sourceWidth, sourceHeight), RasterOperations.SourceInvert),
            Marshal.GetLastWin32Error());

        _ = Gdi32Api.SelectObject(hdcSrc, hbmOld);
    }

    /// <summary>
    /// Draws a captured cursor onto a bitmap at the specified position using pixel-level bitmap operations.
    /// This method is more reliable than DrawCursorOnGraphics when working directly with Bitmap objects.
    /// </summary>
    /// <remarks>
    /// This method uses typed Span-based pixel access (Bgra32/Bgr24) for efficient and readable bitmap manipulation.
    /// It supports both modern alpha-blended cursors and legacy XOR/mask cursors. For legacy cursors, it properly
    /// applies the AND mask followed by the XOR operation to achieve the correct visual effect.
    /// <example>
    /// Basic usage:
    /// <code>
    /// // Capture the current cursor
    /// if (CursorHelper.TryGetCurrentCursor(out var cursor))
    /// {
    ///     // Create or use an existing bitmap
    ///     var bitmap = new Bitmap(800, 600, PixelFormat.Format32bppArgb);
    ///     // Draw the cursor at position (100, 100)
    ///     CursorHelper.DrawCursorOnBitmap(bitmap, cursor, new NativePoint(100, 100));
    ///     // Optionally scale the cursor
    ///     CursorHelper.DrawCursorOnBitmap(bitmap, cursor, new NativePoint(200, 200), new NativeSize(64, 64));
    ///     cursor.Dispose();
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    /// <param name="targetBitmap">The bitmap to draw the cursor onto.</param>
    /// <param name="cursor">The captured cursor data to draw.</param>
    /// <param name="position">The position at which to draw the cursor (typically mouse coordinates).</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when targetBitmap or cursor is null.</exception>
    /// <exception cref="T:System.NotSupportedException">Thrown when the target bitmap's pixel format is not supported.</exception>
    public static void DrawCursorOnBitmap(Bitmap targetBitmap, CapturedCursor cursor, NativePoint position) => DrawCursorOnBitmap(targetBitmap, cursor, position, default(NativeSize));

    /// <summary>
    /// Draws a captured cursor onto a bitmap at the specified position using pixel-level bitmap operations.
    /// This method is more reliable than DrawCursorOnGraphics when working directly with Bitmap objects.
    /// </summary>
    /// <param name="targetBitmap">The bitmap to draw the cursor onto.</param>
    /// <param name="cursor">The captured cursor data to draw.</param>
    /// <param name="position">The position at which to draw the cursor.</param>
    /// <param name="destinationSize">The destination size. If empty, uses the cursor's natural size.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when targetBitmap or cursor is null.</exception>
    /// <exception cref="T:System.NotSupportedException">Thrown when the target bitmap's pixel format is not supported.</exception>
    public static void DrawCursorOnBitmap(Bitmap targetBitmap, CapturedCursor cursor, NativePoint position, NativeSize destinationSize)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(targetBitmap);
        if (cursor is not null && cursor.ColorLayer is not null)
        {
            int x = position.X;
            int y = position.Y;
            int sourceWidth = cursor.Size.Width;
            int sourceHeight = cursor.Size.Height;
            if (destinationSize.IsEmpty)
            {
                destinationSize = new(sourceWidth, sourceHeight);
            }

            bool needsScaling = destinationSize.Width != sourceWidth || destinationSize.Height != sourceHeight;
            if (cursor.MaskLayer is null)
            {
                DrawScaledAlphaCursorOnBitmap(targetBitmap, cursor.ColorLayer, destinationSize, x, y, needsScaling);
            }
            else
            {
                DrawScaledMaskedCursorOnBitmap(targetBitmap, cursor, destinationSize, x, y, needsScaling);
            }
        }
    }

    /// <summary>Replaces the cursor base size provider for deterministic tests.</summary>
    /// <param name="provider">The replacement provider.</param>
    /// <returns>The previous provider.</returns>
    internal static Func<int?> SetCursorBaseSizeProviderForTesting(Func<int?> provider)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(provider);
        Func<int?> cursorBaseSizeProvider = _cursorBaseSizeProvider;
        _cursorBaseSizeProvider = provider;
        return cursorBaseSizeProvider;
    }

    /// <summary>Replaces the cursor information provider for deterministic tests.</summary>
    /// <param name="provider">The replacement provider.</param>
    /// <returns>The previous provider.</returns>
    internal static CursorInfoProvider SetCursorInfoProviderForTesting(CursorInfoProvider provider)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(provider);
        CursorInfoProvider cursorInfoProvider = _cursorInfoProvider;
        _cursorInfoProvider = provider;
        return cursorInfoProvider;
    }

    /// <summary>Replaces the cursor icon information provider for deterministic tests.</summary>
    /// <param name="provider">The replacement provider.</param>
    /// <returns>The previous provider.</returns>
    internal static CursorIconInfoProvider SetCursorIconInfoProviderForTesting(CursorIconInfoProvider provider)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(provider);
        CursorIconInfoProvider cursorIconInfoProvider = _cursorIconInfoProvider;
        _cursorIconInfoProvider = provider;
        return cursorIconInfoProvider;
    }

    /// <summary>Replaces the cursor layer capture operation for deterministic tests.</summary>
    /// <param name="operation">The replacement operation.</param>
    /// <returns>The previous operation.</returns>
    internal static CursorCaptureOperation SetCursorCaptureOperationForTesting(CursorCaptureOperation operation)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(operation);
        CursorCaptureOperation cursorCaptureOperation = _cursorCaptureOperation;
        _cursorCaptureOperation = operation;
        return cursorCaptureOperation;
    }

    /// <summary>Replaces the cursor bitmap renderer for deterministic tests.</summary>
    /// <param name="renderer">The replacement renderer.</param>
    /// <returns>The previous renderer.</returns>
    internal static CursorBitmapRenderer SetCursorBitmapRendererForTesting(CursorBitmapRenderer renderer)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(renderer);
        CursorBitmapRenderer cursorBitmapRenderer = _cursorBitmapRenderer;
        _cursorBitmapRenderer = renderer;
        return cursorBitmapRenderer;
    }

    /// <summary>Replaces the cursor mask-layer provider for deterministic tests.</summary>
    /// <param name="provider">The replacement provider.</param>
    /// <returns>The previous provider.</returns>
    internal static CursorMaskLayerProvider SetCursorMaskLayerProviderForTesting(CursorMaskLayerProvider provider)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(provider);
        CursorMaskLayerProvider cursorMaskLayerProvider = _cursorMaskLayerProvider;
        _cursorMaskLayerProvider = provider;
        return cursorMaskLayerProvider;
    }

    /// <summary>Throws when selecting a source bitmap into a graphics context failed.</summary>
    /// <param name="selectionFailed">A value indicating whether the native selection failed.</param>
    /// <param name="errorCode">The native error code to include in the exception.</param>
    internal static void ThrowIfObjectSelectionFailed(bool selectionFailed, int errorCode)
    {
        if (selectionFailed)
        {
            throw new Win32Exception(errorCode);
        }
    }

    /// <summary>Throws when a cursor raster operation failed.</summary>
    /// <param name="operationSucceeded">A value indicating whether the native raster operation succeeded.</param>
    /// <param name="errorCode">The native error code to include in the exception.</param>
    internal static void ThrowIfRasterOperationFailed(bool operationSucceeded, int errorCode)
    {
        if (!operationSucceeded)
        {
            throw new Win32Exception(errorCode);
        }
    }

    /// <summary>Captures cursor layers from already-retrieved native cursor information.</summary>
    /// <param name="result">The captured cursor to populate.</param>
    /// <param name="cursorHandle">The cursor handle.</param>
    /// <param name="iconInfo">The icon information.</param>
    internal static void CaptureCurrentCursor(CapturedCursor result, IntPtr cursorHandle, in IconInfoEx iconInfo)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(result);
        int baseSize = GetCursorBaseSize();
        uint dpi = NativeDpiMethods.GetDpiForSystem();
        int targetWidth = checked((int)((float)baseSize * ((float)dpi / DpiScaleBase)));
        int targetHeight = targetWidth;
        CursorHandleSelection bestCursor = GetBestCursorHandle(in iconInfo, cursorHandle, targetWidth, targetHeight);
        NativeSize nativeSize = GetNativeCursorSize(in iconInfo, bestCursor.IsFresh, targetWidth, targetHeight);
        bool isCustomCursor = nativeSize.Width != DefaultCursorSize || nativeSize.Height != DefaultCursorSize;
        ApplyCursorSize(
            result,
            in iconInfo,
            targetWidth,
            targetHeight,
            nativeSize,
            baseSize > DefaultCursorSize,
            isCustomCursor);
        CaptureCursorLayers(result, in iconInfo, bestCursor.ToIntPtr(), new(targetWidth, targetHeight), nativeSize, isCustomCursor);
        if (bestCursor.IsFresh)
        {
            _ = User32Api.DestroyCursor(bestCursor.ToIntPtr());
        }
    }

    /// <summary>Converts a configured registry value to a cursor base size.</summary>
    /// <param name="value">The registry value.</param>
    /// <returns>The configured size, or <see langword="null" /> when the value is not an integer.</returns>
    internal static int? GetConfiguredCursorBaseSize(object value) => value is int size ? size : null;

    /// <summary>Finalizes a raw cursor bitmap after its native pixels have been copied.</summary>
    /// <param name="copiedScanLines">The number of scan lines copied by the native operation.</param>
    /// <param name="bitmap">The locked destination bitmap.</param>
    /// <param name="bitmapData">The bitmap lock data.</param>
    /// <param name="width">The bitmap width.</param>
    /// <param name="height">The bitmap height.</param>
    /// <param name="hasAlpha">Receives whether any copied pixel has alpha.</param>
    /// <returns>The finalized bitmap, or <see langword="null" /> when no scan lines were copied.</returns>
    internal static unsafe Bitmap CreateRawColorBitmap(
        int copiedScanLines,
        Bitmap bitmap,
        BitmapData bitmapData,
        int width,
        int height,
        out bool hasAlpha)
    {
        hasAlpha = false;
        if (copiedScanLines == 0)
        {
            bitmap.UnlockBits(bitmapData);
            bitmap.Dispose();
            return null;
        }

        byte* pixel = unchecked((byte*)(void*)bitmapData.Scan0);
        int byteCount = checked(width * height * PixelByteCount);
        for (int index = AlphaByteOffset; index < byteCount; index += PixelByteCount)
        {
            if (pixel[index] != 0)
            {
                hasAlpha = true;
                break;
            }
        }

        if (!hasAlpha)
        {
            for (int index = AlphaByteOffset; index < byteCount; index += PixelByteCount)
            {
                pixel[index] = byte.MaxValue;
            }
        }

        bitmap.UnlockBits(bitmapData);
        return bitmap;
    }

    /// <summary>Creates the optional mask layer for a captured cursor.</summary>
    /// <param name="hasAlpha">A value indicating whether the cursor color layer has alpha.</param>
    /// <param name="cursorHandle">The native cursor handle.</param>
    /// <param name="targetSize">The requested mask-layer size.</param>
    /// <returns>The mask layer, or <see langword="null" /> when alpha makes it unnecessary.</returns>
    internal static Bitmap CreateMaskLayer(bool hasAlpha, IntPtr cursorHandle, NativeSize targetSize) =>
        hasAlpha ? null : _cursorMaskLayerProvider(cursorHandle, targetSize.Width, targetSize.Height);

    /// <summary>Gets a cursor bitmap width from a native bitmap-information result.</summary>
    /// <param name="copiedBytes">The byte count returned by the native operation.</param>
    /// <param name="bitmapWidth">The width from the native bitmap information.</param>
    /// <returns>The bitmap width, or the default cursor size when no information was copied.</returns>
    internal static int GetBitmapWidth(int copiedBytes, int bitmapWidth) =>
        copiedBytes <= 0 ? DefaultCursorSize : bitmapWidth;

    /// <summary>Reads the cursor base size configured by Windows.</summary>
    /// <returns>The configured cursor base size, or <see langword="null" /> when unavailable.</returns>
    private static int? ReadConfiguredCursorBaseSize()
    {
        using RegistryKey key = Registry.CurrentUser.OpenSubKey("Control Panel\\Cursors");
        return GetConfiguredCursorBaseSize(RegistryValueReader.GetValue(key, "CursorBaseSize"));
    }

    /// <summary>Draws a scaled alpha cursor on a bitmap.</summary>
    /// <param name="targetBitmap">The target bitmap.</param>
    /// <param name="cursorBitmap">The cursor bitmap.</param>
    /// <param name="destinationSize">The destination size.</param>
    /// <param name="x">The destination x-coordinate.</param>
    /// <param name="y">The destination y-coordinate.</param>
    /// <param name="needsScaling">A value indicating whether scaling is required.</param>
    private static void DrawScaledAlphaCursorOnBitmap(Bitmap targetBitmap, Bitmap cursorBitmap, NativeSize destinationSize, int x, int y, bool needsScaling)
    {
        Bitmap cursorToUse = cursorBitmap;
        if (needsScaling)
        {
            cursorToUse = new(cursorBitmap, destinationSize.Width, destinationSize.Height);
        }

        try
        {
            DrawAlphaCursorOnBitmap(targetBitmap, cursorToUse, x, y);
        }
        finally
        {
            if (needsScaling && cursorToUse != cursorBitmap)
            {
                cursorToUse.Dispose();
            }
        }
    }

    /// <summary>Draws a scaled masked cursor on a bitmap.</summary>
    /// <param name="targetBitmap">The target bitmap.</param>
    /// <param name="cursor">The captured cursor.</param>
    /// <param name="destinationSize">The destination size.</param>
    /// <param name="x">The destination x-coordinate.</param>
    /// <param name="y">The destination y-coordinate.</param>
    /// <param name="needsScaling">A value indicating whether scaling is required.</param>
    private static void DrawScaledMaskedCursorOnBitmap(Bitmap targetBitmap, CapturedCursor cursor, NativeSize destinationSize, int x, int y, bool needsScaling)
    {
        Bitmap scaledColor = cursor.ColorLayer;
        Bitmap scaledMask = cursor.MaskLayer;
        if (needsScaling)
        {
            scaledColor = new(cursor.ColorLayer, destinationSize.Width, destinationSize.Height);
            scaledMask = new(cursor.MaskLayer, destinationSize.Width, destinationSize.Height);
        }

        try
        {
            DrawMaskedCursorOnBitmap(targetBitmap, scaledColor, scaledMask, x, y);
        }
        finally
        {
            if (needsScaling)
            {
                if (scaledColor != cursor.ColorLayer)
                {
                    scaledColor.Dispose();
                }

                if (scaledMask != cursor.MaskLayer)
                {
                    scaledMask.Dispose();
                }
            }
        }
    }

    /// <summary>Applies the captured cursor size and hotspot.</summary>
    /// <param name="result">The captured cursor to update.</param>
    /// <param name="iconInfo">The native icon information.</param>
    /// <param name="targetWidth">The target width.</param>
    /// <param name="targetHeight">The target height.</param>
    /// <param name="nativeSize">The native cursor size.</param>
    /// <param name="isCursorEnlarged">A value indicating whether the cursor is enlarged.</param>
    /// <param name="isCustomCursor">A value indicating whether the cursor is custom-sized.</param>
    private static void ApplyCursorSize(CapturedCursor result, in IconInfoEx iconInfo, int targetWidth, int targetHeight, NativeSize nativeSize, bool isCursorEnlarged, bool isCustomCursor)
    {
        if (!isCursorEnlarged || isCustomCursor)
        {
            result.HotSpot = iconInfo.Hotspot;
            result.Size = nativeSize;
            return;
        }

        int handleWidth = GetMaskWidth(in iconInfo);
        float scale = (float)targetWidth / (float)handleWidth;
        result.HotSpot = checked(new NativePoint((int)((float)iconInfo.Hotspot.X * scale), (int)((float)iconInfo.Hotspot.Y * scale)));
        result.Size = new(targetWidth, targetHeight);
    }

    /// <summary>Captures cursor color and mask layers.</summary>
    /// <param name="result">The captured cursor to update.</param>
    /// <param name="iconInfo">The native icon information.</param>
    /// <param name="cursorHandle">The cursor handle.</param>
    /// <param name="targetSize">The target cursor size.</param>
    /// <param name="nativeSize">The native cursor size.</param>
    /// <param name="isCustomCursor">A value indicating whether the cursor is custom-sized.</param>
    private static void CaptureCursorLayers(CapturedCursor result, in IconInfoEx iconInfo, IntPtr cursorHandle, NativeSize targetSize, NativeSize nativeSize, bool isCustomCursor)
    {
        if (iconInfo.ColorBitmapHandle.IsInvalid)
        {
            result.ColorLayer = BitmapFromHIcon(cursorHandle, targetSize.Width, targetSize.Height, DrawIconExFlags.DI_IMAGE, PixelFormat.Format24bppRgb);
            result.MaskLayer = BitmapFromHIcon(cursorHandle, targetSize.Width, targetSize.Height, DrawIconExFlags.DI_MASK, PixelFormat.Format24bppRgb);
        }
        else
        {
            result.ColorLayer = GetColorCursorLayer(result, in iconInfo, cursorHandle, targetSize, nativeSize, isCustomCursor, out var hasAlpha);
            result.MaskLayer = CreateMaskLayer(hasAlpha, cursorHandle, targetSize);
        }
    }

    /// <summary>Gets the best cursor handle for capture.</summary>
    /// <param name="iconInfo">The native icon information.</param>
    /// <param name="fallbackHandle">The fallback cursor handle.</param>
    /// <param name="targetWidth">The target width.</param>
    /// <param name="targetHeight">The target height.</param>
    /// <returns>The selected cursor handle and ownership flag.</returns>
    private static CursorHandleSelection GetBestCursorHandle(in IconInfoEx iconInfo, IntPtr fallbackHandle, int targetWidth, int targetHeight)
    {
        IntPtr cursorHandle = LoadSystemCursor(in iconInfo, targetWidth, targetHeight);
        return cursorHandle != IntPtr.Zero
            ? new(cursorHandle, isFresh: true)
            : new(fallbackHandle, isFresh: false);
    }

    /// <summary>Gets a color cursor layer.</summary>
    /// <param name="result">The captured cursor to update.</param>
    /// <param name="iconInfo">The native icon information.</param>
    /// <param name="cursorHandle">The cursor handle.</param>
    /// <param name="targetSize">The target cursor size.</param>
    /// <param name="nativeSize">The native cursor size.</param>
    /// <param name="isCustomCursor">A value indicating whether the cursor is custom-sized.</param>
    /// <param name="hasAlpha">A value indicating whether the color layer has alpha.</param>
    /// <returns>The color layer bitmap.</returns>
    private static Bitmap GetColorCursorLayer(CapturedCursor result, in IconInfoEx iconInfo, IntPtr cursorHandle, NativeSize targetSize, NativeSize nativeSize, bool isCustomCursor, out bool hasAlpha)
    {
        if (!isCustomCursor)
        {
            hasAlpha = true;
            return BitmapFromHIcon(cursorHandle, targetSize.Width, targetSize.Height);
        }

        result.HotSpot = iconInfo.Hotspot;
        result.Size = nativeSize;
        return ExtractRawColorBitmap(iconInfo.ColorBitmapHandle, nativeSize.Width, nativeSize.Height, out hasAlpha);
    }

    /// <summary>Creates a cursor mask bitmap through the native icon renderer.</summary>
    /// <param name="cursorHandle">The native cursor handle.</param>
    /// <param name="width">The requested mask width.</param>
    /// <param name="height">The requested mask height.</param>
    /// <returns>The rendered mask bitmap.</returns>
    private static Bitmap CreateCursorMaskLayer(IntPtr cursorHandle, int width, int height) =>
        _cursorBitmapRenderer(cursorHandle, width, height, DrawIconExFlags.DI_MASK);

    /// <summary>Gets the mask bitmap width.</summary>
    /// <param name="iconInfo">The native icon information.</param>
    /// <returns>The mask width.</returns>
    private static int GetMaskWidth(in IconInfoEx iconInfo)
    {
        GdiBitmap maskInfo = default;
        int copiedBytes = Gdi32Api.GetObject(iconInfo.BitmaskBitmapHandle, Marshal.SizeOf<GdiBitmap>(), ref maskInfo);
        return GetBitmapWidth(copiedBytes, maskInfo.Width);
    }

    /// <summary>Gets the native cursor size.</summary>
    /// <param name="iconInfo">The native icon information.</param>
    /// <param name="isFreshHandle">A value indicating whether a fresh handle was loaded.</param>
    /// <param name="targetWidth">The target width.</param>
    /// <param name="targetHeight">The target height.</param>
    /// <returns>The native cursor size.</returns>
    private static NativeSize GetNativeCursorSize(in IconInfoEx iconInfo, bool isFreshHandle, int targetWidth, int targetHeight)
    {
        if (isFreshHandle)
        {
            return new(targetWidth, targetHeight);
        }

        GdiBitmap bitmapInfo = default;
        if (Gdi32Api.GetObject(iconInfo.ColorBitmapHandle.IsInvalid ? iconInfo.BitmaskBitmapHandle : iconInfo.ColorBitmapHandle, Marshal.SizeOf<GdiBitmap>(), ref bitmapInfo) <= 0)
        {
            return new(targetWidth, targetHeight);
        }

        int height = iconInfo.ColorBitmapHandle.IsInvalid
            ? bitmapInfo.Height / MonochromeMaskHeightDivisor
            : bitmapInfo.Height;
        return new(bitmapInfo.Width, height);
    }

    /// <summary>Loads a system cursor at the target size when possible.</summary>
    /// <param name="iconInfo">The native icon information.</param>
    /// <param name="targetWidth">The target width.</param>
    /// <param name="targetHeight">The target height.</param>
    /// <returns>The loaded cursor handle, or zero.</returns>
    private static IntPtr LoadSystemCursor(in IconInfoEx iconInfo, int targetWidth, int targetHeight)
    {
        if (!IsSystemCursor(iconInfo.ModuleName) || string.IsNullOrEmpty(iconInfo.ModuleName))
        {
            return IntPtr.Zero;
        }

        return iconInfo.ResourceId == 0
            ? NativeCursorMethods.LoadImage(
                IntPtr.Zero,
                iconInfo.ModuleName,
                ImageType.IMAGE_CURSOR,
                targetWidth,
                targetHeight,
                LoadImageFlags.LR_LOADFROMFILE)
            : NativeCursorMethods.LoadImage(
                Kernel32Api.GetModuleHandle(iconInfo.ModuleName),
                (IntPtr)iconInfo.ResourceId,
                ImageType.IMAGE_CURSOR,
                targetWidth,
                targetHeight,
                LoadImageFlags.None);
    }

    /// <summary>Gets visible cursor information.</summary>
    /// <param name="cursorHandle">The cursor handle.</param>
    /// <returns><see langword="true" /> when the cursor is visible.</returns>
    private static bool TryGetVisibleCursor(out IntPtr cursorHandle)
    {
        cursorHandle = IntPtr.Zero;
        CursorInfo cursorInfo = CursorInfo.Create();
        if (!_cursorInfoProvider(ref cursorInfo) || !cursorInfo.IsShowing)
        {
            return false;
        }

        cursorHandle = GetCursorHandle(cursorInfo.CursorHandle);
        return true;
    }

    /// <summary>Gets the raw cursor handle from a non-owned cursor reference.</summary>
    /// <param name="cursorHandle">The cursor reference handle.</param>
    /// <returns>The raw cursor handle.</returns>
    private static IntPtr GetCursorHandle(SafeCursorReferenceHandle cursorHandle) =>
        cursorHandle.UseNativeHandle(static nativeCursorHandle => nativeCursorHandle);

    /// <summary>Gets the bitmap pixel format for an icon draw operation.</summary>
    /// <param name="flags">The draw flags.</param>
    /// <param name="pixelFormat">The requested pixel format.</param>
    /// <returns>The resolved pixel format.</returns>
    private static PixelFormat GetIconBitmapPixelFormat(DrawIconExFlags flags, PixelFormat pixelFormat)
    {
        if (pixelFormat != PixelFormat.Undefined)
        {
            return pixelFormat;
        }

        return flags - 1 > DrawIconExFlags.DI_MASK
            ? PixelFormat.Format32bppArgb
            : PixelFormat.Format24bppRgb;
    }

    /// <summary>Gets a value indicating whether the pixel format is 32-bit.</summary>
    /// <param name="format">The pixel format.</param>
    /// <returns><see langword="true" /> when the format is 32-bit.</returns>
    private static bool Is32BitFormat(PixelFormat format) =>
        format is PixelFormat.Format32bppRgb or PixelFormat.Format32bppArgb or PixelFormat.Format32bppPArgb;

    /// <summary>Draws a modern alpha-blended cursor onto a bitmap.</summary>
    /// <param name="targetBitmap">The target bitmap.</param>
    /// <param name="cursorBitmap">The cursor bitmap.</param>
    /// <param name="x">The destination x-coordinate.</param>
    /// <param name="y">The destination y-coordinate.</param>
    private static void DrawAlphaCursorOnBitmap(Bitmap targetBitmap, Bitmap cursorBitmap, int x, int y)
    {
        Bitmap convertedBitmap = null;
        try
        {
            if (cursorBitmap.PixelFormat != PixelFormat.Format32bppArgb && cursorBitmap.PixelFormat != PixelFormat.Format32bppPArgb)
            {
                convertedBitmap = new(cursorBitmap.Width, cursorBitmap.Height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(convertedBitmap))
                {
                    g.DrawImage(cursorBitmap, 0, 0, cursorBitmap.Width, cursorBitmap.Height);
                }

                cursorBitmap = convertedBitmap;
            }

            if (Is32BitFormat(targetBitmap.PixelFormat))
            {
                DrawAlphaCursor<Bgra32>(targetBitmap, cursorBitmap, x, y);
                return;
            }

            if (targetBitmap.PixelFormat == PixelFormat.Format24bppRgb)
            {
                DrawAlphaCursor<Bgr24>(targetBitmap, cursorBitmap, x, y);
                return;
            }

            throw new NotSupportedException($"Target bitmap format {targetBitmap.PixelFormat} is not supported.");
        }
        finally
        {
            convertedBitmap?.Dispose();
        }
    }

    /// <summary>Generic implementation for alpha-blended cursor drawing.</summary>
    /// <typeparam name="TTarget">The target pixel type.</typeparam>
    /// <param name="targetBitmap">The target bitmap.</param>
    /// <param name="cursorBitmap">The cursor bitmap.</param>
    /// <param name="x">The destination x-coordinate.</param>
    /// <param name="y">The destination y-coordinate.</param>
    private static void DrawAlphaCursor<TTarget>(Bitmap targetBitmap, Bitmap cursorBitmap, int x, int y)
        where TTarget : struct
    {
        checked
        {
            using BitmapAccessor<TTarget> targetAccessor = new(targetBitmap, readOnly: false);
            using BitmapAccessor<Bgra32> cursorAccessor = new(cursorBitmap, readOnly: true);
            for (int cy = 0; cy < cursorBitmap.Height; cy++)
            {
                int ty = y + cy;
                if (ty < 0 || ty >= targetAccessor.Height)
                {
                    continue;
                }

                Span<TTarget> targetRow = targetAccessor.GetRowSpan(ty);
                Span<Bgra32> cursorRow = cursorAccessor.GetRowSpan(cy);
                for (int cx = 0; cx < cursorBitmap.Width; cx++)
                {
                    int tx = x + cx;
                    if (tx >= 0 && tx < targetAccessor.Width)
                    {
                        ref Bgra32 cursorPixel = ref cursorRow[cx];
                        if (cursorPixel.A != 0)
                        {
                            BlendAlphaPixel(targetRow, tx, cursorPixel);
                        }
                    }
                }
            }
        }
    }

    /// <summary>Blends one alpha pixel into the target row.</summary>
    /// <typeparam name="TTarget">The target pixel type.</typeparam>
    /// <param name="targetRow">The target row.</param>
    /// <param name="targetIndex">The target pixel index.</param>
    /// <param name="cursorPixel">The cursor pixel.</param>
    private static void BlendAlphaPixel<TTarget>(Span<TTarget> targetRow, int targetIndex, Bgra32 cursorPixel)
        where TTarget : struct
    {
        if (typeof(TTarget) == typeof(Bgra32))
        {
            Bgra32.AlphaBlend(ref Unsafe.As<TTarget, Bgra32>(ref targetRow[targetIndex]), cursorPixel);
        }
        else
        {
            Bgr24.AlphaBlend(ref Unsafe.As<TTarget, Bgr24>(ref targetRow[targetIndex]), cursorPixel);
        }
    }

    /// <summary>Draws a legacy cursor with mask using AND/XOR operations.</summary>
    /// <param name="targetBitmap">The target bitmap.</param>
    /// <param name="colorBitmap">The cursor color bitmap.</param>
    /// <param name="maskBitmap">The cursor mask bitmap.</param>
    /// <param name="x">The destination x-coordinate.</param>
    /// <param name="y">The destination y-coordinate.</param>
    private static void DrawMaskedCursorOnBitmap(Bitmap targetBitmap, Bitmap colorBitmap, Bitmap maskBitmap, int x, int y)
    {
        if (Is32BitFormat(targetBitmap.PixelFormat))
        {
            DrawMaskedCursor<Bgra32>(targetBitmap, colorBitmap, maskBitmap, x, y);
            return;
        }

        if (targetBitmap.PixelFormat == PixelFormat.Format24bppRgb)
        {
            DrawMaskedCursor<Bgr24>(targetBitmap, colorBitmap, maskBitmap, x, y);
            return;
        }

        throw new NotSupportedException($"Target bitmap format {targetBitmap.PixelFormat} is not supported.");
    }

    /// <summary>Generic implementation for masked cursor drawing with AND/XOR operations.</summary>
    /// <typeparam name="TTarget">The target pixel type.</typeparam>
    /// <param name="targetBitmap">The target bitmap.</param>
    /// <param name="colorBitmap">The cursor color bitmap.</param>
    /// <param name="maskBitmap">The cursor mask bitmap.</param>
    /// <param name="x">The destination x-coordinate.</param>
    /// <param name="y">The destination y-coordinate.</param>
    private static void DrawMaskedCursor<TTarget>(Bitmap targetBitmap, Bitmap colorBitmap, Bitmap maskBitmap, int x, int y)
        where TTarget : struct
    {
        using BitmapAccessor<TTarget> targetAccessor = new(targetBitmap, readOnly: false);
        if (Is32BitFormat(colorBitmap.PixelFormat))
        {
            using BitmapAccessor<Bgra32> colorAccessor = new(colorBitmap, readOnly: true);
            using BitmapAccessor<Bgra32> maskAccessor = new(maskBitmap, readOnly: true);
            ApplyMask(targetAccessor, colorAccessor, maskAccessor, x, y);
            return;
        }

        using BitmapAccessor<Bgr24> colorAccessor2 = new(colorBitmap, readOnly: true);
        using BitmapAccessor<Bgr24> maskAccessor2 = new(maskBitmap, readOnly: true);
        ApplyMask(targetAccessor, colorAccessor2, maskAccessor2, x, y);
    }

    /// <summary>Applies mask using AND/XOR operations.</summary>
    /// <typeparam name="TTarget">The target pixel type.</typeparam>
    /// <typeparam name="TSource">The source pixel type.</typeparam>
    /// <param name="targetAccessor">The target bitmap accessor.</param>
    /// <param name="colorAccessor">The color bitmap accessor.</param>
    /// <param name="maskAccessor">The mask bitmap accessor.</param>
    /// <param name="x">The destination x-coordinate.</param>
    /// <param name="y">The destination y-coordinate.</param>
    private static void ApplyMask<TTarget, TSource>(BitmapAccessor<TTarget> targetAccessor, BitmapAccessor<TSource> colorAccessor, BitmapAccessor<TSource> maskAccessor, int x, int y)
        where TTarget : struct
        where TSource : struct
    {
        checked
        {
            for (int cy = 0; cy < colorAccessor.Height; cy++)
            {
                int ty = y + cy;
                if (ty < 0 || ty >= targetAccessor.Height)
                {
                    continue;
                }

                Span<TTarget> targetRow = targetAccessor.GetRowSpan(ty);
                Span<TSource> colorRow = colorAccessor.GetRowSpan(cy);
                Span<TSource> maskRow = maskAccessor.GetRowSpan(cy);
                for (int cx = 0; cx < colorAccessor.Width; cx++)
                {
                    int tx = x + cx;
                    if (tx >= 0 && tx < targetAccessor.Width)
                    {
                        MaskColor sourcePixel = GetMaskColor(colorRow, maskRow, cx);
                        ApplyMaskToTarget(targetRow, tx, sourcePixel);
                    }
                }
            }
        }
    }

    /// <summary>Applies a mask/color pixel to the target row.</summary>
    /// <typeparam name="TTarget">The target pixel type.</typeparam>
    /// <param name="targetRow">The target row.</param>
    /// <param name="targetIndex">The target index.</param>
    /// <param name="sourcePixel">The source mask/color pixel.</param>
    private static void ApplyMaskToTarget<TTarget>(Span<TTarget> targetRow, int targetIndex, MaskColor sourcePixel)
        where TTarget : struct
    {
        checked
        {
            if (typeof(TTarget) == typeof(Bgra32))
            {
                ref Bgra32 target = ref Unsafe.As<TTarget, Bgra32>(ref targetRow[targetIndex]);
                target = new(
                    (byte)((target.R & sourcePixel.Mask) ^ sourcePixel.Red),
                    (byte)((target.G & sourcePixel.Mask) ^ sourcePixel.Green),
                    (byte)((target.B & sourcePixel.Mask) ^ sourcePixel.Blue));
            }
            else
            {
                ref Bgr24 bgrTarget = ref Unsafe.As<TTarget, Bgr24>(ref targetRow[targetIndex]);
                bgrTarget = new(
                    (byte)((bgrTarget.R & sourcePixel.Mask) ^ sourcePixel.Red),
                    (byte)((bgrTarget.G & sourcePixel.Mask) ^ sourcePixel.Green),
                    (byte)((bgrTarget.B & sourcePixel.Mask) ^ sourcePixel.Blue));
            }
        }
    }

    /// <summary>Gets a mask/color pixel from source rows.</summary>
    /// <typeparam name="TSource">The source pixel type.</typeparam>
    /// <param name="colorRow">The color row.</param>
    /// <param name="maskRow">The mask row.</param>
    /// <param name="sourceIndex">The source index.</param>
    /// <returns>The mask/color pixel.</returns>
    private static MaskColor GetMaskColor<TSource>(ReadOnlySpan<TSource> colorRow, ReadOnlySpan<TSource> maskRow, int sourceIndex)
        where TSource : struct
    {
        if (typeof(TSource) == typeof(Bgra32))
        {
            ref Bgra32 reference = ref Unsafe.As<TSource, Bgra32>(ref Unsafe.AsRef(in maskRow[sourceIndex]));
            ref Bgra32 color = ref Unsafe.As<TSource, Bgra32>(ref Unsafe.AsRef(in colorRow[sourceIndex]));
            return new(reference.B, color.R, color.G, color.B);
        }

        ref Bgr24 reference2 = ref Unsafe.As<TSource, Bgr24>(ref Unsafe.AsRef(in maskRow[sourceIndex]));
        ref Bgr24 bgrColor = ref Unsafe.As<TSource, Bgr24>(ref Unsafe.AsRef(in colorRow[sourceIndex]));
        return new(reference2.B, bgrColor.R, bgrColor.G, bgrColor.B);
    }

    /// <summary>Stores a source mask/color pixel.</summary>
    /// <param name="Mask">The mask value.</param>
    /// <param name="Red">The red component.</param>
    /// <param name="Green">The green component.</param>
    /// <param name="Blue">The blue component.</param>
    private readonly record struct MaskColor(byte Mask, byte Red, byte Green, byte Blue);

    /// <summary>Stores the selected cursor handle and whether this helper owns it.</summary>
    private readonly record struct CursorHandleSelection
    {
        /// <summary>The selected cursor handle.</summary>
        private readonly IntPtr _handle;

        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.CursorHelper.CursorHandleSelection" /> struct.</summary>
        /// <param name="handle">The selected cursor handle.</param>
        /// <param name="isFresh">A value indicating whether this helper owns the handle.</param>
        public CursorHandleSelection(IntPtr handle, bool isFresh)
        {
            _handle = handle;
            IsFresh = isFresh;
        }

        /// <summary>Gets a value indicating whether this helper owns the handle.</summary>
        public bool IsFresh { get; }

        /// <summary>Gets the native cursor handle.</summary>
        /// <returns>The native cursor handle.</returns>
        public IntPtr ToIntPtr() => _handle;
    }
}
