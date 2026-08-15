// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using ReactiveUI.Primitives.Disposables;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Keyboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;
#endif
/// <summary>A global keyboard hook using ReactiveUI.Primitives.Reactive.</summary>
public sealed class KeyboardHook
{
    /// <summary>The resolved keyboard state for a hook event.</summary>
    /// <param name="LeftShift">A value indicating whether the left Shift key is pressed.</param>
    /// <param name="RightShift">A value indicating whether the right Shift key is pressed.</param>
    /// <param name="LeftControl">A value indicating whether the left Control key is pressed.</param>
    /// <param name="RightControl">A value indicating whether the right Control key is pressed.</param>
    /// <param name="LeftAlt">A value indicating whether the left Alt key is pressed.</param>
    /// <param name="RightAlt">A value indicating whether the right Alt key is pressed.</param>
    /// <param name="LeftWin">A value indicating whether the left Windows key is pressed.</param>
    /// <param name="RightWin">A value indicating whether the right Windows key is pressed.</param>
    /// <param name="CapsLock">A value indicating whether Caps Lock is active.</param>
    /// <param name="NumLock">A value indicating whether Num Lock is active.</param>
    /// <param name="ScrollLock">A value indicating whether Scroll Lock is active.</param>
    private readonly record struct KeyboardState(bool LeftShift, bool RightShift, bool LeftControl, bool RightControl, bool LeftAlt, bool RightAlt, bool LeftWin, bool RightWin, bool CapsLock, bool NumLock, bool ScrollLock);

    /// <summary>The key down Windows message id.</summary>
    private const int WmKeyDown = 256;

    /// <summary>The system key up Windows message id.</summary>
    private const int WmSysKeyUp = 261;

    /// <summary>The system key down Windows message id.</summary>
    private const int WmSysKeyDown = 260;

    /// <summary>Shared keyboard hook singleton.</summary>
    private static readonly Lazy<KeyboardHook> Singleton = new(() => new KeyboardHook());

    /// <summary>Stores the shared keyboard event stream.</summary>
    private readonly IObservable<KeyboardHookEventArgs> _keyObservable;

    /// <summary>Stores the native hook callback so it cannot be garbage collected while hooked.</summary>
    private LowLevelHookProc _callback;

    /// <summary>Gets the global keyboard hook event stream.</summary>
    public static IObservable<KeyboardHookEventArgs> KeyboardHookEvents => Singleton.Value._keyObservable;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard.KeyboardHook" /> class.</summary>
    private KeyboardHook()
    {
        _keyObservable = ReactiveSignal.CreateSafe(delegate(IObserver<KeyboardHookEventArgs> observer)
        {
            IntPtr hookId = IntPtr.Zero;
            _callback = delegate(int code, IntPtr parameter, IntPtr data)
            {
                if (code >= 0)
                {
                    KeyboardHookEventArgs e = CreateKeyboardEventArgs(parameter, data);
                    observer.OnNext(e);
                    if (e.Handled)
                    {
                        return (IntPtr)1;
                    }
                }

                return NativeHookMethods.CallNextHookEx(hookId, code, parameter, data);
            };
            hookId = NativeHookMethods.SetWindowsHookEx(HookTypes.WH_KEYBOARD_LL, _callback, IntPtr.Zero, 0U);
            return new ActionDisposable(delegate
            {
                _ = NativeHookMethods.UnhookWindowsHookEx(hookId);
                _callback = null;
            });
        }).Publish().RefCount();
    }

