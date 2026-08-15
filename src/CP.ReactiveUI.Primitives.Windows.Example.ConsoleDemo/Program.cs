// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
using CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;
using CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;

namespace CP.ReactiveUI.Primitives.Windows.Example.ConsoleDemo;

/// <summary>Provides the entry point for the console demonstration.</summary>
internal static class Program
{
    /// <summary>Starts the keyboard-hook demonstration message loop.</summary>
    [STAThread]
    private static void Main()
    {
        var key = new KeyCombinationHandler(VirtualKeyCode.KeyA);
        using (KeyboardHook.KeyboardHookEvents.Where(key).Subscribe(static _ => Hit()))
        {
            MessageLoop.ProcessMessages();
        }
    }

    /// <summary>Records that the configured key combination was pressed.</summary>
    private static void Hit() => System.Diagnostics.Debug.WriteLine("Hit");
}
