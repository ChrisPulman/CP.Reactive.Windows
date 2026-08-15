// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using Microsoft.Win32;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Functionality to use the RawInput API.</summary>
public static class RawInputApi
{
    /// <summary>The number of bytes in a UTF-16 code unit.</summary>
    private const int UnicodeCharacterSize = 2;

    /// <summary>The number of characters before the useful device path segments.</summary>
    private const int DeviceNamePrefixLength = 4;

    /// <summary>The shared error message for GetRawInputDeviceInfo failures.</summary>
    private const string GetRawInputDeviceInfoFailure = "Calling GetRawInputDeviceInfo";

    /// <summary>The raw-input native API used by this process.</summary>
    private static IRawInputNativeApi _nativeApi = new WindowsRawInputNativeApi();

    /// <summary>Creates a raw input device with the default input sink flag.</summary>
    /// <param name="windowHandle">The window handle which handles the messages.</param>
    /// <param name="device">The raw input device.</param>
    /// <returns>The raw input device registration data.</returns>
    public static RawInputDevice CreateRawInputDevice(IntPtr windowHandle, RawInputDevices device) => CreateRawInputDevice(windowHandle, device, RawInputDeviceFlags.InputSink);

    /// <summary>Creates a generic raw input device with the default input sink flag.</summary>
    /// <param name="windowHandle">The window handle which handles the messages.</param>
    /// <param name="usage">The generic raw input usage.</param>
    /// <returns>The raw input device registration data.</returns>
    public static RawInputDevice CreateRawInputDevice(IntPtr windowHandle, HidUsagesGeneric usage) => CreateRawInputDevice(windowHandle, usage, RawInputDeviceFlags.InputSink);

    /// <summary>Creates a consumer raw input device with the default input sink flag.</summary>
    /// <param name="windowHandle">The window handle which handles the messages.</param>
    /// <param name="usage">The consumer raw input usage.</param>
    /// <returns>The raw input device registration data.</returns>
    public static RawInputDevice CreateRawInputDevice(IntPtr windowHandle, HidUsagesConsumer usage) => CreateRawInputDevice(windowHandle, usage, RawInputDeviceFlags.InputSink);

    /// <summary>Create RawInputDevice.</summary>
    /// <param name="windowHandle">IntPtr with the window handle which handles the messages.</param>
    /// <param name="device">RawInputDevices.</param>
    /// <param name="flags">RawInputDeviceFlags.</param>
    /// <returns>RawInputDevice filled.</returns>
    public static RawInputDevice CreateRawInputDevice(IntPtr windowHandle, RawInputDevices device, RawInputDeviceFlags flags) => device switch
        {
            RawInputDevices.Pointer => CreateRawInputDevice(windowHandle, HidUsagesGeneric.Pointer, flags),
            RawInputDevices.Mouse => CreateRawInputDevice(windowHandle, HidUsagesGeneric.Mouse, flags),
            RawInputDevices.Joystick => CreateRawInputDevice(windowHandle, HidUsagesGeneric.Joystick, flags),
            RawInputDevices.GamePad => CreateRawInputDevice(windowHandle, HidUsagesGeneric.Gamepad, flags),
            RawInputDevices.Keyboard => CreateRawInputDevice(windowHandle, HidUsagesGeneric.Keyboard, flags),
            RawInputDevices.Keypad => CreateRawInputDevice(windowHandle, HidUsagesGeneric.Keypad, flags),
            RawInputDevices.SystemControl => CreateRawInputDevice(windowHandle, HidUsagesGeneric.SystemControl, flags),
            RawInputDevices.ConsumerAudioControl => CreateRawInputDevice(windowHandle, HidUsagesConsumer.ConsumerControl, flags),
            _ => throw new NotSupportedException($"Unknown RawInputDevices: {device}"),
        };

    /// <summary>Create RawInputDevice, to use with RegisterRawInput.</summary>
    /// <param name="windowHandle">IntPtr with the window handle which handles the messages.</param>
    /// <param name="usage">Generic Usage for the raw input device.</param>
    /// <param name="flags">RawInputDeviceFlags.</param>
    /// <returns>RawInputDevice filled.</returns>
    public static RawInputDevice CreateRawInputDevice(IntPtr windowHandle, HidUsagesGeneric usage, RawInputDeviceFlags flags) => new RawInputDevice
        {
            TargetHwnd = windowHandle,
            Flags = flags,
            UsagePage = HidUsagePages.Generic,
            Usage = checked((ushort)usage)
        };

