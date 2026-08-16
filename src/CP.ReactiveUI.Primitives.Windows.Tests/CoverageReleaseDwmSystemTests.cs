// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Enums;
using Microsoft.Win32.SafeHandles;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Deterministic release coverage for DWM and system-state native adapters.</summary>
public sealed class CoverageReleaseDwmSystemTests
{
    /// <summary>Defines a synthetic colorization ARGB value.</summary>
    private const int ColorizationArgb = 0x7F102030;

    /// <summary>Defines a synthetic relative timer due time.</summary>
    private const long DueTimeValue = -Thirteen;

    /// <summary>Defines a synthetic non-owning waitable timer handle.</summary>
    private const int TimerHandleValue = 19;

    /// <summary>Defines the native wait-timeout result.</summary>
    private const uint WaitTimeout = 258U;

    /// <summary>Covers optional export resolution and an unavailable DWM registry key without native mutation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DwmApi_OptionalLookupAndUnavailableColorizationKey_ReturnFallbacksAsync()
    {
        IntPtr exportAddress = new(Seven);
        var resolvedExport = DwmApi.ResolveOptionalExport(found: true, exportAddress);
        var missingExport = DwmApi.ResolveOptionalExport(found: false, exportAddress);
        var missingColorization = DwmApi.GetColorizationValue(null);

        await Assert.That(resolvedExport).IsEqualTo(exportAddress);
        await Assert.That(missingExport).IsEqualTo(IntPtr.Zero);
        await Assert.That(missingColorization).IsNull();
    }

    /// <summary>Covers DWM optional exports and platform-gated outcomes without changing desktop composition.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DwmApi_UsesStubbedOptionalExportsAndEnvironmentProvidersAsync()
    {
        var stubs = new DwmNativeStubs();
        var isWindows8X = true;
        var isWindowsBeforeVista = false;
        const bool IsWindows8OrLater = false;
        const bool IsWindows11OrLater = false;

        _ = DwmApi.ColorizationSystemDrawingColor;
        using var exportScope = stubs.OverrideExports();
        using var environmentScope = DwmApi.OverrideEnvironmentForTesting(
            static () => ColorizationArgb,
            () => isWindows8X,
            () => isWindowsBeforeVista,
            static () => IsWindows8OrLater,
            static () => IsWindows11OrLater);

        var colorizationColor = DwmApi.ColorizationSystemDrawingColor;
        var color = DwmApi.ColorizationColor;
        var drawingColor = DwmApi.ColorizationDrawingColor;
        var enabledForWindows8 = DwmApi.IsDwmEnabled;
        isWindows8X = false;
        isWindowsBeforeVista = true;
        var enabledBeforeVista = DwmApi.IsDwmEnabled;
        var isCloakedBeforeWindows8 = DwmApi.IsWindowCloaked(new(One));
        var cornerPreferenceBeforeWindows11 = DwmApi.GetWindowCornerPreference(new(One));
        var setCornerPreferenceBeforeWindows11 = DwmApi.SetWindowCornerPreference(new(One), DwmWindowCornerPreference.Round);
        var disableComposition = DwmApi.DisableComposition();
        var enableComposition = DwmApi.EnableComposition();
        var flip3D = DwmApi.DwmpStartOrStopFlip3D();
        var d3DFormat = (uint)Three;
        var getSharedSurface = DwmApi.GetSharedSurface(new(One), Two, Three, Four, ref d3DFormat, out var sharedHandle, Five);
        var updateSharedWindow = DwmApi.UpdateWindowShared(new(One), Two, Three, Four, new(Five), new(Six));

        await Assert.That(colorizationColor.ToArgb()).IsEqualTo(ColorizationArgb);
        await Assert.That(color.A).IsEqualTo(colorizationColor.A);
        await Assert.That(drawingColor.ToArgb()).IsEqualTo(colorizationColor.ToArgb());
        await Assert.That(enabledForWindows8).IsTrue();
        await Assert.That(enabledBeforeVista).IsFalse();
        await Assert.That(isCloakedBeforeWindows8).IsFalse();
        await Assert.That(cornerPreferenceBeforeWindows11).IsEqualTo(DwmWindowCornerPreference.Default);
        await Assert.That(setCornerPreferenceBeforeWindows11).IsFalse();
        await Assert.That(disableComposition).IsTrue();
        await Assert.That(enableComposition).IsTrue();
        await Assert.That(flip3D).IsTrue();
        await Assert.That(d3DFormat).IsEqualTo((uint)Eight);
        await Assert.That(sharedHandle).IsEqualTo(new(Nine));
        await Assert.That(getSharedSurface).IsEqualTo(Ten);
        await Assert.That(updateSharedWindow).IsEqualTo(Eleven);
        await Assert.That(DwmNativeStubs.CompositionActions).IsEquivalentTo([(uint)Zero, (uint)One]);
    }

