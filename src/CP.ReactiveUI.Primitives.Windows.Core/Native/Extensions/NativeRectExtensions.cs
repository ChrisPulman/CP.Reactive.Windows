// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Media;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Internal;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Extensions;

/// <summary>Helper method for the NativeRect struct.</summary>
public static class NativeRectExtensions
{
    extension(NativeRect rect)
    {
        /// <summary>Create a new NativeRect, from the supplied one, using the specified X coordinate.</summary>
        /// <param name="x">int</param>
        /// <returns>NativeRect.</returns>
        public NativeRect ChangeX(int x) => NativeRectangleConversions.ToNativeRect(NativeRectangleGeometry<int>.ChangeX(NativeRectangleConversions.ToBounds(rect), x));

        /// <summary>Create a new NativeRect, from the supplied one, using the specified Y coordinate.</summary>
        /// <param name="y">int</param>
        /// <returns>NativeRect.</returns>
        public NativeRect ChangeY(int y) => NativeRectangleConversions.ToNativeRect(NativeRectangleGeometry<int>.ChangeY(NativeRectangleConversions.ToBounds(rect), y));

        /// <summary>Create a new NativeRect, from the supplied one, using the specified width.</summary>
        /// <param name="width">int</param>
        /// <returns>NativeRect.</returns>
        public NativeRect ChangeWidth(int width) => NativeRectangleConversions.ToNativeRect(NativeRectangleGeometry<int>.ChangeWidth(NativeRectangleConversions.ToBounds(rect), width));

        /// <summary>Create a new NativeRect, from the supplied one, using the specified height.</summary>
        /// <param name="height">int</param>
        /// <returns>NativeRect.</returns>
        public NativeRect ChangeHeight(int height) => NativeRectangleConversions.ToNativeRect(NativeRectangleGeometry<int>.ChangeHeight(NativeRectangleConversions.ToBounds(rect), height));

        /// <summary>Test if this NativeRect contains the specified NativePoint.</summary>
        /// <param name="point">NativePoint</param>
        /// <returns>true if it contains.</returns>
        public bool Contains(NativePoint point) => NativeRectangleGeometry<int>.Contains(NativeRectangleConversions.ToBounds(rect), point.X, point.Y);

        /// <summary>Test if this NativeRect contains the specified integer coordinates.</summary>
        /// <param name="x">The horizontal coordinate to test.</param>
        /// <param name="y">The vertical coordinate to test.</param>
        /// <returns>true when the integer coordinate pair is inside the rectangle.</returns>
        public bool Contains(int x, int y) => NativeRectangleGeometry<int>.Contains(NativeRectangleConversions.ToBounds(rect), x, y);

        /// <summary>True if small NativeRect is entirely contained within the larger NativeRect.</summary>
        /// <param name="smallerRectangle">NativeRect, the smaller rectangle</param>
        /// <returns>True if small NativeRect is entirely contained within the larger NativeRect, false otherwise.</returns>
        public bool Contains(NativeRect smallerRectangle) => NativeRectangleGeometry<int>.Contains(NativeRectangleConversions.ToBounds(rect), NativeRectangleConversions.ToBounds(smallerRectangle));

        /// <summary>Check that two rectangles overlap with each other.</summary>
        /// <param name="rect2">The second rectangle</param>
        /// <returns>The rectangles overlap.</returns>
        public bool HasOverlap(NativeRect rect2) => NativeRectangleGeometry<int>.HasOverlap(NativeRectangleConversions.ToBounds(rect), NativeRectangleConversions.ToBounds(rect2));

        /// <summary>True if either rectangle is adjacent to the other rectangle.</summary>
        /// <param name="rect2">The second rectangle</param>
        /// <returns>At least one rectangle is adjacent to the other rectangle.</returns>
        public AdjacentTo IsAdjacent(NativeRect rect2) => NativeRectangleGeometry<int>.IsAdjacent(NativeRectangleConversions.ToBounds(rect), NativeRectangleConversions.ToBounds(rect2));

