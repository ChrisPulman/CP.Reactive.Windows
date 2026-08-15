// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Structs;

/// <summary>This structure is returned when WFQuerySessionInformation is called with WFInfoClasses.ClientInfo.</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public readonly struct ClientInfo : IEquatable<ClientInfo>
{
    /// <summary>The client name.</summary>
    [MarshalAs(UnmanagedType.LPWStr)]
    private readonly string _name;

    /// <summary>The client directory.</summary>
    [MarshalAs(UnmanagedType.LPWStr)]
    private readonly string _directory;

    /// <summary>The client build number.</summary>
    private readonly int _buildNumber;

    /// <summary>The client product identifier.</summary>
    private readonly int _productId;

    /// <summary>The client hardware identifier.</summary>
    private readonly int _hardwareId;

    /// <summary>The client address.</summary>
    private readonly ClientAddress _address;

    /// <summary>Initializes a new instance of the <see cref="ClientInfo"/> struct.</summary>
    /// <param name="name">The client name.</param>
    /// <param name="directory">The client directory.</param>
    /// <param name="buildNumber">The client build number.</param>
    /// <param name="productId">The client product identifier.</param>
    /// <param name="hardwareId">The client hardware identifier.</param>
    /// <param name="address">The client address.</param>
    internal ClientInfo(string name, string directory, int buildNumber, int productId, int hardwareId, ClientAddress address)
    {
        _name = name;
        _directory = directory;
        _buildNumber = buildNumber;
        _productId = productId;
        _hardwareId = hardwareId;
        _address = address;
    }

    /// <summary>Gets the client's name.</summary>
    public string Name => _name;

    /// <summary>Gets the client's directory.</summary>
    public string Directory => _directory;

    /// <summary>Gets the client's build number.</summary>
    public int BuildNumber => _buildNumber;

    /// <summary>Gets the client's product ID.</summary>
    public int ProductId => _productId;

    /// <summary>Gets the client's hardware ID.</summary>
    public int HardwareId => _hardwareId;

    /// <summary>Gets the client's address.</summary>
    public ClientAddress Address => _address;

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(ClientInfo left, ClientInfo right) => left.Equals(right);

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the values are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(ClientInfo left, ClientInfo right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is ClientInfo other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(ClientInfo other) =>
        string.Equals(_name, other._name, StringComparison.Ordinal)
        && string.Equals(_directory, other._directory, StringComparison.Ordinal)
        && _buildNumber == other._buildNumber
        && _productId == other._productId
        && _hardwareId == other._hardwareId
        && _address.Equals(other._address);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = default(HashCode);
        hashCode.Add(_name, StringComparer.Ordinal);
        hashCode.Add(_directory, StringComparer.Ordinal);
        hashCode.Add(_buildNumber);
        hashCode.Add(_productId);
        hashCode.Add(_hardwareId);
        hashCode.Add(_address);

        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Name}|{Directory}|{BuildNumber}|{ProductId}|{HardwareId}|{Address}";
}
