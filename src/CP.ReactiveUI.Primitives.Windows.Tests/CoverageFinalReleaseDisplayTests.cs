// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Verifies the composable display interop boundary without invoking Windows APIs.</summary>
public sealed class CoverageFinalReleaseDisplayTests
{
    /// <summary>A rooted successful non-client DPI scaling callback.</summary>
    private static readonly EnableNonClientDpiScalingOperation SuccessfulNonClientDpiScaling = static _ => One;

    /// <summary>Represents the non-client DPI scaling native operation.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>A non-zero native Boolean success value.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int EnableNonClientDpiScalingOperation(IntPtr windowHandle);

    /// <summary>Verifies empty WPF visual trees leave their layout transform untouched.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowDpiExtensions_EmptyVisualTree_DoesNotApplyATransformAsync()
    {
        var grid = new System.Windows.Controls.Grid();

        grid.UpdateLayoutTransform(OneAndHalfDouble);

        await Assert.That(grid.LayoutTransform.Value.IsIdentity).IsTrue();
    }

    /// <summary>Verifies the convenience bitmap apply-action overload defers execution.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapScaleHandler_AddApplyActionConvenienceOverload_DefersExecutionAsync()
    {
        using var handler = new BitmapScaleHandler<int, MemoryStream>();
        var wasApplied = false;

        var returnedHandler = handler.AddApplyAction(_ => wasApplied = true, One);

        await Assert.That(returnedHandler).IsSameReferenceAs(handler);
        await Assert.That(wasApplied).IsFalse();
    }

    /// <summary>Verifies successful non-client DPI scaling maps to a successful result.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeDpiMethods_NonClientScalingCore_MapsSuccessfulNativeResultAsync()
    {
        using var replacement = NativeDpiMethods.NativeMethods.ExchangeExportProviders(
            static _ => IntPtr.Zero,
            GetSuccessfulUser32Export);

        var result = NativeDpiMethods.EnableNonClientDpiScalingCore(new(One));

        await Assert.That(result).IsEqualTo(HResult.Ok);
    }

    /// <summary>Verifies each Windows DPI core operation delegates to its supplied implementation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsDpiCoreInterop_ForwardsAllOperationsToDeterministicCompositionAsync()
    {
        var deviceContext = new SafeWindowDcHandle(IntPtr.Zero, IntPtr.Zero);
        var rect = new NativeRect(One, Two, Three, Four);
        var observedDeviceCaps = GdiEnums.DeviceCaps.HORZRES;
        var observedWindow = IntPtr.Zero;
        var observedFlags = MonitorFrom.DefaultToNull;
        var interop = new WindowsDpiCoreInterop(
            (handle, caps) =>
            {
                observedDeviceCaps = caps;
                return ReferenceEquals(handle, deviceContext) ? OneHundredTwenty : Zero;
            },
            window =>
            {
                observedWindow = window;
                return deviceContext;
            },
            static window => window == new IntPtr(Three),
            (ref NativeRect source, MonitorFrom flags) =>
            {
                observedFlags = flags;
                source = new(Five, Six, Seven, Eight);
                return new(Nine);
            },
            (window, flags) =>
            {
                observedWindow = window;
                observedFlags = flags;
                return new(Ten);
            });

        var capability = interop.GetDeviceCaps(deviceContext, GdiEnums.DeviceCaps.VERTRES);
        var fromWindow = interop.FromWindow(new(Two));
        var isWindow = interop.IsWindow(new(Three));
        var monitorFromRect = interop.MonitorFromRect(ref rect, MonitorFrom.DefaultToPrimary);
        var monitorFromWindow = interop.MonitorFromWindow(new(Four), MonitorFrom.DefaultToNull);

        await Assert.That(capability).IsEqualTo(OneHundredTwenty);
        await Assert.That(fromWindow).IsSameReferenceAs(deviceContext);
        await Assert.That(isWindow).IsTrue();
        await Assert.That(monitorFromRect).IsEqualTo(new(Nine));
        await Assert.That(monitorFromWindow).IsEqualTo(new(Ten));
        await Assert.That(observedDeviceCaps).IsEqualTo(GdiEnums.DeviceCaps.VERTRES);
        await Assert.That(observedWindow).IsEqualTo(new(Four));
        await Assert.That(observedFlags).IsEqualTo(MonitorFrom.DefaultToNull);
        await Assert.That(rect).IsEqualTo(new(Five, Six, Seven, Eight));
        deviceContext.Dispose();
    }

    /// <summary>Gets a deterministic successful non-client DPI scaling export.</summary>
    /// <param name="exportName">The requested export name.</param>
    /// <returns>The callback pointer for the requested export.</returns>
    private static IntPtr GetSuccessfulUser32Export(string exportName) => exportName == "EnableNonClientDpiScaling"
        ? Marshal.GetFunctionPointerForDelegate(SuccessfulNonClientDpiScaling)
        : IntPtr.Zero;
}
