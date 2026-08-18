// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>Provides a simple message loop for console applications.</summary>
public static class MessageLoop
{
    /// <summary>Message-loop operations used by this process.</summary>
    private static MessageLoopOperations _operations = new(GetMessageNative, DispatchNative);

    /// <summary>Defines a delegate for handling Windows messages.</summary>
    /// <param name="message">The message.</param>
    /// <returns><c>true</c> to continue processing; otherwise <c>false</c>.</returns>
    public delegate bool MessageProc(ref Msg message);

    /// <summary>Retrieves a message through a composable operation.</summary>
    /// <param name="message">The retrieved message.</param>
    /// <param name="windowHandle">The target window handle.</param>
    /// <param name="minimumMessage">The minimum message value.</param>
    /// <param name="maximumMessage">The maximum message value.</param>
    /// <returns>A positive value for a message, zero for quit, or a negative value for failure.</returns>
    internal delegate sbyte GetMessageOperation(out Msg message, nint windowHandle, uint minimumMessage, uint maximumMessage);

    /// <summary>Dispatches a message through a composable operation.</summary>
    /// <param name="message">The message to dispatch.</param>
    internal delegate void DispatchOperation(ref Msg message);

    /// <summary>Processes every message for the calling thread.</summary>
    public static void ProcessMessages() => ProcessMessagesCore(null, 0, 0U, 0U);

    /// <summary>Processes every message for the calling thread.</summary>
    /// <param name="handler">The function that processes each message. Return <c>false</c> to stop handling.</param>
    public static void ProcessMessages(MessageProc handler) => ProcessMessagesCore(handler, 0, 0U, 0U);

    /// <summary>Processes messages for a window handle value.</summary>
    /// <param name="handler">The function that processes each message. Return <c>false</c> to stop handling.</param>
    /// <param name="windowHandle">The window handle value whose messages are retrieved.</param>
    public static void ProcessMessages(MessageProc handler, long windowHandle) => ProcessMessagesCore(handler, checked((nint)windowHandle), 0U, 0U);

    /// <summary>Processes messages for a window handle value and message range.</summary>
    /// <param name="handler">The function that processes each message. Return <c>false</c> to stop handling.</param>
    /// <param name="windowHandle">The window handle value whose messages are retrieved.</param>
    /// <param name="minimumMessage">The lowest message value to retrieve.</param>
    /// <param name="maximumMessage">The highest message value to retrieve.</param>
    public static void ProcessMessages(
        MessageProc handler,
        long windowHandle,
        uint minimumMessage,
        uint maximumMessage) =>
        ProcessMessagesCore(handler, checked((nint)windowHandle), minimumMessage, maximumMessage);

    /// <summary>Retrieves a queued message for the current thread.</summary>
    /// <param name="message">The retrieved message.</param>
    /// <returns><c>true</c> when a message was retrieved; otherwise <c>false</c>.</returns>
    internal static bool TryGetMessage(out Msg message) => _operations.GetMessage(out message, 0, 0U, 0U) > 0;

    /// <summary>Translates and dispatches a native message.</summary>
    /// <param name="message">The message to dispatch.</param>
    internal static void Dispatch(ref Msg message) => _operations.Dispatch(ref message);

    /// <summary>Overrides message retrieval and dispatch for deterministic tests.</summary>
    /// <param name="getMessage">The replacement message retrieval operation.</param>
    /// <param name="dispatch">The replacement dispatch operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(GetMessageOperation getMessage, DispatchOperation dispatch)
    {
        Throw.IfNull(getMessage);
        Throw.IfNull(dispatch);
        var operations = _operations;
        _operations = new(getMessage, dispatch);
        return Scope.Create(operations, static previous =>
        {
            _operations = previous;
        });
    }

    /// <summary>Processes messages using native handle values kept internal to the implementation.</summary>
    /// <param name="handler">The optional message handler.</param>
    /// <param name="windowHandle">The native window handle.</param>
    /// <param name="minimumMessage">The lowest message value to retrieve.</param>
    /// <param name="maximumMessage">The highest message value to retrieve.</param>
    private static void ProcessMessagesCore(MessageProc handler, nint windowHandle, uint minimumMessage, uint maximumMessage)
    {
        while (_operations.GetMessage(out var msg, windowHandle, minimumMessage, maximumMessage) > 0)
        {
            _operations.Dispatch(ref msg);
            if (handler is not null && !handler(ref msg))
            {
                break;
            }
        }
    }

