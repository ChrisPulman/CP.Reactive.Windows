// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs.Interop;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop;
#endif
/// <summary>
/// Options for the file open and save dialogs.
/// See <a href="https://docs.microsoft.com/en-us/windows/win32/api/shobjidl_core/ne-shobjidl_core-_fileopendialogoptions">FILEOPENDIALOGOPTIONS enumeration</a>
/// </summary>
[Flags]
internal enum FileOpenOptions : uint
{
    /// <summary>No file-open options are set.</summary>
    None = 0U,
    /// <summary>When saving a file, prompt before overwriting an existing file of the same name.</summary>
    OverwritePrompt = 2U,
    /// <summary>In the Save dialog, only allow the user to choose a file that has one of the file name extensions specified through IFileDialog::SetFileTypes.</summary>
    StrictFileTypes = 4U,
    /// <summary>Don't change the current working directory.</summary>
    NoChangeDir = 8U,
    /// <summary>Present an Open dialog that offers a choice of folders rather than files.</summary>
    PickFolders = 0x20U,
    /// <summary>Ensures that returned items are file system items (SFGAO_FILESYSTEM).</summary>
    ForceFileSystem = 0x40U,
    /// <summary>Enables the user to choose any item in the Shell namespace.</summary>
    AllNonStorageItems = 0x80U,
    /// <summary>Do not check for situations that would prevent an application from opening the selected file.</summary>
    NoValidate = 0x100U,
    /// <summary>Enables the user to select multiple items in the open dialog.</summary>
    AllowMultiSelect = 0x200U,
    /// <summary>The item returned must be in an existing folder.</summary>
    PathMustExist = 0x800U,
    /// <summary>The item returned must exist.</summary>
    FileMustExist = 0x1000U,
    /// <summary>Prompt to create a new file if the user specifies a file that does not exist.</summary>
    CreatePrompt = 0x2000U,
    /// <summary>Always show the readonly checkbox.</summary>
    ShareAware = 0x4000U,
    /// <summary>Do not return read-only items.</summary>
    NoReadOnlyReturn = 0x8000U,
    /// <summary>Do not test creation of the returned item.</summary>
    NoTestFileCreate = 0x10000U,
    /// <summary>Hide the list of places from which the user has recently opened or saved items.</summary>
    HideMruPlaces = 0x20000U,
    /// <summary>Hide items shown by default in the view's navigation pane.</summary>
    HidePinnedPlaces = 0x40000U,
    /// <summary>Shortcuts should not be treated as their target items.</summary>
    NoDereferenceLinks = 0x100000U,
    /// <summary>Do not add the item being opened or saved to the recent documents list.</summary>
    DontAddToRecent = 0x2000000U,
    /// <summary>Include hidden and system items.</summary>
    ForceShowHidden = 0x10000000U,
    /// <summary>Indicates to the Save As dialog box that it should open in expanded mode.</summary>
    DefaultNoMiniMode = 0x20000000U,
    /// <summary>Indicates to the Open dialog box that the preview pane should always be displayed.</summary>
    ForcePreviewPaneOn = 0x40000000U,
}
