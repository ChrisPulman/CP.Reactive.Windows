// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using ReactiveUI.Primitives.Disposables;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Provides access to Process Status API functions.</summary>
#if NETFRAMEWORK
public static class PsApi
#else
public static partial class PsApi
#endif
{
    /// <summary>The capacity of the fixed-size path buffers.</summary>
    private const int PathBufferCapacity = 512;

    /// <summary>PSAPI operations used by this process.</summary>
    private static unsafe PsApiOperations _operations = new(
        NativeMethods.EmptyWorkingSet,
        NativeMethods.GetModuleFileNameEx,
        NativeMethods.GetProcessImageFileName);

    /// <summary>Removes as many pages as possible from the current process.</summary>
    public static void EmptyWorkingSet()
    {
        using Process currentProcess = Process.GetCurrentProcess();
        _ = EmptyWorkingSet(currentProcess.Handle);
    }

    /// <summary>Retrieves the fully qualified path for the file containing the specified module.</summary>
    /// <param name="processHandle">A handle to the process that contains the module.</param>
    /// <param name="moduleHandle">A module handle, or <see cref="IntPtr.Zero"/> for the executable.</param>
    /// <returns>The module file name, or <see langword="null"/> when it cannot be retrieved.</returns>
    public static unsafe string GetModuleFilename(IntPtr processHandle, IntPtr moduleHandle)
    {
        char* pathBuffer = stackalloc char[PathBufferCapacity];
        int characterCount = _operations.GetModuleFileNameEx(
            processHandle,
            moduleHandle,
            pathBuffer,
            PathBufferCapacity);
        return characterCount <= 0 ? null : new(pathBuffer, 0, characterCount);
    }

    /// <summary>Retrieves the fully qualified executable image path for the specified process.</summary>
    /// <param name="processHandle">A handle to the process that contains the executable.</param>
    /// <returns>The process image file name, or <see langword="null"/> when it cannot be retrieved.</returns>
    public static unsafe string GetProcessImageFileName(IntPtr processHandle)
    {
        char* pathBuffer = stackalloc char[PathBufferCapacity];
        int characterCount = GetProcessImageFileName(
            processHandle,
            pathBuffer,
            PathBufferCapacity);
        return characterCount <= 0 ? null : new(pathBuffer, 0, characterCount);
    }

    /// <summary>Retrieves the executable image file name for the specified process.</summary>
    /// <param name="processHandle">A handle to the process that contains the executable.</param>
    /// <param name="imageFileName">Receives the executable image file name.</param>
    /// <param name="size">The capacity of <paramref name="imageFileName"/> in characters.</param>
    /// <returns>The number of copied characters, or zero when the function fails.</returns>
    public static unsafe int GetProcessImageFileName(
        IntPtr processHandle,
        [Out] char* imageFileName,
        int size) => _operations.GetProcessImageFileName(processHandle, imageFileName, size);

    /// <summary>Overrides native PSAPI operations for deterministic tests.</summary>
    /// <param name="emptyWorkingSet">The replacement empty-working-set operation.</param>
    /// <param name="getModuleFileName">The replacement module-file-name operation.</param>
    /// <param name="getProcessImageFileName">The replacement process-image-file-name operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(
        EmptyWorkingSetOperation emptyWorkingSet,
        GetModuleFileNameOperation getModuleFileName,
        GetProcessImageFileNameOperation getProcessImageFileName)
    {
        Throw.IfNull(emptyWorkingSet);
        Throw.IfNull(getModuleFileName);
        Throw.IfNull(getProcessImageFileName);
        PsApiOperations operations = _operations;
        _operations = new(emptyWorkingSet, getModuleFileName, getProcessImageFileName);
        return Scope.Create(operations, static previous => _operations = previous);
    }

    /// <summary>Removes as many pages as possible from the working set of the specified process.</summary>
    /// <param name="processHandle">A handle to the process.</param>
    /// <returns>Nonzero if the working set was emptied; otherwise, zero.</returns>
    private static int EmptyWorkingSet(IntPtr processHandle) => _operations.EmptyWorkingSet(processHandle);

    /// <summary>Native PSAPI entry points.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>The PSAPI DLL library name.</summary>
        private const string PsApiDll = "psapi.dll";

        /// <summary>Removes as many pages as possible from a process working set.</summary>
        /// <param name="processHandle">Process handle.</param>
        /// <returns>Nonzero on success; otherwise, zero.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport(PsApiDll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int EmptyWorkingSet(IntPtr processHandle);
