// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Final deterministic coverage for the remaining Windows device, input, and power paths.</summary>
public sealed class CoverageFinalReleaseWindowsMiscTests
{
    /// <summary>Defines the deterministic waitable-timer handle value.</summary>
    private const int TimerHandleValue = 123;

    /// <summary>Defines the deterministic waitable-timer handle.</summary>
    private static readonly IntPtr TimerHandle = new(TimerHandleValue);

    /// <summary>Exercises safe malformed-device and undefined-enum paths without opening the registry.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DeviceInterface_InvalidMetadataAndMalformedRegistryPathAreHandledAsync()
    {
        var device = DevBroadcastDeviceInterface.Test(@"\?\USB##INSTANCE");
        device.DeviceClass = (DeviceInterfaceClass)int.MaxValue;

        await Assert.That(device.DeviceClassGuid).IsEqualTo(Guid.Empty);
        await Assert.That(device.DeviceClass).IsEqualTo(DeviceInterfaceClass.Unknown);
        await Assert.That(device.DeviceSetupClassGuid).IsNull();
        await Assert.That(device.FriendlyDeviceName).IsEqualTo(@"\?\USB##INSTANCE");
    }

    /// <summary>Exercises modifier parsing and the empty localized-name fallback through a composed display adapter.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyHelper_UsesShiftAliasAndEmptyLocalizedNameFallbackAsync()
    {
        var displayApi = new EmptyKeyboardDisplayApi();
        INativeKeyboardDisplayApi previousApi = KeyHelper.SetDisplayApiForTesting(displayApi);

        try
        {
            await Assert.That(KeyHelper.VirtualKeyCodeFromString("shift")).IsEqualTo(VirtualKeyCode.Shift);
            await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.KeyA, false)).IsEqualTo(nameof(VirtualKeyCode.KeyA));
        }
        finally
        {
            _ = KeyHelper.SetDisplayApiForTesting(previousApi);
        }
    }

    /// <summary>Exercises observable error delivery when the composed wait operation reports an unexpected native result.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WaitableTimer_ObservableForwardsComposedWaitErrorsAsync()
    {
        using var handle = new Microsoft.Win32.SafeHandles.SafeWaitHandle(TimerHandle, ownsHandle: false);
        using var timer = new WaitableTimer(handle);
        var error = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);

        using (WaitableTimer.OverrideWaitForSingleObjectForTesting(static (_, _) => throw new InvalidOperationException("expected")))
        using (System.ObservableExtensions.Subscribe(timer.ObserveSignals(), static _ => { }, exception => _ = error.TrySetResult(exception)))
        {
            Task completed = await Task.WhenAny(error.Task, Task.Delay(TimeSpan.FromSeconds(One)));
            await Assert.That(completed).IsSameReferenceAs(error.Task);
            Exception observedError = await error.Task;
            await Assert.That(observedError).IsTypeOf<InvalidOperationException>();
        }
    }

    /// <summary>Provides a deterministic native keyboard-display adapter that returns an empty localized key name.</summary>
    private sealed class EmptyKeyboardDisplayApi : INativeKeyboardDisplayApi
    {
        /// <inheritdoc />
        public IntPtr GetKeyboardLayout(uint threadId) => IntPtr.Zero;

        /// <inheritdoc />
        public int GetKeyNameText(uint longParameter, Span<char> text) => 0;

        /// <inheritdoc />
        public uint MapVirtualKeyEx(uint code, uint mapType, IntPtr keyboardLayout) => 1U;
    }
}
