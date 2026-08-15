// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_TEST_SHIM
using DpiAwarenessValue = CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi.Enums.DpiAwareness;
using DpiDialogScalingBehaviors = CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi.Enums.DialogScalingBehaviors;
using DpiHostingBehaviorValue = CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi.Enums.DpiHostingBehavior;
using DpiMonitorType = CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi.Enums.MonitorDpiType;
#else
using DpiAwarenessValue = CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Enums.DpiAwareness;
using DpiDialogScalingBehaviors = CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Enums.DialogScalingBehaviors;
using DpiHostingBehaviorValue = CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Enums.DpiHostingBehavior;
using DpiMonitorType = CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Enums.MonitorDpiType;
#endif

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Covers DPI export adapters without loading native Windows modules.</summary>
public sealed class CoverageReleaseNativeDpiTests
{
    /// <summary>The deterministic primary DPI value.</summary>
    private const uint TestDpiX = 120U;

    /// <summary>The deterministic secondary DPI value.</summary>
    private const uint TestDpiY = 125U;

    /// <summary>Verifies the direct native DPI wrappers marshal every remaining export signature.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeDpiMethods_ExportProviders_MarshalRemainingDirectCallsAsync()
    {
        using var exports = new DpiExports();
        using var replacement = NativeDpiMethods.NativeMethods.ExchangeExportProviders(exports.GetShcore, exports.GetUser32);

        var processResult = NativeDpiMethods.GetProcessDpiAwareness(new(One), out var processAwareness);
        var setProcessResult = NativeDpiMethods.SetProcessDpiAwareness(DpiAwarenessValue.PerMonitorAware);
        var setContext = NativeDpiMethods.SetProcessDpiAwarenessContext(DpiAwarenessContext.PerMonitorAware);
        var noContext = NativeDpiMethods.SetProcessDpiAwarenessContext(DpiAwarenessContext.None);
        var monitorResult = NativeDpiMethods.GetDpiForMonitor(new(Two), DpiMonitorType.None, out var dpiX, out var dpiY);
        var threadContext = NativeDpiMethods.GetThreadDpiAwarenessContext();
        var contextAwareness = NativeDpiMethods.GetAwarenessFromDpiAwarenessContext(DpiAwarenessContext.PerMonitorAware);
        var contextDpi = NativeDpiMethods.GetDpiFromDpiAwarenessContext(DpiAwarenessContext.PerMonitorAware);
        var windowHosting = NativeDpiMethods.GetWindowDpiHostingBehavior(new(Three));
        var previousHosting = NativeDpiMethods.SetThreadDpiHostingBehavior(DpiHostingBehaviorValue.Mixed);
        var threadHosting = NativeDpiMethods.GetThreadDpiHostingBehavior();
        var dialogSet = NativeDpiMethods.SetDialogControlDpiChangeBehavior(
            new(Four),
            DpiDialogScalingBehaviors.DisableFontUpdate,
            DpiDialogScalingBehaviors.DisableRelayout);
        var dialogGet = NativeDpiMethods.GetDialogControlDpiChangeBehavior(new(Five));
        var logicalPoint = new NativePoint(One, Two);
        var logical = NativeDpiMethods.LogicalToPhysicalPointForPerMonitorDPI(new(Six), ref logicalPoint);
        var physicalPoint = new NativePoint(Three, Four);
        var physical = NativeDpiMethods.PhysicalToLogicalPointForPerMonitorDPI(new(Seven), ref physicalPoint);

        await Assert.That(processResult).IsEqualTo(HResult.Ok);
        await Assert.That(processAwareness).IsEqualTo(DpiAwarenessValue.PerMonitorAware);
        await Assert.That(setProcessResult).IsEqualTo(HResult.Ok);
        await Assert.That(setContext).IsTrue();
        await Assert.That(noContext).IsFalse();
        await Assert.That(monitorResult).IsEqualTo(HResult.Ok);
        await Assert.That(dpiX).IsEqualTo(TestDpiX);
        await Assert.That(dpiY).IsEqualTo(TestDpiY);
        await Assert.That(threadContext).IsEqualTo(DpiAwarenessContext.PerMonitorAwareV2);
        await Assert.That(contextAwareness).IsEqualTo(DpiAwarenessValue.PerMonitorAware);
        await Assert.That(contextDpi).IsEqualTo(TestDpiX);
        await Assert.That(windowHosting).IsEqualTo(DpiHostingBehaviorValue.Mixed);
        await Assert.That(previousHosting).IsEqualTo(DpiHostingBehaviorValue.Default);
        await Assert.That(threadHosting).IsEqualTo(DpiHostingBehaviorValue.Mixed);
        await Assert.That(dialogSet).IsTrue();
        await Assert.That(dialogGet).IsEqualTo(DpiDialogScalingBehaviors.DisableFontUpdate);
        await Assert.That(logical).IsTrue();
        await Assert.That(logicalPoint).IsEqualTo(new(Two, Three));
        await Assert.That(physical).IsTrue();
        await Assert.That(physicalPoint).IsEqualTo(new(Four, Five));
    }

