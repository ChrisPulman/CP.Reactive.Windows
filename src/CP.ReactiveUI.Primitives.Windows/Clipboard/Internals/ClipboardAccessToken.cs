// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard.Internals;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.Internals;
#endif
/// <summary>This is the clipboard access token.</summary>
internal sealed class ClipboardAccessToken : IClipboardAccessToken
{
    /// <summary>The action that releases the native clipboard lock.</summary>
    private readonly Action _disposeAction;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.Internals.ClipboardAccessToken" /> class.</summary>
    internal ClipboardAccessToken()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.Internals.ClipboardAccessToken" /> class.</summary>
    /// <param name="disposeAction">The action that releases the native clipboard lock.</param>
    internal ClipboardAccessToken(Action disposeAction)
    {
        _disposeAction = disposeAction ?? throw new ArgumentNullException(nameof(disposeAction));
    }

    /// <inheritdoc />
    public bool CanAccess { get; internal set; } = true;

    /// <inheritdoc />
    public bool IsOpenTimeout { get; internal set; }

    /// <inheritdoc />
    public bool IsLockTimeout { get; internal set; }

    /// <inheritdoc />
    public void Dispose()
    {
        CanAccess = false;
        _disposeAction?.Invoke();
    }

    /// <inheritdoc />
    public void ThrowWhenNoAccess()
    {
        if (CanAccess)
        {
            return;
        }

        if (IsLockTimeout)
        {
            throw new ClipboardAccessDeniedException("The clipboard was already locked by another thread or task in your application, a timeout occured.");
        }

        if (IsOpenTimeout)
        {
            throw new ClipboardAccessDeniedException("The clipboard couldn't be opened for usage, it's probably locked by another process");
        }

        throw new ClipboardAccessDeniedException("The clipboard is no longer locked, please check your disposing code.");
    }
}
