// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Media.Imaging;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Icon Tests behavior.</summary>
public class IconTests
{
    /// <summary>Defines the TestValue32512 test value.</summary>
    private const int TestValue32512 = 32_512;

    /// <summary>Defines the TestValue32514 test value.</summary>
    private const int TestValue32514 = 32_514;

    /// <summary>Defines the TestValue4096 test value.</summary>
    private const int TestValue4096 = 4096;

    /// <summary>Defines the TestValue1024 test value.</summary>
    private const int TestValue1024 = 1024;

    /// <summary>Defines the TestValue2048 test value.</summary>
    private const int TestValue2048 = 2048;

    /// <summary>Defines the TestValue4000 test value.</summary>
    private const int TestValue4000 = 4000;

    /// <summary>Defines the TestValue512 test value.</summary>
    private const int TestValue512 = 512;

    /// <summary>Defines the TestValue200 test value.</summary>
    private const int TestValue200 = 200;

    /// <summary>Defines the TestValue128 test value.</summary>
    private const int TestValue128 = 128;

    /// <summary>Defines the TestValue255 test value.</summary>
    private const int TestValue255 = 255;

    /// <summary>Defines the TestValue100 test value.</summary>
    private const int TestValue100 = 100;

    /// <summary>Defines the TestValue256 test value.</summary>
    private const int TestValue256 = 256;

    /// <summary>Defines the TestValue50 test value.</summary>
    private const int TestValue50 = 50;

    /// <summary>Defines the TestValue36 test value.</summary>
    private const int TestValue36 = 36;

    /// <summary>Defines the TestValue20 test value.</summary>
    private const int TestValue20 = 20;

    /// <summary>Defines the TestValue26 test value.</summary>
    private const int TestValue26 = 26;

    /// <summary>Defines the TestValue10 test value.</summary>
    private const int TestValue10 = 10;

    /// <summary>Defines the TestValue82 test value.</summary>
    private const int TestValue82 = 82;

    /// <summary>Defines the TestValue64 test value.</summary>
    private const int TestValue64 = 64;

    /// <summary>Defines the TestValue16 test value.</summary>
    private const int TestValue16 = 16;

    /// <summary>Defines the TestValue32 test value.</summary>
    private const int TestValue32 = 32;

    /// <summary>Defines the TestValue48 test value.</summary>
    private const int TestValue48 = 48;

    /// <summary>Defines the TestValue5 test value.</summary>
    private const int TestValue5 = 5;

    /// <summary>Defines the TestValue8 test value.</summary>
    private const int TestValue8 = 8;

    /// <summary>Defines the TestValue3 test value.</summary>
    private const int TestValue3 = 3;

    /// <summary>Defines the TestValue2 test value.</summary>
    private const int TestValue2 = 2;

    /// <summary>Defines the TestValue6 test value.</summary>
    private const int TestValue6 = 6;

    /// <summary>Defines the TestValue4 test value.</summary>
    private const int TestValue4 = 4;

    /// <summary>The Common Controls module name.</summary>
    private const string CommonControlsModuleName = "comctl32.dll";

    /// <summary>Test getting an Icon for a top level window.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestIcon_GetIconAsync()
    {
        // Start a process to test against
        using var process = Process.Start("charmap.exe");

        // Make sure it's started
        await Assert.That(process).IsNotNull();

        // Wait until the process started it's message pump (listening for input)
        var processReady = process.WaitForInputIdle(TestValue4000);
        await Assert.That(processReady).IsTrue();
        if (!processReady)
        {
            return;
        }

        _ = User32Api.SetWindowText(process.MainWindowHandle, "TestIcon_GetIcon");

        var window = InteropWindowFactory.CreateFor(process.MainWindowHandle);
        var icon = window.GetIcon(default(BitmapSource));
        await Assert.That(icon).IsNotNull();

        // Kill the process
        process.Kill();
    }

