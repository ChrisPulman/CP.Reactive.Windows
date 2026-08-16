// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing.Imaging;
using CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Additional icon and cursor coverage focused on pure and safe interop paths.</summary>
public class IconCoverageAdditionalTests
{
    /// <summary>Validates ICO and CUR directory structure metadata and packed sizes.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IconDirectoryStructs_ExposePackedSizesAndCursorFieldsAsync()
    {
        await Assert.That(IconDir.Size).IsEqualTo(Six);
        await Assert.That(IconDirEntry.Size).IsEqualTo(Sixteen);
        await Assert.That(GrpIconDir.Size).IsEqualTo(Six);
        await Assert.That(GrpIconDirEntry.Size).IsEqualTo(Fourteen);
        await Assert.That(Marshal.SizeOf<IconDir>()).IsEqualTo(IconDir.Size);
        await Assert.That(Marshal.SizeOf<IconDirEntry>()).IsEqualTo(IconDirEntry.Size);
        await Assert.That(Marshal.SizeOf<GrpIconDir>()).IsEqualTo(GrpIconDir.Size);
        await Assert.That(Marshal.SizeOf<GrpIconDirEntry>()).IsEqualTo(GrpIconDirEntry.Size);

        var icon = IconDir.CreateIcon(Three);
        var cursor = IconDir.CreateCursor(Four);
        var groupCursor = GrpIconDir.CreateCursor(Five);
        var cursorEntry = IconDirEntry.CreateForCursor(TwoHundredFiftySix, Sixteen, Seven, Nine, OneThousandTwoHundredThirtyFour, FortyTwo);
        var groupCursorEntry = GrpIconDirEntry.CreateForCursor(ThirtyTwo, TwoHundredFiftySix, Eleven, Thirteen, FourThousandThreeHundredTwentyOne, Seventeen);

        await Assert.That((int)icon.Type).IsEqualTo(1);
        await Assert.That((int)icon.Count).IsEqualTo(Three);
        await Assert.That((int)cursor.Type).IsEqualTo(Two);
        await Assert.That((int)cursor.Count).IsEqualTo(Four);
        await Assert.That((int)groupCursor.Type).IsEqualTo(Two);
        await Assert.That((int)groupCursor.Count).IsEqualTo(Five);
        await Assert.That((int)cursorEntry.Width).IsEqualTo(0);
        await Assert.That((int)cursorEntry.Height).IsEqualTo(Sixteen);
        await Assert.That((int)cursorEntry.Planes).IsEqualTo(Seven);
        await Assert.That((int)cursorEntry.BitCount).IsEqualTo(Nine);
        await Assert.That((int)cursorEntry.BytesInRes).IsEqualTo(OneThousandTwoHundredThirtyFour);
        await Assert.That((int)cursorEntry.ImageOffset).IsEqualTo(FortyTwo);
        await Assert.That((int)groupCursorEntry.Width).IsEqualTo(ThirtyTwo);
        await Assert.That((int)groupCursorEntry.Height).IsEqualTo(0);
        await Assert.That((int)groupCursorEntry.Planes).IsEqualTo(Eleven);
        await Assert.That((int)groupCursorEntry.BitCount).IsEqualTo(Thirteen);
        await Assert.That((int)groupCursorEntry.BytesInRes).IsEqualTo(FourThousandThreeHundredTwentyOne);
        await Assert.That((int)groupCursorEntry.Id).IsEqualTo(Seventeen);
    }

