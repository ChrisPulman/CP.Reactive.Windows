// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Lifecycle;

/// <summary>Represents a Citrix client connect event.</summary>
public sealed class CitrixConnectEvent : CitrixSessionLifecycleEvent
{
    /// <summary>Initializes a new instance of the <see cref="CitrixConnectEvent" /> class.</summary>
    /// <param name="session">The Citrix session payload.</param>
    /// <param name="timestamp">The event timestamp.</param>
    public CitrixConnectEvent(CitrixSessionInfo session, DateTimeOffset timestamp)
        : base(session, timestamp, EventMask.Connect)
    {
    }
}
