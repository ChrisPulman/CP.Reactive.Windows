// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Final deterministic coverage tests for <see cref="WindowScroller"/>.</summary>
public sealed class CoverageFinalWindowScrollerTests
{
    /// <summary>Defines a synthetic window handle.</summary>
    private const int WindowHandleValue = 7421;

    /// <summary>Defines a synthetic horizontal window handle.</summary>
    private const int HorizontalWindowHandleValue = 7422;

    /// <summary>Defines the first expected value.</summary>
    private const int ExpectedOne = 1;

    /// <summary>Defines the second expected value.</summary>
    private const int ExpectedTwo = 2;

    /// <summary>Defines the third expected value.</summary>
    private const int ExpectedThree = 3;

    /// <summary>Defines the fourth expected value.</summary>
    private const int ExpectedFour = 4;

    /// <summary>Defines the fifth expected value.</summary>
    private const int ExpectedFive = 5;

    /// <summary>Defines the sixth expected value.</summary>
    private const int ExpectedSix = 6;

    /// <summary>Defines the eighth expected value.</summary>
    private const int ExpectedEight = 8;

    /// <summary>Defines a centered X coordinate.</summary>
    private const int ExpectedCenterX = 35;

    /// <summary>Defines a centered Y coordinate.</summary>
    private const int ExpectedCenterY = 47;

    /// <summary>Defines a non-zero scroll position.</summary>
    private const int EndPosition = 1;

    /// <summary>Defines a zero scroll position.</summary>
    private const int StartPosition = 0;

    /// <summary>Defines a failed input count.</summary>
    private const uint FailedInputCount = 0;

    /// <summary>Defines the wheel delta under test.</summary>
    private const int WheelDeltaValue = 120;

    /// <summary>Defines a valid registry scroll line value.</summary>
    private const string ExpectedEightText = "8";

    /// <summary>Defines an invalid registry scroll line value.</summary>
    private const string InvalidScrollLinesText = "not-a-number";

    /// <summary>Defines the cached window bounds X coordinate.</summary>
    private const int BoundsX = 10;

    /// <summary>Defines the cached window bounds Y coordinate.</summary>
    private const int BoundsY = 20;

    /// <summary>Defines the cached window width.</summary>
    private const int BoundsWidth = 50;

    /// <summary>Defines the cached window height.</summary>
    private const int BoundsHeight = 55;

    /// <summary>Covers scrollbar information caching and object-id mapping branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ScrollbarInfo_CachesAndMapsObjectIdentifiersAsync()
    {
        var recording = new RecordingScrollOperations();
        using var overrideScope = WindowScroller.OverrideOperationsForTesting(recording);
        var scroller = CreateScroller();

        scroller.ScrollBarType = ScrollBarTypes.Vertical;
        var first = scroller.GetScrollbarInfo();
        var cached = scroller.GetScrollbarInfo();
        var refreshed = scroller.GetScrollbarInfo(true);

        scroller.ScrollBarType = ScrollBarTypes.Horizontal;
        _ = scroller.GetScrollbarInfo(true);

        scroller.ScrollBarType = ScrollBarTypes.Control;
        _ = scroller.GetScrollbarInfo(true);

        scroller.ScrollBarType = ScrollBarTypes.Both;
        _ = scroller.GetScrollbarInfo(true);

        recording.GetScrollBarInfoResult = false;
        var missing = scroller.GetScrollbarInfo(true);

        scroller.ScrollBarType = (ScrollBarTypes)int.MaxValue;

        await Assert.That(first.HasValue).IsTrue();
        await Assert.That(cached.HasValue).IsTrue();
        await Assert.That(refreshed.HasValue).IsTrue();
        await Assert.That(missing.HasValue).IsFalse();
        await Assert.That(recording.ObjectIdentifiers.Count).IsEqualTo(ExpectedSix);
        await Assert.That(recording.ObjectIdentifiers[StartPosition]).IsEqualTo(ObjectIdentifiers.VerticalScrollbar);
        await Assert.That(recording.ObjectIdentifiers[ExpectedOne]).IsEqualTo(ObjectIdentifiers.VerticalScrollbar);
        await Assert.That(recording.ObjectIdentifiers[ExpectedTwo]).IsEqualTo(ObjectIdentifiers.HorizontalScrollbar);
        await Assert.That(recording.ObjectIdentifiers[ExpectedThree]).IsEqualTo(ObjectIdentifiers.Client);
        await Assert.That(recording.ObjectIdentifiers[ExpectedFour]).IsEqualTo(ObjectIdentifiers.Client);
        await Assert.That(recording.ObjectIdentifiers[ExpectedFive]).IsEqualTo(ObjectIdentifiers.Client);
        await Assert.That(() => scroller.GetScrollbarInfo(true)).Throws<ArgumentOutOfRangeException>();
    }

