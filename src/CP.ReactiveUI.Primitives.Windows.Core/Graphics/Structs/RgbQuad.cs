// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Structs;

/// <summary>The RGBQUAD structure describes a color consisting of relative intensities of red, green, and blue.</summary>
public struct RgbQuad : IEquatable<RgbQuad>
{
    /// <summary>The blue intensity.</summary>
    private byte _blue;

    /// <summary>The green intensity.</summary>
    private byte _green;

    /// <summary>The red intensity.</summary>
    private byte _red;

    /// <summary>The reserved value.</summary>
    private byte _reserved;

    /// <summary>Gets or sets the intensity of blue in the color.</summary>
    public byte Blue
    {
        readonly get => _blue;
        set => _blue = value;
    }

    /// <summary>Gets or sets the intensity of green in the color.</summary>
    public byte Green
    {
        readonly get => _green;
        set => _green = value;
    }

    /// <summary>Gets or sets the intensity of red in the color.</summary>
    public byte Red
    {
        readonly get => _red;
        set => _red = value;
    }

    /// <summary>Gets or sets the reserved value.</summary>
    public byte Reserved
    {
        readonly get => _reserved;
        set => _reserved = value;
    }

    /// <summary>Determines whether two colors are equal.</summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns><see langword="true" /> when the colors are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(RgbQuad left, RgbQuad right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two colors are not equal.</summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns><see langword="true" /> when the colors are not equal; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(RgbQuad left, RgbQuad right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public readonly bool Equals(RgbQuad other) => _blue == other._blue && _green == other._green && _red == other._red && _reserved == other._reserved;

    /// <inheritdoc />
    public override readonly bool Equals(object obj) => obj is RgbQuad other && Equals(other);

    /// <inheritdoc />
    public override readonly int GetHashCode() => typeof(RgbQuad).GetHashCode();
}
