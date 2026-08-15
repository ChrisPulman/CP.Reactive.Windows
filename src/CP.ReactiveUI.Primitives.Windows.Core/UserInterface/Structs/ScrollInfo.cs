// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;

/// <summary>Represents the native SCROLLINFO structure.</summary>
[Serializable]
public struct ScrollInfo : IEquatable<ScrollInfo>
{
    /// <summary>Size of this struct.</summary>
    private readonly uint _nativeSize;

    /// <summary>Mask specifying which values to get.</summary>
    private readonly ScrollInfoMask _nativeMask;

    /// <summary>Native _nativeMinimum field.</summary>
    private readonly int _nativeMinimum;

    /// <summary>Native _nativeMaximum field.</summary>
    private readonly int _nativeMaximum;

    /// <summary>Native _nativePageSize field.</summary>
    private readonly uint _nativePageSize;

    /// <summary>Native _nativePosition field.</summary>
    private int _nativePosition;

    /// <summary>Native _nativeTrackingPosition field.</summary>
    private int _nativeTrackingPosition;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs.ScrollInfo" /> struct.</summary>
    /// <param name="nativeSize">The native structure size.</param>
    /// <param name="mask">The scroll info mask.</param>
    private ScrollInfo(uint nativeSize, ScrollInfoMask mask)
    {
        _nativeSize = nativeSize;
        _nativeMask = mask;
        _nativeMinimum = 0;
        _nativeMaximum = 0;
        _nativePageSize = 0U;
        _nativePosition = 0;
        _nativeTrackingPosition = 0;
    }

    /// <summary>Gets the minimum value to scroll to, e.g. the start.</summary>
    public readonly int Minimum => _nativeMinimum;

    /// <summary>Gets the maximum value to scroll to, e.g. the end.</summary>
    public readonly int Maximum => _nativeMaximum;

    /// <summary>Gets the size of a page.</summary>
    public readonly uint PageSize => _nativePageSize;

    /// <summary>Gets or sets the current position.</summary>
    public int Position
    {
        get => _nativePosition;
        set => _nativePosition = value;
    }

    /// <summary>Gets or sets the current tracking position.</summary>
    public int TrackingPosition
    {
        get => _nativeTrackingPosition;
        set => _nativeTrackingPosition = value;
    }

    /// <summary>Create a ScrollInfo struct with the specified mask.</summary>
    /// <param name="mask">ScrollInfoMask.</param>
    /// <returns>The initialized scroll information.</returns>
    public static ScrollInfo Create(ScrollInfoMask mask) =>
        new(checked((uint)Marshal.SizeOf<ScrollInfo>()), mask);

    /// <summary>Compares two ScrollInfo values for equality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>True when both values are equal.</returns>
    public static bool operator ==(ScrollInfo left, ScrollInfo right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two ScrollInfo values for inequality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>True when the values are not equal.</returns>
    public static bool operator !=(ScrollInfo left, ScrollInfo right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override readonly string ToString() =>
        $"{{Minimum = {_nativeMinimum}; Maximum = {_nativeMaximum};PageSize = {_nativePageSize};"
        + $"Position = {_nativePosition};TrackingPosition = {_nativeTrackingPosition};}}";

    /// <inheritdoc />
    public readonly bool Equals(ScrollInfo other) =>
        _nativeSize == other._nativeSize
        && _nativeMask == other._nativeMask
        && _nativeMinimum == other._nativeMinimum
        && _nativeMaximum == other._nativeMaximum
        && _nativePageSize == other._nativePageSize
        && _nativePosition == other._nativePosition
        && _nativeTrackingPosition == other._nativeTrackingPosition;

    /// <inheritdoc />
    public override readonly bool Equals(object obj) => obj is ScrollInfo other && Equals(other);

    /// <inheritdoc />
    public override readonly int GetHashCode() => 0;
}
