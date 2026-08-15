// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Power.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Power.Enums;
#endif
/// <summary>
/// Power broadcast events (wordParameter values for WM_POWERBROADCAST).
/// See <a href="https://learn.microsoft.com/en-us/windows/win32/power/wm-powerbroadcast">WM_POWERBROADCAST message</a>
/// </summary>
public enum PowerBroadcastEvent : uint
{
    /// <summary>No power broadcast event.</summary>
    None = 0U,
    /// <summary>The system is about to suspend. Applications should save any data they need before the system suspends.</summary>
    PBT_APMSUSPEND = 4U,
    /// <summary>
    /// The system has resumed operation after being suspended.
    /// This event is broadcast when user activity or an application is detected on the resumed system,
    /// or if the user presses the power button.
    /// </summary>
    PBT_APMRESUMESUSPEND = 7U,
    /// <summary>Battery power is low.</summary>
    PBT_APMBATTERYLOW = 9U,
    /// <summary>
    /// A change in the power status of the computer is detected, such as a switch from battery power to AC.
    /// The system also broadcasts this event when remaining battery power slips below the threshold
    /// specified by the user or if the battery power changes by a specified percentage.
    /// </summary>
    PBT_APMPOWERSTATUSCHANGE = 10U,
    /// <summary>The system has resumed operation after a critical suspension caused by a failing battery.</summary>
    PBT_APMRESUMEDCRITICAL = 6U,
    /// <summary>
    /// The system has resumed operation automatically to handle an event.
    /// Applications that receive the PBT_APMRESUMEAUTOMATIC event are not allowed to interact with the user.
    /// If the user is present, another PBT_APMRESUMESUSPEND event will follow.
    /// See <a href="https://learn.microsoft.com/en-us/windows/win32/power/system-wake-up-events">System Wake-Up Events</a>
    /// </summary>
    PBT_APMRESUMEAUTOMATIC = 18U,
    /// <summary>A power setting change event has been received. The longParameter parameter points to a POWERBROADCAST_SETTING structure.</summary>
    PBT_POWERSETTINGCHANGE = 32_787U,
}
