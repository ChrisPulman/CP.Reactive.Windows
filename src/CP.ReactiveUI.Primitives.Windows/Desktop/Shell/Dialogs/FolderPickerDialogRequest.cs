// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs;
#endif
/// <summary>Immutable folder-picker dialog request settings.</summary>
/// <param name="OwnerHandle">The owner window handle.</param>
/// <param name="Title">The dialog title.</param>
/// <param name="InitialDirectory">The initial directory.</param>
internal sealed record FolderPickerDialogRequest(long OwnerHandle, string Title, string InitialDirectory);
