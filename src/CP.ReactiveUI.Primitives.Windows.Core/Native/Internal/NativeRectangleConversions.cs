// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Internal;

/// <summary>Converts native rectangle surfaces to and from their shared representation.</summary>
internal static class NativeRectangleConversions
{
    /// <summary>Converts an integer native rectangle to bounds.</summary>
    /// <param name="rectangle">The native rectangle.</param>
    /// <returns>The shared bounds.</returns>
    internal static NativeRectangleBounds<int> ToBounds(NativeRect rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

    /// <summary>Converts a floating-point native rectangle to bounds.</summary>
    /// <param name="rectangle">The native rectangle.</param>
    /// <returns>The shared bounds.</returns>
    internal static NativeRectangleBounds<float> ToBounds(NativeRectFloat rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

    /// <summary>Converts integer bounds to a native rectangle.</summary>
    /// <param name="rectangle">The shared bounds.</param>
    /// <returns>The native rectangle.</returns>
    internal static NativeRect ToNativeRect(NativeRectangleBounds<int> rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

    /// <summary>Converts floating-point bounds to an integer native rectangle.</summary>
    /// <param name="rectangle">The shared bounds.</param>
    /// <returns>The native rectangle.</returns>
    internal static NativeRect ToNativeRect(NativeRectangleBounds<float> rectangle)
    {
        Rectangle drawingRectangle = NativeRectangleGeometry<float>.ToRectangle(rectangle);
        return new(drawingRectangle.Left, drawingRectangle.Top, drawingRectangle.Width, drawingRectangle.Height);
    }

    /// <summary>Converts transformed corners to an integer native rectangle.</summary>
    /// <param name="corners">The transformed corners.</param>
    /// <returns>The native rectangle.</returns>
    internal static NativeRect ToNativeRect(NativeRectangleCorners corners) => checked(new NativeRect(new NativePoint((int)corners.TopLeftX, (int)corners.TopLeftY), new NativePoint((int)corners.BottomRightX, (int)corners.BottomRightY)));

    /// <summary>Converts floating-point bounds to a native rectangle.</summary>
    /// <param name="rectangle">The shared bounds.</param>
    /// <returns>The native rectangle.</returns>
    internal static NativeRectFloat ToNativeRectFloat(NativeRectangleBounds<float> rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

    /// <summary>Converts transformed corners to a floating-point native rectangle.</summary>
    /// <param name="corners">The transformed corners.</param>
    /// <returns>The native rectangle.</returns>
    internal static NativeRectFloat ToNativeRectFloat(NativeRectangleCorners corners) => new(new NativePointFloat(corners.TopLeftX, corners.TopLeftY), new NativePointFloat(corners.BottomRightX, corners.BottomRightY));
}
