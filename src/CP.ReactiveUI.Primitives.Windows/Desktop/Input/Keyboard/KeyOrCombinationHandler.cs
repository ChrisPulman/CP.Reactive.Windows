// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Keyboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;
#endif
/// <summary>
/// This is an IKeyboardHookEventHandler which checks multiple IKeyboardHookEventHandler.
/// Handle returns true if one can handle the key press.
/// </summary>
public class KeyOrCombinationHandler : IKeyboardHookEventHandler
{
    /// <summary>The handlers that are evaluated for each keyboard event.</summary>
    private readonly IKeyboardHookEventHandler[] _keyCombinations;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard.KeyOrCombinationHandler" /> class.</summary>
    /// <param name="keyCombinations">IEnumerable with KeyCombinationHandler.</param>
    public KeyOrCombinationHandler(IEnumerable<IKeyboardHookEventHandler> keyCombinations)
    {
        _keyCombinations = CopyHandlers(keyCombinations);
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard.KeyOrCombinationHandler" /> class.</summary>
    /// <param name="keyCombinations">params with KeyCombinationHandler.</param>
    public KeyOrCombinationHandler(params IKeyboardHookEventHandler[] keyCombinations)
    {
        _keyCombinations = CopyHandlers(keyCombinations);
    }

    /// <inheritdoc />
    public bool HasKeysPressed => Array.Exists(_keyCombinations, static (handler) => handler.HasKeysPressed);

    /// <summary>Check if the combinations are pressed.</summary>
    /// <param name="keyboardHookEventArgs">KeyboardHookEventArgs.</param>
    /// <returns><see langword="true" /> when any handler handled the event.</returns>
    public bool Handle(KeyboardHookEventArgs keyboardHookEventArgs)
    {
        var handled = false;
        var keyCombinations = _keyCombinations;
        foreach (var keyboardHookEventHandler in keyCombinations)
        {
            handled |= keyboardHookEventHandler.Handle(keyboardHookEventArgs);
        }

        return handled;
    }

    /// <summary>Copies the supplied handlers to an array.</summary>
    /// <param name="handlers">The handlers to copy.</param>
    /// <returns>An array containing the handlers.</returns>
    private static IKeyboardHookEventHandler[] CopyHandlers(IEnumerable<IKeyboardHookEventHandler> handlers)
    {
        List<IKeyboardHookEventHandler> copy = new();
        foreach (var handler in handlers)
        {
            copy.Add(handler);
        }

        return copy.ToArray();
    }
}
