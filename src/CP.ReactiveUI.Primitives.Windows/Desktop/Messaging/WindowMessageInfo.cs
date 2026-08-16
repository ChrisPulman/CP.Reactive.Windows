// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>Container for the windows messages.</summary>
public sealed class WindowMessageInfo
{
    /// <summary>Gets the handle of the window.</summary>
    public long Handle { get; private set; }

    /// <summary>Gets the actual message.</summary>
    public WindowsMessages Message { get; private set; }

    /// <summary>Gets the word parameter.</summary>
    public long WordParam { get; private set; }

    /// <summary>Gets the long parameter.</summary>
    public long LongParam { get; private set; }

    /// <summary>Factory method for the Window Message Info.</summary>
    /// <param name="windowHandle">The window handle value.</param>
    /// <param name="msg">WindowsMessages which is the actual message.</param>
    /// <param name="wordParameter">The word parameter value.</param>
    /// <param name="longParameter">The long parameter value.</param>
    /// <returns>WindowMessageInfo.</returns>
    public static WindowMessageInfo Create(long windowHandle, int msg, long wordParameter, long longParameter) => new WindowMessageInfo
    {
        Handle = windowHandle,
        Message = (WindowsMessages)checked((uint)msg),
        WordParam = wordParameter,
        LongParam = longParameter,
    };
}
