// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Lifecycle;

/// <summary>Represents a Citrix session state change event.</summary>
public sealed class CitrixSessionStateChangeEvent : CitrixSessionLifecycleEvent
{
    /// <summary>Initializes a new instance of the <see cref="CitrixSessionStateChangeEvent" /> class.</summary>
    /// <param name="session">The Citrix session payload.</param>
    /// <param name="previousState">The previous connection state, when known.</param>
    /// <param name="currentState">The current connection state, when known.</param>
    /// <param name="timestamp">The event timestamp.</param>
    public CitrixSessionStateChangeEvent(
        CitrixSessionInfo session,
        ConnectStates? previousState,
        ConnectStates? currentState,
        DateTimeOffset timestamp)
        : base(session, timestamp, EventMask.StateChange)
    {
        PreviousState = previousState;
        CurrentState = currentState;
    }

    /// <summary>Gets the previous connection state, when known.</summary>
    public ConnectStates? PreviousState { get; }

    /// <summary>Gets the current connection state, when known.</summary>
    public ConnectStates? CurrentState { get; }
}
