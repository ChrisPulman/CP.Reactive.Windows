// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Interop.Com;

/// <summary>Converts a program identifier into a class identifier.</summary>
/// <param name="programId">The program identifier.</param>
/// <param name="classId">Receives the class identifier.</param>
/// <returns>The native result code.</returns>
internal delegate HResult ClassIdFromProgIdOperation(string programId, out Guid classId);
