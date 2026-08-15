// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.Enumerations;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface;

/// <summary>Native wrappers for the User32 DLL.</summary>
public static partial class User32Api
{
    /// <summary>Stores the default SendMessage timeout in milliseconds.</summary>
    private const uint DefaultSendMessageTimeoutMilliseconds = 300U;

    /// <summary>Stores the GetGuiResources flag for GDI object counts.</summary>
    private const int GdiObjectCountFlag = 0;

    /// <summary>Stores the initial one-based display index.</summary>
    private const int InitialDisplayIndex = 1;

    /// <summary>Stores the size of the <c>MONITORINFOEXW</c> structure.</summary>
    private const int MonitorInfoExSize = 104;

    /// <summary>Stores the <c>MONITORINFOEXW.dwFlags</c> byte offset.</summary>
    private const int MonitorInfoExFlagsOffset = 36;

    /// <summary>Stores the <c>MONITORINFOEXW.rcMonitor</c> byte offset.</summary>
    private const int MonitorInfoExMonitorOffset = 4;

    /// <summary>Stores the <c>MONITORINFOEXW.szDevice</c> byte offset.</summary>
    private const int MonitorInfoExDeviceNameOffset = 40;

    /// <summary>Stores the <c>MONITORINFOEXW.szDevice</c> byte length.</summary>
    private const int MonitorInfoExDeviceNameSize = 64;

    /// <summary>Stores the byte size of a native rectangle.</summary>
    private const int NativeRectSize = 16;

    /// <summary>Stores the <c>MONITORINFOEXW.rcWork</c> byte offset.</summary>
    private const int MonitorInfoExWorkAreaOffset = 20;

    /// <summary>Stores the maximum WM_GETTEXT buffer size allocated on the stack.</summary>
    private const int StackTextBufferLimit = 256;

    /// <summary>Stores the standard fixed window text buffer capacity.</summary>
    private const int StandardWindowTextCapacity = 260;

    /// <summary>Stores the GetGuiResources flag for USER object counts.</summary>
    private const int UserObjectCountFlag = 1;

    /// <summary>Stores the major Windows version where physical cursor position is available.</summary>
    private const int WindowsVistaMajorVersion = 6;

    /// <summary>Stores the pointer size for 64-bit Windows.</summary>
    private const int WindowsX64PointerSize = 8;

    /// <summary>Keeps the legacy monitor-enumeration callback alive.</summary>
    private static readonly unsafe AddDisplayMonitorCallback AddDisplayMonitorDelegate =
        AddDisplayMonitor;

    /// <summary>The legacy monitor-enumeration callback pointer.</summary>
    private static readonly IntPtr AddDisplayMonitorPointer = Marshal.GetFunctionPointerForDelegate(
        AddDisplayMonitorDelegate);

    /// <summary>Indicates whether the physical cursor-position entry point is available.</summary>
    private static bool _canCallGetPhysicalCursorPos = true;

    /// <summary>Stores the composed operations used by managed User32 wrappers.</summary>
    private static User32Operations _operations = new();

    /// <summary>Delegate used for window enumeration callbacks.</summary>
    /// <param name="windowHandle">The enumerated window handle.</param>
    /// <param name="longParameter">The callback state.</param>
    /// <returns>True to continue enumeration; otherwise, false.</returns>
    public delegate bool EnumWindowsProc(IntPtr windowHandle, IntPtr longParameter);

    /// <summary>Defines the legacy monitor-enumeration callback signature.</summary>
    /// <param name="monitorHandle">The monitor handle.</param>
    /// <param name="deviceContextHandle">The monitor device-context handle.</param>
    /// <param name="monitorRectangle">The monitor bounds.</param>
    /// <param name="state">The callback state.</param>
    /// <returns>One to continue enumeration; otherwise, zero.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private unsafe delegate int AddDisplayMonitorCallback(
        IntPtr monitorHandle,
        IntPtr deviceContextHandle,
        NativeRect* monitorRectangle,
        IntPtr state);

    /// <summary>Atomically overrides managed User32 operations for deterministic tests.</summary>
    /// <param name="operations">The replacement operation set.</param>
    /// <returns>A scope that restores the previous operation set.</returns>
    internal static IDisposable OverrideOperationsForTesting(User32Operations operations)
    {
        Throw.IfNull(operations);
        User32Operations previous = Interlocked.Exchange(ref _operations, operations);
        return Scope.Create(previous, static value => Volatile.Write(ref _operations, value));
    }

    /// <summary>Invokes the monitor callback with a caller-supplied state object for deterministic tests.</summary>
    /// <param name="monitorHandle">The monitor handle supplied to the callback.</param>
    /// <param name="stateTarget">The managed callback state target.</param>
    /// <returns>One when the callback continues; otherwise, zero.</returns>
    internal static unsafe int AddDisplayMonitorForTesting(IntPtr monitorHandle, object stateTarget)
    {
        Throw.IfNull(stateTarget);
        GCHandle state = GCHandle.Alloc(stateTarget);
        try
        {
            return AddDisplayMonitor(
                monitorHandle,
                IntPtr.Zero,
                (NativeRect*)IntPtr.Zero,
                GCHandle.ToIntPtr(state));
        }
        finally
        {
            state.Free();
        }
    }
}

/// <summary>Contains private User32 entry point declarations.</summary>
public static partial class User32Api
{
#if NETFRAMEWORK
    /// <summary>Private native User32 entry points.</summary>
    internal static class NativeMethods
#else
    /// <summary>Private native User32 entry points.</summary>
    internal static partial class NativeMethods
#endif
    {
        /// <summary>Specifies the User32 library name.</summary>
        private const string User32 = "user32";

        /// <summary>Invokes the native <c>IsWindowVisible</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowVisible(IntPtr windowHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool IsWindowVisible(IntPtr windowHandle);
#endif

        /// <summary>Invokes the native <c>IsWindow</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindow(IntPtr windowHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool IsWindow(IntPtr windowHandle);
#endif

        /// <summary>Invokes the native <c>GetWindowThreadProcessId</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="processId">The native <paramref name="processId" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetWindowThreadProcessId(IntPtr windowHandle, out int processId);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int GetWindowThreadProcessId(
            IntPtr windowHandle,
            out int processId);
#endif

        /// <summary>Invokes the native <c>GetWindowThreadProcessId</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="processId">The native <paramref name="processId" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetWindowThreadProcessId(IntPtr windowHandle, IntPtr processId);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int GetWindowThreadProcessId(IntPtr windowHandle, IntPtr processId);
#endif

        /// <summary>Invokes the native <c>AttachThreadInput</c> entry point.</summary>
        /// <param name="idAttach">The native <paramref name="idAttach" /> value.</param>
        /// <param name="idAttachTo">The native <paramref name="idAttachTo" /> value.</param>
        /// <param name="attach">The native <paramref name="attach" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr AttachThreadInput(int idAttach, int idAttachTo, int attach);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr AttachThreadInput(int idAttach, int idAttachTo, int attach);
#endif

        /// <summary>Invokes the native <c>GetParent</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetParent(IntPtr windowHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr GetParent(IntPtr windowHandle);
#endif

        /// <summary>Invokes the native <c>SetParent</c> entry point.</summary>
        /// <param name="childWindowHandle">The native <paramref name="childWindowHandle" /> value.</param>
        /// <param name="newParentWindowHandle">The native <paramref name="newParentWindowHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr SetParent(IntPtr childWindowHandle, IntPtr newParentWindowHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr SetParent(
            IntPtr childWindowHandle,
            IntPtr newParentWindowHandle);
#endif

        /// <summary>Invokes the native <c>GetWindow</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="getWindowCommand">The native <paramref name="getWindowCommand" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetWindow(IntPtr windowHandle, GetWindowCommands getWindowCommand);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr GetWindow(
            IntPtr windowHandle,
            GetWindowCommands getWindowCommand);
#endif

        /// <summary>Invokes the native <c>ShowWindow</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="showCommand">The native <paramref name="showCommand" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr windowHandle, ShowWindowCommands showCommand);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool ShowWindow(
            IntPtr windowHandle,
            ShowWindowCommands showCommand);
#endif

        /// <summary>Invokes the native <c>SetWindowText</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="caption">The native <paramref name="caption" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "SetWindowTextW", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int SetWindowText(IntPtr windowHandle, string caption);
