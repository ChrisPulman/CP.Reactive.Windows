// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Structs;

/// <summary>Represents a BITMAPV5HEADER structure.</summary>
[StructLayout(LayoutKind.Explicit)]
public struct BitmapV5Header : IEquatable<BitmapV5Header>
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

    /// <summary>Stable hash code for a mutable native header.</summary>
    private const int StableMutableHashCode = 0;

    /// <summary>Gets or sets the number of bytes required by the structure.</summary>
    [field: FieldOffset(0)]
    public uint Size { get; set; }

    /// <summary>Gets or sets the width of the bitmap, in pixels.</summary>
    [field: FieldOffset(4)]
    public int Width { get; set; }

    /// <summary>Gets or sets the height of the bitmap, in pixels.</summary>
    [field: FieldOffset(8)]
    public int Height { get; set; }

    /// <summary>Gets or sets the number of planes for the target device.</summary>
    [field: FieldOffset(12)]
    public ushort Planes { get; set; }

    /// <summary>Gets or sets the number of bits that define each pixel and the maximum number of colors in the bitmap.</summary>
    [field: FieldOffset(14)]
    public ushort BitCount { get; set; }

    /// <summary>Gets or sets the bitmap compression mode.</summary>
    [field: FieldOffset(16)]
    public BitmapCompressionMethods Compression { get; set; }

    /// <summary>Gets or sets the size, in bytes, of the image.</summary>
    [field: FieldOffset(20)]
    public uint SizeImage { get; set; }

    /// <summary>Gets or sets the horizontal resolution, in pixels-per-meter, of the target device for the bitmap.</summary>
    [field: FieldOffset(24)]
    public int XPelsPerMeter { get; set; }

    /// <summary>Gets or sets the vertical resolution, in pixels-per-meter, of the target device for the bitmap.</summary>
    [field: FieldOffset(28)]
    public int YPelsPerMeter { get; set; }

    /// <summary>Gets or sets the number of color indexes in the color table that are actually used by the bitmap.</summary>
    [field: FieldOffset(32)]
    public uint ColorsUsed { get; set; }

    /// <summary>Gets or sets the number of color indexes that are required for displaying the bitmap.</summary>
    [field: FieldOffset(36)]
    public uint ColorsImportant { get; set; }

    /// <summary>Gets or sets the color mask that specifies the red component of each pixel.</summary>
    [field: FieldOffset(40)]
    public uint RedMask { get; set; }

    /// <summary>Gets or sets the color mask that specifies the green component of each pixel.</summary>
    [field: FieldOffset(44)]
    public uint GreenMask { get; set; }

    /// <summary>Gets or sets the color mask that specifies the blue component of each pixel.</summary>
    [field: FieldOffset(48)]
    public uint BlueMask { get; set; }

    /// <summary>Gets or sets the color mask that specifies the alpha component of each pixel.</summary>
    [field: FieldOffset(52)]
    public uint AlphaMask { get; set; }

    /// <summary>Gets or sets the color space of the DIB.</summary>
    [field: FieldOffset(56)]
    public ColorSpace ColorSpace { get; set; }

    /// <summary>Gets or sets the CIE XYZ endpoints for the bitmap color space.</summary>
    [field: FieldOffset(60)]
    public CieXyzTriple Endpoints { get; set; }

    /// <summary>Gets or sets the toned response curve for red.</summary>
    [field: FieldOffset(96)]
    public uint GammaRed { get; set; }

    /// <summary>Gets or sets the toned response curve for green.</summary>
    [field: FieldOffset(100)]
    public uint GammaGreen { get; set; }

    /// <summary>Gets or sets the toned response curve for blue.</summary>
    [field: FieldOffset(104)]
    public uint GammaBlue { get; set; }

    /// <summary>Gets or sets the rendering intent for the bitmap.</summary>
    [field: FieldOffset(108)]
    public ColorSpace Intent { get; set; }

    /// <summary>Gets or sets the offset, in bytes, from the beginning of the structure to the profile data.</summary>
    [field: FieldOffset(112)]
    public uint ProfileData { get; set; }

    /// <summary>Gets or sets the size, in bytes, of embedded profile data.</summary>
    [field: FieldOffset(116)]
    public uint ProfileSize { get; set; }

    /// <summary>Gets or sets the reserved value, which should be set to zero.</summary>
    [field: FieldOffset(120)]
    public uint Reserved { get; set; }

    /// <summary>Gets a value indicating whether this is a DIB V4 header.</summary>
    public readonly bool IsDibV4 => Size == checked((uint)Marshal.SizeOf<BitmapV4Header>());

    /// <summary>Gets a value indicating whether this is a DIB V5 header.</summary>
    public readonly bool IsDibV5 => Size >= checked((uint)Marshal.SizeOf<BitmapV5Header>());

    /// <summary>Gets the offset to the pixels.</summary>
    public readonly uint OffsetToPixels =>
        Compression != BitmapCompressionMethods.BI_BITFIELDS
            ? Size
            : checked(Size + BitfieldColorMaskByteCount);

    /// <summary>Create a BitmapV5Header with values.</summary>
    /// <param name="width">The width of the bitmap.</param>
    /// <param name="height">The height of the bitmap.</param>
    /// <param name="bpp">The bits per pixel of the bitmap.</param>
    /// <returns>The created bitmap V5 header.</returns>
    public static BitmapV5Header Create(int width, int height, ushort bpp) =>
        checked(
            new BitmapV5Header
            {
                Size = (uint)Marshal.SizeOf<BitmapV5Header>(),
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
                Intent = ColorSpace.LCS_GM_IMAGES,
                ProfileData = 0U,
                ProfileSize = 0U,
                Reserved = 0U,
            });

    /// <summary>Determines whether two bitmap V5 headers are equal.</summary>
    /// <param name="left">The left header.</param>
    /// <param name="right">The right header.</param>
    /// <returns><see langword="true" /> when both headers are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(BitmapV5Header left, BitmapV5Header right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two bitmap V5 headers are not equal.</summary>
    /// <param name="left">The left header.</param>
    /// <param name="right">The right header.</param>
    /// <returns><see langword="true" /> when the headers are not equal; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(BitmapV5Header left, BitmapV5Header right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public readonly bool Equals(BitmapV5Header other) =>
        EqualsBitmapInfo(other)
        && EqualsColorMasks(other)
        && EqualsColorSpace(other)
        && EqualsProfile(other);

    /// <inheritdoc />
    public override readonly bool Equals(object obj) =>
        obj is BitmapV5Header other && Equals(other);

    /// <inheritdoc />
    public override readonly int GetHashCode() => 0;

    /// <summary>Compares the BITMAPINFOHEADER-compatible fields.</summary>
    /// <param name="other">The header to compare with this instance.</param>
    /// <returns><see langword="true" /> when the fields are equal; otherwise, <see langword="false" />.</returns>
    private readonly bool EqualsBitmapInfo(BitmapV5Header other) =>
        (Size, Width, Height, Planes, BitCount, Compression, SizeImage).Equals(
            (
                other.Size,
                other.Width,
                other.Height,
                other.Planes,
                other.BitCount,
                other.Compression,
                other.SizeImage))
        && (XPelsPerMeter, YPelsPerMeter, ColorsUsed, ColorsImportant).Equals(
            (other.XPelsPerMeter, other.YPelsPerMeter, other.ColorsUsed, other.ColorsImportant));

    /// <summary>Compares the bitfield color masks.</summary>
    /// <param name="other">The header to compare with this instance.</param>
    /// <returns><see langword="true" /> when the fields are equal; otherwise, <see langword="false" />.</returns>
    private readonly bool EqualsColorMasks(BitmapV5Header other) =>
        (RedMask, GreenMask, BlueMask, AlphaMask).Equals(
            (other.RedMask, other.GreenMask, other.BlueMask, other.AlphaMask));

    /// <summary>Compares the bitmap color-space fields.</summary>
    /// <param name="other">The header to compare with this instance.</param>
    /// <returns><see langword="true" /> when the fields are equal; otherwise, <see langword="false" />.</returns>
    private readonly bool EqualsColorSpace(BitmapV5Header other) =>
        (ColorSpace, Endpoints, GammaRed, GammaGreen, GammaBlue, Intent).Equals(
            (
                other.ColorSpace,
                other.Endpoints,
                other.GammaRed,
                other.GammaGreen,
                other.GammaBlue,
                other.Intent));

    /// <summary>Compares the bitmap profile fields.</summary>
    /// <param name="other">The header to compare with this instance.</param>
    /// <returns><see langword="true" /> when the fields are equal; otherwise, <see langword="false" />.</returns>
    private readonly bool EqualsProfile(BitmapV5Header other) =>
        (ProfileData, ProfileSize, Reserved).Equals(
            (other.ProfileData, other.ProfileSize, other.Reserved));
}
