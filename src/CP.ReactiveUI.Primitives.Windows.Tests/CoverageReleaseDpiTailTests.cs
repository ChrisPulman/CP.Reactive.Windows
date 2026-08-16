// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Deterministic coverage for DPI display adapter boundaries.</summary>
public sealed class CoverageReleaseDpiTailTests
{
    /// <summary>The expected number of display topology snapshots.</summary>
    private const int ExpectedSnapshotCount = 2;

    /// <summary>A test DPI value.</summary>
    private const int DpiOneHundredFortyFour = 144;

    /// <summary>The deterministic scale multiplier.</summary>
    private const float ScaleMultiplier = 2F;

    /// <summary>The unscaled scalar value.</summary>
    private const double UnscaledScalar = 24D;

    /// <summary>The unscaled horizontal coordinate.</summary>
    private const int UnscaledX = 10;

    /// <summary>The unscaled vertical coordinate.</summary>
    private const int UnscaledY = 20;

    /// <summary>The unscaled width.</summary>
    private const int UnscaledWidth = 30;

    /// <summary>The unscaled height.</summary>
    private const int UnscaledHeight = 40;

    /// <summary>The expected scaled scalar value.</summary>
    private const double ScaledScalar = 32D;

    /// <summary>The maximum accepted floating-point rounding difference.</summary>
    private const double ComparisonTolerance = 0.001D;

    /// <summary>The expected scaled width.</summary>
    private const int ScaledWidth = 32;

    /// <summary>A test message handle.</summary>
    private static readonly IntPtr WindowHandle = new(17);

    /// <summary>The deterministic input size.</summary>
    private static readonly NativeSize UnscaledSize = new((int)UnscaledScalar, UnscaledWidth);

    /// <summary>The deterministic input point.</summary>
    private static readonly NativePoint UnscaledPoint = new((int)UnscaledScalar, UnscaledWidth);

    /// <summary>The expected scaled size.</summary>
    private static readonly NativeSize ScaledSize = new(ScaledWidth, UnscaledHeight);

    /// <summary>The expected scaled point.</summary>
    private static readonly NativePoint ScaledPoint = new(ScaledWidth, UnscaledHeight);

    /// <summary>Verifies every unscale overload applies supplied modifiers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DpiCalculator_UnscaleModifiers_ApplyToEveryValueShapeAsync()
    {
        static float DoubleScale(float value) => value * ScaleMultiplier;

        var scalar = DpiCalculator.UnscaleWithDpi(UnscaledScalar, DpiOneHundredFortyFour, DoubleScale);
        var size = DpiCalculator.UnscaleWithDpi(UnscaledSize, DpiOneHundredFortyFour, DoubleScale);
        var point = DpiCalculator.UnscaleWithDpi(UnscaledPoint, DpiOneHundredFortyFour, DoubleScale);

        await Assert.That(Math.Abs(scalar - ScaledScalar) < ComparisonTolerance).IsTrue();
        await Assert.That(size).IsEqualTo(ScaledSize);
        await Assert.That(point).IsEqualTo(ScaledPoint);
    }

    /// <summary>Verifies read-only native DPI export resolution against the current process.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeDpiMethods_DefaultExports_QueryCurrentProcessWithoutChangingDpiStateAsync()
    {
        using Process process = Process.GetCurrentProcess();
        _ = NativeDpiMethods.GetProcessDpiAwareness(process.Handle, out DpiAwareness awareness);
        var isDpiAware = NativeDpiMethods.IsDpiAware;

#if NETFRAMEWORK
        await Assert.That(Enum.IsDefined(typeof(DpiAwareness), awareness)).IsTrue();
#else
        await Assert.That(Enum.IsDefined(awareness)).IsTrue();
#endif
        await Assert.That(isDpiAware).IsEqualTo(awareness is not DpiAwareness.Unaware and not DpiAwareness.Invalid);
    }

