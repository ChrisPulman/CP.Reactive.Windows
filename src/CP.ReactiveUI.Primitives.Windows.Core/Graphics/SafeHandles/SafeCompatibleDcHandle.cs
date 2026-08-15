// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;

/// <summary>Provides a safe handle for a compatible device context.</summary>
public class SafeCompatibleDcHandle : SafeDcHandle
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeCompatibleDcHandle" /> class.</summary>
    public SafeCompatibleDcHandle()
        : base(ownsHandle: true)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeCompatibleDcHandle" /> class from an existing handle.</summary>
    /// <param name="preexistingHandle">The existing device-context handle.</param>
    public SafeCompatibleDcHandle(IntPtr preexistingHandle)
        : base(ownsHandle: true)
    {
        SetHandle(preexistingHandle);
    }

    /// <summary>Contains native device-context entry points.</summary>
    private static class NativeMethods
    {
        /// <summary>Invokes the native <c>DeleteDC</c> entry point.</summary>
        /// <param name="deviceContext">The native device-context handle.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdi32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteDC(IntPtr deviceContext);
    }

    /// <summary>Selects an object into this device context.</summary>
    /// <param name="objectSafeHandle">The object to select.</param>
    /// <returns>A handle that restores the previously selected object when disposed.</returns>
    public SafeSelectObjectHandle SelectObject(SafeHandle objectSafeHandle) => new(this, objectSafeHandle);

    /// <inheritdoc />
    protected override bool ReleaseHandle() => NativeMethods.DeleteDC(handle);
}
