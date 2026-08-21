// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Host;

/// <summary>Delegate-backed Citrix CCM host-session adapter.</summary>
public sealed class DelegatingCitrixCcmHostSessionApi : ICitrixCcmHostSessionApi
{
    /// <summary>The CCMGetSessionInfo delegate.</summary>
    private readonly Func<int, CcmHostSessionInformationResult> _getSessionInfo;

    /// <summary>The CCMDisconnectSession delegate.</summary>
    private readonly Func<int, CcmHostOperationResult> _disconnectSession;

    /// <summary>The CCMLogoffSession delegate.</summary>
    private readonly Func<int, CcmHostOperationResult> _logoffSession;

    /// <summary>Initializes a new instance of the <see cref="DelegatingCitrixCcmHostSessionApi"/> class.</summary>
    /// <param name="getSessionInfo">The CCMGetSessionInfo delegate.</param>
    /// <param name="disconnectSession">The CCMDisconnectSession delegate.</param>
    /// <param name="logoffSession">The CCMLogoffSession delegate.</param>
    public DelegatingCitrixCcmHostSessionApi(
        Func<int, CcmHostSessionInformationResult> getSessionInfo,
        Func<int, CcmHostOperationResult> disconnectSession,
        Func<int, CcmHostOperationResult> logoffSession)
    {
        Throw.IfNull(getSessionInfo);
        Throw.IfNull(disconnectSession);
        Throw.IfNull(logoffSession);
        _getSessionInfo = getSessionInfo;
        _disconnectSession = disconnectSession;
        _logoffSession = logoffSession;
    }

    /// <inheritdoc />
    public CcmHostSessionInformationResult GetSessionInfo(
        int sessionId) =>
        _getSessionInfo(sessionId);

    /// <inheritdoc />
    public CcmHostOperationResult DisconnectSession(
        int sessionId) =>
        _disconnectSession(sessionId);

    /// <inheritdoc />
    public CcmHostOperationResult LogoffSession(
        int sessionId) =>
        _logoffSession(sessionId);
}
