// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     This struct defines the raw input data coming from the specified keyboard.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645587.aspx">RID_DEVICE_INFO_KEYBOARD structure</a>
/// Remarks:
/// For the keyboard, the Usage Page is 1 and the Usage is 6.
/// </summary>
public readonly record struct RawInputDeviceInfoKeyboard
{
    /// <summary>Gets the type of the keyboard.</summary>
    public int Type { get; }

    /// <summary>Gets the subtype of the keyboard.</summary>
    public int SubType { get; }

    /// <summary>Gets the scan code mode.</summary>
    public int KeyboardMode { get; }

    /// <summary>Gets the number of function keys on the keyboard.</summary>
    public int NumberOfFunctionKeys { get; }

    /// <summary>Gets the number of LED indicators on the keyboard.</summary>
    public int NumberOfIndicators { get; }

    /// <summary>Gets the total number of keys on the keyboard.</summary>
    public int NumberOfKeysTotal { get; }
}
