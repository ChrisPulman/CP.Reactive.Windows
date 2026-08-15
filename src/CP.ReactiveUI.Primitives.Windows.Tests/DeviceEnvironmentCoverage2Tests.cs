// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Focused coverage for device, environment, software, and bitmap primitives.</summary>
public sealed class DeviceEnvironmentCoverage2Tests
{
    /// <summary>The fake message-window handle.</summary>
    private const long TestWindowHandle = 0x1234;

    /// <summary>The fake device-notification registration handle.</summary>
    private const long TestDeviceNotificationHandle = 0x5678;

    /// <summary>The expected device unit mask for A and C drives.</summary>
    private const uint TestVolumeMask = 0b101U;

    /// <summary>The expected media and network flags.</summary>
    private const ushort TestVolumeFlags = 0x0003;

    /// <summary>The test estimated size value.</summary>
    private const int TestEstimatedSize = 1_234;

    /// <summary>The test installed size value.</summary>
    private const long TestInstalledSize = 9_876_543_210L;

    /// <summary>The test product code.</summary>
    private static readonly Guid TestProductCode = new("11111111-2222-3333-4444-555555555555");

    /// <summary>Tests the composed device-notification observable without native registration.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DeviceNotificationObservableCoversRegistrationFilteringTeardownAndErrorsAsync()
    {
        const string deviceName = @"\?\USB#VID_1234&PID_5678#1#{a5dcbf10-6530-11d2-901f-00c04fb951ed}";
        using var deviceMemory = DeviceBroadcastMemory.CreateInterface(DevBroadcastDeviceInterface.Test(deviceName, DeviceInterfaceClass.UsbDevice));
        using var ignoredVolumeMemory = DeviceBroadcastMemory.CreateVolume(TestVolumeMask, TestVolumeFlags);
        var source = new DeviceNotificationSource(
        [
            new(unchecked((nint)TestWindowHandle), WindowsMessages.WM_APP, 0, deviceMemory.Pointer),
            new(unchecked((nint)TestWindowHandle), WindowsMessages.WM_DEVICECHANGE, (nint)DeviceChangeEvent.DeviceArrival, 0),
            new(unchecked((nint)TestWindowHandle), WindowsMessages.WM_DEVICECHANGE, (nint)DeviceChangeEvent.DeviceArrival, deviceMemory.Pointer),
            new(unchecked((nint)TestWindowHandle), WindowsMessages.WM_DEVICECHANGE, (nint)DeviceChangeEvent.DeviceRemoveComplete, ignoredVolumeMemory.Pointer),
        ]);
        var unregisterHandle = IntPtr.Zero;
        var observer = new CollectingObserver<DeviceNotificationEvent>();

        using (DeviceNotification.ObserveDeviceNotifications(
            DeviceInterfaceClass.UsbDevice,
            source.Listen,
            (recipientHandle, notificationFilter, flags) =>
            {
                source.SetRegistrationRecipient(recipientHandle);
                source.RegistrationFilter = notificationFilter;
                source.RegistrationFlags = flags;
                return (nint)TestDeviceNotificationHandle;
            },
            handle =>
            {
                unregisterHandle = handle;
                return true;
            }).Subscribe(observer))
        {
            await Assert.That(observer.Values.Count).IsEqualTo(Two);
        }

        await Assert.That(source.SetupHandle).IsEqualTo(TestWindowHandle);
        await Assert.That(source.TeardownHandle).IsEqualTo(TestWindowHandle);
        await Assert.That(source.RegistrationRecipientValue).IsEqualTo(TestWindowHandle);
        await Assert.That(source.RegistrationFilter.DeviceClass).IsEqualTo(DeviceInterfaceClass.UsbDevice);
        await Assert.That(source.RegistrationFlags).IsEqualTo(DeviceNotifyFlags.None);
        await Assert.That(unregisterHandle).IsEqualTo((nint)TestDeviceNotificationHandle);
        await Assert.That(observer.Values[0].EventType).IsEqualTo(DeviceChangeEvent.DeviceArrival);
        await Assert.That(observer.Values[1].EventType).IsEqualTo(DeviceChangeEvent.DeviceRemoveComplete);

        var errorObserver = new CollectingObserver<DeviceNotificationEvent>();
        using var errorSubscription = DeviceNotification.ObserveDeviceNotifications(
            DeviceInterfaceClass.Unknown,
            new DeviceNotificationSource([]).Listen,
            static (_, _, _) => IntPtr.Zero,
            static _ => true)
            .Subscribe(errorObserver);

        await Assert.That(errorObserver.Error).IsTypeOf<Win32Exception>();
    }

