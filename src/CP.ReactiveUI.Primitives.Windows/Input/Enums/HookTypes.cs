// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>
///     The type of hook procedure to be installed via the SetWindowsHookEx function. This parameter can be one of the
///     following values:
/// </summary>
public enum HookTypes
{
    /// <summary>
    ///     Installs a hook procedure that monitors messages generated as a result of an input event in a dialog box, message
    ///     box, menu, or scroll bar. For more information, see the MessageProc hook procedure.
    /// </summary>
    WH_MSGFILTER = -1,
    /// <summary>
    ///     Installs a hook procedure that records input messages posted to the system message queue. This hook is useful for
    ///     recording macros. For more information, see the JournalRecordProc hook procedure.
    /// </summary>
    WH_JOURNALRECORD,
    /// <summary>
    ///     Installs a hook procedure that posts messages previously recorded by a WH_JOURNALRECORD hook procedure. For more
    ///     information, see the JournalPlaybackProc hook procedure.
    /// </summary>
    WH_JOURNALPLAYBACK,
    /// <summary>Installs a hook procedure that monitors keystroke messages. For more information, see the KeyboardProc hook procedure.</summary>
    WH_KEYBOARD,
    /// <summary>
    ///     Installs a hook procedure that monitors messages posted to a message queue. For more information, see the
    ///     GetMsgProc hook procedure.
    /// </summary>
    WH_GETMESSAGE,
    /// <summary>
    ///     Installs a hook procedure that monitors messages before the system sends them to the destination window procedure.
    ///     For more information, see the CallWndProc hook procedure.
    /// </summary>
    WH_CALLWNDPROC,
    /// <summary>
    ///     Installs a hook procedure that receives notifications useful to a CBT application. For more information, see the
    ///     CBTProc hook procedure.
    /// </summary>
    WH_CBT,
    /// <summary>
    ///     Installs a hook procedure that monitors messages generated as a result of an input event in a dialog box, message
    ///     box, menu, or scroll bar. The hook procedure monitors these messages for all applications in the same desktop as
    ///     the calling thread. For more information, see the SysMsgProc hook procedure.
    /// </summary>
    WH_SYSMSGFILTER,
    /// <summary>Installs a hook procedure that monitors mouse messages. For more information, see the MouseProc hook procedure.</summary>
    WH_MOUSE,
    /// <summary>Installs a hook procedure that monitors hardware messages. For more information, see the HardwareProc hook procedure.</summary>
    WH_HARDWARE,
    /// <summary>
    ///     Installs a hook procedure useful for debugging other hook procedures. For more information, see the DebugProc hook
    ///     procedure.
    /// </summary>
    WH_DEBUG,
    /// <summary>
    ///     Installs a hook procedure that receives notifications useful to shell applications. For more information, see the
    ///     ShellProc hook procedure.
    /// </summary>
    WH_SHELL,
    /// <summary>
    ///     Installs a hook procedure that will be called when the application's foreground thread is about to become idle.
    ///     This hook is useful for performing low priority tasks during idle time. For more information, see the
    ///     ForegroundIdleProc hook procedure.
    /// </summary>
    WH_FOREGROUNDIDLE,
    /// <summary>
    ///     Installs a hook procedure that monitors messages after they have been processed by the destination window
    ///     procedure. For more information, see the CallWndRetProc hook procedure.
    /// </summary>
    WH_CALLWNDPROCRET,
    /// <summary>
    ///     Installs a hook procedure that monitors low-level keyboard input events. For more information, see the
    ///     LowLevelKeyboardProc hook procedure.
    /// </summary>
    WH_KEYBOARD_LL,
    /// <summary>
    ///     Installs a hook procedure that monitors low-level mouse input events. For more information, see the
    ///     LowLevelMouseProc hook procedure.
    /// </summary>
    WH_MOUSE_LL
}
