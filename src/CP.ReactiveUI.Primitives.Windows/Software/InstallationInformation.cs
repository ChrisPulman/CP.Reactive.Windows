// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using Microsoft.Win32;
using log4net;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Software;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Software;
#endif
/// <summary>A helper class to evaluate the installed software.</summary>
public static class InstallationInformation
{
    /// <summary>Production installed-software registry reader.</summary>
    private sealed class WindowsInstalledSoftwareRegistry : IInstalledSoftwareRegistry
    {
        /// <summary>The shared production registry reader.</summary>
        internal static readonly WindowsInstalledSoftwareRegistry Instance = new();

        /// <inheritdoc />
        public IInstalledSoftwareRegistryKey OpenLocalMachineSubKey(string subkeyName)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(subkeyName);
            if (registryKey is not null)
            {
                return new WindowsInstalledSoftwareRegistryKey(registryKey);
            }

            return null;
        }
    }

    /// <summary>Production registry key reader.</summary>
    /// <param name="registryKey">The wrapped registry key.</param>
    private sealed class WindowsInstalledSoftwareRegistryKey(RegistryKey registryKey) : IInstalledSoftwareRegistryKey
    {
        /// <inheritdoc />
        public void Dispose() => registryKey.Dispose();

        /// <inheritdoc />
        public string[] GetSubKeyNames() => registryKey.GetSubKeyNames();

        /// <inheritdoc />
        public IInstalledSoftwareRegistryKey OpenSubKey(string subkeyName)
        {
            RegistryKey subKey = registryKey.OpenSubKey(subkeyName);
            if (subKey is not null)
            {
                return new WindowsInstalledSoftwareRegistryKey(subKey);
            }

            return null;
        }

        /// <inheritdoc />
        public object GetValue(string valueName) => registryKey.GetValue(valueName);

        /// <inheritdoc />
        public RegistryValueKind GetValueKind(string valueName) => registryKey.GetValueKind(valueName);

        /// <inheritdoc />
        public string[] GetValueNames() => registryKey.GetValueNames();
    }

    /// <summary>Registry path containing uninstall entries.</summary>
    private const string UninstallKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";

    /// <summary>Logging source for registry parsing issues.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(InstallationInformation));

    /// <summary>Public writable properties available on <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Software.SoftwareDetails" /> by registry value name.</summary>
    private static readonly Dictionary<string, PropertyInfo> SoftwareDetailsPropertyMap = CreateSoftwareDetailsPropertyMap();

    /// <summary>The registry reader override used by deterministic tests.</summary>
    private static IInstalledSoftwareRegistry _registryOverride;

    /// <summary>Retrieves all the installed software.</summary>
    /// <returns>IEnumerable with SoftwareDetails.</returns>
    public static IEnumerable<SoftwareDetails> InstalledSoftware()
    {
        using IInstalledSoftwareRegistryKey registryKey = GetRegistry().OpenLocalMachineSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall");
        if (registryKey is null)
        {
            yield break;
        }

        string[] subKeyNames = registryKey.GetSubKeyNames();
        foreach (string subKeyName in subKeyNames)
        {
            using IInstalledSoftwareRegistryKey subKey = registryKey.OpenSubKey(subKeyName);
            if (subKey is not null)
            {
                yield return MapFromRegistryKey(subKeyName, subKey);
            }
        }
    }

    /// <summary>Maps registry-like values into software details.</summary>
    /// <param name="subkeyName">The registry subkey name.</param>
    /// <param name="valueNames">Available value names.</param>
    /// <param name="getValue">Reads a registry value.</param>
    /// <param name="getValueKind">Reads a registry value kind.</param>
    /// <returns>The mapped software details.</returns>
    internal static SoftwareDetails MapFromRegistryValues(string subkeyName, IEnumerable<string> valueNames, Func<string, object> getValue, Func<string, RegistryValueKind> getValueKind)
    {
        SoftwareDetails softwareDetails = CreateSoftwareDetails(subkeyName);
        foreach (string valueName in valueNames)
        {
            if (SoftwareDetailsPropertyMap.TryGetValue(valueName, out var propertyInfo) && TryGetRegistryValue(propertyInfo, getValue, getValueKind, out var value))
            {
                propertyInfo.SetValue(softwareDetails, value);
            }
        }

        return softwareDetails;
    }

    /// <summary>Replaces the registry reader for deterministic tests.</summary>
    /// <param name="registry">The replacement registry reader.</param>
    /// <returns>The previous registry reader.</returns>
    internal static IInstalledSoftwareRegistry SetRegistryForTesting(IInstalledSoftwareRegistry registry)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(registry);
        IInstalledSoftwareRegistry registry2 = GetRegistry();
        _registryOverride = ((registry == WindowsInstalledSoftwareRegistry.Instance) ? null : registry);
        return registry2;
    }

    /// <summary>Helper method to convert from a RegistryKey object to a SoftwareDetails class.</summary>
    /// <param name="subkeyName">string.</param>
    /// <param name="subKey">Registry key reader.</param>
    /// <returns>SoftwareDetails.</returns>
    private static SoftwareDetails MapFromRegistryKey(string subkeyName, IInstalledSoftwareRegistryKey subKey) => MapFromRegistryValues(subkeyName, subKey.GetValueNames(), subKey.GetValue, subKey.GetValueKind);

    /// <summary>Creates base software details for the registry subkey.</summary>
    /// <param name="subkeyName">Subkey name.</param>
    /// <returns>The initialized software details.</returns>
    private static SoftwareDetails CreateSoftwareDetails(string subkeyName)
    {
        SoftwareDetails softwareDetails = new SoftwareDetails { DisplayName = subkeyName };
        if (Guid.TryParse(subkeyName, out var id))
        {
            softwareDetails.Id = id;
        }

        return softwareDetails;
    }

    /// <summary>Creates the static registry-value-to-property map.</summary>
    /// <returns>The keyed property map.</returns>
    private static Dictionary<string, PropertyInfo> CreateSoftwareDetailsPropertyMap()
    {
        Dictionary<string, PropertyInfo> propertyMap = new(StringComparer.OrdinalIgnoreCase);
        PropertyInfo[] properties = typeof(SoftwareDetails).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (PropertyInfo propertyInfo in properties)
        {
            propertyMap[propertyInfo.Name] = propertyInfo;
        }

        return propertyMap;
    }

    /// <summary>Tries to get and convert a registry value for the target property.</summary>
    /// <param name="propertyInfo">Target property.</param>
    /// <param name="getValue">Reads a registry value.</param>
    /// <param name="getValueKind">Reads a registry value kind.</param>
    /// <param name="value">Converted value.</param>
    /// <returns>true when a value was converted.</returns>
    private static bool TryGetRegistryValue(PropertyInfo propertyInfo, Func<string, object> getValue, Func<string, RegistryValueKind> getValueKind, out object value)
    {
        value = null;
        object propertyValue = getValue(propertyInfo.Name);
        if (propertyValue is null)
        {
            return false;
        }

        try
        {
            value = ConvertRegistryValue(getValueKind(propertyInfo.Name), propertyValue, propertyInfo.PropertyType);
            return value is not null;
        }
        catch (Exception exception)
        {
            Log.Warn($"Couldn't parse value {propertyValue} to {propertyInfo.Name}", exception);
            return false;
        }
    }

    /// <summary>Converts a registry value to the requested property type.</summary>
    /// <param name="valueKind">Registry value kind.</param>
    /// <param name="propertyValue">Raw property value.</param>
    /// <param name="propertyType">Target property type.</param>
    /// <returns>The converted value, or null when the value should be ignored.</returns>
    private static object ConvertRegistryValue(RegistryValueKind valueKind, object propertyValue, Type propertyType)
    {
        switch (valueKind)
        {
        case RegistryValueKind.DWord:
        {
            int intValue = Convert.ToInt32(propertyValue);
            if (propertyType != typeof(bool))
            {
                return intValue;
            }

            return intValue == 1;
        }

        case RegistryValueKind.QWord:
        {
            long longValue = Convert.ToInt64(propertyValue);
            if (propertyType != typeof(bool))
            {
                return longValue;
            }

            return longValue == 1;
        }

        default:
        {
            if (!string.IsNullOrEmpty(propertyValue as string))
            {
                return Convert.ChangeType(propertyValue, propertyType);
            }

            return null;
        }
        }
    }

    /// <summary>Gets the active installed-software registry reader.</summary>
    /// <returns>The active registry reader.</returns>
    private static IInstalledSoftwareRegistry GetRegistry() => _registryOverride ?? WindowsInstalledSoftwareRegistry.Instance;
}
