// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Gets affected applications for a Restart Manager session.</summary>
/// <param name="sessionHandle">The session handle.</param>
/// <param name="processInfoNeeded">Receives the required process information count.</param>
/// <param name="processInfoCount">The supplied process information count.</param>
/// <param name="affectedApplications">Receives affected applications.</param>
/// <param name="rebootReasons">Receives reboot reasons.</param>
/// <returns>The native result code.</returns>
internal delegate int GetListOperation(
    int sessionHandle,
    out uint processInfoNeeded,
    ref uint processInfoCount,
    RmProcessInfo[] affectedApplications,
    out RmRebootReason rebootReasons);
