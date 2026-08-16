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
    /// <summary>The key down Windows message id.</summary>
    private const int WmKeyDown = 256;

    /// <summary>The system key up Windows message id.</summary>
    private const int WmSysKeyUp = 261;

    /// <summary>The system key down Windows message id.</summary>
    private const int WmSysKeyDown = 260;

    /// <summary>Shared keyboard hook singleton.</summary>
    private static readonly Lazy<KeyboardHook> Singleton = new(static () => new KeyboardHook());

    /// <summary>The key down Windows message pointer.</summary>
    private static readonly IntPtr WmKeyDownParameter = (IntPtr)WmKeyDown;

    /// <summary>The system key down Windows message pointer.</summary>
    private static readonly IntPtr WmSysKeyDownParameter = (IntPtr)WmSysKeyDown;

    /// <summary>The system key up Windows message pointer.</summary>
    private static readonly IntPtr WmSysKeyUpParameter = (IntPtr)WmSysKeyUp;

    /// <summary>Stores the shared keyboard event stream.</summary>
    private readonly IObservable<KeyboardHookEventArgs> _keyObservable;

    /// <summary>Stores the native hook callback so it cannot be garbage collected while hooked.</summary>
    private LowLevelHookProc _callback;

    /// <summary>Initializes a new instance of the <see cref="KeyboardHook" /> class.</summary>
    private KeyboardHook()
    {
        _keyObservable = ReactiveSignal.CreateSafe<KeyboardHookEventArgs>(CreateSubscription).Publish().RefCount();
    }

    /// <summary>Gets the global keyboard hook event stream.</summary>
    public static IObservable<KeyboardHookEventArgs> KeyboardHookEvents => Singleton.Value._keyObservable;

    /// <summary>Creates keyboard event arguments from native hook parameters.</summary>
    /// <param name="parameter">The hook message parameter.</param>
    /// <param name="data">The hook data pointer.</param>
    /// <returns>The keyboard hook event arguments.</returns>
    private static KeyboardHookEventArgs CreateKeyboardEventArgs(IntPtr parameter, IntPtr data)
    {
        bool isKeyDown = parameter == WmKeyDownParameter || parameter == WmSysKeyDownParameter;
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
            IsCapsLockActive = keyState.CapsLock,
        };
        if (!keyEventArgs.IsAlt && (parameter == WmSysKeyDownParameter || parameter == WmSysKeyUpParameter))
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
        KeyboardState state = new(
            IsKeyPressed(VirtualKeyCode.LeftShift),
            IsKeyPressed(VirtualKeyCode.RightShift),
            IsKeyPressed(VirtualKeyCode.LeftControl),
            IsKeyPressed(VirtualKeyCode.RightControl),
            IsKeyPressed(VirtualKeyCode.LeftMenu),
            IsKeyPressed(VirtualKeyCode.RightMenu),
            IsKeyPressed(VirtualKeyCode.LeftWin),
            IsKeyPressed(VirtualKeyCode.RightWin),
            IsLockKeyActive(VirtualKeyCode.Capital),
            IsLockKeyActive(VirtualKeyCode.NumLock),
            IsLockKeyActive(VirtualKeyCode.Scroll));

        return key switch
        {
            VirtualKeyCode.LeftShift => state with { LeftShift = isKeyDown },
            VirtualKeyCode.RightShift => state with { RightShift = isKeyDown },
            VirtualKeyCode.LeftControl => state with { LeftControl = isKeyDown },
            VirtualKeyCode.RightControl => state with { RightControl = isKeyDown },
            VirtualKeyCode.LeftMenu => state with { LeftAlt = isKeyDown },
            VirtualKeyCode.RightMenu => state with { RightAlt = isKeyDown },
            VirtualKeyCode.LeftWin => state with { LeftWin = isKeyDown },
            VirtualKeyCode.RightWin => state with { RightWin = isKeyDown },
            VirtualKeyCode.Capital => state with { CapsLock = isKeyDown ? !state.CapsLock : state.CapsLock },
            VirtualKeyCode.NumLock => state with { NumLock = isKeyDown ? !state.NumLock : state.NumLock },
            VirtualKeyCode.Scroll => state with { ScrollLock = isKeyDown ? !state.ScrollLock : state.ScrollLock },
            _ => state,
        };
    }

    /// <summary>Gets a value indicating whether the key is currently pressed.</summary>
    /// <param name="keyCode">The virtual key code.</param>
    /// <returns><see langword="true" /> when the key is pressed.</returns>
    private static bool IsKeyPressed(VirtualKeyCode keyCode) => (NativeHookMethods.GetAsyncKeyState(keyCode) & 0x8000) != 0;

    /// <summary>Gets a value indicating whether the lock key is active.</summary>
    /// <param name="keyCode">The virtual key code.</param>
    /// <returns><see langword="true" /> when the lock key is active.</returns>
    private static bool IsLockKeyActive(VirtualKeyCode keyCode) => (NativeHookMethods.GetKeyState(keyCode) & 1) != 0;

    /// <summary>Creates the subscription that owns the native keyboard hook.</summary>
    /// <param name="observer">The observer that receives keyboard hook events.</param>
    /// <returns>The hook lifetime.</returns>
    private ActionDisposable CreateSubscription(IObserver<KeyboardHookEventArgs> observer)
    {
        IntPtr hookId = IntPtr.Zero;
        _callback = (code, parameter, data) =>
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
        return new(() =>
        {
            _ = NativeHookMethods.UnhookWindowsHookEx(hookId);
            _callback = null;
        });
    }

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
    private readonly record struct KeyboardState(
        bool LeftShift,
        bool RightShift,
        bool LeftControl,
        bool RightControl,
        bool LeftAlt,
        bool RightAlt,
        bool LeftWin,
        bool RightWin,
        bool CapsLock,
        bool NumLock,
        bool ScrollLock);
}
