// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>Extensions for Bitmaps.</summary>
public static class BitmapExtensions
{
    extension(Bitmap bitmap)
    {
        /// <summary>Convert a Bitmap to a BitmapSource.</summary>
        /// <returns>BitmapSource.</returns>
        public BitmapSource ToBitmapSource()
        {
            CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(bitmap);
            IntPtr bitmapHandle = bitmap.GetHbitmap();
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

    extension(Image image)
    {
        /// <summary>Convert a Image (Bitmap) to a BitmapSource.</summary>
        /// <returns>BitmapSource.</returns>
        public BitmapSource ToBitmapSource()
        {
            CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(image);
            return (image as Bitmap).ToBitmapSource();
        }
    }
}