    /// <summary>Tests the public device-notification filters over deterministic notification events.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DeviceNotificationFiltersCoverArrivalRemovalAndVolumeSelectionsAsync()
    {
        const string deviceName = @"\?\USB#VID_1234&PID_5678#1#{a5dcbf10-6530-11d2-901f-00c04fb951ed}";
        using var deviceMemory = DeviceBroadcastMemory.CreateInterface(DevBroadcastDeviceInterface.Test(deviceName, DeviceInterfaceClass.UsbDevice));
        using var volumeMemory = DeviceBroadcastMemory.CreateVolume(TestVolumeMask, TestVolumeFlags);
        var source = new ArrayObservable<DeviceNotificationEvent>(
        [
            deviceMemory.CreateEvent(DeviceChangeEvent.DeviceArrival),
            deviceMemory.CreateEvent(DeviceChangeEvent.DeviceRemoveComplete),
            volumeMemory.CreateEvent(DeviceChangeEvent.DeviceArrival),
            volumeMemory.CreateEvent(DeviceChangeEvent.DeviceRemoveComplete),
        ]);
        var arrivals = new List<DeviceInterfaceChangeInfo>();
        var removals = new List<DeviceInterfaceChangeInfo>();
        var volumes = new List<VolumeInfo>();
        var addedVolumes = new List<VolumeInfo>();
        var removedVolumes = new List<VolumeInfo>();

        using var arrivalSubscription = DeviceNotification.ObserveDeviceArrivals(source).Subscribe(arrivals.Add);
        using var removalSubscription = DeviceNotification.ObserveDeviceRemovals(source).Subscribe(removals.Add);
        using var volumeSubscription = DeviceNotification.ObserveVolumeChanges(source).Subscribe(volumes.Add);
        using var addedVolumeSubscription = DeviceNotification.ObserveVolumeAdditions(source).Subscribe(addedVolumes.Add);
        using var removedVolumeSubscription = DeviceNotification.ObserveVolumeRemovals(source).Subscribe(removedVolumes.Add);

        await Assert.That(arrivals.Count).IsEqualTo(1);
        await Assert.That(removals.Count).IsEqualTo(1);
        await Assert.That(volumes.Count).IsEqualTo(Two);
        await Assert.That(addedVolumes.Count).IsEqualTo(1);
        await Assert.That(removedVolumes.Count).IsEqualTo(1);
        await Assert.That(arrivals[0].Device.Name).IsEqualTo(deviceName);
        await Assert.That(volumes[0].Volume.Drives).IsEqualTo("AC");
    }

    /// <summary>Tests device-struct parsing, equality, and registry-miss fallbacks.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DeviceBroadcastStructsCoverFactoriesEqualityAndInvalidPathsAsync()
    {
        const string deviceName = @"\?\PCI#VEN_10DE&DEV_1FB8#4&32af3f68&0&0008#{1ca05180-a699-450a-9a0c-de4fbe3ddd89}";
        var first = DevBroadcastDeviceInterface.Test(deviceName, DeviceInterfaceClass.DisplayDeviceArrival);
        var second = DevBroadcastDeviceInterface.Test(deviceName, DeviceInterfaceClass.DisplayDeviceArrival);
        var created = DevBroadcastDeviceInterface.Create();
        var defaultHeader = default(DevBroadcastHeader);

        created.DeviceClass = DeviceInterfaceClass.Keyboard;

        await Assert.That(first.DisplayName).IsEqualTo(@"PCI\VEN_10DE&DEV_1FB8\4&32af3f68&0&0008");
        await Assert.That(first.DeviceSetupClassGuid).IsNull();
        await Assert.That(first.FriendlyDeviceName).IsEqualTo(deviceName);
        await Assert.That(first.Equals((object)second)).IsTrue();
        await Assert.That(first == second).IsTrue();
        await Assert.That(first != created).IsTrue();
        await Assert.That(first.GetHashCode()).IsEqualTo(typeof(DevBroadcastDeviceInterface).GetHashCode());
        await Assert.That(created.DeviceClass).IsEqualTo(DeviceInterfaceClass.Hid);
        await Assert.That(defaultHeader.Equals((object)defaultHeader)).IsTrue();
        await Assert.That(defaultHeader == default).IsTrue();
        await Assert.That(defaultHeader != default).IsFalse();
    }

