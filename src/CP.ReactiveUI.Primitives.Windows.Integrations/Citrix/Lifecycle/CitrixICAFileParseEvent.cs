// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Lifecycle;

/// <summary>Represents a Citrix ICA file parse event.</summary>
public sealed class CitrixICAFileParseEvent : CitrixLifecycleEvent
{
    /// <summary>Initializes a new instance of the <see cref="CitrixICAFileParseEvent" /> class.</summary>
    /// <param name="icaFile">The parsed ICA file payload.</param>
    /// <param name="timestamp">The event timestamp.</param>
    public CitrixICAFileParseEvent(CitrixICAFileInfo icaFile, DateTimeOffset timestamp)
        : base(timestamp, EventMask.None)
    {
        ICAFile = icaFile ?? CitrixICAFileInfo.Empty;
    }

    /// <summary>Gets the parsed ICA file payload.</summary>
    public CitrixICAFileInfo ICAFile { get; }
}