    /// <summary>Covers raw waitable-timer adapters with unmanaged stubs and no kernel timer allocation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SystemStateApi_UsesStubbedRawTimerExportsAsync()
    {
        var stubs = new SystemStateNativeStubs();
        using var exports = SystemStateApi.OverrideTimerOperationsForTesting(
            stubs.OpenWaitableTimer,
            stubs.SetWaitableTimer);
        using var handle = new SafeWaitHandle(new(Twelve), ownsHandle: false);
        var dueTime = DueTimeValue;

        await Assert.That(static () => SystemStateApi.OpenWaitableTimer(Fourteen, inheritHandle: true, "coverage")).Throws<Win32Exception>();
        var set = SystemStateApi.SetWaitableTimer(handle, ref dueTime, Fifteen, new(Sixteen), new(Seventeen), resume: true);
        await Assert.That(static () => SystemStateApi.CreateSafeWaitHandle(IntPtr.Zero)).Throws<Win32Exception>();
        await Assert.That(set).IsTrue();
        await Assert.That(stubs.OpenDesiredAccess).IsEqualTo((uint)Fourteen);
        await Assert.That(stubs.OpenInheritHandle).IsTrue();
        await Assert.That(stubs.OpenName).IsEqualTo("coverage");
        await Assert.That(stubs.TimerHandle).IsSameReferenceAs(handle);
        await Assert.That(stubs.DueTime).IsEqualTo(DueTimeValue);
        await Assert.That(stubs.Period).IsEqualTo(Fifteen);
        await Assert.That(stubs.CompletionRoutine).IsEqualTo(new(Sixteen));
        await Assert.That(stubs.CompletionRoutineArgument).IsEqualTo(new(Seventeen));
        await Assert.That(stubs.Resume).IsTrue();
    }

    /// <summary>Covers composed power operations without suspending, shutting down, restarting, or logging off Windows.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task PowerManagementApi_UsesComposedOperationsAsync()
    {
        var suspendArguments = new List<(bool Hibernate, bool ForceCritical, bool DisableWakeEvent)>();
        var exitArguments = new List<(ExitWindowsFlags Flags, uint Reason)>();
        using var operations = PowerManagementApi.OverrideOperationsForTesting(
            (hibernate, forceCritical, disableWakeEvent) =>
            {
                suspendArguments.Add((hibernate, forceCritical, disableWakeEvent));
                return true;
            },
            (flags, reason) =>
            {
                exitArguments.Add((flags, reason));
                return true;
            });

        var suspend = PowerManagementApi.SetSuspendState(hibernate: true, forceCritical: true, disableWakeEvent: true);
        var exit = PowerManagementApi.ExitWindowsEx(ExitWindowsFlags.EWX_REBOOT | ExitWindowsFlags.EWX_FORCE, Eighteen);

        await Assert.That(suspend).IsTrue();
        await Assert.That(exit).IsTrue();
        await Assert.That(suspendArguments).IsEquivalentTo([(true, true, true)]);
        await Assert.That(exitArguments).IsEquivalentTo([(ExitWindowsFlags.EWX_REBOOT | ExitWindowsFlags.EWX_FORCE, (uint)Eighteen)]);
    }

