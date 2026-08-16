// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;

/// <summary>Specifies the unit of measure used by GDI+.</summary>
public enum GpUnit
{
    /// <summary>Specifies a nonphysical world-coordinate unit.</summary>
    UnitWorld,

    /// <summary>Specifies a variable unit used only for page transforms.</summary>
    UnitDisplay,

    /// <summary>Specifies a device pixel.</summary>
    UnitPixel,

    /// <summary>Specifies one printer point, or one seventy-second of an inch.</summary>
    UnitPoint,

    /// <summary>Specifies one inch.</summary>
    UnitInch,

    /// <summary>Specifies one three-hundredth of an inch.</summary>
    UnitDocument,

    /// <summary>Specifies one millimeter.</summary>
    UnitMillimeter,
}
