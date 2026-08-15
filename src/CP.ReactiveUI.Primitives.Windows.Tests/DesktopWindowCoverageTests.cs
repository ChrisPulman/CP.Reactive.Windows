// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using CP.ReactiveUI.Primitives.Windows.Desktop.Windows.Enums;
namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Additional desktop window coverage for interop windows and native window helpers.</summary>
public class DesktopWindowCoverageTests
{
    /// <summary>Defines the test form height.</summary>
    private const int TestFormHeight = 180;

    /// <summary>Defines the test form width.</summary>
    private const int TestFormWidth = 260;

    /// <summary>Defines the expected single item count.</summary>
    private const int TestValue1 = 1;

    /// <summary>Defines the expected child control width.</summary>
    private const int TestValue80 = 80;

    /// <summary>Defines the expected child control height.</summary>
    private const int TestValue24 = 24;

    /// <summary>Defines a test timeout in seconds.</summary>
    private const int TestValue2 = 2;

    /// <summary>Defines a small form location coordinate.</summary>
    private const int TestValue30 = 30;

    /// <summary>Defines a WPF window location coordinate.</summary>
    private const int TestValue40 = 40;

    /// <summary>Defines a test event thread value.</summary>
    private const ulong TestValue42 = 42;

    /// <summary>Defines a test event time value.</summary>
    private const ulong TestValue84 = 84;

    /// <summary>Defines a synthetic bounds X coordinate.</summary>
    private const int TestValue10 = 10;

    /// <summary>Defines a synthetic bounds Y coordinate.</summary>
    private const int TestValue20 = 20;

    /// <summary>Defines a synthetic bounds width.</summary>
    private const int TestValue100 = 100;

    /// <summary>Defines a synthetic bounds height.</summary>
    private const int TestValue120 = 120;

    /// <summary>Defines a synthetic client X coordinate.</summary>
    private const int TestValue12 = 12;

    /// <summary>Defines a synthetic client Y coordinate.</summary>
    private const int TestValue22 = 22;

    /// <summary>Defines a synthetic client height.</summary>
    private const int TestValue90 = 90;

    /// <summary>Defines a synthetic handle value.</summary>
    private const int SyntheticHandle = 1234;

    /// <summary>Defines a second synthetic handle value.</summary>
    private const int OtherSyntheticHandle = 5678;

    /// <summary>Defines the cached button class name.</summary>
    private const string ButtonClassName = nameof(Button);

    /// <summary>Gets the current process identifier on every supported target framework.</summary>
    private static int CurrentProcessId =>
#if NETFRAMEWORK
        Process.GetCurrentProcess().Id;
#else
        Environment.ProcessId;
#endif

    /// <summary>Tests cached interop window state, equality, and dump output without native calls.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InteropWindow_CachedStateEqualityAndDump_AreStableAsync()
    {
        var info = WindowInfo.Create();
        info.Bounds = new(TestValue10, TestValue20, TestValue100, TestValue120);
        info.ClientBounds = new(TestValue12, TestValue22, TestValue80, TestValue90);

        var parent = new InteropWindow(new(SyntheticHandle))
        {
            Caption = "Cached caption",
            Classname = "CachedClass",
            Text = "Cached text",
            Info = info,
            IsMaximized = false,
            IsMinimized = false,
            IsVisible = true,
            Parent = IntPtr.Zero,
            CanScroll = false,
            Children = [CreateCachedWindow(OtherSyntheticHandle, "ChildClass")],
        };

        var sameHandle = InteropWindowFactory.CreateFor(SyntheticHandle);
        var otherHandle = InteropWindowFactory.CreateFor(OtherSyntheticHandle);
        var dump = parent.Dump(
            InteropWindowRetrieveSettings.Caption
            | InteropWindowRetrieveSettings.Classname
            | InteropWindowRetrieveSettings.Text
            | InteropWindowRetrieveSettings.Info
            | InteropWindowRetrieveSettings.Maximized
            | InteropWindowRetrieveSettings.Minimized
            | InteropWindowRetrieveSettings.Visible
            | InteropWindowRetrieveSettings.Parent
            | InteropWindowRetrieveSettings.ScrollInfo,
            new(),
            "  ");

        await Assert.That(parent.HasClassname).IsTrue();
        await Assert.That(parent.HasChildren).IsTrue();
        await Assert.That(parent.HasParent).IsFalse();
        await Assert.That(parent.Equals((IInteropWindow)sameHandle)).IsTrue();
        await Assert.That(parent.Equals(otherHandle)).IsFalse();
        await Assert.That(parent.Equals((object)sameHandle)).IsTrue();
        await Assert.That(parent.GetHashCode()).IsEqualTo(SyntheticHandle.GetHashCode());
        await Assert.That(dump.ToString()).Contains("Caption=Cached caption");
        await Assert.That(dump.ToString()).Contains("Classname=CachedClass");
        await Assert.That(dump.ToString()).Contains("CanScroll=False");
    }

