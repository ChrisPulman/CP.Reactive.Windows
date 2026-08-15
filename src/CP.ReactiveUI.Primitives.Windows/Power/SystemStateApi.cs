// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Power;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Power;
#endif
/// <summary>Provides access to Windows system state APIs, including thread execution state and waitable timer functions.</summary>
public static class SystemStateApi
{
    /// <summary>Contains native methods used by system-state APIs.</summary>
    private static class NativeMethods
    {
        /// <summary>The Kernel32 library name.</summary>
        private const string Kernel32Dll = "kernel32.dll";

        /// <summary>Sets the thread execution state.</summary>
        /// <param name="flags">The thread execution state flags.</param>
        /// <returns>The previous thread execution state, or <c>0</c> on failure.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern ThreadExecutionStateFlags SetThreadExecutionState(ThreadExecutionStateFlags flags);

        /// <summary>Creates or opens a waitable timer object.</summary>
        /// <param name="timerAttributes">The security attributes pointer.</param>
        /// <param name="manualReset">A value indicating whether the timer uses manual reset.</param>
        /// <param name="timerName">The timer name.</param>
        /// <returns>The timer handle, or zero on failure.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateWaitableTimerW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr CreateWaitableTimer(IntPtr timerAttributes, [MarshalAs(UnmanagedType.Bool)] bool manualReset, string timerName);

        /// <summary>Opens an existing named waitable timer object.</summary>
        /// <param name="desiredAccess">The desired access flags.</param>
        /// <param name="inheritHandle">A value indicating whether child processes inherit the handle.</param>
        /// <param name="timerName">The timer name.</param>
        /// <returns>The timer handle, or zero on failure.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "OpenWaitableTimerW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr OpenWaitableTimer(uint desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, string timerName);

        /// <summary>Activates the specified waitable timer.</summary>
        /// <param name="timerHandle">The timer handle.</param>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The timer period in milliseconds.</param>
        /// <param name="completionRoutine">The completion routine pointer.</param>
        /// <param name="completionRoutineArgument">The completion routine argument pointer.</param>
        /// <param name="resume">A value indicating whether the system should resume when the timer fires.</param>
        /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWaitableTimer(IntPtr timerHandle, ref long dueTime, int period, IntPtr completionRoutine, IntPtr completionRoutineArgument, [MarshalAs(UnmanagedType.Bool)] bool resume);

        /// <summary>Activates the specified waitable timer.</summary>
        /// <param name="timerHandle">The timer safe handle.</param>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The timer period in milliseconds.</param>
        /// <param name="completionRoutine">The completion routine pointer.</param>
        /// <param name="completionRoutineArgument">The completion routine argument pointer.</param>
        /// <param name="resume">A value indicating whether the system should resume when the timer fires.</param>
        /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWaitableTimer(SafeWaitHandle timerHandle, ref long dueTime, int period, IntPtr completionRoutine, IntPtr completionRoutineArgument, [MarshalAs(UnmanagedType.Bool)] bool resume);

        /// <summary>Cancels a waitable timer.</summary>
        /// <param name="timerHandle">The timer handle.</param>
        /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CancelWaitableTimer(SafeWaitHandle timerHandle);

        /// <summary>Waits until the specified object is signaled or the timeout elapses.</summary>
        /// <param name="objectHandle">The object safe handle.</param>
        /// <param name="milliseconds">The timeout in milliseconds.</param>
        /// <returns>The native wait result.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern uint WaitForSingleObject(SafeWaitHandle objectHandle, uint milliseconds);
    }

    /// <summary>
    /// Enables an application to inform the system that it is in use, thereby preventing the system
    /// from entering sleep or turning off the display while the application is running.
    /// See <a href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-setthreadexecutionstate">SetThreadExecutionState function</a>
    /// </summary>
    /// <param name="executionStateFlags">The thread's execution requirements. Can be a combination of <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Power.Enums.ThreadExecutionStateFlags" />.</param>
    /// <returns>
    /// If the function succeeds, the return value is the previous thread execution state.
    /// If the function fails, the return value is <c>0</c>.
    /// </returns>
    public static ThreadExecutionStateFlags SetThreadExecutionState(ThreadExecutionStateFlags executionStateFlags) => NativeMethods.SetThreadExecutionState(executionStateFlags);

