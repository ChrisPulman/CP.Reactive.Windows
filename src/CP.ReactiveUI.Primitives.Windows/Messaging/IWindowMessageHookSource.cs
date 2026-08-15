// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Windows.Interop;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>Abstracts a window-message hook source.</summary>
internal interface IWindowMessageHookSource
{
    /// <summary>Gets the hook source handle.</summary>
    long Handle { get; }

    /// <summary>Gets a value indicating whether the hook source has been disposed.</summary>
    bool IsDisposed { get; }

    /// <summary>Occurs when the hook source is disposed.</summary>
    event EventHandler Disposed;

    /// <summary>Adds a window-message hook.</summary>
    /// <param name="hook">The hook to add.</param>
    void AddHook(HwndSourceHook hook);

    /// <summary>Removes a window-message hook.</summary>
    /// <param name="hook">The hook to remove.</param>
    void RemoveHook(HwndSourceHook hook);
}
