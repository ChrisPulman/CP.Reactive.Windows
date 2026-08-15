// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;

/// <summary>Abstracts the native GDI operations used when safe handles are released.</summary>
internal interface IGdiSafeHandleApi
{
    /// <summary>Deletes a GDI object.</summary>
    /// <param name="objectHandle">The GDI object handle.</param>
    /// <returns><see langword="true" /> when the object was deleted.</returns>
    bool DeleteObject(IntPtr objectHandle);

    /// <summary>Selects a GDI object into a device context.</summary>
    /// <param name="deviceContext">The device context.</param>
    /// <param name="objectHandle">The object to select.</param>
    /// <returns>The previously selected object.</returns>
    IntPtr SelectObject(SafeHandle deviceContext, SafeHandle objectHandle);

    /// <summary>Restores a previously selected GDI object.</summary>
    /// <param name="deviceContext">The device context.</param>
    /// <param name="objectHandle">The previously selected object.</param>
    /// <returns>The object that was replaced.</returns>
    IntPtr RestoreObject(SafeHandle deviceContext, IntPtr objectHandle);
}
