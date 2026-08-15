// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;

/// <summary>
///     The structure for the WINDOWINFO
///     See <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/ms632610.aspx">WINDOWINFO struct</a>
/// </summary>
[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 60)]
public struct WindowInfo : IEquatable<WindowInfo>
{
    /// <summary>Native _nativeStyle field.</summary>
    [FieldOffset(36)]
    private readonly WindowStyleFlags _nativeStyle;

    /// <summary>Native _nativeExtendedStyle field.</summary>
    [FieldOffset(40)]
    private readonly ExtendedWindowStyleFlags _nativeExtendedStyle;

    /// <summary>The window status. If this member is WS_ACTIVECAPTION (0x0001), the window is active. Otherwise, this member is zero.</summary>
    [FieldOffset(44)]
    private readonly uint _nativeWindowStatus;

    /// <summary>The width of the window border, in pixels.</summary>
    [FieldOffset(48)]
    private readonly uint _nativeWindowBorderWidth;

    /// <summary>The height of the window border, in pixels.</summary>
    [FieldOffset(52)]
    private readonly uint _nativeWindowBorderHeight;

    /// <summary>Native _nativeAtomWindowType field.</summary>
    [FieldOffset(56)]
    private readonly ushort _nativeAtomWindowType;

    /// <summary>Native _nativeCreatorVersion field.</summary>
    [FieldOffset(58)]
    private readonly ushort _nativeCreatorVersion;

    /// <summary>The size of the structure, in bytes. The caller must set this member to sizeof(WINDOWINFO).</summary>
    [FieldOffset(0)]
    private uint _nativeSize;

    /// <summary>Native _nativeWindowBounds field.</summary>
    [FieldOffset(4)]
    private NativeRect _nativeWindowBounds;

    /// <summary>Native _nativeClientBounds field.</summary>
    [FieldOffset(20)]
    private NativeRect _nativeClientBounds;

    /// <summary>Gets a value indicating whether the window is active.</summary>
    public readonly bool IsActive => _nativeWindowStatus == 1;

    /// <summary>Gets or sets the coordinates of the window, or client if the Window is returned as empty.</summary>
    public NativeRect Bounds
    {
        get => _nativeWindowBounds;
        set => _nativeWindowBounds = value;
    }

    /// <summary>Gets or sets the coordinates of the client area.</summary>
    public NativeRect ClientBounds
    {
        get => _nativeClientBounds;
        set => _nativeClientBounds = value;
    }

    /// <summary>Gets the window styles.</summary>
    public readonly WindowStyleFlags Style => _nativeStyle;

    /// <summary>Gets the extended window styles.</summary>
    public readonly ExtendedWindowStyleFlags ExtendedStyle => _nativeExtendedStyle;

    /// <summary>Gets the size of the border.</summary>
    public readonly NativeSize BorderSize => checked(new NativeSize((int)_nativeWindowBorderWidth, (int)_nativeWindowBorderHeight));

    /// <summary>Gets the Windows version of the application that created the window.</summary>
    public readonly ushort CreatorVersion => _nativeCreatorVersion;

    /// <summary>Gets the window class atom.</summary>
    public readonly ushort AtomWindowType => _nativeAtomWindowType;

    /// <summary>Factory method for a default WindowInfo.</summary>
    /// <returns>The initialized window information.</returns>
    public static WindowInfo Create() => new WindowInfo { _nativeSize = checked((uint)Marshal.SizeOf<WindowInfo>()) };

    /// <summary>Compares two WindowInfo values for equality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>True when both values are equal.</returns>
    public static bool operator ==(WindowInfo left, WindowInfo right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two WindowInfo values for inequality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>True when the values are not equal.</returns>
    public static bool operator !=(WindowInfo left, WindowInfo right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override readonly string ToString() => $"{{IsActive: {IsActive}; Bounds: {_nativeWindowBounds}; ClientBounds: {_nativeClientBounds}; Style: {_nativeStyle}; ExtendedStyle: {_nativeExtendedStyle}; BorderSize: {BorderSize};}}";

    /// <inheritdoc />
    public readonly bool Equals(WindowInfo other) => _nativeSize == other._nativeSize && _nativeWindowBounds.Equals(other._nativeWindowBounds) && _nativeClientBounds.Equals(other._nativeClientBounds) && _nativeStyle == other._nativeStyle && _nativeExtendedStyle == other._nativeExtendedStyle && _nativeWindowStatus == other._nativeWindowStatus && _nativeWindowBorderWidth == other._nativeWindowBorderWidth && _nativeWindowBorderHeight == other._nativeWindowBorderHeight && _nativeAtomWindowType == other._nativeAtomWindowType && _nativeCreatorVersion == other._nativeCreatorVersion;

    /// <inheritdoc />
    public override readonly bool Equals(object obj) => obj is WindowInfo other && Equals(other);

    /// <inheritdoc />
    public override readonly int GetHashCode() => 0;
}
