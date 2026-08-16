// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Dpi Tests behavior.</summary>
public class DpiTests
{
    /// <summary>Defines the TestValue120 test value.</summary>
    private const int TestValue120 = 120;

    /// <summary>Defines the TestValue144 test value.</summary>
    private const int TestValue144 = 144;

    /// <summary>Defines the TestValue192 test value.</summary>
    private const int TestValue192 = 192;

    /// <summary>Defines the TestValue800 test value.</summary>
    private const int TestValue800 = 800;

    /// <summary>Defines the TestValue600 test value.</summary>
    private const int TestValue600 = 600;

    /// <summary>Defines the TestValue16 test value.</summary>
    private const int TestValue16 = 16;

    /// <summary>Defines the TestValue96 test value.</summary>
    private const int TestValue96 = 96;

    /// <summary>Defines the TestValue20 test value.</summary>
    private const int TestValue20 = 20;

    /// <summary>Defines the TestValue24 test value.</summary>
    private const int TestValue24 = 24;

    /// <summary>Defines the TestValue32 test value.</summary>
    private const int TestValue32 = 32;

    /// <summary>Defines the TestValue12 test value.</summary>
    private const int TestValue12 = 12;

    /// <summary>Defines the TestValue10 test value.</summary>
    private const int TestValue10 = 10;

    /// <summary>Defines the TestValue8 test value.</summary>
    private const int TestValue8 = 8;

    /// <summary>Defines the TestValue1 test value.</summary>
    private const int TestValue1 = 1;

    /// <summary>Defines the TestValue2 test value.</summary>
    private const int TestValue2 = 2;

    /// <summary>Defines the TestValue1Point5 test value.</summary>
    private const float TestValue1Point5 = 1.5F;

    /// <summary>Defines the TestValue2Point25 test value.</summary>
    private const float TestValue2Point25 = 2.25F;

    /// <summary>Defines the TestValue2Point5 test value.</summary>
    private const float TestValue2Point5 = 2.5F;

    /// <summary>Defines the TestValue3Point75 test value.</summary>
    private const float TestValue3Point75 = 3.75F;

    /// <summary>Test ScaleWithDpi.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_ScaleWithDpiAsync()
    {
        var size96 = DpiCalculator.ScaleWithDpi(TestValue16, TestValue96);
        await Assert.That(size96).IsEqualTo(TestValue16);
        var size120 = DpiCalculator.ScaleWithDpi(TestValue16, TestValue120);
        await Assert.That(size120).IsEqualTo(TestValue20);
        var size144 = DpiCalculator.ScaleWithDpi(TestValue16, TestValue144);
        await Assert.That(size144).IsEqualTo(TestValue24);
        var size192 = DpiCalculator.ScaleWithDpi(TestValue16, TestValue192);
        await Assert.That(size192).IsEqualTo(TestValue32);
    }

    /// <summary>Test ScaleWithDpi with floating-point values.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_ScaleWithDpi_NativePointFloatAsync()
    {
        var point = DpiCalculator.ScaleWithDpi(new NativePointFloat(TestValue1Point5, TestValue2Point5), TestValue144);

        await Assert.That(point.X).IsEqualTo(TestValue2Point25);
        await Assert.That(point.Y).IsEqualTo(TestValue3Point75);
    }

    /// <summary>Test ScaleWithDpi applies a modifier once for two-dimensional values.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_ScaleWithDpi_AppliesScaleModifierOnceAsync()
    {
        var modifierCalls = 0;
        var size = DpiCalculator.ScaleWithDpi(new NativeSizeFloat(TestValue16, TestValue16), TestValue96, scale =>
        {
            modifierCalls++;
            return scale * TestValue2;
        });

        await Assert.That(size).IsEqualTo(new(TestValue32, TestValue32));
        await Assert.That(modifierCalls).IsEqualTo(TestValue1);
    }

    /// <summary>Test UnscaleWithDpi.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_UnscaleWithDpiAsync()
    {
        var size96 = DpiCalculator.UnscaleWithDpi(TestValue16, TestValue96);
        await Assert.That(size96).IsEqualTo(TestValue16);
        var size120 = DpiCalculator.UnscaleWithDpi(TestValue16, TestValue120);
        await Assert.That(size120).IsEqualTo(TestValue12);
        var size144 = DpiCalculator.UnscaleWithDpi(TestValue16, TestValue144);
        await Assert.That(size144).IsEqualTo(TestValue10);
        var size192 = DpiCalculator.UnscaleWithDpi(TestValue16, TestValue192);
        await Assert.That(size192).IsEqualTo(TestValue8);
    }

