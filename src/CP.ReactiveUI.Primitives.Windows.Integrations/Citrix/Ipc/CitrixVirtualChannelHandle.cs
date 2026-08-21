// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Ipc;

/// <summary>Represents a virtual channel opened by the injected Citrix virtual-driver adapter.</summary>
public readonly struct CitrixVirtualChannelHandle : IEquatable<CitrixVirtualChannelHandle>
{
    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelHandle"/> struct.</summary>
    /// <param name="value">The host-provided channel identifier.</param>
    public CitrixVirtualChannelHandle(long value) => Value = value;

    /// <summary>Gets an empty channel handle.</summary>
    public static CitrixVirtualChannelHandle Empty { get; } = new(0);

    /// <summary>Gets the host-provided channel identifier.</summary>
    public long Value { get; }

    /// <summary>Gets a value indicating whether this handle carries a non-zero value.</summary>
    public bool HasValue => Value != 0;

    /// <summary>Compares two handles for equality.</summary>
    /// <param name="left">The left handle.</param>
    /// <param name="right">The right handle.</param>
    /// <returns><see langword="true"/> when the handles are equal.</returns>
    public static bool operator ==(CitrixVirtualChannelHandle left, CitrixVirtualChannelHandle right) => left.Equals(right);

    /// <summary>Compares two handles for inequality.</summary>
    /// <param name="left">The left handle.</param>
    /// <param name="right">The right handle.</param>
    /// <returns><see langword="true"/> when the handles are not equal.</returns>
    public static bool operator !=(CitrixVirtualChannelHandle left, CitrixVirtualChannelHandle right) => !left.Equals(right);

    /// <inheritdoc />
    public bool Equals(CitrixVirtualChannelHandle other) => Value == other.Value;

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is CitrixVirtualChannelHandle other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
