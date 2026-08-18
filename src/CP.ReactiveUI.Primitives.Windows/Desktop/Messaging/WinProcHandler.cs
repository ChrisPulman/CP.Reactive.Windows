// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Interop;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>This can be used to handle WinProc messages, for instance when there is no running WinProc.</summary>
public class WinProcHandler
{
    /// <summary>The WM_NCDESTROY message.</summary>
    private const uint NonClientDestroyMessage = 130U;

    /// <summary>Hold the singleton.</summary>
    private static readonly Lazy<WinProcHandler> Singleton = new(static () => new WinProcHandler());

    /// <summary>The shared message source.</summary>
    private static IMessageHandlerWindow _messageSource;

    /// <summary>Creates the shared message source.</summary>
    private static Func<IMessageHandlerWindow> _messageWindowFactory = static () => new HwndSourceMessageHandlerWindow(CreateMessageWindow());

    /// <summary>Store hooks, so they can be removed.</summary>
    private List<WinProcHandlerHook> _hooks = new();

    /// <summary>Gets the singleton instance of the WinProcHandler.</summary>
    public static WinProcHandler Instance => Singleton.Value;

    /// <summary>Gets the special top-level HwndSource that handles all window messages.</summary>
    public HwndSource MessageHandlerWindow => GetMessageHandlerWindow(this).Source;

    /// <summary>Gets the actual handle for the HwndSource.</summary>
    public long Handle => GetMessageHandlerWindow(this).Handle;

    /// <summary>Creates a HwndSource to catch windows message.</summary>
    /// <returns>HwndSource.</returns>
    public static HwndSource CreateMessageWindow() => CreateMessageWindow(0L, "CP.ReactiveUI.Primitives.Windows.MessageHandlerWindow");

    /// <summary>Creates a HwndSource to catch Windows messages.</summary>
    /// <param name="parentWindowHandle">The parent window handle value.</param>
    /// <param name="title">The window title.</param>
    /// <returns>The message window source.</returns>
    public static HwndSource CreateMessageWindow(long parentWindowHandle, string title) => new(new HwndSourceParameters
    {
        ParentWindow = checked((nint)parentWindowHandle),
        Width = 0,
        Height = 0,
        PositionX = 0,
        PositionY = 0,
        AcquireHwndFocusInMenuMode = false,
        ExtendedWindowStyle = 0,
        WindowStyle = 0,
        WindowClassStyle = 0,
        WindowName = title,
    });

    /// <summary>Subscribe a hook to handle messages.</summary>
    /// <param name="winProcHandlerHook">WinProcHandlerHook.</param>
    /// <returns>IDisposable which unsubscribes the hWndSourceHook when Dispose is called.</returns>
    public IDisposable Subscribe(WinProcHandlerHook winProcHandlerHook)
    {
        var hooks = _hooks;
        if (hooks is not null && hooks.Contains(winProcHandlerHook))
        {
            return EmptyDisposable.Instance;
        }

        GetMessageHandlerWindow(this).AddHook(winProcHandlerHook.Hook);
        var newHooks = ((_hooks is null) ? new List<WinProcHandlerHook>() : new List<WinProcHandlerHook>(_hooks));
        newHooks.Add(winProcHandlerHook);
        _hooks = newHooks;
        return Scope.Create(winProcHandlerHook, Unsubscribe);
    }

    /// <summary>Unsubscribe all current hooks.</summary>
    public void UnsubscribeAllHooks()
    {
        foreach (var winProcHandlerHook in _hooks ?? new List<WinProcHandlerHook>())
        {
            GetMessageHandlerWindow(this).RemoveHook(winProcHandlerHook.Hook);
            winProcHandlerHook.Disposable?.Dispose();
        }

        _hooks = null;
    }

    /// <summary>Creates the internal message-window adapter for deterministic contract tests.</summary>
    /// <param name="source">The HWND source to adapt.</param>
    /// <returns>The message-window adapter.</returns>
    internal static IMessageHandlerWindow CreateMessageHandlerWindowForTesting(HwndSource source) => new HwndSourceMessageHandlerWindow(source);

    /// <summary>Overrides the message-window factory for deterministic tests.</summary>
    /// <param name="messageWindowFactory">The replacement factory.</param>
    /// <returns>A scope that restores the previous factory and source.</returns>
    internal static IDisposable OverrideMessageWindowFactoryForTesting(Func<IMessageHandlerWindow> messageWindowFactory)
    {
        Throw.IfNull(messageWindowFactory);
        var messageWindowFactory2 = _messageWindowFactory;
        var previousSource = _messageSource;
        _messageWindowFactory = messageWindowFactory;
        _messageSource = null;
        return Scope.Create((MessageWindowFactory: messageWindowFactory2, MessageSource: previousSource), static previous =>
        {
            (_messageWindowFactory, _messageSource) = (previous.MessageWindowFactory, previous.MessageSource);
        });
    }

    /// <summary>Gets or creates the static message handler window for the supplied handler.</summary>
    /// <param name="handler">The handler that owns hook state.</param>
    /// <returns>The message handler window.</returns>
    private static IMessageHandlerWindow GetMessageHandlerWindow(WinProcHandler handler)
    {
        if (_messageSource is not null && !_messageSource.IsDisposed)
        {
            return _messageSource;
        }

        _messageSource = _messageWindowFactory();
        _messageSource.Disposed += (_, _) =>
        {
            handler.UnsubscribeAllHooks();
        };
        _messageSource.AddHook((IntPtr windowHandle, int msg, IntPtr param, IntPtr longParam, ref bool handled) =>
        {
            if (checked((uint)msg) != NonClientDestroyMessage)
            {
                return IntPtr.Zero;
            }

            handler._hooks = null;
            return IntPtr.Zero;
        });
        return _messageSource;
    }

    /// <summary>Unsubscribe a hook.</summary>
    /// <param name="winProcHandlerHook">WinProcHandlerHook.</param>
    private void Unsubscribe(WinProcHandlerHook winProcHandlerHook)
    {
        GetMessageHandlerWindow(this).RemoveHook(winProcHandlerHook.Hook);
        if (_hooks is not null)
        {
            List<WinProcHandlerHook> newHooks = new(_hooks);
            _ = newHooks.Remove(winProcHandlerHook);
            _hooks = newHooks;
            winProcHandlerHook.Disposable?.Dispose();
        }
    }

    /// <summary>Adapts a WPF source to the internal message-handler window contract.</summary>
    /// <param name="source">The WPF source.</param>
    private sealed class HwndSourceMessageHandlerWindow(HwndSource source) : IMessageHandlerWindow
    {
        /// <inheritdoc />
        public event EventHandler Disposed
        {
            add
            {
                source.Disposed += value;
            }
            remove
            {
                source.Disposed -= value;
            }
        }

        /// <inheritdoc />
        public long Handle => source.Handle.ToInt64();

        /// <inheritdoc />
        public bool IsDisposed => source.IsDisposed;

        /// <inheritdoc />
        public HwndSource Source => source;

        /// <inheritdoc />
        public void AddHook(HwndSourceHook hook) => source.AddHook(hook);

        /// <inheritdoc />
        public void RemoveHook(HwndSourceHook hook) => source.RemoveHook(hook);
    }
}
