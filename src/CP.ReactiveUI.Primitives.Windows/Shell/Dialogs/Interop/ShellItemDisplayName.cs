// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs.Interop;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop;
#endif
/// <summary>
/// Requests the form of an item's display name to retrieve through IShellItem::GetDisplayName.
/// See <a href="https://docs.microsoft.com/en-us/windows/win32/api/shobjidl_core/ne-shobjidl_core-sigdn">SIGDN enumeration</a>
/// </summary>
internal enum ShellItemDisplayName : uint
{
    /// <summary>Returns the display name relative to the parent folder.</summary>
    NormalDisplay = 0U,
    /// <summary>Returns the parsing name relative to the parent folder.</summary>
    ParentRelativeParsing = 2_147_581_953U,
    /// <summary>Returns the parsing name relative to the desktop.</summary>
    DesktopAbsoluteParsing = 2_147_647_488U,
    /// <summary>Returns the editing name relative to the parent folder.</summary>
    ParentRelativeEditing = 2_147_684_353U,
    /// <summary>Returns the editing name relative to the desktop.</summary>
    DesktopAbsoluteEditing = 2_147_794_944U,
    /// <summary>Returns the item's file system path, if it has one.</summary>
    FileSysPath = 2_147_844_096U,
    /// <summary>Returns the item's URL, if it has one.</summary>
    Url = 2_147_909_632U,
    /// <summary>Returns the path relative to the parent folder in a friendly format as displayed in an address bar.</summary>
    ParentRelativeForAddressBar = 2_147_991_553U,
    /// <summary>Returns the path relative to the parent folder.</summary>
    ParentRelative = 2_148_007_937U,
}
