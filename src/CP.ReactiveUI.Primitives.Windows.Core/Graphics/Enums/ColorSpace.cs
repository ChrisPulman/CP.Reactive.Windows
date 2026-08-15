// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;

/// <summary>Specifies the color-space and gamut-mapping values used by a bitmap V5 header.</summary>
public enum ColorSpace : uint
{
    /// <summary>Specifies calibrated red, green, blue values.</summary>
    LCS_CALIBRATED_RGB = 0U,
    /// <summary>Maintains saturation for business charts and other undithered colors.</summary>
    LCS_GM_BUSINESS = 1U,
    /// <summary>Maintains a colorimetric match for graphic designs and named colors.</summary>
    LCS_GM_GRAPHICS = 2U,
    /// <summary>Maintains contrast for photographs and natural images.</summary>
    LCS_GM_IMAGES = 4U,
    /// <summary>Maintains the white point by mapping colors to the nearest destination-gamut color.</summary>
    LCS_GM_ABS_COLORIMETRIC = 8U,
    /// <summary>Specifies the sRGB color space.</summary>
    LCS_sRGB = 1_934_772_034U,
    /// <summary>Specifies the Windows default color space.</summary>
    LCS_WINDOWS_COLOR_SPACE = 1_466_527_264U,
    /// <summary>Specifies that the profile data points to a profile file name.</summary>
    PROFILE_LINKED = 1_466_527_265U,
    /// <summary>Specifies that the profile data points to an in-memory profile buffer.</summary>
    PROFILE_EMBEDDED = 1_466_527_266U
}
