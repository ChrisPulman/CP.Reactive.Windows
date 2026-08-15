// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Internal;

/// <summary>Small numeric helper for the rectangle coordinate types supported by the native rectangle APIs.</summary>
/// <typeparam name="T">The coordinate type.</typeparam>
internal static class NativeNumber<T>
    where T : struct, IComparable<T>
{
    /// <summary>Gets a value that represents zero.</summary>
    internal static T Zero => typeof(T) != typeof(int) ? FromFloat(0F) : FromInt(0);

    /// <summary>Adds two values.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>The sum.</returns>
    internal static T Add(T left, T right) =>
        typeof(T) != typeof(int)
            ? FromFloat(ToFloat(left) + ToFloat(right))
            : FromInt(checked(ToInt(left) + ToInt(right)));

    /// <summary>Subtracts one value from another.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>The difference.</returns>
    internal static T Subtract(T left, T right) =>
        typeof(T) != typeof(int)
            ? FromFloat(ToFloat(left) - ToFloat(right))
            : FromInt(checked(ToInt(left) - ToInt(right)));

    /// <summary>Gets the absolute value.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The absolute value.</returns>
    internal static T Abs(T value) =>
        typeof(T) != typeof(int)
            ? FromFloat(Math.Abs(ToFloat(value)))
            : FromInt(Math.Abs(ToInt(value)));

    /// <summary>Gets the smaller value.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>The smaller value.</returns>
    internal static T Min(T left, T right) => !LessThan(left, right) ? right : left;

    /// <summary>Gets the larger value.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>The larger value.</returns>
    internal static T Max(T left, T right) => !GreaterThan(left, right) ? right : left;

    /// <summary>Tests whether a value is less than another value.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true" /> when the left value is less than the right value.</returns>
    internal static bool LessThan(T left, T right) => left.CompareTo(right) < 0;

    /// <summary>Tests whether a value is greater than another value.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true" /> when the left value is greater than the right value.</returns>
    internal static bool GreaterThan(T left, T right) => left.CompareTo(right) > 0;

    /// <summary>Tests whether a value is less than or equal to another value.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true" /> when the left value is less than or equal to the right value.</returns>
    internal static bool LessThanOrEqual(T left, T right) => left.CompareTo(right) <= 0;

    /// <summary>Tests whether a value is greater than or equal to another value.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true" /> when the left value is greater than or equal to the right value.</returns>
    internal static bool GreaterThanOrEqual(T left, T right) => left.CompareTo(right) >= 0;

    /// <summary>Converts a value to a single-precision floating-point number.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The converted value.</returns>
    internal static float ToSingle(T value) =>
        typeof(T) != typeof(int) ? ToFloat(value) : ToInt(value);

    /// <summary>Converts a value to a double-precision floating-point number.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The converted value.</returns>
    internal static double ToDouble(T value) => ToSingle(value);

    /// <summary>Converts a supported value to an integer.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The integer value.</returns>
    private static int ToInt(T value) => (int)(object)value;

    /// <summary>Converts a supported value to a single-precision floating-point value.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The single-precision value.</returns>
    private static float ToFloat(T value) => (float)(object)value;

    /// <summary>Converts an integer to the supported coordinate type.</summary>
    /// <param name="value">The integer value.</param>
    /// <returns>The coordinate value.</returns>
    private static T FromInt(int value) => (T)(object)value;

    /// <summary>Converts a single-precision value to the supported coordinate type.</summary>
    /// <param name="value">The single-precision value.</param>
    /// <returns>The coordinate value.</returns>
    private static T FromFloat(float value) => (T)(object)value;
}
