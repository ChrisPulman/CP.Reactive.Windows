// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs.Interop;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop;
#endif
/// <summary>Defines the filter specifications used in common file dialogs.</summary>
/// <param name="name">The display name.</param>
/// <param name="spec">The filter specification.</param>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal readonly struct FilterSpec(string name, string spec) : IEquatable<FilterSpec>
{
    /// <summary>The display name for the filter.</summary>
    [MarshalAs(UnmanagedType.LPWStr)]
    private readonly string _name = name;

    /// <summary>The semicolon-delimited filter pattern.</summary>
    [MarshalAs(UnmanagedType.LPWStr)]
    private readonly string _spec = spec;

    /// <summary>Gets the display name.</summary>
    public string Name => _name;

    /// <summary>Gets the semicolon-delimited filter pattern.</summary>
    public string Spec => _spec;

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when the values are equal.</returns>
    public static bool operator ==(FilterSpec left, FilterSpec right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when the values are not equal.</returns>
    public static bool operator !=(FilterSpec left, FilterSpec right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is FilterSpec other && Equals(other);

    /// <inheritdoc />
    public bool Equals(FilterSpec other) =>
        string.Equals(_name, other._name, StringComparison.Ordinal)
        && string.Equals(_spec, other._spec, StringComparison.Ordinal);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_name, _spec);
}
