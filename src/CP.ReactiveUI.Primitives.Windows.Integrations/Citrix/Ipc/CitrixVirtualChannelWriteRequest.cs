// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Ipc;

/// <summary>Describes a DriverWrite request.</summary>
public sealed class CitrixVirtualChannelWriteRequest
{
    /// <summary>The immutable payload snapshot.</summary>
    private readonly byte[] _payload;

    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelWriteRequest"/> class.</summary>
    /// <param name="channel">The target channel.</param>
    /// <param name="payload">The payload to write.</param>
    public CitrixVirtualChannelWriteRequest(CitrixVirtualChannelHandle channel, byte[] payload)
        : this(channel, payload, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelWriteRequest"/> class.</summary>
    /// <param name="channel">The target channel.</param>
    /// <param name="payload">The payload to write.</param>
    /// <param name="context">Optional host-specific context passed to the adapter.</param>
    public CitrixVirtualChannelWriteRequest(CitrixVirtualChannelHandle channel, byte[] payload, object context)
    {
        Throw.IfNull(payload);
        Channel = channel;
        _payload = Copy(payload);
        Context = context;
    }

    /// <summary>Gets the target channel.</summary>
    public CitrixVirtualChannelHandle Channel { get; }

    /// <summary>Gets the payload byte count.</summary>
    public int PayloadLength => _payload.Length;

    /// <summary>Gets optional host-specific context passed to the adapter.</summary>
    public object Context { get; }

    /// <summary>Copies the immutable payload snapshot.</summary>
    /// <returns>A copy of the payload.</returns>
    public byte[] CopyPayload() => Copy(_payload);

    /// <summary>Copies a byte array.</summary>
    /// <param name="source">The source bytes.</param>
    /// <returns>The copied bytes.</returns>
    private static byte[] Copy(byte[] source)
    {
        var copy = new byte[source.Length];
        Array.Copy(source, copy, source.Length);
        return copy;
    }
}
