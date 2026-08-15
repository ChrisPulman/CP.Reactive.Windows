// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Additional coverage for DPI and display topology behavior.</summary>
public sealed class DpiDisplayCoverageTests
{
    /// <summary>The standard Windows DPI.</summary>
    private const int Dpi96 = 96;

    /// <summary>A 125 percent DPI value.</summary>
    private const int Dpi120 = 120;

    /// <summary>A 150 percent DPI value.</summary>
    private const int Dpi144 = 144;

    /// <summary>A 200 percent DPI value.</summary>
    private const int Dpi192 = 192;

    /// <summary>The tolerance used for floating-point DPI calculations.</summary>
    private const double ComparisonTolerance = 0.001D;

    /// <summary>Verifies scalar, integer, point, and size DPI scaling with modifiers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DpiCalculator_Scales_AllSupportedValueShapesAsync()
    {
        var modifierCalls = 0;
        float Modifier(float scale)
        {
            modifierCalls++;
            return scale + QuarterFloat;
        }

        await Assert.That(DpiCalculator.DefaultScreenDpi).IsEqualTo(Dpi96);
        await Assert.That(DpiCalculator.DpiScaleFactor(Dpi96, Dpi144)).IsEqualTo(OneAndHalfFloat);
        await Assert.That(DpiCalculator.DpiScaleFactor(Dpi192)).IsEqualTo(TwoFloat);
        await Assert.That(DpiCalculator.ScaleWithDpi(TenFloat, Dpi120)).IsEqualTo(TwelveAndHalfFloat);
        await Assert.That(DpiCalculator.ScaleWithDpi(TenDouble, Dpi144)).IsEqualTo(FifteenDouble);
        await Assert.That(DpiCalculator.ScaleWithDpi(Eleven, Dpi144)).IsEqualTo(Sixteen);
        await Assert.That(DpiCalculator.ScaleWithDpi(Eight, Dpi96, Modifier)).IsEqualTo(Ten);
        await Assert.That(DpiCalculator.ScaleWithDpi(new NativeSize(Seven, Nine), Dpi192)).IsEqualTo(new(Fourteen, Eighteen));
        await Assert.That(DpiCalculator.ScaleWithDpi(new NativePoint(Seven, Nine), Dpi192)).IsEqualTo(new(Fourteen, Eighteen));
        await Assert.That(DpiCalculator.ScaleWithDpi(new NativeSizeFloat(TwoAndHalfFloat, FourAndHalfFloat), Dpi144)).IsEqualTo(new(ThreeAndThreeQuartersFloat, SixAndThreeQuartersFloat));
        await Assert.That(DpiCalculator.ScaleWithDpi(new NativePointFloat(TwoAndHalfFloat, FourAndHalfFloat), Dpi144)).IsEqualTo(new(ThreeAndThreeQuartersFloat, SixAndThreeQuartersFloat));
        await Assert.That(modifierCalls).IsEqualTo(1);
    }

    /// <summary>Verifies scalar, integer, point, and size DPI unscaling with modifiers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DpiCalculator_Unscales_AllSupportedValueShapesAsync()
    {
        var modifierCalls = 0;
        float Modifier(float scale)
        {
            modifierCalls++;
            return scale * TwoFloat;
        }

        await Assert.That(DpiCalculator.DpiUnscaleFactor(Dpi96, Dpi192)).IsEqualTo(HalfFloat);
        await Assert.That(DpiCalculator.DpiUnscaleFactor(Dpi192)).IsEqualTo(HalfFloat);
        await Assert.That(Math.Abs(DpiCalculator.UnscaleWithDpi(FifteenDouble, Dpi144) - TenDouble) < ComparisonTolerance).IsTrue();
        await Assert.That(DpiCalculator.UnscaleWithDpi(Sixteen, Dpi144)).IsEqualTo(Ten);
        await Assert.That(DpiCalculator.UnscaleWithDpi(Eight, Dpi192, Modifier)).IsEqualTo(Eight);
        await Assert.That(DpiCalculator.UnscaleWithDpi(new NativeSize(Fourteen, Eighteen), Dpi192)).IsEqualTo(new(Seven, Nine));
        await Assert.That(DpiCalculator.UnscaleWithDpi(new NativePoint(Fourteen, Eighteen), Dpi192)).IsEqualTo(new(Seven, Nine));
        await Assert.That(DpiCalculator.UnscaleWithDpi(new NativeSizeFloat(ThreeAndThreeQuartersFloat, SixAndThreeQuartersFloat), Dpi144)).IsEqualTo(new(TwoAndHalfFloat, FourAndHalfFloat));
        await Assert.That(DpiCalculator.UnscaleWithDpi(new NativePointFloat(ThreeAndThreeQuartersFloat, SixAndThreeQuartersFloat), Dpi144)).IsEqualTo(new(TwoAndHalfFloat, FourAndHalfFloat));
        await Assert.That(modifierCalls).IsEqualTo(1);
    }

