// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Registers resources with a Restart Manager session.</summary>
/// <param name="sessionHandle">The session handle.</param>
/// <param name="fileCount">The file count.</param>
/// <param name="filenames">The file names.</param>
/// <param name="applicationCount">The application count.</param>
/// <param name="applications">The applications.</param>
/// <param name="serviceCount">The service count.</param>
/// <param name="serviceNames">The service names.</param>
/// <returns>The native result code.</returns>
internal delegate int RegisterResourcesOperation(int sessionHandle, uint fileCount, string[] filenames, uint applicationCount, RmUniqueProcess[] applications, uint serviceCount, string[] serviceNames);
