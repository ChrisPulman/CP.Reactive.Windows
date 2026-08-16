// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Windows;
using System.Windows.Interop;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using ReactiveUI.Primitives.Disposables;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>A monitor for window messages.</summary>
public static class WinProcWindowsExtensions
{
    /// <summary>Provides extension members for <see cref="HwndSource" />.</summary>
    /// <param name="hwndSource">The HWND source to observe.</param>
    extension(HwndSource hwndSource)
    {
        /// <summary>Create an observable for the specified HwndSource.</summary>
        /// <returns>An observable sequence of window messages.</returns>
        public IObservable<WindowMessageInfo> ObserveWindowMessages() => ObserveWindowMessages<object>(null, hwndSource, null, null);
    }

    /// <summary>Provides extension members for <see cref="Window" />.</summary>
    /// <param name="window">The WPF window to observe.</param>
    extension(Window window)
    {
        /// <summary>Create an observable for the specified window.</summary>
        /// <returns>An observable sequence of window messages.</returns>
        public IObservable<WindowMessageInfo> ObserveWindowMessages() => ObserveWindowMessages<object>(window, null, null, null);
    }

    /// <summary>Create an observable for the specified window or HwndSource.</summary>
    /// <typeparam name="TState">The type of the state returned by the setup callback.</typeparam>
    /// <param name="window">Window.</param>
    /// <param name="hwndSource">HwndSource.</param>
    /// <param name="before">The callback that runs as soon as the observable is created.</param>
    /// <param name="disposeAction">The callback that disposes the setup state.</param>
    /// <returns>IObservable.</returns>
    internal static IObservable<WindowMessageInfo> ObserveWindowMessages<TState>(
        Window window,
        HwndSource hwndSource,
        Func<long, TState> before,
        Action<TState> disposeAction)
    {
        if (window is null && hwndSource is null)
        {
            throw new NotSupportedException("One of Window or HwndSource must be supplied");
        }

        return ReactiveSignal.Create<WindowMessageInfo>(observer => new WindowMessageSubscriptionState<TState>(
            window,
            hwndSource,
            hwndSource?.Handle.ToInt64() ?? 0,
            before,
            disposeAction,
            observer)).ShareLatest();
    }

