// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing.Imaging;
using System.Xml.Linq;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Exercises cursor and package-icon decisions without invoking desktop APIs.</summary>
public sealed class CoverageFinalCursorIconDecisionTests
{
    /// <summary>The Appx manifest foundation namespace.</summary>
    private const string ManifestNamespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";

    /// <summary>The Appx package manifest root element name.</summary>
    private const string PackageElement = "Package";

    /// <summary>Verifies raw cursor bitmap completion handles failed, alpha, and opaque reads in memory.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CursorHelper_RawBitmapCompletionHandlesAllOutcomesAsync()
    {
        var failedBitmap = new Bitmap(One, One, PixelFormat.Format32bppPArgb);
        BitmapData failedBitmapData = failedBitmap.LockBits(new(Zero, Zero, One, One), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
        Bitmap failedResult = Icons.CursorHelper.CreateRawColorBitmap(Zero, failedBitmap, failedBitmapData, One, One, out var failedHasAlpha);

        using var alphaBitmap = new Bitmap(One, One, PixelFormat.Format32bppPArgb);
        alphaBitmap.SetPixel(Zero, Zero, Color.FromArgb(OneHundredTwentyEight, Color.Red));
        BitmapData alphaBitmapData = alphaBitmap.LockBits(new(Zero, Zero, One, One), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
        Bitmap alphaResult = Icons.CursorHelper.CreateRawColorBitmap(One, alphaBitmap, alphaBitmapData, One, One, out var alphaHasAlpha);

        using var opaqueBitmap = new Bitmap(One, One, PixelFormat.Format32bppPArgb);
        opaqueBitmap.SetPixel(Zero, Zero, Color.FromArgb(Zero, Color.Blue));
        BitmapData opaqueBitmapData = opaqueBitmap.LockBits(new(Zero, Zero, One, One), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
        Bitmap opaqueResult = Icons.CursorHelper.CreateRawColorBitmap(One, opaqueBitmap, opaqueBitmapData, One, One, out var opaqueHasAlpha);

        await Assert.That(failedResult).IsNull();
        await Assert.That(failedHasAlpha).IsFalse();
        await Assert.That(alphaResult).IsSameReferenceAs(alphaBitmap);
        await Assert.That(alphaHasAlpha).IsTrue();
        await Assert.That(opaqueResult).IsSameReferenceAs(opaqueBitmap);
        await Assert.That(opaqueHasAlpha).IsFalse();
        await Assert.That(opaqueBitmap.GetPixel(Zero, Zero).A).IsEqualTo(byte.MaxValue);
    }

    /// <summary>Verifies cursor composition and native-result decisions are deterministic.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CursorHelper_CompositionDecisionsCoverNativeFailureAndSuccessPathsAsync()
    {
        await Assert.That(static () => Icons.CursorHelper.ThrowIfObjectSelectionFailed(selectionFailed: true, One)).Throws<Win32Exception>();
        await Assert.That(static () => Icons.CursorHelper.ThrowIfRasterOperationFailed(operationSucceeded: false, Two)).Throws<Win32Exception>();

        Icons.CursorHelper.ThrowIfObjectSelectionFailed(selectionFailed: false, Zero);
        Icons.CursorHelper.ThrowIfRasterOperationFailed(operationSucceeded: true, Zero);

        CursorMaskLayerProvider previousMaskLayerProvider = Icons.CursorHelper.SetCursorMaskLayerProviderForTesting(
            static (_, width, height) => new Bitmap(width, height, PixelFormat.Format24bppRgb));
        try
        {
            Bitmap alphaMaskLayer = Icons.CursorHelper.CreateMaskLayer(hasAlpha: true, cursorHandle: IntPtr.Zero, targetSize: new(One, One));
            using Bitmap opaqueMaskLayer = Icons.CursorHelper.CreateMaskLayer(hasAlpha: false, cursorHandle: IntPtr.Zero, targetSize: new(One, One));

            await Assert.That(alphaMaskLayer).IsNull();
            await Assert.That(opaqueMaskLayer).IsNotNull();
        }
        finally
        {
            _ = Icons.CursorHelper.SetCursorMaskLayerProviderForTesting(previousMaskLayerProvider);
        }

        CursorBitmapRenderer previousRenderer = Icons.CursorHelper.SetCursorBitmapRendererForTesting(
            static (_, width, height, flags) =>
            {
                var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                using Graphics graphics = Graphics.FromImage(bitmap);
                graphics.Clear(flags == DrawIconExFlags.DI_MASK ? Color.White : Color.Black);
                return bitmap;
            });
        try
        {
            using Bitmap defaultMaskLayer = Icons.CursorHelper.CreateMaskLayer(
                hasAlpha: false,
                cursorHandle: IntPtr.Zero,
                targetSize: new(Two, Two));

            await Assert.That(defaultMaskLayer.Width).IsEqualTo(Two);
            await Assert.That(defaultMaskLayer.Height).IsEqualTo(Two);
            await Assert.That(defaultMaskLayer.GetPixel(Zero, Zero).ToArgb()).IsEqualTo(Color.White.ToArgb());
        }
        finally
        {
            _ = Icons.CursorHelper.SetCursorBitmapRendererForTesting(previousRenderer);
        }

        await Assert.That(Icons.CursorHelper.GetConfiguredCursorBaseSize(ThirtyTwo)).IsEqualTo(ThirtyTwo);
        await Assert.That(Icons.CursorHelper.GetConfiguredCursorBaseSize("32")).IsNull();
        await Assert.That(Icons.CursorHelper.GetConfiguredCursorBaseSize(null)).IsNull();
        await Assert.That(Icons.CursorHelper.GetBitmapWidth(Zero, Forty)).IsEqualTo(ThirtyTwo);
        await Assert.That(Icons.CursorHelper.GetBitmapWidth(One, Forty)).IsEqualTo(Forty);
    }

    /// <summary>Verifies package-logo parsing covers every optional manifest element without file or shell access.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IconHelper_ManifestLogoParsingCoversMissingAndPresentElementsAsync()
    {
        XNamespace foundation = ManifestNamespace;
        var noRoot = new XDocument();
        var noProperties = new XDocument(new XElement(foundation + PackageElement));
        var noLogo = new XDocument(new XElement(foundation + PackageElement, new XElement(foundation + "Properties")));
        var withLogo = new XDocument(
            new XElement(
                foundation + PackageElement,
                new XElement(foundation + "Properties", new XElement(foundation + "Logo", "Assets/App.png"))));

        await Assert.That(Icons.IconHelper.ReadLogoPath(noRoot)).IsNull();
        await Assert.That(Icons.IconHelper.ReadLogoPath(noProperties)).IsNull();
        await Assert.That(Icons.IconHelper.ReadLogoPath(noLogo)).IsNull();
        await Assert.That(Icons.IconHelper.ReadLogoPath(withLogo)).IsEqualTo("Assets/App.png");
        await Assert.That(static () => Icons.IconHelper.ReadLogoPath(null)).Throws<ArgumentNullException>();
    }
}
