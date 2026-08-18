// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs;
#endif
/// <summary>Fluent builder for showing a modern Windows folder-picker dialog.</summary>
/// <example>
/// <code>
/// var result = new FolderPickerBuilder()
/// .WithTitle("Select Output Folder")
/// .WithInitialDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
/// .ShowDialog();
///
/// if (!result.WasCancelled)
/// settings.OutputDirectory = result.SelectedPath;
/// </code>
/// </example>
public sealed class FolderPickerBuilder
{
    /// <summary>The configured dialog title.</summary>
    private string _title;

    /// <summary>The configured initial directory.</summary>
    private string _initialDirectory;

    /// <summary>Sets the text shown in the dialog title bar.</summary>
    /// <param name="title">Title bar text. <see langword="null" /> uses the Windows default.</param>
    /// <returns>The current builder or result value.</returns>
    public FolderPickerBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the folder displayed when the dialog first opens.
    /// Ignored when <paramref name="path" /> is <see langword="null" /> or the directory does not exist;
    /// Windows then reuses the last folder visited by the application.
    /// </summary>
    /// <param name="path">The path value.</param>
    /// <returns>The current builder or result value.</returns>
    public FolderPickerBuilder WithInitialDirectory(string path)
    {
        _initialDirectory = path;
        return this;
    }

    /// <summary>Shows the dialog and returns the result.</summary>
    /// <returns>A <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FileDialogResult" /> describing the outcome.</returns>
    /// <exception cref="T:System.Runtime.InteropServices.COMException">An unexpected COM error occurred.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">Called on a non-Windows platform.</exception>
    public FileDialogResult ShowDialog() => ShowDialog((IntPtr)0);

    /// <summary>Shows the dialog and returns the result.</summary>
    /// <param name="ownerHandle">
    /// Handle of the owner window. Pass <see cref="F:System.IntPtr.Zero" /> for a top-level dialog.
    /// </param>
    /// <returns>
    /// A <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FileDialogResult" /> describing the outcome.
    /// Check <see cref="P:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FileDialogResult.WasCancelled" /> to distinguish a cancellation from a selection.
    /// </returns>
    /// <exception cref="T:System.Runtime.InteropServices.COMException">An unexpected COM error occurred.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">Called on a non-Windows platform.</exception>
    public FileDialogResult ShowDialog(IntPtr ownerHandle) => ShowDialog(ownerHandle, FileDialog.DialogExecutor);

    /// <summary>Shows the dialog through the supplied executor.</summary>
    /// <param name="ownerHandle">The owner window handle.</param>
    /// <param name="executor">The dialog executor.</param>
    /// <returns>A <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FileDialogResult" /> describing the outcome.</returns>
    internal FileDialogResult ShowDialog(IntPtr ownerHandle, IFileDialogExecutor executor) => executor.ShowFolder(new(ownerHandle.ToInt64(), _title, _initialDirectory));
}
