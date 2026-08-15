// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Composable Restart Manager native-session operations.</summary>
internal interface IRestartManagerSessionApi
{
    /// <summary>Starts a Restart Manager session.</summary>
    /// <param name="sessionHandle">Restart Manager session handle.</param>
    /// <param name="sessionFlags">Reserved session flags.</param>
    /// <param name="sessionKey">Session key output buffer.</param>
    /// <returns>Win32 result code.</returns>
    int StartSession(out int sessionHandle, int sessionFlags, StringBuilder sessionKey);

    /// <summary>Ends a Restart Manager session.</summary>
    /// <param name="sessionHandle">Restart Manager session handle.</param>
    /// <returns>Win32 result code.</returns>
    int EndSession(int sessionHandle);

    /// <summary>Registers resources with a Restart Manager session.</summary>
    /// <param name="sessionHandle">Restart Manager session handle.</param>
    /// <param name="fileCount">Number of file names.</param>
    /// <param name="filenames">File names.</param>
    /// <param name="applicationCount">Number of process identities.</param>
    /// <param name="applications">Process identities.</param>
    /// <param name="serviceCount">Number of service names.</param>
    /// <param name="serviceNames">Service names.</param>
    /// <returns>Win32 result code.</returns>
    int RegisterResources(
        int sessionHandle,
        uint fileCount,
        string[] filenames,
        uint applicationCount,
        RmUniqueProcess[] applications,
        uint serviceCount,
        string[] serviceNames);

    /// <summary>Gets affected applications for a Restart Manager session.</summary>
    /// <param name="sessionHandle">Restart Manager session handle.</param>
    /// <param name="processInfoNeeded">Required process-info count.</param>
    /// <param name="processInfoCount">Supplied process-info count.</param>
    /// <param name="affectedApplications">Affected applications buffer.</param>
    /// <param name="rebootReasons">Reboot reason flags.</param>
    /// <returns>Win32 result code.</returns>
    int GetList(
        int sessionHandle,
        out uint processInfoNeeded,
        ref uint processInfoCount,
        RmProcessInfo[] affectedApplications,
        out RmRebootReason rebootReasons);

    /// <summary>Shuts down affected applications.</summary>
    /// <param name="sessionHandle">Restart Manager session handle.</param>
    /// <param name="shutdownType">Shutdown type.</param>
    /// <param name="statusCallback">Status callback.</param>
    /// <returns>Win32 result code.</returns>
    int Shutdown(int sessionHandle, RmShutdownType shutdownType, RmStatusCallback statusCallback);

    /// <summary>Restarts affected applications.</summary>
    /// <param name="sessionHandle">Restart Manager session handle.</param>
    /// <param name="restartFlags">Reserved restart flags.</param>
    /// <param name="statusCallback">Status callback.</param>
    /// <returns>Win32 result code.</returns>
    int Restart(int sessionHandle, int restartFlags, RmStatusCallback statusCallback);
}
