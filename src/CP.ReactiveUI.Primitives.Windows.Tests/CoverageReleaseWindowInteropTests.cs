// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Covers deterministic release paths in window interop and app queries.</summary>
public sealed class CoverageReleaseWindowInteropTests
{
    /// <summary>Defines the cached window name.</summary>
    private const string CachedName = "cached";

    /// <summary>Defines the gutter window name.</summary>
    private const string GutterName = "gutter";

    /// <summary>Defines the child window name.</summary>
    private const string ChildName = "child";

    /// <summary>Defines the left window name.</summary>
    private const string LeftName = "left";

    /// <summary>Defines the right window name.</summary>
    private const string RightName = "right";

    /// <summary>Defines the empty-test window name.</summary>
    private const string EmptyName = "empty";

    /// <summary>Defines the popup-test window name.</summary>
    private const string PopupName = "popup";

    /// <summary>Defines the top-level-test window name.</summary>
    private const string TopLevelName = "top-level";

    /// <summary>Defines the invalid-print-test window name.</summary>
    private const string InvalidPrintName = "invalid-print";

    /// <summary>Handle assigned to cached-only test windows.</summary>
    private const int CachedWindowHandle = 0x5234;

    /// <summary>The WINDOWINFO native style field offset.</summary>
    private const int WindowInfoStyleOffset = 36;

    /// <summary>Handle returned by the deterministic monitor lookup.</summary>
    private const int MonitorHandle = 0x3812;

    /// <summary>Defines the synthetic window extent.</summary>
    private const int WindowExtent = Hundred;

    /// <summary>Defines the synthetic process identifier.</summary>
    private const int ProcessIdentifier = FortyTwo;

    /// <summary>Defines the synthetic thread identifier.</summary>
    private const int ThreadIdentifier = TwentyFour;

    /// <summary>Defines the primary display origin.</summary>
    private const int PrimaryDisplayOrigin = Zero;

    /// <summary>Defines the small synthetic display extent.</summary>
    private const int SmallDisplayExtent = Ten;

    /// <summary>Defines the secondary display horizontal origin.</summary>
    private const int SecondaryDisplayHorizontalOrigin = Twenty;

    /// <summary>Defines the visible test window extent.</summary>
    private const int VisibleWindowExtent = Five;

    /// <summary>Defines the relocated test window coordinate.</summary>
    private const int RelocatedWindowCoordinate = Hundred;

    /// <summary>Defines the oversized test window extent.</summary>
    private const int OversizedWindowExtent = Twenty;

    /// <summary>Defines the positive mouse wheel delta.</summary>
    private const int PositiveMouseWheelDelta = One;

    /// <summary>Defines the synthetic mouse location horizontal coordinate.</summary>
    private const int MouseLocationHorizontalCoordinate = One;

    /// <summary>Defines the synthetic mouse location vertical coordinate.</summary>
    private const int MouseLocationVerticalCoordinate = Two;

    /// <summary>Defines the native success result for an injected mouse operation.</summary>
    private const uint MouseOperationSuccessResult = One;

    /// <summary>Defines the native no-operation result for an injected mouse operation.</summary>
    private const uint MouseOperationNoResult = Zero;

    /// <summary>Covers app visibility outcomes using only supplied display observations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AppVisible_CompositionObservations_CoversFullscreenAndMonitorBranchesAsync()
    {
        var displayBounds = new NativeRect(Zero, Zero, WindowExtent, WindowExtent);
        var display = new DisplayInfo { Bounds = displayBounds };
        var partialBounds = new NativeRect(Zero, Zero, NinetyNine, WindowExtent);
        var externalBounds = new NativeRect(WindowExtent, WindowExtent, TwoHundred, TwoHundred);

        await Assert.That(AppQueryExtensions.AppVisibleFromObservations(
            partialBounds,
            static _ => MonitorAppVisibility.MAV_NO_APP_VISIBLE,
            [display],
            static _ => IntPtr.Zero)).IsTrue();
        await Assert.That(AppQueryExtensions.AppVisibleFromObservations(displayBounds, static _ => MonitorAppVisibility.MAV_APP_VISIBLE, [display], static _ => new(MonitorHandle))).IsTrue();
        await Assert.That(AppQueryExtensions.AppVisibleFromObservations(displayBounds, static _ => MonitorAppVisibility.MAV_NO_APP_VISIBLE, [display], static _ => new(MonitorHandle))).IsFalse();
        await Assert.That(AppQueryExtensions.AppVisibleFromObservations(displayBounds, static _ => MonitorAppVisibility.MAV_APP_VISIBLE, [display], static _ => IntPtr.Zero)).IsFalse();
        await Assert.That(AppQueryExtensions.AppVisibleFromObservations(externalBounds, static _ => MonitorAppVisibility.MAV_APP_VISIBLE, [display], static _ => new(MonitorHandle))).IsFalse();
    }