    /// <summary>Creates keyboard event arguments from native hook parameters.</summary>
    /// <param name="parameter">The hook message parameter.</param>
    /// <param name="data">The hook data pointer.</param>
    /// <returns>The keyboard hook event arguments.</returns>
    private static KeyboardHookEventArgs CreateKeyboardEventArgs(IntPtr parameter, IntPtr data)
    {
        bool isKeyDown = parameter == (IntPtr)256 || parameter == (IntPtr)260;
        KeyboardLowLevelHookStruct keyboardLowLevelHookStruct = Marshal.PtrToStructure<KeyboardLowLevelHookStruct>(data);
        VirtualKeyCode key = keyboardLowLevelHookStruct.VirtualKeyCode;
        KeyboardState keyState = GetKeyboardState(key, isKeyDown);
        KeyboardHookEventArgs keyEventArgs = new KeyboardHookEventArgs
        {
            TimeStamp = keyboardLowLevelHookStruct.TimeStamp,
            Key = key,
            Flags = keyboardLowLevelHookStruct.Flags,
            IsModifier = key.IsModifier(),
            IsKeyDown = isKeyDown,
            IsLeftShift = keyState.LeftShift,
            IsRightShift = keyState.RightShift,
            IsLeftAlt = keyState.LeftAlt,
            IsRightAlt = keyState.RightAlt,
            IsLeftControl = keyState.LeftControl,
            IsRightControl = keyState.RightControl,
            IsLeftWindows = keyState.LeftWin,
            IsRightWindows = keyState.RightWin,
            IsScrollLockActive = keyState.ScrollLock,
            IsNumLockActive = keyState.NumLock,
            IsCapsLockActive = keyState.CapsLock
        };
        if (!keyEventArgs.IsAlt && (parameter == (IntPtr)260 || parameter == (IntPtr)261))
        {
            keyEventArgs.IsLeftAlt = true;
            keyEventArgs.IsSystemKey = true;
        }

        return keyEventArgs;
    }

