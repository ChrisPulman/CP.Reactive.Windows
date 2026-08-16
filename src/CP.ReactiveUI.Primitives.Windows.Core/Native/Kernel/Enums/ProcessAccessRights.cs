// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;

/// <summary>Process security and access rights.</summary>
[Flags]
public enum ProcessAccessRights : uint
{
    /// <summary>No process access rights.</summary>
    None = 0U,

    /// <summary>Enables usage of the process handle in the TerminateProcess function to terminate the process.</summary>
    Terminate = 1U,

    /// <summary>Enables usage of the process handle in the CreateRemoteThread function to create a thread in the process.</summary>
    CreateThread = 2U,

    /// <summary>Enables usage of the process handle in the SetProcessAffinityUpdateMode function to set session information.</summary>
    SetSessionId = 4U,

    /// <summary>Enables usage of the process handle in the VirtualProtectEx and WriteProcessMemory functions to modify the virtual memory of the process.</summary>
    VirtualMemoryOperation = 8U,

    /// <summary>Enables usage of the process handle in the ReadProcessMemory function to' read from the virtual memory of the process.</summary>
    VirtualMemoryRead = 0x10U,

    /// <summary>Enables usage of the process handle in the WriteProcessMemory function to write to the virtual memory of the process.</summary>
    VirtualMemoryWrite = 0x20U,

    /// <summary>Enables usage of the process handle as either the source or target process in the DuplicateHandle function to duplicate a handle.</summary>
    DuplicateHandle = 0x40U,

    /// <summary>Enables usage of the process handle in the CreateProcess function as a parent process.</summary>
    CreateProcess = 0x80U,

    /// <summary>Enables usage of the process handle in quota-management functions.</summary>
    SetQuota = 0x100U,

    /// <summary>Enables usage of the process handle in the SetPriorityClass function to set the priority class of the process.</summary>
    SetInformation = 0x200U,

    /// <summary>Enables usage of the process handle in the GetExitCodeProcess and GetPriorityClass functions to read information from the process object.</summary>
    QueryInformation = 0x400U,

    /// <summary>Enables usage of the process handle in the SuspendThread and ResumeThread functions.</summary>
    SuspendResume = 0x800U,

    /// <summary>Required to retrieve certain limited information about a process.</summary>
    QueryLimitedInformation = 0x1000U,

    /// <summary>Required to delete the object.</summary>
    Delete = 0x10000U,

    /// <summary>Required to read information in the security descriptor for the object.</summary>
    ReadControl = 0x20000U,

    /// <summary>Required to modify the discretionary access control list in the security descriptor for the object.</summary>
    WriteDac = 0x40000U,

    /// <summary>Required to change the owner in the security descriptor for the object.</summary>
    WriteOwner = 0x80000U,

    /// <summary>The right to use the object for synchronization. This enables a thread to wait until the object is in the signaled state.</summary>
    Synchronize = 0x100000U,

    /// <summary>Combined value for all process access rights exposed by this enum.</summary>
    All =
        Terminate
        | CreateThread
        | SetSessionId
        | VirtualMemoryOperation
        | VirtualMemoryRead
        | VirtualMemoryWrite
        | DuplicateHandle
        | CreateProcess
        | SetQuota
        | SetInformation
        | QueryInformation
        | SuspendResume
        | QueryLimitedInformation
        | Delete
        | ReadControl
        | WriteDac
        | WriteOwner
        | Synchronize,
}
