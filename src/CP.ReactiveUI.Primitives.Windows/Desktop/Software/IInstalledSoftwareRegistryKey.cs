// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Win32;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Software;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Software;
#endif
/// <summary>Represents a registry key used by installed-software enumeration.</summary>
internal interface IInstalledSoftwareRegistryKey : IDisposable
{
    /// <summary>Gets child subkey names.</summary>
    /// <returns>The child subkey names.</returns>
    string[] GetSubKeyNames();

    /// <summary>Opens a child subkey.</summary>
    /// <param name="subkeyName">The child subkey name.</param>
    /// <returns>The opened child subkey, or null when it is unavailable.</returns>
    IInstalledSoftwareRegistryKey OpenSubKey(string subkeyName);

    /// <summary>Gets a registry value.</summary>
    /// <param name="valueName">The value name.</param>
    /// <returns>The registry value.</returns>
    object GetValue(string valueName);

    /// <summary>Gets a registry value kind.</summary>
    /// <param name="valueName">The value name.</param>
    /// <returns>The registry value kind.</returns>
    RegistryValueKind GetValueKind(string valueName);

    /// <summary>Gets the value names available under the key.</summary>
    /// <returns>The value names.</returns>
    string[] GetValueNames();
}