#else
        [LibraryImport(
            "user32",
            EntryPoint = "SetWindowTextW",
            SetLastError = true,
            StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int SetWindowText(IntPtr windowHandle, string caption);
#endif

        /// <summary>Invokes the native <c>GetWindowTextLength</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "GetWindowTextLengthW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetWindowTextLength(IntPtr windowHandle);
#else
        [LibraryImport("user32", EntryPoint = "GetWindowTextLengthW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int GetWindowTextLength(IntPtr windowHandle);
#endif

        /// <summary>Invokes the native <c>GetWindowText</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="text">The native <paramref name="text" /> value.</param>
        /// <param name="capacity">The native <paramref name="capacity" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "GetWindowTextW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern unsafe int GetWindowText(IntPtr windowHandle, char* text, int capacity);
#else
        [LibraryImport("user32", EntryPoint = "GetWindowTextW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static unsafe partial int GetWindowText(
            IntPtr windowHandle,
            char* text,
            int capacity);
#endif

        /// <summary>Invokes the native <c>GetSysColor</c> entry point.</summary>
        /// <param name="index">The native <paramref name="index" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern uint GetSysColor(int index);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial uint GetSysColor(int index);
#endif

        /// <summary>Invokes the native <c>BringWindowToTop</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BringWindowToTop(IntPtr windowHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool BringWindowToTop(IntPtr windowHandle);
#endif

        /// <summary>Invokes the native <c>GetForegroundWindow</c> entry point.</summary>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetForegroundWindow();
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr GetForegroundWindow();
#endif

        /// <summary>Invokes the native <c>GetDesktopWindow</c> entry point.</summary>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetDesktopWindow();
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr GetDesktopWindow();
#endif

        /// <summary>Invokes the native <c>SetForegroundWindow</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr windowHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetForegroundWindow(IntPtr windowHandle);
#endif

        /// <summary>Invokes the native <c>SetFocus</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr SetFocus(IntPtr windowHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr SetFocus(IntPtr windowHandle);
#endif

        /// <summary>Invokes the native <c>GetWindowPlacement</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="windowPlacement">The native <paramref name="windowPlacement" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(IntPtr windowHandle, ref WindowPlacement windowPlacement);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetWindowPlacement(
            IntPtr windowHandle,
            ref WindowPlacement windowPlacement);
#endif

        /// <summary>Invokes the native <c>SetWindowPlacement</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="windowPlacement">The native <paramref name="windowPlacement" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPlacement(IntPtr windowHandle, ref WindowPlacement windowPlacement);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetWindowPlacement(
            IntPtr windowHandle,
            ref WindowPlacement windowPlacement);
#endif

        /// <summary>Invokes the native <c>IsIconic</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsIconic(IntPtr windowHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool IsIconic(IntPtr windowHandle);
#endif

        /// <summary>Invokes the native <c>IsZoomed</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsZoomed(IntPtr windowHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool IsZoomed(IntPtr windowHandle);
#endif

        /// <summary>Invokes the native <c>PrintWindow</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="deviceContextHandle">The native <paramref name="deviceContextHandle" /> value.</param>
        /// <param name="printWindowFlags">The native <paramref name="printWindowFlags" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PrintWindow(IntPtr windowHandle, IntPtr deviceContextHandle, PrintWindowFlags printWindowFlags);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool PrintWindow(
            IntPtr windowHandle,
            IntPtr deviceContextHandle,
            PrintWindowFlags printWindowFlags);
#endif

        /// <summary>Invokes the native <c>GetClassName</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="className">The native <paramref name="className" /> value.</param>
        /// <param name="maxCount">The native <paramref name="maxCount" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "GetClassNameW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern unsafe int GetClassName(IntPtr windowHandle, char* className, int maxCount);
#else
        [LibraryImport("user32", EntryPoint = "GetClassNameW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static unsafe partial int GetClassName(
            IntPtr windowHandle,
            char* className,
            int maxCount);
#endif

        /// <summary>Invokes the native <c>GetClassLong</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="index">The native <paramref name="index" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "GetClassLongW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetClassLong(IntPtr windowHandle, ClassLongIndex index);
#else
        [LibraryImport("user32", EntryPoint = "GetClassLongW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr GetClassLong(IntPtr windowHandle, ClassLongIndex index);
#endif

        /// <summary>Invokes the native <c>GetClassLongPtr</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="index">The native <paramref name="index" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "GetClassLongPtrW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetClassLongPtr(IntPtr windowHandle, ClassLongIndex index);
#else
        [LibraryImport("user32", EntryPoint = "GetClassLongPtrW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr GetClassLongPtr(IntPtr windowHandle, ClassLongIndex index);
#endif

        /// <summary>Invokes the native <c>SendMessage</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="windowsMessage">The native <paramref name="windowsMessage" /> value.</param>
        /// <param name="scrollBarCommand">The native <paramref name="scrollBarCommand" /> value.</param>
        /// <param name="parameter">The native <paramref name="parameter" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "SendMessageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int SendMessage(IntPtr windowHandle, WindowsMessages windowsMessage, ScrollBarCommands scrollBarCommand, int parameter);
#else
        [LibraryImport("user32", EntryPoint = "SendMessageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int SendMessage(
            IntPtr windowHandle,
            WindowsMessages windowsMessage,
            ScrollBarCommands scrollBarCommand,
            int parameter);
#endif

        /// <summary>Invokes the native <c>SendMessage</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="windowsMessage">The native <paramref name="windowsMessage" /> value.</param>
        /// <param name="wordParameter">The native <paramref name="wordParameter" /> value.</param>
        /// <param name="parameter">The native <paramref name="parameter" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "SendMessageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr SendMessage(IntPtr windowHandle, WindowsMessages windowsMessage, IntPtr wordParameter, IntPtr parameter);
#else
        [LibraryImport("user32", EntryPoint = "SendMessageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr SendMessage(
            IntPtr windowHandle,
            WindowsMessages windowsMessage,
            IntPtr wordParameter,
            IntPtr parameter);
#endif

        /// <summary>Invokes the native <c>SendMessage</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="windowsMessage">The native <paramref name="windowsMessage" /> value.</param>
        /// <param name="wordParameter">The native <paramref name="wordParameter" /> value.</param>
        /// <param name="parameter">The native <paramref name="parameter" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "SendMessageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr SendMessage(IntPtr windowHandle, WindowsMessages windowsMessage, int wordParameter, int parameter);
#else
        [LibraryImport("user32", EntryPoint = "SendMessageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr SendMessage(
            IntPtr windowHandle,
            WindowsMessages windowsMessage,
            int wordParameter,
            int parameter);
#endif

        /// <summary>Invokes the native <c>SendMessage</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="windowsMessage">The native <paramref name="windowsMessage" /> value.</param>
        /// <param name="wordParameter">The native <paramref name="wordParameter" /> value.</param>
        /// <param name="parameter">The native <paramref name="parameter" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "SendMessageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr SendMessage(IntPtr windowHandle, WindowsMessages windowsMessage, IntPtr wordParameter, ref TitleBarInfoEx parameter);
#else
        [LibraryImport("user32", EntryPoint = "SendMessageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr SendMessage(
            IntPtr windowHandle,
            WindowsMessages windowsMessage,
            IntPtr wordParameter,
            ref TitleBarInfoEx parameter);
#endif

        /// <summary>Invokes the native <c>SendMessage</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="windowsMessage">The native <paramref name="windowsMessage" /> value.</param>
        /// <param name="wordParameter">The native <paramref name="wordParameter" /> value.</param>
        /// <param name="parameter">The native <paramref name="parameter" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "SendMessageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern unsafe IntPtr SendMessage(IntPtr windowHandle, WindowsMessages windowsMessage, int wordParameter, char* parameter);
#else
        [LibraryImport("user32", EntryPoint = "SendMessageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static unsafe partial IntPtr SendMessage(
            IntPtr windowHandle,
            WindowsMessages windowsMessage,
            int wordParameter,
            char* parameter);
#endif

        /// <summary>Invokes the native <c>SendMessage</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="windowsMessage">The native <paramref name="windowsMessage" /> value.</param>
        /// <param name="wordParameter">The native <paramref name="wordParameter" /> value.</param>
        /// <param name="parameter">The native <paramref name="parameter" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "SendMessageW", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr SendMessage(IntPtr windowHandle, WindowsMessages windowsMessage, IntPtr wordParameter, string parameter);
#else
        [LibraryImport(
            "user32",
            EntryPoint = "SendMessageW",
            SetLastError = true,
            StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr SendMessage(
            IntPtr windowHandle,
            WindowsMessages windowsMessage,
            IntPtr wordParameter,
            string parameter);
#endif

        /// <summary>Invokes the native <c>GetWindowLong</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="index">The native <paramref name="index" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "GetWindowLongW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetWindowLong(IntPtr windowHandle, WindowLongIndex index);
#else
        [LibraryImport("user32", EntryPoint = "GetWindowLongW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr GetWindowLong(IntPtr windowHandle, WindowLongIndex index);
#endif

        /// <summary>Invokes the native <c>GetWindowLongPtr</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="index">The native <paramref name="index" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetWindowLongPtr(IntPtr windowHandle, WindowLongIndex index);
#else
        [LibraryImport("user32", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr GetWindowLongPtr(IntPtr windowHandle, WindowLongIndex index);
#endif

        /// <summary>Invokes the native <c>SetWindowLong</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="index">The native <paramref name="index" /> value.</param>
        /// <param name="replacementValue">The native <paramref name="replacementValue" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "SetWindowLongW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int SetWindowLong(IntPtr windowHandle, WindowLongIndex index, int replacementValue);
#else
        [LibraryImport("user32", EntryPoint = "SetWindowLongW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int SetWindowLong(
            IntPtr windowHandle,
            WindowLongIndex index,
            int replacementValue);
#endif

        /// <summary>Invokes the native <c>SetWindowLongPtr</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="index">The native <paramref name="index" /> value.</param>
        /// <param name="replacementValue">The native <paramref name="replacementValue" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr SetWindowLongPtr(IntPtr windowHandle, WindowLongIndex index, IntPtr replacementValue);
#else
        [LibraryImport("user32", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr SetWindowLongPtr(
            IntPtr windowHandle,
            WindowLongIndex index,
            IntPtr replacementValue);
#endif

        /// <summary>Invokes the native <c>MonitorFromWindow</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="monitorFrom">The native <paramref name="monitorFrom" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr MonitorFromWindow(IntPtr windowHandle, MonitorFrom monitorFrom);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr MonitorFromWindow(
            IntPtr windowHandle,
            MonitorFrom monitorFrom);
#endif

        /// <summary>Invokes the native <c>MonitorFromRect</c> entry point.</summary>
        /// <param name="rect">The native <paramref name="rect" /> value.</param>
        /// <param name="monitorFrom">The native <paramref name="monitorFrom" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr MonitorFromRect(ref NativeRect rect, MonitorFrom monitorFrom);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr MonitorFromRect(
            ref NativeRect rect,
            MonitorFrom monitorFrom);
#endif

        /// <summary>Invokes the native <c>GetWindowInfo</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="windowInfo">The native <paramref name="windowInfo" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowInfo(IntPtr windowHandle, ref WindowInfo windowInfo);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetWindowInfo(IntPtr windowHandle, ref WindowInfo windowInfo);
#endif

        /// <summary>Invokes the native <c>EnumWindows</c> entry point.</summary>
        /// <param name="enumFunc">The native <paramref name="enumFunc" /> value.</param>
        /// <param name="param">The native <paramref name="param" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumWindows(EnumWindowsProc enumFunc, IntPtr param);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool EnumWindows(EnumWindowsProc enumFunc, IntPtr param);
#endif

        /// <summary>Invokes the native <c>EnumThreadWindows</c> entry point.</summary>
        /// <param name="threadId">The native <paramref name="threadId" /> value.</param>
        /// <param name="enumFunc">The native <paramref name="enumFunc" /> value.</param>
        /// <param name="param">The native <paramref name="param" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumThreadWindows(int threadId, EnumWindowsProc enumFunc, IntPtr param);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool EnumThreadWindows(
            int threadId,
            EnumWindowsProc enumFunc,
            IntPtr param);
#endif

        /// <summary>Invokes the native <c>EnumChildWindows</c> entry point.</summary>
        /// <param name="parentWindowHandle">The native <paramref name="parentWindowHandle" /> value.</param>
        /// <param name="enumFunc">The native <paramref name="enumFunc" /> value.</param>
        /// <param name="param">The native <paramref name="param" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumChildWindows(IntPtr parentWindowHandle, EnumWindowsProc enumFunc, IntPtr param);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool EnumChildWindows(
            IntPtr parentWindowHandle,
            EnumWindowsProc enumFunc,
            IntPtr param);
#endif

        /// <summary>Invokes the native <c>GetScrollInfo</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="scrollBar">The native <paramref name="scrollBar" /> value.</param>
        /// <param name="scrollInfo">The native <paramref name="scrollInfo" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetScrollInfo(IntPtr windowHandle, ScrollBarTypes scrollBar, ref ScrollInfo scrollInfo);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetScrollInfo(
            IntPtr windowHandle,
            ScrollBarTypes scrollBar,
            ref ScrollInfo scrollInfo);
#endif

        /// <summary>Invokes the native <c>SetScrollInfo</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="scrollBar">The native <paramref name="scrollBar" /> value.</param>
        /// <param name="scrollInfo">The native <paramref name="scrollInfo" /> value.</param>
        /// <param name="redraw">The native <paramref name="redraw" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int SetScrollInfo(IntPtr windowHandle, ScrollBarTypes scrollBar, ref ScrollInfo scrollInfo, [MarshalAs(UnmanagedType.Bool)] bool redraw);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int SetScrollInfo(
            IntPtr windowHandle,
            ScrollBarTypes scrollBar,
            ref ScrollInfo scrollInfo,
            [MarshalAs(UnmanagedType.Bool)] bool redraw);
#endif

        /// <summary>Invokes the native <c>ShowScrollBar</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="scrollBar">The native <paramref name="scrollBar" /> value.</param>
        /// <param name="show">The native <paramref name="show" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowScrollBar(IntPtr windowHandle, ScrollBarTypes scrollBar, [MarshalAs(UnmanagedType.Bool)] bool show);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool ShowScrollBar(
            IntPtr windowHandle,
            ScrollBarTypes scrollBar,
            [MarshalAs(UnmanagedType.Bool)] bool show);
#endif

        /// <summary>Invokes the native <c>GetScrollBarInfo</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="objectId">The native <paramref name="objectId" /> value.</param>
        /// <param name="scrollBarInfo">The native <paramref name="scrollBarInfo" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetScrollBarInfo(IntPtr windowHandle, ObjectIdentifiers objectId, ref ScrollBarInfo scrollBarInfo);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetScrollBarInfo(
            IntPtr windowHandle,
            ObjectIdentifiers objectId,
            ref ScrollBarInfo scrollBarInfo);
#endif

        /// <summary>Invokes the native <c>SetWindowDisplayAffinity</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="windowDisplayAffinity">The native <paramref name="windowDisplayAffinity" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowDisplayAffinity(IntPtr windowHandle, WindowDisplayAffinity windowDisplayAffinity);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetWindowDisplayAffinity(
            IntPtr windowHandle,
            WindowDisplayAffinity windowDisplayAffinity);
#endif

        /// <summary>Invokes the native <c>GetWindowDisplayAffinity</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="windowDisplayAffinity">The native <paramref name="windowDisplayAffinity" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowDisplayAffinity(IntPtr windowHandle, out WindowDisplayAffinity windowDisplayAffinity);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetWindowDisplayAffinity(
            IntPtr windowHandle,
            out WindowDisplayAffinity windowDisplayAffinity);
#endif

        /// <summary>Invokes the native <c>GetWindowRgn</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="regionHandle">The native <paramref name="regionHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern RegionResults GetWindowRgn(IntPtr windowHandle, SafeHandle regionHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial RegionResults GetWindowRgn(
            IntPtr windowHandle,
            SafeHandle regionHandle);
#endif

        /// <summary>Invokes the native <c>SetWindowPos</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="insertAfterWindowHandle">The native <paramref name="insertAfterWindowHandle" /> value.</param>
        /// <param name="x">The native <paramref name="x" /> value.</param>
        /// <param name="y">The native <paramref name="y" /> value.</param>
        /// <param name="width">The native <paramref name="width" /> value.</param>
        /// <param name="height">The native <paramref name="height" /> value.</param>
        /// <param name="flags">The native <paramref name="flags" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(IntPtr windowHandle, IntPtr insertAfterWindowHandle, int x, int y, int width, int height, WindowPos flags);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetWindowPos(
            IntPtr windowHandle,
            IntPtr insertAfterWindowHandle,
            int x,
            int y,
            int width,
            int height,
            WindowPos flags);
#endif

        /// <summary>Invokes the native <c>GetTopWindow</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetTopWindow(IntPtr windowHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr GetTopWindow(IntPtr windowHandle);
#endif

        /// <summary>Invokes the native <c>GetWindowDC</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetWindowDC(IntPtr windowHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr GetWindowDC(IntPtr windowHandle);
#endif

        /// <summary>Invokes the native <c>GetDC</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetDC(IntPtr windowHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr GetDC(IntPtr windowHandle);
#endif

        /// <summary>Invokes the native <c>ReleaseDC</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="deviceContextHandle">The native <paramref name="deviceContextHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReleaseDC(IntPtr windowHandle, IntPtr deviceContextHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool ReleaseDC(IntPtr windowHandle, IntPtr deviceContextHandle);
#endif

        /// <summary>Invokes the native <c>FindWindow</c> entry point.</summary>
        /// <param name="className">The native <paramref name="className" /> value.</param>
        /// <param name="windowName">The native <paramref name="windowName" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "FindWindowW", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr FindWindow(string className, string windowName);
#else
        [LibraryImport(
            "user32",
            EntryPoint = "FindWindowW",
            SetLastError = true,
            StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr FindWindow(string className, string windowName);
#endif

        /// <summary>Invokes the native <c>FindWindowEx</c> entry point.</summary>
        /// <param name="parentWindowHandle">The native <paramref name="parentWindowHandle" /> value.</param>
        /// <param name="childAfterWindowHandle">The native <paramref name="childAfterWindowHandle" /> value.</param>
        /// <param name="className">The native <paramref name="className" /> value.</param>
        /// <param name="windowName">The native <paramref name="windowName" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "FindWindowExW", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr FindWindowEx(IntPtr parentWindowHandle, IntPtr childAfterWindowHandle, string className, string windowName);
#else
        [LibraryImport(
            "user32",
            EntryPoint = "FindWindowExW",
            SetLastError = true,
            StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr FindWindowEx(
            IntPtr parentWindowHandle,
            IntPtr childAfterWindowHandle,
            string className,
            string windowName);
#endif

        /// <summary>Invokes the native <c>GetGuiResources</c> entry point.</summary>
        /// <param name="processHandle">The native <paramref name="processHandle" /> value.</param>
        /// <param name="flags">The native <paramref name="flags" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern uint GetGuiResources(IntPtr processHandle, uint flags);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial uint GetGuiResources(IntPtr processHandle, uint flags);
#endif

        /// <summary>Invokes the native <c>SendMessageTimeout</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <param name="message">The native <paramref name="message" /> value.</param>
        /// <param name="wordParameter">The native <paramref name="wordParameter" /> value.</param>
        /// <param name="parameter">The native <paramref name="parameter" /> value.</param>
        /// <param name="flags">The native <paramref name="flags" /> value.</param>
        /// <param name="timeout">The native <paramref name="timeout" /> value.</param>
        /// <param name="resultPointer">The native <paramref name="resultPointer" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "SendMessageTimeoutW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SendMessageTimeout(
            IntPtr windowHandle,
            uint message,
            IntPtr wordParameter,
            IntPtr parameter,
            SendMessageTimeoutFlags flags,
            uint timeout,
            out IntPtr resultPointer);
#else
        [LibraryImport("user32", EntryPoint = "SendMessageTimeoutW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SendMessageTimeout(
            IntPtr windowHandle,
            uint message,
            IntPtr wordParameter,
            IntPtr parameter,
            SendMessageTimeoutFlags flags,
            uint timeout,
            out IntPtr resultPointer);
#endif

        /// <summary>Invokes the native <c>GetPhysicalCursorPos</c> entry point.</summary>
        /// <param name="cursorLocation">The native <paramref name="cursorLocation" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetPhysicalCursorPos(out NativePoint cursorLocation);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetPhysicalCursorPos(out NativePoint cursorLocation);
#endif

        /// <summary>Invokes the native <c>MapWindowPoints</c> entry point.</summary>
        /// <param name="windowHandleFrom">The native <paramref name="windowHandleFrom" /> value.</param>
        /// <param name="windowHandleTo">The native <paramref name="windowHandleTo" /> value.</param>
        /// <param name="points">The native <paramref name="points" /> value.</param>
        /// <param name="pointCount">The native <paramref name="pointCount" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int MapWindowPoints(IntPtr windowHandleFrom, IntPtr windowHandleTo, ref NativePoint points, int pointCount);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int MapWindowPoints(
            IntPtr windowHandleFrom,
            IntPtr windowHandleTo,
            ref NativePoint points,
            int pointCount);
#endif

        /// <summary>Invokes the native <c>GetSystemMetrics</c> entry point.</summary>
        /// <param name="index">The native <paramref name="index" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetSystemMetrics(SystemMetric index);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int GetSystemMetrics(SystemMetric index);
#endif

        /// <summary>Invokes the native <c>SetCapture</c> entry point.</summary>
        /// <param name="windowHandle">The native <paramref name="windowHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr SetCapture(IntPtr windowHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr SetCapture(IntPtr windowHandle);
#endif

        /// <summary>Invokes the native <c>ReleaseCapture</c> entry point.</summary>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReleaseCapture();
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool ReleaseCapture();
#endif

        /// <summary>Invokes the native <c>OpenInputDesktop</c> entry point.</summary>
        /// <param name="flags">The native <paramref name="flags" /> value.</param>
        /// <param name="inherit">The native <paramref name="inherit" /> value.</param>
        /// <param name="desiredAccess">The native <paramref name="desiredAccess" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr OpenInputDesktop(uint flags, [MarshalAs(UnmanagedType.Bool)] bool inherit, DesktopAccessRight desiredAccess);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr OpenInputDesktop(
            uint flags,
            [MarshalAs(UnmanagedType.Bool)] bool inherit,
            DesktopAccessRight desiredAccess);
#endif

        /// <summary>Invokes the native <c>SetThreadDesktop</c> entry point.</summary>
        /// <param name="desktopHandle">The native <paramref name="desktopHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetThreadDesktop(IntPtr desktopHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetThreadDesktop(IntPtr desktopHandle);
#endif

        /// <summary>Invokes the native <c>CloseDesktop</c> entry point.</summary>
        /// <param name="desktopHandle">The native <paramref name="desktopHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseDesktop(IntPtr desktopHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool CloseDesktop(IntPtr desktopHandle);
#endif

        /// <summary>Invokes the native <c>SystemParametersInfo</c> entry point.</summary>
        /// <param name="action">The native <paramref name="action" /> value.</param>
        /// <param name="parameterValue">The native <paramref name="parameterValue" /> value.</param>
        /// <param name="value">The native <paramref name="value" /> value.</param>
        /// <param name="behavior">The native <paramref name="behavior" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "SystemParametersInfoW", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SystemParametersInfo(SystemParametersInfoActions action, uint parameterValue, string value, SystemParametersInfoBehaviors behavior);
#else
        [LibraryImport(
            "user32",
            EntryPoint = "SystemParametersInfoW",
            SetLastError = true,
            StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SystemParametersInfo(
            SystemParametersInfoActions action,
            uint parameterValue,
            string value,
            SystemParametersInfoBehaviors behavior);
#endif

        /// <summary>Invokes the native <c>SystemParametersInfo</c> entry point.</summary>
        /// <param name="action">The native <paramref name="action" /> value.</param>
        /// <param name="parameterValue">The native <paramref name="parameterValue" /> value.</param>
        /// <param name="value">The native <paramref name="value" /> value.</param>
        /// <param name="behavior">The native <paramref name="behavior" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "SystemParametersInfoW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool SystemParametersInfo(SystemParametersInfoActions action, uint parameterValue, char* value, SystemParametersInfoBehaviors behavior);
#else
        [LibraryImport("user32", EntryPoint = "SystemParametersInfoW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static unsafe partial bool SystemParametersInfo(
            SystemParametersInfoActions action,
            uint parameterValue,
            char* value,
            SystemParametersInfoBehaviors behavior);
#endif

        /// <summary>Invokes the native <c>SystemParametersInfo</c> entry point.</summary>
        /// <param name="action">The native <paramref name="action" /> value.</param>
        /// <param name="parameterValue">The native <paramref name="parameterValue" /> value.</param>
        /// <param name="animationInfo">The native <paramref name="animationInfo" /> value.</param>
        /// <param name="behavior">The native <paramref name="behavior" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "SystemParametersInfoW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SystemParametersInfo(SystemParametersInfoActions action, uint parameterValue, ref AnimationInfo animationInfo, SystemParametersInfoBehaviors behavior);
#else
        [LibraryImport("user32", EntryPoint = "SystemParametersInfoW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SystemParametersInfo(
            SystemParametersInfoActions action,
            uint parameterValue,
            ref AnimationInfo animationInfo,
            SystemParametersInfoBehaviors behavior);
#endif

        /// <summary>Invokes the native <c>LockWorkStation</c> entry point.</summary>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool LockWorkStation();
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool LockWorkStation();
#endif

        /// <summary>Invokes the native <c>GetCursorInfo</c> entry point.</summary>
        /// <param name="cursorInfo">The native <paramref name="cursorInfo" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorInfo(ref CursorInfo cursorInfo);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetCursorInfo(ref CursorInfo cursorInfo);
#endif

        /// <summary>Invokes the native <c>DestroyCursor</c> entry point.</summary>
        /// <param name="cursorHandle">The native <paramref name="cursorHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyCursor(IntPtr cursorHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool DestroyCursor(IntPtr cursorHandle);
#endif

        /// <summary>Invokes the native <c>FillRect</c> entry point.</summary>
        /// <param name="deviceContextHandle">The native <paramref name="deviceContextHandle" /> value.</param>
        /// <param name="rectangle">The native <paramref name="rectangle" /> value.</param>
        /// <param name="brushHandle">The native <paramref name="brushHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int FillRect(IntPtr deviceContextHandle, ref NativeRect rectangle, IntPtr brushHandle);
#else
        [LibraryImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int FillRect(
            IntPtr deviceContextHandle,
            ref NativeRect rectangle,
            IntPtr brushHandle);
#endif

        /// <summary>Invokes the native <c>EnumDisplayMonitorsNative</c> entry point.</summary>
        /// <param name="deviceContextHandle">The native <paramref name="deviceContextHandle" /> value.</param>
        /// <param name="clipRectangle">The native <paramref name="clipRectangle" /> value.</param>
        /// <param name="enumCallback">The native <paramref name="enumCallback" /> value.</param>
        /// <param name="data">The native <paramref name="data" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "EnumDisplayMonitors", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int EnumDisplayMonitorsNative(IntPtr deviceContextHandle, IntPtr clipRectangle, IntPtr enumCallback, IntPtr data);
#else
        [LibraryImport("user32", EntryPoint = "EnumDisplayMonitors", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int EnumDisplayMonitorsNative(
            IntPtr deviceContextHandle,
            IntPtr clipRectangle,
            IntPtr enumCallback,
            IntPtr data);
#endif

        /// <summary>Retrieves information about a display monitor.</summary>
        /// <param name="monitorHandle">The monitor handle.</param>
        /// <param name="monitorInfo">The monitor information value.</param>
        /// <returns>True when monitor information was retrieved.</returns>
        internal static bool GetMonitorInfo(IntPtr monitorHandle, ref MonitorInfoEx monitorInfo)
        {
            Span<byte> buffer = stackalloc byte[MonitorInfoExSize];
            BinaryPrimitives.WriteInt32LittleEndian(buffer, MonitorInfoExSize);
            if (!GetMonitorInfoNative(monitorHandle, ref MemoryMarshal.GetReference(buffer)))
            {
                return false;
            }

            NativeRect monitor = MemoryMarshal.Read<NativeRect>(
                buffer.Slice(MonitorInfoExMonitorOffset, NativeRectSize));
            NativeRect workArea = MemoryMarshal.Read<NativeRect>(
                buffer.Slice(MonitorInfoExWorkAreaOffset, NativeRectSize));
            MonitorInfoFlags flags = (MonitorInfoFlags)
                checked((int)BinaryPrimitives.ReadUInt32LittleEndian(
                    buffer.Slice(MonitorInfoExFlagsOffset, sizeof(uint))));
            string deviceName = NativeUtf16String.ReadNullTerminated(
                buffer.Slice(MonitorInfoExDeviceNameOffset, MonitorInfoExDeviceNameSize));
            monitorInfo = new(MonitorInfoExSize, monitor, workArea, flags, deviceName);
            return true;
        }

        /// <summary>Invokes the native <c>GetMonitorInfoNative</c> entry point.</summary>
        /// <param name="monitorHandle">The native <paramref name="monitorHandle" /> value.</param>
        /// <param name="monitorInfo">The native <paramref name="monitorInfo" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32", EntryPoint = "GetMonitorInfoW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetMonitorInfoNative(IntPtr monitorHandle, ref byte monitorInfo);
#else
        [LibraryImport("user32", EntryPoint = "GetMonitorInfoW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetMonitorInfoNative(
            IntPtr monitorHandle,
            ref byte monitorInfo);
#endif
    }
}

/// <summary>Native wrappers for the User32 DLL.</summary>
public static partial class User32Api
{
    /// <summary>Creates a Win32 exception annotated with the method name.</summary>
    /// <param name="method">The method name.</param>
    /// <returns>The exception.</returns>
    public static Exception CreateWin32Exception(string method) =>
        new Win32Exception { Data = { { (object)"Method", (object)method } } };

    /// <summary>Gets a window device-context handle.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The device-context handle.</returns>
    public static IntPtr GetWindowDC(IntPtr windowHandle) =>
        NativeMethods.GetWindowDC(windowHandle);

    /// <summary>Gets a device-context handle.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The device-context handle.</returns>
    public static IntPtr GetDC(IntPtr windowHandle) => NativeMethods.GetDC(windowHandle);

    /// <summary>Releases a device-context handle.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="deviceContextHandle">The device-context handle.</param>
    /// <returns>True when the device context was released.</returns>
    public static bool ReleaseDC(IntPtr windowHandle, IntPtr deviceContextHandle) =>
        NativeMethods.ReleaseDC(windowHandle, deviceContextHandle);

    /// <summary>Gets the desktop window handle.</summary>
    /// <returns>The desktop window handle.</returns>
    public static IntPtr GetDesktopWindow() => NativeMethods.GetDesktopWindow();

    /// <summary>Opens the current input desktop.</summary>
    /// <param name="flags">The desktop open flags.</param>
    /// <param name="inherit">True when the handle is inheritable.</param>
    /// <param name="desiredAccess">The desired desktop access rights.</param>
    /// <returns>The opened desktop handle.</returns>
    public static IntPtr OpenInputDesktop(
        uint flags,
        bool inherit,
        DesktopAccessRight desiredAccess) =>
        Volatile.Read(ref _operations).OpenInputDesktop(flags, inherit, desiredAccess);

    /// <summary>Assigns the current thread to a desktop.</summary>
    /// <param name="desktopHandle">The desktop handle.</param>
    /// <returns>True when the current thread was assigned to the desktop.</returns>
    public static bool SetThreadDesktop(IntPtr desktopHandle) =>
        Volatile.Read(ref _operations).SetThreadDesktop(desktopHandle);

    /// <summary>Closes an input desktop handle.</summary>
    /// <param name="desktopHandle">The desktop handle.</param>
    /// <returns>True when the desktop handle was closed.</returns>
    public static bool CloseDesktop(IntPtr desktopHandle) =>
        Volatile.Read(ref _operations).CloseDesktop(desktopHandle);

    /// <summary>Gets the current cursor location in physical screen coordinates when supported.</summary>
    /// <returns>The cursor location.</returns>
    public static NativePoint GetCursorLocation()
    {
        if (WindowsRuntimeVersion.IsAtLeast(WindowsVistaMajorVersion, 0, 0, 0)
            && _canCallGetPhysicalCursorPos)
        {
            try
            {
                if (Volatile.Read(ref _operations).GetPhysicalCursorPos(out var cursorLocation))
                {
                    return cursorLocation;
                }
            }
            catch (EntryPointNotFoundException)
            {
                _canCallGetPhysicalCursorPos = false;
            }
        }

        return Volatile.Read(ref _operations).GetCursorPosition();
    }

    /// <summary>Gets information about a display monitor.</summary>
    /// <param name="monitorHandle">The monitor handle.</param>
    /// <param name="index">The one-based monitor index.</param>
    /// <returns>The display information, or <see langword="null" /> when the monitor cannot be queried.</returns>
    public static DisplayInfo GetDisplayInfo(IntPtr monitorHandle, int index)
    {
        MonitorInfoEx monitorInfo = MonitorInfoEx.Create();
        return !Volatile.Read(ref _operations).GetMonitorInfo(monitorHandle, ref monitorInfo)
            ? null
            : new DisplayInfo
            {
                MonitorHandle = new(monitorHandle),
                Index = index,
                ScreenWidth = monitorInfo.Monitor.Width,
                ScreenHeight = monitorInfo.Monitor.Height,
                Bounds = monitorInfo.Monitor,
                WorkingArea = monitorInfo.WorkArea,
                IsPrimary = (
                    (monitorInfo.Flags & MonitorInfoFlags.Primary) == MonitorInfoFlags.Primary),
                DeviceName = monitorInfo.DeviceName,
            };
    }

    /// <summary>Enumerates the display monitors attached to the desktop.</summary>
    /// <returns>The display-monitor information values.</returns>
    public static IReadOnlyList<DisplayInfo> EnumDisplays()
    {
        List<DisplayInfo> displays = new();
        GCHandle state = GCHandle.Alloc(displays);
        try
        {
            _ = NativeMethods.EnumDisplayMonitorsNative(
                IntPtr.Zero,
                IntPtr.Zero,
                AddDisplayMonitorPointer,
                GCHandle.ToIntPtr(state));
            return displays;
        }
        finally
        {
            state.Free();
        }
    }

    /// <summary>Gets the window handles created by a thread.</summary>
    /// <param name="threadId">The thread identifier.</param>
    /// <returns>The window handles created by the thread.</returns>
    public static List<IntPtr> EnumThreadWindows(int threadId)
    {
        List<IntPtr> windowHandles = new();
        _ = NativeMethods.EnumThreadWindows(
            threadId,
            (windowHandle, _) =>
            {
                windowHandles.Add(windowHandle);
                return true;
            },
            IntPtr.Zero);
        return windowHandles;
    }

    /// <summary>Gets a class value using the correct pointer-width entry point.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="index">The class-value index.</param>
    /// <returns>The requested class value.</returns>
    public static IntPtr GetClassLongWrapper(IntPtr windowHandle, ClassLongIndex index) =>
        !Volatile.Read(ref _operations).Is64BitProcess()
            ? Volatile.Read(ref _operations).GetClassLong(windowHandle, index)
            : Volatile.Read(ref _operations).GetClassLongPtr(windowHandle, index);

    /// <summary>Gets a window class name.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The class name, or an empty string when no class name is available.</returns>
    public static unsafe string GetClassname(IntPtr windowHandle)
    {
        char* className = stackalloc char[StandardWindowTextCapacity];
        int characterCount = NativeMethods.GetClassName(
            windowHandle,
            className,
            StandardWindowTextCapacity);
        return characterCount != 0 ? new(className, 0, characterCount) : string.Empty;
    }

    /// <summary>Gets the number of GDI objects used by the current process.</summary>
    /// <returns>The GDI object count.</returns>
    public static uint GetGuiResourcesGdiCount()
    {
        using Process process = Process.GetCurrentProcess();
        return NativeMethods.GetGuiResources(process.Handle, (uint)GdiObjectCountFlag);
    }

    /// <summary>Gets the number of USER objects used by the current process.</summary>
    /// <returns>The USER object count.</returns>
    public static uint GetGuiResourcesUserCount()
    {
        using Process process = Process.GetCurrentProcess();
        return NativeMethods.GetGuiResources(process.Handle, (uint)UserObjectCountFlag);
    }

    /// <summary>Gets a window caption.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The caption text, or an empty string when no caption is available.</returns>
    public static unsafe string GetText(IntPtr windowHandle)
    {
        char* text = stackalloc char[StandardWindowTextCapacity];
        int characterCount = NativeMethods.GetWindowText(
            windowHandle,
            text,
            StandardWindowTextCapacity);
        return characterCount != 0 ? new(text, 0, characterCount) : string.Empty;
    }

    /// <summary>Gets control text by sending <c>WM_GETTEXT</c>.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The control text, or <see langword="null" /> when the control has no text.</returns>
    public static unsafe string GetTextFromWindow(IntPtr windowHandle)
    {
        int textLength = NativeMethods
            .SendMessage(windowHandle, WindowsMessages.WM_GETTEXTLENGTH, 0, 0)
            .ToInt32();
        if (textLength <= 0)
        {
            return null;
        }

        int capacity = checked(textLength + 1);
        if (capacity <= StackTextBufferLimit)
        {
            char* text = stackalloc char[capacity];
            _ = NativeMethods.SendMessage(windowHandle, WindowsMessages.WM_GETTEXT, capacity, text);
            return new(text, 0, textLength);
        }

        char[] array = new char[capacity];
        fixed (char* text2 = &MemoryMarshal.GetReference(array.AsSpan()))
        {
            _ = NativeMethods.SendMessage(
                windowHandle,
                WindowsMessages.WM_GETTEXT,
                capacity,
                text2);
        }

        return new(array, 0, textLength);
    }

    /// <summary>Gets extended title-bar information for a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The title-bar information.</returns>
    public static TitleBarInfoEx GetTitleBarInfoEx(IntPtr windowHandle)
    {
        TitleBarInfoEx result = TitleBarInfoEx.Create();
        _ = NativeMethods.SendMessage(
            windowHandle,
            WindowsMessages.WM_GETTITLEBARINFOEX,
            IntPtr.Zero,
            ref result);
        return result;
    }

    /// <summary>Gets a window value using the correct pointer-width entry point.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="index">The window-value index.</param>
    /// <returns>The requested window value.</returns>
    public static IntPtr GetWindowLongWrapper(IntPtr windowHandle, WindowLongIndex index) =>
        !Volatile.Read(ref _operations).Is64BitProcess()
            ? Volatile.Read(ref _operations).GetWindowLong(windowHandle, index)
            : Volatile.Read(ref _operations).GetWindowLongPtr(windowHandle, index);

    /// <summary>Sets a window value using the correct pointer-width entry point.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="index">The window-value index.</param>
    /// <param name="replacementValue">The replacement value.</param>
    /// <returns>The previous window value.</returns>
    public static IntPtr SetWindowLongWrapper(
        IntPtr windowHandle,
        WindowLongIndex index,
        IntPtr replacementValue) =>
        !Volatile.Read(ref _operations).Is64BitProcess()
            ? new(Volatile.Read(ref _operations).SetWindowLong(windowHandle, index, replacementValue.ToInt32()))
            : Volatile.Read(ref _operations).SetWindowLongPtr(windowHandle, index, replacementValue);

    /// <summary>Sets extended window-style flags.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="styleFlags">The replacement style flags.</param>
    /// <returns>The previous window-style value.</returns>
    public static IntPtr SetExtendedWindowStyle(
        IntPtr windowHandle,
        ExtendedWindowStyleFlags styleFlags) =>
        SetWindowLongWrapper(
            windowHandle,
            WindowLongIndex.GWL_EXSTYLE,
            new(checked((uint)styleFlags)));

    /// <summary>Sets window-style flags.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="styleFlags">The replacement style flags.</param>
    /// <returns>The previous window-style value.</returns>
    public static IntPtr SetWindowStyle(IntPtr windowHandle, WindowStyleFlags styleFlags) =>
        SetWindowLongWrapper(windowHandle, WindowLongIndex.GWL_STYLE, new((long)styleFlags));

    /// <summary>Tries to send a message without waiting indefinitely for the target window.</summary>
    /// <param name="windowHandle">The target window handle.</param>
    /// <param name="message">The window message.</param>
    /// <param name="wordParameter">The message word parameter.</param>
    /// <param name="result">The message result.</param>
    /// <returns>True when the message was delivered.</returns>
    public static bool TrySendMessage(
        IntPtr windowHandle,
        WindowsMessages message,
        IntPtr wordParameter,
        out IntPtr result) => TrySendMessage(
            windowHandle,
            message,
            wordParameter,
            IntPtr.Zero,
            DefaultSendMessageTimeoutMilliseconds,
            out result);

    /// <summary>Tries to send a message without waiting indefinitely for the target window.</summary>
    /// <param name="windowHandle">The target window handle.</param>
    /// <param name="message">The window message.</param>
    /// <param name="wordParameter">The message word parameter.</param>
    /// <param name="longParameter">The message long parameter.</param>
    /// <param name="timeout">The timeout in milliseconds.</param>
    /// <param name="result">The message result.</param>
    /// <returns>True when the message was delivered.</returns>
    public static bool TrySendMessage(
        IntPtr windowHandle,
        WindowsMessages message,
        IntPtr wordParameter,
        IntPtr longParameter,
        uint timeout,
        out IntPtr result)
    {
        bool num = NativeMethods.SendMessageTimeout(
            windowHandle,
            (uint)message,
            wordParameter,
            longParameter,
            SendMessageTimeoutFlags.AbortIfHung | SendMessageTimeoutFlags.ErrorOnExit,
            timeout,
            out result);
        if (!num)
        {
            result = IntPtr.Zero;
        }

        return num;
    }

    /// <summary>Determines whether a window is visible.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>True when the window is visible.</returns>
    public static bool IsWindowVisible(IntPtr windowHandle) =>
        NativeMethods.IsWindowVisible(windowHandle);

    /// <summary>Determines whether a handle identifies an existing window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>True when the window exists.</returns>
    public static bool IsWindow(IntPtr windowHandle) => NativeMethods.IsWindow(windowHandle);

    /// <summary>Gets the creating thread and process identifiers for a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="processId">The receiving process identifier.</param>
    /// <returns>The creating thread identifier.</returns>
    public static int GetWindowThreadProcessId(IntPtr windowHandle, out int processId) =>
        NativeMethods.GetWindowThreadProcessId(windowHandle, out processId);

    /// <summary>Gets the creating thread identifier for a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="processId">A null process-identifier pointer.</param>
    /// <returns>The creating thread identifier.</returns>
    public static int GetWindowThreadProcessId(IntPtr windowHandle, IntPtr processId) =>
        NativeMethods.GetWindowThreadProcessId(windowHandle, processId);

    /// <summary>Attaches or detaches two thread input queues.</summary>
    /// <param name="attachThreadId">The attaching thread identifier.</param>
    /// <param name="attachToThreadId">The target thread identifier.</param>
    /// <param name="attach">A nonzero value attaches the queues.</param>
    /// <returns>The native result value.</returns>
    public static IntPtr AttachThreadInput(int attachThreadId, int attachToThreadId, int attach) =>
        NativeMethods.AttachThreadInput(attachThreadId, attachToThreadId, attach);

    /// <summary>Gets the parent window handle.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The parent window handle.</returns>
    public static IntPtr GetParent(IntPtr windowHandle) => NativeMethods.GetParent(windowHandle);

    /// <summary>Changes a window parent.</summary>
    /// <param name="childWindowHandle">The child window handle.</param>
    /// <param name="newParentWindowHandle">The replacement parent handle.</param>
    /// <returns>The previous parent window handle.</returns>
    public static IntPtr SetParent(IntPtr childWindowHandle, IntPtr newParentWindowHandle) =>
        NativeMethods.SetParent(childWindowHandle, newParentWindowHandle);

    /// <summary>Gets a related window handle.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="command">The relationship to retrieve.</param>
    /// <returns>The related window handle.</returns>
    public static IntPtr GetWindow(IntPtr windowHandle, GetWindowCommands command) =>
        NativeMethods.GetWindow(windowHandle, command);

    /// <summary>Changes a window show state.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="command">The show-state command.</param>
    /// <returns>True when the window was previously visible.</returns>
    public static bool ShowWindow(IntPtr windowHandle, ShowWindowCommands command) =>
        NativeMethods.ShowWindow(windowHandle, command);

    /// <summary>Sets a window caption.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="caption">The caption text.</param>
    /// <returns>The native result value.</returns>
    public static int SetWindowText(IntPtr windowHandle, string caption) =>
        NativeMethods.SetWindowText(windowHandle, caption);

    /// <summary>Gets the length of a window caption.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The caption length in UTF-16 characters.</returns>
    public static int GetWindowTextLength(IntPtr windowHandle) =>
        NativeMethods.GetWindowTextLength(windowHandle);

    /// <summary>Gets a system color.</summary>
    /// <param name="index">The system-color index.</param>
    /// <returns>The color value in BGR byte order.</returns>
    public static uint GetSysColor(SystemColorIndex index) =>
        NativeMethods.GetSysColor(index.Value);

    /// <summary>Brings a window to the top of the Z order.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>True when the window was brought to the top.</returns>
    public static bool BringWindowToTop(IntPtr windowHandle) =>
        NativeMethods.BringWindowToTop(windowHandle);

    /// <summary>Gets the foreground window handle.</summary>
    /// <returns>The foreground window handle.</returns>
    public static IntPtr GetForegroundWindow() => NativeMethods.GetForegroundWindow();

    /// <summary>Sets the foreground window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>True when the window became foreground.</returns>
    public static bool SetForegroundWindow(IntPtr windowHandle) =>
        NativeMethods.SetForegroundWindow(windowHandle);

    /// <summary>Sets keyboard focus to a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The previous focus window handle.</returns>
    public static IntPtr SetFocus(IntPtr windowHandle) => Volatile.Read(ref _operations).SetFocus(windowHandle);

    /// <summary>Gets window-placement information.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="placement">The placement information.</param>
    /// <returns>True when the information was retrieved.</returns>
    public static bool GetWindowPlacement(IntPtr windowHandle, ref WindowPlacement placement) =>
        NativeMethods.GetWindowPlacement(windowHandle, ref placement);

    /// <summary>Sets window-placement information.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="placement">The placement information.</param>
    /// <returns>True when the information was set.</returns>
    public static bool SetWindowPlacement(IntPtr windowHandle, ref WindowPlacement placement) =>
        NativeMethods.SetWindowPlacement(windowHandle, ref placement);

    /// <summary>Determines whether a window is minimized.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>True when the window is minimized.</returns>
    public static bool IsIconic(IntPtr windowHandle) => NativeMethods.IsIconic(windowHandle);

    /// <summary>Determines whether a window is maximized.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>True when the window is maximized.</returns>
    public static bool IsZoomed(IntPtr windowHandle) => NativeMethods.IsZoomed(windowHandle);

    /// <summary>Copies a window into a device context.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="deviceContextHandle">The target device-context handle.</param>
    /// <param name="flags">The copy flags.</param>
    /// <returns>True when the window was copied.</returns>
    public static bool PrintWindow(
        IntPtr windowHandle,
        IntPtr deviceContextHandle,
        PrintWindowFlags flags) => NativeMethods.PrintWindow(windowHandle, deviceContextHandle, flags);

    /// <summary>Sends a system command to a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="message">The window message.</param>
    /// <param name="command">The system command.</param>
    /// <param name="longParameter">The message long parameter.</param>
    /// <returns>The message result.</returns>
    public static IntPtr SendMessage(
        IntPtr windowHandle,
        WindowsMessages message,
        SysCommands command,
        IntPtr longParameter) => NativeMethods.SendMessage(windowHandle, message, new((int)command), longParameter);

    /// <summary>Sends a scroll-bar command to a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="message">The window message.</param>
    /// <param name="command">The scroll-bar command.</param>
    /// <param name="parameter">The message long parameter.</param>
    /// <returns>The message result.</returns>
    public static int SendMessage(
        IntPtr windowHandle,
        WindowsMessages message,
        ScrollBarCommands command,
        int parameter) => NativeMethods.SendMessage(windowHandle, message, command, parameter);

    /// <summary>Sends a message to a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="message">The window message.</param>
    /// <param name="wordParameter">The message word parameter.</param>
    /// <param name="longParameter">The message long parameter.</param>
    /// <returns>The message result.</returns>
    public static IntPtr SendMessage(
        IntPtr windowHandle,
        WindowsMessages message,
        IntPtr wordParameter,
        IntPtr longParameter) => NativeMethods.SendMessage(windowHandle, message, wordParameter, longParameter);

    /// <summary>Sends an integer message to a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="message">The window message.</param>
    /// <param name="wordParameter">The message word parameter.</param>
    /// <param name="longParameter">The message long parameter.</param>
    /// <returns>The message result.</returns>
    public static IntPtr SendMessage(
        IntPtr windowHandle,
        WindowsMessages message,
        int wordParameter,
        int longParameter) => NativeMethods.SendMessage(windowHandle, message, wordParameter, longParameter);

    /// <summary>Sends a text message to a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="message">The window message.</param>
    /// <param name="wordParameter">The message word parameter.</param>
    /// <param name="text">The message text parameter.</param>
    /// <returns>The message result.</returns>
    public static IntPtr SendMessage(
        IntPtr windowHandle,
        WindowsMessages message,
        IntPtr wordParameter,
        string text) => NativeMethods.SendMessage(windowHandle, message, wordParameter, text);

    /// <summary>Gets the monitor nearest to a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="monitorFrom">The fallback behavior.</param>
    /// <returns>The monitor handle.</returns>
    public static IntPtr MonitorFromWindow(IntPtr windowHandle, MonitorFrom monitorFrom) =>
        NativeMethods.MonitorFromWindow(windowHandle, monitorFrom);

    /// <summary>Gets the monitor nearest to a rectangle.</summary>
    /// <param name="rectangle">The source rectangle.</param>
    /// <param name="monitorFrom">The fallback behavior.</param>
    /// <returns>The monitor handle.</returns>
    public static IntPtr MonitorFromRect(ref NativeRect rectangle, MonitorFrom monitorFrom) =>
        NativeMethods.MonitorFromRect(ref rectangle, monitorFrom);

    /// <summary>Gets a window-information structure.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="windowInfo">The window information.</param>
    /// <returns>True when the information was retrieved.</returns>
    public static bool GetWindowInfo(IntPtr windowHandle, ref WindowInfo windowInfo) =>
        NativeMethods.GetWindowInfo(windowHandle, ref windowInfo);

    /// <summary>Enumerates top-level windows.</summary>
    /// <param name="callback">The enumeration callback.</param>
    /// <param name="parameter">The callback parameter.</param>
    /// <returns>True when enumeration completed.</returns>
    public static bool EnumWindows(EnumWindowsProc callback, IntPtr parameter) =>
        NativeMethods.EnumWindows(callback, parameter);

    /// <summary>Enumerates child windows.</summary>
    /// <param name="parentWindowHandle">The parent window handle.</param>
    /// <param name="callback">The enumeration callback.</param>
    /// <param name="parameter">The callback parameter.</param>
    /// <returns>True when enumeration completed.</returns>
    public static bool EnumChildWindows(
        IntPtr parentWindowHandle,
        EnumWindowsProc callback,
        IntPtr parameter) => NativeMethods.EnumChildWindows(parentWindowHandle, callback, parameter);

    /// <summary>Gets scroll-bar information.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="scrollBar">The scroll-bar type.</param>
    /// <param name="scrollInfo">The scroll information.</param>
    /// <returns>True when the information was retrieved.</returns>
    public static bool GetScrollInfo(
        IntPtr windowHandle,
        ScrollBarTypes scrollBar,
        ref ScrollInfo scrollInfo) => NativeMethods.GetScrollInfo(windowHandle, scrollBar, ref scrollInfo);

    /// <summary>Sets scroll-bar information.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="scrollBar">The scroll-bar type.</param>
    /// <param name="scrollInfo">The scroll information.</param>
    /// <param name="redraw">True to redraw the scroll bar.</param>
    /// <returns>The new scroll-box position.</returns>
    public static int SetScrollInfo(
        IntPtr windowHandle,
        ScrollBarTypes scrollBar,
        ref ScrollInfo scrollInfo,
        bool redraw) => NativeMethods.SetScrollInfo(windowHandle, scrollBar, ref scrollInfo, redraw);

    /// <summary>Shows or hides a scroll bar.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="scrollBar">The scroll-bar type.</param>
    /// <param name="show">True to show the scroll bar.</param>
    /// <returns>True when the visibility was changed.</returns>
    public static bool ShowScrollBar(IntPtr windowHandle, ScrollBarTypes scrollBar, bool show) =>
        NativeMethods.ShowScrollBar(windowHandle, scrollBar, show);

    /// <summary>Gets scroll-bar accessibility information.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="objectId">The accessibility object identifier.</param>
    /// <param name="scrollBarInfo">The scroll-bar information.</param>
    /// <returns>True when the information was retrieved.</returns>
    public static bool GetScrollBarInfo(
        IntPtr windowHandle,
        ObjectIdentifiers objectId,
        ref ScrollBarInfo scrollBarInfo) => NativeMethods.GetScrollBarInfo(windowHandle, objectId, ref scrollBarInfo);

    /// <summary>Sets display-capture affinity for a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="affinity">The display affinity.</param>
    /// <returns>True when the affinity was set.</returns>
    public static bool SetWindowDisplayAffinity(
        IntPtr windowHandle,
        WindowDisplayAffinity affinity) => NativeMethods.SetWindowDisplayAffinity(windowHandle, affinity);

    /// <summary>Gets display-capture affinity for a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="affinity">The display affinity.</param>
    /// <returns>True when the affinity was retrieved.</returns>
    public static bool GetWindowDisplayAffinity(
        IntPtr windowHandle,
        out WindowDisplayAffinity affinity) => NativeMethods.GetWindowDisplayAffinity(windowHandle, out affinity);

    /// <summary>Gets a window region.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="regionHandle">The receiving region handle.</param>
    /// <returns>The region result.</returns>
    public static RegionResults GetWindowRgn(IntPtr windowHandle, SafeHandle regionHandle) =>
        NativeMethods.GetWindowRgn(windowHandle, regionHandle);

    /// <summary>Changes a window position or size.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="insertAfterWindowHandle">The Z-order insertion handle.</param>
    /// <param name="x">The new X coordinate.</param>
    /// <param name="y">The new Y coordinate.</param>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    /// <param name="flags">The positioning flags.</param>
    /// <returns>True when the position was changed.</returns>
    public static bool SetWindowPos(
        IntPtr windowHandle,
        IntPtr insertAfterWindowHandle,
        int x,
        int y,
        int width,
        int height,
        WindowPos flags) =>
        NativeMethods.SetWindowPos(
            windowHandle,
            insertAfterWindowHandle,
            x,
            y,
            width,
            height,
            flags);

    /// <summary>Gets the top window in a Z order.</summary>
    /// <param name="windowHandle">The window handle, or zero for the desktop.</param>
    /// <returns>The top window handle.</returns>
    public static IntPtr GetTopWindow(IntPtr windowHandle) =>
        NativeMethods.GetTopWindow(windowHandle);

    /// <summary>Finds a top-level window.</summary>
    /// <param name="className">The window class name.</param>
    /// <param name="windowName">The window caption.</param>
    /// <returns>The window handle.</returns>
    public static IntPtr FindWindow(string className, string windowName) =>
        NativeMethods.FindWindow(className, windowName);

    /// <summary>Finds a child window.</summary>
    /// <param name="parentWindowHandle">The parent window handle.</param>
    /// <param name="childAfterWindowHandle">The child search starting point.</param>
    /// <param name="className">The window class name.</param>
    /// <param name="windowName">The window caption.</param>
    /// <returns>The child window handle.</returns>
    public static IntPtr FindWindowEx(
        IntPtr parentWindowHandle,
        IntPtr childAfterWindowHandle,
        string className,
        string windowName) =>
        NativeMethods.FindWindowEx(
            parentWindowHandle,
            childAfterWindowHandle,
            className,
            windowName);

    /// <summary>Maps a point between window coordinate systems.</summary>
    /// <param name="fromWindowHandle">The source window handle.</param>
    /// <param name="toWindowHandle">The destination window handle.</param>
    /// <param name="point">The point to map.</param>
    /// <param name="pointCount">The number of points to map.</param>
    /// <returns>The number of pixels added to the horizontal and vertical coordinates.</returns>
    public static int MapWindowPoints(
        IntPtr fromWindowHandle,
        IntPtr toWindowHandle,
        ref NativePoint point,
        int pointCount) => NativeMethods.MapWindowPoints(fromWindowHandle, toWindowHandle, ref point, pointCount);

    /// <summary>Gets a system metric.</summary>
    /// <param name="metric">The metric to retrieve.</param>
    /// <returns>The metric value.</returns>
    public static int GetSystemMetrics(SystemMetric metric) =>
        NativeMethods.GetSystemMetrics(metric);

    /// <summary>Captures mouse input for a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The previous capture window handle.</returns>
    public static IntPtr SetCapture(IntPtr windowHandle) => Volatile.Read(ref _operations).SetCapture(windowHandle);

    /// <summary>Releases mouse input capture.</summary>
    /// <returns>True when the capture was released.</returns>
    public static bool ReleaseCapture() => Volatile.Read(ref _operations).ReleaseCapture();

    /// <summary>Gets or sets a system parameter using a text value.</summary>
    /// <param name="action">The system-parameter action.</param>
    /// <param name="parameterValue">The action parameter value.</param>
    /// <param name="value">The text value.</param>
    /// <param name="behavior">The profile-update behavior.</param>
    /// <returns>True when the operation succeeded.</returns>
    public static bool SystemParametersInfo(
        SystemParametersInfoActions action,
        uint parameterValue,
        string value,
        SystemParametersInfoBehaviors behavior) => NativeMethods.SystemParametersInfo(action, parameterValue, value, behavior);

    /// <summary>Gets or sets a system parameter using a text buffer.</summary>
    /// <param name="action">The system-parameter action.</param>
    /// <param name="parameterValue">The action parameter value.</param>
    /// <param name="value">The text buffer.</param>
    /// <param name="behavior">The profile-update behavior.</param>
    /// <returns>True when the operation succeeded.</returns>
    public static unsafe bool SystemParametersInfo(
        SystemParametersInfoActions action,
        uint parameterValue,
        StringBuilder value,
        SystemParametersInfoBehaviors behavior)
    {
        Throw.IfNull(value);
        char[] buffer = new char[value.Capacity];
        int valueLength = Math.Min(value.Length, buffer.Length);
        value.CopyTo(0, buffer, 0, valueLength);
        ref char bufferReference = ref MemoryMarshal.GetReference(buffer.AsSpan());
        fixed (char* valuePointer = &bufferReference)
        {
            if (!Volatile.Read(ref _operations).SystemParametersInfoBuffer(action, parameterValue, (IntPtr)valuePointer, behavior))
            {
                return false;
            }
        }

        int characterCount = Array.IndexOf(buffer, '\0');
        _ = value.Clear();
        _ = value.Append(buffer, 0, (characterCount < 0) ? buffer.Length : characterCount);
        return true;
    }

    /// <summary>Gets or sets a system parameter using animation information.</summary>
    /// <param name="action">The system-parameter action.</param>
    /// <param name="parameterValue">The action parameter value.</param>
    /// <param name="animationInfo">The animation information.</param>
    /// <param name="behavior">The profile-update behavior.</param>
    /// <returns>True when the operation succeeded.</returns>
    public static bool SystemParametersInfo(
        SystemParametersInfoActions action,
        uint parameterValue,
        ref AnimationInfo animationInfo,
        SystemParametersInfoBehaviors behavior) => NativeMethods.SystemParametersInfo(action, parameterValue, ref animationInfo, behavior);

    /// <summary>Locks the current workstation.</summary>
    /// <returns>True when the workstation was locked.</returns>
    public static bool LockWorkStation() => Volatile.Read(ref _operations).LockWorkStation();

    /// <summary>Gets information about the current cursor.</summary>
    /// <param name="cursorInfo">The cursor information.</param>
    /// <returns>True when the information was retrieved.</returns>
    public static bool GetCursorInfo(ref CursorInfo cursorInfo) =>
        NativeMethods.GetCursorInfo(ref cursorInfo);

    /// <summary>Destroys a cursor.</summary>
    /// <param name="cursorHandle">The cursor handle.</param>
    /// <returns>True when the cursor was destroyed.</returns>
    public static bool DestroyCursor(IntPtr cursorHandle) =>
        NativeMethods.DestroyCursor(cursorHandle);

    /// <summary>Fills a rectangle with a brush.</summary>
    /// <param name="deviceContextHandle">The target device-context handle.</param>
    /// <param name="rectangle">The rectangle to fill.</param>
    /// <param name="brushHandle">The brush handle.</param>
    /// <returns>The native result value.</returns>
    public static int FillRect(
        IntPtr deviceContextHandle,
        ref NativeRect rectangle,
        IntPtr brushHandle) => NativeMethods.FillRect(deviceContextHandle, ref rectangle, brushHandle);

    /// <summary>Adds an enumerated display monitor to the caller-owned result list.</summary>
    /// <param name="monitorHandle">The monitor handle.</param>
    /// <param name="deviceContextHandle">The monitor device-context handle.</param>
    /// <param name="monitorRectangle">The monitor bounds.</param>
    /// <param name="state">The GC handle for the result list.</param>
    /// <returns>One to continue enumeration; otherwise, zero.</returns>
    private static unsafe int AddDisplayMonitor(
        IntPtr monitorHandle,
        IntPtr deviceContextHandle,
        NativeRect* monitorRectangle,
        IntPtr state)
    {
        _ = deviceContextHandle;
        _ = monitorRectangle;
        if (!(GCHandle.FromIntPtr(state).Target is List<DisplayInfo> displays))
        {
            return 0;
        }

        DisplayInfo displayInfo = GetDisplayInfo(
            monitorHandle,
            checked(displays.Count + InitialDisplayIndex));
        if (displayInfo is not null)
        {
            displays.Add(displayInfo);
        }

        return 1;
    }
}