    /// <summary>Covers app enumeration with supplied availability and candidate observations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsStoreApps_CompositionObservations_ReturnAppsAndGutterAsync()
    {
        var app = CreateCachedWindow(AppQueryExtensions.AppWindowClass);
        var ordinaryWindow = CreateCachedWindow("ordinary", handle: CachedWindowHandle + One);
        var gutter = CreateCachedWindow(GutterName, handle: CachedWindowHandle + Two);

        IInteropWindow[] windows = [.. AppQueryExtensions.EnumerateWindowsStoreApps(
            true,
            [app, ordinaryWindow],
            static () => new IntPtr(MonitorHandle),
            _ => gutter)];

        await Assert.That(windows).Contains(app);
        await Assert.That(windows).Contains(gutter);
        var containsOrdinaryWindow = false;
        foreach (IInteropWindow window in windows)
        {
            if (ReferenceEquals(window, ordinaryWindow))
            {
                containsOrdinaryWindow = true;
                break;
            }
        }

        await Assert.That(containsOrdinaryWindow).IsFalse();
        await Assert.That(AppQueryExtensions.EnumerateWindowsStoreApps(false, [app], static () => new IntPtr(MonitorHandle), _ => gutter)).IsEmpty();
    }

    /// <summary>Covers cached interop-window members without obtaining or changing native window state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InteropWindow_CachedMembersAndFill_UseCachedStateOnlyAsync()
    {
        var child = CreateCachedWindow(ChildName);
        var window = CreateCachedWindow(CachedName);
        window.Children = [child];
        window.HasZOrderedChildren = true;
        window.ParentWindow = child;

        var filled = window.Fill(InteropWindowRetrieveSettings.CacheAll);

        await Assert.That(filled).IsSameReferenceAs(window);
        await Assert.That(window.GetCaption()).IsEqualTo(CachedName);
        await Assert.That(window.GetChildren()).IsSameReferenceAs(window.Children);
        await Assert.That(window.GetClassname()).IsEqualTo(CachedName);
        await Assert.That(window.GetInfo()).IsEqualTo(window.Info!.Value);
        await Assert.That(window.GetParent()).IsEqualTo(IntPtr.Zero);
        await Assert.That(window.GetParentWindow()).IsSameReferenceAs(child);
        await Assert.That(window.GetPlacement()).IsEqualTo(window.Placement!.Value);
        await Assert.That(window.GetProcessId()).IsEqualTo(ProcessIdentifier);
        await Assert.That(window.GetText()).IsEqualTo(CachedName);
        await Assert.That(window.GetWindowScroller()).IsNull();
        await Assert.That(window.GetWindowScroller(ScrollBarTypes.Control)).IsNull();
        await Assert.That(window.GetZOrderedChildren()).IsSameReferenceAs(window.Children);
        await Assert.That(window.IsMaximized()).IsFalse();
        await Assert.That(window.IsMinimized()).IsFalse();
        await Assert.That(window.IsVisible()).IsTrue();
    }

