// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Extensions;

/// <summary>Helper method for the NativeSizeFloatExtensions struct.</summary>
public static class NativeSizeFloatExtensions
{
    extension(NativeSizeFloat size)
    {
        /// <summary>Create a new NativeSizeFloat, from the supplied one, using the specified width.</summary>
        /// <param name="width">float</param>
        /// <returns>NativeSizeFloat.</returns>
        public NativeSizeFloat ChangeWidth(float width) => new(width, size.Height);

        /// <summary>Create a new NativeSizeFloat, from the supplied one, using the specified height.</summary>
        /// <param name="height">float</param>
        /// <returns>NativeSizeFloat.</returns>
        public NativeSizeFloat ChangeHeight(float height) => new(size.Width, height);

        /// <summary>Create a NativeSize, using rounded values, from the specified NativeSizeFloat.</summary>
        /// <returns>NativeSize.</returns>
        public NativeSize Round() => checked(new NativeSize((int)Math.Round(size.Width), (int)Math.Round(size.Height)));
    }
}
