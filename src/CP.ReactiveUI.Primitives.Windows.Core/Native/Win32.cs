// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native;

/// <summary>Helper class for Win32 errors.</summary>
public static class Win32
{
    /// <summary>The HRESULT facility prefix used for Win32 errors.</summary>
    private const uint HResultFacilityWin32 = 2_147_942_400U;

    /// <summary>The HRESULT severity bit used to identify failures.</summary>
    private const uint HResultSeverityFailure = 2_147_483_648U;

    /// <summary>The mask used to keep the Win32 error code portion.</summary>
    private const uint HResultCodeMask = 65_535U;

    /// <summary>The FormatMessage flags used to retrieve system messages.</summary>
    private const uint FormatMessageFlags = 12_800U;

    /// <summary>The default language identifier passed to FormatMessage.</summary>
    private const uint DefaultLanguageId = 0U;

    /// <summary>The stack buffer length for formatted messages.</summary>
    private const int MessageBufferCapacity = 256;

    /// <summary>The byte offset between UTF-16 characters in the stack buffer.</summary>
    private const nint Utf16CharacterByteOffset = (nint)2;

    /// <summary>Get the error code from the Win32Error.</summary>
    /// <param name="errorCode">The Win32 error code.</param>
    /// <returns>The HRESULT representation of the Win32 error.</returns>
    public static long GetHResult(Win32Error errorCode)
    {
        uint error = (uint)errorCode;
        return (error & HResultSeverityFailure) != HResultSeverityFailure
            ? HResultFacilityWin32 | (error & HResultCodeMask)
            : unchecked((int)error);
    }

    /// <summary>Get the last Win32 error as an exception.</summary>
    /// <returns>The last Win32 error code recorded for the current thread.</returns>
    public static Win32Error GetLastErrorCode() =>
        (Win32Error)checked((uint)Marshal.GetLastWin32Error());

    /// <summary>Get the message for a Win32 error.</summary>
    /// <param name="errorCode">Win32Error</param>
    /// <returns>string with the message.</returns>
    public static string GetMessage(Win32Error errorCode) => GetMessage(errorCode, 0U);

    /// <summary>Get the message for a Win32 error.</summary>
    /// <param name="errorCode">Win32Error</param>
    /// <param name="languageId">
    ///     uint with language ID, see
    ///     <a href="https://msdn.microsoft.com/en-us/library/dd318693.aspx">here</a>
    /// </param>
    /// <returns>string with the message.</returns>
    public static unsafe string GetMessage(Win32Error errorCode, uint languageId)
    {
        char* buffer = stackalloc char[MessageBufferCapacity];
        int characterCount = NativeMethods.FormatMessage(
            FormatMessageFlags,
            IntPtr.Zero,
            (uint)errorCode,
            languageId,
            buffer,
            MessageBufferCapacity,
            IntPtr.Zero);
        if (characterCount == 0)
        {
            return $"Unknown error (0x{checked((int)errorCode):x})";
        }

        StringBuilder result = new();
        for (int i = 0; i < characterCount; i = checked(i + 1))
        {
            char currentCharacter = *(char*)(
                (byte*)buffer + checked(unchecked((nint)i) * Utf16CharacterByteOffset));
            if (!ShouldKeepMessageCharacter(currentCharacter))
            {
                break;
            }

            _ = result.Append(currentCharacter);
        }

        return result.ToString().Replace("\r\n", string.Empty);
    }

    /// <summary>Determines whether a formatted message character should be copied.</summary>
    /// <param name="character">The character to inspect.</param>
    /// <returns>true when the character belongs to the printable message.</returns>
    private static bool ShouldKeepMessageCharacter(char character) =>
        char.IsLetterOrDigit(character)
        || char.IsPunctuation(character)
        || char.IsSymbol(character)
        || char.IsWhiteSpace(character);

    /// <summary>Contains native Windows API bindings.</summary>
    private static class NativeMethods
    {
        /// <summary>The loaded kernel32 module handle.</summary>
        private static readonly IntPtr Kernel32Module = NativeLibrary.Load(
            Path.Combine(Environment.SystemDirectory, "kernel32.dll"));

        /// <summary>The exported FormatMessageW function pointer.</summary>
        private static readonly IntPtr FormatMessagePointer = NativeLibrary.GetExport(
            Kernel32Module,
            "FormatMessageW");

        /// <summary>Formats a system message string for the specified message identifier.</summary>
        /// <param name="flags">The formatting options.</param>
        /// <param name="source">The message source.</param>
        /// <param name="messageId">The requested message identifier.</param>
        /// <param name="languageId">The requested language identifier.</param>
        /// <param name="buffer">The destination buffer.</param>
        /// <param name="size">The destination buffer size.</param>
        /// <param name="arguments">Optional format arguments.</param>
        /// <returns>The number of characters written to the buffer.</returns>
        internal static unsafe int FormatMessage(
            uint flags,
            IntPtr source,
            uint messageId,
            uint languageId,
            char* buffer,
            int size,
            IntPtr arguments) =>
            (
                (delegate* unmanaged[Stdcall]<uint, IntPtr, uint, uint, char*, int, IntPtr, int>)
                    (void*)FormatMessagePointer)(flags, source, messageId, languageId, buffer, size, arguments);
    }
}