    /// <summary>Covers cached docking convenience overloads without native-window interaction.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InteropWindow_CachedDockingConvenienceOverloads_AreDirectionalAsync()
    {
        var left = CreateCachedWindow(LeftName, new NativeRect(Zero, Zero, WindowExtent, Fifty));
        var right = CreateCachedWindow(RightName, new NativeRect(Hundred + One, Zero, WindowExtent, Fifty));

        await Assert.That(left.IsDockedToLeftOf(right)).IsTrue();
        await Assert.That(right.IsDockedToRightOf(left)).IsTrue();
    }

    /// <summary>Covers popup and top-level decisions using complete cached window metadata.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InteropWindowQuery_CachedStyleAndIgnoreClassBranches_AreDeterministicAsync()
    {
        var ignored = CreateCachedWindow(nameof(Button));
        var empty = CreateCachedWindow(EmptyName, default);
        var popup = CreateCachedWindow(PopupName, style: WindowStyleFlags.WS_POPUP | WindowStyleFlags.WS_VISIBLE);
        var topLevel = CreateCachedWindow(TopLevelName, style: WindowStyleFlags.WS_VISIBLE);

        await Assert.That(ignored.CanIgnoreClass()).IsTrue();
        await Assert.That(ignored.IsPopup()).IsFalse();
        await Assert.That(empty.IsPopup(false)).IsFalse();
        await Assert.That(popup.IsPopup(false)).IsTrue();
        await Assert.That(topLevel.IsTopLevel()).IsTrue();
    }

    /// <summary>Covers display placement decisions from supplied geometry without reading desktop state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task VisibleLocation_ComposedDisplayGeometry_CoversAllPlacementOutcomesAsync()
    {
        var primary = new DisplayInfo
        {
            Bounds = new(PrimaryDisplayOrigin, PrimaryDisplayOrigin, SmallDisplayExtent, SmallDisplayExtent),
            IsPrimary = true,
            WorkingArea = new(PrimaryDisplayOrigin, PrimaryDisplayOrigin, SmallDisplayExtent, SmallDisplayExtent),
        };
        var secondary = new DisplayInfo
        {
            Bounds = new(SecondaryDisplayHorizontalOrigin, PrimaryDisplayOrigin, SmallDisplayExtent, SmallDisplayExtent),
            IsPrimary = false,
            WorkingArea = new(SecondaryDisplayHorizontalOrigin, PrimaryDisplayOrigin, SmallDisplayExtent, SmallDisplayExtent),
        };
        var visibleBounds = new NativeRect(PrimaryDisplayOrigin, PrimaryDisplayOrigin, VisibleWindowExtent, VisibleWindowExtent);
        var relocatedBounds = new NativeRect(RelocatedWindowCoordinate, RelocatedWindowCoordinate, VisibleWindowExtent, VisibleWindowExtent);
        var oversizedBounds = new NativeRect(RelocatedWindowCoordinate, RelocatedWindowCoordinate, OversizedWindowExtent, OversizedWindowExtent);

        await Assert.That(InteropWindowExtensions.GetVisibleLocation(visibleBounds, [], out var noDisplayLocation)).IsFalse();
        await Assert.That(noDisplayLocation).IsEqualTo(visibleBounds.Location);
        await Assert.That(InteropWindowExtensions.GetVisibleLocation(visibleBounds, [primary], out var alreadyVisibleLocation)).IsTrue();
        await Assert.That(alreadyVisibleLocation).IsEqualTo(visibleBounds.Location);
        await Assert.That(InteropWindowExtensions.GetVisibleLocation(relocatedBounds, [primary, secondary], out var relocatedLocation)).IsTrue();
        await Assert.That(relocatedLocation).IsEqualTo(primary.Bounds.Location);
        await Assert.That(InteropWindowExtensions.GetVisibleLocation(oversizedBounds, [primary, secondary], out var unavailableLocation)).IsFalse();
        await Assert.That(unavailableLocation).IsEqualTo(oversizedBounds.Location);
    }

