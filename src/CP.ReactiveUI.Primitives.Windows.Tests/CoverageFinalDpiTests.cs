// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Controls;
using System.Windows.Media;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Final focused coverage for DPI native wrappers and WPF layout helpers.</summary>
public sealed class CoverageFinalDpiTests
{
    /// <summary>Verifies the concrete Windows native DPI API delegates every member to exchangeable operations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsNativeDpiApi_DelegatesEveryMember_ToExchangeableOperationsAsync()
    {
        using var operations = new NativeDpiOperationsProbe();
        var originalOperations = WindowsNativeDpiApi.ExchangeOperations(operations);
        try
        {
            var api = WindowsNativeDpiApi.Instance;
            var rect = new NativeRect(Ten, Twenty, Hundred, TwoHundred);
            using var scope = api.DefaultScopedThreadDpiAwarenessContext();
            var nonClientResult = api.EnableNonClientDpiScaling(new(Two));
            var windowDpi = api.GetDpi(new IntPtr(Three));
            var pointDpi = api.GetDpi(new NativePoint(Four, Five));
            var systemDpi = api.GetDpiForSystem();
            var metric = api.GetSystemMetricsForDpi(SystemMetric.SM_CXSCREEN, UIntThirtyTwo);
            var adjusted = api.AdjustWindowRectExForDpi(
                ref rect,
                WindowStyleFlags.WS_OVERLAPPEDWINDOW,
                true,
                ExtendedWindowStyleFlags.WS_EX_TOOLWINDOW,
                UIntFortyTwo);
            var systemParameters = api.SystemParametersInfoForDpi(
                SystemParametersInfoActions.SPI_GETWORKAREA,
                UIntFour,
                new(One),
                SystemParametersInfoBehaviors.None,
                UIntNinetyNine);

            await Assert.That(scope).IsSameReferenceAs(operations.Scope);
            await Assert.That(nonClientResult).IsEqualTo(HResult.Ok);
            await Assert.That(windowDpi).IsEqualTo(OneHundredTwenty);
            await Assert.That(pointDpi).IsEqualTo(OneHundredFiftySix);
            await Assert.That(systemDpi).IsEqualTo(UIntNinetyNine);
            await Assert.That(metric).IsEqualTo(OneHundredTwentyEight);
            await Assert.That(adjusted).IsTrue();
            await Assert.That(systemParameters).IsTrue();
            await Assert.That(rect).IsEqualTo(new(Eleven, TwentyOne, OneHundredTwentyFive, TwoHundredEighty));
            await Assert.That(operations.LastHasMenu).IsTrue();
            await Assert.That(operations.Calls).IsEqualTo(Eight);
        }
        finally
        {
            _ = WindowsNativeDpiApi.ExchangeOperations(originalOperations);
        }
    }

    /// <summary>Verifies operation exchange validation and restoration semantics.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsNativeDpiApi_ExchangeOperations_ReturnsPreviousAndRejectsNullAsync()
    {
        using var firstOperations = new NativeDpiOperationsProbe();
        using var secondOperations = new NativeDpiOperationsProbe();
        var originalOperations = WindowsNativeDpiApi.ExchangeOperations(firstOperations);
        try
        {
            var returnedOperations = WindowsNativeDpiApi.ExchangeOperations(secondOperations);

            await Assert.That(returnedOperations).IsSameReferenceAs(firstOperations);
            await Assert.That(static () => WindowsNativeDpiApi.ExchangeOperations(null)).Throws<ArgumentNullException>();
        }
        finally
        {
            _ = WindowsNativeDpiApi.ExchangeOperations(originalOperations);
        }
    }

