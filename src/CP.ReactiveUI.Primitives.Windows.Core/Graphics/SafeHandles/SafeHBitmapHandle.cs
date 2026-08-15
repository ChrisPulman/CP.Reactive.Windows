// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Drawing;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;

/// <summary>Provides a safe handle for a disposable HBITMAP.</summary>
public class SafeHBitmapHandle : SafeObjectHandle
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeHBitmapHandle" /> class.</summary>
    public SafeHBitmapHandle()
        : base(ownsHandle: true)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeHBitmapHandle" /> class from an existing handle.</summary>
    /// <param name="preexistingHandle">The existing HBITMAP handle.</param>
    public SafeHBitmapHandle(IntPtr preexistingHandle)
        : base(ownsHandle: true)
    {
        SetHandle(preexistingHandle);
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeHBitmapHandle" /> class from a bitmap.</summary>
    /// <param name="bitmap">The bitmap from which to obtain an HBITMAP.</param>
    public SafeHBitmapHandle(Bitmap bitmap)
        : base(ownsHandle: true)
    {
        SetHandle(bitmap.GetHbitmap());
    }
}
