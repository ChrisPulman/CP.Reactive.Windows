// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;

/// <summary>Abstracts window device-context operations used by safe handles.</summary>
internal interface IWindowDcHandleApi
{
    /// <summary>Gets the device context for an entire window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The device-context handle.</returns>
    IntPtr GetWindowDc(IntPtr windowHandle);

    /// <summary>Gets the device context for a window client area.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The device-context handle.</returns>
    IntPtr GetDc(IntPtr windowHandle);

    /// <summary>Gets the desktop window.</summary>
    /// <returns>The desktop window handle.</returns>
    IntPtr GetDesktopWindow();

    /// <summary>Releases a window device context.</summary>
    /// <param name="windowHandle">The owning window.</param>
    /// <param name="deviceContextHandle">The device-context handle.</param>
    /// <returns><see langword="true" /> when the device context was released.</returns>
    bool ReleaseDc(IntPtr windowHandle, IntPtr deviceContextHandle);
}
