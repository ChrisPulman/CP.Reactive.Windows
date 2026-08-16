// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs.Interop;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop;
#endif
/// <summary>Wrapper for the native IShellItemArray interface.</summary>
internal class IShellItemArray : ComObject
{
    /// <summary>The GetCount vtable slot.</summary>
    private const int GetCountSlot = 7;

    /// <summary>The GetItemAt vtable slot.</summary>
    private const int GetItemAtSlot = 8;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop.IShellItemArray" /> class.</summary>
    /// <param name="handle">The owned COM interface pointer.</param>
    internal IShellItemArray(IntPtr handle)
        : base(handle)
    {
    }

    /// <summary>Gets the number of shell items.</summary>
    /// <returns>The number of shell items.</returns>
    internal virtual unsafe uint GetCount()
    {
        uint count = default;
        ComObject.ThrowIfFailed(((delegate* unmanaged[Stdcall]<IntPtr, out uint, int>)(void*)GetMethod(GetCountSlot))(Handle, out count));
        return count;
    }

    /// <summary>Gets an item by index.</summary>
    /// <param name="index">The item index.</param>
    /// <returns>The shell item.</returns>
    internal virtual unsafe IShellItem GetItemAt(uint index)
    {
        IntPtr item = default;
        ComObject.ThrowIfFailed(((delegate* unmanaged[Stdcall]<IntPtr, uint, out IntPtr, int>)(void*)GetMethod(GetItemAtSlot))(Handle, index, out item));
        return new(item);
    }
}