    /// <summary>Covers the failed print path with an invalid synthetic handle and no real window interaction.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task PrintWindow_InvalidSyntheticHandle_ReturnsNullAsync()
    {
        var window = CreateCachedWindow(InvalidPrintName);
        window = new(IntPtr.Zero)
        {
            Caption = window.Caption,
            Children = window.Children,
            Classname = window.Classname,
            Info = window.Info,
            IsMaximized = window.IsMaximized,
            IsMinimized = window.IsMinimized,
            IsVisible = window.IsVisible,
            Parent = window.Parent,
            Placement = window.Placement,
            ProcessId = window.ProcessId,
            Text = window.Text,
            ThreadId = window.ThreadId,
        };

        using var printedWindow = window.PrintWindow();

        await Assert.That(printedWindow).IsNull();
    }

    /// <summary>Covers default scroller-operation mouse-wheel branching without generating input.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowScrollerOperations_InjectedMouseWheel_CoversInputDecisionBranchesAsync()
    {
        var operationCalls = new List<(int Delta, NativePoint? Location)>();
        var operations = new WindowScroller.WindowScrollerOperations((delta, location) =>
        {
            operationCalls.Add((delta, location));
            return MouseOperationSuccessResult;
        });

        await Assert.That(operations.MoveMouseWheel(Zero, null)).IsEqualTo(MouseOperationNoResult);
        await Assert.That(operations.MoveMouseWheel(PositiveMouseWheelDelta, null)).IsEqualTo(MouseOperationSuccessResult);
        await Assert.That(operations.MoveMouseWheel(Zero, new NativePoint(MouseLocationHorizontalCoordinate, MouseLocationVerticalCoordinate))).IsEqualTo(MouseOperationSuccessResult);
        await Assert.That(operationCalls.Count).IsEqualTo(Two);
        await Assert.That(operationCalls[Zero].Delta).IsEqualTo(PositiveMouseWheelDelta);
        await Assert.That(operationCalls[One].Location).IsEqualTo(new NativePoint(MouseLocationHorizontalCoordinate, MouseLocationVerticalCoordinate));
        await Assert.That(static () => new WindowScroller.WindowScrollerOperations(null)).Throws<ArgumentNullException>();
    }

    /// <summary>Creates an interop window whose public cache is complete for cache-only operations.</summary>
    /// <param name="name">Cached text and class name.</param>
    /// <param name="bounds">Optional cached bounds.</param>
    /// <param name="handle">The synthetic native window handle.</param>
    /// <param name="style">The cached native window style.</param>
    /// <returns>A fully cached window wrapper.</returns>
    private static InteropWindow CreateCachedWindow(
        string name,
        NativeRect? bounds = null,
        int handle = CachedWindowHandle,
        WindowStyleFlags style = default)
    {
        var resolvedBounds = bounds ?? new NativeRect(Zero, Zero, WindowExtent, WindowExtent);
        var info = CreateWindowInfo(resolvedBounds, style);

        return new(new(handle))
        {
            CanScroll = false,
            Caption = name,
            Children = [],
            Classname = name,
            Info = info,
            IsMaximized = false,
            IsMinimized = false,
            IsVisible = true,
            Parent = IntPtr.Zero,
            Placement = WindowPlacement.Create(),
            ProcessId = ProcessIdentifier,
            Text = name,
            ThreadId = ThreadIdentifier,
        };
    }

    /// <summary>Creates cached native window information with deterministic readonly native style storage.</summary>
    /// <param name="bounds">The cached window and client bounds.</param>
    /// <param name="style">The native style value.</param>
    /// <returns>The window information.</returns>
    private static WindowInfo CreateWindowInfo(NativeRect bounds, WindowStyleFlags style)
    {
        var info = WindowInfo.Create();
        info.Bounds = bounds;
        info.ClientBounds = bounds;

        if (style == default)
        {
            return info;
        }

        var size = Marshal.SizeOf<WindowInfo>();
        var nativeInfo = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(info, nativeInfo, false);
            Marshal.WriteInt32(nativeInfo, WindowInfoStyleOffset, unchecked((int)(uint)style));
            return Marshal.PtrToStructure<WindowInfo>(nativeInfo);
        }
        finally
        {
            Marshal.FreeHGlobal(nativeInfo);
        }
    }
}
