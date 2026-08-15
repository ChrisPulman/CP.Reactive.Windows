// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using CP.ReactiveUI.Primitives.Windows.Native.Internal;
using CP.ReactiveUI.Primitives.Windows.Native.TypeConverters;

namespace CP.ReactiveUI.Primitives.Windows.Native.Structs;

/// <summary>
///     NativeRect represents the native RECT structure for calling native methods.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/dd162897.aspx">RECT struct</a>
///     It has conversions from and to System.Drawing.Rectangle or System.Windows.Rect.
/// </summary>
[Serializable]
[TypeConverter(typeof(NativeRectTypeConverter))]
public readonly struct NativeRect : IEquatable<NativeRect>
{
    /// <summary>The stored left coordinate.</summary>
    private readonly int _left;

    /// <summary>The stored top coordinate.</summary>
    private readonly int _top;

    /// <summary>The stored right coordinate.</summary>
    private readonly int _right;

    /// <summary>The stored bottom coordinate.</summary>
    private readonly int _bottom;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeRect" /> struct.</summary>
    /// <param name="left">The left edge.</param>
    /// <param name="top">The top edge.</param>
    /// <param name="width">The rectangle width.</param>
    /// <param name="height">The rectangle height.</param>
    public NativeRect(int left, int top, int width, int height)
    {
        _left = left;
        _top = top;
        checked
        {
            _right = left + width;
            _bottom = top + height;
        }
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeRect" /> struct.</summary>
    /// <param name="location">NativePoint</param>
    /// <param name="size">NativeSize</param>
    public NativeRect(NativePoint location, NativeSize size)
    {
        _left = location.X;
        _top = location.Y;
        checked
        {
            _right = _left + size.Width;
            _bottom = _top + size.Height;
        }
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeRect" /> struct.</summary>
    /// <param name="topLeft">The upper-left corner.</param>
    /// <param name="bottomRight">The lower-right corner.</param>
    public NativeRect(NativePoint topLeft, NativePoint bottomRight)
    {
        this = checked(
            new NativeRect(
                topLeft.X,
                topLeft.Y,
                bottomRight.X - topLeft.X,
                bottomRight.Y - topLeft.Y));
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeRect" /> struct.</summary>
    /// <param name="left">The left edge.</param>
    /// <param name="top">The top edge.</param>
    /// <param name="size">NativeSize</param>
    public NativeRect(int left, int top, NativeSize size)
        : this(new NativePoint(left, top), size) { }

    /// <summary>Gets the empty native rectangle.</summary>
    public static NativeRect Empty { get; }

    /// <summary>Gets sizeOf for this struct.</summary>
    public static int SizeOf => Marshal.SizeOf<NativeRect>();

    /// <summary>Gets x value.</summary>
    public int X => _left;

    /// <summary>Gets x location of the NativeRect.</summary>
    public int Y => _top;

    /// <summary>Gets left value of the NativeRect.</summary>
    public int Left => _left;

    /// <summary>Gets top of the NativeRect.</summary>
    public int Top => _top;

    /// <summary>Gets right of the NativeRect.</summary>
    public int Right => _right;

    /// <summary>Gets bottom of the NativeRect.</summary>
    public int Bottom => _bottom;

    /// <summary>Gets height of the NativeRect.</summary>
    public int Height => _bottom - _top;

    /// <summary>Gets width of the NativeRect.</summary>
    public int Width => _right - _left;

    /// <summary>Gets location of this NativeRect.</summary>
    public NativePoint Location => new(Left, Top);

    /// <summary>Gets size for this NativeRect.</summary>
    public NativeSize Size => new(Width, Height);

    /// <summary>Gets coordinates of the bottom left.</summary>
    public NativePoint BottomLeft => new(X, checked(Y + Height));

    /// <summary>Gets coordinates of the top left.</summary>
    public NativePoint TopLeft => new(X, Y);

    /// <summary>Gets coordinates of the bottom right.</summary>
    public NativePoint BottomRight => checked(new NativePoint(X + Width, Y + Height));

    /// <summary>Gets coordinates of the top right.</summary>
    public NativePoint TopRight => new(checked(X + Width), Y);

    /// <summary>Gets a value indicating whether this native rectangle has zero area.</summary>
    public bool IsEmpty => Width * Height == 0;

    /// <summary>Cast NativeRect to Rect</summary>
    /// <param name="rectangle">NativeRect</param>
    /// <returns>Rect</returns>
    public static implicit operator Rect(NativeRect rectangle)
    {
        return rectangle.ToRect();
    }

    /// <summary>Cast NativeRect to Int32Rect</summary>
    /// <param name="rectangle">NativeRect</param>
    /// <returns>Int32Rect</returns>
    public static implicit operator Int32Rect(NativeRect rectangle)
    {
        return rectangle.ToInt32Rect();
    }

    /// <summary>Cast Int32Rect to NativeRect</summary>
    /// <param name="rectangle">Int32Rect</param>
    /// <returns>NativeRect</returns>
    public static implicit operator NativeRect(Int32Rect rectangle)
    {
        return new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }

    /// <summary>Cast NativeRect to RectangleF</summary>
    /// <param name="rectangle">NativeRect</param>
    /// <returns>RectangleF</returns>
    public static implicit operator RectangleF(NativeRect rectangle)
    {
        return rectangle.ToRectangleF();
    }

    /// <summary>Cast NativeRect to Rectangle</summary>
    /// <param name="rectangle">NativeRect</param>
    /// <returns>Rectangle</returns>
    public static implicit operator Rectangle(NativeRect rectangle)
    {
        return rectangle.ToRectangle();
    }

    /// <summary>Cast Rectangle to NativeRect</summary>
    /// <param name="rectangle">Rectangle</param>
    /// <returns>NativeRect</returns>
    public static implicit operator NativeRect(Rectangle rectangle)
    {
        return new(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
    }

    /// <summary>Compares two native rectangles for equality.</summary>
    /// <param name="rectangle1">The left-hand native rectangle for equality.</param>
    /// <param name="rectangle2">The right-hand native rectangle for equality.</param>
    /// <returns>true if all rectangle edges are equal.</returns>
    public static bool operator ==(NativeRect rectangle1, NativeRect rectangle2)
    {
        return rectangle1.Equals(rectangle2);
    }

    /// <summary>Not is operator</summary>
    /// <param name="rectangle1"></param>
    /// <param name="rectangle2"></param>
    /// <returns>bool</returns>
    public static bool operator !=(NativeRect rectangle1, NativeRect rectangle2)
    {
        return !rectangle1.Equals(rectangle2);
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{{Left: {_left}; Top: {_top}; Width: {Width}; Height: {Height};}}";

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (!(obj is NativeRect nativeRect))
        {
            if (obj is Rectangle rectangle)
            {
                NativeRect rect = rectangle;
                return Equals(rect);
            }

            return false;
        }

        return Equals(nativeRect);
    }

    /// <inheritdoc />
    public bool Equals(NativeRect other) =>
        other.Left == _left
        && other.Top == _top
        && other.Right == _right
        && other.Bottom == _bottom;

    /// <inheritdoc />
    public override int GetHashCode() =>
        (((((_left * 397) ^ _top) * 397) ^ _right) * 397) ^ _bottom;

    /// <summary>Deconstructs this native rectangle into location and size values.</summary>
    /// <param name="location">NativePoint</param>
    /// <param name="size">NativeSize</param>
    public void Deconstruct(out NativePoint location, out NativeSize size)
    {
        location = Location;
        size = Size;
    }

    /// <summary>Converts this value to a drawing rectangle with floating-point dimensions.</summary>
    /// <returns>A drawing rectangle with the same bounds.</returns>
    public RectangleF ToRectangleF() =>
        NativeRectangleGeometry<int>.ToRectangleF(NativeRectangleConversions.ToBounds(this));

    /// <summary>Converts this value to a drawing rectangle.</summary>
    /// <returns>A drawing rectangle with the same bounds.</returns>
    public Rectangle ToRectangle() =>
        NativeRectangleGeometry<int>.ToRectangle(NativeRectangleConversions.ToBounds(this));

    /// <summary>Converts this value to a Windows rectangle.</summary>
    /// <returns>A Windows rectangle with the same bounds.</returns>
    public Rect ToRect() =>
        NativeRectangleGeometry<int>.ToRect(NativeRectangleConversions.ToBounds(this));

    /// <summary>Converts this value to a Windows integer rectangle.</summary>
    /// <returns>A Windows integer rectangle with the same bounds.</returns>
    public Int32Rect ToInt32Rect() =>
        NativeRectangleGeometry<int>.ToInt32Rect(NativeRectangleConversions.ToBounds(this));

    /// <summary>Returns this value as a native rectangle.</summary>
    /// <returns>The current native rectangle.</returns>
    public NativeRect ToNativeRect() => this;
}
