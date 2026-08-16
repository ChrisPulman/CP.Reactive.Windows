// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;

/// <summary>
///     Any additional information about the system. This member can be one of the following values.
///     See
///     <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms724833(v=vs.85).aspx">OSVERSIONINFOEX structure</a>
/// </summary>
public enum WindowsProductTypes
{
    /// <summary>The operating system is Windows 8, Windows 7, Windows Vista, Windows XP Professional, Windows XP Home Edition, or Windows 2000 Professional.</summary>
    None,

    /// <summary>The operating system is Windows 8, Windows 7, Windows Vista, Windows XP Professional, Windows XP Home Edition, or Windows 2000 Professional.</summary>
    VER_NT_WORKSTATION,

    /// <summary>The system is a domain controller and the operating system is Windows Server 2012 , Windows Server 2008 R2, Windows Server 2008, Windows Server 2003, or Windows 2000 Server.</summary>
    VER_NT_DOMAIN_CONTROLLER,

    /// <summary>
    ///     The operating system is Windows Server 2012, Windows Server 2008 R2, Windows Server 2008, Windows Server 2003, or
    ///     Windows 2000 Server.
    ///     Note that a server that is also a domain controller is reported as VER_NT_DOMAIN_CONTROLLER, not VER_NT_SERVER.
    /// </summary>
    VER_NT_SERVER,
}
