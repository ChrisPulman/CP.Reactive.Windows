// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Keyboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;
#endif
/// <summary>Extensions to assist with the Keyboard Hooks.</summary>
public static class KeyboardHookExtensions
{
    extension(IObservable<KeyboardHookEventArgs> keyboardEvents)
    {
        /// <summary>Filter the KeyboardHookEventArgs with a IKeyboardHookEventHandler.</summary>
        /// <param name="keyboardHookEventHandler">IKeyboardHookEventHandler.</param>
        /// <returns>IObservable with the KeyboardHookEventArgs which was handled.</returns>
        public IObservable<KeyboardHookEventArgs> Where(IKeyboardHookEventHandler keyboardHookEventHandler) => keyboardEvents.Where(keyboardHookEventHandler.Handle);
    }
}
