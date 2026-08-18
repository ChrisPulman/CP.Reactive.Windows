// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
#endif

/// <summary>Obtains a native clipboard format name into a caller-provided character buffer.</summary>
/// <param name="formatId">The clipboard format identifier.</param>
/// <param name="formatName">The destination character buffer.</param>
/// <param name="characterCapacity">The capacity of <paramref name="formatName" /> in characters.</param>
/// <returns>The number of copied characters, or zero when the format has no registered name.</returns>
internal unsafe delegate int NativeClipboardFormatNamePointerOperation(uint formatId, char* formatName, int characterCapacity);
