// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Callback used by low-level Windows hooks.</summary>
/// <param name="code">The hook code.</param>
/// <param name="parameter">The hook message parameter.</param>
/// <param name="data">The hook data pointer.</param>
/// <returns>The hook result.</returns>
internal delegate IntPtr LowLevelHookProc(int code, IntPtr parameter, IntPtr data);
