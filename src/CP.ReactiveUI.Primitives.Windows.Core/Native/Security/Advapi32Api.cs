// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Security.Principal;
using CP.ReactiveUI.Primitives.Windows.Native.Security.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Security.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Security;

/// <summary>Native methods to get registry or the login session information.</summary>
#if NETFRAMEWORK
public static class Advapi32Api
#else
public static partial class Advapi32Api
#endif
{
    /// <summary>The security group logon identifier flag.</summary>
    private const uint SeGroupLogonId = 3_221_225_472U;

    /// <summary>Advapi32 operations used by this process.</summary>
    private static Advapi32Operations _operations = new(
        NativeMethods.ConvertSidToStringSid,
        NativeMethods.RegNotifyChangeKeyValue,
        NativeMethods.RegOpenKeyEx,
        NativeMethods.GetTokenInformation);

    /// <summary>Gets the current Session-ID SID.</summary>
    /// <returns>SessionId as SID.</returns>
    public static string CurrentSessionId
    {
        get
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            return GetCurrentSessionId(identity.Token);
        }
    }

    /// <summary>
    /// See more about <a href="https://docs.microsoft.com/en-us/windows/desktop/api/sddl/nf-sddl-convertsidtostringsida">ConvertSidToStringSidA function</a>
    /// The ConvertSidToStringSid function converts a security identifier (SID) to a string format suitable for display, storage, or transmission.
    /// </summary>
    /// <param name="sid">Security identifier pointer.</param>
    /// <param name="sidString">Allocated string pointer.</param>
    /// <returns>True if the SID was converted; otherwise, false.</returns>
    internal static bool ConvertSidToStringSid(IntPtr sid, out IntPtr sidString) =>
        _operations.ConvertSidToStringSid(sid, out sidString);

    /// <summary>Notifies the caller about changes to the attributes or contents of a specified registry key.</summary>
    /// <param name="key">A handle to an open registry key.</param>
    /// <param name="watchSubtree">True to report changes in the specified key and its subkeys.</param>
    /// <param name="notifyFilter">A value that indicates the changes that should be reported.</param>
    /// <param name="eventHandle">Event handle to signal.</param>
    /// <param name="asynchronous">True to return immediately and report changes by signaling the specified event.</param>
    /// <returns>Win32 result code.</returns>
    internal static int RegNotifyChangeKeyValue(
        SafeRegistryHandle key,
        bool watchSubtree,
        RegistryNotifyFilter notifyFilter,
        SafeWaitHandle eventHandle,
        bool asynchronous) =>
        _operations.RegNotifyChangeKeyValue(
            key,
            watchSubtree,
            notifyFilter,
            eventHandle,
            asynchronous);

    /// <summary>Opens the specified registry key.</summary>
    /// <param name="key">A handle to an open registry key.</param>
    /// <param name="subKey">The name of the registry subkey to be opened.</param>
    /// <param name="options">Registry open options.</param>
    /// <param name="desiredAccess">Requested registry key access rights.</param>
    /// <param name="openedKey">The opened registry key.</param>
    /// <returns>Win32 result code.</returns>
    internal static int RegOpenKeyEx(
        IntPtr key,
        string subKey,
        RegistryOpenOptions options,
        RegistryKeySecurityAccessRights desiredAccess,
        out SafeRegistryHandle openedKey) => _operations.RegOpenKeyEx(key, subKey, options, desiredAccess, out openedKey);

    /// <summary>Overrides Advapi32 operations for deterministic tests.</summary>
    /// <param name="convertSidToStringSid">The replacement SID conversion operation.</param>
    /// <param name="regNotifyChangeKeyValue">The replacement registry notification operation.</param>
    /// <param name="regOpenKey">The replacement registry open operation.</param>
    /// <param name="getTokenInformation">The replacement token information operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(
        ConvertSidToStringSidOperation convertSidToStringSid,
        RegNotifyChangeKeyValueOperation regNotifyChangeKeyValue,
        RegOpenKeyOperation regOpenKey,
        GetTokenInformationOperation getTokenInformation)
    {
        Throw.IfNull(convertSidToStringSid);
        Throw.IfNull(regNotifyChangeKeyValue);
        Throw.IfNull(regOpenKey);
        Throw.IfNull(getTokenInformation);
        Advapi32Operations operations = _operations;
        _operations = new(
            convertSidToStringSid,
            regNotifyChangeKeyValue,
            regOpenKey,
            getTokenInformation);
        return Scope.Create(operations, static previous => _operations = previous);
    }

    /// <summary>Gets the current session id for an access token.</summary>
    /// <param name="token">Access token handle.</param>
    /// <returns>SessionId as SID.</returns>
    internal static string GetCurrentSessionId(IntPtr token)
    {
        int tokenInfLength = 0;
        _ = GetTokenInformation(
            token,
            TokenInformationClasses.TokenGroups,
            IntPtr.Zero,
            tokenInfLength,
            out tokenInfLength);
        IntPtr tokenInformation = Marshal.AllocHGlobal(tokenInfLength);
        checked
        {
            try
            {
                if (
                    !GetTokenInformation(
                        token,
                        TokenInformationClasses.TokenGroups,
                        tokenInformation,
                        tokenInfLength,
                        out tokenInfLength))
                {
                    return string.Empty;
                }

                string retVal = string.Empty;
                int groupCount = Marshal.ReadInt32(tokenInformation);
                int sidAndAttrSize = Marshal.SizeOf<SidAndAttributes>();
                for (int i = 0; i < groupCount; i++)
                {
                    SidAndAttributes sidAndAttributes = Marshal.PtrToStructure<SidAndAttributes>(
                        IntPtr.Add(tokenInformation, IntPtr.Size + (i * sidAndAttrSize)));
                    if (sidAndAttributes.HasAttributes(SeGroupLogonId))
                    {
                        retVal = sidAndAttributes.ToSidString();
                        break;
                    }
                }

                return retVal;
            }
            finally
            {
                Marshal.FreeHGlobal(tokenInformation);
            }
        }
    }

    /// <summary>Retrieves a specified type of information about an access token.</summary>
    /// <param name="tokenHandle">A handle to an access token from which information is retrieved.</param>
    /// <param name="tokenInformationClasses">The token information class to retrieve.</param>
    /// <param name="tokenInformation">A pointer to a buffer the function fills with the requested information.</param>
    /// <param name="tokenInformationLength">The size, in bytes, of the token information buffer.</param>
    /// <param name="returnLength">The number of bytes needed for the token information buffer.</param>
    /// <returns>If the function succeeds, the return value is nonzero.</returns>
    private static bool GetTokenInformation(
        IntPtr tokenHandle,
        TokenInformationClasses tokenInformationClasses,
        IntPtr tokenInformation,
        int tokenInformationLength,
        out int returnLength) =>
        _operations.GetTokenInformation(
            tokenHandle,
            tokenInformationClasses,
            tokenInformation,
            tokenInformationLength,
            out returnLength);

    /// <summary>Native advapi32 entry points.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>The Advapi32 DLL library name.</summary>
        private const string Advapi32Dll = "advapi32.dll";

        /// <summary>Converts a security identifier to string form.</summary>
        /// <param name="sid">Security identifier pointer.</param>
        /// <param name="ptrSid">Allocated string pointer.</param>
        /// <returns>True on success; otherwise, false.</returns>