        /// <summary>Test if a NativeRect is docked to the left of another NativeRect.</summary>
        /// <param name="rect2">NativeRect rect to be docked to</param>
        /// <returns>bool with true if they are docked.</returns>
        public bool IsDockedToLeftOf(NativeRect rect2)
        {
            if (rect.Right == checked(rect2.Left - 1))
            {
                return NativeRectangleGeometry<int>.IsBetween(rect.Top, rect2.Top, rect2.Bottom) || NativeRectangleGeometry<int>.IsBetween(rect.Bottom, rect2.Top, rect2.Bottom);
            }

            return false;
        }

        /// <summary>Test if a NativeRect is docked to the right of another NativeRect.</summary>
        /// <param name="rect2">NativeRect rect to be docked to</param>
        /// <returns>bool with true if they are docked.</returns>
        public bool IsDockedToRightOf(NativeRect rect2)
        {
            if (rect.Left == checked(rect2.Right + 1))
            {
                return NativeRectangleGeometry<int>.IsBetween(rect.Top, rect2.Top, rect2.Bottom) || NativeRectangleGeometry<int>.IsBetween(rect.Bottom, rect2.Top, rect2.Bottom);
            }

            return false;
        }

        /// <summary>Creates a new NativeRect which is the union of rect and rect2.</summary>
        /// <param name="rect2">NativeRect</param>
        /// <returns>NativeRect which is the union of rect and rect2.</returns>
        public NativeRect Union(NativeRect rect2) => NativeRectangleConversions.ToNativeRect(NativeRectangleGeometry<int>.Union(NativeRectangleConversions.ToBounds(rect), NativeRectangleConversions.ToBounds(rect2)));

        /// <summary>Creates the normalized rectangle shared by both native rectangles.</summary>
        /// <param name="rect2">NativeRect</param>
        /// <returns>NativeRect which is the intersection of rect and rect2.</returns>
        public NativeRect Intersect2(NativeRect rect2) => NativeRectangleConversions.ToNativeRect(NativeRectangleGeometry<int>.Intersect2(NativeRectangleConversions.ToBounds(rect), NativeRectangleConversions.ToBounds(rect2)));

        /// <summary>Creates a new NativeRect which is the intersection of rect and rect2.</summary>
        /// <param name="rect2">NativeRect</param>
        /// <returns>NativeRect which is the intersection of rect and rect2.</returns>
        public NativeRect Intersect(NativeRect rect2) => NativeRectangleConversions.ToNativeRect(NativeRectangleGeometry<int>.Intersect(NativeRectangleConversions.ToBounds(rect), NativeRectangleConversions.ToBounds(rect2)));

        /// <summary>Creates a new NativeRect which is rect but inflated with the specified width and height.</summary>
        /// <param name="width">The horizontal inflation amount.</param>
        /// <param name="height">The vertical inflation amount.</param>
        /// <returns>A rectangle inflated by the supplied integer dimensions.</returns>
        public NativeRect Inflate(int width, int height) => NativeRectangleConversions.ToNativeRect(NativeRectangleGeometry<int>.Inflate(NativeRectangleConversions.ToBounds(rect), width, height));

        /// <summary>Creates a new NativeRect which is rect but inflated with the specified size.</summary>
        /// <param name="size">NativeSize</param>
        /// <returns>NativeRect.</returns>
        public NativeRect Inflate(NativeSize size) => NativeRectangleConversions.ToNativeRect(NativeRectangleGeometry<int>.Inflate(NativeRectangleConversions.ToBounds(rect), size.Width, size.Height));

        /// <summary>Test if the current rectangle intersects with the specified.</summary>
        /// <param name="rect2">NativeRect</param>
        /// <returns>bool.</returns>
        public bool IntersectsWith(NativeRect rect2) => NativeRectangleGeometry<int>.IntersectsWith(NativeRectangleConversions.ToBounds(rect), NativeRectangleConversions.ToBounds(rect2));

        /// <summary>Create a new NativeRect by applying nullable coordinate offsets.</summary>
        /// <param name="offset">NativePoint</param>
        /// <returns>NativeRect.</returns>
        public NativeRect Offset(NativePoint offset) => NativeRectangleConversions.ToNativeRect(NativeRectangleGeometry<int>.Offset(NativeRectangleConversions.ToBounds(rect), offset.X, offset.Y));