    /// <summary>Verifies DPI platform and operation composition paths are deterministic and restorable.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeDpiMethods_PlatformAndOperations_CoverFallbackAndScopedBranchesAsync()
    {
        using var exports = new DpiExports();
        using var replacement = NativeDpiMethods.NativeMethods.ExchangeExportProviders(exports.GetShcore, exports.GetUser32);

        using (NativeDpiMethods.ExchangePlatform(static () => false, static () => false, static _ => false))
        {
            await Assert.That(NativeDpiMethods.EnableDpiAware()).IsFalse();
            using var unsupportedScope = NativeDpiMethods.ScopedThreadDpiAwarenessContext(DpiAwarenessContext.PerMonitorAware);
            await Assert.That(unsupportedScope).IsSameReferenceAs(global::ReactiveUI.Primitives.Disposables.Scope.Empty);
            await Assert.That(WindowsNativeDpiApi.Instance.GetDpi(new NativePoint(One, One)))
                .IsEqualTo(DpiCalculator.DefaultScreenDpi);
        }

        using (NativeDpiMethods.ExchangePlatform(static () => true, static () => true, static _ => true))
        {
            await Assert.That(NativeDpiMethods.EnableDpiAware()).IsTrue();
            await Assert.That(NativeDpiMethods.IsValidDpiAwarenessContext(DpiAwarenessContext.PerMonitorAware)).IsTrue();
            await Assert.That(NativeDpiMethods.SetThreadDpiAwarenessContext(DpiAwarenessContext.PerMonitorAware))
                .IsEqualTo(DpiAwarenessContext.PerMonitorAware);
            using var primaryScope = NativeDpiMethods.ScopedThreadDpiAwarenessContext(DpiAwarenessContext.PerMonitorAware);
            using var alternativeScope = NativeDpiMethods.ScopedThreadDpiAwarenessContext(
                DpiAwarenessContext.None,
                DpiAwarenessContext.PerMonitorAwareV2);
            using var emptyScope = NativeDpiMethods.ScopedThreadDpiAwarenessContext(DpiAwarenessContext.None);
            await Assert.That(ReferenceEquals(primaryScope, global::ReactiveUI.Primitives.Disposables.Scope.Empty)).IsFalse();
            await Assert.That(ReferenceEquals(alternativeScope, global::ReactiveUI.Primitives.Disposables.Scope.Empty)).IsFalse();
            await Assert.That(emptyScope).IsSameReferenceAs(global::ReactiveUI.Primitives.Disposables.Scope.Empty);
        }

        using (NativeDpiMethods.ExchangePlatform(static () => true, static () => true, static _ => false))
        {
            await Assert.That(NativeDpiMethods.EnableDpiAware()).IsTrue();
        }

        var parameters = NativeDpiMethods.SystemParametersInfoForDpi(
            SystemParametersInfoActions.SPI_GETWORKAREA,
            0U,
            new(One),
            SystemParametersInfoBehaviors.None,
            TestDpiX);
        var nativeParameters = WindowsNativeDpiApi.Instance.SystemParametersInfoForDpi(
            SystemParametersInfoActions.SPI_GETWORKAREA,
            0U,
            new(One),
            SystemParametersInfoBehaviors.None,
            TestDpiX);

        await Assert.That(parameters).IsTrue();
        await Assert.That(nativeParameters).IsTrue();
    }

