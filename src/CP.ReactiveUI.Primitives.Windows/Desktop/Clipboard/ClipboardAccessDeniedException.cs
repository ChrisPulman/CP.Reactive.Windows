// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
#endif
/// <inheritdoc />
public class ClipboardAccessDeniedException : Exception
{
    /// <inheritdoc />
    public ClipboardAccessDeniedException()
    {
    }

    /// <inheritdoc />
    public ClipboardAccessDeniedException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public ClipboardAccessDeniedException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
