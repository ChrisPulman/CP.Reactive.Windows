// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi;

/// <summary>Provides extensions for GDI types.</summary>
public static class GdiExtensions
{
    /// <summary>Provides bitmap-specific GDI handle helpers.</summary>
    /// <param name="bitmap">The bitmap to extend.</param>
    extension(Bitmap bitmap)
    {
        /// <summary>Gets a SafeHBitmapHandle so callers can use automatic HBITMAP cleanup.</summary>
        public SafeHBitmapHandle SafeHBitmapHandle => new(bitmap);
    }

    /// <summary>Provides graphics-specific GDI drawing helpers.</summary>
    /// <param name="graphics">The graphics target to extend.</param>
    extension(Graphics graphics)
    {
        /// <summary>Gets a SafeHandle for GetHdc so callers can use automatic device context cleanup.</summary>
        /// <returns>SafeGraphicsDcHandle.</returns>
        public SafeGraphicsDcHandle GetSafeDeviceContext() =>
            SafeGraphicsDcHandle.FromGraphics(graphics);

        /// <summary>Performs a BitBlt operation from a bitmap into the graphics target.</summary>
        /// <param name="sourceBitmap">Bitmap.</param>
        /// <param name="source">Rectangle.</param>
        /// <param name="destination">Point.</param>
        /// <param name="rasterOperations">RasterOperations.</param>
        public void BitBlt(
            Bitmap sourceBitmap,
            Rectangle source,
            NativePoint destination,
            RasterOperations rasterOperations)
        {
            using SafeGraphicsDcHandle targetDeviceContext = graphics.GetSafeDeviceContext();
            using SafeCompatibleDcHandle compatibleDeviceContext = Gdi32Api.CreateCompatibleDC(
                targetDeviceContext);
            using SafeHBitmapHandle bitmapHandle = new(sourceBitmap.GetHbitmap());
            using (compatibleDeviceContext.SelectObject(bitmapHandle))
            {
                _ = Gdi32Api.BitBlt(
                    targetDeviceContext,
                    new(destination.X, destination.Y, source.Width, source.Height),
                    compatibleDeviceContext,
                    new(source.Left, source.Top),
                    rasterOperations);
            }
        }

        /// <summary>Performs a StretchBlt operation from a bitmap into the graphics target.</summary>
        /// <param name="sourceBitmap">Bitmap.</param>
        /// <param name="source">The source bitmap rectangle.</param>
        /// <param name="destination">The destination graphics rectangle.</param>
        /// <param name="rasterOperation">RasterOperations.</param>
        public void StretchBlt(
            Bitmap sourceBitmap,
            Rectangle source,
            Rectangle destination,
            RasterOperations rasterOperation)
        {
            using SafeGraphicsDcHandle targetDeviceContext = graphics.GetSafeDeviceContext();
            using SafeCompatibleDcHandle compatibleDeviceContext = Gdi32Api.CreateCompatibleDC(
                targetDeviceContext);
            using SafeHBitmapHandle bitmapHandle = new(sourceBitmap);
            using (compatibleDeviceContext.SelectObject(bitmapHandle))
            {
                _ = Gdi32Api.StretchBlt(
                    targetDeviceContext,
                    destination,
                    compatibleDeviceContext,
                    source,
                    rasterOperation);
            }
        }
    }

    /// <summary>Provides region visibility helpers.</summary>
    /// <param name="region">The region to extend.</param>
    extension(Region region)
    {
        /// <summary>
        /// Checks if all corners of the rectangle are visible in the region.
        /// Not a perfect check, but this is currently a workaround for checking if a window is completely visible.
        /// </summary>
        /// <param name="rectangle">The rectangle to test.</param>
        /// <returns>True when all rectangle corners are visible in the region.</returns>
        public bool AreRectangleCornersVisisble(NativeRect rectangle)
        {
            Point topLeft = new(rectangle.X, rectangle.Y);
            checked
            {
                Point topRight = new(rectangle.X + rectangle.Width, rectangle.Y);
                Point bottomLeft = new(rectangle.X, rectangle.Y + rectangle.Height);
                Point bottomRight = new(
                    rectangle.X + rectangle.Width,
                    rectangle.Y + rectangle.Height);
                bool num = region.IsVisible(topLeft);
                bool topRightVisible = region.IsVisible(topRight);
                bool bottomLeftVisible = region.IsVisible(bottomLeft);
                bool bottomRightVisible = region.IsVisible(bottomRight);
                return num && topRightVisible && bottomLeftVisible && bottomRightVisible;
            }
        }
    }
}