    /// <summary>Validates equality and hash behavior for icon info structures.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IconInfoStructs_RespectEqualityAndHashInputsAsync()
    {
        var left = new IconInfo { IsIcon = true, Hotspot = new(Three, Four) };
        var right = new IconInfo { IsIcon = true, Hotspot = new(Three, Four) };
        var different = new IconInfo { IsIcon = false, Hotspot = new(Three, Four) };

        await Assert.That(left == right).IsTrue();
        await Assert.That(left != different).IsTrue();
        await Assert.That(left.Equals((object)right)).IsTrue();
        await Assert.That(left.Equals("icon")).IsFalse();
        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
        await Assert.That(left.GetHashCode()).IsNotEqualTo(different.GetHashCode());
        await Assert.That(left.BitmaskBitmapHandle.IsInvalid).IsTrue();
        await Assert.That(left.ColorBitmapHandle.IsInvalid).IsTrue();

        var extendedLeft = IconInfoEx.Create();
        extendedLeft.IsIcon = true;
        extendedLeft.Hotspot = new(Five, Six);
        var extendedRight = IconInfoEx.Create();
        extendedRight.IsIcon = true;
        extendedRight.Hotspot = new(Five, Six);
        var extendedDifferent = IconInfoEx.Create();
        extendedDifferent.IsIcon = true;
        extendedDifferent.Hotspot = new(Six, Five);

        await Assert.That(extendedLeft.StructureSize).IsEqualTo((uint)Marshal.SizeOf<IconInfoEx>());
        await Assert.That(extendedLeft.ModuleName).IsEqualTo(string.Empty);
        await Assert.That(extendedLeft.ResourceName).IsEqualTo(string.Empty);
        await Assert.That(extendedLeft == extendedRight).IsTrue();
        await Assert.That(extendedLeft != extendedDifferent).IsTrue();
        await Assert.That(extendedLeft.Equals((object)extendedRight)).IsTrue();
        await Assert.That(extendedLeft.Equals(new object())).IsFalse();
        await Assert.That(extendedLeft.GetHashCode()).IsEqualTo(extendedRight.GetHashCode());
        await Assert.That(extendedLeft.GetHashCode()).IsNotEqualTo(extendedDifferent.GetHashCode());

        extendedLeft.Dispose();
        await Assert.That(extendedLeft.ResourceId).IsEqualTo((ushort)0);
        await Assert.That(extendedLeft.ModuleName).IsEqualTo(string.Empty);
        await Assert.That(extendedLeft.ResourceName).IsEqualTo(string.Empty);
    }

    /// <summary>Validates IconHelper null, invalid and real bitmap handle conversions.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IconHelper_HandlesInvalidAndOwnedIconHandlesSafelyAsync()
    {
        await Assert.That(IconHelper.IconHandleTo(IntPtr.Zero, default(Icon))).IsNull();
        await Assert.That(IconHelper.IconHandleTo(new SafeIconHandle(IntPtr.Zero), default(Icon))).IsNull();
        await Assert.That(IconHelper.IconHandleTo(default(SafeIconHandle), default(Bitmap))).IsNull();
        await Assert.That(IconHelper.IconHandleTo(new SafeIconHandle(new IntPtr(-1)), default(Bitmap))).IsNull();

        using var bitmap = CreateSolidBitmap(Sixteen, Sixteen, Color.Magenta);
        using var safeIconHandle = bitmap.SafeIconHandle;
        using var copiedIconHandle = NativeIconMethods.CopyIcon(safeIconHandle);
        await Assert.That(copiedIconHandle.IsInvalid).IsFalse();

        using var icon = IconHelper.IconHandleTo(copiedIconHandle, default(Icon));
        using var convertedBitmap = IconHelper.IconHandleTo(copiedIconHandle, default(Bitmap));
        var unsupported = IconHelper.IconHandleTo(copiedIconHandle, string.Empty);

        await Assert.That(icon).IsNotNull();
        await Assert.That(convertedBitmap).IsNotNull();
        await Assert.That(convertedBitmap.Width).IsEqualTo(Sixteen);
        await Assert.That(convertedBitmap.Height).IsEqualTo(Sixteen);
        await Assert.That(unsupported).IsNull();
    }

