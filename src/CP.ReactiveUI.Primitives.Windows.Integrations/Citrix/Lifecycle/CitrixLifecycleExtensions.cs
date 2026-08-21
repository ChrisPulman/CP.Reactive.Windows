// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Lifecycle;

/// <summary>Provides source-specific Citrix lifecycle observable filters.</summary>
public static class CitrixLifecycleExtensions
{
    /// <summary>Provides lifecycle filters for explicitly supplied Citrix event sources.</summary>
    /// <param name="source">The lifecycle event source.</param>
    extension(ICitrixLifecycleEventSource source)
    {
        /// <summary>Observes Citrix client connect events.</summary>
        /// <returns>The connect event stream.</returns>
        public IObservable<CitrixConnectEvent> OnConnect() => Filter<CitrixConnectEvent>(source);

        /// <summary>Observes Citrix client disconnect events.</summary>
        /// <returns>The disconnect event stream.</returns>
        public IObservable<CitrixDisconnectEvent> OnDisconnect() => Filter<CitrixDisconnectEvent>(source);

        /// <summary>Observes Citrix client login events.</summary>
        /// <returns>The login event stream.</returns>
        public IObservable<CitrixLoginEvent> OnLogin() => Filter<CitrixLoginEvent>(source);

        /// <summary>Observes Citrix client window creation events.</summary>
        /// <returns>The window creation event stream.</returns>
        public IObservable<CitrixWindowCreatedEvent> OnWindowCreated() => Filter<CitrixWindowCreatedEvent>(source);

        /// <summary>Observes Citrix client window destruction events.</summary>
        /// <returns>The window destruction event stream.</returns>
        public IObservable<CitrixWindowDestroyedEvent> OnWindowDestroyed() => Filter<CitrixWindowDestroyedEvent>(source);

        /// <summary>Compatibility alias for OnWindowDestroyed.</summary>
        /// <returns>The window destruction event stream.</returns>
        public IObservable<CitrixWindowDestroyedEvent> OnWindowDistroyed() => Filter<CitrixWindowDestroyedEvent>(source);

        /// <summary>Observes Citrix ICA file parse events.</summary>
        /// <returns>The ICA file parse event stream.</returns>
        public IObservable<CitrixICAFileParseEvent> OnICAFileParse() => Filter<CitrixICAFileParseEvent>(source);

        /// <summary>Observes Citrix session state change events.</summary>
        /// <returns>The session state change event stream.</returns>
        public IObservable<CitrixSessionStateChangeEvent> OnSessionStateChange() => Filter<CitrixSessionStateChangeEvent>(source);
    }

    /// <summary>Creates a typed event filter over a lifecycle source.</summary>
    /// <typeparam name="TEvent">The lifecycle event type.</typeparam>
    /// <param name="source">The lifecycle event source.</param>
    /// <returns>The typed event stream.</returns>
    private static LifecycleEventFilter<TEvent> Filter<TEvent>(ICitrixLifecycleEventSource source)
        where TEvent : CitrixLifecycleEvent
    {
        Throw.IfNull(source);
        return new(source);
    }

    /// <summary>Filters lifecycle events by type.</summary>
    /// <typeparam name="TEvent">The lifecycle event type.</typeparam>
    /// <param name="source">The lifecycle event source.</param>
    private sealed class LifecycleEventFilter<TEvent>(ICitrixLifecycleEventSource source) : IObservable<TEvent>
        where TEvent : CitrixLifecycleEvent
    {
        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<TEvent> observer)
        {
            Throw.IfNull(observer);
            var events = source.ObserveLifecycleEvents();
            Throw.IfNull(events);
            return events.Subscribe(new LifecycleEventFilterObserver<TEvent>(observer));
        }
    }

    /// <summary>Forwards matching lifecycle events.</summary>
    /// <typeparam name="TEvent">The lifecycle event type.</typeparam>
    /// <param name="observer">The observer that receives matching lifecycle events.</param>
    private sealed class LifecycleEventFilterObserver<TEvent>(IObserver<TEvent> observer) : IObserver<CitrixLifecycleEvent>
        where TEvent : CitrixLifecycleEvent
    {
        /// <inheritdoc />
        public void OnCompleted() => observer.OnCompleted();

        /// <inheritdoc />
        public void OnError(Exception error) => observer.OnError(error);

        /// <inheritdoc />
        public void OnNext(CitrixLifecycleEvent value)
        {
            if (value is TEvent typedValue)
            {
                observer.OnNext(typedValue);
            }
        }
    }
}
