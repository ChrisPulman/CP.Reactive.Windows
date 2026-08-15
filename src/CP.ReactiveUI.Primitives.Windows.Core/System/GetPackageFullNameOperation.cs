// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Defines a package-name-query operation.</summary>
/// <param name="processHandle">The process handle.</param>
/// <param name="packageFullNameLength">The package-name buffer length.</param>
/// <param name="fullName">The package-name buffer.</param>
/// <returns>The native result code.</returns>
internal unsafe delegate int GetPackageFullNameOperation(
    IntPtr processHandle,
    ref int packageFullNameLength,
    char* fullName);
