// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Windows.Forms;
using ReactiveUI.Primitives.Disposables;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>A monitor for window messages.</summary>
public static class WinProcFormsExtensions
{
    /// <summary>Provides extension members for <see cref="Control" />.</summary>
    /// <param name="control">The control to observe.</param>
    extension(Control control)
    {
        /// <summary>Create an observable for the specified Control (Form).</summary>
        /// <returns>An observable sequence of window messages.</returns>
        public IObservable<WindowMessageInfo> ObserveWindowMessages()
        {
            WinProcListener winProcListener = new(control);
            return ReactiveSignal.Create<WindowMessageInfo>(observer =>
            {
                winProcListener.AddHook(WindowMessageHandler);
                return Scope.Create(winProcListener, static listener =>
                {
                    listener.Dispose();
                });
                IntPtr WindowMessageHandler(IntPtr windowHandle, int msg, IntPtr wordParam, IntPtr longParam, ref bool handled)
                {
                    _ = handled;
                    return PublishWindowMessageForTesting(winProcListener, observer, windowHandle, msg, wordParam, longParam);
                }
            }).ShareLatest();
        }
    }

    /// <summary>Publishes a Forms window message through an abstract listener state.</summary>
    /// <param name="listenerState">The listener lifecycle state.</param>
    /// <param name="observer">The observer receiving messages.</param>
    /// <param name="windowHandle">The native window handle.</param>
    /// <param name="messageId">The Windows message identifier.</param>
    /// <param name="wordParam">The word parameter.</param>
    /// <param name="longParam">The long parameter.</param>
    /// <returns>The message result.</returns>
    internal static IntPtr PublishWindowMessageForTesting(
        IWinProcListenerState listenerState,
        IObserver<WindowMessageInfo> observer,
        IntPtr windowHandle,
        int messageId,
        IntPtr wordParam,
        IntPtr longParam)
    {
        WindowMessageInfo message = WindowMessageInfo.Create(
            windowHandle.ToInt64(),
            messageId,
            wordParam.ToInt64(),
            longParam.ToInt64());
        observer.OnNext(message);
        if (listenerState.IsDisposed || message.Message == WindowsMessages.WM_DESTROY)
        {
            observer.OnCompleted();
        }

        return IntPtr.Zero;
    }
}
