// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

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

    [CompilerGenerated]
    private NativeRect _003CDestination_003Ek__BackingField;

    [CompilerGenerated]
    private NativeRect _003CSource_003Ek__BackingField;

    [CompilerGenerated]
    private byte _003COpacity_003Ek__BackingField;

    [CompilerGenerated]
    private bool _003CVisible_003Ek__BackingField;

    [CompilerGenerated]
    private bool _003CSourceClientAreaOnly_003Ek__BackingField;

    /// <summary>Gets or sets the destination rectangle and marks the corresponding flag.</summary>
    public NativeRect Destination
    {
        [CompilerGenerated]
        readonly get => _003CDestination_003Ek__BackingField;
        set
        {
            _flags |= DwmThumbnailPropertyFlags.Destination;
            _003CDestination_003Ek__BackingField = value;
        }
    }

    /// <summary>Gets or sets the source rectangle and marks the corresponding flag.</summary>
    public NativeRect Source
    {
        [CompilerGenerated]
        readonly get => _003CSource_003Ek__BackingField;
        set
        {
            _flags |= DwmThumbnailPropertyFlags.Source;
            _003CSource_003Ek__BackingField = value;
        }
    }

    /// <summary>Gets or sets the opacity and marks the corresponding flag.</summary>
    public byte Opacity
    {
        [CompilerGenerated]
        readonly get => _003COpacity_003Ek__BackingField;
        set
        {
            _flags |= DwmThumbnailPropertyFlags.Opacity;
            _003COpacity_003Ek__BackingField = value;
        }
    }

    /// <summary>Gets or sets a value indicating whether the thumbnail is visible and marks the corresponding flag.</summary>
    public bool Visible
    {
        [CompilerGenerated]
        readonly get => _003CVisible_003Ek__BackingField;
        set
        {
            _flags |= DwmThumbnailPropertyFlags.Visible;
            _003CVisible_003Ek__BackingField = value;
        }
    }

    /// <summary>Gets or sets a value indicating whether only the source client area is used and marks the corresponding flag.</summary>
    public bool SourceClientAreaOnly
    {
        [CompilerGenerated]
        readonly get => _003CSourceClientAreaOnly_003Ek__BackingField;
        set
        {
            _flags |= DwmThumbnailPropertyFlags.SourceClientAreaOnly;
            _003CSourceClientAreaOnly_003Ek__BackingField = value;
        }
    }

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when both values are equal.</returns>
    public static bool operator ==(DwmThumbnailProperties left, DwmThumbnailProperties right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when the values are not equal.</returns>
    public static bool operator !=(DwmThumbnailProperties left, DwmThumbnailProperties right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override readonly bool Equals(object obj)
    {
        if (obj is DwmThumbnailProperties other)
        {
            return Equals(other);
        }

        return false;
    }

    /// <inheritdoc />
    public readonly bool Equals(DwmThumbnailProperties other)
    {
        if (_flags == other._flags && Destination == other.Destination && Source == other.Source && Opacity == other.Opacity && Visible == other.Visible)
        {
            return SourceClientAreaOnly == other.SourceClientAreaOnly;
        }

        return false;
    }

    /// <inheritdoc />
    public override readonly int GetHashCode() => typeof(DwmThumbnailProperties).GetHashCode();

    /// <summary>Converts this managed value to its native layout.</summary>
    /// <returns>The native layout value.</returns>
    internal readonly NativeDwmThumbnailProperties ToNative() => new(_flags, Destination, Source, Opacity, Visible ? 1 : 0, SourceClientAreaOnly ? 1 : 0);
}
