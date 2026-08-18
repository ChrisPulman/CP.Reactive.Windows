// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     This structdefines the raw input data coming from any device.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645581(v=vs.85).aspx">RID_DEVICE_INFO structure</a>
/// Remarks:
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly record struct RawInputDeviceInfo
{
    /// <summary>Stores the native structure size.</summary>
    [FieldOffset(0)]
    private readonly int _size;

    /// <summary>Stores the native raw input device type.</summary>
    [FieldOffset(4)]
    private readonly RawInputDeviceTypes _type;

    /// <summary>Stores mouse device information.</summary>
    [FieldOffset(8)]
    private readonly RawInputDeviceInfoMouse _mouse;

    /// <summary>Stores keyboard device information.</summary>
    [FieldOffset(8)]
    private readonly RawInputDeviceInfoKeyboard _keyboard;

    /// <summary>Stores HID device information.</summary>
    [FieldOffset(8)]
    private readonly RawInputDeviceInfoHID _hid;

    /// <summary>Initializes a new instance of the <see cref="RawInputDeviceInfo"/> struct for the supplied device type.</summary>
    /// <param name="type">The raw-input device type.</param>
    internal RawInputDeviceInfo(RawInputDeviceTypes type)
    {
        _size = Marshal.SizeOf<RawInputDeviceInfo>();
        _type = type;
        _mouse = default;
        _keyboard = default;
        _hid = default;
    }

    /// <summary>Gets the type RawInput device.</summary>
    public RawInputDeviceTypes Type => _type;

    /// <summary>Gets information on the mouse device.</summary>
    public RawInputDeviceInfoMouse Mouse
    {
        get
        {
            if (_type != RawInputDeviceTypes.Mouse)
            {
                throw new NotSupportedException($"The RawInputDeviceInfo contains info on {_type} and not on mouse.");
            }

            return _mouse;
        }
    }

    /// <summary>Gets information on the keyboard device.</summary>
    public RawInputDeviceInfoKeyboard Keyboard
    {
        get
        {
            if (_type != RawInputDeviceTypes.Keyboard)
            {
                throw new NotSupportedException($"The RawInputDeviceInfo contains info on {_type} and not on keyboard.");
            }

            return _keyboard;
        }
    }

    /// <summary>Gets information on the HID device.</summary>
    public RawInputDeviceInfoHID HID
    {
        get
        {
            if (_type != RawInputDeviceTypes.HID)
            {
                throw new NotSupportedException($"The RawInputDeviceInfo contains info on {_type} and not on HID.");
            }

            return _hid;
        }
    }

    /// <summary>Gets the native structure size.</summary>
    internal int Size => _size;
}
