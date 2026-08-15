// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing;
using System.Windows;
using CP.ReactiveUI.Primitives.Windows.Native.TypeConverters;

namespace CP.ReactiveUI.Primitives.Windows.Native.Structs;

/// <summary>
/// NativeRect represents the native RECTF structure for calling native methods.
/// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms534497(v=vs.85).aspx">RectF class</a>.
/// It has conversions from and to System.Drawing.RectangleF or System.Windows.Rect.
/// </summary>
[Serializable]
[TypeConverter(typeof(NativeRectFloatTypeConverter))]
public readonly struct NativeRectFloat : IEquatable<NativeRectFloat>
{
    /// <summary>The stored x-coordinate.</summary>
    private readonly float _x;

    /// <summary>The stored y-coordinate.</summary>
    private readonly float _y;

    /// <summary>The stored width.</summary>
    private readonly float _width;

    /// <summary>The stored height.</summary>
    private readonly float _height;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeRectFloat" /> struct.</summary>
    /// <param name="left">The left edge.</param>
    /// <param name="top">The top edge.</param>
    /// <param name="width">The rectangle width.</param>
    /// <param name="height">The rectangle height.</param>
    public NativeRectFloat(float left, float top, float width, float height)
    {
        _x = left;
        _y = top;
        _width = width;
        _height = height;
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeRectFloat" /> struct.</summary>
    /// <param name="x">The horizontal origin.</param>
    /// <param name="y">The vertical origin.</param>
    /// <param name="nativeSizeFloat">The floating-point size.</param>
    public NativeRectFloat(float x, float y, NativeSizeFloat nativeSizeFloat)
    {
        _x = x;
        _y = y;
        _width = nativeSizeFloat.Width;
        _height = nativeSizeFloat.Height;
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeRectFloat" /> struct.</summary>
    /// <param name="topLeft">The upper-left corner.</param>
    /// <param name="bottomRight">The lower-right corner.</param>
    public NativeRectFloat(NativePointFloat topLeft, NativePointFloat bottomRight)
        : this(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y) { }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeRectFloat" /> struct.</summary>
    /// <param name="location">The floating-point location.</param>
    /// <param name="nativeSizeFloat">The floating-point size.</param>
    public NativeRectFloat(NativePointFloat location, NativeSizeFloat nativeSizeFloat)
    {
        _x = location.X;
        _y = location.Y;
        _width = nativeSizeFloat.Width;
        _height = nativeSizeFloat.Height;
    }

    /// <summary>Gets the empty floating-point native rectangle.</summary>
    public static NativeRectFloat Empty { get; }

    /// <summary>Gets x value.</summary>
    public float X => _x;

    /// <summary>Gets x location of the rectangle.</summary>
    public float Y => _y;

    /// <summary>Gets left value of the rectangle.</summary>
    public float Left => _x;

    /// <summary>Gets top of the rectangle.</summary>
    public float Top => _y;

    /// <summary>Gets right of the rectangle.</summary>
    public float Right => _x + _width;

    /// <summary>Gets bottom of the rectangle.</summary>
    public float Bottom => _y + _height;

    /// <summary>Gets heigh of the NativeRectFloat.</summary>
    public float Height => _height;

    /// <summary>Gets width of the NativeRectFloat.</summary>
    public float Width => _width;

    /// <summary>Gets coordinates of the bottom left.</summary>
    public NativePointFloat BottomLeft => new(X, Y + Height);

    /// <summary>Gets coordinates of the top left.</summary>
    public NativePointFloat TopLeft => new(X, Y);

    /// <summary>Gets coordinates of the bottom right.</summary>
    public NativePointFloat BottomRight => new(X + Width, Y + Height);

    /// <summary>Gets coordinates of the top right.</summary>
    public NativePointFloat TopRight => new(X + Width, Y);

    /// <summary>Gets location for this NativeRectFloat.</summary>
    public NativePointFloat Location => new(Left, Top);

    /// <summary>Gets size for this NativeRectFloat.</summary>
    public NativeSizeFloat Size => new(Width, Height);

    /// <summary>Gets a value indicating whether this floating-point rectangle has zero area.</summary>
    public bool IsEmpty => Math.Abs(_width * _height) < float.Epsilon;

    /// <summary>Cast NativeRect to NativeRectFloat</summary>
    /// <param name="rectangle">NativeRect</param>
    /// <returns>NativeRectFloat</returns>
    public static implicit operator NativeRectFloat(NativeRect rectangle)
    {
        return new(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
    }

    /// <summary>Cast Rect to NativeRectFloat</summary>
    /// <param name="rectangle">Rect</param>
    /// <returns>NativeRectFloat</returns>
    public static implicit operator NativeRectFloat(Rect rectangle)
    {
        return new(
            (float)rectangle.Left,
            (float)rectangle.Top,
            (float)rectangle.Width,
            (float)rectangle.Height);
    }

    /// <summary>Cast Int32Rect to NativeRectFloat</summary>
    /// <param name="rectangle">Int32Rect</param>
    /// <returns>NativeRectFloat</returns>
    public static implicit operator NativeRectFloat(Int32Rect rectangle)
    {
        return new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }

    /// <summary>Cast NativeRectFloat to Rect</summary>
    /// <param name="rectangle">NativeRectFloat</param>
    /// <returns>Rect</returns>
    public static implicit operator Rect(NativeRectFloat rectangle)
    {
        return rectangle.ToRect();
    }

    /// <summary>Cast NativeRectFloat to Int32Rect</summary>
    /// <param name="rectangle">NativeRectFloat</param>
    /// <returns>Int32Rect</returns>
    public static implicit operator Int32Rect(NativeRectFloat rectangle)
    {
        return rectangle.ToInt32Rect();
    }

    /// <summary>Cast RectangleF to NativeRectFloat</summary>
    /// <param name="rectangle">RectangleF</param>
    /// <returns>NativeRectFloat</returns>
    public static implicit operator NativeRectFloat(RectangleF rectangle)
    {
        return new(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
    }

    /// <summary>Cast Rectangle to NativeRectFloat</summary>
    /// <param name="rectangle">Rectangle</param>
    /// <returns>NativeRectFloat</returns>
    public static implicit operator NativeRectFloat(Rectangle rectangle)
    {
        return new(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
    }

    /// <summary>Cast NativeRectFloat to NativeRect</summary>
    /// <param name="rectangle">NativeRectFloat</param>
    /// <returns>NativeRect</returns>
    public static implicit operator NativeRect(NativeRectFloat rectangle)
    {
        return rectangle.ToNativeRect();
    }

    /// <summary>Cast NativeRectFloat to RectangleF</summary>
    /// <param name="rectangle">NativeRectFloat</param>
    /// <returns>RectangleF</returns>
    public static implicit operator RectangleF(NativeRectFloat rectangle)
    {
        return rectangle.ToRectangleF();
    }

    /// <summary>Cast NativeRectFloat to Rectangle</summary>
    /// <param name="rectangle">NativeRectFloat</param>
    /// <returns>Rectangle</returns>
    public static implicit operator Rectangle(NativeRectFloat rectangle)
    {
        return rectangle.ToRectangle();
    }

    /// <summary>Compares two floating-point native rectangles for equality.</summary>
    /// <param name="rectangle1">The left-hand floating-point rectangle for equality.</param>
    /// <param name="rectangle2">The right-hand floating-point rectangle for equality.</param>
    /// <returns>true if all floating-point bounds are equal.</returns>
    public static bool operator ==(NativeRectFloat rectangle1, NativeRectFloat rectangle2)
    {
        return rectangle1.Equals(rectangle2);
    }

    /// <summary>Compares two floating-point native rectangles for inequality.</summary>
    /// <param name="rectangle1">The left-hand floating-point rectangle for inequality.</param>
    /// <param name="rectangle2">The right-hand floating-point rectangle for inequality.</param>
    /// <returns>true if any floating-point bound is different.</returns>
    public static bool operator !=(NativeRectFloat rectangle1, NativeRectFloat rectangle2)
    {
        return !rectangle1.Equals(rectangle2);
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{{Left: {_x}; Top: {_y}; Width: {_width}; Height: {_height};}}";

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (!(obj is NativeRectFloat f))
        {
            return !(obj is Rect rect1)
                ? obj is RectangleF rectangleF && Equals(rectangleF)
                : Equals(rect1);
        }

        return Equals(f);
    }

    /// <inheritdoc />
    public bool Equals(NativeRectFloat other) =>
        Math.Abs(other._x - _x) < float.Epsilon
        && Math.Abs(other._y - _y) < float.Epsilon
        && Math.Abs(other._width - _width) < float.Epsilon
        && Math.Abs(other._height - _height) < float.Epsilon;

    /// <inheritdoc />
    public override int GetHashCode() => System.HashCode.Combine(_x, _x, _y, _width, _height);

    /// <summary>Test if this NativeRectFloat contains the specified NativePoint.</summary>
    /// <param name="point">NativePoint</param>
    /// <returns>true if it contains.</returns>
    public bool Contains(NativePoint point) =>
        (float)point.X >= Left
        && (float)point.X <= Right
        && (float)point.Y >= Top
        && (float)point.Y <= Bottom;

    /// <summary>Deconstructs this floating-point native rectangle into location and size values.</summary>
    /// <param name="location">NativePointFloat</param>
    /// <param name="size">NativeSizeFloat</param>
    public void Deconstruct(out NativePointFloat location, out NativeSizeFloat size)
    {
        location = Location;
        size = Size;
    }

    /// <summary>Returns this value as a floating-point native rectangle.</summary>
    /// <returns>The current floating-point native rectangle.</returns>
    public NativeRectFloat ToNativeRectFloat() => this;

    /// <summary>Converts this value to an integer native rectangle.</summary>
    /// <returns>An integer native rectangle with truncated bounds.</returns>
    public NativeRect ToNativeRect() =>
        NativeRectangleConversions.ToNativeRect(NativeRectangleConversions.ToBounds(this));

    /// <summary>Converts this value to a drawing rectangle with floating-point dimensions.</summary>
    /// <returns>A drawing rectangle with the same bounds.</returns>
    public RectangleF ToRectangleF() =>
        NativeRectangleGeometry<float>.ToRectangleF(NativeRectangleConversions.ToBounds(this));

    /// <summary>Converts this value to a drawing rectangle.</summary>
    /// <returns>A drawing rectangle with truncated bounds.</returns>
    public Rectangle ToRectangle() =>
        NativeRectangleGeometry<float>.ToRectangle(NativeRectangleConversions.ToBounds(this));

    /// <summary>Converts this value to a Windows rectangle.</summary>
    /// <returns>A Windows rectangle with the same bounds.</returns>
    public Rect ToRect() =>
        NativeRectangleGeometry<float>.ToRect(NativeRectangleConversions.ToBounds(this));

    /// <summary>Converts this value to a Windows integer rectangle.</summary>
    /// <returns>A Windows integer rectangle with truncated bounds.</returns>
    public Int32Rect ToInt32Rect() =>
        NativeRectangleGeometry<float>.ToInt32Rect(NativeRectangleConversions.ToBounds(this));
}
