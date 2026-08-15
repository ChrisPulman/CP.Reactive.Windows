// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;

/// <summary>Provides a non-owning safe handle for an object selected by GDI.</summary>
public class SafeNonDisposableObjectHandle : SafeObjectHandle
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeNonDisposableObjectHandle" /> class.</summary>
    public SafeNonDisposableObjectHandle()
        : base(ownsHandle: false) { }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeNonDisposableObjectHandle" /> class from an existing handle.</summary>
    /// <param name="preexistingHandle">The existing GDI object handle.</param>
    public SafeNonDisposableObjectHandle(IntPtr preexistingHandle)
        : base(ownsHandle: false)
    {
        SetHandle(preexistingHandle);
    }
}
