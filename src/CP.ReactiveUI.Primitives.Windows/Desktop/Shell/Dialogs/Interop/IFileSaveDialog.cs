// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs.Interop;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop;
#endif
/// <summary>Wrapper for the native IFileSaveDialog interface.</summary>
internal class IFileSaveDialog : FileDialogComObject
{
    /// <summary>The IFileSaveDialog interface identifier.</summary>
    internal static readonly Guid InterfaceId = new("84BCCD23-5FDE-4CDB-AEA4-AF64B83D78AB");

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop.IFileSaveDialog" /> class.</summary>
    /// <param name="handle">The owned COM interface pointer.</param>
    internal IFileSaveDialog(IntPtr handle)
        : base(handle)
    {
    }
}
