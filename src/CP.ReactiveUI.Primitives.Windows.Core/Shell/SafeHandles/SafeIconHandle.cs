// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using Microsoft.Win32.SafeHandles;

namespace CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles;

/// <summary>Owns a Windows icon handle returned by Shell APIs.</summary>
public sealed class SafeIconHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle" /> class.</summary>
    public SafeIconHandle()
        : base(ownsHandle: true)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle" /> class.</summary>
    /// <param name="preexistingHandle">The existing native icon handle.</param>
    public SafeIconHandle(IntPtr preexistingHandle)
        : base(ownsHandle: true)
    {
        SetHandle(preexistingHandle);
    }

    /// <summary>Native icon lifetime entry points.</summary>
    private static class NativeMethods
    {
        /// <summary>Destroys an icon handle.</summary>
        /// <param name="iconHandle">The icon handle.</param>
        /// <returns><see langword="true" /> when the icon handle is destroyed.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyIcon(IntPtr iconHandle);
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
}