    /// <summary>Tests WinForms interop extension methods against a temporary window.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task FormsExtensions_AndInteropWindowExtensions_ReadTemporaryFormAsync()
    {
        using var form = CreateShownForm();
        var interopWindow = form.AsInteropWindow();
        using var printedWindow = interopWindow.PrintWindow();

        var info = interopWindow.GetInfo(true, false);
        var placement = form.RetrievePlacement();
        var appliedWindow = form.ApplyPlacement(placement);

        await interopWindow.ToForegroundAsync();

        await Assert.That(interopWindow.Exists()).IsTrue();
        await Assert.That(interopWindow.GetClassname(true)).IsNotEmpty();
        await Assert.That(interopWindow.GetParent(true)).IsEqualTo(IntPtr.Zero);
        await Assert.That(interopWindow.GetParentWindow(true)).IsNull();
        await Assert.That(interopWindow.GetProcessId(true)).IsEqualTo(CurrentProcessId);
        await Assert.That(interopWindow.IsOwnedByCurrentProcess()).IsTrue();
        await Assert.That(interopWindow.IsOwnedByCurrentThread()).IsTrue();
        await Assert.That(interopWindow.IsVisible(true)).IsTrue();
        await Assert.That(interopWindow.IsMinimized(true)).IsFalse();
        await Assert.That(interopWindow.IsMaximized(true)).IsFalse();
        await Assert.That(info.Bounds.Width > 0).IsTrue();
        await Assert.That(info.Bounds.Height > 0).IsTrue();
        await Assert.That(((IInteropWindow)appliedWindow).Handle).IsEqualTo(form.Handle);
        await Assert.That(interopWindow.GetVisibleLocation(out var visibleLocation)).IsTrue();
        await Assert.That(visibleLocation).IsEqualTo(interopWindow.GetInfo().Bounds.Location);
        await Assert.That(printedWindow is null || printedWindow.Width > 0).IsTrue();
    }

