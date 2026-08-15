// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;

/// <summary>Composes GDI safe-handle operations from supplied functions.</summary>
internal sealed class GdiSafeHandleOperations : IGdiSafeHandleApi
{
    /// <summary>Stores the delete operation.</summary>
    private readonly Func<IntPtr, bool> _deleteObject;

    /// <summary>Stores the restore operation.</summary>
    private readonly Func<SafeHandle, IntPtr, IntPtr> _restoreObject;

    /// <summary>Stores the select operation.</summary>
    private readonly Func<SafeHandle, SafeHandle, IntPtr> _selectObject;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.GdiSafeHandleOperations" /> class.</summary>
    /// <param name="deleteObject">The delete operation.</param>
    /// <param name="selectObject">The select operation.</param>
    /// <param name="restoreObject">The restore operation.</param>
    internal GdiSafeHandleOperations(
        Func<IntPtr, bool> deleteObject,
        Func<SafeHandle, SafeHandle, IntPtr> selectObject,
        Func<SafeHandle, IntPtr, IntPtr> restoreObject)
    {
        Throw.IfNull(deleteObject);
        Throw.IfNull(selectObject);
        Throw.IfNull(restoreObject);
        _deleteObject = deleteObject;
        _selectObject = selectObject;
        _restoreObject = restoreObject;
    }

    /// <inheritdoc />
    public bool DeleteObject(IntPtr objectHandle) => _deleteObject(objectHandle);

    /// <inheritdoc />
    public IntPtr SelectObject(SafeHandle deviceContext, SafeHandle objectHandle) =>
        _selectObject(deviceContext, objectHandle);

    /// <inheritdoc />
    public IntPtr RestoreObject(SafeHandle deviceContext, IntPtr objectHandle) =>
        _restoreObject(deviceContext, objectHandle);
}
