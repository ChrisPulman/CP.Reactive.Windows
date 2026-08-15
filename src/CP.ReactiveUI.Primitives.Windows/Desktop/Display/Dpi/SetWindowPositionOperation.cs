// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
#endif

/// <summary>Positions a window in response to a DPI-change notification.</summary>
/// <param name="windowHandle">The window to position.</param>
/// <param name="insertAfterWindowHandle">The sibling window used for z-ordering.</param>
/// <param name="x">The target x-coordinate.</param>
/// <param name="y">The target y-coordinate.</param>
/// <param name="width">The target width.</param>
/// <param name="height">The target height.</param>
/// <param name="flags">The positioning flags.</param>
/// <returns><see langword="true" /> when the operation succeeds.</returns>
internal delegate bool SetWindowPositionOperation(
    IntPtr windowHandle,
    IntPtr insertAfterWindowHandle,
    int x,
    int y,
    int width,
    int height,
    WindowPos flags);
