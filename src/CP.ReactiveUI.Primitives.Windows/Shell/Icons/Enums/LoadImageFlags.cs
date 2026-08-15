// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.Enums;
#endif
/// <summary>
/// Specifies options for loading images, such as icons, cursors, or bitmaps, including color mode, size, source, and
/// sharing behavior.
/// </summary>
/// <remarks>This enumeration supports bitwise combination of its member values to specify multiple loading
/// options simultaneously. It is typically used with image loading functions to control how images are loaded from
/// files or resources, including whether to use system colors, load images as DIB sections, or share image handles
/// across multiple loads.</remarks>
[Flags]
public enum LoadImageFlags : uint
{
    /// <summary>No load-image flags are enabled.</summary>
    None = 0U,
    /// <summary>Loads the image in black and white.</summary>
    LR_MONOCHROME = 1U,
    /// <summary>Loads the image in color.</summary>
    LR_COLOR = 2U,
    /// <summary>Replaces the first gray pixel in the color table with the system window color.</summary>
    LR_LOADTRANSPARENT = 0x20U,
    /// <summary>Uses system metric values for cursors or icons if the requested width or height is zero.</summary>
    LR_DEFAULTSIZE = 0x40U,
    /// <summary>Uses true VGA colors.</summary>
    LR_VGACOLOR = 0x80U,
    /// <summary>Loads the standalone image from the file specified by lpszName (icon, cursor, or bitmap file).</summary>
    LR_LOADFROMFILE = 0x10U,
    /// <summary>Replaces shades of gray in the color table with corresponding 3-D object colors.</summary>
    LR_LOADMAP3DCOLORS = 0x1000U,
    /// <summary>Maps the first image color table to the current desktop colors.</summary>
    LR_CREATEDIBSECTION = 0x2000U,
    /// <summary>Loads the image as a DIB section rather than a compatible bitmap.</summary>
    LR_COPYFROMRESOURCE = 0x4000U,
    /// <summary>Shares the image handle if the image is loaded multiple times.</summary>
    LR_SHARED = 0x8000U
}
