// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;

/// <summary>See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms648381(v=vs.85).aspx"></a>.</summary>
public struct CursorInfo : IEquatable<CursorInfo>
{
    /// <summary>Native _nativeFlags field.</summary>
    private readonly CursorInfoFlags _nativeFlags;

    /// <summary>Native _nativeCursorHandle field.</summary>
    private readonly IntPtr _nativeCursorHandle;

    /// <summary>Native _nativeScreenPosition field.</summary>
    private readonly NativePoint _nativeScreenPosition;

    /// <summary>Size of the struct.</summary>
    private int _nativeSize;

    /// <summary>Gets the cursor state, as CursorInfoFlags.</summary>
    public readonly CursorInfoFlags Flags => _nativeFlags;

    /// <summary>Gets a non-owning handle to the cursor.</summary>
    public readonly SafeCursorReferenceHandle CursorHandle => new(_nativeCursorHandle);

    /// <summary>Gets the screen coordinates of the cursor.</summary>
    public readonly NativePoint Location => _nativeScreenPosition;

    /// <summary>Gets a value indicating whether the cursor is currently visible.</summary>
    public readonly bool IsShowing => _nativeCursorHandle != IntPtr.Zero && _nativeFlags == CursorInfoFlags.Showing;

    /// <summary>Factory for the structure.</summary>
    /// <returns>The initialized cursor information.</returns>
    public static CursorInfo Create() => new CursorInfo { _nativeSize = Marshal.SizeOf<CursorInfo>() };

    /// <summary>Compares two CursorInfo values for equality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>True when both values are equal.</returns>
    public static bool operator ==(CursorInfo left, CursorInfo right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two CursorInfo values for inequality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>True when the values are not equal.</returns>
    public static bool operator !=(CursorInfo left, CursorInfo right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public readonly bool Equals(CursorInfo other) => _nativeSize == other._nativeSize && _nativeFlags == other._nativeFlags && _nativeCursorHandle == other._nativeCursorHandle && _nativeScreenPosition.Equals(other._nativeScreenPosition);

    /// <inheritdoc />
    public override readonly bool Equals(object obj) => obj is CursorInfo other && Equals(other);

    /// <inheritdoc />
    public override readonly int GetHashCode() => 0;
}
