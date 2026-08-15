// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Extensions;

/// <summary>Helper method for the NativePointFloat struct.</summary>
public static class NativePointFloatExtensions
{
    extension(NativePointFloat point)
    {
        /// <summary>Create a new NativePointFloat, from the supplied one, using the specified X coordinate.</summary>
        /// <param name="x">float</param>
        /// <returns>NativePointFloat.</returns>
        public NativePointFloat ChangeX(float x) => new(x, point.Y);

        /// <summary>Create a new NativePointFloat, from the supplied one, using the specified Y coordinate.</summary>
        /// <param name="y">float</param>
        /// <returns>NativePointFloat.</returns>
        public NativePointFloat ChangeY(float y) => new(point.X, y);

        /// <summary>Create a new NativePointFloat using no nullable offset values.</summary>
        /// <returns>A native point at the previous default offset result.</returns>
        public NativePointFloat Offset() => point.Offset(null, null);

        /// <summary>Create a new NativePointFloat using the specified nullable X offset.</summary>
        /// <param name="offsetX">The nullable X offset.</param>
        /// <returns>The offset native point.</returns>
        public NativePointFloat Offset(float? offsetX) => point.Offset(offsetX, null);

        /// <summary>Create a new NativePointFloat using the specified nullable offsets.</summary>
        /// <param name="offsetX">The nullable X offset.</param>
        /// <param name="offsetY">The nullable Y offset.</param>
        /// <returns>The offset native point.</returns>
        public NativePointFloat Offset(float? offsetX, float? offsetY) => new((point.X + offsetX).GetValueOrDefault(), (point.Y + offsetY).GetValueOrDefault());

        /// <summary>Create a new NativePointFloat by applying another point as an offset.</summary>
        /// <param name="offset">NativePointFloat</param>
        /// <returns>NativePointFloat.</returns>
        public NativePointFloat Offset(NativePointFloat offset) => new(point.X + offset.X, point.Y + offset.Y);

        /// <summary>Create a NativePoint, using rounded values, from the specified NativePointFloat.</summary>
        /// <returns>NativePoint.</returns>
        public NativePoint Round() => checked(new NativePoint((int)Math.Round(point.X), (int)Math.Round(point.Y)));
    }
}