    /// <summary>Verifies window DPI dispatch uses injected operations without touching a native window.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DpiHandler_WindowDispatch_UsesDeterministicAvailabilityAndPositioningAsync()
    {
        var nativeApi = new DpiApiStub();
        INativeDpiApi previousApi = NativeDpiMethods.ExchangeApi(nativeApi);
        Func<bool> previousAvailability = DpiHandler.ExchangeWindows10Availability(static () => false);
        try
        {
            await Assert.That(DpiHandler.TryEnableNonClientDpiScaling(WindowHandle)).IsFalse();
        }
        finally
        {
            _ = DpiHandler.ExchangeWindows10Availability(previousAvailability);
        }

        var positionCalls = 0;
        SetWindowPositionOperation previousPositionOperation = DpiHandler.ExchangeWindowPositionOperation(
            (windowHandle, insertAfterWindowHandle, x, y, width, height, flags) =>
            {
                positionCalls++;
                return windowHandle == WindowHandle
                    && insertAfterWindowHandle == IntPtr.Zero
                    && x == UnscaledX
                    && y == UnscaledY
                    && width == UnscaledWidth
                    && height == UnscaledHeight
                    && flags == (WindowPos.SWP_NOACTIVATE | WindowPos.SWP_NOOWNERZORDER | WindowPos.SWP_NOZORDER);
            });
        var rectanglePointer = Marshal.AllocHGlobal(NativeRect.SizeOf);
        try
        {
            Marshal.StructureToPtr(new NativeRect(UnscaledX, UnscaledY, UnscaledWidth, UnscaledHeight), rectanglePointer, false);
            using var handler = new DpiHandler();
            var handled = false;
            var result = handler.HandleWindowMessages(
                WindowHandle,
                (int)WindowsMessages.WM_DPICHANGED,
                new(DpiOneHundredFortyFour),
                rectanglePointer,
                ref handled);
            var parentResult = handler.HandleWindowMessages(
                WindowMessageInfo.Create(
                    WindowHandle.ToInt64(),
                    (int)WindowsMessages.WM_DPICHANGED_AFTERPARENT,
                    0,
                    0));

            await Assert.That(result).IsEqualTo(IntPtr.Zero);
            await Assert.That(handled).IsTrue();
            await Assert.That(positionCalls).IsEqualTo(1);
            await Assert.That(parentResult).IsFalse();
        }
        finally
        {
            Marshal.FreeHGlobal(rectanglePointer);
            _ = DpiHandler.ExchangeWindowPositionOperation(previousPositionOperation);
            _ = NativeDpiMethods.ExchangeApi(previousApi);
        }
    }

    /// <summary>Verifies Forms and WPF adapters use injectable message sources without creating native handles.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DpiAdapters_AttachUsingDeterministicMessageSourcesAsync()
    {
        var nativeApi = new DpiApiStub();
        INativeDpiApi previousApi = NativeDpiMethods.ExchangeApi(nativeApi);
        using var messages = new ManualObservable<WindowMessageInfo>();
        Func<System.Windows.Forms.Control, IObservable<WindowMessageInfo>> previousFormsSource =
            FormsDpiExtensions.ExchangeWindowMessageSource(_ => messages);
        Func<System.Windows.Window, IObservable<WindowMessageInfo>> previousWpfSource =
            WindowDpiExtensions.ExchangeWindowMessageSource(_ => messages);
        try
        {
            System.Windows.Forms.ContextMenuStrip contextMenuStrip = null;
            System.Windows.Forms.Form form = null;
            using DpiHandler contextMenuHandler = contextMenuStrip.AttachDpiHandler();
            using DpiHandler formHandler = form.AttachDpiHandler();
            using DpiHandler windowHandler = new System.Windows.Window().AttachDpiHandler();

            messages.OnNext(WindowMessageInfo.Create(WindowHandle.ToInt64(), (int)WindowsMessages.WM_NULL, 0, 0));

            await Assert.That(contextMenuHandler).IsNotNull();
            await Assert.That(formHandler).IsNotNull();
            await Assert.That(windowHandler).IsNotNull();
        }
        finally
        {
            _ = FormsDpiExtensions.ExchangeWindowMessageSource(previousFormsSource);
            _ = WindowDpiExtensions.ExchangeWindowMessageSource(previousWpfSource);
            _ = NativeDpiMethods.ExchangeApi(previousApi);
        }
    }

