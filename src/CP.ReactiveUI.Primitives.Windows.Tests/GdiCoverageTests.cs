// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing.Imaging;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides extended coverage for GDI structs, handles, and safe native paths.</summary>
public sealed class GdiCoverageTests
{
    /// <summary>Defines the bitmap header bits-per-pixel test value.</summary>
    private const ushort ThirtyTwoBitsPerPixel = 32;

    /// <summary>Defines the bitmap width test value.</summary>
    private const int BitmapWidth = 3;

    /// <summary>Defines the bitmap height test value.</summary>
    private const int BitmapHeight = 2;

    /// <summary>Defines the expected byte size of the test bitmap image.</summary>
    private const uint BitmapImageSize = 24U;

    /// <summary>Defines the number of bytes occupied by three DWORD color masks.</summary>
    private const uint BitfieldMaskBytes = 12U;

    /// <summary>Gets returns a 24 bits-per-pixel value without widening the constant's visible use.</summary>
    /// <returns>Twenty four bits per pixel.</returns>
    private static ushort TwentyFourBitsPerPixel => TwentyFour;

    /// <summary>Tests BitmapInfoHeader creation and equality behavior.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapInfoHeader_Create_InitializesExpectedDibInfoFieldsAsync()
    {
        var header = BitmapInfoHeader.Create(BitmapWidth, -BitmapHeight, ThirtyTwoBitsPerPixel);
        var same = BitmapInfoHeader.Create(BitmapWidth, -BitmapHeight, ThirtyTwoBitsPerPixel);
        var different = BitmapInfoHeader.Create(BitmapWidth, BitmapHeight, TwentyFourBitsPerPixel);

        await Assert.That(header.Size).IsEqualTo((uint)Marshal.SizeOf<BitmapInfoHeader>());
        await Assert.That(header.Width).IsEqualTo(BitmapWidth);
        await Assert.That(header.Height).IsEqualTo(-BitmapHeight);
        await Assert.That(header.Planes).IsEqualTo((ushort)1);
        await Assert.That(header.BitCount).IsEqualTo(ThirtyTwoBitsPerPixel);
        await Assert.That(header.Compression).IsEqualTo(BitmapCompressionMethods.BI_RGB);
        await Assert.That(header.SizeImage).IsEqualTo(BitmapImageSize);
        await Assert.That(header.XPelsPerMeter).IsEqualTo(0);
        await Assert.That(header.YPelsPerMeter).IsEqualTo(0);
        await Assert.That(header.ColorsImportant).IsEqualTo(0U);
        await Assert.That(header.OffsetToPixels).IsEqualTo(header.Size);
        header.Compression = BitmapCompressionMethods.BI_BITFIELDS;
        await Assert.That(header.OffsetToPixels).IsEqualTo(header.Size + BitfieldMaskBytes);
        header.Compression = BitmapCompressionMethods.BI_RGB;
        await Assert.That(header.IsDibV4).IsFalse();
        await Assert.That(header.IsDibV5).IsFalse();
        await Assert.That(header == same).IsTrue();
        await Assert.That(header != different).IsTrue();
        await Assert.That(header.Equals((object)same)).IsTrue();
        await Assert.That(header.Equals(new object())).IsFalse();
        await Assert.That(header.GetHashCode()).IsEqualTo(0);
    }

    /// <summary>Tests BitmapInfoHeader bitfield offset calculation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapInfoHeader_OffsetToPixels_AddsBitfieldMasksAsync()
    {
        var header = BitmapInfoHeader.Create(BitmapWidth, BitmapHeight, ThirtyTwoBitsPerPixel);
        header.Compression = BitmapCompressionMethods.BI_BITFIELDS;

        await Assert.That(header.OffsetToPixels).IsEqualTo(header.Size + BitfieldMaskBytes);
    }

