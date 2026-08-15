// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Structs;

/// <summary>
/// Represents a bitmap image, providing properties to access its width and height in pixels.
/// See <a href="https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmap">BITMAP structure</a>
/// </summary>
/// <remarks>This structure is used to define the dimensions of a bitmap image. The width and height can be set to
/// modify the bitmap's size.</remarks>
public readonly struct GdiBitmap : IEquatable<GdiBitmap>
{
    /// <summary>The bitmap type.</summary>
    private readonly int _type;

    /// <summary>The bitmap width, in pixels.</summary>
    private readonly int _width;

    /// <summary>The bitmap height, in pixels.</summary>
    private readonly int _height;

    /// <summary>The number of bytes in each scan line.</summary>
    private readonly int _widthBytes;

    /// <summary>The count of color planes.</summary>
    private readonly short _planes;

    /// <summary>The count of adjacent color bits on each plane.</summary>
    private readonly short _bitsPixel;

    /// <summary>A pointer to the location of the bit values for the bitmap.</summary>
    private readonly IntPtr _bits;

    /// <summary>Initializes a new instance of the <see cref="GdiBitmap" /> struct.</summary>
    public GdiBitmap()
    {
        _type = default;
        _width = default;
        _height = default;
        _widthBytes = default;
        _planes = default;
        _bitsPixel = default;
        _bits = default;
    }

    /// <summary>Gets the width of the bitmap, in pixels.</summary>
    public int Width => _width;

    /// <summary>Gets the height of the bitmap, in pixels.</summary>
    public int Height => _height;

    /// <summary>Determines whether two bitmaps are equal.</summary>
    /// <param name="left">The first bitmap.</param>
    /// <param name="right">The second bitmap.</param>
    /// <returns><see langword="true" /> when the bitmaps are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(GdiBitmap left, GdiBitmap right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two bitmaps are not equal.</summary>
    /// <param name="left">The first bitmap.</param>
    /// <param name="right">The second bitmap.</param>
    /// <returns><see langword="true" /> when the bitmaps are not equal; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(GdiBitmap left, GdiBitmap right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public bool Equals(GdiBitmap other) =>
        _type == other._type
        && _width == other._width
        && _height == other._height
        && _widthBytes == other._widthBytes
        && _planes == other._planes
        && _bitsPixel == other._bitsPixel
        && _bits == other._bits;

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is GdiBitmap other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => typeof(GdiBitmap).GetHashCode();
}
