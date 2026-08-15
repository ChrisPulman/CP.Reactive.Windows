// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;

/// <summary>Provides a safe handle that restores a GDI object's previous selection.</summary>
public class SafeSelectObjectHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>Stores the device context whose selected object is restored on release.</summary>
    private readonly SafeHandle _deviceContext;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeSelectObjectHandle" /> class.</summary>
    public SafeSelectObjectHandle()
        : base(ownsHandle: true)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeSelectObjectHandle" /> class and selects an object.</summary>
    /// <param name="deviceContextHandle">The device context in which to select the object.</param>
    /// <param name="newObjectSafeHandle">The GDI object to select.</param>
    public SafeSelectObjectHandle(SafeHandle deviceContextHandle, SafeHandle newObjectSafeHandle)
        : base(ownsHandle: true)
    {
        _deviceContext = deviceContextHandle;
        SetHandle(GdiSafeHandleApi.Current.SelectObject(deviceContextHandle, newObjectSafeHandle));
    }

    /// <inheritdoc />
    protected override bool ReleaseHandle()
    {
        if (_deviceContext is null)
        {
            return false;
        }

        _ = GdiSafeHandleApi.Current.RestoreObject(_deviceContext, handle);
        return true;
    }
}