    /// <summary>Tests BitmapV4Header creation and equality behavior.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapV4Header_Create_InitializesMasksAndTopDownImageSizeAsync()
    {
        var header = BitmapV4Header.Create(BitmapWidth, -BitmapHeight, ThirtyTwoBitsPerPixel);
        var same = BitmapV4Header.Create(BitmapWidth, -BitmapHeight, ThirtyTwoBitsPerPixel);
        var different = header;
        different.GammaBlue = 1;

        await Assert.That(header.Size).IsEqualTo((uint)Marshal.SizeOf<BitmapV4Header>());
        await Assert.That(header.SizeImage).IsEqualTo(BitmapImageSize);
        await Assert.That(header.RedMask).IsEqualTo(0x00FF0000U);
        await Assert.That(header.GreenMask).IsEqualTo(0x0000FF00U);
        await Assert.That(header.BlueMask).IsEqualTo(0x000000FFU);
        await Assert.That(header.AlphaMask).IsEqualTo(0xFF000000U);
        await Assert.That(header.ColorSpace).IsEqualTo(ColorSpace.LCS_sRGB);
        await Assert.That(header.IsDibV4).IsTrue();
        await Assert.That(header.IsDibV5).IsFalse();
        await Assert.That(header.OffsetToPixels).IsEqualTo(header.Size);
        await Assert.That(header == same).IsTrue();
        await Assert.That(header != different).IsTrue();
        await Assert.That(header.Equals((object)same)).IsTrue();
        await Assert.That(header.Equals(new object())).IsFalse();
        await Assert.That(header.GetHashCode()).IsEqualTo(0);
    }

    /// <summary>Tests BitmapV5Header creation and equality behavior.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapV5Header_Create_InitializesProfileFieldsAndTopDownImageSizeAsync()
    {
        var header = BitmapV5Header.Create(BitmapWidth, -BitmapHeight, ThirtyTwoBitsPerPixel);
        var same = BitmapV5Header.Create(BitmapWidth, -BitmapHeight, ThirtyTwoBitsPerPixel);
        var different = header;
        different.ProfileSize = 1;

        await Assert.That(header.Size).IsEqualTo((uint)Marshal.SizeOf<BitmapV5Header>());
        await Assert.That(header.SizeImage).IsEqualTo(BitmapImageSize);
        await Assert.That(header.RedMask).IsEqualTo(0x00FF0000U);
        await Assert.That(header.GreenMask).IsEqualTo(0x0000FF00U);
        await Assert.That(header.BlueMask).IsEqualTo(0x000000FFU);
        await Assert.That(header.AlphaMask).IsEqualTo(0xFF000000U);
        await Assert.That(header.ColorSpace).IsEqualTo(ColorSpace.LCS_sRGB);
        await Assert.That(header.Intent).IsEqualTo(ColorSpace.LCS_GM_IMAGES);
        await Assert.That(header.ProfileData).IsEqualTo(0U);
        await Assert.That(header.ProfileSize).IsEqualTo(0U);
        await Assert.That(header.Reserved).IsEqualTo(0U);
        await Assert.That(header.IsDibV4).IsFalse();
        await Assert.That(header.IsDibV5).IsTrue();
        await Assert.That(header.OffsetToPixels).IsEqualTo(header.Size);
        header.Compression = BitmapCompressionMethods.BI_BITFIELDS;
        await Assert.That(header.OffsetToPixels).IsEqualTo(header.Size + BitfieldMaskBytes);
        header.Compression = BitmapCompressionMethods.BI_RGB;
        await Assert.That(header == same).IsTrue();
        await Assert.That(header != different).IsTrue();
        await Assert.That(header.Equals((object)same)).IsTrue();
        await Assert.That(header.Equals(new object())).IsFalse();
        await Assert.That(header.GetHashCode()).IsEqualTo(0);
    }

