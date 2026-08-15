// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;

/// <summary>
///     The directories to search. This parameter can be any combination of the following values.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/hh310515(v=vs.85).aspx">SetDefaultDllDirectories function</a>
/// </summary>
[Flags]
public enum DefaultDllDirectories
{
    /// <summary>If this value is used, the application's installation directory is searched.</summary>
    None = 0,

    /// <summary>If this value is used, the application's installation directory is searched.</summary>
    SearchApplicationDirectory = 0x200,

    /// <summary>If this value is used, any path explicitly added using the AddDllDirectory or SetDllDirectory function is searched.</summary>
    SearchUserDirectories = 0x400,

    /// <summary>If this value is used, %windows%\system32 is searched.</summary>
    SearchSystem32Directory = 0x800,

    /// <summary>
    /// This value is a combination of LOAD_LIBRARY_SEARCH_APPLICATION_DIR, LOAD_LIBRARY_SEARCH_SYSTEM32, and LOAD_LIBRARY_SEARCH_USER_DIRS.
    /// This value represents the recommended maximum number of directories an application should include in its DLL search path.
    /// </summary>
    SearchDefaultDirectories = 0x1000,
}
