// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing.Imaging;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Final deterministic coverage for cursor helper and native cursor seams.</summary>
public sealed class CoverageFinalCursorTests
{
    /// <summary>The icon name buffer length.</summary>
    private const int IconNameBufferLength = 260;

    /// <summary>The custom cursor file name.</summary>
    private const string CustomCursorFileName = "custom.cur";

    /// <summary>The native 32-bit integer byte count.</summary>
    private const int NativeInt32ByteCount = 4;

    /// <summary>The native 16-bit unsigned integer byte count.</summary>
    private const int NativeUInt16ByteCount = 2;

    /// <summary>The system cursor module name.</summary>
    private const string SystemCursorModuleName = "user32.dll";

    /// <summary>The test cursor no-resource identifier.</summary>
    private const ushort NoCursorResourceId = 0;

    /// <summary>The test cursor resource identifier.</summary>
    private const ushort TestCursorResourceId = 1;

    /// <summary>The private to native pointer method name.</summary>
    private const string ToIntPtrMethodName = "ToIntPtr";

    /// <summary>Validates replaceable cursor base size branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CursorBaseSizeProvider_CoversConfiguredFallbackAndExceptionBranchesAsync()
    {
        var previousProvider = Icons.CursorHelper.SetCursorBaseSizeProviderForTesting(static () => Forty);
        try
        {
            await Assert.That(Icons.CursorHelper.GetCursorBaseSize()).IsEqualTo(Forty);

            _ = Icons.CursorHelper.SetCursorBaseSizeProviderForTesting(static () => null);
            await Assert.That(Icons.CursorHelper.GetCursorBaseSize()).IsEqualTo(ThirtyTwo);

            _ = Icons.CursorHelper.SetCursorBaseSizeProviderForTesting(static () => throw new Win32Exception());
            await Assert.That(Icons.CursorHelper.GetCursorBaseSize()).IsEqualTo(ThirtyTwo);

            await Assert
                .That(static () => Icons.CursorHelper.SetCursorBaseSizeProviderForTesting(null))
                .Throws<ArgumentNullException>();
        }
        finally
        {
            _ = Icons.CursorHelper.SetCursorBaseSizeProviderForTesting(previousProvider);
        }
    }

    /// <summary>Validates native cursor method forwarding through the injected API.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeCursorMethods_ForwardEveryWrapperToInjectedApiAsync()
    {
        var fakeApi = new FakeNativeCursorApi();
        var previousApi = Icons.NativeCursorMethods.SetApiForTesting(fakeApi);
        try
        {
            await Assert.That(static () => Icons.NativeCursorMethods.SetApiForTesting(null)).Throws<ArgumentNullException>();

            var copied = Icons.NativeCursorMethods.CopyImage(
                new(Ten),
                ImageType.IMAGE_CURSOR,
                Sixteen,
                TwentyFour,
                CopyImageFlags.LR_COPYRETURNORG);
            var loadedById = Icons.NativeCursorMethods.LoadImage(
                new(Eleven),
                new IntPtr(Twelve),
                ImageType.IMAGE_ICON,
                ThirtyTwo,
                Forty,
                LoadImageFlags.LR_SHARED);
            var loadedByName = Icons.NativeCursorMethods.LoadImage(
                new(Thirteen),
                "sample.cur",
                ImageType.IMAGE_CURSOR,
                Forty,
                ThirtyTwo,
                LoadImageFlags.LR_LOADFROMFILE);

            await Assert.That(copied).IsEqualTo(new(OneThousandTwoHundredThirtyFour));
            await Assert.That(loadedById).IsEqualTo(new(FourThousandThreeHundredTwentyOne));
            await Assert.That(loadedByName).IsEqualTo(new(TwentySixThousandOneHundred));
            await Assert.That(fakeApi.CopyImageCalls).IsEqualTo(One);
            await Assert.That(fakeApi.LoadImageByIdCalls).IsEqualTo(One);
            await Assert.That(fakeApi.LoadImageByNameCalls).IsEqualTo(One);
            await Assert.That(fakeApi.LastCopySize).IsEqualTo(new(Sixteen, TwentyFour));
            await Assert.That(fakeApi.LastLoadName).IsEqualTo("sample.cur");
        }
        finally
        {
            _ = Icons.NativeCursorMethods.SetApiForTesting(previousApi);
        }
    }

