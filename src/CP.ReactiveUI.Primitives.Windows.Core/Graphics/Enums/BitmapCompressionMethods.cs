// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;

/// <summary>Specifies the compression type used by a bitmap information header.</summary>
public enum BitmapCompressionMethods : uint
{
    /// <summary>Specifies no compression.</summary>
    BI_RGB,
    /// <summary>Specifies run-length encoding for 8-bit-per-pixel bitmaps.</summary>
    BI_RLE8,
    /// <summary>Specifies run-length encoding for 4-bit-per-pixel bitmaps.</summary>
    BI_RLE4,
    /// <summary>Specifies uncompressed pixels with three DWORD color masks.</summary>
    BI_BITFIELDS,
    /// <summary>Specifies a JPEG image.</summary>
    BI_JPEG,
    /// <summary>Specifies a PNG image.</summary>
    BI_PNG
}
