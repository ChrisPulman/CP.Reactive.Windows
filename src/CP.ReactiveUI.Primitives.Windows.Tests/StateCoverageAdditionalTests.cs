// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Additional safe-path coverage for clipboard, device, power, timer, and lifecycle APIs.</summary>
public sealed class StateCoverageAdditionalTests
{
    /// <summary>The expected Restart Manager command-line limit.</summary>
    private const int ExpectedRestartMaxCommandLine = 1024;

    /// <summary>A native wait result that is neither signaled nor timed out.</summary>
    private const uint InvalidNativeWaitResult = 0xDEADU;

    /// <summary>One millisecond beyond the UInt32 timeout ceiling.</summary>
    private const double OneMillisecondBeyondUInt32Max = 4294967296D;

    /// <summary>Tests standard clipboard format names and unknown format mapping.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardFormatMappingsCoverStandardCustomAndUnknownAsync()
    {
        var unicodeText = StandardClipboardFormats.UnicodeText.AsString();
        var displayBitmap = StandardClipboardFormats.DisplayBitmap.AsString();
        var unknown = ((StandardClipboardFormats)uint.MaxValue).AsString();
        var customFormat = $"CP_STATE_COVERAGE_{Guid.NewGuid():N}";

        var customId = ClipboardFormatExtensions.RegisterFormat(customFormat);
        var customIdAgain = ClipboardFormatExtensions.MapFormatToId(customFormat);

        await Assert.That(unicodeText).IsEqualTo("CF_UNICODETEXT");
        await Assert.That(displayBitmap).IsEqualTo("CF_DSPBITMAP");
        await Assert.That(unknown).IsNull();
        await Assert.That(customIdAgain).IsEqualTo(customId);
        await Assert.That(ClipboardFormatExtensions.MapIdToFormat(customId)).IsEqualTo(customFormat);
        await Assert.That(ClipboardFormatExtensions.MapIdToFormat(1)).IsEqualTo("CF_TEXT");
    }

    /// <summary>Tests clipboard stream edge cases that do not require a particular existing clipboard payload.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardStreamWritersCoverUnreadableEmptyAndNonSeekableStreamsAsync()
    {
        var format = $"CP_STATE_STREAM_{Guid.NewGuid():N}";
        var expected = "non-seekable clipboard payload"u8.ToArray();

        using var clipboardAccessToken = await ClipboardNative.AccessAsync();
        await Assert.That(clipboardAccessToken.CanAccess).IsTrue();
        clipboardAccessToken.ClearContents();

#if NETFRAMEWORK
        using var unreadableStream = new UnreadableStream();
#else
        await using var unreadableStream = new UnreadableStream();
#endif
        await Assert.That(() => clipboardAccessToken.SetAsStream(format, unreadableStream)).Throws<NotSupportedException>();

#if NETFRAMEWORK
        using var emptyStream = new MemoryStream();
#else
        await using var emptyStream = new MemoryStream();
#endif
        await Assert.That(() => clipboardAccessToken.SetAsStream(format, emptyStream)).Throws<NotSupportedException>();

#if NETFRAMEWORK
        using var nonSeekableStream = new NonSeekableReadStream(expected);
#else
        await using var nonSeekableStream = new NonSeekableReadStream(expected);
#endif
        clipboardAccessToken.SetAsStream(format, nonSeekableStream);

        await Assert.That(clipboardAccessToken.GetAsBytes(format)).IsEquivalentTo(expected);
        await Assert.That(clipboardAccessToken.TryGetAsStream("CP_STATE_STREAM_MISSING", out var missingStream)).IsFalse();
        await Assert.That(missingStream).IsNull();
    }

