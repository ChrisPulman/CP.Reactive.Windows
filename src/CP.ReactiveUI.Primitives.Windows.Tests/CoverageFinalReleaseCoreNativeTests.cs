// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Bgr24 = CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats.Bgr24;
using Indexed8 = CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats.Indexed8;
using NativePointTypeConverter = CP.ReactiveUI.Primitives.Windows.Native.TypeConverters.NativePointTypeConverter;
using NativeRectFloatTypeConverter = CP.ReactiveUI.Primitives.Windows.Native.TypeConverters.NativeRectFloatTypeConverter;
using NativeRectTypeConverter = CP.ReactiveUI.Primitives.Windows.Native.TypeConverters.NativeRectTypeConverter;
using NativeSizeFloatTypeConverter = CP.ReactiveUI.Primitives.Windows.Native.TypeConverters.NativeSizeFloatTypeConverter;
using NativeSizeTypeConverter = CP.ReactiveUI.Primitives.Windows.Native.TypeConverters.NativeSizeTypeConverter;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides deterministic final coverage for native and graphics primitives.</summary>
public sealed class CoverageFinalReleaseCoreNativeTests
{
    /// <summary>Defines a Windows 11 build number.</summary>
    private const int Windows11Build = 22_000;

    /// <summary>Defines the unsigned access-denied HRESULT value.</summary>
    private const long AccessDeniedHResult = 2_147_942_405L;

    /// <summary>Defines the signed failed access-denied HRESULT value.</summary>
    private const long FailedAccessDeniedHResult = -2_147_024_891L;

    /// <summary>Defines the UTF-16 byte for uppercase A.</summary>
    private const byte UppercaseA = 65;

    /// <summary>Defines a small rectangle side length.</summary>
    private const int RectangleSide = 2;

    /// <summary>Defines the blur radius used in the disabled-path test.</summary>
    private const int BlurRadius = 20;

    /// <summary>Defines the graphics device-context handle value.</summary>
    private const int GraphicsDeviceContextHandle = 1;

    /// <summary>Defines the selected GDI object handle value.</summary>
    private const int SelectedGdiObjectHandle = 2;

    /// <summary>Defines the deterministic major operating-system version.</summary>
    private const int MajorOperatingSystemVersion = 10;

    /// <summary>Exercises every Windows-version predicate against deterministic version data.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsVersion_AsyncLocalProvider_EvaluatesLegacyAndModernPredicatesAsync()
    {
        using (WindowsVersion.OverrideVersionProviderForTesting(static () => new(4, 0)))
        {
            await Assert.That(WindowsVersion.IsWindowsBeforeVista).IsTrue();
            await Assert.That(WindowsVersion.IsWindowsXpOrLater).IsFalse();
            await Assert.That(WindowsVersion.IsWindows10BuildOrLater(1)).IsFalse();
        }

        using (WindowsVersion.OverrideVersionProviderForTesting(static () => new(5, 1)))
        {
            await Assert.That(WindowsVersion.IsWindowsXp).IsTrue();
            await Assert.That(WindowsVersion.IsWindowsXpOrLater).IsTrue();
            await Assert.That(WindowsVersion.IsWindowsVistaOrLater).IsFalse();
        }

        using (WindowsVersion.OverrideVersionProviderForTesting(static () => new(6, 0)))
        {
            await Assert.That(WindowsVersion.IsWindowsVista).IsTrue();
            await Assert.That(WindowsVersion.IsWindows7OrLater).IsFalse();
            await Assert.That(WindowsVersion.IsWindows8OrLater).IsFalse();
            await Assert.That(WindowsVersion.IsWindows81OrLater).IsFalse();
        }

        using (WindowsVersion.OverrideVersionProviderForTesting(static () => new(6, 2)))
        {
            await Assert.That(WindowsVersion.IsWindows8).IsTrue();
            await Assert.That(WindowsVersion.IsWindows81).IsFalse();
            await Assert.That(WindowsVersion.IsWindows8X).IsTrue();
        }

        using (WindowsVersion.OverrideVersionProviderForTesting(static () => new(6, 3)))
        {
            await Assert.That(WindowsVersion.IsWindows81).IsTrue();
            await Assert.That(WindowsVersion.IsWindows81OrLater).IsTrue();
            await Assert.That(WindowsVersion.IsWindows8OrLater).IsTrue();
        }

        using (WindowsVersion.OverrideVersionProviderForTesting(static () => new(10, 0, Windows11Build)))
        {
            await Assert.That(WindowsVersion.IsWindows10).IsTrue();
            await Assert.That(WindowsVersion.IsWindows10OrLater).IsTrue();
            await Assert.That(WindowsVersion.IsWindows11OrLater).IsTrue();
            await Assert.That(WindowsVersion.IsWindows10BuildOrLater(Windows11Build)).IsTrue();
        }

        await Assert.That(static () => WindowsVersion.OverrideVersionProviderForTesting(null)).Throws<ArgumentNullException>();
    }

