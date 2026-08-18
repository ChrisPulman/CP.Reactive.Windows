// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.Enums;
#endif
/// <summary>Defines flags that specify options for copying images, such as color depth and resource handling.</summary>
/// <remarks>Use this enumeration with image-related functions to control behaviors like creating monochrome
/// images, using system default sizes, or managing the original image resource. Multiple flags can be combined using a
/// bitwise OR operation to specify more than one option.</remarks>
[Flags]
public enum CopyImageFlags : uint
{
    /// <summary>No copy-image flags are enabled.</summary>
    None = 0U,
    /// <summary>Creates a new monochrome image.</summary>
    LR_MONOCHROME = 1U,
    /// <summary>Returns the original handle if it satisfies the size and color-depth criteria.</summary>
    LR_COPYRETURNORG = 4U,
    /// <summary>Deletes the original image after creating the copy.</summary>
    LR_COPYDELETEORG = 8U,
    /// <summary>Uses system metric values for cursors or icons if the width or height is zero.</summary>
    LR_DEFAULTSIZE = 0x40U,
    /// <summary>If uType is IMAGE_BITMAP, creates a DIB section instead of a DDB.</summary>
    LR_CREATEDIBSECTION = 0x2000U,
    /// <summary>Tries to reload the icon or cursor from the original resource file for a better resize.</summary>
    LR_COPYFROMRESOURCE = 0x4000U,
}
