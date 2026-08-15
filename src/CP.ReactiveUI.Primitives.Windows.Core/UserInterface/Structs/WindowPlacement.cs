// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.TypeConverters;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;

/// <summary>Contains information about the placement of a window on the screen.</summary>
[Serializable]
[TypeConverter(typeof(WindowPlacementTypeConverter))]
public struct WindowPlacement : IEquatable<WindowPlacement>
{
    /// <summary>
    ///     The length of the structure, in bytes. Before calling the GetWindowPlacement or SetWindowPlacement functions, set this member to sizeof(WINDOWPLACEMENT).
    ///     <para>
    ///         GetWindowPlacement and SetWindowPlacement fail if this member is not set correctly.
    ///     </para>
    /// </summary>
    private int _nativeSize;

    /// <summary>Native _nativeFlags field.</summary>
    private WindowPlacementFlags _nativeFlags;

    /// <summary>Native _nativeShowCommand field.</summary>
    private ShowWindowCommands _nativeShowCommand;

    /// <summary>Native _nativeMinimumPosition field.</summary>
    private NativePoint _nativeMinimumPosition;

    /// <summary>Native _nativeMaximumPosition field.</summary>
    private NativePoint _nativeMaximumPosition;

    /// <summary>Native _nativeNormalPosition field.</summary>
    private NativeRect _nativeNormalPosition;

    /// <summary>Gets or sets flags that control the position of the minimized window and the method by which the window is restored.</summary>
    public WindowPlacementFlags Flags
    {
        get => _nativeFlags;
        set => _nativeFlags = value;
    }

    /// <summary>Gets or sets the current show state of the window.</summary>
    public ShowWindowCommands ShowCmd
    {
        get => _nativeShowCommand;
        set => _nativeShowCommand = value;
    }

    /// <summary>Gets or sets the coordinates of the window's upper-left corner when the window is minimized.</summary>
    public NativePoint MinPosition
    {
        get => _nativeMinimumPosition;
        set => _nativeMinimumPosition = value;
    }

    /// <summary>Gets or sets the coordinates of the window's upper-left corner when the window is maximized.</summary>
    public NativePoint MaxPosition
    {
        get => _nativeMaximumPosition;
        set => _nativeMaximumPosition = value;
    }

    /// <summary>Gets or sets the window's coordinates when the window is in the restored position.</summary>
    public NativeRect NormalPosition
    {
        get => _nativeNormalPosition;
        set => _nativeNormalPosition = value;
    }

    /// <summary>Gets the default (empty) value.</summary>
    /// <returns>The initialized window placement.</returns>
    public static WindowPlacement Create() => new WindowPlacement { _nativeSize = Marshal.SizeOf<WindowPlacement>() };

    /// <summary>Compares two WindowPlacement values for equality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>True when both values are equal.</returns>
    public static bool operator ==(WindowPlacement left, WindowPlacement right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two WindowPlacement values for inequality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>True when the values are not equal.</returns>
    public static bool operator !=(WindowPlacement left, WindowPlacement right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override readonly string ToString() => $"{{Flags: {_nativeFlags}; ShowCmd: {_nativeShowCommand}; MinPosition: {_nativeMinimumPosition}; MaxPosition: {_nativeMaximumPosition}; NormalPosition: {_nativeNormalPosition}}}";

    /// <inheritdoc />
    public readonly bool Equals(WindowPlacement other) => _nativeSize == other._nativeSize && _nativeFlags == other._nativeFlags && _nativeShowCommand == other._nativeShowCommand && _nativeMinimumPosition.Equals(other._nativeMinimumPosition) && _nativeMaximumPosition.Equals(other._nativeMaximumPosition) && _nativeNormalPosition.Equals(other._nativeNormalPosition);

    /// <inheritdoc />
    public override readonly bool Equals(object obj) => obj is WindowPlacement other && Equals(other);

    /// <inheritdoc />
    public override readonly int GetHashCode() => 0;
}
