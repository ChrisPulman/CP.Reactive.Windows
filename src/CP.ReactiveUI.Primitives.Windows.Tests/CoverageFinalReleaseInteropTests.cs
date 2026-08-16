// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Deterministic release coverage for composable desktop interop operations.</summary>
public sealed class CoverageFinalReleaseInteropTests
{
    /// <summary>Defines a synthetic native window handle.</summary>
    private const int WindowHandleValue = 731;

    /// <summary>Defines a synthetic linked-window handle.</summary>
    private const int LinkedWindowHandleValue = 732;

    /// <summary>Defines a synthetic foreign-window handle.</summary>
    private const int ForeignWindowHandleValue = 733;

    /// <summary>Defines a synthetic owning process identifier.</summary>
    private const int ProcessIdentifier = 83;

    /// <summary>Defines the first synthetic thread identifier.</summary>
    private const int FirstThreadIdentifier = 84;

    /// <summary>Defines a data size large enough for a single native rectangle.</summary>
    private const int RegionDataSize = 48;

    /// <summary>Defines the RGNDATA rectangle-count offset.</summary>
    private const int RegionDataRectangleCountOffset = 8;

    /// <summary>Defines the first RGNDATA rectangle offset.</summary>
    private const int FirstRegionRectangleOffset = 32;

    /// <summary>Defines the second rectangle coordinate value.</summary>
    private const int SecondRegionCoordinate = 2;

    /// <summary>Defines the third rectangle coordinate value.</summary>
    private const int ThirdRegionCoordinate = 3;

    /// <summary>Defines the fourth rectangle coordinate value.</summary>
    private const int FourthRegionCoordinate = 4;

    /// <summary>Defines the native window-info style offset.</summary>
    private const int WindowInfoStyleOffset = 36;

    /// <summary>Defines the deterministic cached window width.</summary>
    private const int CachedWindowWidth = 10;

    /// <summary>Defines the deterministic cached window height.</summary>
    private const int CachedWindowHeight = 10;

    /// <summary>Defines the expected number of repeated foreground-operation requests.</summary>
    private const int RepeatedOperationRequestCount = 2;

    /// <summary>Covers region, scroller, foreground, and linked-window extension paths through composable operations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InteropWindowExtensions_ComposedOperations_CoverNativeDecisionPathsAsync()
    {
        var operations = new RecordingInteropOperations();
        using var scope = InteropWindowExtensions.OverrideOperationsForTesting(operations);
        var window = CreateWindow(WindowHandleValue);

        using var region = window.GetRegion();
        await Assert.That(region).IsNotNull();
        await Assert.That(window.GetWindowScroller(ScrollBarTypes.Vertical, true)).IsNull();

        window.IsVisible = true;
        window.IsMinimized = true;
        operations.ForegroundWindow = new(LinkedWindowHandleValue);
        await window.ToForegroundAsync();

        window.IsMinimized = false;
        await window.ToForegroundAsync();

        operations.ForegroundWindow = ((IInteropWindow)window).Handle;
        await window.ToForegroundAsync();

        var matchingParent = CreateWindow(LinkedWindowHandleValue);
        window.ParentWindow = matchingParent;
        operations.ParentWindow = ((IInteropWindow)matchingParent).Handle;
        await Assert.That(window.GetParent(true)).IsEqualTo(((IInteropWindow)matchingParent).Handle);
        operations.ParentWindow = new(ForeignWindowHandleValue);
        await Assert.That(window.GetParent(true)).IsEqualTo(new(ForeignWindowHandleValue));
        await Assert.That(window.ParentWindow).IsNull();
        operations.ParentWindow = IntPtr.Zero;
        await Assert.That(window.GetParent(true)).IsEqualTo(IntPtr.Zero);

        IInteropWindow[] linked = [.. window.GetLinkedWindows()];
        await Assert.That(linked.Length).IsEqualTo(1);
        await Assert.That(linked[0].Handle).IsEqualTo(new(LinkedWindowHandleValue));
        await Assert.That(operations.AttachCalls).IsEqualTo(FourthRegionCoordinate);
        await Assert.That(operations.ForegroundRequests).IsEqualTo(FourthRegionCoordinate);
        await Assert.That(operations.BringToTopRequests).IsEqualTo(RepeatedOperationRequestCount);

        operations.RegionResult = RegionResults.Error;
        await Assert.That(window.GetRegion()).IsNull();
    }