    /// <summary>Verifies DPI core fallback logic without invoking real window or device-context APIs.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeDpiMethods_CoreInterop_CoversWindowAndPointFallbackBranchesAsync()
    {
        using var exports = new DpiExports();
        using var replacement = NativeDpiMethods.NativeMethods.ExchangeExportProviders(exports.GetShcore, exports.GetUser32);
        var coreInterop = new DpiCoreInteropProbe();
        var originalCoreInterop = NativeDpiMethods.ExchangeCoreInterop(coreInterop);
        try
        {
            using (NativeDpiMethods.ExchangePlatform(static () => true, static () => true, static _ => true))
            {
                await Assert.That(GetDpiForWindow(new(One))).IsEqualTo((int)TestDpiX);
            }

            coreInterop.IsWindowResult = false;
            await Assert.That(GetDpiForWindow(new(Two))).IsEqualTo(DpiCalculator.DefaultScreenDpi);

            coreInterop.IsWindowResult = true;
            using (NativeDpiMethods.ExchangePlatform(static () => true, static () => false, static _ => false))
            {
                await Assert.That(GetDpiForWindow(new(Three))).IsEqualTo((int)TestDpiX);
            }

            using (NativeDpiMethods.ExchangePlatform(static () => false, static () => false, static _ => false))
            {
                coreInterop.DeviceContextHandle = null;
                await Assert.That(GetDpiForWindow(new(Four))).IsEqualTo(DpiCalculator.DefaultScreenDpi);

                coreInterop.DeviceContextHandle = new(IntPtr.Zero, IntPtr.Zero);
                coreInterop.DeviceCapsResult = OneHundredFiftySix;
                await Assert.That(GetDpiForWindow(new(Five))).IsEqualTo(OneHundredFiftySix);
                await Assert.That(NativeDpiMethods.GetDpiCore(new NativePoint(Six, Seven)))
                    .IsEqualTo(DpiCalculator.DefaultScreenDpi);
            }

            using (NativeDpiMethods.ExchangePlatform(static () => true, static () => false, static _ => false))
            {
                await Assert.That(NativeDpiMethods.GetDpiCore(new NativePoint(Eight, Nine))).IsEqualTo((int)TestDpiX);
                exports.MonitorResult = HResult.Fail;
                await Assert.That(NativeDpiMethods.GetDpiCore(new NativePoint(Ten, Eleven)))
                    .IsEqualTo(DpiCalculator.DefaultScreenDpi);
            }

            exports.WindowResultValue = Zero;
            await Assert.That(NativeDpiMethods.EnableNonClientDpiScalingCore(new(Twelve))).IsEqualTo(HResult.Fail);
        }
        finally
        {
            _ = NativeDpiMethods.ExchangeCoreInterop(originalCoreInterop);
        }
    }

    /// <summary>Verifies direct DPI state helpers and remaining wrapper branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeDpiMethods_StateAndDpiApi_CoverRemainingBranchesAsync()
    {
        using var exports = new DpiExports();
        using var replacement = NativeDpiMethods.NativeMethods.ExchangeExportProviders(exports.GetShcore, exports.GetUser32);

        using (NativeDpiMethods.ExchangePlatform(static () => false, static () => false, static _ => false))
        {
            await Assert.That(NativeDpiMethods.IsDpiAware).IsFalse();
        }

        using (NativeDpiMethods.ExchangePlatform(static () => true, static () => true, static _ => true))
        {
            await Assert.That(NativeDpiMethods.IsDpiAware).IsTrue();

            exports.ProcessAwareness = DpiAwarenessValue.Unaware;
            await Assert.That(NativeDpiMethods.IsDpiAware).IsFalse();

            using var invalidAlternativeScope = NativeDpiMethods.ScopedThreadDpiAwarenessContext(
                DpiAwarenessContext.None,
                DpiAwarenessContext.None);
            await Assert.That(invalidAlternativeScope).IsSameReferenceAs(global::ReactiveUI.Primitives.Disposables.Scope.Empty);
        }

        var api = new NativeDpiApiProbe();
        var originalApi = NativeDpiMethods.ExchangeApi(api);
        try
        {
            var failedWindowRect = DpiApi.AdjustWindowRectForWindow(
                new(One, Two, Three, Four),
                WindowStyleFlags.WS_OVERLAPPEDWINDOW,
                new(Thirteen),
                true,
                ExtendedWindowStyleFlags.WS_EX_TOOLWINDOW);

            await Assert.That(failedWindowRect).IsNull();
        }
        finally
        {
            _ = NativeDpiMethods.ExchangeApi(originalApi);
        }
    }