    /// <summary>Validates native icon methods return safe results for invalid handles and stock icon resources.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeIconMethods_InvalidAndStockResourcePathsAreSafeAsync()
    {
        using var nullCopy = NativeIconMethods.CopyIcon(IntPtr.Zero);
        await Assert.That(nullCopy.IsInvalid).IsTrue();

        using var bitmap = CreateSolidBitmap(Sixteen, Sixteen, Color.Cyan);
        using var iconHandle = bitmap.SafeIconHandle;
        var gotInfo = NativeIconMethods.GetIconInfo(iconHandle, out var iconInfo);
        await Assert.That(gotInfo).IsTrue();
        await Assert.That(iconInfo.IsIcon).IsTrue();
        iconInfo.BitmaskBitmapHandle.Dispose();
        iconInfo.ColorBitmapHandle.Dispose();

        const int idiApplication = 32_512;
        try
        {
            var metricResult = NativeIconMethods.LoadIconMetric(IntPtr.Zero, new IntPtr(idiApplication), IconMetricSize.SmallIcon, out var metricHandle);
            using var metricSafeHandle = new SafeIconHandle(metricHandle);
            await Assert.That(metricResult == 0 || metricSafeHandle.IsInvalid).IsTrue();

            var namedMetricResult = NativeIconMethods.LoadIconMetric(IntPtr.Zero, "missing-icon-resource", IconMetricSize.SmallIcon, out var namedMetricHandle);
            using var namedMetricSafeHandle = new SafeIconHandle(namedMetricHandle);
            await Assert.That(namedMetricResult != 0 || namedMetricSafeHandle.IsInvalid).IsTrue();

            var scaleResult = NativeIconMethods.LoadIconWithScaleDown(IntPtr.Zero, new IntPtr(idiApplication), Sixteen, Sixteen, out var scaledHandle);
            using var scaledSafeHandle = new SafeIconHandle(scaledHandle);
            await Assert.That(scaleResult == 0 || scaledSafeHandle.IsInvalid).IsTrue();

            var namedScaleResult = NativeIconMethods.LoadIconWithScaleDown(IntPtr.Zero, "missing-icon-resource", Sixteen, Sixteen, out var namedScaledHandle);
            using var namedScaledSafeHandle = new SafeIconHandle(namedScaledHandle);
            await Assert.That(namedScaleResult != 0 || namedScaledSafeHandle.IsInvalid).IsTrue();
        }
        catch (EntryPointNotFoundException exception)
        {
            await Assert.That(exception.Message).IsNotEmpty();
        }
    }

    /// <summary>Validates extraction of 256x256 PNG icon entries and invalid stream handling.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IconStreamExtensions_ExtractVistaIconReadsOnlyEncoded256EntryAsync()
    {
        using var small = CreateSolidBitmap(Sixteen, Sixteen, Color.Red);
        using var vista = CreateSolidBitmap(TwoHundredFiftySix, TwoHundredFiftySix, Color.Blue);
#if NETFRAMEWORK
        using var stream = new MemoryStream();
#else
        await using var stream = new MemoryStream();
#endif

        IconFileWriter.WriteIconFile(stream, [small, vista]);
        _ = stream.Seek(0, SeekOrigin.Begin);
        using var extracted = stream.ExtractVistaIcon();

        await Assert.That(extracted).IsNotNull();
        await Assert.That(extracted.Width).IsEqualTo(TwoHundredFiftySix);
        await Assert.That(extracted.Height).IsEqualTo(TwoHundredFiftySix);

#if NETFRAMEWORK
        using var noVistaStream = new MemoryStream();
#else
        await using var noVistaStream = new MemoryStream();
#endif
        IconFileWriter.WriteIconFile(noVistaStream, [small]);
        _ = noVistaStream.Seek(0, SeekOrigin.Begin);
        using var missing = noVistaStream.ExtractVistaIcon();
        await Assert.That(missing).IsNull();

#if NETFRAMEWORK
        using var invalidImageStream = CreateInvalidVistaIconStream();
#else
        await using var invalidImageStream = CreateInvalidVistaIconStream();
#endif
        using var invalid = invalidImageStream.ExtractVistaIcon();
        await Assert.That(invalid).IsNull();
    }

