// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Buffers.Binary;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Kernel 32 functionality.</summary>
#if NETFRAMEWORK
public static class Kernel32Api
#else
public static partial class Kernel32Api
#endif
{
    /// <summary>Default value for AttachProcess if not specifying a process ID, this uses the console of the parent of the current process.</summary>
    private const uint AttachParentProcess = uint.MaxValue;

    /// <summary>The Windows Vista major version.</summary>
    private const int VistaMajorVersion = 6;

    /// <summary>The maximum package full name length to allocate on the stack.</summary>
    private const int MaxStackPackageFullNameLength = 512;

    /// <summary>The native OSVERSIONINFOEXW byte size.</summary>
    private const int OsVersionInfoExSize = 284;

    /// <summary>The native major-version byte offset.</summary>
    private const int MajorVersionOffset = 4;

    /// <summary>The native minor-version byte offset.</summary>
    private const int MinorVersionOffset = 8;

    /// <summary>The native build-number byte offset.</summary>
    private const int BuildNumberOffset = 12;

    /// <summary>The native platform-id byte offset.</summary>
    private const int PlatformIdOffset = 16;

    /// <summary>The native service-pack string byte offset.</summary>
    private const int ServicePackVersionOffset = 20;

    /// <summary>The native service-pack string character capacity.</summary>
    private const int ServicePackVersionCharacterCapacity = 128;

    /// <summary>The native UTF-16 character byte size.</summary>
    private const int Utf16CharacterSize = 2;

    /// <summary>The native service-pack major-version byte offset.</summary>
    private const int ServicePackMajorOffset = 276;

    /// <summary>The native service-pack minor-version byte offset.</summary>
    private const int ServicePackMinorOffset = 278;

    /// <summary>The native suite-mask byte offset.</summary>
    private const int SuiteMaskOffset = 280;

    /// <summary>The native product-type byte offset.</summary>
    private const int ProductTypeOffset = 282;

    /// <summary>The time provider used for system time calculations.</summary>
    private static readonly TimeProvider Clock = TimeProvider.System;

    /// <summary>The directory separator characters.</summary>
    private static readonly char[] DirectorySeparator = ['\\'];

    /// <summary>The cached system startup time.</summary>
    private static DateTimeOffset? _systemStartup;

    /// <summary>The Kernel32 operations used by the managed wrappers.</summary>
    private static unsafe Kernel32Operations _operations = new()
    {
        SetDefaultDllDirectories = NativeMethods.SetDefaultDllDirectories,
        SetDllDirectory = NativeMethods.SetDllDirectory,
        AllocConsole = NativeMethods.AllocConsole,
        AttachConsole = NativeMethods.AttachConsole,
        CloseHandle = NativeMethods.CloseHandle,
        OpenProcess = NativeMethods.OpenProcess,
        QueryDosDevice = NativeMethods.QueryDosDevice,
        QueryFullProcessImageName = NativeMethods.QueryFullProcessImageName,
        GetVersionEx = NativeMethods.GetVersionEx,
        GetPackageFullName = NativeMethods.GetPackageFullName,
    };

    /// <summary>Gets a <see cref="DateTimeOffset"/> which specifies when the system started.</summary>
    public static DateTimeOffset SystemStartup
    {
        get
        {
            if (!_systemStartup.HasValue)
            {
                _systemStartup = Clock
                    .GetLocalNow()
                    .Subtract(TimeSpan.FromMilliseconds(GetTickCount64()));
            }

            return _systemStartup.Value;
        }
    }

    /// <summary>A helper method to prevent Dll Hijacking, this drastically reduces the DLL search paths! This will not help against a version.dll attack..</summary>
    public static void PreventDllHijacking() => PreventDllHijacking(string.Empty);

    /// <summary>A helper method to prevent Dll Hijacking, this drastically reduces the DLL search paths! This will not help against a version.dll attack..</summary>
    /// <param name="allowDllDirectory">A single directory where additional DLL searches are made.</param>
    public static void PreventDllHijacking(string allowDllDirectory)
    {
        _ = SetDllDirectory(allowDllDirectory);
        if (string.IsNullOrWhiteSpace(allowDllDirectory))
        {
            _ = SetDefaultDllDirectories(DefaultDllDirectories.SearchSystem32Directory);
        }
        else
        {
            _ = SetDefaultDllDirectories(
                DefaultDllDirectories.SearchUserDirectories
                    | DefaultDllDirectories.SearchSystem32Directory);
        }
    }

    /// <summary>Method to get the process path.</summary>
    /// <param name="processId">Process ID.</param>
    /// <returns>Process path.</returns>
    public static unsafe string GetProcessPath(int processId)
    {
        IntPtr processHandle = OpenProcess(
            ProcessAccessRights.VirtualMemoryRead | ProcessAccessRights.QueryInformation,
            inheritHandle: false,
            processId);
        if (processHandle != IntPtr.Zero)
        {
            try
            {
                string path = PsApi.GetModuleFilename(processHandle, IntPtr.Zero);
                if (path is not null)
                {
                    return path;
                }
            }
            finally
            {
                _ = CloseHandle(processHandle);
            }
        }

        processHandle = OpenProcess(
            ProcessAccessRights.QueryInformation,
            inheritHandle: false,
            processId);
        if (processHandle == IntPtr.Zero)
        {
            return null;
        }

        char* pathBuffer = stackalloc char[MaxStackPackageFullNameLength];
        try
        {
            int bufferSize = MaxStackPackageFullNameLength;
            if (
                Environment.OSVersion.Version.Major >= VistaMajorVersion
                && QueryFullProcessImageName(processHandle, 0U, pathBuffer, ref bufferSize)
                && bufferSize > 0)
            {
                return new(pathBuffer, 0, bufferSize);
            }

            string dosPath = PsApi.GetProcessImageFileName(processHandle);
            if (dosPath is not null)
            {
                return GetProcessPathFromDosDevices(
                    dosPath,
                    pathBuffer,
                    MaxStackPackageFullNameLength);
            }
        }
        finally
        {
            _ = CloseHandle(processHandle);
        }

        return null;
    }

    /// <summary>Specifies a default set of directories to search when the calling process loads a DLL.</summary>
    /// <param name="directoryFlags">DefaultDllDirectories with the directories to search. This parameter can be any combination of the following values.</param>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>The process DLL search path applies only to the calling process and persists for the life of the process.</remarks>
    public static bool SetDefaultDllDirectories(DefaultDllDirectories directoryFlags) =>
        _operations.SetDefaultDllDirectories(directoryFlags);

    /// <summary>Adds a directory to the search path used to locate DLLs for the application.</summary>
    /// <param name="pathName">The directory to be added to the search path.</param>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
    /// </returns>
    public static bool SetDllDirectory(string pathName) => _operations.SetDllDirectory(pathName);

    /// <summary>Allocates a new console for the calling process.</summary>
    /// <returns>True if the console was allocated; otherwise, false.</returns>
    public static bool AllocConsole() => _operations.AllocConsole();

    /// <summary>Retrieves the process identifier of the calling process.</summary>
    /// <returns>The current process identifier.</returns>
    public static int GetCurrentProcessId() => NativeMethods.GetCurrentProcessId();

    /// <summary>Retrieves the thread identifier of the calling thread.</summary>
    /// <returns>The current thread identifier.</returns>
    public static int GetCurrentThreadId() => NativeMethods.GetCurrentThreadId();

    /// <summary>
    /// Attaches the calling process to the console of the specified process.
    /// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms681944(v=vs.85).aspx">AllocConsole function</a>.
    /// </summary>
    /// <returns>True if the console was attached; otherwise, false.</returns>
    public static bool AttachConsole() => AttachConsole(uint.MaxValue);

    /// <summary>
    /// Attaches the calling process to the console of the specified process.
    /// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms681944(v=vs.85).aspx">AllocConsole function</a>.
    /// </summary>
    /// <param name="processId">The identifier of the process whose console is to be used. Or -1 to use the parent process console.</param>
    /// <returns>True if the console was attached; otherwise, false.</returns>
    public static bool AttachConsole(uint processId) => _operations.AttachConsole(processId);

    /// <summary>
    /// Closes an open object handle.
    /// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms724211(v=vs.85).aspx">CloseHandle function</a>.
    /// The CloseHandle function closes handles for Win32 objects such as processes, threads, files, events, mutexes, pipes, and waitable timers.
    /// </summary>
    /// <param name="objectHandle">A valid handle to an open object.</param>
    /// <returns>True if the handle was closed; otherwise, false.</returns>
    public static bool CloseHandle(IntPtr objectHandle) => _operations.CloseHandle(objectHandle);

    /// <summary>
    ///     Frees the loaded dynamic-link library (DLL) module and, if necessary, decrements its reference count.
    ///     When the reference count reaches zero, the module is unloaded from the address space of the calling process and the
    ///     handle is no longer valid.
    /// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms683152(v=vs.85).aspx">FreeLibrary function</a>
    /// </summary>
    /// <param name="module">IntPtr</param>
    /// <returns>True if the library was freed; otherwise, false.</returns>
    public static bool FreeLibrary(IntPtr module) => NativeMethods.FreeLibrary(module);

    /// <summary>
    /// Retrieves a module handle for the specified module. The module must have been loaded by the calling process.
    /// To avoid the race conditions described in the Remarks section, use the GetModuleHandleEx function.
    /// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms683199(v=vs.85).aspx">GetModuleHandle function</a>
    /// </summary>
    /// <param name="moduleName">The name of the loaded module, or null to return the executable module handle.</param>
    /// <returns>If the function succeeds, the return value is a handle to the specified module.</returns>
    public static IntPtr GetModuleHandle(string moduleName) =>
        NativeMethods.GetModuleHandle(moduleName);

    /// <summary>See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms724358.aspx">GetProductInfo function</a>.</summary>
    /// <param name="operatingSystemMajorVersion">
    ///     The major version number of the operating system. The minimum value is 6.
    ///     The combination of the dwOSMajorVersion, dwOSMinorVersion, dwSpMajorVersion, and dwSpMinorVersion parameters
    ///     describes the maximum target operating system version for the application. For example, Windows Vista and Windows
    ///     Server 2008 are version 6.0.0.0 and Windows 7 and Windows Server 2008 R2 are version 6.1.0.0.
    /// </param>
    /// <param name="operatingSystemMinorVersion">The minor version number of the operating system. The minimum value is 0.</param>
    /// <param name="servicePackMajorVersion">The major version number of the operating system service pack. The minimum value is 0.</param>
    /// <param name="servicePackMinorVersion">The minor version number of the operating system service pack. The minimum value is 0.</param>
    /// <param name="edition">WindowsProducts</param>
    /// <returns>True if product information was retrieved; otherwise, false.</returns>
    public static bool GetProductInfo(
        int operatingSystemMajorVersion,
        int operatingSystemMinorVersion,
        int servicePackMajorVersion,
        int servicePackMinorVersion,
        out WindowsProducts edition) =>
        NativeMethods.GetProductInfo(
            operatingSystemMajorVersion,
            operatingSystemMinorVersion,
            servicePackMajorVersion,
            servicePackMinorVersion,
            out edition);

    /// <summary>See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms724451(v=vs.85).aspx">GetVersionEx function</a>.</summary>
    /// <param name="operatingSystemVersionInfo">OsVersionInfoEx</param>
    /// <returns>If the function fails, the return value is false. To get extended error information, call GetLastError.</returns>
    public static unsafe bool GetVersionEx(ref OsVersionInfoEx operatingSystemVersionInfo)
    {
        Span<byte> versionInfo = stackalloc byte[OsVersionInfoExSize];
        BinaryPrimitives.WriteInt32LittleEndian(versionInfo, OsVersionInfoExSize);
        fixed (byte* versionInfo2 = versionInfo)
        {
            if (!_operations.GetVersionEx(versionInfo2))
            {
                return false;
            }
        }

        operatingSystemVersionInfo = CreateOsVersionInfo(versionInfo);
        return true;
    }

    /// <summary>
    ///     Loads the specified module into the address space of the calling process. The specified module may cause other
    ///     modules to be loaded.
    ///     See <a href="https://msdn.microsoft.com/en-us/library/ms684175(VS.85).aspx">LoadLibrary function</a>
    /// </summary>
    /// <param name="fileName">string with the library</param>
    /// <returns>The module handle, or <see cref="F:System.IntPtr.Zero" /> if the library could not be loaded.</returns>
    public static IntPtr LoadLibrary(string fileName) => NativeMethods.LoadLibrary(fileName);

    /// <summary>Opens an existing local process object. See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms684320(v=vs.85).aspx">OpenProcess function</a>.</summary>
    /// <param name="desiredAccess">Process access rights.</param>
    /// <param name="inheritHandle">True to allow child processes to inherit the handle; otherwise, false.</param>
    /// <param name="processId">The identifier of the local process to be opened.</param>
    /// <returns>If the function succeeds, the return value is an open handle to the specified process.</returns>
    public static IntPtr OpenProcess(
        ProcessAccessRights desiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
        int processId) => _operations.OpenProcess(desiredAccess, inheritHandle, processId);

    /// <summary>See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa365461(v=vs.85).aspx">QueryDosDevice function</a>.</summary>
    /// <param name="deviceName">An MS-DOS device name string specifying the target of the query.</param>
    /// <param name="targetPath">A pointer to a buffer that receives the result of the query.</param>
    /// <param name="uuchMax">The maximum number of TCHARs that can be stored into the buffer pointed to by targetPath.</param>
    /// <returns>If the function succeeds, the return value is the number of TCHARs stored into the buffer pointed to by targetPath.</returns>
    public static unsafe int QueryDosDevice(string deviceName, char* targetPath, int uuchMax) =>
        _operations.QueryDosDevice(deviceName, targetPath, uuchMax);

    /// <summary>Retrieves the full name of the executable image for the specified process.</summary>
    /// <param name="processHandle">A handle to the process.</param>
    /// <param name="flags">
    /// This parameter can be one of the following values:
    ///     0 - The name should use the Win32 path format.
    /// PROCESS_NAME_NATIVE 0x00000001 - The name should use the native system path format.
    /// </param>
    /// <param name="executableName">The path to the executable image.</param>
    /// <param name="lpdwSize">On input, the buffer size; on success, the characters written.</param>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
    /// </returns>
    public static unsafe bool QueryFullProcessImageName(
        IntPtr processHandle,
        uint flags,
        char* executableName,
        ref int lpdwSize) =>
        _operations.QueryFullProcessImageName(processHandle, flags, executableName, ref lpdwSize);

    /// <summary>Allocates the specified number of bytes from the heap.</summary>
    /// <param name="globalMemorySettings">The memory allocation attributes.</param>
    /// <param name="bytes">The number of bytes to allocate.</param>
    /// <returns>
    /// If the function succeeds, the return value is a handle to the newly allocated memory object.
    /// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
    /// </returns>
    public static IntPtr GlobalAlloc(GlobalMemorySettings globalMemorySettings, UIntPtr bytes) =>
        NativeMethods.GlobalAlloc(globalMemorySettings, bytes);

    /// <summary>Locks a global memory object and returns a pointer to the first byte of the object's memory block.</summary>
    /// <param name="memoryHandle">IntPtr with a hGlobal, handle for a global memory blockk</param>
    /// <returns>A pointer to the first byte of the global memory block.</returns>
    public static IntPtr GlobalLock(IntPtr memoryHandle) => NativeMethods.GlobalLock(memoryHandle);

    /// <summary>Decrements the lock count associated with a movable global memory object.</summary>
    /// <param name="memoryHandle">IntPtr with a hGlobal, handle for a global memory block</param>
    /// <returns>bool if the unlock worked.</returns>
    public static bool GlobalUnlock(IntPtr memoryHandle) =>
        NativeMethods.GlobalUnlock(memoryHandle);

    /// <summary>Retrieves the current size of the specified global memory object, in bytes.</summary>
    /// <param name="memoryHandle">IntPtr with a hGlobal, handle for a global memory blockk</param>
    /// <returns>The memory object size.</returns>
    public static int GlobalSize(IntPtr memoryHandle) => NativeMethods.GlobalSize(memoryHandle);

    /// <summary>Retrieves the number of milliseconds that have elapsed since the system was started.</summary>
    /// <returns>The elapsed milliseconds since system startup.</returns>
    public static ulong GetTickCount64() => NativeMethods.GetTickCount64();

    /// <summary>Frees the specified local memory object and invalidates its handle.</summary>
    /// <param name="memoryHandle">IntPtr</param>
    /// <returns>The freed handle result.</returns>
    public static IntPtr LocalFree(IntPtr memoryHandle) => NativeMethods.LocalFree(memoryHandle);

    /// <summary>Change the last error.</summary>
    /// <param name="errorCode">The last error code to set.</param>
    public static void SetLastError(uint errorCode) => NativeMethods.SetLastError(errorCode);

    /// <summary>The OpenThread value.</summary>
    /// <param name="desiredAccess">ThreadAccess</param>
    /// <param name="inheritHandle">bool</param>
    /// <param name="threadId">uint</param>
    /// <returns>The thread handle.</returns>
    public static IntPtr OpenThread(
        ThreadAccess desiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
        uint threadId) => NativeMethods.OpenThread(desiredAccess, inheritHandle, threadId);

    /// <summary>See <a href="https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-suspendthread">Suspend thread</a>.</summary>
    /// <param name="threadHandle">IntPtr</param>
    /// <returns>The previous suspend count.</returns>
    public static uint SuspendThread(IntPtr threadHandle) =>
        NativeMethods.SuspendThread(threadHandle);

    /// <summary>The ResumeThread value.</summary>
    /// <param name="threadHandle">IntPtr</param>
    /// <returns>The previous suspend count.</returns>
    public static int ResumeThread(IntPtr threadHandle) => NativeMethods.ResumeThread(threadHandle);

    /// <summary>Gets the package full name for a process.</summary>
    /// <param name="processHandle">IntPtr</param>
    /// <param name="packageFullNameLength">The package full name buffer length.</param>
    /// <param name="fullName">StringBuilder to place the fullname in</param>
    /// <returns>Win32 result code.</returns>
    public static unsafe int GetPackageFullName(
        IntPtr processHandle,
        ref int packageFullNameLength,
        StringBuilder fullName)
    {
        Throw.IfNull(fullName);
        int capacity = fullName.Capacity;
        Span<char> span = (
            (capacity > MaxStackPackageFullNameLength)
                ? ((Span<char>)new char[capacity])
                : stackalloc char[capacity]);
        Span<char> buffer = span;
        fixed (char* bufferPointer = buffer)
        {
            int result = _operations.GetPackageFullName(
                processHandle,
                ref packageFullNameLength,
                bufferPointer);
            if (result != 0)
            {
                return result;
            }

            int length;
            for (length = 0; length < capacity && buffer[length] != 0; length = checked(length + 1))
            {
                // Advance to the terminating null character written by kernel32.
            }

            _ = fullName.Clear();
            _ = fullName.Append(bufferPointer, length);
            return result;
        }
    }

    /// <summary>Overrides Kernel32 operations for deterministic tests.</summary>
    /// <param name="operations">The replacement operations.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(Kernel32Operations operations)
    {
        Throw.IfNull(operations);
        Kernel32Operations previous = _operations;
        _operations = operations;
        return Scope.Create(previous, static value => _operations = value);
    }

    /// <summary>Creates a managed OS version value from an OSVERSIONINFOEXW byte buffer.</summary>
    /// <param name="versionInfo">The native OSVERSIONINFOEXW bytes.</param>
    /// <returns>The managed OS version value.</returns>
    internal static OsVersionInfoEx CreateOsVersionInfo(ReadOnlySpan<byte> versionInfo) =>
        new OsVersionInfoEx
        {
            MajorVersion = BinaryPrimitives.ReadInt32LittleEndian(versionInfo.Slice(MajorVersionOffset)),
            MinorVersion = BinaryPrimitives.ReadInt32LittleEndian(versionInfo.Slice(MinorVersionOffset)),
            BuildNumber = BinaryPrimitives.ReadInt32LittleEndian(versionInfo.Slice(BuildNumberOffset)),
            PlatformId = BinaryPrimitives.ReadInt32LittleEndian(versionInfo.Slice(PlatformIdOffset)),
            ServicePackVersion = NativeUtf16String.ReadNullTerminated(
                versionInfo.Slice(
                    ServicePackVersionOffset,
                    ServicePackVersionCharacterCapacity * Utf16CharacterSize)),
            ServicePackMajor = BinaryPrimitives.ReadInt16LittleEndian(versionInfo.Slice(ServicePackMajorOffset)),
            ServicePackMinor = BinaryPrimitives.ReadInt16LittleEndian(versionInfo.Slice(ServicePackMinorOffset)),
            SuiteMask = (WindowsSuites)
                BinaryPrimitives.ReadUInt16LittleEndian(versionInfo.Slice(SuiteMaskOffset)),
            ProductType = (WindowsProductTypes)versionInfo[ProductTypeOffset],
        };

    /// <summary>Gets a Win32 path from a DOS device path.</summary>
    /// <param name="dosPath">DOS device path.</param>
    /// <param name="pathBuffer">Reusable path buffer.</param>
    /// <param name="capacity">Reusable path buffer capacity.</param>
    /// <returns>Win32 path if resolved; otherwise, null.</returns>
    private static unsafe string GetProcessPathFromDosDevices(
        string dosPath,
        char* pathBuffer,
        int capacity)
    {
        string[] logicalDrives = _operations.GetLogicalDrives();
        foreach (string drive in logicalDrives)
        {
            int charCount = QueryDosDevice(drive.TrimEnd(DirectorySeparator), pathBuffer, capacity);
            if (charCount != 0)
            {
                string dosDevice = new(pathBuffer, 0, charCount);
                if (dosPath.StartsWith(dosDevice, StringComparison.Ordinal))
                {
                    return drive.TrimEnd(DirectorySeparator) + dosPath.Remove(0, charCount);
                }
            }
        }

        return null;
    }

    /// <summary>Native kernel32 entry points that need managed public wrappers.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>The Kernel32 DLL library name.</summary>
        private const string Kernel32Dll = "kernel32.dll";

        /// <summary>The loaded Kernel32 module.</summary>
        private static readonly IntPtr Kernel32Module = NativeLibrary.Load("kernel32.dll");

        /// <summary>The exported LocalFree function pointer.</summary>
        private static readonly IntPtr LocalFreePointer = NativeLibrary.GetExport(
            Kernel32Module,
            nameof(LocalFree));

        /// <summary>The exported SetLastError function pointer.</summary>
        private static readonly IntPtr SetLastErrorPointer = NativeLibrary.GetExport(
            Kernel32Module,
            nameof(SetLastError));

        /// <summary>Gets the package full name for a process.</summary>
        /// <param name="processHandle">Process handle.</param>
        /// <param name="packageFullNameLength">Package name buffer length.</param>
        /// <param name="fullName">Package name buffer.</param>
        /// <returns>Win32 result code.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern unsafe int GetPackageFullName(
            IntPtr processHandle,
            ref int packageFullNameLength,
            char* fullName);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern unsafe int GetPackageFullName(
            IntPtr processHandle,
            ref int packageFullNameLength,
            char* fullName);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static unsafe partial int GetPackageFullName(
            IntPtr processHandle,
            ref int packageFullNameLength,
            char* fullName);
