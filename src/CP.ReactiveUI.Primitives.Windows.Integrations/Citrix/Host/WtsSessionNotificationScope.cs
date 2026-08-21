// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Host;

/// <summary>Defines the scope used with WTSRegisterSessionNotification.</summary>
public enum WtsSessionNotificationScope
{
    /// <summary>Receive session notifications for the session attached to the registered window.</summary>
    ThisSession = 0,

    /// <summary>Receive session notifications for all sessions.</summary>
    AllSessions = 1,
}