    /// <summary>Covers native-region-data decoding without an operating-system handle.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RegionData_ComposedReaders_CoverUnavailableAndDecodedPathsAsync()
    {
        using var handle = new NonReleasingSafeHandle();
        byte[] regionData = CreateRegionData();

        await Assert.That(
            InteropWindowExtensions.InteropWindowOperations.CreateRegionFromHandle(
                handle,
                static _ => 0U,
                static (_, _, _) => 0U)).IsNull();
        await Assert.That(
            InteropWindowExtensions.InteropWindowOperations.CreateRegionFromHandle(
                handle,
                static _ => uint.MaxValue,
                static (_, _, _) => 0U)).IsNull();
        await Assert.That(
            InteropWindowExtensions.InteropWindowOperations.CreateRegionFromHandle(
                handle,
                static _ => RegionDataSize,
                static (_, _, _) => 0U)).IsNull();

        using Region region = InteropWindowExtensions.InteropWindowOperations.CreateRegionFromHandle(
            handle,
            static _ => RegionDataSize,
            (_, _, destination) =>
            {
                regionData.CopyTo(destination, 0);
                return RegionDataSize;
            });

        await Assert.That(region).IsNotNull();
    }

    /// <summary>Covers synchronous and observable enumeration using deterministic callbacks.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsEnumerator_ComposedCallbacks_CoverFiltersAndObservationAsync()
    {
        var operations = new RecordingEnumerationOperations([new(WindowHandleValue), new(LinkedWindowHandleValue), new(ForeignWindowHandleValue)]);
        using var scope = WindowsEnumerator.OverrideOperationsForTesting(operations);

        IntPtr[] handles = [.. WindowsEnumerator.EnumerateWindowHandles(null, static handle => handle != (IntPtr)LinkedWindowHandleValue, static (_, count) => count < SecondRegionCoordinate)];
        IInteropWindow[] windows =
        [
            .. WindowsEnumerator.EnumerateWindows(
                null,
                static window => window.Handle != (IntPtr)ForeignWindowHandleValue,
                static (_, count) => count < SecondRegionCoordinate),
        ];
        var handleObserver = new CompletionObserver<IntPtr>();
        var windowObserver = new CompletionObserver<IInteropWindow>();

        using var handleSubscription = WindowsEnumerator.ObserveWindowHandles().Subscribe(handleObserver);
        using var windowSubscription = WindowsEnumerator.ObserveWindows().Subscribe(windowObserver);

        await Task.WhenAll(handleObserver.Completion.Task, windowObserver.Completion.Task);

        await Assert.That(handles.Length).IsEqualTo(SecondRegionCoordinate);
        await Assert.That(handles[0]).IsEqualTo(new(WindowHandleValue));
        await Assert.That(windows.Length).IsEqualTo(SecondRegionCoordinate);
        await Assert.That(handleObserver.Values.Count).IsEqualTo(ThirdRegionCoordinate);
        await Assert.That(windowObserver.Values.Count).IsEqualTo(ThirdRegionCoordinate);
        await Assert.That(operations.ParentHandles.Count).IsEqualTo(FourthRegionCoordinate);
    }

    /// <summary>Covers cached popup and top-level decisions that do not query the operating system.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InteropWindowQuery_CachedMetadata_CoversEmptyAndPopupBranchesAsync()
    {
        var empty = CreateWindow(WindowHandleValue, bounds: default);
        empty.Info = WindowInfo.Create();
        var popup = CreateWindow(LinkedWindowHandleValue, style: WindowStyleFlags.WS_POPUP | WindowStyleFlags.WS_VISIBLE);
        var hiddenPopup = CreateWindow(ForeignWindowHandleValue, style: WindowStyleFlags.WS_POPUP);

        await Assert.That(empty.IsPopup(false)).IsFalse();
        await Assert.That(empty.IsTopLevel(false)).IsFalse();
        await Assert.That(popup.IsPopup(false)).IsTrue();
        await Assert.That(hiddenPopup.IsPopup(false)).IsFalse();
    }

