// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;

/// <summary>
/// Describes the animation effects associated with user actions. This structure is used with the SystemParametersInfo function when the SPI_GETANIMATION or
/// SPI_SETANIMATION action value is specified.
/// </summary>
public struct AnimationInfo : IEquatable<AnimationInfo>
{
    /// <summary>Native _nativeSize field.</summary>
    private uint _nativeSize;

    /// <summary>Stores the minimum animate value.</summary>
    private int _minimumAnimate;

    /// <summary>Factory method to create AnimationInfo with animations enabled.</summary>
    /// <returns>The initialized animation information.</returns>
    public static AnimationInfo Create() => Create(enableAnimations: true);

    /// <summary>Factory method to create AnimationInfo.</summary>
    /// <param name="enableAnimations">True to enable animations.</param>
    /// <returns>The initialized animation information.</returns>
    public static AnimationInfo Create(bool enableAnimations) =>
        new AnimationInfo { _nativeSize = checked((uint)Marshal.SizeOf<AnimationInfo>()), _minimumAnimate = enableAnimations ? 1 : 0 };

    /// <summary>Compares two AnimationInfo values for equality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>True when both values are equal.</returns>
    public static bool operator ==(AnimationInfo left, AnimationInfo right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two AnimationInfo values for inequality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>True when the values are not equal.</returns>
    public static bool operator !=(AnimationInfo left, AnimationInfo right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public readonly bool Equals(AnimationInfo other) =>
        _nativeSize == other._nativeSize && _minimumAnimate == other._minimumAnimate;

    /// <inheritdoc />
    public override readonly bool Equals(object obj) => obj is AnimationInfo other && Equals(other);

    /// <inheritdoc />
    public override readonly int GetHashCode() => 0;
}
