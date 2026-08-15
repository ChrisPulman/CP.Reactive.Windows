// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Structs;

/// <summary>Represents a BITMAPINFOHEADER structure.</summary>
[StructLayout(LayoutKind.Explicit)]
public struct BitmapInfoHeader : IEquatable<BitmapInfoHeader>
{
    /// <summary>Number of bitfield color masks after a BITMAPINFOHEADER.</summary>
    private const uint BitfieldColorMaskCount = 3U;

    /// <summary>Number of bytes in each bitfield color mask.</summary>
    private const uint BitfieldColorMaskSize = 4U;

    /// <summary>Number of bits to shift to convert bits per pixel to bytes per pixel.</summary>
    private const int BitsPerByteShift = 3;

    /// <summary>Required device plane count.</summary>
    private const ushort DevicePlaneCount = 1;

    /// <summary>Hash code used for mutable value instances.</summary>
    private const int MutableValueHashCode = 0;

    /// <summary>Stores the number of bytes required by the structure.</summary>
    [FieldOffset(0)]
    private uint _size;

    /// <summary>Stores the width of the bitmap, in pixels.</summary>
    [FieldOffset(4)]
    private int _width;

    /// <summary>Stores the height of the bitmap, in pixels.</summary>
    [FieldOffset(8)]
    private int _height;

    /// <summary>Stores the number of planes for the target device.</summary>
    [FieldOffset(12)]
    private ushort _planes;

    /// <summary>Stores the number of bits that define each pixel and the maximum number of colors in the bitmap.</summary>
    [FieldOffset(14)]
    private ushort _bitCount;

    /// <summary>Stores the bitmap compression mode.</summary>
    [FieldOffset(16)]
    private BitmapCompressionMethods _compression;

    /// <summary>Stores the size, in bytes, of the image.</summary>
    [FieldOffset(20)]
    private uint _sizeImage;

    /// <summary>Stores the horizontal resolution, in pixels-per-meter, of the target device for the bitmap.</summary>
    [FieldOffset(24)]
    private int _horizontalPixelsPerMeter;

    /// <summary>Stores the vertical resolution, in pixels-per-meter, of the target device for the bitmap.</summary>
    [FieldOffset(28)]
    private int _verticalPixelsPerMeter;

    /// <summary>Stores the number of color indexes in the color table that are actually used by the bitmap.</summary>
    [FieldOffset(32)]
    private uint _colorsUsed;

    /// <summary>Stores the number of color indexes that are required for displaying the bitmap.</summary>
    [FieldOffset(36)]
    private uint _colorsImportant;

    /// <summary>Gets or sets the number of bytes required by the structure.</summary>
    public uint Size
    {
        readonly get => _size;
        set => _size = value;
    }

    /// <summary>Gets or sets the width of the bitmap, in pixels.</summary>
    public int Width
    {
        readonly get => _width;
        set => _width = value;
    }

    /// <summary>Gets or sets the height of the bitmap, in pixels.</summary>
    public int Height
    {
        readonly get => _height;
        set => _height = value;
    }

    /// <summary>Gets or sets the number of planes for the target device.</summary>
    public ushort Planes
    {
        readonly get => _planes;
        set => _planes = value;
    }

    /// <summary>Gets or sets the number of bits that define each pixel and the maximum number of colors in the bitmap.</summary>
    public ushort BitCount
    {
        readonly get => _bitCount;
        set => _bitCount = value;
    }

    /// <summary>Gets or sets the bitmap compression mode.</summary>
    public BitmapCompressionMethods Compression
    {
        readonly get => _compression;
        set => _compression = value;
    }

    /// <summary>Gets or sets the size, in bytes, of the image.</summary>
    public uint SizeImage
    {
        readonly get => _sizeImage;
        set => _sizeImage = value;
    }

    /// <summary>Gets or sets the horizontal resolution, in pixels-per-meter, of the target device for the bitmap.</summary>
    public int XPelsPerMeter
    {
        readonly get => _horizontalPixelsPerMeter;
        set => _horizontalPixelsPerMeter = value;
    }

    /// <summary>Gets or sets the vertical resolution, in pixels-per-meter, of the target device for the bitmap.</summary>
    public int YPelsPerMeter
    {
        readonly get => _verticalPixelsPerMeter;
        set => _verticalPixelsPerMeter = value;
    }

    /// <summary>Gets or sets the number of color indexes in the color table that are actually used by the bitmap.</summary>
    public uint ColorsUsed
    {
        readonly get => _colorsUsed;
        set => _colorsUsed = value;
    }

    /// <summary>Gets or sets the number of color indexes that are required for displaying the bitmap.</summary>
    public uint ColorsImportant
    {
        readonly get => _colorsImportant;
        set => _colorsImportant = value;
    }

    /// <summary>Gets a value indicating whether this is a DIB V4 header.</summary>
    public readonly bool IsDibV4 => _size == checked((uint)Marshal.SizeOf<BitmapV4Header>());

    /// <summary>Gets a value indicating whether this is a DIB V5 header.</summary>
    public readonly bool IsDibV5 => _size >= checked((uint)Marshal.SizeOf<BitmapV5Header>());

    /// <summary>Gets the offset to the pixels.</summary>
    public readonly uint OffsetToPixels => _compression != BitmapCompressionMethods.BI_BITFIELDS ? _size : checked(_size + 12);

    /// <summary>Create a BitmapInfoHeader with values.</summary>
    /// <param name="width">The width of the bitmap.</param>
    /// <param name="height">The height of the bitmap.</param>
    /// <param name="bpp">The bits per pixel of the bitmap.</param>
    /// <returns>The created bitmap information header.</returns>
    public static BitmapInfoHeader Create(int width, int height, ushort bpp) => checked(new BitmapInfoHeader
        {
            Size = (uint)Marshal.SizeOf<BitmapInfoHeader>(),
            Planes = 1,
            Compression = BitmapCompressionMethods.BI_RGB,
            Width = width,
            Height = height,
            BitCount = bpp,
            SizeImage = (uint)(width * Math.Abs(height) * (bpp >> 3)),
            XPelsPerMeter = 0,
            YPelsPerMeter = 0,
            ColorsUsed = 0U,
            ColorsImportant = 0U
        });

    /// <summary>Determines whether two bitmap information headers are equal.</summary>
    /// <param name="left">The left header.</param>
    /// <param name="right">The right header.</param>
    /// <returns><see langword="true" /> when both headers are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(BitmapInfoHeader left, BitmapInfoHeader right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two bitmap information headers are not equal.</summary>
    /// <param name="left">The left header.</param>
    /// <param name="right">The right header.</param>
    /// <returns><see langword="true" /> when the headers are not equal; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(BitmapInfoHeader left, BitmapInfoHeader right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public readonly bool Equals(BitmapInfoHeader other) => (_size, _width, _height, _planes, _bitCount, _compression).Equals((other._size, other._width, other._height, other._planes, other._bitCount, other._compression)) && (_sizeImage, _horizontalPixelsPerMeter, _verticalPixelsPerMeter, _colorsUsed, _colorsImportant).Equals((other._sizeImage, other._horizontalPixelsPerMeter, other._verticalPixelsPerMeter, other._colorsUsed, other._colorsImportant));

    /// <inheritdoc />
    public override readonly bool Equals(object obj) => obj is BitmapInfoHeader other && Equals(other);

    /// <inheritdoc />
    public override readonly int GetHashCode() => 0;
}
