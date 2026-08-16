// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing;

namespace CP.ReactiveUI.Primitives.Windows.Native;

/// <summary>Provides extension methods for <see cref="T:System.Drawing.Bitmap" /> to enable typed span-based pixel access.</summary>
public static class BitmapAccessorExtensions
{
    /// <summary>Provides extension members for the target value.</summary>
    /// <param name="targetBitmap">The target value.</param>
    extension(Bitmap targetBitmap)
    {
        /// <summary>Processes pixel rows of two bitmaps simultaneously, providing typed access to both as source and target.</summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="sourceBitmap">The source bitmap to read from.</param>
        /// <param name="processRows">An action that processes each pair of rows.</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="T:System.ArgumentException">Thrown when bitmaps have different dimensions.</exception>
        public void ProcessPixelRows<TPixel>(
            Bitmap sourceBitmap,
            Action<BitmapAccessor<TPixel>, BitmapAccessor<TPixel>> processRows)
            where TPixel : struct
        {
            Throw.IfNull(targetBitmap);
            Throw.IfNull(sourceBitmap);
            Throw.IfNull(processRows);
            if (
                targetBitmap.Width != sourceBitmap.Width
                || targetBitmap.Height != sourceBitmap.Height)
            {
                throw new ArgumentException(
                    "Source and target bitmaps must have the same dimensions.");
            }

            using BitmapAccessor<TPixel> targetAccessor = new(targetBitmap);
            using BitmapAccessor<TPixel> sourceAccessor = new(sourceBitmap, readOnly: true);
            processRows(targetAccessor, sourceAccessor);
        }

        /// <summary>Processes pixel rows of a single targetBitmap with typed pixel access.</summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="processRows">An action that processes the targetBitmap rows.</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown when any parameter is null.</exception>
        public void ProcessPixelRows<TPixel>(Action<BitmapAccessor<TPixel>> processRows)
            where TPixel : struct
        {
            Throw.IfNull(targetBitmap);
            Throw.IfNull(processRows);
            using BitmapAccessor<TPixel> accessor = new(targetBitmap);
            processRows(accessor);
        }
    }
}