    /// <summary>Gets DPI for a window handle through the handle overload.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The DPI for the supplied handle.</returns>
    private static int GetDpiForWindow(IntPtr windowHandle) => NativeDpiMethods.GetDpiCore(windowHandle);

    /// <summary>Provides managed StdCall callback pointers for the native DPI export seams.</summary>
    private sealed partial class DpiExports : IDisposable
    {
        /// <summary>The deterministic metric value.</summary>
        private const int TestMetric = 120;

        /// <summary>Keeps Shcore callback delegates alive.</summary>
        private readonly Dictionary<string, Delegate> _shcore;

        /// <summary>Keeps User32 callback delegates alive.</summary>
        private readonly Dictionary<string, Delegate> _user32;

        /// <summary>Initializes a new instance of the <see cref="DpiExports"/> class.</summary>
        public DpiExports()
        {
            ProcessAwarenessDelegate processAwareness = GetProcessAwareness;
            SetAwarenessDelegate setAwareness = SetAwareness;
            MonitorDpiDelegate monitorDpi = GetMonitorDpi;
            ContextDelegate setProcessContext = SetProcessContext;
            GetContextDelegate getThreadContext = GetThreadContext;
            ContextExchangeDelegate setThreadContext = SetThreadContext;
            AwarenessDelegate awareness = GetAwareness;
            ContextDpiDelegate contextDpi = GetContextDpi;
            WindowHostingDelegate windowHosting = GetWindowHosting;
            HostingDelegate threadHosting = SetThreadHosting;
            GetHostingDelegate getHosting = static () => DpiHostingBehaviorValue.Mixed;
            SetDialogDelegate setDialog = SetDialog;
            GetDialogDelegate getDialog = GetDialog;
            PointDelegate point = IncrementPoint;
            GetSystemDpiDelegate systemDpi = static () => TestDpiX;
            GetWindowDpiDelegate windowDpi = GetWindowDpi;
            MetricDelegate metric = GetMetric;
            AdjustRectDelegate adjustRect = AdjustRect;
            WindowResultDelegate windowResult = GetWindowResult;
            ParametersDelegate parameters = Parameters;

            _shcore = new(StringComparer.Ordinal) { ["GetProcessDpiAwareness"] = processAwareness, ["SetProcessDpiAwareness"] = setAwareness, ["GetDpiForMonitor"] = monitorDpi, };
            _user32 = new(StringComparer.Ordinal)
            {
                ["SetProcessDpiAwarenessContext"] = setProcessContext,
                ["GetThreadDpiAwarenessContext"] = getThreadContext,
                ["SetThreadDpiAwarenessContext"] = setThreadContext,
                ["GetAwarenessFromDpiAwarenessContext"] = awareness,
                ["GetDpiFromDpiAwarenessContext"] = contextDpi,
                ["IsValidDpiAwarenessContext"] = setProcessContext,
                ["GetWindowDpiHostingBehavior"] = windowHosting,
                ["SetThreadDpiHostingBehavior"] = threadHosting,
                ["GetThreadDpiHostingBehavior"] = getHosting,
                ["SetDialogControlDpiChangeBehavior"] = setDialog,
                ["GetDialogControlDpiChangeBehavior"] = getDialog,
                ["LogicalToPhysicalPointForPerMonitorDPI"] = point,
                ["PhysicalToLogicalPointForPerMonitorDPI"] = point,
                ["GetDpiForSystem"] = systemDpi,
                ["GetDpiForWindow"] = windowDpi,
                ["GetSystemMetricsForDpi"] = metric,
                ["AdjustWindowRectExForDpi"] = adjustRect,
                ["EnableNonClientDpiScaling"] = windowResult,
                ["SystemParametersInfoForDpi"] = parameters,
            };
        }

        /// <summary>Gets or sets the process awareness returned by the process-awareness export.</summary>
        public DpiAwarenessValue ProcessAwareness { get; set; } = DpiAwarenessValue.PerMonitorAware;

        /// <summary>Gets or sets the monitor DPI export result.</summary>
        public HResult MonitorResult { get; set; } = HResult.Ok;

