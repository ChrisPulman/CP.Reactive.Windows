// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Host;

/// <summary>Abstracts Citrix CCM host-session calls for composition and tests.</summary>
public interface ICitrixCcmHostSessionApi
{
    /// <summary>Retrieves information for a running CCM ICA session.</summary>
    /// <param name="sessionId">The CCM session identifier.</param>
    /// <returns>The session information result.</returns>
    CcmHostSessionInformationResult GetSessionInfo(
        int sessionId);

    /// <summary>Disconnects a running CCM ICA session.</summary>
    /// <param name="sessionId">The CCM session identifier.</param>
    /// <returns>The operation result.</returns>
    CcmHostOperationResult DisconnectSession(
        int sessionId);

    /// <summary>Logs off a running CCM ICA session.</summary>
    /// <param name="sessionId">The CCM session identifier.</param>
    /// <returns>The operation result.</returns>
    CcmHostOperationResult LogoffSession(
        int sessionId);
}