    /// <summary>Exercises pure HRESULT, UTF-16, point, rectangle, and converter paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeValues_PureBranches_ReturnExpectedResultsAsync()
    {
        await Assert.That(Win32.GetHResult(Win32Error.AccessDenied)).IsEqualTo(AccessDeniedHResult);
        await Assert.That(Win32.GetHResult(unchecked((Win32Error)0x80070005U))).IsEqualTo(FailedAccessDeniedHResult);
        await Assert.That(NativeUtf16String.ReadNullTerminated([UppercaseA, 0, 0, 0])).IsEqualTo("A");
        await Assert.That(NativeUtf16String.ReadNullTerminated([0, 1, 0, 0])).IsEqualTo("Ā");
        await Assert.That(NativePoint.Empty).IsEqualTo(new(0, 0));
        await Assert.That(new NativeRectFloat(0, 0, RectangleSide, RectangleSide).Contains(new(1, 1))).IsTrue();

        var pointConverter = new NativePointTypeConverter();
        var sizeConverter = new NativeSizeTypeConverter();
        var sizeFloatConverter = new NativeSizeFloatTypeConverter();
        var rectConverter = new NativeRectTypeConverter();
        var rectFloatConverter = new NativeRectFloatTypeConverter();
        await Assert.That(pointConverter.CanConvertFrom(null, typeof(string))).IsTrue();
        await Assert.That(pointConverter.CanConvertTo(null, typeof(string))).IsTrue();
        await Assert.That(sizeConverter.CanConvertFrom(null, typeof(int))).IsFalse();
        await Assert.That(sizeFloatConverter.CanConvertTo(null, typeof(int))).IsFalse();
        await Assert.That(rectConverter.CanConvertFrom(null, typeof(int))).IsFalse();
        await Assert.That(rectFloatConverter.CanConvertTo(null, typeof(int))).IsFalse();
    }

    /// <summary>Exercises managed bitmap validation branches without native calls.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapAccessor_ManagedValidation_RejectsInvalidPixelCombinationsAsync()
    {
        using var bitmap = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        await Assert.That(() => new BitmapAccessor<UnrecognizedPixel>(bitmap)).Throws<NotSupportedException>();
        await Assert.That(() => new BitmapAccessor<Bgr24>(bitmap)).Throws<NotSupportedException>();
        await Assert.That(() => new BitmapAccessor<Indexed8>(bitmap)).Throws<NotSupportedException>();
    }

    /// <summary>Exercises the graphics-context selection composition path without GDI calls.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SafeGraphicsDcHandle_SelectObject_UsesComposedGdiApiAsync()
    {
        var probe = new GdiHandleProbe();
        var operations = new GdiSafeHandleOperations(GdiHandleProbe.DeleteObject, probe.SelectObject, GdiHandleProbe.RestoreObject);
        var previousApi = GdiSafeHandleApi.Swap(operations);
        try
        {
            using var graphicsHandle = new SafeGraphicsDcHandle(new(GraphicsDeviceContextHandle), false, static _ => { }, static () => { });
            using var objectHandle = new SafeNonDisposableObjectHandle(new(SelectedGdiObjectHandle));
            using var selection = graphicsHandle.SelectObject(objectHandle);
            await Assert.That(selection.IsInvalid).IsFalse();
            await Assert.That(probe.SelectCount).IsEqualTo(1);
        }
        finally
        {
            _ = GdiSafeHandleApi.Swap(previousApi);
        }
    }

    /// <summary>Exercises the short-circuit blur path without allocating native GDI+ state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task GdiPlus_DrawWithBlur_DisabledStateReturnsFalseBeforeNativeArgumentsAreUsedAsync()
    {
        var operations = new Gdi.GdiPlusBlurOperations { GetOperatingSystemVersion = static () => new(MajorOperatingSystemVersion, 0) };
        var previousState = Gdi.GdiPlusApi.ReplaceStateForTesting(operations, false);
        try
        {
            await Assert.That(Gdi.GdiPlusApi.DrawWithBlur(null, null, default, null, null, BlurRadius, false)).IsFalse();
        }
        finally
        {
            Gdi.GdiPlusApi.RestoreStateForTesting(previousState);
        }
    }

    /// <summary>Represents an unsupported managed bitmap pixel type.</summary>
    private readonly struct UnrecognizedPixel
    {
        /// <summary>Gets the deterministic marker value.</summary>
        internal static byte Marker => 0;

        /// <inheritdoc />
        public override string ToString() => $"{nameof(UnrecognizedPixel)}:{Marker}";
    }

    /// <summary>Records deterministic GDI safe-handle operations.</summary>
    private sealed class GdiHandleProbe
    {
        /// <summary>Defines the selected-object native handle value.</summary>
        private const int SelectedObjectHandle = 3;

        /// <summary>Gets the number of selection calls.</summary>
        internal int SelectCount { get; private set; }

        /// <summary>Records a delete request.</summary>
        /// <param name="handle">The handle to delete.</param>
        /// <returns>Always true.</returns>
        internal static bool DeleteObject(IntPtr handle)
        {
            _ = handle;
            return true;
        }

        /// <summary>Records an object restoration.</summary>
        /// <param name="deviceContext">The target device context.</param>
        /// <param name="objectHandle">The restored object.</param>
        /// <returns>The synthetic restored object.</returns>
        internal static IntPtr RestoreObject(SafeHandle deviceContext, IntPtr objectHandle)
        {
            _ = deviceContext;
            return objectHandle;
        }

        /// <summary>Records an object selection.</summary>
        /// <param name="deviceContext">The target device context.</param>
        /// <param name="objectHandle">The selected object.</param>
        /// <returns>A synthetic previous object.</returns>
        internal IntPtr SelectObject(SafeHandle deviceContext, SafeHandle objectHandle)
        {
            _ = deviceContext;
            _ = objectHandle;
            SelectCount++;
            return new(SelectedObjectHandle);
        }
    }
}