    /// <summary>Verifies the public topology stream filters display messages through deterministic sources.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DisplayTopology_ObserveChanges_UsesInjectedSourcesAsync()
    {
        using var messages = new ManualObservable<WindowMessage>();
        IReadOnlyList<DisplayInfo> expectedSnapshot = [];
        var snapshotCalls = 0;
        using IDisposable scope = DisplayTopology.ExchangeSources(
            () => messages,
            () =>
            {
                snapshotCalls++;
                return expectedSnapshot;
            });
        var snapshots = new List<IReadOnlyList<DisplayInfo>>();
        using IDisposable subscription = DisplayTopology.ObserveChanges().Subscribe(snapshots.Add);

        messages.OnNext(new(IntPtr.Zero, WindowsMessages.WM_NULL, IntPtr.Zero, IntPtr.Zero));
        messages.OnNext(new(IntPtr.Zero, WindowsMessages.WM_DISPLAYCHANGE, IntPtr.Zero, IntPtr.Zero));

        await Assert.That(snapshots.Count).IsEqualTo(ExpectedSnapshotCount);
        await Assert.That(snapshots[0]).IsSameReferenceAs(expectedSnapshot);
        await Assert.That(snapshots[1]).IsSameReferenceAs(expectedSnapshot);
        await Assert.That(snapshotCalls).IsEqualTo(ExpectedSnapshotCount);
    }

    /// <summary>Verifies lookup falls back to a primary display and returns empty bounds when no display is available.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DisplayTopology_GetBounds_UsesPrimaryAndEmptyFallbacksAsync()
    {
        var primaryBounds = new NativeRect(UnscaledX, UnscaledY, UnscaledWidth, UnscaledHeight);
        IReadOnlyList<DisplayInfo> primarySnapshot = [new() { Bounds = primaryBounds, IsPrimary = true }];
        NativeRect primaryFallback;
        NativePoint unmatchedPoint = new(-UnscaledX, -UnscaledY);
        using (DisplayTopology.ExchangeSources(CreateEmptyMessages, () => primarySnapshot))
        {
            primaryFallback = DisplayTopology.GetBounds(unmatchedPoint);
        }

        NativeRect emptyFallback;
        using (DisplayTopology.ExchangeSources(CreateEmptyMessages, static () => []))
        {
            emptyFallback = DisplayTopology.GetBounds(unmatchedPoint);
        }

        await Assert.That(primaryFallback).IsEqualTo(primaryBounds);
        await Assert.That(emptyFallback).IsEqualTo(NativeRect.Empty);
    }

    /// <summary>Creates an empty message observable.</summary>
    /// <returns>An empty message stream.</returns>
    private static IObservable<WindowMessage> CreateEmptyMessages() => EmptyWindowMessageObservable.Instance;

    /// <summary>Provides an empty message observable for display-topology source overrides.</summary>
    private sealed class EmptyWindowMessageObservable : IObservable<WindowMessage>
    {
        /// <summary>Gets the shared empty message observable.</summary>
        internal static EmptyWindowMessageObservable Instance { get; } = new();

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<WindowMessage> observer)
        {
            observer.OnCompleted();
            return EmptyDisposable.Instance;
        }
    }

    /// <summary>Provides deterministic native DPI responses.</summary>
    private sealed class DpiApiStub : INativeDpiApi
    {
        /// <inheritdoc />
        public bool AdjustWindowRectExForDpi(
            ref NativeRect rect,
            WindowStyleFlags style,
            bool hasMenu,
            ExtendedWindowStyleFlags extendedStyle,
            uint dpi) => true;

        /// <inheritdoc />
        public IDisposable DefaultScopedThreadDpiAwarenessContext() => global::ReactiveUI.Primitives.Disposables.Scope.Empty;

        /// <inheritdoc />
        public HResult EnableNonClientDpiScaling(IntPtr windowHandle) => HResult.Fail;

        /// <inheritdoc />
        public int GetDpi(IntPtr windowHandle) => DpiOneHundredFortyFour;

        /// <inheritdoc />
        public int GetDpi(NativePoint location) => DpiOneHundredFortyFour;

        /// <inheritdoc />
        public uint GetDpiForSystem() => DpiOneHundredFortyFour;

        /// <inheritdoc />
        public int GetSystemMetricsForDpi(SystemMetric index, uint dpi) => 0;

        /// <inheritdoc />
        public bool SystemParametersInfoForDpi(
            SystemParametersInfoActions action,
            uint uiParameter,
            IntPtr parameter,
            SystemParametersInfoBehaviors updateProfileFlags,
            uint dpi) => false;
    }
}
