// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Composition.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Composition.Structs;
#endif
/// <summary>Native DWM_THUMBNAIL_PROPERTIES layout with BOOL values represented as four-byte integers.</summary>
internal readonly struct NativeDwmThumbnailProperties : IEquatable<NativeDwmThumbnailProperties>
{
    /// <summary>A bitwise combination of active members.</summary>
    private readonly DwmThumbnailPropertyFlags _flags;

    /// <summary>The destination rectangle.</summary>
    private readonly NativeRect _destination;

    /// <summary>The source rectangle.</summary>
    private readonly NativeRect _source;

    /// <summary>The opacity value.</summary>
    private readonly byte _opacity;

    /// <summary>The native visible BOOL value.</summary>
    private readonly int _visible;

    /// <summary>The native source-client-area-only BOOL value.</summary>
    private readonly int _sourceClientAreaOnly;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Composition.Structs.NativeDwmThumbnailProperties" /> struct.</summary>
    /// <param name="flags">The active members.</param>
    /// <param name="destination">The destination rectangle.</param>
    /// <param name="source">The source rectangle.</param>
    /// <param name="opacity">The opacity value.</param>
    /// <param name="visible">The native visible value.</param>
    /// <param name="sourceClientAreaOnly">The native source-client-area-only value.</param>
    internal NativeDwmThumbnailProperties(DwmThumbnailPropertyFlags flags, NativeRect destination, NativeRect source, byte opacity, int visible, int sourceClientAreaOnly)
    {
        _flags = flags;
        _destination = destination;
        _source = source;
        _opacity = opacity;
        _visible = visible;
        _sourceClientAreaOnly = sourceClientAreaOnly;
    }

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when both values are equal.</returns>
    public static bool operator ==(NativeDwmThumbnailProperties left, NativeDwmThumbnailProperties right) => left.Equals(right);

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when the values are not equal.</returns>
    public static bool operator !=(NativeDwmThumbnailProperties left, NativeDwmThumbnailProperties right) => !left.Equals(right);

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is NativeDwmThumbnailProperties other && Equals(other);

    /// <inheritdoc />
    public bool Equals(NativeDwmThumbnailProperties other) =>
        _flags == other._flags
        && _destination == other._destination
        && _source == other._source
        && _opacity == other._opacity
        && _visible == other._visible
        && _sourceClientAreaOnly == other._sourceClientAreaOnly;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_flags, _destination, _source, _opacity, _visible, _sourceClientAreaOnly);

    /// <summary>Reads ABI-only fields so analyzers recognize the native layout values as intentionally consumed.</summary>
    internal void MarkFieldsAsRead()
    {
        _ = _flags;
        _ = _destination;
        _ = _source;
        _ = _opacity;
        _ = _visible;
        _ = _sourceClientAreaOnly;
    }
}
