// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Host;

/// <summary>Abstracts WTS session-notification registration calls for composition and tests.</summary>
public interface IWtsSessionNotificationApi
{
    /// <summary>Registers a window for WTS session notifications.</summary>
    /// <param name="windowHandle">The window handle to register.</param>
    /// <param name="scope">The notification scope.</param>
    /// <returns><see langword="true"/> when registration succeeds.</returns>
    bool WTSRegisterSessionNotification(
        IntPtr windowHandle,
        WtsSessionNotificationScope scope);

    /// <summary>Unregisters a window from WTS session notifications.</summary>
    /// <param name="windowHandle">The registered window handle.</param>
    /// <returns><see langword="true"/> when unregistration succeeds.</returns>
    bool WTSUnRegisterSessionNotification(
        IntPtr windowHandle);

    /// <summary>Gets the last Win32 error for the current thread.</summary>
    /// <returns>The last Win32 error.</returns>
    int GetLastError();
}
