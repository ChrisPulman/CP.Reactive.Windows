// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
#endif
/// <summary>Calculate with DPI.</summary>
public static class DpiCalculator
{
    /// <summary>This is the default DPI for the screen.</summary>
    public static readonly int DefaultScreenDpi = 96;

    /// <summary>Calculate a DPI scale factor.</summary>
    /// <param name="oldDpi">The source DPI.</param>
    /// <param name="newDpi">The target DPI.</param>
    /// <returns>float.</returns>
    public static float DpiScaleFactor(int oldDpi, int newDpi) => (float)newDpi / (float)oldDpi;

    /// <summary>Calculate a DPI scale factor based on the default screen DPI.</summary>
    /// <param name="dpi">int.</param>
    /// <returns>float.</returns>
    public static float DpiScaleFactor(int dpi) => (float)dpi / (float)DefaultScreenDpi;

    /// <summary>Scale the supplied number according to the supplied dpi.</summary>
    /// <param name="someNumber">float with e.g. the width 16 for 16x16 images.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <returns>float with the scaled number.</returns>
    public static float ScaleWithDpi(float someNumber, int dpi) => ScaleWithDpi(someNumber, dpi, null);

    /// <summary>Scale the supplied number according to the supplied dpi.</summary>
    /// <param name="someNumber">float with e.g. the width 16 for 16x16 images.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <param name="scaleModifier">A function which can modify the scale factor.</param>
    /// <returns>float with the scaled number.</returns>
    public static float ScaleWithDpi(float someNumber, int dpi, Func<float, float> scaleModifier) => ScaleFactor(dpi, scaleModifier) * someNumber;

    /// <summary>Scale the supplied number according to the supplied dpi.</summary>
    /// <param name="someNumber">double with e.g. the width 16 for 16x16 images.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <returns>double with the scaled number.</returns>
    public static double ScaleWithDpi(double someNumber, int dpi) => ScaleWithDpi(someNumber, dpi, null);

    /// <summary>Scale the supplied number according to the supplied dpi.</summary>
    /// <param name="someNumber">double with e.g. the width 16 for 16x16 images.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <param name="scaleModifier">A function which can modify the scale factor.</param>
    /// <returns>double with the scaled number.</returns>
    public static double ScaleWithDpi(double someNumber, int dpi, Func<float, float> scaleModifier) => (double)ScaleFactor(dpi, scaleModifier) * someNumber;

    /// <summary>Scale the supplied number according to the supplied dpi.</summary>
    /// <param name="number">int with e.g. 16 for 16x16 images.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <returns>Scaled width.</returns>
    public static int ScaleWithDpi(int number, int dpi) => ScaleWithDpi(number, dpi, null);

    /// <summary>Scale the supplied number according to the supplied dpi.</summary>
    /// <param name="number">int with e.g. 16 for 16x16 images.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <param name="scaleModifier">A function which can modify the scale factor.</param>
    /// <returns>Scaled width.</returns>
    public static int ScaleWithDpi(int number, int dpi, Func<float, float> scaleModifier) => checked((int)(ScaleFactor(dpi, scaleModifier) * (float)number));

    /// <summary>Scale the supplied NativeSize according to the supplied dpi.</summary>
    /// <param name="size">NativeSize to resize.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <returns>NativeSize scaled.</returns>
    public static NativeSize ScaleWithDpi(NativeSize size, int dpi) => ScaleWithDpi(size, dpi, null);

    /// <summary>Scale the supplied NativeSize according to the supplied dpi.</summary>
    /// <param name="size">NativeSize to resize.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <param name="scaleModifier">A function which can modify the scale factor.</param>
    /// <returns>NativeSize scaled.</returns>
    public static NativeSize ScaleWithDpi(NativeSize size, int dpi, Func<float, float> scaleModifier)
    {
        float scaleFactor = ScaleFactor(dpi, scaleModifier);
        return checked(new NativeSize((int)(scaleFactor * (float)size.Width), (int)(scaleFactor * (float)size.Height)));
    }

    /// <summary>Scale the supplied NativePoint according to the supplied dpi.</summary>
    /// <param name="size">NativePoint to resize.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <returns>NativePoint scaled.</returns>
    public static NativePoint ScaleWithDpi(NativePoint size, int dpi) => ScaleWithDpi(size, dpi, null);

    /// <summary>Scale the supplied NativePoint according to the supplied dpi.</summary>
    /// <param name="size">NativePoint to resize.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <param name="scaleModifier">A function which can modify the scale factor.</param>
    /// <returns>NativePoint scaled.</returns>
    public static NativePoint ScaleWithDpi(NativePoint size, int dpi, Func<float, float> scaleModifier)
    {
        float scaleFactor = ScaleFactor(dpi, scaleModifier);
        return checked(new NativePoint((int)(scaleFactor * (float)size.X), (int)(scaleFactor * (float)size.Y)));
    }

