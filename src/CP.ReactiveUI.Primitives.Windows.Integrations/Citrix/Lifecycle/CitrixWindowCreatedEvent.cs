// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Lifecycle;

/// <summary>Represents a Citrix client window creation event.</summary>
public sealed class CitrixWindowCreatedEvent : CitrixLifecycleEvent
{
    /// <summary>Initializes a new instance of the <see cref="CitrixWindowCreatedEvent" /> class.</summary>
    /// <param name="window">The Citrix window payload.</param>
    /// <param name="timestamp">The event timestamp.</param>
    public CitrixWindowCreatedEvent(CitrixWindowInfo window, DateTimeOffset timestamp)
        : base(timestamp, EventMask.None)
    {
        Window = window ?? CitrixWindowInfo.Empty;
    }

    /// <summary>Gets the Citrix window payload.</summary>
    public CitrixWindowInfo Window { get; }
}