        /// <summary>Gets or sets the window Boolean result returned by window operation exports.</summary>
        public int WindowResultValue { get; set; } = 1;

        /// <summary>Gets a deterministic Shcore export pointer.</summary>
        /// <param name="name">The requested export name.</param>
        /// <returns>The managed callback pointer.</returns>
        public IntPtr GetShcore(string name) => Marshal.GetFunctionPointerForDelegate(_shcore[name]);

        /// <summary>Gets a deterministic User32 export pointer.</summary>
        /// <param name="name">The requested export name.</param>
        /// <returns>The managed callback pointer.</returns>
        public IntPtr GetUser32(string name) => Marshal.GetFunctionPointerForDelegate(_user32[name]);

        /// <inheritdoc />
        public void Dispose()
        {
            _shcore.Clear();
            _user32.Clear();
        }

        /// <summary>Gets deterministic process awareness.</summary>
        /// <param name="process">The process handle.</param>
        /// <param name="awareness">The returned awareness.</param>
        /// <returns>The operation result.</returns>
        private uint GetProcessAwareness(IntPtr process, out DpiAwarenessValue awareness)
        {
            _ = process;
            awareness = ProcessAwareness;
            return (uint)HResult.Ok;
        }

        /// <summary>Accepts deterministic process awareness.</summary>
        /// <param name="awareness">The requested awareness.</param>
        /// <returns>The operation result.</returns>
        private HResult SetAwareness(DpiAwarenessValue awareness)
        {
            _ = _shcore.Count;
            _ = awareness;
            return HResult.Ok;
        }

        /// <summary>Gets deterministic monitor DPI.</summary>
        /// <param name="monitor">The monitor handle.</param>
        /// <param name="type">The DPI type.</param>
        /// <param name="x">The horizontal DPI.</param>
        /// <param name="y">The vertical DPI.</param>
        /// <returns>The operation result.</returns>
        private uint GetMonitorDpi(IntPtr monitor, DpiMonitorType type, out uint x, out uint y)
        {
            _ = monitor;
            _ = type;
            x = TestDpiX;
            y = TestDpiY;
            return (uint)MonitorResult;
        }

        /// <summary>Returns success when a process context is non-zero.</summary>
        /// <param name="context">The DPI context.</param>
        /// <returns>The native Boolean result.</returns>
        private int SetProcessContext(IntPtr context)
        {
            _ = _user32.Count;
            return context == IntPtr.Zero ? 0 : 1;
        }

        /// <summary>Gets a deterministic thread context.</summary>
        /// <returns>The current context.</returns>
        private IntPtr GetThreadContext()
        {
            _ = _user32.Count;
            return new((int)DpiAwarenessContext.PerMonitorAwareV2);
        }

        /// <summary>Sets a deterministic thread context.</summary>
        /// <param name="context">The requested context.</param>
        /// <returns>The previous context.</returns>
        private IntPtr SetThreadContext(IntPtr context)
        {
            _ = _user32.Count;
            _ = context;
            return new((int)DpiAwarenessContext.PerMonitorAware);
        }

        /// <summary>Gets deterministic awareness for a context.</summary>
        /// <param name="context">The DPI context.</param>
        /// <returns>The awareness.</returns>
        private DpiAwarenessValue GetAwareness(IntPtr context)
        {
            _ = _user32.Count;
            _ = context;
            return DpiAwarenessValue.PerMonitorAware;
        }

        /// <summary>Gets deterministic DPI for a context.</summary>
        /// <param name="context">The DPI context.</param>
        /// <returns>The DPI.</returns>
        private uint GetContextDpi(IntPtr context)
        {
            _ = _user32.Count;
            _ = context;
            return TestDpiX;
        }

        /// <summary>Gets deterministic window hosting behavior.</summary>
        /// <param name="window">The window handle.</param>
        /// <returns>The behavior.</returns>
        private DpiHostingBehaviorValue GetWindowHosting(IntPtr window)
        {
            _ = _user32.Count;
            _ = window;
            return DpiHostingBehaviorValue.Mixed;
        }

        /// <summary>Sets deterministic thread hosting behavior.</summary>
        /// <param name="behavior">The requested behavior.</param>
        /// <returns>The previous behavior.</returns>
        private DpiHostingBehaviorValue SetThreadHosting(DpiHostingBehaviorValue behavior)
        {
            _ = _user32.Count;
            _ = behavior;
            return DpiHostingBehaviorValue.Default;
        }

