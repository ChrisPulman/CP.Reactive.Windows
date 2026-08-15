// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Composition;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Composition;
#endif

/// <summary>Represents a DWM unsigned-integer window-attribute operation.</summary>
/// <param name="windowHandle">The target window handle.</param>
/// <param name="attribute">The requested window attribute.</param>
/// <param name="value">The retrieved attribute value.</param>
/// <param name="size">The size of the attribute value.</param>
/// <returns>The operation result.</returns>
internal delegate HResult DwmGetUIntWindowAttributeOperation(
    IntPtr windowHandle,
    DwmWindowAttributes attribute,
    out uint value,
    int size);
