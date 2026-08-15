// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Restarts affected applications.</summary>
/// <param name="sessionHandle">The session handle.</param>
/// <param name="restartFlags">The restart flags.</param>
/// <param name="statusCallback">The status callback.</param>
/// <returns>The native result code.</returns>
internal delegate int RestartOperation(int sessionHandle, int restartFlags, RmStatusCallback statusCallback);
