// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Desktop.Windows.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Wave 3 coverage for desktop interop helpers.</summary>
public sealed class CoverageWave3DesktopInteropTests
{
    /// <summary>Primary non-zero synthetic window handle.</summary>
    private const int SyntheticHandle = 0x4321;

    /// <summary>Secondary non-zero synthetic parent handle.</summary>
    private const int OtherSyntheticHandle = 0x6789;

    /// <summary>Width and height used for synthetic non-empty bounds.</summary>
    private const int TestBoundsSize = 42;

    /// <summary>Height of the temporary WinForms host.</summary>
    private const int TestFormHeight = 180;

    /// <summary>Screen location of the temporary WinForms host.</summary>
    private const int TestFormLocation = 20;

    /// <summary>Width of the temporary WinForms host.</summary>
    private const int TestFormWidth = 320;

    /// <summary>Number of lines used to force scrolling in the temporary editor.</summary>
    private const int TestLineCount = 160;

    /// <summary>Maximum seconds to wait for the expected WinEventHook error.</summary>
    private const int TestTimeoutSeconds = 2;

    /// <summary>Non-zero synthetic monitor handle.</summary>
    private const int SyntheticMonitorHandle = 0x2468;

    /// <summary>Class name used by the immersive launcher.</summary>
    private const string AppLauncherClass = "ImmersiveLauncher";

    /// <summary>Class name used by the immersive gutter.</summary>
    private const string GutterClass = "ImmersiveGutter";

    /// <summary>Class name that top-level filtering ignores.</summary>
    private const string IgnoredButtonClass = nameof(Button);

    /// <summary>Child class name used by dump assertions.</summary>
    private const string DumpChildClass = "Wave3DumpChild";

    /// <summary>Gets the current process identifier on every supported target framework.</summary>
    private static int CurrentProcessId =>
#if NETFRAMEWORK
        Process.GetCurrentProcess().Id;
#else
        Environment.ProcessId;
#endif

    /// <summary>Checks app classification against cached synthetic windows without native enumeration.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AppClassification_CachedWindows_CoversLauncherGutterAndWin10BranchesAsync()
    {
        var launcher = CreateWindow(AppLauncherClass);
        var gutter = CreateWindow(GutterClass);
        var coreWindow = CreateWindow(AppQueryExtensions.AppWindowClass);
        var frameWithChild = CreateWindow(AppQueryExtensions.AppFrameWindowClass, coreWindow);
        var frameWithoutChild = CreateWindow(AppQueryExtensions.AppFrameWindowClass);

        await Assert.That(launcher.IsAppLauncher()).IsTrue();
        await Assert.That(gutter.IsGutter()).IsTrue();
        await Assert.That(coreWindow.IsWin8App()).IsEqualTo(WindowsVersion.IsWindows8 || WindowsVersion.IsWindows81);
        await Assert.That(coreWindow.IsWin10App()).IsEqualTo(WindowsVersion.IsWindows10OrLater);
        await Assert.That(frameWithChild.IsWin10App()).IsEqualTo(WindowsVersion.IsWindows10OrLater);
        await Assert.That(frameWithoutChild.IsBackgroundWin10App()).IsEqualTo(WindowsVersion.IsWindows10OrLater);
        await Assert.That(frameWithChild.IsApp()).IsEqualTo(WindowsVersion.IsWindows8OrLater && WindowsVersion.IsWindows10OrLater);
    }

