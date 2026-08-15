// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
#endif
/// <summary>These are extensions to work with the clipboard.</summary>
public static class ClipboardMiscExtensions
{
    extension(IClipboardAccessToken clipboardAccessToken)
    {
        /// <summary>Empties the clipboard, this assumes that a lock has already been retrieved.</summary>
        public void ClearContents()
        {
            clipboardAccessToken.ThrowWhenNoAccess();
            _ = NativeMethods.EmptyClipboard();
        }

        /// <summary>This places delayed rendered content on the clipboard.</summary>
        /// <param name="format">StandardClipboardFormats with the clipboard format.</param>
        public void SetDelayedRenderedContent(StandardClipboardFormats format) => clipboardAccessToken.SetDelayedRenderedContent((uint)format);

        /// <summary>This places delayed rendered content on the clipboard, don't forget to subscribe to ClipboardNative.ClipboardRenderFormatRequests.</summary>
        /// <param name="format">string with the clipboard format.</param>
        public void SetDelayedRenderedContent(string format) => clipboardAccessToken.SetDelayedRenderedContent(ClipboardFormatExtensions.MapFormatToId(format));

        /// <summary>This places delayed rendered content on the clipboard.</summary>
        /// <param name="formatId">uint with the clipboard format.</param>
        public void SetDelayedRenderedContent(uint formatId)
        {
            clipboardAccessToken.ThrowWhenNoAccess();
            NativeMethods.SetClipboardDataWithErrorHandling(formatId, IntPtr.Zero);
        }
    }
}
