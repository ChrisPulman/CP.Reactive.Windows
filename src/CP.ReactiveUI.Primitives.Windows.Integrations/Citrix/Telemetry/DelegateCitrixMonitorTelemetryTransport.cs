// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Telemetry;

/// <summary>Function-backed Citrix Monitor Service telemetry transport.</summary>
public sealed class DelegateCitrixMonitorTelemetryTransport : ICitrixMonitorTelemetryTransport
{
    /// <summary>The source function used to get telemetry JSON.</summary>
    private readonly Func<CitrixMonitorTelemetryRequest, CancellationToken, Task<string>> _getJsonAsync;

    /// <summary>Initializes a new instance of the <see cref="DelegateCitrixMonitorTelemetryTransport"/> class.</summary>
    /// <param name="getJsonAsync">The source function used to get telemetry JSON.</param>
    public DelegateCitrixMonitorTelemetryTransport(Func<CitrixMonitorTelemetryRequest, CancellationToken, Task<string>> getJsonAsync)
    {
        Throw.IfNull(getJsonAsync);
        _getJsonAsync = getJsonAsync;
    }

    /// <inheritdoc />
    public Task<string> GetJsonAsync(CitrixMonitorTelemetryRequest request, CancellationToken cancellationToken)
    {
        Throw.IfNull(request);
        return _getJsonAsync(request, cancellationToken);
    }
}
