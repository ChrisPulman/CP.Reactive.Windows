// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Composition;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Composition;
#endif
/// <summary>
/// See
/// <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa969500(v=vs.85).aspx">DWM_BLURBEHIND structure</a>
/// Specifies Desktop Window Manager (DWM) blur-behind properties.
/// Used by the DwmEnableBlurBehindWindow function.
/// </summary>
public struct DwmBlurBehind : IEquatable<DwmBlurBehind>
{
    /// <summary>A bitwise combination of DWM blur-behind values indicating which members have been set.</summary>
    private DwmBlurBehindFlags _flags;

    /// <summary>The native blur region handle.</summary>
    private IntPtr _blurRegion;

    [CompilerGenerated]
    private bool _003CEnable_003Ek__BackingField;

    [CompilerGenerated]
    private bool _003CTransitionOnMaximized_003Ek__BackingField;

    /// <summary>Gets or sets a value indicating whether the window handle is registered for DWM blur behind.</summary>
    public bool Enable
    {
        [CompilerGenerated]
        readonly get => _003CEnable_003Ek__BackingField;
        set
        {
            _003CEnable_003Ek__BackingField = value;
            _flags |= DwmBlurBehindFlags.Enable;
        }
    }

    /// <summary>Gets or sets a value indicating whether the window colorization transitions when maximized.</summary>
    public bool TransitionOnMaximized
    {
        [CompilerGenerated]
        readonly get => _003CTransitionOnMaximized_003Ek__BackingField;
        set
        {
            _003CTransitionOnMaximized_003Ek__BackingField = value;
            _flags |= DwmBlurBehindFlags.TransitionMaximized;
        }
    }

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when both values are equal.</returns>
    public static bool operator ==(DwmBlurBehind left, DwmBlurBehind right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when the values are not equal.</returns>
    public static bool operator !=(DwmBlurBehind left, DwmBlurBehind right)
    {
        return !left.Equals(right);
    }

    /// <summary>Sets the client-area region where blur behind is applied.</summary>
    /// <param name="blurRegion">The native blur region handle.</param>
    public void SetBlurRegion(IntPtr blurRegion)
    {
        _blurRegion = blurRegion;
        _flags |= DwmBlurBehindFlags.BlurRegion;
    }

    /// <inheritdoc />
    public override readonly bool Equals(object obj)
    {
        if (obj is DwmBlurBehind other)
        {
            return Equals(other);
        }

        return false;
    }

    /// <inheritdoc />
    public readonly bool Equals(DwmBlurBehind other)
    {
        if (_flags == other._flags && Enable == other.Enable && _blurRegion == other._blurRegion)
        {
            return TransitionOnMaximized == other.TransitionOnMaximized;
        }

        return false;
    }

    /// <inheritdoc />
    public override readonly int GetHashCode() => typeof(DwmBlurBehind).GetHashCode();

    /// <summary>Converts this managed value to its native layout.</summary>
    /// <returns>The native layout value.</returns>
    internal readonly NativeDwmBlurBehind ToNative() => new(_flags, Enable ? 1 : 0, _blurRegion, TransitionOnMaximized ? 1 : 0);
}
