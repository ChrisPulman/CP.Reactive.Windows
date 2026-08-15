// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Windows.Media;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Internal;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Extensions;

/// <summary>Helper methods for the NativeRectFloat struct.</summary>
public static class NativeRectFloatExtensions
{
    extension(NativeRectFloat rect)
    {
        /// <summary>Create a new NativeRectFloat, from the supplied one, using the specified X coordinate.</summary>
        /// <param name="x">float</param>
        /// <returns>NativeRectFloat.</returns>
        public NativeRectFloat ChangeX(float x) => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.ChangeX(NativeRectangleConversions.ToBounds(rect), x));

        /// <summary>Create a new NativeRectFloat, from the supplied one, using the specified Y coordinate.</summary>
        /// <param name="y">float</param>
        /// <returns>NativeRectFloat.</returns>
        public NativeRectFloat ChangeY(float y) => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.ChangeY(NativeRectangleConversions.ToBounds(rect), y));

        /// <summary>Create a new NativeRectFloat, from the supplied one, using the specified width.</summary>
        /// <param name="width">float</param>
        /// <returns>NativeRectFloat.</returns>
        public NativeRectFloat ChangeWidth(float width) => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.ChangeWidth(NativeRectangleConversions.ToBounds(rect), width));

        /// <summary>Create a new NativeRectFloat, from the supplied one, using the specified height.</summary>
        /// <param name="height">float</param>
        /// <returns>NativeRectFloat.</returns>
        public NativeRectFloat ChangeHeight(float height) => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.ChangeHeight(NativeRectangleConversions.ToBounds(rect), height));

        /// <summary>Test if this NativeRectFloat contains the specified NativePointFloat.</summary>
        /// <param name="point">NativePointFloat</param>
        /// <returns>true if it contains.</returns>
        public bool Contains(NativePointFloat point) => NativeRectangleGeometry<float>.Contains(NativeRectangleConversions.ToBounds(rect), point.X, point.Y);

        /// <summary>Test if this NativeRectFloat contains the specified floating-point coordinates.</summary>
        /// <param name="x">The horizontal coordinate to test.</param>
        /// <param name="y">The vertical coordinate to test.</param>
        /// <returns>true when the floating-point coordinate pair is inside the rectangle.</returns>
        public bool Contains(float x, float y) => NativeRectangleGeometry<float>.Contains(NativeRectangleConversions.ToBounds(rect), x, y);

        /// <summary>True if small NativeRectFloat is entirely contained within the larger NativeRectFloat.</summary>
        /// <param name="smallerRectangle">NativeRectFloat, the smaller rectangle</param>
        /// <returns>True if small rectangle is entirely contained within the larger rectangle, false otherwise.</returns>
        public bool Contains(NativeRectFloat smallerRectangle) => NativeRectangleGeometry<float>.Contains(NativeRectangleConversions.ToBounds(rect), NativeRectangleConversions.ToBounds(smallerRectangle));

        /// <summary>Check that two rectangles overlap with each other.</summary>
        /// <param name="rect2">The second rectangle</param>
        /// <returns>The rectangles overlap.</returns>
        public bool HasOverlap(NativeRectFloat rect2) => NativeRectangleGeometry<float>.HasOverlap(NativeRectangleConversions.ToBounds(rect), NativeRectangleConversions.ToBounds(rect2));

        /// <summary>True if either rectangle is adjacent to the other rectangle.</summary>
        /// <param name="rect2">The second rectangle</param>
        /// <returns>At least one rectangle is adjacent to the other rectangle.</returns>
        public AdjacentTo IsAdjacent(NativeRectFloat rect2) => NativeRectangleGeometry<float>.IsAdjacent(NativeRectangleConversions.ToBounds(rect), NativeRectangleConversions.ToBounds(rect2));

        /// <summary>Test if a NativeRectFloat is docked to the left of another NativeRectFloat.</summary>
        /// <param name="rect2">NativeRectFloat rect to be docked to</param>
        /// <returns>bool with true if they are docked.</returns>
        public bool IsDockedToLeftOf(NativeRectFloat rect2)
        {
            if (Math.Abs(rect.Right - (rect2.Left - 1F)) < float.Epsilon)
            {
                return NativeRectangleGeometry<float>.IsBetween(rect.Top, rect2.Top, rect2.Bottom) || NativeRectangleGeometry<float>.IsBetween(rect.Bottom, rect2.Top, rect2.Bottom);
            }

            return false;
        }

        /// <summary>Test if a NativeRectFloat is docked to the right of another NativeRectFloat.</summary>
        /// <param name="rect2">NativeRectFloat rect to be docked to</param>
        /// <returns>bool with true if they are docked.</returns>
        public bool IsDockedToRightOf(NativeRectFloat rect2)
        {
            if (Math.Abs(rect.Left - (rect2.Right + 1F)) < float.Epsilon)
            {
                return NativeRectangleGeometry<float>.IsBetween(rect.Top, rect2.Top, rect2.Bottom) || NativeRectangleGeometry<float>.IsBetween(rect.Bottom, rect2.Top, rect2.Bottom);
            }

            return false;
        }

        /// <summary>Creates a new NativeRectFloat which is the union of rect and rect2.</summary>
        /// <param name="rect2">NativeRectFloat</param>
        /// <returns>NativeRectFloat which is the union of rect and rect2.</returns>
        public NativeRectFloat Union(NativeRectFloat rect2) => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.Union(NativeRectangleConversions.ToBounds(rect), NativeRectangleConversions.ToBounds(rect2)));

        /// <summary>Creates the normalized floating-point rectangle shared by both rectangles.</summary>
        /// <param name="rect2">NativeRectFloat</param>
        /// <returns>NativeRectFloat which is the intersection of rect and rect2.</returns>
        public NativeRectFloat Intersect(NativeRectFloat rect2) => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.Intersect(NativeRectangleConversions.ToBounds(rect), NativeRectangleConversions.ToBounds(rect2)));

        /// <summary>Creates a new NativeRectFloat inflated by integer width and height values.</summary>
        /// <param name="width">The integer horizontal inflation amount.</param>
        /// <param name="height">The integer vertical inflation amount.</param>
        /// <returns>A floating-point rectangle inflated by integer dimensions.</returns>
        public NativeRectFloat Inflate(int width, int height) => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.Inflate(NativeRectangleConversions.ToBounds(rect), width, height));

        /// <summary>Creates a new NativeRectFloat which is rect but inflated with the specified width and height.</summary>
        /// <param name="width">The floating-point horizontal inflation amount.</param>
        /// <param name="height">The floating-point vertical inflation amount.</param>
        /// <returns>A floating-point rectangle inflated by floating-point dimensions.</returns>
        public NativeRectFloat Inflate(float width, float height) => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.Inflate(NativeRectangleConversions.ToBounds(rect), width, height));

        /// <summary>Creates a new NativeRect which is rect but inflated with the specified size.</summary>
        /// <param name="size">NativeSizeFloat</param>
        /// <returns>NativeRectFloat.</returns>
        public NativeRectFloat Inflate(NativeSizeFloat size) => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.Inflate(NativeRectangleConversions.ToBounds(rect), size.Width, size.Height));

        /// <summary>Test if the current NativeRectFloat intersects with the specified.</summary>
        /// <param name="rect2">NativeRectFloat</param>
        /// <returns>bool.</returns>
        public bool IntersectsWith(NativeRectFloat rect2) => NativeRectangleGeometry<float>.IntersectsWith(NativeRectangleConversions.ToBounds(rect), NativeRectangleConversions.ToBounds(rect2));

        /// <summary>Create a new NativeRect by offsetting the specified one.</summary>
        /// <param name="offset">NativePointFloat</param>
        /// <returns>NativeRectFloat.</returns>
        public NativeRectFloat Offset(NativePointFloat offset) => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.Offset(NativeRectangleConversions.ToBounds(rect), offset.X, offset.Y));

        /// <summary>Create a new NativeRectFloat by applying no nullable offset values.</summary>
        /// <returns>A rectangle offset by the previous nullable defaults.</returns>
        public NativeRectFloat Offset() => rect.Offset(null, null);

        /// <summary>Create a new NativeRectFloat by applying a nullable X offset.</summary>
        /// <param name="offsetX">The nullable X offset.</param>
        /// <returns>The offset rectangle.</returns>
        public NativeRectFloat Offset(float? offsetX) => rect.Offset(offsetX, null);

        /// <summary>Create a new NativeRectFloat by applying nullable coordinate offsets.</summary>
        /// <param name="offsetX">The nullable horizontal offset.</param>
        /// <param name="offsetY">The nullable vertical offset.</param>
        /// <returns>A floating-point rectangle offset by nullable coordinates.</returns>
        public NativeRectFloat Offset(float? offsetX, float? offsetY) => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.Offset(NativeRectangleConversions.ToBounds(rect), offsetX.GetValueOrDefault(), offsetY.GetValueOrDefault()));

        /// <summary>Create a new NativeRectFloat at nullable coordinates while preserving unspecified axes.</summary>
        /// <param name="location">NativePointFloat</param>
        /// <returns>NativeRectFloat.</returns>
        public NativeRectFloat MoveTo(NativePointFloat location) => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.MoveTo(NativeRectangleConversions.ToBounds(rect), location.X, location.Y));

        /// <summary>Create a new NativeRectFloat at the current location.</summary>
        /// <returns>A rectangle with the same location and size.</returns>
        public NativeRectFloat MoveTo() => rect.MoveTo(null, null);

        /// <summary>Create a new NativeRectFloat at a nullable X coordinate.</summary>
        /// <param name="x">The nullable X coordinate.</param>
        /// <returns>The moved rectangle.</returns>
        public NativeRectFloat MoveTo(float? x) => rect.MoveTo(x, null);

        /// <summary>Create a new NativeRectFloat from the specified one, but on a different location.</summary>
        /// <param name="x">The nullable horizontal destination.</param>
        /// <param name="y">The nullable vertical destination.</param>
        /// <returns>A floating-point rectangle moved to nullable coordinates.</returns>
        public NativeRectFloat MoveTo(float? x, float? y) => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.MoveTo(NativeRectangleConversions.ToBounds(rect), x ?? rect.X, y ?? rect.Y));

        /// <summary>Create a new NativeRectFloat with nullable dimensions while preserving unspecified dimensions.</summary>
        /// <param name="size">NativeSizeFloat</param>
        /// <returns>NativeRectFloat.</returns>
        public NativeRectFloat Resize(NativeSizeFloat size) => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.Resize(NativeRectangleConversions.ToBounds(rect), size.Width, size.Height));

        /// <summary>Create a new NativeRectFloat with the current size.</summary>
        /// <returns>A rectangle with the same location and size.</returns>
        public NativeRectFloat Resize() => rect.Resize(null, null);

        /// <summary>Create a new NativeRectFloat with a nullable width.</summary>
        /// <param name="width">The nullable width.</param>
        /// <returns>The resized rectangle.</returns>
        public NativeRectFloat Resize(float? width) => rect.Resize(width, null);

        /// <summary>Create a new NativeRectFloat from the specified one, but with a different size.</summary>
        /// <param name="width">The nullable destination width.</param>
        /// <param name="height">The nullable destination height.</param>
        /// <returns>A floating-point rectangle resized to nullable dimensions.</returns>
        public NativeRectFloat Resize(float? width, float? height) => rect.Resize(new NativeSizeFloat(width ?? rect.Width, height ?? rect.Height));

        /// <summary>Create a NativeRect, using rounded values, from the specified NativeRectFloat.</summary>
        /// <returns>NativeRect.</returns>
        public NativeRect Round() => checked(new NativeRect((int)Math.Round(rect.X), (int)Math.Round(rect.Y), (int)Math.Round(rect.Width), (int)Math.Round(rect.Height)));

        /// <summary>Transform the specified NativeRectFloat.</summary>
        /// <param name="matrix">Matrix</param>
        /// <returns>NativeRectFloat.</returns>
        public NativeRectFloat Transform(Matrix matrix) => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.Transform(NativeRectangleConversions.ToBounds(rect), matrix));

        /// <summary>Normalize the NativeRectFloat by making a negative width and or height absolute.</summary>
        /// <returns>NativeRectFloat.</returns>
        public NativeRectFloat Normalize() => NativeRectangleConversions.ToNativeRectFloat(NativeRectangleGeometry<float>.Normalize(NativeRectangleConversions.ToBounds(rect)));
    }
}
