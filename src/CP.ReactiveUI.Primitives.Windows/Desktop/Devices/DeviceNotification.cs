// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Devices;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Devices;
#endif
/// <summary>Provides observable Windows device notification helpers.</summary>
#if NET7_0_OR_GREATER
public static partial class DeviceNotification
#else
public static class DeviceNotification
#endif
{
    /// <summary>The registration operation used by native device notifications.</summary>
    private static Func<IntPtr, IntPtr, DeviceNotifyFlags, IntPtr> _registerDeviceNotification = NativeMethods.RegisterDeviceNotification;

    /// <summary>Gets the shared observable device-notification stream.</summary>
    public static IObservable<DeviceNotificationEvent> DeviceNotificationEvents { get; } = ObserveDeviceNotifications();

    /// <summary>Creates a device-notification observable.</summary>
    /// <returns>An observable sequence of device-notification events.</returns>
    public static IObservable<DeviceNotificationEvent> ObserveDeviceNotifications() => ObserveDeviceNotifications(DeviceInterfaceClass.Unknown);

    /// <summary>Creates a device-notification observable.</summary>
    /// <param name="deviceInterfaceClass">The device interface class to observe.</param>
    /// <returns>An observable sequence of device-notification events.</returns>
    public static IObservable<DeviceNotificationEvent> ObserveDeviceNotifications(DeviceInterfaceClass deviceInterfaceClass) =>
        deviceInterfaceClass != DeviceInterfaceClass.Unknown || DeviceNotificationEvents is null
            ? ObserveDeviceNotifications(
                deviceInterfaceClass,
                SharedMessageWindow.ObserveWindowMessages,
                RegisterDeviceNotification,
                NativeMethods.UnregisterDeviceNotification)
            : DeviceNotificationEvents;

    /// <summary>Filters device-arrival notifications.</summary>
    /// <returns>An observable sequence of device-interface arrival details.</returns>
    public static IObservable<DeviceInterfaceChangeInfo> ObserveDeviceArrivals() => ObserveDeviceArrivals(DeviceNotificationEvents);

    /// <summary>Filters device-arrival notifications.</summary>
    /// <param name="observable">The source observable.</param>
    /// <returns>An observable sequence of device-interface arrival details.</returns>
    public static IObservable<DeviceInterfaceChangeInfo> ObserveDeviceArrivals(IObservable<DeviceNotificationEvent> observable) =>
        (observable ?? DeviceNotificationEvents)
        .Where(
            static deviceNotificationEvent =>
                deviceNotificationEvent.EventType == DeviceChangeEvent.DeviceArrival
                && deviceNotificationEvent.Is(DeviceBroadcastDeviceType.DeviceInterface))
        .Select(static deviceNotificationEvent =>
        {
            _ = deviceNotificationEvent.TryGetDevBroadcastDeviceInterface(out var devBroadcastDeviceInterface);
            return new DeviceInterfaceChangeInfo { EventType = deviceNotificationEvent.EventType, Device = devBroadcastDeviceInterface };
        });

    /// <summary>Filters device-remove-complete notifications.</summary>
    /// <returns>An observable sequence of device-interface removal details.</returns>
    public static IObservable<DeviceInterfaceChangeInfo> ObserveDeviceRemovals() => ObserveDeviceRemovals(DeviceNotificationEvents);

    /// <summary>Filters device-remove-complete notifications.</summary>
    /// <param name="observable">The source observable.</param>
    /// <returns>An observable sequence of device-interface removal details.</returns>
    public static IObservable<DeviceInterfaceChangeInfo> ObserveDeviceRemovals(IObservable<DeviceNotificationEvent> observable) =>
        (observable ?? DeviceNotificationEvents)
        .Where(
            static deviceNotificationEvent =>
                deviceNotificationEvent.EventType == DeviceChangeEvent.DeviceRemoveComplete
                && deviceNotificationEvent.Is(DeviceBroadcastDeviceType.DeviceInterface))
        .Select(static deviceNotificationEvent =>
        {
            _ = deviceNotificationEvent.TryGetDevBroadcastDeviceInterface(out var devBroadcastDeviceInterface);
            return new DeviceInterfaceChangeInfo { EventType = deviceNotificationEvent.EventType, Device = devBroadcastDeviceInterface };
        });

    /// <summary>Filters volume-change notifications.</summary>
    /// <returns>An observable sequence of volume-change details.</returns>
    public static IObservable<VolumeInfo> ObserveVolumeChanges() => ObserveVolumeChanges(DeviceNotificationEvents);

    /// <summary>Filters volume-change notifications.</summary>
    /// <param name="observable">The source observable.</param>
    /// <returns>An observable sequence of volume-change details.</returns>
    public static IObservable<VolumeInfo> ObserveVolumeChanges(IObservable<DeviceNotificationEvent> observable) =>
        (observable ?? DeviceNotificationEvents)
        .Where(static deviceNotificationEvent => deviceNotificationEvent.Is(DeviceBroadcastDeviceType.Volume))
        .Select(static deviceNotificationEvent =>
        {
            _ = deviceNotificationEvent.TryGetDevBroadcastVolume(out var devBroadcastVolume);
            return new VolumeInfo { EventType = deviceNotificationEvent.EventType, Volume = devBroadcastVolume };
        });

    /// <summary>Filters volume-added notifications.</summary>
    /// <returns>An observable sequence of added volumes.</returns>
    public static IObservable<VolumeInfo> ObserveVolumeAdditions() => ObserveVolumeAdditions(DeviceNotificationEvents);

    /// <summary>Filters volume-added notifications.</summary>
    /// <param name="observable">The source observable.</param>
    /// <returns>An observable sequence of added volumes.</returns>
    public static IObservable<VolumeInfo> ObserveVolumeAdditions(IObservable<DeviceNotificationEvent> observable) => from volumeInfo in ObserveVolumeChanges(observable)
                                                                                                                     where volumeInfo.EventType == DeviceChangeEvent.DeviceArrival
                                                                                                                     select volumeInfo;

    /// <summary>Filters volume-removed notifications.</summary>
    /// <returns>An observable sequence of removed volumes.</returns>
    public static IObservable<VolumeInfo> ObserveVolumeRemovals() => ObserveVolumeRemovals(DeviceNotificationEvents);

    /// <summary>Filters volume-removed notifications.</summary>
    /// <param name="observable">The source observable.</param>
    /// <returns>An observable sequence of removed volumes.</returns>
    public static IObservable<VolumeInfo> ObserveVolumeRemovals(IObservable<DeviceNotificationEvent> observable) => from volumeInfo in ObserveVolumeChanges(observable)
                                                                                                                    where volumeInfo.EventType == DeviceChangeEvent.DeviceRemoveComplete
                                                                                                                    select volumeInfo;

    /// <summary>Creates a composed device-notification observable.</summary>
    /// <param name="deviceInterfaceClass">The device interface class to observe.</param>
    /// <param name="listen">Creates the source window-message stream.</param>
    /// <param name="register">Registers device notifications for a window handle.</param>
    /// <param name="unregister">Unregisters a device-notification handle.</param>
    /// <returns>An observable sequence of device-notification events.</returns>
    internal static IObservable<DeviceNotificationEvent> ObserveDeviceNotifications(
        DeviceInterfaceClass deviceInterfaceClass,
        Func<Action<long>, Action<long>, IObservable<WindowMessage>> listen,
        Func<IntPtr, DevBroadcastDeviceInterface, DeviceNotifyFlags, IntPtr> register,
        Func<IntPtr, bool> unregister) =>
        ReactiveSignal.Create((IObserver<DeviceNotificationEvent> observer) =>
        {
            DevBroadcastDeviceInterface devBroadcastDeviceInterface = DevBroadcastDeviceInterface.Create();
            var deviceNotifyFlags = DeviceNotifyFlags.None;
            if (deviceInterfaceClass != DeviceInterfaceClass.Unknown)
            {
                devBroadcastDeviceInterface.DeviceClass = deviceInterfaceClass;
            }
            else
            {
                deviceNotifyFlags |= DeviceNotifyFlags.AllInterfaceClasses;
            }

            var deviceNotificationHandle = IntPtr.Zero;
            return (from message in listen(
                windowHandle =>
                {
                    deviceNotificationHandle = register((nint)windowHandle, devBroadcastDeviceInterface, deviceNotifyFlags);
                    if (deviceNotificationHandle == IntPtr.Zero)
                    {
                        observer.OnError(new Win32Exception());
                    }
                },
                teardownWindowHandle =>
                {
                    if (deviceNotificationHandle != IntPtr.Zero)
                    {
                        _ = unregister(deviceNotificationHandle);
                        deviceNotificationHandle = IntPtr.Zero;
                    }
                })
                    where message.Msg == WindowsMessages.WM_DEVICECHANGE
                        && message.LParam != 0
                    select message).Subscribe(
                message => observer.OnNext(new((nint)message.WParam, (nint)message.LParam)),
                observer.OnError,
                observer.OnCompleted);
        }).ShareLatest();

    /// <summary>Registers a device-notification recipient through the supplied registration operation.</summary>
    /// <param name="recipientHandle">The recipient window handle.</param>
    /// <param name="notificationFilter">The notification filter.</param>
    /// <param name="flags">The notification flags.</param>
    /// <param name="register">The native registration operation.</param>
    /// <returns>The device-notification handle.</returns>
    internal static IntPtr RegisterDeviceNotificationCore(
        IntPtr recipientHandle,
        DevBroadcastDeviceInterface notificationFilter,
        DeviceNotifyFlags flags,
        Func<IntPtr, IntPtr, DeviceNotifyFlags, IntPtr> register)
    {
        var notificationFilterPointer = Marshal.AllocHGlobal(Marshal.SizeOf<DevBroadcastDeviceInterface>());
        try
        {
            Marshal.StructureToPtr(notificationFilter, notificationFilterPointer, fDeleteOld: false);
            return register(recipientHandle, notificationFilterPointer, flags);
        }
        finally
        {
            Marshal.FreeHGlobal(notificationFilterPointer);
        }
    }

    /// <summary>Contains native device-notification methods.</summary>
    /// <param name="recipientHandle">The recipient window handle.</param>
    /// <param name="notificationFilter">The notification filter.</param>
    /// <param name="flags">The notification flags.</param>
    /// <returns>The device-notification handle.</returns>
    internal static IntPtr RegisterDeviceNotification(
        IntPtr recipientHandle,
        DevBroadcastDeviceInterface notificationFilter,
        DeviceNotifyFlags flags) =>
        RegisterDeviceNotificationCore(recipientHandle, notificationFilter, flags, _registerDeviceNotification);

    /// <summary>Overrides native device-notification registration for deterministic tests.</summary>
    /// <param name="registerDeviceNotification">The replacement registration operation.</param>
    /// <returns>The previous registration operation.</returns>
    internal static Func<IntPtr, IntPtr, DeviceNotifyFlags, IntPtr> SetRegisterDeviceNotificationForTesting(
        Func<IntPtr, IntPtr, DeviceNotifyFlags, IntPtr> registerDeviceNotification)
    {
        Throw.IfNull(registerDeviceNotification);
        var previousRegisterDeviceNotification = _registerDeviceNotification;
        _registerDeviceNotification = registerDeviceNotification;
        return previousRegisterDeviceNotification;
    }

    /// <summary>Contains native device-notification methods.</summary>
#if NET7_0_OR_GREATER
    private static partial class NativeMethods
#else
    private static class NativeMethods
#endif
    {
        /// <summary>The User32 library name.</summary>
        private const string User32Dll = "user32.dll";

        /// <summary>Registers a device-notification recipient.</summary>
        /// <param name="recipientHandle">The recipient window handle.</param>
        /// <param name="notificationFilter">The notification filter.</param>
        /// <param name="flags">The notification flags.</param>
        /// <returns>The device-notification handle.</returns>
#if NET7_0_OR_GREATER
        [LibraryImport(User32Dll, EntryPoint = "RegisterDeviceNotificationW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static partial IntPtr RegisterDeviceNotification(
            IntPtr recipientHandle,
            IntPtr notificationFilter,
            DeviceNotifyFlags flags);
#else
        [DllImport(User32Dll, EntryPoint = "RegisterDeviceNotificationW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr RegisterDeviceNotification(
            IntPtr recipientHandle,
            IntPtr notificationFilter,
            DeviceNotifyFlags flags);
#endif

        /// <summary>Unregisters a device-notification handle.</summary>
        /// <param name="handle">The device-notification handle.</param>
        /// <returns><c>true</c> if unregistration succeeded; otherwise <c>false</c>.</returns>
#if NET7_0_OR_GREATER
        [LibraryImport(User32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool UnregisterDeviceNotification(IntPtr handle);
#else
        [DllImport(User32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterDeviceNotification(IntPtr handle);
#endif
    }
}
