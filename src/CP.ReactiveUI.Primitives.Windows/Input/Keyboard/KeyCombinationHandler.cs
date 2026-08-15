// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Keyboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;
#endif
/// <summary>This is an IKeyboardHookEventHandler which can handle a combination of VirtualKeyCode presses.</summary>
public class KeyCombinationHandler : IKeyboardHookEventHandler
{
    /// <summary>An array with all the current available keys, the locations represent the TriggerCombination array.</summary>
    private bool[] _availableKeys;

    /// <summary>The number of configured combination keys that are currently pressed.</summary>
    private int _pressedCombinationKeyCount;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard.KeyCombinationHandler" /> class.</summary>
    /// <param name="keyCombination">IEnumerable with VirtualKeyCodes.</param>
    public KeyCombinationHandler(IEnumerable<VirtualKeyCode> keyCombination)
    {
        Configure(keyCombination);
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard.KeyCombinationHandler" /> class.</summary>
    /// <param name="keyCombination">params with VirtualKeyCodes.</param>
    public KeyCombinationHandler(params VirtualKeyCode[] keyCombination)
    {
        Configure(keyCombination);
    }

    /// <summary>Gets get the VirtualKeyCodes which trigger the combination.</summary>
    public VirtualKeyCode[] TriggerCombination { get; private set; }

    /// <summary>Gets or sets defines if repeats are allowed, default is false.</summary>
    public bool CanRepeat { get; set; }

    /// <summary>Gets or sets defines if generated (injected) key presses need to be ignored, By default (true) only "real" key presses are handled.</summary>
    public bool IgnoreInjected { get; set; } = true;

    /// <summary>
    /// Gets or sets defines if the key press needs to be passed through to other applications.
    /// By default (false) a keypress which is specified is marked as handled and will not be seen by others.
    /// </summary>
    public bool IsPassThrough { get; set; }

    /// <summary>
    /// Gets or sets defines if the handler should trigger when all keys are released instead of when all keys are pressed.
    /// By default (false) the handler triggers when all keys in the combination are down.
    /// When true, the handler triggers when the first key of the combination is released (key up),
    /// but only if all keys were previously pressed together and no other keys are pressed.
    /// This is useful when you need to inject keypresses after the user has released the modifier keys
    /// to avoid interference from still-pressed modifier keys.
    /// </summary>
    public bool TriggerOnKeyUp { get; set; }

    /// <inheritdoc />
    public bool HasKeysPressed => OtherPressedKeys.Count > 0 || _pressedCombinationKeyCount > 0;

    /// <summary>Gets the keys that do not makeup the combination, where there are any Handle cannot return true.</summary>
    protected ISet<VirtualKeyCode> OtherPressedKeys { get; } = new HashSet<VirtualKeyCode>();

    /// <summary>Configure the key combinations.</summary>
    /// <param name="keyCombination">IEnumerable of VirtualKeyCode.</param>
    public void Configure(IEnumerable<VirtualKeyCode> keyCombination)
    {
        TriggerCombination = DistinctKeys(keyCombination);
        _availableKeys = new bool[TriggerCombination.Length];
        _pressedCombinationKeyCount = 0;
        OtherPressedKeys.Clear();
    }

    /// <summary>Processes a keyboard hook event against this combination.</summary>
    /// <param name="keyboardHookEventArgs">The keyboard hook event arguments.</param>
    /// <returns><see langword="true" /> when the key combination was handled.</returns>
    public virtual bool Handle(KeyboardHookEventArgs keyboardHookEventArgs)
    {
        if (IgnoreInjected && keyboardHookEventArgs.IsInjectedByProcess)
        {
            return false;
        }

        bool wasAllKeysDown = _pressedCombinationKeyCount == TriggerCombination.Length;
        bool keyMatched = TryUpdateMatchedKey(keyboardHookEventArgs, out var isRepeat);
        if (!keyMatched)
        {
            TrackOtherKey(keyboardHookEventArgs);
        }

        bool isHandled = IsCombinationHandled(keyboardHookEventArgs, keyMatched, wasAllKeysDown);
        if (isHandled && !IsPassThrough)
        {
            keyboardHookEventArgs.Handled = true;
        }

        return (CanRepeat || !isRepeat) && isHandled;
    }

    /// <summary>Compares two virtual key codes.</summary>
    /// <param name="current">The event virtual key code.</param>
    /// <param name="expected">The configured virtual key code.</param>
    /// <returns><see langword="true" /> when the current key matches the expected key.</returns>
    protected virtual bool CompareVirtualKeyCode(VirtualKeyCode current, VirtualKeyCode expected) =>
        current == expected
        || expected switch
        {
            VirtualKeyCode.Shift => current is VirtualKeyCode.LeftShift or VirtualKeyCode.RightShift,
            VirtualKeyCode.Control => current is VirtualKeyCode.LeftControl or VirtualKeyCode.RightControl,
            VirtualKeyCode.Menu => current is VirtualKeyCode.LeftMenu or VirtualKeyCode.RightMenu,
            _ => false,
        };

    /// <summary>Returns the distinct keys from the supplied key combination.</summary>
    /// <param name="keyCombination">The key combination to inspect.</param>
    /// <returns>The distinct keys.</returns>
    private static VirtualKeyCode[] DistinctKeys(IEnumerable<VirtualKeyCode> keyCombination)
    {
        List<VirtualKeyCode> keys = new();
        foreach (VirtualKeyCode key in keyCombination)
        {
            if (!keys.Contains(key))
            {
                keys.Add(key);
            }
        }

        return keys.ToArray();
    }

    /// <summary>Updates the tracked state for a matching key.</summary>
    /// <param name="keyboardHookEventArgs">The keyboard hook event arguments.</param>
    /// <param name="isRepeat">A value indicating whether the event repeats an already pressed key.</param>
    /// <returns><see langword="true" /> when the event key matched the configured combination.</returns>
    private bool TryUpdateMatchedKey(KeyboardHookEventArgs keyboardHookEventArgs, out bool isRepeat)
    {
        isRepeat = false;
        for (int i = 0; i < TriggerCombination.Length; i = checked(i + 1))
        {
            if (CompareVirtualKeyCode(keyboardHookEventArgs.Key, TriggerCombination[i]))
            {
                isRepeat = keyboardHookEventArgs.IsKeyDown && _availableKeys[i];
                UpdatePressedCombinationKeyCount(i, keyboardHookEventArgs.IsKeyDown);
                _availableKeys[i] = keyboardHookEventArgs.IsKeyDown;
                return true;
            }
        }

        return false;
    }

    /// <summary>Tracks a key that is not part of this combination.</summary>
    /// <param name="keyboardHookEventArgs">The keyboard hook event arguments.</param>
    private void TrackOtherKey(KeyboardHookEventArgs keyboardHookEventArgs)
    {
        if (keyboardHookEventArgs.IsKeyDown)
        {
            _ = OtherPressedKeys.Add(keyboardHookEventArgs.Key);
        }
        else
        {
            _ = OtherPressedKeys.Remove(keyboardHookEventArgs.Key);
        }
    }

    /// <summary>Determines whether the current key event completes the configured combination.</summary>
    /// <param name="keyboardHookEventArgs">The keyboard hook event arguments.</param>
    /// <param name="keyMatched">A value indicating whether the event key is part of this combination.</param>
    /// <param name="wasAllKeysDown">A value indicating whether all combination keys were down before this event.</param>
    /// <returns><see langword="true" /> when this event handles the combination.</returns>
    private bool IsCombinationHandled(KeyboardHookEventArgs keyboardHookEventArgs, bool keyMatched, bool wasAllKeysDown) =>
        OtherPressedKeys.Count == 0
        && (TriggerOnKeyUp
            ? !keyboardHookEventArgs.IsKeyDown && keyMatched && wasAllKeysDown
            : keyboardHookEventArgs.IsKeyDown && _pressedCombinationKeyCount == TriggerCombination.Length);

    /// <summary>Updates the count of configured keys currently pressed.</summary>
    /// <param name="keyIndex">The configured key index.</param>
    /// <param name="isPressed">A value indicating whether the key is currently pressed.</param>
    private void UpdatePressedCombinationKeyCount(int keyIndex, bool isPressed)
    {
        checked
        {
            if (_availableKeys[keyIndex] != isPressed)
            {
                _pressedCombinationKeyCount += (isPressed ? 1 : (-1));
            }
        }
    }
}
