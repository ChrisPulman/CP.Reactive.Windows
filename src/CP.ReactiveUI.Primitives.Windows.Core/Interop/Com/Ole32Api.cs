// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Extensions;

namespace CP.ReactiveUI.Primitives.Windows.Interop.Com;

/// <summary>This provides an API for OLE32.</summary>
#if NETFRAMEWORK
public static class Ole32Api
#else
public static partial class Ole32Api
#endif
{
    /// <summary>The OLE32 operations used by this process.</summary>
    private static Ole32Operations _operations = new(
        NativeMethods.CLSIDFromProgID,
        NativeMethods.ProgIDFromCLSID);

    /// <summary>Converts a ProgID into a class identifier.</summary>
    /// <param name="programId">The program identifier.</param>
    /// <returns>The class identifier.</returns>
    public static Guid ClassIdFromProgId(string programId)
    {
        _ = _operations.ClassIdFromProgId(programId, out var clsId);
        return clsId;
    }

    /// <summary>Converts a class identifier into a ProgID.</summary>
    /// <param name="clsId">The class identifier.</param>
    /// <returns>The program identifier.</returns>
    public static string ProgIdFromClassId(Guid clsId) =>
        !_operations.ProgIdFromClassId(ref clsId, out var progId).Succeeded() ? null : progId;

    /// <summary>Overrides OLE32 operations for deterministic tests.</summary>
    /// <param name="classIdFromProgId">The replacement ProgID-to-class-ID operation.</param>
    /// <param name="progIdFromClassId">The replacement class-ID-to-ProgID operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(
        ClassIdFromProgIdOperation classIdFromProgId,
        ProgIdFromClassIdOperation progIdFromClassId)
    {
        Throw.IfNull(classIdFromProgId);
        Throw.IfNull(progIdFromClassId);
        Ole32Operations operations = _operations;
        _operations = new(classIdFromProgId, progIdFromClassId);
        return Scope.Create(operations, static previous => _operations = previous);
    }

    /// <summary>Native OLE32 entry points.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>The OLE32 library name.</summary>
        private const string Ole32Dll = "ole32.dll";

        /// <summary>Converts a program identifier into a class identifier.</summary>
        /// <param name="progId">The program identifier.</param>
        /// <param name="clsId">The class identifier.</param>
        /// <returns>The operation result.</returns>
#if NETFRAMEWORK
        [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern HResult CLSIDFromProgID(
            [MarshalAs(UnmanagedType.LPWStr)] string progId,
            out Guid clsId);
#else
        [LibraryImport("ole32.dll", StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial HResult CLSIDFromProgID(
            [MarshalAs(UnmanagedType.LPWStr)] string progId,
            out Guid clsId);
#endif

        /// <summary>Converts a class identifier into a program identifier.</summary>
        /// <param name="clsId">The class identifier for which the ProgID is requested.</param>
        /// <param name="programId">The program identifier.</param>
        /// <returns>The operation result.</returns>
#if NETFRAMEWORK
        [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern HResult ProgIDFromCLSID(
            ref Guid clsId,
            [MarshalAs(UnmanagedType.LPWStr)] out string programId);
#else
        [LibraryImport("ole32.dll", StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial HResult ProgIDFromCLSID(
            ref Guid clsId,
            [MarshalAs(UnmanagedType.LPWStr)] out string programId);
#endif
    }

    /// <summary>Composes OLE32 operations without invoking them during construction.</summary>
    /// <param name="classIdFromProgId">The ProgID-to-class-ID operation.</param>
    /// <param name="progIdFromClassId">The class-ID-to-ProgID operation.</param>
    private sealed class Ole32Operations(
        ClassIdFromProgIdOperation classIdFromProgId,
        ProgIdFromClassIdOperation progIdFromClassId)
    {
        /// <summary>Invokes the configured ProgID-to-class-ID operation.</summary>
        /// <param name="programId">The program identifier.</param>
        /// <param name="classId">Receives the class identifier.</param>
        /// <returns>The operation result.</returns>
        public HResult ClassIdFromProgId(string programId, out Guid classId) =>
            classIdFromProgId(programId, out classId);

        /// <summary>Invokes the configured class-ID-to-ProgID operation.</summary>
        /// <param name="classId">The class identifier.</param>
        /// <param name="programId">Receives the program identifier.</param>
        /// <returns>The operation result.</returns>
        public HResult ProgIdFromClassId(ref Guid classId, out string programId) =>
            progIdFromClassId(ref classId, out programId);
    }
}
