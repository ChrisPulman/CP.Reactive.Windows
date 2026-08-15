// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

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

    /// <summary>Gets get the current windows version.</summary>
    public static Version WinVersion { get; } = Environment.OSVersion.Version;

    /// <summary>Gets test if the current OS is Windows 10.</summary>
    /// <returns>true if we are running on Windows 10.</returns>
    public static bool IsWindows10 { get; } = WinVersion.Major == 10;

    /// <summary>Gets test if the current OS is Windows 11 or later.</summary>
    /// <returns>true if we are running on Windows 11 or later.</returns>
    public static bool IsWindows11OrLater { get; } = WinVersion.Major >= 10 && WinVersion.Build >= 22_000;

    /// <summary>Gets test if the current OS is Windows 10 or later.</summary>
    /// <returns>true if we are running on Windows 10 or later.</returns>
    public static bool IsWindows10OrLater { get; } = WinVersion.Major >= 10;

    /// <summary>Gets test if the current OS is Windows 7 or later.</summary>
    /// <returns>true if we are running on Windows 7 or later.</returns>
    public static bool IsWindows7OrLater { get; } = (WinVersion.Major == 6 && WinVersion.Minor >= 1) || WinVersion.Major > 6;

    /// <summary>Gets test if the current OS is Windows 8.0.</summary>
    /// <returns>true if we are running on Windows 8.0.</returns>
    public static bool IsWindows8 { get; } = WinVersion.Major == 6 && WinVersion.Minor == 2;

    /// <summary>Gets test if the current OS is Windows 8(.1).</summary>
    /// <returns>true if we are running on Windows 8(.1).</returns>
    public static bool IsWindows81 { get; } = WinVersion.Major == 6 && WinVersion.Minor == 3;

    /// <summary>Gets test if the current OS is Windows 8.0 or 8.1.</summary>
    /// <returns>true if we are running on Windows 8.1 or 8.0.</returns>
    public static bool IsWindows8X { get; } = IsWindows8 || IsWindows81;

    /// <summary>Gets test if the current OS is Windows 8.1 or later.</summary>
    /// <returns>true if we are running on Windows 8.1 or later.</returns>
    public static bool IsWindows81OrLater { get; } = (WinVersion.Major == 6 && WinVersion.Minor >= 3) || WinVersion.Major > 6;

    /// <summary>Gets test if the current OS is Windows 8 or later.</summary>
    /// <returns>true if we are running on Windows 8 or later.</returns>
    public static bool IsWindows8OrLater { get; } = (WinVersion.Major == 6 && WinVersion.Minor >= 2) || WinVersion.Major > 6;

    /// <summary>Gets test if the current OS is Windows Vista.</summary>
    /// <returns>true if we are running on Windows Vista or later.</returns>
    public static bool IsWindowsVista { get; } = WinVersion.Major >= 6 && WinVersion.Minor == 0;

    /// <summary>Gets test if the current OS is Windows Vista or later.</summary>
    /// <returns>true if we are running on Windows Vista or later.</returns>
    public static bool IsWindowsVistaOrLater { get; } = WinVersion.Major >= 6;

    /// <summary>Gets test if the current OS is from before Windows Vista (e.g. Windows XP).</summary>
    /// <returns>true if we are running on Windows from before Vista.</returns>
    public static bool IsWindowsBeforeVista { get; } = WinVersion.Major < 6;

    /// <summary>Gets test if the current OS is Windows XP.</summary>
    /// <returns>true if we are running on Windows XP or later.</returns>
    public static bool IsWindowsXp { get; } = WinVersion.Major == 5 && WinVersion.Minor >= 1;

    /// <summary>Gets test if the current OS is Windows XP or later.</summary>
    /// <returns>true if we are running on Windows XP or later.</returns>
    public static bool IsWindowsXpOrLater { get; } = WinVersion.Major >= 5 || (WinVersion.Major == 5 && WinVersion.Minor >= 1);

    /// <summary>
    ///     Test if the current Windows version is 10 and the build number or later
    ///     See the build numbers <a href="https://en.wikipedia.org/wiki/Windows_10_version_history">here</a>
    /// </summary>
    /// <param name="minimalBuildNumber">int</param>
    /// <returns>bool.</returns>
    public static bool IsWindows10BuildOrLater(int minimalBuildNumber) => IsWindows10 && WinVersion.Build >= minimalBuildNumber;
}
