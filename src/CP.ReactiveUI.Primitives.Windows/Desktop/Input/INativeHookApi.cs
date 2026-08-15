// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Composes native hook access for production and deterministic tests.</summary>
internal interface INativeHookApi
{
    /// <summary>Calls the next hook in the chain.</summary>
    /// <param name="hookHandle">The hook handle.</param>
    /// <param name="code">The hook code.</param>
    /// <param name="parameter">The hook message parameter.</param>
    /// <param name="data">The hook data pointer.</param>
    /// <returns>The hook result.</returns>
    IntPtr CallNextHookEx(IntPtr hookHandle, int code, IntPtr parameter, IntPtr data);

    /// <summary>Retrieves the asynchronous state of a key.</summary>
    /// <param name="keyCode">The virtual key code.</param>
    /// <returns>The key state.</returns>
    short GetAsyncKeyState(VirtualKeyCode keyCode);

    /// <summary>Retrieves the state of a key.</summary>
    /// <param name="keyCode">The virtual key code.</param>
    /// <returns>The key state.</returns>
    short GetKeyState(VirtualKeyCode keyCode);

    /// <summary>Registers a Windows hook.</summary>
    /// <param name="hookType">The hook type.</param>
    /// <param name="callback">The hook callback.</param>
    /// <param name="moduleHandle">The module handle.</param>
    /// <param name="threadId">The target thread id, or 0 for all threads.</param>
    /// <returns>The hook handle.</returns>
    IntPtr SetWindowsHookEx(HookTypes hookType, LowLevelHookProc callback, IntPtr moduleHandle, uint threadId);

    /// <summary>Removes a Windows hook.</summary>
    /// <param name="hookHandle">The hook handle.</param>
    /// <returns><see langword="true" /> when the hook is removed.</returns>
    bool UnhookWindowsHookEx(IntPtr hookHandle);
}
