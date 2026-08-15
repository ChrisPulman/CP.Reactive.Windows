// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi;

/// <summary>Represents a native GDI+ bitmap effect application.</summary>
/// <param name="bitmap">The target bitmap.</param>
/// <param name="effect">The effect to apply.</param>
/// <param name="rectOfInterest">The rectangle of interest.</param>
/// <param name="useAuxData">Whether auxiliary data should be used.</param>
/// <param name="auxData">The auxiliary data pointer.</param>
/// <param name="auxDataSize">The auxiliary data size.</param>
/// <returns>The GDI+ status.</returns>
internal delegate GdiPlusStatus GdiPlusApplyEffectOperation(nint bitmap, nint effect, ref NativeRect rectOfInterest, bool useAuxData, nint auxData, int auxDataSize);
