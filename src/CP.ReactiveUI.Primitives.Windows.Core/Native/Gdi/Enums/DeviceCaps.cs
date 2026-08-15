// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;

/// <summary>Specifies an index accepted by GDI32.GetDeviceCaps.</summary>
public enum DeviceCaps
{
    /// <summary>Specifies the device driver version.</summary>
    DRIVERVERSION = 0,

    /// <summary>Specifies the device classification.</summary>
    TECHNOLOGY = 2,

    /// <summary>Specifies the horizontal size in millimeters.</summary>
    HORZSIZE = 4,

    /// <summary>Specifies the vertical size in millimeters.</summary>
    VERTSIZE = 6,

    /// <summary>Specifies the horizontal resolution in pixels.</summary>
    HORZRES = 8,

    /// <summary>Specifies the vertical resolution in pixels.</summary>
    VERTRES = 10,

    /// <summary>Specifies the number of bits per pixel.</summary>
    BITSPIXEL = 12,

    /// <summary>Specifies the number of planes.</summary>
    PLANES = 14,

    /// <summary>Specifies the number of brushes.</summary>
    NUMBRUSHES = 16,

    /// <summary>Specifies the number of pens.</summary>
    NUMPENS = 18,

    /// <summary>Specifies the number of markers.</summary>
    NUMMARKERS = 20,

    /// <summary>Specifies the number of fonts.</summary>
    NUMFONTS = 22,

    /// <summary>Specifies the number of colors.</summary>
    NUMCOLORS = 24,

    /// <summary>Specifies the device descriptor size.</summary>
    PDEVICESIZE = 26,

    /// <summary>Specifies the curve capabilities.</summary>
    CURVECAPS = 28,

    /// <summary>Specifies the line capabilities.</summary>
    LINECAPS = 30,

    /// <summary>Specifies the polygonal capabilities.</summary>
    POLYGONALCAPS = 32,

    /// <summary>Specifies the text capabilities.</summary>
    TEXTCAPS = 34,

    /// <summary>Specifies the clipping capabilities.</summary>
    CLIPCAPS = 36,

    /// <summary>Specifies the bit-block transfer capabilities.</summary>
    RASTERCAPS = 38,

    /// <summary>Specifies the X-axis length.</summary>
    ASPECTX = 40,

    /// <summary>Specifies the Y-axis length.</summary>
    ASPECTY = 42,

    /// <summary>Specifies the hypotenuse length.</summary>
    ASPECTXY = 44,

    /// <summary>Specifies the shading and blending capabilities.</summary>
    SHADEBLENDCAPS = 45,

    /// <summary>Specifies the logical pixels per inch on the X-axis.</summary>
    LOGPIXELSX = 88,

    /// <summary>Specifies the logical pixels per inch on the Y-axis.</summary>
    LOGPIXELSY = 90,

    /// <summary>Specifies the number of entries in the physical palette.</summary>
    SIZEPALETTE = 104,

    /// <summary>Specifies the number of reserved palette entries.</summary>
    NUMRESERVED = 106,

    /// <summary>Specifies the actual color resolution.</summary>
    COLORRES = 108,

    /// <summary>Specifies the physical width in device units.</summary>
    PHYSICALWIDTH = 110,

    /// <summary>Specifies the physical height in device units.</summary>
    PHYSICALHEIGHT = 111,

    /// <summary>Specifies the physical printable-area X margin.</summary>
    PHYSICALOFFSETX = 112,

    /// <summary>Specifies the physical printable-area Y margin.</summary>
    PHYSICALOFFSETY = 113,

    /// <summary>Specifies the X-axis scaling factor.</summary>
    SCALINGFACTORX = 114,

    /// <summary>Specifies the Y-axis scaling factor.</summary>
    SCALINGFACTORY = 115,

    /// <summary>Specifies the display vertical refresh rate in hertz.</summary>
    VREFRESH = 116,

    /// <summary>Specifies the desktop height in pixels.</summary>
    DESKTOPVERTRES = 117,

    /// <summary>Specifies the desktop width in pixels.</summary>
    DESKTOPHORZRES = 118,

    /// <summary>Specifies the preferred bit-block transfer alignment.</summary>
    BLTALIGNMENT = 119,
}