    /// <summary>Verifies DPI message handling publishes changes and ignores unchanged/non-DPI paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DpiHandler_HandleWindowMessages_PublishesOnlyRealDpiChangesAsync()
    {
        using var handler = new DpiHandler();
        var changes = new List<DpiChangeInfo>();
        using var subscription = handler.ObserveDpiChanges().Subscribe(changes.Add);

        var paintHandled = handler.HandleWindowMessages(WindowMessageInfo.Create(0, (int)WindowsMessages.WM_PAINT, 0, 0));
        var setIconHandled = handler.HandleWindowMessages(WindowMessageInfo.Create(0, (int)WindowsMessages.WM_SETICON, 0, 0));
        var firstChangeHandled = SendDpiChanged(handler, Dpi144);
        var unchangedHandled = SendDpiChanged(handler, Dpi144);
        var secondChangeHandled = SendDpiChanged(handler, Dpi192);

        await Assert.That(paintHandled).IsFalse();
        await Assert.That(setIconHandled).IsFalse();
        await Assert.That(firstChangeHandled).IsTrue();
        await Assert.That(unchangedHandled).IsTrue();
        await Assert.That(secondChangeHandled).IsTrue();
        await Assert.That(handler.CurrentDpi).IsEqualTo(Dpi192);
        await Assert.That(changes.Count).IsEqualTo(Three);
        await Assert.That(changes[0].PreviousDpi).IsEqualTo(0);
        await Assert.That(changes[0].NewDpi).IsEqualTo(Dpi96);
        await Assert.That(changes[1].PreviousDpi).IsEqualTo(Dpi96);
        await Assert.That(changes[1].NewDpi).IsEqualTo(Dpi144);
        await Assert.That(changes[2].PreviousDpi).IsEqualTo(Dpi144);
        await Assert.That(changes[2].NewDpi).IsEqualTo(Dpi192);
    }

    /// <summary>Verifies current-DPI helper methods delegate to the calculator with current state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DpiHandler_CurrentDpiHelpers_UseCurrentDpiAsync()
    {
        using var handler = new DpiHandler();
        _ = SendDpiChanged(handler, Dpi144);

        await Assert.That(handler.ScaleWithCurrentDpi(TenDouble)).IsEqualTo(FifteenDouble);
        await Assert.That(handler.ScaleWithCurrentDpi(Ten)).IsEqualTo(Fifteen);
        await Assert.That(handler.ScaleWithCurrentDpi(new NativeSize(Ten, Twelve))).IsEqualTo(new(Fifteen, Eighteen));
        await Assert.That(handler.ScaleWithCurrentDpi(new NativePoint(Ten, Twelve))).IsEqualTo(new(Fifteen, Eighteen));
        await Assert.That(handler.ScaleWithCurrentDpi(new NativeSizeFloat(TenFloat, TwelveFloat))).IsEqualTo(new(FifteenFloat, EighteenFloat));
        await Assert.That(handler.ScaleWithCurrentDpi(new NativePointFloat(TenFloat, TwelveFloat))).IsEqualTo(new(FifteenFloat, EighteenFloat));
        await Assert.That(Math.Abs(handler.UnscaleWithCurrentDpi(FifteenDouble) - TenDouble) < ComparisonTolerance).IsTrue();
        await Assert.That(handler.UnscaleWithCurrentDpi(Fifteen)).IsEqualTo(Ten);
        await Assert.That(handler.UnscaleWithCurrentDpi(new NativeSize(Fifteen, Eighteen))).IsEqualTo(new(Ten, Twelve));
        await Assert.That(handler.UnscaleWithCurrentDpi(new NativePoint(Fifteen, Eighteen))).IsEqualTo(new(Ten, Twelve));
        await Assert.That(handler.UnscaleWithCurrentDpi(new NativeSizeFloat(FifteenFloat, EighteenFloat))).IsEqualTo(new(TenFloat, TwelveFloat));
        await Assert.That(handler.UnscaleWithCurrentDpi(new NativePointFloat(FifteenFloat, EighteenFloat))).IsEqualTo(new(TenFloat, TwelveFloat));
        await Assert.That(handler.ScaleWithCurrentDpi(Ten, static scale => scale * TwoFloat)).IsEqualTo(Thirty);
        await Assert.That(handler.UnscaleWithCurrentDpi(Fifteen, static scale => scale * TwoFloat)).IsEqualTo(Twenty);
    }

