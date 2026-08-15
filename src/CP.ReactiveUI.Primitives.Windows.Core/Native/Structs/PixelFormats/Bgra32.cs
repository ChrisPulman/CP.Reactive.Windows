// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats;

/// <summary>
/// Represents a pixel in BGRA format (Blue, Green, Red, Alpha) - 32 bits per pixel.
/// This is the native format for Format32bppArgb in System.Drawing.
/// </summary>
/// <param name="r">The red component.</param>
/// <param name="g">The green component.</param>
/// <param name="b">The blue component.</param>
/// <param name="a">The alpha component.</param>
public readonly struct Bgra32(byte r, byte g, byte b, byte a = byte.MaxValue) : IEquatable<Bgra32>
{
    /// <summary>The maximum channel value for an opaque pixel component.</summary>
    private const byte MaxChannelValue = byte.MaxValue;

    /// <summary>Gets the blue component value.</summary>
    public byte B { get; } = b;

    /// <summary>Gets the green component value.</summary>
    public byte G { get; } = g;

    /// <summary>Gets the red component value.</summary>
    public byte R { get; } = r;

    /// <summary>Gets the alpha component value.</summary>
    public byte A { get; } = a;

    /// <summary>Compares two <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats.Bgra32" /> objects for equality.</summary>
    /// <param name="left">The left pixel.</param>
    /// <param name="right">The right pixel.</param>
    /// <returns>true when the pixels have equal channels.</returns>
    public static bool operator ==(Bgra32 left, Bgra32 right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats.Bgra32" /> objects for inequality.</summary>
    /// <param name="left">The left pixel.</param>
    /// <param name="right">The right pixel.</param>
    /// <returns>true when the pixels have any different channel.</returns>
    public static bool operator !=(Bgra32 left, Bgra32 right)
    {
        return !left.Equals(right);
    }

    /// <summary>Alpha blends a source pixel onto this target pixel.</summary>
    /// <param name="target">The target pixel to blend onto.</param>
    /// <param name="source">The source pixel to blend from.</param>
    public static void AlphaBlend(ref Bgra32 target, Bgra32 source)
    {
        checked
        {
            if (source.A != 0)
            {
                if (source.A == byte.MaxValue)
                {
                    target = source;
                    return;
                }

                byte alpha = source.A;
                int inverseAlpha = MaxChannelValue - alpha;
                byte blue = (byte)
                    unchecked(
                        checked((source.B * alpha) + (target.B * inverseAlpha)) / MaxChannelValue);
                byte green = (byte)
                    unchecked(
                        checked((source.G * alpha) + (target.G * inverseAlpha)) / MaxChannelValue);
                byte red = (byte)
                    unchecked(
                        checked((source.R * alpha) + (target.R * inverseAlpha)) / MaxChannelValue);
                target = new(red, green, blue);
            }
        }
    }

    /// <summary>Compares this pixel with another <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats.Bgra32" /> for equality.</summary>
    /// <param name="other">The pixel to compare with this instance.</param>
    /// <returns>true when all channels match.</returns>
    public bool Equals(Bgra32 other) =>
        B == other.B && G == other.G && R == other.R && A == other.A;

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is Bgra32 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(B, G, R, A);

    /// <inheritdoc />
    public override string ToString() => $"Bgra32({R}, {G}, {B}, {A})";
}