    /// <summary>Tests device interface parsing for USB, PCI, invalid, and explicit device-class paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DeviceInterfaceParsingCoversValidInvalidAndClassPathsAsync()
    {
        const string usbDevice = @"\?\USB#VID_05AC&PID_1294&MI_00#0#{6bdd1fc6-810f-11d0-bec7-08002be2092f}";
        const string pciDevice = @"\?\PCI#VEN_10DE&DEV_1FB8&SUBSYS_09061028&REV_A1#4&32af3f68&0&0008#{1ca05180-a699-450a-9a0c-de4fbe3ddd89}";
        const string invalidDevice = "not-a-device-path";

        var usb = DevBroadcastDeviceInterface.Test(usbDevice, DeviceInterfaceClass.StillImage);
        var pci = DevBroadcastDeviceInterface.Test(pciDevice, DeviceInterfaceClass.DisplayDeviceArrival);
        var invalid = DevBroadcastDeviceInterface.Test(invalidDevice);
        var created = DevBroadcastDeviceInterface.Create();
        created.DeviceClass = DeviceInterfaceClass.UsbDevice;

        await Assert.That(usb.DeviceType).IsEqualTo("USB");
        await Assert.That(usb.VendorId).IsEqualTo("05AC");
        await Assert.That(usb.ProductId).IsEqualTo("1294");
        await Assert.That(usb.DeviceId).IsNull();
        await Assert.That(usb.IsUsb).IsTrue();
        await Assert.That(usb.IsPci).IsFalse();
        await Assert.That(usb.FriendlyDeviceName).IsEqualTo(usbDevice);
        await Assert.That(usb.UsbDeviceInfoUri.ToString()).Contains("v=0x05AC");

        await Assert.That(pci.DeviceType).IsEqualTo("PCI");
        await Assert.That(pci.VendorId).IsEqualTo("10DE");
        await Assert.That(pci.DeviceId).IsEqualTo("1FB8");
        await Assert.That(pci.ProductId).IsNull();
        await Assert.That(pci.IsPci).IsTrue();
        await Assert.That(pci.DeviceClass).IsEqualTo(DeviceInterfaceClass.DisplayDeviceArrival);

        await Assert.That(invalid.DeviceType).IsNull();
        await Assert.That(invalid.VendorId).IsNull();
        await Assert.That(invalid.ProductId).IsNull();
        await Assert.That(invalid.FriendlyDeviceName).IsEqualTo(invalidDevice);
        await Assert.That(created.DeviceClass).IsEqualTo(DeviceInterfaceClass.UsbDevice);
        await Assert.That(DevBroadcastDeviceInterface.Test(usbDevice) == DevBroadcastDeviceInterface.Test(usbDevice)).IsTrue();
        await Assert.That(DevBroadcastDeviceInterface.Test(usbDevice) != DevBroadcastDeviceInterface.Test(pciDevice)).IsTrue();
    }

    /// <summary>Tests device broadcast message decoding without sending operating-system messages.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DeviceNotificationEventDecodesVolumeAndInterfaceMessagesAsync()
    {
        using var volumeMemory = DeviceBroadcastMemory.CreateVolume(unitMask: 0b101U, flags: 0x0003);
        var volumeEvent = volumeMemory.CreateEvent(DeviceChangeEvent.DeviceArrival);

        await Assert.That(volumeEvent.EventType).IsEqualTo(DeviceChangeEvent.DeviceArrival);
        await Assert.That(volumeEvent.Is(DeviceBroadcastDeviceType.Volume)).IsTrue();
        await Assert.That(volumeEvent.Is(DeviceBroadcastDeviceType.DeviceInterface)).IsFalse();
        await Assert.That(volumeEvent.TryGetDevBroadcastVolume(out var volume)).IsTrue();
        await Assert.That(volume.Drives).IsEqualTo("AC");
        await Assert.That(volume.IsMediaChange).IsTrue();
        await Assert.That(volume.IsNetworkVolume).IsTrue();
        await Assert.That(volumeEvent.TryGetDevBroadcastDeviceInterface(out _)).IsFalse();

        const string deviceName = @"\?\USB#VID_1234&PID_ABCD#1#{a5dcbf10-6530-11d2-901f-00c04fb951ed}";
        using var interfaceMemory = DeviceBroadcastMemory.CreateInterface(DevBroadcastDeviceInterface.Test(deviceName, DeviceInterfaceClass.UsbDevice));
        var interfaceEvent = interfaceMemory.CreateEvent(DeviceChangeEvent.DeviceRemoveComplete);

        await Assert.That(interfaceEvent.EventType).IsEqualTo(DeviceChangeEvent.DeviceRemoveComplete);
        await Assert.That(interfaceEvent.Is(DeviceBroadcastDeviceType.DeviceInterface)).IsTrue();
        await Assert.That(interfaceEvent.TryGetDevBroadcastDeviceInterface(out var deviceInterface)).IsTrue();
        await Assert.That(deviceInterface.Name).IsEqualTo(deviceName);
        await Assert.That(deviceInterface.DeviceClass).IsEqualTo(DeviceInterfaceClass.UsbDevice);
        await Assert.That(interfaceEvent.TryGetDevBroadcastVolume(out _)).IsFalse();
    }

