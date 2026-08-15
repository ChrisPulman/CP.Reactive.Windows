// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Structs;

/// <summary>Specifies the color masks used by a bitfield bitmap.</summary>
public readonly struct BitfieldColorMask : IEquatable<BitfieldColorMask>
{
    /// <summary>Stores the blue mask.</summary>
    private readonly uint _blue;

    /// <summary>Stores the green mask.</summary>
    private readonly uint _green;

    /// <summary>Stores the red mask.</summary>
    private readonly uint _red;

    /// <summary>Initializes a new instance of the <see cref="BitfieldColorMask" /> struct with maximum components.</summary>
    public BitfieldColorMask()
        : this(byte.MaxValue, byte.MaxValue, byte.MaxValue) { }

    /// <summary>Initializes a new instance of the <see cref="BitfieldColorMask" /> struct with a red component.</summary>
    /// <param name="r">The red component.</param>
    public BitfieldColorMask(byte r)
        : this(r, byte.MaxValue, byte.MaxValue) { }

    /// <summary>Initializes a new instance of the <see cref="BitfieldColorMask" /> struct with red and green components.</summary>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    public BitfieldColorMask(byte r, byte g)
        : this(r, g, byte.MaxValue) { }

    /// <summary>Initializes a new instance of the <see cref="BitfieldColorMask" /> struct.</summary>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="b">The blue component.</param>
    public BitfieldColorMask(byte r, byte g, byte b)
    {
        _red = (uint)r << 8;
        _green = (uint)g << 16;
        _blue = (uint)b << 24;
    }

    /// <summary>Gets the blue component of the mask.</summary>
    public uint Blue => _blue;

    /// <summary>Gets the green component of the mask.</summary>
    public uint Green => _green;

    /// <summary>Gets the red component of the mask.</summary>
    public uint Red => _red;

    /// <summary>Creates a color mask with all components set to their maximum values.</summary>
    /// <returns>A color mask.</returns>
    public static BitfieldColorMask Create() => new();

    /// <summary>Creates a color mask with a red component and maximum green and blue components.</summary>
    /// <param name="r">The red component.</param>
    /// <returns>A color mask.</returns>
    public static BitfieldColorMask Create(byte r) => new(r);

    /// <summary>Creates a color mask with red and green components and a maximum blue component.</summary>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <returns>A color mask.</returns>
    public static BitfieldColorMask Create(byte r, byte g) => new(r, g);

    /// <summary>Creates a color mask.</summary>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="b">The blue component.</param>
    /// <returns>A color mask.</returns>
    public static BitfieldColorMask Create(byte r, byte g, byte b) => new(r, g, b);

    /// <summary>Determines whether two masks are equal.</summary>
    /// <param name="left">The first mask.</param>
    /// <param name="right">The second mask.</param>
    /// <returns><see langword="true" /> when the masks are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(BitfieldColorMask left, BitfieldColorMask right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two masks are unequal.</summary>
    /// <param name="left">The first mask.</param>
    /// <param name="right">The second mask.</param>
    /// <returns><see langword="true" /> when the masks are unequal; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(BitfieldColorMask left, BitfieldColorMask right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public bool Equals(BitfieldColorMask other) =>
        _blue == other._blue && _green == other._green && _red == other._red;

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is BitfieldColorMask other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_blue, _green, _red);
}
