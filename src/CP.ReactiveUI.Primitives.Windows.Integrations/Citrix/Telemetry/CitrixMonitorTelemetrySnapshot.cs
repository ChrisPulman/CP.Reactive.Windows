// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Telemetry;

/// <summary>Contains one raw JSON snapshot returned by the Citrix Monitor Service.</summary>
public sealed class CitrixMonitorTelemetrySnapshot
{
    /// <summary>Initializes a new instance of the <see cref="CitrixMonitorTelemetrySnapshot"/> class.</summary>
    /// <param name="request">The request that produced the snapshot.</param>
    /// <param name="json">The raw JSON response payload.</param>
    /// <param name="observedAt">The UTC time at which the payload was observed.</param>
    public CitrixMonitorTelemetrySnapshot(CitrixMonitorTelemetryRequest request, string json, DateTimeOffset observedAt)
    {
        Throw.IfNull(request);
        Name = request.Name;
        Entity = request.Entity;
        Query = request.Query;
        Json = json ?? string.Empty;
        ObservedAt = observedAt;
    }

    /// <summary>Gets the public telemetry stream name.</summary>
    public string Name { get; }

    /// <summary>Gets the Citrix Monitor Service OData entity used for the request.</summary>
    public string Entity { get; }

    /// <summary>Gets the OData query string used for the request.</summary>
    public string Query { get; }

    /// <summary>Gets the raw JSON response payload.</summary>
    public string Json { get; }

    /// <summary>Gets the UTC time at which the payload was observed.</summary>
    public DateTimeOffset ObservedAt { get; }
}
