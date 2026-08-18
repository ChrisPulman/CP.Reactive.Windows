// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.Structs;
#endif
/// <summary>Represents the ICONDIRENTRY structure in an ICO file.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct IconDirEntry
{
    /// <summary>Defines the maximum encoded icon dimension.</summary>
    private const int MaximumEncodedDimension = 256;

    /// <summary>Defines the native structure size.</summary>
    private const int StructureSize = 16;

    /// <summary>Gets the size of the ICONDIRENTRY structure in bytes.</summary>
    public static int Size => StructureSize;

    /// <summary>Gets or sets the image width in pixels.</summary>
    public byte Width { get; init; }

    /// <summary>Gets or sets the image height in pixels.</summary>
    public byte Height { get; init; }

    /// <summary>Gets or sets the palette color count.</summary>
    public byte ColorCount { get; init; }

    /// <summary>Gets or sets the reserved value, which must be zero.</summary>
    public byte Reserved { get; init; }

    /// <summary>Gets or sets the color planes or cursor horizontal hotspot.</summary>
    public ushort Planes { get; init; }

    /// <summary>Gets or sets the bit count or cursor vertical hotspot.</summary>
    public ushort BitCount { get; init; }

    /// <summary>Gets or sets the image data size in bytes.</summary>
    public uint BytesInRes { get; init; }

    /// <summary>Gets or sets the image data offset from the beginning of the file.</summary>
    public uint ImageOffset { get; init; }

    /// <summary>Creates a new ICONDIRENTRY for an icon.</summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="bitCount">Bits per pixel.</param>
    /// <param name="imageSize">Size of the image data in bytes.</param>
    /// <param name="imageOffset">Offset to the image data.</param>
    /// <returns>The initialized ICONDIRENTRY structure.</returns>
    public static IconDirEntry CreateForIcon(int width, int height, ushort bitCount, uint imageSize, uint imageOffset) => new IconDirEntry
    {
        Width = EncodeDimension(width),
        Height = EncodeDimension(height),
        BitCount = bitCount,
        BytesInRes = imageSize,
        ImageOffset = imageOffset,
    };

    /// <summary>Creates a new ICONDIRENTRY for a cursor.</summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="hotspotX">Horizontal coordinate of the hotspot.</param>
    /// <param name="hotspotY">Vertical coordinate of the hotspot.</param>
    /// <param name="imageSize">Size of the image data in bytes.</param>
    /// <param name="imageOffset">Offset to the image data.</param>
    /// <returns>The initialized ICONDIRENTRY structure.</returns>
    /// <remarks>
    /// For cursors, the Planes and BitCount fields store the horizontal and vertical hotspot coordinates.
    /// </remarks>
    public static IconDirEntry CreateForCursor(int width, int height, ushort hotspotX, ushort hotspotY, uint imageSize, uint imageOffset) => new IconDirEntry
    {
        Width = EncodeDimension(width),
        Height = EncodeDimension(height),
        Planes = hotspotX,
        BitCount = hotspotY,
        BytesInRes = imageSize,
        ImageOffset = imageOffset,
    };

    /// <summary>Encodes an icon dimension for an ICO directory entry.</summary>
    /// <param name="dimension">The source dimension.</param>
    /// <returns>The encoded dimension byte.</returns>
    private static byte EncodeDimension(int dimension) =>
        dimension != MaximumEncodedDimension ? checked((byte)dimension) : (byte)0;
}
