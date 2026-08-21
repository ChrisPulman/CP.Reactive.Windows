// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Telemetry;

/// <summary>Describes a Citrix Monitor Service OData snapshot request.</summary>
public sealed class CitrixMonitorTelemetryRequest
{
    /// <summary>Initializes a new instance of the <see cref="CitrixMonitorTelemetryRequest"/> class.</summary>
    /// <param name="name">The public telemetry stream name.</param>
    /// <param name="entity">The Citrix Monitor Service OData entity.</param>
    /// <param name="query">The optional OData query string.</param>
    public CitrixMonitorTelemetryRequest(string name, string entity, string query)
    {
        Throw.IfNullOrEmpty(name);
        Throw.IfNullOrEmpty(entity);
        Name = name;
        Entity = entity;
        Query = query ?? string.Empty;
    }

    /// <summary>Gets the public telemetry stream name.</summary>
    public string Name { get; }

    /// <summary>Gets the Citrix Monitor Service OData entity.</summary>
    public string Entity { get; }

    /// <summary>Gets the optional OData query string.</summary>
    public string Query { get; }
}
