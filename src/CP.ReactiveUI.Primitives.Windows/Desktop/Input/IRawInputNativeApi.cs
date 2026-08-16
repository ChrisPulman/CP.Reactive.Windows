// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Native raw-input methods.</summary>
internal interface IRawInputNativeApi
{
    /// <summary>Gets the list of raw input devices attached to the system.</summary>
    /// <param name="rawInputDeviceList">The destination device list.</param>
    /// <param name="numDevices">The number of devices.</param>
    /// <param name="size">The size of each list item.</param>
    /// <returns>The number of copied devices.</returns>
    uint GetRawInputDeviceList(RawInputDeviceList[] rawInputDeviceList, ref uint numDevices, uint size);

    /// <summary>Gets information for a raw input device.</summary>
    /// <param name="deviceHandle">The raw input device handle.</param>
    /// <param name="command">The requested information command.</param>
    /// <param name="deviceName">The destination data pointer.</param>
    /// <param name="dataSize">The destination data size.</param>
    /// <returns>The number of copied bytes, or uint.MaxValue on failure.</returns>
    uint GetRawInputDeviceInfo(IntPtr deviceHandle, RawInputDeviceInfoCommands command, IntPtr deviceName, ref uint dataSize);

    /// <summary>Registers raw input devices for a window.</summary>
    /// <param name="rawInputDevices">The raw input devices to register.</param>
    /// <param name="numberOfDevices">The number of devices.</param>
    /// <param name="size">The size of each device registration.</param>
    /// <returns><see langword="true" /> when registration succeeds.</returns>
    bool RegisterRawInputDevices(RawInputDevice[] rawInputDevices, int numberOfDevices, int size);

    /// <summary>Gets raw input data for a raw input message handle.</summary>
    /// <param name="rawInputHandle">The raw input message handle.</param>
    /// <param name="command">The requested data command.</param>
    /// <param name="data">The destination raw input data.</param>
    /// <param name="size">The destination size.</param>
    /// <param name="headerSize">The raw input header size.</param>
    /// <returns>The number of copied bytes.</returns>
    int GetRawInputData(IntPtr rawInputHandle, RawInputDataCommands command, ref RawInput data, ref int size, int headerSize);
}