    /// <summary>
    /// Creates or opens a waitable timer object.
    /// See <a href="https://learn.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-createwaitabletimerw">CreateWaitableTimer function</a>
    /// </summary>
    /// <param name="timerAttributes">
    /// A pointer to a SECURITY_ATTRIBUTES structure. If this parameter is <see cref="F:System.IntPtr.Zero" />,
    /// the timer handle cannot be inherited by child processes.
    /// </param>
    /// <param name="manualReset">
    /// If <c>true</c>, creates a manual-reset notification timer. If <c>false</c>, creates a synchronization timer.
    /// </param>
    /// <param name="timerName">The name of the timer object. If <c>null</c>, creates an unnamed timer.</param>
    /// <returns>
    /// If the function succeeds, the return value is a safe handle to the timer object.
    /// </returns>
    public static SafeWaitHandle CreateWaitableTimer(IntPtr timerAttributes, [MarshalAs(UnmanagedType.Bool)] bool manualReset, string timerName) => CreateSafeWaitHandle(NativeMethods.CreateWaitableTimer(timerAttributes, manualReset, timerName));

    /// <summary>
    /// Opens an existing named waitable timer object.
    /// See <a href="https://learn.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-openwaitabletimerw">OpenWaitableTimer function</a>
    /// </summary>
    /// <param name="desiredAccess">The access to the timer object. The TIMER_ALL_ACCESS (0x1F0003) flag is typically used.</param>
    /// <param name="inheritHandle">
    /// If <c>true</c>, processes created by this process will inherit the handle.
    /// </param>
    /// <param name="timerName">The name of the timer object to open.</param>
    /// <returns>
    /// If the function succeeds, the return value is a safe handle to the timer object.
    /// </returns>
    public static SafeWaitHandle OpenWaitableTimer(uint desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, string timerName) => CreateSafeWaitHandle(NativeMethods.OpenWaitableTimer(desiredAccess, inheritHandle, timerName));

    /// <summary>
    /// Activates the specified waitable timer. When the due time arrives, the timer is signaled
    /// and the optional completion routine is called.
    /// See <a href="https://learn.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-setwaitabletimer">SetWaitableTimer function</a>
    /// </summary>
    /// <param name="timerHandle">A handle to the timer object. The CreateWaitableTimer or OpenWaitableTimer function returns this handle.</param>
    /// <param name="dueTime">
    /// The time after which the state of the timer is to be set to signaled. This must be a negative value
    /// expressed in 100-nanosecond intervals (e.g., -10000000 for 1 second). A positive value specifies an
    /// absolute time in UTC.
    /// </param>
    /// <param name="period">
    /// The period of the timer, in milliseconds. If zero, the timer is signaled once. If greater than zero,
    /// the timer is periodic.
    /// </param>
    /// <param name="completionRoutine">
    /// A pointer to an optional completion routine. If <see cref="F:System.IntPtr.Zero" />, no routine is called.
    /// </param>
    /// <param name="completionRoutineArgument">
    /// A pointer to a structure that is passed to the completion routine. Ignored if <paramref name="completionRoutine" /> is null.
    /// </param>
    /// <param name="resume">
    /// If <c>true</c> and the system supports it, restores a system in suspended sleep or hibernation when the timer fires.
    /// Requires the SE_SYSTEMTIME_NAME privilege.
    /// </param>
    /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
    public static bool SetWaitableTimer(SafeWaitHandle timerHandle, ref long dueTime, int period, IntPtr completionRoutine, IntPtr completionRoutineArgument, [MarshalAs(UnmanagedType.Bool)] bool resume) => NativeMethods.SetWaitableTimer(timerHandle, ref dueTime, period, completionRoutine, completionRoutineArgument, resume);