#if NETFRAMEWORK
        [DllImport(Advapi32Dll, EntryPoint = "ConvertSidToStringSidW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ConvertSidToStringSid(IntPtr sid, out IntPtr ptrSid);
#else
        [LibraryImport(Advapi32Dll, EntryPoint = "ConvertSidToStringSidW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool ConvertSidToStringSid(IntPtr sid, out IntPtr ptrSid);
#endif

        /// <summary>Retrieves information about an access token.</summary>
        /// <param name="tokenHandle">Access token handle.</param>
        /// <param name="tokenInformationClasses">Token information class.</param>
        /// <param name="tokenInformation">Output token information buffer.</param>
        /// <param name="tokenInformationLength">Output token information buffer length.</param>
        /// <param name="returnLength">Required or written buffer length.</param>
        /// <returns>True on success; otherwise, false.</returns>
#if NETFRAMEWORK
        [DllImport(Advapi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetTokenInformation(
            IntPtr tokenHandle,
            TokenInformationClasses tokenInformationClasses,
            IntPtr tokenInformation,
            int tokenInformationLength,
            out int returnLength);
#else
        [LibraryImport(Advapi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetTokenInformation(
            IntPtr tokenHandle,
            TokenInformationClasses tokenInformationClasses,
            IntPtr tokenInformation,
            int tokenInformationLength,
            out int returnLength);
#endif

        /// <summary>Registers for registry key change notifications.</summary>
        /// <param name="key">Registry key handle.</param>
        /// <param name="watchSubtree">Whether subkeys are watched.</param>
        /// <param name="notifyFilter">Notification filter.</param>
        /// <param name="eventHandle">Event handle.</param>
        /// <param name="asynchronous">Whether notification is asynchronous.</param>
        /// <returns>Win32 result code.</returns>
#if NETFRAMEWORK
        [DllImport(Advapi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int RegNotifyChangeKeyValue(
            SafeRegistryHandle key,
            [MarshalAs(UnmanagedType.Bool)] bool watchSubtree,
            RegistryNotifyFilter notifyFilter,
            SafeWaitHandle eventHandle,
            [MarshalAs(UnmanagedType.Bool)] bool asynchronous);
#else
        [LibraryImport(Advapi32Dll, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int RegNotifyChangeKeyValue(
            SafeRegistryHandle key,
            [MarshalAs(UnmanagedType.Bool)] bool watchSubtree,
            RegistryNotifyFilter notifyFilter,
            SafeWaitHandle eventHandle,
            [MarshalAs(UnmanagedType.Bool)] bool asynchronous);
#endif

        /// <summary>Opens a registry key.</summary>
        /// <param name="parentKeyHandle">Parent registry key handle.</param>
        /// <param name="subKey">Subkey name.</param>
        /// <param name="options">Open options.</param>
        /// <param name="desiredAccess">Requested access rights.</param>
        /// <param name="openedKey">Opened registry key handle.</param>
        /// <returns>Win32 result code.</returns>
#if NETFRAMEWORK
        [DllImport(
            Advapi32Dll,
            EntryPoint = "RegOpenKeyExW",
            SetLastError = true,
            CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int RegOpenKeyEx(
            IntPtr parentKeyHandle,
            string subKey,
            RegistryOpenOptions options,
            RegistryKeySecurityAccessRights desiredAccess,
            out SafeRegistryHandle openedKey);
#else
        [LibraryImport(
            Advapi32Dll,
            EntryPoint = "RegOpenKeyExW",
            SetLastError = true,
            StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int RegOpenKeyEx(
            IntPtr parentKeyHandle,
            string subKey,
            RegistryOpenOptions options,
            RegistryKeySecurityAccessRights desiredAccess,
            out SafeRegistryHandle openedKey);
#endif
    }

    /// <summary>Composes Advapi32 operations without invoking them during construction.</summary>
    /// <param name="convertSidToStringSid">The SID conversion operation.</param>
    /// <param name="regNotifyChangeKeyValue">The registry notification operation.</param>
    /// <param name="regOpenKey">The registry open operation.</param>
    /// <param name="getTokenInformation">The token information operation.</param>
    private sealed class Advapi32Operations(
        ConvertSidToStringSidOperation convertSidToStringSid,
        RegNotifyChangeKeyValueOperation regNotifyChangeKeyValue,
        RegOpenKeyOperation regOpenKey,
        GetTokenInformationOperation getTokenInformation)
    {
        /// <summary>Invokes the configured SID conversion operation.</summary>
        /// <param name="sid">The SID pointer.</param>
        /// <param name="sidString">The allocated SID string pointer.</param>
        /// <returns>The configured operation result.</returns>
        public bool ConvertSidToStringSid(IntPtr sid, out IntPtr sidString) =>
            convertSidToStringSid(sid, out sidString);

        /// <summary>Invokes the configured registry notification operation.</summary>
        /// <param name="key">The registry key handle.</param>
        /// <param name="watchSubtree">Whether subkeys are watched.</param>
        /// <param name="notifyFilter">The notification filter.</param>
        /// <param name="eventHandle">The event handle.</param>
        /// <param name="asynchronous">Whether notification is asynchronous.</param>
        /// <returns>The configured operation result.</returns>
        public int RegNotifyChangeKeyValue(
            SafeRegistryHandle key,
            bool watchSubtree,
            RegistryNotifyFilter notifyFilter,
            SafeWaitHandle eventHandle,
            bool asynchronous) => regNotifyChangeKeyValue(key, watchSubtree, notifyFilter, eventHandle, asynchronous);

        /// <summary>Invokes the configured registry open operation.</summary>
        /// <param name="key">The parent registry key handle.</param>
        /// <param name="subKey">The subkey name.</param>
        /// <param name="options">The registry open options.</param>
        /// <param name="desiredAccess">The requested access rights.</param>
        /// <param name="openedKey">The opened registry key handle.</param>
        /// <returns>The configured operation result.</returns>
        public int RegOpenKeyEx(
            IntPtr key,
            string subKey,
            RegistryOpenOptions options,
            RegistryKeySecurityAccessRights desiredAccess,
            out SafeRegistryHandle openedKey) => regOpenKey(key, subKey, options, desiredAccess, out openedKey);

        /// <summary>Invokes the configured token information operation.</summary>
        /// <param name="tokenHandle">The access token handle.</param>
        /// <param name="tokenInformationClasses">The token information class.</param>
        /// <param name="tokenInformation">The output token information buffer.</param>
        /// <param name="tokenInformationLength">The output token information buffer length.</param>
        /// <param name="returnLength">The required or written buffer length.</param>
        /// <returns>The configured operation result.</returns>
        public bool GetTokenInformation(
            IntPtr tokenHandle,
            TokenInformationClasses tokenInformationClasses,
            IntPtr tokenInformation,
            int tokenInformationLength,
            out int returnLength) =>
            getTokenInformation(
                tokenHandle,
                tokenInformationClasses,
                tokenInformation,
                tokenInformationLength,
                out returnLength);
    }
}