    /// <summary>Tests WPF window extension methods against a temporary window.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsExtensions_ReadTemporaryWpfWindowAsync()
    {
        var window = new Window
        {
            Width = TestFormWidth,
            Height = TestFormHeight,
            Left = TestValue40,
            Top = TestValue40,
            ShowInTaskbar = false,
            WindowStyle = WindowStyle.ToolWindow,
            Title = nameof(WindowsExtensions_ReadTemporaryWpfWindowAsync),
        };

        try
        {
            window.Show();
            var interopWindow = window.AsInteropWindow();
            var placement = window.RetrievePlacement();
            var appliedWindow = window.ApplyPlacement(placement);

            await Assert.That(window.Handle).IsNotEqualTo(IntPtr.Zero);
            await Assert.That(interopWindow.Exists()).IsTrue();
            await Assert.That(((IInteropWindow)appliedWindow).Handle).IsEqualTo(window.Handle);
            await Assert.That(interopWindow.GetInfo(true).Bounds.Width > 0).IsTrue();
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Tests parent child enumeration, z-order children, and query classification paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InteropWindowQueryExtensions_ClassifyAndEnumerateTemporaryChildrenAsync()
    {
        using var form = CreateShownForm();
        using var button = new Button { Text = "Child", Width = TestValue80, Height = TestValue24, Parent = form };
        FormsApplication.DoEvents();

        var parentWindow = form.AsInteropWindow();
        var childWindow = InteropWindowFactory.CreateFor(button.Handle);
        var children = ToArray(parentWindow.GetChildren(true));
        var orderedChildren = ToArray(parentWindow.GetZOrderedChildren(true));
        var topChildren = ToArray(InteropWindowQueryExtensions.GetTopWindows(parentWindow));
        var topLevelMatches = CountHandles(InteropWindowQueryExtensions.GetWindowsForProcess(CurrentProcessId), form.Handle);

        await Assert.That(ContainsHandle(children, button.Handle)).IsTrue();
        await Assert.That(ContainsHandle(orderedChildren, button.Handle)).IsTrue();
        await Assert.That(ContainsHandle(topChildren, button.Handle)).IsTrue();
        await Assert.That(topLevelMatches >= TestValue1).IsTrue();
        await Assert.That(childWindow.GetParent(true)).IsEqualTo(form.Handle);
        await Assert.That(childWindow.IsTopLevel(false)).IsFalse();
        await Assert.That(childWindow.IsPopup(false)).IsFalse();
        await Assert.That(CreateCachedWindow(0, ButtonClassName).CanIgnoreClass()).IsTrue();
    }

    /// <summary>Tests WindowsEnumerator predicate, take-while, observable, and empty child paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsEnumerator_PredicatesObservablesAndEmptyParent_AreHandledAsync()
    {
        using var form = CreateShownForm();
        using var button = new Button { Text = "Child", Width = TestValue80, Height = TestValue24, Parent = form };
        FormsApplication.DoEvents();

        var parentWindow = form.AsInteropWindow();
        var observedChild = new TaskCompletionSource<IntPtr>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = WindowsEnumerator.ObserveWindowHandles(form.Handle).SubscribeOnNext(handle =>
        {
            if (handle == button.Handle)
            {
                _ = observedChild.TrySetResult(handle);
            }
        });

        var filteredHandles = ToArray(WindowsEnumerator.EnumerateWindowHandles(parentWindow, handle => handle == button.Handle));
        var takenHandles = ToArray(WindowsEnumerator.EnumerateWindowHandles(parentWindow, static _ => true, static (_, count) => count < TestValue1));
        var filteredWindows = ToArray(WindowsEnumerator.EnumerateWindows(parentWindow, window => window.Handle == button.Handle));
        var takenWindows = ToArray(WindowsEnumerator.EnumerateWindows(parentWindow, static _ => true, static (_, count) => count < TestValue1));
        var emptyTopWindows = ToArray(InteropWindowQueryExtensions.GetTopWindows(InteropWindowFactory.CreateFor(button.Handle)));

        await Assert.That(Array.Exists(filteredHandles, handle => handle == button.Handle)).IsTrue();
        await Assert.That(takenHandles.Length).IsEqualTo(TestValue1);
        await Assert.That(ContainsHandle(filteredWindows, button.Handle)).IsTrue();
        await Assert.That(takenWindows.Length).IsEqualTo(TestValue1);
        var completedTask = await Task.WhenAny(observedChild.Task, Task.Delay(TimeSpan.FromSeconds(TestValue2)));
        await Assert.That(completedTask).IsSameReferenceAs(observedChild.Task);
        await Assert.That(await observedChild.Task).IsEqualTo(button.Handle);
        await Assert.That(emptyTopWindows.Length).IsEqualTo(0);
    }

    /// <summary>Tests WindowScroller false and exception paths with invalid handles.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowScroller_InvalidHandleAndUnsupportedModes_ReturnFalseOrThrowAsync()
    {
        var invalidWindow = InteropWindowFactory.CreateFor(IntPtr.Zero);
        var scroller = new WindowScroller { ScrollBarWindow = invalidWindow, ScrollingWindow = invalidWindow, ScrollBarType = ScrollBarTypes.Both };

        await Assert.That(scroller.GetPosition(out var scrollInfo)).IsFalse();
        await Assert.That(scrollInfo).IsEqualTo(ScrollInfo.Create(ScrollInfoMask.All));
        await Assert.That(scroller.IsAtStart).IsFalse();
        await Assert.That(scroller.IsAtEnd).IsFalse();
        await Assert.That(scroller.GetScrollbarInfo()).IsNull();
        await Assert.That(scroller.NeedsFocus()).IsFalse();
        await Assert.That(scroller.Start()).IsFalse();
        await Assert.That(scroller.End()).IsFalse();
        await Assert.That(scroller.Next()).IsFalse();
        await Assert.That(scroller.Previous()).IsFalse();

        scroller.ScrollMode = ScrollModes.KeyboardPageUpDown;
        await Assert.That(scroller.NeedsFocus()).IsTrue();

        scroller.ScrollMode = (ScrollModes)int.MaxValue;
        await Assert.That(() => scroller.Start()).Throws<ArgumentOutOfRangeException>();

        scroller.ScrollMode = ScrollModes.WindowsMessage;
        scroller.ScrollBarType = (ScrollBarTypes)int.MaxValue;
        await Assert.That(() => scroller.GetScrollbarInfo(true)).Throws<ArgumentOutOfRangeException>();
    }

    /// <summary>Tests WinEventInfo creation and hook observable factories.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinEventInfo_AndHookFactories_CreateExpectedValuesAsync()
    {
        using var form = CreateShownForm();
        var hookHandle = new IntPtr(SyntheticHandle);
        var eventInfo = WinEventInfo.Create(
            hookHandle,
            WinEvents.EVENT_OBJECT_NAMECHANGE,
            form.Handle,
            ObjectIdentifiers.Window,
            0,
            TestValue42,
            TestValue84);

        using var titleSubscription = WinEventHook.ObserveWindowTitleChanges().SubscribeOnNext(static _ => { });
        using var createDestroySubscription = WinEventHook.ObserveWindowLifecycleEvents().SubscribeOnNext(static _ => { });

        await Assert.That(eventInfo.EventHook).IsNotNull();
        await Assert.That(eventInfo.EventHook.IsInvalid).IsFalse();
        await Assert.That(eventInfo.Window.Handle).IsEqualTo(form.Handle);
        await Assert.That(eventInfo.ObjectIdentifier).IsEqualTo(ObjectIdentifiers.Window);
        await Assert.That(eventInfo.WinEvent).IsEqualTo(WinEvents.EVENT_OBJECT_NAMECHANGE);
        await Assert.That(eventInfo.IdChild).IsEqualTo(0);
        await Assert.That(eventInfo.IsSelf).IsTrue();
        await Assert.That(eventInfo.EventThread).IsEqualTo(TestValue42);
        await Assert.That(eventInfo.EventTime).IsEqualTo(TestValue84);
    }

    /// <summary>Creates a cached synthetic interop window.</summary>
    /// <param name="handle">The synthetic handle value.</param>
    /// <param name="className">The cached class name.</param>
    /// <returns>The cached interop window.</returns>
    private static InteropWindow CreateCachedWindow(int handle, string className)
    {
        var info = WindowInfo.Create();
        info.Bounds = new(0, 0, TestValue20, TestValue20);
        info.ClientBounds = new(0, 0, TestValue20, TestValue20);

        return new(new(handle))
        {
            Caption = className,
            Classname = className,
            Text = className,
            Info = info,
            IsMaximized = false,
            IsMinimized = false,
            IsVisible = true,
            Parent = IntPtr.Zero,
            CanScroll = false,
            Placement = WindowPlacement.Create(),
        };
    }

    /// <summary>Creates and shows a temporary form.</summary>
    /// <returns>The shown form.</returns>
    private static Form CreateShownForm()
    {
        var form = new Form
        {
            Width = TestFormWidth,
            Height = TestFormHeight,
            StartPosition = System.Windows.Forms.FormStartPosition.Manual,
            Location = new(TestValue30, TestValue30),
            Text = Guid.NewGuid().ToString(),
            ShowInTaskbar = false,
        };

        form.Show();
        FormsApplication.DoEvents();
        return form;
    }

    /// <summary>Counts windows matching the supplied handle.</summary>
    /// <param name="windows">The windows to inspect.</param>
    /// <param name="handle">The handle to find.</param>
    /// <returns>The matching handle count.</returns>
    private static int CountHandles(IEnumerable<IInteropWindow> windows, IntPtr handle)
    {
        var count = 0;
        foreach (var window in windows)
        {
            if (window.Handle == handle)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Checks whether a window collection contains a handle.</summary>
    /// <param name="windows">The windows to inspect.</param>
    /// <param name="handle">The handle to find.</param>
    /// <returns>True when the handle exists.</returns>
    private static bool ContainsHandle(IInteropWindow[] windows, IntPtr handle) =>
        Array.Exists(windows, window => window.Handle == handle);

    /// <summary>Copies an enumerable to an array without LINQ.</summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="items">The source items.</param>
    /// <returns>The copied array.</returns>
    private static T[] ToArray<T>(IEnumerable<T> items)
    {
        var results = new List<T>();
        foreach (var item in items)
        {
            results.Add(item);
        }

        return [.. results];
    }
}
