// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Browser;

/// <summary>Helper class for the Internet Explorer version.</summary>
public static class InternetExplorerVersion
{
    /// <summary>The Internet Explorer registry key.</summary>
    private const string IeKey = @"Software\Microsoft\Internet Explorer";

    /// <summary>The Internet Explorer registry values to inspect.</summary>
    private static readonly string[] IeVersionValueNames = ["svcVersion", "svcUpdateVersion", "Version", "W2kVersion"];

    /// <summary>The registry adapter used by the public convenience methods.</summary>
    private static IInternetExplorerRegistry _registryAccessor = new WindowsInternetExplorerRegistry();

    /// <summary>Gets the version of Internet Explorer.</summary>
    /// <returns>The browser version.</returns>
    public static int Version => GetVersion(_registryAccessor);

    /// <summary>Gets the highest possible version for the embedded browser.</summary>
    /// <returns>The IE feature version.</returns>
    public static int GetEmbVersion() => GetEmbVersion(true);

    /// <summary>Gets the highest possible version for the embedded browser.</summary>
    /// <param name="ignoreDoctype"><see langword="true" /> to ignore the doctype when loading a page.</param>
    /// <returns>The IE feature version.</returns>
    public static int GetEmbVersion(bool ignoreDoctype) => GetEmbVersion(ignoreDoctype, Version);

    /// <summary>Changes the browser version to the highest possible version.</summary>
    public static void ChangeEmbeddedVersion() => ChangeEmbeddedVersion(true);

