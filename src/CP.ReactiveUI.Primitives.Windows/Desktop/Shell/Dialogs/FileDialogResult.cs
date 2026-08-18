// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs;
#endif
/// <summary>Represents the outcome of showing a file or folder dialog.</summary>
/// <remarks>
/// <para>
/// A result is either a <em>cancellation</em> (<see cref="P:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FileDialogResult.WasCancelled" /> is <see langword="true" />)
/// or a <em>successful selection</em> (<see cref="P:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FileDialogResult.WasCancelled" /> is <see langword="false" />).
/// </para>
/// <para>
/// If the dialog fails unexpectedly (e.g. COM error, platform not supported), the builder's
/// <c>ShowDialog</c> method throws — a <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FileDialogResult" /> is never returned for failures.
/// This makes it easy to distinguish the three cases:
/// </para>
/// <code>
/// FileDialogResult result;
/// try
/// {
/// result = new FileOpenDialogBuilder().AddFilter("All files", "*.*").ShowDialog();
/// }
/// catch (System.Runtime.InteropServices.COMException ex)
/// {
/// // Unexpected COM failure — log or rethrow.
/// logger.LogError(ex, "Dialog failed: {Hr}", ex.ErrorCode);
/// return;
/// }
/// catch (System.PlatformNotSupportedException)
/// {
/// // Non-Windows OS — fall back to a text input.
/// return;
/// }
///
/// if (result.WasCancelled)
/// return; // User pressed Cancel or Escape — not an error.
///
/// string path = result.SelectedPath; // guaranteed non-null here
/// </code>
/// </remarks>
public sealed class FileDialogResult
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FileDialogResult" /> class.</summary>
    /// <param name="wasCancelled">A value indicating whether the user cancelled the dialog.</param>
    /// <param name="selectedPath">The selected path.</param>
    /// <param name="selectedPaths">The selected paths.</param>
    private FileDialogResult(bool wasCancelled, string selectedPath, IReadOnlyList<string> selectedPaths)
    {
        WasCancelled = wasCancelled;
        SelectedPath = selectedPath;
        SelectedPaths = selectedPaths;
    }

    /// <summary>
    /// Gets a value indicating whether the user dismissed the dialog without making a selection
    /// (by pressing Cancel, Escape, or the × button).
    /// </summary>
    /// <remarks>
    /// When <see langword="true" />, both
    /// <see cref="P:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FileDialogResult.SelectedPath" />
    /// and
    /// <see cref="P:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FileDialogResult.SelectedPaths" />
    /// will be <see langword="null" />. No exception is thrown for cancellations.
    /// </remarks>
    public bool WasCancelled { get; }

    /// <summary>
    /// Gets the selected file or folder path, or <see langword="null" /> if the dialog was cancelled.
    /// For multi-select results, returns the first selected path.
    /// </summary>
    public string SelectedPath { get; }

    /// <summary>
    /// Gets all selected paths when the dialog was configured for multiple selection
    /// (via <see cref="M:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.FileOpenDialogBuilder.AllowMultipleSelection" />),
    /// or <see langword="null" /> if the dialog was cancelled or configured for single selection.
    /// </summary>
    public IReadOnlyList<string> SelectedPaths { get; }

    /// <summary>Creates a cancelled dialog result.</summary>
    /// <returns>The cancelled dialog result.</returns>
    internal static FileDialogResult Cancelled() => new(wasCancelled: true, null, null);

    /// <summary>Creates a dialog result for a single path.</summary>
    /// <param name="path">The selected path.</param>
    /// <returns>The dialog result.</returns>
    internal static FileDialogResult FromPath(string path) => new(wasCancelled: false, path, null);

    /// <summary>Creates a dialog result for multiple paths.</summary>
    /// <param name="paths">The selected paths.</param>
    /// <returns>The dialog result.</returns>
    internal static FileDialogResult FromPaths(IReadOnlyList<string> paths) =>
        new(wasCancelled: false, paths.Count > 0 ? paths[0] : null, paths);
}