    /// <summary>Scale the supplied NativeSizeFloat according to the supplied dpi.</summary>
    /// <param name="size">NativeSizeFloat to resize.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <returns>NativeSize scaled.</returns>
    public static NativeSizeFloat ScaleWithDpi(NativeSizeFloat size, int dpi) => ScaleWithDpi(size, dpi, null);

    /// <summary>Scale the supplied NativeSizeFloat according to the supplied dpi.</summary>
    /// <param name="size">NativeSizeFloat to resize.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <param name="scaleModifier">A function which can modify the scale factor.</param>
    /// <returns>NativeSize scaled.</returns>
    public static NativeSizeFloat ScaleWithDpi(NativeSizeFloat size, int dpi, Func<float, float> scaleModifier)
    {
        float scaleFactor = ScaleFactor(dpi, scaleModifier);
        return new(scaleFactor * size.Width, scaleFactor * size.Height);
    }

    /// <summary>Scale the supplied NativePointFloat according to the supplied dpi.</summary>
    /// <param name="point">NativePointFloat to resize.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <returns>NativePointFloat scaled.</returns>
    public static NativePointFloat ScaleWithDpi(NativePointFloat point, int dpi) => ScaleWithDpi(point, dpi, null);

    /// <summary>Scale the supplied NativePointFloat according to the supplied dpi.</summary>
    /// <param name="point">NativePointFloat to resize.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <param name="scaleModifier">A function which can modify the scale factor.</param>
    /// <returns>NativePointFloat scaled.</returns>
    public static NativePointFloat ScaleWithDpi(NativePointFloat point, int dpi, Func<float, float> scaleModifier)
    {
        float scaleFactor = ScaleFactor(dpi, scaleModifier);
        return new(scaleFactor * point.X, scaleFactor * point.Y);
    }

    /// <summary>Calculate a DPI unscale factor.</summary>
    /// <param name="oldDpi">int with the old dpi.</param>
    /// <param name="newDpi">int with the new dpi.</param>
    /// <returns>float with the unscale factor.</returns>
    public static float DpiUnscaleFactor(int oldDpi, int newDpi) => (float)oldDpi / (float)newDpi;

    /// <summary>Calculate a DPI unscale factor against the default screen DPI.</summary>
    /// <param name="dpi">int.</param>
    /// <returns>float with the unscale factor.</returns>
    public static float DpiUnscaleFactor(int dpi) => DpiUnscaleFactor(DefaultScreenDpi, dpi);

    /// <summary>Unscale the supplied number according to the supplied dpi.</summary>
    /// <param name="someNumber">double with e.g. the scaled width.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <returns>float with the unscaled number.</returns>
    public static double UnscaleWithDpi(double someNumber, int dpi) => UnscaleWithDpi(someNumber, dpi, null);

    /// <summary>Unscale the supplied number according to the supplied dpi.</summary>
    /// <param name="someNumber">double with e.g. the scaled width.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <param name="scaleModifier">A function which can modify the scale factor.</param>
    /// <returns>float with the unscaled number.</returns>
    public static double UnscaleWithDpi(double someNumber, int dpi, Func<float, float> scaleModifier)
    {
        float dpiUnscaleFactor = DpiUnscaleFactor(dpi);
        if (scaleModifier is not null)
        {
            dpiUnscaleFactor = scaleModifier(dpiUnscaleFactor);
        }

        return (double)dpiUnscaleFactor * someNumber;
    }

    /// <summary>Unscale the supplied number according to the supplied dpi.</summary>
    /// <param name="number">int with a scaled width.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <returns>Unscaled width.</returns>
    public static int UnscaleWithDpi(int number, int dpi) => UnscaleWithDpi(number, dpi, null);

    /// <summary>Unscale the supplied number according to the supplied dpi.</summary>
    /// <param name="number">int with a scaled width.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <param name="scaleModifier">A function which can modify the scale factor.</param>
    /// <returns>Unscaled width.</returns>
    public static int UnscaleWithDpi(int number, int dpi, Func<float, float> scaleModifier)
    {
        float dpiUnscaleFactor = DpiUnscaleFactor(dpi);
        if (scaleModifier is not null)
        {
            dpiUnscaleFactor = scaleModifier(dpiUnscaleFactor);
        }

        return checked((int)(dpiUnscaleFactor * (float)number));
    }

    /// <summary>Unscale the supplied NativeSize according to the supplied dpi.</summary>
    /// <param name="size">NativeSize to unscale.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <returns>NativeSize unscaled.</returns>
    public static NativeSize UnscaleWithDpi(NativeSize size, int dpi) => UnscaleWithDpi(size, dpi, null);

