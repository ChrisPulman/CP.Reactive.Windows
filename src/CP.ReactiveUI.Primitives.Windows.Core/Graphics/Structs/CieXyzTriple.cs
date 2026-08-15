// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Structs;

/// <summary>Color representation using CIEXYZ color components.</summary>
public struct CieXyzTriple : IEquatable<CieXyzTriple>
{
    /// <summary>The red color component.</summary>
    private CieXyz _cieXyzRed;

    /// <summary>The green color component.</summary>
    private CieXyz _cieXyzGreen;

    /// <summary>The blue color component.</summary>
    private CieXyz _cieXyzBlue;

    /// <summary>Gets or sets the CIE XYZ 1931 color space for the red component.</summary>
    public CieXyz Red
    {
        readonly get => _cieXyzRed;
        set => _cieXyzRed = value;
    }

    /// <summary>Gets or sets the CIE XYZ 1931 color space for the green component.</summary>
    public CieXyz Green
    {
        readonly get => _cieXyzGreen;
        set => _cieXyzGreen = value;
    }

    /// <summary>Gets or sets the CIE XYZ 1931 color space for the blue component.</summary>
    public CieXyz Blue
    {
        readonly get => _cieXyzBlue;
        set => _cieXyzBlue = value;
    }

    /// <summary>Creates a color representation from red, green, and blue CIE XYZ components.</summary>
    /// <param name="red">The red CIE XYZ component.</param>
    /// <param name="green">The green CIE XYZ component.</param>
    /// <param name="blue">The blue CIE XYZ component.</param>
    /// <returns>The CIE XYZ triplet.</returns>
    public static CieXyzTriple Create(CieXyz red, CieXyz green, CieXyz blue) => new CieXyzTriple { _cieXyzRed = red, _cieXyzGreen = green, _cieXyzBlue = blue };

    /// <summary>Determines whether two CIE XYZ triplets are equal.</summary>
    /// <param name="left">The first CIE XYZ triplet.</param>
    /// <param name="right">The second CIE XYZ triplet.</param>
    /// <returns><see langword="true" /> when the triplets are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(CieXyzTriple left, CieXyzTriple right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two CIE XYZ triplets are not equal.</summary>
    /// <param name="left">The first CIE XYZ triplet.</param>
    /// <param name="right">The second CIE XYZ triplet.</param>
    /// <returns><see langword="true" /> when the triplets are not equal; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(CieXyzTriple left, CieXyzTriple right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public readonly bool Equals(CieXyzTriple other) => _cieXyzRed.Equals(other._cieXyzRed) && _cieXyzGreen.Equals(other._cieXyzGreen) && _cieXyzBlue.Equals(other._cieXyzBlue);

    /// <inheritdoc />
    public override readonly bool Equals(object obj) => obj is CieXyzTriple other && Equals(other);

    /// <inheritdoc />
    public override readonly int GetHashCode() => typeof(CieXyzTriple).GetHashCode();
}
