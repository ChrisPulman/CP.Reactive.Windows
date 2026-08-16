// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.Structs;
#endif
/// <summary>Contains information about an icon or a cursor.</summary>
public readonly struct IconInfo : IEquatable<IconInfo>
{
    /// <summary>Stores whether this structure describes an icon.</summary>
    private readonly int _isIcon;

    /// <summary>Stores the hotspot x-coordinate.</summary>
    private readonly int _hotspotX;

    /// <summary>Stores the hotspot y-coordinate.</summary>
    private readonly int _hotspotY;

    /// <summary>Stores the bitmask bitmap handle.</summary>
    private readonly IntPtr _maskBitmapHandle;

    /// <summary>Stores the color bitmap handle.</summary>
    private readonly IntPtr _colorBitmapHandle;

    /// <summary>Gets or sets a value indicating whether this structure defines an icon or a cursor.</summary>
    public bool IsIcon
    {
        get => _isIcon != 0;
        set => Unsafe.AsRef(in _isIcon) = (value ? 1 : 0);
    }

    /// <summary>Gets or sets the coordinates of a cursor hot spot.</summary>
    public NativePoint Hotspot
    {
        get => new(_hotspotX, _hotspotY);
        set
        {
            Unsafe.AsRef(in _hotspotX) = value.X;
            Unsafe.AsRef(in _hotspotY) = value.Y;
        }
    }

    /// <summary>Gets the icon bitmask bitmap handle.</summary>
    public SafeHBitmapHandle BitmaskBitmapHandle => new(_maskBitmapHandle);

    /// <summary>Gets the icon color bitmap handle.</summary>
    public SafeHBitmapHandle ColorBitmapHandle => new(_colorBitmapHandle);

    /// <summary>Determines whether two icon information values are equal.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>A value indicating whether the values are equal.</returns>
    public static bool operator ==(IconInfo left, IconInfo right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two icon information values are not equal.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>A value indicating whether the values are not equal.</returns>
    public static bool operator !=(IconInfo left, IconInfo right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public bool Equals(IconInfo other) =>
        _isIcon == other._isIcon
        && _hotspotX == other._hotspotX
        && _hotspotY == other._hotspotY
        && _maskBitmapHandle == other._maskBitmapHandle
        && _colorBitmapHandle == other._colorBitmapHandle;

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is IconInfo other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_isIcon, _hotspotX, _hotspotY, _maskBitmapHandle, _colorBitmapHandle);
}