    /// <summary>Tests simple device structs and DTO default/equality paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DeviceStructsAndInfoObjectsCoverDefaultsFactoriesAndEqualityAsync()
    {
        var port = DevBroadcastPort.Create();
        var samePort = DevBroadcastPort.Create();
        var handle = DevBroadcastHandle.Create();
        var sameHandle = DevBroadcastHandle.Create();
        var volume = default(DevBroadcastVolume);
        var header = default(DevBroadcastHeader);
        var volumeInfo = new VolumeInfo();
        var interfaceInfo = new DeviceInterfaceChangeInfo();

        await Assert.That(port.Name).IsNull();
        await Assert.That(port == samePort).IsTrue();
        await Assert.That(port != default).IsTrue();
        await Assert.That(port.Equals((object)samePort)).IsTrue();
        await Assert.That(handle == sameHandle).IsTrue();
        await Assert.That(handle != default).IsTrue();
        await Assert.That(handle.Equals((object)sameHandle)).IsTrue();
        await Assert.That(volume.Drives).IsEqualTo(string.Empty);
        await Assert.That(volume.IsMediaChange).IsFalse();
        await Assert.That(volume.IsNetworkVolume).IsFalse();
        await Assert.That(header.DeviceType).IsEqualTo(DeviceBroadcastDeviceType.Oem);
        await Assert.That(volumeInfo.EventType).IsEqualTo(DeviceChangeEvent.None);
        await Assert.That(interfaceInfo.EventType).IsEqualTo(DeviceChangeEvent.None);
    }

    /// <summary>Tests waitable timer timeout, periodic, cancellation, and argument validation paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WaitableTimerCoversTimeoutPeriodicCancellationAndValidationAsync()
    {
        using var timer = new WaitableTimer(manualReset: true);
        await Assert.That(timer.IsValid).IsTrue();
        await Assert.That(await timer.WaitAsync(TimeSpan.Zero)).IsFalse();
        await Assert.That(() => timer.SetOnce(TimeSpan.FromMilliseconds(-1))).Throws<ArgumentOutOfRangeException>();
        await Assert.That(static () => new WaitableTimer(string.Empty)).Throws<ArgumentException>();

        await Assert.That(timer.SetPeriodic(TimeSpan.FromMilliseconds(Twenty), Twenty)).IsTrue();
        await Assert.That(await timer.WaitAsync(TimeSpan.FromSeconds(Two))).IsTrue();
        await Assert.That(timer.Cancel()).IsTrue();

        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = timer.ObserveSignals(TimeSpan.FromMilliseconds(Twenty)).Subscribe(
            _ => completion.TrySetResult(false),
            exception => completion.TrySetException(exception),
            () => completion.TrySetResult(true));

        await Assert.That(await completion.Task).IsFalse();
    }

    /// <summary>Tests waitable timer overflow, invalid native wait, and disposed validity paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WaitableTimerCoversOverflowInvalidNativeWaitAndDisposedValidityAsync()
    {
        using var waitOverride = WaitableTimer.OverrideWaitForSingleObjectForTesting(static (_, _) => InvalidNativeWaitResult);
        var timer = new WaitableTimer();

        await Assert.That(() => timer.Wait(TimeSpan.FromMilliseconds(OneMillisecondBeyondUInt32Max))).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => timer.Wait(TimeSpan.Zero)).Throws<InvalidOperationException>();

        timer.Dispose();

        await Assert.That(timer.IsValid).IsFalse();

