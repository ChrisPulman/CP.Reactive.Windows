// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
#endif
/// <summary>These are extensions to work with the clipboard.</summary>
public static class ClipboardByteExtensions
{
    /// <summary>Provides extension members for the target instance.</summary>
    /// <param name="clipboardAccessToken">The extended instance.</param>
    extension(IClipboardAccessToken clipboardAccessToken)
    {
        /// <summary>
        /// Retrieve the content for the specified format.
        /// You will need to "lock" (OpenClipboard) the clipboard before calling this.
        /// </summary>
        /// <param name="format">StandardClipboardFormats with the format to retrieve the content for.</param>
        /// <returns>byte array.</returns>
        public byte[] GetAsBytes(StandardClipboardFormats format) => clipboardAccessToken.GetAsBytes((uint)format);

        /// <summary>
        /// Retrieve the content for the specified format.
        /// You will need to "lock" (OpenClipboard) the clipboard before calling this.
        /// </summary>
        /// <param name="format">string with the format to retrieve the content for.</param>
        /// <returns>byte array.</returns>
        public byte[] GetAsBytes(string format) => clipboardAccessToken.GetAsBytes(ClipboardFormatExtensions.MapFormatToId(format));

        /// <summary>
        /// Retrieve the content for the specified format.
        /// You will need to "lock" (OpenClipboard) the clipboard before calling this.
        /// </summary>
        /// <param name="formatId">uint with the format to retrieve the content for.</param>
        /// <returns>byte array.</returns>
        public byte[] GetAsBytes(uint formatId)
        {
            using var readInfo = clipboardAccessToken.ReadInfo(formatId);
            var bytes = new byte[readInfo.Size];
            Marshal.Copy(readInfo.MemoryPtr, bytes, 0, readInfo.Size);
            return bytes;
        }

        /// <summary>Place byte[] on the clipboard, this assumes you already locked the clipboard.</summary>
        /// <param name="bytes">bytes to place on the clipboard.</param>
        /// <param name="format">StandardClipboardFormats with format to place the bytes under.</param>
        public void SetAsBytes(byte[] bytes, StandardClipboardFormats format) => clipboardAccessToken.SetAsBytes(bytes, (uint)format);

        /// <summary>Place byte[] on the clipboard, this assumes you already locked the clipboard.</summary>
        /// <param name="bytes">bytes to place on the clipboard.</param>
        /// <param name="format">string with the format to place the bytes under.</param>
        public void SetAsBytes(byte[] bytes, string format) => clipboardAccessToken.SetAsBytes(bytes, ClipboardFormatExtensions.MapFormatToId(format));

        /// <summary>Place byte[] on the clipboard, this assumes you already locked the clipboard.</summary>
        /// <param name="bytes">bytes to place on the clipboard.</param>
        /// <param name="formatId">uint with the format ID to place the bytes under.</param>
        public unsafe void SetAsBytes(byte[] bytes, uint formatId)
        {
            using var writeInfo = clipboardAccessToken.WriteInfo(formatId, bytes.Length);
            using UnmanagedMemoryStream unsafeMemoryStream = new((byte*)(void*)writeInfo.MemoryPtr, bytes.Length, bytes.Length, FileAccess.Write);
            unsafeMemoryStream.Write(bytes, 0, bytes.Length);
        }
    }
}
