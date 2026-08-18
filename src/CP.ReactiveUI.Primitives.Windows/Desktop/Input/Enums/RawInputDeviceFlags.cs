// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>
///     Mode flag that specifies how to interpret the information provided by usUsagePage and usUsage.
///     It can be zero (the default) or one of the following values. By default, the operating system sends raw input from
///     devices with the specified top level collection (TLC) to the registered application as long as it has the window focus.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645565.aspx">RAWINPUTDEVICE structure</a>
/// </summary>
[Flags]
public enum RawInputDeviceFlags
{
    /// <summary>
    ///     RIDEV_REMOVE: If set, this removes the top level collection from the inclusion list.
    ///     This tells the operating system to stop reading from a device which matches the top level collection.
    /// </summary>
    None = 0,
    /// <summary>
    ///     RIDEV_REMOVE: If set, this removes the top level collection from the inclusion list.
    ///     This tells the operating system to stop reading from a device which matches the top level collection.
    /// </summary>
    Remove = 1,
    /// <summary>
    ///     RIDEV_EXCLUDE: If set, this specifies the top level collections to exclude when reading a complete usage page.
    ///     This flag only affects a TLC whose usage page is already specified with RIDEV_PAGEONLY.
    /// </summary>
    Exclude = 0x10,
    /// <summary>
    ///     RIDEV_PAGEONLY: If set, this specifies all devices whose top level collection is from the specified usUsagePage.
    ///     Note that usUsage must be zero.
    ///     To exclude a particular top level collection, use RIDEV_EXCLUDE.
    /// </summary>
    PageOnly = 0x20,
    /// <summary>
    ///     RIDEV_NOLEGACY: If set, this prevents any devices specified by usUsagePage or usUsage from generating legacy messages.
    ///     This is only for the mouse and keyboard. See Remarks.
    /// </summary>
    NoLegacy = Exclude | PageOnly,
    /// <summary>
    ///     RIDEV_INPUTSINK: If set, this enables the caller to receive the input even when the caller is not in the foreground.
    ///     Note that hWndTarget must be specified.
    /// </summary>
    InputSink = 0x100,
    /// <summary>
    ///     RIDEV_NOHOTKEYS: If set, the application-defined keyboard device hotkeys are not handled.
    ///     However, the system hotkeys; for example, ALT+TAB and CTRL+ALT+DEL, are still handled.
    ///     By default, all keyboard hotkeys are handled.
    ///     RIDEV_NOHOTKEYS can be specified even if RIDEV_NOLEGACY is not specified and hWndTarget is NULL.
    /// </summary>
    NoHotkeys = 0x200,
    /// <summary>
    ///     RIDEV_APPKEYS: If set, the application command keys are handled.
    ///     RIDEV_APPKEYS can be specified only if RIDEV_NOLEGACY is specified for a keyboard device.
    /// </summary>
    AppKeys = 0x400,
    /// <summary>
    ///     RIDEV_EXINPUTSINK: If set, this enables the caller to receive input in the background only if the foreground application does not process it.
    ///     In other words, if the foreground application is not registered for raw input, then the background application that is registered will receive the input.
    ///     Windows XP:  This flag is not supported until Windows Vista.
    /// </summary>
    ExInputSink = 0x1000,
    /// <summary>
    ///     RIDEV_DEVNOTIFY: If set, this enables the caller to receive WM_INPUT_DEVICE_CHANGE notifications for device arrival and device removal.
    ///     Windows XP:  This flag is not supported until Windows Vista.
    /// </summary>
    DeviceNotify = 0x2000,
}