    /// <summary>Verifies native DPI methods route through the replaceable API for deterministic wrapper coverage.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeDpiMethods_ExchangeableApi_CoversForwardingWrappersAsync()
    {
        using var api = new NativeDpiApiProbe();
        var originalApi = NativeDpiMethods.ExchangeApi(api);
        try
        {
            var rect = new NativeRect(One, Two, Three, Four);
            using var scope = NativeDpiMethods.DefaultScopedThreadDpiAwarenessContext();
            var windowDpi = NativeDpiMethods.GetDpi(new IntPtr(Five));
            var locationDpi = NativeDpiMethods.GetDpi(new NativePoint(Six, Seven));
            var nonClient = NativeDpiMethods.EnableNonClientDpiScaling(new(Eight));
            var systemDpi = NativeDpiMethods.GetDpiForSystem();
            var metric = NativeDpiMethods.GetSystemMetricsForDpi(SystemMetric.SM_CYSCREEN, UIntTwentyFour);
            var adjusted = NativeDpiMethods.AdjustWindowRectExForDpi(
                ref rect,
                WindowStyleFlags.WS_OVERLAPPEDWINDOW,
                false,
                ExtendedWindowStyleFlags.None,
                UIntThirtyTwo);
            var parameters = NativeDpiMethods.SystemParametersInfoForDpi(
                SystemParametersInfoActions.SPI_GETWORKAREA,
                UIntFour,
                IntPtr.Zero,
                SystemParametersInfoBehaviors.None,
                UIntFortyTwo);

            await Assert.That(scope).IsSameReferenceAs(api.Scope);
            await Assert.That(windowDpi).IsEqualTo(OneHundredTwenty);
            await Assert.That(locationDpi).IsEqualTo(OneHundredFiftySix);
            await Assert.That(nonClient).IsEqualTo(HResult.Ok);
            await Assert.That(systemDpi).IsEqualTo(UIntNinetyNine);
            await Assert.That(metric).IsEqualTo(OneHundredTwentyFive);
            await Assert.That(adjusted).IsTrue();
            await Assert.That(parameters).IsTrue();
            await Assert.That(rect).IsEqualTo(new(Two, Three, Four, Five));
            await Assert.That(api.Calls).IsEqualTo(Eight);
        }
        finally
        {
            _ = NativeDpiMethods.ExchangeApi(originalApi);
        }
    }

    /// <summary>Verifies DPI handler branches for create, paint, set-icon workaround, callback overload, and destroy.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DpiHandler_MessageBranches_PublishCompleteAndDisposeScopeAsync()
    {
        using var api = new NativeDpiApiProbe { WindowDpi = OneHundredTwenty };
        var originalApi = NativeDpiMethods.ExchangeApi(api);
        try
        {
            using var handler = new DpiHandler(needsListenerWorkaround: true);
            var changes = new List<DpiChangeInfo>();
            var completed = false;
            using var subscription = handler.ObserveDpiChanges().Subscribe(changes.Add, () => completed = true);

            var ignored = handler.HandleWindowMessages(WindowMessageInfo.Create(0, (int)WindowsMessages.WM_DPICHANGED_BEFOREPARENT, 0, 0));
            var created = handler.HandleWindowMessages(WindowMessageInfo.Create(Ten, (int)WindowsMessages.WM_CREATE, 0, 0));
            api.WindowDpi = OneHundredFiftySix;
            var setIcon = handler.HandleWindowMessages(WindowMessageInfo.Create(Ten, (int)WindowsMessages.WM_SETICON, 0, 0));
            api.WindowDpi = OneHundredNinetyTwo;
            var painted = handler.HandleWindowMessages(WindowMessageInfo.Create(Ten, (int)WindowsMessages.WM_PAINT, 0, 0));
            var handled = false;
            var callbackResult = handler.HandleWindowMessages(
                new(Ten),
                (int)WindowsMessages.WM_SHOWWINDOW,
                IntPtr.Zero,
                IntPtr.Zero,
                ref handled);
            var destroyed = handler.HandleWindowMessages(WindowMessageInfo.Create(Ten, (int)WindowsMessages.WM_DESTROY, 0, 0));

            await Assert.That(ignored).IsFalse();
            await Assert.That(created).IsFalse();
            await Assert.That(setIcon).IsFalse();
            await Assert.That(painted).IsFalse();
            await Assert.That(callbackResult).IsEqualTo(IntPtr.Zero);
            await Assert.That(handled).IsFalse();
            await Assert.That(destroyed).IsFalse();
            await Assert.That(api.Scope.IsDisposed).IsTrue();
            await Assert.That(completed).IsTrue();
            await Assert.That(changes.Count).IsEqualTo(Two);
            await Assert.That(changes[0].NewDpi).IsEqualTo(OneHundredTwenty);
            await Assert.That(changes[1].NewDpi).IsEqualTo(OneHundredFiftySix);
        }
        finally
        {
            _ = NativeDpiMethods.ExchangeApi(originalApi);
        }
    }

