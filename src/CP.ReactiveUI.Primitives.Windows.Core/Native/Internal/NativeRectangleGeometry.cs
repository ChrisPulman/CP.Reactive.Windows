// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.Internal;

/// <summary>Shared generic algorithms for integer and floating-point native rectangles.</summary>
/// <typeparam name="T">The coordinate type.</typeparam>
internal static class NativeRectangleGeometry<T>
    where T : struct, IComparable<T>
{
    /// <summary>Tests whether a value belongs to a half-open range.</summary>
    /// <param name="value">The value to test.</param>
    /// <param name="minimum">The inclusive lower bound.</param>
    /// <param name="maximum">The exclusive upper bound.</param>
    /// <returns><see langword="true" /> when the value is in the range.</returns>
    internal static bool IsBetween(T value, T minimum, T maximum) => NativeNumber<T>.GreaterThanOrEqual(value, minimum) && NativeNumber<T>.LessThan(value, maximum);

    /// <summary>Tests whether a point belongs to a rectangle.</summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <param name="x">The horizontal coordinate.</param>
    /// <param name="y">The vertical coordinate.</param>
    /// <returns><see langword="true" /> when the point belongs to the rectangle.</returns>
    internal static bool Contains(NativeRectangleBounds<T> rectangle, T x, T y) => IsBetween(x, rectangle.Left, rectangle.Right) && IsBetween(y, rectangle.Top, rectangle.Bottom);

    /// <summary>Tests whether a rectangle contains another rectangle.</summary>
    /// <param name="rectangle">The containing rectangle.</param>
    /// <param name="other">The rectangle to test.</param>
    /// <returns><see langword="true" /> when the other rectangle is contained.</returns>
    internal static bool Contains(NativeRectangleBounds<T> rectangle, NativeRectangleBounds<T> other) => NativeNumber<T>.LessThanOrEqual(rectangle.Left, other.Left) && NativeNumber<T>.LessThanOrEqual(other.Right, rectangle.Right) && NativeNumber<T>.LessThanOrEqual(rectangle.Top, other.Top) && NativeNumber<T>.LessThanOrEqual(other.Bottom, rectangle.Bottom);

    /// <summary>Changes a rectangle x-coordinate.</summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <param name="x">The replacement horizontal coordinate.</param>
    /// <returns>The updated rectangle.</returns>
    internal static NativeRectangleBounds<T> ChangeX(NativeRectangleBounds<T> rectangle, T x) => rectangle with
        {
            X = x
        };

    /// <summary>Changes a rectangle y-coordinate.</summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <param name="y">The replacement vertical coordinate.</param>
    /// <returns>The updated rectangle.</returns>
    internal static NativeRectangleBounds<T> ChangeY(NativeRectangleBounds<T> rectangle, T y) => rectangle with
        {
            Y = y
        };

    /// <summary>Changes a rectangle width.</summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <param name="width">The replacement width.</param>
    /// <returns>The updated rectangle.</returns>
    internal static NativeRectangleBounds<T> ChangeWidth(NativeRectangleBounds<T> rectangle, T width) => rectangle with
        {
            Width = width
        };

    /// <summary>Changes a rectangle height.</summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <param name="height">The replacement height.</param>
    /// <returns>The updated rectangle.</returns>
    internal static NativeRectangleBounds<T> ChangeHeight(NativeRectangleBounds<T> rectangle, T height) => rectangle with
        {
            Height = height
        };

    /// <summary>Tests whether two rectangles overlap according to native rectangle semantics.</summary>
    /// <param name="first">The first rectangle.</param>
    /// <param name="second">The second rectangle.</param>
    /// <returns><see langword="true" /> when the rectangles overlap.</returns>
    internal static bool HasOverlap(NativeRectangleBounds<T> first, NativeRectangleBounds<T> second)
    {
        if (IsAdjacent(first, second) != AdjacentTo.None)
        {
            return true;
        }

        bool num = IsBetween(first.X, second.Left, second.Right) || IsBetween(second.X, first.Left, first.Right);
        bool verticalOverlap = IsBetween(first.Y, second.Y, NativeNumber<T>.Add(second.Y, second.Height)) || IsBetween(second.Y, first.Y, NativeNumber<T>.Add(first.Y, first.Height));
        bool firstContainsSecond = Contains(first, second);
        bool secondContainsFirst = NativeNumber<T>.LessThanOrEqual(second.Left, first.Left) && NativeNumber<T>.LessThanOrEqual(first.Right, second.Right) && NativeNumber<T>.LessThanOrEqual(second.Top, first.Top) && NativeNumber<T>.LessThanOrEqual(first.Bottom, second.Bottom);
        return num && verticalOverlap && !(firstContainsSecond || secondContainsFirst);
    }

    /// <summary>Inflates a rectangle along both axes.</summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <param name="width">The horizontal inflation amount.</param>
    /// <param name="height">The vertical inflation amount.</param>
    /// <returns>The inflated rectangle.</returns>
    internal static NativeRectangleBounds<T> Inflate(NativeRectangleBounds<T> rectangle, T width, T height) => new(NativeNumber<T>.Subtract(rectangle.X, width), NativeNumber<T>.Subtract(rectangle.Y, height), NativeNumber<T>.Add(NativeNumber<T>.Add(rectangle.Width, width), width), NativeNumber<T>.Add(NativeNumber<T>.Add(rectangle.Height, height), height));

    /// <summary>Builds the normalized intersection of two rectangles.</summary>
    /// <param name="rectangle">The first rectangle.</param>
    /// <param name="other">The second rectangle.</param>
    /// <returns>The normalized intersection.</returns>
    internal static NativeRectangleBounds<T> Intersect(NativeRectangleBounds<T> rectangle, NativeRectangleBounds<T> other)
    {
        T x5 = NativeNumber<T>.Max(rectangle.Left, other.Left);
        T y5 = NativeNumber<T>.Max(rectangle.Top, other.Top);
        T x6 = NativeNumber<T>.Min(rectangle.Right, other.Right);
        T y6 = NativeNumber<T>.Min(rectangle.Bottom, other.Bottom);
        return !NativeNumber<T>.GreaterThan(x5, x6) && !NativeNumber<T>.GreaterThan(y5, y6) ? Normalize(new(x5, y6, NativeNumber<T>.Subtract(x6, x5), NativeNumber<T>.Subtract(y5, y6))) : default(NativeRectangleBounds<T>);
    }

    /// <summary>Builds the shared area of two normalized rectangles.</summary>
    /// <param name="rectangle">The first rectangle.</param>
    /// <param name="other">The second rectangle.</param>
    /// <returns>The shared rectangle.</returns>
    internal static NativeRectangleBounds<T> Intersect2(NativeRectangleBounds<T> rectangle, NativeRectangleBounds<T> other)
    {
        rectangle = Normalize(rectangle);
        other = Normalize(other);
        T sharedLeft = NativeNumber<T>.Max(rectangle.Left, other.Left);
        T sharedRight = NativeNumber<T>.Min(rectangle.Right, other.Right);
        T sharedBottom = NativeNumber<T>.Max(rectangle.Bottom, other.Bottom);
        T sharedTop = NativeNumber<T>.Min(rectangle.Top, other.Top);
        return new(sharedLeft, sharedTop, NativeNumber<T>.Subtract(sharedRight, sharedLeft), NativeNumber<T>.Subtract(sharedBottom, sharedTop));
    }

    /// <summary>Tests whether two rectangles intersect.</summary>
    /// <param name="rectangle">The first rectangle.</param>
    /// <param name="other">The second rectangle.</param>
    /// <returns><see langword="true" /> when the rectangles intersect.</returns>
    internal static bool IntersectsWith(NativeRectangleBounds<T> rectangle, NativeRectangleBounds<T> other) => NativeNumber<T>.LessThan(other.X, NativeNumber<T>.Add(rectangle.X, rectangle.Width)) && NativeNumber<T>.LessThan(rectangle.X, NativeNumber<T>.Add(other.X, other.Width)) && NativeNumber<T>.LessThan(other.Y, NativeNumber<T>.Add(rectangle.Y, rectangle.Height)) && NativeNumber<T>.LessThan(rectangle.Y, NativeNumber<T>.Add(other.Y, other.Height));

    /// <summary>Determines the side of a rectangle adjacent to another rectangle.</summary>
    /// <param name="rectangle">The first rectangle.</param>
    /// <param name="other">The second rectangle.</param>
    /// <returns>The adjacent side, if any.</returns>
    internal static AdjacentTo IsAdjacent(NativeRectangleBounds<T> rectangle, NativeRectangleBounds<T> other)
    {
        bool hasVerticalOverlap = IsBetween(rectangle.Top, other.Top, other.Bottom) || IsBetween(other.Top, rectangle.Top, rectangle.Bottom);
        bool hasHorizontalOverlap = IsBetween(rectangle.Left, other.Left, other.Right) || IsBetween(other.Left, rectangle.Left, rectangle.Right);
        bool num = EqualityComparer<T>.Default.Equals(rectangle.Left, other.Right) && hasVerticalOverlap;
        bool flag = EqualityComparer<T>.Default.Equals(rectangle.Right, other.Left) && hasVerticalOverlap;
        bool flag2 = EqualityComparer<T>.Default.Equals(rectangle.Top, other.Bottom) && hasHorizontalOverlap;
        bool flag3 = EqualityComparer<T>.Default.Equals(rectangle.Bottom, other.Top) && hasHorizontalOverlap;
        if (!num)
        {
            if (!flag)
            {
                if (!flag2)
                {
                    return flag3 ? AdjacentTo.Bottom : AdjacentTo.None;
                }

                return AdjacentTo.Top;
            }

            return AdjacentTo.Right;
        }

        return AdjacentTo.Left;
    }

    /// <summary>Offsets a rectangle.</summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <param name="x">The horizontal offset.</param>
    /// <param name="y">The vertical offset.</param>
    /// <returns>The offset rectangle.</returns>
    internal static NativeRectangleBounds<T> Offset(NativeRectangleBounds<T> rectangle, T x, T y) => new(NativeNumber<T>.Add(rectangle.X, x), NativeNumber<T>.Add(rectangle.Y, y), rectangle.Width, rectangle.Height);

    /// <summary>Moves a rectangle while preserving its size.</summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <param name="x">The horizontal destination.</param>
    /// <param name="y">The vertical destination.</param>
    /// <returns>The moved rectangle.</returns>
    internal static NativeRectangleBounds<T> MoveTo(NativeRectangleBounds<T> rectangle, T x, T y) => new(x, y, rectangle.Width, rectangle.Height);

    /// <summary>Resizes a rectangle while preserving its location.</summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <param name="width">The replacement width.</param>
    /// <param name="height">The replacement height.</param>
    /// <returns>The resized rectangle.</returns>
    internal static NativeRectangleBounds<T> Resize(NativeRectangleBounds<T> rectangle, T width, T height) => new(rectangle.X, rectangle.Y, width, height);

    /// <summary>Normalizes negative rectangle dimensions.</summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <returns>The normalized rectangle.</returns>
    internal static NativeRectangleBounds<T> Normalize(NativeRectangleBounds<T> rectangle)
    {
        NativeRectangleBounds<T> nativeRectangleBounds = rectangle;
        var (x, y, width, height) = nativeRectangleBounds;
        if (NativeNumber<T>.LessThan(width, NativeNumber<T>.Zero))
        {
            x = NativeNumber<T>.Add(x, width);
            width = NativeNumber<T>.Abs(width);
        }

        if (NativeNumber<T>.LessThan(height, NativeNumber<T>.Zero))
        {
            y = NativeNumber<T>.Add(y, height);
            height = NativeNumber<T>.Abs(height);
        }

        return new(x, y, width, height);
    }

    /// <summary>Converts bounds to a drawing rectangle.</summary>
    /// <param name="rectangle">The bounds.</param>
    /// <returns>The drawing rectangle.</returns>
    internal static Rectangle ToRectangle(NativeRectangleBounds<T> rectangle) => Rectangle.Truncate(ToRectangleF(rectangle));

    /// <summary>Converts bounds to a floating-point drawing rectangle.</summary>
    /// <param name="rectangle">The bounds.</param>
    /// <returns>The floating-point drawing rectangle.</returns>
    internal static RectangleF ToRectangleF(NativeRectangleBounds<T> rectangle) => new(NativeNumber<T>.ToSingle(rectangle.X), NativeNumber<T>.ToSingle(rectangle.Y), NativeNumber<T>.ToSingle(rectangle.Width), NativeNumber<T>.ToSingle(rectangle.Height));

    /// <summary>Converts bounds to a Windows integer rectangle.</summary>
    /// <param name="rectangle">The bounds.</param>
    /// <returns>The Windows integer rectangle.</returns>
    internal static Int32Rect ToInt32Rect(NativeRectangleBounds<T> rectangle)
    {
        Rectangle drawingRectangle = ToRectangle(rectangle);
        return new(drawingRectangle.X, drawingRectangle.Y, drawingRectangle.Width, drawingRectangle.Height);
    }

    /// <summary>Converts bounds to a Windows rectangle.</summary>
    /// <param name="rectangle">The bounds.</param>
    /// <returns>The Windows rectangle.</returns>
    internal static Rect ToRect(NativeRectangleBounds<T> rectangle)
    {
        RectangleF drawingRectangle = ToRectangleF(rectangle);
        return new(drawingRectangle.X, drawingRectangle.Y, drawingRectangle.Width, drawingRectangle.Height);
    }

    /// <summary>Transforms the two corners used by the native rectangle transform contract.</summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <param name="matrix">The transform matrix.</param>
    /// <returns>The transformed corners.</returns>
    internal static NativeRectangleCorners Transform(NativeRectangleBounds<T> rectangle, Matrix matrix)
    {
        System.Windows.Point[] points = new System.Windows.Point[2]
        {
            new System.Windows.Point(NativeNumber<T>.ToDouble(rectangle.X), NativeNumber<T>.ToDouble(rectangle.Y)),
            new System.Windows.Point(NativeNumber<T>.ToDouble(rectangle.Right), NativeNumber<T>.ToDouble(rectangle.Bottom))
        };
        matrix.Transform(points);
        return new((float)points[0].X, (float)points[0].Y, (float)points[1].X, (float)points[1].Y);
    }

    /// <summary>Builds the smallest rectangle containing both inputs.</summary>
    /// <param name="rectangle">The first rectangle.</param>
    /// <param name="other">The second rectangle.</param>
    /// <returns>The containing rectangle.</returns>
    internal static NativeRectangleBounds<T> Union(NativeRectangleBounds<T> rectangle, NativeRectangleBounds<T> other)
    {
        T minX = NativeNumber<T>.Min(NativeNumber<T>.Min(rectangle.Left, rectangle.Right), NativeNumber<T>.Min(other.Left, other.Right));
        T maxX = NativeNumber<T>.Max(NativeNumber<T>.Max(rectangle.Left, rectangle.Right), NativeNumber<T>.Max(other.Left, other.Right));
        T minY = NativeNumber<T>.Min(NativeNumber<T>.Min(rectangle.Top, rectangle.Bottom), NativeNumber<T>.Min(other.Top, other.Bottom));
        T maxY = NativeNumber<T>.Max(NativeNumber<T>.Max(rectangle.Top, rectangle.Bottom), NativeNumber<T>.Max(other.Top, other.Bottom));
        return new(minX, minY, NativeNumber<T>.Subtract(maxX, minX), NativeNumber<T>.Subtract(maxY, minY));
    }
}
