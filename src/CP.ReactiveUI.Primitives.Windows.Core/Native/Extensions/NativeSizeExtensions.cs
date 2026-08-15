// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Extensions;

/// <summary>Helper method for the NativeSizeExtensions struct.</summary>
public static class NativeSizeExtensions
{
    /// <summary>Provides extension members for the target value.</summary>
    /// <param name="size">The target value.</param>
    extension(NativeSize size)
    {
        /// <summary>Create a new NativeSize, from the supplied one, using the specified width.</summary>
        /// <param name="width">int</param>
        /// <returns>NativeSize.</returns>
        public NativeSize ChangeWidth(int width) => new(width, size.Height);

        /// <summary>Create a new NativeSize, from the supplied one, using the specified height.</summary>
        /// <param name="height">int</param>
        /// <returns>NativeSize.</returns>
        public NativeSize ChangeHeight(int height) => new(size.Width, height);
    }
}