    /// <summary>Checks app query composition seams without COM or native window dependencies.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AppQuery_CompositionSeams_CoverVisibilityLauncherAndEnumerationBranchesAsync()
    {
        var displayBounds = new NativeRect(0, 0, TestBoundsSize, TestBoundsSize);
        var partialBounds = new NativeRect(0, 0, TestBoundsSize - 1, TestBoundsSize);
        var outsideBounds = new NativeRect(TestBoundsSize, TestBoundsSize, TestBoundsSize, TestBoundsSize);
        var display = new DisplayInfo { Bounds = displayBounds };
        var appWindow = CreateWindow(AppQueryExtensions.AppWindowClass);

        await Assert.That(AppQueryExtensions.AppLauncher).IsEqualTo(IntPtr.Zero);
        await Assert.That(AppQueryExtensions.IsLauncherVisible).IsFalse();
        await Assert.That(AppQueryExtensions.WindowsStoreApps).IsNotNull();
        await Assert.That(AppQueryExtensions.AppVisible(displayBounds, null, [display], static _ => IntPtr.Zero)).IsTrue();
        await Assert.That(AppQueryExtensions.AppVisible(partialBounds, null, [display], static _ => new IntPtr(SyntheticMonitorHandle))).IsTrue();
        await Assert.That(AppQueryExtensions.AppVisible(outsideBounds, null, [display], static _ => new IntPtr(SyntheticMonitorHandle))).IsTrue();
        await Assert.That(AppQueryExtensions.GetAppLauncher(true, static () => new IntPtr(SyntheticHandle), static handle => new InteropWindow(handle))).IsNull();
        await Assert.That(AppQueryExtensions.GetAppLauncher(false, static () => IntPtr.Zero, static handle => new InteropWindow(handle))).IsNull();
        var foundLauncher = AppQueryExtensions.GetAppLauncher(false, static () => new IntPtr(SyntheticHandle), static _ => CreateWindow(AppLauncherClass));
        await Assert.That(foundLauncher).IsNotNull();
        await Assert.That(foundLauncher?.Classname).IsEqualTo(AppLauncherClass);
        await Assert.That(foundLauncher?.Handle).IsEqualTo(new IntPtr(SyntheticHandle));
        await Assert.That(AppQueryExtensions.EnumerateWindowsStoreApps(null, [appWindow], static () => new IntPtr(OtherSyntheticHandle), static handle => new InteropWindow(handle))).IsEmpty();
    }

    /// <summary>Checks window query decisions from cached metadata and one real native window.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InteropWindowQuery_CachedAndNativeMetadata_CoversTopLevelBranchesAsync()
    {
        var emptyBounds = CreateWindow("Wave3Empty", bounds: default);
        var child = CreateWindow("Wave3Child", parent: new(OtherSyntheticHandle));
        var ignored = CreateWindow(IgnoredButtonClass);

        await Assert.That(emptyBounds.IsTopLevel(false)).IsFalse();
        await Assert.That(child.IsTopLevel(false)).IsFalse();
        await Assert.That(ignored.CanIgnoreClass()).IsTrue();
        await Assert.That(ignored.IsTopLevel()).IsFalse();

        using var form = new Form
        {
            Text = "Wave3 top level",
            Width = TestFormWidth,
            Height = TestFormHeight,
            ShowInTaskbar = false,
            StartPosition = System.Windows.Forms.FormStartPosition.Manual,
            Location = new(TestFormLocation, TestFormLocation),
        };

        form.Show();
        FormsApplication.DoEvents();

        var nativeWindow = InteropWindowFactory.CreateFor(form.Handle);

        await Assert.That(nativeWindow.IsTopLevel(false)).IsFalse();
        await Assert.That(nativeWindow.IsPopup(false)).IsFalse();
    }