    /// <summary>Unscale the supplied NativeSize according to the supplied dpi.</summary>
    /// <param name="size">NativeSize to unscale.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <param name="scaleModifier">A function which can modify the scale factor.</param>
    /// <returns>NativeSize unscaled.</returns>
    public static NativeSize UnscaleWithDpi(NativeSize size, int dpi, Func<float, float> scaleModifier)
    {
        float dpiUnscaleFactor = DpiUnscaleFactor(dpi);
        if (scaleModifier is not null)
        {
            dpiUnscaleFactor = scaleModifier(dpiUnscaleFactor);
        }

        return checked(new NativeSize((int)(dpiUnscaleFactor * (float)size.Width), (int)(dpiUnscaleFactor * (float)size.Height)));
    }

    /// <summary>Unscale the supplied NativePoint according to the supplied dpi.</summary>
    /// <param name="size">NativePoint to unscale.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <returns>NativePoint unscaled.</returns>
    public static NativePoint UnscaleWithDpi(NativePoint size, int dpi) => UnscaleWithDpi(size, dpi, null);

    /// <summary>Unscale the supplied NativePoint according to the supplied dpi.</summary>
    /// <param name="size">NativePoint to unscale.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <param name="scaleModifier">A function which can modify the scale factor.</param>
    /// <returns>NativePoint unscaled.</returns>
    public static NativePoint UnscaleWithDpi(NativePoint size, int dpi, Func<float, float> scaleModifier)
    {
        float dpiUnscaleFactor = DpiUnscaleFactor(dpi);
        if (scaleModifier is not null)
        {
            dpiUnscaleFactor = scaleModifier(dpiUnscaleFactor);
        }

        return checked(new NativePoint((int)(dpiUnscaleFactor * (float)size.X), (int)(dpiUnscaleFactor * (float)size.Y)));
    }

    /// <summary>Unscale the supplied NativeSizeFloat according to the supplied dpi.</summary>
    /// <param name="size">NativeSizeFloat to resize.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <returns>NativeSize unscaled.</returns>
    public static NativeSizeFloat UnscaleWithDpi(NativeSizeFloat size, int dpi) => UnscaleWithDpi(size, dpi, null);

    /// <summary>Unscale the supplied NativeSizeFloat according to the supplied dpi.</summary>
    /// <param name="size">NativeSizeFloat to resize.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <param name="scaleModifier">A function which can modify the scale factor.</param>
    /// <returns>NativeSize unscaled.</returns>
    public static NativeSizeFloat UnscaleWithDpi(NativeSizeFloat size, int dpi, Func<float, float> scaleModifier)
    {
        float unscaleFactor = UnscaleFactor(dpi, scaleModifier);
        return new(unscaleFactor * size.Width, unscaleFactor * size.Height);
    }

    /// <summary>Unscale the supplied NativePointFloat according to the supplied dpi.</summary>
    /// <param name="point">NativePointFloat to resize.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <returns>NativePointFloat unscaled.</returns>
    public static NativePointFloat UnscaleWithDpi(NativePointFloat point, int dpi) => UnscaleWithDpi(point, dpi, null);

    /// <summary>Unscale the supplied NativePointFloat according to the supplied dpi.</summary>
    /// <param name="point">NativePointFloat to resize.</param>
    /// <param name="dpi">current dpi, normal is 96.</param>
    /// <param name="scaleModifier">A function which can modify the scale factor.</param>
    /// <returns>NativePointFloat unscaled.</returns>
    public static NativePointFloat UnscaleWithDpi(NativePointFloat point, int dpi, Func<float, float> scaleModifier)
    {
        float unscaleFactor = UnscaleFactor(dpi, scaleModifier);
        return new(unscaleFactor * point.X, unscaleFactor * point.Y);
    }

    /// <summary>Calculates the effective scale factor for the supplied DPI.</summary>
    /// <param name="dpi">The DPI to scale for.</param>
    /// <param name="scaleModifier">An optional scale-factor modifier.</param>
    /// <returns>The effective scale factor.</returns>
    private static float ScaleFactor(int dpi, Func<float, float> scaleModifier) => ApplyScaleModifier(DpiScaleFactor(dpi), scaleModifier);

    /// <summary>Calculates the effective unscale factor for the supplied DPI.</summary>
    /// <param name="dpi">The DPI to unscale for.</param>
    /// <param name="scaleModifier">An optional scale-factor modifier.</param>
    /// <returns>The effective unscale factor.</returns>
    private static float UnscaleFactor(int dpi, Func<float, float> scaleModifier) => ApplyScaleModifier(DpiUnscaleFactor(dpi), scaleModifier);

    /// <summary>Applies a modifier to a scale factor when one is supplied.</summary>
    /// <param name="scaleFactor">The source scale factor.</param>
    /// <param name="scaleModifier">An optional scale-factor modifier.</param>
    /// <returns>The original or modified scale factor.</returns>
    private static float ApplyScaleModifier(float scaleFactor, Func<float, float> scaleModifier) => scaleModifier?.Invoke(scaleFactor) ?? scaleFactor;
}
