// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Interop.Com;

/// <summary>Converts a class identifier into a program identifier.</summary>
/// <param name="classId">The class identifier.</param>
/// <param name="programId">Receives the program identifier.</param>
/// <returns>The native result code.</returns>
internal delegate HResult ProgIdFromClassIdOperation(ref Guid classId, out string programId);
