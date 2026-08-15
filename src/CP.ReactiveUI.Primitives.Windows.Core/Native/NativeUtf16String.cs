// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Text;

namespace CP.ReactiveUI.Primitives.Windows.Native;

/// <summary>Helpers for fixed native UTF-16 buffers.</summary>
internal static class NativeUtf16String
{
    /// <summary>Stores the byte width of one UTF-16 code unit.</summary>
    private const int Utf16CodeUnitByteWidth = 2;

    /// <summary>Reads a null-terminated UTF-16 string from a native fixed buffer.</summary>
    /// <param name="value">The UTF-16 bytes.</param>
    /// <returns>The decoded string.</returns>
    internal static unsafe string ReadNullTerminated(ReadOnlySpan<byte> value)
    {
        checked
        {
            int byteCount;
            for (byteCount = 0; byteCount + 1 < value.Length && (value[byteCount] != 0 || value[byteCount + 1] != 0); byteCount += 2)
            {
            }

            fixed (byte* valuePointer = value)
            {
                return Encoding.Unicode.GetString(valuePointer, byteCount);
            }
        }
    }
}