    /// <summary>Covers start/end state when native position retrieval succeeds and fails.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task PositionState_UsesInitialAndCurrentBoundsAsync()
    {
        var recording = new RecordingScrollOperations { GetScrollInfoResult = false };
        using var overrideScope = WindowScroller.OverrideOperationsForTesting(recording);
        var scroller = CreateScroller();

        await Assert.That(scroller.GetPosition(out var failedScrollInfo)).IsFalse();
        await Assert.That(failedScrollInfo).IsEqualTo(ScrollInfo.Create(ScrollInfoMask.All));
        await Assert.That(scroller.IsAtStart).IsFalse();
        await Assert.That(scroller.IsAtEnd).IsFalse();

        recording.GetScrollInfoResult = true;
        recording.Position = StartPosition;

        await Assert.That(scroller.IsAtStart).IsTrue();
        await Assert.That(scroller.IsAtEnd).IsFalse();

        recording.Position = EndPosition;

        await Assert.That(scroller.IsAtStart).IsFalse();
        await Assert.That(scroller.IsAtEnd).IsTrue();

        scroller.KeepInitialBounds = false;
        recording.TrackingPosition = EndPosition;

        await Assert.That(scroller.IsAtStart).IsFalse();
        await Assert.That(scroller.IsAtEnd).IsTrue();
    }

    /// <summary>Covers Windows message mode command routing.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsMessageMode_RoutesVerticalHorizontalAndUnsupportedBarsAsync()
    {
        var recording = new RecordingScrollOperations();
        using var overrideScope = WindowScroller.OverrideOperationsForTesting(recording);
        var scroller = CreateScroller();

        scroller.ScrollMode = ScrollModes.WindowsMessage;
        scroller.ScrollBarType = ScrollBarTypes.Vertical;

        await Assert.That(scroller.Start()).IsTrue();
        await Assert.That(scroller.End()).IsTrue();
        await Assert.That(scroller.Next()).IsTrue();
        await Assert.That(scroller.Previous()).IsTrue();

        scroller.ScrollBarType = ScrollBarTypes.Horizontal;

        await Assert.That(scroller.Start()).IsTrue();

        scroller.ScrollBarType = ScrollBarTypes.Control;

        await Assert.That(scroller.Start()).IsFalse();
        await Assert.That(recording.CommandMessages.Count).IsEqualTo(ExpectedFive);
        await Assert.That(recording.CommandMessages[StartPosition]).IsEqualTo(WindowsMessages.WM_VSCROLL);
        await Assert.That(recording.CommandMessages[ExpectedFour]).IsEqualTo(WindowsMessages.WM_HSCROLL);
        await Assert.That(recording.ScrollCommands[StartPosition]).IsEqualTo(ScrollBarCommands.SB_TOP);
        await Assert.That(recording.ScrollCommands[ExpectedOne]).IsEqualTo(ScrollBarCommands.SB_BOTTOM);
        await Assert.That(recording.ScrollCommands[ExpectedTwo]).IsEqualTo(ScrollBarCommands.SB_PAGEDOWN);
        await Assert.That(recording.ScrollCommands[ExpectedThree]).IsEqualTo(ScrollBarCommands.SB_PAGEUP);
    }

