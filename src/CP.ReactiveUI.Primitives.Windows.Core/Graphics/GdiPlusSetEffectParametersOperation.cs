// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi;

/// <summary>Represents native GDI+ effect-parameter configuration.</summary>
/// <param name="effect">The native effect handle.</param>
/// <param name="parameters">The parameter memory.</param>
/// <param name="size">The parameter size.</param>
/// <returns>The GDI+ status.</returns>
internal delegate GdiPlusStatus GdiPlusSetEffectParametersOperation(nint effect, nint parameters, uint size);
