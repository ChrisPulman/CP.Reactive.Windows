// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.Shell.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Shell.Structs;

/// <summary>
/// Contains information about a system appbar message.
/// This is used by the <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/bb762108.aspx">SHAppBarMessage function</a>
/// </summary>
public struct AppBarData : IEquatable<AppBarData>
{
    /// <summary>The appbar window handle.</summary>
    private IntPtr _windowHandle;

    /// <summary>The message-specific parameter value.</summary>
    private int _parameter;

    /// <summary>Gets or sets the application-defined message identifier.</summary>
    public uint CallbackMessageIdentifier { get; set; }

    /// <summary>Gets or sets the appbar edge used by appbar positioning messages.</summary>
    public AppBarEdges AppBarEdge { get; set; }

    /// <summary>Gets or sets the bounding rectangle used by appbar messages.</summary>
    public NativeRect Bounds { get; set; }

    /// <summary>Gets or sets a value indicating whether the appbar auto-hide flag is set.</summary>
    public bool AutoHide
    {
        get => _parameter != 0;
        set => _parameter = (value ? 1 : 0);
    }

    /// <summary>Gets or sets the appbar state.</summary>
    public AppBarStates State
    {
        get => (AppBarStates)_parameter;
        set => _parameter = (int)value;
    }

    /// <summary>Gets the default (empty) value.</summary>
    /// <returns>An initialized appbar data value.</returns>
    public static AppBarData Create() => default;

    /// <summary>Compares two values for equality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true" /> when the values are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(AppBarData left, AppBarData right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two values for inequality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true" /> when the values are not equal; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(AppBarData left, AppBarData right)
    {
        return !left.Equals(right);
    }

    /// <summary>Sets the handle to the appbar window.  Not all messages use this member. See the individual message page to see if you need to provide an hWindow value.</summary>
    /// <param name="windowHandle">The appbar window handle.</param>
    public void SetWindowHandle(IntPtr windowHandle) => _windowHandle = windowHandle;

    /// <inheritdoc />
    public override readonly bool Equals(object obj) => obj is AppBarData other && Equals(other);

    /// <inheritdoc />
    public readonly bool Equals(AppBarData other) =>
        _windowHandle == other._windowHandle
        && _parameter == other._parameter
        && CallbackMessageIdentifier == other.CallbackMessageIdentifier
        && AppBarEdge == other.AppBarEdge
        && Bounds.Equals(other.Bounds);

    /// <inheritdoc />
    public override readonly int GetHashCode() => 0;

    /// <summary>Converts the public data to the native layout.</summary>
    /// <returns>The native appbar data.</returns>
    internal readonly NativeAppBarData ToNative() =>
        new(_windowHandle, CallbackMessageIdentifier, AppBarEdge, Bounds, _parameter);

    /// <summary>Copies native results back to the public data.</summary>
    /// <param name="data">The native appbar data.</param>
    internal void Apply(in NativeAppBarData data)
    {
        _windowHandle = data.WindowHandle;
        CallbackMessageIdentifier = data.CallbackMessageIdentifier;
        AppBarEdge = data.Edge;
        Bounds = data.Bounds;
        _parameter = data.Parameter;
    }

    /// <summary>Native appbar data layout.</summary>
    internal readonly struct NativeAppBarData : IEquatable<NativeAppBarData>
    {
        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Shell.Structs.AppBarData.NativeAppBarData" /> struct.</summary>
        /// <param name="windowHandle">The appbar window handle.</param>
        /// <param name="callbackMessageIdentifier">The callback message identifier.</param>
        /// <param name="edge">The appbar edge.</param>
        /// <param name="bounds">The appbar bounds.</param>
        /// <param name="parameter">The message-specific parameter.</param>
        internal NativeAppBarData(
            IntPtr windowHandle,
            uint callbackMessageIdentifier,
            AppBarEdges edge,
            NativeRect bounds,
            int parameter)
        {
            Size = Marshal.SizeOf<NativeAppBarData>();
            WindowHandle = windowHandle;
            CallbackMessageIdentifier = callbackMessageIdentifier;
            Edge = edge;
            Bounds = bounds;
            Parameter = parameter;
        }

        /// <summary>Gets the size of the structure.</summary>
        internal int Size { get; }

        /// <summary>Gets the appbar window handle.</summary>
        internal IntPtr WindowHandle { get; }

        /// <summary>Gets the appbar callback message identifier.</summary>
        internal uint CallbackMessageIdentifier { get; }

        /// <summary>Gets the appbar edge.</summary>
        internal AppBarEdges Edge { get; }

        /// <summary>Gets the appbar bounds.</summary>
        internal NativeRect Bounds { get; }

        /// <summary>Gets the message-specific parameter.</summary>
        internal int Parameter { get; }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is NativeAppBarData other && Equals(other);

        /// <inheritdoc />
        public bool Equals(NativeAppBarData other) =>
            Size == other.Size
            && WindowHandle == other.WindowHandle
            && CallbackMessageIdentifier == other.CallbackMessageIdentifier
            && Edge == other.Edge
            && Bounds.Equals(other.Bounds)
            && Parameter == other.Parameter;

        /// <inheritdoc />
        public override int GetHashCode() =>
            HashCode.Combine(
                Size,
                WindowHandle,
                CallbackMessageIdentifier,
                Edge,
                Bounds,
                Parameter);
    }
}
