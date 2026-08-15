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
/// <summary>
/// Provides one-line convenience wrappers around the Windows Common Item Dialog for the most common scenarios.
/// Returns <see langword="null" /> on cancellation; throws on unexpected errors.
/// </summary>
/// <remarks>
/// For more control (custom sidebar places, multi-select, suggested filenames, etc.)
/// use the builder classes directly: <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FileOpenDialogBuilder" />,
/// <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FileSaveDialogBuilder" />, or <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FolderPickerBuilder" />.
/// See <c>README.md</c> for full documentation and error-handling guidance.
/// </remarks>
public static class FileDialog
{
    /// <summary>Gets or sets the executor used by public convenience APIs.</summary>
    internal static IFileDialogExecutor DialogExecutor { get; set; } = NativeFileDialogExecutor.Instance;

    /// <summary>Presents a modern open-file dialog and returns the selected file path.</summary>
    /// <returns>The selected file path, or <see langword="null" /> when cancelled.</returns>
    /// <exception cref="T:System.Runtime.InteropServices.COMException">Unexpected COM failure.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">Called on a non-Windows platform.</exception>
    public static string PickFileToOpen() => PickFileToOpen((IntPtr)0, null, null, null, null);

    /// <summary>Presents a modern open-file dialog and returns the selected file path.</summary>
    /// <param name="ownerHandle">Handle of the owner window (<see cref="F:System.IntPtr.Zero" /> for top-level).</param>
    /// <param name="title">Title bar text; <see langword="null" /> uses the Windows default ("Open").</param>
    /// <param name="initialDirectory">Folder shown on open; <see langword="null" /> reuses the last-visited folder.</param>
    /// <param name="filters">File-type filters, e.g. <c>new[] { ("Images", "*.png;*.jpg"), ("All files", "*.*") }</c>.</param>
    /// <param name="defaultExtension">Extension appended when the user omits one (no leading dot, e.g. <c>"txt"</c>).</param>
    /// <returns>The selected file path, or <see langword="null" /> when cancelled.</returns>
    /// <exception cref="T:System.Runtime.InteropServices.COMException">Unexpected COM failure.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">Called on a non-Windows platform.</exception>
    public static string PickFileToOpen(IntPtr ownerHandle, string title, string initialDirectory, (string Name, string Pattern)[] filters, string defaultExtension)
    {
        FileOpenDialogBuilder builder = new();
        if (title is not null)
        {
            _ = builder.WithTitle(title);
        }

        if (initialDirectory is not null)
        {
            _ = builder.WithInitialDirectory(initialDirectory);
        }

        if (defaultExtension is not null)
        {
            _ = builder.WithDefaultExtension(defaultExtension);
        }

        if (filters is not null)
        {
            for (int i = 0; i < filters.Length; i++)
            {
                (string, string) f = filters[i];
                _ = builder.AddFilter(f.Item1, f.Item2);
            }
        }

        FileDialogResult result = builder.ShowDialog(ownerHandle, DialogExecutor);
        if (!result.WasCancelled)
        {
            return result.SelectedPath;
        }

        return null;
    }

    /// <summary>
    /// Presents a modern open-file dialog with multiple selection enabled and returns the selected paths,
    /// or <see langword="null" /> if the user cancelled.
    /// </summary>
    /// <returns>The selected file paths, or <see langword="null" /> when cancelled.</returns>
    /// <exception cref="T:System.Runtime.InteropServices.COMException">Unexpected COM failure.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">Called on a non-Windows platform.</exception>
    public static IReadOnlyList<string> PickFilesToOpen() => PickFilesToOpen((IntPtr)0, null, null, null);

    /// <summary>
    /// Presents a modern open-file dialog with multiple selection enabled and returns the selected paths,
    /// or <see langword="null" /> if the user cancelled.
    /// </summary>
    /// <param name="ownerHandle">Handle of the owner window (<see cref="F:System.IntPtr.Zero" /> for top-level).</param>
    /// <param name="title">Title bar text; <see langword="null" /> uses the Windows default ("Open").</param>
    /// <param name="initialDirectory">Folder shown on open; <see langword="null" /> reuses the last-visited folder.</param>
    /// <param name="filters">File-type filters, e.g. <c>new[] { ("Images", "*.png;*.jpg") }</c>.</param>
    /// <returns>The selected file paths, or <see langword="null" /> when cancelled.</returns>
    /// <exception cref="T:System.Runtime.InteropServices.COMException">Unexpected COM failure.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">Called on a non-Windows platform.</exception>
    public static IReadOnlyList<string> PickFilesToOpen(IntPtr ownerHandle, string title, string initialDirectory, (string Name, string Pattern)[] filters)
    {
        FileOpenDialogBuilder builder = new FileOpenDialogBuilder().AllowMultipleSelection();
        if (title is not null)
        {
            _ = builder.WithTitle(title);
        }

        if (initialDirectory is not null)
        {
            _ = builder.WithInitialDirectory(initialDirectory);
        }

        if (filters is not null)
        {
            for (int i = 0; i < filters.Length; i++)
            {
                (string, string) f = filters[i];
                _ = builder.AddFilter(f.Item1, f.Item2);
            }
        }

        FileDialogResult result = builder.ShowDialog(ownerHandle, DialogExecutor);
        if (!result.WasCancelled)
        {
            return result.SelectedPaths;
        }

        return [];
    }

