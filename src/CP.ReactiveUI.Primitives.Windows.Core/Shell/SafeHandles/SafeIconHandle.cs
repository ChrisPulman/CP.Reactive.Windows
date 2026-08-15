// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

namespace CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles;

/// <summary>Owns a Windows icon handle returned by Shell APIs.</summary>
#if NET462 || NET472 || NET48 || NET481
public sealed class SafeIconHandle : SafeHandleZeroOrMinusOneIsInvalid
#else
public sealed partial class SafeIconHandle : SafeHandleZeroOrMinusOneIsInvalid
#endif
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle" /> class.</summary>
    public SafeIconHandle()
        : base(ownsHandle: true) { }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle" /> class.</summary>
    /// <param name="preexistingHandle">The existing native icon handle.</param>
    public SafeIconHandle(IntPtr preexistingHandle)
        : base(ownsHandle: true)
    {
        SetHandle(preexistingHandle);
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle" /> class from a bitmap.</summary>
    /// <param name="bitmap">The source bitmap.</param>
    public SafeIconHandle(Bitmap bitmap)
        : base(ownsHandle: true)
    {
        SetHandle(bitmap.GetHicon());
    }

    /// <summary>Uses the native handle while this safe handle is reference-counted.</summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="handleAction">The action that uses the native handle.</param>
    /// <returns>The action result.</returns>
    public T UseNativeHandle<T>(Func<IntPtr, T> handleAction)
    {
        Throw.IfNull(handleAction);
        bool handleAcquired = false;
        try
        {
            DangerousAddRef(ref handleAcquired);
            return handleAction(handle);
        }
        finally
        {
            if (handleAcquired)
            {
                DangerousRelease();
            }
        }
    }

    /// <inheritdoc />
    protected override bool ReleaseHandle() => NativeMethods.DestroyIcon(handle);

    /// <summary>Native icon lifetime entry points.</summary>
#if NET462 || NET472 || NET48 || NET481
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>Destroys an icon handle.</summary>
        /// <param name="iconHandle">The icon handle.</param>
        /// <returns><see langword="true" /> when the icon handle is destroyed.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyIcon(IntPtr iconHandle);
#else
        [LibraryImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool DestroyIcon(IntPtr iconHandle);
#endif
    }
}