    /// <summary>Tests BitmapFileHeader creation from DIB headers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapFileHeader_Create_CalculatesSizesOffsetsAndEqualityAsync()
    {
        var bitmapInfoHeader = BitmapInfoHeader.Create(BitmapWidth, BitmapHeight, ThirtyTwoBitsPerPixel);
        bitmapInfoHeader.ColorsUsed = Two;
        var infoFileHeader = BitmapFileHeader.Create(bitmapInfoHeader);
        var bitmapV5Header = BitmapV5Header.Create(BitmapWidth, BitmapHeight, ThirtyTwoBitsPerPixel);
        bitmapV5Header.ColorsUsed = Three;
        var v5FileHeader = BitmapFileHeader.Create(bitmapV5Header);

        await Assert.That(Marshal.SizeOf<BitmapFileHeader>()).IsEqualTo(Fourteen);
        await Assert.That(infoFileHeader.FileType).IsEqualTo((short)0x4D42);
        await Assert.That(infoFileHeader.Size).IsEqualTo(Fourteen + (int)bitmapInfoHeader.Size + (int)bitmapInfoHeader.SizeImage);
        await Assert.That(infoFileHeader.OffsetToBitmapBits).IsEqualTo(Fourteen + (int)bitmapInfoHeader.Size + Eight);
        await Assert.That(v5FileHeader.OffsetToBitmapBits).IsEqualTo(Fourteen + (int)bitmapV5Header.Size + Twelve);
        await Assert.That(infoFileHeader == BitmapFileHeader.Create(bitmapInfoHeader)).IsTrue();
        await Assert.That(infoFileHeader != v5FileHeader).IsTrue();
        await Assert.That(infoFileHeader.Equals((object)BitmapFileHeader.Create(bitmapInfoHeader))).IsTrue();
        await Assert.That(infoFileHeader.Equals(new object())).IsFalse();
        await Assert.That(infoFileHeader.GetHashCode()).IsEqualTo(typeof(BitmapFileHeader).GetHashCode());
    }

    /// <summary>Tests color struct construction and equality behavior.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ColorStructs_ExposeExpectedComponentsAndEqualityAsync()
    {
        var rgb = new RgbQuad { Blue = 1, Green = Two, Red = Three, Reserved = Four };
        var sameRgb = new RgbQuad { Blue = 1, Green = Two, Red = Three, Reserved = Four };
        var differentRgb = new RgbQuad { Blue = 1, Green = Two, Red = Nine, Reserved = Four };
        var xyz = CieXyz.Create(FortyTwo);
        var triple = CieXyzTriple.Create(CieXyz.Create(1), CieXyz.Create(Two), CieXyz.Create(Three));
        var sameTriple = CieXyzTriple.Create(CieXyz.Create(1), CieXyz.Create(Two), CieXyz.Create(Three));

        await Assert.That(Marshal.SizeOf<RgbQuad>()).IsEqualTo(Four);
        await Assert.That(rgb.Blue).IsEqualTo((byte)1);
        await Assert.That(rgb.Green).IsEqualTo((byte)Two);
        await Assert.That(rgb.Red).IsEqualTo((byte)Three);
        await Assert.That(rgb.Reserved).IsEqualTo((byte)Four);
        await Assert.That(rgb == sameRgb).IsTrue();
        await Assert.That(rgb != differentRgb).IsTrue();
        await Assert.That(rgb.Equals((object)sameRgb)).IsTrue();
        await Assert.That(rgb.Equals(new object())).IsFalse();
        await Assert.That(rgb.GetHashCode()).IsEqualTo(typeof(RgbQuad).GetHashCode());
        await Assert.That(xyz.X).IsEqualTo(UIntFortyTwo);
        await Assert.That(xyz.Y).IsEqualTo(UIntFortyTwo);
        await Assert.That(xyz.Z).IsEqualTo(UIntFortyTwo);
        await Assert.That(xyz == CieXyz.Create(FortyTwo)).IsTrue();
        await Assert.That(xyz != CieXyz.Create(FortyThree)).IsTrue();
        await Assert.That(xyz.Equals((object)CieXyz.Create(FortyTwo))).IsTrue();
        await Assert.That(xyz.Equals(new object())).IsFalse();
        await Assert.That(xyz.GetHashCode()).IsEqualTo(typeof(CieXyz).GetHashCode());
        await Assert.That(triple.Red).IsEqualTo(CieXyz.Create(1));
        await Assert.That(triple.Green).IsEqualTo(CieXyz.Create(Two));
        await Assert.That(triple.Blue).IsEqualTo(CieXyz.Create(Three));
        await Assert.That(triple == sameTriple).IsTrue();
        await Assert.That(triple != CieXyzTriple.Create(CieXyz.Create(1), CieXyz.Create(Nine), CieXyz.Create(Three))).IsTrue();
        await Assert.That(triple.Equals((object)sameTriple)).IsTrue();
        await Assert.That(triple.Equals(new object())).IsFalse();
        await Assert.That(triple.GetHashCode()).IsEqualTo(typeof(CieXyzTriple).GetHashCode());
    }

