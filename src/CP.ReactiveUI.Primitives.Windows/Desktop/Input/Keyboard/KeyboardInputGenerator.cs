// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Keyboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;
#endif
/// <summary>This is a utility class to help to generate input for mouse and keyboard.</summary>
public static class KeyboardInputGenerator
{
    /// <summary>The number of input records required for a key down and key up transition.</summary>
    private const int KeyTransitionInputCount = 2;

    /// <summary>Generate key down.</summary>
    /// <param name="keycodes">VirtualKeyCodes for the key downs.</param>
    /// <returns>number of input events generated.</returns>
    public static uint KeyDown(params VirtualKeyCode[] keycodes)
    {
        if (keycodes.Length == 0)
        {
            return 0U;
        }

        KeyboardInput[] keyboardInputs = new KeyboardInput[keycodes.Length];
        var index = 0;
        foreach (var virtualKeyCode in keycodes)
        {
            keyboardInputs[index] = KeyboardInput.ForKeyDown(virtualKeyCode);
            index = checked(index + 1);
        }

        return NativeInput.SendInput(DesktopInput.CreateKeyboardInputs(keyboardInputs));
    }

    /// <summary>Generate a key combination press(es).</summary>
    /// <param name="keycodes">params VirtualKeyCodes.</param>
    /// <returns>number of input events generated.</returns>
    public static uint KeyCombinationPress(params VirtualKeyCode[] keycodes)
    {
        if (keycodes.Length == 0)
        {
            return 0U;
        }

        checked
        {
            KeyboardInput[] keyboardInputs = new KeyboardInput[keycodes.Length * KeyTransitionInputCount];
            var index = 0;
            var array = keycodes;
            foreach (var virtualKeyCode in array)
            {
                keyboardInputs[index] = KeyboardInput.ForKeyDown(virtualKeyCode);
                index++;
            }

            array = keycodes;
            foreach (var virtualKeyCode2 in array)
            {
                keyboardInputs[index] = KeyboardInput.ForKeyUp(virtualKeyCode2);
                index++;
            }

            return NativeInput.SendInput(DesktopInput.CreateKeyboardInputs(keyboardInputs));
        }
    }

    /// <summary>Generate key press(es).</summary>
    /// <param name="keycodes">params VirtualKeyCodes.</param>
    /// <returns>number of input events generated.</returns>
    public static uint KeyPresses(params VirtualKeyCode[] keycodes)
    {
        if (keycodes.Length == 0)
        {
            return 0U;
        }

        checked
        {
            KeyboardInput[] keyboardInputs = new KeyboardInput[keycodes.Length * KeyTransitionInputCount];
            var index = 0;
            foreach (var virtualKeyCode in keycodes)
            {
                keyboardInputs[index] = KeyboardInput.ForKeyDown(virtualKeyCode);
                index++;
                keyboardInputs[index] = KeyboardInput.ForKeyUp(virtualKeyCode);
                index++;
            }

            return NativeInput.SendInput(DesktopInput.CreateKeyboardInputs(keyboardInputs));
        }
    }

    /// <summary>Generate key(s) up.</summary>
    /// <param name="keycodes">VirtualKeyCodes for the keys to release.</param>
    /// <returns>number of input events generated.</returns>
    public static uint KeyUp(params VirtualKeyCode[] keycodes)
    {
        if (keycodes.Length == 0)
        {
            return 0U;
        }

        KeyboardInput[] keyboardInputs = new KeyboardInput[keycodes.Length];
        var index = 0;
        foreach (var virtualKeyCode in keycodes)
        {
            keyboardInputs[index] = KeyboardInput.ForKeyUp(virtualKeyCode);
            index = checked(index + 1);
        }

        return NativeInput.SendInput(DesktopInput.CreateKeyboardInputs(keyboardInputs));
    }
}
