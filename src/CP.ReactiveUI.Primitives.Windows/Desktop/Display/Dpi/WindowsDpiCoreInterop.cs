// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
#endif

/// <summary>Default Windows interop implementation for DPI fallback logic.</summary>
internal sealed class WindowsDpiCoreInterop : IDpiCoreInterop
{
    /// <summary>Retrieves device capabilities.</summary>
    private readonly Func<SafeWindowDcHandle, DeviceCaps, int> _getDeviceCaps;

    /// <summary>Creates a device context for a window.</summary>
    private readonly Func<IntPtr, SafeWindowDcHandle> _fromWindow;

    /// <summary>Determines whether a handle identifies a window.</summary>
    private readonly Func<IntPtr, bool> _isWindow;

    /// <summary>Retrieves the monitor nearest a rectangle.</summary>
    private readonly MonitorFromRectOperation _monitorFromRect;

    /// <summary>Retrieves the monitor associated with a window.</summary>
    private readonly Func<IntPtr, MonitorFrom, IntPtr> _monitorFromWindow;

    /// <summary>Initializes a new instance of the <see cref="WindowsDpiCoreInterop"/> class.</summary>
    /// <param name="getDeviceCaps">Retrieves device capabilities.</param>
    /// <param name="fromWindow">Creates a window device context.</param>
    /// <param name="isWindow">Determines whether a handle is a window.</param>
    /// <param name="monitorFromRect">Retrieves the monitor nearest a rectangle.</param>
    /// <param name="monitorFromWindow">Retrieves the monitor associated with a window.</param>
    internal WindowsDpiCoreInterop(
        Func<SafeWindowDcHandle, DeviceCaps, int> getDeviceCaps,
        Func<IntPtr, SafeWindowDcHandle> fromWindow,
        Func<IntPtr, bool> isWindow,
        MonitorFromRectOperation monitorFromRect,
        Func<IntPtr, MonitorFrom, IntPtr> monitorFromWindow)
    {
        _getDeviceCaps = getDeviceCaps;
        _fromWindow = fromWindow;
        _isWindow = isWindow;
        _monitorFromRect = monitorFromRect;
        _monitorFromWindow = monitorFromWindow;
    }

    /// <summary>Initializes a new instance of the <see cref="WindowsDpiCoreInterop"/> class.</summary>
    private WindowsDpiCoreInterop()
        : this(
            Gdi32Api.GetDeviceCaps,
            SafeWindowDcHandle.FromWindow,
            User32Api.IsWindow,
            User32Api.MonitorFromRect,
            User32Api.MonitorFromWindow) { }

    /// <summary>Gets the singleton Windows DPI core interop implementation.</summary>
    internal static WindowsDpiCoreInterop Instance { get; } = new();

    /// <inheritdoc />
    public int GetDeviceCaps(SafeWindowDcHandle deviceContextHandle, DeviceCaps deviceCaps) =>
        _getDeviceCaps(deviceContextHandle, deviceCaps);

    /// <inheritdoc />
    public SafeWindowDcHandle FromWindow(IntPtr windowHandle) => _fromWindow(windowHandle);

    /// <inheritdoc />
    public bool IsWindow(IntPtr windowHandle) => _isWindow(windowHandle);

    /// <inheritdoc />
    public IntPtr MonitorFromRect(ref NativeRect rect, MonitorFrom flags) => _monitorFromRect(ref rect, flags);

    /// <inheritdoc />
    public IntPtr MonitorFromWindow(IntPtr windowHandle, MonitorFrom flags) =>
        _monitorFromWindow(windowHandle, flags);
}
