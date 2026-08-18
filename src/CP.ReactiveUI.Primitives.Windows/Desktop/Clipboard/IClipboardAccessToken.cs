// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
#endif
/// <summary>
/// This interface is returned by the ClipboardNative.Access(), which calls the ClipboardLockProvider.
/// The access token is only valid within the same thread or window.
/// When you got a IClipboardAccessToken, you can access the clipboard, until it's disposed.
/// Don't forget to dispose this!!!
/// </summary>
public interface IClipboardAccessToken : IDisposable
{
    /// <summary>Gets a value indicating whether the clipboard can be accessed.</summary>
    bool CanAccess { get; }

    /// <summary>Gets a value indicating whether clipboard access was denied due to a lock timeout.</summary>
    bool IsLockTimeout { get; }

    /// <summary>Gets a value indicating whether the clipboard could not be opened before the timeout.</summary>
    bool IsOpenTimeout { get; }

    /// <summary>Throws a <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.ClipboardAccessDeniedException" /> when the clipboard cannot be accessed.</summary>
    void ThrowWhenNoAccess();
}
