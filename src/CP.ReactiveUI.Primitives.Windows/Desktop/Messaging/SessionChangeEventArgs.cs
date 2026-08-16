// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>Provides event arguments for session change events.</summary>
public class SessionChangeEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.SessionChangeEventArgs" /> class.</summary>
    /// <param name="eventType">The type of session change.</param>
    /// <param name="sessionId">The session ID.</param>
    public SessionChangeEventArgs(WtsSessionChangeEvents eventType, int sessionId)
    {
        EventType = eventType;
        SessionId = sessionId;
    }

    /// <summary>Gets the type of session change that occurred.</summary>
    public WtsSessionChangeEvents EventType { get; }

    /// <summary>Gets the session ID that was affected.</summary>
    public int SessionId { get; }
}
