// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>Provides the session-listener native boundary.</summary>
#if NETFRAMEWORK
internal static class WindowsSessionListenerNativeMethods
#else
internal static partial class WindowsSessionListenerNativeMethods
#endif
{
    /// <summary>The registration operation.</summary>
    private static Func<IntPtr, int, bool> _registerOperation = NativeMethods.WtsRegisterSessionNotification;

    /// <summary>The unregistration operation.</summary>
    private static Func<IntPtr, bool> _unregisterOperation = NativeMethods.WtsUnRegisterSessionNotification;

    /// <summary>Registers the specified window to receive session change notifications.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="flags">The session notification flags.</param>
    /// <returns><c>true</c> when registration succeeds; otherwise <c>false</c>.</returns>
    internal static bool WtsRegisterSessionNotification(IntPtr windowHandle, int flags) => _registerOperation(windowHandle, flags);

    /// <summary>Unregisters the specified window from session change notifications.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns><c>true</c> when unregistration succeeds; otherwise <c>false</c>.</returns>
    internal static bool WtsUnRegisterSessionNotification(IntPtr windowHandle) => _unregisterOperation(windowHandle);

    /// <summary>Overrides native session operations for deterministic tests.</summary>
    /// <param name="register">The replacement registration operation.</param>
    /// <param name="unregister">The replacement unregistration operation.</param>
    /// <returns>A scope that restores the production operations.</returns>
    internal static IDisposable OverrideForTesting(Func<IntPtr, int, bool> register, Func<IntPtr, bool> unregister)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(register);
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(unregister);
        (Func<IntPtr, int, bool> Register, Func<IntPtr, bool> Unregister) previous = (_registerOperation, _unregisterOperation);
        (_registerOperation, _unregisterOperation) = (register, unregister);
        return Scope.Create(previous, static previousOperations =>
        {
            (_registerOperation, _unregisterOperation) = previousOperations;
        });
    }

    /// <summary>Contains native session-notification entry points.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>The WTS API library name.</summary>
        private const string WtsApi32Dll = "wtsapi32.dll";

        /// <summary>Registers the specified window to receive session change notifications.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="flags">The session notification flags.</param>
        /// <returns><c>true</c> when registration succeeds; otherwise <c>false</c>.</returns>
#if NETFRAMEWORK
        [DllImport(WtsApi32Dll, EntryPoint = "WTSRegisterSessionNotification", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WtsRegisterSessionNotification(IntPtr windowHandle, int flags);
#else
        [LibraryImport(WtsApi32Dll, EntryPoint = "WTSRegisterSessionNotification", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool WtsRegisterSessionNotification(IntPtr windowHandle, int flags);
#endif

        /// <summary>Unregisters the specified window from session change notifications.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <returns><c>true</c> when unregistration succeeds; otherwise <c>false</c>.</returns>
#if NETFRAMEWORK
        [DllImport(WtsApi32Dll, EntryPoint = "WTSUnRegisterSessionNotification", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WtsUnRegisterSessionNotification(IntPtr windowHandle);
#else
        [LibraryImport(WtsApi32Dll, EntryPoint = "WTSUnRegisterSessionNotification", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool WtsUnRegisterSessionNotification(IntPtr windowHandle);
#endif
    }
}
