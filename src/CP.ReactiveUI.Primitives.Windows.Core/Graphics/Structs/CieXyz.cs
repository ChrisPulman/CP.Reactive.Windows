// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Structs;

/// <summary>CIE XYZ 1931 color space.</summary>
public struct CieXyz : IEquatable<CieXyz>
{
    /// <summary>The X component.</summary>
    private uint _x;

    /// <summary>The Y component.</summary>
    private uint _y;

    /// <summary>The Z component.</summary>
    private uint _z;

    /// <summary>Gets or sets a mix of cone response curves chosen to be orthogonal to luminance and non-negative.</summary>
    public uint X
    {
        readonly get => _x;
        set => _x = value;
    }

    /// <summary>Gets or sets the luminance component.</summary>
    public uint Y
    {
        readonly get => _y;
        set => _y = value;
    }

    /// <summary>Gets or sets the component that is somewhat equal to blue.</summary>
    public uint Z
    {
        readonly get => _z;
        set => _z = value;
    }

    /// <summary>Creates a CIE XYZ value from an FXPT2DOT30 fixed-point value.</summary>
    /// <param name="fixedPoint2Dot30">The fixed-point value with a 2-bit integer part and a 30-bit fractional part.</param>
    /// <returns>The CIE XYZ value.</returns>
    public static CieXyz Create(uint fixedPoint2Dot30) =>
        new CieXyz { _x = fixedPoint2Dot30, _y = fixedPoint2Dot30, _z = fixedPoint2Dot30 };

    /// <summary>Determines whether two CIE XYZ values are equal.</summary>
    /// <param name="left">The first CIE XYZ value.</param>
    /// <param name="right">The second CIE XYZ value.</param>
    /// <returns><see langword="true" /> when the values are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(CieXyz left, CieXyz right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two CIE XYZ values are not equal.</summary>
    /// <param name="left">The first CIE XYZ value.</param>
    /// <param name="right">The second CIE XYZ value.</param>
    /// <returns><see langword="true" /> when the values are not equal; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(CieXyz left, CieXyz right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public readonly bool Equals(CieXyz other) => _x == other._x && _y == other._y && _z == other._z;

    /// <inheritdoc />
    public override readonly bool Equals(object obj) => obj is CieXyz other && Equals(other);

    /// <inheritdoc />
    public override readonly int GetHashCode() => typeof(CieXyz).GetHashCode();
}