        timer.Dispose();
    }

    /// <summary>Tests safe power-management constants and non-destructive execution-state helpers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task PowerManagementSafePathsCoverFlagsAndExecutionStateHelpersAsync()
    {
        var forcedShutdown = GetEnumValue(ExitWindowsFlags.EWX_SHUTDOWN | ExitWindowsFlags.EWX_FORCE);
        var forcedReboot = GetEnumValue(ExitWindowsFlags.EWX_REBOOT | ExitWindowsFlags.EWX_FORCE);
        var continuousDisplayRequired = GetEnumValue(ThreadExecutionStateFlags.ES_CONTINUOUS | ThreadExecutionStateFlags.ES_DISPLAY_REQUIRED);
        var resumedCritical = GetEnumValue(PowerBroadcastEvent.PBT_APMRESUMEDCRITICAL);

        await Assert.That(forcedShutdown).IsEqualTo(0x00000005U);
        await Assert.That(forcedReboot).IsEqualTo(0x00000006U);
        await Assert.That(continuousDisplayRequired).IsEqualTo(0x80000002U);
        await Assert.That(resumedCritical).IsEqualTo(0x0006U);

        _ = SystemStateApi.PreventSystemSleep();
        var restored = SystemStateApi.AllowSleep();

        await Assert.That(restored != 0).IsTrue();
        await Assert.That(PowerBroadcastListener.PowerBroadcastEvents).IsNotNull();
        await Assert.That(PowerBroadcastListener.SystemSuspendingEvents).IsNotNull();
        await Assert.That(PowerBroadcastListener.SystemResumedFromSuspendEvents).IsNotNull();
        await Assert.That(PowerBroadcastListener.SystemAutomaticResumeEvents).IsNotNull();
        await Assert.That(PowerBroadcastListener.PowerStatusChanges).IsNotNull();
    }

    /// <summary>Tests application restart argument validation and session-message value behavior without triggering restart.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ApplicationRestartManagerSafePathsCoverValidationMessagesAndArgumentInspectionAsync()
    {
        var tooLong = new string('a', ApplicationRestartManager.MaxCommandLineLength + 1);
        var message = new EndSessionMessage(WindowsMessages.WM_ENDSESSION, EndSessionReasons.ENDSESSION_CLOSEAPP) { Handled = true, Result = 1 };

        await Assert.That(ApplicationRestartManager.RestartMaxCmdLine).IsEqualTo(ExpectedRestartMaxCommandLine);
        await Assert.That(ApplicationRestartManager.MaxCommandLineLength).IsEqualTo(ApplicationRestartManager.RestartMaxCmdLine);
        await Assert.That(() => ApplicationRestartManager.RegisterForRestart(tooLong, ApplicationRestartFlags.RestartNoCrash)).Throws<ArgumentException>();
        await Assert.That(ApplicationRestartManager.GetRestartCommandLineArgs().Length).IsEqualTo(Math.Max(0, Environment.GetCommandLineArgs().Length - 1));
        await Assert.That(message.Msg).IsEqualTo(WindowsMessages.WM_ENDSESSION);
        await Assert.That(message.EndSessionReason).IsEqualTo(EndSessionReasons.ENDSESSION_CLOSEAPP);
        await Assert.That(message.Handled).IsTrue();
        await Assert.That(message.Result).IsEqualTo(1);
        await Assert.That(ApplicationRestartManager.ObserveEndSessionMessages()).IsNotNull();
        await Assert.That(ApplicationRestartManager.ObserveEndSessionMessages(static _ => true)).IsNotNull();
        await Assert.That(ApplicationRestartManager.ObserveEndSessionMessages(static _ => true, static _ => false)).IsNotNull();
    }

    /// <summary>Tests Restart Manager public paths that do not shut down or restart applications.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RestartManagerSafePathsCoverEmptyRegistrationQueriesAndDisposedGuardsAsync()
    {
        var session = RestartManager.CreateSession();

        session.RegisterFiles();
        session.RegisterFiles(null);
        session.RegisterProcesses();
        session.RegisterProcesses(null);
        session.RegisterServices();
        session.RegisterServices(null);

        var processes = session.GetProcessesUsingResources();
        var processesWithReason = session.GetProcessesUsingResources(out var rebootReason);

        await Assert.That(session.SessionKey).IsNotNull();
        await Assert.That(processes).IsEmpty();
        await Assert.That(processesWithReason).IsEmpty();
        await Assert.That(rebootReason).IsEqualTo(RmRebootReason.None);
        await Assert.That(session.IsRebootRequired()).IsFalse();
        await Assert.That(session.GetRebootReason()).IsEqualTo(RmRebootReason.None);

        session.Dispose();
        session.Dispose();

        await Assert.That(() => session.RegisterFiles("file.txt")).Throws<ObjectDisposedException>();
        await Assert.That(() => session.RegisterProcesses(default)).Throws<ObjectDisposedException>();
        await Assert.That(() => session.RegisterServices("service")).Throws<ObjectDisposedException>();
        await Assert.That(() => session.GetProcessesUsingResources()).Throws<ObjectDisposedException>();
    }

    /// <summary>Reads an enum's native unsigned integer value through the runtime conversion path.</summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The enum value.</param>
    /// <returns>The native unsigned integer value.</returns>
    private static uint GetEnumValue<TEnum>(TEnum value)
        where TEnum : struct, Enum => Convert.ToUInt32(value, CultureInfo.InvariantCulture);

    /// <summary>A readable stream that does not support seeking.</summary>
    /// <param name="bytes">The bytes to read.</param>
    private sealed class NonSeekableReadStream(byte[] bytes) : Stream
    {
        /// <summary>The backing data.</summary>
        private readonly MemoryStream _inner = new(bytes);

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    /// <summary>A stream that intentionally cannot be read.</summary>
    private sealed class UnreadableStream : Stream
    {
        /// <inheritdoc/>
        public override bool CanRead => false;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