#endif
#endif

        /// <summary>Specifies the default directories searched for DLLs.</summary>
        /// <param name="directoryFlags">Directory search flags.</param>
        /// <returns>True if the directory set was applied; otherwise, false.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetDefaultDllDirectories(DefaultDllDirectories directoryFlags);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetDefaultDllDirectories(DefaultDllDirectories directoryFlags);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetDefaultDllDirectories(DefaultDllDirectories directoryFlags);
#endif
#endif

        /// <summary>Adds a directory to the DLL search path.</summary>
        /// <param name="pathName">The path name.</param>
        /// <returns>True if the directory was set; otherwise, false.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetDllDirectory(string pathName);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetDllDirectory(string pathName);
#else
        [LibraryImport(
            "kernel32.dll",
            SetLastError = true,
            StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetDllDirectory(string pathName);
#endif
#endif

        /// <summary>Allocates a console.</summary>
        /// <returns>True if a console was allocated; otherwise, false.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AllocConsole();
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AllocConsole();
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool AllocConsole();
#endif
#endif

        /// <summary>Gets the current process id.</summary>
        /// <returns>The process id.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetCurrentProcessId();
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetCurrentProcessId();
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int GetCurrentProcessId();
#endif
#endif

        /// <summary>Gets the current thread id.</summary>
        /// <returns>The thread id.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetCurrentThreadId();
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetCurrentThreadId();
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int GetCurrentThreadId();
#endif
#endif

        /// <summary>Attaches the current process to a console.</summary>
        /// <param name="processId">The process id.</param>
        /// <returns>True if the console was attached; otherwise, false.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AttachConsole(uint processId);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AttachConsole(uint processId);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool AttachConsole(uint processId);
#endif
#endif

        /// <summary>Closes a native handle.</summary>
        /// <param name="objectHandle">The object handle.</param>
        /// <returns>True if the handle was closed; otherwise, false.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr objectHandle);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr objectHandle);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool CloseHandle(IntPtr objectHandle);
#endif
#endif

        /// <summary>Frees a loaded module.</summary>
        /// <param name="module">The module handle.</param>
        /// <returns>True if the module was freed; otherwise, false.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr module);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr module);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool FreeLibrary(IntPtr module);
#endif
#endif

        /// <summary>Gets a module handle.</summary>
        /// <param name="moduleName">The module name.</param>
        /// <returns>The module handle.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport(
            "kernel32.dll",
            EntryPoint = "GetModuleHandleW",
            SetLastError = true,
            CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetModuleHandle(string moduleName);
#else
#if NETFRAMEWORK
        [DllImport(
            "kernel32.dll",
            EntryPoint = "GetModuleHandleW",
            SetLastError = true,
            CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetModuleHandle(string moduleName);
#else
        [LibraryImport(
            "kernel32.dll",
            EntryPoint = "GetModuleHandleW",
            SetLastError = true,
            StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr GetModuleHandle(string moduleName);
#endif
#endif

        /// <summary>Gets Windows product information.</summary>
        /// <param name="operatingSystemMajorVersion">The operating system major version.</param>
        /// <param name="operatingSystemMinorVersion">The operating system minor version.</param>
        /// <param name="servicePackMajorVersion">The service pack major version.</param>
        /// <param name="servicePackMinorVersion">The service pack minor version.</param>
        /// <param name="edition">The Windows product edition.</param>
        /// <returns>True if product information was retrieved; otherwise, false.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetProductInfo(
            int operatingSystemMajorVersion,
            int operatingSystemMinorVersion,
            int servicePackMajorVersion,
            int servicePackMinorVersion,
            out WindowsProducts edition);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetProductInfo(
            int operatingSystemMajorVersion,
            int operatingSystemMinorVersion,
            int servicePackMajorVersion,
            int servicePackMinorVersion,
            out WindowsProducts edition);
#else
        [LibraryImport("kernel32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetProductInfo(
            int operatingSystemMajorVersion,
            int operatingSystemMinorVersion,
            int servicePackMajorVersion,
            int servicePackMinorVersion,
            out WindowsProducts edition);
#endif
#endif

        /// <summary>Gets version information.</summary>
        /// <param name="versionInfo">The version information.</param>
        /// <returns>True if version information was retrieved; otherwise, false.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", EntryPoint = "GetVersionExW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool GetVersionEx(void* versionInfo);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", EntryPoint = "GetVersionExW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool GetVersionEx(void* versionInfo);
#else
        [LibraryImport("kernel32.dll", EntryPoint = "GetVersionExW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static unsafe partial bool GetVersionEx(void* versionInfo);
#endif
#endif

        /// <summary>Loads a native library.</summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The module handle.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport(
            "kernel32.dll",
            EntryPoint = "LoadLibraryW",
            SetLastError = true,
            CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr LoadLibrary(string fileName);
#else
#if NETFRAMEWORK
        [DllImport(
            "kernel32.dll",
            EntryPoint = "LoadLibraryW",
            SetLastError = true,
            CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr LoadLibrary(string fileName);
#else
        [LibraryImport(
            "kernel32.dll",
            EntryPoint = "LoadLibraryW",
            SetLastError = true,
            StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr LoadLibrary(string fileName);
#endif
#endif

        /// <summary>Opens a process.</summary>
        /// <param name="desiredAccess">Desired access rights.</param>
        /// <param name="inheritHandle">True to inherit the handle; otherwise, false.</param>
        /// <param name="processId">The process id.</param>
        /// <returns>The process handle.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr OpenProcess(
            ProcessAccessRights desiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
            int processId);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr OpenProcess(
            ProcessAccessRights desiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
            int processId);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr OpenProcess(
            ProcessAccessRights desiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
            int processId);
#endif
#endif

        /// <summary>Queries a DOS device path.</summary>
        /// <param name="deviceName">The device name.</param>
        /// <param name="targetPath">The target path buffer.</param>
        /// <param name="maximumLength">The buffer capacity.</param>
        /// <returns>The number of characters copied.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport(
            "kernel32.dll",
            EntryPoint = "QueryDosDeviceW",
            SetLastError = true,
            CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern unsafe int QueryDosDevice(
            string deviceName,
            char* targetPath,
            int maximumLength);
#else
#if NETFRAMEWORK
        [DllImport(
            "kernel32.dll",
            EntryPoint = "QueryDosDeviceW",
            SetLastError = true,
            CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern unsafe int QueryDosDevice(
            string deviceName,
            char* targetPath,
            int maximumLength);
#else
        [LibraryImport(
            "kernel32.dll",
            EntryPoint = "QueryDosDeviceW",
            SetLastError = true,
            StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static unsafe partial int QueryDosDevice(
            string deviceName,
            char* targetPath,
            int maximumLength);
#endif
#endif

        /// <summary>Queries a process image name.</summary>
        /// <param name="processHandle">The process handle.</param>
        /// <param name="flags">Query flags.</param>
        /// <param name="exeName">The executable name buffer.</param>
        /// <param name="size">The buffer size.</param>
        /// <returns>True if the image name was retrieved; otherwise, false.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", EntryPoint = "QueryFullProcessImageNameW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool QueryFullProcessImageName(
            IntPtr processHandle,
            uint flags,
            char* exeName,
            ref int size);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", EntryPoint = "QueryFullProcessImageNameW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool QueryFullProcessImageName(
            IntPtr processHandle,
            uint flags,
            char* exeName,
            ref int size);
#else
        [LibraryImport(
            "kernel32.dll",
            EntryPoint = "QueryFullProcessImageNameW",
            SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static unsafe partial bool QueryFullProcessImageName(
            IntPtr processHandle,
            uint flags,
            char* exeName,
            ref int size);
#endif
#endif

        /// <summary>Allocates global memory.</summary>
        /// <param name="globalMemorySettings">Memory allocation attributes.</param>
        /// <param name="bytes">Bytes to allocate.</param>
        /// <returns>The memory handle.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GlobalAlloc(
            GlobalMemorySettings globalMemorySettings,
            UIntPtr bytes);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GlobalAlloc(
            GlobalMemorySettings globalMemorySettings,
            UIntPtr bytes);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr GlobalAlloc(
            GlobalMemorySettings globalMemorySettings,
            UIntPtr bytes);
#endif
#endif

        /// <summary>Locks global memory.</summary>
        /// <param name="memoryHandle">The memory handle.</param>
        /// <returns>The memory pointer.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GlobalLock(IntPtr memoryHandle);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GlobalLock(IntPtr memoryHandle);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr GlobalLock(IntPtr memoryHandle);
#endif
#endif

        /// <summary>Unlocks global memory.</summary>
        /// <param name="memoryHandle">The memory handle.</param>
        /// <returns>True if the memory was unlocked; otherwise, false.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GlobalUnlock(IntPtr memoryHandle);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GlobalUnlock(IntPtr memoryHandle);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GlobalUnlock(IntPtr memoryHandle);
#endif
#endif

        /// <summary>Gets the global memory size.</summary>
        /// <param name="memoryHandle">The memory handle.</param>
        /// <returns>The memory size.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GlobalSize(IntPtr memoryHandle);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GlobalSize(IntPtr memoryHandle);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int GlobalSize(IntPtr memoryHandle);
#endif
#endif

        /// <summary>Gets the system tick count.</summary>
        /// <returns>The elapsed milliseconds since startup.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern ulong GetTickCount64();
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern ulong GetTickCount64();
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial ulong GetTickCount64();
#endif
#endif

        /// <summary>Frees local memory.</summary>
        /// <param name="memoryHandle">The memory handle.</param>
        /// <returns>The freed memory handle result.</returns>
        internal static unsafe IntPtr LocalFree(IntPtr memoryHandle) =>
            ((delegate* unmanaged[Stdcall]<IntPtr, IntPtr>)(void*)LocalFreePointer)(memoryHandle);

        /// <summary>Sets the thread last-error value.</summary>
        /// <param name="errorCode">The error code.</param>
        internal static unsafe void SetLastError(uint errorCode) =>
            ((delegate* unmanaged[Stdcall]<uint, void>)(void*)SetLastErrorPointer)(errorCode);

        /// <summary>Opens a thread.</summary>
        /// <param name="desiredAccess">Desired access rights.</param>
        /// <param name="inheritHandle">True to inherit the handle; otherwise, false.</param>
        /// <param name="threadId">The thread id.</param>
        /// <returns>The thread handle.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr OpenThread(
            ThreadAccess desiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
            uint threadId);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr OpenThread(
            ThreadAccess desiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
            uint threadId);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr OpenThread(
            ThreadAccess desiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
            uint threadId);
#endif
#endif

        /// <summary>Suspends a thread.</summary>
        /// <param name="threadHandle">The thread handle.</param>
        /// <returns>The previous suspend count.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern uint SuspendThread(IntPtr threadHandle);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern uint SuspendThread(IntPtr threadHandle);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial uint SuspendThread(IntPtr threadHandle);
#endif
#endif

        /// <summary>Resumes a thread.</summary>
        /// <param name="threadHandle">The thread handle.</param>
        /// <returns>The previous suspend count.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int ResumeThread(IntPtr threadHandle);
#else
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int ResumeThread(IntPtr threadHandle);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int ResumeThread(IntPtr threadHandle);
#endif
#endif
    }
}
