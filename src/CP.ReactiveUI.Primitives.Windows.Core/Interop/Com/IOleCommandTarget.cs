// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace CP.ReactiveUI.Primitives.Windows.Interop.Com;

/// <summary>
/// Enables objects and their containers to dispatch commands to each other.
/// See the IOleCommandTarget interface documentation.
/// </summary>
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[ComVisible(true)]
[Guid("B722BCCB-4E68-101B-A2BC-00AA00404770")]
public interface IOleCommandTarget
{
    /// <summary>Queries the object for the status of one or more commands.</summary>
    /// <param name="commandGroup">The unique identifier of the command group.</param>
    /// <param name="commandCount">The number of commands in the commands array.</param>
    /// <param name="commands">A caller-allocated array of OLECMD structures.</param>
    /// <param name="commandText">A pointer to an OLECMDTEXT structure.</param>
    /// <returns>
    /// This method returns S_OK on success. Other possible return values include the following.
    /// E_FAIL
    /// The operation failed.
    /// /// E_UNEXPECTED
    /// An unexpected error has occurred.
    /// /// E_POINTER
    /// The prgCmds argument is NULL.
    /// /// OLECMDERR_E_UNKNOWNGROUP
    /// The pguidCmdGroup parameter is not NULL but does not specify a recognized command group.
    /// </returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I4)]
    int QueryStatus(Guid commandGroup, int commandCount, IntPtr commands, IntPtr commandText);

    /// <summary>Executes the specified command or displays help for the command.</summary>
    /// <param name="commandGroup">The unique identifier of the command group.</param>
    /// <param name="commandId">The command to be executed.</param>
    /// <param name="commandOptions">Specifies how the object should execute the command.</param>
    /// <param name="input">A pointer to a VARIANTARG structure containing input arguments.</param>
    /// <param name="output">Pointer to a VARIANTARG structure to receive command output.</param>
    /// <returns>
    /// This method returns S_OK on success. Other possible return values include the following.
    /// OLECMDERR_E_UNKNOWNGROUP
    /// The pguidCmdGroup parameter is not NULL but does not specify a recognized command group.
    /// OLECMDERR_E_NOTSUPPORTED
    /// The nCmdID parameter is not a valid command in the group identified by pguidCmdGroup.
    /// OLECMDERR_E_DISABLED
    /// The command identified by nCmdID is currently disabled and cannot be executed.
    /// OLECMDERR_E_NOHELP
    /// The caller has asked for help on the command identified by nCmdID, but no help is available.
    /// OLECMDERR_E_CANCELED
    /// The user canceled the execution of the command.
    /// </returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I4)]
    int Exec(Guid commandGroup, int commandId, int commandOptions, IntPtr input, IntPtr output);
}
