// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Composition.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Composition.Structs;
#endif
/// <summary>Specifies Desktop Window Manager (DWM) thumbnail properties used by the DwmUpdateThumbnailProperties function.</summary>
public struct DwmThumbnailProperties : IEquatable<DwmThumbnailProperties>
{
    /// <summary>A bitwise combination of DWM thumbnail values indicating which members are set.</summary>
    private DwmThumbnailPropertyFlags _flags;

    /// <summary>The configured destination rectangle.</summary>
    private NativeRect _destination;

    /// <summary>The configured source rectangle.</summary>
    private NativeRect _source;

    /// <summary>The configured opacity.</summary>
    private byte _opacity;

    /// <summary>The configured visibility state.</summary>
    private bool _visible;

    /// <summary>The configured source client-area-only state.</summary>
    private bool _sourceClientAreaOnly;

    /// <summary>Gets or sets the destination rectangle and marks the corresponding flag.</summary>
    public NativeRect Destination
    {
        readonly get => _destination;
        set
        {
            _flags |= DwmThumbnailPropertyFlags.Destination;
            _destination = value;
        }
    }

    /// <summary>Gets or sets the source rectangle and marks the corresponding flag.</summary>
    public NativeRect Source
    {
        readonly get => _source;
        set
        {
            _flags |= DwmThumbnailPropertyFlags.Source;
            _source = value;
        }
    }

    /// <summary>Gets or sets the opacity and marks the corresponding flag.</summary>
    public byte Opacity
    {
        readonly get => _opacity;
        set
        {
            _flags |= DwmThumbnailPropertyFlags.Opacity;
            _opacity = value;
        }
    }

    /// <summary>Gets or sets a value indicating whether the thumbnail is visible and marks the corresponding flag.</summary>
    public bool Visible
    {
        readonly get => _visible;
        set
        {
            _flags |= DwmThumbnailPropertyFlags.Visible;
            _visible = value;
        }
    }

    /// <summary>Gets or sets a value indicating whether only the source client area is used and marks the corresponding flag.</summary>
    public bool SourceClientAreaOnly
    {
        readonly get => _sourceClientAreaOnly;
        set
        {
            _flags |= DwmThumbnailPropertyFlags.SourceClientAreaOnly;
            _sourceClientAreaOnly = value;
        }
    }

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when both values are equal.</returns>
    public static bool operator ==(DwmThumbnailProperties left, DwmThumbnailProperties right) => left.Equals(right);

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when the values are not equal.</returns>
    public static bool operator !=(DwmThumbnailProperties left, DwmThumbnailProperties right) => !left.Equals(right);

    /// <inheritdoc />
    public override readonly bool Equals(object obj) => obj is DwmThumbnailProperties other && Equals(other);

    /// <inheritdoc />
    public readonly bool Equals(DwmThumbnailProperties other) =>
        _flags == other._flags
        && _destination == other._destination
        && _source == other._source
        && _opacity == other._opacity
        && _visible == other._visible
        && _sourceClientAreaOnly == other._sourceClientAreaOnly;

    /// <inheritdoc />
    public override readonly int GetHashCode() => typeof(DwmThumbnailProperties).GetHashCode();

    /// <summary>Converts this managed value to its native layout.</summary>
    /// <returns>The native layout value.</returns>
    internal readonly NativeDwmThumbnailProperties ToNative() => new(_flags, Destination, Source, Opacity, Visible ? 1 : 0, SourceClientAreaOnly ? 1 : 0);
}
