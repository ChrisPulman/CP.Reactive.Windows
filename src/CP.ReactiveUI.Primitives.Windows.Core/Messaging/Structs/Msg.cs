// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.Structs;

/// <summary>
/// This structure represents the message information of Windows
/// See <a href="https://docs.microsoft.com/en-us/windows/desktop/api/winuser/ns-winuser-tagmsg">tagMSG structure</a>
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public readonly struct Msg : IEquatable<Msg>
{
    /// <summary>The time source used to translate native message ticks.</summary>
    private static readonly TimeProvider Clock = TimeProvider.System;

    /// <summary>The window handle.</summary>
    private readonly IntPtr _windowHandle;

    /// <summary>The message identifier.</summary>
    private readonly WindowsMessages _message;

    /// <summary>The word parameter.</summary>
    private readonly UIntPtr _wordParam;

    /// <summary>The long parameter.</summary>
    private readonly UIntPtr _longParam;

    /// <summary>The message time.</summary>
    private readonly uint _time;

    /// <summary>The cursor position.</summary>
    private readonly NativePoint _cursorPosition;

    /// <summary>Gets the window handle value whose window procedure receives the message.</summary>
    public long Handle => _windowHandle.ToInt64();

    /// <summary>Gets the message identifier.</summary>
    public WindowsMessages Message => _message;

    /// <summary>Gets additional information about the message. The exact meaning depends on the value of the message member.</summary>
    public ulong WParam => _wordParam.ToUInt64();

    /// <summary>Gets additional information about the message. The exact meaning depends on the value of the message member.</summary>
    public ulong LParam => _longParam.ToUInt64();

    /// <summary>Gets the time of the message.</summary>
    public DateTimeOffset Time =>
        Clock
            .GetLocalNow()
            .Subtract(TimeSpan.FromMilliseconds(checked(Environment.TickCount - _time)));

    /// <summary>Gets the cursor position, in screen coordinates, when the message was posted.</summary>
    public NativePoint CursorPosition => _cursorPosition;

    /// <summary>Gets the window whose window procedure receives the message.</summary>
    internal IntPtr WindowHandle => _windowHandle;

    /// <summary>Compares two message values for equality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><c>true</c> when the values are equal; otherwise <c>false</c>.</returns>
    public static bool operator ==(Msg left, Msg right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two message values for inequality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><c>true</c> when the values are not equal; otherwise <c>false</c>.</returns>
    public static bool operator !=(Msg left, Msg right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public bool Equals(Msg other) =>
        _windowHandle.Equals(other._windowHandle)
        && _message == other._message
        && _wordParam.Equals(other._wordParam)
        && _longParam.Equals(other._longParam)
        && _time == other._time
        && _cursorPosition.Equals(other._cursorPosition);

    /// <inheritdoc />
    public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object obj) =>
        obj is Msg other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(_windowHandle, _message, _wordParam, _longParam, _time, _cursorPosition);
}