    /// <summary>Validates icon writer error paths and file overloads.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IconFileWriter_RejectsInvalidInputsAndWritesFileOverloadsAsync()
    {
        await Assert.That(static () => IconFileWriter.WriteIconFile(default(Stream), [])).Throws<ArgumentNullException>();
        await Assert.That(static () => IconFileWriter.WriteIconFile(Stream.Null, default)).Throws<ArgumentNullException>();
        await Assert.That(static () => IconFileWriter.WriteIconFile(Stream.Null, [])).Throws<ArgumentException>();
        await Assert.That(static () => IconFileWriter.WriteCursorFile(default(Stream), [])).Throws<ArgumentNullException>();
        await Assert.That(static () => IconFileWriter.WriteCursorFile(Stream.Null, default)).Throws<ArgumentNullException>();
        await Assert.That(static () => IconFileWriter.WriteCursorFile(Stream.Null, [])).Throws<ArgumentException>();
        await Assert.That(static () => IconFileWriter.WriteIconFile(default(string), [])).Throws<ArgumentNullException>();
        await Assert.That(static () => IconFileWriter.WriteCursorFile(default(string), [])).Throws<ArgumentNullException>();

        var tempDirectory = CreateTemporaryDirectory("cp-reactive-icon-tests-");
        try
        {
            using var iconImage = CreateSolidBitmap(TwentyFour, TwentyFour, Color.Green);
            var iconPath = Path.Combine(tempDirectory.FullName, "sample.ico");
            var cursorPath = Path.Combine(tempDirectory.FullName, "sample.cur");

            IconFileWriter.WriteIconFile(iconPath, [iconImage]);
            IconFileWriter.WriteCursorFile(cursorPath, [(Image: (Image)iconImage, Hotspot: new Point(Two, Three))]);

            await Assert.That(File.Exists(iconPath)).IsTrue();
            await Assert.That(new FileInfo(iconPath).Length > 0).IsTrue();
            await Assert.That(File.Exists(cursorPath)).IsTrue();
            await Assert.That(new FileInfo(cursorPath).Length > 0).IsTrue();
        }
        finally
        {
            Directory.Delete(tempDirectory.FullName, recursive: true);
        }
    }

    /// <summary>Validates pure CursorHelper classification, bitmap creation, and invalid bitmap handles.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CursorHelper_PureAndInvalidImagePathsAreSafeAsync()
    {
        await Assert.That(CursorHelper.GetCursorBaseSize() > 0).IsTrue();
        await Assert.That(CursorHelper.IsSystemCursor(default)).IsFalse();
        await Assert.That(CursorHelper.IsSystemCursor(string.Empty)).IsFalse();
        await Assert.That(CursorHelper.IsSystemCursor("user32.dll")).IsTrue();
        await Assert.That(CursorHelper.IsSystemCursor(@"C:\Windows\Cursors\aero_arrow.cur")).IsTrue();
        await Assert.That(CursorHelper.IsSystemCursor(@"C:\Windows\System32\main.cpl")).IsTrue();
        await Assert.That(CursorHelper.IsSystemCursor("custom.cur")).IsFalse();

        using var invalidBitmap = CursorHelper.BitmapFromHIcon(IntPtr.Zero, Eight, Eight, DrawIconExFlags.DI_MASK);
        await Assert.That(invalidBitmap.PixelFormat).IsEqualTo(PixelFormat.Format24bppRgb);
        await Assert.That(invalidBitmap.GetPixel(0, 0).ToArgb()).IsEqualTo(Color.White.ToArgb());

        using var invalidImageBitmap = CursorHelper.BitmapFromHIcon(IntPtr.Zero, Eight, Eight, DrawIconExFlags.DI_IMAGE);
        await Assert.That(invalidImageBitmap.PixelFormat).IsEqualTo(PixelFormat.Format24bppRgb);
        await Assert.That(invalidImageBitmap.GetPixel(0, 0).ToArgb()).IsEqualTo(Color.Black.ToArgb());

        using var invalidNormalBitmap = CursorHelper.BitmapFromHIcon(IntPtr.Zero, Eight, Eight);
        await Assert.That(invalidNormalBitmap.PixelFormat).IsEqualTo(PixelFormat.Format32bppArgb);

        using var invalidHandle = new SafeHBitmapHandle(IntPtr.Zero);
        using var extracted = CursorHelper.ExtractRawColorBitmap(invalidHandle, Sixteen, Sixteen, out var hasAlpha);
        await Assert.That(extracted).IsNull();
        await Assert.That(hasAlpha).IsFalse();
    }