    /// <summary>
    /// Sets a cancel on a waitable timer, so it is no longer activated.
    /// See <a href="https://learn.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-cancelwaitabletimer">CancelWaitableTimer function</a>
    /// </summary>
    /// <param name="timerHandle">A handle to the timer object.</param>
    /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
    public static bool CancelWaitableTimer(SafeWaitHandle timerHandle) => NativeMethods.CancelWaitableTimer(timerHandle);

    /// <summary>
    /// Keeps the system awake and prevents the screen from turning off until <see cref="M:CP.ReactiveUI.Primitives.Windows.Desktop.Power.SystemStateApi.AllowSleep" /> is called.
    /// Equivalent to calling SetThreadExecutionState with ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED.
    /// </summary>
    /// <returns>The previous execution state, or <c>0</c> on failure.</returns>
    public static ThreadExecutionStateFlags PreventSleep() => SetThreadExecutionState(ThreadExecutionStateFlags.ES_CONTINUOUS | ThreadExecutionStateFlags.ES_DISPLAY_REQUIRED | ThreadExecutionStateFlags.ES_SYSTEM_REQUIRED);

    /// <summary>
    /// Keeps the system awake (without keeping the screen on) until <see cref="M:CP.ReactiveUI.Primitives.Windows.Desktop.Power.SystemStateApi.AllowSleep" /> is called.
    /// Equivalent to calling SetThreadExecutionState with ES_CONTINUOUS | ES_SYSTEM_REQUIRED.
    /// </summary>
    /// <returns>The previous execution state, or <c>0</c> on failure.</returns>
    public static ThreadExecutionStateFlags PreventSystemSleep() => SetThreadExecutionState(ThreadExecutionStateFlags.ES_CONTINUOUS | ThreadExecutionStateFlags.ES_SYSTEM_REQUIRED);

    /// <summary>
    /// Allows the system to sleep and the screen to turn off when idle.
    /// Equivalent to calling SetThreadExecutionState with ES_CONTINUOUS.
    /// </summary>
    /// <returns>The previous execution state, or <c>0</c> on failure.</returns>
    public static ThreadExecutionStateFlags AllowSleep() => SetThreadExecutionState(ThreadExecutionStateFlags.ES_CONTINUOUS);

    /// <summary>Activates the specified waitable timer using a safe handle.</summary>
    /// <param name="timerHandle">The timer safe handle.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="period">The timer period in milliseconds.</param>
    /// <param name="resume">A value indicating whether the system should resume when the timer fires.</param>
    /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
    internal static bool SetWaitableTimer(SafeWaitHandle timerHandle, ref long dueTime, int period, [MarshalAs(UnmanagedType.Bool)] bool resume) => NativeMethods.SetWaitableTimer(timerHandle, ref dueTime, period, IntPtr.Zero, IntPtr.Zero, resume);

    /// <summary>Waits for a waitable timer safe handle.</summary>
    /// <param name="timerHandle">The timer safe handle.</param>
    /// <param name="milliseconds">The timeout in milliseconds.</param>
    /// <returns>The native wait result.</returns>
    internal static uint WaitForSingleObject(SafeWaitHandle timerHandle, uint milliseconds) => NativeMethods.WaitForSingleObject(timerHandle, milliseconds);

    /// <summary>Creates a safe wait handle around the native timer handle.</summary>
    /// <param name="timerHandle">The native waitable timer handle.</param>
    /// <returns>The safe wait handle.</returns>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the timer handle could not be created.</exception>
    internal static SafeWaitHandle CreateSafeWaitHandle(IntPtr timerHandle)
    {
        if (timerHandle == IntPtr.Zero)
        {
            throw new Win32Exception();
        }

        return new(timerHandle, ownsHandle: true);
    }
}