    /// <summary>Tests bitfield color mask constructors and factories.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitfieldColorMask_ConstructorsAndFactories_ShiftComponentsAsync()
    {
        var defaultMask = new BitfieldColorMask();
        var redMask = BitfieldColorMask.Create(0x12);
        var redGreenMask = BitfieldColorMask.Create(0x12, 0x34);
        var rgbMask = BitfieldColorMask.Create(0x12, 0x34, 0x56);

        await Assert.That(defaultMask).IsEqualTo(BitfieldColorMask.Create());
        await Assert.That(defaultMask.Red).IsEqualTo(0x0000FF00U);
        await Assert.That(defaultMask.Green).IsEqualTo(0x00FF0000U);
        await Assert.That(defaultMask.Blue).IsEqualTo(0xFF000000U);
        await Assert.That(redMask.Red).IsEqualTo(0x00001200U);
        await Assert.That(redMask.Green).IsEqualTo(0x00FF0000U);
        await Assert.That(redGreenMask.Green).IsEqualTo(0x00340000U);
        await Assert.That(rgbMask.Blue).IsEqualTo(0x56000000U);
        await Assert.That(rgbMask == new BitfieldColorMask(0x12, 0x34, 0x56)).IsTrue();
        await Assert.That(rgbMask != new BitfieldColorMask(0x12, 0x34, 0x57)).IsTrue();
        await Assert.That(rgbMask.Equals((object)new BitfieldColorMask(0x12, 0x34, 0x56))).IsTrue();
        await Assert.That(rgbMask.Equals(new object())).IsFalse();
        await Assert.That(rgbMask.GetHashCode()).IsEqualTo(new BitfieldColorMask(0x12, 0x34, 0x56).GetHashCode());
    }

    /// <summary>Tests default safe handles report invalid without invoking native deletion.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SafeHandle_DefaultConstructors_AreInvalidAsync()
    {
        using var bitmap = new SafeHBitmapHandle();
        using var dib = new SafeDibSectionHandle();
        using var region = new SafeRegionHandle();
        using var compatibleDc = new SafeCompatibleDcHandle();
        using var graphicsDc = new SafeGraphicsDcHandle();
        using var selected = new SafeSelectObjectHandle();
        using var nonDisposable = new SafeNonDisposableObjectHandle();

        await Assert.That(bitmap.IsInvalid).IsTrue();
        await Assert.That(dib.IsInvalid).IsTrue();
        await Assert.That(region.IsInvalid).IsTrue();
        await Assert.That(compatibleDc.IsInvalid).IsTrue();
        await Assert.That(graphicsDc.IsInvalid).IsTrue();
        await Assert.That(selected.IsInvalid).IsTrue();
        await Assert.That(nonDisposable.IsInvalid).IsTrue();
    }

