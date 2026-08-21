// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Ipc;

/// <summary>Identifies a Citrix virtual-channel event kind.</summary>
public enum CitrixVirtualChannelEventKind
{
    /// <summary>A channel was opened.</summary>
    Opened,

    /// <summary>A channel was closed.</summary>
    Closed,

    /// <summary>A write operation completed or changed state.</summary>
    WriteCompleted,

    /// <summary>A feature was registered.</summary>
    FeatureRegistered,

    /// <summary>Incoming data was received.</summary>
    DataReceived,

    /// <summary>The adapter reported a state transition.</summary>
    StateChanged,

    /// <summary>The adapter reported an error.</summary>
    AdapterError,
}