    /// <summary>Covers absolute message mode and reset position application branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AbsoluteMessageMode_AppliesHorizontalVerticalBothAndInvalidBarsAsync()
    {
        var recording = new RecordingScrollOperations { Position = EndPosition };
        using var overrideScope = WindowScroller.OverrideOperationsForTesting(recording);
        var scroller = CreateScroller();

        scroller.ScrollMode = ScrollModes.AbsoluteWindowMessage;
        scroller.ScrollBarType = ScrollBarTypes.Horizontal;

        await Assert.That(scroller.End()).IsTrue();

        scroller.ShowChanges = false;
        scroller.ScrollBarType = ScrollBarTypes.Vertical;

        await Assert.That(scroller.Start()).IsTrue();
        await Assert.That(scroller.Next()).IsTrue();
        await Assert.That(scroller.Previous()).IsTrue();

        scroller.ScrollBarType = ScrollBarTypes.Both;

        await Assert.That(scroller.Reset()).IsTrue();

        scroller.ScrollBarType = (ScrollBarTypes)int.MaxValue;

        await Assert.That(() => scroller.Reset()).Throws<ArgumentOutOfRangeException>();
        await Assert.That(recording.SetScrollInfoCalls).IsEqualTo(ExpectedOne);
        await Assert.That(recording.IntegerMessages.Count).IsEqualTo(ExpectedFour);
        await Assert.That(recording.IntegerMessages[StartPosition]).IsEqualTo(WindowsMessages.WM_HSCROLL);
        await Assert.That(recording.IntegerMessages[ExpectedOne]).IsEqualTo(WindowsMessages.WM_VSCROLL);
    }

    /// <summary>Covers keyboard mode success and failure paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyboardMode_UsesConfiguredInputOperationsAsync()
    {
        var recording = new RecordingScrollOperations();
        using var overrideScope = WindowScroller.OverrideOperationsForTesting(recording);
        var scroller = CreateScroller();

        scroller.ScrollMode = ScrollModes.KeyboardPageUpDown;

        await Assert.That(scroller.NeedsFocus()).IsTrue();
        await Assert.That(scroller.Start()).IsTrue();
        await Assert.That(scroller.End()).IsTrue();
        await Assert.That(scroller.Next()).IsTrue();
        await Assert.That(scroller.Previous()).IsTrue();

        recording.KeyPressResult = FailedInputCount;

        await Assert.That(scroller.Next()).IsFalse();
        await Assert.That(scroller.Previous()).IsFalse();
        await Assert.That(recording.KeyDownCount).IsEqualTo(ExpectedTwo);
        await Assert.That(recording.KeyUpCount).IsEqualTo(ExpectedTwo);
        await Assert.That(recording.KeyPressKeys.Count).IsEqualTo(ExpectedSix);
    }

    /// <summary>Covers mouse-wheel direct moves and boundary loops.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MouseWheelMode_MovesAtWindowCenterAndStopsAtBoundariesAsync()
    {
        var recording = new RecordingScrollOperations { Position = StartPosition };
        using var overrideScope = WindowScroller.OverrideOperationsForTesting(recording);
        var scroller = CreateScroller();

        scroller.ScrollMode = ScrollModes.MouseWheel;
        scroller.WheelDelta = WheelDeltaValue;

        await Assert.That(scroller.Next()).IsTrue();
        await Assert.That(recording.WheelDeltas[StartPosition]).IsEqualTo(-WheelDeltaValue);
        await Assert.That(recording.WheelLocations[StartPosition].X).IsEqualTo(ExpectedCenterX);
        await Assert.That(recording.WheelLocations[StartPosition].Y).IsEqualTo(ExpectedCenterY);

        await Assert.That(scroller.Previous()).IsTrue();
        await Assert.That(recording.WheelDeltas[ExpectedOne]).IsEqualTo(WheelDeltaValue);

        recording.Position = StartPosition;

        await Assert.That(scroller.End()).IsTrue();

        recording.Position = EndPosition;

        await Assert.That(scroller.Start()).IsTrue();

        recording.MouseWheelResult = FailedInputCount;
        recording.Position = StartPosition;

        await Assert.That(scroller.End()).IsTrue();
    }