        /// <summary>Sets deterministic dialog behavior.</summary>
        /// <param name="window">The dialog handle.</param>
        /// <param name="mask">The behavior mask.</param>
        /// <param name="values">The behavior values.</param>
        /// <returns>The native Boolean result.</returns>
        private int SetDialog(IntPtr window, DpiDialogScalingBehaviors mask, DpiDialogScalingBehaviors values)
        {
            _ = _user32.Count;
            _ = window;
            _ = mask;
            _ = values;
            return 1;
        }

        /// <summary>Gets deterministic dialog behavior.</summary>
        /// <param name="window">The dialog handle.</param>
        /// <returns>The behavior.</returns>
        private DpiDialogScalingBehaviors GetDialog(IntPtr window)
        {
            _ = _user32.Count;
            _ = window;
            return DpiDialogScalingBehaviors.DisableFontUpdate;
        }

        /// <summary>Increments a point in place.</summary>
        /// <param name="window">The window handle.</param>
        /// <param name="point">The point to mutate.</param>
        /// <returns>The native Boolean result.</returns>
        private int IncrementPoint(IntPtr window, ref NativePoint point)
        {
            _ = _user32.Count;
            _ = window;
            point = new(point.X + 1, point.Y + 1);
            return 1;
        }

        /// <summary>Gets a deterministic metric.</summary>
        /// <param name="index">The metric index.</param>
        /// <param name="dpi">The DPI.</param>
        /// <returns>The metric.</returns>
        private int GetMetric(SystemMetric index, uint dpi)
        {
            _ = _user32.Count;
            _ = index;
            _ = dpi;
            return TestMetric;
        }

        /// <summary>Gets deterministic DPI for a window.</summary>
        /// <param name="window">The window handle.</param>
        /// <returns>The DPI value.</returns>
        private uint GetWindowDpi(IntPtr window)
        {
            _ = _user32.Count;
            _ = window;
            return TestDpiX;
        }

        /// <summary>Adjusts a rectangle deterministically.</summary>
        /// <param name="rect">The rectangle.</param>
        /// <param name="style">The window style.</param>
        /// <param name="menu">The menu flag.</param>
        /// <param name="extendedStyle">The extended window style.</param>
        /// <param name="dpi">The DPI.</param>
        /// <returns>The native Boolean result.</returns>
        private int AdjustRect(ref NativeRect rect, WindowStyleFlags style, int menu, ExtendedWindowStyleFlags extendedStyle, uint dpi)
        {
            _ = _user32.Count;
            _ = rect;
            _ = style;
            _ = menu;
            _ = extendedStyle;
            _ = dpi;
            return 1;
        }

        /// <summary>Returns deterministic success for a window operation.</summary>
        /// <param name="window">The window handle.</param>
        /// <returns>The native Boolean result.</returns>
        private int GetWindowResult(IntPtr window)
        {
            _ = window;
            return WindowResultValue;
        }

        /// <summary>Returns deterministic success for SystemParametersInfoForDpi.</summary>
        /// <param name="action">The requested action.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="value">The value pointer.</param>
        /// <param name="flags">The behavior flags.</param>
        /// <param name="dpi">The DPI.</param>
        /// <returns>The native Boolean result.</returns>
        private int Parameters(
            SystemParametersInfoActions action,
            uint parameter,
            IntPtr value,
            SystemParametersInfoBehaviors flags,
            uint dpi)
        {
            _ = _user32.Count;
            _ = action;
            _ = parameter;
            _ = value;
            _ = flags;
            _ = dpi;
            return 1;
        }
    }

    /// <summary>Declares the unmanaged DPI callback shapes retained by the export fixture.</summary>
    private sealed partial class DpiExports
    {
        /// <summary>Gets the process DPI awareness.</summary>
        /// <param name="process">The process handle.</param>
        /// <param name="awareness">The returned awareness.</param>
        /// <returns>The native operation result.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint ProcessAwarenessDelegate(IntPtr process, out DpiAwarenessValue awareness);

