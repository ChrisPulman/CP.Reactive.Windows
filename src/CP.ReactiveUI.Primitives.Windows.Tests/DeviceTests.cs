// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Device Tests behavior.</summary>
public class DeviceTests
{
    /// <summary>Writes diagnostic messages for these tests.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(DeviceTests));

    /// <summary>Tests Parse.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestParseAsync()
    {
        const string appleDeviceName = @"\?\USB#VID_05AC&PID_1294&MI_00#0#{6bdd1fc6-810F-11d0-bec7-08002be2092F}";
        var devBroadcastDeviceInterface = DevBroadcastDeviceInterface.Test(appleDeviceName, DeviceInterfaceClass.StillImage);
        await Assert.That(devBroadcastDeviceInterface.ProductId).IsEqualTo("1294");
        await Assert.That(devBroadcastDeviceInterface.VendorId).IsEqualTo("05AC");
        await Assert.That(StringComparer.OrdinalIgnoreCase.Equals(devBroadcastDeviceInterface.DeviceClassGuid.ToString(), "6bdd1fc6-810F-11d0-bec7-08002be2092F")).IsTrue();
        await Assert.That(devBroadcastDeviceInterface.IsUsb).IsTrue();
        Log.InfoFormat("More information: {0}", devBroadcastDeviceInterface.UsbDeviceInfoUri);
    }

    /// <summary>Tests Parse 2.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestParse_2Async()
    {
        const string graphicsCard =
            @"\?\PCI#VEN_10DE&DEV_1FB8&SUBSYS_09061028&REV_A1#4&32af3f68&0&0008#{1ca05180-a699-450a-9a0c-de4fbe3ddd89}";
        var devBroadcastDeviceInterface = DevBroadcastDeviceInterface.Test(graphicsCard, DeviceInterfaceClass.DisplayDeviceArrival);
        await Assert.That(devBroadcastDeviceInterface.VendorId).IsEqualTo("10DE");
        await Assert.That(StringComparer.OrdinalIgnoreCase.Equals(devBroadcastDeviceInterface.DeviceClassGuid.ToString(), "1ca05180-a699-450a-9a0c-de4fbe3ddd89")).IsTrue();
        await Assert.That(devBroadcastDeviceInterface.DisplayName).IsEqualTo(@"PCI\VEN_10DE&DEV_1FB8&SUBSYS_09061028&REV_A1\4&32af3f68&0&0008");
        await Assert.That(devBroadcastDeviceInterface.IsPci).IsTrue();
        Log.InfoFormat("More information: {0}", devBroadcastDeviceInterface.UsbDeviceInfoUri);
    }

    /// <summary>Tests Device Class Guids.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestDeviceClassGuidsAsync()
    {
        // This test documents the difference between DeviceClassGuid and DeviceSetupClassGuid
        // Using an NVIDIA Quadro T1000 graphics card as the example device
        // DeviceClassGuid (Device Interface Class GUID): comes from the device notification message
        // For display device arrival events, this is the GUID_DISPLAY_DEVICE_ARRIVAL constant
        const string displayDeviceArrivalGuid = "1ca05180-a699-450a-9a0c-de4fbe3ddd89";

        const string graphicsCard =
            @"\?\PCI#VEN_10DE&DEV_1FB8&SUBSYS_09061028&REV_A1#4&32af3f68&0&0008#{1ca05180-a699-450a-9a0c-de4fbe3ddd89}";
        var devBroadcastDeviceInterface = DevBroadcastDeviceInterface.Test(graphicsCard, DeviceInterfaceClass.DisplayDeviceArrival);

        // DeviceClassGuid should be the interface class from the notification
        await Assert.That(devBroadcastDeviceInterface.DeviceClassGuid).IsEqualTo(new(displayDeviceArrivalGuid));

        // DeviceSetupClassGuid would be retrieved from registry (e.g., {4d36e968-e325-11ce-bfc1-08002be10318} for Display adapters)
        // In a test environment without the actual registry key, this will be null
        // Note: DeviceSetupClassGuid is what's shown in Windows Device Manager under "Class GUID"
        var setupClassGuid = devBroadcastDeviceInterface.DeviceSetupClassGuid;
        Log.InfoFormat("DeviceSetupClassGuid (from registry): {0}", setupClassGuid?.ToString() ?? "null (registry key not found)");
    }
}
