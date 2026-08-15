// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Power;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Power;
#endif

/// <summary>Provides access to Windows system state APIs, including thread execution state and waitable timer functions.</summary>
#if NETFRAMEWORK
public static class SystemStateApi
#else
public static partial class SystemStateApi
#endif
{
    /// <summary>The operation used to open a waitable timer.</summary>
    private static Func<uint, bool, string, IntPtr> _openWaitableTimer = NativeMethods.OpenWaitableTimer;

    /// <summary>The operation used to set a waitable timer.</summary>
    private static SetWaitableTimerOperation _setWaitableTimer = NativeMethods.SetWaitableTimer;

    /// <summary>
    /// Enables an application to inform the system that it is in use, thereby preventing the system
    /// from entering sleep or turning off the display while the application is running.
    /// See <a href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-setthreadexecutionstate">SetThreadExecutionState function</a>.
    /// </summary>
    /// <param name="executionStateFlags">The thread's execution requirements.</param>
    /// <returns>The previous thread execution state, or <c>0</c> on failure.</returns>
    public static ThreadExecutionStateFlags SetThreadExecutionState(ThreadExecutionStateFlags executionStateFlags) =>
        NativeMethods.SetThreadExecutionState(executionStateFlags);

    /// <summary>Creates or opens a waitable timer object.</summary>
    /// <param name="timerAttributes">A pointer to the security attributes, or zero.</param>
    /// <param name="manualReset">Whether the timer uses manual reset.</param>
    /// <param name="timerName">The timer name, or null for an unnamed timer.</param>
    /// <returns>A safe handle to the timer object.</returns>
    public static SafeWaitHandle CreateWaitableTimer(
        IntPtr timerAttributes,
        [MarshalAs(UnmanagedType.Bool)] bool manualReset,
        string timerName) =>
        CreateSafeWaitHandle(NativeMethods.CreateWaitableTimer(timerAttributes, manualReset, timerName));

    /// <summary>Opens an existing named waitable timer object.</summary>
    /// <param name="desiredAccess">The requested timer access.</param>
    /// <param name="inheritHandle">Whether child processes inherit the handle.</param>
    /// <param name="timerName">The timer name.</param>
    /// <returns>A safe handle to the timer object.</returns>
    public static SafeWaitHandle OpenWaitableTimer(
        uint desiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
        string timerName) =>
        CreateSafeWaitHandle(_openWaitableTimer(desiredAccess, inheritHandle, timerName));

    /// <summary>Activates the specified waitable timer.</summary>
    /// <param name="timerHandle">The timer safe handle.</param>
    /// <param name="dueTime">The timer due time.</param>
    /// <param name="period">The timer period in milliseconds.</param>
    /// <param name="completionRoutine">The optional completion routine.</param>
    /// <param name="completionRoutineArgument">The optional completion routine argument.</param>
    /// <param name="resume">Whether the timer resumes the system.</param>
    /// <returns><see langword="true"/> when the timer is set.</returns>
    public static bool SetWaitableTimer(
        SafeWaitHandle timerHandle,
        ref long dueTime,
        int period,
        IntPtr completionRoutine,
        IntPtr completionRoutineArgument,
        [MarshalAs(UnmanagedType.Bool)] bool resume) =>
        _setWaitableTimer(
            timerHandle,
            ref dueTime,
            period,
            completionRoutine,
            completionRoutineArgument,
            resume);

    /// <summary>Cancels a waitable timer.</summary>
    /// <param name="timerHandle">The timer safe handle.</param>
    /// <returns><see langword="true"/> when the timer is cancelled.</returns>
    public static bool CancelWaitableTimer(SafeWaitHandle timerHandle) =>
        NativeMethods.CancelWaitableTimer(timerHandle);

    /// <summary>Keeps the system awake and prevents the screen from turning off.</summary>
    /// <returns>The previous execution state, or <c>0</c> on failure.</returns>
    public static ThreadExecutionStateFlags PreventSleep() => SetThreadExecutionState(
        ThreadExecutionStateFlags.ES_CONTINUOUS
        | ThreadExecutionStateFlags.ES_DISPLAY_REQUIRED
        | ThreadExecutionStateFlags.ES_SYSTEM_REQUIRED);

    /// <summary>Keeps the system awake without keeping the screen on.</summary>
    /// <returns>The previous execution state, or <c>0</c> on failure.</returns>
    public static ThreadExecutionStateFlags PreventSystemSleep() => SetThreadExecutionState(
        ThreadExecutionStateFlags.ES_CONTINUOUS
        | ThreadExecutionStateFlags.ES_SYSTEM_REQUIRED);

    /// <summary>Allows the system to sleep and the screen to turn off when idle.</summary>
    /// <returns>The previous execution state, or <c>0</c> on failure.</returns>
    public static ThreadExecutionStateFlags AllowSleep() =>
        SetThreadExecutionState(ThreadExecutionStateFlags.ES_CONTINUOUS);

    /// <summary>Activates the specified waitable timer without a completion routine.</summary>
    /// <param name="timerHandle">The timer safe handle.</param>
    /// <param name="dueTime">The timer due time.</param>
    /// <param name="period">The timer period in milliseconds.</param>
    /// <param name="resume">Whether the timer resumes the system.</param>
    /// <returns><see langword="true"/> when the timer is set.</returns>
    internal static bool SetWaitableTimer(
        SafeWaitHandle timerHandle,
        ref long dueTime,
        int period,
        [MarshalAs(UnmanagedType.Bool)] bool resume) =>
        SetWaitableTimer(timerHandle, ref dueTime, period, IntPtr.Zero, IntPtr.Zero, resume);

    /// <summary>Waits for a waitable timer safe handle.</summary>
    /// <param name="timerHandle">The timer safe handle.</param>
    /// <param name="milliseconds">The timeout in milliseconds.</param>
    /// <returns>The native wait result.</returns>
    internal static uint WaitForSingleObject(SafeWaitHandle timerHandle, uint milliseconds) =>
        NativeMethods.WaitForSingleObject(timerHandle, milliseconds);

    /// <summary>Overrides timer operations for deterministic tests.</summary>
    /// <param name="openWaitableTimer">The timer-open operation.</param>
    /// <param name="setWaitableTimer">The timer-set operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideTimerOperationsForTesting(
        Func<uint, bool, string, IntPtr> openWaitableTimer,
        SetWaitableTimerOperation setWaitableTimer)
    {
        Throw.IfNull(openWaitableTimer);
        Throw.IfNull(setWaitableTimer);
        var previous = (Open: _openWaitableTimer, Set: _setWaitableTimer);
        _openWaitableTimer = openWaitableTimer;
        _setWaitableTimer = setWaitableTimer;
        return Scope.Create(previous, static operations =>
        {
            _openWaitableTimer = operations.Open;
            _setWaitableTimer = operations.Set;
        });
    }

    /// <summary>Creates a safe wait handle around a native timer handle.</summary>
    /// <param name="timerHandle">The native timer handle.</param>
    /// <returns>The safe wait handle.</returns>
    /// <exception cref="Win32Exception">Thrown when the handle is zero.</exception>
    internal static SafeWaitHandle CreateSafeWaitHandle(IntPtr timerHandle)
    {
        if (timerHandle == IntPtr.Zero)
        {
            throw new Win32Exception();
        }

        return new(timerHandle, ownsHandle: true);
    }

    /// <summary>Contains native system-state methods.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>The Kernel32 library name.</summary>
        private const string Kernel32Dll = "kernel32.dll";

        /// <summary>Sets the thread execution state.</summary>
        /// <param name="flags">The execution-state flags.</param>
        /// <returns>The previous execution state.</returns>
