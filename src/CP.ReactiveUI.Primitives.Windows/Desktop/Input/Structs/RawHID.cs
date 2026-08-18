// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     Describes the format of the raw input from a Human Interface Device (HID).
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645549.aspx">RAWHID structure</a>
/// </summary>
public readonly record struct RawHID
{
    /// <summary>The size, in bytes, of each HID input in the raw data buffer.</summary>
    private readonly uint _inputSize;

    /// <summary>The number of HID inputs in the raw data buffer.</summary>
    private readonly uint _inputCount;

    /// <summary>The raw input data pointer.</summary>
    private readonly IntPtr _rawData;

    /// <summary>Returns the raw input data, as an array of bytes.</summary>
    /// <returns>The raw input data bytes.</returns>
    public byte[] GetData()
    {
        var data = new byte[checked(_inputSize * _inputCount)];
        if (data.Length == 0 || _rawData == IntPtr.Zero)
        {
            return data;
        }

        Marshal.Copy(_rawData, data, 0, data.Length);
        return data;
    }
}
