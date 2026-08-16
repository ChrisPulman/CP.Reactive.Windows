// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix;

/// <summary>Represents a WFAPI session-information query.</summary>
/// <param name="serverHandle">The server handle.</param>
/// <param name="sessionId">The session identifier.</param>
/// <param name="infoType">The information type.</param>
/// <param name="buffer">The returned buffer pointer.</param>
/// <param name="bytesReturned">The number of bytes returned.</param>
/// <returns><see langword="true"/> when the query succeeds.</returns>
internal delegate bool QuerySessionInformationDelegate(
    IntPtr serverHandle,
    int sessionId,
    InfoClasses infoType,
    out IntPtr buffer,
    out int bytesReturned);