    /// <summary>Validates production native cursor API wrappers on invalid input.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsNativeCursorApi_InvalidInputsReturnZeroWithoutCursorStateAsync()
    {
        var api = WindowsNativeCursorApi.Instance;

        var copied = api.CopyImage(IntPtr.Zero, ImageType.IMAGE_CURSOR, Zero, Zero, CopyImageFlags.None);
        var loadedById = api.LoadImage(IntPtr.Zero, IntPtr.Zero, ImageType.IMAGE_CURSOR, Zero, Zero, LoadImageFlags.None);
        var loadedByName = api.LoadImage(
            IntPtr.Zero,
            @"Z:\missing\cursor.cur",
            ImageType.IMAGE_CURSOR,
            Zero,
            Zero,
            LoadImageFlags.LR_LOADFROMFILE);

        await Assert.That(copied).IsEqualTo(IntPtr.Zero);
        await Assert.That(loadedById).IsEqualTo(IntPtr.Zero);
        await Assert.That(loadedByName).IsEqualTo(IntPtr.Zero);
    }

    /// <summary>Validates captured cursor null clone and protected non-disposing branch.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CapturedCursor_NullCloneAndDisposeFalseBranchesAreSafeAsync()
    {
        using var probe = new CapturedCursorProbe { HotSpot = new(One, Two), Size = new(Three, Four) };
        using var clone = probe.Clone();

        probe.DisposeWithoutManagedCleanup();

        await Assert.That(clone.ColorLayer).IsNull();
        await Assert.That(clone.MaskLayer).IsNull();
        await Assert.That(clone.HotSpot).IsEqualTo(new(One, Two));
        await Assert.That(clone.Size).IsEqualTo(new(Three, Four));
    }

    /// <summary>Validates raw color extraction for alpha, no-alpha, and invalid dimensions.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ExtractRawColorBitmap_CoversAlphaOpaqueAndDimensionBranchesAsync()
    {
        using var alphaSource = CreateSolidBitmap(Two, Two, Color.FromArgb(OneHundredTwentyEight, Color.Red));
        using var alphaHandle = new SafeHBitmapHandle(alphaSource);
        using var alphaBitmap = Icons.CursorHelper.ExtractRawColorBitmap(alphaHandle, Two, Two, out var alphaDetected);

        using var opaqueSource = CreateSolidBitmap(Two, Two, Color.FromArgb(Zero, Color.Blue));
        using var opaqueHandle = new SafeHBitmapHandle(opaqueSource);
        using var opaqueBitmap = Icons.CursorHelper.ExtractRawColorBitmap(opaqueHandle, Two, Two, out var opaqueDetected);

        using var invalidWidthBitmap = Icons.CursorHelper.ExtractRawColorBitmap(alphaHandle, Zero, Two, out var invalidWidthAlpha);
        using var invalidHeightBitmap = Icons.CursorHelper.ExtractRawColorBitmap(alphaHandle, Two, Zero, out var invalidHeightAlpha);

        await Assert.That(alphaBitmap).IsNotNull();
        await Assert.That(alphaDetected || alphaBitmap.GetPixel(Zero, Zero).A == byte.MaxValue).IsTrue();
        await Assert.That(opaqueBitmap).IsNotNull();
        await Assert.That(opaqueDetected).IsFalse();
        await Assert.That(opaqueBitmap.GetPixel(Zero, Zero).A).IsEqualTo(byte.MaxValue);
        await Assert.That(invalidWidthBitmap).IsNull();
        await Assert.That(invalidWidthAlpha).IsFalse();
        await Assert.That(invalidHeightBitmap).IsNull();
        await Assert.That(invalidHeightAlpha).IsFalse();
    }

