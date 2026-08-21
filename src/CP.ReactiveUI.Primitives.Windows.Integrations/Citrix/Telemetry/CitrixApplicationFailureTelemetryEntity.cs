// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Telemetry;

/// <summary>Citrix Monitor Service entities that represent application failures.</summary>
public enum CitrixApplicationFailureTelemetryEntity
{
    /// <summary>Use the Monitor Service <c>ApplicationErrors</c> entity.</summary>
    ApplicationErrors,

    /// <summary>Use the Monitor Service <c>ApplicationFaults</c> entity.</summary>
    ApplicationFaults,
}
