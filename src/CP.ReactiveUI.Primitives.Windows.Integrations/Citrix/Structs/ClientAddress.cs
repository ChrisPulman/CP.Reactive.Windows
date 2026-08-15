// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Structs;

/// <summary>This structure is returned when WFQuerySessionInformation is called with WFInfoClasses.ClientAddress.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ClientAddress : IEquatable<ClientAddress>
{
    /// <summary>The address family.</summary>
    private readonly int _adressFamily;

    /// <summary>The first raw address byte.</summary>
    private readonly byte _address0;

    /// <summary>The second raw address byte.</summary>
    private readonly byte _address1;

    /// <summary>The third raw address byte.</summary>
    private readonly byte _address2;

    /// <summary>The fourth raw address byte.</summary>
    private readonly byte _address3;

    /// <summary>The fifth raw address byte.</summary>
    private readonly byte _address4;

    /// <summary>The sixth raw address byte.</summary>
    private readonly byte _address5;

    /// <summary>The seventh raw address byte.</summary>
    private readonly byte _address6;

    /// <summary>The eighth raw address byte.</summary>
    private readonly byte _address7;

    /// <summary>The ninth raw address byte.</summary>
    private readonly byte _address8;

    /// <summary>The tenth raw address byte.</summary>
    private readonly byte _address9;

    /// <summary>The eleventh raw address byte.</summary>
    private readonly byte _address10;

    /// <summary>The twelfth raw address byte.</summary>
    private readonly byte _address11;

    /// <summary>The thirteenth raw address byte.</summary>
    private readonly byte _address12;

    /// <summary>The fourteenth raw address byte.</summary>
    private readonly byte _address13;

    /// <summary>The fifteenth raw address byte.</summary>
    private readonly byte _address14;

    /// <summary>The sixteenth raw address byte.</summary>
    private readonly byte _address15;

    /// <summary>The seventeenth raw address byte.</summary>
    private readonly byte _address16;

    /// <summary>The eighteenth raw address byte.</summary>
    private readonly byte _address17;

    /// <summary>The nineteenth raw address byte.</summary>
    private readonly byte _address18;

    /// <summary>The twentieth raw address byte.</summary>
    private readonly byte _address19;

    /// <summary>Initializes a new instance of the <see cref="ClientAddress"/> struct.</summary>
    /// <param name="addressFamily">The address family.</param>
    /// <param name="address">The 20-byte client address buffer.</param>
    internal ClientAddress(AddressFamily addressFamily, ReadOnlySpan<byte> address)
    {
        const int requiredAddressLength = 20;
        if (address.Length != requiredAddressLength)
        {
            throw new ArgumentException("The client address buffer must contain exactly 20 bytes.", nameof(address));
        }

        _adressFamily = (int)addressFamily;
        _address0 = address[0];
        _address1 = address[1];
        _address2 = address[2];
        _address3 = address[3];
        _address4 = address[4];
        _address5 = address[5];
        _address6 = address[6];
        _address7 = address[7];
        _address8 = address[8];
        _address9 = address[9];
        _address10 = address[10];
        _address11 = address[11];
        _address12 = address[12];
        _address13 = address[13];
        _address14 = address[14];
        _address15 = address[15];
        _address16 = address[16];
        _address17 = address[17];
        _address18 = address[18];
        _address19 = address[19];
    }

    /// <summary>Gets the address family.</summary>
    public AddressFamily AddressFamily => (AddressFamily)_adressFamily;

    /// <summary>Gets the IP address used.</summary>
    public string IpAddress => $"{_address2}.{_address3}.{_address4}.{_address5}";

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true"/> when the values are equal.</returns>
    public static bool operator ==(ClientAddress left, ClientAddress right) => left.Equals(right);

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true"/> when the values are not equal.</returns>
    public static bool operator !=(ClientAddress left, ClientAddress right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is ClientAddress other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(ClientAddress other) =>
        _adressFamily == other._adressFamily && ToAddressBytes().SequenceEqual(other.ToAddressBytes());

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = default(HashCode);
        hashCode.Add(_adressFamily);
        foreach (var addressByte in ToAddressBytes())
        {
            hashCode.Add(addressByte);
        }

        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    public override string ToString() => $"{AddressFamily}|{IpAddress}";

    /// <summary>Copies the address fields into an array for equality and hash-code operations.</summary>
    /// <returns>The address bytes.</returns>
    private byte[] ToAddressBytes() =>
    [
        _address0,
        _address1,
        _address2,
        _address3,
        _address4,
        _address5,
        _address6,
        _address7,
        _address8,
        _address9,
        _address10,
        _address11,
        _address12,
        _address13,
        _address14,
        _address15,
        _address16,
        _address17,
        _address18,
        _address19,
    ];
}
