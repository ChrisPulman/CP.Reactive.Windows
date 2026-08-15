// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;

/// <summary>See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms724833(v=vs.85).aspx">OSVERSIONINFOEX structure</a>.</summary>
public readonly struct OsVersionInfoEx : IEquatable<OsVersionInfoEx>
{
    /// <summary>Initializes a new instance of the <see cref="OsVersionInfoEx" /> struct.</summary>
    public OsVersionInfoEx()
    {
        ServicePackVersion = string.Empty;
    }

    /// <summary>Gets the major version number of the operating system.</summary>
    public int MajorVersion { get; internal init; }

    /// <summary>Gets the minor version number of the operating system.</summary>
    public int MinorVersion { get; internal init; }

    /// <summary>Gets the build number of the operating system.</summary>
    public int BuildNumber { get; internal init; }

    /// <summary>Gets the operating system platform. This member can be VER_PLATFORM_WIN32_NT (2).</summary>
    public int PlatformId { get; internal init; }

    /// <summary>Gets a null-terminated string that indicates the latest Service Pack installed on the system.</summary>
    public string ServicePackVersion { get; internal init; }

    /// <summary>Gets the major version number of the latest Service Pack installed on the system.</summary>
    public short ServicePackMajor { get; internal init; }

    /// <summary>Gets the minor version number of the latest Service Pack installed on the system. For example, for Service Pack 3, the minor version number is 0.</summary>
    public short ServicePackMinor { get; internal init; }

    /// <summary>Gets a bit mask that identifies the product suites available on the system. This member can be a combination of the following values.</summary>
    public WindowsSuites SuiteMask { get; internal init; }

    /// <summary>Gets any additional information about the system.</summary>
    public WindowsProductTypes ProductType { get; internal init; }

    /// <summary>Factory for an empty OsVersionInfoEx.</summary>
    /// <returns>An initialized OS version structure.</returns>
    public static OsVersionInfoEx Create() => new();

    /// <summary>Checks whether two values are equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns>True if the values are equal; otherwise, false.</returns>
    public static bool operator ==(OsVersionInfoEx left, OsVersionInfoEx right)
    {
        return left.Equals(right);
    }

    /// <summary>Checks whether two values are different.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns>True if the values are different; otherwise, false.</returns>
    public static bool operator !=(OsVersionInfoEx left, OsVersionInfoEx right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public bool Equals(OsVersionInfoEx other) =>
        MajorVersion == other.MajorVersion
        && MinorVersion == other.MinorVersion
        && BuildNumber == other.BuildNumber
        && PlatformId == other.PlatformId
        && string.Equals(ServicePackVersion, other.ServicePackVersion, StringComparison.Ordinal)
        && ServicePackMajor == other.ServicePackMajor
        && ServicePackMinor == other.ServicePackMinor
        && SuiteMask == other.SuiteMask
        && ProductType == other.ProductType;

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is OsVersionInfoEx other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = default;
        hash.Add(MajorVersion);
        hash.Add(MinorVersion);
        hash.Add(BuildNumber);
        hash.Add(PlatformId);
        hash.Add(ServicePackVersion, StringComparer.Ordinal);
        hash.Add(ServicePackMajor);
        hash.Add(ServicePackMinor);
        hash.Add(SuiteMask);
        hash.Add(ProductType);
        return hash.ToHashCode();
    }
}
