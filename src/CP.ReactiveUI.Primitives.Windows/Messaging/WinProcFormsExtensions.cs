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
    extension(Control control)
    {
        /// <summary>Create an observable for the specified Control (Form).</summary>
        /// <returns>An observable sequence of window messages.</returns>
        public IObservable<WindowMessageInfo> ObserveWindowMessages()
        {
            WinProcListener winProcListener = new(control);
            return ReactiveSignal.Create(delegate(IObserver<WindowMessageInfo> observer)
            {
                winProcListener.AddHook(WindowMessageHandler);
                return Scope.Create(winProcListener, delegate(WinProcListener listener)
                {
                    listener.Dispose();
                });
                IntPtr WindowMessageHandler(IntPtr windowHandle, int msg, IntPtr wordParam, IntPtr longParam, ref bool handled)
                {
                    WindowMessageInfo message = WindowMessageInfo.Create(windowHandle.ToInt64(), msg, wordParam.ToInt64(), longParam.ToInt64());
                    observer.OnNext(message);
                    if (winProcListener.IsDisposed || message.Message == WindowsMessages.WM_DESTROY)
                    {
                        observer.OnCompleted();
                    }

                    return IntPtr.Zero;
                }
            }).ShareLatest();
        }
    }
}
