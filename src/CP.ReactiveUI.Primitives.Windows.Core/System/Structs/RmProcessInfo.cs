// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;

/// <summary>
///     Describes an application that is to be registered with the Restart Manager.
///     See <a href="https://docs.microsoft.com/en-us/windows/win32/api/restartmanager/ns-restartmanager-rm_process_info">RM_PROCESS_INFO structure</a>
/// </summary>
/// <param name="process">The process identity.</param>
/// <param name="applicationName">The application name.</param>
/// <param name="serviceShortName">The service short name.</param>
/// <param name="applicationType">The application type.</param>
/// <param name="applicationStatus">The application status.</param>
/// <param name="terminalServicesSessionId">The Terminal Services session identifier.</param>
/// <param name="restartable">A value indicating whether the application can be restarted.</param>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public readonly struct RmProcessInfo(RmUniqueProcess process, string applicationName, string serviceShortName, RmAppType applicationType, RmAppStatus applicationStatus, uint terminalServicesSessionId, bool restartable) : IEquatable<RmProcessInfo>
{
    /// <summary>The maximum Restart Manager application name length.</summary>
    private const int RmMaxAppName = 255;

    /// <summary>The maximum Restart Manager service name length.</summary>
    private const int RmMaxSvcName = 63;

    /// <summary>The Restart Manager application name buffer length.</summary>
    private const int CchRmMaxAppName = 256;

    /// <summary>The Restart Manager service name buffer length.</summary>
    private const int CchRmMaxSvcName = 64;

    /// <summary>Gets the process identity.</summary>
    public RmUniqueProcess Process { get; } = process;

    /// <summary>Gets the application name.</summary>
    public string ApplicationName { get; } = applicationName;

    /// <summary>Gets the service short name.</summary>
    public string ServiceShortName { get; } = serviceShortName;

    /// <summary>Gets the application type.</summary>
    public RmAppType ApplicationType { get; } = applicationType;

    /// <summary>Gets the application status.</summary>
    public RmAppStatus ApplicationStatus { get; } = applicationStatus;

    /// <summary>Gets the Terminal Services session identifier.</summary>
    public uint TerminalServicesSessionId { get; } = terminalServicesSessionId;

    /// <summary>Gets a value indicating whether the application can be restarted.</summary>
    public bool Restartable { get; } = restartable;

    /// <summary>Checks whether two values are equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns>True if the values are equal; otherwise, false.</returns>
    public static bool operator ==(RmProcessInfo left, RmProcessInfo right)
    {
        return left.Equals(right);
    }

    /// <summary>Checks whether two values are different.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns>True if the values are different; otherwise, false.</returns>
    public static bool operator !=(RmProcessInfo left, RmProcessInfo right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public bool Equals(RmProcessInfo other) => Process == other.Process && string.Equals(ApplicationName, other.ApplicationName, StringComparison.Ordinal) && string.Equals(ServiceShortName, other.ServiceShortName, StringComparison.Ordinal) && ApplicationType == other.ApplicationType && ApplicationStatus == other.ApplicationStatus && TerminalServicesSessionId == other.TerminalServicesSessionId && Restartable == other.Restartable;

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is RmProcessInfo other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Process, ApplicationName, ServiceShortName, ApplicationType, ApplicationStatus, TerminalServicesSessionId, Restartable);
}
