// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface;

/// <summary>Composes the native operations used by testable User32 wrappers.</summary>
internal sealed class User32Operations
{
    /// <summary>Stores the pointer size for a 64-bit process.</summary>
    private const int WindowsX64PointerSize = 8;

    /// <summary>Initializes a new instance of the <see cref="User32Operations"/> class.</summary>
    internal User32Operations()
    {
        GetPhysicalCursorPos = User32Api.NativeMethods.GetPhysicalCursorPos;
        GetCursorPosition = static () => System.Windows.Forms.Cursor.Position;
        GetMonitorInfo = User32Api.NativeMethods.GetMonitorInfo;
        OpenInputDesktop = User32Api.NativeMethods.OpenInputDesktop;
        SetThreadDesktop = User32Api.NativeMethods.SetThreadDesktop;
        CloseDesktop = User32Api.NativeMethods.CloseDesktop;
        GetClassLong = User32Api.NativeMethods.GetClassLong;
        GetClassLongPtr = User32Api.NativeMethods.GetClassLongPtr;
        GetWindowLong = User32Api.NativeMethods.GetWindowLong;
        GetWindowLongPtr = User32Api.NativeMethods.GetWindowLongPtr;
        SetWindowLong = User32Api.NativeMethods.SetWindowLong;
        SetWindowLongPtr = User32Api.NativeMethods.SetWindowLongPtr;
        SetFocus = User32Api.NativeMethods.SetFocus;
        SetCapture = User32Api.NativeMethods.SetCapture;
        ReleaseCapture = User32Api.NativeMethods.ReleaseCapture;
        SystemParametersInfoBuffer = InvokeSystemParametersInfoBuffer;
        LockWorkStation = User32Api.NativeMethods.LockWorkStation;
        Is64BitProcess = static () => IntPtr.Size == WindowsX64PointerSize;
    }

    /// <summary>Gets the physical cursor-position operation.</summary>
    internal GetPhysicalCursorPosOperation GetPhysicalCursorPos { get; init; }

    /// <summary>Gets the fallback cursor-position operation.</summary>
    internal Func<NativePoint> GetCursorPosition { get; init; }

    /// <summary>Gets the monitor-information operation.</summary>
    internal GetMonitorInfoOperation GetMonitorInfo { get; init; }

    /// <summary>Gets the input-desktop opening operation.</summary>
    internal Func<uint, bool, DesktopAccessRight, IntPtr> OpenInputDesktop { get; init; }

    /// <summary>Gets the thread-desktop assignment operation.</summary>
    internal Func<IntPtr, bool> SetThreadDesktop { get; init; }

    /// <summary>Gets the input-desktop closing operation.</summary>
    internal Func<IntPtr, bool> CloseDesktop { get; init; }

    /// <summary>Gets the 32-bit class-long operation.</summary>
    internal Func<IntPtr, ClassLongIndex, IntPtr> GetClassLong { get; init; }

    /// <summary>Gets the pointer-sized class-long operation.</summary>
    internal Func<IntPtr, ClassLongIndex, IntPtr> GetClassLongPtr { get; init; }

    /// <summary>Gets the 32-bit window-long operation.</summary>
    internal Func<IntPtr, WindowLongIndex, IntPtr> GetWindowLong { get; init; }

    /// <summary>Gets the pointer-sized window-long operation.</summary>
    internal Func<IntPtr, WindowLongIndex, IntPtr> GetWindowLongPtr { get; init; }

    /// <summary>Gets the 32-bit window-long setter.</summary>
    internal Func<IntPtr, WindowLongIndex, int, int> SetWindowLong { get; init; }

    /// <summary>Gets the pointer-sized window-long setter.</summary>
    internal Func<IntPtr, WindowLongIndex, IntPtr, IntPtr> SetWindowLongPtr { get; init; }

    /// <summary>Gets the focus operation.</summary>
    internal Func<IntPtr, IntPtr> SetFocus { get; init; }

    /// <summary>Gets the capture operation.</summary>
    internal Func<IntPtr, IntPtr> SetCapture { get; init; }

    /// <summary>Gets the release-capture operation.</summary>
    internal Func<bool> ReleaseCapture { get; init; }

    /// <summary>Gets the system-parameters buffer operation.</summary>
    internal SystemParametersInfoBufferOperation SystemParametersInfoBuffer { get; init; }

    /// <summary>Gets the workstation-lock operation.</summary>
    internal Func<bool> LockWorkStation { get; init; }

    /// <summary>Gets the process pointer-width predicate.</summary>
    internal Func<bool> Is64BitProcess { get; init; }

    /// <summary>Invokes the native system-parameters buffer operation.</summary>
    /// <param name="action">The system parameter action.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <param name="value">The mutable UTF-16 buffer.</param>
    /// <param name="behavior">The update behavior.</param>
    /// <returns>True when the operation succeeded.</returns>
    private static unsafe bool InvokeSystemParametersInfoBuffer(
        SystemParametersInfoActions action,
        uint parameterValue,
        IntPtr value,
        SystemParametersInfoBehaviors behavior) =>
        User32Api.NativeMethods.SystemParametersInfo(action, parameterValue, (char*)value, behavior);
}
