// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Keyboard Hook Tests behavior.</summary>
public class KeyboardHookTests
{
    /// <summary>Defines the TestValue200 test value.</summary>
    private const int TestValue200 = 200;

    /// <summary>Defines the TestValue400 test value.</summary>
    private const int TestValue400 = 400;

    /// <summary>Defines the asynchronous event timeout.</summary>
    private const int TestValue2000 = 2000;

    /// <summary>Defines the TestValue20 test value.</summary>
    private const int TestValue20 = 20;

    /// <summary>Defines the TestValue2 test value.</summary>
    private const int TestValue2 = 2;

    /// <summary>The hook API name expected in missing-entry-point failures.</summary>
    private const string SetWindowsHookExApiName = "SetWindowsHookEx";

    /// <summary>Writes diagnostic messages for these tests.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(KeyboardHookTests));

    /// <summary>Tests Key Handler Single Combination.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHandler_SingleCombinationAsync()
    {
        var pressCount = 0;
        var keyHandler = new KeyCombinationHandler(VirtualKeyCode.Back, VirtualKeyCode.RightShift) { IgnoreInjected = false, IsPassThrough = false, };
        IDisposable subscription;
        try
        {
            subscription = KeyboardHook.KeyboardHookEvents.Where(keyHandler).SubscribeOnNext(keyboardHookEventArgs => pressCount++);
        }
        catch (EntryPointNotFoundException exception)
        {
            await Assert.That(exception.Message).Contains(SetWindowsHookExApiName);
            return;
        }

        using (subscription)
        {
            await Task.Delay(TestValue20);
            _ = KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Back, VirtualKeyCode.RightShift);
            await Task.Delay(TestValue20);
            _ = KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Back, VirtualKeyCode.RightShift);
            await Task.Delay(TestValue20);
        }

        await Assert.That(pressCount == TestValue2).IsTrue();
        _ = KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Back, VirtualKeyCode.RightShift);
        await Task.Delay(TestValue20);
        await Assert.That(pressCount == TestValue2).IsTrue();
    }

    /// <summary>Tests Key Handler Slow Subscriber.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHandler_Slow_SubscriberAsync()
    {
        var pressCount = 0;
        const int pressHandlingTime = 500;

        // Wait 2x press plus overhead
        const int waitForPressHandling = (int)((pressHandlingTime * TestValue2) * 1.1);

        var sequenceHandler = new KeyCombinationHandler(VirtualKeyCode.Shift, VirtualKeyCode.KeyA) { IgnoreInjected = false };

        IDisposable subscription;
        try
        {
            subscription = KeyboardHook.KeyboardHookEvents.Where(sequenceHandler).SubscribeOnNext(keyboardHookEventArgs =>
            {
                Log.Info("Key combination was pressed, slow handling!");
                Log.Info("Key combination was pressed, finished!");
                pressCount++;
            });
        }
        catch (EntryPointNotFoundException exception)
        {
            await Assert.That(exception.Message).Contains(SetWindowsHookExApiName);
            return;
        }

        using (subscription)
        {
            Log.Info("Pressing key combination");
            _ = KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Shift, VirtualKeyCode.KeyA);
            Log.Info("Pressed key combination");
            Log.Info("Pressing key combination");
            _ = KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Shift, VirtualKeyCode.KeyA);
            Log.Info("Pressed key combination");
            await Task.Delay(waitForPressHandling);
            await Assert.That(pressCount).IsEqualTo(TestValue2);
        }
    }

    /// <summary>Tests Key Handler Sequence Input Generator.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHandler_Sequence_InputGeneratorAsync()
    {
        var pressCount = 0;
        var sequenceHandler = new KeySequenceHandler(
            new KeyCombinationHandler(VirtualKeyCode.Print) { IgnoreInjected = false },
            new KeyCombinationHandler(VirtualKeyCode.Shift, VirtualKeyCode.KeyA) { IgnoreInjected = false });

        IDisposable subscription;
        try
        {
            subscription = KeyboardHook.KeyboardHookEvents.Where(sequenceHandler).SubscribeOnNext(keyboardHookEventArgs => pressCount++);
        }
        catch (EntryPointNotFoundException exception)
        {
            await Assert.That(exception.Message).Contains(SetWindowsHookExApiName);
            return;
        }

        using (subscription)
        {
            await Task.Delay(TestValue20);
            _ = KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Print);
            await Task.Delay(TestValue20);
            await Assert.That(pressCount == 0).IsTrue();
            _ = KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Shift, VirtualKeyCode.KeyB);
            await Task.Delay(TestValue20);
            await Assert.That(pressCount == 0).IsTrue();
            _ = KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Print);
            await Task.Delay(TestValue20);
            _ = KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Shift, VirtualKeyCode.KeyA);
            await Task.Delay(TestValue20);
            await Assert.That(pressCount == 1).IsTrue();
        }
    }

    /// <summary>Tests Key Handler Key Sequence Handler Wrong Right.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHandler_KeySequenceHandler_Wrong_RightAsync()
    {
        var sequenceHandler = new KeySequenceHandler(
            new KeyCombinationHandler(VirtualKeyCode.Print),
            new KeyCombinationHandler(VirtualKeyCode.Shift, VirtualKeyCode.KeyA))
        {
            // Prevent debug issues, we are not testing the timeout here!
            Timeout = null,
        };

        var result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Print));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Shift));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyB));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyB));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Shift));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Print));
        await Assert.That(result).IsFalse();

        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Shift));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyA));
        await Assert.That(result).IsTrue();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyA));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Shift));
        await Assert.That(result).IsFalse();
        await Assert.That(sequenceHandler.HasKeysPressed).IsFalse();
    }

    /// <summary>Tests Key Handler Key Sequence Handler Right.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHandler_KeySequenceHandler_RightAsync()
    {
        var sequenceHandler = new KeySequenceHandler(
            new KeyCombinationHandler(VirtualKeyCode.Print),
            new KeyCombinationHandler(VirtualKeyCode.Shift, VirtualKeyCode.KeyA))
        { Timeout = null, };

        var result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Print));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Print));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Control));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Control));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Control));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Shift));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyA));
        await Assert.That(result).IsTrue();
    }

    /// <summary>Tests Key Handler Key Combination Handler Repeat.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHandler_KeyCombinationHandler_RepeatAsync()
    {
        var keyCombinationHandler = new KeyCombinationHandler(VirtualKeyCode.Print);

        var keyPrintDown = KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Print);

        var keyPrintUp = KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Print);

        var result = keyCombinationHandler.Handle(keyPrintDown);
        await Assert.That(result).IsTrue();
        result = keyCombinationHandler.Handle(keyPrintDown);
        await Assert.That(result).IsFalse();

        // Key up again
        result = keyCombinationHandler.Handle(keyPrintUp);
        await Assert.That(result).IsFalse();
        result = keyCombinationHandler.Handle(keyPrintDown);
        await Assert.That(result).IsTrue();
    }

    /// <summary>Test that after a key down, having a not matching key down and up we should not have a "hit".</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHandler_KeyCombinationHandler_KeyUpAsync()
    {
        var keyCombinationHandler = new KeyCombinationHandler(VirtualKeyCode.Print);

        var result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Print));
        await Assert.That(result).IsTrue();
        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Control));
        await Assert.That(result).IsFalse();
        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Control));
        await Assert.That(result).IsFalse();
    }

    /// <summary>Tests Key Handler Sequence With Optional Keys Keyboard Input Generator.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHandler_SequenceWithOptionalKeys_KeyboardInputGeneratorAsync()
    {
        var pressCount = 0;
        var sequenceHandler = new KeySequenceHandler(
            new KeyCombinationHandler(VirtualKeyCode.Print) { IgnoreInjected = false },
            new KeyOrCombinationHandler(
                new KeyCombinationHandler(VirtualKeyCode.Shift, VirtualKeyCode.KeyA) { IgnoreInjected = false },
                new KeyCombinationHandler(VirtualKeyCode.Shift, VirtualKeyCode.KeyB) { IgnoreInjected = false }))
        {
            // Timeout for test
            Timeout = TimeSpan.FromMilliseconds(TestValue200),
        };

        IDisposable subscription;
        try
        {
            subscription = KeyboardHook.KeyboardHookEvents.Where(sequenceHandler).SubscribeOnNext(keyboardHookEventArgs => pressCount++);
        }
        catch (EntryPointNotFoundException exception)
        {
            await Assert.That(exception.Message).Contains(SetWindowsHookExApiName);
            return;
        }

        using (subscription)
        {
            await Task.Delay(TestValue20);
            _ = KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Print);
            await Task.Delay(TestValue20);
            await Assert.That(pressCount).IsEqualTo(0);
            _ = KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Shift, VirtualKeyCode.KeyB);
            await Task.Delay(TestValue20);
            await Assert.That(pressCount).IsEqualTo(1);
            _ = KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Print);
            await Task.Delay(TestValue20);
            _ = KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Shift, VirtualKeyCode.KeyA);
            await Task.Delay(TestValue20);
            await Assert.That(pressCount).IsEqualTo(TestValue2);

            // Test with timeout, waiting to long
            _ = KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Print);
            await Task.Delay(TestValue400);
            _ = KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Shift, VirtualKeyCode.KeyA);
            await Task.Delay(TestValue20);
            await Assert.That(pressCount).IsEqualTo(TestValue2);
        }
    }

    /// <summary>Tests Key Handler Sequence With Optional Keys Timeout.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHandler_SequenceWithOptionalKeys_TimeoutAsync()
    {
        var sequenceHandler = new KeySequenceHandler(
            new KeyCombinationHandler(VirtualKeyCode.Print),
            new KeyOrCombinationHandler(
                new KeyCombinationHandler(VirtualKeyCode.Shift, VirtualKeyCode.KeyA),
                new KeyCombinationHandler(VirtualKeyCode.Shift, VirtualKeyCode.KeyB)))
        { Timeout = TimeSpan.FromMilliseconds(TestValue200), };

        var result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Print));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Print));
        await Assert.That(result).IsFalse();

        // Shift KeyB
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Shift));
        await Assert.That(sequenceHandler.HasKeysPressed).IsTrue();
        await Assert.That(result).IsFalse();
        await Task.Delay(TestValue400, CancellationToken.None);
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyB));
        await Assert.That(sequenceHandler.HasKeysPressed).IsTrue();
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyB));
        await Assert.That(sequenceHandler.HasKeysPressed).IsTrue();
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Shift));
        await Assert.That(sequenceHandler.HasKeysPressed).IsFalse();
        await Assert.That(result).IsFalse();
    }

    /// <summary>Tests Key Handler Sequence With Optional Keys One Try.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHandler_SequenceWithOptionalKeys_OneTryAsync()
    {
        var sequenceHandler = new KeySequenceHandler(
            new KeyCombinationHandler(VirtualKeyCode.Print),
            new KeyOrCombinationHandler(
                new KeyCombinationHandler(VirtualKeyCode.Shift, VirtualKeyCode.KeyA),
                new KeyCombinationHandler(VirtualKeyCode.Shift, VirtualKeyCode.KeyB)))
        { Timeout = null, };

        var result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Print));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Print));
        await Assert.That(result).IsFalse();

        // Shift KeyB
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Shift));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyB));
        await Assert.That(result).IsTrue();

        // Shift KeyB
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyB));
        await Assert.That(result).IsFalse();
        result = sequenceHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Shift));
        await Assert.That(result).IsFalse();
    }

    /// <summary>Tests Key Helper Virtual Key Codes From String.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHelper_VirtualKeyCodesFromStringAsync()
    {
        const string testKeys = "ctrl + shift + A";
        var virtualKeyCodes = ToList(KeyHelper.VirtualKeyCodesFromString(testKeys));
        await Assert.That(virtualKeyCodes).IsNotEmpty();
        await Assert.That(virtualKeyCodes).Contains(VirtualKeyCode.Shift);
        await Assert.That(virtualKeyCodes).Contains(VirtualKeyCode.Control);
        await Assert.That(virtualKeyCodes).Contains(VirtualKeyCode.KeyA);
    }

    /// <summary>Tests Key Helper Virtual Code To Locale Display Text.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHelper_VirtualCodeToLocaleDisplayTextAsync()
    {
        var keyDisplayTexts = new[]
        {
            KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.LeftShift, false),
            KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.KeyA, false),
        };
        var keyCombination = string.Join(" + ", keyDisplayTexts);

        await Assert.That(keyCombination).IsNotEmpty();
        await Assert.That(keyCombination).Contains("+ A");
    }

    /// <summary>Tests Handling Key.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestHandlingKeyAsync()
    {
        var args = KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyA);
        args.IsLeftWindows = true;
        args.IsLeftShift = true;
        args.IsLeftControl = true;
        args.IsLeftAlt = true;

        if (args.IsWindows && args.IsShift && args.IsControl && args.IsAlt)
        {
            args.Handled = true;
        }

        await Assert.That(args.Handled).IsTrue();
    }

    /// <summary>Tests Mapping.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestMappingAsync()
    {
        var observed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        IDisposable subscription;
        try
        {
            subscription = KeyboardHook.KeyboardHookEvents
                .Where(static info => info.IsLeftShift && info.IsKeyDown)
                .SubscribeOnNext(_ => observed.TrySetResult(true));
        }
        catch (EntryPointNotFoundException exception)
        {
            await Assert.That(exception.Message).Contains(SetWindowsHookExApiName);
            return;
        }

        using (subscription)
        {
            _ = KeyboardInputGenerator.KeyPresses(VirtualKeyCode.LeftShift);

            var completedTask = await Task.WhenAny(observed.Task, Task.Delay(TestValue2000));
            await Assert.That(ReferenceEquals(completedTask, observed.Task)).IsTrue();
            await Assert.That(await observed.Task).IsTrue();
        }
    }

    /// <summary>Tests Suppress Volume.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestSuppressVolumeAsync()
    {
        var volumeArgs = KeyboardHookEventArgs.KeyDown(VirtualKeyCode.VolumeUp);
        var shouldPassVolume = HandleSuppressVolume(volumeArgs);
        await Assert.That(shouldPassVolume).IsFalse();
        await Assert.That(volumeArgs.Handled).IsTrue();

        var otherArgs = KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyA);
        var shouldPassOtherKey = HandleSuppressVolume(otherArgs);
        await Assert.That(shouldPassOtherKey).IsTrue();
        await Assert.That(otherArgs.Handled).IsFalse();
    }

    /// <summary>Test that TriggerOnKeyUp triggers when all keys are released, not when pressed.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHandler_KeyCombinationHandler_TriggerOnKeyUp_SingleKeyAsync()
    {
        var keyCombinationHandler = new KeyCombinationHandler(VirtualKeyCode.Print) { TriggerOnKeyUp = true, };

        // Key down should not trigger
        var result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Print));
        await Assert.That(result).IsFalse();

        // Key up should trigger
        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Print));
        await Assert.That(result).IsTrue();
    }

    /// <summary>Test that TriggerOnKeyUp works correctly with key combinations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHandler_KeyCombinationHandler_TriggerOnKeyUp_CombinationAsync()
    {
        var keyCombinationHandler = new KeyCombinationHandler(VirtualKeyCode.Control, VirtualKeyCode.Shift, VirtualKeyCode.KeyA) { TriggerOnKeyUp = true, };

        // Press all keys in the combination
        var result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Control));
        await Assert.That(result).IsFalse();

        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Shift));
        await Assert.That(result).IsFalse();

        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyA));
        await Assert.That(result).IsFalse(); // Should not trigger on key down

        // Start releasing keys - should trigger on first key up
        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyA));
        await Assert.That(result).IsTrue();

        // Further releases should not trigger
        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Shift));
        await Assert.That(result).IsFalse();

        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Control));
        await Assert.That(result).IsFalse();
    }

    /// <summary>Test that TriggerOnKeyUp does not trigger if an extra key was pressed.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHandler_KeyCombinationHandler_TriggerOnKeyUp_WithExtraKeyAsync()
    {
        var keyCombinationHandler = new KeyCombinationHandler(VirtualKeyCode.Control, VirtualKeyCode.KeyA) { TriggerOnKeyUp = true, };

        // Press the combination keys
        var result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Control));
        await Assert.That(result).IsFalse();

        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyA));
        await Assert.That(result).IsFalse();

        // Press an extra key
        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyB));
        await Assert.That(result).IsFalse();

        // Release combination key - should not trigger because extra key is pressed
        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyA));
        await Assert.That(result).IsFalse();

        // Release extra key
        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyB));
        await Assert.That(result).IsFalse();

        // Release last combination key
        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Control));
        await Assert.That(result).IsFalse();
    }

    /// <summary>Test that TriggerOnKeyUp does not trigger if not all combination keys were pressed.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKeyHandler_KeyCombinationHandler_TriggerOnKeyUp_PartialPressAsync()
    {
        var keyCombinationHandler = new KeyCombinationHandler(VirtualKeyCode.Control, VirtualKeyCode.Shift, VirtualKeyCode.KeyA) { TriggerOnKeyUp = true, };

        // Press only some keys
        var result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.Control));
        await Assert.That(result).IsFalse();

        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyA));
        await Assert.That(result).IsFalse();

        // Release a key without having pressed Shift - should not trigger
        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyA));
        await Assert.That(result).IsFalse();

        result = keyCombinationHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.Control));
        await Assert.That(result).IsFalse();
    }

    /// <summary>Creates a list from the supplied values without relying on LINQ materialization.</summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="values">The values to materialize.</param>
    /// <returns>The materialized list.</returns>
    private static List<T> ToList<T>(IEnumerable<T> values)
    {
        var result = new List<T>();
        foreach (var value in values)
        {
            result.Add(value);
        }

        return result;
    }

    /// <summary>Marks volume keys as handled so they are suppressed.</summary>
    /// <param name="args">The keyboard hook event arguments.</param>
    /// <returns><see langword="false"/> for suppressed volume keys; otherwise, <see langword="true"/>.</returns>
    private static bool HandleSuppressVolume(KeyboardHookEventArgs args)
    {
        if (args.Key is VirtualKeyCode.VolumeDown or VirtualKeyCode.VolumeMute or VirtualKeyCode.VolumeUp)
        {
            args.Handled = true;
            return false;
        }

        return true;
    }
}
