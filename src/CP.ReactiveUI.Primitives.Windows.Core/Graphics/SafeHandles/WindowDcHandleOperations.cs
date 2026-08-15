// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;

/// <summary>Composes window device-context operations from supplied functions.</summary>
internal sealed class WindowDcHandleOperations : IWindowDcHandleApi
{
    /// <summary>Stores the client-area device-context operation.</summary>
    private readonly Func<IntPtr, IntPtr> _getDc;

    /// <summary>Stores the desktop-window operation.</summary>
    private readonly Func<IntPtr> _getDesktopWindow;

    /// <summary>Stores the whole-window device-context operation.</summary>
    private readonly Func<IntPtr, IntPtr> _getWindowDc;

    /// <summary>Stores the release operation.</summary>
    private readonly Func<IntPtr, IntPtr, bool> _releaseDc;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.WindowDcHandleOperations" /> class.</summary>
    /// <param name="getWindowDc">The whole-window device-context operation.</param>
    /// <param name="getDc">The client-area device-context operation.</param>
    /// <param name="getDesktopWindow">The desktop-window operation.</param>
    /// <param name="releaseDc">The device-context release operation.</param>
    internal WindowDcHandleOperations(
        Func<IntPtr, IntPtr> getWindowDc,
        Func<IntPtr, IntPtr> getDc,
        Func<IntPtr> getDesktopWindow,
        Func<IntPtr, IntPtr, bool> releaseDc)
    {
        Throw.IfNull(getWindowDc);
        Throw.IfNull(getDc);
        Throw.IfNull(getDesktopWindow);
        Throw.IfNull(releaseDc);
        _getWindowDc = getWindowDc;
        _getDc = getDc;
        _getDesktopWindow = getDesktopWindow;
        _releaseDc = releaseDc;
    }

    /// <inheritdoc />
    public IntPtr GetWindowDc(IntPtr windowHandle) => _getWindowDc(windowHandle);

    /// <inheritdoc />
    public IntPtr GetDc(IntPtr windowHandle) => _getDc(windowHandle);

    /// <inheritdoc />
    public IntPtr GetDesktopWindow() => _getDesktopWindow();

    /// <inheritdoc />
    public bool ReleaseDc(IntPtr windowHandle, IntPtr deviceContextHandle) =>
        _releaseDc(windowHandle, deviceContextHandle);
}
