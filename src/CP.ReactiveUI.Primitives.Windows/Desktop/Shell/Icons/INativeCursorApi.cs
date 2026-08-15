// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif
/// <summary>Composes native cursor access for production and deterministic tests.</summary>
internal interface INativeCursorApi
{
    /// <summary>Copies an image handle.</summary>
    /// <param name="imageHandle">The image handle.</param>
    /// <param name="type">The image type.</param>
    /// <param name="cx">The requested width.</param>
    /// <param name="cy">The requested height.</param>
    /// <param name="flags">The copy flags.</param>
    /// <returns>The copied image handle.</returns>
    IntPtr CopyImage(IntPtr imageHandle, ImageType type, int cx, int cy, CopyImageFlags flags);

    /// <summary>Loads an image by integer resource name.</summary>
    /// <param name="instanceHandle">The instance handle.</param>
    /// <param name="name">The image resource name.</param>
    /// <param name="type">The image type.</param>
    /// <param name="cx">The requested width.</param>
    /// <param name="cy">The requested height.</param>
    /// <param name="loadFlags">The load flags.</param>
    /// <returns>The loaded image handle.</returns>
    IntPtr LoadImage(IntPtr instanceHandle, IntPtr name, ImageType type, int cx, int cy, LoadImageFlags loadFlags);

    /// <summary>Loads an image by string resource name.</summary>
    /// <param name="instanceHandle">The instance handle.</param>
    /// <param name="name">The image resource name.</param>
    /// <param name="type">The image type.</param>
    /// <param name="cx">The requested width.</param>
    /// <param name="cy">The requested height.</param>
    /// <param name="loadFlags">The load flags.</param>
    /// <returns>The loaded image handle.</returns>
    IntPtr LoadImage(IntPtr instanceHandle, string name, ImageType type, int cx, int cy, LoadImageFlags loadFlags);
}
