// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using ReactiveUI.Primitives.Disposables;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Kernel 32 functionality for app packages.</summary>
public static class PackageInfo
{
    /// <summary>The AppModel error code returned when the current process has no package identity.</summary>
    private const long AppModelErrorNoPackage = 15_700L;

    /// <summary>The Windows 8 major version.</summary>
    private const int Windows8MajorVersion = 6;

    /// <summary>Native kernel32 package entry points.</summary>
    private static class NativeMethods
    {
        /// <summary>The Kernel32 DLL library name.</summary>
        private const string Kernel32Dll = "kernel32.dll";

        /// <summary>Gets the package full name for the current process.</summary>
        /// <param name="packageFullNameLength">Package name buffer length.</param>
        /// <param name="packageFullName">Package name buffer.</param>
        /// <returns>Win32 result code.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern unsafe int GetCurrentPackageFullName(ref int packageFullNameLength, char* packageFullName);
    }

    /// <summary>Composes package identity operations without invoking them during construction.</summary>
    /// <param name="isWindowsVersionAtLeast">The Windows version operation.</param>
    /// <param name="getCurrentPackageFullName">The package-name operation.</param>
    private sealed class PackageInfoOperations(Func<int, int, int, int, bool> isWindowsVersionAtLeast, GetCurrentPackageFullNameOperation getCurrentPackageFullName)
    {
        /// <summary>Invokes the configured Windows version operation.</summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <returns>The configured operation result.</returns>
        public bool IsWindowsVersionAtLeast(int major, int minor) => isWindowsVersionAtLeast(major, minor, 0, 0);

        /// <summary>Invokes the configured package-name operation.</summary>
        /// <param name="packageFullNameLength">The package-name buffer length.</param>
        /// <param name="packageFullName">The package-name buffer.</param>
        /// <returns>The configured operation result.</returns>
        public unsafe int GetCurrentPackageFullName(ref int packageFullNameLength, char* packageFullName) => getCurrentPackageFullName(ref packageFullNameLength, packageFullName);
    }

    /// <summary>The Windows 8 minor version.</summary>
    private const int Windows8MinorVersion = 2;

    /// <summary>The maximum package full name length to allocate on the stack.</summary>
    private const int MaxStackPackageFullNameLength = 512;

    /// <summary>Package identity operations used by this process.</summary>
    private static unsafe PackageInfoOperations _operations = new(WindowsRuntimeVersion.IsAtLeast, NativeMethods.GetCurrentPackageFullName);

    /// <summary>Gets the current package full name.</summary>
    /// <returns>The current package full name.</returns>
    public static string CurrentPackageFullName => !_operations.IsWindowsVersionAtLeast(6, 2) ? null : GetCurrentPackageFullNameCore();

    /// <summary>Gets test if the current process is running as a UWP app ("on the UWP").</summary>
    public static bool IsRunningOnUwp => CurrentPackageFullName is not null;

    /// <summary>Overrides package identity operations for deterministic tests.</summary>
    /// <param name="isWindowsVersionAtLeast">The replacement Windows version operation.</param>
    /// <param name="getCurrentPackageFullName">The replacement package-name operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(Func<int, int, int, int, bool> isWindowsVersionAtLeast, GetCurrentPackageFullNameOperation getCurrentPackageFullName)
    {
        Throw.IfNull(isWindowsVersionAtLeast);
        Throw.IfNull(getCurrentPackageFullName);
        PackageInfoOperations operations = _operations;
        _operations = new(isWindowsVersionAtLeast, getCurrentPackageFullName);
        return Scope.Create(operations, delegate(PackageInfoOperations previous)
        {
            _operations = previous;
        });
    }

    /// <summary>Gets the current package full name from kernel32.</summary>
    /// <returns>The current package full name.</returns>
    internal static unsafe string GetCurrentPackageFullNameCore()
    {
        int length = 0;
        if ((long)GetCurrentPackageFullName(ref length, null) == 15_700)
        {
            return null;
        }

        Span<char> span = ((length > 512) ? ((Span<char>)new char[length]) : stackalloc char[length]);
        Span<char> packageName = span;
        fixed (char* packageNamePointer = packageName)
        {
            return (long)GetCurrentPackageFullName(ref length, packageNamePointer) != 15_700 ? new(packageNamePointer, 0, length) : null;
        }
    }

    /// <summary>The get current package full name delegate.</summary>
    /// <param name="packageFullNameLength">The package full name buffer length.</param>
    /// <param name="packageFullName">The package full name buffer.</param>
    /// <returns>Win32 result code.</returns>
    private static unsafe int GetCurrentPackageFullName(ref int packageFullNameLength, char* packageFullName) => _operations.GetCurrentPackageFullName(ref packageFullNameLength, packageFullName);
}
