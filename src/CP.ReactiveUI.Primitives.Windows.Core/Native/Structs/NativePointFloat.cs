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
[TypeConverter(typeof(NativePointFloatTypeConverter))]
public readonly struct NativePointFloat(float x, float y) : IEquatable<NativePointFloat>
{
    /// <summary>Gets empty NativePointFloat.</summary>
    public static NativePointFloat Empty { get; }

    /// <summary>Gets the X coordinate.</summary>
    public float X { get; } = x;

    /// <summary>Gets the Y coordinate.</summary>
    public float Y { get; } = y;

    /// <summary>Converts a floating-point native point to a Windows point.</summary>
    /// <param name="point">NativePointFloat</param>
    public static implicit operator System.Windows.Point(NativePointFloat point)
    {
        return new(point.X, point.Y);
    }

    /// <summary>Converts a Windows point to a floating-point native point.</summary>
    /// <param name="point">Point</param>
    public static implicit operator NativePointFloat(System.Windows.Point point)
    {
        return new((float)point.X, (float)point.Y);
    }

    /// <summary>Converts a floating-point native point to an integer drawing point.</summary>
    /// <param name="point">NativePointFloat</param>
    public static implicit operator Point(NativePointFloat point)
    {
        return checked(new Point((int)point.X, (int)point.Y));
    }

    /// <summary>Converts a floating-point native point to a drawing point.</summary>
    /// <param name="point">NativePointFloat</param>
    public static implicit operator PointF(NativePointFloat point)
    {
        return new(point.X, point.Y);
    }

    /// <summary>Converts an integer native point to a floating-point native point.</summary>
    /// <param name="point">NativePoint</param>
    public static implicit operator NativePointFloat(NativePoint point)
    {
        return new(point.X, point.Y);
    }

    /// <summary>Converts an integer drawing point to a floating-point native point.</summary>
    /// <param name="point">System.Drawing.Point</param>
    public static implicit operator NativePointFloat(Point point)
    {
        return new(point.X, point.Y);
    }

    /// <summary>Converts a drawing point to a floating-point native point.</summary>
    /// <param name="point">System.Drawing.PointF</param>
    public static implicit operator NativePointFloat(PointF point)
    {
        return new(point.X, point.Y);
    }

    /// <summary>Returns true when both floating-point native points use equivalent coordinates.</summary>
    /// <param name="lhs">NativePointFloat left hand side</param>
    /// <param name="rhs">NativePointFloat right hand side</param>
    /// <returns>bool true if the values are equal</returns>
    public static bool operator ==(NativePointFloat lhs, NativePointFloat rhs)
    {
        return lhs.Equals(rhs);
    }

    /// <summary>Returns true when either floating-point native point coordinate differs.</summary>
    /// <param name="lhs">NativePointFloat left hand side</param>
    /// <param name="rhs">NativePointFloat right hand side</param>
    /// <returns>bool true if the values are not equal</returns>
    public static bool operator !=(NativePointFloat lhs, NativePointFloat rhs)
    {
        return !lhs.Equals(rhs);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (!(obj is NativePointFloat nativePointFloat))
        {
            if (!(obj is Point drawingPoint))
            {
                if (!(obj is NativePoint nativePoint))
                {
                    return obj is System.Windows.Point point && Equals(point);
                }

                return Equals(nativePoint);
            }

            return Equals(drawingPoint);
        }

        return Equals(nativePointFloat);
    }

    /// <inheritdoc />
    public bool Equals(NativePointFloat other) => Math.Abs(X - other.X) < float.Epsilon && Math.Abs(Y - other.Y) < float.Epsilon;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(X, Y);

    /// <inheritdoc />
    public override string ToString() => $"{X},{Y}";

    /// <summary>Deconstructs this point into tuple coordinates.</summary>
    /// <param name="x">The horizontal coordinate.</param>
    /// <param name="y">The vertical coordinate.</param>
    public void Deconstruct(out float x, out float y)
    {
        x = X;
        y = Y;
    }

    /// <summary>Converts this value to a Windows point.</summary>
    /// <returns>A Windows point with the same coordinates.</returns>
    public System.Windows.Point ToPoint()
    {
        PointF point = ToPointF();
        return new(point.X, point.Y);
    }

    /// <summary>Returns this value as a floating-point native point.</summary>
    /// <returns>The current floating-point native point.</returns>
    public NativePointFloat ToNativePointFloat() => this;

    /// <summary>Converts this value to a drawing point.</summary>
    /// <returns>A drawing point with the same coordinates.</returns>
    public PointF ToPointF() => new(X, Y);
}
