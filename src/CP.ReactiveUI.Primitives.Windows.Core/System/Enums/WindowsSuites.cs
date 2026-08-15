// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;

/// <summary>
///     A bit mask that identifies the product suites available on the system. This member can be a combination of the
///     following values.
///     See
///     <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms724833(v=vs.85).aspx">OSVERSIONINFOEX structure</a>
/// </summary>
[Flags]
public enum WindowsSuites
{
    /// <summary>Microsoft Small Business Server was once installed on the system..</summary>
    None = 0,
    /// <summary>Microsoft Small Business Server was once installed on the system..</summary>
    [Description("Microsoft Small Business Server was once installed on the system.")]
    SmallBusiness = 1,
    /// <summary>Enterprise Edition, or Advanced Server is installed..</summary>
    [Description("Enterprise Edition, or Advanced Server is installed.")]
    Enterprise = 2,
    /// <summary>Microsoft BackOffice components are installed..</summary>
    [Description("Microsoft BackOffice components are installed.")]
    BackOffice = 4,
    /// <summary>The CommunicationServer value.</summary>
    [Description("CommunicationServer")]
    CommunicationServer = 8,
    /// <summary>Terminal Services is installed..</summary>
    [Description("Terminal Services is installed.")]
    TerminalServer = 0x10,
    /// <summary>Microsoft Small Business Server is installed with the restrictive client license in force..</summary>
    [Description("Microsoft Small Business Server is installed with the restrictive client license in force.")]
    SmallBusinessRestricted = 0x20,
    /// <summary>Windows XP Embedded is installed..</summary>
    [Description("Windows XP Embedded is installed.")]
    EmbeddedNT = 0x40,
    /// <summary>Windows Server 2008 Datacenter, Windows Server 2003, Datacenter Edition, or Windows 2000 Datacenter Server is installed..</summary>
    [Description("Windows Server 2008 Datacenter, Windows Server 2003, Datacenter Edition, or Windows 2000 Datacenter Server is installed.")]
    DataCenter = 0x80,
    /// <summary>Remote Desktop is supported, but only one interactive session is supported..</summary>
    [Description("Remote Desktop is supported, but only one interactive session is supported.")]
    SingleUserTS = 0x100,
    /// <summary>Home Edition is installed.</summary>
    [Description(" Home Edition is installed")]
    Personal = 0x200,
    /// <summary>Web Edition is installed..</summary>
    [Description("Web Edition is installed.")]
    Blade = 0x400,
    /// <summary>Embedded 'restricted'..</summary>
    [Description("Embedded 'restricted'.")]
    EmbeddedRestricted = 0x800,
    /// <summary>Security appliance.</summary>
    [Description("Security appliance")]
    SecurityAppliance = 0x1000,
    /// <summary>Storage Server is installed.</summary>
    [Description("Storage Server is installed")]
    StorageServer = 0x2000,
    /// <summary>Windows Server 2003, Compute Cluster Edition is installed..</summary>
    [Description("Windows Server 2003, Compute Cluster Edition is installed.")]
    ComputeServer = 0x4000,
    /// <summary>Windows Home Server is installed..</summary>
    [Description("Windows Home Server is installed.")]
    WHServer = 0x8000
}
