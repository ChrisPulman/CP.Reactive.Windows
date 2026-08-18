// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard.Internals;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.Internals;
#endif
/// <summary>Represents a dropped-file enumeration operation.</summary>
/// <param name="dropHandle">The HDROP handle.</param>
/// <param name="fileIndex">The file index, or uint.MaxValue for the count.</param>
/// <param name="fileName">The output filename buffer.</param>
/// <param name="characterCount">The output buffer character count.</param>
/// <returns>The copied character count or file count.</returns>
internal unsafe delegate int DragQueryFileOperation(IntPtr dropHandle, uint fileIndex, char* fileName, int characterCount);
