// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Structs;

/// <summary>This structure is returned when WFQuerySessionInformation is called with WFInfoClasses.ClientLatency.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ClientLatency : IEquatable<ClientLatency>
{
    /// <summary>The average latency.</summary>
    private readonly uint _avarage;

    /// <summary>The last latency.</summary>
    private readonly uint _last;

    /// <summary>The latency derivation.</summary>
    private readonly uint _derivation;

    /// <summary>Initializes a new instance of the <see cref="ClientLatency"/> struct.</summary>
    /// <param name="avarage">The average latency.</param>
    /// <param name="last">The last latency.</param>
    /// <param name="derivation">The latency derivation.</param>
    internal ClientLatency(uint avarage, uint last, uint derivation)
    {
        _avarage = avarage;
        _last = last;
        _derivation = derivation;
    }

    /// <summary>Gets the client's avarage latency.</summary>
    public uint Avarage => _avarage;

    /// <summary>Gets the client's last latency.</summary>
    public uint Last => _last;

    /// <summary>Gets the client's latency derivation.</summary>
    public uint Derivation => _derivation;

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(ClientLatency left, ClientLatency right) => left.Equals(right);

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the values are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(ClientLatency left, ClientLatency right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is ClientLatency other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(ClientLatency other) =>
        _avarage == other._avarage
        && _last == other._last
        && _derivation == other._derivation;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_avarage, _last, _derivation);

    /// <inheritdoc/>
    public override string ToString() => $"{Avarage}|{Last}|{Derivation}";
}
