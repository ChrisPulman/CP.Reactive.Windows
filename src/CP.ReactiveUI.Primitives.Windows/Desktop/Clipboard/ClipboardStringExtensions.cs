// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
#endif
/// <summary>These are extensions to work with the clipboard.</summary>
public static class ClipboardStringExtensions
{
    /// <summary>Provides extension members for the target instance.</summary>
    /// <param name="clipboardAccessToken">The extended instance.</param>
    extension(IClipboardAccessToken clipboardAccessToken)
    {
        /// <summary>Place string on the clipboard, this assumes you already locked the clipboard.</summary>
        /// <param name="text">string to place on the clipboard.</param>
        /// <param name="format">StandardClipboardFormats with the clipboard format to use.</param>
        public void SetAsUnicodeString(string text, StandardClipboardFormats format) => clipboardAccessToken.SetAsUnicodeString(text, (uint)format);

        /// <summary>Place string on the clipboard, this assumes you already locked the clipboard.</summary>
        /// <param name="text">string to place on the clipboard.</param>
        /// <param name="format">string with the clipboard format to use.</param>
        public void SetAsUnicodeString(string text, string format) => clipboardAccessToken.SetAsUnicodeString(text, ClipboardFormatExtensions.MapFormatToId(format));

        /// <summary>
        /// Place string on the clipboard, this assumes you already locked the clipboard.
        /// It uses Unicode (CF_UNICODETEXT) by default, as all other formats are automatically generated from this by Windows.
        /// </summary>
        /// <param name="text">string to place on the clipboard.</param>
        public void SetAsUnicodeString(string text) =>
            clipboardAccessToken.SetAsUnicodeString(text, (uint)StandardClipboardFormats.UnicodeText);

        /// <summary>
        /// Place string on the clipboard, this assumes you already locked the clipboard.
        /// It uses Unicode (CF_UNICODETEXT) by default, as all other formats are automatically generated from this by Windows.
        /// </summary>
        /// <param name="text">string to place on the clipboard.</param>
        /// <param name="formatId">uint with the clipboard format id.</param>
        public void SetAsUnicodeString(string text, uint formatId)
        {
            var unicodeBytes = Encoding.Unicode.GetBytes($"{text}\u0000");
            clipboardAccessToken.SetAsBytes(unicodeBytes, formatId);
        }

        /// <summary>Get a string from the clipboard, this assumes you already locked the clipboard.</summary>
        /// <param name="format">StandardClipboardFormats with the clipboard format.</param>
        /// <returns>string.</returns>
        public string GetAsUnicodeString(StandardClipboardFormats format) => clipboardAccessToken.GetAsUnicodeString((uint)format);

        /// <summary>
        /// Get a string from the clipboard, this assumes you already locked the clipboard.
        /// This always takes the CF_UNICODETEXT format, as Windows automatically converts.
        /// </summary>
        /// <param name="format">string with the clipboard format.</param>
        /// <returns>string.</returns>
        public string GetAsUnicodeString(string format) => clipboardAccessToken.GetAsUnicodeString(ClipboardFormatExtensions.MapFormatToId(format));

        /// <summary>
        /// Get a string from the clipboard, this assumes you already locked the clipboard.
        /// This by default takes the CF_UNICODETEXT format, as Windows automatically converts.
        /// </summary>
        /// <returns>string.</returns>
        public string GetAsUnicodeString() =>
            clipboardAccessToken.GetAsUnicodeString((uint)StandardClipboardFormats.UnicodeText);

        /// <summary>
        /// Get a string from the clipboard, this assumes you already locked the clipboard.
        /// This by default takes the CF_UNICODETEXT format, as Windows automatically converts.
        /// </summary>
        /// <param name="formatId">uint with the clipboard format.</param>
        /// <returns>string.</returns>
        public string GetAsUnicodeString(uint formatId)
        {
            var bytes = clipboardAccessToken.GetAsBytes(formatId);
            return Encoding.Unicode.GetString(bytes, 0, bytes.Length).TrimEnd(default(char));
        }
    }
}
