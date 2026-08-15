// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;

/// <summary>Provides a safe handle for a compatible device context.</summary>
#if NETFRAMEWORK
public class SafeCompatibleDcHandle : SafeDcHandle
#else
public partial class SafeCompatibleDcHandle : SafeDcHandle
#endif
{
    /// <summary>Initializes a new instance of the <see cref="SafeCompatibleDcHandle" /> class.</summary>
    public SafeCompatibleDcHandle()
        : base(ownsHandle: true) { }

    /// <summary>Initializes a new instance of the <see cref="SafeCompatibleDcHandle" /> class from an existing handle.</summary>
    /// <param name="preexistingHandle">The existing device-context handle.</param>
    public SafeCompatibleDcHandle(IntPtr preexistingHandle)
        : base(ownsHandle: true) => SetHandle(preexistingHandle);

    /// <summary>Selects an object into this device context.</summary>
    /// <param name="objectSafeHandle">The object to select.</param>
    /// <returns>A handle that restores the previously selected object.</returns>
    public SafeSelectObjectHandle SelectObject(SafeHandle objectSafeHandle) =>
        new(this, objectSafeHandle);

    /// <inheritdoc />
    protected override bool ReleaseHandle() => NativeMethods.DeleteDC(handle);

    /// <summary>Contains native device-context entry points.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
#if NETFRAMEWORK
        /// <summary>Invokes the native <c>DeleteDC</c> entry point.</summary>
        /// <param name="deviceContext">The device context to release.</param>
        /// <returns><see langword="true" /> when the context is released; otherwise, <see langword="false" />.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteDC(IntPtr deviceContext);
#else
        [LibraryImport("gdi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool DeleteDC(IntPtr deviceContext);
#endif
    }
}
