// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing.Imaging;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Exercises the remaining in-memory cursor-composition paths used by release builds.</summary>
public sealed class CoverageReleaseCursorHelperTests
{
    /// <summary>Verifies that an enlarged standard cursor retains a scaled hotspot and target size.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CaptureCurrentCursor_ScalesStandardFallbackCursorFromInMemoryBitmapsAsync()
    {
        var previousProvider = Icons.CursorHelper.SetCursorBaseSizeProviderForTesting(static () => ThirtyTwo * Two);
        try
        {
            using var colorBitmap = CreateBitmap(ThirtyTwo, ThirtyTwo, PixelFormat.Format32bppArgb, Color.CornflowerBlue);
            using var maskBitmap = CreateBitmap(ThirtyTwo, ThirtyTwo, PixelFormat.Format32bppArgb, Color.White);
            var iconInfo = CreateIconInfo(colorBitmap.GetHbitmap(), maskBitmap.GetHbitmap(), new(Three, Four));
            using var captured = new CapturedCursor();

            Icons.CursorHelper.CaptureCurrentCursor(captured, new(One), iconInfo);
            iconInfo.Dispose();

            await Assert.That(captured.Size).IsEqualTo(new(ThirtyTwo * Two, ThirtyTwo * Two));
            await Assert.That(captured.HotSpot).IsEqualTo(new(Six, Eight));
            await Assert.That(captured.ColorLayer).IsNotNull();
            await Assert.That(captured.MaskLayer).IsNull();
        }
        finally
        {
            _ = Icons.CursorHelper.SetCursorBaseSizeProviderForTesting(previousProvider);
        }
    }

    /// <summary>Verifies fallback layer creation when native bitmap metadata cannot be read.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CaptureCurrentCursor_UsesTargetSizeWhenNativeBitmapMetadataIsUnavailableAsync()
    {
        var previousProvider = Icons.CursorHelper.SetCursorBaseSizeProviderForTesting(static () => ThirtyTwo);
        try
        {
            var iconInfo = IconInfoEx.Create();
            using var captured = new CapturedCursor();

            Icons.CursorHelper.CaptureCurrentCursor(captured, new(One), iconInfo);

            await Assert.That(captured.Size).IsEqualTo(new(ThirtyTwo, ThirtyTwo));
            await Assert.That(captured.ColorLayer).IsNotNull();
            await Assert.That(captured.MaskLayer).IsNotNull();
        }
        finally
        {
            _ = Icons.CursorHelper.SetCursorBaseSizeProviderForTesting(previousProvider);
        }
    }

    /// <summary>Verifies BGR mask composition without depending on the desktop cursor state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DrawCursorOnBitmap_ComposesTwentyFourBitMaskPixelsAsync()
    {
        using var target = CreateBitmap(Four, Four, PixelFormat.Format24bppRgb, Color.White);
        using var color = CreateBitmap(Two, Two, PixelFormat.Format24bppRgb, Color.Blue);
        using var mask = CreateBitmap(Two, Two, PixelFormat.Format24bppRgb, Color.Black);
        using var cursor = new CapturedCursor { ColorLayer = color, MaskLayer = mask, Size = new(Two, Two) };

        Icons.CursorHelper.DrawCursorOnBitmap(target, cursor, new(One, One));

        await Assert.That(target.GetPixel(One, One).ToArgb()).IsEqualTo(Color.Blue.ToArgb());
    }

    /// <summary>Creates an in-memory bitmap with a deterministic fill.</summary>
    /// <param name="width">The bitmap width.</param>
    /// <param name="height">The bitmap height.</param>
    /// <param name="format">The bitmap pixel format.</param>
    /// <param name="color">The fill color.</param>
    /// <returns>The created bitmap.</returns>
    private static Bitmap CreateBitmap(int width, int height, PixelFormat format, Color color)
    {
        var bitmap = new Bitmap(width, height, format);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(color);
        return bitmap;
    }

    /// <summary>Creates native icon metadata with the supplied in-memory bitmap handles.</summary>
    /// <param name="colorBitmapHandle">The color bitmap handle.</param>
    /// <param name="maskBitmapHandle">The mask bitmap handle.</param>
    /// <param name="hotspot">The cursor hotspot.</param>
    /// <returns>The populated icon metadata.</returns>
    private static IconInfoEx CreateIconInfo(IntPtr colorBitmapHandle, IntPtr maskBitmapHandle, NativePoint hotspot)
    {
        var iconInfo = IconInfoEx.Create();
        var buffer = Marshal.AllocHGlobal(Marshal.SizeOf<IconInfoEx>());
        const int isIconOffset = sizeof(uint);
        const int hotspotXOffset = isIconOffset + sizeof(int);
        const int hotspotYOffset = hotspotXOffset + sizeof(int);
        const int maskBitmapHandleOffset = hotspotYOffset + sizeof(int);
        var colorBitmapHandleOffset = maskBitmapHandleOffset + IntPtr.Size;
        try
        {
            Marshal.StructureToPtr(iconInfo, buffer, fDeleteOld: false);
            Marshal.WriteInt32(buffer, isIconOffset, 0);
            Marshal.WriteInt32(buffer, hotspotXOffset, hotspot.X);
            Marshal.WriteInt32(buffer, hotspotYOffset, hotspot.Y);
            Marshal.WriteIntPtr(buffer, maskBitmapHandleOffset, maskBitmapHandle);
            Marshal.WriteIntPtr(buffer, colorBitmapHandleOffset, colorBitmapHandle);
            return Marshal.PtrToStructure<IconInfoEx>(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
