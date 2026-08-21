// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Telemetry;

/// <summary>Citrix Monitor Service entities that represent machine resource utilization.</summary>
public enum CitrixMachineResourceUtilizationTelemetryEntity
{
    /// <summary>Use the Monitor Service <c>ResourceUtilizationSummary</c> entity.</summary>
    ResourceUtilizationSummary,

    /// <summary>Use the Monitor Service <c>ResourceUtilization</c> entity.</summary>
    ResourceUtilization,
}
