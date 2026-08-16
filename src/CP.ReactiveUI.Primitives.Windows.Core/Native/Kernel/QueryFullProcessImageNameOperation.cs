// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Defines a process-image-name-query operation.</summary>
/// <param name="processHandle">The process handle.</param>
/// <param name="flags">The query flags.</param>
/// <param name="executableName">The destination buffer.</param>
/// <param name="size">The buffer size on entry and characters written on success.</param>
/// <returns><see langword="true"/> when the query succeeds.</returns>
internal unsafe delegate bool QueryFullProcessImageNameOperation(
    IntPtr processHandle,
    uint flags,
    char* executableName,
    ref int size);
