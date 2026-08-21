// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Ipc;

/// <summary>Describes a DriverOpen result.</summary>
public sealed class CitrixVirtualChannelOpenResult
{
    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelOpenResult"/> class.</summary>
    /// <param name="channel">The opened channel.</param>
    /// <param name="status">The operation status.</param>
    public CitrixVirtualChannelOpenResult(CitrixVirtualChannelHandle channel, CitrixVirtualChannelStatus status)
        : this(channel, status, 0, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelOpenResult"/> class.</summary>
    /// <param name="channel">The opened channel.</param>
    /// <param name="status">The operation status.</param>
    /// <param name="nativeStatus">The optional host status code.</param>
    /// <param name="message">The optional host status message.</param>
    public CitrixVirtualChannelOpenResult(CitrixVirtualChannelHandle channel, CitrixVirtualChannelStatus status, int nativeStatus, string message)
    {
        Channel = channel;
        Status = status;
        NativeStatus = nativeStatus;
        Message = message;
    }

    /// <summary>Gets the opened channel.</summary>
    public CitrixVirtualChannelHandle Channel { get; }

    /// <summary>Gets the operation status.</summary>
    public CitrixVirtualChannelStatus Status { get; }

    /// <summary>Gets the optional host status code.</summary>
    public int NativeStatus { get; }

    /// <summary>Gets the optional host status message.</summary>
    public string Message { get; }
}