        /// <summary>Sets process DPI awareness.</summary>
        /// <param name="awareness">The requested awareness.</param>
        /// <returns>The native operation result.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate HResult SetAwarenessDelegate(DpiAwarenessValue awareness);

        /// <summary>Gets a monitor DPI value.</summary>
        /// <param name="monitor">The monitor handle.</param>
        /// <param name="type">The requested DPI type.</param>
        /// <param name="x">The returned horizontal DPI.</param>
        /// <param name="y">The returned vertical DPI.</param>
        /// <returns>The native operation result.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint MonitorDpiDelegate(IntPtr monitor, DpiMonitorType type, out uint x, out uint y);

        /// <summary>Tests whether a DPI context is valid.</summary>
        /// <param name="context">The DPI context.</param>
        /// <returns>The native Boolean result.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ContextDelegate(IntPtr context);

        /// <summary>Gets the current thread DPI context.</summary>
        /// <returns>The current DPI context.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr GetContextDelegate();

        /// <summary>Exchanges a thread DPI awareness context.</summary>
        /// <param name="context">The requested DPI context.</param>
        /// <returns>The preceding DPI context.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr ContextExchangeDelegate(IntPtr context);

        /// <summary>Gets awareness for a DPI context.</summary>
        /// <param name="context">The DPI context.</param>
        /// <returns>The DPI awareness.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate DpiAwarenessValue AwarenessDelegate(IntPtr context);

        /// <summary>Gets the DPI for a DPI context.</summary>
        /// <param name="context">The DPI context.</param>
        /// <returns>The DPI value.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint ContextDpiDelegate(IntPtr context);

        /// <summary>Gets a window DPI hosting behavior.</summary>
        /// <param name="window">The window handle.</param>
        /// <returns>The window DPI hosting behavior.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate DpiHostingBehaviorValue WindowHostingDelegate(IntPtr window);

        /// <summary>Sets a thread DPI hosting behavior.</summary>
        /// <param name="behavior">The requested hosting behavior.</param>
        /// <returns>The preceding hosting behavior.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate DpiHostingBehaviorValue HostingDelegate(DpiHostingBehaviorValue behavior);

        /// <summary>Gets the thread DPI hosting behavior.</summary>
        /// <returns>The thread DPI hosting behavior.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate DpiHostingBehaviorValue GetHostingDelegate();

        /// <summary>Sets dialog DPI change behavior.</summary>
        /// <param name="window">The dialog window handle.</param>
        /// <param name="mask">The behavior mask.</param>
        /// <param name="values">The behavior values.</param>
        /// <returns>The native Boolean result.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SetDialogDelegate(IntPtr window, DpiDialogScalingBehaviors mask, DpiDialogScalingBehaviors values);

        /// <summary>Gets dialog DPI change behavior.</summary>
        /// <param name="window">The dialog window handle.</param>
        /// <returns>The dialog DPI change behavior.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate DpiDialogScalingBehaviors GetDialogDelegate(IntPtr window);

        /// <summary>Converts a point for DPI scaling.</summary>
        /// <param name="window">The window handle.</param>
        /// <param name="point">The point to convert.</param>
        /// <returns>The native Boolean result.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int PointDelegate(IntPtr window, ref NativePoint point);

        /// <summary>Gets the system DPI.</summary>
        /// <returns>The system DPI.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GetSystemDpiDelegate();

        /// <summary>Gets a DPI value for a window.</summary>
        /// <param name="window">The window handle.</param>
        /// <returns>The window DPI.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GetWindowDpiDelegate(IntPtr window);

        /// <summary>Gets a DPI-scaled system metric.</summary>
        /// <param name="index">The requested metric.</param>
        /// <param name="dpi">The DPI used for scaling.</param>
        /// <returns>The metric value.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int MetricDelegate(SystemMetric index, uint dpi);

        /// <summary>Adjusts a window rectangle for DPI.</summary>
        /// <param name="rect">The rectangle to adjust.</param>
        /// <param name="style">The window style.</param>
        /// <param name="menu">The menu flag.</param>
        /// <param name="extendedStyle">The extended window style.</param>
        /// <param name="dpi">The DPI used for adjustment.</param>
        /// <returns>The native Boolean result.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int AdjustRectDelegate(ref NativeRect rect, WindowStyleFlags style, int menu, ExtendedWindowStyleFlags extendedStyle, uint dpi);

