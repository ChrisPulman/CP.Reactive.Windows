// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;

/// <summary>
///     Uniquely identifies a process by its PID and the time the process began.
///     An array of RmUniqueProcess structures can be passed to the RmRegisterResources function.
///     See <a href="https://docs.microsoft.com/en-us/windows/win32/api/restartmanager/ns-restartmanager-rm_unique_process">RM_UNIQUE_PROCESS structure</a>
/// </summary>
/// <param name="processId">The process identifier.</param>
/// <param name="processStartTime">The process start time.</param>
public readonly struct RmUniqueProcess(
    int processId,
    System.Runtime.InteropServices.ComTypes.FILETIME processStartTime) : IEquatable<RmUniqueProcess>
{
    /// <summary>Gets the process identifier.</summary>
    public int ProcessId { get; } = processId;

    /// <summary>Gets the process start time.</summary>
    public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime { get; } =
        processStartTime;

    /// <summary>Checks whether two values are equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns>True if the values are equal; otherwise, false.</returns>
    public static bool operator ==(RmUniqueProcess left, RmUniqueProcess right)
    {
        return left.Equals(right);
    }

    /// <summary>Checks whether two values are different.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns>True if the values are different; otherwise, false.</returns>
    public static bool operator !=(RmUniqueProcess left, RmUniqueProcess right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public bool Equals(RmUniqueProcess other) =>
        ProcessId == other.ProcessId
        && ProcessStartTime.dwLowDateTime == other.ProcessStartTime.dwLowDateTime
        && ProcessStartTime.dwHighDateTime == other.ProcessStartTime.dwHighDateTime;

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is RmUniqueProcess other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(
            ProcessId,
            ProcessStartTime.dwLowDateTime,
            ProcessStartTime.dwHighDateTime);
}
