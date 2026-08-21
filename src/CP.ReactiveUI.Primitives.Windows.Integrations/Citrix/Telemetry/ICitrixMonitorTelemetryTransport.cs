// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Telemetry;

/// <summary>Transports Citrix Monitor Service telemetry requests.</summary>
public interface ICitrixMonitorTelemetryTransport
{
    /// <summary>Gets a raw JSON snapshot for the specified request.</summary>
    /// <param name="request">The telemetry request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that yields the raw JSON response payload.</returns>
    Task<string> GetJsonAsync(CitrixMonitorTelemetryRequest request, CancellationToken cancellationToken);
}
