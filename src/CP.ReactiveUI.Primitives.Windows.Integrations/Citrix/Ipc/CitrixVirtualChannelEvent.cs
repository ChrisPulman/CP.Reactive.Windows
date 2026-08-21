// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Ipc;

/// <summary>Describes a Citrix virtual-channel lifecycle or diagnostic event.</summary>
public sealed class CitrixVirtualChannelEvent
{
    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelEvent"/> class.</summary>
    /// <param name="kind">The event kind.</param>
    /// <param name="channel">The related channel.</param>
    /// <param name="status">The event status.</param>
    public CitrixVirtualChannelEvent(CitrixVirtualChannelEventKind kind, CitrixVirtualChannelHandle channel, CitrixVirtualChannelStatus status)
        : this(kind, channel, status, 0, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelEvent"/> class.</summary>
    /// <param name="kind">The event kind.</param>
    /// <param name="channel">The related channel.</param>
    /// <param name="status">The event status.</param>
    /// <param name="nativeStatus">The optional host status code.</param>
    /// <param name="message">The optional host status message.</param>
    public CitrixVirtualChannelEvent(
        CitrixVirtualChannelEventKind kind,
        CitrixVirtualChannelHandle channel,
        CitrixVirtualChannelStatus status,
        int nativeStatus,
        string message)
    {
        Kind = kind;
        Channel = channel;
        Status = status;
        NativeStatus = nativeStatus;
        Message = message;
    }

    /// <summary>Gets the event kind.</summary>
    public CitrixVirtualChannelEventKind Kind { get; }

    /// <summary>Gets the related channel.</summary>
    public CitrixVirtualChannelHandle Channel { get; }

    /// <summary>Gets the event status.</summary>
    public CitrixVirtualChannelStatus Status { get; }

    /// <summary>Gets the optional host status code.</summary>
    public int NativeStatus { get; }

    /// <summary>Gets the optional host status message.</summary>
    public string Message { get; }
}