    /// <summary>Test getting an Icon for the desktop, which doesn't have one.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestIcon_GetIcon_NullAsync()
    {
        var window = InteropWindowQueryExtensions.GetDesktopWindow();
        var icon = window.GetIcon(default(BitmapSource));

        await Assert.That(icon).IsNull();
    }

    /// <summary>Test getting system-preferred icon sizes.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestIcon_GetSystemIconSizesAsync()
    {
        // Test small icon metrics
        var smallWidth = IconHelper.GetSmallIconWidth();
        var smallHeight = IconHelper.GetSmallIconHeight();
        await Assert.That(smallWidth > 0).IsTrue();
        await Assert.That(smallHeight > 0).IsTrue();

        // Test standard/large icon metrics
        var standardWidth = IconHelper.GetStandardIconWidth();
        var standardHeight = IconHelper.GetStandardIconHeight();
        await Assert.That(standardWidth > 0).IsTrue();
        await Assert.That(standardHeight > 0).IsTrue();

        // Standard icons should typically be larger than or equal to small icons
        await Assert.That(standardWidth >= smallWidth).IsTrue();
        await Assert.That(standardHeight >= smallHeight).IsTrue();

        // Test icon spacing metrics
        var spacingWidth = IconHelper.GetIconSpacingWidth();
        var spacingHeight = IconHelper.GetIconSpacingHeight();
        await Assert.That(spacingWidth > 0).IsTrue();
        await Assert.That(spacingHeight > 0).IsTrue();

        // Icon spacing should be greater than or equal to standard icon size
        await Assert.That(spacingWidth >= standardWidth).IsTrue();
        await Assert.That(spacingHeight >= standardHeight).IsTrue();
    }

    /// <summary>Tests Icon V6 Loaded.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestIcon_CommonControlsLoadedAsync()
    {
        try
        {
            var idiApplication = new IntPtr(TestValue32512);
            _ = IconHelper.LoadIconWithSystemMetrics(default(BitmapSource), IntPtr.Zero, idiApplication, Icons.Enums.IconMetricSize.SmallIcon);
        }
        catch (EntryPointNotFoundException)
        {
            // Just to make sure it's loaded
        }

        var comctl32 = FindProcessModule(CommonControlsModuleName);

        await Assert.That(comctl32).IsNotNull();
        if (comctl32 is null)
        {
            return;
        }

        await Assert.That(comctl32.FileVersionInfo.FileMajorPart > 0).IsTrue();
    }

    /// <summary>Test getting system icon size using the helper method.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestIcon_GetSystemIconSizeAsync()
    {
        var smallSize = IconHelper.GetSystemIconSize(Icons.Enums.IconMetricSize.SmallIcon);
        await Assert.That(smallSize.Width > 0).IsTrue();
        await Assert.That(smallSize.Height > 0).IsTrue();

        var standardSize = IconHelper.GetSystemIconSize(Icons.Enums.IconMetricSize.StandardIcon);
        await Assert.That(standardSize.Width > 0).IsTrue();
        await Assert.That(standardSize.Height > 0).IsTrue();

        // Standard should be >= small
        await Assert.That(standardSize.Width >= smallSize.Width).IsTrue();
        await Assert.That(standardSize.Height >= smallSize.Height).IsTrue();
    }

    /// <summary>Test LoadIconMetric with system stock icons.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestIcon_LoadIconMetricAsync()
    {
        var idiApplication = new IntPtr(TestValue32512);

        try
        {
            // Load small icon using LoadIconMetric
            var smallIcon = IconHelper.LoadIconWithSystemMetrics(default(BitmapSource), IntPtr.Zero, idiApplication, Icons.Enums.IconMetricSize.SmallIcon);
            await Assert.That(smallIcon).IsNotNull();

            var expectedSmallSize = IconHelper.GetSystemIconSize(Icons.Enums.IconMetricSize.SmallIcon);
            await Assert.That(smallIcon.PixelWidth).IsEqualTo(expectedSmallSize.Width);
            await Assert.That(smallIcon.PixelHeight).IsEqualTo(expectedSmallSize.Height);

            // Load standard icon using LoadIconMetric
            var standardIcon = IconHelper.LoadIconWithSystemMetrics(default(BitmapSource), IntPtr.Zero, idiApplication, Icons.Enums.IconMetricSize.StandardIcon);
            await Assert.That(standardIcon).IsNotNull();

            var expectedStandardSize = IconHelper.GetSystemIconSize(Icons.Enums.IconMetricSize.StandardIcon);
            await Assert.That(standardIcon.PixelWidth).IsEqualTo(expectedStandardSize.Width);
            await Assert.That(standardIcon.PixelHeight).IsEqualTo(expectedStandardSize.Height);
        }
        catch (EntryPointNotFoundException)
        {
            // Skip test on systems without LoadIconMetric API
        }
    }