    /// <summary>Validates cursor drawing branches for conversion, scaling, clipping, and unsupported formats.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DrawCursorOnBitmap_CoversRemainingAlphaAndMaskCompositionBranchesAsync()
    {
        using var alphaTarget = CreateSolidBitmap(Four, Four, PixelFormat.Format24bppRgb, Color.White);
        using var alphaLayer = CreateSolidBitmap(Three, Three, PixelFormat.Format24bppRgb, Color.Red);
        using var alphaCursor = new CapturedCursor { ColorLayer = alphaLayer, Size = new(Three, Three) };

        Icons.CursorHelper.DrawCursorOnBitmap(alphaTarget, alphaCursor, new(-One, -One));
        Icons.CursorHelper.DrawCursorOnBitmap(alphaTarget, alphaCursor, new(Four, Four));

        using var maskTarget = CreateSolidBitmap(Four, Four, PixelFormat.Format24bppRgb, Color.White);
        using var maskColor = CreateSolidBitmap(Two, Two, PixelFormat.Format24bppRgb, Color.Blue);
        using var maskLayer = CreateSolidBitmap(Two, Two, PixelFormat.Format24bppRgb, Color.Black);
        using var maskCursor = new CapturedCursor { ColorLayer = maskColor, MaskLayer = maskLayer, Size = new(Two, Two) };

        Icons.CursorHelper.DrawCursorOnBitmap(maskTarget, maskCursor, new(One, One), new(Three, Three));
        Icons.CursorHelper.DrawCursorOnBitmap(maskTarget, maskCursor, new(Four, Four));

        using var indexedTarget = new Bitmap(Four, Four, PixelFormat.Format8bppIndexed);
        await Assert
            .That(() => Icons.CursorHelper.DrawCursorOnBitmap(indexedTarget, maskCursor, new(Zero, Zero)))
            .Throws<NotSupportedException>();

        await Assert.That(alphaTarget.GetPixel(Zero, Zero).R > OneHundredTwenty).IsTrue();
        await Assert.That(maskTarget.GetPixel(One, One).R < TwoHundredFiftyFive).IsTrue();
    }

    /// <summary>Validates graphics drawing for masked cursors against an in-memory bitmap.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DrawCursorOnGraphics_CoversMaskedCompositionBranchAsync()
    {
        using var target = CreateSolidBitmap(Eight, Eight, PixelFormat.Format32bppArgb, Color.White);
        using var graphics = Graphics.FromImage(target);
        using var colorLayer = CreateSolidBitmap(Two, Two, PixelFormat.Format32bppArgb, Color.Black);
        using var maskLayer = CreateSolidBitmap(Two, Two, PixelFormat.Format32bppArgb, Color.White);
        using var cursor = new CapturedCursor { ColorLayer = colorLayer, MaskLayer = maskLayer, Size = new(Two, Two) };

        Icons.CursorHelper.DrawCursorOnGraphics(graphics, cursor, new(One, One));

        await Assert.That(target.Width).IsEqualTo(Eight);
    }

