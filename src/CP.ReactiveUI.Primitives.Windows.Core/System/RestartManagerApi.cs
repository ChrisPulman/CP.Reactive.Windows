// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Restart Manager API functionality See <a href="https://docs.microsoft.com/en-us/windows/win32/rstmgr/restart-manager-portal">Restart Manager</a>.</summary>
public static class RestartManagerApi
{
    /// <summary>The native application name character capacity.</summary>
    internal const int ApplicationNameCharacterCapacity = 256;

    /// <summary>The native application name byte offset.</summary>
    internal const int ApplicationNameOffset = 12;

    /// <summary>Native Restart Manager entry points.</summary>
    private static class NativeMethods
    {
        /// <summary>The Restart Manager DLL library name.</summary>
        private const string Rstrtmgr = "rstrtmgr.dll";

        /// <summary>The loaded Restart Manager module.</summary>
        private static readonly IntPtr RestartManagerModule = NativeLibrary.Load("rstrtmgr.dll");

        /// <summary>The exported RmRegisterResources function pointer.</summary>
        private static readonly IntPtr RmRegisterResourcesPointer = NativeLibrary.GetExport(RestartManagerModule, "RmRegisterResources");

        /// <summary>The exported RmGetList function pointer.</summary>
        private static readonly IntPtr RmGetListPointer = NativeLibrary.GetExport(RestartManagerModule, "RmGetList");

        /// <summary>The exported RmEndSession function pointer.</summary>
        private static readonly IntPtr RmEndSessionPointer = NativeLibrary.GetExport(RestartManagerModule, "RmEndSession");

        /// <summary>The exported RmShutdown function pointer.</summary>
        private static readonly IntPtr RmShutdownPointer = NativeLibrary.GetExport(RestartManagerModule, "RmShutdown");

        /// <summary>The exported RmRestart function pointer.</summary>
        private static readonly IntPtr RmRestartPointer = NativeLibrary.GetExport(RestartManagerModule, "RmRestart");

