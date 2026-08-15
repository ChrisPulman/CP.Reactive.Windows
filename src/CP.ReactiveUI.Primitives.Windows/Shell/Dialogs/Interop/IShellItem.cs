// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs.Interop;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop;
#endif
/// <summary>Wrapper for the native IShellItem interface.</summary>
internal class IShellItem : ComObject
{
    /// <summary>The GetDisplayName vtable slot.</summary>
    private const int GetDisplayNameSlot = 5;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop.IShellItem" /> class.</summary>
    /// <param name="handle">The owned COM interface pointer.</param>
    internal IShellItem(IntPtr handle)
        : base(handle)
    {
    }

    /// <summary>Gets the requested display name.</summary>
    /// <param name="displayName">The display name shape.</param>
    /// <returns>The display name.</returns>
    internal virtual unsafe string GetDisplayName(ShellItemDisplayName displayName)
    {
        IntPtr name = default;
        ComObject.ThrowIfFailed(((delegate* unmanaged[Stdcall]<IntPtr, ShellItemDisplayName, out IntPtr, int>)(void*)GetMethod(GetDisplayNameSlot))(Handle, displayName, out name));
        try
        {
            return Marshal.PtrToStringUni(name);
        }
        finally
        {
            Marshal.FreeCoTaskMem(name);
        }
    }
}