    /// <summary>Validates cursor helper composition through its internal test seam.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CursorHelper_CaptureCurrentCursorCoversNativeDecisionBranchesAsync()
    {
        var fakeApi = new FakeNativeCursorApi();
        var previousApi = Icons.NativeCursorMethods.SetApiForTesting(fakeApi);
        var previousProvider = Icons.CursorHelper.SetCursorBaseSizeProviderForTesting(static () => Sixty);
        try
        {
            await Assert.That(static () => Icons.CursorHelper.CaptureCurrentCursor(null, IntPtr.Zero, default)).Throws<ArgumentNullException>();

            var resourceCursorInfo = CreateIconInfo(SystemCursorModuleName, TestCursorResourceId, IntPtr.Zero, IntPtr.Zero, new(Four, Five));
            var namedCursorInfo = CreateIconInfo(SystemCursorModuleName, NoCursorResourceId, IntPtr.Zero, IntPtr.Zero, new(Four, Five));
            await Assert.That(resourceCursorInfo.ModuleName).IsEqualTo(SystemCursorModuleName);
            await Assert.That(resourceCursorInfo.ResourceId).IsEqualTo(TestCursorResourceId);
            await Assert.That(namedCursorInfo.ModuleName).IsEqualTo(SystemCursorModuleName);
            await Assert.That(namedCursorInfo.ResourceId).IsEqualTo(NoCursorResourceId);

            using var captured = new CapturedCursor();
            Icons.CursorHelper.CaptureCurrentCursor(captured, new(One), resourceCursorInfo);
            using var namedCaptured = new CapturedCursor();
            Icons.CursorHelper.CaptureCurrentCursor(namedCaptured, new(Two), namedCursorInfo);

            await Assert.That(captured.ColorLayer).IsNotNull();
            await Assert.That(captured.MaskLayer).IsNotNull();
            await Assert.That(fakeApi.LoadImageByIdCalls).IsEqualTo(One);
            await Assert.That(fakeApi.LoadImageByNameCalls).IsEqualTo(One);
        }
        finally
        {
            _ = Icons.CursorHelper.SetCursorBaseSizeProviderForTesting(previousProvider);
            _ = Icons.NativeCursorMethods.SetApiForTesting(previousApi);
        }
    }

    /// <summary>Validates cursor-size branches through the internal cursor capture seam.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CursorHelper_SizeHelpersCoverBitmapAndFallbackBranchesAsync()
    {
        using var colorSource = CreateSolidBitmap(Six, Eight, PixelFormat.Format32bppArgb, Color.Green);
        using var maskSource = CreateSolidBitmap(Six, Eight, PixelFormat.Format32bppArgb, Color.White);
        var colorHandle = colorSource.GetHbitmap();
        var maskHandle = maskSource.GetHbitmap();

        var colorIconInfo = CreateIconInfo(CustomCursorFileName, Zero, colorHandle, IntPtr.Zero, new(Two, Three));
        var maskIconInfo = CreateIconInfo(CustomCursorFileName, Zero, IntPtr.Zero, maskHandle, new(Two, Three));

        using var colorResult = new CapturedCursor();
        using var maskResult = new CapturedCursor();
        Icons.CursorHelper.CaptureCurrentCursor(colorResult, new(Five), colorIconInfo);
        Icons.CursorHelper.CaptureCurrentCursor(maskResult, new(Five), maskIconInfo);

        await Assert.That(colorResult.Size).IsEqualTo(new(Six, Eight));
        await Assert.That(maskResult.Size).IsEqualTo(new(Six, Four));
    }

    /// <summary>Creates a solid 32-bit bitmap.</summary>
    /// <param name="width">The bitmap width.</param>
    /// <param name="height">The bitmap height.</param>
    /// <param name="color">The fill color.</param>
    /// <returns>The created bitmap.</returns>
    private static Bitmap CreateSolidBitmap(int width, int height, Color color) =>
        CreateSolidBitmap(width, height, PixelFormat.Format32bppArgb, color);

    /// <summary>Creates a solid bitmap.</summary>
    /// <param name="width">The bitmap width.</param>
    /// <param name="height">The bitmap height.</param>
    /// <param name="format">The bitmap pixel format.</param>
    /// <param name="color">The fill color.</param>
    /// <returns>The created bitmap.</returns>
    private static Bitmap CreateSolidBitmap(int width, int height, PixelFormat format, Color color)
    {
        var bitmap = new Bitmap(width, height, format);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(color);
        return bitmap;
    }

