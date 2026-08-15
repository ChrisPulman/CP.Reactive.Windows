// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.Enumerations;

/// <summary>Defines session change events for the <see cref="F:CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.Enumerations.WindowsMessages.WM_WTSSESSION_CHANGE" /> message.</summary>
public enum WtsSessionChangeEvents
{
    /// <summary>No session change event was specified.</summary>
    None,

    /// <summary>A session was connected to the console terminal.</summary>
    WTS_CONSOLE_CONNECT,

    /// <summary>A session was disconnected from the console terminal.</summary>
    WTS_CONSOLE_DISCONNECT,

    /// <summary>A session was connected to the remote terminal.</summary>
    WTS_REMOTE_CONNECT,

    /// <summary>A session was disconnected from the remote terminal.</summary>
    WTS_REMOTE_DISCONNECT,

    /// <summary>A user has logged on to the session.</summary>
    WTS_SESSION_LOGON,

    /// <summary>A user has logged off the session.</summary>
    WTS_SESSION_LOGOFF,

    /// <summary>A session has been locked.</summary>
    WTS_SESSION_LOCK,

    /// <summary>A session has been unlocked.</summary>
    WTS_SESSION_UNLOCK,

    /// <summary>A session has changed its remote controlled status.</summary>
    WTS_SESSION_REMOTE_CONTROL,

    /// <summary>A session was created.</summary>
    WTS_SESSION_CREATE,

    /// <summary>A session was terminated.</summary>
    WTS_SESSION_TERMINATE,
}
