// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi;

/// <summary>Represents native GDI+ effect creation.</summary>
/// <param name="guid">The effect identifier.</param>
/// <param name="effect">Receives the native effect handle.</param>
/// <returns>The GDI+ status.</returns>
internal delegate GdiPlusStatus GdiPlusCreateEffectOperation(ref Guid guid, out nint effect);
