// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Internal;

/// <summary>The transformed top-left and bottom-right rectangle corners.</summary>
/// <param name="TopLeftX">The transformed top-left x-coordinate.</param>
/// <param name="TopLeftY">The transformed top-left y-coordinate.</param>
/// <param name="BottomRightX">The transformed bottom-right x-coordinate.</param>
/// <param name="BottomRightY">The transformed bottom-right y-coordinate.</param>
internal readonly record struct NativeRectangleCorners(float TopLeftX, float TopLeftY, float BottomRightX, float BottomRightY);
