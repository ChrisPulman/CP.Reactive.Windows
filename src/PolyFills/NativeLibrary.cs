// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NETFRAMEWORK
namespace System.Runtime.InteropServices;

/// <summary>Compatibility implementation of the modern NativeLibrary API for .NET Framework.</summary>
internal static class NativeLibrary
{
    /// <summary>Loads a native library.</summary>
    /// <param name="libraryPath">The library path or name.</param>
    /// <returns>The loaded module handle.</returns>
    internal static IntPtr Load(string libraryPath)
    {
        var handle = NativeMethods.LoadLibrary(libraryPath);
        if (handle == IntPtr.Zero)
        {
            throw new NativeWin32Exception(Marshal.GetLastWin32Error());
        }

        return handle;
    }

    /// <summary>Loads a native library.</summary>
    /// <param name="libraryName">The library name.</param>
    /// <param name="assembly">The assembly requesting the load.</param>
    /// <param name="searchPath">The DLL search path.</param>
    /// <returns>The loaded module handle.</returns>
    internal static IntPtr Load(string libraryName, ReflectionAssembly assembly, DllImportSearchPath? searchPath)
    {
        _ = assembly;
        _ = searchPath;
        return Load(libraryName);
    }

    /// <summary>Gets an exported symbol address.</summary>
    /// <param name="handle">The module handle.</param>
    /// <param name="name">The export name.</param>
    /// <returns>The export address.</returns>
    internal static IntPtr GetExport(IntPtr handle, string name)
    {
        var export = GetProcAddress(handle, name);
        if (export == IntPtr.Zero)
        {
            throw new EntryPointNotFoundException(name);
        }

        return export;
    }

    /// <summary>Attempts to get an exported symbol address.</summary>
    /// <param name="handle">The module handle.</param>
    /// <param name="name">The export name.</param>
    /// <param name="address">The export address.</param>
    /// <returns><see langword="true"/> when the export exists.</returns>
    internal static bool TryGetExport(IntPtr handle, string name, out IntPtr address)
    {
        address = GetProcAddress(handle, name);
        return address != IntPtr.Zero;
    }

    /// <summary>Gets a native export address.</summary>
    /// <param name="moduleHandle">The module handle.</param>
    /// <param name="procedureName">The export name.</param>
    /// <returns>The export address.</returns>
    private static IntPtr GetProcAddress(IntPtr moduleHandle, string procedureName)
    {
        var procedureNamePointer = Marshal.StringToHGlobalAnsi(procedureName);
        try
        {
            return NativeMethods.GetProcAddress(moduleHandle, procedureNamePointer);
        }
        finally
        {
            Marshal.FreeHGlobal(procedureNamePointer);
        }
    }

    /// <summary>Contains the native loader entry points.</summary>
    private static class NativeMethods
    {
        /// <summary>Loads a native module.</summary>
        /// <param name="fileName">The module file name.</param>
        /// <returns>The loaded module handle.</returns>
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string fileName);

        /// <summary>Gets a native export address.</summary>
        /// <param name="moduleHandle">The module handle.</param>
        /// <param name="procedureName">The export name.</param>
        /// <returns>The export address.</returns>
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern IntPtr GetProcAddress(IntPtr moduleHandle, IntPtr procedureName);
    }
}
#endif