    /// <summary>Covers observable timeout completion and cancellation without allocating a Windows timer.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WaitableTimer_ObservableTimeoutAndCancellationAreDeterministicAsync()
    {
        using var handle = new SafeWaitHandle(new(TimerHandleValue), ownsHandle: false);
        using var timer = new WaitableTimer(handle);
        var completed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using (WaitableTimer.OverrideWaitForSingleObjectForTesting(static (_, _) => WaitTimeout))
        using (System.ObservableExtensions.Subscribe(
                   timer.ObserveSignals(TimeSpan.Zero),
                   static _ => { },
                   error => _ = completed.TrySetException(error),
                   () => completed.TrySetResult(true)))
        {
            await completed.Task;
        }

        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancellationObserved = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var waitGate = new ManualResetEventSlim();
        using (WaitableTimer.OverrideWaitForSingleObjectForTesting((_, _) =>
               {
                   _ = started.TrySetResult(true);
                   _ = waitGate.Wait(TimeSpan.FromSeconds(One));
                   return WaitTimeout;
               }))
        using (WaitableTimer.OverrideSignalObservationCancellationForTesting(
                   () => _ = cancellationObserved.TrySetResult(true)))
        {
            var subscription = System.ObservableExtensions.Subscribe(timer.ObserveSignals(), static _ => { });
            await started.Task;
            subscription.Dispose();
            waitGate.Set();
            Task completedCancellation = await Task.WhenAny(cancellationObserved.Task, Task.Delay(TimeSpan.FromSeconds(Five)));
            await Assert.That(completedCancellation).IsSameReferenceAs(cancellationObserved.Task);
        }

        await Assert.That(completed.Task.Status).IsEqualTo(TaskStatus.RanToCompletion);
    }

    /// <summary>Provides unmanaged DWM export stubs.</summary>
    internal sealed class DwmNativeStubs
    {
        /// <summary>The deterministic Flip3D result.</summary>
        private readonly bool _flip3DResult = true;

        /// <summary>The deterministic shared-surface result.</summary>
        private readonly int _sharedSurfaceResult = Ten;

        /// <summary>The deterministic shared-window update result.</summary>
        private readonly int _updateWindowSharedResult = Eleven;

        /// <summary>Initializes a new instance of the <see cref="DwmNativeStubs" /> class.</summary>
        internal DwmNativeStubs()
        {
            CompositionActions.Clear();
        }

        /// <summary>Gets the composition actions supplied to the stub.</summary>
        internal static List<uint> CompositionActions { get; } = [];

        /// <summary>Overrides the DWM exports with the stub operations.</summary>
        /// <returns>A scope that restores the native exports.</returns>
        internal IDisposable OverrideExports() => DwmApi.OverrideOptionalOperationsForTesting(
            EnableComposition,
            StartOrStopFlip3D,
            GetSharedSurface,
            UpdateWindowShared);

        /// <summary>Records the requested composition action.</summary>
        /// <param name="compositionAction">The requested composition action.</param>
        /// <returns>A successful result.</returns>
        private HResult EnableComposition(uint compositionAction)
        {
            _ = _flip3DResult;
            CompositionActions.Add(compositionAction);
            return HResult.Ok;
        }

        /// <summary>Reports Flip3D success.</summary>
        /// <returns>A non-zero success value.</returns>
        private bool StartOrStopFlip3D() => _flip3DResult;

