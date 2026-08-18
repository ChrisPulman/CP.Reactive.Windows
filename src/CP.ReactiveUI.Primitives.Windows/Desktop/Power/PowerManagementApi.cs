// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Power;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Power;
#endif
/// <summary>Provides access to Windows power management API functions.</summary>
#if NETFRAMEWORK
public static class PowerManagementApi
#else
public static partial class PowerManagementApi
#endif
{
    /// <summary>Power-management operations used by this process.</summary>
    private static PowerManagementOperations _operations = new(NativeMethods.SetSuspendState, NativeMethods.ExitWindowsEx);

    /// <summary>
    /// Suspends the system by transitioning it to sleep mode or hibernation.
    /// See <a href="https://learn.microsoft.com/en-us/windows/win32/api/powrprof/nf-powrprof-setsuspendstate">SetSuspendState function</a>
    /// </summary>
    /// <param name="hibernate">
    /// If <c>true</c>, the system hibernates. If <c>false</c>, the system is suspended.
    /// </param>
    /// <param name="forceCritical">
    /// If <c>true</c>, the system is suspended or hibernated immediately without sending the WM_POWERBROADCAST message.
    /// If <c>false</c>, the function broadcasts a WM_POWERBROADCAST message with the PBT_APMSUSPEND parameter value.
    /// Applications that have registered for power notification will have the opportunity to prevent the suspend.
    /// </param>
    /// <param name="disableWakeEvent">
    /// If <c>true</c>, the system disables all wake events. If <c>false</c>, enabled wake events remain enabled.
    /// </param>
    /// <returns><c>true</c> if the function succeeds, otherwise <c>false</c>.</returns>
    public static bool SetSuspendState(
        [MarshalAs(UnmanagedType.Bool)] bool hibernate,
        [MarshalAs(UnmanagedType.Bool)] bool forceCritical,
        [MarshalAs(UnmanagedType.Bool)] bool disableWakeEvent) =>
        _operations.SetSuspendState(hibernate, forceCritical, disableWakeEvent);

    /// <summary>
    /// Logs off the interactive user, shuts down the system, or shuts down and restarts the system.
    /// See <a href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-exitwindowsex">ExitWindowsEx function</a>
    /// </summary>
    /// <param name="flags">The shutdown type. One or more <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Power.Enums.ExitWindowsFlags" /> values.</param>
    /// <param name="reason">
    /// The reason for initiating the shutdown. This parameter must be one of the system shutdown reason codes.
    /// If this parameter is zero, the SHTDN_REASON_FLAG_PLANNED reason code will not be set, and therefore the default
    /// action is to create an "unplanned" shutdown.
    /// </param>
    /// <returns><c>true</c> if the function succeeds, otherwise <c>false</c>.</returns>
    public static bool ExitWindowsEx(ExitWindowsFlags flags, uint reason) => _operations.ExitWindows(flags, reason);

    /// <summary>
    /// Logs off the interactive user, shuts down the system, or shuts down and restarts the system.
    /// See <a href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-exitwindowsex">ExitWindowsEx function</a>
    /// </summary>
    /// <param name="flags">The shutdown type. One or more <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Power.Enums.ExitWindowsFlags" /> values.</param>
    /// <returns><c>true</c> if the function succeeds, otherwise <c>false</c>.</returns>
    public static bool ExitWindowsEx(ExitWindowsFlags flags) => ExitWindowsEx(flags, 0U);

    /// <summary>Suspends the system (puts it to sleep).</summary>
    /// <returns><c>true</c> if the system was suspended successfully.</returns>
    public static bool Sleep() => Sleep(disableWakeEvent: false);

    /// <summary>Suspends the system (puts it to sleep).</summary>
    /// <param name="disableWakeEvent">If <c>true</c>, disables all wake events.</param>
    /// <returns><c>true</c> if the system was suspended successfully.</returns>
    public static bool Sleep(bool disableWakeEvent) => SetSuspendState(hibernate: false, forceCritical: false, disableWakeEvent);

    /// <summary>Hibernates the system.</summary>
    /// <returns><c>true</c> if the system was hibernated successfully.</returns>
    public static bool Hibernate() => Hibernate(disableWakeEvent: false);

    /// <summary>Hibernates the system.</summary>
    /// <param name="disableWakeEvent">If <c>true</c>, disables all wake events.</param>
    /// <returns><c>true</c> if the system was hibernated successfully.</returns>
    public static bool Hibernate(bool disableWakeEvent) => SetSuspendState(hibernate: true, forceCritical: false, disableWakeEvent);

    /// <summary>Shuts down the system. The calling process must have the SE_SHUTDOWN_NAME privilege.</summary>
    /// <returns><c>true</c> if the operation was initiated successfully.</returns>
    public static bool Shutdown() => Shutdown(force: false);

    /// <summary>Shuts down the system. The calling process must have the SE_SHUTDOWN_NAME privilege.</summary>
    /// <param name="force">If <c>true</c>, forces running applications to close.</param>
    /// <returns><c>true</c> if the operation was initiated successfully.</returns>
    public static bool Shutdown(bool force)
    {
        var flags = ExitWindowsFlags.EWX_SHUTDOWN;
        if (force)
        {
            flags |= ExitWindowsFlags.EWX_FORCE;
        }

        return ExitWindowsEx(flags);
    }

    /// <summary>Restarts the system. The calling process must have the SE_SHUTDOWN_NAME privilege.</summary>
    /// <returns><c>true</c> if the operation was initiated successfully.</returns>
    public static bool Restart() => Restart(force: false);

    /// <summary>Restarts the system. The calling process must have the SE_SHUTDOWN_NAME privilege.</summary>
    /// <param name="force">If <c>true</c>, forces running applications to close.</param>
    /// <returns><c>true</c> if the operation was initiated successfully.</returns>
    public static bool Restart(bool force)
    {
        var flags = ExitWindowsFlags.EWX_REBOOT;
        if (force)
        {
            flags |= ExitWindowsFlags.EWX_FORCE;
        }

        return ExitWindowsEx(flags);
    }

    /// <summary>Logs off the current user.</summary>
    /// <returns><c>true</c> if the operation was initiated successfully.</returns>
    public static bool LogOff() => LogOff(force: false);

    /// <summary>Logs off the current user.</summary>
    /// <param name="force">If <c>true</c>, forces running applications to close.</param>
    /// <returns><c>true</c> if the operation was initiated successfully.</returns>
    public static bool LogOff(bool force)
    {
        var flags = ExitWindowsFlags.None;
        if (force)
        {
            flags |= ExitWindowsFlags.EWX_FORCE;
        }

        return ExitWindowsEx(flags);
    }

    /// <summary>Overrides native power-management operations for deterministic tests.</summary>
    /// <param name="setSuspendState">The replacement suspend-state operation.</param>
    /// <param name="exitWindows">The replacement exit-Windows operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(Func<bool, bool, bool, bool> setSuspendState, Func<ExitWindowsFlags, uint, bool> exitWindows)
    {
        Throw.IfNull(setSuspendState);
        Throw.IfNull(exitWindows);
        var operations = _operations;
        _operations = new(setSuspendState, exitWindows);
        return Scope.Create(operations, static previous =>
        {
            _operations = previous;
        });
    }

    /// <summary>Contains native methods used by power-management APIs.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>The Powrprof library name.</summary>
        private const string PowrprofDll = "powrprof.dll";

        /// <summary>The User32 library name.</summary>
        private const string User32Dll = "user32.dll";

        /// <summary>Suspends the system by transitioning it to sleep mode or hibernation.</summary>
        /// <param name="hibernate">A value indicating whether the system should hibernate.</param>
        /// <param name="forceCritical">A value indicating whether the system should suspend immediately.</param>
        /// <param name="disableWakeEvent">A value indicating whether wake events should be disabled.</param>
        /// <returns><c>true</c> if the function succeeds, otherwise <c>false</c>.</returns>
#if NETFRAMEWORK
        [DllImport(PowrprofDll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetSuspendState(
            [MarshalAs(UnmanagedType.Bool)] bool hibernate,
            [MarshalAs(UnmanagedType.Bool)] bool forceCritical,
            [MarshalAs(UnmanagedType.Bool)] bool disableWakeEvent);
#else
        [LibraryImport(PowrprofDll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetSuspendState(
            [MarshalAs(UnmanagedType.Bool)] bool hibernate,
            [MarshalAs(UnmanagedType.Bool)] bool forceCritical,
            [MarshalAs(UnmanagedType.Bool)] bool disableWakeEvent);
#endif

        /// <summary>Logs off the interactive user, shuts down the system, or shuts down and restarts the system.</summary>
        /// <param name="flags">The shutdown type.</param>
        /// <param name="reason">The reason for initiating the shutdown.</param>
        /// <returns><c>true</c> if the function succeeds, otherwise <c>false</c>.</returns>
#if NETFRAMEWORK
        [DllImport(User32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ExitWindowsEx(ExitWindowsFlags flags, uint reason);
#else
        [LibraryImport(User32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool ExitWindowsEx(ExitWindowsFlags flags, uint reason);
#endif
    }

    /// <summary>Composes power-management operations without invoking them during construction.</summary>
    /// <param name="setSuspendState">The suspend-state operation.</param>
    /// <param name="exitWindows">The exit-Windows operation.</param>
    private sealed class PowerManagementOperations(
        Func<bool, bool, bool, bool> setSuspendState,
        Func<ExitWindowsFlags, uint, bool> exitWindows)
    {
        /// <summary>Invokes the configured exit-Windows operation.</summary>
        /// <param name="flags">The exit flags.</param>
        /// <param name="reason">The exit reason.</param>
        /// <returns>The configured operation result.</returns>
        public bool ExitWindows(ExitWindowsFlags flags, uint reason) => exitWindows(flags, reason);

        /// <summary>Invokes the configured suspend-state operation.</summary>
        /// <param name="hibernate">A value indicating whether hibernation is requested.</param>
        /// <param name="forceCritical">A value indicating whether the transition is forced.</param>
        /// <param name="disableWakeEvent">A value indicating whether wake events are disabled.</param>
        /// <returns>The configured operation result.</returns>
        public bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent) =>
            setSuspendState(hibernate, forceCritical, disableWakeEvent);
    }
}