    /// <summary>Test scale -> unscale.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_ScaleWithDpi_UnscaleWithDpiAsync()
    {
        var testSize = new NativeSize(TestValue16, TestValue16);
        var size96 = DpiCalculator.ScaleWithDpi(testSize, TestValue96);
        var resultSize96 = DpiCalculator.UnscaleWithDpi(size96, TestValue96);
        await Assert.That(resultSize96).IsEqualTo(testSize);

        var size120 = DpiCalculator.ScaleWithDpi(testSize, TestValue120);
        var resultSize120 = DpiCalculator.UnscaleWithDpi(size120, TestValue120);
        await Assert.That(resultSize120).IsEqualTo(testSize);

        var size144 = DpiCalculator.ScaleWithDpi(testSize, TestValue144);
        var resultSize144 = DpiCalculator.UnscaleWithDpi(size144, TestValue144);
        await Assert.That(resultSize144).IsEqualTo(testSize);
    }

    /// <summary>Test GetSystemMetrics.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_GetSystemMetricsAsync()
    {
        // Test with default DPI
        var screenWidth = DpiApi.GetSystemMetrics(User32.Enums.SystemMetric.SM_CXSCREEN);
        await Assert.That(screenWidth > 0).IsTrue();

        // Test with specific DPI values
        var screenWidth96 = DpiApi.GetSystemMetrics(User32.Enums.SystemMetric.SM_CXSCREEN, TestValue96);
        await Assert.That(screenWidth96 > 0).IsTrue();

        var screenWidth144 = DpiApi.GetSystemMetrics(User32.Enums.SystemMetric.SM_CXSCREEN, TestValue144);
        await Assert.That(screenWidth144 > 0).IsTrue();

        // Higher DPI should generally result in larger values for most metrics
        // Note: This may not always be true depending on the system configuration
    }

    /// <summary>Test AdjustWindowRect.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_AdjustWindowRectAsync()
    {
        // Create a client rectangle
        var clientRect = new NativeRect(0, 0, TestValue800, TestValue600);

        // Adjust for a standard overlapped window with caption and sizing border
        var windowRect = DpiApi.AdjustWindowRect(
            clientRect,
            User32.Enums.WindowStyleFlags.WS_OVERLAPPEDWINDOW,
            hasMenu: false,
            User32.Enums.ExtendedWindowStyleFlags.None,
            dpi: TestValue96);

        await Assert.That(windowRect).IsNotNull();

        // The window rect should be larger than the client rect to account for borders and title bar
        await Assert.That(windowRect.Value.Width >= clientRect.Width).IsTrue();
        await Assert.That(windowRect.Value.Height >= clientRect.Height).IsTrue();
    }

    /// <summary>Test AdjustWindowRect with different DPI values.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_AdjustWindowRect_DpiScalingAsync()
    {
        var clientRect = new NativeRect(0, 0, TestValue800, TestValue600);

        var windowRect96 = DpiApi.AdjustWindowRect(
            clientRect,
            User32.Enums.WindowStyleFlags.WS_OVERLAPPEDWINDOW,
            hasMenu: false,
            User32.Enums.ExtendedWindowStyleFlags.None,
            dpi: TestValue96);

        var windowRect144 = DpiApi.AdjustWindowRect(
            clientRect,
            User32.Enums.WindowStyleFlags.WS_OVERLAPPEDWINDOW,
            hasMenu: false,
            User32.Enums.ExtendedWindowStyleFlags.None,
            dpi: TestValue144);

        await Assert.That(windowRect96).IsNotNull();
        await Assert.That(windowRect144).IsNotNull();

        // The border size should scale with DPI
        var border96 = windowRect96.Value.Width - clientRect.Width;
        var border144 = windowRect144.Value.Width - clientRect.Width;

        // At TestValue144 DPI (150%), borders should be larger than at TestValue96 DPI (100%)
        await Assert.That(border144 >= border96).IsTrue();
    }

    /// <summary>Test DpiHandler exposes current state and observable changes.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_DpiHandler_ObservableSurfaceAsync()
    {
        using var handler = new DpiHandler();
        await Assert.That(handler.CurrentDpi).IsEqualTo(0);
        await Assert.That(handler.ObserveDpiChanges()).IsNotNull();
    }

    /// <summary>Test composable DPI-aware form behavior can attach to a plain form.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_DpiAwareFormBehavior_AttachesToPlainFormAsync()
    {
        using var form = new Form();
        using var behavior = form.AttachDpiAwareBehavior();

        await Assert.That(form.IsHandleCreated).IsTrue();
        await Assert.That(behavior.DpiHandler).IsNotNull();
        await Assert.That(behavior.DpiHandler.CurrentDpi > 0).IsTrue();
    }

    /// <summary>Test composable DPI-unaware form behavior can attach to a plain form.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_DpiUnawareFormBehavior_AttachesToPlainFormAsync()
    {
        using var form = new Form();
        using var behavior = form.AttachDpiUnawareBehavior();

        await Assert.That(form.IsHandleCreated).IsTrue();
        await Assert.That(behavior).IsNotNull();
    }

    /// <summary>Test inherited DPI form types are no longer part of the public surface.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_DpiFormInheritanceTypesRemovedAsync()
    {
        var assembly = typeof(DpiCalculator).Assembly;

        await Assert.That(assembly.GetType("CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Forms.DpiAwareForm")).IsNull();
        await Assert.That(assembly.GetType("CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Forms.DpiUnawareForm")).IsNull();
    }
}
