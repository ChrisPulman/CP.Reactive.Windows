// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>
///     The WinEventHook can register handlers to become important windows events
///     This makes it possible to know a.o. when a window is created, moved, updated and closed.
///     Make sure you have a message pump running (WinProc), otherwise the behavior is sporadic / none.
/// </summary>
#if NETFRAMEWORK
public static class WinEventHook
#else
public static partial class WinEventHook
#endif
{
    /// <summary>The delegate called by SetWinEventHook when an event occurs.</summary>
    /// <param name="eventHook">IntPtr with the eventhook that this call belongs to.</param>
    /// <param name="eventType">WinEvent.</param>
    /// <param name="windowHandle">IntPtr.</param>
    /// <param name="idObject">ObjectIdentifiers.</param>
    /// <param name="idChild">int.</param>
    /// <param name="eventThread">uint with EventThread.</param>
    /// <param name="eventTime">uint with EventTime.</param>
    private delegate void WinEventDelegate(IntPtr eventHook, WinEvents eventType, IntPtr windowHandle, ObjectIdentifiers idObject, int idChild, uint eventThread, uint eventTime);

    /// <summary>Create a WinEventHook as observable.</summary>
    /// <param name="winEventStart">WinEvent "start" of which you are interested.</param>
    /// <returns>IObservable which processes WinEventInfo.</returns>
    public static IObservable<WinEventInfo> ObserveWinEvents(WinEvents winEventStart) => ObserveWinEvents(winEventStart, winEventStart, 0, 0);

    /// <summary>Create a WinEventHook as observable.</summary>
    /// <param name="winEventStart">WinEvent "start" of which you are interested.</param>
    /// <param name="winEventEnd">WinEvent "end" of which you are interested.</param>
    /// <returns>IObservable which processes WinEventInfo.</returns>
    public static IObservable<WinEventInfo> ObserveWinEvents(WinEvents winEventStart, WinEvents winEventEnd) => ObserveWinEvents(winEventStart, winEventEnd, 0, 0);

    /// <summary>Create a WinEventHook as observable.</summary>
    /// <param name="winEventStart">WinEvent "start" of which you are interested.</param>
    /// <param name="winEventEnd">WinEvent "end" of which you are interested.</param>
    /// <param name="process">Process identifier to monitor, or zero for all processes.</param>
    /// <returns>IObservable which processes WinEventInfo.</returns>
    public static IObservable<WinEventInfo> ObserveWinEvents(WinEvents winEventStart, WinEvents winEventEnd, int process) => ObserveWinEvents(winEventStart, winEventEnd, process, 0);

    /// <summary>Create a WinEventHook as observable.</summary>
    /// <param name="winEventStart">WinEvent "start" of which you are interested.</param>
    /// <param name="winEventEnd">WinEvent "end" of which you are interested.</param>
    /// <param name="process">Process identifier to monitor, or zero for all processes.</param>
    /// <param name="thread">Thread identifier to monitor, or zero for all threads.</param>
    /// <returns>IObservable which processes WinEventInfo.</returns>
    public static IObservable<WinEventInfo> ObserveWinEvents(
        WinEvents winEventStart,
        WinEvents winEventEnd,
        int process,
        int thread) => ReactiveSignal.CreateSafe<WinEventInfo>(observer =>
        {
            WinEventDelegate winEventDelegate = WinEventHookDelegate;
            var hookHandle = NativeMethods.SetWinEventHook(winEventStart, winEventEnd, IntPtr.Zero, winEventDelegate, process, thread, WinEventHookFlags.None);
            if (hookHandle == IntPtr.Zero)
            {
                observer.OnError(new Win32Exception(Marshal.GetLastWin32Error()));
                return Scope.Empty;
            }

            return Scope.Create((HookHandle: hookHandle, Callback: winEventDelegate), static state =>
            {
                _ = NativeMethods.UnhookWinEvent(state.HookHandle);
                GC.KeepAlive(state.Callback);
            });
            void WinEventHookDelegate(
                IntPtr eventHook,
                WinEvents winEvent,
                IntPtr windowHandle,
                ObjectIdentifiers idObject,
                int idChild,
                uint eventThread,
                uint eventTime) => observer.OnNext(
                    WinEventInfo.Create(
                        eventHook,
                        winEvent,
                        windowHandle,
                        idObject,
                        idChild,
                        eventThread,
                        eventTime));
        }).Publish().RefCount();

    /// <summary>Create an observable which only monitors title changes.</summary>
    /// <returns>IObservable with WinEventInfo.</returns>
    public static IObservable<WinEventInfo> ObserveWindowTitleChanges() =>
        ObserveWindowObjects(ObserveWinEvents(WinEvents.EVENT_OBJECT_NAMECHANGE));

    /// <summary>Create an observable which only monitors created (open) and destroyed (closed) windows.</summary>
    /// <returns>IObservable with WinEventInfo.</returns>
    public static IObservable<WinEventInfo> ObserveWindowLifecycleEvents() =>
        ObserveWindowObjects(ObserveWinEvents(WinEvents.EVENT_OBJECT_CREATE, WinEvents.EVENT_OBJECT_DESTROY));

    /// <summary>Filters a supplied event stream to window-object events.</summary>
    /// <param name="events">The source event stream.</param>
    /// <returns>The window-only event stream.</returns>
    internal static IObservable<WinEventInfo> ObserveWindowObjects(IObservable<WinEventInfo> events) => events.Where(IsWindowObject);

    /// <summary>Checks whether a WinEvent describes a window object.</summary>
    /// <param name="winEventInfo">WinEvent information to inspect.</param>
    /// <returns>true when the event object identifier is a window.</returns>
    internal static bool IsWindowObject(WinEventInfo winEventInfo) => winEventInfo.ObjectIdentifier == ObjectIdentifiers.Window;

    /// <summary>Contains native window-event entry points.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>Unhooks a Windows event hook.</summary>
        /// <param name="winEventHookHandle">The event-hook handle.</param>
        /// <returns><see langword="true" /> when the hook was removed.</returns>
#if NETFRAMEWORK
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWinEvent(IntPtr winEventHookHandle);
#else
        [LibraryImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool UnhookWinEvent(IntPtr winEventHookHandle);
#endif

        /// <summary>Installs a Windows event hook.</summary>
        /// <param name="eventMin">The first event value.</param>
        /// <param name="eventMax">The last event value.</param>
        /// <param name="eventProcedureModule">The event procedure module.</param>
        /// <param name="eventProcedure">The event callback.</param>
        /// <param name="processId">The process identifier.</param>
        /// <param name="threadId">The thread identifier.</param>
        /// <param name="winEventHookFlags">The hook flags.</param>
        /// <returns>The event-hook handle.</returns>
#if NETFRAMEWORK
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr SetWinEventHook(
            WinEvents eventMin,
            WinEvents eventMax,
            IntPtr eventProcedureModule,
            WinEventDelegate eventProcedure,
            int processId,
            int threadId,
            WinEventHookFlags winEventHookFlags);
#else
        [LibraryImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr SetWinEventHook(
            WinEvents eventMin,
            WinEvents eventMax,
            IntPtr eventProcedureModule,
            WinEventDelegate eventProcedure,
            int processId,
            int threadId,
            WinEventHookFlags winEventHookFlags);
#endif
    }
}
