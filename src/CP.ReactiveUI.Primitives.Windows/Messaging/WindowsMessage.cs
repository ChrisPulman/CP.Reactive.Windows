// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>Provides helper methods for working with Windows messages.</summary>
public static class WindowsMessage
{
    /// <summary>Contains the message-name imports.</summary>
    private static class NativeMethods
    {
        /// <summary>Registers a unique Windows message.</summary>
        /// <param name="message">The message text.</param>
        /// <returns>The unique registered message identifier.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegisterWindowMessageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern uint RegisterWindowMessage(string message);
    }

    /// <summary>Gets the name of a windows message that was registered with RegisterWindowMessage.</summary>
    /// <param name="messageId">The message ID returned by RegisterWindowMessage.</param>
    /// <returns>The message name, or <c>null</c> when the message cannot be resolved.</returns>
    public static unsafe string GetWindowsMessage(uint messageId)
    {
        if (messageId < 49_152)
        {
            WindowsMessages windowsMessages = (WindowsMessages)messageId;
            return windowsMessages.ToString();
        }

        char* clipboardFormatName = stackalloc char[256];
        int numberOfChars = ClipboardNativeMethods.GetClipboardFormatName(messageId, clipboardFormatName, 256);
        if (numberOfChars > 0)
        {
            return new(clipboardFormatName, 0, numberOfChars);
        }

        return null;
    }

    /// <summary>Registers a Windows message.</summary>
    /// <param name="message">The Windows message.</param>
    /// <returns>The message ID.</returns>
    public static uint RegisterWindowsMessage(string message) => NativeMethods.RegisterWindowMessage(message);
}