    /// <summary>Validates cursor draw methods handle null, clipping, conversion and unsupported target paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CursorHelper_DrawCursorOnBitmapHandlesBoundariesAndFormatsAsync()
    {
        using var target = CreateSolidBitmap(Twelve, Twelve, Color.White);

        CursorHelper.DrawCursorOnBitmap(target, default, new(1, 1));
        CursorHelper.DrawCursorOnBitmap(target, new(), new(1, 1));
        await Assert.That(target.GetPixel(1, 1).ToArgb()).IsEqualTo(Color.White.ToArgb());

        using var colorLayer = CreateSolidBitmap(Four, Four, Color.FromArgb(OneHundredTwentyEight, TwoHundredFiftyFive, 0, 0));
        using var cursor = new CapturedCursor { ColorLayer = colorLayer, HotSpot = new(0, 0), Size = new(Four, Four), };

        CursorHelper.DrawCursorOnBitmap(target, cursor, new(-Two, -Two));
        await Assert.That(target.GetPixel(0, 0).R > OneHundredTwentyFive).IsTrue();

        using var indexedTarget = new Bitmap(Eight, Eight, PixelFormat.Format8bppIndexed);
        await Assert.That(() => CursorHelper.DrawCursorOnBitmap(indexedTarget, cursor, new(0, 0))).Throws<NotSupportedException>();

        using var graphicsTarget = CreateSolidBitmap(Twelve, Twelve, Color.White);
        using var graphics = Graphics.FromImage(graphicsTarget);
        CursorHelper.DrawCursorOnGraphics(graphics, default, new(1, 1));
        CursorHelper.DrawCursorOnGraphics(graphics, new(), new(1, 1));
        CursorHelper.DrawCursorOnGraphics(graphics, cursor, new(Two, Two), new(Six, Six));

        await Assert.That(graphicsTarget.GetPixel(Four, Four).R > OneHundredTwentyFive).IsTrue();
    }

    /// <summary>Validates captured cursor cloning and disposal behavior.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CapturedCursor_CloneCopiesImagesAndKeepsValuePropertiesAsync()
    {
        using var source = new CapturedCursor
        {
            ColorLayer = CreateSolidBitmap(Four, Four, Color.Red),
            MaskLayer = CreateSolidBitmap(Four, Four, Color.White),
            HotSpot = new(1, Two),
            Size = new(Four, Four),
        };

        using var clone = source.Clone();
        source.ColorLayer.SetPixel(0, 0, Color.Blue);

        await Assert.That(ReferenceEquals(source.ColorLayer, clone.ColorLayer)).IsFalse();
        await Assert.That(ReferenceEquals(source.MaskLayer, clone.MaskLayer)).IsFalse();
        await Assert.That(clone.HotSpot).IsEqualTo(new(1, Two));
        await Assert.That(clone.Size).IsEqualTo(new(Four, Four));
        await Assert.That(clone.ColorLayer.GetPixel(0, 0).ToArgb()).IsEqualTo(Color.Red.ToArgb());
    }

    /// <summary>Creates a solid bitmap for cursor clone tests.</summary>
    /// <param name="width">The bitmap width.</param>
    /// <param name="height">The bitmap height.</param>
    /// <param name="color">The fill color.</param>
    /// <returns>The created bitmap.</returns>
    private static Bitmap CreateSolidBitmap(int width, int height, Color color)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(color);
        return bitmap;
    }

    /// <summary>Creates a unique temporary directory.</summary>
    /// <param name="prefix">The directory name prefix.</param>
    /// <returns>The created directory.</returns>
    private static DirectoryInfo CreateTemporaryDirectory(string prefix) =>
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("N")));

    /// <summary>Creates a deliberately invalid Vista icon stream.</summary>
    /// <returns>The invalid icon stream.</returns>
    private static MemoryStream CreateInvalidVistaIconStream()
    {
        var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, System.Text.Encoding.Default, leaveOpen: true);
        writer.Write((ushort)0);
        writer.Write((ushort)1);
        writer.Write((ushort)1);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((ushort)1);
        writer.Write(ThirtyTwo);
        writer.Write(Four);
        writer.Write(TwentyTwo);
        writer.Write([1, Two, Three, Four]);
        _ = stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }
}
