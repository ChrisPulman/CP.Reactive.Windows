// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Production Win32 hook API implementation.</summary>
internal sealed class WindowsNativeHookApi : INativeHookApi
{
    /// <summary>The singleton instance.</summary>
    internal static readonly WindowsNativeHookApi Instance = new();

    /// <inheritdoc />
    public IntPtr CallNextHookEx(IntPtr hookHandle, int code, IntPtr parameter, IntPtr data) => NativeHookMethods.NativeMethods.CallNextHookEx(hookHandle, code, parameter, data);

    /// <inheritdoc />
    public short GetAsyncKeyState(VirtualKeyCode keyCode) => NativeHookMethods.NativeMethods.GetAsyncKeyState(keyCode);

    /// <inheritdoc />
    public short GetKeyState(VirtualKeyCode keyCode) => NativeHookMethods.NativeMethods.GetKeyState(keyCode);

    /// <inheritdoc />
    public IntPtr SetWindowsHookEx(HookTypes hookType, LowLevelHookProc callback, IntPtr moduleHandle, uint threadId) => NativeHookMethods.NativeMethods.SetWindowsHookEx(hookType, callback, moduleHandle, threadId);

    /// <inheritdoc />
    public bool UnhookWindowsHookEx(IntPtr hookHandle) => NativeHookMethods.NativeMethods.UnhookWindowsHookEx(hookHandle);
}
