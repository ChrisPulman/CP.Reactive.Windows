// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Lifecycle;

/// <summary>Adapts an existing observable sequence into a Citrix lifecycle event source.</summary>
public sealed class CitrixObservableLifecycleEventSource : ICitrixLifecycleEventSource
{
    /// <summary>The lifecycle event stream.</summary>
    private readonly IObservable<CitrixLifecycleEvent> _events;

    /// <summary>Initializes a new instance of the <see cref="CitrixObservableLifecycleEventSource" /> class.</summary>
    /// <param name="events">The lifecycle event stream.</param>
    public CitrixObservableLifecycleEventSource(IObservable<CitrixLifecycleEvent> events)
    {
        Throw.IfNull(events);
        _events = events;
    }

    /// <inheritdoc />
    public IObservable<CitrixLifecycleEvent> ObserveLifecycleEvents() => _events;
}
