// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;

/// <summary>Specifies a raster-operation code for combining source and destination colors.</summary>
public enum RasterOperations : uint
{
    /// <summary>Specifies no raster operation.</summary>
    None = 0U,

    /// <summary>Copies the source area to the destination area.</summary>
    SourceCopy = 13_369_376U,

    /// <summary>Combines source and destination colors with a Boolean OR operation.</summary>
    SourcePaint = 15_597_702U,

    /// <summary>Combines source and destination colors with a Boolean AND operation.</summary>
    SourceAnd = 8_913_094U,

    /// <summary>Combines source and destination colors with a Boolean XOR operation.</summary>
    SourceInvert = 6_684_742U,

    /// <summary>Combines the source with the inverse destination using Boolean AND.</summary>
    SourceErase = 4_457_256U,

    /// <summary>Prevents bitmap mirroring.</summary>
    NoMirrorBitmap = 2_147_483_648U,

    /// <summary>Copies the inverse source area to the destination area.</summary>
    NotSourceCopy = 3_342_344U,

    /// <summary>Combines inverse source and inverse destination colors with Boolean AND.</summary>
    NotSourceErase = 1_114_278U,

    /// <summary>Combines source and pattern colors with a Boolean AND operation.</summary>
    MergeCopy = 12_583_114U,

    /// <summary>Combines inverse source and destination colors with a Boolean OR operation.</summary>
    MergePaint = 12_255_782U,

    /// <summary>Copies the selected brush pattern to the destination bitmap.</summary>
    PatternCopy = 15_728_673U,

    /// <summary>Combines the selected brush, source, and destination colors.</summary>
    PatternPaint = 16_452_105U,

    /// <summary>Combines the selected brush and destination colors with Boolean XOR.</summary>
    PatternInvert = 5_898_313U,

    /// <summary>Inverts the destination area.</summary>
    DestinationInvert = 5_570_569U,

    /// <summary>Fills the destination area with black.</summary>
    Blackness = 66U,

    /// <summary>Fills the destination area with white.</summary>
    Whiteness = 16_711_778U,

    /// <summary>Includes layered windows in the resulting image.</summary>
    CaptureBlt = 1_073_741_824U,
}
