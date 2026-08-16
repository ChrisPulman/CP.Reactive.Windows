// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Structs;

/// <summary>Represents a BITMAPV4HEADER structure.</summary>
[StructLayout(LayoutKind.Explicit)]
public struct BitmapV4Header : IEquatable<BitmapV4Header>
{
    /// <summary>Number of bitfield color masks after a BITMAPINFOHEADER.</summary>
    private const uint BitfieldColorMaskCount = 3U;

    /// <summary>Number of bytes in each bitfield color mask.</summary>
    private const uint BitfieldColorMaskSize = 4U;

    /// <summary>Number of bytes used by all bitfield color masks.</summary>
    private const uint BitfieldColorMaskByteCount = BitfieldColorMaskCount * BitfieldColorMaskSize;

    /// <summary>Number of bits to shift to convert bits per pixel to bytes per pixel.</summary>
    private const int BitsPerByteShift = 3;

    /// <summary>Required device plane count.</summary>
    private const ushort DevicePlaneCount = 1;

    /// <summary>Full eight-bit color channel mask.</summary>
    private const uint ColorChannelMask = 255U;

    /// <summary>Red channel bit shift.</summary>
    private const int RedMaskShift = 16;

    /// <summary>Green channel bit shift.</summary>
    private const int GreenMaskShift = 8;

    /// <summary>Alpha channel bit shift.</summary>
    private const int AlphaMaskShift = 24;

    /// <summary>Default red channel mask.</summary>
    private const uint DefaultRedMask = ColorChannelMask << RedMaskShift;

    /// <summary>Default green channel mask.</summary>
    private const uint DefaultGreenMask = ColorChannelMask << GreenMaskShift;

    /// <summary>Default blue channel mask.</summary>
    private const uint DefaultBlueMask = ColorChannelMask;

    /// <summary>Default alpha channel mask.</summary>
    private const uint DefaultAlphaMask = ColorChannelMask << AlphaMaskShift;

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

    /// <summary>Stores the color mask that specifies the red component of each pixel.</summary>
    [FieldOffset(40)]
    private uint _redMask;

    /// <summary>Stores the color mask that specifies the green component of each pixel.</summary>
    [FieldOffset(44)]
    private uint _greenMask;

    /// <summary>Stores the color mask that specifies the blue component of each pixel.</summary>
    [FieldOffset(48)]
    private uint _blueMask;

    /// <summary>Stores the color mask that specifies the alpha component of each pixel.</summary>
    [FieldOffset(52)]
    private uint _alphaMask;

    /// <summary>Stores the color space of the DIB.</summary>
    [FieldOffset(56)]
    private ColorSpace _colorSpace;

    /// <summary>Stores the CIE XYZ endpoints for the bitmap color space.</summary>
    [FieldOffset(60)]
    private CieXyzTriple _endpoints;

    /// <summary>Stores the toned response curve for red.</summary>
    [FieldOffset(96)]
    private uint _gammaRed;

    /// <summary>Stores the toned response curve for green.</summary>
    [FieldOffset(100)]
    private uint _gammaGreen;

    /// <summary>Stores the toned response curve for blue.</summary>
    [FieldOffset(104)]
    private uint _gammaBlue;

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

    /// <summary>Gets or sets the color mask that specifies the red component of each pixel.</summary>
    public uint RedMask
    {
        readonly get => _redMask;
        set => _redMask = value;
    }

    /// <summary>Gets or sets the color mask that specifies the green component of each pixel.</summary>
    public uint GreenMask
    {
        readonly get => _greenMask;
        set => _greenMask = value;
    }

    /// <summary>Gets or sets the color mask that specifies the blue component of each pixel.</summary>
    public uint BlueMask
    {
        readonly get => _blueMask;
        set => _blueMask = value;
    }

    /// <summary>Gets or sets the color mask that specifies the alpha component of each pixel.</summary>
    public uint AlphaMask
    {
        readonly get => _alphaMask;
        set => _alphaMask = value;
    }

    /// <summary>Gets or sets the color space of the DIB.</summary>
    public ColorSpace ColorSpace
    {
        readonly get => _colorSpace;
        set => _colorSpace = value;
    }

    /// <summary>Gets or sets the CIE XYZ endpoints for the bitmap color space.</summary>
    public CieXyzTriple Endpoints
    {
        readonly get => _endpoints;
        set => _endpoints = value;
    }

