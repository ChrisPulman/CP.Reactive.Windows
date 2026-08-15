// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Input Tests behavior.</summary>
public class InputTests
{
    /// <summary>Defines the TestValue10 test value.</summary>
    private const int TestValue10 = 10;

    /// <summary>Defines the TestValue100 test value.</summary>
    private const int TestValue100 = 100;

    /// <summary>Defines the TestValue50 test value.</summary>
    private const int TestValue50 = 50;

    /// <summary>Test LastInputTimeSpan.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestInput_LastInputTimeSpanAsync()
    {
        var nativeApi = new InputCoverage2Tests.FakeNativeInputApi { LastInputResult = true };
        var previousApi = NativeInput.SetApiForTesting(nativeApi);
        try
        {
            var initialLastInputTimeSpan = NativeInput.LastInputTimeSpan;
            await Task.Delay(TestValue100, CancellationToken.None);
            var laterLastInputTimeSpan = NativeInput.LastInputTimeSpan;
            await Assert.That(laterLastInputTimeSpan > initialLastInputTimeSpan).IsTrue();
        }
        finally
        {
            _ = NativeInput.SetApiForTesting(previousApi);
        }
    }

    /// <summary>Test LastInputDateTime.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestInput_LastInputDateTimeAsync()
    {
        var nativeApi = new InputCoverage2Tests.FakeNativeInputApi { LastInputResult = true };
        var previousApi = NativeInput.SetApiForTesting(nativeApi);
        try
        {
            var initialLastInput = NativeInput.LastInputDateTime;
            await Task.Delay(TestValue50, CancellationToken.None);
            var laterLastInput = NativeInput.LastInputDateTime;
            var deviation = laterLastInput.Subtract(initialLastInput);
            await Assert.That(deviation < TimeSpan.FromMilliseconds(TestValue100)).IsTrue();
        }
        finally
        {
            _ = NativeInput.SetApiForTesting(previousApi);
        }
    }

    /// <summary>Tests keyboard input composition without requiring a desktop application.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestInputAsync()
    {
        const uint expectedSendCount = TestValue10;
        var nativeApi = new InputCoverage2Tests.FakeNativeInputApi { SendReturn = expectedSendCount };
        var previousApi = NativeInput.SetApiForTesting(nativeApi);
        try
        {
            var sentInputs = KeyboardInputGenerator.KeyPresses(VirtualKeyCode.KeyR, VirtualKeyCode.KeyO, VirtualKeyCode.KeyB, VirtualKeyCode.KeyI, VirtualKeyCode.KeyN);
            await Assert.That(sentInputs).IsEqualTo(expectedSendCount);
            await Assert.That(nativeApi.SendCalls).IsEqualTo(One);
            await Assert.That(nativeApi.LastInputs.Length).IsEqualTo(TestValue10);
            await Assert.That(nativeApi.LastInputs[0].InputUnion.KeyboardInput.VirtualKeyCode).IsEqualTo(VirtualKeyCode.KeyR);
            await Assert.That(nativeApi.LastInputs[1].InputUnion.KeyboardInput.KeyEventFlags).IsEqualTo(KeyEventFlags.KeyUp);
        }
        finally
        {
            _ = NativeInput.SetApiForTesting(previousApi);
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
