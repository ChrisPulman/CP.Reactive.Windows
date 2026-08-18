// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Interop;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>Wrapper of the HwndSourceHook for the WinProcHandler, to allow to specify a disposable.</summary>
public class WinProcHandlerHook
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.WinProcHandlerHook" /> class.</summary>
    /// <param name="hook">HwndSourceHook.</param>
    public WinProcHandlerHook(HwndSourceHook hook)
    {
        Hook = hook;
    }

    /// <summary>Gets the actual HwndSourceHook.</summary>
    public HwndSourceHook Hook { get; }

    /// <summary>Gets or sets the optional disposable which is called to make a cleanup possible.</summary>
    public IDisposable Disposable { get; set; }
}
