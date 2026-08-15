// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Starts a Restart Manager session.</summary>
/// <param name="sessionHandle">Receives the session handle.</param>
/// <param name="sessionFlags">The session flags.</param>
/// <param name="sessionKey">Receives the session key.</param>
/// <returns>The native result code.</returns>
internal delegate int StartSessionOperation(
    out int sessionHandle,
    int sessionFlags,
    StringBuilder sessionKey);
