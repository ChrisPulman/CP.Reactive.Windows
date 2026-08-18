// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
#endif

/// <summary>Copies a native clipboard format name into the supplied buffer.</summary>
/// <param name="formatId">The clipboard format identifier.</param>
/// <param name="destination">The destination buffer.</param>
/// <returns>The copied character count, or zero when no name is available.</returns>
internal delegate int NativeFormatNameOperation(uint formatId, Span<char> destination);
