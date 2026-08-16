// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;

/// <summary>Provides a safe handle for a GDI region.</summary>
public class SafeRegionHandle : SafeObjectHandle
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeRegionHandle" /> class.</summary>
    public SafeRegionHandle()
        : base(ownsHandle: true) { }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeRegionHandle" /> class from an existing handle.</summary>
    /// <param name="preexistingHandle">The existing region handle.</param>
    public SafeRegionHandle(IntPtr preexistingHandle)
        : base(ownsHandle: true)
    {
        SetHandle(preexistingHandle);
    }

    /// <summary>Creates a rectangular region.</summary>
    /// <param name="left">The x-coordinate of the left edge.</param>
    /// <param name="top">The y-coordinate of the top edge.</param>
    /// <param name="right">The x-coordinate of the right edge.</param>
    /// <param name="bottom">The y-coordinate of the bottom edge.</param>
    /// <returns>A safe handle for the created region.</returns>
    public static SafeRegionHandle CreateRectRgn(int left, int top, int right, int bottom) =>
        Gdi32Api.CreateRectRgn(left, top, right, bottom);
}
