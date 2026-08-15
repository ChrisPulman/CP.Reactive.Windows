// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif

/// <summary>Retrieves native cursor information.</summary>
/// <param name="cursorInfo">The cursor information to populate.</param>
/// <returns><see langword="true" /> when the cursor information was retrieved.</returns>
internal delegate bool CursorInfoProvider(ref CursorInfo cursorInfo);
