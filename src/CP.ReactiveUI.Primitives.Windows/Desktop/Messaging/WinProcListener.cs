// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NETFRAMEWORK
using System.Security.Permissions;
#endif
using System.Windows.Forms;
using System.Windows.Interop;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>This is a Listener for WinProc messages.</summary>
public sealed class WinProcListener : NativeWindow, IDisposable, IWinProcListenerState
{
    /// <summary>The control whose native handle is observed.</summary>
    private readonly Control _control;

    /// <summary>The subscribed window procedure hooks.</summary>
    private List<HwndSourceHook> _hooks = new();

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.WinProcListener" /> class.</summary>
    /// <param name="control">Control to listen to.</param>
    public WinProcListener(Control control)
    {
        Throw.IfNull(control);
        Throw.IfDisposed(control.IsDisposed, control);

        _control = control;
        control.HandleCreated += OnHandleCreated;
        control.HandleDestroyed += OnHandleDestroyed;
        control.Disposed += OnControlDisposed;
        if (control.IsHandleCreated)
        {
            AssignHandle(control.Handle);
        }
    }

    /// <summary>Gets a value indicating whether the WinProcListener is already disposed.</summary>
    public bool IsDisposed { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
        _control.HandleCreated -= OnHandleCreated;
        _control.HandleDestroyed -= OnHandleDestroyed;
        _control.Disposed -= OnControlDisposed;
        _hooks = new();
        if (Handle != IntPtr.Zero)
        {
            ReleaseHandle();
        }
    }

    /// <summary>Adds an event handler.</summary>
    /// <param name="hook">HwndSourceHook.</param>
    public void AddHook(HwndSourceHook hook)
    {
        Throw.IfNull(hook);
        if (IsDisposed)
        {
            return;
        }

        List<HwndSourceHook> newHooks = new(_hooks);
        newHooks.Add(hook);
        _hooks = newHooks;
    }

    /// <summary>Removes the event handlers that were added by AddHook.</summary>
    /// <param name="hook">HwndSourceHook, The event handler to remove.</param>
    public void RemoveHook(HwndSourceHook hook)
    {
        Throw.IfNull(hook);
        if (IsDisposed)
        {
            return;
        }

        List<HwndSourceHook> newHooks = new(_hooks);
        _ = newHooks.Remove(hook);
        _hooks = newHooks;
    }

    /// <summary>Processes hooks through an abstract listener state.</summary>
    /// <param name="listenerState">The listener lifecycle state.</param>
    /// <param name="hooks">The hooks to process.</param>
    /// <param name="message">The message to process.</param>
    /// <returns><c>true</c> when a hook handles the message.</returns>
    internal static bool ProcessHooksForTesting(IWinProcListenerState listenerState, IEnumerable<HwndSourceHook> hooks, ref Message message)
    {
        var handled = false;
        foreach (var sourceHook in hooks ?? new List<HwndSourceHook>())
        {
            if (listenerState.IsDisposed)
            {
                break;
            }

            message.Result = sourceHook(message.HWnd, message.Msg, message.WParam, message.LParam, ref handled);
            if (handled)
            {
                break;
            }
        }

        return handled;
    }

    /// <inheritdoc />
#if NETFRAMEWORK
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
#endif
    protected override void WndProc(ref Message m)
    {
        if (!IsDisposed && !ProcessMessage(ref m))
        {
            base.WndProc(ref m);
        }
    }

    /// <summary>Listen for the control's window creation and then hook into it.</summary>
    /// <param name="sender">object.</param>
    /// <param name="e">EventArgs.</param>
    private void OnHandleCreated(object sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        if (!IsDisposed && Handle == IntPtr.Zero)
        {
            AssignHandle(_control.Handle);
        }
    }

    /// <summary>Remove the handle.</summary>
    /// <param name="sender">object.</param>
    /// <param name="e">EventArgs.</param>
    private void OnHandleDestroyed(object sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        if (Handle != IntPtr.Zero)
        {
            ReleaseHandle();
        }
    }

    /// <summary>Disposes the listener when its control is disposed.</summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event arguments.</param>
    private void OnControlDisposed(object sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        Dispose();
    }

    /// <summary>Helper class to process the message.</summary>
    /// <param name="message">Message.</param>
    /// <returns>bool if the message was handled.</returns>
    private bool ProcessMessage(ref Message message) => ProcessHooksForTesting(this, _hooks, ref message);
}
