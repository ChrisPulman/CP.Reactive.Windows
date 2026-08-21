// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Ipc;

/// <summary>Describes a DriverWrite result.</summary>
public sealed class CitrixVirtualChannelWriteResult
{
    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelWriteResult"/> class.</summary>
    /// <param name="channel">The target channel.</param>
    /// <param name="status">The operation status.</param>
    /// <param name="bytesWritten">The number of bytes accepted by the host.</param>
    public CitrixVirtualChannelWriteResult(CitrixVirtualChannelHandle channel, CitrixVirtualChannelStatus status, int bytesWritten)
        : this(channel, status, bytesWritten, 0, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelWriteResult"/> class.</summary>
    /// <param name="channel">The target channel.</param>
    /// <param name="status">The operation status.</param>
    /// <param name="bytesWritten">The number of bytes accepted by the host.</param>
    /// <param name="nativeStatus">The optional host status code.</param>
    /// <param name="message">The optional host status message.</param>
    public CitrixVirtualChannelWriteResult(
        CitrixVirtualChannelHandle channel,
        CitrixVirtualChannelStatus status,
        int bytesWritten,
        int nativeStatus,
        string message)
    {
        Channel = channel;
        Status = status;
        BytesWritten = bytesWritten;
        NativeStatus = nativeStatus;
        Message = message;
    }

    /// <summary>Gets the target channel.</summary>
    public CitrixVirtualChannelHandle Channel { get; }

    /// <summary>Gets the operation status.</summary>
    public CitrixVirtualChannelStatus Status { get; }

    /// <summary>Gets the number of bytes accepted by the host.</summary>
    public int BytesWritten { get; }

    /// <summary>Gets the optional host status code.</summary>
    public int NativeStatus { get; }

    /// <summary>Gets the optional host status message.</summary>
    public string Message { get; }
}
