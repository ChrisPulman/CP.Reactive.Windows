// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using CP.ReactiveUI.Primitives.Windows.Native.TypeConverters;

namespace CP.ReactiveUI.Primitives.Windows.Native.Structs;

/// <summary>This structure should be used everywhere where native methods need a size struct.</summary>
[Serializable]
[TypeConverter(typeof(NativeSizeTypeConverter))]
public readonly struct NativeSize : IEquatable<NativeSize>, IComparable<NativeSize>
{
    /// <summary>The stored width.</summary>
    private readonly int _width;

    /// <summary>The stored height.</summary>
    private readonly int _height;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeSize" /> struct.</summary>
    /// <param name="size">System.Windows.Size</param>
    public NativeSize(System.Windows.Size size)
    {
        this = checked(new NativeSize((int)size.Width, (int)size.Height));
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeSize" /> struct.</summary>
    /// <param name="size">The drawing size to copy.</param>
    public NativeSize(System.Drawing.Size size)
        : this(size.Width, size.Height)
    {
    }

    /// <summary>Gets the zero-width and zero-height native size.</summary>
    public static NativeSize Empty { get; } = new(0, 0);

    /// <summary>Gets the Width of the size struct.</summary>
    public int Width => _width;

    /// <summary>Gets the Height of the size struct.</summary>
    public int Height => _height;

    /// <summary>Gets a value indicating whether the area is zero.</summary>
    /// <returns>true if the size is empty.</returns>
    public bool IsEmpty => checked(_width * _height) == 0;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeSize" /> struct.</summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public NativeSize(int width, int height)
    {
        _width = width;
        _height = height;
    }

    /// <summary>Implicit cast from System.Windows.Size to NativeSize</summary>
    /// <param name="size">System.Windows.Size</param>
    public static implicit operator NativeSize(System.Windows.Size size)
    {
        return checked(new NativeSize((int)size.Width, (int)size.Height));
    }

    /// <summary>Implicit cast from NativeSize to Size</summary>
    /// <param name="size">NativeSize</param>
    public static implicit operator System.Windows.Size(NativeSize size)
    {
        return new(size.Width, size.Height);
    }

    /// <summary>Converts a native size to a drawing size.</summary>
    /// <param name="size">NativeSize</param>
    public static implicit operator System.Drawing.Size(NativeSize size)
    {
        return new(size.Width, size.Height);
    }

    /// <summary>Converts a drawing size to a native size.</summary>
    /// <param name="size">System.Drawing.Size</param>
    public static implicit operator NativeSize(System.Drawing.Size size)
    {
        return new(size.Width, size.Height);
    }

    /// <summary>Equals operator overloading</summary>
    /// <param name="size1">The left-hand native size for equality.</param>
    /// <param name="size2">The right-hand native size for equality.</param>
    /// <returns>true if both sizes are equal.</returns>
    public static bool operator ==(NativeSize size1, NativeSize size2)
    {
        return size1.Equals(size2);
    }

    /// <summary>Not Equals operator overloading</summary>
    /// <param name="size1">The left-hand native size for inequality.</param>
    /// <param name="size2">The right-hand native size for inequality.</param>
    /// <returns>true if either dimension is different.</returns>
    public static bool operator !=(NativeSize size1, NativeSize size2)
    {
        return !size1.Equals(size2);
    }

    /// <summary>Equals operator overloading</summary>
    /// <param name="size1">NativeSize</param>
    /// <param name="size2">System.Drawing.Size</param>
    /// <returns>bool</returns>
    public static bool operator ==(NativeSize size1, System.Drawing.Size size2)
    {
        return size1.Equals(size2);
    }

    /// <summary>Not Equals operator overloading</summary>
    /// <param name="size1">NativeSize</param>
    /// <param name="size2">System.Drawing.Size</param>
    /// <returns>bool</returns>
    public static bool operator !=(NativeSize size1, System.Drawing.Size size2)
    {
        return !size1.Equals(size2);
    }

    /// <summary>Equals operator overloading</summary>
    /// <param name="size1">System.Drawing.Size</param>
    /// <param name="size2">NativeSize</param>
    /// <returns>bool</returns>
    public static bool operator ==(System.Drawing.Size size1, NativeSize size2)
    {
        return size2.Equals(size1);
    }

    /// <summary>Not Equals operator overloading</summary>
    /// <param name="size1">System.Drawing.Size</param>
    /// <param name="size2">NativeSize</param>
    /// <returns>bool</returns>
    public static bool operator !=(System.Drawing.Size size1, NativeSize size2)
    {
        return !size2.Equals(size1);
    }

    /// <summary>Equals operator overloading</summary>
    /// <param name="size1">NativeSize</param>
    /// <param name="size2">System.Windows.Size</param>
    /// <returns>bool</returns>
    public static bool operator ==(NativeSize size1, System.Windows.Size size2)
    {
        return size1.Equals(size2);
    }

    /// <summary>Not Equals operator overloading</summary>
    /// <param name="size1">NativeSize</param>
    /// <param name="size2">System.Windows.Size</param>
    /// <returns>bool</returns>
    public static bool operator !=(NativeSize size1, System.Windows.Size size2)
    {
        return !size1.Equals(size2);
    }

    /// <summary>Equals operator overloading</summary>
    /// <param name="size1">System.Windows.Size</param>
    /// <param name="size2">NativeSize</param>
    /// <returns>bool</returns>
    public static bool operator ==(System.Windows.Size size1, NativeSize size2)
    {
        return size2.Equals(size1);
    }

    /// <summary>Not Equals operator overloading</summary>
    /// <param name="size1">System.Windows.Size</param>
    /// <param name="size2">NativeSize</param>
    /// <returns>bool</returns>
    public static bool operator !=(System.Windows.Size size1, NativeSize size2)
    {
        return !size2.Equals(size1);
    }

    /// <inheritdoc />
    public int CompareTo(NativeSize other) => (other.Width * other.Height).CompareTo(Width * Height);

    /// <inheritdoc />
    public override string ToString() => $"{{Width: {_width}; Height: {_height};}}";

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (!(obj is NativeSize size))
        {
            if (!(obj is System.Drawing.Size drawingSize))
            {
                return obj is System.Windows.Size windowsSize && Equals(windowsSize);
            }

            return Equals(drawingSize);
        }

        return Equals(size);
    }

    /// <inheritdoc />
    public bool Equals(NativeSize other) => _width == other._width && _height == other._height;

    /// <summary>Deconstructs this native size into integer width and height values.</summary>
    /// <param name="width">The deconstructed width.</param>
    /// <param name="height">The deconstructed height.</param>
    public void Deconstruct(out int width, out int height)
    {
        width = Width;
        height = Height;
    }

    /// <inheritdoc />
    public override int GetHashCode() => (_width * 397) ^ _height;

    /// <summary>Returns this value as a native size.</summary>
    /// <returns>The current native size.</returns>
    public NativeSize ToNativeSize() => this;

    /// <summary>Converts this value to a Windows size.</summary>
    /// <returns>A Windows size with the same width and height.</returns>
    public System.Windows.Size ToSize() => new(Width, Height);
}
