// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Shuts down affected applications.</summary>
/// <param name="sessionHandle">The session handle.</param>
/// <param name="shutdownType">The shutdown type.</param>
/// <param name="statusCallback">The status callback.</param>
/// <returns>The native result code.</returns>
internal delegate int ShutdownOperation(int sessionHandle, RmShutdownType shutdownType, RmStatusCallback statusCallback);
