// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI.Primitives.Disposables;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>
/// Reactive access to RawInput device information and changes.
/// This class provides an observable stream of device change events, and a cache of currently known devices.
/// </summary>
public static class RawInputDeviceMonitor
{
    /// <summary>Synchronizes access to raw-input monitor state.</summary>
    private static readonly object SyncRoot = new();

    /// <summary>The raw-input device cache keyed by native device handle.</summary>
    private static readonly Dictionary<IntPtr, RawInputDeviceInformation> DeviceCache = new();

    /// <summary>The shared raw-input device change observable.</summary>
    private static IObservable<RawInputDeviceChangeEventArgs> _rawInputDeviceObservable;

    /// <summary>Gets a snapshot of the raw-input devices currently in the system.</summary>
    /// <returns>A point-in-time raw-input device snapshot keyed by device handle.</returns>
    public static IReadOnlyDictionary<IntPtr, RawInputDeviceInformation> GetDevicesSnapshot()
    {
        lock (SyncRoot)
        {
            RefreshDeviceCache();
            return new ReadOnlyDictionary<IntPtr, RawInputDeviceInformation>(new Dictionary<IntPtr, RawInputDeviceInformation>(DeviceCache));
        }
    }

    /// <summary>
    /// Observes Raw Input device arrival and removal notifications.
    /// Multiple subscribers will share the same underlying hook, and the hook
    /// is automatically disposed when the subscriber count reaches zero.
    /// </summary>
    /// <param name="devices">The raw input device types to register.</param>
    /// <returns>A shared stream of raw input device change events.</returns>
    public static IObservable<RawInputDeviceChangeEventArgs> ObserveDeviceChanges(params RawInputDevices[] devices)
    {
        if (_rawInputDeviceObservable is not null)
        {
            return _rawInputDeviceObservable;
        }

        _rawInputDeviceObservable = ReactiveSignal.CreateSafe(delegate(IObserver<RawInputDeviceChangeEventArgs> observer)
        {
            IDisposable messageSubscription = RawInputMonitor.Infrastructure.MessageSource.Messages.Where((windowsMessage) => windowsMessage.Msg == WindowsMessages.WM_INPUT_DEVICE_CHANGE).Subscribe(delegate(WindowMessage windowsMessage)
            {
                windowsMessage.Handled = true;
                bool flag = checked((int)windowsMessage.WParam) == 1;
                IntPtr intPtr = new(windowsMessage.LParam);
                lock (SyncRoot)
                {
                    RawInputDeviceInformation value;
                    if (flag)
                    {
                        value = RawInputApi.GetDeviceInformation(intPtr);
                        DeviceCache[intPtr] = value;
                    }
                    else if (!DeviceCache.TryGetValue(intPtr, out value))
                    {
                        value = new RawInputDeviceInformation { Handle = intPtr };
                    }

                    observer.OnNext(new RawInputDeviceChangeEventArgs { Added = flag, DeviceInformation = value });
                    if (!flag)
                    {
                        _ = DeviceCache.Remove(intPtr);
                    }
                }
            });
            lock (SyncRoot)
            {
                RefreshDeviceCache();
            }

            IDisposable registrationSubscription = (from windowHandle in RawInputMonitor.Infrastructure.MessageSource.ObserveHandleChanges()
                where windowHandle != 0
                select windowHandle).Take(1).Subscribe(delegate(long windowHandle)
            {
                RawInputApi.RegisterRawInput((nint)windowHandle, RawInputDeviceFlags.DeviceNotify, devices);
            });
            return new ActionDisposable(delegate
            {
                _rawInputDeviceObservable = null;
                registrationSubscription.Dispose();
                messageSubscription.Dispose();
            });
        }).Publish().RefCount();
        return _rawInputDeviceObservable;
    }

    /// <summary>Resets cached raw-input device monitor state for deterministic tests.</summary>
    internal static void ResetForTesting()
    {
        lock (SyncRoot)
        {
            _rawInputDeviceObservable = null;
            DeviceCache.Clear();
        }
    }

    /// <summary>Refreshes the device cache from the operating system.</summary>
    private static void RefreshDeviceCache()
    {
        DeviceCache.Clear();
        foreach (RawInputDeviceInformation deviceInformation in RawInputApi.GetAllDevices())
        {
            DeviceCache[deviceInformation.Handle] = deviceInformation;
        }
    }
}
