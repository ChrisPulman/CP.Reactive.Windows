// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Browser;

/// <summary>Abstracts registry access for embedded-browser configuration.</summary>
internal interface IInternetExplorerRegistry
{
    /// <summary>Gets a registry value.</summary>
    /// <param name="keyName">The registry key name.</param>
    /// <param name="valueName">The registry value name.</param>
    /// <returns>The registry value, or a fallback value.</returns>
    object GetValue(string keyName, string valueName);

    /// <summary>Sets a registry value.</summary>
    /// <param name="keyName">The registry key name.</param>
    /// <param name="valueName">The registry value name.</param>
    /// <param name="value">The registry value to write.</param>
    void SetValue(string keyName, string valueName, object value);
}
