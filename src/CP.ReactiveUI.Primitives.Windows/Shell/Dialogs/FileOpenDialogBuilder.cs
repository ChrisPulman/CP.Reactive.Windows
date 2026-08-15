// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs;
#endif
/// <summary>Fluent builder for showing a modern Windows open-file dialog.</summary>
/// <example>
/// Single file:
/// <code>
/// var result = new FileOpenDialogBuilder()
/// .WithTitle("Open Document")
/// .WithInitialDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
/// .AddFilter("Text files", "*.txt")
/// .AddFilter("All files", "*.*")
/// .ShowDialog();
///
/// if (!result.WasCancelled)
/// ProcessDocument(result.SelectedPath);
/// </code>
/// Multiple files:
/// <code>
/// var result = new FileOpenDialogBuilder()
/// .AddFilter("Images", "*.png;*.jpg;*.bmp")
/// .AllowMultipleSelection()
/// .ShowDialog();
///
/// if (!result.WasCancelled)
/// foreach (string path in result.SelectedPaths)
/// ImportImage(path);
/// </code>
/// </example>
public sealed class FileOpenDialogBuilder
{
    /// <summary>The configured file type filters.</summary>
    private readonly List<(string Name, string Pattern)> _filters = [];

    /// <summary>The configured custom places.</summary>
    private readonly List<(string Path, bool AtTop)> _places = [];

    /// <summary>The configured dialog title.</summary>
    private string _title;

    /// <summary>The configured initial directory.</summary>
    private string _initialDirectory;

    /// <summary>The configured default extension.</summary>
    private string _defaultExtension;

    /// <summary>A value indicating whether multiple selection is enabled.</summary>
    private bool _allowMultiSelect;

    /// <summary>Sets the text shown in the dialog title bar.</summary>
    /// <param name="title">Title bar text. <see langword="null" /> uses the Windows default ("Open").</param>
    /// <returns>The current builder or result value.</returns>
    public FileOpenDialogBuilder WithTitle(string title)
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
    public FileOpenDialogBuilder WithInitialDirectory(string path)
    {
        _initialDirectory = path;
        return this;
    }

    /// <summary>Adds a file-type filter entry to the type drop-down.</summary>
    /// <param name="name">Friendly name, e.g. <c>"Image files"</c>.</param>
    /// <param name="pattern">File pattern, e.g. <c>"*.png;*.jpg"</c>.</param>
    /// <returns>The current builder or result value.</returns>
    public FileOpenDialogBuilder AddFilter(string name, string pattern)
    {
        _filters.Add((name, pattern));
        return this;
    }

    /// <summary>Sets the extension appended when the user omits one (without the leading dot, e.g. <c>"txt"</c>).</summary>
    /// <param name="extension">The extension value.</param>
    /// <returns>The current builder or result value.</returns>
    public FileOpenDialogBuilder WithDefaultExtension(string extension)
    {
        _defaultExtension = extension;
        return this;
    }

    /// <summary>Enables multiple file selection. Use <see cref="P:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FileDialogResult.SelectedPaths" /> for the result.</summary>
    /// <returns>The current builder or result value.</returns>
    public FileOpenDialogBuilder AllowMultipleSelection()
    {
        _allowMultiSelect = true;
        return this;
    }

    /// <summary>Adds a custom location to the dialog's navigation sidebar.</summary>
    /// <param name="path">Full path to the directory to add.</param>
    /// <param name="atTop">
    /// <see langword="true" /> to add the place at the top of the sidebar;
    /// <see langword="false" /> (default) to add it at the bottom.
    /// </param>
    /// <returns>The current builder or result value.</returns>
    public FileOpenDialogBuilder AddPlace(string path, bool atTop)
    {
        _places.Add((path, atTop));
        return this;
    }

    /// <summary>Adds a custom location to the bottom of the dialog's navigation sidebar.</summary>
    /// <param name="path">Full path to the directory to add.</param>
    /// <returns>The current builder.</returns>
    public FileOpenDialogBuilder AddPlace(string path) => AddPlace(path, atTop: false);

    /// <summary>Shows the dialog and returns the result.</summary>
    /// <returns>A <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FileDialogResult" /> describing the outcome.</returns>
    /// <exception cref="T:System.Runtime.InteropServices.COMException">An unexpected COM error occurred.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">Called on a non-Windows platform.</exception>
    public FileDialogResult ShowDialog() => ShowDialog((IntPtr)0);

    /// <summary>Shows the dialog and returns the result.</summary>
    /// <param name="ownerHandle">
    /// Handle of the owner window. Pass <see cref="F:System.IntPtr.Zero" /> (default) for a top-level dialog.
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
    internal FileDialogResult ShowDialog(IntPtr ownerHandle, IFileDialogExecutor executor) => executor.ShowOpen(new(ownerHandle.ToInt64(), _title, _initialDirectory, _defaultExtension, _filters.ToArray(), _places.ToArray(), _allowMultiSelect));
}