    /// <summary>Test LoadIconWithScaleDown with system stock icons.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestIcon_LoadIconWithScaleDownAsync()
    {
        // IDI_QUESTION is TestValue32514
        var idiQuestion = new IntPtr(TestValue32514);

        // Load icon at a specific size
        const int targetSize = 24;
        try
        {
            var icon = IconHelper.LoadIconWithScaleDown(default(BitmapSource), IntPtr.Zero, idiQuestion, targetSize, targetSize);
            await Assert.That(icon).IsNotNull();

            // The icon should be scaled to the requested size.
            await Assert.That(icon.PixelWidth).IsEqualTo(targetSize);
            await Assert.That(icon.PixelHeight).IsEqualTo(targetSize);
        }
        catch (EntryPointNotFoundException)
        {
            // Microsoft Testing Platform can host the module without a Common Controls v6
            // activation context. In that environment the documented API is unavailable.
            var loadedModule = FindProcessModule(CommonControlsModuleName);
            await Assert.That(loadedModule).IsNotNull();
            if (loadedModule is null)
            {
                return;
            }

            await Assert.That(loadedModule.ModuleName).IsEqualTo(CommonControlsModuleName);
        }
    }

    /// <summary>Test creating an icon file with multiple sizes using IconFileWriter.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestIconFileWriter_WriteIconFileAsync()
    {
        // Create test bitmaps of different sizes
        var images = new List<Bitmap> { new(TestValue16, TestValue16), new(TestValue32, TestValue32), new(TestValue48, TestValue48), new(TestValue256, TestValue256), };

        // Fill each bitmap with a different color for visual verification
        using (var g1 = Graphics.FromImage(images[0]))
        {
            g1.Clear(Color.Red);
        }

        using (var g2 = Graphics.FromImage(images[1]))
        {
            g2.Clear(Color.Green);
        }

        using (var g3 = Graphics.FromImage(images[TestValue2]))
        {
            g3.Clear(Color.Blue);
        }

        using (var g4 = Graphics.FromImage(images[TestValue3]))
        {
            g4.Clear(Color.Yellow);
        }

        // Write to a memory stream
#if NETFRAMEWORK
        using (var stream = new MemoryStream())
#else
        await using (var stream = new MemoryStream())
#endif
        {
            IconFileWriter.WriteIconFile(stream, images);

            // Verify the stream has data
            await Assert.That(stream.Length > 0).IsTrue();

            // Verify the header
            _ = stream.Seek(0, SeekOrigin.Begin);
            using var reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen: true);
            var reserved = reader.ReadUInt16();
            var type = reader.ReadUInt16();
            var count = reader.ReadUInt16();

            await Assert.That((int)reserved).IsEqualTo(0);
            await Assert.That((int)type).IsEqualTo(1); // 1 = icon
            await Assert.That((int)count).IsEqualTo(TestValue4); // TestValue4 images
        }

        // Clean up
        foreach (var image in images)
        {
            image.Dispose();
        }
    }

    /// <summary>Test creating a cursor file with hotspot information.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestIconFileWriter_WriteCursorFileAsync()
    {
        // Create test bitmap for cursor
        var bitmap = new Bitmap(TestValue32, TestValue32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.White);
        }

        var cursorData = new List<(Image, Point)>
        {
            (bitmap, new Point(TestValue16, TestValue16)), // Hotspot in the center
        };

        // Write to a memory stream