        /// <summary>Writes deterministic shared surface values.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="adapterLuid">The adapter LUID.</param>
        /// <param name="one">The first option.</param>
        /// <param name="two">The second option.</param>
        /// <param name="format">The Direct3D format pointer.</param>
        /// <param name="sharedHandle">The shared-handle pointer.</param>
        /// <param name="unknown">The undocumented option.</param>
        /// <returns>The deterministic result.</returns>
        private int GetSharedSurface(
            IntPtr windowHandle,
            long adapterLuid,
            uint one,
            uint two,
            ref uint format,
            out IntPtr sharedHandle,
            ulong unknown)
        {
            _ = windowHandle;
            _ = adapterLuid;
            _ = one;
            _ = two;
            _ = unknown;
            format = Eight;
            sharedHandle = new(Nine);
            return _sharedSurfaceResult;
        }

        /// <summary>Reports deterministic shared window update success.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="one">The first option.</param>
        /// <param name="two">The second option.</param>
        /// <param name="three">The third option.</param>
        /// <param name="monitorHandle">The monitor handle.</param>
        /// <param name="unknown">The undocumented option.</param>
        /// <returns>The deterministic result.</returns>
        private int UpdateWindowShared(
            IntPtr windowHandle,
            int one,
            int two,
            int three,
            IntPtr monitorHandle,
            IntPtr unknown)
        {
            _ = windowHandle;
            _ = one;
            _ = two;
            _ = three;
            _ = monitorHandle;
            _ = unknown;
            return _updateWindowSharedResult;
        }
    }

    /// <summary>Provides managed system-state operation stubs.</summary>
    internal sealed class SystemStateNativeStubs
    {
        /// <summary>Gets the OpenWaitableTimer desired access value.</summary>
        internal uint OpenDesiredAccess { get; private set; }

        /// <summary>Gets the OpenWaitableTimer inherit-handle value.</summary>
        internal bool OpenInheritHandle { get; private set; }

        /// <summary>Gets the OpenWaitableTimer name.</summary>
        internal string OpenName { get; private set; }

        /// <summary>Gets the SetWaitableTimer handle.</summary>
        internal SafeWaitHandle TimerHandle { get; private set; }

        /// <summary>Gets the SetWaitableTimer due time.</summary>
        internal long DueTime { get; private set; }

        /// <summary>Gets the SetWaitableTimer period.</summary>
        internal int Period { get; private set; }

        /// <summary>Gets the SetWaitableTimer completion routine.</summary>
        internal IntPtr CompletionRoutine { get; private set; }

        /// <summary>Gets the SetWaitableTimer completion routine argument.</summary>
        internal IntPtr CompletionRoutineArgument { get; private set; }

        /// <summary>Gets the SetWaitableTimer resume value.</summary>
        internal bool Resume { get; private set; }

        /// <summary>Provides an OpenWaitableTimer result.</summary>
        /// <param name="desiredAccess">The requested access.</param>
        /// <param name="inheritHandle">The inherit-handle flag.</param>
        /// <param name="timerName">The timer name.</param>
        /// <returns>The native timer handle.</returns>
        internal IntPtr OpenWaitableTimer(uint desiredAccess, bool inheritHandle, string timerName)
        {
            OpenDesiredAccess = desiredAccess;
            OpenInheritHandle = inheritHandle;
            OpenName = timerName;
            return IntPtr.Zero;
        }

        /// <summary>Provides a SetWaitableTimer result.</summary>
        /// <param name="timerHandle">The timer safe handle.</param>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The timer period.</param>
        /// <param name="completionRoutine">The completion routine pointer.</param>
        /// <param name="completionRoutineArgument">The completion routine argument.</param>
        /// <param name="resume">The resume flag.</param>
        /// <returns><see langword="true"/>.</returns>
        internal bool SetWaitableTimer(
            SafeWaitHandle timerHandle,
            ref long dueTime,
            int period,
            IntPtr completionRoutine,
            IntPtr completionRoutineArgument,
            bool resume)
        {
            TimerHandle = timerHandle;
            DueTime = dueTime;
            Period = period;
            CompletionRoutine = completionRoutine;
            CompletionRoutineArgument = completionRoutineArgument;
            Resume = resume;
            return true;
        }
    }
}
