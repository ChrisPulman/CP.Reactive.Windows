// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Enums;

/// <summary>
/// XenApp Server creates ICA connections dynamically as needed. The following table lists and describes
/// the events (possible values for EventMask), and indicates the flags triggered by the event.
/// </summary>
[Flags]
public enum EventMask : ulong
{
    /// <summary>WF_EVENT_NONE: No event (this event is used only as a return value in pEventFlags).</summary>
    None = 0x00000000,

    /// <summary>WF_EVENT_CREATE: New ICA session created Create, State Change, All.</summary>
    Create = 0x00000001,

    /// <summary>WF_EVENT_DELETE: Existing ICA session deleted Delete, State Change, All.</summary>
    Delete = 0x00000002,

    /// <summary>WF_EVENT_LOGON: User logon to system (from console or WinStation) Logon, State Change, All.</summary>
    Logon = 0x00000020,

    /// <summary>WF_EVENT_LOGOFF: User logoff to system (from console or WinStation) Logoff, State Change, All.</summary>
    Logoff = 0x00000040,

    /// <summary>WF_EVENT_CONNECT: ICA session connect from client Connect, State Change, All.</summary>
    Connect = 0x00000008,

    /// <summary>
    /// WF_EVENT_DISCONNECT: ICA session disconnect from client Disconnect, State Event Description Flags triggered Change,
    /// All.
    /// </summary>
    Disconnect = 0x00000010,

    /// <summary>WF_EVENT_RENAME: Existing ICA session renamed Rename, All.</summary>
    Rename = 0x00000004,

    /// <summary>
    /// WF_EVENT_STATECHANGE: ICA session state change (this event is triggered when WF_CONNECTSTATE_CLASS (defined in
    /// Wfapi.h) changes).
    /// </summary>
    StateChange = 0x00000080,

    /// <summary>
    /// WF_EVENT_LICENSE: License state change (this event is triggered when a license is added or deleted using License
    /// Manager) License, All.
    /// </summary>
    License = 0x00000100,

    /// <summary>Reserved event mask bit.</summary>
    Reserved00000200 = 0x00000200,

    /// <summary>Reserved event mask bit.</summary>
    Reserved00000400 = 0x00000400,

    /// <summary>Reserved event mask bit.</summary>
    Reserved00000800 = 0x00000800,

    /// <summary>Reserved event mask bit.</summary>
    Reserved00001000 = 0x00001000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved00002000 = 0x00002000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved00004000 = 0x00004000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved00008000 = 0x00008000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved00010000 = 0x00010000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved00020000 = 0x00020000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved00040000 = 0x00040000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved00080000 = 0x00080000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved00100000 = 0x00100000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved00200000 = 0x00200000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved00400000 = 0x00400000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved00800000 = 0x00800000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved01000000 = 0x01000000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved02000000 = 0x02000000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved04000000 = 0x04000000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved08000000 = 0x08000000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved10000000 = 0x10000000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved20000000 = 0x20000000,

    /// <summary>Reserved event mask bit.</summary>
    Reserved40000000 = 0x40000000,

    /// <summary>
    /// WF_EVENT_ALL: Wait for any event type WF_EVENT_FLUSH Unblock all waiting events(this event is used only as an
    /// EventMask).
    /// </summary>
    All = Create | Delete | Rename | Connect | Disconnect | Logon | Logoff | StateChange | License
        | Reserved00000200 | Reserved00000400 | Reserved00000800 | Reserved00001000 | Reserved00002000
        | Reserved00004000 | Reserved00008000 | Reserved00010000 | Reserved00020000 | Reserved00040000
        | Reserved00080000 | Reserved00100000 | Reserved00200000 | Reserved00400000 | Reserved00800000
        | Reserved01000000 | Reserved02000000 | Reserved04000000 | Reserved08000000 | Reserved10000000
        | Reserved20000000 | Reserved40000000,
}
