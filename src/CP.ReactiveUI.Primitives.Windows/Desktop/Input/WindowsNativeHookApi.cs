// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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

    /// <summary>The operation that forwards a hook invocation.</summary>
    private static Func<IntPtr, int, IntPtr, IntPtr, IntPtr> _callNextHookEx = NativeHookMethods.NativeMethods.CallNextHookEx;

    /// <summary>The operation that gets asynchronous key state.</summary>
    private static Func<VirtualKeyCode, short> _getAsyncKeyState = NativeHookMethods.NativeMethods.GetAsyncKeyState;

    /// <summary>The operation that gets key state.</summary>
    private static Func<VirtualKeyCode, short> _getKeyState = NativeHookMethods.NativeMethods.GetKeyState;

    /// <summary>The operation that registers a low-level hook.</summary>
    private static Func<HookTypes, LowLevelHookProc, IntPtr, uint, IntPtr> _setWindowsHookEx = NativeHookMethods.NativeMethods.SetWindowsHookEx;

    /// <summary>The operation that unregisters a low-level hook.</summary>
    private static Func<IntPtr, bool> _unhookWindowsHookEx = NativeHookMethods.NativeMethods.UnhookWindowsHookEx;

    /// <inheritdoc />
    public IntPtr CallNextHookEx(IntPtr hookHandle, int code, IntPtr parameter, IntPtr data) => _callNextHookEx(hookHandle, code, parameter, data);

    /// <inheritdoc />
    public short GetAsyncKeyState(VirtualKeyCode keyCode) => _getAsyncKeyState(keyCode);

    /// <inheritdoc />
    public short GetKeyState(VirtualKeyCode keyCode) => _getKeyState(keyCode);

    /// <inheritdoc />
    public IntPtr SetWindowsHookEx(HookTypes hookType, LowLevelHookProc callback, IntPtr moduleHandle, uint threadId) => _setWindowsHookEx(hookType, callback, moduleHandle, threadId);

    /// <inheritdoc />
    public bool UnhookWindowsHookEx(IntPtr hookHandle) => _unhookWindowsHookEx(hookHandle);

    /// <summary>Overrides native hook operations for deterministic tests.</summary>
    /// <param name="callNextHookEx">The replacement forwarding operation.</param>
    /// <param name="getAsyncKeyState">The replacement asynchronous-key-state operation.</param>
    /// <param name="getKeyState">The replacement key-state operation.</param>
    /// <param name="setWindowsHookEx">The replacement hook-registration operation.</param>
    /// <param name="unhookWindowsHookEx">The replacement hook-unregistration operation.</param>
    /// <returns>A lifetime that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(
        Func<IntPtr, int, IntPtr, IntPtr, IntPtr> callNextHookEx,
        Func<VirtualKeyCode, short> getAsyncKeyState,
        Func<VirtualKeyCode, short> getKeyState,
        Func<HookTypes, LowLevelHookProc, IntPtr, uint, IntPtr> setWindowsHookEx,
        Func<IntPtr, bool> unhookWindowsHookEx)
    {
        Throw.IfNull(callNextHookEx);
        Throw.IfNull(getAsyncKeyState);
        Throw.IfNull(getKeyState);
        Throw.IfNull(setWindowsHookEx);
        Throw.IfNull(unhookWindowsHookEx);
        var previousCallNextHookEx = _callNextHookEx;
        var previousGetAsyncKeyState = _getAsyncKeyState;
        var previousGetKeyState = _getKeyState;
        var previousSetWindowsHookEx = _setWindowsHookEx;
        var previousUnhookWindowsHookEx = _unhookWindowsHookEx;
        _callNextHookEx = callNextHookEx;
        _getAsyncKeyState = getAsyncKeyState;
        _getKeyState = getKeyState;
        _setWindowsHookEx = setWindowsHookEx;
        _unhookWindowsHookEx = unhookWindowsHookEx;
        return new ActionDisposable(() =>
        {
            _callNextHookEx = previousCallNextHookEx;
            _getAsyncKeyState = previousGetAsyncKeyState;
            _getKeyState = previousGetKeyState;
            _setWindowsHookEx = previousSetWindowsHookEx;
            _unhookWindowsHookEx = previousUnhookWindowsHookEx;
        });
    }
}
