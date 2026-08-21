// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Lifecycle;

/// <summary>Base class for Citrix session lifecycle events.</summary>
public class CitrixSessionLifecycleEvent : CitrixLifecycleEvent
{
    /// <summary>Initializes a new instance of the <see cref="CitrixSessionLifecycleEvent" /> class.</summary>
    /// <param name="session">The Citrix session payload.</param>
    /// <param name="timestamp">The event timestamp.</param>
    /// <param name="eventMask">The matching WFAPI event mask.</param>
    protected CitrixSessionLifecycleEvent(CitrixSessionInfo session, DateTimeOffset timestamp, EventMask eventMask)
        : base(timestamp, eventMask)
    {
        Session = session ?? CitrixSessionInfo.Empty;
    }

    /// <summary>Gets the Citrix session payload.</summary>
    public CitrixSessionInfo Session { get; }
}