        /// <summary>Starts a Restart Manager session.</summary>
        /// <param name="sessionHandle">Restart Manager session handle.</param>
        /// <param name="sessionFlags">Reserved session flags.</param>
        /// <param name="sessionKey">Session key output buffer.</param>
        /// <returns>Win32 result code.</returns>
        [DllImport("rstrtmgr.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern unsafe int RmStartSession(out int sessionHandle, int sessionFlags, char* sessionKey);

        /// <summary>Ends a Restart Manager session.</summary>
        /// <param name="sessionHandle">Restart Manager session handle.</param>
        /// <returns>Win32 result code.</returns>
        internal static unsafe int RmEndSession(int sessionHandle) => ((delegate* unmanaged[Stdcall]<int, int>)(void*)RmEndSessionPointer)(sessionHandle);

        /// <summary>Registers resources with a Restart Manager session.</summary>
        /// <param name="sessionHandle">Restart Manager session handle.</param>
        /// <param name="fileCount">Number of file names.</param>
        /// <param name="rgsFilenames">File names.</param>
        /// <param name="applicationCount">Number of application records.</param>
        /// <param name="applications">Application records.</param>
        /// <param name="serviceCount">Number of service names.</param>
        /// <param name="rgsServiceNames">Service names.</param>
        /// <returns>Win32 result code.</returns>
        internal static unsafe int RmRegisterResources(int sessionHandle, uint fileCount, string[] rgsFilenames, uint applicationCount, RmUniqueProcess[] applications, uint serviceCount, string[] rgsServiceNames)
        {
            IntPtr[] filePointers = CreateStringPointers(rgsFilenames, fileCount);
            IntPtr[] servicePointers = CreateStringPointers(rgsServiceNames, serviceCount);
            try
            {
                fixed (IntPtr* files = filePointers)
                {
                    fixed (RmUniqueProcess* applicationPointer = applications)
                    {
                        fixed (IntPtr* services = servicePointers)
                        {
                            return ((delegate* unmanaged[Stdcall]<int, uint, char**, uint, RmUniqueProcess*, uint, char**, int>)(void*)RmRegisterResourcesPointer)(sessionHandle, fileCount, (char**)files, applicationCount, applicationPointer, serviceCount, (char**)services);
                        }
                    }
                }
            }
            finally
            {
                FreeStringPointers(filePointers);
                FreeStringPointers(servicePointers);
            }
        }

        /// <summary>Gets affected applications for a Restart Manager session.</summary>
        /// <param name="sessionHandle">Restart Manager session handle.</param>
        /// <param name="processInfoNeeded">Required process-info count.</param>
        /// <param name="processInfoCount">Supplied process-info count.</param>
        /// <param name="affectedApps">Affected applications buffer.</param>
        /// <param name="rebootReasons">Reboot reason flags.</param>
        /// <returns>Win32 result code.</returns>
        internal static unsafe int RmGetList(int sessionHandle, out uint processInfoNeeded, ref uint processInfoCount, RmProcessInfo[] affectedApps, out RmRebootReason rebootReasons)
        {
            byte[] nativeAffectedApps = ((affectedApps is null) ? null : new byte[checked(affectedApps.Length * 668)]);
            fixed (byte* affectedAppsPointer = nativeAffectedApps)
            {
                delegate* unmanaged[Stdcall]<int, uint*, uint*, void*, RmRebootReason*, int> getList = (delegate* unmanaged[Stdcall]<int, uint*, uint*, void*, RmRebootReason*, int>)(void*)RmGetListPointer;
                fixed (uint* needed = &processInfoNeeded)
                {
                    fixed (uint* count = &processInfoCount)
                    {
                        fixed (RmRebootReason* rebootReasonsPointer = &rebootReasons)
                        {
                            int result = getList(sessionHandle, needed, count, affectedAppsPointer, rebootReasonsPointer);
                            CopyAffectedApps(nativeAffectedApps, affectedApps, processInfoCount);
                            return result;
                        }
                    }
                }
            }
        }

        /// <summary>Shuts down affected applications.</summary>
        /// <param name="sessionHandle">Restart Manager session handle.</param>
        /// <param name="actionFlags">Shutdown flags.</param>
        /// <param name="status">Status callback pointer.</param>
        /// <returns>Win32 result code.</returns>
        internal static unsafe int RmShutdown(int sessionHandle, RmShutdownType actionFlags, IntPtr status) => ((delegate* unmanaged[Stdcall]<int, RmShutdownType, IntPtr, int>)(void*)RmShutdownPointer)(sessionHandle, actionFlags, status);

        /// <summary>Restarts affected applications.</summary>
        /// <param name="sessionHandle">Restart Manager session handle.</param>
        /// <param name="restartFlags">Reserved restart flags.</param>
        /// <param name="status">Status callback pointer.</param>
        /// <returns>Win32 result code.</returns>
        internal static unsafe int RmRestart(int sessionHandle, int restartFlags, IntPtr status) => ((delegate* unmanaged[Stdcall]<int, int, IntPtr, int>)(void*)RmRestartPointer)(sessionHandle, restartFlags, status);

        /// <summary>Creates native UTF-16 string pointers for a Restart Manager string array parameter.</summary>
        /// <param name="values">Managed strings.</param>
        /// <param name="count">Native string count.</param>
        /// <returns>Allocated UTF-16 string pointers.</returns>
        private static nint[] CreateStringPointers(string[] values, uint count)
        {
            if (count == 0)
            {
                return [];
            }

            IntPtr[] pointers = new IntPtr[count];
            for (int i = 0; i < pointers.Length; i = checked(i + 1))
            {
                pointers[i] = Marshal.StringToHGlobalUni(values[i]);
            }

            return pointers;
        }

        /// <summary>Frees native UTF-16 string pointers.</summary>
        /// <param name="pointers">Pointers to free.</param>
        private static void FreeStringPointers(nint[] pointers)
        {
            if (pointers is not null)
            {
                for (int i = 0; i < pointers.Length; i++)
                {
                    Marshal.FreeHGlobal(pointers[i]);
                }
            }
        }
    }

    /// <summary>The native application status byte offset.</summary>
    internal const int ApplicationStatusOffset = 656;

    /// <summary>The native application type byte offset.</summary>
    internal const int ApplicationTypeOffset = 652;

    /// <summary>The native RM_PROCESS_INFO byte size.</summary>
    internal const int NativeProcessInfoSize = 668;

    /// <summary>The native process identifier byte offset.</summary>
    internal const int ProcessIdOffset = 0;

    /// <summary>The native process start time high-value byte offset.</summary>
    internal const int ProcessStartTimeHighOffset = 8;

    /// <summary>The native process start time low-value byte offset.</summary>
    internal const int ProcessStartTimeLowOffset = 4;

    /// <summary>The native restartable flag byte offset.</summary>
    internal const int RestartableOffset = 664;

    /// <summary>The native service short name character capacity.</summary>
    internal const int ServiceShortNameCharacterCapacity = 64;

    /// <summary>The native service short name byte offset.</summary>
    internal const int ServiceShortNameOffset = 524;

    /// <summary>The native Terminal Services session id byte offset.</summary>
    internal const int TerminalServicesSessionIdOffset = 660;

    /// <summary>The native UTF-16 character byte size.</summary>
    internal const int Utf16CharacterSize = 2;

    /// <summary>The maximum session key length to allocate on the stack.</summary>
    private const int MaxStackSessionKeyLength = 33;

    /// <summary>Invalid session value.</summary>
    private const int RmInvalidSession = -1;

    /// <summary>Maximum length of a session key string (in characters).</summary>
    private const int RmSessionKeyLen = 32;

    /// <summary>Invalid Terminal Services session ID.</summary>
    private const uint RmInvalidTsSession = uint.MaxValue;

    /// <summary>Gets the maximum length of a session key string, in characters.</summary>
    public static int SessionKeyLength => 32;

    /// <summary>Gets the invalid session value.</summary>
    public static int InvalidSession => -1;

    /// <summary>Gets the invalid Terminal Services session ID.</summary>
    public static uint InvalidTerminalServicesSession => uint.MaxValue;

    /// <summary>
    ///     Starts a new Restart Manager session.
    ///     See <a href="https://docs.microsoft.com/en-us/windows/win32/api/restartmanager/nf-restartmanager-rmstartsession">RmStartSession function</a>
    /// </summary>
    /// <param name="sessionHandlePointer">
    ///     A pointer to the handle of a Restart Manager session.
    ///     The session handle can be passed in subsequent calls to the Restart Manager API.
    /// </param>
    /// <param name="sessionFlags">Reserved. This parameter should be 0.</param>
    /// <param name="strSessionKey">
    ///     A null-terminated string that contains the session key to the new session.
    ///     The string must be allocated before calling this function and should be at least CCH_RM_SESSION_KEY characters in length.
    /// </param>
    /// <returns>
    ///     Returns ERROR_SUCCESS (0) on success, or an error code on failure.
    /// </returns>
    public static unsafe int RmStartSession(out int sessionHandlePointer, int sessionFlags, StringBuilder strSessionKey)
    {
        Throw.IfNull(strSessionKey);
        int capacity = strSessionKey.Capacity;
        Span<char> span = ((capacity > 33) ? ((Span<char>)new char[capacity]) : stackalloc char[capacity]);
        Span<char> buffer = span;
        fixed (char* bufferPointer = buffer)
        {
            int result = NativeMethods.RmStartSession(out sessionHandlePointer, sessionFlags, bufferPointer);
            if (result != 0)
            {
                return result;
            }

            int length;
            for (length = 0; length < capacity && buffer[length] != 0; length = checked(length + 1))
            {
            }

            _ = strSessionKey.Clear();
            _ = strSessionKey.Append(bufferPointer, length);
            return result;
        }
    }

    /// <summary>
    ///     Ends the Restart Manager session.
    ///     This function should be called by the primary installer that has previously started the session by calling the RmStartSession function.
    ///     See <a href="https://docs.microsoft.com/en-us/windows/win32/api/restartmanager/nf-restartmanager-rmendsession">RmEndSession function</a>
    /// </summary>
    /// <param name="sessionHandle">A handle to an existing Restart Manager session.</param>
    /// <returns>
    ///     Returns ERROR_SUCCESS (0) on success, or an error code on failure.
    /// </returns>
    public static int RmEndSession(int sessionHandle) => NativeMethods.RmEndSession(sessionHandle);

    /// <summary>
    ///     Registers resources to a Restart Manager session.
    ///     The Restart Manager uses the list of resources registered with the session to determine which applications and services must be shut down and restarted.
    ///     See <a href="https://docs.microsoft.com/en-us/windows/win32/api/restartmanager/nf-restartmanager-rmregisterresources">RmRegisterResources function</a>
    /// </summary>
    /// <param name="sessionHandle">A handle to an existing Restart Manager session.</param>
    /// <param name="fileCount">The number of files being registered.</param>
    /// <param name="rgsFilenames">
    ///     An array of null-terminated strings of full filename paths.
    ///     This parameter can be NULL if fileCount is 0.
    /// </param>
    /// <param name="applicationCount">The number of processes being registered.</param>
    /// <param name="applications">
    ///     An array of RM_UNIQUE_PROCESS structures.
    ///     This parameter can be NULL if applicationCount is 0.
    /// </param>
    /// <param name="serviceCount">The number of services to be registered.</param>
    /// <param name="rgsServiceNames">
    ///     An array of null-terminated strings of service short names.
    ///     This parameter can be NULL if serviceCount is 0.
    /// </param>
    /// <returns>
    ///     Returns ERROR_SUCCESS (0) on success, or an error code on failure.
    /// </returns>
    public static int RmRegisterResources(int sessionHandle, uint fileCount, string[] rgsFilenames, uint applicationCount, RmUniqueProcess[] applications, uint serviceCount, string[] rgsServiceNames) => NativeMethods.RmRegisterResources(sessionHandle, fileCount, rgsFilenames, applicationCount, applications, serviceCount, rgsServiceNames);

    /// <summary>
    ///     Gets a list of all applications and services that are currently using resources that have been registered with the Restart Manager session.
    ///     See <a href="https://docs.microsoft.com/en-us/windows/win32/api/restartmanager/nf-restartmanager-rmgetlist">RmGetList function</a>
    /// </summary>
    /// <param name="sessionHandle">A handle to an existing Restart Manager session.</param>
    /// <param name="processInfoNeeded">
    ///     A pointer to an array size necessary to receive RM_PROCESS_INFO results.
    ///     If the buffer size is insufficient, this parameter receives the size required.
    /// </param>
    /// <param name="processInfoCount">
    ///     A pointer to the total number of RM_PROCESS_INFO structures that are returned to the caller.
    /// </param>
    /// <param name="affectedApplications">
    ///     An array of RM_PROCESS_INFO structures that list the applications and services using resources that have been registered with the session.
    ///     This parameter can be NULL if lpdwRebootReasons is not NULL.
    /// </param>
    /// <param name="lpdwRebootReasons">
    ///     Pointer to location that receives a value of the RM_REBOOT_REASON enumeration that describes the reason a system restart is needed.
    /// </param>
    /// <returns>
    ///     Returns ERROR_SUCCESS (0) on success.
    ///     Returns ERROR_MORE_DATA if the affectedApplications buffer is too small.
    ///     Returns an error code on other failures.
    /// </returns>
    public static int RmGetList(int sessionHandle, out uint processInfoNeeded, ref uint processInfoCount, [In][Out] RmProcessInfo[] affectedApplications, out RmRebootReason lpdwRebootReasons) => NativeMethods.RmGetList(sessionHandle, out processInfoNeeded, ref processInfoCount, affectedApplications, out lpdwRebootReasons);

    /// <summary>Initiates the shutdown of applications. See <a href="https://docs.microsoft.com/en-us/windows/win32/api/restartmanager/nf-restartmanager-rmshutdown">RmShutdown function</a>.</summary>
    /// <param name="sessionHandle">A handle to an existing Restart Manager session.</param>
    /// <param name="actionFlags">
    ///     One or more RM_SHUTDOWN_TYPE values that configure the shut down of components.
    /// </param>
    /// <param name="statusCallback">
    ///     A pointer to a status message callback function that is used to communicate status while the RmShutdown function is executing.
    ///     This parameter can be NULL if you do not want to receive status updates.
    /// </param>
    /// <returns>
    ///     Returns ERROR_SUCCESS (0) on success, or an error code on failure.
    /// </returns>
    public static int RmShutdown(int sessionHandle, RmShutdownType actionFlags, RmStatusCallback statusCallback) => NativeMethods.RmShutdown(sessionHandle, actionFlags, (statusCallback is null) ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(statusCallback));

    /// <summary>
    ///     Restarts applications and services that have been shut down by the RmShutdown function and that have been registered to be restarted using the RegisterApplicationRestart function.
    ///     See <a href="https://docs.microsoft.com/en-us/windows/win32/api/restartmanager/nf-restartmanager-rmrestart">RmRestart function</a>
    /// </summary>
    /// <param name="sessionHandle">A handle to an existing Restart Manager session.</param>
    /// <param name="restartFlags">Reserved. This parameter should be 0.</param>
    /// <param name="statusCallback">
    ///     A pointer to a status message callback function that is used to communicate status while the RmRestart function is executing.
    ///     This parameter can be NULL if you do not want to receive status updates.
    /// </param>
    /// <returns>
    ///     Returns ERROR_SUCCESS (0) on success, or an error code on failure.
    /// </returns>
    public static int RmRestart(int sessionHandle, int restartFlags, RmStatusCallback statusCallback) => NativeMethods.RmRestart(sessionHandle, restartFlags, (statusCallback is null) ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(statusCallback));

    /// <summary>Copies native process records into the public managed record shape.</summary>
    /// <param name="source">Native process record bytes.</param>
    /// <param name="destination">Managed destination records.</param>
    /// <param name="count">Number of records returned by the native call.</param>
    internal static void CopyAffectedApps(byte[] source, RmProcessInfo[] destination, uint count)
    {
        checked
        {
            if (source is not null && destination is not null)
            {
                int length = Math.Min((int)count, destination.Length);
                for (int i = 0; i < length; i++)
                {
                    destination[i] = ReadNativeProcessInfo(source.AsSpan(i * 668, 668));
                }
            }
        }
    }

    /// <summary>Reads a native RM_PROCESS_INFO record.</summary>
    /// <param name="source">The native record bytes.</param>
    /// <returns>The managed process record.</returns>
    internal static RmProcessInfo ReadNativeProcessInfo(ReadOnlySpan<byte> source)
    {
        System.Runtime.InteropServices.ComTypes.FILETIME processStartTime = new System.Runtime.InteropServices.ComTypes.FILETIME
        {
            dwLowDateTime = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(4)),
            dwHighDateTime = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(8))
        };
        return new(new RmUniqueProcess(BinaryPrimitives.ReadInt32LittleEndian(source.Slice(0)), processStartTime), NativeUtf16String.ReadNullTerminated(source.Slice(12, 512)), NativeUtf16String.ReadNullTerminated(source.Slice(524, 128)), (RmAppType)BinaryPrimitives.ReadInt32LittleEndian(source.Slice(652)), (RmAppStatus)BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(656)), BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(660)), BinaryPrimitives.ReadInt32LittleEndian(source.Slice(664)) != 0);
    }
}