    /// <summary>Dispatches a message through the Windows message APIs.</summary>
    /// <param name="message">The message to dispatch.</param>
    private static unsafe void DispatchNative(ref Msg message)
    {
        fixed (Msg* message2 = &message)
        {
            _ = NativeMethods.TranslateMessage(message2);
            NativeMethods.DispatchMessage(message2);
        }
    }

    /// <summary>Retrieves a message through the Windows message APIs.</summary>
    /// <param name="message">The retrieved message.</param>
    /// <param name="windowHandle">The native window handle.</param>
    /// <param name="minimumMessage">The minimum message value.</param>
    /// <param name="maximumMessage">The maximum message value.</param>
    /// <returns>A positive value for a message, zero for quit, or a negative value for failure.</returns>
    private static unsafe sbyte GetMessageNative(out Msg message, nint windowHandle, uint minimumMessage, uint maximumMessage)
    {
        fixed (Msg* message2 = &message)
        {
            return NativeMethods.GetMessage(message2, windowHandle, minimumMessage, maximumMessage);
        }
    }

    /// <summary>Contains the message-loop imports.</summary>
    private static class NativeMethods
    {
        /// <summary>The User32 library name.</summary>
        private const string User32Dll = "user32.dll";

        /// <summary>The User32 module handle.</summary>
        private static readonly nint User32Module =
            NativeLibrary.Load(Path.Combine(Environment.SystemDirectory, User32Dll));

        /// <summary>The GetMessageW export pointer.</summary>
        private static readonly nint GetMessageExport = NativeLibrary.GetExport(User32Module, "GetMessageW");

        /// <summary>The DispatchMessageW export pointer.</summary>
        private static readonly nint DispatchMessageExport =
            NativeLibrary.GetExport(User32Module, "DispatchMessageW");

        /// <summary>The TranslateMessage export pointer.</summary>
        private static readonly nint TranslateMessageExport =
            NativeLibrary.GetExport(User32Module, nameof(TranslateMessage));

        /// <summary>Dispatches a message to its window procedure.</summary>
        /// <param name="message">The message to dispatch.</param>
        internal static unsafe void DispatchMessage(Msg* message) =>
            ((delegate* unmanaged[Stdcall]<Msg*, IntPtr>)checked((nuint)DispatchMessageExport))(message);

        /// <summary>Retrieves a message from the calling thread queue.</summary>
        /// <param name="message">The message buffer.</param>
        /// <param name="windowHandle">The native window handle.</param>
        /// <param name="minimumFilter">The lowest message value to retrieve.</param>
        /// <param name="maximumFilter">The highest message value to retrieve.</param>
        /// <returns>A value greater than zero for a message, zero for WM_QUIT, or -1 for an error.</returns>
        internal static unsafe sbyte GetMessage(
            Msg* message,
            nint windowHandle,
            uint minimumFilter,
            uint maximumFilter) =>
            ((delegate* unmanaged[Stdcall]<Msg*, nint, uint, uint, sbyte>)checked((nuint)GetMessageExport))(
                message,
                windowHandle,
                minimumFilter,
                maximumFilter);

        /// <summary>Translates virtual-key messages.</summary>
        /// <param name="message">The message to translate.</param>
        /// <returns><c>true</c> when the message was translated; otherwise <c>false</c>.</returns>
        internal static unsafe bool TranslateMessage(Msg* message) =>
            ((delegate* unmanaged[Stdcall]<Msg*, int>)checked((nuint)TranslateMessageExport))(message) != 0;
    }

    /// <summary>Composes message retrieval and dispatch operations.</summary>
    /// <param name="getMessage">The retrieval operation.</param>
    /// <param name="dispatch">The dispatch operation.</param>
    private sealed class MessageLoopOperations(GetMessageOperation getMessage, DispatchOperation dispatch)
    {
        /// <summary>Dispatches a message.</summary>
        /// <param name="message">The message to dispatch.</param>
        public void Dispatch(ref Msg message) => dispatch(ref message);

        /// <summary>Retrieves a message.</summary>
        /// <param name="message">The retrieved message.</param>
        /// <param name="windowHandle">The native window handle.</param>
        /// <param name="minimumMessage">The minimum message value.</param>
        /// <param name="maximumMessage">The maximum message value.</param>
        /// <returns>The configured operation result.</returns>
        public sbyte GetMessage(out Msg message, nint windowHandle, uint minimumMessage, uint maximumMessage) =>
            getMessage(out message, windowHandle, minimumMessage, maximumMessage);
    }
}
