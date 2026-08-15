// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Extensions;

namespace CP.ReactiveUI.Primitives.Windows.Interop.Com;

/// <summary>This provides an API for OLE32.</summary>
public static class Ole32Api
{
    /// <summary>Converts a ProgID into a class identifier.</summary>
    /// <param name="programId">The program identifier.</param>
    /// <returns>The class identifier.</returns>
    public static Guid ClassIdFromProgId(string programId)
    {
        _ = NativeMethods.CLSIDFromProgID(programId, out var clsId);
        return clsId;
    }

    /// <summary>Converts a class identifier into a ProgID.</summary>
    /// <param name="clsId">The class identifier.</param>
    /// <returns>The program identifier.</returns>
    public static string ProgIdFromClassId(Guid clsId) => !NativeMethods.ProgIDFromCLSID(ref clsId, out var progId).Succeeded() ? null : progId;

    /// <summary>Native OLE32 entry points.</summary>
    private static class NativeMethods
    {
        /// <summary>The OLE32 library name.</summary>
        private const string Ole32Dll = "ole32.dll";

        /// <summary>Converts a program identifier into a class identifier.</summary>
        /// <param name="progId">The program identifier.</param>
        /// <param name="clsId">The class identifier.</param>
        /// <returns>The operation result.</returns>
        [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern HResult CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsId);

        /// <summary>Converts a class identifier into a program identifier.</summary>
        /// <param name="clsId">The class identifier for which the ProgID is requested.</param>
        /// <param name="programId">The program identifier.</param>
        /// <returns>The operation result.</returns>
        [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern HResult ProgIDFromCLSID(ref Guid clsId, [MarshalAs(UnmanagedType.LPWStr)] out string programId);
    }
}
