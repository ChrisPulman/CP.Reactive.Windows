// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Host;

/// <summary>Native WTS session-notification adapter.</summary>
internal sealed class NativeWtsSessionNotificationApi : IWtsSessionNotificationApi
{
    /// <summary>The shared native WTS adapter instance.</summary>
    internal static readonly IWtsSessionNotificationApi Instance = new NativeWtsSessionNotificationApi();

    /// <summary>Initializes a new instance of the <see cref="NativeWtsSessionNotificationApi"/> class.</summary>
    private NativeWtsSessionNotificationApi()
    {
    }

    /// <inheritdoc />
    public bool WTSRegisterSessionNotification(
        IntPtr windowHandle,
        WtsSessionNotificationScope scope) =>
        NativeMethods.WTSRegisterSessionNotification(
            windowHandle,
            (int)scope);

    /// <inheritdoc />
    public bool WTSUnRegisterSessionNotification(
        IntPtr windowHandle) =>
        NativeMethods.WTSUnRegisterSessionNotification(windowHandle);

    /// <inheritdoc />
    public int GetLastError() => Marshal.GetLastWin32Error();
}
