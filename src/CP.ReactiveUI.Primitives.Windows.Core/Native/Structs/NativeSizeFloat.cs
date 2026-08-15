// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Drawing;
using CP.ReactiveUI.Primitives.Windows.Native.TypeConverters;

namespace CP.ReactiveUI.Primitives.Windows.Native.Structs;

/// <summary>This structure should be used everywhere where native methods need a size-f struct.</summary>
[Serializable]
[TypeConverter(typeof(NativeSizeFloatTypeConverter))]
public readonly struct NativeSizeFloat : IEquatable<NativeSizeFloat>, IComparable<NativeSizeFloat>
{
    /// <summary>The stored width.</summary>
    private readonly float _width;

    /// <summary>The stored height.</summary>
    private readonly float _height;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeSizeFloat" /> struct.</summary>
    /// <param name="size">The drawing size to copy.</param>
    public NativeSizeFloat(Size size)
        : this(size.Width, size.Height)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeSizeFloat" /> struct.</summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public NativeSizeFloat(float width, float height)
    {
        _width = width;
        _height = height;
    }

    /// <summary>Gets the zero-width and zero-height floating-point native size.</summary>
    public static NativeSizeFloat Empty { get; } = new(0F, 0F);

    /// <summary>Gets the Width of the size struct.</summary>
    public float Width => _width;

    /// <summary>Gets the Height of the size struct.</summary>
    public float Height => _height;

    /// <summary>Gets a value indicating whether the area is zero.</summary>
    /// <returns>true if the size is empty.</returns>
    public bool IsEmpty => Math.Abs(_width * _height) < float.Epsilon;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeSizeFloat" /> struct.</summary>
    /// <param name="width">The double-precision width.</param>
    /// <param name="height">The double-precision height.</param>
    public NativeSizeFloat(double width, double height)
    {
        _width = (float)width;
        _height = (float)height;
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeSizeFloat" /> struct.</summary>
    /// <param name="size">The Windows size to copy.</param>
    public NativeSizeFloat(System.Windows.Size size)
        : this((float)size.Width, (float)size.Height)
    {
    }

    /// <summary>Implicit cast from NativeSizeFloat to System.Windows.Size</summary>
    /// <param name="size">NativeSize</param>
    /// <returns>System.Windows.Size</returns>
    public static implicit operator System.Windows.Size(NativeSizeFloat size)
    {
        return new(size.Width, size.Height);
    }

    /// <summary>Implicit cast from Size to NativeSizeFloat</summary>
    /// <param name="size">System.Windows.Size</param>
    /// <returns>NativeSizeFloat</returns>
    public static implicit operator NativeSizeFloat(System.Windows.Size size)
    {
        return new((float)size.Width, (float)size.Height);
    }

    /// <summary>Equals operator</summary>
    /// <param name="float1">NativeSizeFloat</param>
    /// <param name="float2">System.Windows.Size</param>
    /// <returns>bool</returns>
    public static bool operator ==(NativeSizeFloat float1, System.Windows.Size float2)
    {
        return float1.Equals(float2);
    }

    /// <summary>Not equals operator</summary>
    /// <param name="float1">NativeSizeFloat</param>
    /// <param name="float2">System.Windows.Size</param>
    /// <returns>bool</returns>
    public static bool operator !=(NativeSizeFloat float1, System.Windows.Size float2)
    {
        return !float1.Equals(float2);
    }

    /// <summary>Equals operator</summary>
    /// <param name="float1">System.Windows.Size</param>
    /// <param name="float2">NativeSizeFloat</param>
    /// <returns>bool</returns>
    public static bool operator ==(System.Windows.Size float1, NativeSizeFloat float2)
    {
        return float2.Equals(float1);
    }

    /// <summary>Not equals operator</summary>
    /// <param name="float1">System.Windows.Size</param>
    /// <param name="float2">NativeSizeFloat</param>
    /// <returns>bool</returns>
    public static bool operator !=(System.Windows.Size float1, NativeSizeFloat float2)
    {
        return !float2.Equals(float1);
    }

    /// <summary>Implicit cast from NativeSize to System.Drawing.Size</summary>
    /// <param name="size">NativeSizeFloat</param>
    /// <returns>System.Drawing.Size</returns>
    public static implicit operator Size(NativeSizeFloat size)
    {
        return checked(new Size((int)size.Width, (int)size.Height));
    }

    /// <summary>Implicit cast from NativeSize to System.Drawing.SizeF</summary>
    /// <param name="size">NativeSizeFloat</param>
    /// <returns>System.Drawing.SizeF</returns>
    public static implicit operator SizeF(NativeSizeFloat size)
    {
        return new(size.Width, size.Height);
    }

    /// <summary>Implicit cast from System.Drawing.Size to NativeSizeFloat</summary>
    /// <param name="size">System.Drawing.Size</param>
    /// <returns>NativeSizeFloat</returns>
    public static implicit operator NativeSizeFloat(Size size)
    {
        return new(size.Width, size.Height);
    }

    /// <summary>Implicit cast from NativeSize to NativeSizeFloat</summary>
    /// <param name="size">NativeSize</param>
    /// <returns>NativeSizeFloat</returns>
    public static implicit operator NativeSizeFloat(NativeSize size)
    {
        return new(size.Width, size.Height);
    }

    /// <summary>Implicit cast from System.Drawing.SizeF to NativeSizeFloat</summary>
    /// <param name="size">System.Drawing.Size</param>
    /// <returns>NativeSizeFloat</returns>
    public static implicit operator NativeSizeFloat(SizeF size)
    {
        return new(size.Width, size.Height);
    }

    /// <summary>Compares two floating-point native sizes for equality.</summary>
    /// <param name="float1">The left-hand floating-point native size for equality.</param>
    /// <param name="float2">The right-hand floating-point native size for equality.</param>
    /// <returns>true if both floating-point dimensions are equal.</returns>
    public static bool operator ==(NativeSizeFloat float1, NativeSizeFloat float2)
    {
        return float1.Equals(float2);
    }

    /// <summary>Compares two floating-point native sizes for inequality.</summary>
    /// <param name="float1">The left-hand floating-point native size for inequality.</param>
    /// <param name="float2">The right-hand floating-point native size for inequality.</param>
    /// <returns>true if either floating-point dimension is different.</returns>
    public static bool operator !=(NativeSizeFloat float1, NativeSizeFloat float2)
    {
        return !float1.Equals(float2);
    }

    /// <summary>Compares a floating-point native size to a drawing size.</summary>
    /// <param name="float1">NativeSizeFloat</param>
    /// <param name="float2">System.Windows.Size</param>
    /// <returns>bool</returns>
    public static bool operator ==(NativeSizeFloat float1, Size float2)
    {
        return float1.Equals(float2);
    }

    /// <summary>Compares a floating-point native size to a drawing size for inequality.</summary>
    /// <param name="float1">NativeSizeFloat</param>
    /// <param name="float2">System.Windows.Size</param>
    /// <returns>bool</returns>
    public static bool operator !=(NativeSizeFloat float1, Size float2)
    {
        return !float1.Equals(float2);
    }

    /// <summary>Compares a drawing size to a floating-point native size.</summary>
    /// <param name="float1">System.Windows.Size</param>
    /// <param name="float2">NativeSizeFloat</param>
    /// <returns>bool</returns>
    public static bool operator ==(Size float1, NativeSizeFloat float2)
    {
        return float2.Equals(float1);
    }

    /// <summary>Compares a drawing size to a floating-point native size for inequality.</summary>
    /// <param name="float1">System.Windows.Size</param>
    /// <param name="float2">NativeSizeFloat</param>
    /// <returns>bool</returns>
    public static bool operator !=(Size float1, NativeSizeFloat float2)
    {
        return !float2.Equals(float1);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (!(obj is NativeSizeFloat f))
        {
            if (!(obj is System.Windows.Size size))
            {
                return obj is Size drawingSize && Equals(drawingSize);
            }

            return Equals(size);
        }

        return Equals(f);
    }

    /// <inheritdoc />
    public bool Equals(NativeSizeFloat other) => Math.Abs(_width - other._width) < float.Epsilon && Math.Abs(_height - other._height) < float.Epsilon;

    /// <inheritdoc />
    public int CompareTo(NativeSizeFloat other) => (other.Width * other.Height).CompareTo(Width * Height);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_width, _height);

    /// <summary>Deconstructs this floating-point native size into floating width and height values.</summary>
    /// <param name="width">The deconstructed floating-point width.</param>
    /// <param name="height">The deconstructed floating-point height.</param>
    public void Deconstruct(out float width, out float height)
    {
        width = Width;
        height = Height;
    }

    /// <inheritdoc />
    public override string ToString() => $"{{Width: {_width}; Height: {_height};}}";

    /// <summary>Converts this value to a Windows size.</summary>
    /// <returns>A Windows size with the same width and height.</returns>
    public System.Windows.Size ToSize()
    {
        SizeF size = ToSizeF();
        return new(size.Width, size.Height);
    }

    /// <summary>Returns this value as a floating-point native size.</summary>
    /// <returns>The current floating-point native size.</returns>
    public NativeSizeFloat ToNativeSizeFloat() => this;

    /// <summary>Converts this value to a drawing size.</summary>
    /// <returns>A drawing size with the same width and height.</returns>
    public SizeF ToSizeF() => new(Width, Height);
}
