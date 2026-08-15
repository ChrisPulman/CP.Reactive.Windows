// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
#if NETFRAMEWORK
using System.Security.Permissions;
#endif
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using Microsoft.Win32.SafeHandles;
using log4net;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles;

/// <summary>A SafeHandle class implementation for the current input desktop.</summary>
public class SafeCurrentInputDesktopHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>Stores the log value.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(SafeCurrentInputDesktopHandle));

    /// <summary>Closes a desktop handle.</summary>
    private readonly Func<IntPtr, bool> _closeDesktop;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles.SafeCurrentInputDesktopHandle" /> class.</summary>
    public SafeCurrentInputDesktopHandle()
        : this(() => User32Api.OpenInputDesktop(0U, inherit: true, DesktopAccessRight.GENERIC_ALL), User32Api.SetThreadDesktop, User32Api.CloseDesktop)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles.SafeCurrentInputDesktopHandle" /> class using composed desktop operations.</summary>
    /// <param name="openInputDesktop">Opens the current input desktop.</param>
    /// <param name="setThreadDesktop">Assigns the current thread to a desktop.</param>
    /// <param name="closeDesktop">Closes a desktop handle.</param>
    protected SafeCurrentInputDesktopHandle(Func<IntPtr> openInputDesktop, Func<IntPtr, bool> setThreadDesktop, Func<IntPtr, bool> closeDesktop)
        : base(ownsHandle: true)
    {
        Throw.IfNull(openInputDesktop);
        Throw.IfNull(setThreadDesktop);
        Throw.IfNull(closeDesktop);
        _closeDesktop = closeDesktop;
        IntPtr desktopHandle = openInputDesktop();
        if (desktopHandle != IntPtr.Zero)
        {
            SetHandle(desktopHandle);
            if (setThreadDesktop(desktopHandle))
            {
                Log.DebugFormat("Switched to desktop {0}", desktopHandle);
                return;
            }

            Log.WarnFormat("Couldn't switch to desktop {0}", desktopHandle);
            Log.Error("SetThreadDesktop failed.", User32Api.CreateWin32Exception("SetThreadDesktop"));
        }
        else
        {
            Log.Warn("Couldn't get current desktop.");
            Log.Error("OpenInputDesktop failed.", User32Api.CreateWin32Exception("OpenInputDesktop"));
        }
    }

    /// <summary>Close the desktop.</summary>
    /// <returns>True if this succeeded.</returns>
#if NETFRAMEWORK
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
#endif
    protected override bool ReleaseHandle() => _closeDesktop(handle);
}
