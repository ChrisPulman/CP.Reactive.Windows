// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Keyboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;
#endif
/// <summary>This can handle KeyboardHookEventArgs, the handle method returns true if the key was handled.</summary>
public interface IKeyboardHookEventHandler
{
    /// <summary>Gets a value indicating whether this event handler currently has keys pressed.</summary>
    bool HasKeysPressed { get; }

    /// <summary>Handle a KeyboardHookEventArgs.</summary>
    /// <param name="keyboardHookEventArgs">The keyboard hook event arguments.</param>
    /// <returns>bool true if handled.</returns>
    bool Handle(KeyboardHookEventArgs keyboardHookEventArgs);
}