    /// <summary>Create RawInputDevice, to use with RegisterRawInput.</summary>
    /// <param name="windowHandle">IntPtr with the window handle which handles the messages.</param>
    /// <param name="usage">Consumer Usage for the raw input device.</param>
    /// <param name="flags">RawInputDeviceFlags.</param>
    /// <returns>RawInputDevice filled.</returns>
    public static RawInputDevice CreateRawInputDevice(IntPtr windowHandle, HidUsagesConsumer usage, RawInputDeviceFlags flags) => new RawInputDevice
        {
            TargetHwnd = windowHandle,
            Flags = flags,
            UsagePage = HidUsagePages.Consumer,
            Usage = checked((ushort)usage)
        };

    /// <summary>Register the specified window to receive raw input, coming from the specified device.</summary>
    /// <param name="windowHandle">IntPtr for the window to receive the events.</param>
    /// <param name="flags">RawInputDeviceFlags.</param>
    /// <param name="devices">one or more RawInputDevices.</param>
    public static void RegisterRawInput(IntPtr windowHandle, RawInputDeviceFlags flags, params RawInputDevices[] devices)
    {
        RawInputDevice[] rawInputDevices = new RawInputDevice[devices.Length];
        for (int index = 0; index < devices.Length; index = checked(index + 1))
        {
            IntPtr targetWindowHandle = (((flags & RawInputDeviceFlags.Remove) == RawInputDeviceFlags.Remove) ? IntPtr.Zero : windowHandle);
            rawInputDevices[index] = CreateRawInputDevice(targetWindowHandle, devices[index], flags);
        }

        RegisterRawInput(rawInputDevices);
    }

    /// <summary>
    /// Register to handle RawInput events. To receive WM_INPUT messages, an application must first register the raw input
    /// devices using RegisterRawInputDevices. WM_INPUT_DEVICE_CHANGE messages require the RIDEV_DEVNOTIFY flag for each
    /// registered device class. If RIDEV_REMOVE is set and the target window member is not NULL, parameter validation fails.
    /// </summary>
    /// <param name="rawInputDevices">RawInputDevice(s) specifying what to register.</param>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Win32Exception when the registration failed.</exception>
    public static void RegisterRawInput(params RawInputDevice[] rawInputDevices)
    {
        if (!_nativeApi.RegisterRawInputDevices(rawInputDevices, rawInputDevices.Length, Marshal.SizeOf<RawInputDevice>()))
        {
            throw new Win32Exception();
        }
    }

    /// <summary>
    /// Retrieve RawInputDeviceInformation on the by the handle specified RawInput device
    /// This is used when calling GetAllDevices, but can also be called when getting a WM_INPUT_DEVICE_CHANGE message.
    /// </summary>
    /// <param name="handle">IntPtr handle to the raw input device.</param>
    /// <returns>RawInputDeviceInformation.</returns>
    public static RawInputDeviceInformation GetDeviceInformation(IntPtr handle)
    {
        RawInputDeviceInformation result = new RawInputDeviceInformation { Handle = handle };
        uint pcbSize = 0U;
        if (_nativeApi.GetRawInputDeviceInfo(handle, RawInputDeviceInfoCommands.DeviceName, IntPtr.Zero, ref pcbSize) == uint.MaxValue)
        {
            throw new Win32Exception("Calling GetRawInputDeviceInfo");
        }

        checked
        {
            if (pcbSize != 0)
            {
                IntPtr deviceNamePtr = Marshal.AllocHGlobal((int)pcbSize * 2);
                try
                {
                    if (_nativeApi.GetRawInputDeviceInfo(handle, RawInputDeviceInfoCommands.DeviceName, deviceNamePtr, ref pcbSize) == uint.MaxValue)
                    {
                        throw new Win32Exception("Calling GetRawInputDeviceInfo");
                    }

                    result.DeviceName = Marshal.PtrToStringUni(deviceNamePtr);
                    result.DisplayName = GetDisplayName(result.DeviceName);
                }
                finally
                {
                    Marshal.FreeHGlobal(deviceNamePtr);
                }
            }

            if (_nativeApi.GetRawInputDeviceInfo(handle, RawInputDeviceInfoCommands.DeviceInfo, IntPtr.Zero, ref pcbSize) == uint.MaxValue)
            {
                throw new Win32Exception("Calling GetRawInputDeviceInfo");
            }

            IntPtr deviceInfoPtr = Marshal.AllocHGlobal((int)pcbSize);
            try
            {
                if (_nativeApi.GetRawInputDeviceInfo(handle, RawInputDeviceInfoCommands.DeviceInfo, deviceInfoPtr, ref pcbSize) == uint.MaxValue)
                {
                    throw new Win32Exception("Calling GetRawInputDeviceInfo");
                }

                result.DeviceInfo = Marshal.PtrToStructure<RawInputDeviceInfo>(deviceInfoPtr);
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(deviceInfoPtr);
            }
        }
    }

