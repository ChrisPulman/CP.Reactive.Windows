// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Represents GetSharedSurface.</summary>
/// <param name="windowHandle">The window handle.</param>
/// <param name="adapterLuid">The adapter LUID.</param>
/// <param name="one">The first option.</param>
/// <param name="two">The second option.</param>
/// <param name="format">The output Direct3D format pointer.</param>
/// <param name="sharedHandle">The output shared-handle pointer.</param>
/// <param name="unknown">The undocumented option.</param>
/// <returns>The native result.</returns>
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
internal delegate int GetSharedSurfaceStub(
    IntPtr windowHandle,
    long adapterLuid,
    uint one,
    uint two,
    IntPtr format,
    IntPtr sharedHandle,
    ulong unknown);