        /// <summary>Enables a DPI operation for a window.</summary>
        /// <param name="window">The window handle.</param>
        /// <returns>The native Boolean result.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int WindowResultDelegate(IntPtr window);

        /// <summary>Executes SystemParametersInfoForDpi.</summary>
        /// <param name="action">The requested action.</param>
        /// <param name="parameter">The action parameter.</param>
        /// <param name="value">The value pointer.</param>
        /// <param name="flags">The behavior flags.</param>
        /// <param name="dpi">The DPI used for the operation.</param>
        /// <returns>The native Boolean result.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ParametersDelegate(SystemParametersInfoActions action, uint parameter, IntPtr value, SystemParametersInfoBehaviors flags, uint dpi);
    }

    /// <summary>Deterministic DPI core interop probe.</summary>
    private sealed class DpiCoreInteropProbe : IDpiCoreInterop
    {
        /// <summary>Gets or sets the device capability result.</summary>
        public int DeviceCapsResult { get; set; } = DpiCalculator.DefaultScreenDpi;

        /// <summary>Gets or sets the device-context handle returned by <see cref="FromWindow"/>.</summary>
        public SafeWindowDcHandle DeviceContextHandle { get; set; }

        /// <summary>Gets or sets a value indicating whether the supplied handle is a window.</summary>
        public bool IsWindowResult { get; set; } = true;

        /// <inheritdoc />
        public int GetDeviceCaps(SafeWindowDcHandle deviceContextHandle, GdiEnums.DeviceCaps deviceCaps)
        {
            _ = deviceContextHandle;
            _ = deviceCaps;
            return DeviceCapsResult;
        }

        /// <inheritdoc />
        public SafeWindowDcHandle FromWindow(IntPtr windowHandle)
        {
            _ = windowHandle;
            return DeviceContextHandle;
        }

        /// <inheritdoc />
        public bool IsWindow(IntPtr windowHandle)
        {
            _ = windowHandle;
            return IsWindowResult;
        }

        /// <inheritdoc />
        public IntPtr MonitorFromRect(ref NativeRect rect, MonitorFrom flags)
        {
            _ = rect;
            _ = flags;
            return new(Fifteen);
        }

        /// <inheritdoc />
        public IntPtr MonitorFromWindow(IntPtr windowHandle, MonitorFrom flags)
        {
            _ = windowHandle;
            _ = flags;
            return new(Fourteen);
        }
    }

    /// <summary>Native DPI API probe for DPI API branch coverage.</summary>
    private sealed class NativeDpiApiProbe : INativeDpiApi, IDisposable
    {
        /// <inheritdoc />
        public IDisposable DefaultScopedThreadDpiAwarenessContext() => global::ReactiveUI.Primitives.Disposables.Scope.Empty;

        /// <inheritdoc />
        public HResult EnableNonClientDpiScaling(IntPtr windowHandle)
        {
            _ = windowHandle;
            return HResult.Fail;
        }

        /// <inheritdoc />
        public int GetDpi(IntPtr windowHandle)
        {
            _ = windowHandle;
            return OneHundredTwenty;
        }

        /// <inheritdoc />
        public int GetDpi(NativePoint location)
        {
            _ = location;
            return OneHundredTwenty;
        }

        /// <inheritdoc />
        public uint GetDpiForSystem() => TestDpiX;

        /// <inheritdoc />
        public int GetSystemMetricsForDpi(SystemMetric index, uint dpi)
        {
            _ = index;
            return checked((int)dpi);
        }

        /// <inheritdoc />
        public bool AdjustWindowRectExForDpi(
            ref NativeRect rect,
            WindowStyleFlags style,
            bool hasMenu,
            ExtendedWindowStyleFlags extendedStyle,
            uint dpi)
        {
            _ = rect;
            _ = style;
            _ = hasMenu;
            _ = extendedStyle;
            _ = dpi;
            return false;
        }

        /// <inheritdoc />
        public bool SystemParametersInfoForDpi(
            SystemParametersInfoActions action,
            uint uiParameter,
            IntPtr parameter,
            SystemParametersInfoBehaviors updateProfileFlags,
            uint dpi)
        {
            _ = action;
            _ = uiParameter;
            _ = parameter;
            _ = updateProfileFlags;
            _ = dpi;
            return false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
