// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Extensions;

namespace CP.ReactiveUI.Primitives.Windows.Interop.Com;

/// <summary>API for OLEAUT32.</summary>
#if NETFRAMEWORK
public static class OleAut32Api
#else
public static partial class OleAut32Api
#endif
{
    /// <summary>OLE Automation operations used by this process.</summary>
    private static OleAut32Operations _operations = new(
        NativeMethods.GetActiveObject,
        Ole32Api.ClassIdFromProgId,
        Marshal.GetObjectForIUnknown,
        Marshal.Release);

    /// <summary>Gets the active instance of the COM object with the specified GUID.</summary>
    /// <typeparam name="T">Type for the instance.</typeparam>
    /// <param name="clsId">The class identifier.</param>
    /// <param name="materializer">Converts the active COM object to the requested type.</param>
    /// <returns>The disposable COM object wrapper.</returns>
    public static IDisposableCom<T> GetActiveObject<T>(ref Guid clsId, Func<object, T> materializer)
    {
        Throw.IfNull(materializer);
        if (
            !_operations.GetActiveObject(ref clsId, IntPtr.Zero, out var activeObject).Succeeded()
            || activeObject == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            return DisposableCom.Create(materializer(_operations.GetObjectForIUnknown(activeObject)));
        }
        finally
        {
            _ = _operations.Release(activeObject);
        }
    }

    /// <summary>Gets the active instance of the COM object with the specified class identifier.</summary>
    /// <param name="clsId">The class identifier.</param>
    /// <returns>The disposable COM object wrapper.</returns>
    public static IDisposableCom<object> GetActiveObject(ref Guid clsId) =>
        GetActiveObject(ref clsId, static activeObject => activeObject);

    /// <summary>Gets the active instance of the COM object with the specified ProgID.</summary>
    /// <param name="progId">The program identifier.</param>
    /// <returns>The disposable COM object wrapper.</returns>
    public static IDisposableCom<object> GetActiveObject(string progId)
    {
        Guid clsId = _operations.ClassIdFromProgId(progId);
        return GetActiveObject(ref clsId);
    }

    /// <summary>Gets the active instance of the COM object with the specified ProgID.</summary>
    /// <typeparam name="T">Type for the instance.</typeparam>
    /// <param name="progId">The program identifier.</param>
    /// <param name="materializer">Converts the active COM object to the requested type.</param>
    /// <returns>The disposable COM object wrapper.</returns>
    public static IDisposableCom<T> GetActiveObject<T>(string progId, Func<object, T> materializer)
    {
        Guid clsId = _operations.ClassIdFromProgId(progId);
        return GetActiveObject(ref clsId, materializer);
    }

    /// <summary>Overrides OLE Automation operations for deterministic tests.</summary>
    /// <param name="getActiveObject">The replacement active-object operation.</param>
    /// <param name="classIdFromProgId">The replacement program-identifier conversion operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(
        GetActiveObjectOperation getActiveObject,
        Func<string, Guid> classIdFromProgId) =>
        OverrideOperationsForTesting(
            getActiveObject,
            classIdFromProgId,
            Marshal.GetObjectForIUnknown,
            Marshal.Release);

    /// <summary>Overrides every OLE Automation operation for deterministic tests.</summary>
    /// <param name="getActiveObject">The replacement active-object operation.</param>
    /// <param name="classIdFromProgId">The replacement program-identifier conversion operation.</param>
    /// <param name="getObjectForIUnknown">The replacement object materialization operation.</param>
    /// <param name="release">The replacement COM pointer release operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(
        GetActiveObjectOperation getActiveObject,
        Func<string, Guid> classIdFromProgId,
        Func<IntPtr, object> getObjectForIUnknown,
        Func<IntPtr, int> release)
    {
        Throw.IfNull(getActiveObject);
        Throw.IfNull(classIdFromProgId);
        Throw.IfNull(getObjectForIUnknown);
        Throw.IfNull(release);
        OleAut32Operations operations = _operations;
        _operations = new(getActiveObject, classIdFromProgId, getObjectForIUnknown, release);
        return Scope.Create(
            operations,
            static previous => _operations = previous);
    }

    /// <summary>Native OLEAUT32 entry points.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>Retrieves a pointer to a running object.</summary>
        /// <param name="classId">The class identifier (CLSID) of the active object from the OLE registration database.</param>
        /// <param name="reserved">Reserved for future use. Must be null.</param>
        /// <param name="activeObject">The requested active object.</param>
        /// <returns>The operation result.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("oleaut32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern HResult GetActiveObject(
            ref Guid classId,
            IntPtr reserved,
            out IntPtr activeObject);
#else
        [LibraryImport("oleaut32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial HResult GetActiveObject(
            ref Guid classId,
            IntPtr reserved,
            out IntPtr activeObject);
#endif
    }

    /// <summary>Composes OLE Automation operations without invoking them during construction.</summary>
    /// <param name="getActiveObject">The active-object operation.</param>
    /// <param name="classIdFromProgId">The program-identifier conversion operation.</param>
    /// <param name="getObjectForIUnknown">The object materialization operation.</param>
    /// <param name="release">The COM pointer release operation.</param>
    private sealed class OleAut32Operations(
        GetActiveObjectOperation getActiveObject,
        Func<string, Guid> classIdFromProgId,
        Func<IntPtr, object> getObjectForIUnknown,
        Func<IntPtr, int> release)
    {
        /// <summary>Invokes the configured active-object operation.</summary>
        /// <param name="classId">The class identifier.</param>
        /// <param name="reserved">The reserved pointer.</param>
        /// <param name="activeObject">The active object pointer.</param>
        /// <returns>The configured operation result.</returns>
        public HResult GetActiveObject(
            ref Guid classId,
            IntPtr reserved,
            out IntPtr activeObject) => getActiveObject(ref classId, reserved, out activeObject);

        /// <summary>Invokes the configured program-identifier conversion operation.</summary>
        /// <param name="progId">The program identifier.</param>
        /// <returns>The configured class identifier.</returns>
        public Guid ClassIdFromProgId(string progId) => classIdFromProgId(progId);

        /// <summary>Materializes the configured COM pointer.</summary>
        /// <param name="unknown">The COM interface pointer.</param>
        /// <returns>The materialized object.</returns>
        public object GetObjectForIUnknown(IntPtr unknown) => getObjectForIUnknown(unknown);

        /// <summary>Releases the configured COM pointer.</summary>
        /// <param name="unknown">The COM interface pointer.</param>
        /// <returns>The remaining reference count.</returns>
        public int Release(IntPtr unknown) => release(unknown);
    }
}
