// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.Structs;

/// <summary>Represents a Windows message, including its window handle, message identifier, and associated parameters.</summary>
/// <remarks>This struct is commonly used when processing Windows messages in low-level window procedures or
/// interop scenarios. The meaning of the parameters depends on the specific message identified by Msg.</remarks>
/// <param name="hwnd">The window handle.</param>
/// <param name="msg">The message identifier.</param>
/// <param name="wordParameter">The message word parameter.</param>
/// <param name="longParameter">The message long parameter.</param>
public sealed class WindowMessage(
    nint hwnd,
    WindowsMessages msg,
    nint wordParameter,
    nint longParameter)
{
    /// <summary>Gets the handle value for the window that receives the message.</summary>
    public long Hwnd { get; } = hwnd;

    /// <summary>Gets the Windows message value.</summary>
    public WindowsMessages Msg { get; } = msg;

    /// <summary>Gets the additional message-specific information provided as the first parameter.</summary>
    public long WParam { get; } = wordParameter;

    /// <summary>Gets the additional message-specific information provided as the second parameter.</summary>
    public long LParam { get; } = longParameter;

    /// <summary>Gets or sets a value indicating whether the message has been handled and should not be processed further by the default window procedure.</summary>
    public bool Handled { get; set; }

    /// <summary>Gets or sets the result of processing the message.</summary>
    public ulong Result { get; set; }

    /// <summary>Gets the handle to the window that receives the message.</summary>
    internal nint WindowHandle { get; } = hwnd;

    /// <summary>Gets the native word parameter.</summary>
    internal nint WordParameter { get; } = wordParameter;

    /// <summary>Gets the native long parameter.</summary>
    internal nint LongParameter { get; } = longParameter;
}
