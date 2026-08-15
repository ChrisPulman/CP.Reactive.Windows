// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Win32;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop;
#endif
/// <summary>Reads optional Windows registry values through one shared null-safe implementation.</summary>
internal static class RegistryValueReader
{
    /// <summary>Reads a named value from an optional registry key.</summary>
    /// <param name="key">The optional registry key.</param>
    /// <param name="valueName">The registry value name.</param>
    /// <returns>The registry value, or <see langword="null" /> when the key or value is unavailable.</returns>
    internal static object GetValue(RegistryKey key, string valueName) => key?.GetValue(valueName);
}
