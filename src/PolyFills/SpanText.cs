// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.PolyFills;

/// <summary>Creates strings from character spans without intermediate allocations.</summary>
internal static class SpanText
{
    /// <summary>Determines whether text contains a character.</summary>
    /// <param name="value">The text to search.</param>
    /// <param name="character">The character to find.</param>
    /// <returns><see langword="true"/> when the character is present.</returns>
    internal static bool Contains(string value, char character)
    {
#if NETFRAMEWORK
        return value.IndexOf(character) >= 0;
#else
        return value.Contains(character);
#endif
    }

    /// <summary>Copies the supplied characters into a new string.</summary>
    /// <param name="value">The character span.</param>
    /// <returns>The created string.</returns>
    internal static unsafe string Create(ReadOnlySpan<char> value)
    {
        fixed (char* valuePointer = value)
        {
            return new(valuePointer, 0, value.Length);
        }
    }

    /// <summary>Returns the text beginning at a zero-based index.</summary>
    /// <param name="value">The source text.</param>
    /// <param name="startIndex">The zero-based start index.</param>
    /// <returns>The requested trailing text.</returns>
    internal static string Slice(string value, int startIndex)
    {
#if NETFRAMEWORK
        return value.Remove(0, startIndex);
#else
        return Create(value.AsSpan(startIndex));
#endif
    }
}