    /// <summary>Create an observable for the specified hook source.</summary>
    /// <typeparam name="TState">The type of the state returned by the setup callback.</typeparam>
    /// <param name="hookSource">The hook source to observe.</param>
    /// <param name="before">The callback that runs as soon as the observable is created.</param>
    /// <param name="disposeAction">The callback that disposes the setup state.</param>
    /// <returns>An observable sequence of window messages.</returns>
    internal static IObservable<WindowMessageInfo> ObserveWindowMessages<TState>(
        IWindowMessageHookSource hookSource,
        Func<long, TState> before,
        Action<TState> disposeAction)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(hookSource);
        return CreateHookSourceMessages(hookSource, before, disposeAction).ShareLatest();
    }

    /// <summary>Selects the setup handle for an HWND source.</summary>
    /// <param name="initialHandle">The handle captured when the subscription was created.</param>
    /// <param name="activeHandle">The current HWND source handle.</param>
    /// <returns>The initial non-zero handle, or the active handle when initialization was deferred.</returns>
    internal static long SelectSourceHandle(long initialHandle, long activeHandle) =>
        initialHandle != 0L ? initialHandle : activeHandle;

    /// <summary>Create an observable for an abstract hook source.</summary>
    /// <typeparam name="TState">The type of the state returned by the setup callback.</typeparam>
    /// <param name="hookSource">The hook source to observe.</param>
    /// <param name="before">The setup callback.</param>
    /// <param name="disposeAction">The disposal callback.</param>
    /// <returns>An observable sequence of window messages.</returns>
    private static IObservable<WindowMessageInfo> CreateHookSourceMessages<TState>(
        IWindowMessageHookSource hookSource,
        Func<long, TState> before,
        Action<TState> disposeAction) =>
        ReactiveSignal.Create<WindowMessageInfo>(observer =>
        {
            EventHandler eventHandler = (_, _) => observer.OnCompleted();
            hookSource.Disposed += eventHandler;
            hookSource.AddHook(WindowMessageHandler);
            TState state = before is null ? default : before(hookSource.Handle);
            return Scope.Create(
                new HookSourceDisposalState<TState>(disposeAction, state, hookSource, eventHandler, WindowMessageHandler),
                static stateToDispose =>
                {
                    stateToDispose.Dispose();
                });

            IntPtr WindowMessageHandler(IntPtr windowHandle, int msg, IntPtr wordParam, IntPtr longParam, ref bool handled)
            {
                observer.OnNext(WindowMessageInfo.Create(
                    windowHandle.ToInt64(),
                    msg,
                    wordParam.ToInt64(),
                    longParam.ToInt64()));
                if (hookSource.IsDisposed)
                {
                    observer.OnCompleted();
                }

                return IntPtr.Zero;
            }
        });

    /// <summary>Stores WPF window-message subscription state without closure captures.</summary>
    /// <typeparam name="TState">The setup state type.</typeparam>
    private sealed class WindowMessageSubscriptionState<TState> : IDisposable
    {
        /// <summary>The WM_NCCREATE message emitted when a source is initialized.</summary>
        private const int SourceInitializedMessage = 129;

        /// <summary>The WPF window to observe.</summary>
        private readonly Window _window;

        /// <summary>The initial HWND source handle.</summary>
        private readonly long _initialHwndSourceHandle;

        /// <summary>The optional setup callback.</summary>
        private readonly Func<long, TState> _before;

        /// <summary>The optional disposal callback.</summary>
        private readonly Action<TState> _disposeAction;

        /// <summary>The observer receiving window messages.</summary>
        private readonly IObserver<WindowMessageInfo> _observer;

        /// <summary>The source-disposed handler.</summary>
        private readonly EventHandler _hwndSourceDisposedHandler;

        /// <summary>The window-message hook.</summary>
        private readonly HwndSourceHook _windowMessageHandler;

        /// <summary>Tracks whether the source-initialized handler is registered.</summary>
        private bool _sourceInitializedRegistered;

        /// <summary>The setup state.</summary>
        private TState _state;

        /// <summary>Initializes a new instance of the <see cref="WindowMessageSubscriptionState{TState}" /> class.</summary>
        /// <param name="window">The WPF window.</param>
        /// <param name="hwndSource">The HWND source.</param>
        /// <param name="initialHwndSourceHandle">The initial HWND source handle.</param>
        /// <param name="before">The optional setup callback.</param>
        /// <param name="disposeAction">The optional disposal callback.</param>
        /// <param name="observer">The observer receiving window messages.</param>
        internal WindowMessageSubscriptionState(
            Window window,
            HwndSource hwndSource,
            long initialHwndSourceHandle,
            Func<long, TState> before,
            Action<TState> disposeAction,
            IObserver<WindowMessageInfo> observer)
        {
            _window = window;
            ActiveHwndSource = hwndSource;
            _initialHwndSourceHandle = initialHwndSourceHandle;
            _before = before;
            _disposeAction = disposeAction;
            _observer = observer;
            _hwndSourceDisposedHandler = OnHwndSourceDisposed;
            _windowMessageHandler = OnWindowMessage;

            if (_window is not null)
            {
                ActiveHwndSource = ToHwndSource(_window);
            }

            if (ActiveHwndSource is not null)
            {
                RegisterHwndSource();
            }
            else if (_window is not null)
            {
                _window.SourceInitialized += OnSourceInitialized;
                _sourceInitializedRegistered = true;
            }
        }

        /// <summary>Gets or sets the active HWND source.</summary>
        private HwndSource ActiveHwndSource { get; set; }

        /// <summary>Create a HwndSource for the specified Window.</summary>
        /// <param name="window">Window.</param>
        /// <returns>HwndSource.</returns>
        private static HwndSource ToHwndSource(Window window)
        {
            IntPtr windowHandle = new WindowInteropHelper(window).Handle;
            return windowHandle != IntPtr.Zero ? HwndSource.FromHwnd(windowHandle) : null;
        }

        /// <summary>Disposes the WPF message subscription state.</summary>
        void IDisposable.Dispose()
        {
            _disposeAction?.Invoke(_state);
            if (_sourceInitializedRegistered)
            {
                _window.SourceInitialized -= OnSourceInitialized;
                _sourceInitializedRegistered = false;
            }

            if (ActiveHwndSource is not null)
            {
                ActiveHwndSource.Disposed -= _hwndSourceDisposedHandler;
                ActiveHwndSource.RemoveHook(_windowMessageHandler);
            }
        }

        /// <summary>Handles deferred source initialization.</summary>
        /// <param name="sender">The event source.</param>
        /// <param name="args">The event arguments.</param>
        private void OnSourceInitialized(object sender, EventArgs args)
        {
            _ = sender;
            _ = args;
            _window.SourceInitialized -= OnSourceInitialized;
            _sourceInitializedRegistered = false;
            ActiveHwndSource = ToHwndSource(_window);
            RegisterHwndSource();
            _observer.OnNext(WindowMessageInfo.Create(
                ActiveHwndSource.Handle.ToInt64(),
                SourceInitializedMessage,
                0L,
                0L));
        }

        /// <summary>Handles source disposal.</summary>
        /// <param name="sender">The event source.</param>
        /// <param name="args">The event arguments.</param>
        private void OnHwndSourceDisposed(object sender, EventArgs args)
        {
            _ = sender;
            _ = args;
            _observer.OnCompleted();
        }

        /// <summary>Registers hooks and invokes the setup callback.</summary>
        private void RegisterHwndSource()
        {
            ActiveHwndSource.Disposed += _hwndSourceDisposedHandler;
            ActiveHwndSource.AddHook(_windowMessageHandler);
            if (_before is not null)
            {
                long sourceHandle = SelectSourceHandle(_initialHwndSourceHandle, ActiveHwndSource.Handle.ToInt64());
                _state = _before(sourceHandle);
            }
        }

        /// <summary>Handles a native window message.</summary>
        /// <param name="windowHandle">The native window handle.</param>
        /// <param name="msg">The window message.</param>
        /// <param name="wordParam">The word parameter.</param>
        /// <param name="longParam">The long parameter.</param>
        /// <param name="handled">The handled flag.</param>
        /// <returns>The message result.</returns>
        private nint OnWindowMessage(nint windowHandle, int msg, nint wordParam, nint longParam, ref bool handled)
        {
            _ = handled;
            _observer.OnNext(WindowMessageInfo.Create(
                (long)windowHandle,
                msg,
                (long)wordParam,
                (long)longParam));
            return 0;
        }
    }

    /// <summary>Stores abstract hook-source disposal state without closure captures.</summary>
    /// <typeparam name="TState">The setup state type.</typeparam>
    /// <param name="disposeAction">The optional disposal callback.</param>
    /// <param name="state">The setup state value.</param>
    /// <param name="hookSource">The hook source.</param>
    /// <param name="hookSourceDisposedHandler">The hook source disposed handler.</param>
    /// <param name="windowMessageHandler">The window-message hook.</param>
    private sealed class HookSourceDisposalState<TState>(
        Action<TState> disposeAction,
        TState state,
        IWindowMessageHookSource hookSource,
        EventHandler hookSourceDisposedHandler,
        HwndSourceHook windowMessageHandler) : IDisposable
    {
        /// <summary>Disposes the hook source state.</summary>
        public void Dispose()
        {
            disposeAction?.Invoke(state);
            hookSource.Disposed -= hookSourceDisposedHandler;
            hookSource.RemoveHook(windowMessageHandler);
        }
    }
}
