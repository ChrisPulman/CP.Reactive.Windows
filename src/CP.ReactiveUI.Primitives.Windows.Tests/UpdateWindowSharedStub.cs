// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Represents UpdateWindowShared.</summary>
/// <param name="windowHandle">The window handle.</param>
/// <param name="one">The first option.</param>
/// <param name="two">The second option.</param>
/// <param name="three">The third option.</param>
/// <param name="monitorHandle">The monitor handle.</param>
/// <param name="unknown">The undocumented option.</param>
/// <returns>The native result.</returns>
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
internal delegate int UpdateWindowSharedStub(
    IntPtr windowHandle,
    int one,
    int two,
    int three,
    IntPtr monitorHandle,
    IntPtr unknown);
