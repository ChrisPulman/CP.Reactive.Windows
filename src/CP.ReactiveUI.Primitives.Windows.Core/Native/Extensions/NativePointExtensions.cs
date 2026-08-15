// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Extensions;

/// <summary>Helper method for the NativePoint struct.</summary>
public static class NativePointExtensions
{
    extension(NativePoint point)
    {
        /// <summary>Create a new NativePoint, from the supplied one, using the specified X coordinate.</summary>
        /// <param name="x">int</param>
        /// <returns>NativePoint.</returns>
        public NativePoint ChangeX(int x) => new(x, point.Y);

        /// <summary>Create a new NativePoint, from the supplied one, using the specified Y coordinate.</summary>
        /// <param name="y">int</param>
        /// <returns>NativePoint.</returns>
        public NativePoint ChangeY(int y) => new(point.X, y);

        /// <summary>Create a new NativePoint by applying another point as an offset.</summary>
        /// <param name="offset">NativePoint offset</param>
        /// <returns>The offset native point.</returns>
        public NativePoint Offset(NativePoint offset) => checked(new NativePoint(point.X + offset.X, point.Y + offset.Y));

        /// <summary>Create a new NativePoint using no nullable offset values.</summary>
        /// <returns>A native point at the previous default offset result.</returns>
        public NativePoint Offset() => point.Offset(null, null);

        /// <summary>Create a new NativePoint using the specified nullable X offset.</summary>
        /// <param name="offsetX">The nullable X offset.</param>
        /// <returns>The offset native point.</returns>
        public NativePoint Offset(int? offsetX) => point.Offset(offsetX, null);

        /// <summary>Create a new NativePoint by applying nullable coordinate offsets.</summary>
        /// <param name="offsetX">The nullable horizontal offset.</param>
        /// <param name="offsetY">The nullable vertical offset.</param>
        /// <returns>A native point offset by the supplied nullable coordinates.</returns>
        public NativePoint Offset(int? offsetX, int? offsetY) => checked(new NativePoint((point.X + offsetX).GetValueOrDefault(), (point.Y + offsetY).GetValueOrDefault()));
    }
}
