// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
#endif

/// <summary>Invokes the monitor lookup that accepts a rectangle by reference.</summary>
/// <param name="rect">The monitor-selection rectangle.</param>
/// <param name="flags">The fallback behavior when no monitor intersects the rectangle.</param>
/// <returns>The selected monitor handle.</returns>
internal delegate IntPtr MonitorFromRectOperation(ref NativeRect rect, MonitorFrom flags);
