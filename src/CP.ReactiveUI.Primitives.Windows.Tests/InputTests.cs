// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Input Tests behavior.</summary>
public class InputTests
{
    /// <summary>Defines the TestValue10 test value.</summary>
    private const int TestValue10 = 10;

    /// <summary>Defines the TestValue10000 test value.</summary>
    private const int TestValue10000 = 10_000;

    /// <summary>Defines the TestValue100 test value.</summary>
    private const int TestValue100 = 100;

    /// <summary>Defines the TestValue50 test value.</summary>
    private const int TestValue50 = 50;

    /// <summary>Test LastInputTimeSpan.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestInput_LastInputTimeSpanAsync()
    {
        var initialLastInputTimeSpan = NativeInput.LastInputTimeSpan;
        await Task.Delay(TestValue100, CancellationToken.None);
        var laterLastInputTimeSpan = NativeInput.LastInputTimeSpan;
        await Assert.That(laterLastInputTimeSpan > initialLastInputTimeSpan).IsTrue();
    }

    /// <summary>Test LastInputDateTime.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestInput_LastInputDateTimeAsync()
    {
        var initialLastInput = NativeInput.LastInputDateTime;
        await Task.Delay(TestValue50, CancellationToken.None);
        var laterLastInput = NativeInput.LastInputDateTime;
        var deviation = laterLastInput.Subtract(initialLastInput);
        await Assert.That(deviation < TimeSpan.FromMilliseconds(TestValue100)).IsTrue();
    }

    /// <summary>Test typing in a notepad.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestInputAsync()
    {
        // Start a process to test against
        using var process = Process.Start("charmap.exe");

        // Make sure it's started
        await Assert.That(process).IsNotNull();
        if (process is null)
        {
            return;
        }

        try
        {
            // Wait until the process started its message pump (listening for input).
            var processReady = process.WaitForInputIdle(TestValue10000);
            await Assert.That(processReady).IsTrue();
            if (!processReady)
            {
                return;
            }

            _ = User32Api.SetWindowText(process.MainWindowHandle, "TestInput");

            // Find the belonging window.
            var testWindow = InteropWindowFactory.CreateFor(process.MainWindowHandle);
            await Assert.That(testWindow).IsNotNull();

            // Send input.
            var sentInputs = KeyboardInputGenerator.KeyPresses(VirtualKeyCode.KeyR, VirtualKeyCode.KeyO, VirtualKeyCode.KeyB, VirtualKeyCode.KeyI, VirtualKeyCode.KeyN);

            // Test if we sent TestValue10 inputs (5 x down & up).
            await Assert.That((int)sentInputs).IsEqualTo(TestValue10);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
                _ = process.WaitForExit(TestValue10000);
            }
        }
    }

    /// <summary>Test typing in a notepad.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestMouseInputAsync()
    {
        _ = MouseInputGenerator.MoveMouse(new(TestValue10, TestValue10));
        await Task.Delay(TestValue100, CancellationToken.None);
        _ = MouseInputGenerator.MoveMouse(new(TestValue100, TestValue100));
        await Task.Delay(TestValue100, CancellationToken.None);
    }
}
