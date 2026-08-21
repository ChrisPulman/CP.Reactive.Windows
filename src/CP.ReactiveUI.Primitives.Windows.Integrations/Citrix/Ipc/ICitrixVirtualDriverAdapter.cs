// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Ipc;

/// <summary>Abstracts a Citrix virtual-driver host for composable virtual-channel IPC.</summary>
public interface ICitrixVirtualDriverAdapter
{
    /// <summary>Gets the incoming data published by the virtual-driver host.</summary>
    IObservable<CitrixVirtualChannelData> IncomingData { get; }

    /// <summary>Gets lifecycle and diagnostic events published by the virtual-driver host.</summary>
    IObservable<CitrixVirtualChannelEvent> Events { get; }

    /// <summary>Opens a virtual channel through the host-provided DriverOpen callback.</summary>
    /// <param name="request">The open request.</param>
    /// <param name="cancellationToken">A token that cancels the open operation.</param>
    /// <returns>The open result.</returns>
    CitrixVirtualChannelOpenResult DriverOpen(
        CitrixVirtualChannelOpenRequest request,
        CancellationToken cancellationToken);

    /// <summary>Closes a virtual channel through the host-provided DriverClose callback.</summary>
    /// <param name="channel">The channel to close.</param>
    /// <param name="cancellationToken">A token that cancels the close operation.</param>
    /// <returns>The close result.</returns>
    CitrixVirtualChannelCloseResult DriverClose(
        CitrixVirtualChannelHandle channel,
        CancellationToken cancellationToken);

    /// <summary>Writes data through the host-provided DriverWrite callback.</summary>
    /// <param name="request">The write request.</param>
    /// <param name="cancellationToken">A token that cancels the write operation.</param>
    /// <returns>The write result.</returns>
    CitrixVirtualChannelWriteResult DriverWrite(
        CitrixVirtualChannelWriteRequest request,
        CancellationToken cancellationToken);

    /// <summary>Registers a virtual-channel feature through the host-provided VdRegisterFeature callback.</summary>
    /// <param name="feature">The feature to register.</param>
    /// <param name="cancellationToken">A token that cancels the registration operation.</param>
    /// <returns>The feature registration result.</returns>
    CitrixVirtualChannelFeatureRegistration VdRegisterFeature(
        CitrixVirtualChannelFeature feature,
        CancellationToken cancellationToken);
}
