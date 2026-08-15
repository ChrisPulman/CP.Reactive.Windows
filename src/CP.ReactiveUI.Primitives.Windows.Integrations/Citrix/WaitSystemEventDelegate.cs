// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix;

/// <summary>Represents a WFAPI session-event wait.</summary>
/// <param name="serverHandle">The server handle.</param>
/// <param name="eventMask">The events to observe.</param>
/// <param name="eventFlags">The events that occurred.</param>
/// <returns><see langword="true"/> when an event is returned.</returns>
internal delegate bool WaitSystemEventDelegate(IntPtr serverHandle, EventMask eventMask, out EventMask eventFlags);