    /// <summary>Creates icon information with deterministic private native fields.</summary>
    /// <param name="moduleName">The icon module name.</param>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="colorBitmapHandle">The color bitmap handle.</param>
    /// <param name="maskBitmapHandle">The mask bitmap handle.</param>
    /// <param name="hotspot">The cursor hotspot.</param>
    /// <returns>The created icon information.</returns>
    private static IconInfoEx CreateIconInfo(
        string moduleName,
        ushort resourceId,
        IntPtr colorBitmapHandle,
        IntPtr maskBitmapHandle,
        NativePoint hotspot)
    {
        var iconInfo = IconInfoEx.Create();
        var size = Marshal.SizeOf<IconInfoEx>();
        var buffer = Marshal.AllocHGlobal(size);
        const int hotspotXOffset = NativeInt32ByteCount + NativeInt32ByteCount;
        const int hotspotYOffset = hotspotXOffset + NativeInt32ByteCount;
        const int maskBitmapHandleOffset = hotspotYOffset + NativeInt32ByteCount;
        var colorBitmapHandleOffset = maskBitmapHandleOffset + IntPtr.Size;
        var resourceIdOffset = colorBitmapHandleOffset + IntPtr.Size;
        var moduleNameOffset = resourceIdOffset + NativeUInt16ByteCount;
        try
        {
            Marshal.StructureToPtr(iconInfo, buffer, fDeleteOld: false);
            Marshal.WriteInt32(buffer, hotspotXOffset, hotspot.X);
            Marshal.WriteInt32(buffer, hotspotYOffset, hotspot.Y);
            Marshal.WriteIntPtr(buffer, maskBitmapHandleOffset, maskBitmapHandle);
            Marshal.WriteIntPtr(buffer, colorBitmapHandleOffset, colorBitmapHandle);
            Marshal.WriteInt16(buffer, resourceIdOffset, unchecked((short)resourceId));
            var moduleNameBytes = System.Text.Encoding.Unicode.GetBytes(moduleName + '\0');
            Marshal.Copy(moduleNameBytes, Zero, IntPtr.Add(buffer, moduleNameOffset), Math.Min(moduleNameBytes.Length, IconNameBufferLength * NativeUInt16ByteCount));
            return Marshal.PtrToStructure<IconInfoEx>(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>Exposes protected cursor disposal for branch coverage.</summary>
    private sealed class CapturedCursorProbe : CapturedCursor
    {
        /// <summary>Invokes the protected non-disposing branch.</summary>
        internal void DisposeWithoutManagedCleanup() => Dispose(disposing: false);
    }

    /// <summary>Fake native cursor API for deterministic forwarding assertions.</summary>
    private sealed class FakeNativeCursorApi : INativeCursorApi
    {
        /// <summary>Gets the copy-image call count.</summary>
        public int CopyImageCalls { get; private set; }

        /// <summary>Gets the load-image-by-id call count.</summary>
        public int LoadImageByIdCalls { get; private set; }

        /// <summary>Gets the load-image-by-name call count.</summary>
        public int LoadImageByNameCalls { get; private set; }

        /// <summary>Gets the last copy size.</summary>
        public NativeSize LastCopySize { get; private set; }

        /// <summary>Gets the last loaded image name.</summary>
        public string LastLoadName { get; private set; }

        /// <inheritdoc />
        public IntPtr CopyImage(IntPtr imageHandle, ImageType type, int cx, int cy, CopyImageFlags flags)
        {
            CopyImageCalls++;
            LastCopySize = new(cx, cy);
            return new(OneThousandTwoHundredThirtyFour);
        }

        /// <inheritdoc />
        public IntPtr LoadImage(IntPtr instanceHandle, IntPtr name, ImageType type, int cx, int cy, LoadImageFlags loadFlags)
        {
            LoadImageByIdCalls++;
            return new(FourThousandThreeHundredTwentyOne);
        }

        /// <inheritdoc />
        public IntPtr LoadImage(IntPtr instanceHandle, string name, ImageType type, int cx, int cy, LoadImageFlags loadFlags)
        {
            LoadImageByNameCalls++;
            LastLoadName = name;
            return new(TwentySixThousandOneHundred);
        }
    }
}