    /// <summary>Changes the browser version to the highest possible version.</summary>
    /// <param name="ignoreDoctype"><see langword="true" /> to ignore the doctype when loading a page.</param>
    public static void ChangeEmbeddedVersion(bool ignoreDoctype) =>
        ChangeEmbeddedVersion(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location), ignoreDoctype);

    /// <summary>Changes the browser version for the specified application.</summary>
    /// <param name="applicationName">The process name.</param>
    public static void ChangeEmbeddedVersion(string applicationName) => ChangeEmbeddedVersion(applicationName, true);

    /// <summary>Changes the browser version for the specified application.</summary>
    /// <param name="applicationName">The process name.</param>
    /// <param name="ignoreDoctype"><see langword="true" /> to ignore the doctype when loading a page.</param>
    public static void ChangeEmbeddedVersion(string applicationName, bool ignoreDoctype) =>
        ChangeEmbeddedVersion(applicationName, GetEmbVersion(ignoreDoctype));

    /// <summary>Fixes the browser version for the specified application.</summary>
    /// <param name="applicationName">The process name.</param>
    /// <param name="browserVersion">
    /// Version, see
    /// <a href="https://msdn.microsoft.com/en-us/library/ee330730(v=vs.85).aspx#browser_emulation">Browser Emulation</a>
    /// </param>
    public static void ChangeEmbeddedVersion(string applicationName, int browserVersion) =>
        ChangeEmbeddedVersion(applicationName, browserVersion, _registryAccessor);

    /// <summary>Gets the major version from a registry value.</summary>
    /// <param name="value">The registry value.</param>
    /// <returns>The major version.</returns>
    public static int GetMajorVersion(object value)
    {
        var version = Convert.ToString(value);
        var separatorIndex = version.IndexOf('.');
        if (separatorIndex > 0)
        {
            version = version.Remove(separatorIndex);
        }

        return int.TryParse(version, out var result) ? result : 0;
    }

    /// <summary>Replaces the registry adapter used by the public convenience methods.</summary>
    /// <param name="registry">The replacement registry adapter.</param>
    /// <returns>The replaced registry adapter.</returns>
    internal static IInternetExplorerRegistry ExchangeRegistry(IInternetExplorerRegistry registry)
    {
        Throw.IfNull(registry);
        return System.Threading.Interlocked.Exchange(ref _registryAccessor, registry);
    }

    /// <summary>Gets the highest possible version for a known browser major version.</summary>
    /// <param name="ignoreDoctype"><see langword="true" /> to ignore the doctype when loading a page.</param>
    /// <param name="browserVersion">The installed browser major version.</param>
    /// <returns>The IE feature version.</returns>
    internal static int GetEmbVersion(bool ignoreDoctype, int browserVersion)
    {
        const int internetExplorerNine = 9;
        const int internetExplorerSeven = 7;
        const int doctypeMultiplier = 1000;
        const int compatibilityMultiplier = 1111;
        const int fallbackVersion = 7000;

        if (browserVersion > internetExplorerNine)
        {
            return (browserVersion * doctypeMultiplier) + (ignoreDoctype ? 1 : 0);
        }

        return browserVersion > internetExplorerSeven ? browserVersion * compatibilityMultiplier : fallbackVersion;
    }

    /// <summary>Changes an embedded browser version through a registry adapter.</summary>
    /// <param name="applicationName">The process name.</param>
    /// <param name="browserVersion">The feature version to use.</param>
    /// <param name="registry">The registry adapter to write.</param>
    internal static void ChangeEmbeddedVersion(string applicationName, int browserVersion, IInternetExplorerRegistry registry)
    {
        Throw.IfNullOrWhiteSpace(applicationName);
        Throw.IfNull(registry);

        ModifyRegistry(registry, "HKEY_CURRENT_USER", $"{applicationName}.exe", browserVersion);
#if DEBUG
        ModifyRegistry(registry, "HKEY_CURRENT_USER", $"{applicationName}.vshost.exe", browserVersion);
#endif
    }

    /// <summary>Gets the highest installed browser major version from a registry adapter.</summary>
    /// <param name="registry">The registry adapter to read.</param>
    /// <returns>The highest browser major version.</returns>
    internal static int GetVersion(IInternetExplorerRegistry registry)
    {
        Throw.IfNull(registry);

        var maxVersion = 0;
        foreach (var valueName in IeVersionValueNames)
        {
            maxVersion = Math.Max(maxVersion, GetMajorVersion(registry.GetValue(IeKey, valueName)));
        }

        return maxVersion;
    }

    /// <summary>Makes the change to the registry.</summary>
    /// <param name="registry">The registry adapter to write.</param>
    /// <param name="root">The registry root.</param>
    /// <param name="applicationName">The executable name.</param>
    /// <param name="featureVersion">The feature version to use.</param>
    private static void ModifyRegistry(IInternetExplorerRegistry registry, string root, string applicationName, int featureVersion)
    {
        var regKey = $@"{root}\Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
        try
        {
            registry.SetValue(regKey, applicationName, featureVersion);
        }
        catch (UnauthorizedAccessException)
        {
            // Some configurations hit access rights exceptions.
        }
        catch (SecurityException)
        {
            // Some configurations hit access rights exceptions.
        }
    }

    /// <summary>Provides Windows Registry access for Internet Explorer version discovery and configuration.</summary>
    internal sealed class WindowsInternetExplorerRegistry : IInternetExplorerRegistry
    {
        /// <summary>The registry reader.</summary>
        private readonly Func<string, string, object> _getValue;

        /// <summary>The registry writer.</summary>
        private readonly Action<string, string, object> _setValue;

        /// <summary>Initializes a new instance of the <see cref="WindowsInternetExplorerRegistry" /> class.</summary>
        public WindowsInternetExplorerRegistry()
            : this(ReadValue, Registry.SetValue)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="WindowsInternetExplorerRegistry" /> class.</summary>
        /// <param name="getValue">The value reader.</param>
        /// <param name="setValue">The value writer.</param>
        internal WindowsInternetExplorerRegistry(Func<string, string, object> getValue, Action<string, string, object> setValue)
        {
            Throw.IfNull(getValue);
            Throw.IfNull(setValue);
            _getValue = getValue;
            _setValue = setValue;
        }

        /// <inheritdoc />
        public object GetValue(string keyName, string valueName) => _getValue(keyName, valueName);

        /// <inheritdoc />
        public void SetValue(string keyName, string valueName, object value) => _setValue(keyName, valueName, value);

        /// <summary>Reads an Internet Explorer registry value from an opened registry key.</summary>
        /// <param name="internetExplorerKey">The opened Internet Explorer registry key.</param>
        /// <param name="valueName">The registry value name.</param>
        /// <returns>The registry value, or a fallback value.</returns>
        internal static object ReadValue(RegistryKey internetExplorerKey, string valueName) =>
            internetExplorerKey?.GetValue(valueName, "0") ?? "0";

        /// <summary>Reads an Internet Explorer registry value.</summary>
        /// <param name="keyName">The registry key name.</param>
        /// <param name="valueName">The registry value name.</param>
        /// <returns>The registry value, or a fallback value.</returns>
        private static object ReadValue(string keyName, string valueName)
        {
            using var internetExplorerKey = Registry.LocalMachine.OpenSubKey(keyName, false);
            return ReadValue(internetExplorerKey, valueName);
        }
    }
}