#if NETFRAMEWORK
        using (var stream = new MemoryStream())
#else
        await using (var stream = new MemoryStream())
#endif
        {
            IconFileWriter.WriteCursorFile(stream, cursorData);

            // Verify the stream has data
            await Assert.That(stream.Length > 0).IsTrue();

            // Verify the header
            _ = stream.Seek(0, SeekOrigin.Begin);
            using var reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen: true);
            var reserved = reader.ReadUInt16();
            var type = reader.ReadUInt16();
            var count = reader.ReadUInt16();

            await Assert.That((int)reserved).IsEqualTo(0);
            await Assert.That((int)type).IsEqualTo(TestValue2); // TestValue2 = cursor
            await Assert.That((int)count).IsEqualTo(1);
        }

        bitmap.Dispose();
    }

    /// <summary>Test IconDir structure creation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestIconDir_CreateIconAsync()
    {
        var iconDir = Icons.Structs.IconDir.CreateIcon(TestValue3);

        await Assert.That((int)iconDir.Reserved).IsEqualTo(0);
        await Assert.That((int)iconDir.Type).IsEqualTo(1);
        await Assert.That((int)iconDir.Count).IsEqualTo(TestValue3);
    }

    /// <summary>Test IconDir structure creation for cursor.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestIconDir_CreateCursorAsync()
    {
        var iconDir = Icons.Structs.IconDir.CreateCursor(TestValue2);

        await Assert.That((int)iconDir.Reserved).IsEqualTo(0);
        await Assert.That((int)iconDir.Type).IsEqualTo(TestValue2);
        await Assert.That((int)iconDir.Count).IsEqualTo(TestValue2);
    }

    /// <summary>Test IconDirEntry structure creation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestIconDirEntry_CreateForIconAsync()
    {
        var entry = Icons.Structs.IconDirEntry.CreateForIcon(TestValue32, TestValue32, TestValue32, TestValue1024, TestValue128);

        await Assert.That((int)entry.Width).IsEqualTo(TestValue32);
        await Assert.That((int)entry.Height).IsEqualTo(TestValue32);
        await Assert.That((int)entry.ColorCount).IsEqualTo(0);
        await Assert.That((int)entry.Reserved).IsEqualTo(0);
        await Assert.That((int)entry.Planes).IsEqualTo(0);
        await Assert.That((int)entry.BitCount).IsEqualTo(TestValue32);
        await Assert.That((int)entry.BytesInRes).IsEqualTo(TestValue1024);
        await Assert.That((int)entry.ImageOffset).IsEqualTo(TestValue128);
    }

    /// <summary>Test IconDirEntry structure creation with 256x256 size (should use 0).</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestIconDirEntry_Create256x256Async()
    {
        var entry = Icons.Structs.IconDirEntry.CreateForIcon(TestValue256, TestValue256, TestValue32, TestValue2048, TestValue256);

        await Assert.That((int)entry.Width).IsEqualTo(0); // 0 represents TestValue256
        await Assert.That((int)entry.Height).IsEqualTo(0); // 0 represents TestValue256
    }

    /// <summary>Test GrpIconDir structure creation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestGrpIconDir_CreateIconAsync()
    {
        var grpIconDir = Icons.Structs.GrpIconDir.CreateIcon(TestValue5);

        await Assert.That((int)grpIconDir.Reserved).IsEqualTo(0);
        await Assert.That((int)grpIconDir.Type).IsEqualTo(1);
        await Assert.That((int)grpIconDir.Count).IsEqualTo(TestValue5);
    }

    /// <summary>Test GrpIconDirEntry structure creation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestGrpIconDirEntry_CreateForIconAsync()
    {
        var entry = Icons.Structs.GrpIconDirEntry.CreateForIcon(TestValue48, TestValue48, TestValue32, TestValue4096, 1);

        await Assert.That((int)entry.Width).IsEqualTo(TestValue48);
        await Assert.That((int)entry.Height).IsEqualTo(TestValue48);
        await Assert.That((int)entry.ColorCount).IsEqualTo(0);
        await Assert.That((int)entry.Reserved).IsEqualTo(0);
        await Assert.That((int)entry.Planes).IsEqualTo(0);
        await Assert.That((int)entry.BitCount).IsEqualTo(TestValue32);
        await Assert.That((int)entry.BytesInRes).IsEqualTo(TestValue4096);
        await Assert.That((int)entry.Id).IsEqualTo(1);
    }

    /// <summary>Test writing GrpIconDir and GrpIconDirEntry structures.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestIconFileWriter_WriteGrpIconStructuresAsync()
    {
        // Test writing group icon structures for PE resources
#if NETFRAMEWORK
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
#else
        await using var stream = new MemoryStream();
        await using var writer = new BinaryWriter(stream);
#endif

        // Write group icon directory
        var grpIconDir = Icons.Structs.GrpIconDir.CreateIcon(TestValue2);
        IconFileWriter.WriteGrpIconDir(writer, grpIconDir);

        // Write group icon directory entries
        var entry1 = Icons.Structs.GrpIconDirEntry.CreateForIcon(TestValue16, TestValue16, TestValue32, TestValue512, 1);
        var entry2 = Icons.Structs.GrpIconDirEntry.CreateForIcon(TestValue32, TestValue32, TestValue32, TestValue2048, TestValue2);
        IconFileWriter.WriteGrpIconDirEntry(writer, entry1);
        IconFileWriter.WriteGrpIconDirEntry(writer, entry2);

        writer.Flush();

        // Verify the written data
        _ = stream.Seek(0, SeekOrigin.Begin);
        using var reader = new BinaryReader(stream);

        // Read GRPICONDIR
        var reserved = reader.ReadUInt16();
        var type = reader.ReadUInt16();
        var count = reader.ReadUInt16();
        await Assert.That((int)reserved).IsEqualTo(0);
        await Assert.That((int)type).IsEqualTo(1);
        await Assert.That((int)count).IsEqualTo(TestValue2);

        // Read first GRPICONDIRENTRY
        var width1 = reader.ReadByte();
        var height1 = reader.ReadByte();
        _ = reader.ReadByte(); // color count
        _ = reader.ReadByte(); // reserved
        _ = reader.ReadUInt16(); // planes
        _ = reader.ReadUInt16(); // bitcount
        _ = reader.ReadUInt32(); // bytes in res
        var id1 = reader.ReadUInt16();
        await Assert.That((int)width1).IsEqualTo(TestValue16);
        await Assert.That((int)height1).IsEqualTo(TestValue16);
        await Assert.That((int)id1).IsEqualTo(1);

        // Read second GRPICONDIRENTRY
        var width2 = reader.ReadByte();
        var height2 = reader.ReadByte();
        _ = reader.ReadByte(); // color count
        _ = reader.ReadByte(); // reserved
        _ = reader.ReadUInt16(); // planes
        _ = reader.ReadUInt16(); // bitcount
        _ = reader.ReadUInt32(); // bytes in res
        var id2 = reader.ReadUInt16();
        await Assert.That((int)width2).IsEqualTo(TestValue32);
        await Assert.That((int)height2).IsEqualTo(TestValue32);
        await Assert.That((int)id2).IsEqualTo(TestValue2);
    }

    /// <summary>Test backward compatibility - existing WriteIcon method.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestIconHelper_WriteIcon_BackwardCompatibilityAsync()
    {
        // Create test bitmap
        var bitmap = new Bitmap(TestValue32, TestValue32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.Blue);
        }

        var images = new List<Image> { bitmap };

        // Write using the old WriteIcon method (which now delegates to IconFileWriter)