    /// <summary>Verifies WPF DPI layout transform updates scale and reset child transforms.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowDpiExtensions_UpdateLayoutTransform_SetsAndClearsChildScaleAsync()
    {
        var grid = new Grid();
        var child = new Border();
        _ = grid.Children.Add(child);

        grid.UpdateLayoutTransform(OneAndHalfDouble);
        var scaledTransform = child.LayoutTransform as ScaleTransform;

        grid.UpdateLayoutTransform((double)DpiCalculator.DefaultScreenDpi / DpiCalculator.DefaultScreenDpi);

        await Assert.That(scaledTransform).IsNotNull();
        await Assert.That(scaledTransform.ScaleX).IsEqualTo(OneAndHalfDouble);
        await Assert.That(scaledTransform.ScaleY).IsEqualTo(OneAndHalfDouble);
        await Assert.That(child.LayoutTransform).IsNull();
    }

    /// <summary>Verifies bitmap scale handler factories and null-value branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapScaleHandler_CoversFactoriesTargetsAndNullBranchesAsync()
    {
        using var api = new NativeDpiApiProbe { WindowDpi = OneHundredTwenty };
        var originalApi = NativeDpiMethods.ExchangeApi(api);
        try
        {
            using var dpiHandler = new DpiHandler();
            using var resourceHandler = BitmapScaleHandler.WithComponentResourceManager<Bitmap>(
                dpiHandler,
                typeof(CoverageFinalDpiTests),
                static (bitmap, _) => bitmap);
            Bitmap missingResource = null;
            await Assert.That(() =>
                _ = resourceHandler.AddApplyAction(bitmap => missingResource = bitmap, "MissingBitmapResource", true))
                .Throws<System.Resources.MissingManifestResourceException>();

            DisposableProbe uninitializedValue = new();
            var uninitializedHandler = new BitmapScaleHandler<int, DisposableProbe>();
            _ = uninitializedHandler.AddApplyAction(value => uninitializedValue = value, One, true);

            using var nullProviderHandler = BitmapScaleHandler.Create<int, DisposableProbe>(
                dpiHandler,
                static (_, _) => null);
            DisposableProbe nullProvidedValue = new();
            _ = nullProviderHandler.AddApplyAction(value => nullProvidedValue = value, Two, true);

            using var nullScalerHandler = BitmapScaleHandler.Create<int, DisposableProbe>(
                dpiHandler,
                static (_, _) => new(),
                static (value, _) =>
                {
                    value.Dispose();
                    return null;
                });
            DisposableProbe nullScaledValue = new();
            _ = nullScalerHandler.AddApplyAction(value => nullScaledValue = value, Three, true);

            await Assert.That(missingResource).IsNull();
            await Assert.That(uninitializedValue).IsNull();
            await Assert.That(nullProvidedValue).IsNull();
            await Assert.That(nullScaledValue).IsNull();
            uninitializedHandler.Dispose();
        }
        finally
        {
            _ = NativeDpiMethods.ExchangeApi(originalApi);
        }
    }