    /// <summary>Verifies context menu messages update DPI without claiming native handling.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DpiHandler_ContextMenuMessages_UpdateAndCompleteObservableAsync()
    {
        using var handler = new DpiHandler();
        var changes = new List<DpiChangeInfo>();
        var completed = false;
        using var subscription = handler.ObserveDpiChanges().Subscribe(changes.Add, () => completed = true);

        var showResult = handler.HandleContextMenuMessages(WindowMessageInfo.Create(0, (int)WindowsMessages.WM_SHOWWINDOW, 0, 0));
        var ignoredResult = handler.HandleContextMenuMessages(WindowMessageInfo.Create(0, (int)WindowsMessages.WM_PAINT, 0, 0));
        var destroyResult = handler.HandleContextMenuMessages(WindowMessageInfo.Create(0, (int)WindowsMessages.WM_DESTROY, 0, 0));

        await Assert.That(showResult).IsEqualTo(IntPtr.Zero);
        await Assert.That(ignoredResult).IsEqualTo(IntPtr.Zero);
        await Assert.That(destroyResult).IsEqualTo(IntPtr.Zero);
        await Assert.That(handler.CurrentDpi).IsEqualTo(Dpi96);
        await Assert.That(changes.Count).IsEqualTo(1);
        await Assert.That(completed).IsTrue();
    }

    /// <summary>Verifies DPI-aware and unaware form behaviors create handles and enforce disposal.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task FormDpiBehaviors_CreateHandlesAndThrowAfterDisposeAsync()
    {
        using var awareForm = new System.Windows.Forms.Form();
        var awareBehavior = awareForm.AttachDpiAwareBehavior();
        awareBehavior.EnsureHandleCreated();
        await Assert.That(awareForm.IsHandleCreated).IsTrue();
        await Assert.That(awareBehavior.DpiHandler.CurrentDpi > 0).IsTrue();
        awareBehavior.Dispose();
        awareBehavior.Dispose();
        await Assert.That(awareBehavior.EnsureHandleCreated).Throws<ObjectDisposedException>();

        using var unawareForm = new System.Windows.Forms.Form();
        var unawareBehavior = unawareForm.AttachDpiUnawareBehavior();
        unawareBehavior.EnsureHandleCreated();
        await Assert.That(unawareForm.IsHandleCreated).IsTrue();
        unawareBehavior.Dispose();
        unawareBehavior.Dispose();
        await Assert.That(unawareBehavior.EnsureHandleCreated).Throws<ObjectDisposedException>();
    }

    /// <summary>Verifies attach helpers create disposable handlers for forms and context menus.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task FormsDpiExtensions_AttachHandlers_ReturnDisposableHandlersAsync()
    {
        using var form = new System.Windows.Forms.Form();
        using var contextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
        using var formHandler = form.AttachDpiHandler();
        using var contextHandler = contextMenuStrip.AttachDpiHandler();

        await Assert.That(formHandler.ObserveDpiChanges()).IsNotNull();
        await Assert.That(contextHandler.ObserveDpiChanges()).IsNotNull();
    }