    /// <summary>Checks deterministic interop helper seams without private reflection.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InteropWindowExtensions_CompositionSeams_CoverRegionDisplayAndSettingsBranchesAsync()
    {
        const int rectangleCountOffset = 8;
        const int regionHeaderBytes = 32;
        const int nativeRectangleBytes = 16;
        const int rectangleLeft = 1;
        const int rectangleTop = 2;
        const int rectangleRight = 12;
        const int rectangleBottom = 14;
        const int secondRectangle = 2;
        const int rectangleBottomFieldIndex = 3;

        var emptyRegionBytes = new byte[regionHeaderBytes + nativeRectangleBytes];
        var truncatedRegionBytes = new byte[regionHeaderBytes + nativeRectangleBytes - 1];
        WriteInt32(truncatedRegionBytes, rectangleCountOffset, 1);

        using var truncatedRegion = InteropWindowExtensions.CreateRegionFromData(truncatedRegionBytes);
        await Assert.That(InteropWindowExtensions.CreateRegionFromData(emptyRegionBytes)).IsNull();
        await Assert.That(truncatedRegion).IsNotNull();

        var regionBytes = new byte[regionHeaderBytes + (nativeRectangleBytes * secondRectangle)];
        WriteInt32(regionBytes, rectangleCountOffset, secondRectangle);
        WriteInt32(regionBytes, regionHeaderBytes, rectangleLeft);
        WriteInt32(regionBytes, regionHeaderBytes + sizeof(int), rectangleTop);
        WriteInt32(regionBytes, regionHeaderBytes + (sizeof(int) * secondRectangle), rectangleRight);
        WriteInt32(regionBytes, regionHeaderBytes + (sizeof(int) * rectangleBottomFieldIndex), rectangleBottom);

        using var region = InteropWindowExtensions.CreateRegionFromData(regionBytes);
        using var bitmap = new Bitmap(TestBoundsSize, TestBoundsSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Red);

        InteropWindowExtensions.ApplyRegionTransparency(null, graphics);
        InteropWindowExtensions.ApplyRegionTransparency(truncatedRegion, graphics);
        InteropWindowExtensions.ApplyRegionTransparency(region, graphics);

        var primary = new DisplayInfo { IsPrimary = true, Bounds = new(rectangleLeft, rectangleTop, rectangleRight, rectangleBottom) };
        var secondary = new DisplayInfo { IsPrimary = false, Bounds = new(0, 0, rectangleRight, rectangleBottom) };

        await Assert.That(region).IsNotNull();
        await Assert.That(InteropWindowExtensions.FindPrimaryDisplay([secondary, primary])).IsSameReferenceAs(primary);
        await Assert.That(InteropWindowExtensions.FindPrimaryDisplay([secondary])).IsSameReferenceAs(secondary);
        await Assert.That(InteropWindowExtensions.FindPrimaryDisplay([])).IsNull();
        await Assert.That(InteropWindowExtensions.GetCapturePixelFormat(null)).IsEqualTo(System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        await Assert.That(InteropWindowExtensions.GetCapturePixelFormat(region)).IsEqualTo(System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        await Assert.That(InteropWindowExtensions.HasSetting(InteropWindowRetrieveSettings.Info, InteropWindowRetrieveSettings.Info)).IsTrue();
        await Assert.That(InteropWindowExtensions.HasSetting(InteropWindowRetrieveSettings.Info, InteropWindowRetrieveSettings.Text)).IsFalse();
        InteropWindowExtensions.ValidateRetrieveSettings(InteropWindowRetrieveSettings.Info);
        await Assert
            .That(static () => InteropWindowExtensions.ValidateRetrieveSettings(InteropWindowRetrieveSettings.Children | InteropWindowRetrieveSettings.ZOrderedChildren))
            .Throws<ArgumentException>();
    }

    /// <summary>Checks cached and handle-mutating interop extension paths without requiring a valid native window.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InteropWindowExtensions_CachedAndMutationPaths_CoverPublicBranchesAsync()
    {
        const int dockingGap = 1;
        const int dockedWindowHeight = 30;
        const int leftWindowTop = 0;
        const int movePointX = 1;
        const int movePointY = 2;
        const int rightWindowTop = 10;
        const int rightWindowLeft = TestBoundsSize + dockingGap;

        var child = CreateWindow("Wave3Child");
        var parent = CreateWindow("Wave3Parent", child);
        parent.ParentWindow = child;
        parent.Placement = WindowPlacement.Create();
        parent.CanScroll = false;
        parent.HasZOrderedChildren = true;

        await Assert.That(parent.GetParentWindow()).IsSameReferenceAs(child);
        await Assert.That(parent.GetPlacement()).IsEqualTo(parent.Placement.Value);
        await Assert.That(parent.GetText()).IsEqualTo(parent.Text);
        await Assert.That(parent.GetWindowScroller()).IsNull();
        await Assert.That(parent.GetZOrderedChildren()).IsSameReferenceAs(parent.Children);

        var left = CreateWindow("Wave3Left", bounds: new(0, leftWindowTop, TestBoundsSize, dockedWindowHeight));
        var right = CreateWindow("Wave3Right", bounds: new(rightWindowLeft, rightWindowTop, TestBoundsSize, dockedWindowHeight));
        await Assert.That(left.IsDockedToLeftOf(right, static window => window.Info.Value.Bounds)).IsTrue();
        await Assert.That(right.IsDockedToRightOf(left, static window => window.Info.Value.Bounds)).IsTrue();

        parent.IsVisible = false;
        await parent.ToForegroundAsync();

        _ = parent.Maximize();
        await Assert.That(parent.IsMaximized).IsTrue();
        await Assert.That(parent.IsMinimized).IsFalse();

        _ = parent.Minimize();
        await Assert.That(parent.IsMinimized).IsTrue();

        _ = parent.Restore();
        await Assert.That(parent.IsMinimized).IsFalse();
        await Assert.That(parent.IsMaximized).IsFalse();

        _ = parent.SetExtendedStyle(ExtendedWindowStyleFlags.WS_EX_TOOLWINDOW);
        await Assert.That(parent.Info).IsNull();

        parent.Info = WindowInfo.Create();
        _ = parent.SetStyle(WindowStyleFlags.WS_VISIBLE);
        await Assert.That(parent.Info).IsNull();

        var placement = WindowPlacement.Create();
        _ = parent.SetPlacement(placement);
        await Assert.That(parent.Placement).IsEqualTo(placement);

        parent.Info = WindowInfo.Create();
        _ = parent.MoveTo(new(movePointX, movePointY));
        await Assert.That(parent.Info).IsNull();
    }

    /// <summary>Checks InteropWindow container overloads and equality behavior.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InteropWindow_DumpAndEquality_CoverContainerBranchesAsync()
    {
        var child = CreateWindow(DumpChildClass);
        var window = CreateWindow("Wave3Dump", child);
        var sameHandle = CreateWindow("Wave3DumpSameHandle");
        var otherHandle = new InteropWindow(new(OtherSyntheticHandle));

        var emptyDump = window.Dump(InteropWindowRetrieveSettings.None);
        var suppliedDump = window.Dump(InteropWindowRetrieveSettings.Classname | InteropWindowRetrieveSettings.Caption, new(), "  ");
        var childDump = window.Dump(InteropWindowRetrieveSettings.Children | InteropWindowRetrieveSettings.Classname, new(), string.Empty);

        window.HasZOrderedChildren = true;
        var orderedDump = window.Dump(InteropWindowRetrieveSettings.ZOrderedChildren | InteropWindowRetrieveSettings.Classname, new(), string.Empty);

        await Assert.That(emptyDump.ToString()).Contains(nameof(InteropWindow.Handle));
        await Assert.That(suppliedDump.ToString()).Contains(nameof(InteropWindow.Classname));
        await Assert.That(childDump.ToString()).Contains(DumpChildClass);
        await Assert.That(orderedDump.ToString()).Contains(DumpChildClass);
        await Assert.That(window.Equals((IInteropWindow)sameHandle)).IsTrue();
        await Assert.That(window.Equals((IInteropWindow)otherHandle)).IsFalse();
        await Assert.That(window.Equals((object)sameHandle)).IsTrue();
        await Assert.That(window.Equals(new object())).IsFalse();
        await Assert.That(window.GetHashCode()).IsEqualTo(new IntPtr(SyntheticHandle).GetHashCode());
    }

    /// <summary>Checks non-owning safe handle factories.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SafeHandleFactories_FromUnowned_ReportExpectedValidityAsync()
    {
        using var nativeWindow = SafeNativeWindowHandle.FromUnowned(new(SyntheticHandle));
        using var zeroWindow = SafeNativeWindowHandle.FromUnowned(IntPtr.Zero);
        using var invalidNativeWindow = SafeNativeWindowHandle.CreateInvalid();
        using var eventHook = SafeWinEventHookHandle.FromUnowned(new(SyntheticHandle));
        using var invalidEventHook = SafeWinEventHookHandle.CreateInvalid();

        await Assert.That(nativeWindow.IsInvalid).IsFalse();
        await Assert.That(zeroWindow.IsInvalid).IsTrue();
        await Assert.That(invalidNativeWindow.IsInvalid).IsTrue();
        await Assert.That(invalidNativeWindow.TryRelease()).IsTrue();
        await Assert.That(eventHook.IsInvalid).IsFalse();
        await Assert.That(invalidEventHook.IsInvalid).IsTrue();
        await Assert.That(invalidEventHook.TryRelease()).IsTrue();
    }

    /// <summary>Checks additional scroller paths against a temporary hidden RichTextBox.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowScroller_RichTextBox_CoversEndAndAbsoluteBranchesAsync()
    {
        using var form = new Form
        {
            Width = TestFormWidth,
            Height = TestFormHeight,
            ShowInTaskbar = false,
            StartPosition = System.Windows.Forms.FormStartPosition.Manual,
            Location = new(TestFormLocation, TestFormLocation),
        };
        using var editor = new System.Windows.Forms.RichTextBox
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical,
            Lines = CreateLines(),
        };

        form.Controls.Add(editor);
        form.Show();
        FormsApplication.DoEvents();

        var scroller = InteropWindowFactory.CreateFor(editor.Handle).GetWindowScroller(true);
        await Assert.That(scroller).IsNotNull();
        if (scroller is null)
        {
            return;
        }

        scroller.ScrollMode = ScrollModes.AbsoluteWindowMessage;
        scroller.ShowChanges = false;
        await Assert.That(scroller.Start()).IsTrue();
        await Assert.That(scroller.Next()).IsTrue();
        await Assert.That(scroller.Previous()).IsTrue();
        await Assert.That(scroller.End()).IsTrue();

        scroller.KeepInitialBounds = false;
        await Assert.That(scroller.IsAtEnd || scroller.GetPosition(out _)).IsTrue();
    }

    /// <summary>Checks WinEventHook error handling with an invalid event range.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinEventHook_Create_InvalidEventRange_PublishesErrorAsync()
    {
        var completion = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = WinEventHook
            .ObserveWinEvents(WinEvents.EVENT_OBJECT_DESTROY, WinEvents.EVENT_OBJECT_CREATE)
            .Subscribe(new ErrorObserver(completion));

        var completedTask = await Task.WhenAny(completion.Task, Task.Delay(TimeSpan.FromSeconds(TestTimeoutSeconds)));
        await Assert.That(completedTask).IsSameReferenceAs(completion.Task);
        var exception = await completion.Task;

        await Assert.That(exception).IsTypeOf<Win32Exception>();

        await Assert.That(WinEventHook.ObserveWinEvents(WinEvents.EVENT_OBJECT_CREATE, WinEvents.EVENT_OBJECT_DESTROY, CurrentProcessId)).IsNotNull();
        await Assert.That(WinEventHook.IsWindowObject(WinEventInfo.Create(IntPtr.Zero, WinEvents.EVENT_OBJECT_CREATE, IntPtr.Zero, ObjectIdentifiers.Window, 0, 0, 0))).IsTrue();
        await Assert.That(WinEventHook.IsWindowObject(WinEventInfo.Create(IntPtr.Zero, WinEvents.EVENT_OBJECT_CREATE, IntPtr.Zero, ObjectIdentifiers.Client, 0, 0, 0))).IsFalse();
    }

    /// <summary>Creates enough editor lines to force a vertical scrollbar.</summary>
    /// <returns>The editor line values.</returns>
    private static string[] CreateLines()
    {
        var lines = new string[TestLineCount];
        for (var index = 0; index < lines.Length; index++)
        {
            lines[index] = $"Wave3 line {index}";
        }

        return lines;
    }

    /// <summary>Writes a 32-bit integer into a byte buffer.</summary>
    /// <param name="target">Target byte buffer.</param>
    /// <param name="offset">Byte offset.</param>
    /// <param name="value">Value to write.</param>
    private static void WriteInt32(byte[] target, int offset, int value) =>
        BitConverter.GetBytes(value).CopyTo(target.AsSpan(offset));

    /// <summary>Creates a cached interop window with public container state only.</summary>
    /// <param name="className">Class name to expose from the window.</param>
    /// <param name="child">Optional child window.</param>
    /// <param name="bounds">Optional cached bounds.</param>
    /// <param name="parent">Optional parent handle.</param>
    /// <returns>The configured interop window.</returns>
    private static InteropWindow CreateWindow(
        string className,
        IInteropWindow child = null,
        NativeRect? bounds = null,
        IntPtr? parent = null)
    {
        var info = WindowInfo.Create();
        info.Bounds = bounds ?? new NativeRect(0, 0, TestBoundsSize, TestBoundsSize);
        info.ClientBounds = info.Bounds;

        return new(new IntPtr(SyntheticHandle))
        {
            Caption = className,
            Children = child is null ? Array.Empty<IInteropWindow>() : [child],
            Classname = className,
            Info = info,
            IsMaximized = false,
            IsMinimized = false,
            IsVisible = true,
            Parent = parent ?? IntPtr.Zero,
            Placement = WindowPlacement.Create(),
            ProcessId = CurrentProcessId,
            Text = className,
            ThreadId = Environment.CurrentManagedThreadId,
        };
    }

    /// <summary>Observer that exposes the first hook error through a task completion source.</summary>
    /// <param name="completion">Completion source for the first error.</param>
    private sealed class ErrorObserver(TaskCompletionSource<Exception> completion) : IObserver<WinEventInfo>
    {
        /// <inheritdoc />
        public void OnCompleted()
        {
        }

        /// <inheritdoc />
        public void OnError(Exception error) => completion.TrySetResult(error);

        /// <inheritdoc />
        public void OnNext(WinEventInfo value) => _ = value;
    }
}