    /// <summary>Verifies bitmap scale handler cache invalidation and target overloads.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapScaleHandler_CoversCacheInvalidationAndTargetsAsync()
    {
        using var api = new NativeDpiApiProbe { WindowDpi = OneHundredTwenty };
        var originalApi = NativeDpiMethods.ExchangeApi(api);
        try
        {
            using var dpiHandler = new DpiHandler();
            var providerCalls = Zero;
            var scalerCalls = Zero;
            using var cachedHandler = BitmapScaleHandler.Create<int, DisposableProbe>(
                dpiHandler,
                (_, _) =>
                {
                    providerCalls++;
                    return new();
                },
                (value, _) =>
                {
                    scalerCalls++;
                    return value;
                });
            DisposableProbe firstAppliedValue = null;
            DisposableProbe secondAppliedValue = null;
            _ = cachedHandler.AddApplyAction(value => firstAppliedValue = value, Four, true);
            _ = cachedHandler.AddApplyAction(value => secondAppliedValue = value, Four, true);
            var cachedBeforeDpiChange = firstAppliedValue;

            using var button = new System.Windows.Forms.Button();
            using var toolStripButton = new System.Windows.Forms.ToolStripButton();
            using var bitmapHandler = BitmapScaleHandler.Create<int, Bitmap>(
                dpiHandler,
                static (_, _) => new(One, One),
                static (bitmap, _) => bitmap);
            _ = bitmapHandler.AddTarget(button, Five, static bitmap => bitmap);
            _ = bitmapHandler.AddTarget(button, Five, static bitmap => bitmap, true);
            _ = bitmapHandler.AddTarget(toolStripButton, Six, static bitmap => bitmap);
            _ = bitmapHandler.AddTarget(toolStripButton, Six, static bitmap => bitmap, true);

            api.WindowDpi = OneHundredFiftySix;
            _ = dpiHandler.HandleWindowMessages(WindowMessageInfo.Create(Ten, (int)WindowsMessages.WM_CREATE, 0, 0));

            await Assert.That(firstAppliedValue).IsSameReferenceAs(secondAppliedValue);
            await Assert.That(providerCalls).IsEqualTo(Two);
            await Assert.That(scalerCalls).IsEqualTo(Two);
            await Assert.That(cachedBeforeDpiChange.IsDisposed).IsTrue();
            await Assert.That(button.Image).IsNotNull();
            await Assert.That(toolStripButton.Image).IsNotNull();

            button.Image = null;
            toolStripButton.Image = null;
        }
        finally
        {
            _ = NativeDpiMethods.ExchangeApi(originalApi);
        }
    }

    /// <summary>Verifies WPF DPI handler attachment through injectable messages and layout callbacks.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowDpiExtensions_AttachDpiHandlerCore_UsesMessagesAndLayoutCallbackAsync()
    {
        using var api = new NativeDpiApiProbe { WindowDpi = OneHundredNinetyTwo };
        var originalApi = NativeDpiMethods.ExchangeApi(api);
        try
        {
            using var dpiHandler = new DpiHandler();
            var window = new System.Windows.Window();
            var scales = new List<double>();
            var messages = new ImmediateWindowMessages(
                WindowMessageInfo.Create(Ten, (int)WindowsMessages.WM_CREATE, 0, 0),
                WindowMessageInfo.Create(Ten, (int)WindowsMessages.WM_NCCREATE, 0, 0),
                WindowMessageInfo.Create(Ten, (int)WindowsMessages.WM_DESTROY, 0, 0));

            WindowDpiExtensions.AttachDpiHandlerCore(
                window,
                dpiHandler,
                messages,
                (_, scaleFactor) => scales.Add(scaleFactor));

            window.Close();

            await Assert.That(scales.Count).IsEqualTo(Two);
            await Assert.That(scales[0]).IsEqualTo((double)Two);
            await Assert.That(scales[1]).IsEqualTo((double)Two);
        }
        finally
        {
            _ = NativeDpiMethods.ExchangeApi(originalApi);
        }
    }

    /// <summary>Native DPI API probe for forwarding tests.</summary>
    private sealed class NativeDpiApiProbe : INativeDpiApi, IDisposable
    {
        /// <summary>A one-hundred-and-one test value.</summary>
        private const int OneHundredOne = 101;

        /// <summary>Gets the disposable scope returned by the probe.</summary>
        public DisposableProbe Scope { get; } = new();

        /// <summary>Gets or sets the returned window DPI.</summary>
        public int WindowDpi { get; set; } = OneHundredTwenty;

        /// <summary>Gets the number of method calls.</summary>
        public int Calls { get; private set; }

        /// <inheritdoc />
        public IDisposable DefaultScopedThreadDpiAwarenessContext()
        {
            Calls++;
            return Scope;
        }

        /// <inheritdoc />
        public HResult EnableNonClientDpiScaling(IntPtr windowHandle)
        {
            Calls++;
            return HResult.Ok;
        }

        /// <inheritdoc />
        public int GetDpi(IntPtr windowHandle)
        {
            Calls++;
            return WindowDpi;
        }

        /// <inheritdoc />
        public int GetDpi(NativePoint location)
        {
            Calls++;
            return OneHundredFiftySix;
        }

        /// <inheritdoc />
        public uint GetDpiForSystem()
        {
            Calls++;
            return UIntNinetyNine;
        }

