// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Windows-backed raw-input native API.</summary>
internal sealed class WindowsRawInputNativeApi : IRawInputNativeApi
{
    /// <inheritdoc />
    public uint GetRawInputDeviceList(RawInputDeviceList[] rawInputDeviceList, ref uint numDevices, uint size) =>
        RawInputNativeMethods.GetRawInputDeviceList(rawInputDeviceList, ref numDevices, size);

    /// <inheritdoc />
    public uint GetRawInputDeviceInfo(
        IntPtr deviceHandle,
        RawInputDeviceInfoCommands command,
        IntPtr deviceName,
        ref uint dataSize) =>
        RawInputNativeMethods.GetRawInputDeviceInfo(deviceHandle, command, deviceName, ref dataSize);

    /// <inheritdoc />
    public bool RegisterRawInputDevices(RawInputDevice[] rawInputDevices, int numberOfDevices, int size) =>
        RawInputNativeMethods.RegisterRawInputDevices(rawInputDevices, numberOfDevices, size);

    /// <inheritdoc />
    public unsafe int GetRawInputData(IntPtr rawInputHandle, RawInputDataCommands command, ref RawInput data, ref int size, int headerSize)
    {
        fixed (RawInput* dataPointer = &data)
        {
            fixed (int* sizePointer = &size)
            {
                return RawInputNativeMethods.GetRawInputData(rawInputHandle, command, dataPointer, sizePointer, headerSize);
            }
        }
    }
}
