// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs;
#endif
/// <summary>Production Windows Common Item Dialog executor.</summary>
internal sealed class NativeFileDialogExecutor : IFileDialogExecutor
{
    /// <summary>Successful HRESULT value.</summary>
    private const int HResultSuccess = 0;

    /// <summary>Creates open-file dialog wrappers.</summary>
    private readonly Func<IFileOpenDialog> _createOpenDialog;

    /// <summary>Creates save-file dialog wrappers.</summary>
    private readonly Func<IFileSaveDialog> _createSaveDialog;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.NativeFileDialogExecutor" /> class.</summary>
    internal NativeFileDialogExecutor()
        : this(
            static () => ComDialogHelper.CreateDialog<IFileOpenDialog>(ComDialogHelper.ClsidFileOpenDialog),
            static () => ComDialogHelper.CreateDialog<IFileSaveDialog>(ComDialogHelper.ClsidFileSaveDialog))
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.NativeFileDialogExecutor" /> class.</summary>
    /// <param name="createOpenDialog">The open-dialog factory.</param>
    /// <param name="createSaveDialog">The save-dialog factory.</param>
    internal NativeFileDialogExecutor(Func<IFileOpenDialog> createOpenDialog, Func<IFileSaveDialog> createSaveDialog)
    {
        _createOpenDialog = createOpenDialog ?? throw new ArgumentNullException(nameof(createOpenDialog));
        _createSaveDialog = createSaveDialog ?? throw new ArgumentNullException(nameof(createSaveDialog));
    }

    /// <summary>Gets the shared production executor.</summary>
    internal static NativeFileDialogExecutor Instance { get; } = new();

    /// <inheritdoc />
    public FileDialogResult ShowOpen(FileOpenDialogRequest request)
    {
        using IFileOpenDialog dialog = _createOpenDialog();
        if (request.Title is not null)
        {
            dialog.SetTitle(request.Title);
        }

        FileOpenOptions options = FileOpenOptions.FileMustExist;
        if (request.AllowMultiSelect)
        {
            options |= FileOpenOptions.AllowMultiSelect;
        }

        dialog.SetOptions(options);
        ComDialogHelper.ApplyFilters(dialog.SetFileTypes, dialog.SetFileTypeIndex, request.Filters);
        if (request.DefaultExtension is not null)
        {
            dialog.SetDefaultExtension(request.DefaultExtension);
        }

        ComDialogHelper.ApplyInitialDirectory(dialog.SetFolder, request.InitialDirectory);
        ComDialogHelper.ApplyPlaces(dialog.AddPlace, request.Places);
        int num = dialog.Show((IntPtr)request.OwnerHandle);
        ThrowIfUnexpectedResult(num);
        if (num == ComDialogHelper.HResultCancelled)
        {
            return FileDialogResult.Cancelled();
        }

        if (request.AllowMultiSelect)
        {
            return FileDialogResult.FromPaths(ComDialogHelper.CollectPaths(dialog.GetResults()));
        }

        using IShellItem item = dialog.GetResult();
        return FileDialogResult.FromPath(ComDialogHelper.GetFileSysPath(item));
    }

    /// <inheritdoc />
    public FileDialogResult ShowSave(FileSaveDialogRequest request)
    {
        using IFileSaveDialog dialog = _createSaveDialog();
        if (request.Title is not null)
        {
            dialog.SetTitle(request.Title);
        }

        dialog.SetOptions(FileOpenOptions.OverwritePrompt);
        ComDialogHelper.ApplyFilters(dialog.SetFileTypes, dialog.SetFileTypeIndex, request.Filters);
        if (request.SuggestedFileName is not null)
        {
            dialog.SetFileName(request.SuggestedFileName);
        }

        if (request.DefaultExtension is not null)
        {
            dialog.SetDefaultExtension(request.DefaultExtension);
        }

        ComDialogHelper.ApplyInitialDirectory(dialog.SetFolder, request.InitialDirectory);
        ComDialogHelper.ApplyPlaces(dialog.AddPlace, request.Places);
        int num = dialog.Show((IntPtr)request.OwnerHandle);
        ThrowIfUnexpectedResult(num);
        if (num == ComDialogHelper.HResultCancelled)
        {
            return FileDialogResult.Cancelled();
        }

        using IShellItem item = dialog.GetResult();
        return FileDialogResult.FromPath(ComDialogHelper.GetFileSysPath(item));
    }

    /// <inheritdoc />
    public FileDialogResult ShowFolder(FolderPickerDialogRequest request)
    {
        using IFileOpenDialog dialog = _createOpenDialog();
        if (request.Title is not null)
        {
            dialog.SetTitle(request.Title);
        }

        dialog.SetOptions(FileOpenOptions.PickFolders);
        ComDialogHelper.ApplyInitialDirectory(dialog.SetFolder, request.InitialDirectory);
        int num = dialog.Show((IntPtr)request.OwnerHandle);
        ThrowIfUnexpectedResult(num);
        if (num == ComDialogHelper.HResultCancelled)
        {
            return FileDialogResult.Cancelled();
        }

        using IShellItem item = dialog.GetResult();
        return FileDialogResult.FromPath(ComDialogHelper.GetFileSysPath(item));
    }

    /// <summary>Throws for non-success dialog HRESULT values other than cancellation.</summary>
    /// <param name="hr">The dialog HRESULT.</param>
    private static void ThrowIfUnexpectedResult(int hr)
    {
        if (hr is not HResultSuccess and not ComDialogHelper.HResultCancelled)
        {
            Marshal.ThrowExceptionForHR(hr);
        }
    }
}
