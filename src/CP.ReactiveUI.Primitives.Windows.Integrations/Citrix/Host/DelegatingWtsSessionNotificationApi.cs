// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Host;

/// <summary>Delegate-backed WTS session-notification adapter.</summary>
public sealed class DelegatingWtsSessionNotificationApi : IWtsSessionNotificationApi
{
    /// <summary>The WTSRegisterSessionNotification delegate.</summary>
    private readonly Func<IntPtr, WtsSessionNotificationScope, bool> _registerSessionNotification;

    /// <summary>The WTSUnRegisterSessionNotification delegate.</summary>
    private readonly Func<IntPtr, bool> _unregisterSessionNotification;

    /// <summary>The last-error provider.</summary>
    private readonly Func<int> _getLastError;

    /// <summary>Initializes a new instance of the <see cref="DelegatingWtsSessionNotificationApi"/> class.</summary>
    /// <param name="registerSessionNotification">The WTSRegisterSessionNotification delegate.</param>
    /// <param name="unregisterSessionNotification">The WTSUnRegisterSessionNotification delegate.</param>
    /// <param name="getLastError">The last-error provider.</param>
    public DelegatingWtsSessionNotificationApi(
        Func<IntPtr, WtsSessionNotificationScope, bool> registerSessionNotification,
        Func<IntPtr, bool> unregisterSessionNotification,
        Func<int> getLastError)
    {
        Throw.IfNull(registerSessionNotification);
        Throw.IfNull(unregisterSessionNotification);
        Throw.IfNull(getLastError);
        _registerSessionNotification = registerSessionNotification;
        _unregisterSessionNotification = unregisterSessionNotification;
        _getLastError = getLastError;
    }

    /// <inheritdoc />
    public bool WTSRegisterSessionNotification(
        IntPtr windowHandle,
        WtsSessionNotificationScope scope) =>
        _registerSessionNotification(
            windowHandle,
            scope);

    /// <inheritdoc />
    public bool WTSUnRegisterSessionNotification(
        IntPtr windowHandle) =>
        _unregisterSessionNotification(windowHandle);

    /// <inheritdoc />
    public int GetLastError() => _getLastError();
}
