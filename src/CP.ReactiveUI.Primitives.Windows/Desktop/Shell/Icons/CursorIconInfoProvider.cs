// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif

/// <summary>Retrieves extended icon information for a cursor handle.</summary>
/// <param name="cursorHandle">The cursor handle.</param>
/// <param name="iconInfo">The icon information to populate.</param>
/// <returns><see langword="true" /> when the icon information was retrieved.</returns>
internal delegate bool CursorIconInfoProvider(IntPtr cursorHandle, ref IconInfoEx iconInfo);
