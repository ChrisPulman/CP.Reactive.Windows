// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Deterministic final coverage for DWM platform gates and device value tails.</summary>
public sealed class CoverageFinalDwmDeviceTailTests
{
    /// <summary>Covers lazy device stream creation and null fallback paths without subscribing or registering with Windows.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DeviceNotificationLazyAndNullSources_ReturnPipelinesAsync()
    {
        await Assert.That(DeviceNotification.ObserveDeviceNotifications(DeviceInterfaceClass.UsbDevice)).IsNotNull();
        await Assert.That(DeviceNotification.ObserveDeviceArrivals(null)).IsNotNull();
        await Assert.That(DeviceNotification.ObserveDeviceRemovals(null)).IsNotNull();
        await Assert.That(DeviceNotification.ObserveVolumeChanges(null)).IsNotNull();
    }

    /// <summary>Covers value-type object inequality and marshalled-data fallback paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DeviceBroadcastValueTails_CompareWithoutNativeNotificationsAsync()
    {
        var header = default(DevBroadcastHeader);
        var port = DevBroadcastPort.Create();
        var handle = default(DevBroadcastHandle);
        var createdHandle = DevBroadcastHandle.Create();

        await Assert.That(header.Equals((object)"header")).IsFalse();
        await Assert.That(header.GetHashCode()).IsEqualTo(default(DevBroadcastHeader).GetHashCode());
        await Assert.That(port.Equals((object)"port")).IsFalse();
        await Assert.That(port.GetHashCode()).IsEqualTo(DevBroadcastPort.Create().GetHashCode());
        await Assert.That(handle.Equals((object)"handle")).IsFalse();
        await Assert.That(handle.Equals(default(DevBroadcastHandle))).IsTrue();
        await Assert.That(createdHandle.Equals(default(DevBroadcastHandle))).IsFalse();
        await Assert.That(handle.GetHashCode()).IsEqualTo(default(DevBroadcastHandle).GetHashCode());
    }
}
