// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Ipc;

/// <summary>Represents an explicitly disposable Citrix virtual-channel lifetime.</summary>
public sealed class CitrixVirtualChannelSession : IDisposable
{
    /// <summary>The adapter that owns the channel callbacks.</summary>
    private readonly ICitrixVirtualDriverAdapter _adapter;

    /// <summary>Serializes close attempts.</summary>
    private readonly Lock _closeGate = new();

    /// <summary>Tracks whether this lifetime has already been closed.</summary>
    private int _closed;

    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelSession"/> class.</summary>
    /// <param name="adapter">The adapter that owns the channel callbacks.</param>
    /// <param name="openResult">The DriverOpen result.</param>
    public CitrixVirtualChannelSession(
        ICitrixVirtualDriverAdapter adapter,
        CitrixVirtualChannelOpenResult openResult)
    {
        Throw.IfNull(adapter);
        Throw.IfNull(openResult);
        _adapter = adapter;
        OpenResult = openResult;
        Channel = openResult.Channel;
        IncomingData = CitrixVirtualChannelIpc.ObserveIncomingData(adapter, Channel);
        Events = CitrixVirtualChannelIpc.ObserveEvents(adapter, Channel);
    }

    /// <summary>Gets the DriverOpen result that created this lifetime.</summary>
    public CitrixVirtualChannelOpenResult OpenResult { get; }

    /// <summary>Gets the channel associated with this lifetime.</summary>
    public CitrixVirtualChannelHandle Channel { get; }

    /// <summary>Gets a value indicating whether the channel opened successfully and has not been closed by this wrapper.</summary>
    public bool IsOpen => OpenResult.Status == CitrixVirtualChannelStatus.Success && Volatile.Read(ref _closed) == 0;

    /// <summary>Gets incoming data for this channel.</summary>
    public IObservable<CitrixVirtualChannelData> IncomingData { get; }

    /// <summary>Gets events for this channel.</summary>
    public IObservable<CitrixVirtualChannelEvent> Events { get; }

    /// <summary>Closes this channel once through the injected DriverClose callback.</summary>
    /// <returns>The close result.</returns>
    public CitrixVirtualChannelCloseResult Close() => Close(CancellationToken.None);

    /// <summary>Closes this channel once through the injected DriverClose callback.</summary>
    /// <param name="cancellationToken">A token that cancels the close operation.</param>
    /// <returns>The close result.</returns>
    public CitrixVirtualChannelCloseResult Close(CancellationToken cancellationToken)
    {
        lock (_closeGate)
        {
            if (Volatile.Read(ref _closed) != 0)
            {
                return new(Channel, CitrixVirtualChannelStatus.Closed);
            }

            if (!Channel.HasValue)
            {
                return new(Channel, CitrixVirtualChannelStatus.InvalidParameter);
            }

            var result = _adapter.DriverClose(Channel, cancellationToken);
            Throw.IfNull(result);
            if (result.Status is CitrixVirtualChannelStatus.Success or CitrixVirtualChannelStatus.Closed)
            {
                Volatile.Write(ref _closed, 1);
            }

            return result;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (IsOpen)
        {
            _ = Close();
        }
    }
}
