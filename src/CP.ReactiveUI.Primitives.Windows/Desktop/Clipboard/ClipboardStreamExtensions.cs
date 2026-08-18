// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
#endif
/// <summary>These are extensions to work with the clipboard.</summary>
public static class ClipboardStreamExtensions
{
    /// <summary>Stream operations used by this type.</summary>
    private static ClipboardStreamOperations _operations = new(CopyToClipboardMemory, static readInfo => readInfo.Size);

    /// <summary>Provides extension members for the target instance.</summary>
    /// <param name="clipboardAccessToken">The extended instance.</param>
    extension(IClipboardAccessToken clipboardAccessToken)
    {
        /// <summary>Set the content for the specified format.</summary>
        /// <param name="format">StandardClipboardFormats with the format to set the content for.</param>
        /// <param name="stream">MemoryStream with the content.</param>
        public void SetAsStream(StandardClipboardFormats format, Stream stream) => clipboardAccessToken.SetAsStream((uint)format, stream);

        /// <summary>Set the content for the specified format.</summary>
        /// <param name="format">StandardClipboardFormats with the format to set the content for.</param>
        /// <param name="stream">MemoryStream with the content.</param>
        /// <param name="size">long with the size, if the stream is not seekable.</param>
        public void SetAsStream(StandardClipboardFormats format, Stream stream, long size) => clipboardAccessToken.SetAsStream((uint)format, stream, size);

        /// <summary>Set the content for the specified format.</summary>
        /// <param name="format">string with the format to set the content for.</param>
        /// <param name="stream">MemoryStream with the content.</param>
        public void SetAsStream(string format, Stream stream) => clipboardAccessToken.SetAsStream(ClipboardFormatExtensions.MapFormatToId(format), stream);

        /// <summary>Set the content for the specified format.</summary>
        /// <param name="format">string with the format to set the content for.</param>
        /// <param name="stream">MemoryStream with the content.</param>
        /// <param name="size">long with the size, if the stream is not seekable.</param>
        public void SetAsStream(string format, Stream stream, long size) => clipboardAccessToken.SetAsStream(ClipboardFormatExtensions.MapFormatToId(format), stream, size);

        /// <summary>Set the content for the specified format.</summary>
        /// <param name="formatId">uint with the format to set the content for.</param>
        /// <param name="stream">MemoryStream with the content.</param>
        public void SetAsStream(uint formatId, Stream stream) => SetAsStreamCore(clipboardAccessToken, formatId, stream, null);

        /// <summary>Set the content for the specified format.</summary>
        /// <param name="formatId">uint with the format to set the content for.</param>
        /// <param name="stream">MemoryStream with the content.</param>
        /// <param name="size">long with the size, if the stream is not seekable.</param>
        public void SetAsStream(uint formatId, Stream stream, long size) => SetAsStreamCore(clipboardAccessToken, formatId, stream, size);

        /// <summary>
        /// Try to retrieve the content for the specified format as a stream.
        /// You will need to "lock" (OpenClipboard) the clipboard before calling this.
        /// </summary>
        /// <param name="format">StandardClipboardFormats with the format to retrieve the content for.</param>
        /// <param name="stream">Stream output parameter.</param>
        /// <returns>true if the format can be read as a stream, false otherwise.</returns>
        public bool TryGetAsStream(StandardClipboardFormats format, out Stream stream) => clipboardAccessToken.TryGetAsStream((uint)format, out stream);

        /// <summary>
        /// Try to retrieve the content for the specified format as a stream.
        /// You will need to "lock" (OpenClipboard) the clipboard before calling this.
        /// </summary>
        /// <param name="format">string with the format to retrieve the content for.</param>
        /// <param name="stream">Stream output parameter.</param>
        /// <returns>true if the format can be read as a stream, false otherwise.</returns>
        public bool TryGetAsStream(string format, out Stream stream) => clipboardAccessToken.TryGetAsStream(ClipboardFormatExtensions.MapFormatToId(format), out stream);

        /// <summary>
        /// Try to retrieve the content for the specified format as a stream.
        /// You will need to "lock" (OpenClipboard) the clipboard before calling this.
        /// </summary>
        /// <param name="formatId">uint with the format to retrieve the content for.</param>
        /// <param name="stream">Stream output parameter.</param>
        /// <returns>true if the format can be read as a stream, false otherwise.</returns>
        public bool TryGetAsStream(uint formatId, out Stream stream)
        {
            stream = null;
            if (!clipboardAccessToken.TryReadInfo(formatId, out var readInfo))
            {
                return false;
            }

            using (readInfo)
            {
                stream = CreateManagedReadStream(readInfo);
                return true;
            }
        }

        /// <summary>
        /// Retrieve the content for the specified format.
        /// You will need to "lock" (OpenClipboard) the clipboard before calling this.
        /// </summary>
        /// <param name="format">StandardClipboardFormats with the format to retrieve the content for.</param>
        /// <returns>MemoryStream.</returns>
        public Stream GetAsStream(StandardClipboardFormats format) => clipboardAccessToken.GetAsStream((uint)format);