    /// <summary>Covers cache-only window state, dump overloads, and equality outcomes.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InteropWindow_CachedStateAndDumpOverloads_CoverContainerBranchesAsync()
    {
        var window = CreateWindow(WindowHandleValue);
        window.Parent = new IntPtr(LinkedWindowHandleValue);
        var peer = CreateWindow(WindowHandleValue);
        var other = CreateWindow(LinkedWindowHandleValue);

        StringBuilder dump = window.Dump(InteropWindowRetrieveSettings.CacheAll, null);

        await Assert.That(window.Handle).IsNotNull();
        await Assert.That(window.HasParent).IsTrue();
        await Assert.That(window.Equals((IInteropWindow)null)).IsFalse();
        await Assert.That(window.Equals(peer)).IsTrue();
        await Assert.That(window.Equals(other)).IsFalse();
        await Assert.That(dump.Length > 0).IsTrue();
    }

    /// <summary>Covers deterministic region, scrollbar, foreground, and enumeration operation defaults.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InteropWindowOperations_ComposedDefaults_CoverRemainingManagedPathsAsync()
    {
        var operations = new RecordingInteropOperations { ReturnInvalidRegion = true };
        using var scope = InteropWindowExtensions.OverrideOperationsForTesting(operations);
        var window = CreateWindow(WindowHandleValue);

        await Assert.That(window.GetRegion()).IsNull();

        operations.ReturnInvalidRegion = false;
        operations.ReturnControlScrollbar = true;
        window.CanScroll = null;
        WindowScroller scroller = window.GetWindowScroller(ScrollBarTypes.Vertical, forceUpdate: true);

        await Assert.That(scroller).IsNotNull();
        await Assert.That(scroller.ScrollBarType).IsEqualTo(ScrollBarTypes.Control);

        operations.ForegroundWindow = new(LinkedWindowHandleValue);
        operations.Threads[1] = operations.Threads[0];
        await window.ToForegroundAsync();
        await Assert.That(operations.ForegroundRequests).IsEqualTo(RepeatedOperationRequestCount);

        using SafeRegionHandle nativeRegion = Gdi.Gdi32Api.CreateRectRgn(0, 0, SecondRegionCoordinate, SecondRegionCoordinate);
        var nativeOperations = new InteropWindowExtensions.InteropWindowOperations();
        using Region managedRegion = nativeOperations.CreateRegion(nativeRegion);
        await Assert.That(managedRegion).IsNotNull();

        IEnumerable<IInteropWindow> topLevelWindows = nativeOperations.GetTopLevelWindows();
        await Assert.That(topLevelWindows).IsNotNull();
    }

    /// <summary>Covers cancellation before observable window enumeration invokes a callback.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsEnumerator_CancelledObservableCallbacks_StopBeforeEmissionAsync()
    {
        using var handleOperations = new BlockingEnumerationOperations(new(WindowHandleValue));
        using (WindowsEnumerator.OverrideOperationsForTesting(handleOperations))
        {
            var observer = new CompletionObserver<IntPtr>();
            IDisposable subscription = WindowsEnumerator.ObserveWindowHandles().Subscribe(observer);
            await handleOperations.WaitUntilStartedAsync();
            subscription.Dispose();
            handleOperations.Release();
            await handleOperations.WaitUntilCompletedAsync();
            await Assert.That(observer.Values).IsEmpty();
        }

        using var windowOperations = new BlockingEnumerationOperations(new(LinkedWindowHandleValue));
        using (WindowsEnumerator.OverrideOperationsForTesting(windowOperations))
        {
            var observer = new CompletionObserver<IInteropWindow>();
            IDisposable subscription = WindowsEnumerator.ObserveWindows().Subscribe(observer);
            await windowOperations.WaitUntilStartedAsync();
            subscription.Dispose();
            windowOperations.Release();
            await windowOperations.WaitUntilCompletedAsync();
            await Assert.That(observer.Values).IsEmpty();
        }
    }

    /// <summary>Covers unavailable and failed app-visibility activation without invoking COM.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AppQuery_ComposedAppVisibilityActivation_CoversUnavailableAndFailureAsync()
    {
        await Assert.That(AppQueryExtensions.GetAppLauncher()).IsNull();

        using (WindowsVersion.OverrideVersionProviderForTesting(static () => new Version(6, 1)))
        {
            await Assert.That(AppQueryExtensions.CreateAppVisibility(static _ => throw new InvalidOperationException())).IsNull();
        }

        using (WindowsVersion.OverrideVersionProviderForTesting(static () => new Version(10, 0)))
        {
            await Assert.That(AppQueryExtensions.CreateAppVisibility(static _ => throw new InvalidOperationException())).IsNull();
        }
    }