    /// <summary>Gets the current keyboard state adjusted with the active hook event.</summary>
    /// <param name="key">The key associated with the active hook event.</param>
    /// <param name="isKeyDown">A value indicating whether the active hook event is a key-down event.</param>
    /// <returns>The resolved keyboard state.</returns>
    private static KeyboardState GetKeyboardState(VirtualKeyCode key, bool isKeyDown)
    {
        KeyboardState state = new(IsKeyPressed(VirtualKeyCode.LeftShift), IsKeyPressed(VirtualKeyCode.RightShift), IsKeyPressed(VirtualKeyCode.LeftControl), IsKeyPressed(VirtualKeyCode.RightControl), IsKeyPressed(VirtualKeyCode.LeftMenu), IsKeyPressed(VirtualKeyCode.RightMenu), IsKeyPressed(VirtualKeyCode.LeftWin), IsKeyPressed(VirtualKeyCode.RightWin), IsLockKeyActive(VirtualKeyCode.Capital), IsLockKeyActive(VirtualKeyCode.NumLock), IsLockKeyActive(VirtualKeyCode.Scroll));
        switch (key)
        {
            case VirtualKeyCode.LeftShift:
                return state with
                {
                    LeftShift = isKeyDown
                };
            case VirtualKeyCode.RightShift:
                return state with
                {
                    RightShift = isKeyDown
                };
            case VirtualKeyCode.LeftControl:
                return state with
                {
                    LeftControl = isKeyDown
                };
            case VirtualKeyCode.RightControl:
                return state with
                {
                    RightControl = isKeyDown
                };
            case VirtualKeyCode.LeftMenu:
                return state with
                {
                    LeftAlt = isKeyDown
                };
            case VirtualKeyCode.RightMenu:
                return state with
                {
                    RightAlt = isKeyDown
                };
            case VirtualKeyCode.LeftWin:
                return state with
                {
                    LeftWin = isKeyDown
                };
            case VirtualKeyCode.RightWin:
                return state with
                {
                    RightWin = isKeyDown
                };
            case VirtualKeyCode.Capital:
                {
                    if (isKeyDown)
                    {
                        return state with
                        {
                            CapsLock = !state.CapsLock
                        };
                    }

                    break;
                }

            case VirtualKeyCode.NumLock:
                {
                    if (isKeyDown)
                    {
                        return state with
                        {
                            NumLock = !state.NumLock
                        };
                    }

                    break;
                }

            case VirtualKeyCode.Scroll:
                {
                    if (isKeyDown)
                    {
                        return state with
                        {
                            ScrollLock = !state.ScrollLock
                        };
                    }

                    break;
                }

            case VirtualKeyCode.None:
                break;
            case VirtualKeyCode.Lbutton:
                break;
            case VirtualKeyCode.Rbutton:
                break;
            case VirtualKeyCode.Cancel:
                break;
            case VirtualKeyCode.Mbutton:
                break;
            case VirtualKeyCode.Xbutton1:
                break;
            case VirtualKeyCode.Xbutton2:
                break;
            case VirtualKeyCode.Back:
                break;
            case VirtualKeyCode.Tab:
                break;
            case VirtualKeyCode.Clear:
                break;
            case VirtualKeyCode.Return:
                break;
            case VirtualKeyCode.Shift:
                break;
            case VirtualKeyCode.Control:
                break;
            case VirtualKeyCode.Menu:
                break;
            case VirtualKeyCode.Pause:
                break;
            case VirtualKeyCode.Kana:
                break;
            case VirtualKeyCode.Junja:
                break;
            case VirtualKeyCode.Final:
                break;
            case VirtualKeyCode.Hanja:
                break;
            case VirtualKeyCode.Escape:
                break;
            case VirtualKeyCode.Convert:
                break;
            case VirtualKeyCode.Nonconvert:
                break;
            case VirtualKeyCode.Accept:
                break;
            case VirtualKeyCode.Modechange:
                break;
            case VirtualKeyCode.Space:
                break;
            case VirtualKeyCode.Prior:
                break;
            case VirtualKeyCode.Next:
                break;
            case VirtualKeyCode.End:
                break;
            case VirtualKeyCode.Home:
                break;
            case VirtualKeyCode.Left:
                break;
            case VirtualKeyCode.Up:
                break;
            case VirtualKeyCode.Right:
                break;
            case VirtualKeyCode.Down:
                break;
            case VirtualKeyCode.Select:
                break;
            case VirtualKeyCode.Print:
                break;
            case VirtualKeyCode.Execute:
                break;
            case VirtualKeyCode.PrintScreen:
                break;
            case VirtualKeyCode.Insert:
                break;
            case VirtualKeyCode.Delete:
                break;
            case VirtualKeyCode.Help:
                break;
            case VirtualKeyCode.Key0:
                break;
            case VirtualKeyCode.Key1:
                break;
            case VirtualKeyCode.Key2:
                break;
            case VirtualKeyCode.Key3:
                break;
            case VirtualKeyCode.Key4:
                break;
            case VirtualKeyCode.Key5:
                break;
            case VirtualKeyCode.Key6:
                break;
            case VirtualKeyCode.Key7:
                break;
            case VirtualKeyCode.Key8:
                break;
            case VirtualKeyCode.Key9:
                break;
            case VirtualKeyCode.KeyA:
                break;
            case VirtualKeyCode.KeyB:
                break;
            case VirtualKeyCode.KeyC:
                break;
            case VirtualKeyCode.KeyD:
                break;
            case VirtualKeyCode.KeyE:
                break;
            case VirtualKeyCode.KeyF:
                break;
            case VirtualKeyCode.KeyG:
                break;
            case VirtualKeyCode.KeyH:
                break;
            case VirtualKeyCode.KeyI:
                break;
            case VirtualKeyCode.KeyJ:
                break;
            case VirtualKeyCode.KeyK:
                break;
            case VirtualKeyCode.KeyL:
                break;
            case VirtualKeyCode.KeyM:
                break;
            case VirtualKeyCode.KeyN:
                break;
            case VirtualKeyCode.KeyO:
                break;
            case VirtualKeyCode.KeyP:
                break;
            case VirtualKeyCode.KeyQ:
                break;
            case VirtualKeyCode.KeyR:
                break;
            case VirtualKeyCode.KeyS:
                break;
            case VirtualKeyCode.KeyT:
                break;
            case VirtualKeyCode.KeyU:
                break;
            case VirtualKeyCode.KeyV:
                break;
            case VirtualKeyCode.KeyW:
                break;
            case VirtualKeyCode.KeyX:
                break;
            case VirtualKeyCode.KeyY:
                break;
            case VirtualKeyCode.KeyZ:
                break;
            case VirtualKeyCode.Apps:
                break;
            case VirtualKeyCode.Sleep:
                break;
            case VirtualKeyCode.Numpad0:
                break;
            case VirtualKeyCode.Numpad1:
                break;
            case VirtualKeyCode.Numpad2:
                break;
            case VirtualKeyCode.Numpad3:
                break;
            case VirtualKeyCode.Numpad4:
                break;
            case VirtualKeyCode.Numpad5:
                break;
            case VirtualKeyCode.Numpad6:
                break;
            case VirtualKeyCode.Numpad7:
                break;
            case VirtualKeyCode.Numpad8:
                break;
            case VirtualKeyCode.Numpad9:
                break;
            case VirtualKeyCode.Multiply:
                break;
            case VirtualKeyCode.Add:
                break;
            case VirtualKeyCode.Separator:
                break;
            case VirtualKeyCode.Subtract:
                break;
            case VirtualKeyCode.Decimal:
                break;
            case VirtualKeyCode.Divide:
                break;
            case VirtualKeyCode.F1:
                break;
            case VirtualKeyCode.F2:
                break;
            case VirtualKeyCode.F3:
                break;
            case VirtualKeyCode.F4:
                break;
            case VirtualKeyCode.F5:
                break;
            case VirtualKeyCode.F6:
                break;
            case VirtualKeyCode.F7:
                break;
            case VirtualKeyCode.F8:
                break;
            case VirtualKeyCode.F9:
                break;
            case VirtualKeyCode.F10:
                break;
            case VirtualKeyCode.F11:
                break;
            case VirtualKeyCode.F12:
                break;
            case VirtualKeyCode.F13:
                break;
            case VirtualKeyCode.F14:
                break;
            case VirtualKeyCode.F15:
                break;
            case VirtualKeyCode.F16:
                break;
            case VirtualKeyCode.F17:
                break;
            case VirtualKeyCode.F18:
                break;
            case VirtualKeyCode.F19:
                break;
            case VirtualKeyCode.F20:
                break;
            case VirtualKeyCode.F21:
                break;
            case VirtualKeyCode.F22:
                break;
            case VirtualKeyCode.F23:
                break;
            case VirtualKeyCode.F24:
                break;
            case VirtualKeyCode.BrowserBack:
                break;
            case VirtualKeyCode.BrowserForward:
                break;
            case VirtualKeyCode.BrowserRefresh:
                break;
            case VirtualKeyCode.BrowserStop:
                break;
            case VirtualKeyCode.BrowserSearch:
                break;
            case VirtualKeyCode.BrowserFavorites:
                break;
            case VirtualKeyCode.BrowserHome:
                break;
            case VirtualKeyCode.VolumeMute:
                break;
            case VirtualKeyCode.VolumeDown:
                break;
            case VirtualKeyCode.VolumeUp:
                break;
            case VirtualKeyCode.MediaNextTrack:
                break;
            case VirtualKeyCode.MediaPrevTrack:
                break;
            case VirtualKeyCode.MediaStop:
                break;
            case VirtualKeyCode.MediaPlayPause:
                break;
            case VirtualKeyCode.LaunchMail:
                break;
            case VirtualKeyCode.LaunchMediaSelect:
                break;
            case VirtualKeyCode.LaunchApp1:
                break;
            case VirtualKeyCode.LaunchApp2:
                break;
            case VirtualKeyCode.Oem1:
                break;
            case VirtualKeyCode.OemPlus:
                break;
            case VirtualKeyCode.OemComma:
                break;
            case VirtualKeyCode.OemMinus:
                break;
            case VirtualKeyCode.OemPeriod:
                break;
            case VirtualKeyCode.Oem2:
                break;
            case VirtualKeyCode.Oem3:
                break;
            case VirtualKeyCode.Oem4:
                break;
            case VirtualKeyCode.Oem5:
                break;
            case VirtualKeyCode.Oem6:
                break;
            case VirtualKeyCode.Oem7:
                break;
            case VirtualKeyCode.Oem8:
                break;
            case VirtualKeyCode.Oem102:
                break;
            case VirtualKeyCode.Processkey:
                break;
            case VirtualKeyCode.Packet:
                break;
            case VirtualKeyCode.Attn:
                break;
            case VirtualKeyCode.Crsel:
                break;
            case VirtualKeyCode.Exsel:
                break;
            case VirtualKeyCode.Ereof:
                break;
            case VirtualKeyCode.Play:
                break;
            case VirtualKeyCode.Zoom:
                break;
            case VirtualKeyCode.Noname:
                break;
            case VirtualKeyCode.Pa1:
                break;
            case VirtualKeyCode.OemClear:
                break;
        }
        return state;
    }

    /// <summary>Gets a value indicating whether the key is currently pressed.</summary>
    /// <param name="keyCode">The virtual key code.</param>
    /// <returns><see langword="true" /> when the key is pressed.</returns>
    private static bool IsKeyPressed(VirtualKeyCode keyCode) => (NativeHookMethods.GetAsyncKeyState(keyCode) & 0x8000) != 0;

    /// <summary>Gets a value indicating whether the lock key is active.</summary>
    /// <param name="keyCode">The virtual key code.</param>
    /// <returns><see langword="true" /> when the lock key is active.</returns>
    private static bool IsLockKeyActive(VirtualKeyCode keyCode) => (NativeHookMethods.GetKeyState(keyCode) & 1) != 0;
}
