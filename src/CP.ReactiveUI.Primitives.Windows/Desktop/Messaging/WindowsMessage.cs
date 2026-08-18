// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>Provides helper methods for working with Windows messages.</summary>
#if NETFRAMEWORK
public static class WindowsMessage
#else
public static partial class WindowsMessage
#endif
{
    /// <summary>The first registered message identifier.</summary>
    private const uint FirstRegisteredMessage = 49_152U;

    /// <summary>The maximum clipboard format name length.</summary>
    private const int ClipboardFormatNameLength = 256;

    /// <summary>The optional registration operation override used by deterministic tests.</summary>
    private static Func<string, uint> _registerWindowsMessageOverride;

    /// <summary>Gets the name of a windows message that was registered with RegisterWindowMessage.</summary>
    /// <param name="messageId">The message ID returned by RegisterWindowMessage.</param>
    /// <returns>The message name, or <c>null</c> when the message cannot be resolved.</returns>
    public static string GetWindowsMessage(uint messageId)
    {
        if (messageId < FirstRegisteredMessage)
        {
            WindowsMessages windowsMessages = (WindowsMessages)messageId;
            return windowsMessages.ToString();
        }

        unsafe
        {
            var clipboardFormatName = stackalloc char[ClipboardFormatNameLength];
            var numberOfChars = ClipboardNativeMethods.GetClipboardFormatName(
                messageId,
                clipboardFormatName,
                ClipboardFormatNameLength);
            return numberOfChars > 0 ? new(clipboardFormatName, 0, numberOfChars) : null;
        }
    }

    /// <summary>Registers a Windows message.</summary>
    /// <param name="message">The Windows message.</param>
    /// <returns>The message ID.</returns>
    public static uint RegisterWindowsMessage(string message) => _registerWindowsMessageOverride is null
        ? NativeMethods.RegisterWindowMessage(message)
        : _registerWindowsMessageOverride(message);

    /// <summary>Overrides message registration for deterministic tests.</summary>
    /// <param name="registerWindowsMessage">The replacement registration operation.</param>
    /// <returns>A scope that restores the production registration operation.</returns>
    internal static IDisposable OverrideRegistrationForTesting(Func<string, uint> registerWindowsMessage)
    {
        Throw.IfNull(registerWindowsMessage);
        var previous = _registerWindowsMessageOverride;
        _registerWindowsMessageOverride = registerWindowsMessage;
        return Scope.Create(previous, static previousOperation =>
        {
            _registerWindowsMessageOverride = previousOperation;
        });
    }

    /// <summary>Contains the message-name imports.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>The User32 library name.</summary>
        private const string User32Dll = "user32.dll";

        /// <summary>Registers a unique Windows message.</summary>
        /// <param name="message">The message text.</param>
        /// <returns>The unique registered message identifier.</returns>
#if NETFRAMEWORK
        [DllImport(User32Dll, CharSet = CharSet.Unicode, EntryPoint = "RegisterWindowMessageW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern uint RegisterWindowMessage(string message);
#else
        [LibraryImport(User32Dll, EntryPoint = "RegisterWindowMessageW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial uint RegisterWindowMessage(string message);
#endif
    }
}