#else
        [LibraryImport(PsApiDll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int EmptyWorkingSet(IntPtr processHandle);
#endif

        /// <summary>Gets the module file name for a process module.</summary>
        /// <param name="processHandle">Process handle.</param>
        /// <param name="moduleHandle">Module handle.</param>
        /// <param name="filename">Output file name buffer.</param>
        /// <param name="size">Output buffer size.</param>
        /// <returns>Number of copied characters.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport(PsApiDll, EntryPoint = "GetModuleFileNameExW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern unsafe int GetModuleFileNameEx(
            IntPtr processHandle,
            IntPtr moduleHandle,
            char* filename,
            int size);
#else
        [LibraryImport(PsApiDll, EntryPoint = "GetModuleFileNameExW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static unsafe partial int GetModuleFileNameEx(
            IntPtr processHandle,
            IntPtr moduleHandle,
            char* filename,
            int size);
#endif

        /// <summary>Gets the executable image file name for a process.</summary>
        /// <param name="processHandle">Process handle.</param>
        /// <param name="imageFileName">Output image file name buffer.</param>
        /// <param name="size">Output buffer size.</param>
        /// <returns>Number of copied characters.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport(PsApiDll, EntryPoint = "GetProcessImageFileNameW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern unsafe int GetProcessImageFileName(
            IntPtr processHandle,
            char* imageFileName,
            int size);
#else
        [LibraryImport(PsApiDll, EntryPoint = "GetProcessImageFileNameW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static unsafe partial int GetProcessImageFileName(
            IntPtr processHandle,
            char* imageFileName,
            int size);
#endif
    }

    /// <summary>Composes PSAPI operations without invoking them during construction.</summary>
    /// <param name="emptyWorkingSet">The empty-working-set operation.</param>
    /// <param name="getModuleFileName">The module-file-name operation.</param>
    /// <param name="getProcessImageFileName">The process-image-file-name operation.</param>
    private sealed class PsApiOperations(
        EmptyWorkingSetOperation emptyWorkingSet,
        GetModuleFileNameOperation getModuleFileName,
        GetProcessImageFileNameOperation getProcessImageFileName)
    {
        /// <summary>Invokes the configured empty-working-set operation.</summary>
        /// <param name="processHandle">The process handle.</param>
        /// <returns>The configured operation result.</returns>
        public int EmptyWorkingSet(IntPtr processHandle) => emptyWorkingSet(processHandle);

        /// <summary>Invokes the configured module-file-name operation.</summary>
        /// <param name="processHandle">The process handle.</param>
        /// <param name="moduleHandle">The module handle.</param>
        /// <param name="filename">The output filename buffer.</param>
        /// <param name="size">The output buffer size.</param>
        /// <returns>The configured operation result.</returns>
        public unsafe int GetModuleFileNameEx(
            IntPtr processHandle,
            IntPtr moduleHandle,
            char* filename,
            int size) => getModuleFileName(processHandle, moduleHandle, filename, size);

        /// <summary>Invokes the configured process-image-file-name operation.</summary>
        /// <param name="processHandle">The process handle.</param>
        /// <param name="imageFileName">The output image filename buffer.</param>
        /// <param name="size">The output buffer size.</param>
        /// <returns>The configured operation result.</returns>
        public unsafe int GetProcessImageFileName(
            IntPtr processHandle,
            char* imageFileName,
            int size) => getProcessImageFileName(processHandle, imageFileName, size);
    }
}