    /// <summary>Tests environment-change arguments and observable accessors.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task EnvironmentChangeObjectsCoverFactoriesAndObservableAccessAsync()
    {
        var defaultArgs = EnvironmentChangedEventArgs.Create();
        var actionArgs = EnvironmentChangedEventArgs.Create(SystemParametersInfoActions.SPI_SETDESKWALLPAPER);
        var areaArgs = EnvironmentChangedEventArgs.Create(SystemParametersInfoActions.SPI_SETWORKAREA, "Environment");

        await Assert.That(defaultArgs.SystemParametersInfoAction).IsEqualTo(SystemParametersInfoActions.SPI_NONE);
        await Assert.That(defaultArgs.Area).IsNull();
        await Assert.That(actionArgs.SystemParametersInfoAction).IsEqualTo(SystemParametersInfoActions.SPI_SETDESKWALLPAPER);
        await Assert.That(actionArgs.Area).IsNull();
        await Assert.That(areaArgs.SystemParametersInfoAction).IsEqualTo(SystemParametersInfoActions.SPI_SETWORKAREA);
        await Assert.That(areaArgs.Area).IsEqualTo("Environment");
        await Assert.That(EnvironmentMonitor.EnvironmentChangeEvents).IsNotNull();
    }

    /// <summary>Tests software mapping and formatting without reading the real uninstall registry.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SoftwareDetailsMappingCoversConversionsFormattingAndInvalidValuesAsync()
    {
        var values = new Dictionary<string, (object Value, RegistryValueKind Kind)>(StringComparer.OrdinalIgnoreCase)
        {
            ["DisplayName"] = ("Reactive Windows", RegistryValueKind.String),
            ["DisplayVersion"] = ("1.2.3", RegistryValueKind.String),
            ["Publisher"] = ("CP", RegistryValueKind.String),
            ["EstimatedSize"] = (TestEstimatedSize, RegistryValueKind.DWord),
            ["Size"] = (TestInstalledSize, RegistryValueKind.QWord),
            ["SystemComponent"] = (1, RegistryValueKind.DWord),
            ["WindowsInstaller"] = (0L, RegistryValueKind.QWord),
            ["Version"] = ("42", RegistryValueKind.String),
            ["Language"] = ("not-an-int", RegistryValueKind.String),
            ["Comments"] = (string.Empty, RegistryValueKind.String),
        };
        var mapped = InstallationInformation.MapFromRegistryValues(
            TestProductCode.ToString("B", CultureInfo.InvariantCulture),
            values.Keys,
            name => values[name].Value,
            name => values[name].Kind);
        var unnamed = new SoftwareDetails();

        await Assert.That(mapped.Id).IsEqualTo(TestProductCode);
        await Assert.That(mapped.DisplayName).IsEqualTo("Reactive Windows");
        await Assert.That(mapped.DisplayVersion).IsEqualTo("1.2.3");
        await Assert.That(mapped.Publisher).IsEqualTo("CP");
        await Assert.That(mapped.EstimatedSize).IsEqualTo(OneThousandTwoHundredThirtyFour);
        await Assert.That(mapped.Size).IsEqualTo(TestInstalledSize);
        await Assert.That(mapped.SystemComponent).IsTrue();
        await Assert.That(mapped.WindowsInstaller).IsFalse();
        await Assert.That(mapped.Version).IsEqualTo(FortyTwo);
        await Assert.That(mapped.Language).IsEqualTo(0);
        await Assert.That(mapped.Comments).IsNull();
        await Assert.That(mapped.ToString()).Contains("Reactive Windows - 1.2.3");
        await Assert.That(unnamed.ToString()).Contains("00000000-0000-0000-0000-000000000000");
    }

    /// <summary>Tests bitmap extension conversions and argument validation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapExtensionsCoverBitmapImageAndNullConversionsAsync()
    {
        using var bitmap = new Bitmap(Two, Three);
        Image image = bitmap;
        var bitmapSource = bitmap.ToBitmapSource();
        var imageSource = image.ToBitmapSource();
        Bitmap nullBitmap = null;
        Image nullImage = null;

        await Assert.That(bitmapSource.PixelWidth).IsEqualTo(Two);
        await Assert.That(bitmapSource.PixelHeight).IsEqualTo(Three);
        await Assert.That(imageSource.PixelWidth).IsEqualTo(Two);
        await Assert.That(imageSource.PixelHeight).IsEqualTo(Three);
        await Assert.That(() => nullBitmap.ToBitmapSource()).Throws<ArgumentNullException>();
        await Assert.That(() => nullImage.ToBitmapSource()).Throws<ArgumentNullException>();
    }

