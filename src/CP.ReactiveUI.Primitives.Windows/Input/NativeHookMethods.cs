// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Shared native methods for low-level Windows hooks.</summary>
internal static class NativeHookMethods
{
    /// <summary>Native low-level hook imports.</summary>
    internal static class NativeMethods
    {
        /// <summary>Calls the next hook in the chain.</summary>
        /// <param name="hookHandle">The hook handle.</param>
        /// <param name="code">The hook code.</param>
        /// <param name="parameter">The hook message parameter.</param>
        /// <param name="data">The hook data pointer.</param>
        /// <returns>The hook result.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr CallNextHookEx(IntPtr hookHandle, int code, IntPtr parameter, IntPtr data);

        /// <summary>Retrieves the asynchronous state of a key.</summary>
        /// <param name="keyCode">The virtual key code.</param>
        /// <returns>The key state.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern short GetAsyncKeyState(VirtualKeyCode keyCode);

        /// <summary>Retrieves the state of a key.</summary>
        /// <param name="keyCode">The virtual key code.</param>
        /// <returns>The key state.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern short GetKeyState(VirtualKeyCode keyCode);

        /// <summary>Registers a Windows hook.</summary>
        /// <param name="hookType">The hook type.</param>
        /// <param name="callback">The hook callback.</param>
        /// <param name="moduleHandle">The module handle.</param>
        /// <param name="threadId">The target thread id, or 0 for all threads.</param>
        /// <returns>The hook handle.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr SetWindowsHookEx(HookTypes hookType, LowLevelHookProc callback, IntPtr moduleHandle, uint threadId);

        /// <summary>Removes a Windows hook.</summary>
        /// <param name="hookHandle">The hook handle.</param>
        /// <returns><see langword="true" /> when the hook is removed.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hookHandle);
    }

    /// <summary>The hook API used by this process.</summary>
    private static INativeHookApi _api = WindowsNativeHookApi.Instance;

    /// <summary>Calls the next hook in the chain.</summary>
    /// <param name="hookHandle">The hook handle.</param>
    /// <param name="code">The hook code.</param>
    /// <param name="parameter">The hook message parameter.</param>
    /// <param name="data">The hook data pointer.</param>
    /// <returns>The hook result.</returns>
    internal static IntPtr CallNextHookEx(IntPtr hookHandle, int code, IntPtr parameter, IntPtr data) => _api.CallNextHookEx(hookHandle, code, parameter, data);

    /// <summary>Retrieves the asynchronous state of a key.</summary>
    /// <param name="keyCode">The virtual key code.</param>
    /// <returns>The key state.</returns>
    internal static short GetAsyncKeyState(VirtualKeyCode keyCode) => _api.GetAsyncKeyState(keyCode);

    /// <summary>Retrieves the state of a key.</summary>
    /// <param name="keyCode">The virtual key code.</param>
    /// <returns>The key state.</returns>
    internal static short GetKeyState(VirtualKeyCode keyCode) => _api.GetKeyState(keyCode);

    /// <summary>Registers a Windows hook.</summary>
    /// <param name="hookType">The hook type.</param>
    /// <param name="callback">The hook callback.</param>
    /// <param name="moduleHandle">The module handle.</param>
    /// <param name="threadId">The target thread id, or 0 for all threads.</param>
    /// <returns>The hook handle.</returns>
    internal static IntPtr SetWindowsHookEx(HookTypes hookType, LowLevelHookProc callback, IntPtr moduleHandle, uint threadId) => _api.SetWindowsHookEx(hookType, callback, moduleHandle, threadId);

    /// <summary>Removes a Windows hook.</summary>
    /// <param name="hookHandle">The hook handle.</param>
    /// <returns><see langword="true" /> when the hook is removed.</returns>
    internal static bool UnhookWindowsHookEx(IntPtr hookHandle) => _api.UnhookWindowsHookEx(hookHandle);

    /// <summary>Replaces the hook API for deterministic tests.</summary>
    /// <param name="api">The replacement hook API.</param>
    /// <returns>The previous hook API.</returns>
    internal static INativeHookApi SetApiForTesting(INativeHookApi api)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(api);
        INativeHookApi api2 = _api;
        _api = api;
        return api2;
    }
}
