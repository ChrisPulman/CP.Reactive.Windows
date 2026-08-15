// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Defines a version-query operation.</summary>
/// <param name="versionInfo">The native version information buffer.</param>
/// <returns><see langword="true"/> when the query succeeds.</returns>
internal unsafe delegate bool GetVersionExOperation(void* versionInfo);
