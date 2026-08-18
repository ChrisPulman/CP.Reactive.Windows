// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Power;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Power;
#endif
/// <summary>
/// Provides an observable stream of power broadcast events from the Windows message queue.
/// Listens for <see cref="F:CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.Enumerations.WindowsMessages.WM_POWERBROADCAST" /> messages and exposes them as
/// an <see cref="T:System.IObservable`1" /> of <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Power.Enums.PowerBroadcastEvent" /> values.
/// See <a href="https://learn.microsoft.com/en-us/windows/win32/power/wm-powerbroadcast">WM_POWERBROADCAST message</a>
/// and <a href="https://learn.microsoft.com/en-us/windows/win32/power/system-wake-up-events">System Wake-Up Events</a>
/// </summary>
public static class PowerBroadcastListener
{
    /// <summary>The shared observable sequence of power broadcast events.</summary>
    private static readonly IObservable<PowerBroadcastEvent> BroadcastEvents;

    /// <summary>Initializes static members of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Power.PowerBroadcastListener" /> class.</summary>
    static PowerBroadcastListener()
    {
        BroadcastEvents = CreatePowerBroadcastEvents(SharedMessageWindow.WindowMessageEvents);
    }

    /// <summary>
    /// Gets an observable sequence of power broadcast events.
    /// Subscribe to this to be notified of system power state changes such as
    /// suspend, resume, battery status changes, etc.
    /// </summary>
    public static IObservable<PowerBroadcastEvent> PowerBroadcastEvents => BroadcastEvents;

    /// <summary>Gets an observable sequence that emits when the system is about to suspend.</summary>
    public static IObservable<PowerBroadcastEvent> SystemSuspendingEvents => BroadcastEvents.Where(static (powerEvent) => powerEvent == PowerBroadcastEvent.PBT_APMSUSPEND);

    /// <summary>
    /// Gets an observable sequence that emits when the system has resumed from suspension.
    /// This fires when user activity or an application is detected on the resumed system.
    /// </summary>
    public static IObservable<PowerBroadcastEvent> SystemResumedFromSuspendEvents => BroadcastEvents.Where(static (powerEvent) => powerEvent == PowerBroadcastEvent.PBT_APMRESUMESUSPEND);

    /// <summary>
    /// Gets an observable sequence that emits when the system has resumed automatically to handle an event.
    /// Applications that receive this event are not allowed to interact with the user.
    /// If the user is present, <see cref="P:CP.ReactiveUI.Primitives.Windows.Desktop.Power.PowerBroadcastListener.SystemResumedFromSuspendEvents" /> will follow.
    /// </summary>
    public static IObservable<PowerBroadcastEvent> SystemAutomaticResumeEvents => BroadcastEvents.Where(static (powerEvent) => powerEvent == PowerBroadcastEvent.PBT_APMRESUMEAUTOMATIC);

    /// <summary>
    /// Gets an observable sequence that emits when the system power status has changed
    /// (e.g., switch from battery to AC power, or battery level changed significantly).
    /// </summary>
    public static IObservable<PowerBroadcastEvent> PowerStatusChanges => BroadcastEvents.Where(static (powerEvent) => powerEvent == PowerBroadcastEvent.PBT_APMPOWERSTATUSCHANGE);

    /// <summary>Creates a power-broadcast event stream from a supplied message stream for deterministic tests.</summary>
    /// <param name="windowMessageEvents">The source Windows message stream.</param>
    /// <returns>A shared stream of power-broadcast events.</returns>
    internal static IObservable<PowerBroadcastEvent> CreatePowerBroadcastEventsForTesting(IObservable<WindowMessage> windowMessageEvents) =>
        CreatePowerBroadcastEvents(windowMessageEvents);

    /// <summary>Creates a shared power-broadcast event stream from Windows messages.</summary>
    /// <param name="windowMessageEvents">The source Windows message stream.</param>
    /// <returns>A shared stream of power-broadcast events.</returns>
    private static IObservable<PowerBroadcastEvent> CreatePowerBroadcastEvents(IObservable<WindowMessage> windowMessageEvents) =>
        (from message in windowMessageEvents
         where message.Msg == WindowsMessages.WM_POWERBROADCAST
         select (PowerBroadcastEvent)checked((uint)message.WParam)).ShareLatest();
}
