// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Keyboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;
#endif
/// <summary>This is an IKeyboardHookEventHandler which can handle sequences of key combinations.</summary>
public class KeySequenceHandler : IKeyboardHookEventHandler
{
    /// <summary>The handlers that make up the sequence.</summary>
    private IKeyboardHookEventHandler[] _keyboardHookEventHandlers;

    /// <summary>The handled state for each handler in the sequence.</summary>
    private bool[] _isHandled;

    /// <summary>The active handler offset.</summary>
    private int _offset;

    /// <summary>The time after which the active sequence expires.</summary>
    private DateTimeOffset? _expireAfter;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard.KeySequenceHandler" /> class.</summary>
    /// <param name="keyCombinations">IEnumerable with KeyCombinationHandler.</param>
    public KeySequenceHandler(IEnumerable<IKeyboardHookEventHandler> keyCombinations)
    {
        Configure(keyCombinations);
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard.KeySequenceHandler" /> class.</summary>
    /// <param name="keyCombinations">params with KeyCombinationHandler.</param>
    public KeySequenceHandler(params IKeyboardHookEventHandler[] keyCombinations)
    {
        Configure(keyCombinations);
    }

    /// <inheritdoc />
    public bool HasKeysPressed => CurrentHandler.HasKeysPressed;

    /// <summary>
    /// Gets or sets this sets the timeout time between key presses.
    /// If the user waits longer than this TimeSpan, the sequence is reset to the start.
    /// </summary>
    public TimeSpan? Timeout { get; set; } = TimeSpan.FromSeconds(1.0);

    /// <summary>Gets get the current handler.</summary>
    private IKeyboardHookEventHandler CurrentHandler => _keyboardHookEventHandlers[_offset];

    /// <summary>Private method to configure the fields.</summary>
    /// <param name="keyCombinations">IEnumerable of IKeyboardHookEventHandler.</param>
    public void Configure(IEnumerable<IKeyboardHookEventHandler> keyCombinations)
    {
        _keyboardHookEventHandlers = CopyHandlers(keyCombinations);
        _isHandled = new bool[_keyboardHookEventHandlers.Length];
        _offset = 0;
        _expireAfter = null;
    }

    /// <summary>Check if the combinations are pressed.</summary>
    /// <param name="keyboardHookEventArgs">KeyboardHookEventArgs.</param>
    /// <returns><see langword="true" /> when the sequence was handled.</returns>
    public bool Handle(KeyboardHookEventArgs keyboardHookEventArgs)
    {
        bool currentHandled = CurrentHandler.Handle(keyboardHookEventArgs);
        bool currentNotPressed = !CurrentHandler.HasKeysPressed;
        if (currentHandled)
        {
            _isHandled[_offset] = true;
        }
        else if (((!keyboardHookEventArgs.IsKeyDown && _offset > 0) && currentNotPressed) && !keyboardHookEventArgs.IsModifier)
        {
            Reset();
        }

        if (HasSequenceExpired(currentNotPressed))
        {
            return false;
        }

        bool allHandled = Array.TrueForAll(_isHandled, static (b) => b);
        AdvanceOrReset(currentNotPressed);
        return currentHandled && allHandled;
    }

    /// <summary>Copies the supplied handlers to an array.</summary>
    /// <param name="handlers">The handlers to copy.</param>
    /// <returns>An array containing the handlers.</returns>
    private static IKeyboardHookEventHandler[] CopyHandlers(IEnumerable<IKeyboardHookEventHandler> handlers)
    {
        List<IKeyboardHookEventHandler> copy = new();
        foreach (IKeyboardHookEventHandler handler in handlers)
        {
            copy.Add(handler);
        }

        return copy.ToArray();
    }

    /// <summary>This does a reset of the offset.</summary>
    private void Reset()
    {
        _expireAfter = null;
        _offset = 0;
        for (int i = 0; i < _isHandled.Length; i = checked(i + 1))
        {
            _isHandled[i] = false;
        }
    }

    /// <summary>Helper method to advance a handler.</summary>
    /// <returns>bool with true if we advanced.</returns>
    private bool AdvanceHandler()
    {
        if (Timeout.HasValue)
        {
            _expireAfter = TimeProvider.System.GetLocalNow().Add(Timeout.Value);
        }

        checked
        {
            _offset++;
            return _offset < _keyboardHookEventHandlers.Length;
        }
    }

    /// <summary>Returns whether the current sequence has expired.</summary>
    /// <param name="currentNotPressed">A value indicating whether the current handler has no pressed keys.</param>
    /// <returns><see langword="true" /> when the sequence expired.</returns>
    private bool HasSequenceExpired(bool currentNotPressed)
    {
        if (!_expireAfter.HasValue || _expireAfter.Value >= TimeProvider.System.GetLocalNow())
        {
            return false;
        }

        if (currentNotPressed)
        {
            Reset();
        }

        return true;
    }

    /// <summary>Advances to the next handler or resets when the current sequence is complete.</summary>
    /// <param name="currentNotPressed">A value indicating whether the current handler has no pressed keys.</param>
    private void AdvanceOrReset(bool currentNotPressed)
    {
        if ((_isHandled[_offset] && currentNotPressed) && !AdvanceHandler())
        {
            Reset();
        }
    }
}
