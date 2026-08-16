// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs.Interop;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop;
#endif
/// <summary>Wrapper for the native IFileOpenDialog interface.</summary>
internal class IFileOpenDialog : FileDialogComObject
{
    /// <summary>The IFileOpenDialog interface identifier.</summary>
    internal static readonly Guid InterfaceId = new("D57C7288-D4AD-4768-BE02-9D969532D960");

    /// <summary>The GetResults vtable slot.</summary>
    private const int GetResultsSlot = 27;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop.IFileOpenDialog" /> class.</summary>
    /// <param name="handle">The owned COM interface pointer.</param>
    internal IFileOpenDialog(IntPtr handle)
        : base(handle)
    {
    }

    /// <summary>Gets the selected items.</summary>
    /// <returns>The selected items.</returns>
    internal virtual unsafe IShellItemArray GetResults()
    {
        IntPtr items = default;
        ComObject.ThrowIfFailed(((delegate* unmanaged[Stdcall]<IntPtr, out IntPtr, int>)(void*)GetMethod(GetResultsSlot))(Handle, out items));
        return new(items);
    }
}
