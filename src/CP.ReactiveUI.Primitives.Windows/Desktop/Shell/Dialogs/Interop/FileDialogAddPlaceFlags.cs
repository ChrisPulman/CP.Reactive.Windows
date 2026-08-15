// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs.Interop;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop;
#endif
/// <summary>
/// Specifies how the place is to be added to the list of available places.
/// See <a href="https://docs.microsoft.com/en-us/windows/win32/api/shobjidl_core/ne-shobjidl_core-fdap">FDAP enumeration</a>
/// </summary>
internal enum FileDialogAddPlaceFlags : uint
{
    /// <summary>The place is added to the bottom of the default list.</summary>
    Bottom,
    /// <summary>The place is added to the top of the default list.</summary>
    Top,
}