    /// <summary>Verifies bitmap scaling returns original at default DPI and resizes at higher DPI.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapScaleHandler_SimpleBitmapScaler_HandlesDefaultAndScaledDpiAsync()
    {
        using var bitmap = new Bitmap(Four, Six, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        var defaultResult = BitmapScaleHandler.SimpleBitmapScaler(bitmap, Dpi96);
        using var scaledResult = BitmapScaleHandler.SimpleBitmapScaler(bitmap, Dpi144);

        await Assert.That(ReferenceEquals(bitmap, defaultResult)).IsTrue();
        await Assert.That(ReferenceEquals(bitmap, scaledResult)).IsFalse();
        await Assert.That(scaledResult.Size).IsEqualTo(new(Six, Nine));
        await Assert.That(static () => BitmapScaleHandler.SimpleBitmapScaler(null, Dpi96)).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies bitmap scale handler caches, re-applies, removes targets, and disposes cached images.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapScaleHandler_CachesReappliesRemovesAndDisposesValuesAsync()
    {
        using var dpiHandler = new DpiHandler();
        var providerCalls = 0;
        var applied = new List<TrackedDisposable>();
        Action<TrackedDisposable> applyTracked = applied.Add;
        using var scaleHandler = BitmapScaleHandler.Create<string, TrackedDisposable>(
            dpiHandler,
            (key, dpi) =>
            {
                providerCalls++;
                return new($"{key}:{dpi}:{providerCalls}");
            });

        _ = scaleHandler.AddApplyAction(applyTracked, "save", true);
        _ = scaleHandler.AddApplyAction(applyTracked, "save", true);
        _ = SendDpiChanged(dpiHandler, Dpi144);
        _ = scaleHandler.RemoveTarget(applyTracked);
        _ = SendDpiChanged(dpiHandler, Dpi192);

        await Assert.That(providerCalls).IsEqualTo(Two);
        await Assert.That(applied.Count).IsEqualTo(Three);
        await Assert.That(ReferenceEquals(applied[0], applied[1])).IsTrue();
        await Assert.That(applied[0].Name).IsEqualTo("save:0:1");
        await Assert.That(applied[0].IsDisposed).IsTrue();
        await Assert.That(applied[2].Name).IsEqualTo("save:144:2");
        await Assert.That(applied[2].IsDisposed).IsTrue();
    }

    /// <summary>Verifies button and tool-strip targets receive converted bitmaps and can be removed.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapScaleHandler_TargetHelpers_AssignButtonAndToolStripImagesAsync()
    {
        using var dpiHandler = new DpiHandler();
        using var button = new System.Windows.Forms.Button();
        using var item = new System.Windows.Forms.ToolStripButton();
        using var scaleHandler = BitmapScaleHandler.Create<string, TrackedDisposable>(
            dpiHandler,
            static (key, dpi) => new($"{key}:{dpi}"));

        _ = scaleHandler.AddTarget(button, "button", static value => TrackedDisposable.CreateBitmap(), true);
        _ = scaleHandler.AddTarget(item, "item", static value => TrackedDisposable.CreateBitmap(), true);
        var firstButtonImage = button.Image;
        var firstItemImage = item.Image;

        _ = scaleHandler.RemoveTarget(button);
        _ = SendDpiChanged(dpiHandler, Dpi144);

        await Assert.That(firstButtonImage).IsNotNull();
        await Assert.That(firstItemImage).IsNotNull();
        await Assert.That(ReferenceEquals(button.Image, firstButtonImage)).IsTrue();
        await Assert.That(ReferenceEquals(item.Image, firstItemImage)).IsFalse();

        firstButtonImage?.Dispose();
        firstItemImage?.Dispose();
        item.Image?.Dispose();
    }

    /// <summary>Verifies display info stores all topology values.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DisplayInfo_StoresAllDisplayPropertiesAsync()
    {
        var displayInfo = new DisplayInfo
        {
            Bounds = new(1, Two, ThreeHundred, FourHundred),
            DeviceName = @"\\.\DISPLAY7",
            Index = Seven,
            IsPrimary = true,
            ScreenHeight = FourHundred,
            ScreenWidth = ThreeHundred,
            WorkingArea = new(1, Two, TwoHundredEighty, ThreeHundredSixty),
        };

        await Assert.That(displayInfo.Bounds).IsEqualTo(new(1, Two, ThreeHundred, FourHundred));
        await Assert.That(displayInfo.DeviceName).IsEqualTo(@"\\.\DISPLAY7");
        await Assert.That(displayInfo.Index).IsEqualTo(Seven);
        await Assert.That(displayInfo.IsPrimary).IsTrue();
        await Assert.That(displayInfo.ScreenHeight).IsEqualTo(FourHundred);
        await Assert.That(displayInfo.ScreenWidth).IsEqualTo(ThreeHundred);
        await Assert.That(displayInfo.WorkingArea).IsEqualTo(new(1, Two, TwoHundredEighty, ThreeHundredSixty));
        await Assert.That(displayInfo.MonitorHandle).IsNull();
    }

    /// <summary>Verifies display topology can calculate virtual-desktop bounds for representative layouts.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DisplayTopology_CalculateScreenBounds_HandlesEmptySingleAndOffsetDisplaysAsync()
    {
        var empty = DisplayTopology.CalculateScreenBounds([]);
        var single = DisplayTopology.CalculateScreenBounds([new() { Bounds = new(Ten, Twenty, Hundred, TwoHundred) }]);
        var combined = DisplayTopology.CalculateScreenBounds(
            [
                new() { Bounds = new(0, 0, OneThousandNineHundredTwenty, OneThousandEighty), IsPrimary = true },
                new() { Bounds = new(-OneThousandTwoHundredEighty, Hundred, OneThousandTwoHundredEighty, OneThousandTwentyFour) },
                new() { Bounds = new(OneThousandNineHundredTwenty, -TwoHundred, OneThousandSixHundred, NineHundred) },
            ]);

        await Assert.That(empty).IsEqualTo(NativeRect.Empty);
        await Assert.That(single).IsEqualTo(new(Ten, Twenty, Hundred, TwoHundred));
        await Assert.That(combined).IsEqualTo(new(-OneThousandTwoHundredEighty, -TwoHundred, FourThousandEightHundred, OneThousandThreeHundredTwentyFour));
    }

    /// <summary>Verifies display topology's public snapshot and lookup surfaces are usable on the current machine.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DisplayTopology_PublicSurface_ReturnsUsableSnapshotAndObservableAsync()
    {
        var snapshot = DisplayTopology.GetSnapshot();
        var screenBounds = DisplayTopology.ScreenBounds;
        var firstDisplay = snapshot.Count > 0 ? snapshot[0] : null;
        var bounds = firstDisplay is null ? NativeRect.Empty : DisplayTopology.GetBounds(firstDisplay.Bounds.Location);

        await Assert.That(snapshot).IsNotNull();
        await Assert.That(DisplayTopology.ObserveChanges()).IsNotNull();
        await Assert.That(screenBounds.Width >= 0).IsTrue();
        await Assert.That(screenBounds.Height >= 0).IsTrue();
        await Assert.That(firstDisplay is null || bounds == firstDisplay.Bounds).IsTrue();
    }

    /// <summary>Sends a DPI-changed message with a valid suggested rectangle pointer.</summary>
    /// <param name="handler">The handler under test.</param>
    /// <param name="dpi">The DPI to encode into the message.</param>
    /// <returns>The handler result.</returns>
    private static bool SendDpiChanged(DpiHandler handler, int dpi)
    {
        var rectangle = new NativeRect(Ten, Twenty, ThreeHundred, FourHundred);
        var rectanglePointer = Marshal.AllocHGlobal(NativeRect.SizeOf);
        try
        {
            Marshal.StructureToPtr(rectangle, rectanglePointer, false);
            return handler.HandleWindowMessages(WindowMessageInfo.Create(0, (int)WindowsMessages.WM_DPICHANGED, dpi, rectanglePointer.ToInt64()));
        }
        finally
        {
            Marshal.FreeHGlobal(rectanglePointer);
        }
    }

    /// <summary>Disposable test value used by bitmap scale handler tests.</summary>
    /// <param name="name">The test value name.</param>
    private sealed class TrackedDisposable(string name) : IDisposable
    {
        /// <summary>Gets the test value name.</summary>
        public string Name { get; } = name;

        /// <summary>Gets a value indicating whether the value has been disposed.</summary>
        public bool IsDisposed { get; private set; }

        /// <summary>Creates a bitmap associated with this tracked value.</summary>
        /// <returns>A bitmap.</returns>
        public static Bitmap CreateBitmap() => new(Two, Two);

        /// <inheritdoc />
        public void Dispose() => IsDisposed = true;
    }
}
