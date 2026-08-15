// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>Extensions for interop windows; get and set members update the supplied window cache.</summary>
#if NETFRAMEWORK
public static class InteropWindowExtensions
#else
public static partial class InteropWindowExtensions
#endif
{
    /// <summary>Delta reported for one Windows mouse-wheel detent.</summary>
    private const int MouseWheelDelta = 120;

    /// <summary>Scale factor used by WM_HSCROLL thumb positioning.</summary>
    private const int HorizontalThumbPositionScale = 65_536;

    /// <summary>Base value used by WM_HSCROLL thumb positioning.</summary>
    private const int HorizontalThumbPositionBase = 4;

    /// <summary>Bit shift used by WM_VSCROLL thumb positioning.</summary>
    private const int VerticalThumbPositionShift = 16;

    /// <summary>Byte offset for the RGNDATA rectangle count.</summary>
    private const int RegionDataRectangleCountOffset = 8;

    /// <summary>Byte size of an RGNDATAHEADER structure.</summary>
    private const int RegionDataHeaderSize = 32;

    /// <summary>Byte size of a native RECT structure.</summary>
    private const int NativeRectangleByteSize = 16;

    /// <summary>Byte offset for the left value in a native RECT.</summary>
    private const int NativeRectangleLeftOffset = 0;

    /// <summary>Byte offset for the top value in a native RECT.</summary>
    private const int NativeRectangleTopOffset = 4;

    /// <summary>Byte offset for the right value in a native RECT.</summary>
    private const int NativeRectangleRightOffset = 8;

    /// <summary>Byte offset for the bottom value in a native RECT.</summary>
    private const int NativeRectangleBottomOffset = 12;

    /// <summary>Logger for native window operations.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(InteropWindowExtensions));

    /// <summary>Composable native operations used by interop-window extensions.</summary>
    private static InteropWindowOperations _operations = new();

    /// <summary>Provides cached native-window operations.</summary>
    /// <param name="interopWindow">The native window to operate on.</param>
    extension(IInteropWindow interopWindow)
    {
        /// <summary>Tests if the interopWindow still exists.</summary>
        /// <returns>
        ///     True if the window still exists. Window handles are recycled, so the handle may point to a different window.
        /// </returns>
        public bool Exists() => User32Api.IsWindow(interopWindow.Handle);

        /// <summary>Fill all supported information of the InteropWindow.</summary>
        /// <returns>IInteropWindow for fluent calls.</returns>
        public IInteropWindow Fill() => interopWindow.Fill(InteropWindowRetrieveSettings.CacheAllAutoCorrect);

        /// <summary>Fill selected information of the InteropWindow.</summary>
        /// <param name="retrieveSettings">InteropWindowRetrieveSettings to specify which information is retrieved.</param>
        /// <returns>IInteropWindow for fluent calls.</returns>
        public IInteropWindow Fill(InteropWindowRetrieveSettings retrieveSettings)
        {
            ValidateRetrieveSettings(retrieveSettings);
            bool forceUpdate = HasSetting(retrieveSettings, InteropWindowRetrieveSettings.ForceUpdate);
            bool autoCorrect = HasSetting(retrieveSettings, InteropWindowRetrieveSettings.AutoCorrectValues);
            FillCachedState(interopWindow, retrieveSettings, forceUpdate, autoCorrect);
            FillRelationshipState(interopWindow, retrieveSettings, forceUpdate);
            return interopWindow;
        }

        /// <summary>Get the Windows caption title.</summary>
        /// <returns>string with the caption.</returns>
        public string GetCaption() => interopWindow.GetCaption(forceUpdate: false);

        /// <summary>Get the Windows caption title.</summary>
        /// <param name="forceUpdate">set to true to make sure the value is updated.</param>
        /// <returns>string with the caption.</returns>
        public string GetCaption(bool forceUpdate)
        {
            if (interopWindow.Caption is not null && !forceUpdate)
            {
                return interopWindow.Caption;
            }

            if (interopWindow.IsOwnedByCurrentThread())
            {
                interopWindow.Caption = string.Empty;
                Log.WarnFormat("Do not call GetWindowText for a Window ({0}) which belongs the current thread. An empty string is returned.", interopWindow.Handle);
            }
            else
            {
                interopWindow.Caption = User32Api.GetText(interopWindow.Handle);
            }

            return interopWindow.Caption;
        }

        /// <summary>Get the children of the specified interopWindow; this is not lazy.</summary>
        /// <returns>IEnumerable with InteropWindow.</returns>
        public IEnumerable<IInteropWindow> GetChildren() => interopWindow.GetChildren(forceUpdate: false);

        /// <summary>Get the children of the specified interopWindow; this is not lazy.</summary>
        /// <param name="forceUpdate">True to force updating.</param>
        /// <returns>IEnumerable with InteropWindow.</returns>
        public IEnumerable<IInteropWindow> GetChildren(bool forceUpdate)
        {
            if (interopWindow.HasChildren && !interopWindow.HasZOrderedChildren && !forceUpdate)
            {
                return interopWindow.Children;
            }

            interopWindow.HasZOrderedChildren = false;
            List<IInteropWindow> children = [];
            interopWindow.Children = children;
            foreach (IInteropWindow child in WindowsEnumerator.EnumerateWindows(interopWindow))
            {
                child.ParentWindow = interopWindow;
                children.Add(child);
            }

            return children;
        }

        /// <summary>Get the Windows class name.</summary>
        /// <returns>string with the class name.</returns>
        public string GetClassname() => interopWindow.GetClassname(forceUpdate: false);

        /// <summary>Get the Windows class name.</summary>
        /// <param name="forceUpdate">set to true to make sure the value is updated.</param>
        /// <returns>string with the class name.</returns>
        public string GetClassname(bool forceUpdate)
        {
            if (interopWindow.Classname is not null && !forceUpdate)
            {
                return interopWindow.Classname;
            }

            interopWindow.Classname = User32Api.GetClassname(interopWindow.Handle);
            return interopWindow.Classname;
        }

        /// <summary>Get the WindowInfo.</summary>
        /// <returns>WindowInfo.</returns>
        public WindowInfo GetInfo() => interopWindow.GetInfo(forceUpdate: false, autoCorrect: true);

        /// <summary>Get the WindowInfo.</summary>
        /// <param name="forceUpdate">set to true to make sure the value is updated.</param>
        /// <returns>WindowInfo.</returns>
        public WindowInfo GetInfo(bool forceUpdate) => interopWindow.GetInfo(forceUpdate, autoCorrect: true);

        /// <summary>Get the WindowInfo.</summary>
        /// <param name="forceUpdate">set to true to make sure the value is updated.</param>
        /// <param name="autoCorrect">enable auto correction, e.g. have the bounds cropped to the parent.</param>
        /// <returns>WindowInfo.</returns>
        public WindowInfo GetInfo(bool forceUpdate, bool autoCorrect)
        {
            if (interopWindow.Info.HasValue && !forceUpdate)
            {
                return interopWindow.Info.Value;
            }

            WindowInfo windowInfo = WindowInfo.Create();
            _ = User32Api.GetWindowInfo(interopWindow.Handle, ref windowInfo);
            if (autoCorrect)
            {
                if (DwmApi.IsDwmEnabled
                    && DwmApi.GetExtendedFrameBounds(interopWindow.Handle, out var extendedFrameBounds)
                    && (interopWindow.IsApp()
                        || (WindowsVersion.IsWindows10OrLater && !interopWindow.IsMaximized())))
                {
                    windowInfo.Bounds = extendedFrameBounds;
                }

                IInteropWindow parentWindow = interopWindow.GetParentWindow();
                if (interopWindow.HasParent)
                {
                    WindowInfo parentInfo = parentWindow.GetInfo(forceUpdate);
                    windowInfo.Bounds = windowInfo.Bounds.Intersect(parentInfo.Bounds);
                    windowInfo.ClientBounds = windowInfo.ClientBounds.Intersect(parentInfo.ClientBounds);
                }
            }

            interopWindow.Info = windowInfo;
            return windowInfo;
        }

        /// <summary>Get the parent handle.</summary>
        /// <returns>IntPtr for the parent.</returns>
        public IntPtr GetParent() => interopWindow.GetParent(forceUpdate: false);

        /// <summary>Get the parent handle.</summary>
        /// <param name="forceUpdate">set to true to make sure the value is updated.</param>
        /// <returns>IntPtr for the parent.</returns>
        public IntPtr GetParent(bool forceUpdate)
        {
            if (interopWindow.Parent.HasValue && !forceUpdate)
            {
                return interopWindow.Parent.Value;
            }

            IntPtr parent = Volatile.Read(ref _operations).GetParent(interopWindow.Handle);
            IInteropWindow parentWindow = interopWindow.ParentWindow;
            if (parentWindow is null || parentWindow.Handle != parent)
            {
                interopWindow.ParentWindow = null;
            }

            interopWindow.Parent = parent;
            return interopWindow.Parent.Value;
        }

        /// <summary>Get the parent IInteropWindow.</summary>
        /// <returns>IInteropWindow for the parent.</returns>
        public IInteropWindow GetParentWindow() => interopWindow.GetParentWindow(forceUpdate: false);

        /// <summary>Get the parent IInteropWindow.</summary>
        /// <param name="forceUpdate">set to true to make sure the value is updated.</param>
        /// <returns>IInteropWindow for the parent.</returns>
        public IInteropWindow GetParentWindow(bool forceUpdate)
        {
            if (interopWindow.ParentWindow is not null && !forceUpdate)
            {
                return interopWindow.ParentWindow;
            }

            IntPtr parent = interopWindow.Parent ?? interopWindow.GetParent(forceUpdate);
            interopWindow.ParentWindow = ((parent == IntPtr.Zero) ? null : InteropWindowFactory.CreateFor(parent));
            return interopWindow.ParentWindow;
        }

        /// <summary>Get the WindowPlacement.</summary>
        /// <returns>WindowPlacement.</returns>
        public WindowPlacement GetPlacement() => interopWindow.GetPlacement(forceUpdate: false);

        /// <summary>Get the WindowPlacement.</summary>
        /// <param name="forceUpdate">set to true to make sure the value is updated.</param>
        /// <returns>WindowPlacement.</returns>
        public WindowPlacement GetPlacement(bool forceUpdate)
        {
            if (interopWindow.Placement.HasValue && !forceUpdate)
            {
                return interopWindow.Placement.Value;
            }

            WindowPlacement placement = WindowPlacement.Create();
            _ = User32Api.GetWindowPlacement(interopWindow.Handle, ref placement);
            interopWindow.Placement = placement;
            return interopWindow.Placement.Value;
        }

        /// <summary>Get the process and thread which the specified window belongs to.</summary>
        /// <returns>int with process Id.</returns>
        public int GetProcessId() => interopWindow.GetProcessId(forceUpdate: false);

        /// <summary>Get the process and thread which the specified window belongs to.</summary>
        /// <param name="forceUpdate">set to true to make sure the value is updated.</param>
        /// <returns>int with process Id.</returns>
        public int GetProcessId(bool forceUpdate)
        {
            if (interopWindow.ProcessId.HasValue && !forceUpdate)
            {
                return interopWindow.ProcessId.Value;
            }

            int threadId = User32Api.GetWindowThreadProcessId(interopWindow.Handle, out var processId);
            interopWindow.ThreadId = threadId;
            interopWindow.ProcessId = processId;
            return interopWindow.ProcessId.Value;
        }

        /// <summary>Get the region for a window.</summary>
        /// <returns>The window region, or null when no region is available.</returns>
        public Region GetRegion()
        {
            InteropWindowOperations operations = Volatile.Read(ref _operations);
            using (SafeRegionHandle region = operations.CreateRectRegion())
            {
                if (region.IsInvalid)
                {
                    return null;
                }

                RegionResults result = operations.GetWindowRegion(interopWindow.Handle, region);
                if (result is not RegionResults.Error and not RegionResults.NullRegion)
                {
                    return operations.CreateRegion(region);
                }
            }

            return null;
        }

        /// <summary>Get text from the window.</summary>
        /// <returns>string with the text.</returns>
        public string GetText() => interopWindow.GetText(forceUpdate: false);

        /// <summary>Get text from the window.</summary>
        /// <param name="forceUpdate">set to true to make sure the value is updated.</param>
        /// <returns>string with the text.</returns>
        public string GetText(bool forceUpdate)
        {
            if (interopWindow.Text is not null && !forceUpdate)
            {
                return interopWindow.Text;
            }

            interopWindow.Text = User32Api.GetTextFromWindow(interopWindow.Handle);
            return interopWindow.Text;
        }

        /// <summary>Create a WindowScroller.</summary>
        /// <returns>WindowScroller or null.</returns>
        public WindowScroller GetWindowScroller() => interopWindow.GetWindowScroller(ScrollBarTypes.Vertical, forceUpdate: false);

        /// <summary>Create a WindowScroller.</summary>
        /// <param name="forceUpdate">true to force a retry, even if the previous check failed.</param>
        /// <returns>WindowScroller or null.</returns>
        public WindowScroller GetWindowScroller(bool forceUpdate) => interopWindow.GetWindowScroller(ScrollBarTypes.Vertical, forceUpdate);

        /// <summary>Create a WindowScroller.</summary>
        /// <param name="scrollBarType">ScrollBarTypes.</param>
        /// <returns>WindowScroller or null.</returns>
        public WindowScroller GetWindowScroller(ScrollBarTypes scrollBarType) => interopWindow.GetWindowScroller(scrollBarType, forceUpdate: false);

        /// <summary>Create a WindowScroller.</summary>
        /// <param name="scrollBarType">ScrollBarTypes.</param>
        /// <param name="forceUpdate">true to force a retry, even if the previous check failed.</param>
        /// <returns>WindowScroller or null.</returns>
        public WindowScroller GetWindowScroller(ScrollBarTypes scrollBarType, bool forceUpdate)
        {
            if (!forceUpdate && interopWindow.CanScroll.HasValue && !interopWindow.CanScroll.Value)
            {
                return null;
            }

            ScrollInfo initialScrollInfo = ScrollInfo.Create(ScrollInfoMask.All);
            checked
            {
                InteropWindowOperations operations = Volatile.Read(ref _operations);
                if (operations.GetScrollInfo(interopWindow.Handle, scrollBarType, ref initialScrollInfo) && initialScrollInfo.Minimum != initialScrollInfo.Maximum)
                {
                    WindowScroller result = new WindowScroller
                    {
                        ScrollingWindow = interopWindow,
                        ScrollBarWindow = interopWindow,
                        ScrollBarType = scrollBarType,
                        InitialScrollInfo = initialScrollInfo,
                        WheelDelta = (int)(MouseWheelDelta * unchecked(initialScrollInfo.PageSize / WindowScroller.ScrollWheelLinesFromRegistry)),
                    };
                    interopWindow.CanScroll = true;
                    return result;
                }

                if (operations.GetScrollInfo(interopWindow.Handle, ScrollBarTypes.Control, ref initialScrollInfo) && initialScrollInfo.Minimum != initialScrollInfo.Maximum)
                {
                    WindowScroller result2 = new WindowScroller
                    {
                        ScrollingWindow = interopWindow,
                        ScrollBarWindow = interopWindow,
                        ScrollBarType = ScrollBarTypes.Control,
                        InitialScrollInfo = initialScrollInfo,
                        WheelDelta = (int)(MouseWheelDelta * unchecked(initialScrollInfo.PageSize / WindowScroller.ScrollWheelLinesFromRegistry)),
                    };
                    interopWindow.CanScroll = true;
                    return result2;
                }

                interopWindow.CanScroll = false;
                return null;
            }
        }

        /// <summary>Get the children from top to bottom; this is not lazy.</summary>
        /// <returns>IEnumerable with InteropWindow.</returns>
        public IEnumerable<IInteropWindow> GetZOrderedChildren() => interopWindow.GetZOrderedChildren(forceUpdate: false);

        /// <summary>Get the children from top to bottom; this is not lazy.</summary>
        /// <param name="forceUpdate">True to force updating.</param>
        /// <returns>IEnumerable with InteropWindow.</returns>
        public IEnumerable<IInteropWindow> GetZOrderedChildren(bool forceUpdate)
        {
            if (interopWindow.HasChildren && interopWindow.HasZOrderedChildren && !forceUpdate)
            {
                return interopWindow.Children;
            }

            interopWindow.HasZOrderedChildren = true;
            List<IInteropWindow> children = [];
            interopWindow.Children = children;
            foreach (IInteropWindow child in InteropWindowQueryExtensions.GetTopWindows(interopWindow))
            {
                child.ParentWindow = interopWindow;
                children.Add(child);
            }

            return children;
        }

        /// <summary>Returns if this window is docked to the left of another window.</summary>
        /// <param name="otherWindow">IInteropWindow to compare against.</param>
        /// <returns>bool true if docked.</returns>
        public bool IsDockedToLeftOf(IInteropWindow otherWindow) => interopWindow.IsDockedToLeftOf(otherWindow, static (window) => window.GetInfo().Bounds);

        /// <summary>Returns if this window is docked to the left of another window.</summary>
        /// <param name="otherWindow">IInteropWindow to compare against.</param>
        /// <param name="retrieveBoundsFunc">Function which returns the bounds for the IInteropWindow.</param>
        /// <returns>bool true if docked.</returns>
        public bool IsDockedToLeftOf(IInteropWindow otherWindow, Func<IInteropWindow, NativeRect> retrieveBoundsFunc) =>
            retrieveBoundsFunc(interopWindow).IsDockedToLeftOf(retrieveBoundsFunc(otherWindow));

        /// <summary>Returns if this window is docked to the right of another window.</summary>
        /// <param name="otherWindow">IInteropWindow to compare against.</param>
        /// <returns>bool true if docked.</returns>
        public bool IsDockedToRightOf(IInteropWindow otherWindow) => interopWindow.IsDockedToRightOf(otherWindow, static (window) => window.GetInfo().Bounds);

        /// <summary>Returns if this window is docked to the right of another window.</summary>
        /// <param name="otherWindow">IInteropWindow to compare against.</param>
        /// <param name="retrieveBoundsFunc">Function which returns the bounds for the IInteropWindow.</param>
        /// <returns>bool true if docked.</returns>
        public bool IsDockedToRightOf(IInteropWindow otherWindow, Func<IInteropWindow, NativeRect> retrieveBoundsFunc) =>
            retrieveBoundsFunc(interopWindow).IsDockedToRightOf(retrieveBoundsFunc(otherWindow));

        /// <summary>Retrieve if the window is maximized.</summary>
        /// <returns>bool true if maximized.</returns>
        public bool IsMaximized() => interopWindow.IsMaximized(forceUpdate: false);

        /// <summary>Retrieve if the window is maximized.</summary>
        /// <param name="forceUpdate">set to true to make sure the value is updated.</param>
        /// <returns>bool true if maximized.</returns>
        public bool IsMaximized(bool forceUpdate)
        {
            if (!interopWindow.IsMaximized.HasValue || forceUpdate)
            {
                interopWindow.IsMaximized = User32Api.IsZoomed(interopWindow.Handle);
            }

            return interopWindow.IsMaximized.Value;
        }

        /// <summary>Retrieve if the window is minimized.</summary>
        /// <returns>bool true if minimized.</returns>
        public bool IsMinimized() => interopWindow.IsMinimized(forceUpdate: false);

        /// <summary>Retrieve if the window is minimized.</summary>
        /// <param name="forceUpdate">set to true to make sure the value is updated.</param>
        /// <returns>bool true if minimized.</returns>
        public bool IsMinimized(bool forceUpdate)
        {
            if (!interopWindow.IsMinimized.HasValue || forceUpdate)
            {
                interopWindow.IsMinimized = User32Api.IsIconic(interopWindow.Handle);
            }

            return interopWindow.IsMinimized.Value;
        }

        /// <summary>Retrieve if the window is visible and not cloaked.</summary>
        /// <returns>bool true if visible.</returns>
        public bool IsVisible() => interopWindow.IsVisible(forceUpdate: false);

        /// <summary>Retrieve if the window is visible and not cloaked.</summary>
        /// <param name="forceUpdate">set to true to make sure the value is updated.</param>
        /// <returns>bool true if visible.</returns>
        public bool IsVisible(bool forceUpdate)
        {
            if (!interopWindow.IsVisible.HasValue || forceUpdate)
            {
                interopWindow.IsVisible = User32Api.IsWindowVisible(interopWindow.Handle) && !DwmApi.IsWindowCloaked(interopWindow.Handle);
            }

            return interopWindow.IsVisible.Value;
        }

        /// <summary>Test if the window is owned by the current process.</summary>
        /// <returns>bool true if the window is owned by the current process.</returns>
        public bool IsOwnedByCurrentProcess() => Kernel32Api.GetCurrentProcessId() == interopWindow.GetProcessId();

        /// <summary>Test if the window is owned by the current thread.</summary>
        /// <returns>bool true if the window is owned by the current thread.</returns>
        public bool IsOwnedByCurrentThread()
        {
            _ = interopWindow.GetProcessId();
            return Kernel32Api.GetCurrentThreadId() == interopWindow.ThreadId;
        }

        /// <summary>Maximize the window.</summary>
        /// <returns>IInteropWindow for fluent calls.</returns>
        public IInteropWindow Maximize()
        {
            _ = User32Api.ShowWindow(interopWindow.Handle, ShowWindowCommands.Maximize);
            interopWindow.IsMaximized = true;
            interopWindow.IsMinimized = false;
            return interopWindow;
        }

        /// <summary>Minimize the Window.</summary>
        /// <returns>IInteropWindow for fluent calls.</returns>
        public IInteropWindow Minimize()
        {
            _ = User32Api.ShowWindow(interopWindow.Handle, ShowWindowCommands.Minimize);
            interopWindow.IsMinimized = true;
            return interopWindow;
        }

        /// <summary>Restore the Window.</summary>
        /// <returns>IInteropWindow for fluent calls.</returns>
        public IInteropWindow Restore()
        {
            _ = User32Api.ShowWindow(interopWindow.Handle, ShowWindowCommands.Restore);
            interopWindow.IsMinimized = false;
            interopWindow.IsMaximized = false;
            return interopWindow;
        }

        /// <summary>Set the extended window style.</summary>
        /// <param name="extendedWindowStyleFlags">ExtendedWindowStyleFlags.</param>
        /// <returns>IInteropWindow for fluent calls.</returns>
        public IInteropWindow SetExtendedStyle(ExtendedWindowStyleFlags extendedWindowStyleFlags)
        {
            _ = User32Api.SetExtendedWindowStyle(interopWindow.Handle, extendedWindowStyleFlags);
            interopWindow.Info = null;
            return interopWindow;
        }

        /// <summary>Set the window style.</summary>
        /// <param name="windowStyleFlags">WindowStyleFlags.</param>
        /// <returns>IInteropWindow for fluent calls.</returns>
        public IInteropWindow SetStyle(WindowStyleFlags windowStyleFlags)
        {
            _ = User32Api.SetWindowStyle(interopWindow.Handle, windowStyleFlags);
            interopWindow.Info = null;
            return interopWindow;
        }

        /// <summary>Set the WindowPlacement.</summary>
        /// <param name="placement">WindowPlacement.</param>
        /// <returns>IInteropWindow for fluent calls.</returns>
        public IInteropWindow SetPlacement(WindowPlacement placement)
        {
            _ = User32Api.SetWindowPlacement(interopWindow.Handle, ref placement);
            interopWindow.Placement = placement;
            return interopWindow;
        }

        /// <summary>Set the window as foreground window.</summary>
        /// <returns>A task that completes when the foreground request has been sent.</returns>
        public ValueTask ToForegroundAsync()
        {
            if (!interopWindow.IsVisible())
            {
                return default;
            }

            InteropWindowOperations operations = Volatile.Read(ref _operations);
            IntPtr foregroundWindow = operations.GetForegroundWindow();
            if (foregroundWindow == interopWindow.Handle)
            {
                return default;
            }

            if (interopWindow.IsMinimized())
            {
                _ = interopWindow.Restore();
            }

            int threadId1 = operations.GetWindowThreadProcessId(foregroundWindow);
            int threadId2 = operations.GetWindowThreadProcessId(interopWindow.Handle);
            if (threadId1 != threadId2)
            {
                _ = operations.AttachThreadInput(threadId1, threadId2, true);
                _ = operations.SetForegroundWindow(interopWindow.Handle);
                _ = operations.AttachThreadInput(threadId1, threadId2, false);
            }
            else
            {
                _ = operations.SetForegroundWindow(interopWindow.Handle);
            }

            _ = operations.BringWindowToTop(interopWindow.Handle);
            _ = operations.SetForegroundWindow(interopWindow.Handle);
            return default;
        }

        /// <summary>Move the specified window to a new location.</summary>
        /// <param name="location">NativePoint with the offset.</param>
        /// <returns>IInteropWindow for fluent calls.</returns>
        public IInteropWindow MoveTo(NativePoint location)
        {
            _ = User32Api.SetWindowPos(
                interopWindow.Handle,
                IntPtr.Zero,
                location.X,
                location.Y,
                0,
                0,
                WindowPos.SWP_NOACTIVATE | WindowPos.SWP_NOSIZE | WindowPos.SWP_NOZORDER | WindowPos.SWP_SHOWWINDOW);
            interopWindow.Info = null;
            return interopWindow;
        }

        /// <summary>Get all other windows belonging to the process that owns the specified window.</summary>
        /// <returns>IEnumerable of IInteropWindow.</returns>
        public IEnumerable<IInteropWindow> GetLinkedWindows()
        {
            int selectedProcessId = interopWindow.GetProcessId();
            foreach (IInteropWindow window in Volatile.Read(ref _operations).GetTopLevelWindows())
            {
                if (window.Handle != interopWindow.Handle && window.GetProcessId() == selectedProcessId)
                {
                    yield return window;
                }
            }
        }

        /// <summary>Get a location where this window would be visible.</summary>
        /// <param name="formLocation">NativePoint with the location where the window will fit.</param>
        /// <returns>true if a location if found, and the formLocation is also set.</returns>
        public bool GetVisibleLocation(out NativePoint formLocation)
        {
            NativeRect windowRectangle = interopWindow.GetInfo().Bounds;
            return GetVisibleLocation(windowRectangle, DisplayTopology.GetSnapshot(), out formLocation);
        }

        /// <summary>Return a bitmap representing the Window as GDI+ draws it.</summary>
        /// <returns>A bitmap capture, or null when capture fails.</returns>
        public Bitmap PrintWindow()
        {
            NativeRect windowRect = interopWindow.GetInfo().Bounds;
            Win32Exception exceptionOccurred = null;
            using Region region = interopWindow.GetRegion();
            PixelFormat pixelFormat = GetCapturePixelFormat(region);
            Bitmap printWindowBitmap = new(windowRect.Width, windowRect.Height, pixelFormat);
            using (Graphics graphics = Graphics.FromImage(printWindowBitmap))
            {
                if (!TryPrintWindow(interopWindow, graphics))
                {
                    exceptionOccurred = new();
                }

                ApplyRegionTransparency(region, graphics);
                graphics.Flush();
            }

            if (exceptionOccurred is null)
            {
                return printWindowBitmap;
            }

            Log.ErrorFormat("Error calling print window: {0}", exceptionOccurred.Message);
            printWindowBitmap.Dispose();
            return null;
        }
    }

    /// <summary>Replaces native operations for a bounded deterministic test scope.</summary>
    /// <param name="operations">The operations to use while the returned scope is alive.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static OperationsOverride OverrideOperationsForTesting(InteropWindowOperations operations)
    {
        Throw.IfNull(operations);
        return new(Interlocked.Exchange(ref _operations, operations));
    }

    /// <summary>Gets a visible location from supplied cached window and display geometry.</summary>
    /// <param name="windowRectangle">Bounds of the window to position.</param>
    /// <param name="displays">Display snapshot used to find a visible location.</param>
    /// <param name="formLocation">The selected visible location.</param>
    /// <returns>true when a location was found.</returns>
    internal static bool GetVisibleLocation(
        NativeRect windowRectangle,
        IReadOnlyList<DisplayInfo> displays,
        out NativePoint formLocation)
    {
        formLocation = windowRectangle.Location;
        DisplayInfo primaryDisplay = FindPrimaryDisplay(displays);
        if (primaryDisplay is null)
        {
            return false;
        }

        using (Region workingArea = new(primaryDisplay.Bounds))
        {
            foreach (DisplayInfo display in displays)
            {
                if (!display.IsPrimary)
                {
                    workingArea.Union(display.Bounds);
                }
            }

            if (workingArea.AreRectangleCornersVisisble(windowRectangle))
            {
                return true;
            }

            foreach (DisplayInfo display2 in displays)
            {
                Rectangle newWindowRectangle = new(display2.WorkingArea.Location, windowRectangle.Size);
                if (workingArea.AreRectangleCornersVisisble(newWindowRectangle))
                {
                    formLocation = display2.Bounds.Location;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Applies a captured window region as transparent pixels.</summary>
    /// <param name="region">The captured window region.</param>
    /// <param name="graphics">The bitmap graphics surface.</param>
    internal static void ApplyRegionTransparency(Region region, Graphics graphics)
    {
        if (region is not null && !region.IsEmpty(graphics))
        {
            graphics.ExcludeClip(region);
            graphics.Clear(Color.Transparent);
        }
    }

    /// <summary>Creates a managed region from native RGNDATA bytes.</summary>
    /// <param name="regionDataBytes">Native RGNDATA bytes.</param>
    /// <returns>The managed region.</returns>
    internal static Region CreateRegionFromData(byte[] regionDataBytes)
    {
        int rectangleCount = BitConverter.ToInt32(regionDataBytes, RegionDataRectangleCountOffset);
        if (rectangleCount <= 0)
        {
            return null;
        }

        Region region = new();
        region.MakeEmpty();
        checked
        {
            for (int index = 0; index < rectangleCount; index++)
            {
                int offset = RegionDataHeaderSize + (index * NativeRectangleByteSize);
                if (offset + NativeRectangleByteSize > regionDataBytes.Length)
                {
                    break;
                }

                int left = BitConverter.ToInt32(regionDataBytes, offset + NativeRectangleLeftOffset);
                int top = BitConverter.ToInt32(regionDataBytes, offset + NativeRectangleTopOffset);
                int right = BitConverter.ToInt32(regionDataBytes, offset + NativeRectangleRightOffset);
                int bottom = BitConverter.ToInt32(regionDataBytes, offset + NativeRectangleBottomOffset);
                region.Union(Rectangle.FromLTRB(left, top, right, bottom));
            }

            return region;
        }
    }

    /// <summary>Finds the primary display from a display snapshot.</summary>
    /// <param name="displays">The display snapshot.</param>
    /// <returns>The primary display, or null when the snapshot is empty.</returns>
    internal static DisplayInfo FindPrimaryDisplay(IReadOnlyList<DisplayInfo> displays)
    {
        DisplayInfo firstDisplay = null;
        foreach (DisplayInfo display in displays)
        {
            firstDisplay ??= display;

            if (display.IsPrimary)
            {
                return display;
            }
        }

        return firstDisplay;
    }

    /// <summary>Gets the pixel format for a window capture.</summary>
    /// <param name="region">The optional captured window region.</param>
    /// <returns>The pixel format for the capture bitmap.</returns>
    internal static PixelFormat GetCapturePixelFormat(Region region) => region is null ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppArgb;

    /// <summary>Checks whether a retrieve setting is enabled.</summary>
    /// <param name="settings">The configured settings.</param>
    /// <param name="setting">The setting to test.</param>
    /// <returns>true when the setting is enabled.</returns>
    internal static bool HasSetting(InteropWindowRetrieveSettings settings, InteropWindowRetrieveSettings setting) => (settings & setting) != 0;

    /// <summary>Validates retrieve settings before a fill operation.</summary>
    /// <param name="retrieveSettings">InteropWindowRetrieveSettings to validate.</param>
    internal static void ValidateRetrieveSettings(InteropWindowRetrieveSettings retrieveSettings)
    {
        if (HasSetting(retrieveSettings, InteropWindowRetrieveSettings.Children) && HasSetting(retrieveSettings, InteropWindowRetrieveSettings.ZOrderedChildren))
        {
            throw new ArgumentException("Can't have both Children & ZOrderedChildren", nameof(retrieveSettings));
        }
    }

    /// <summary>Reads cacheable scalar window state.</summary>
    /// <param name="interopWindow">The window cache to fill.</param>
    /// <param name="retrieveSettings">InteropWindowRetrieveSettings to specify which information is retrieved.</param>
    /// <param name="forceUpdate">true to force refresh.</param>
    /// <param name="autoCorrect">true to correct native bounds.</param>
    private static void FillCachedState(IInteropWindow interopWindow, InteropWindowRetrieveSettings retrieveSettings, bool forceUpdate, bool autoCorrect)
    {
        if (HasSetting(retrieveSettings, InteropWindowRetrieveSettings.Info))
        {
            _ = interopWindow.GetInfo(forceUpdate, autoCorrect);
        }

        if (HasSetting(retrieveSettings, InteropWindowRetrieveSettings.Caption))
        {
            _ = interopWindow.GetCaption(forceUpdate);
        }

        if (HasSetting(retrieveSettings, InteropWindowRetrieveSettings.Classname))
        {
            _ = interopWindow.GetClassname(forceUpdate);
        }

        if (HasSetting(retrieveSettings, InteropWindowRetrieveSettings.ProcessId))
        {
            _ = interopWindow.GetProcessId(forceUpdate);
        }

        if (HasSetting(retrieveSettings, InteropWindowRetrieveSettings.Parent))
        {
            _ = interopWindow.GetParent(forceUpdate);
        }

        if (HasSetting(retrieveSettings, InteropWindowRetrieveSettings.Visible))
        {
            _ = interopWindow.IsVisible(forceUpdate);
        }

        if (HasSetting(retrieveSettings, InteropWindowRetrieveSettings.Maximized))
        {
            _ = interopWindow.IsMaximized(forceUpdate);
        }

        if (HasSetting(retrieveSettings, InteropWindowRetrieveSettings.Minimized))
        {
            _ = interopWindow.IsMinimized(forceUpdate);
        }
    }

    /// <summary>Reads cacheable relationship and placement window state.</summary>
    /// <param name="interopWindow">The window cache to fill.</param>
    /// <param name="retrieveSettings">InteropWindowRetrieveSettings to specify which information is retrieved.</param>
    /// <param name="forceUpdate">true to force refresh.</param>
    private static void FillRelationshipState(IInteropWindow interopWindow, InteropWindowRetrieveSettings retrieveSettings, bool forceUpdate)
    {
        if (HasSetting(retrieveSettings, InteropWindowRetrieveSettings.ScrollInfo))
        {
            _ = interopWindow.GetWindowScroller(forceUpdate);
        }

        if (HasSetting(retrieveSettings, InteropWindowRetrieveSettings.Children))
        {
            _ = interopWindow.GetChildren(forceUpdate);
        }

        if (HasSetting(retrieveSettings, InteropWindowRetrieveSettings.ZOrderedChildren))
        {
            _ = interopWindow.GetZOrderedChildren(forceUpdate);
        }

        if (HasSetting(retrieveSettings, InteropWindowRetrieveSettings.Placement))
        {
            _ = interopWindow.GetPlacement(forceUpdate);
        }

        if (HasSetting(retrieveSettings, InteropWindowRetrieveSettings.Text))
        {
            _ = interopWindow.GetText(forceUpdate);
        }
    }

    /// <summary>Renders a window into a graphics device context.</summary>
    /// <param name="interopWindow">The window to render.</param>
    /// <param name="graphics">The graphics surface.</param>
    /// <returns>true when the window was rendered.</returns>
    private static bool TryPrintWindow(IInteropWindow interopWindow, Graphics graphics)
    {
        IntPtr deviceContext = graphics.GetHdc();
        try
        {
            return User32Api.PrintWindow(interopWindow.Handle, deviceContext, PrintWindowFlags.PW_COMPLETE);
        }
        finally
        {
            graphics.ReleaseHdc(deviceContext);
        }
    }

    /// <summary>Restores a previous operation set when disposed.</summary>
    internal sealed class OperationsOverride : IDisposable
    {
        /// <summary>The operation set to restore.</summary>
        private readonly InteropWindowOperations _previous;

        /// <summary>Tracks whether restoration has already occurred.</summary>
        private int _disposed;

        /// <summary>Initializes a new instance of the <see cref="OperationsOverride"/> class.</summary>
        /// <param name="previous">The previous operations.</param>
        internal OperationsOverride(InteropWindowOperations previous) => _previous = previous;

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _ = Interlocked.Exchange(ref _operations, _previous);
            }
        }
    }

    /// <summary>Native operations used by the interop-window extension implementation.</summary>
    internal class InteropWindowOperations
    {
        /// <summary>Creates a managed region from supplied native-region data operations.</summary>
        /// <param name="regionHandle">The region handle.</param>
        /// <param name="getSize">Gets the required native-region data size.</param>
        /// <param name="getData">Copies native-region data into a managed buffer.</param>
        /// <returns>The managed region, or null when data is unavailable.</returns>
        internal static Region CreateRegionFromHandle(
            SafeHandle regionHandle,
            Func<SafeHandle, uint> getSize,
            Func<SafeHandle, uint, byte[], uint> getData)
        {
            Throw.IfNull(regionHandle);
            Throw.IfNull(getSize);
            Throw.IfNull(getData);
            uint dataSize = getSize(regionHandle);
            if (dataSize == 0 || dataSize > int.MaxValue)
            {
                return null;
            }

            byte[] regionDataBytes = new byte[checked((int)dataSize)];
            return getData(regionHandle, dataSize, regionDataBytes) == 0
                ? null
                : CreateRegionFromData(regionDataBytes);
        }

        /// <summary>Creates an empty native region.</summary>
        /// <returns>The native region wrapper.</returns>
        internal virtual SafeRegionHandle CreateRectRegion() => Gdi32Api.CreateRectRgn(0, 0, 0, 0);

        /// <summary>Gets the current region state for a window.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="region">The target native region.</param>
        /// <returns>The native region result.</returns>
        internal virtual RegionResults GetWindowRegion(IntPtr windowHandle, SafeRegionHandle region) => User32Api.GetWindowRgn(windowHandle, region);

        /// <summary>Creates a managed region from a native region.</summary>
        /// <param name="region">The native region.</param>
        /// <returns>The managed region.</returns>
        internal virtual Region CreateRegion(SafeHandle region) =>
            CreateRegionFromHandle(
                region,
                static handle => NativeMethods.GetRegionData(handle, 0U, IntPtr.Zero),
                NativeMethods.GetRegionData);

        /// <summary>Gets scroll information for a window.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="scrollBarType">The scroll bar type.</param>
        /// <param name="scrollInfo">The target scroll information.</param>
        /// <returns>true when information was retrieved.</returns>
        internal virtual bool GetScrollInfo(IntPtr windowHandle, ScrollBarTypes scrollBarType, ref ScrollInfo scrollInfo) => User32Api.GetScrollInfo(windowHandle, scrollBarType, ref scrollInfo);

        /// <summary>Gets the foreground window.</summary>
        /// <returns>The foreground window handle.</returns>
        internal virtual IntPtr GetForegroundWindow() => User32Api.GetForegroundWindow();

        /// <summary>Gets the parent of a window.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <returns>The parent window handle.</returns>
        internal virtual IntPtr GetParent(IntPtr windowHandle) => User32Api.GetParent(windowHandle);

        /// <summary>Gets a window's owning thread.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <returns>The owning thread identifier.</returns>
        internal virtual int GetWindowThreadProcessId(IntPtr windowHandle) => User32Api.GetWindowThreadProcessId(windowHandle, IntPtr.Zero);

        /// <summary>Connects or disconnects two input queues.</summary>
        /// <param name="firstThreadId">The first thread identifier.</param>
        /// <param name="secondThreadId">The second thread identifier.</param>
        /// <param name="attach">Whether to attach rather than detach.</param>
        /// <returns>true when the operation succeeds.</returns>
        internal virtual bool AttachThreadInput(int firstThreadId, int secondThreadId, bool attach) =>
            User32Api.AttachThreadInput(firstThreadId, secondThreadId, attach ? 1 : 0) != IntPtr.Zero;

        /// <summary>Requests foreground activation.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <returns>true when the request succeeds.</returns>
        internal virtual bool SetForegroundWindow(IntPtr windowHandle) => User32Api.SetForegroundWindow(windowHandle);

        /// <summary>Moves a window to the top of its z-order.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <returns>true when the request succeeds.</returns>
        internal virtual bool BringWindowToTop(IntPtr windowHandle) => User32Api.BringWindowToTop(windowHandle);

        /// <summary>Enumerates top-level windows.</summary>
        /// <returns>The top-level windows.</returns>
        internal virtual IEnumerable<IInteropWindow> GetTopLevelWindows() => InteropWindowQueryExtensions.GetTopLevelWindows();
    }

    /// <summary>Contains native region entry points.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>Gets the region data size for a native region.</summary>
        /// <param name="regionHandle">The region safe handle.</param>
        /// <param name="dataSize">The supplied buffer size.</param>
        /// <param name="regionData">The native region data pointer.</param>
        /// <returns>The required or copied data size.</returns>
#if NETFRAMEWORK
        [DllImport("gdi32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern uint GetRegionData(SafeHandle regionHandle, uint dataSize, IntPtr regionData);
#else
        [LibraryImport("gdi32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial uint GetRegionData(SafeHandle regionHandle, uint dataSize, IntPtr regionData);
#endif

        /// <summary>Gets the region data for a native region.</summary>
        /// <param name="regionHandle">The region safe handle.</param>
        /// <param name="dataSize">The supplied buffer size.</param>
        /// <param name="regionData">The managed region data buffer.</param>
        /// <returns>The copied data size.</returns>
#if NETFRAMEWORK
        [DllImport("gdi32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern uint GetRegionData(SafeHandle regionHandle, uint dataSize, [Out] byte[] regionData);
#else
        [LibraryImport("gdi32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial uint GetRegionData(SafeHandle regionHandle, uint dataSize, [Out] byte[] regionData);
#endif
    }
}