        /// <summary>
        /// Retrieve the content for the specified format.
        /// You will need to "lock" (OpenClipboard) the clipboard before calling this.
        /// </summary>
        /// <param name="format">string with the format to retrieve the content for.</param>
        /// <returns>MemoryStream.</returns>
        public Stream GetAsStream(string format) => clipboardAccessToken.GetAsStream(ClipboardFormatExtensions.MapFormatToId(format));

        /// <summary>
        /// Retrieve the content for the specified format.
        /// You will need to "lock" (OpenClipboard) the clipboard before calling this.
        /// </summary>
        /// <param name="formatId">uint with the format to retrieve the content for.</param>
        /// <returns>MemoryStream.</returns>
        public Stream GetAsStream(uint formatId)
        {
            using var readInfo = clipboardAccessToken.ReadInfo(formatId);
            return CreateManagedReadStream(readInfo);
        }
    }

    /// <summary>Overrides stream operations for deterministic tests.</summary>
    /// <param name="copyToClipboard">The replacement clipboard copy operation.</param>
    /// <param name="getSize">The replacement readable data size operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(Action<IClipboardAccessToken, uint, Stream, long> copyToClipboard, Func<ClipboardNativeInfo, int> getSize)
    {
        Throw.IfNull(copyToClipboard);
        Throw.IfNull(getSize);
        var operations = _operations;
        _operations = new(copyToClipboard, getSize);
        return Scope.Create(operations, static previous => _operations = previous);
    }

    /// <summary>Creates a managed stream copy from the supplied clipboard native memory.</summary>
    /// <param name="readInfo">The clipboard native memory information.</param>
    /// <returns>A readable stream containing the clipboard data.</returns>
    private static MemoryStream CreateManagedReadStream(ClipboardNativeInfo readInfo)
    {
        var bytes = new byte[_operations.GetSize(readInfo)];
        Marshal.Copy(readInfo.MemoryPtr, bytes, 0, bytes.Length);
        return new(bytes, writable: false);
    }

    /// <summary>Sets the stream content for the specified clipboard format.</summary>
    /// <param name="clipboardAccessToken">The clipboard access token.</param>
    /// <param name="formatId">The format identifier.</param>
    /// <param name="stream">The stream containing clipboard data.</param>
    /// <param name="size">The stream size when it cannot be determined from the stream.</param>
    private static void SetAsStreamCore(IClipboardAccessToken clipboardAccessToken, uint formatId, Stream stream, long? size)
    {
        clipboardAccessToken.ThrowWhenNoAccess();
        if (!stream.CanRead)
        {
            throw new NotSupportedException("Can't read stream");
        }

        MemoryStream bufferStream = null;
        long length;
        if (stream.CanSeek)
        {
            length = checked(stream.Length - stream.Position);
            if (length <= 0)
            {
                throw new NotSupportedException($"Cannot write {length} length stream.");
            }
        }
        else if (size.HasValue)
        {
            length = size.Value;
        }
        else
        {
            bufferStream = new();
            stream.CopyTo(bufferStream);
            length = bufferStream.Length;
            bufferStream.Position = 0L;
            stream = bufferStream;
        }

        _operations.CopyToClipboard(clipboardAccessToken, formatId, stream, length);
        bufferStream?.Dispose();
    }

    /// <summary>Copies stream content into native clipboard memory.</summary>
    /// <param name="clipboardAccessToken">The clipboard access token.</param>
    /// <param name="formatId">The clipboard format identifier.</param>
    /// <param name="stream">The source stream.</param>
    /// <param name="length">The number of bytes to copy.</param>
    private static unsafe void CopyToClipboardMemory(IClipboardAccessToken clipboardAccessToken, uint formatId, Stream stream, long length)
    {
        using var writeInfo = clipboardAccessToken.WriteInfo(formatId, length);
        using UnmanagedMemoryStream unsafeMemoryStream = new((byte*)(void*)writeInfo.MemoryPtr, length, length, FileAccess.Write);
        stream.CopyTo(unsafeMemoryStream);
    }

    /// <summary>Composes clipboard stream operations without invoking them during construction.</summary>
    /// <param name="copyToClipboard">The clipboard copy operation.</param>
    /// <param name="getSize">The readable data size operation.</param>
    private sealed class ClipboardStreamOperations(
        Action<IClipboardAccessToken, uint, Stream, long> copyToClipboard,
        Func<ClipboardNativeInfo, int> getSize)
    {
        /// <summary>Copies a stream into clipboard-owned memory.</summary>
        /// <param name="clipboardAccessToken">The clipboard access token.</param>
        /// <param name="formatId">The clipboard format identifier.</param>
        /// <param name="stream">The stream to copy.</param>
        /// <param name="length">The number of bytes to copy.</param>
        public void CopyToClipboard(
            IClipboardAccessToken clipboardAccessToken,
            uint formatId,
            Stream stream,
            long length) =>
            copyToClipboard(clipboardAccessToken, formatId, stream, length);

        /// <summary>Gets the readable clipboard data size.</summary>
        /// <param name="readInfo">The clipboard native memory information.</param>
        /// <returns>The readable byte count.</returns>
        public int GetSize(ClipboardNativeInfo readInfo) => getSize(readInfo);
    }
}
