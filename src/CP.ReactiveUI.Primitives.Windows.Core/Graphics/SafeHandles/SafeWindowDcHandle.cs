// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using Microsoft.Win32.SafeHandles;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;

/// <summary>Provides a safe handle for a window device context.</summary>
public class SafeWindowDcHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>Stores the active window device-context implementation.</summary>
    private static IWindowDcHandleApi _api = new WindowDcHandleOperations(
        User32Api.GetWindowDC,
        User32Api.GetDC,
        User32Api.GetDesktopWindow,
        User32Api.ReleaseDC);

    /// <summary>Stores the window that owns the device context.</summary>
    private readonly IntPtr _windowHandle;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeWindowDcHandle" /> class.</summary>
    public SafeWindowDcHandle()
        : base(ownsHandle: true) { }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeWindowDcHandle" /> class from an existing handle.</summary>
    /// <param name="windowHandle">The window that owns the device context.</param>
    /// <param name="existingDcHandle">The existing device-context handle.</param>
    public SafeWindowDcHandle(IntPtr windowHandle, IntPtr existingDcHandle)
        : base(ownsHandle: true)
    {
        _windowHandle = windowHandle;
        SetHandle(existingDcHandle);
    }

    /// <summary>Creates a safe device-context handle for an entire window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>A safe handle for the window's device context, or <see langword="null" /> when <paramref name="windowHandle" /> is zero.</returns>
    public static SafeWindowDcHandle FromWindow(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return null;
        }

        IntPtr deviceContext = Volatile.Read(ref _api).GetWindowDc(windowHandle);
        return new(windowHandle, deviceContext);
    }

    /// <summary>Creates a safe device-context handle for a window client area.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>A safe handle for the window client area's device context.</returns>
    public static SafeWindowDcHandle FromWindowClientArea(IntPtr windowHandle)
    {
        IntPtr deviceContext = Volatile.Read(ref _api).GetDc(windowHandle);
        return new(windowHandle, deviceContext);
    }

    /// <summary>Creates a safe device-context handle for the desktop.</summary>
    /// <returns>A safe handle for the desktop device context.</returns>
    public static SafeWindowDcHandle FromDesktop() =>
        FromWindow(Volatile.Read(ref _api).GetDesktopWindow());

    /// <summary>Atomically replaces the window device-context implementation.</summary>
    /// <param name="replacement">The replacement implementation.</param>
    /// <returns>The previous implementation.</returns>
    internal static IWindowDcHandleApi SwapApi(IWindowDcHandleApi replacement)
    {
        Throw.IfNull(replacement);
        return Interlocked.Exchange(ref _api, replacement);
    }

    /// <inheritdoc />
    protected override bool ReleaseHandle() =>
        Volatile.Read(ref _api).ReleaseDc(_windowHandle, handle);
}
