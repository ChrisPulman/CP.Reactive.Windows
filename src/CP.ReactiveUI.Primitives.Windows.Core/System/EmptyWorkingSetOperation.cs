// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Removes as many pages as possible from a process working set.</summary>
/// <param name="processHandle">Process handle.</param>
/// <returns>Nonzero on success; otherwise, zero.</returns>
internal delegate int EmptyWorkingSetOperation(nint processHandle);
