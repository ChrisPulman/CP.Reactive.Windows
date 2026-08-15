// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Structs;

/// <summary>Contains members that specify the nature of a Gaussian blur.</summary>
/// <remarks>Cannot be pinned with GCHandle due to bool value.</remarks>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BlurParams : IEquatable<BlurParams>
{
    /// <summary>The blur radius, in pixels.</summary>
    private float _radius;

    /// <summary>Whether the bitmap expands by an amount equal to the blur radius.</summary>
    private bool _expandEdges;

    /// <summary>Creates blur parameters.</summary>
    /// <param name="radius">
    ///     Real number that specifies the blur radius (the radius of the Gaussian convolution kernel) in
    ///     pixels. The radius must be in the range 0 through 255. As the radius increases, the resulting
    ///     bitmap becomes more blurry.
    /// </param>
    /// <param name="expandEdges">
    ///     Boolean value that specifies whether the bitmap expands by an amount equal to the blur radius.
    ///     If TRUE, the bitmap expands by an amount equal to the radius so that it can have soft edges.
    ///     If FALSE, the bitmap remains the same size and the soft edges are clipped.
    /// </param>
    /// <returns>The blur parameters.</returns>
    public static BlurParams Create(float radius, bool expandEdges) =>
        new BlurParams { _radius = radius, _expandEdges = expandEdges };

    /// <summary>Determines whether two blur parameters are equal.</summary>
    /// <param name="left">The first blur parameters.</param>
    /// <param name="right">The second blur parameters.</param>
    /// <returns><see langword="true" /> when the parameters are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(BlurParams left, BlurParams right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two blur parameters are not equal.</summary>
    /// <param name="left">The first blur parameters.</param>
    /// <param name="right">The second blur parameters.</param>
    /// <returns><see langword="true" /> when the parameters are not equal; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(BlurParams left, BlurParams right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public readonly bool Equals(BlurParams other) =>
        _radius.Equals(other._radius) && _expandEdges == other._expandEdges;

    /// <inheritdoc />
    public override readonly bool Equals(object obj) => obj is BlurParams other && Equals(other);

    /// <inheritdoc />
    public override readonly int GetHashCode() => typeof(BlurParams).GetHashCode();
}
