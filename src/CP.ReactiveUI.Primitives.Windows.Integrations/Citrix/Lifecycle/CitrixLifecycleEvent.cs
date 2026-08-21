// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Lifecycle;

/// <summary>Base class for immutable Citrix lifecycle events.</summary>
public class CitrixLifecycleEvent
{
    /// <summary>Initializes a new instance of the <see cref="CitrixLifecycleEvent" /> class.</summary>
    /// <param name="timestamp">The event timestamp.</param>
    /// <param name="eventMask">The matching WFAPI event mask when the event comes from WFAPI; otherwise <see cref="EventMask.None" />.</param>
    protected CitrixLifecycleEvent(DateTimeOffset timestamp, EventMask eventMask)
    {
        Timestamp = timestamp;
        EventMask = eventMask;
    }

    /// <summary>Gets the event timestamp.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Gets the matching WFAPI event mask when the event comes from WFAPI; otherwise <see cref="EventMask.None" />.</summary>
    public EventMask EventMask { get; }
}
