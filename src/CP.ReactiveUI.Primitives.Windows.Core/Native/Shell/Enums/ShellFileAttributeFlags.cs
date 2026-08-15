// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Shell.Enums;

/// <summary>
/// A combination of one or more file attribute flags (FILE_ATTRIBUTE_ values as defined in Winnt.h).
/// If flags does not include the SHGFI_USEFILEATTRIBUTES flag, this parameter is ignored.
/// </summary>
[Flags]
public enum ShellFileAttributeFlags : uint
{
    /// <summary>No file attributes.</summary>
    None = 0U,

    /// <summary>A file or directory that is an archive file or directory.</summary>
    Archive = 0x20U,

    /// <summary>A file or directory that is compressed.</summary>
    Compressed = 0x800U,

    /// <summary>This value is reserved for system use.</summary>
    Device = 0x40U,

    /// <summary>The handle that identifies a directory.</summary>
    Directory = 0x10U,

    /// <summary>
    /// A file or directory that is encrypted. For a file, all data streams in the file are encrypted. For a directory, encryption is the default for newly created files and subdirectories.
    /// </summary>
    Encrypted = 0x4000U,

    /// <summary>The file or directory is hidden. It is not included in an ordinary directory listing.</summary>
    Hidden = 2U,

    /// <summary>The directory or user data stream is configured with integrity.</summary>
    IntegrityStream = 0x8000U,

    /// <summary>A file that does not have other attributes set. This attribute is valid only when used alone.</summary>
    Normal = 0x80U,

    /// <summary>The file or directory is not to be indexed by the content indexing service.</summary>
    NotContextIndexed = 0x2000U,

    /// <summary>The user data stream is not read by the background data integrity scanner.</summary>
    NoScrubData = 0x20000U,

    /// <summary>
    /// The data of a file is not available immediately. This attribute indicates that the file data is physically moved to offline storage.
    /// This attribute is used by Remote Storage, which is the hierarchical storage management software. Applications should not arbitrarily change this attribute.
    /// </summary>
    Offline = 0x1000U,

    /// <summary>A file that is read-only.</summary>
    Readonly = 1U,

    /// <summary>The file or directory is not fully present locally.</summary>
    RecallOnDataAccess = 0x400000U,

    /// <summary>
    /// This attribute only appears in directory enumeration classes(FILE_DIRECTORY_INFORMATION, FILE_BOTH_DIR_INFORMATION, etc.).
    /// When this attribute is set, it means that the file or directory has no physical representation on the local system; the item is virtual.
    /// Opening the item will be more expensive than normal, e.g.it will cause at least some of it to be fetched from a remote store.
    /// </summary>
    RecallOnOpen = 0x40000U,

    /// <summary>A file or directory that has an associated reparse point, or a file that is a symbolic link.</summary>
    ReparsePoint = 0x400U,

    /// <summary>A file that is a sparse file.</summary>
    SparseFile = 0x200U,

    /// <summary>A file or directory that the operating system uses a part of, or uses exclusively.</summary>
    System = 4U,

    /// <summary>A file that is being used for temporary storage.</summary>
    Temporary = 0x100U,

    /// <summary>This value is reserved for system use.</summary>
    Virtual = 0x10000U,
}