    /// <summary>Presents a modern save-file dialog and returns the chosen path.</summary>
    /// <returns>The selected save path, or <see langword="null" /> when cancelled.</returns>
    /// <exception cref="T:System.Runtime.InteropServices.COMException">Unexpected COM failure.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">Called on a non-Windows platform.</exception>
    public static string PickFileToSave() => PickFileToSave((IntPtr)0, null, null, null, null, null);

    /// <summary>Presents a modern save-file dialog and returns the chosen path.</summary>
    /// <param name="ownerHandle">Handle of the owner window (<see cref="F:System.IntPtr.Zero" /> for top-level).</param>
    /// <param name="title">Title bar text; <see langword="null" /> uses the Windows default ("Save As").</param>
    /// <param name="initialDirectory">Folder shown on open; <see langword="null" /> reuses the last-visited folder.</param>
    /// <param name="suggestedFileName">Pre-filled file-name, e.g. <c>"screenshot.png"</c>.</param>
    /// <param name="filters">File-type filters, e.g. <c>new[] { ("PNG Image", "*.png") }</c>.</param>
    /// <param name="defaultExtension">Extension appended when the user omits one (no leading dot, e.g. <c>"png"</c>).</param>
    /// <returns>The selected save path, or <see langword="null" /> when cancelled.</returns>
    /// <exception cref="T:System.Runtime.InteropServices.COMException">Unexpected COM failure.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">Called on a non-Windows platform.</exception>
    public static string PickFileToSave(IntPtr ownerHandle, string title, string initialDirectory, string suggestedFileName, (string Name, string Pattern)[] filters, string defaultExtension)
    {
        FileSaveDialogBuilder builder = new();
        if (title is not null)
        {
            _ = builder.WithTitle(title);
        }

        if (initialDirectory is not null)
        {
            _ = builder.WithInitialDirectory(initialDirectory);
        }

        if (suggestedFileName is not null)
        {
            _ = builder.WithSuggestedFileName(suggestedFileName);
        }

        if (defaultExtension is not null)
        {
            _ = builder.WithDefaultExtension(defaultExtension);
        }

        if (filters is not null)
        {
            for (int i = 0; i < filters.Length; i++)
            {
                (string, string) f = filters[i];
                _ = builder.AddFilter(f.Item1, f.Item2);
            }
        }

        FileDialogResult result = builder.ShowDialog(ownerHandle, DialogExecutor);
        if (!result.WasCancelled)
        {
            return result.SelectedPath;
        }

        return null;
    }

    /// <summary>Presents a modern folder-picker dialog and returns the selected folder path.</summary>
    /// <returns>The selected folder path, or <see langword="null" /> when cancelled.</returns>
    /// <exception cref="T:System.Runtime.InteropServices.COMException">Unexpected COM failure.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">Called on a non-Windows platform.</exception>
    public static string PickFolder() => PickFolder((IntPtr)0, null, null);

    /// <summary>Presents a modern folder-picker dialog and returns the selected folder path.</summary>
    /// <param name="ownerHandle">Handle of the owner window (<see cref="F:System.IntPtr.Zero" /> for top-level).</param>
    /// <param name="title">Title bar text; <see langword="null" /> uses the Windows default.</param>
    /// <param name="initialDirectory">Folder shown on open; <see langword="null" /> reuses the last-visited folder.</param>
    /// <returns>The selected folder path, or <see langword="null" /> when cancelled.</returns>
    /// <exception cref="T:System.Runtime.InteropServices.COMException">Unexpected COM failure.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">Called on a non-Windows platform.</exception>
    public static string PickFolder(IntPtr ownerHandle, string title, string initialDirectory)
    {
        FolderPickerBuilder builder = new();
        if (title is not null)
        {
            _ = builder.WithTitle(title);
        }

        if (initialDirectory is not null)
        {
            _ = builder.WithInitialDirectory(initialDirectory);
        }

        FileDialogResult result = builder.ShowDialog(ownerHandle, DialogExecutor);
        if (!result.WasCancelled)
        {
            return result.SelectedPath;
        }

        return null;
    }

    /// <summary>Restores the production dialog executor.</summary>
    internal static void RestoreExecutorForTesting() => DialogExecutor = NativeFileDialogExecutor.Instance;
}
