// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Ipc;

/// <summary>Describes a DriverOpen request.</summary>
public sealed class CitrixVirtualChannelOpenRequest
{
    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelOpenRequest"/> class.</summary>
    /// <param name="channelName">The virtual-channel name.</param>
    public CitrixVirtualChannelOpenRequest(string channelName)
        : this(channelName, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelOpenRequest"/> class.</summary>
    /// <param name="channelName">The virtual-channel name.</param>
    /// <param name="context">Optional host-specific context passed to the adapter.</param>
    public CitrixVirtualChannelOpenRequest(string channelName, object context)
    {
        Throw.IfNull(channelName);
        ChannelName = channelName;
        Context = context;
    }

    /// <summary>Gets the virtual-channel name.</summary>
    public string ChannelName { get; }

    /// <summary>Gets optional host-specific context passed to the adapter.</summary>
    public object Context { get; }
}
