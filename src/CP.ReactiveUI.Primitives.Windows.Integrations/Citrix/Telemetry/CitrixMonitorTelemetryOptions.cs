// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Telemetry;

/// <summary>Configures Citrix Monitor Service telemetry polling.</summary>
public sealed class CitrixMonitorTelemetryOptions
{
    /// <summary>The default polling interval.</summary>
    public static readonly TimeSpan DefaultPollingInterval = TimeSpan.FromMinutes(1.0);

    /// <summary>Initializes a new instance of the <see cref="CitrixMonitorTelemetryOptions"/> class.</summary>
    public CitrixMonitorTelemetryOptions()
    {
        PollingInterval = DefaultPollingInterval;
        PollImmediately = true;
        SessionsQuery = string.Empty;
        MachineResourceUtilizationsQuery = string.Empty;
        ConnectionFailureLogsQuery = string.Empty;
        ApplicationFailureLogsQuery = string.Empty;
        MachineResourceUtilizationEntity = CitrixMachineResourceUtilizationTelemetryEntity.ResourceUtilizationSummary;
        ApplicationFailureEntity = CitrixApplicationFailureTelemetryEntity.ApplicationErrors;
        TimeProvider = TimeProvider.System;
    }

    /// <summary>Gets or sets the interval between successful poll attempts.</summary>
    public TimeSpan PollingInterval { get; set; }

    /// <summary>Gets or sets a value indicating whether each stream polls before waiting for the first interval.</summary>
    public bool PollImmediately { get; set; }

    /// <summary>Gets or sets the OData query string used with the <c>Sessions</c> entity.</summary>
    public string SessionsQuery { get; set; }

    /// <summary>Gets or sets the OData query string used with the machine resource utilization entity.</summary>
    public string MachineResourceUtilizationsQuery { get; set; }

    /// <summary>Gets or sets the OData query string used with the <c>ConnectionFailureLogs</c> entity.</summary>
    public string ConnectionFailureLogsQuery { get; set; }

    /// <summary>Gets or sets the OData query string used with the configured application failure entity.</summary>
    public string ApplicationFailureLogsQuery { get; set; }

    /// <summary>Gets or sets the Monitor Service entity used by <c>MachineResourceUtilizations</c>.</summary>
    public CitrixMachineResourceUtilizationTelemetryEntity MachineResourceUtilizationEntity { get; set; }

    /// <summary>Gets or sets the Monitor Service entity used by <c>ApplicationFailureLogs</c>.</summary>
    public CitrixApplicationFailureTelemetryEntity ApplicationFailureEntity { get; set; }

    /// <summary>Gets or sets the clock used to timestamp snapshots.</summary>
    public TimeProvider TimeProvider { get; set; }

    /// <summary>Creates a copy of this options instance.</summary>
    /// <returns>The copied options.</returns>
    public CitrixMonitorTelemetryOptions Clone() =>
        new()
        {
            PollingInterval = PollingInterval,
            PollImmediately = PollImmediately,
            SessionsQuery = SessionsQuery,
            MachineResourceUtilizationsQuery = MachineResourceUtilizationsQuery,
            ConnectionFailureLogsQuery = ConnectionFailureLogsQuery,
            ApplicationFailureLogsQuery = ApplicationFailureLogsQuery,
            MachineResourceUtilizationEntity = MachineResourceUtilizationEntity,
            ApplicationFailureEntity = ApplicationFailureEntity,
            TimeProvider = TimeProvider,
        };
}
