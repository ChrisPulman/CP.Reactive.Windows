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
    extension(IClipboardAccessToken clipboardAccessToken)
    {
        /// <summary>Get a list of file-names on the clipboard.</summary>
        /// <returns>IEnumerable of string.</returns>
        public unsafe IEnumerable<string> GetFileNames()
        {
            clipboardAccessToken.ThrowWhenNoAccess();
            IntPtr dropHandle = NativeMethods.GetClipboardData(15U);
            int files = NativeMethods.DragQueryFile(dropHandle, uint.MaxValue, null, 0);
            if (files == 0)
            {
                return [];
            }

            List<string> result = new(files);
            char* filename = stackalloc char[260];
            for (uint i = 0U; i < files; i = checked(i + 1))
            {
                int characterCount = NativeMethods.DragQueryFile(dropHandle, i, filename, 260);
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
            int dataSize = 22;
            checked
            {
                foreach (string fileName in fileNames)
                {
                    files.Add(fileName);
                    dataSize += (fileName.Length + 1) * 2;
                }

                using ClipboardNativeInfo writeInfo = clipboardAccessToken.WriteInfo(15U, dataSize);
                Marshal.WriteInt32(writeInfo.MemoryPtr, 0, 20);
                Marshal.WriteInt32(writeInfo.MemoryPtr, 16, 1);
                int offset = 20;
                foreach (string file in files)
                {
                    byte[] fileBytes = Encoding.Unicode.GetBytes($"{file}\u0000");
                    Marshal.Copy(fileBytes, 0, writeInfo.MemoryPtr + offset, fileBytes.Length);
                    offset += fileBytes.Length;
                }
            }
        }
    }

    /// <summary>The DROPFILES header size in bytes.</summary>
    private const int DropFilesHeaderSize = 20;

    /// <summary>The fWide field offset in bytes.</summary>
    private const int WideFlagOffset = 16;
}
