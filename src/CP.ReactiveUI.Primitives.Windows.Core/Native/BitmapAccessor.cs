// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

namespace CP.ReactiveUI.Primitives.Windows.Native;

/// <summary>
/// Provides pixel-level access to a bitmap using typed pixel structs for efficient row-based operations.
/// This is a shim that mimics ImageSharp's ProcessPixelRows API for easier migration.
/// See BitmapAccessor.md for detailed documentation and examples.
/// </summary>
/// <typeparam name="TPixel">The pixel type (Bgra32 for 32-bit, Bgr24 for 24-bit, Indexed8 for 8-bit indexed).</typeparam>
public sealed class BitmapAccessor<TPixel> : IDisposable
    where TPixel : struct
{
    /// <summary>The zero coordinate used when locking the full bitmap.</summary>
    private const int ZeroCoordinate = 0;

    /// <summary>The bitmap that owns the locked data.</summary>
    private readonly Bitmap _bitmap;

    /// <summary>The locked bitmap data.</summary>
    private readonly BitmapData _bitmapData;

    /// <summary>Indicates whether the accessor is read-only.</summary>
    private readonly bool _readOnly;

    /// <summary>The cached indexed palette.</summary>
    private Bgra32[] _paletteCache = [];

    /// <summary>Initializes a new instance of the BitmapAccessor class.</summary>
    /// <param name="bitmap">The bitmap to access.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when bitmap is null.</exception>
    /// <exception cref="T:System.NotSupportedException">Thrown when the bitmap's pixel format doesn't match TPixel.</exception>
    public BitmapAccessor(Bitmap bitmap)
        : this(bitmap, false) { }

    /// <summary>Initializes a new instance of the BitmapAccessor class.</summary>
    /// <param name="bitmap">The bitmap to access.</param>
    /// <param name="readOnly">If true, the bitmap is locked for read-only access; otherwise, it's locked for read-write access.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when bitmap is null.</exception>
    /// <exception cref="T:System.NotSupportedException">Thrown when the bitmap's pixel format doesn't match TPixel.</exception>
    public BitmapAccessor(Bitmap bitmap, bool readOnly)
    {
        _bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
        _readOnly = readOnly;
        PixelFormatValidator.Validate(bitmap.PixelFormat);
        ImageLockMode flags = (readOnly ? ImageLockMode.ReadOnly : ImageLockMode.ReadWrite);
        Rectangle rect = new(ZeroCoordinate, ZeroCoordinate, bitmap.Width, bitmap.Height);
        _bitmapData = bitmap.LockBits(rect, flags, bitmap.PixelFormat);
        if (typeof(TPixel) == typeof(Indexed8))
        {
            CachePalette();
        }
    }

    /// <summary>Delegate for processing a single row of pixel data.</summary>
    /// <typeparam name="TRowPixel">The pixel type.</typeparam>
    /// <param name="rowIndex">The zero-based row index.</param>
    /// <param name="rowSpan">A span of pixels representing the row.</param>
    public delegate void ProcessRowDelegate<TRowPixel>(int rowIndex, Span<TRowPixel> rowSpan)
        where TRowPixel : struct;

    /// <summary>Gets the width of the bitmap in pixels.</summary>
    public int Width => _bitmapData.Width;

    /// <summary>Gets the height of the bitmap in pixels.</summary>
    public int Height => _bitmapData.Height;

    /// <summary>Gets the stride (bytes per row) of the bitmap data.</summary>
    public int Stride => Math.Abs(_bitmapData.Stride);

    /// <summary>Gets the pixel format of the bitmap.</summary>
    public PixelFormat PixelFormat => _bitmapData.PixelFormat;

    /// <summary>Gets the indexed palette as a mutable span of BGRA colors.</summary>
    /// <exception cref="T:System.NotSupportedException">Thrown when the pixel format is not indexed.</exception>
    public Span<Bgra32> PaletteSpan => CreatePaletteSpan();

    /// <summary>Gets a span representing a single row of typed pixel data.</summary>
    /// <param name="y">The zero-based row index.</param>
    /// <returns>A span of pixels representing the specified row.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when y is outside the bounds of the bitmap.</exception>
    /// <remarks>
    /// This method correctly handles both top-down and bottom-up bitmaps (negative stride).
    /// The returned span contains typed pixels, allowing direct manipulation of pixel components.
    /// </remarks>
    public unsafe Span<TPixel> GetRowSpan(int y)
    {
        if (y < 0 || y >= Height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(y),
                $"Row index {y} is out of range [0, {Height}).");
        }

        return new(
            (void*)
                checked(
                    unchecked((nuint)(void*)_bitmapData.Scan0)
                    + unchecked((nuint)checked(y * _bitmapData.Stride))),
            Width);
    }

    /// <summary>Processes all rows of the bitmap using the provided action.</summary>
    /// <param name="processRow">An action that processes each row. The action receives the row index and a span of typed pixels.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when processRow is null.</exception>
    public void ProcessRows(ProcessRowDelegate<TPixel> processRow)
    {
        Throw.IfNull(processRow);
        for (int y = 0; y < Height; y = checked(y + 1))
        {
            Span<TPixel> rowSpan = GetRowSpan(y);
            processRow(y, rowSpan);
        }
    }

    /// <summary>Releases the bitmap lock and frees resources. For indexed formats, applies palette changes back to the bitmap.</summary>
    public void Dispose()
    {
        ApplyPalette();
        _bitmap.UnlockBits(_bitmapData);
    }

    /// <summary>Caches the bitmap palette for indexed pixel access.</summary>
    private void CachePalette()
    {
        ColorPalette palette = _bitmap.Palette;
        _paletteCache = new Bgra32[palette.Entries.Length];
        for (int i = 0; i < palette.Entries.Length; i = checked(i + 1))
        {
            Color color = palette.Entries[i];
            _paletteCache[i] = new(color.R, color.G, color.B, color.A);
        }
    }

    /// <summary>Applies cached palette changes back to the bitmap.</summary>
    private void ApplyPalette()
    {
        if (typeof(TPixel) == typeof(Indexed8) && !_readOnly)
        {
            ColorPalette palette = _bitmap.Palette;
            for (
                int i = 0;
                i < _paletteCache.Length && i < palette.Entries.Length;
                i = checked(i + 1))
            {
                Bgra32 bgra = _paletteCache[i];
                palette.Entries[i] = Color.FromArgb(bgra.A, bgra.R, bgra.G, bgra.B);
            }

            _bitmap.Palette = palette;
        }
    }

    /// <summary>Creates a mutable span over the cached indexed palette.</summary>
    /// <returns>A span over the cached palette.</returns>
    private Span<Bgra32> CreatePaletteSpan()
    {
        if (typeof(TPixel) != typeof(Indexed8))
        {
            throw new NotSupportedException(
                "PaletteSpan is only supported for Indexed8 pixel format.");
        }

        return new(_paletteCache);
    }

    /// <summary>Validates pixel format compatibility for the requested typed pixel.</summary>
    private static class PixelFormatValidator
    {
        /// <summary>Validates that the bitmap pixel format matches the requested typed pixel.</summary>
        /// <param name="pixelFormat">The bitmap pixel format.</param>
        public static void Validate(PixelFormat pixelFormat)
        {
            Type pixelType = typeof(TPixel);
            if (pixelType == typeof(Bgra32))
            {
                ValidateBgra32(pixelFormat);
                return;
            }

            if (pixelType == typeof(Bgr24))
            {
                ValidateBgr24(pixelFormat);
                return;
            }

            if (pixelType == typeof(Indexed8))
            {
                ValidateIndexed8(pixelFormat);
                return;
            }

            throw new NotSupportedException(
                $"Pixel type {pixelType.Name} is not supported. Use Bgra32, Bgr24, or Indexed8.");
        }

        /// <summary>Validates a BGRA pixel format.</summary>
        /// <param name="pixelFormat">The bitmap pixel format.</param>
        private static void ValidateBgra32(PixelFormat pixelFormat)
        {
            if (
                pixelFormat is not PixelFormat.Format32bppArgb and not PixelFormat.Format32bppRgb
                && pixelFormat != PixelFormat.Format32bppPArgb)
            {
                throw new NotSupportedException(
                    $"Pixel format {pixelFormat} is not compatible with Bgra32. Use Format32bppArgb, Format32bppRgb, or Format32bppPArgb.");
            }
        }

        /// <summary>Validates a BGR pixel format.</summary>
        /// <param name="pixelFormat">The bitmap pixel format.</param>
        private static void ValidateBgr24(PixelFormat pixelFormat)
        {
            if (pixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new NotSupportedException(
                    $"Pixel format {pixelFormat} is not compatible with Bgr24. Use Format24bppRgb.");
            }
        }

        /// <summary>Validates an indexed pixel format.</summary>
        /// <param name="pixelFormat">The bitmap pixel format.</param>
        private static void ValidateIndexed8(PixelFormat pixelFormat)
        {
            if (pixelFormat != PixelFormat.Format8bppIndexed)
            {
                throw new NotSupportedException(
                    $"Pixel format {pixelFormat} is not compatible with Indexed8. Use Format8bppIndexed.");
            }
        }
    }
}
