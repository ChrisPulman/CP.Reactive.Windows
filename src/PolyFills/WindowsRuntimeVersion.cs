// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.PolyFills;

/// <summary>Provides a target-independent Windows version check.</summary>
internal static class WindowsRuntimeVersion
{
    /// <summary>Determines whether the current Windows version is at least the requested version.</summary>
    /// <param name="major">The major version.</param>
    /// <param name="minor">The minor version.</param>
    /// <param name="build">The build number.</param>
    /// <param name="revision">The revision number.</param>
    /// <returns><see langword="true" /> when the current Windows version is at least the requested version.</returns>
    internal static bool IsAtLeast(int major, int minor, int build, int revision)
    {
#if NETFRAMEWORK
        return Environment.OSVersion.Platform == PlatformID.Win32NT
            && Environment.OSVersion.Version.CompareTo(new(major, minor, build, revision)) >= 0;
#else
        return OperatingSystem.IsWindowsVersionAtLeast(major, minor, build, revision);
#endif
    }
}
