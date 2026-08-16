// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix;

/// <summary>Abstracts Citrix WFAPI calls so the managed behavior can be composed independently.</summary>
internal interface IWinFrameApi
{
    /// <summary>Queries session information.</summary>
    /// <param name="serverHandle">The server handle.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="infoType">The information type.</param>
    /// <param name="buffer">The returned buffer.</param>
    /// <param name="bytesReturned">The returned byte count.</param>
    /// <returns><see langword="true"/> when information was returned.</returns>
    bool QuerySessionInformation(IntPtr serverHandle, int sessionId, InfoClasses infoType, out IntPtr buffer, out int bytesReturned);

    /// <summary>Waits for a session event.</summary>
    /// <param name="serverHandle">The server handle.</param>
    /// <param name="eventMask">The events to wait for.</param>
    /// <param name="eventFlags">The events that occurred.</param>
    /// <returns><see langword="true"/> when an event was returned.</returns>
    bool WaitSystemEvent(IntPtr serverHandle, EventMask eventMask, out EventMask eventFlags);

    /// <summary>Frees a session-information allocation.</summary>
    /// <param name="memory">The memory pointer to free.</param>
    void FreeMemory(IntPtr memory);
}
