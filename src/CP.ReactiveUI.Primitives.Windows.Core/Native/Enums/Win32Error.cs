// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Enums;

/// <summary>A Win32 error code.</summary>
public enum Win32Error : uint
{
    /// <summary>The operation completed successfully.</summary>
    Success = 0U,
    /// <summary>The function is invalid.</summary>
    InvalidFunction = 1U,
    /// <summary>The file was not found.</summary>
    FileNotFound = 2U,
    /// <summary>The path was not found.</summary>
    PathNotFound = 3U,
    /// <summary>Too many files are open.</summary>
    TooManyOpenFiles = 4U,
    /// <summary>Access is denied.</summary>
    AccessDenied = 5U,
    /// <summary>The handle is invalid.</summary>
    InvalidHandle = 6U,
    /// <summary>The storage control blocks were destroyed.</summary>
    ArenaTrashed = 7U,
    /// <summary>There is not enough memory to complete the operation.</summary>
    NotEnoughMemory = 8U,
    /// <summary>The storage control block address is invalid.</summary>
    InvalidBlock = 9U,
    /// <summary>The environment is incorrect.</summary>
    BadEnvironment = 10U,
    /// <summary>An attempt was made to load a program with an incorrect format.</summary>
    BadFormat = 11U,
    /// <summary>The access code is invalid.</summary>
    InvalidAccess = 12U,
    /// <summary>The data is invalid.</summary>
    InvalidData = 13U,
    /// <summary>Not enough storage is available to complete the operation.</summary>
    OutOfMemory = 14U,
    /// <summary>The system cannot find the drive specified.</summary>
    InvalidDrive = 15U,
    /// <summary>The directory cannot be removed.</summary>
    CurrentDirectory = 16U,
    /// <summary>The system cannot move the file to a different disk drive.</summary>
    NotSameDevice = 17U,
    /// <summary>There are no more files.</summary>
    NoMoreFiles = 18U,
    /// <summary>The media is write protected.</summary>
    WriteProtect = 19U,
    /// <summary>The system cannot find the device specified.</summary>
    BadUnit = 20U,
    /// <summary>The device is not ready.</summary>
    NotReady = 21U,
    /// <summary>The device does not recognize the command.</summary>
    BadCommand = 22U,
    /// <summary>Data error caused by cyclic redundancy check.</summary>
    Crc = 23U,
    /// <summary>The program issued a command but the command length is incorrect.</summary>
    BadLength = 24U,
    /// <summary>The drive cannot locate a specific area or track on the disk.</summary>
    Seek = 25U,
    /// <summary>The specified disk cannot be accessed.</summary>
    NotDosDisk = 26U,
    /// <summary>The drive cannot find the sector requested.</summary>
    SectorNotFound = 27U,
    /// <summary>The printer is out of paper.</summary>
    OutOfPaper = 28U,
    /// <summary>The system cannot write to the specified device.</summary>
    WriteFault = 29U,
    /// <summary>The system cannot read from the specified device.</summary>
    ReadFault = 30U,
    /// <summary>A device attached to the system is not functioning.</summary>
    GenFailure = 31U,
    /// <summary>The process cannot access the file because another process is using it.</summary>
    SharingViolation = 32U,
    /// <summary>The process cannot access the file because another process has locked a portion of it.</summary>
    LockViolation = 33U,
    /// <summary>The wrong disk is in the drive.</summary>
    WrongDisk = 34U,
    /// <summary>Too many files are opened for sharing.</summary>
    SharingBufferExceeded = 36U,
    /// <summary>The end of the file has been reached.</summary>
    HandleEof = 38U,
    /// <summary>The disk is full.</summary>
    HandleDiskFull = 39U,
    /// <summary>The request is not supported.</summary>
    NotSupported = 50U,
    /// <summary>The remote computer is not available.</summary>
    RemNotList = 51U,
    /// <summary>A duplicate name exists on the network.</summary>
    DupName = 52U,
    /// <summary>The network path was not found.</summary>
    BadNetPath = 53U,
    /// <summary>The network is busy.</summary>
    NetworkBusy = 54U,
    /// <summary>The specified network resource or device is no longer available.</summary>
    DevNotExist = 55U,
    /// <summary>Too many commands were issued.</summary>
    TooManyCmds = 56U,
    /// <summary>The file exists.</summary>
    FileExists = 80U,
    /// <summary>The directory or file cannot be created.</summary>
    CannotMake = 82U,
    /// <summary>The local device name is already in use.</summary>
    AlreadyAssigned = 85U,
    /// <summary>The specified network password is not correct.</summary>
    InvalidPassword = 86U,
    /// <summary>The parameter is incorrect.</summary>
    InvalidParameter = 87U,
    /// <summary>A write fault occurred on the network.</summary>
    NetWriteFault = 88U,
    /// <summary>The system cannot start another process at this time.</summary>
    NoProcSlots = 89U,
    /// <summary>Cannot create another system semaphore.</summary>
    TooManySemaphores = 100U,
    /// <summary>The exclusive semaphore is owned by another process.</summary>
    ExclSemAlreadyOwned = 101U,
    /// <summary>The semaphore is set and cannot be closed.</summary>
    SemIsSet = 102U,
    /// <summary>The semaphore cannot be set again.</summary>
    TooManySemRequests = 103U,
    /// <summary>The semaphore cannot be requested at interrupt time.</summary>
    InvalidAtInterruptTime = 104U,
    /// <summary>The previous ownership of this semaphore has ended.</summary>
    SemOwnerDied = 105U,
    /// <summary>The limit for the number of semaphores has been reached.</summary>
    SemUserLimit = 106U
}