    /// <summary>Gets or sets the toned response curve for red.</summary>
    public uint GammaRed
    {
        readonly get => _gammaRed;
        set => _gammaRed = value;
    }

    /// <summary>Gets or sets the toned response curve for green.</summary>
    public uint GammaGreen
    {
        readonly get => _gammaGreen;
        set => _gammaGreen = value;
    }

    /// <summary>Gets or sets the toned response curve for blue.</summary>
    public uint GammaBlue
    {
        readonly get => _gammaBlue;
        set => _gammaBlue = value;
    }

    /// <summary>Gets a value indicating whether this is a DIB V4 header.</summary>
    public readonly bool IsDibV4 => _size == checked((uint)Marshal.SizeOf<BitmapV4Header>());

    /// <summary>Gets a value indicating whether this is a DIB V5 header.</summary>
    public readonly bool IsDibV5 => _size >= checked((uint)Marshal.SizeOf<BitmapV5Header>());

    /// <summary>Gets the offset to the pixels.</summary>
    public readonly uint OffsetToPixels =>
        _compression != BitmapCompressionMethods.BI_BITFIELDS
            ? _size
            : checked(_size + BitfieldColorMaskByteCount);

    /// <summary>Create a BitmapV4Header with values.</summary>
    /// <param name="width">The width of the bitmap.</param>
    /// <param name="height">The height of the bitmap.</param>
    /// <param name="bpp">The bits per pixel of the bitmap.</param>
    /// <returns>The created bitmap V4 header.</returns>
    public static BitmapV4Header Create(int width, int height, ushort bpp) =>
        checked(
            new BitmapV4Header
            {
                Size = (uint)Marshal.SizeOf<BitmapV4Header>(),
                Planes = DevicePlaneCount,
                Compression = BitmapCompressionMethods.BI_RGB,
                Width = width,
                Height = height,
                BitCount = bpp,
                SizeImage = (uint)(width * Math.Abs(height) * (bpp >> 3)),
                XPelsPerMeter = 0,
                YPelsPerMeter = 0,
                ColorsUsed = 0U,
                ColorsImportant = 0U,
                RedMask = DefaultRedMask,
                GreenMask = DefaultGreenMask,
                BlueMask = DefaultBlueMask,
                AlphaMask = DefaultAlphaMask,
                ColorSpace = ColorSpace.LCS_sRGB,
                Endpoints = new CieXyzTriple { Blue = CieXyz.Create(0U), Green = CieXyz.Create(0U), Red = CieXyz.Create(0U) },
                GammaRed = 0U,
                GammaGreen = 0U,
                GammaBlue = 0U,
            });

    /// <summary>Determines whether two bitmap V4 headers are equal.</summary>
    /// <param name="left">The left header.</param>
    /// <param name="right">The right header.</param>
    /// <returns><see langword="true" /> when both headers are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(BitmapV4Header left, BitmapV4Header right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two bitmap V4 headers are not equal.</summary>
    /// <param name="left">The left header.</param>
    /// <param name="right">The right header.</param>
    /// <returns><see langword="true" /> when the headers are not equal; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(BitmapV4Header left, BitmapV4Header right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public readonly bool Equals(BitmapV4Header other) =>
        (_size, _width, _height, _planes, _bitCount, _compression).Equals(
            (
                other._size,
                other._width,
                other._height,
                other._planes,
                other._bitCount,
                other._compression))
        && (
            _sizeImage,
            _horizontalPixelsPerMeter,
            _verticalPixelsPerMeter,
            _colorsUsed,
            _colorsImportant,
            _redMask).Equals(
            (
                other._sizeImage,
                other._horizontalPixelsPerMeter,
                other._verticalPixelsPerMeter,
                other._colorsUsed,
                other._colorsImportant,
                other._redMask))
        && (
            _greenMask,
            _blueMask,
            _alphaMask,
            _colorSpace,
            _endpoints,
            _gammaRed,
            _gammaGreen,
            _gammaBlue).Equals(
            (
                other._greenMask,
                other._blueMask,
                other._alphaMask,
                other._colorSpace,
                other._endpoints,
                other._gammaRed,
                other._gammaGreen,
                other._gammaBlue));

    /// <inheritdoc />
    public override readonly bool Equals(object obj) =>
        obj is BitmapV4Header other && Equals(other);

    /// <inheritdoc />
    public override readonly int GetHashCode() => 0;
}
