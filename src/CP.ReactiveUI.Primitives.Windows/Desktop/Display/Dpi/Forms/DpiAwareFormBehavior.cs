// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Windows.Forms;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi.Forms;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Forms;
#endif
/// <summary>Composes DPI-aware behavior onto an existing form.</summary>
public sealed class DpiAwareFormBehavior : IDisposable
{
    /// <summary>The form receiving DPI-aware behavior.</summary>
    private readonly Form _form;

    /// <summary>Creates the form handle inside the requested DPI awareness context.</summary>
    private readonly FormDpiAwarenessHandleScope _handleScope;

    /// <summary>Tracks whether the behavior has been disposed.</summary>
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Forms.DpiAwareFormBehavior" /> class.</summary>
    /// <param name="form">The form to attach DPI-aware behavior to.</param>
    public DpiAwareFormBehavior(Form form)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(form);
        _form = form;
        _handleScope = new(form, DpiAwarenessContext.PerMonitorAwareV2, DpiAwarenessContext.PerMonitorAware);
        DpiHandler = new(needsListenerWorkaround: true);
        _form.HandleCreated += OnHandleCreated;
        DpiHandler.MessageHandler = form.ObserveWindowMessages().Subscribe(message =>
        {
            _ = DpiHandler.HandleWindowMessages(message);
        });
        EnsureHandleCreated();
    }

    /// <summary>Gets the DpiHandler used for this form.</summary>
    public DpiHandler DpiHandler { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _form.HandleCreated -= OnHandleCreated;
            DpiHandler.Dispose();
            _handleScope.Dispose();
        }
    }

    /// <summary>Creates the form handle inside the configured DPI awareness context when needed.</summary>
    public void EnsureHandleCreated()
    {
        ThrowIfDisposed();
        _handleScope.EnsureHandleCreated();
        InitializeDpiForCurrentHandle();
    }

    /// <summary>Initializes the DPI handler for the current form handle.</summary>
    private void InitializeDpiForCurrentHandle()
    {
        if (_form.IsHandleCreated)
        {
            long windowHandle = _form.Handle.ToInt64();
            _ = DpiHandler.HandleWindowMessages(WindowMessageInfo.Create(windowHandle, (int)WindowsMessages.WM_NCCREATE, 0L, 0L));
            _ = DpiHandler.HandleWindowMessages(WindowMessageInfo.Create(windowHandle, (int)WindowsMessages.WM_CREATE, 0L, 0L));
        }
    }

    /// <summary>Initializes the DPI handler when the form creates a new handle.</summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    private void OnHandleCreated(object sender, EventArgs e) => InitializeDpiForCurrentHandle();

    /// <summary>Throws when this behavior has been disposed.</summary>
    private void ThrowIfDisposed() => CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfDisposed(_disposed, this);
}
