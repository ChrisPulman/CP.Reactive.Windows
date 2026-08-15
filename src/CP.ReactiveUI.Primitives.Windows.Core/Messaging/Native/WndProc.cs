// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.Enumerations;

namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.Native;

/// <summary>Represents a method that processes messages sent to a window in a Windows application.</summary>
/// <remarks>This delegate is typically used to define a window procedure (WndProc) that handles messages from the
/// operating system or other applications. Implementations should return an appropriate result based on the message
/// processed. For more information about window procedures and message handling, see the Windows API
/// documentation.</remarks>
/// <param name="windowHandle">A handle to the window that is receiving the message.</param>
/// <param name="msg">WindowsMessages that specifies the type of message being sent.</param>
/// <param name="wordParameter">The first message-specific value. The meaning depends on the value of the msg parameter.</param>
/// <param name="longParameter">The second message-specific value. The meaning depends on the value of the msg parameter.</param>
/// <returns>A value that indicates the result of the message processing, as defined by the message being handled.</returns>
public delegate nuint WndProc(nint windowHandle, WindowsMessages msg, nint wordParameter, nint longParameter);
