// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native;

/// <summary>Extension methods to test the windows version.</summary>
public static class WindowsVersion
{
    /// <summary>The Windows XP major version number.</summary>
    private const int WindowsXpMajor = 5;

    /// <summary>The Windows XP minor version number.</summary>
    private const int WindowsXpMinor = 1;

    /// <summary>The Windows Vista through Windows 8.1 major version number.</summary>
    private const int WindowsVistaMajor = 6;

    /// <summary>The Windows Vista minor version number.</summary>
    private const int WindowsVistaMinor = 0;

    /// <summary>The Windows 7 minor version number.</summary>
    private const int Windows7Minor = 1;

    /// <summary>The Windows 8 minor version number.</summary>
    private const int Windows8Minor = 2;

    /// <summary>The Windows 8.1 minor version number.</summary>
    private const int Windows81Minor = 3;

    /// <summary>The Windows 10 major version number.</summary>
    private const int Windows10Major = 10;

    /// <summary>The first Windows 11 build number.</summary>
    private const int Windows11MinimumBuild = 22_000;

    /// <summary>Provides the operating-system version for the current async flow.</summary>
    private static readonly AsyncLocal<Func<Version>> VersionProvider = new();

    /// <summary>Provides the production operating-system version.</summary>
    private static readonly Func<Version> SystemVersionProvider = static () => Environment.OSVersion.Version;

    /// <summary>Gets get the current windows version.</summary>
    public static Version WinVersion => (VersionProvider.Value ?? SystemVersionProvider)();

    /// <summary>Gets test if the current OS is Windows 10.</summary>
    /// <returns>true if we are running on Windows 10.</returns>
    public static bool IsWindows10 => WinVersion.Major == Windows10Major;

    /// <summary>Gets test if the current OS is Windows 11 or later.</summary>
    /// <returns>true if we are running on Windows 11 or later.</returns>
    public static bool IsWindows11OrLater =>
        WinVersion.Major > Windows10Major
        || (WinVersion.Major == Windows10Major && WinVersion.Build >= Windows11MinimumBuild);

    /// <summary>Gets test if the current OS is Windows 10 or later.</summary>
    /// <returns>true if we are running on Windows 10 or later.</returns>
    public static bool IsWindows10OrLater => WinVersion.Major >= Windows10Major;

    /// <summary>Gets test if the current OS is Windows 7 or later.</summary>
    /// <returns>true if we are running on Windows 7 or later.</returns>
    public static bool IsWindows7OrLater =>
        (WinVersion.Major == WindowsVistaMajor && WinVersion.Minor >= Windows7Minor)
        || WinVersion.Major > WindowsVistaMajor;

    /// <summary>Gets test if the current OS is Windows 8.0.</summary>
    /// <returns>true if we are running on Windows 8.0.</returns>
    public static bool IsWindows8 =>
        WinVersion.Major == WindowsVistaMajor && WinVersion.Minor == Windows8Minor;

    /// <summary>Gets test if the current OS is Windows 8(.1).</summary>
    /// <returns>true if we are running on Windows 8(.1).</returns>
    public static bool IsWindows81 =>
        WinVersion.Major == WindowsVistaMajor && WinVersion.Minor == Windows81Minor;

    /// <summary>Gets test if the current OS is Windows 8.0 or 8.1.</summary>
    /// <returns>true if we are running on Windows 8.1 or 8.0.</returns>
    public static bool IsWindows8X => IsWindows8 || IsWindows81;

    /// <summary>Gets test if the current OS is Windows 8.1 or later.</summary>
    /// <returns>true if we are running on Windows 8.1 or later.</returns>
    public static bool IsWindows81OrLater =>
        (WinVersion.Major == WindowsVistaMajor && WinVersion.Minor >= Windows81Minor)
        || WinVersion.Major > WindowsVistaMajor;

    /// <summary>Gets test if the current OS is Windows 8 or later.</summary>
    /// <returns>true if we are running on Windows 8 or later.</returns>
    public static bool IsWindows8OrLater =>
        (WinVersion.Major == WindowsVistaMajor && WinVersion.Minor >= Windows8Minor)
        || WinVersion.Major > WindowsVistaMajor;

    /// <summary>Gets test if the current OS is Windows Vista.</summary>
    /// <returns>true if we are running on Windows Vista or later.</returns>
    public static bool IsWindowsVista =>
        WinVersion.Major == WindowsVistaMajor && WinVersion.Minor == WindowsVistaMinor;

    /// <summary>Gets test if the current OS is Windows Vista or later.</summary>
    /// <returns>true if we are running on Windows Vista or later.</returns>
    public static bool IsWindowsVistaOrLater => WinVersion.Major >= WindowsVistaMajor;

    /// <summary>Gets test if the current OS is from before Windows Vista (e.g. Windows XP).</summary>
    /// <returns>true if we are running on Windows from before Vista.</returns>
    public static bool IsWindowsBeforeVista => WinVersion.Major < WindowsVistaMajor;

    /// <summary>Gets test if the current OS is Windows XP.</summary>
    /// <returns>true if we are running on Windows XP or later.</returns>
    public static bool IsWindowsXp =>
        WinVersion.Major == WindowsXpMajor && WinVersion.Minor >= WindowsXpMinor;

    /// <summary>Gets test if the current OS is Windows XP or later.</summary>
    /// <returns>true if we are running on Windows XP or later.</returns>
    public static bool IsWindowsXpOrLater =>
        WinVersion.Major > WindowsXpMajor
        || (WinVersion.Major == WindowsXpMajor && WinVersion.Minor >= WindowsXpMinor);

    /// <summary>
    ///     Test if the current Windows version is 10 and the build number or later
    ///     See the build numbers <a href="https://en.wikipedia.org/wiki/Windows_10_version_history">here</a>
    /// </summary>
    /// <param name="minimalBuildNumber">int</param>
    /// <returns>bool.</returns>
    public static bool IsWindows10BuildOrLater(int minimalBuildNumber) =>
        IsWindows10 && WinVersion.Build >= minimalBuildNumber;

    /// <summary>Overrides the operating-system version within the current async flow for deterministic tests.</summary>
    /// <param name="versionProvider">The version provider to use while the returned scope is active.</param>
    /// <returns>A scope that restores the previous provider.</returns>
    internal static IDisposable OverrideVersionProviderForTesting(Func<Version> versionProvider)
    {
        Throw.IfNull(versionProvider);
        Func<Version> previousProvider = VersionProvider.Value;
        VersionProvider.Value = versionProvider;
        return Scope.Create(previousProvider, static previous => VersionProvider.Value = previous);
    }
}