    /// <summary>Tests safe non-owning handles do not delete preexisting GDI objects.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SafeNonDisposableObjectHandle_Dispose_DoesNotDeletePreexistingObjectAsync()
    {
        var brush = Gdi32Api.CreateSolidBrush(0x00010203);
        try
        {
            await Assert.That(brush).IsNotEqualTo(IntPtr.Zero);

            using (var nonDisposable = new SafeNonDisposableObjectHandle(brush))
            {
                await Assert.That(nonDisposable.IsInvalid).IsFalse();
            }

            await Assert.That(Gdi32Api.DeleteObject(brush)).IsTrue();
            brush = IntPtr.Zero;
        }
        finally
        {
            if (brush != IntPtr.Zero)
            {
                _ = Gdi32Api.DeleteObject(brush);
            }
        }
    }

    /// <summary>Tests GDI region factories and region visibility extension behavior.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RegionFactoriesAndExtensions_CreateUsableRectangleRegionsAsync()
    {
        using var apiRegion = Gdi32Api.CreateRectRgn(0, 0, Ten, Ten);
        using var handleRegion = SafeRegionHandle.CreateRectRgn(0, 0, Ten, Ten);
        using var drawingRegion = new Region(new Rectangle(0, 0, Eleven, Eleven));

        await Assert.That(apiRegion.IsInvalid).IsFalse();
        await Assert.That(handleRegion.IsInvalid).IsFalse();
        await Assert.That(drawingRegion.AreRectangleCornersVisisble(new(0, 0, Ten, Ten))).IsTrue();
        await Assert.That(drawingRegion.AreRectangleCornersVisisble(new(0, 0, Twelve, Twelve))).IsFalse();
    }

    /// <summary>Tests bitmap handle creation and GetObject metadata retrieval.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SafeHBitmapHandle_AndGetObject_ReadBitmapMetadataAsync()
    {
        using var bitmap = CreateTestBitmap();
        using var handle = bitmap.SafeHBitmapHandle;
        var gdiBitmap = default(GdiBitmap);

        var copiedBytes = Gdi32Api.GetObject(handle, Marshal.SizeOf<GdiBitmap>(), ref gdiBitmap);

        await Assert.That(handle.IsInvalid).IsFalse();
        await Assert.That(copiedBytes).IsEqualTo(Marshal.SizeOf<GdiBitmap>());
        await Assert.That(gdiBitmap.Width).IsEqualTo(BitmapWidth);
        await Assert.That(gdiBitmap.Height).IsEqualTo(BitmapHeight);
        var sameBitmap = gdiBitmap;
        await Assert.That(gdiBitmap == sameBitmap).IsTrue();
        await Assert.That(gdiBitmap != default).IsTrue();
        await Assert.That(gdiBitmap.Equals((object)gdiBitmap)).IsTrue();
        await Assert.That(gdiBitmap.Equals(new object())).IsFalse();
        await Assert.That(gdiBitmap.GetHashCode()).IsEqualTo(typeof(GdiBitmap).GetHashCode());
    }

    /// <summary>Tests compatible DC selection restore and non-destructive blit wrappers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CompatibleDc_SelectObjectAndBlitWrappers_RunOnTinyInMemoryBitmapsAsync()
    {
        using var sourceBitmap = CreateTestBitmap();
        using var destinationBitmap = new Bitmap(BitmapWidth, BitmapHeight, PixelFormat.Format32bppArgb);
        using var destinationGraphics = Graphics.FromImage(destinationBitmap);
        using var destinationDc = destinationGraphics.GetSafeDeviceContext();
        using var compatibleDc = Gdi32Api.CreateCompatibleDC(destinationDc);
        using var sourceHandle = new SafeHBitmapHandle(sourceBitmap);
        await Assert.That(Gdi32Api.GetDeviceCaps(destinationDc, DeviceCaps.LOGPIXELSX) > 0).IsTrue();
        using (var previous = Gdi32Api.SelectObject(compatibleDc, sourceHandle))
        {
            await Assert.That(previous.IsInvalid).IsFalse();
            using var restored = Gdi32Api.SelectObject(compatibleDc, previous);
            await Assert.That(restored.IsInvalid).IsFalse();
        }

        using (compatibleDc.SelectObject(sourceHandle))
        {
            var destination = new Rectangle(0, 0, BitmapWidth, BitmapHeight);

            await Assert.That(compatibleDc.IsInvalid).IsFalse();
            await Assert.That(Gdi32Api.BitBlt(destinationDc, destination, compatibleDc, Point.Empty, RasterOperations.SourceCopy)).IsTrue();
            await Assert.That(Gdi32Api.StretchBlt(destinationDc, destination, compatibleDc, destination, RasterOperations.SourceCopy)).IsTrue();
            await Assert.That(Gdi32Api.GetPixel(compatibleDc, 0, 0)).IsNotEqualTo(0xFFFFFFFFU);
        }
    }