    /// <summary>Covers non-mutating foreground and monitor interop queries.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InteropWindowQuery_NativeReadOperations_ReturnWindowAndMonitorAsync()
    {
        IInteropWindow foregroundWindow = InteropWindowQueryExtensions.GetForegroundWindow();
        NativeRect monitorBounds = new(0, 0, SecondRegionCoordinate, SecondRegionCoordinate);
        IntPtr monitor = AppQueryExtensions.GetMonitor(monitorBounds);

        await Assert.That(foregroundWindow).IsNotNull();
        await Assert.That(monitor).IsNotEqualTo(IntPtr.Zero);
    }

    /// <summary>Creates deterministic native region data for one rectangle.</summary>
    /// <returns>The native region data.</returns>
    private static byte[] CreateRegionData()
    {
        var data = new byte[RegionDataSize];
        BitConverter.GetBytes(1).CopyTo(data, RegionDataRectangleCountOffset);
        BitConverter.GetBytes(1).CopyTo(data, FirstRegionRectangleOffset);
        BitConverter.GetBytes(SecondRegionCoordinate).CopyTo(data, WindowInfoStyleOffset);
        BitConverter.GetBytes(ThirdRegionCoordinate).CopyTo(data, FirstRegionRectangleOffset + sizeof(long));
        BitConverter.GetBytes(FourthRegionCoordinate).CopyTo(data, FirstRegionRectangleOffset + (sizeof(long) + sizeof(int)));
        return data;
    }

    /// <summary>Creates an interop window with complete cache-only metadata.</summary>
    /// <param name="handle">The synthetic handle.</param>
    /// <param name="bounds">The optional bounds.</param>
    /// <param name="style">The optional style.</param>
    /// <returns>The configured window.</returns>
    private static InteropWindow CreateWindow(int handle, NativeRect? bounds = null, WindowStyleFlags style = default)
    {
        var info = WindowInfo.Create();
        info.Bounds = bounds ?? new NativeRect(0, 0, CachedWindowWidth, CachedWindowHeight);
        info.ClientBounds = info.Bounds;
        if (style != default)
        {
            info = WithStyle(info, style);
        }

        return new(new(handle))
        {
            Caption = nameof(CoverageFinalReleaseInteropTests),
            Children = [],
            Classname = nameof(CoverageFinalReleaseInteropTests),
            Info = info,
            IsMaximized = false,
            IsMinimized = false,
            IsVisible = true,
            Parent = IntPtr.Zero,
            Placement = WindowPlacement.Create(),
            ProcessId = ProcessIdentifier,
            Text = nameof(CoverageFinalReleaseInteropTests),
            ThreadId = FirstThreadIdentifier,
        };
    }

