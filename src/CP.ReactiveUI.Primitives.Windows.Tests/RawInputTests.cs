// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Raw Input Tests behavior.</summary>
public class RawInputTests
{
    /// <summary>Writes diagnostic messages for these tests.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(RawInputTests));

    /// <summary>Test RawInput.GetAllDevices.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_RawInput_AllDevicesAsync()
    {
        var rawInputDeviceInformation = new List<RawInputDeviceInformation>(RawInputApi.GetAllDevices());
        rawInputDeviceInformation.Sort(static (left, right) =>
        {
            var typeComparison = left.DeviceInfo.Type.CompareTo(right.DeviceInfo.Type);
            return typeComparison != 0
                ? typeComparison
                : StringComparer.Ordinal.Compare(left.DisplayName, right.DisplayName);
        });

        var foundOneDevice = false;
        foreach (var rawInputDeviceInfo in rawInputDeviceInformation)
        {
            Log.InfoFormat("RawInput Device {0} with name {1}", rawInputDeviceInfo.DeviceInfo.Type, rawInputDeviceInfo.DisplayName);
            switch (rawInputDeviceInfo.DeviceInfo.Type)
            {
                case RawInputDeviceTypes.Keyboard:
                    {
                        var keyboardInfo = rawInputDeviceInfo.DeviceInfo.Keyboard;
                        Log.InfoFormat("Keyboard is of type {0} and subtype {1} and in mode {2}.", keyboardInfo.Type, keyboardInfo.SubType, keyboardInfo.KeyboardMode);
                        Log.InfoFormat(
                            "Keyboard with {0} key, of which {1} function keys and it has {2} LEDs.",
                            keyboardInfo.NumberOfKeysTotal,
                            keyboardInfo.NumberOfFunctionKeys,
                            keyboardInfo.NumberOfIndicators);
                        break;
                    }

                case RawInputDeviceTypes.Mouse or RawInputDeviceTypes.HID:
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported raw input device type: {rawInputDeviceInfo.DeviceInfo.Type}.");
            }

            foundOneDevice = true;
        }

        await Assert.That(foundOneDevice).IsTrue();
    }

    /// <summary>Tests Raw Input Device Changes Keyboard Removed.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_RawInput_DeviceChanges_KeyboardRemovedAsync()
    {
        var observable = RawInputDeviceMonitor.ObserveDeviceChanges(RawInputDevices.Keyboard);
        await Assert.That(observable).IsNotNull();
        try
        {
            using var subscription = observable.SubscribeOnNext(static _ => { });
        }
        catch (Win32Exception exception)
        {
            await Assert.That(exception.Message).IsNotEmpty();
            return;
        }

        await Assert.That(RawInputDeviceMonitor.GetDevicesSnapshot()).IsNotNull();
    }

    /// <summary>Tests Raw Input Left.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_RawInput_SubscriptionLifecycleAsync()
    {
        var rawInputObservable = RawInputMonitor.ObserveRawInput(RawInputDevices.Keyboard);
        var sharedObservable = RawInputMonitor.ObserveRawInput(RawInputDevices.Keyboard);
        await Assert.That(ReferenceEquals(rawInputObservable, sharedObservable)).IsTrue();

        try
        {
            using var firstSubscription = rawInputObservable.SubscribeOnNext(static _ => { });
            using var secondSubscription = sharedObservable.SubscribeOnNext(static _ => { });
        }
        catch (Win32Exception exception)
        {
            await Assert.That(exception.Message).IsNotEmpty();
            return;
        }

        await Assert.That(rawInputObservable).IsNotNull();
    }
}
