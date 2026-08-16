// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats;

/// <summary>
/// Represents an indexed pixel - 8 bits per pixel.
/// This is the format for Format8bppIndexed in System.Drawing.
/// The value is an index into a color palette.
/// </summary>
/// <param name="index">The palette index.</param>
public readonly struct Indexed8(byte index) : IEquatable<Indexed8>
{
    /// <summary>Gets the palette index value.</summary>
    public byte Index { get; } = index;

    /// <summary>Compares two <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats.Indexed8" /> objects for equality.</summary>
    /// <param name="left">The left pixel.</param>
    /// <param name="right">The right pixel.</param>
    /// <returns>true when the palette indexes are equal.</returns>
    public static bool operator ==(Indexed8 left, Indexed8 right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats.Indexed8" /> objects for inequality.</summary>
    /// <param name="left">The left pixel.</param>
    /// <param name="right">The right pixel.</param>
    /// <returns>true when the palette indexes differ.</returns>
    public static bool operator !=(Indexed8 left, Indexed8 right)
    {
        return !left.Equals(right);
    }

    /// <summary>Compares this pixel with another <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats.Indexed8" /> for equality.</summary>
    /// <param name="other">The pixel to compare with this instance.</param>
    /// <returns>true when the palette indexes match.</returns>
    public bool Equals(Indexed8 other) => Index == other.Index;

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is Indexed8 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Index.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => $"Indexed8({Index})";
}