    /// <summary>Covers unsupported scroll mode branches for all movement methods.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task UnsupportedScrollModes_ThrowForAllMovementMethodsAsync()
    {
        var recording = new RecordingScrollOperations();
        using var overrideScope = WindowScroller.OverrideOperationsForTesting(recording);
        var scroller = CreateScroller();

        scroller.ScrollMode = (ScrollModes)int.MaxValue;

        await Assert.That(() => scroller.Start()).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => scroller.End()).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => scroller.Next()).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => scroller.Previous()).Throws<ArgumentOutOfRangeException>();
    }

    /// <summary>Covers deterministic scroll wheel line parsing without reading the registry.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ScrollWheelLinesFromRegistry_UsesDefaultAndParsedOperationValuesAsync()
    {
        var recording = new RecordingScrollOperations();
        using var overrideScope = WindowScroller.OverrideOperationsForTesting(recording);

        await Assert.That(WindowScroller.ScrollWheelLinesFromRegistry).IsEqualTo(ExpectedThree);

        recording.ScrollWheelLines = InvalidScrollLinesText;

        await Assert.That(WindowScroller.ScrollWheelLinesFromRegistry).IsEqualTo(ExpectedThree);

        recording.ScrollWheelLines = ExpectedEightText;

        await Assert.That(WindowScroller.ScrollWheelLinesFromRegistry).IsEqualTo(ExpectedEight);
    }

    /// <summary>Covers page and absolute modes when position retrieval fails.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task FailedPositionRetrieval_PageAndAbsoluteModes_ReturnFalseAsync()
    {
        var recording = new RecordingScrollOperations { GetScrollInfoResult = false };
        using var overrideScope = WindowScroller.OverrideOperationsForTesting(recording);
        var scroller = CreateScroller();

        scroller.ScrollMode = ScrollModes.AbsoluteWindowMessage;

        await Assert.That(scroller.Start()).IsFalse();
        await Assert.That(scroller.End()).IsFalse();

        await Assert.That(scroller.Next()).IsFalse();
        await Assert.That(scroller.Previous()).IsFalse();
    }

    /// <summary>Covers default operation wrappers without producing input side effects.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DefaultOperations_ZeroHandlesAndNoInput_ReturnNativeFailuresAsync()
    {
        var operations = new WindowScroller.WindowScrollerOperations();
        var scrollInfo = ScrollInfo.Create(ScrollInfoMask.All);
        var scrollBarInfo = ScrollBarInfo.Create();

        await Assert.That(operations.GetScrollInfo(IntPtr.Zero, ScrollBarTypes.Vertical, ref scrollInfo)).IsFalse();
        await Assert.That(operations.SetScrollInfo(IntPtr.Zero, ScrollBarTypes.Vertical, ref scrollInfo, true)).IsEqualTo(StartPosition);
        await Assert.That(operations.SendCommandMessage(IntPtr.Zero, WindowsMessages.WM_VSCROLL, ScrollBarCommands.SB_TOP, StartPosition)).IsEqualTo(StartPosition);
        await Assert.That(operations.SendIntegerMessage(IntPtr.Zero, WindowsMessages.WM_VSCROLL, StartPosition, StartPosition)).IsEqualTo(IntPtr.Zero);
        await Assert.That(operations.GetScrollBarInfo(IntPtr.Zero, ObjectIdentifiers.VerticalScrollbar, ref scrollBarInfo)).IsFalse();
        await Assert.That(operations.KeyDown()).IsEqualTo(FailedInputCount);
        await Assert.That(operations.KeyPresses()).IsEqualTo(FailedInputCount);
        await Assert.That(operations.KeyUp()).IsEqualTo(FailedInputCount);
        await Assert.That(operations.MoveMouseWheel(StartPosition, null)).IsEqualTo(FailedInputCount);
    }

    /// <summary>Creates a scroller with cached synthetic windows.</summary>
    /// <returns>The configured scroller.</returns>
    private static WindowScroller CreateScroller()
    {
        var scrollingWindow = CreateWindow(WindowHandleValue);
        var scrollBarWindow = CreateWindow(HorizontalWindowHandleValue);
        return new()
        {
            InitialScrollInfo = ScrollInfo.Create(ScrollInfoMask.All),
            ScrollBarWindow = scrollBarWindow,
            ScrollingWindow = scrollingWindow,
            ScrollBarType = ScrollBarTypes.Vertical,
            WheelDelta = WheelDeltaValue,
            ShowChanges = true,
        };
    }

    /// <summary>Creates a cached interop window.</summary>
    /// <param name="handle">The synthetic handle.</param>
    /// <returns>The cached window.</returns>
    private static InteropWindow CreateWindow(int handle)
    {
        var info = WindowInfo.Create();
        info.Bounds = new(BoundsX, BoundsY, BoundsWidth, BoundsHeight);
        info.ClientBounds = new(BoundsX, BoundsY, BoundsWidth, BoundsHeight);

        return new(new(handle))
        {
            Caption = nameof(CoverageFinalWindowScrollerTests),
            Classname = nameof(CoverageFinalWindowScrollerTests),
            Info = info,
            Text = nameof(CoverageFinalWindowScrollerTests),
        };
    }

    /// <summary>Records scroller operations without calling the operating system.</summary>
    internal sealed class RecordingScrollOperations : WindowScroller.WindowScrollerOperations
    {
        /// <summary>Defines the input count for a key transition.</summary>
        private const uint KeyTransitionCount = 1;

        /// <summary>Defines the successful key press count.</summary>
        private const uint SuccessfulKeyPressCount = 2;

        /// <summary>Defines the successful mouse wheel count.</summary>
        private const uint SuccessfulMouseWheelCount = 1;

        /// <summary>Gets recorded object identifiers.</summary>
        internal List<ObjectIdentifiers> ObjectIdentifiers { get; } = [];

        /// <summary>Gets recorded command messages.</summary>
        internal List<WindowsMessages> CommandMessages { get; } = [];

        /// <summary>Gets recorded scroll commands.</summary>
        internal List<ScrollBarCommands> ScrollCommands { get; } = [];

        /// <summary>Gets recorded integer messages.</summary>
        internal List<WindowsMessages> IntegerMessages { get; } = [];

        /// <summary>Gets recorded wheel deltas.</summary>
        internal List<int> WheelDeltas { get; } = [];

        /// <summary>Gets recorded wheel locations.</summary>
        internal List<NativePoint> WheelLocations { get; } = [];

        /// <summary>Gets recorded key press keys.</summary>
        internal List<VirtualKeyCode> KeyPressKeys { get; } = [];

        /// <summary>Gets or sets a value indicating whether scroll info retrieval succeeds.</summary>
        internal bool GetScrollInfoResult { get; set; } = true;

        /// <summary>Gets or sets a value indicating whether scroll bar info retrieval succeeds.</summary>
        internal bool GetScrollBarInfoResult { get; set; } = true;

        /// <summary>Gets or sets the configured scroll wheel lines text.</summary>
        internal string ScrollWheelLines { get; set; }

        /// <summary>Gets or sets the current scroll position.</summary>
        internal int Position { get; set; }

        /// <summary>Gets or sets the current tracking position.</summary>
        internal int TrackingPosition { get; set; }

        /// <summary>Gets or sets the key press result.</summary>
        internal uint KeyPressResult { get; set; } = SuccessfulKeyPressCount;

        /// <summary>Gets or sets the mouse wheel result.</summary>
        internal uint MouseWheelResult { get; set; } = SuccessfulMouseWheelCount;

        /// <summary>Gets the key down call count.</summary>
        internal int KeyDownCount { get; private set; }

        /// <summary>Gets the key up call count.</summary>
        internal int KeyUpCount { get; private set; }

        /// <summary>Gets the set scroll info call count.</summary>
        internal int SetScrollInfoCalls { get; private set; }

        /// <summary>Records a scroll information request.</summary>
        /// <param name="windowHandle">The target handle.</param>
        /// <param name="scrollBar">The target scroll bar.</param>
        /// <param name="scrollInfo">The scroll information.</param>
        /// <returns>The configured result.</returns>
        internal override bool GetScrollInfo(IntPtr windowHandle, ScrollBarTypes scrollBar, ref ScrollInfo scrollInfo)
        {
            scrollInfo.Position = Position;
            scrollInfo.TrackingPosition = TrackingPosition;
            return GetScrollInfoResult;
        }

        /// <summary>Records a scroll information set request.</summary>
        /// <param name="windowHandle">The target handle.</param>
        /// <param name="scrollBar">The target scroll bar.</param>
        /// <param name="scrollInfo">The scroll information.</param>
        /// <param name="redraw">Whether redraw was requested.</param>
        /// <returns>The configured result.</returns>
        internal override int SetScrollInfo(IntPtr windowHandle, ScrollBarTypes scrollBar, ref ScrollInfo scrollInfo, bool redraw)
        {
            SetScrollInfoCalls++;
            Position = scrollInfo.Position;
            return ExpectedOne;
        }

        /// <summary>Records a scroll command message.</summary>
        /// <param name="windowHandle">The target handle.</param>
        /// <param name="message">The Windows message.</param>
        /// <param name="scrollBarCommand">The scroll command.</param>
        /// <param name="parameter">The message parameter.</param>
        /// <returns>The configured result.</returns>
        internal override int SendCommandMessage(IntPtr windowHandle, WindowsMessages message, ScrollBarCommands scrollBarCommand, int parameter)
        {
            CommandMessages.Add(message);
            ScrollCommands.Add(scrollBarCommand);
            return ExpectedOne;
        }

        /// <summary>Records an integer scroll message.</summary>
        /// <param name="windowHandle">The target handle.</param>
        /// <param name="message">The Windows message.</param>
        /// <param name="wordParameter">The word parameter.</param>
        /// <param name="parameter">The message parameter.</param>
        /// <returns>The configured result.</returns>
        internal override IntPtr SendIntegerMessage(IntPtr windowHandle, WindowsMessages message, int wordParameter, int parameter)
        {
            IntegerMessages.Add(message);
            return IntPtr.Zero;
        }

        /// <summary>Records a scroll bar information request.</summary>
        /// <param name="windowHandle">The target handle.</param>
        /// <param name="objectId">The object identifier.</param>
        /// <param name="scrollBarInfo">The scroll bar information.</param>
        /// <returns>The configured result.</returns>
        internal override bool GetScrollBarInfo(IntPtr windowHandle, ObjectIdentifiers objectId, ref ScrollBarInfo scrollBarInfo)
        {
            ObjectIdentifiers.Add(objectId);
            scrollBarInfo = ScrollBarInfo.Create();
            return GetScrollBarInfoResult;
        }

        /// <summary>Gets the configured scroll wheel line count text.</summary>
        /// <returns>The configured scroll wheel line count text.</returns>
        internal override string GetScrollWheelLines() => ScrollWheelLines;

        /// <summary>Records key down input.</summary>
        /// <param name="keycodes">The key codes.</param>
        /// <returns>The configured result.</returns>
        internal override uint KeyDown(params VirtualKeyCode[] keycodes)
        {
            KeyDownCount++;
            return KeyTransitionCount;
        }

        /// <summary>Records key press input.</summary>
        /// <param name="keycodes">The key codes.</param>
        /// <returns>The configured result.</returns>
        internal override uint KeyPresses(params VirtualKeyCode[] keycodes)
        {
            foreach (var keycode in keycodes)
            {
                KeyPressKeys.Add(keycode);
            }

            return KeyPressResult;
        }

        /// <summary>Records key up input.</summary>
        /// <param name="keycodes">The key codes.</param>
        /// <returns>The configured result.</returns>
        internal override uint KeyUp(params VirtualKeyCode[] keycodes)
        {
            KeyUpCount++;
            return KeyTransitionCount;
        }

        /// <summary>Records mouse-wheel input.</summary>
        /// <param name="wheelDelta">The wheel delta.</param>
        /// <param name="location">The target location.</param>
        /// <returns>The configured result.</returns>
        internal override uint MoveMouseWheel(int wheelDelta, NativePoint? location)
        {
            WheelDeltas.Add(wheelDelta);
            WheelLocations.Add(location.GetValueOrDefault());
            if (MouseWheelResult == SuccessfulMouseWheelCount)
            {
                Position = wheelDelta < StartPosition ? EndPosition : StartPosition;
            }

            return MouseWheelResult;
        }
    }
}
