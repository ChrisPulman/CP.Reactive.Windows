// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;

/// <summary>Provides a safe handle for GDI objects released with <c>DeleteObject</c>.</summary>
public class SafeObjectHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeObjectHandle" /> class.</summary>
    /// <param name="ownsHandle">A value indicating whether this instance owns the handle.</param>
    protected SafeObjectHandle(bool ownsHandle)
        : base(ownsHandle) { }

    /// <summary>Releases the handle by calling <c>DeleteObject</c>.</summary>
    /// <returns><see langword="true" /> if the object was deleted; otherwise, <see langword="false" />.</returns>
    protected override bool ReleaseHandle() => GdiSafeHandleApi.Current.DeleteObject(handle);
}
