// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>Extensions for Bitmaps.</summary>
public static class BitmapExtensions
{
    /// <summary>Provides bitmap conversion extension methods.</summary>
    /// <param name="bitmap">The bitmap to convert.</param>
    extension(Bitmap bitmap)
    {
        /// <summary>Convert a Bitmap to a BitmapSource.</summary>
        /// <returns>BitmapSource.</returns>
        public BitmapSource ToBitmapSource()
        {
            Throw.IfNull(bitmap);
            var bitmapHandle = bitmap.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(bitmapHandle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                _ = Gdi32Api.DeleteObject(bitmapHandle);
            }
        }
    }

    /// <summary>Provides image conversion extension methods.</summary>
    /// <param name="image">The image to convert.</param>
    extension(Image image)
    {
        /// <summary>Convert a Image (Bitmap) to a BitmapSource.</summary>
        /// <returns>BitmapSource.</returns>
        public BitmapSource ToBitmapSource()
        {
            Throw.IfNull(image);
            return ((Bitmap)image).ToBitmapSource();
        }
    }
}
