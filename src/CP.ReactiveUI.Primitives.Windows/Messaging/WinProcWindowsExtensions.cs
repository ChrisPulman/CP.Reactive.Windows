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
    extension(HwndSource hwndSource)
    {
        /// <summary>Create an observable for the specified HwndSource.</summary>
        /// <returns>An observable sequence of window messages.</returns>
        public IObservable<WindowMessageInfo> ObserveWindowMessages() => ObserveWindowMessages<object>(null, hwndSource, null, null);
    }

    extension(Window window)
    {
        /// <summary>Create an observable for the specified window.</summary>
        /// <returns>An observable sequence of window messages.</returns>
        public IObservable<WindowMessageInfo> ObserveWindowMessages() => ObserveWindowMessages<object>(window, null, null, null);
    }

    /// <summary>Stores WPF window-message disposal state without closure captures.</summary>
    /// <typeparam name="TState">The setup state type.</typeparam>
    /// <param name="disposeAction">The optional disposal callback.</param>
    /// <param name="state">The setup state value.</param>
    /// <param name="hwndSource">The HWND source.</param>
    /// <param name="hwndSourceDisposedHandler">The HWND source disposed handler.</param>
    /// <param name="windowMessageHandler">The window-message hook.</param>
    private sealed class WindowMessageDisposalState<TState>(Action<TState> disposeAction, TState state, HwndSource hwndSource, EventHandler hwndSourceDisposedHandler, HwndSourceHook windowMessageHandler) : IDisposable
    {
        /// <summary>Disposes the WPF message hook state.</summary>
        public void Dispose()
        {
            disposeAction?.Invoke(state);
            if (hwndSource is not null)
            {
                hwndSource.Disposed -= hwndSourceDisposedHandler;
                hwndSource.RemoveHook(windowMessageHandler);
            }
        }
    }

    /// <summary>Stores abstract hook-source disposal state without closure captures.</summary>
    /// <typeparam name="TState">The setup state type.</typeparam>
    /// <param name="disposeAction">The optional disposal callback.</param>
    /// <param name="state">The setup state value.</param>
    /// <param name="hookSource">The hook source.</param>
    /// <param name="hookSourceDisposedHandler">The hook source disposed handler.</param>
    /// <param name="windowMessageHandler">The window-message hook.</param>
    private sealed class HookSourceDisposalState<TState>(Action<TState> disposeAction, TState state, IWindowMessageHookSource hookSource, EventHandler hookSourceDisposedHandler, HwndSourceHook windowMessageHandler) : IDisposable
    {
        /// <summary>Disposes the hook source state.</summary>
        public void Dispose()
        {
            disposeAction?.Invoke(state);
            hookSource.Disposed -= hookSourceDisposedHandler;
            hookSource.RemoveHook(windowMessageHandler);
        }
    }

    /// <summary>Create an observable for the specified window or HwndSource.</summary>
    /// <typeparam name="TState">The type of the state returned by the setup callback.</typeparam>
    /// <param name="window">Window.</param>
    /// <param name="hwndSource">HwndSource.</param>
    /// <param name="before">The callback that runs as soon as the observable is created.</param>
    /// <param name="disposeAction">The callback that disposes the setup state.</param>
    /// <returns>IObservable.</returns>
    internal static IObservable<WindowMessageInfo> ObserveWindowMessages<TState>(Window window, HwndSource hwndSource, Func<long, TState> before, Action<TState> disposeAction)
    {
        if (window is null && hwndSource is null)
        {
            throw new NotSupportedException("One of Window or HwndSource must be supplied");
        }

        long initialHwndSourceHandle = hwndSource?.Handle.ToInt64() ?? 0;
        return ReactiveSignal.Create(delegate(IObserver<WindowMessageInfo> observer)
        {
            TState state = default;
            EventHandler hwndSourceDisposedHandle = delegate
            {
                observer.OnCompleted();
            };
            if (window is not null)
            {
                hwndSource = ToHwndSource(window);
            }

            if (hwndSource is not null)
            {
                RegisterHwndSource();
            }
            else if (window is not null)
            {
                window.SourceInitialized += delegate
                {
                    hwndSource = ToHwndSource(window);
                    RegisterHwndSource();
                    observer.OnNext(WindowMessageInfo.Create(hwndSource.Handle.ToInt64(), 129, 0L, 0L));
                };
            }

            return Scope.Create(new(disposeAction, state, hwndSource, hwndSourceDisposedHandle, WindowMessageHandler), delegate(WindowMessageDisposalState<TState> stateToDispose)
            {
                stateToDispose.Dispose();
            });
            void RegisterHwndSource()
            {
                hwndSource.Disposed += hwndSourceDisposedHandle;
                hwndSource.AddHook(WindowMessageHandler);
                if (before is not null)
                {
                    long sourceHandle = ((initialHwndSourceHandle != 0L) ? initialHwndSourceHandle : hwndSource.Handle.ToInt64());
                    state = before(sourceHandle);
                }
            }
            IntPtr WindowMessageHandler(IntPtr windowHandle, int msg, IntPtr wordParam, IntPtr longParam, ref bool handled)
            {
                observer.OnNext(WindowMessageInfo.Create(windowHandle.ToInt64(), msg, wordParam.ToInt64(), longParam.ToInt64()));
                if (hwndSource.IsDisposed)
                {
                    observer.OnCompleted();
                }

                return IntPtr.Zero;
            }
        }).ShareLatest();
    }

    /// <summary>Create an observable for the specified hook source.</summary>
    /// <typeparam name="TState">The type of the state returned by the setup callback.</typeparam>
    /// <param name="hookSource">The hook source to observe.</param>
    /// <param name="before">The callback that runs as soon as the observable is created.</param>
    /// <param name="disposeAction">The callback that disposes the setup state.</param>
    /// <returns>An observable sequence of window messages.</returns>
    internal static IObservable<WindowMessageInfo> ObserveWindowMessages<TState>(IWindowMessageHookSource hookSource, Func<long, TState> before, Action<TState> disposeAction)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(hookSource);
        return CreateHookSourceMessages(hookSource, before, disposeAction).ShareLatest();
    }

    /// <summary>Create a HwndSource for the specified Window.</summary>
    /// <param name="window">Window.</param>
    /// <returns>HwndSource.</returns>
    private static HwndSource ToHwndSource(Window window)
    {
        IntPtr windowHandle = new WindowInteropHelper(window).Handle;
        if (windowHandle != IntPtr.Zero)
        {
            return HwndSource.FromHwnd(windowHandle);
        }

        return null;
    }

    /// <summary>Create an observable for an abstract hook source.</summary>
    /// <typeparam name="TState">The type of the state returned by the setup callback.</typeparam>
    /// <param name="hookSource">The hook source to observe.</param>
    /// <param name="before">The setup callback.</param>
    /// <param name="disposeAction">The disposal callback.</param>
    /// <returns>An observable sequence of window messages.</returns>
    private static IObservable<WindowMessageInfo> CreateHookSourceMessages<TState>(IWindowMessageHookSource hookSource, Func<long, TState> before, Action<TState> disposeAction) => ReactiveSignal.Create(delegate(IObserver<WindowMessageInfo> observer)
        {
            EventHandler eventHandler = delegate
            {
                observer.OnCompleted();
            };
            hookSource.Disposed += eventHandler;
            hookSource.AddHook(WindowMessageHandler);
            TState state = ((before is null) ? default(TState) : before(hookSource.Handle));
            return Scope.Create(new(disposeAction, state, hookSource, eventHandler, WindowMessageHandler), delegate(HookSourceDisposalState<TState> stateToDispose)
            {
                stateToDispose.Dispose();
            });
            IntPtr WindowMessageHandler(IntPtr windowHandle, int msg, IntPtr wordParam, IntPtr longParam, ref bool handled)
            {
                observer.OnNext(WindowMessageInfo.Create(windowHandle.ToInt64(), msg, wordParam.ToInt64(), longParam.ToInt64()));
                if (hookSource.IsDisposed)
                {
                    observer.OnCompleted();
                }

                return IntPtr.Zero;
            }
        });
}
