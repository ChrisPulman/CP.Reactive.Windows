// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
#endif
/// <summary>Creates bitmap scale handlers.</summary>
public static class BitmapScaleHandler
{
    /// <summary>Creates a handler with custom bitmap-provider logic.</summary>
    /// <typeparam name="TKey">The bitmap key type.</typeparam>
    /// <typeparam name="TValue">The disposable bitmap value type.</typeparam>
    /// <param name="dpiHandler">The DPI handler that supplies change notifications.</param>
    /// <param name="bitmapProvider">The function that provides a requested bitmap.</param>
    /// <returns>A bitmap scale handler.</returns>
    public static BitmapScaleHandler<TKey, TValue> Create<TKey, TValue>(DpiHandler dpiHandler, Func<TKey, int, TValue> bitmapProvider)
        where TValue : IDisposable => Create(dpiHandler, bitmapProvider, null);

    /// <summary>Creates a handler with custom bitmap-provider and scaler logic.</summary>
    /// <typeparam name="TKey">The bitmap key type.</typeparam>
    /// <typeparam name="TValue">The disposable bitmap value type.</typeparam>
    /// <param name="dpiHandler">The DPI handler that supplies change notifications.</param>
    /// <param name="bitmapProvider">The function that provides a requested bitmap.</param>
    /// <param name="bitmapScaler">The function that provides a newly scaled bitmap.</param>
    /// <returns>A bitmap scale handler.</returns>
    public static BitmapScaleHandler<TKey, TValue> Create<TKey, TValue>(DpiHandler dpiHandler, Func<TKey, int, TValue> bitmapProvider, Func<TValue, int, TValue> bitmapScaler)
        where TValue : IDisposable
    {
        BitmapScaleHandler<TKey, TValue> bitmapScaleHandler = new();
        bitmapScaleHandler.Initialize(dpiHandler, bitmapProvider, bitmapScaler);
        return bitmapScaleHandler;
    }

    /// <summary>Creates a handler that obtains values from a component resource manager.</summary>
    /// <typeparam name="TValue">The disposable bitmap value type.</typeparam>
    /// <param name="dpiHandler">The DPI handler that supplies change notifications.</param>
    /// <param name="resourceType">The type used to create the component resource manager.</param>
    /// <param name="bitmapScaler">The function that provides a newly scaled bitmap.</param>
    /// <returns>A bitmap scale handler.</returns>
    public static BitmapScaleHandler<string, TValue> WithComponentResourceManager<TValue>(DpiHandler dpiHandler, Type resourceType, Func<TValue, int, TValue> bitmapScaler)
        where TValue : IDisposable => Create(dpiHandler, (string imageName, int _) => (TValue)new ComponentResourceManager(resourceType).GetObject(imageName), bitmapScaler);

    /// <summary>Scales a bitmap using nearest-neighbour interpolation.</summary>
    /// <param name="bitmap">The bitmap to scale.</param>
    /// <param name="dpi">The DPI to scale for.</param>
    /// <returns>The scaled bitmap.</returns>
    public static Bitmap SimpleBitmapScaler(Bitmap bitmap, int dpi)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(bitmap);
        if (dpi == DpiCalculator.DefaultScreenDpi)
        {
            return bitmap;
        }

        NativeSize newSize = DpiCalculator.ScaleWithDpi(bitmap.Size, dpi);
        Bitmap result = new(newSize.Width, newSize.Height, bitmap.PixelFormat);
        using Graphics graphics = Graphics.FromImage(result);
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.DrawImage(bitmap, new Rectangle(0, 0, newSize.Width, newSize.Height), new(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
        return result;
    }
}
