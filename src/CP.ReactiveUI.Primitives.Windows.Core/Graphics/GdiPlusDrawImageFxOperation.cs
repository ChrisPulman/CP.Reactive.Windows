// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi;

/// <summary>Represents native GDI+ image drawing through an effect.</summary>
/// <param name="graphics">The target graphics handle.</param>
/// <param name="bitmap">The source bitmap handle.</param>
/// <param name="source">The source rectangle.</param>
/// <param name="matrix">The transform matrix handle.</param>
/// <param name="effect">The effect handle.</param>
/// <param name="imageAttributes">The image attributes handle.</param>
/// <param name="sourceUnit">The source unit.</param>
/// <returns>The GDI+ status.</returns>
internal delegate GdiPlusStatus GdiPlusDrawImageFxOperation(
    nint graphics,
    nint bitmap,
    ref NativeRectFloat source,
    nint matrix,
    nint effect,
    nint imageAttributes,
    GpUnit sourceUnit);
