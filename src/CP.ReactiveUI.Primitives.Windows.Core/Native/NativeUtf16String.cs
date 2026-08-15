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
        if (value.IsEmpty)
        {
            return string.Empty;
        }

        checked
        {
            int byteCount = 0;
            while (ShouldContinue(value, byteCount))
            {
                byteCount += Utf16CodeUnitByteWidth;
            }

            fixed (byte* valuePointer = value)
            {
                return Encoding.Unicode.GetString(valuePointer, byteCount);
            }
        }
    }

    /// <summary>Determines whether the UTF-16 buffer scan should continue.</summary>
    /// <param name="value">The byte buffer.</param>
    /// <param name="byteCount">The current byte offset.</param>
    /// <returns>true when another non-null UTF-16 code unit is available.</returns>
    private static bool ShouldContinue(ReadOnlySpan<byte> value, int byteCount) =>
        byteCount + 1 < value.Length && (value[byteCount] != 0 || value[byteCount + 1] != 0);
}
