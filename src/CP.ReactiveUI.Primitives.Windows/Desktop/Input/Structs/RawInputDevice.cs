// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     This struct contains information about a raw input device.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645565.aspx">RAWINPUTDEVICE structure</a>
/// Remarks:
/// If RIDEV_NOLEGACY is set for a mouse or a keyboard, the system does not generate any legacy message for that device for the application.
/// For example, if the mouse TLC is set with RIDEV_NOLEGACY, WM_LBUTTONDOWN and related legacy mouse messages are not generated.
/// Likewise, if the keyboard TLC is set with RIDEV_NOLEGACY, WM_KEYDOWN and related legacy keyboard messages are not generated.
/// If RIDEV_REMOVE is set and the hWndTarget member is not set to NULL, then parameter validation will fail.
/// </summary>
public readonly record struct RawInputDevice
{
    /// <summary>Gets or sets top level collection Usage page for the raw input device.</summary>
    public HidUsagePages UsagePage { get; init; }

    /// <summary>Gets or sets top level collection Usage for the raw input device.</summary>
    public ushort Usage { get; init; }

    /// <summary>
    /// Gets or sets mode flag that specifies how to interpret the information provided by usUsagePage and usUsage.
    /// It can be zero (the default) or one of the following values.
    /// By default, the operating system sends raw input from devices with the specified top level collection (TLC) to the registered application as long as it has the window focus.
    /// </summary>
    public RawInputDeviceFlags Flags { get; init; }

    /// <summary>Gets a handle to the target window. If NULL it follows the keyboard focus.</summary>
    internal IntPtr TargetHwnd { get; init; }

    /// <summary>Returns the target window handle.</summary>
    /// <returns>The target window handle.</returns>
    public IntPtr ToIntPtr() => TargetHwnd;

    /// <inheritdoc />
    public override string ToString() => $"{UsagePage}/{Usage}, flags: {Flags}, target window: {TargetHwnd}";
}