    /// <summary>
    /// A convenient function for getting all raw input devices.
    /// This method will get all devices, including virtual devices-
    /// For remote desktop and any other device driver that's registered as such a device.
    /// </summary>
    /// <returns>The currently registered raw input devices.</returns>
    public static IEnumerable<RawInputDeviceInformation> GetAllDevices()
    {
        uint deviceCount = 0U;
        uint deviceListSize = checked((uint)Marshal.SizeOf<RawInputDeviceList>());
        if (_nativeApi.GetRawInputDeviceList(null, ref deviceCount, deviceListSize) == 0 && deviceCount != 0)
        {
            RawInputDeviceList[] deviceList = new RawInputDeviceList[deviceCount];
            if (_nativeApi.GetRawInputDeviceList(deviceList, ref deviceCount, deviceListSize) == uint.MaxValue)
            {
                throw new Win32Exception("Exception when calling GetRawInputDeviceList");
            }

            RawInputDeviceList[] array = deviceList;
            foreach (RawInputDeviceList rawInputDeviceList in array)
            {
                yield return GetDeviceInformation(rawInputDeviceList.Handle);
            }
        }
    }

    /// <summary>
    /// GetRawInputData function
    /// Retrieves the raw input from the specified device.
    /// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645596.aspx">GetRawInputData function</a>
    /// </summary>
    /// <param name="rawInputHandle">IntPtr to a RawInput struct.</param>
    /// <param name="command">RawInputDataCommands.</param>
    /// <param name="data">The retrieved raw input data.</param>
    /// <param name="dataSize">The data size.</param>
    /// <param name="headerSize">The header size.</param>
    /// <returns>The number of bytes copied.</returns>
    public static int GetRawInputData(IntPtr rawInputHandle, RawInputDataCommands command, out RawInput data, ref int dataSize, int headerSize)
    {
        data = default;
        return _nativeApi.GetRawInputData(rawInputHandle, command, ref data, ref dataSize, headerSize);
    }

    /// <summary>Overrides the native raw-input API for deterministic tests.</summary>
    /// <param name="api">The replacement raw-input API.</param>
    /// <returns>The previous raw-input API.</returns>
    internal static IRawInputNativeApi SetNativeApiForTesting(IRawInputNativeApi api)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(api);
        IRawInputNativeApi nativeApi = _nativeApi;
        _nativeApi = api;
        return nativeApi;
    }

    /// <summary>Gets a display name for a raw input device path.</summary>
    /// <param name="deviceName">The raw input device path.</param>
    /// <returns>The display name, or <see langword="null" /> when one cannot be found.</returns>
    internal static string GetDisplayName(string deviceName)
    {
        if (string.IsNullOrEmpty(deviceName) || deviceName.Length <= 4)
        {
            return null;
        }

        string[] split = deviceName.Substring(DeviceNamePrefixLength).Split('#');
        if (split.Length <= 2)
        {
            return deviceName;
        }

        using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey($"System\\CurrentControlSet\\Enum\\{split[0]}\\{split[1]}\\{split[2]}");
        string deviceDescription = (string)registryKey?.GetValue("DeviceDesc");
        int? startOfDisplayName = deviceDescription?.LastIndexOf(";", StringComparison.Ordinal);
        return (startOfDisplayName >= 0) ? deviceDescription.Substring(checked(startOfDisplayName.Value + 1)) : null;
    }
}
