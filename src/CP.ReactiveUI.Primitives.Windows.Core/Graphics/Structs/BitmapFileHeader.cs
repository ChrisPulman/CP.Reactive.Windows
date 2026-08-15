// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Structs;

/// <summary>Contains information about the type, size, and layout of a DIB file.</summary>
[StructLayout(LayoutKind.Explicit, Pack = 2)]
public struct BitmapFileHeader : IEquatable<BitmapFileHeader>
{
    /// <summary>Bitmap file signature.</summary>
    private const short BitmapSignature = 19_778;

    /// <summary>Number of bytes in a color table entry.</summary>
    private const uint ColorTableEntrySize = 4U;

    /// <summary>Gets the file type; it must be BM.</summary>
    [field: FieldOffset(0)]
    public short FileType { get; private set; }

    /// <summary>Gets the size, in bytes, of the bitmap file.</summary>
    [field: FieldOffset(2)]
    public int Size { get; private set; }

    /// <summary>Gets the offset, in bytes, from the beginning of the BITMAPFILEHEADER structure to the bitmap bits.</summary>
    [field: FieldOffset(10)]
    public int OffsetToBitmapBits { get; private set; }

    /// <summary>Gets or sets the first reserved field.</summary>
    [field: FieldOffset(6)]
    private short Reserved1 { get; set; }

    /// <summary>Gets or sets the second reserved field.</summary>
    [field: FieldOffset(8)]
    private short Reserved2 { get; set; }

    /// <summary>Create a BitmapFileHeader which needs a BitmapV5Header to calculate the values.</summary>
    /// <param name="bitmapV5Header">The bitmap V5 header.</param>
    /// <returns>The calculated bitmap file header.</returns>
    public static BitmapFileHeader Create(BitmapV5Header bitmapV5Header)
    {
        checked
        {
            uint bitmapFileHeaderSize = (uint)Marshal.SizeOf<BitmapFileHeader>();
            return new BitmapFileHeader
            {
                FileType = 19_778,
                Size = (int)(bitmapFileHeaderSize + bitmapV5Header.Size + bitmapV5Header.SizeImage),
                Reserved1 = 0,
                Reserved2 = 0,
                OffsetToBitmapBits = (int)(bitmapFileHeaderSize + bitmapV5Header.Size + (bitmapV5Header.ColorsUsed * 4))
            };
        }
    }

    /// <summary>Create a BitmapFileHeader which needs a BitmapInfoHeader to calculate the values.</summary>
    /// <param name="bitmapInfoHeader">The bitmap information header.</param>
    /// <returns>The calculated bitmap file header.</returns>
    public static BitmapFileHeader Create(BitmapInfoHeader bitmapInfoHeader)
    {
        checked
        {
            uint bitmapFileHeaderSize = (uint)Marshal.SizeOf<BitmapFileHeader>();
            return new BitmapFileHeader
            {
                FileType = 19_778,
                Size = (int)(bitmapFileHeaderSize + bitmapInfoHeader.Size + bitmapInfoHeader.SizeImage),
                Reserved1 = 0,
                Reserved2 = 0,
                OffsetToBitmapBits = (int)(bitmapFileHeaderSize + bitmapInfoHeader.Size + (bitmapInfoHeader.ColorsUsed * 4))
            };
        }
    }

    /// <summary>Determines whether two bitmap file headers are equal.</summary>
    /// <param name="left">The left header.</param>
    /// <param name="right">The right header.</param>
    /// <returns><see langword="true" /> when both headers are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(BitmapFileHeader left, BitmapFileHeader right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two bitmap file headers are not equal.</summary>
    /// <param name="left">The left header.</param>
    /// <param name="right">The second header.</param>
    /// <returns><see langword="true" /> when the headers are not equal; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(BitmapFileHeader left, BitmapFileHeader right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public readonly bool Equals(BitmapFileHeader other) => FileType == other.FileType && Size == other.Size && Reserved1 == other.Reserved1 && Reserved2 == other.Reserved2 && OffsetToBitmapBits == other.OffsetToBitmapBits;

    /// <inheritdoc />
    public override readonly bool Equals(object obj) => obj is BitmapFileHeader other && Equals(other);

    /// <inheritdoc />
    public override readonly int GetHashCode() => typeof(BitmapFileHeader).GetHashCode();
}
