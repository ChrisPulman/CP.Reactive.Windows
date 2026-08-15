// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Drawing;
using CP.ReactiveUI.Primitives.Windows.Native.TypeConverters;

namespace CP.ReactiveUI.Primitives.Windows.Native.Structs;

/// <summary>
///     NativePoint represents the native POINT structure for calling native methods.
///     It has conversions from and to System.Drawing.Point or System.Windows.Point.
/// </summary>
/// <param name="x">The horizontal coordinate.</param>
/// <param name="y">The vertical coordinate.</param>
[Serializable]
[TypeConverter(typeof(NativePointTypeConverter))]
public readonly struct NativePoint(int x, int y) : IEquatable<NativePoint>
{
    /// <summary>Gets empty NativePoint.</summary>
    public static NativePoint Empty { get; }

    /// <summary>Gets the X coordinate.</summary>
    public int X { get; } = x;

    /// <summary>Gets the Y coordinate.</summary>
    public int Y { get; } = y;

    /// <summary>Converts a native point to a Windows point.</summary>
    /// <param name="point">NativePoint</param>
    public static implicit operator System.Windows.Point(NativePoint point)
    {
        return new(point.X, point.Y);
    }

    /// <summary>Converts a native point to a drawing point.</summary>
    /// <param name="point">NativePoint</param>
    public static implicit operator Point(NativePoint point)
    {
        return new(point.X, point.Y);
    }

    /// <summary>Converts a drawing point to a native point.</summary>
    /// <param name="point">System.Drawing.Point</param>
    public static implicit operator NativePoint(Point point)
    {
        return new(point.X, point.Y);
    }

    /// <summary>Converts a floating-point drawing point to a native point.</summary>
    /// <param name="point">System.Drawing.PointF</param>
    public static implicit operator NativePoint(PointF point)
    {
        return checked(new NativePoint((int)point.X, (int)point.Y));
    }

    /// <summary>Converts a floating-point native point to an integer native point.</summary>
    /// <param name="nativePointFloat">NativePointFloat</param>
    public static implicit operator NativePoint(NativePointFloat nativePointFloat)
    {
        return checked(new NativePoint((int)nativePointFloat.X, (int)nativePointFloat.Y));
    }

    /// <summary>Returns true when both native points use identical coordinates.</summary>
    /// <param name="point1">The left-hand native point for equality.</param>
    /// <param name="point2">The right-hand native point for equality.</param>
    /// <returns>true if both coordinates are equal.</returns>
    public static bool operator ==(NativePoint point1, NativePoint point2)
    {
        return point1.Equals(point2);
    }

    /// <summary>Returns true when either native point coordinate differs.</summary>
    /// <param name="point1">The left-hand native point for inequality.</param>
    /// <param name="point2">The right-hand native point for inequality.</param>
    /// <returns>true if either coordinate is different.</returns>
    public static bool operator !=(NativePoint point1, NativePoint point2)
    {
        return !point1.Equals(point2);
    }

    /// <inheritdoc />
    public override int GetHashCode() => (X * 397) ^ Y;

    /// <inheritdoc />
    public override string ToString() => $"{{X: {X}; Y: {Y};}}";

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (!(obj is NativePoint point))
        {
            return obj is Point drawingPoint && Equals(drawingPoint);
        }

        return Equals(point);
    }

    /// <inheritdoc />
    public bool Equals(NativePoint other) => X == other.X && Y == other.Y;

    /// <summary>Deconstructs this point into tuple coordinates.</summary>
    /// <param name="x">The horizontal coordinate.</param>
    /// <param name="y">The vertical coordinate.</param>
    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }

    /// <summary>Converts this value to a Windows point.</summary>
    /// <returns>A Windows point with the same coordinates.</returns>
    public System.Windows.Point ToPoint() => new(X, Y);

    /// <summary>Returns this value as a native point.</summary>
    /// <returns>The current native point.</returns>
    public NativePoint ToNativePoint() => this;
}
