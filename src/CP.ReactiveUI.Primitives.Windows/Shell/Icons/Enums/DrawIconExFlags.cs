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
///     A raster-operation code. These codes define how the color data for the source rectangle is to be combined with the
///     color data for the destination rectangle to achieve the final color.
/// </summary>
[Flags]
public enum DrawIconExFlags : uint
{
    /// <summary>No draw-icon flags are enabled.</summary>
    None = 0U,
    /// <summary>Draws the icon or cursor using the mask.</summary>
    DI_MASK = 1U,
    /// <summary>Draws the icon or cursor using the image.</summary>
    DI_IMAGE = 2U,
    /// <summary>Combines DI_IMAGE and DI_MASK.</summary>
    DI_NORMAL = DI_MASK | DI_IMAGE,
    /// <summary>This flag is ignored.</summary>
    DI_COMPAT = 4U,
    /// <summary>Uses system metric icon values when width and height are zero.</summary>
    DI_DEFAULTSIZE = 8U,
    /// <summary>Draws the icon as an unmirrored icon.</summary>
    DI_NOMIRROR = 0x10U
}
