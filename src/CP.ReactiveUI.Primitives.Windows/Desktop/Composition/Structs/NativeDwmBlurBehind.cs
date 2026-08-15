// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Composition;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Composition;
#endif
/// <summary>Native DWM_BLURBEHIND layout with BOOL values represented as four-byte integers.</summary>
internal readonly struct NativeDwmBlurBehind : IEquatable<NativeDwmBlurBehind>
{
    /// <summary>A bitwise combination of active members.</summary>
    private readonly DwmBlurBehindFlags _flags;

    /// <summary>The native blur-enabled BOOL value.</summary>
    private readonly int _enabled;

    /// <summary>The native blur region handle.</summary>
    private readonly IntPtr _blurRegion;

    /// <summary>The native transition-on-maximized BOOL value.</summary>
    private readonly int _transitionOnMaximized;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Composition.NativeDwmBlurBehind" /> struct.</summary>
    /// <param name="flags">The active members.</param>
    /// <param name="enabled">The blur state.</param>
    /// <param name="blurRegion">The blur region.</param>
    /// <param name="transitionOnMaximized">The maximized transition state.</param>
    internal NativeDwmBlurBehind(DwmBlurBehindFlags flags, int enabled, IntPtr blurRegion, int transitionOnMaximized)
    {
        _flags = flags;
        _enabled = enabled;
        _blurRegion = blurRegion;
        _transitionOnMaximized = transitionOnMaximized;
    }

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when both values are equal.</returns>
    public static bool operator ==(NativeDwmBlurBehind left, NativeDwmBlurBehind right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when the values are not equal.</returns>
    public static bool operator !=(NativeDwmBlurBehind left, NativeDwmBlurBehind right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is NativeDwmBlurBehind other)
        {
            return Equals(other);
        }

        return false;
    }

    /// <inheritdoc />
    public bool Equals(NativeDwmBlurBehind other)
    {
        if (_flags == other._flags && _enabled == other._enabled && _blurRegion == other._blurRegion)
        {
            return _transitionOnMaximized == other._transitionOnMaximized;
        }

        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_flags, _enabled, _blurRegion, _transitionOnMaximized);

    /// <summary>Reads fields so source analyzers do not treat ABI-only values as dead writes.</summary>
    internal void MarkFieldsAsRead()
    {
    }
}