    /// <summary>Tests graphics blit extension methods on in-memory bitmaps.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task GdiExtensions_BitBltAndStretchBlt_CopyBitmapPixelsAsync()
    {
        using var sourceBitmap = CreateTestBitmap();
        using var destinationBitmap = new Bitmap(BitmapWidth, BitmapHeight, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(destinationBitmap);

        graphics.BitBlt(sourceBitmap, new(0, 0, BitmapWidth, BitmapHeight), new(0, 0), RasterOperations.SourceCopy);
        var bitBltColor = destinationBitmap.GetPixel(0, 0);

        graphics.Clear(Color.Transparent);
        graphics.StretchBlt(sourceBitmap, new(0, 0, BitmapWidth, BitmapHeight), new(0, 0, BitmapWidth, BitmapHeight), RasterOperations.SourceCopy);
        var stretchBltColor = destinationBitmap.GetPixel(0, 0);

        await Assert.That(bitBltColor.ToArgb()).IsEqualTo(Color.Red.ToArgb());
        await Assert.That(stretchBltColor.ToArgb()).IsEqualTo(Color.Red.ToArgb());
    }

    /// <summary>Tests DIB section creation and GetDIBits metadata query paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CreateDibSection_AndGetDIBits_CreateTinyBitmapAndQueryHeaderAsync()
    {
        using var desktopDc = SafeWindowDcHandle.FromDesktop();
        var header = BitmapV5Header.Create(BitmapWidth, -BitmapHeight, ThirtyTwoBitsPerPixel);
        using var dib = Gdi32Api.CreateDIBSection(desktopDc, ref header, DibColors.RgbColors, out var bits, IntPtr.Zero, 0);
        await Assert.That(dib.IsInvalid).IsFalse();
        await Assert.That(bits).IsNotEqualTo(IntPtr.Zero);

        using var bitmap = CreateTestBitmap();
        using var bitmapHandle = new SafeHBitmapHandle(bitmap);
        var infoHeader = BitmapInfoHeader.Create(BitmapWidth, BitmapHeight, ThirtyTwoBitsPerPixel);
        var scanLines = Gdi32Api.GetDIBits(desktopDc, bitmapHandle, 0, 0, IntPtr.Zero, ref infoHeader, DibColors.RgbColors);

        await Assert.That(scanLines >= 0).IsTrue();
        await Assert.That(infoHeader.Width).IsEqualTo(BitmapWidth);
        await Assert.That(Math.Abs(infoHeader.Height)).IsEqualTo(BitmapHeight);
    }

    /// <summary>Creates a tiny deterministic bitmap.</summary>
    /// <returns>A bitmap with known color values.</returns>
    private static Bitmap CreateTestBitmap()
    {
        var bitmap = new Bitmap(BitmapWidth, BitmapHeight, PixelFormat.Format32bppArgb);
        bitmap.SetPixel(0, 0, Color.Red);
        bitmap.SetPixel(1, 0, Color.Green);
        bitmap.SetPixel(Two, 0, Color.Blue);
        bitmap.SetPixel(0, 1, Color.White);
        bitmap.SetPixel(1, 1, Color.Black);
        bitmap.SetPixel(Two, 1, Color.Yellow);
        return bitmap;
    }
}
