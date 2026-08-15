// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Gets the executable image file name for a process.</summary>
/// <param name="processHandle">Process handle.</param>
/// <param name="imageFileName">Output image file name buffer.</param>
/// <param name="size">Output buffer size.</param>
/// <returns>Number of copied characters.</returns>
internal unsafe delegate int GetProcessImageFileNameOperation(nint processHandle, char* imageFileName, int size);
