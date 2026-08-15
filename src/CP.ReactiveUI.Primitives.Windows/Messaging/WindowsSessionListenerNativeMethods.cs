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
internal static class WindowsSessionListenerNativeMethods
{
    /// <summary>Contains native session-notification entry points.</summary>
    private static class NativeMethods
    {
        /// <summary>Registers the specified window to receive session change notifications.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="flags">The session notification flags.</param>
        /// <returns><c>true</c> when registration succeeds; otherwise <c>false</c>.</returns>
        [DllImport("wtsapi32.dll", EntryPoint = "WTSRegisterSessionNotification", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WtsRegisterSessionNotification(IntPtr windowHandle, int flags);

        /// <summary>Unregisters the specified window from session change notifications.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <returns><c>true</c> when unregistration succeeds; otherwise <c>false</c>.</returns>
        [DllImport("wtsapi32.dll", EntryPoint = "WTSUnRegisterSessionNotification", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WtsUnRegisterSessionNotification(IntPtr windowHandle);
    }

    /// <summary>Registers the specified window to receive session change notifications.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="flags">The session notification flags.</param>
    /// <returns><c>true</c> when registration succeeds; otherwise <c>false</c>.</returns>
    internal static bool WtsRegisterSessionNotification(IntPtr windowHandle, int flags) => NativeMethods.WtsRegisterSessionNotification(windowHandle, flags);

    /// <summary>Unregisters the specified window from session change notifications.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns><c>true</c> when unregistration succeeds; otherwise <c>false</c>.</returns>
    internal static bool WtsUnRegisterSessionNotification(IntPtr windowHandle) => NativeMethods.WtsUnRegisterSessionNotification(windowHandle);
}
