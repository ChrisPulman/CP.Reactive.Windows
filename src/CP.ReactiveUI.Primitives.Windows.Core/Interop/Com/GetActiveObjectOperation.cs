// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Interop.Com;

/// <summary>Retrieves a pointer to a running object.</summary>
/// <param name="classId">The class identifier (CLSID) of the active object from the OLE registration database.</param>
/// <param name="reserved">Reserved for future use. Must be null.</param>
/// <param name="activeObject">The requested active object.</param>
/// <returns>The operation result.</returns>
internal delegate HResult GetActiveObjectOperation(
    ref Guid classId,
    nint reserved,
    out nint activeObject);
