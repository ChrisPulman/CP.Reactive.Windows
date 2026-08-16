// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Tests.Benchmarks;

/// <summary>Benchmarks screen-bound discovery APIs.</summary>
[MinColumn]
[MaxColumn]
[MemoryDiagnoser]
public class ScreenboundsBenchmark
{
    /// <summary>Warms up display-bound discovery.</summary>
    [GlobalSetup]
    public void Setup() => ScreenBoundsPInvoke();

    /// <summary>Measures native display-bound discovery.</summary>
    [Benchmark]
    public void ScreenBoundsNative() => _ = GetScreenBounds();

    /// <summary>Measures WinForms screen-bound discovery.</summary>
    [Benchmark]
    public void ScreenBoundsPInvoke() => WarmUpScreenBounds();

    /// <summary>Measures cached WinForms screen-bound access.</summary>
    [Benchmark]
    public void ScreenBoundsPInvokeCached()
    {
    }

    /// <summary>Get the bounds of the complete screen.</summary>
    /// <returns>The bounds of the complete screen.</returns>
    public NativeRect GetAllScreenBounds()
    {
        int left = 0;
        int top = 0;
        int bottom = 0;
        int right = 0;
        foreach (var display in DisplayTopology.GetSnapshot())
        {
            var currentBounds = display.Bounds;
            left = Math.Min(left, currentBounds.X);
            top = Math.Min(top, currentBounds.Y);
            var screenAbsRight = currentBounds.X + currentBounds.Width;
            var screenAbsBottom = currentBounds.Y + currentBounds.Height;
            right = Math.Max(right, screenAbsRight);
            bottom = Math.Max(bottom, screenAbsBottom);
        }

        return new(left, top, right + Math.Abs(left), bottom + Math.Abs(top));
    }

    /// <summary>Get the bounds of all screens combined.</summary>
    /// <returns>A NativeRect of the bounds of the entire display area.</returns>
    public NativeRect GetScreenBounds()
    {
        int left = 0;
        int top = 0;
        int bottom = 0;
        int right = 0;
        foreach (var screen in Screen.AllScreens)
        {
            left = Math.Min(left, screen.Bounds.X);
            top = Math.Min(top, screen.Bounds.Y);
            var screenAbsRight = screen.Bounds.X + screen.Bounds.Width;
            var screenAbsBottom = screen.Bounds.Y + screen.Bounds.Height;
            right = Math.Max(right, screenAbsRight);
            bottom = Math.Max(bottom, screenAbsBottom);
        }

        return new(left, top, right + Math.Abs(left), bottom + Math.Abs(top));
    }

    /// <summary>Warms up the native display-bound discovery path.</summary>
    private void WarmUpScreenBounds() => _ = GetAllScreenBounds();
}
