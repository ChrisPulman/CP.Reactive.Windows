// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs;
#endif
/// <summary>Composes file-dialog execution so builder behavior can be tested without showing native UI.</summary>
internal interface IFileDialogExecutor
{
    /// <summary>Shows an open-file dialog.</summary>
    /// <param name="request">The open-file request.</param>
    /// <returns>The dialog result.</returns>
    FileDialogResult ShowOpen(FileOpenDialogRequest request);

    /// <summary>Shows a save-file dialog.</summary>
    /// <param name="request">The save-file request.</param>
    /// <returns>The dialog result.</returns>
    FileDialogResult ShowSave(FileSaveDialogRequest request);

    /// <summary>Shows a folder-picker dialog.</summary>
    /// <param name="request">The folder-picker request.</param>
    /// <returns>The dialog result.</returns>
    FileDialogResult ShowFolder(FolderPickerDialogRequest request);
}
