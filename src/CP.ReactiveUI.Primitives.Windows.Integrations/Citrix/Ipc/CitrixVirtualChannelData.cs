// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Ipc;

/// <summary>Describes data received from a Citrix virtual channel.</summary>
public sealed class CitrixVirtualChannelData
{
    /// <summary>The immutable payload snapshot.</summary>
    private readonly byte[] _payload;

    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelData"/> class.</summary>
    /// <param name="channel">The source channel.</param>
    /// <param name="payload">The received payload.</param>
    public CitrixVirtualChannelData(CitrixVirtualChannelHandle channel, byte[] payload)
    {
        Throw.IfNull(payload);
        Channel = channel;
        _payload = Copy(payload);
    }

    /// <summary>Gets the source channel.</summary>
    public CitrixVirtualChannelHandle Channel { get; }

    /// <summary>Gets the payload byte count.</summary>
    public int PayloadLength => _payload.Length;

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
