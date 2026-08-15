// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Enums;

/// <summary>The HRESULT represents Windows error codes.</summary>
public enum HResult : uint
{
    /// <summary>The operation completed successfully.</summary>
    Ok = 0U,

    /// <summary>The operation completed successfully and returned false.</summary>
    False = 1U,

    /// <summary>The operation is pending.</summary>
    Pending = 2_147_483_658U,

    /// <summary>The method is not implemented.</summary>
    NotImplemented = 2_147_500_033U,

    /// <summary>No such interface is supported.</summary>
    NoInterface = 2_147_500_034U,

    /// <summary>A pointer is invalid.</summary>
    Pointer = 2_147_500_035U,

    /// <summary>The operation was aborted.</summary>
    Abort = 2_147_500_036U,

    /// <summary>Unspecified failure.</summary>
    Fail = 2_147_500_037U,

    /// <summary>An unexpected failure occurred.</summary>
    Unexpected = 2_147_549_183U,

    /// <summary>The requested type element was not found.</summary>
    TypeElementNotFound = 2_147_647_531U,

    /// <summary>The file was not found.</summary>
    FileNotFound = 2_147_942_402U,

    /// <summary>The path was not found.</summary>
    PathNotFound = 2_147_942_403U,

    /// <summary>General access denied error.</summary>
    AccessDenied = 2_147_942_405U,

    /// <summary>An invalid handle was used.</summary>
    Handle = 2_147_942_406U,

    /// <summary>The data is invalid.</summary>
    InvalidData = 2_147_942_413U,

    /// <summary>There is not enough memory to complete the operation.</summary>
    OutOfMemory = 2_147_942_414U,

    /// <summary>The request is not supported.</summary>
    NotSupported = 2_147_942_450U,

    /// <summary>Too many commands were issued.</summary>
    TooManyCommands = 2_147_942_456U,

    /// <summary>One or more arguments are invalid.</summary>
    InvalidArgument = 2_147_942_487U,

    /// <summary>The supplied buffer is too small.</summary>
    InsufficientBuffer = 2_147_942_522U,

    /// <summary>The connection was aborted.</summary>
    ConnectionAborted = 2_147_952_453U,

    /// <summary>The connection was reset.</summary>
    ConnectionReset = 2_147_952_454U,
}
