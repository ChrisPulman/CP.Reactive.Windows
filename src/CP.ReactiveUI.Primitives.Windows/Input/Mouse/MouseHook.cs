// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using ReactiveUI.Primitives.Disposables;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Mouse;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Mouse;
#endif
/// <summary>A global mouse hook using ReactiveUI.Primitives.Reactive.</summary>
public sealed class MouseHook
{
    /// <summary>Shared mouse hook singleton.</summary>
    private static readonly Lazy<MouseHook> Singleton = new(() => new MouseHook());

    /// <summary>Stores the shared mouse event stream.</summary>
    private readonly IObservable<MouseHookEventArgs> _mouseObservable;

    /// <summary>Stores the native hook callback so it cannot be garbage collected while hooked.</summary>
    private LowLevelHookProc _callback;

    /// <summary>Gets the global mouse hook event stream.</summary>
    public static IObservable<MouseHookEventArgs> MouseHookEvents => Singleton.Value._mouseObservable;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Input.Mouse.MouseHook" /> class.</summary>
    private MouseHook()
    {
        _mouseObservable = ReactiveSignal.CreateSafe(delegate(IObserver<MouseHookEventArgs> observer)
        {
            IntPtr hookId = IntPtr.Zero;
            _callback = delegate(int code, IntPtr parameter, IntPtr data)
            {
                if (code >= 0)
                {
                    MouseHookEventArgs e = CreateMouseEventArgs(parameter, data);
                    observer.OnNext(e);
                    if (e.Handled)
                    {
                        return (IntPtr)1;
                    }
                }

                return NativeHookMethods.CallNextHookEx(hookId, code, parameter, data);
            };
            hookId = NativeHookMethods.SetWindowsHookEx(HookTypes.WH_MOUSE_LL, _callback, IntPtr.Zero, 0U);
            return new ActionDisposable(delegate
            {
                _ = NativeHookMethods.UnhookWindowsHookEx(hookId);
                _callback = null;
            });
        }).Publish().RefCount();
    }

    /// <summary>Creates mouse event arguments from native hook parameters.</summary>
    /// <param name="parameter">The hook message parameter.</param>
    /// <param name="data">The hook data pointer.</param>
    /// <returns>The mouse hook event arguments.</returns>
    private static MouseHookEventArgs CreateMouseEventArgs(IntPtr parameter, IntPtr data)
    {
        MouseLowLevelHookStruct mouseLowLevelHookStruct = Marshal.PtrToStructure<MouseLowLevelHookStruct>(data);
        return new MouseHookEventArgs { WindowsMessage = (WindowsMessages)checked((uint)parameter.ToInt32()), Point = mouseLowLevelHookStruct.Pt };
    }
}