        /// <inheritdoc />
        public int GetSystemMetricsForDpi(SystemMetric index, uint dpi)
        {
            Calls++;
            return (int)dpi + OneHundredOne;
        }

        /// <inheritdoc />
        public bool AdjustWindowRectExForDpi(
            ref NativeRect rect,
            WindowStyleFlags style,
            bool hasMenu,
            ExtendedWindowStyleFlags extendedStyle,
            uint dpi)
        {
            Calls++;
            rect = new(rect.Left + One, rect.Top + One, rect.Width + One, rect.Height + One);
            return true;
        }

        /// <inheritdoc />
        public bool SystemParametersInfoForDpi(
            SystemParametersInfoActions action,
            uint uiParameter,
            IntPtr parameter,
            SystemParametersInfoBehaviors updateProfileFlags,
            uint dpi)
        {
            Calls++;
            return true;
        }

        /// <inheritdoc />
        public void Dispose() => Scope.Dispose();
    }

    /// <summary>Native operations probe for concrete Windows native DPI API tests.</summary>
    private sealed class NativeDpiOperationsProbe : INativeDpiOperations, IDisposable
    {
        /// <summary>An eighty test value.</summary>
        private const int Eighty = 80;

        /// <summary>A ninety-six test value.</summary>
        private const int NinetySix = 96;

        /// <summary>Gets the disposable scope returned by the probe.</summary>
        public DisposableProbe Scope { get; } = new();

        /// <summary>Gets the number of method calls.</summary>
        public int Calls { get; private set; }

        /// <summary>Gets a value indicating whether the last adjustment requested a menu.</summary>
        public bool LastHasMenu { get; private set; }

        /// <inheritdoc />
        public IDisposable DefaultScopedThreadDpiAwarenessContext()
        {
            Calls++;
            return Scope;
        }

        /// <inheritdoc />
        public HResult EnableNonClientDpiScaling(IntPtr windowHandle)
        {
            Calls++;
            return HResult.Ok;
        }

        /// <inheritdoc />
        public int GetDpi(IntPtr windowHandle)
        {
            Calls++;
            return OneHundredTwenty;
        }

        /// <inheritdoc />
        public int GetDpi(NativePoint location)
        {
            Calls++;
            return OneHundredFiftySix;
        }

        /// <inheritdoc />
        public uint GetDpiForSystem()
        {
            Calls++;
            return UIntNinetyNine;
        }

        /// <inheritdoc />
        public int GetSystemMetricsForDpi(SystemMetric index, uint dpi)
        {
            Calls++;
            return (int)dpi + NinetySix;
        }

        /// <inheritdoc />
        public bool AdjustWindowRectExForDpi(
            ref NativeRect rect,
            WindowStyleFlags style,
            bool hasMenu,
            ExtendedWindowStyleFlags extendedStyle,
            uint dpi)
        {
            Calls++;
            LastHasMenu = hasMenu;
            rect = new(rect.Left + One, rect.Top + One, rect.Width + TwentyFive, rect.Height + Eighty);
            return true;
        }

        /// <inheritdoc />
        public bool SystemParametersInfoForDpi(
            SystemParametersInfoActions action,
            uint uiParameter,
            IntPtr parameter,
            SystemParametersInfoBehaviors updateProfileFlags,
            uint dpi)
        {
            Calls++;
            return parameter != IntPtr.Zero;
        }

        /// <inheritdoc />
        public void Dispose() => Scope.Dispose();
    }

    /// <summary>Publishes a fixed sequence of window messages immediately to each subscriber.</summary>
    /// <param name="messages">The messages to publish.</param>
    private sealed class ImmediateWindowMessages(params WindowMessageInfo[] messages) : IObservable<WindowMessageInfo>
    {
        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<WindowMessageInfo> observer)
        {
            foreach (var message in messages)
            {
                observer.OnNext(message);
            }

            observer.OnCompleted();
            return new DisposableProbe();
        }
    }

    /// <summary>Tracks disposal from DPI handler paths.</summary>
    private sealed class DisposableProbe : IDisposable
    {
        /// <summary>Gets a value indicating whether this instance has been disposed.</summary>
        public bool IsDisposed { get; private set; }

        /// <inheritdoc />
        public void Dispose() => IsDisposed = true;
    }
}