#if NETFRAMEWORK
        [DllImport(Kernel32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern ThreadExecutionStateFlags SetThreadExecutionState(ThreadExecutionStateFlags flags);
#else
        [LibraryImport(Kernel32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial ThreadExecutionStateFlags SetThreadExecutionState(ThreadExecutionStateFlags flags);
#endif

        /// <summary>Creates a waitable timer.</summary>
        /// <param name="timerAttributes">The security attributes.</param>
        /// <param name="manualReset">Whether the timer uses manual reset.</param>
        /// <param name="timerName">The timer name.</param>
        /// <returns>The native timer handle.</returns>
#if NETFRAMEWORK
        [DllImport(Kernel32Dll, CharSet = CharSet.Unicode, EntryPoint = "CreateWaitableTimerW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr CreateWaitableTimer(
            IntPtr timerAttributes,
            [MarshalAs(UnmanagedType.Bool)] bool manualReset,
            string timerName);
#else
        [LibraryImport(Kernel32Dll, EntryPoint = "CreateWaitableTimerW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr CreateWaitableTimer(
            IntPtr timerAttributes,
            [MarshalAs(UnmanagedType.Bool)] bool manualReset,
            string timerName);
#endif

        /// <summary>Opens a waitable timer.</summary>
        /// <param name="desiredAccess">The requested access.</param>
        /// <param name="inheritHandle">Whether the handle is inherited.</param>
        /// <param name="timerName">The timer name.</param>
        /// <returns>The native timer handle.</returns>
#if NETFRAMEWORK
        [DllImport(Kernel32Dll, CharSet = CharSet.Unicode, EntryPoint = "OpenWaitableTimerW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr OpenWaitableTimer(
            uint desiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
            string timerName);
#else
        [LibraryImport(Kernel32Dll, EntryPoint = "OpenWaitableTimerW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr OpenWaitableTimer(
            uint desiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
            string timerName);
#endif

        /// <summary>Sets a waitable timer.</summary>
        /// <param name="timerHandle">The timer safe handle.</param>
        /// <param name="dueTime">The timer due time.</param>
        /// <param name="period">The timer period.</param>
        /// <param name="completionRoutine">The completion routine.</param>
        /// <param name="completionRoutineArgument">The completion routine argument.</param>
        /// <param name="resume">Whether the timer resumes the system.</param>
        /// <returns><see langword="true"/> when the timer is set.</returns>
#if NETFRAMEWORK
        [DllImport(Kernel32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWaitableTimer(
            SafeWaitHandle timerHandle,
            ref long dueTime,
            int period,
            IntPtr completionRoutine,
            IntPtr completionRoutineArgument,
            [MarshalAs(UnmanagedType.Bool)] bool resume);
#else
        [LibraryImport(Kernel32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetWaitableTimer(
            SafeWaitHandle timerHandle,
            ref long dueTime,
            int period,
            IntPtr completionRoutine,
            IntPtr completionRoutineArgument,
            [MarshalAs(UnmanagedType.Bool)] bool resume);
#endif

        /// <summary>Cancels a waitable timer.</summary>
        /// <param name="timerHandle">The timer safe handle.</param>
        /// <returns><see langword="true"/> when the timer is cancelled.</returns>
#if NETFRAMEWORK
        [DllImport(Kernel32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CancelWaitableTimer(SafeWaitHandle timerHandle);
#else
        [LibraryImport(Kernel32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool CancelWaitableTimer(SafeWaitHandle timerHandle);
#endif

        /// <summary>Waits for a native object.</summary>
        /// <param name="objectHandle">The native object handle.</param>
        /// <param name="milliseconds">The timeout.</param>
        /// <returns>The native wait result.</returns>
#if NETFRAMEWORK
        [DllImport(Kernel32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern uint WaitForSingleObject(SafeWaitHandle objectHandle, uint milliseconds);
#else
        [LibraryImport(Kernel32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial uint WaitForSingleObject(SafeWaitHandle objectHandle, uint milliseconds);
#endif
    }
}
