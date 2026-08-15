// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Provides the raw-input native boundary.</summary>
internal static class RawInputNativeMethods
{
    /// <summary>Contains native raw-input entry points.</summary>
    private static class NativeMethods
    {
        /// <summary>Gets the list of raw input devices attached to the system.</summary>
        /// <param name="rawInputDeviceList">The destination device list.</param>
        /// <param name="numDevices">The number of devices.</param>
        /// <param name="size">The size of each list item.</param>
        /// <returns>The number of copied devices.</returns>
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern uint GetRawInputDeviceList([In][Out] RawInputDeviceList[] rawInputDeviceList, ref uint numDevices, uint size);

        /// <summary>Gets information for a raw input device.</summary>
        /// <param name="deviceHandle">The raw input device handle.</param>
        /// <param name="command">The requested information command.</param>
        /// <param name="deviceName">The destination data pointer.</param>
        /// <param name="dataSize">The destination data size.</param>
        /// <returns>The number of copied bytes, or uint.MaxValue on failure.</returns>
        [DllImport("user32", CharSet = CharSet.Unicode, EntryPoint = "GetRawInputDeviceInfoW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern uint GetRawInputDeviceInfo(IntPtr deviceHandle, RawInputDeviceInfoCommands command, IntPtr deviceName, ref uint dataSize);

        /// <summary>Registers raw input devices for a window.</summary>
        /// <param name="rawInputDevices">The raw input devices to register.</param>
        /// <param name="numberOfDevices">The number of devices.</param>
        /// <param name="size">The size of each device registration.</param>
        /// <returns><see langword="true" /> when registration succeeds.</returns>
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RegisterRawInputDevices([In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] RawInputDevice[] rawInputDevices, int numberOfDevices, int size);

        /// <summary>Gets raw input data for a raw input message handle.</summary>
        /// <param name="rawInputHandle">The raw input message handle.</param>
        /// <param name="command">The requested data command.</param>
        /// <param name="data">The destination raw input pointer.</param>
        /// <param name="size">The destination size pointer.</param>
        /// <param name="headerSize">The raw input header size.</param>
        /// <returns>The number of copied bytes.</returns>
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern unsafe int GetRawInputData(IntPtr rawInputHandle, RawInputDataCommands command, RawInput* data, int* size, int headerSize);
    }

    /// <summary>Gets the list of raw input devices attached to the system.</summary>
    /// <param name="rawInputDeviceList">The destination device list.</param>
    /// <param name="numDevices">The number of devices.</param>
    /// <param name="size">The size of each list item.</param>
    /// <returns>The number of copied devices.</returns>
    internal static uint GetRawInputDeviceList(RawInputDeviceList[] rawInputDeviceList, ref uint numDevices, uint size) => NativeMethods.GetRawInputDeviceList(rawInputDeviceList, ref numDevices, size);

    /// <summary>Gets information for a raw input device.</summary>
    /// <param name="deviceHandle">The raw input device handle.</param>
    /// <param name="command">The requested information command.</param>
    /// <param name="deviceName">The destination data pointer.</param>
    /// <param name="dataSize">The destination data size.</param>
    /// <returns>The number of copied bytes, or uint.MaxValue on failure.</returns>
    internal static uint GetRawInputDeviceInfo(IntPtr deviceHandle, RawInputDeviceInfoCommands command, IntPtr deviceName, ref uint dataSize) => NativeMethods.GetRawInputDeviceInfo(deviceHandle, command, deviceName, ref dataSize);

    /// <summary>Registers raw input devices for a window.</summary>
    /// <param name="rawInputDevices">The raw input devices to register.</param>
    /// <param name="numberOfDevices">The number of devices.</param>
    /// <param name="size">The size of each device registration.</param>
    /// <returns><see langword="true" /> when registration succeeds.</returns>
    internal static bool RegisterRawInputDevices(RawInputDevice[] rawInputDevices, int numberOfDevices, int size) => NativeMethods.RegisterRawInputDevices(rawInputDevices, numberOfDevices, size);

    /// <summary>Gets raw input data for a raw input message handle.</summary>
    /// <param name="rawInputHandle">The raw input message handle.</param>
    /// <param name="command">The requested data command.</param>
    /// <param name="data">The destination raw input pointer.</param>
    /// <param name="size">The destination size pointer.</param>
    /// <param name="headerSize">The raw input header size.</param>
    /// <returns>The number of copied bytes.</returns>
    internal static unsafe int GetRawInputData(IntPtr rawInputHandle, RawInputDataCommands command, RawInput* data, int* size, int headerSize) => NativeMethods.GetRawInputData(rawInputHandle, command, data, size, headerSize);
}
