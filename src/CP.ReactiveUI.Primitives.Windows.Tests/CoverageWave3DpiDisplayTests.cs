// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Coverage for composable DPI API wrappers.</summary>
public sealed class CoverageWave3DpiDisplayTests
{
    /// <summary>A forty-eight test value.</summary>
    private const int FortyEight = 48;

    /// <summary>A one-hundred-and-one test value.</summary>
    private const int OneHundredOne = 101;

    /// <summary>A two-hundred-and-one test value.</summary>
    private const int TwoHundredOne = 201;

    /// <summary>Verifies DPI API wrapper overloads route through the replaceable native DPI API.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DpiApi_WrapperOverloads_UseExchangeableNativeApiAsync()
    {
        var api = new NativeDpiApiProbe();
        var originalApi = NativeDpiMethods.ExchangeApi(api);
        try
        {
            var clientRect = new NativeRect(Ten, Twenty, Hundred, TwoHundred);
            const SystemMetric metric = SystemMetric.SM_CXSCREEN;
            const WindowStyleFlags style = WindowStyleFlags.WS_OVERLAPPEDWINDOW;
            const ExtendedWindowStyleFlags extendedStyle = ExtendedWindowStyleFlags.WS_EX_TOOLWINDOW;
            var windowHandle = new IntPtr(FourThousandThreeHundredTwentyOne);

            var systemMetric = DpiApi.GetSystemMetrics(metric);
            var explicitMetric = DpiApi.GetSystemMetrics(metric, UIntTwentyFour);
            var windowMetric = DpiApi.GetSystemMetricsForWindow(metric, windowHandle);
            var defaultRect = DpiApi.AdjustWindowRect(clientRect, style);
            var menuRect = DpiApi.AdjustWindowRect(clientRect, style, true);
            var extendedRect = DpiApi.AdjustWindowRect(clientRect, style, true, extendedStyle);
            var explicitRect = DpiApi.AdjustWindowRect(clientRect, style, true, extendedStyle, UIntThirtyTwo);
            var windowRect = DpiApi.AdjustWindowRectForWindow(clientRect, style, windowHandle);
            var windowMenuRect = DpiApi.AdjustWindowRectForWindow(clientRect, style, windowHandle, true);
            var windowExtendedRect = DpiApi.AdjustWindowRectForWindow(clientRect, style, windowHandle, true, extendedStyle);

            api.AdjustWindowRectSucceeds = false;
            var failedRect = DpiApi.AdjustWindowRect(clientRect, style, true, extendedStyle, UIntFortyTwo);

            await Assert.That(systemMetric).IsEqualTo(OneHundredFiftySix);
            await Assert.That(explicitMetric).IsEqualTo(FortyEight);
            await Assert.That(windowMetric).IsEqualTo(OneHundredNinetyTwo);
            await Assert.That(defaultRect).IsEqualTo(new NativeRect(Eleven, TwentyOne, OneHundredOne, TwoHundredOne));
            await Assert.That(menuRect).IsEqualTo(new NativeRect(Eleven, TwentyOne, OneHundredOne, TwoHundredOne));
            await Assert.That(extendedRect).IsEqualTo(new NativeRect(Eleven, TwentyOne, OneHundredOne, TwoHundredOne));
            await Assert.That(explicitRect).IsEqualTo(new NativeRect(Eleven, TwentyOne, OneHundredOne, TwoHundredOne));
            await Assert.That(windowRect).IsEqualTo(new NativeRect(Eleven, TwentyOne, OneHundredOne, TwoHundredOne));
            await Assert.That(windowMenuRect).IsEqualTo(new NativeRect(Eleven, TwentyOne, OneHundredOne, TwoHundredOne));
            await Assert.That(windowExtendedRect).IsEqualTo(new NativeRect(Eleven, TwentyOne, OneHundredOne, TwoHundredOne));
            await Assert.That(failedRect).IsNull();
            await Assert.That(api.SystemDpiCalls).IsEqualTo(Four);
            await Assert.That(api.WindowDpiCalls).IsEqualTo(Four);
            await Assert.That(api.LastWindowHandleValue).IsEqualTo(windowHandle.ToInt64());
            await Assert.That(api.LastAdjustedDpi).IsEqualTo(UIntFortyTwo);
        }
        finally
        {
            _ = NativeDpiMethods.ExchangeApi(originalApi);
        }
    }

