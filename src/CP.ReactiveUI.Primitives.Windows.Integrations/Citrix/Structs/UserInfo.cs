// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Structs;

/// <summary>This structure is returned when WFQuerySessionInformation is called with WFInfoClasses.UserInfo.</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public readonly struct UserInfo : IEquatable<UserInfo>
{
    /// <summary>The user name.</summary>
    [MarshalAs(UnmanagedType.LPWStr)]
    private readonly string _userName;

    /// <summary>The domain name.</summary>
    [MarshalAs(UnmanagedType.LPWStr)]
    private readonly string _domainName;

    /// <summary>The connection name.</summary>
    [MarshalAs(UnmanagedType.LPWStr)]
    private readonly string _connectionName;

    /// <summary>Initializes a new instance of the <see cref="UserInfo"/> struct.</summary>
    /// <param name="userName">The user name.</param>
    /// <param name="domainName">The domain name.</param>
    /// <param name="connectionName">The connection name.</param>
    internal UserInfo(string userName, string domainName, string connectionName)
    {
        _userName = userName;
        _domainName = domainName;
        _connectionName = connectionName;
    }

    /// <summary>Gets the user name.</summary>
    public string Username => _userName;

    /// <summary>Gets the domain name.</summary>
    public string Domainname => _domainName;

    /// <summary>Gets the connection name.</summary>
    public string ConnectionName => _connectionName;

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(UserInfo left, UserInfo right) => left.Equals(right);

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the values are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(UserInfo left, UserInfo right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is UserInfo other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(UserInfo other) =>
        string.Equals(_userName, other._userName, StringComparison.Ordinal)
        && string.Equals(_domainName, other._domainName, StringComparison.Ordinal)
        && string.Equals(_connectionName, other._connectionName, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
        StringComparer.Ordinal.GetHashCode(_userName ?? string.Empty),
        StringComparer.Ordinal.GetHashCode(_domainName ?? string.Empty),
        StringComparer.Ordinal.GetHashCode(_connectionName ?? string.Empty));

    /// <inheritdoc/>
    public override string ToString() => $"{Username}|{Domainname}|{ConnectionName}";
}
