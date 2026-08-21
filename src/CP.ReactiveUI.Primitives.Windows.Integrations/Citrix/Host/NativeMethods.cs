// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Host;

/// <summary>Native methods used by Citrix host-session integration adapters.</summary>
#if NETFRAMEWORK
internal static class NativeMethods
#else
internal static partial class NativeMethods
#endif
{
    /// <summary>Loads a native library and its dependencies from controlled locations.</summary>
    /// <param name="fileName">The absolute library path.</param>
    /// <param name="fileHandle">Reserved; must be zero.</param>
    /// <param name="flags">The library search flags.</param>
    /// <returns>The module handle, or zero on failure.</returns>
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
#if NETFRAMEWORK
    [DllImport("kernel32", EntryPoint = "LoadLibraryExW", ExactSpelling = true, SetLastError = true)]
#else
    [LibraryImport("kernel32", EntryPoint = "LoadLibraryExW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
#endif
#if NETFRAMEWORK
    internal static extern IntPtr LoadLibraryEx(
        [MarshalAs(UnmanagedType.LPWStr)] string fileName,
        IntPtr fileHandle,
        uint flags);
#else
    internal static partial IntPtr LoadLibraryEx(
        string fileName,
        IntPtr fileHandle,
        uint flags);
#endif

#if NETFRAMEWORK
    /// <summary>Resolves an exported procedure.</summary>
    /// <param name="module">The native module.</param>
    /// <param name="procedureName">The exported procedure name.</param>
    /// <returns>The procedure address, or zero when it is unavailable.</returns>
    internal static IntPtr GetProcAddress(IntPtr module, string procedureName) =>
        GetProcAddressNative(module, System.Text.Encoding.ASCII.GetBytes(procedureName + '\0'));
#else
    /// <summary>Resolves an exported procedure.</summary>
    /// <param name="module">The native module.</param>
    /// <param name="procedureName">The exported procedure name.</param>
    /// <returns>The procedure address, or zero when it is unavailable.</returns>
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("kernel32", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
    internal static partial IntPtr GetProcAddress(
        IntPtr module,
        string procedureName);
#endif

    /// <summary>Releases a native module.</summary>
    /// <param name="module">The native module.</param>
    /// <returns><see langword="true"/> on success.</returns>
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
#if NETFRAMEWORK
    [DllImport("kernel32", ExactSpelling = true, SetLastError = true)]
#else
    [LibraryImport("kernel32", SetLastError = true)]
#endif
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static
#if NETFRAMEWORK
        extern
#else
        partial
#endif
        bool FreeLibrary(
        IntPtr module);

    /// <summary>Registers a window to receive session change notifications.</summary>
    /// <param name="windowHandle">The window handle to register.</param>
    /// <param name="flags">The notification flags.</param>
    /// <returns><see langword="true"/> when the registration succeeds.</returns>
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
#if NETFRAMEWORK
    [DllImport("wtsapi32", ExactSpelling = true, SetLastError = true)]
#else
    [LibraryImport("wtsapi32", SetLastError = true)]
#endif
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static
#if NETFRAMEWORK
        extern
#else
        partial
#endif
        bool WTSRegisterSessionNotification(
        IntPtr windowHandle,
        int flags);

    /// <summary>Unregisters a window from session change notifications.</summary>
    /// <param name="windowHandle">The window handle to unregister.</param>
    /// <returns><see langword="true"/> when the unregistration succeeds.</returns>
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
#if NETFRAMEWORK
    [DllImport("wtsapi32", ExactSpelling = true, SetLastError = true)]
#else
    [LibraryImport("wtsapi32", SetLastError = true)]
#endif
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static
#if NETFRAMEWORK
        extern
#else
        partial
#endif
        bool WTSUnRegisterSessionNotification(
        IntPtr windowHandle);

#if NETFRAMEWORK
    /// <summary>Resolves an exported procedure from a null-terminated ANSI name.</summary>
    /// <param name="module">The native module.</param>
    /// <param name="procedureName">The null-terminated exported procedure name.</param>
    /// <returns>The procedure address, or zero when it is unavailable.</returns>
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("kernel32", EntryPoint = "GetProcAddress", ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr GetProcAddressNative(
        IntPtr module,
        byte[] procedureName);
#endif
}
