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
/// <summary>Composes DPI-unaware behavior onto an existing form.</summary>
public sealed class DpiUnawareFormBehavior : IDisposable
{
    /// <summary>Creates the form handle inside the requested DPI awareness context.</summary>
    private readonly FormDpiAwarenessHandleScope _handleScope;

    /// <summary>Tracks whether the behavior has been disposed.</summary>
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Forms.DpiUnawareFormBehavior" /> class.</summary>
    /// <param name="form">The form to attach DPI-unaware behavior to.</param>
    public DpiUnawareFormBehavior(Form form)
    {
        _handleScope = new(form, DpiAwarenessContext.Unaware);
        EnsureHandleCreated();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _handleScope.Dispose();
        }
    }

    /// <summary>Creates the form handle inside the configured DPI awareness context when needed.</summary>
    public void EnsureHandleCreated()
    {
        ThrowIfDisposed();
        _handleScope.EnsureHandleCreated();
    }

    /// <summary>Throws when this behavior has been disposed.</summary>
    private void ThrowIfDisposed() => CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfDisposed(_disposed, this);
}
