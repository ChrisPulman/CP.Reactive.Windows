// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Ipc;

/// <summary>Describes the outcome of a Citrix virtual-channel IPC operation.</summary>
public enum CitrixVirtualChannelStatus
{
    /// <summary>The operation completed successfully.</summary>
    Success,

    /// <summary>The operation was accepted and will complete asynchronously through adapter events.</summary>
    Pending,

    /// <summary>The virtual-driver host does not support the operation.</summary>
    NotSupported,

    /// <summary>The operation failed because an input was invalid.</summary>
    InvalidParameter,

    /// <summary>The operation failed because the channel is closed.</summary>
    Closed,

    /// <summary>The operation failed.</summary>
    Failed,
}
