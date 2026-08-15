// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
#endif
/// <summary>These are extensions to work with the clipboard.</summary>
public static class ClipboardFileExtensions
{
    /// <summary>The maximum path length used by DragQueryFileW.</summary>
    private const int MaximumPathLength = 260;

    /// <summary>The number of trailing null characters in each file name.</summary>
    private const int TerminalCharacterCount = 1;

    /// <summary>The size of a UTF-16 character in bytes.</summary>
    private const int WideCharacterSize = 2;

    /// <summary>The DROPFILES header size in bytes.</summary>
    private const int DropFilesHeaderSize = 20;

    /// <summary>The fWide field offset in bytes.</summary>
    private const int WideFlagOffset = 16;

    /// <summary>Provides extension members for the target instance.</summary>
    /// <param name="clipboardAccessToken">The extended instance.</param>
    extension(IClipboardAccessToken clipboardAccessToken)
    {
        /// <summary>Get a list of file-names on the clipboard.</summary>
        /// <returns>IEnumerable of string.</returns>
        public unsafe IEnumerable<string> GetFileNames()
        {
            clipboardAccessToken.ThrowWhenNoAccess();
            using ClipboardNativeInfo readInfo = clipboardAccessToken.ReadInfo((uint)StandardClipboardFormats.Drop);
            int files = NativeMethods.DragQueryFile(readInfo.GlobalHandle, uint.MaxValue, null, 0);
            if (files <= 0)
            {
                return [];
            }

            List<string> result = new(files);
            char* filename = stackalloc char[MaximumPathLength];
            for (uint i = 0U; i < files; i = checked(i + 1))
            {
                int characterCount = NativeMethods.DragQueryFile(readInfo.GlobalHandle, i, filename, MaximumPathLength);
                if (characterCount != 0)
                {
                    result.Add(new(filename, 0, characterCount));
                }
            }

            return result;
        }

        /// <summary>Set a list of file-names on the clipboard in CF_HDROP (Drop) format.</summary>
        /// <param name="fileNames">IEnumerable of strings with the fully-qualified file names.</param>
        public void SetFileNames(IEnumerable<string> fileNames)
        {
            clipboardAccessToken.ThrowWhenNoAccess();
            List<string> files = new();
            int dataSize = DropFilesHeaderSize + WideCharacterSize;
            checked
            {
                foreach (string fileName in fileNames)
                {
                    files.Add(fileName);
                    dataSize += (fileName.Length + TerminalCharacterCount) * WideCharacterSize;
                }

                using ClipboardNativeInfo writeInfo =
                    clipboardAccessToken.WriteInfo((uint)StandardClipboardFormats.Drop, dataSize);
                Marshal.WriteInt32(writeInfo.MemoryPtr, 0, DropFilesHeaderSize);
                Marshal.WriteInt32(writeInfo.MemoryPtr, WideFlagOffset, 1);
                int offset = DropFilesHeaderSize;
                foreach (string file in files)
                {
                    byte[] fileBytes = Encoding.Unicode.GetBytes($"{file}\u0000");
                    Marshal.Copy(fileBytes, 0, writeInfo.MemoryPtr + offset, fileBytes.Length);
                    offset += fileBytes.Length;
                }
            }
        }
    }
}
