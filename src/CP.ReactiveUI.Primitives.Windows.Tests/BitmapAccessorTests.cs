// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Bitmap Accessor Tests behavior.</summary>
public class BitmapAccessorTests
{
    /// <summary>Defines the Large Bitmap Size test value.</summary>
    private const int LargeBitmapSize = 10;

    /// <summary>Defines the Small Bitmap Size test value.</summary>
    private const int SmallBitmapSize = 5;

    /// <summary>Defines the Max Channel Value test value.</summary>
    private const byte MaxChannelValue = byte.MaxValue;

    /// <summary>Defines the Min Channel Value test value.</summary>
    private const byte MinChannelValue = 0;

    /// <summary>Test BitmapAccessor with 32-bit ARGB format.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestBitmapAccessor_32bppArgbAsync()
    {
        using var bitmap = new Bitmap(LargeBitmapSize, LargeBitmapSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var accessor = new BitmapAccessor<Bgra32>(bitmap, readOnly: false))
        {
            await Assert.That(accessor.Width).IsEqualTo(LargeBitmapSize);
            await Assert.That(accessor.Height).IsEqualTo(LargeBitmapSize);
            await Assert.That(accessor.PixelFormat).IsEqualTo(System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var row = accessor.GetRowSpan(0);
            var rowLength = row.Length;
            row[0] = new(MaxChannelValue, MinChannelValue, MinChannelValue, MaxChannelValue);

            await Assert.That(rowLength).IsEqualTo(LargeBitmapSize);
        }

        var color = bitmap.GetPixel(0, 0);
        await Assert.That(color.R).IsEqualTo(MaxChannelValue);
        await Assert.That(color.G).IsEqualTo(MinChannelValue);
        await Assert.That(color.B).IsEqualTo(MinChannelValue);
        await Assert.That(color.A).IsEqualTo(MaxChannelValue);
    }

    /// <summary>Test BitmapAccessor with 24-bit RGB format.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestBitmapAccessor_24bppRgbAsync()
    {
        using var bitmap = new Bitmap(LargeBitmapSize, LargeBitmapSize, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        using var accessor = new BitmapAccessor<Bgr24>(bitmap, readOnly: false);
        await Assert.That(accessor.Width).IsEqualTo(LargeBitmapSize);
        await Assert.That(accessor.Height).IsEqualTo(LargeBitmapSize);
        await Assert.That(accessor.PixelFormat).IsEqualTo(System.Drawing.Imaging.PixelFormat.Format24bppRgb);

        var row = accessor.GetRowSpan(SmallBitmapSize);
        await Assert.That(row.Length).IsEqualTo(LargeBitmapSize);
    }

    /// <summary>Test BitmapAccessor ProcessRows method.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestBitmapAccessor_ProcessRowsAsync()
    {
        using var bitmap = new Bitmap(SmallBitmapSize, SmallBitmapSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var accessor = new BitmapAccessor<Bgra32>(bitmap, readOnly: false))
        {
            var rowsProcessed = 0;
            accessor.ProcessRows((y, row) =>
            {
                _ = y;
                rowsProcessed++;
                for (var x = 0; x < SmallBitmapSize; x++)
                {
                    row[x] = new(MaxChannelValue, MinChannelValue, MinChannelValue, MaxChannelValue);
                }
            });

            await Assert.That(rowsProcessed).IsEqualTo(SmallBitmapSize);
        }

        for (var y = 0; y < SmallBitmapSize; y++)
        {
            for (var x = 0; x < SmallBitmapSize; x++)
            {
                var color = bitmap.GetPixel(x, y);
                await Assert.That(color.R).IsEqualTo(MaxChannelValue);
                await Assert.That(color.G).IsEqualTo(MinChannelValue);
                await Assert.That(color.B).IsEqualTo(MinChannelValue);
            }
        }
    }

    /// <summary>Test BitmapAccessor with unsupported format throws exception.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestBitmapAccessor_UnsupportedFormat_ThrowsExceptionAsync()
    {
        using var bitmap = new Bitmap(LargeBitmapSize, LargeBitmapSize, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

        await Assert.That(() =>
        {
            var accessor = new BitmapAccessor<Bgra32>(bitmap, readOnly: true);
            _ = accessor;
        }).Throws<NotSupportedException>();
    }
}
