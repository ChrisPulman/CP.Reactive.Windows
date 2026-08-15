// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Extensions;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using ReactiveUI.Primitives.Disposables;

namespace CP.ReactiveUI.Primitives.Windows.Interop.Com;

/// <summary>API for OLEAUT32.</summary>
public static class OleAut32Api
{
    /// <summary>OLE Automation operations used by this process.</summary>
    private static OleAut32Operations _operations = new(NativeMethods.GetActiveObject, Ole32Api.ClassIdFromProgId);

    /// <summary>Gets the active instance of the COM object with the specified class identifier.</summary>
    /// <param name="clsId">The class identifier.</param>
    /// <returns>The disposable COM object wrapper.</returns>
    public static IDisposableCom<object> GetActiveObject(ref Guid clsId) => GetActiveObject(ref clsId, (activeObject) => activeObject);

    /// <summary>Native OLEAUT32 entry points.</summary>
    private static class NativeMethods
    {
        /// <summary>The OLEAUT32 library name.</summary>
        private const string OleAut32Dll = "oleaut32.dll";

        /// <summary>Retrieves a pointer to a running object.</summary>
        /// <param name="classId">The class identifier (CLSID) of the active object from the OLE registration database.</param>
        /// <param name="reserved">Reserved for future use. Must be null.</param>
        /// <param name="activeObject">The requested active object.</param>
        /// <returns>The operation result.</returns>
        [DllImport("oleaut32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern HResult GetActiveObject(ref Guid classId, IntPtr reserved, out IntPtr activeObject);
    }

    /// <summary>Composes OLE Automation operations without invoking them during construction.</summary>
    /// <param name="getActiveObject">The active-object operation.</param>
    /// <param name="classIdFromProgId">The program-identifier conversion operation.</param>
    private sealed class OleAut32Operations(GetActiveObjectOperation getActiveObject, Func<string, Guid> classIdFromProgId)
    {
        /// <summary>Invokes the configured active-object operation.</summary>
        /// <param name="classId">The class identifier.</param>
        /// <param name="reserved">The reserved pointer.</param>
        /// <param name="activeObject">The active object pointer.</param>
        /// <returns>The configured operation result.</returns>
        public HResult GetActiveObject(ref Guid classId, IntPtr reserved, out IntPtr activeObject) => getActiveObject(ref classId, reserved, out activeObject);

        /// <summary>Invokes the configured program-identifier conversion operation.</summary>
        /// <param name="progId">The program identifier.</param>
        /// <returns>The configured class identifier.</returns>
        public Guid ClassIdFromProgId(string progId) => classIdFromProgId(progId);
    }

    /// <summary>Gets the active instance of the COM object with the specified GUID.</summary>
    /// <typeparam name="T">Type for the instance.</typeparam>
    /// <param name="clsId">The class identifier.</param>
    /// <param name="materializer">Converts the active COM object to the requested type.</param>
    /// <returns>The disposable COM object wrapper.</returns>
    public static IDisposableCom<T> GetActiveObject<T>(ref Guid clsId, Func<object, T> materializer)
    {
        Throw.IfNull(materializer);
        if (!_operations.GetActiveObject(ref clsId, IntPtr.Zero, out var activeObject).Succeeded() || activeObject == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            return DisposableCom.Create(materializer(Marshal.GetObjectForIUnknown(activeObject)));
        }
        finally
        {
            _ = Marshal.Release(activeObject);
        }
    }

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
    internal static IDisposable OverrideOperationsForTesting(GetActiveObjectOperation getActiveObject, Func<string, Guid> classIdFromProgId)
    {
        Throw.IfNull(getActiveObject);
        Throw.IfNull(classIdFromProgId);
        OleAut32Operations operations = _operations;
        _operations = new(getActiveObject, classIdFromProgId);
        return Scope.Create(operations, delegate(OleAut32Operations previous)
        {
            _operations = previous;
        });
    }
}
