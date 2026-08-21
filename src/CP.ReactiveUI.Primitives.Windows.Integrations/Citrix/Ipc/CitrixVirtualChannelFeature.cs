// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Ipc;

/// <summary>Describes a VdRegisterFeature request.</summary>
public sealed class CitrixVirtualChannelFeature
{
    /// <summary>The immutable metadata snapshot.</summary>
    private readonly byte[] _metadata;

    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelFeature"/> class.</summary>
    /// <param name="name">The feature name.</param>
    /// <param name="version">The feature version.</param>
    public CitrixVirtualChannelFeature(string name, Version version)
        : this(name, version, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelFeature"/> class.</summary>
    /// <param name="name">The feature name.</param>
    /// <param name="version">The feature version.</param>
    /// <param name="metadata">Optional feature metadata.</param>
    public CitrixVirtualChannelFeature(string name, Version version, byte[] metadata)
    {
        Throw.IfNull(name);
        Throw.IfNull(version);
        Name = name;
        Version = version;
        _metadata = metadata is null ? Array.Empty<byte>() : Copy(metadata);
    }

    /// <summary>Gets the feature name.</summary>
    public string Name { get; }

    /// <summary>Gets the feature version.</summary>
    public Version Version { get; }

    /// <summary>Gets the metadata byte count.</summary>
    public int MetadataLength => _metadata.Length;

    /// <summary>Copies the immutable metadata snapshot.</summary>
    /// <returns>A copy of the metadata.</returns>
    public byte[] CopyMetadata() => Copy(_metadata);

    /// <summary>Copies a byte array.</summary>
    /// <param name="source">The source bytes.</param>
    /// <returns>The copied bytes.</returns>
    private static byte[] Copy(byte[] source)
    {
        var copy = new byte[source.Length];
        Array.Copy(source, copy, source.Length);
        return copy;
    }
}