#if NETFRAMEWORK
        using (var stream = new MemoryStream())
#else
        await using (var stream = new MemoryStream())
#endif
        {
            IconHelper.WriteIcon(stream, images);

            // Verify the stream has data
            await Assert.That(stream.Length > 0).IsTrue();

            // Verify the header
            _ = stream.Seek(0, SeekOrigin.Begin);
            using var reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen: true);
            var reserved = reader.ReadUInt16();
            var type = reader.ReadUInt16();
            var count = reader.ReadUInt16();

            await Assert.That((int)reserved).IsEqualTo(0);
            await Assert.That((int)type).IsEqualTo(1); // 1 = icon
            await Assert.That((int)count).IsEqualTo(1);
        }

        bitmap.Dispose();
    }

    /// <summary>Test TryGetCurrentCursor method.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestCursorHelper_TryGetCurrentCursorAsync()
    {
        var result = CursorHelper.TryGetCurrentCursor(out var capturedCursor);

        // In some environments (CI, headless), cursor may not be available
        // So we just verify the method returns a boolean and doesn't crash
        if (result)
        {
            await Assert.That(capturedCursor).IsNotNull();
            capturedCursor.Dispose();
        }
    }

    /// <summary>Test DrawCursorOnBitmap with a modern alpha cursor.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestCursorHelper_DrawCursorOnBitmap_ModernCursorAsync()
    {
        // Create a target bitmap
        var targetBitmap = new Bitmap(TestValue100, TestValue100, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(targetBitmap))
        {
            g.Clear(Color.White);
        }

        // Create a cursor with alpha channel (modern cursor, no mask)
        var cursorBitmap = new Bitmap(TestValue32, TestValue32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(cursorBitmap))
        {
            g.Clear(Color.Transparent);
            using var brush = new SolidBrush(Color.FromArgb(TestValue128, TestValue255, 0, 0));
            g.FillEllipse(brush, 0, 0, TestValue32, TestValue32);
        }

        var cursor = new CapturedCursor
        {
            ColorLayer = cursorBitmap,
            MaskLayer = null, // Modern cursor
            HotSpot = new(TestValue16, TestValue16),
            Size = new(TestValue32, TestValue32),
        };

        // Draw cursor at position (TestValue10, TestValue10)
        CursorHelper.DrawCursorOnBitmap(targetBitmap, cursor, new(TestValue10, TestValue10));

        // Verify that the cursor was drawn
        // The center of the cursor should have some red color
        var centerColor = targetBitmap.GetPixel(TestValue26, TestValue26); // TestValue10 + TestValue16 (center of cursor)
        await Assert.That(centerColor.R > TestValue100).IsTrue();

        // Corners should still be white (cursor is transparent there)
        var cornerColor = targetBitmap.GetPixel(0, 0);
        await Assert.That((int)cornerColor.R).IsEqualTo(TestValue255);
        await Assert.That((int)cornerColor.G).IsEqualTo(TestValue255);
        await Assert.That((int)cornerColor.B).IsEqualTo(TestValue255);

        cursor.Dispose();
        targetBitmap.Dispose();
    }

    /// <summary>Test DrawCursorOnBitmap with a legacy cursor with mask.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestCursorHelper_DrawCursorOnBitmap_LegacyCursorAsync()
    {
        // Create a target bitmap
        var targetBitmap = new Bitmap(TestValue100, TestValue100, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(targetBitmap))
        {
            g.Clear(Color.White);
        }

        // Create a cursor color layer
        var cursorBitmap = new Bitmap(TestValue32, TestValue32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(cursorBitmap))
        {
            g.Clear(Color.Black);
            using var brush = new SolidBrush(Color.White);
            g.FillRectangle(brush, TestValue8, TestValue8, TestValue16, TestValue16);
        }

        // Create a mask layer
        var maskBitmap = new Bitmap(TestValue32, TestValue32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(maskBitmap))
        {
            g.Clear(Color.Black); // Black = transparent area
            using var brush = new SolidBrush(Color.White);
            g.FillRectangle(brush, TestValue8, TestValue8, TestValue16, TestValue16); // White = opaque area for XOR
        }

        var cursor = new CapturedCursor
        {
            ColorLayer = cursorBitmap,
            MaskLayer = maskBitmap, // Legacy cursor with mask
            HotSpot = new(TestValue16, TestValue16),
            Size = new(TestValue32, TestValue32),
        };

        // Draw cursor at position (TestValue20, TestValue20)
        CursorHelper.DrawCursorOnBitmap(targetBitmap, cursor, new(TestValue20, TestValue20));

        // Verify that the cursor was drawn
        // The center of the cursor (where the white square is) should be modified
        var centerColor = targetBitmap.GetPixel(TestValue36, TestValue36); // TestValue20 + TestValue16 (center of white square)

        // With white mask and white color on white background: (TestValue255 & TestValue255) ^ TestValue255 = 0 (black - inverted!)
        await Assert.That(centerColor.R < TestValue255 || centerColor.G < TestValue255 || centerColor.B < TestValue255).IsTrue();

        cursor.Dispose();
        targetBitmap.Dispose();
    }

    /// <summary>Test DrawCursorOnBitmap with scaling.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestCursorHelper_DrawCursorOnBitmap_WithScalingAsync()
    {
        // Create a target bitmap
        var targetBitmap = new Bitmap(TestValue200, TestValue200, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(targetBitmap))
        {
            g.Clear(Color.White);
        }

        // Create a small cursor
        var cursorBitmap = new Bitmap(TestValue16, TestValue16, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(cursorBitmap))
        {
            g.Clear(Color.Transparent);
            using var brush = new SolidBrush(Color.FromArgb(TestValue255, 0, 0, TestValue255));
            g.FillRectangle(brush, 0, 0, TestValue16, TestValue16);
        }

        var cursor = new CapturedCursor { ColorLayer = cursorBitmap, MaskLayer = null, HotSpot = new(TestValue8, TestValue8), Size = new(TestValue16, TestValue16), };

        // Draw cursor at position (TestValue50, TestValue50) scaled to 64x64
        CursorHelper.DrawCursorOnBitmap(targetBitmap, cursor, new(TestValue50, TestValue50), new(TestValue64, TestValue64));

        // Verify that the cursor was drawn and scaled
        var centerColor = targetBitmap.GetPixel(TestValue82, TestValue82); // TestValue50 + TestValue32 (center of scaled cursor)
        await Assert.That(centerColor.B > TestValue200).IsTrue();

        cursor.Dispose();
        targetBitmap.Dispose();
    }

    /// <summary>Test DrawCursorOnBitmap on 24-bit RGB bitmap.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestCursorHelper_DrawCursorOnBitmap_24bppTargetAsync()
    {
        // Create a 24-bit target bitmap
        var targetBitmap = new Bitmap(TestValue100, TestValue100, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        using (var g = Graphics.FromImage(targetBitmap))
        {
            g.Clear(Color.White);
        }

        // Create a cursor with alpha channel
        var cursorBitmap = new Bitmap(TestValue32, TestValue32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(cursorBitmap))
        {
            g.Clear(Color.Transparent);
            using var brush = new SolidBrush(Color.FromArgb(TestValue255, 0, TestValue255, 0));
            g.FillEllipse(brush, 0, 0, TestValue32, TestValue32);
        }

        var cursor = new CapturedCursor { ColorLayer = cursorBitmap, MaskLayer = null, HotSpot = new(TestValue16, TestValue16), Size = new(TestValue32, TestValue32), };

        // Draw cursor at position (TestValue10, TestValue10)
        CursorHelper.DrawCursorOnBitmap(targetBitmap, cursor, new(TestValue10, TestValue10));

        // Verify that the cursor was drawn
        var centerColor = targetBitmap.GetPixel(TestValue26, TestValue26);
        await Assert.That(centerColor.G > TestValue200).IsTrue();

        cursor.Dispose();
        targetBitmap.Dispose();
    }

    /// <summary>Finds a loaded module by name.</summary>
    /// <param name="moduleName">The module name.</param>
    /// <returns>The matching process module, or <see langword="null"/> when no module is loaded.</returns>
    private static ProcessModule FindProcessModule(string moduleName)
    {
        foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
        {
            if (module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
            {
                return module;
            }
        }

        return null;
    }
}
