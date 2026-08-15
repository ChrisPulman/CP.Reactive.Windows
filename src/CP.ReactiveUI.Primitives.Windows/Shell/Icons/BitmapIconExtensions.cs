// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing;
using CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif
/// <summary>Extension methods for bitmap icon handles.</summary>
public static class BitmapIconExtensions
{
    /// <summary>Provides extension members for the target instance.</summary>
    /// <param name="bitmap">The extended instance.</param>
    extension(Bitmap bitmap)
    {
        /// <summary>Gets a safe icon handle for the bitmap.</summary>
        public SafeIconHandle SafeIconHandle => new(bitmap);
    }
}
