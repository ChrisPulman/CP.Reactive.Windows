// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Lifecycle;

/// <summary>Describes the Citrix session associated with a lifecycle event.</summary>
public sealed class CitrixSessionInfo
{
    /// <summary>Initializes a new instance of the <see cref="CitrixSessionInfo" /> class.</summary>
    /// <param name="sessionId">The Citrix session identifier, or -1 for the current session.</param>
    /// <param name="connectState">The current Citrix connection state, when known.</param>
    /// <param name="userName">The Citrix session user name, when known.</param>
    /// <param name="domainName">The Citrix session domain name, when known.</param>
    /// <param name="clientName">The Citrix client name, when known.</param>
    /// <param name="clientAddress">The Citrix client address, when known.</param>
    public CitrixSessionInfo(
        int sessionId,
        ConnectStates? connectState,
        string userName,
        string domainName,
        string clientName,
        string clientAddress)
    {
        SessionId = sessionId;
        ConnectState = connectState;
        UserName = userName;
        DomainName = domainName;
        ClientName = clientName;
        ClientAddress = clientAddress;
    }

    /// <summary>Gets an empty current-session payload.</summary>
    public static CitrixSessionInfo Empty { get; } = new(-1, null, null, null, null, null);

    /// <summary>Gets the Citrix session identifier, or -1 for the current session.</summary>
    public int SessionId { get; }

    /// <summary>Gets the current Citrix connection state, when known.</summary>
    public ConnectStates? ConnectState { get; }

    /// <summary>Gets the Citrix session user name, when known.</summary>
    public string UserName { get; }

    /// <summary>Gets the Citrix session domain name, when known.</summary>
    public string DomainName { get; }

    /// <summary>Gets the Citrix client name, when known.</summary>
    public string ClientName { get; }

    /// <summary>Gets the Citrix client address, when known.</summary>
    public string ClientAddress { get; }
}
