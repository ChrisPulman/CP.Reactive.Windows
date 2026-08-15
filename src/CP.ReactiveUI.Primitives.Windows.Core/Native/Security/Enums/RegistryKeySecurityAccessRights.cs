// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Security.Enums;

/// <summary>Used by RegOpenKeyEx.</summary>
[Flags]
public enum RegistryKeySecurityAccessRights
{
    /// <summary>Required to query the values of a registry key.</summary>
    None = 0,

    /// <summary>Required to query the values of a registry key.</summary>
    QueryValue = 1,

    /// <summary>Required to create, delete, or set a registry value.</summary>
    SetValue = 2,

    /// <summary>Required to create a subkey of a registry key.</summary>
    CreateSubKey = 4,

    /// <summary>Required to enumerate the subkeys of a registry key.</summary>
    EnumerateSubKeys = 8,

    /// <summary>Required to request change notifications for a registry key or for subkeys of a registry key.</summary>
    Notify = 0x10,

    /// <summary>Reserved for system use.</summary>
    CreateLink = 0x20,

    /// <summary>Required to delete an object.</summary>
    Delete = 0x10000,

    /// <summary>Indicates that an application on 64-bit Windows should operate on the 32-bit registry view.</summary>
    WoW6432 = 0x200,

    /// <summary>Indicates that an application on 64-bit Windows should operate on the 64-bit registry view.</summary>
    Wow6464 = 0x100,

    /// <summary>Required to read security descriptor information.</summary>
    ReadControl = 0x20000,

    /// <summary>Required to modify the discretionary access control list.</summary>
    WriteDac = 0x40000,

    /// <summary>Required to modify the owner in the security descriptor.</summary>
    WriteOwner = 0x80000,

    /// <summary>Required to use the object for synchronization.</summary>
    Synchronize = 0x100000,
}
