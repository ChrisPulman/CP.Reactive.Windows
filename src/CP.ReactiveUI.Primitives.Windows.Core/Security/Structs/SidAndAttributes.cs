// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel;

namespace CP.ReactiveUI.Primitives.Windows.Native.Security.Structs;

/// <summary>The SID_AND_ATTRIBUTES structure represents a security identifier (SID) and its attributes. SIDs are used to uniquely identify users or groups.</summary>
internal readonly struct SidAndAttributes : IEquatable<SidAndAttributes>
{
    /// <summary>A pointer to a SID structure.</summary>
    private readonly IntPtr _sid;

    /// <summary>Specifies attributes of the SID. This value contains up to 32 one-bit flags. Its meaning depends on the definition and use of the SID.</summary>
    private readonly uint _attributes;

    /// <summary>Initializes a new instance of the <see cref="SidAndAttributes"/> struct.</summary>
    /// <param name="sid">The SID pointer.</param>
    /// <param name="attributes">The SID attributes.</param>
    internal SidAndAttributes(IntPtr sid, uint attributes)
    {
        _sid = sid;
        _attributes = attributes;
    }

    /// <summary>Compares two SID-and-attribute values for equality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true" /> when both values are equal.</returns>
    public static bool operator ==(SidAndAttributes left, SidAndAttributes right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two SID-and-attribute values for inequality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true" /> when the values differ.</returns>
    public static bool operator !=(SidAndAttributes left, SidAndAttributes right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public bool Equals(SidAndAttributes other) => _sid == other._sid && _attributes == other._attributes;

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is SidAndAttributes other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_sid, _attributes);

    /// <summary>Checks whether all requested SID attributes are set.</summary>
    /// <param name="attributes">The required SID attributes.</param>
    /// <returns><see langword="true" /> when all requested attributes are set; otherwise, <see langword="false" />.</returns>
    internal bool HasAttributes(uint attributes) => (_attributes & attributes) == attributes;

    /// <summary>Converts the SID pointer to the Windows string SID form.</summary>
    /// <returns>The string SID, or <see cref="F:System.String.Empty" /> when conversion fails.</returns>
    internal string ToSidString()
    {
        if (!Advapi32Api.ConvertSidToStringSid(_sid, out var sidString))
        {
            return string.Empty;
        }

        try
        {
            return Marshal.PtrToStringUni(sidString) ?? string.Empty;
        }
        finally
        {
            _ = Kernel32Api.LocalFree(sidString);
        }
    }
}
