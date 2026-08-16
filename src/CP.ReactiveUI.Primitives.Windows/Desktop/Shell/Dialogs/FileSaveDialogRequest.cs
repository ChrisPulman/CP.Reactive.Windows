// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs;
#endif
/// <summary>Immutable save-file dialog request settings.</summary>
/// <param name="OwnerHandle">The owner window handle.</param>
/// <param name="Title">The dialog title.</param>
/// <param name="InitialDirectory">The initial directory.</param>
/// <param name="SuggestedFileName">The suggested file name.</param>
/// <param name="DefaultExtension">The default extension.</param>
/// <param name="Filters">The file-type filters.</param>
/// <param name="Places">The custom sidebar places.</param>
internal sealed record FileSaveDialogRequest(
    long OwnerHandle,
    string Title,
    string InitialDirectory,
    string SuggestedFileName,
    string DefaultExtension,
    IReadOnlyList<(string Name, string Pattern)> Filters,
    IReadOnlyList<(string Path, bool AtTop)> Places);
