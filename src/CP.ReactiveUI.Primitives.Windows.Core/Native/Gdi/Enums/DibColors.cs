// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;

/// <summary>Defines how to interpret values in a device-independent bitmap color table.</summary>
public enum DibColors : uint
{
    /// <summary>Specifies a color table containing literal RGB values.</summary>
    RgbColors,

    /// <summary>Specifies a color table containing indexes into the current logical palette.</summary>
    PalColors,

    /// <summary>Specifies that DIB pixels index the current logical palette.</summary>
    PalIndices,
}
