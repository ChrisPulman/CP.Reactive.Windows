// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
#endif

/// <summary>Provides composable access to window and device-context APIs used by DPI fallback logic.</summary>
internal interface IDpiCoreInterop
{
    /// <summary>Retrieves a device capability.</summary>
    /// <param name="deviceContextHandle">The device-context handle.</param>
    /// <param name="deviceCaps">The requested device capability.</param>
    /// <returns>The requested capability value.</returns>
    int GetDeviceCaps(SafeWindowDcHandle deviceContextHandle, DeviceCaps deviceCaps);

    /// <summary>Creates a window device-context handle.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The device-context handle, or <see langword="null" /> when one is unavailable.</returns>
    SafeWindowDcHandle FromWindow(IntPtr windowHandle);

    /// <summary>Determines whether a handle represents a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns><see langword="true" /> when the handle represents a window.</returns>
    bool IsWindow(IntPtr windowHandle);

    /// <summary>Retrieves the monitor nearest a rectangle.</summary>
    /// <param name="rect">The rectangle to inspect.</param>
    /// <param name="flags">Monitor lookup flags.</param>
    /// <returns>The monitor handle.</returns>
    IntPtr MonitorFromRect(ref NativeRect rect, MonitorFrom flags);

    /// <summary>Retrieves the monitor associated with a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="flags">Monitor lookup flags.</param>
    /// <returns>The monitor handle.</returns>
    IntPtr MonitorFromWindow(IntPtr windowHandle, MonitorFrom flags);
}
