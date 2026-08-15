// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Structs;

/// <summary>This structure is returned when WFQuerySessionInformation is called with WFInfoClasses.Version.</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public readonly struct OsVersionInfo : IEquatable<OsVersionInfo>
{
    /// <summary>The size of this data structure, in bytes. Set this member to sizeof(OSVERSIONINFO).</summary>
    private readonly int _versionInfoSize;

    /// <summary>The major version.</summary>
    private readonly int _majorVersion;

    /// <summary>The minor version.</summary>
    private readonly int _minorVersion;

    /// <summary>The build number.</summary>
    private readonly int _buildNumber;

    /// <summary>The platform identifier.</summary>
    private readonly int _platformId;

    /// <summary>The service pack version buffer.</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    private readonly string _servicePackVersion;

    /// <summary>Initializes a new instance of the <see cref="OsVersionInfo"/> struct.</summary>
    /// <param name="versionInfoSize">The structure size.</param>
    /// <param name="majorVersion">The major version.</param>
    /// <param name="minorVersion">The minor version.</param>
    /// <param name="buildNumber">The build number.</param>
    /// <param name="platformId">The platform identifier.</param>
    /// <param name="servicePackVersion">The service pack version.</param>
    internal OsVersionInfo(
        int versionInfoSize,
        int majorVersion,
        int minorVersion,
        int buildNumber,
        int platformId,
        string servicePackVersion)
    {
        _versionInfoSize = versionInfoSize;
        _majorVersion = majorVersion;
        _minorVersion = minorVersion;
        _buildNumber = buildNumber;
        _platformId = platformId;
        _servicePackVersion = servicePackVersion;
    }

    /// <summary>Initializes a new instance of the <see cref="OsVersionInfo"/> struct.</summary>
    /// <param name="versionInfoSize">The structure size.</param>
    private OsVersionInfo(int versionInfoSize)
    {
        _versionInfoSize = versionInfoSize;
        _majorVersion = 0;
        _minorVersion = 0;
        _buildNumber = 0;
        _platformId = 0;
        _servicePackVersion = string.Empty;
    }

    /// <summary>Gets the major version number of the operating system.</summary>
    public int MajorVersion => _majorVersion;

    /// <summary>Gets the minor version number of the operating system.</summary>
    public int MinorVersion => _minorVersion;

    /// <summary>Gets the build number of the operating system.</summary>
    public int BuildNumber => _buildNumber;

    /// <summary>Gets the operating system platform. This member can be VER_PLATFORM_WIN32_NT (2).</summary>
    public int PlatformId => _platformId;

    /// <summary>
    /// Gets a null-terminated string, such as "Service Pack 3", that indicates the latest Service Pack installed on the system.
    /// If no Service Pack has been installed, the string is empty.
    /// </summary>
    public string ServicePackVersion => _servicePackVersion ?? string.Empty;

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true"/> when the values are equal.</returns>
    public static bool operator ==(OsVersionInfo left, OsVersionInfo right) => left.Equals(right);

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true"/> when the values are not equal.</returns>
    public static bool operator !=(OsVersionInfo left, OsVersionInfo right) => !left.Equals(right);

    /// <summary>Factory for an empty OsVersionInfo.</summary>
    /// <returns>The result.</returns>
    public static OsVersionInfo Create() => new(Marshal.SizeOf<OsVersionInfo>());

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is OsVersionInfo other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(OsVersionInfo other) =>
        _versionInfoSize == other._versionInfoSize
        && _majorVersion == other._majorVersion
        && _minorVersion == other._minorVersion
        && _buildNumber == other._buildNumber
        && _platformId == other._platformId
        && string.Equals(ServicePackVersion, other.ServicePackVersion, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = default(HashCode);
        hashCode.Add(_versionInfoSize);
        hashCode.Add(_majorVersion);
        hashCode.Add(_minorVersion);
        hashCode.Add(_buildNumber);
        hashCode.Add(_platformId);
        hashCode.Add(ServicePackVersion, StringComparer.Ordinal);

        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public override string ToString() => $"{MajorVersion}.{MinorVersion}.{BuildNumber}|{PlatformId}|{ServicePackVersion}";
}
