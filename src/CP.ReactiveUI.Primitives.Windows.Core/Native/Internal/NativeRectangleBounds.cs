// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Internal;

/// <summary>Internal numeric representation shared by native rectangle operations.</summary>
/// <typeparam name="T">The coordinate type.</typeparam>
/// <param name="X">The x-coordinate.</param>
/// <param name="Y">The y-coordinate.</param>
/// <param name="Width">The width.</param>
/// <param name="Height">The height.</param>
internal readonly record struct NativeRectangleBounds<T>(T X, T Y, T Width, T Height)
    where T : struct, IComparable<T>
{
    /// <summary>Gets the left edge.</summary>
    internal T Left => X;

    /// <summary>Gets the top edge.</summary>
    internal T Top => Y;

    /// <summary>Gets the right edge.</summary>
    internal T Right => NativeNumber<T>.Add(X, Width);

    /// <summary>Gets the bottom edge.</summary>
    internal T Bottom => NativeNumber<T>.Add(Y, Height);
}