    /// <summary>Creates a <see cref="WindowInfo"/> with the supplied native style.</summary>
    /// <param name="info">The window information.</param>
    /// <param name="style">The native window style.</param>
    /// <returns>The configured window information.</returns>
    private static WindowInfo WithStyle(WindowInfo info, WindowStyleFlags style)
    {
        IntPtr nativeInfo = Marshal.AllocHGlobal(Marshal.SizeOf<WindowInfo>());
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

    /// <summary>Records interop calls without calling Windows.</summary>
    private sealed class RecordingInteropOperations : InteropWindowExtensions.InteropWindowOperations
    {
        /// <summary>Defines the second synthetic thread identifier.</summary>
        private const int SecondThreadIdentifier = 85;

        /// <summary>Gets or sets the foreground window.</summary>
        internal IntPtr ForegroundWindow { get; set; }

        /// <summary>Gets or sets the parent window returned by the composed operation.</summary>
        internal IntPtr ParentWindow { get; set; }

        /// <summary>Gets or sets thread identifiers returned in request order.</summary>
        internal int[] Threads { get; } = [FirstThreadIdentifier, SecondThreadIdentifier];

        /// <summary>Gets or sets the native region result.</summary>
        internal RegionResults RegionResult { get; set; } = RegionResults.SimpleRegion;

        /// <summary>Gets or sets whether region creation should return an invalid handle.</summary>
        internal bool ReturnInvalidRegion { get; set; }

        /// <summary>Gets or sets whether the control scrollbar should expose a usable range.</summary>
        internal bool ReturnControlScrollbar { get; set; }

        /// <summary>Gets the number of input queue attach or detach operations.</summary>
        internal int AttachCalls { get; private set; }

        /// <summary>Gets the number of foreground requests.</summary>
        internal int ForegroundRequests { get; private set; }

        /// <summary>Gets the number of top-of-z-order requests.</summary>
        internal int BringToTopRequests { get; private set; }

        /// <inheritdoc />
        internal override SafeRegionHandle CreateRectRegion() =>
            ReturnInvalidRegion ? new SafeRegionHandle() : new NonReleasingRegionHandle();

        /// <inheritdoc />
        internal override RegionResults GetWindowRegion(IntPtr windowHandle, SafeRegionHandle region) => RegionResult;

        /// <inheritdoc />
        internal override Region CreateRegion(SafeHandle region) => new();

        /// <inheritdoc />
        internal override bool GetScrollInfo(IntPtr windowHandle, ScrollBarTypes scrollBarType, ref ScrollInfo scrollInfo)
        {
            if (!ReturnControlScrollbar || scrollBarType != ScrollBarTypes.Control)
            {
                return false;
            }

            scrollInfo = CreateScrollableInfo();
            return true;
        }

        /// <inheritdoc />
        internal override IntPtr GetForegroundWindow() => ForegroundWindow;

        /// <inheritdoc />
        internal override IntPtr GetParent(IntPtr windowHandle) => ParentWindow;

        /// <inheritdoc />
        internal override int GetWindowThreadProcessId(IntPtr windowHandle) => windowHandle == ForegroundWindow ? Threads[0] : Threads[1];

        /// <inheritdoc />
        internal override bool AttachThreadInput(int firstThreadId, int secondThreadId, bool attach)
        {
            AttachCalls++;
            return true;
        }

        /// <inheritdoc />
        internal override bool SetForegroundWindow(IntPtr windowHandle)
        {
            ForegroundRequests++;
            return true;
        }

        /// <inheritdoc />
        internal override bool BringWindowToTop(IntPtr windowHandle)
        {
            BringToTopRequests++;
            return true;
        }

        /// <inheritdoc />
        internal override IEnumerable<IInteropWindow> GetTopLevelWindows()
        {
            InteropWindow foreignWindow = CreateWindow(ForeignWindowHandleValue);
            foreignWindow.ProcessId = ProcessIdentifier + 1;
            return
            [
                CreateWindow(WindowHandleValue),
                CreateWindow(LinkedWindowHandleValue),
                foreignWindow,
            ];
        }

        /// <summary>Creates scroll information with a non-empty native range.</summary>
        /// <returns>Scroll information suitable for scroller construction.</returns>
        private static ScrollInfo CreateScrollableInfo()
        {
            const int maximumOffset = 12;
            IntPtr nativeInfo = Marshal.AllocHGlobal(Marshal.SizeOf<ScrollInfo>());
            try
            {
                ScrollInfo scrollInfo = ScrollInfo.Create(ScrollInfoMask.All);
                Marshal.StructureToPtr(scrollInfo, nativeInfo, false);
                Marshal.WriteInt32(nativeInfo, maximumOffset, SecondRegionCoordinate);
                return Marshal.PtrToStructure<ScrollInfo>(nativeInfo);
            }
            finally
            {
                Marshal.FreeHGlobal(nativeInfo);
            }
        }
    }

    /// <summary>Records deterministic window enumeration callbacks.</summary>
    /// <param name="handles">The handles returned to the callback.</param>
    private sealed class RecordingEnumerationOperations(IReadOnlyList<IntPtr> handles) : WindowsEnumerator.WindowsEnumeratorOperations
    {
        /// <summary>Gets observed parent handles.</summary>
        internal List<IntPtr> ParentHandles { get; } = [];

        /// <inheritdoc />
        internal override bool EnumChildWindows(IntPtr parentWindowHandle, Func<IntPtr, bool> callback)
        {
            ParentHandles.Add(parentWindowHandle);
            foreach (IntPtr handle in handles)
            {
                if (!callback(handle))
                {
                    break;
                }
            }

            return true;
        }
    }

    /// <summary>Blocks callback delivery until a cancellation subscription has been disposed.</summary>
    /// <param name="handle">The handle supplied to the callback after release.</param>
    private sealed class BlockingEnumerationOperations(IntPtr handle) : WindowsEnumerator.WindowsEnumeratorOperations, IDisposable
    {
        /// <summary>Signals that enumeration is waiting to invoke its callback.</summary>
        private readonly TaskCompletionSource<bool> _started = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Signals that enumeration has returned from its callback.</summary>
        private readonly TaskCompletionSource<bool> _completed = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Allows enumeration to continue to its callback.</summary>
        private readonly ManualResetEventSlim _release = new();

        /// <inheritdoc />
        public void Dispose() => _release.Dispose();

        /// <summary>Waits until the enumeration task is ready for cancellation.</summary>
        /// <returns>A task that completes when enumeration is ready for cancellation.</returns>
        internal Task<bool> WaitUntilStartedAsync() => WaitWithTimeoutAsync(_started.Task);

        /// <summary>Waits until the enumeration operation has completed.</summary>
        /// <returns>A task that completes when enumeration has returned from its callback.</returns>
        internal Task<bool> WaitUntilCompletedAsync() => WaitWithTimeoutAsync(_completed.Task);

        /// <summary>Allows the enumeration task to invoke its callback.</summary>
        internal void Release() => _release.Set();

        /// <inheritdoc />
        internal override bool EnumChildWindows(IntPtr parentWindowHandle, Func<IntPtr, bool> callback)
        {
            _ = _started.TrySetResult(true);
            _ = _release.Wait(TimeSpan.FromSeconds(1));
            try
            {
                return callback(handle);
            }
            finally
            {
                _ = _completed.TrySetResult(true);
            }
        }

#if NETFRAMEWORK
        /// <summary>Waits for a task while retaining a bounded failure path on .NET Framework.</summary>
        /// <param name="task">The task to wait for.</param>
        /// <returns>The completed task result.</returns>
        private static async Task<bool> WaitWithTimeoutAsync(Task<bool> task)
        {
            Task completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(1)));
            if (!ReferenceEquals(completed, task))
            {
                throw new TimeoutException();
            }

            return await task;
        }
#else
        /// <summary>Waits for a task while retaining a bounded failure path on modern .NET.</summary>
        /// <param name="task">The task to wait for.</param>
        /// <returns>The completed task result.</returns>
        private static Task<bool> WaitWithTimeoutAsync(Task<bool> task) => task.WaitAsync(TimeSpan.FromSeconds(1));
#endif
    }

    /// <summary>Observes a sequence until it completes.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    private sealed class CompletionObserver<T> : IObserver<T>
    {
        /// <summary>Gets the completion source.</summary>
        internal TaskCompletionSource<bool> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Gets observed values.</summary>
        internal List<T> Values { get; } = [];

        /// <inheritdoc />
        public void OnCompleted() => Completion.TrySetResult(true);

        /// <inheritdoc />
        public void OnError(Exception error) => Completion.TrySetException(error);

        /// <inheritdoc />
        public void OnNext(T value) => Values.Add(value);
    }

    /// <summary>A non-releasing valid region handle for managed tests.</summary>
    private sealed class NonReleasingRegionHandle : SafeRegionHandle
    {
        /// <summary>Initializes a new instance of the <see cref="NonReleasingRegionHandle"/> class.</summary>
        internal NonReleasingRegionHandle()
            : base(new(1))
        {
        }

        /// <inheritdoc />
        protected override bool ReleaseHandle() => true;
    }

    /// <summary>A non-releasing valid safe handle for region-data decoding tests.</summary>
    private sealed class NonReleasingSafeHandle : SafeHandle
    {
        /// <summary>Initializes a new instance of the <see cref="NonReleasingSafeHandle"/> class.</summary>
        internal NonReleasingSafeHandle()
            : base(IntPtr.Zero, true) => SetHandle(new(1));

        /// <inheritdoc />
        public override bool IsInvalid => false;

        /// <inheritdoc />
        protected override bool ReleaseHandle() => true;
    }
}