    /// <summary>Verifies system-parameter DPI wrappers marshal success, failure, and window-DPI paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DpiApi_SystemParametersInfo_WritesValuesAndDefaultsOnFailureAsync()
    {
        var api = new NativeDpiApiProbe { SystemParameterValue = new(Seven, Nine) };
        var originalApi = NativeDpiMethods.ExchangeApi(api);
        try
        {
            var windowHandle = new IntPtr(OneThousandTwoHundredThirtyFour);
            var systemSuccess = DpiApi.TryGetSystemParametersInfo<NativeSize>(
                SystemParametersInfoActions.SPI_GETWORKAREA,
                out var systemValue);
            var explicitSuccess = DpiApi.TryGetSystemParametersInfo<NativeSize>(
                SystemParametersInfoActions.SPI_GETWORKAREA,
                UIntThirty,
                out var explicitValue);
            var windowSuccess = DpiApi.TryGetSystemParametersInfoForWindow<NativeSize>(
                SystemParametersInfoActions.SPI_GETWORKAREA,
                windowHandle,
                out var windowValue);

            api.SystemParametersInfoSucceeds = false;
            var failure = DpiApi.TryGetSystemParametersInfo<NativeSize>(
                SystemParametersInfoActions.SPI_GETWORKAREA,
                UIntThirtyTwo,
                out var failedValue);

            await Assert.That(systemSuccess).IsTrue();
            await Assert.That(explicitSuccess).IsTrue();
            await Assert.That(windowSuccess).IsTrue();
            await Assert.That(failure).IsFalse();
            await Assert.That(systemValue).IsEqualTo(new(Seven, Nine));
            await Assert.That(explicitValue).IsEqualTo(new(Seven, Nine));
            await Assert.That(windowValue).IsEqualTo(new(Seven, Nine));
            await Assert.That(failedValue).IsEqualTo(default(NativeSize));
            await Assert.That(api.SystemParametersInfoCalls).IsEqualTo(Four);
            await Assert.That(api.LastSystemParametersDpi).IsEqualTo(UIntThirtyTwo);
            await Assert.That(api.WindowDpiCalls).IsEqualTo(One);
        }
        finally
        {
            _ = NativeDpiMethods.ExchangeApi(originalApi);
        }
    }

    /// <summary>Verifies native DPI API exchange rejects null replacements and returns the previous API.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeDpiMethods_ExchangeApi_RejectsNullAndReturnsPreviousApiAsync()
    {
        var firstApi = new NativeDpiApiProbe();
        var secondApi = new NativeDpiApiProbe();
        var originalApi = NativeDpiMethods.ExchangeApi(firstApi);
        try
        {
            var returnedApi = NativeDpiMethods.ExchangeApi(secondApi);

            await Assert.That(returnedApi).IsSameReferenceAs(firstApi);
            await Assert.That(static () => NativeDpiMethods.ExchangeApi(null)).Throws<ArgumentNullException>();
        }
        finally
        {
            _ = NativeDpiMethods.ExchangeApi(originalApi);
        }
    }

    /// <summary>Native DPI API probe for deterministic wrapper tests.</summary>
    private sealed class NativeDpiApiProbe : INativeDpiApi
    {
        /// <summary>A 132 DPI test value.</summary>
        private const uint UIntOneHundredThirtyTwo = 132U;

        /// <summary>The last window handle requested.</summary>
        private IntPtr _lastWindowHandle;

        /// <summary>Gets or sets a value indicating whether rectangle adjustment succeeds.</summary>
        public bool AdjustWindowRectSucceeds { get; set; } = true;

        /// <summary>Gets or sets a value indicating whether system-parameter retrieval succeeds.</summary>
        public bool SystemParametersInfoSucceeds { get; set; } = true;

        /// <summary>Gets or sets the system-parameter value to write.</summary>
        public NativeSize SystemParameterValue { get; set; }

        /// <summary>Gets the number of system-DPI calls.</summary>
        public int SystemDpiCalls { get; private set; }

        /// <summary>Gets the number of window-DPI calls.</summary>
        public int WindowDpiCalls { get; private set; }

        /// <summary>Gets the number of system-parameter calls.</summary>
        public int SystemParametersInfoCalls { get; private set; }

        /// <summary>Gets the last window handle value requested.</summary>
        public long LastWindowHandleValue => _lastWindowHandle.ToInt64();

        /// <summary>Gets the last adjusted DPI value.</summary>
        public uint LastAdjustedDpi { get; private set; }

        /// <summary>Gets the last system-parameter DPI value.</summary>
        public uint LastSystemParametersDpi { get; private set; }

        /// <inheritdoc />
        public IDisposable DefaultScopedThreadDpiAwarenessContext() => NoopDisposable.Instance;

        /// <inheritdoc />
        public HResult EnableNonClientDpiScaling(IntPtr windowHandle) => HResult.Ok;

        /// <inheritdoc />
        public int GetDpi(IntPtr windowHandle)
        {
            WindowDpiCalls++;
            _lastWindowHandle = windowHandle;
            return OneHundredSixtyEight;
        }

        /// <inheritdoc />
        public int GetDpi(NativePoint location) => OneHundredTwenty;

        /// <inheritdoc />
        public uint GetDpiForSystem()
        {
            SystemDpiCalls++;
            return UIntOneHundredThirtyTwo;
        }

        /// <inheritdoc />
        public int GetSystemMetricsForDpi(SystemMetric index, uint dpi) => (int)dpi + TwentyFour;

        /// <inheritdoc />
        public bool AdjustWindowRectExForDpi(
            ref NativeRect rect,
            WindowStyleFlags style,
            bool hasMenu,
            ExtendedWindowStyleFlags extendedStyle,
            uint dpi)
        {
            LastAdjustedDpi = dpi;
            if (!AdjustWindowRectSucceeds)
            {
                return false;
            }

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
            SystemParametersInfoCalls++;
            LastSystemParametersDpi = dpi;
            if (!SystemParametersInfoSucceeds)
            {
                return false;
            }

            Marshal.StructureToPtr(SystemParameterValue, parameter, false);
            return true;
        }
    }

    /// <summary>No-op disposable for native DPI API probes.</summary>
    private sealed class NoopDisposable : IDisposable
    {
        /// <summary>Gets the singleton no-op disposable instance.</summary>
        public static NoopDisposable Instance { get; } = new();

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
