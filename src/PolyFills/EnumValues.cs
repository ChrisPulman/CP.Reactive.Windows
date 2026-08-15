// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.PolyFills;

/// <summary>Provides enum value enumeration on every supported target framework.</summary>
internal static class EnumValues
{
    /// <summary>Gets all declared values for an enum type.</summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <returns>The declared enum values.</returns>
    internal static TEnum[] Get<TEnum>()
        where TEnum : struct, Enum
    {
#if NET5_0_OR_GREATER
        return Enum.GetValues<TEnum>();
#else
        return (TEnum[])Enum.GetValues(typeof(TEnum));
#endif
    }
}