        /// <summary>Create a new NativeRect by applying no nullable offset values.</summary>
        /// <returns>A rectangle offset by zero on both axes.</returns>
        public NativeRect Offset() => rect.Offset(null, null);

        /// <summary>Create a new NativeRect by applying a nullable X offset.</summary>
        /// <param name="offsetX">The nullable X offset.</param>
        /// <returns>The offset rectangle.</returns>
        public NativeRect Offset(int? offsetX) => rect.Offset(offsetX, null);

        /// <summary>Create a new NativeRect by offsetting the specified one.</summary>
        /// <param name="offsetX">The nullable horizontal offset.</param>
        /// <param name="offsetY">The nullable vertical offset.</param>
        /// <returns>A rectangle offset by the supplied nullable coordinates.</returns>
        public NativeRect Offset(int? offsetX, int? offsetY) => rect.Offset(new NativePoint(offsetX.GetValueOrDefault(), offsetY.GetValueOrDefault()));

        /// <summary>Create a new NativeRect at nullable coordinates while preserving unspecified axes.</summary>
        /// <param name="location">NativePoint</param>
        /// <returns>NativeRect.</returns>
        public NativeRect MoveTo(NativePoint location) => NativeRectangleConversions.ToNativeRect(NativeRectangleGeometry<int>.MoveTo(NativeRectangleConversions.ToBounds(rect), location.X, location.Y));

        /// <summary>Create a new NativeRect at the current location.</summary>
        /// <returns>A rectangle with the same location and size.</returns>
        public NativeRect MoveTo() => rect.MoveTo(null, null);

        /// <summary>Create a new NativeRect at a nullable X coordinate.</summary>
        /// <param name="x">The nullable X coordinate.</param>
        /// <returns>The moved rectangle.</returns>
        public NativeRect MoveTo(int? x) => rect.MoveTo(x, null);

        /// <summary>Create a new NativeRect from the specified one, but on a different location.</summary>
        /// <param name="x">The nullable horizontal destination.</param>
        /// <param name="y">The nullable vertical destination.</param>
        /// <returns>A rectangle moved to the supplied nullable coordinates.</returns>
        public NativeRect MoveTo(int? x, int? y) => rect.MoveTo(new NativePoint(x ?? rect.X, y ?? rect.Y));

        /// <summary>Create a new NativeRect with nullable dimensions while preserving unspecified dimensions.</summary>
        /// <param name="size">NativeSize</param>
        /// <returns>NativeRect.</returns>
        public NativeRect Resize(NativeSize size) => NativeRectangleConversions.ToNativeRect(NativeRectangleGeometry<int>.Resize(NativeRectangleConversions.ToBounds(rect), size.Width, size.Height));

        /// <summary>Create a new NativeRect with the current size.</summary>
        /// <returns>A rectangle with the same location and size.</returns>
        public NativeRect Resize() => rect.Resize(null, null);

        /// <summary>Create a new NativeRect with a nullable width.</summary>
        /// <param name="width">The nullable width.</param>
        /// <returns>The resized rectangle.</returns>
        public NativeRect Resize(int? width) => rect.Resize(width, null);

        /// <summary>Create a new NativeRect from the specified one, but with a different size.</summary>
        /// <param name="width">The nullable destination width.</param>
        /// <param name="height">The nullable destination height.</param>
        /// <returns>A rectangle resized to the supplied nullable dimensions.</returns>
        public NativeRect Resize(int? width, int? height) => rect.Resize(new NativeSize(width ?? rect.Width, height ?? rect.Height));

        /// <summary>Transform the specified NativeRect.</summary>
        /// <param name="matrix">Matrix</param>
        /// <returns>NativeRect.</returns>
        public NativeRect Transform(Matrix matrix) => NativeRectangleConversions.ToNativeRect(NativeRectangleGeometry<int>.Transform(NativeRectangleConversions.ToBounds(rect), matrix));

        /// <summary>Normalize the NativeRect by making a negative width and or height absolute.</summary>
        /// <returns>NativeRect.</returns>
        public NativeRect Normalize() => NativeRectangleConversions.ToNativeRect(NativeRectangleGeometry<int>.Normalize(NativeRectangleConversions.ToBounds(rect)));
    }
}