    /// <summary>Provides a deterministic message source for device-notification composition tests.</summary>
    private sealed class DeviceNotificationSource
    {
        /// <summary>The messages to publish on subscription.</summary>
        private readonly IReadOnlyList<WindowMessage> _messages;

        /// <summary>The received registration recipient value.</summary>
        private long _registrationRecipient;

        /// <summary>Initializes a new instance of the <see cref="DeviceNotificationSource"/> class.</summary>
        /// <param name="messages">The messages to publish on subscription.</param>
        public DeviceNotificationSource(IReadOnlyList<WindowMessage> messages) => _messages = messages;

        /// <summary>Gets or sets the received registration filter.</summary>
        public DevBroadcastDeviceInterface RegistrationFilter { get; set; }

        /// <summary>Gets or sets the received registration flags.</summary>
        public DeviceNotifyFlags RegistrationFlags { get; set; }

        /// <summary>Gets the received registration recipient as an integer value.</summary>
        public long RegistrationRecipientValue => _registrationRecipient;

        /// <summary>Gets the setup handle passed to the listener.</summary>
        public long SetupHandle { get; private set; }

        /// <summary>Gets the teardown handle passed to the listener.</summary>
        public long TeardownHandle { get; private set; }

        /// <summary>Stores the received registration recipient.</summary>
        /// <param name="registrationRecipient">The registration recipient.</param>
        public void SetRegistrationRecipient(IntPtr registrationRecipient) => _registrationRecipient = registrationRecipient.ToInt64();

        /// <summary>Creates the observable source.</summary>
        /// <param name="onSetup">The setup callback.</param>
        /// <param name="onTeardown">The teardown callback.</param>
        /// <returns>The deterministic observable source.</returns>
        public IObservable<WindowMessage> Listen(Action<long> onSetup, Action<long> onTeardown) =>
            new SourceObservable(this, onSetup, onTeardown);

        /// <summary>Subscribes an observer to the deterministic message source.</summary>
        /// <param name="observer">The observer.</param>
        /// <param name="onSetup">The setup callback.</param>
        /// <param name="onTeardown">The teardown callback.</param>
        /// <returns>The subscription.</returns>
        private TeardownDisposable Subscribe(IObserver<WindowMessage> observer, Action<long> onSetup, Action<long> onTeardown)
        {
            SetupHandle = TestWindowHandle;
            onSetup(TestWindowHandle);
            foreach (var message in _messages)
            {
                observer.OnNext(message);
            }

            observer.OnCompleted();
            return new(this, onTeardown);
        }

        /// <summary>Observable adapter for the source.</summary>
        /// <param name="owner">The owning source.</param>
        /// <param name="onSetup">The setup callback.</param>
        /// <param name="onTeardown">The teardown callback.</param>
        private sealed class SourceObservable(DeviceNotificationSource owner, Action<long> onSetup, Action<long> onTeardown) : IObservable<WindowMessage>
        {
            /// <inheritdoc/>
            public IDisposable Subscribe(IObserver<WindowMessage> observer) => owner.Subscribe(observer, onSetup, onTeardown);
        }

        /// <summary>Invokes teardown when disposed.</summary>
        /// <param name="owner">The owning source.</param>
        /// <param name="onTeardown">The teardown callback.</param>
        private sealed class TeardownDisposable(DeviceNotificationSource owner, Action<long> onTeardown) : IDisposable
        {
            /// <inheritdoc/>
            public void Dispose()
            {
                owner.TeardownHandle = TestWindowHandle;
                onTeardown(TestWindowHandle);
            }
        }
    }

    /// <summary>Publishes a fixed set of values to each subscriber.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="values">The values.</param>
    private sealed class ArrayObservable<T>(IReadOnlyList<T> values) : IObservable<T>
    {
        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            foreach (var value in values)
            {
                observer.OnNext(value);
            }

            observer.OnCompleted();
            return new EmptyDisposable();
        }
    }

    /// <summary>Collects observable notifications for assertions.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    private sealed class CollectingObserver<T> : IObserver<T>
    {
        /// <summary>Gets a value indicating whether completion was observed.</summary>
        public bool IsCompleted { get; private set; }

        /// <summary>Gets the observed error.</summary>
        public Exception Error { get; private set; }

        /// <summary>Gets the observed values.</summary>
        public List<T> Values { get; } = [];

        /// <inheritdoc/>
        public void OnCompleted() => IsCompleted = true;

        /// <inheritdoc/>
        public void OnError(Exception error) => Error = error;

        /// <inheritdoc/>
        public void OnNext(T value) => Values.Add(value);
    }

    /// <summary>An empty disposable.</summary>
    private sealed class EmptyDisposable : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
