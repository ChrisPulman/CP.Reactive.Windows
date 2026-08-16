// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Composition;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Composition;
#endif
/// <summary>Updates a shared DWM window surface.</summary>
/// <param name="windowHandle">The window handle.</param>
/// <param name="one">The first native option.</param>
/// <param name="two">The second native option.</param>
/// <param name="three">The third native option.</param>
/// <param name="monitorHandle">The monitor handle.</param>
/// <param name="unknown">The undocumented option value.</param>
/// <returns>The native operation result.</returns>
internal delegate int DwmUpdateWindowSharedOperation(
    IntPtr windowHandle,
    int one,
    int two,
    int three,
    IntPtr monitorHandle,
    IntPtr unknown);
