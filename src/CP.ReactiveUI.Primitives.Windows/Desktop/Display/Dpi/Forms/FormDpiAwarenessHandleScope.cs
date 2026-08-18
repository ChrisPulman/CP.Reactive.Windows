// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi.Forms;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Forms;
#endif
/// <summary>Creates a form handle inside a scoped thread DPI awareness context.</summary>
internal sealed class FormDpiAwarenessHandleScope : IDisposable
{
    /// <summary>The form whose handle should be created.</summary>
    private readonly Form _form;

    /// <summary>The primary DPI awareness context.</summary>
    private readonly DpiAwarenessContext _dpiAwarenessContext;

    /// <summary>The fallback DPI awareness context.</summary>
    private readonly DpiAwarenessContext? _alternativeAwarenessContext;

    /// <summary>The active DPI awareness scope.</summary>
    private IDisposable _dpiAwarenessContextScope;

    /// <summary>Tracks whether this scope has been disposed.</summary>
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Forms.FormDpiAwarenessHandleScope" /> class.</summary>
    /// <param name="form">The form whose handle should be created.</param>
    /// <param name="dpiAwarenessContext">The primary DPI awareness context.</param>
    /// <param name="alternativeAwarenessContext">The fallback DPI awareness context.</param>
    public FormDpiAwarenessHandleScope(Form form, DpiAwarenessContext dpiAwarenessContext, DpiAwarenessContext? alternativeAwarenessContext = null)
    {
        Throw.IfNull(form);
        _form = form;
        _dpiAwarenessContext = dpiAwarenessContext;
        _alternativeAwarenessContext = alternativeAwarenessContext;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            ClearScope();
        }
    }

    /// <summary>Creates the form handle inside the configured DPI awareness scope when needed.</summary>
    internal void EnsureHandleCreated()
    {
        ThrowIfDisposed();
        if (_form.IsHandleCreated)
        {
            return;
        }

        _dpiAwarenessContextScope = NativeDpiMethods.ScopedThreadDpiAwarenessContext(_dpiAwarenessContext, _alternativeAwarenessContext);
        try
        {
            _ = _form.Handle;
        }
        finally
        {
            ClearScope();
        }
    }

    /// <summary>Clears any active DPI awareness scope.</summary>
    private void ClearScope()
    {
        _dpiAwarenessContextScope?.Dispose();
        _dpiAwarenessContextScope = null;
    }

    /// <summary>Throws when this scope has been disposed.</summary>
    private void ThrowIfDisposed() => Throw.IfDisposed(_disposed, this);
}
