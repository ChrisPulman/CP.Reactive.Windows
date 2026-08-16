// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Composes the Kernel32 operations used by managed wrappers.</summary>
internal sealed class Kernel32Operations
{
    /// <summary>Gets or sets the logical-drive enumeration operation.</summary>
    internal Func<string[]> GetLogicalDrives { get; set; } = Environment.GetLogicalDrives;

    /// <summary>Gets or sets the default-DLL-directory operation.</summary>
    internal required Func<DefaultDllDirectories, bool> SetDefaultDllDirectories { get; set; }

    /// <summary>Gets or sets the DLL-directory operation.</summary>
    internal required Func<string, bool> SetDllDirectory { get; set; }

    /// <summary>Gets or sets the console-allocation operation.</summary>
    internal required Func<bool> AllocConsole { get; set; }

    /// <summary>Gets or sets the console-attachment operation.</summary>
    internal required Func<uint, bool> AttachConsole { get; set; }

    /// <summary>Gets or sets the handle-closing operation.</summary>
    internal required Func<IntPtr, bool> CloseHandle { get; set; }

    /// <summary>Gets or sets the process-opening operation.</summary>
    internal required Func<ProcessAccessRights, bool, int, IntPtr> OpenProcess { get; set; }

    /// <summary>Gets or sets the DOS-device-query operation.</summary>
    internal required QueryDosDeviceOperation QueryDosDevice { get; set; }

    /// <summary>Gets or sets the process-image-name-query operation.</summary>
    internal required QueryFullProcessImageNameOperation QueryFullProcessImageName { get; set; }

    /// <summary>Gets or sets the version-query operation.</summary>
    internal required GetVersionExOperation GetVersionEx { get; set; }

    /// <summary>Gets or sets the package-name-query operation.</summary>
    internal required GetPackageFullNameOperation GetPackageFullName { get; set; }
}
