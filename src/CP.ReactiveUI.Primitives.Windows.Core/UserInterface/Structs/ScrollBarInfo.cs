// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;

/// <summary>Represents the native SCROLLBARINFO structure.</summary>
[Serializable]
public readonly struct ScrollBarInfo : IEquatable<ScrollBarInfo>
{
    /// <summary>Size of this struct.</summary>
    private readonly uint _nativeSize;

    /// <summary>Native _nativeScrollBarBounds field.</summary>
    private readonly NativeRect _nativeScrollBarBounds;

    /// <summary>Native _nativeLineButtonSize field.</summary>
    private readonly int _nativeLineButtonSize;

    /// <summary>Native _nativeThumbBottom field.</summary>
    private readonly int _nativeThumbBottom;

    /// <summary>Native _nativeThumbTop field.</summary>
    private readonly int _nativeThumbTop;

    /// <summary>Native _nativeReserved field.</summary>
    private readonly int _nativeReserved;

    /// <summary>Native first object state field.</summary>
    private readonly ObjectStates _nativeState0;

    /// <summary>Native second object state field.</summary>
    private readonly ObjectStates _nativeState1;

    /// <summary>Native third object state field.</summary>
    private readonly ObjectStates _nativeState2;

    /// <summary>Native fourth object state field.</summary>
    private readonly ObjectStates _nativeState3;

    /// <summary>Native fifth object state field.</summary>
    private readonly ObjectStates _nativeState4;

    /// <summary>Native sixth object state field.</summary>
    private readonly ObjectStates _nativeState5;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs.ScrollBarInfo" /> struct.</summary>
    /// <param name="nativeSize">The native structure size.</param>
    private ScrollBarInfo(uint nativeSize)
    {
        _nativeSize = nativeSize;
        _nativeScrollBarBounds = default;
        _nativeLineButtonSize = 0;
        _nativeThumbBottom = 0;
        _nativeThumbTop = 0;
        _nativeReserved = 0;
        _nativeState0 = ObjectStates.None;
        _nativeState1 = ObjectStates.None;
        _nativeState2 = ObjectStates.None;
        _nativeState3 = ObjectStates.None;
        _nativeState4 = ObjectStates.None;
        _nativeState5 = ObjectStates.None;
    }

    /// <summary>Gets the coordinates of the scroll bar as specified in a RECT structure.</summary>
    public NativeRect Bounds => _nativeScrollBarBounds;

    /// <summary>Gets the height or width of the thumb.</summary>
    public int ThumbSize => _nativeLineButtonSize;

    /// <summary>Gets the position of the bottom or right of the thumb.</summary>
    public int ThumbBottom => _nativeThumbBottom;

    /// <summary>Gets the position of the top or left of the thumb.</summary>
    public int ThumbTop => _nativeThumbTop;

    /// <summary>Gets an array of object states. Each element indicates the state of a scroll bar component.</summary>
    public ObjectStates[] States =>
        [_nativeState0, _nativeState1, _nativeState2, _nativeState3, _nativeState4, _nativeState5];

    /// <summary>Create a ScrollBarInfo struct.</summary>
    /// <returns>The initialized scroll bar information.</returns>
    public static ScrollBarInfo Create() => new(checked((uint)Marshal.SizeOf<ScrollBarInfo>()));

    /// <summary>Compares two ScrollBarInfo values for equality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>True when both values are equal.</returns>
    public static bool operator ==(ScrollBarInfo left, ScrollBarInfo right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two ScrollBarInfo values for inequality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>True when the values are not equal.</returns>
    public static bool operator !=(ScrollBarInfo left, ScrollBarInfo right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        string statesString = string.Join(",", States);
        return $"{{Bounds = {Bounds}; ThumbSize = {ThumbSize};ThumbBottom = {ThumbBottom};ThumbTop = {ThumbTop};States = {statesString};}}";
    }

    /// <inheritdoc />
    public bool Equals(ScrollBarInfo other) =>
        HasSameScrollMetrics(in other) && HasSameScrollStates(in other);

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is ScrollBarInfo other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <summary>Compares scroll bar metrics.</summary>
    /// <param name="other">The other value.</param>
    /// <returns>True when metrics match.</returns>
    private bool HasSameScrollMetrics(in ScrollBarInfo other) =>
        _nativeSize == other._nativeSize
        && _nativeScrollBarBounds.Equals(other._nativeScrollBarBounds)
        && _nativeLineButtonSize == other._nativeLineButtonSize
        && _nativeThumbBottom == other._nativeThumbBottom
        && _nativeThumbTop == other._nativeThumbTop
        && _nativeReserved == other._nativeReserved;

    /// <summary>Compares scroll bar state values.</summary>
    /// <param name="other">The other value.</param>
    /// <returns>True when states match.</returns>
    private bool HasSameScrollStates(in ScrollBarInfo other) =>
        _nativeState0 == other._nativeState0
        && _nativeState1 == other._nativeState1
        && _nativeState2 == other._nativeState2
        && _nativeState3 == other._nativeState3
        && _nativeState4 == other._nativeState4
        && _nativeState5 == other._nativeState5;
}
