// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET462 || NET472 || NET48 || NET481
using System.Security.Permissions;
#endif

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles;

/// <summary>A SafeHandle class implementation for the current input desktop.</summary>
public class SafeCurrentInputDesktopHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>Stores the log value.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(SafeCurrentInputDesktopHandle));

    /// <summary>Stores the operations used by the public constructor.</summary>
    private static InputDesktopOperations _defaultOperations = new(
        static () => User32Api.OpenInputDesktop(
            0U,
            inherit: true,
            DesktopAccessRight.GENERIC_ALL),
        User32Api.SetThreadDesktop,
        User32Api.CloseDesktop);

    /// <summary>Closes a desktop handle.</summary>
    private readonly Func<IntPtr, bool> _closeDesktop;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles.SafeCurrentInputDesktopHandle" /> class.</summary>
    public SafeCurrentInputDesktopHandle()
        : this(Volatile.Read(ref _defaultOperations)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles.SafeCurrentInputDesktopHandle" /> class
    /// using composed desktop operations.
    /// </summary>
    /// <param name="openInputDesktop">Opens the current input desktop.</param>
    /// <param name="setThreadDesktop">Assigns the current thread to a desktop.</param>
    /// <param name="closeDesktop">Closes a desktop handle.</param>
    protected SafeCurrentInputDesktopHandle(
        Func<IntPtr> openInputDesktop,
        Func<IntPtr, bool> setThreadDesktop,
        Func<IntPtr, bool> closeDesktop)
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
            Log.Error(
                "SetThreadDesktop failed.",
                User32Api.CreateWin32Exception("SetThreadDesktop"));
        }
        else
        {
            Log.Warn("Couldn't get current desktop.");
            Log.Error(
                "OpenInputDesktop failed.",
                User32Api.CreateWin32Exception("OpenInputDesktop"));
        }
    }

    /// <summary>Initializes a new instance of the <see cref="SafeCurrentInputDesktopHandle"/> class.</summary>
    /// <param name="operations">The operations used to open, switch, and close the desktop.</param>
    private SafeCurrentInputDesktopHandle(InputDesktopOperations operations)
        : this(operations.OpenInputDesktop, operations.SetThreadDesktop, operations.CloseDesktop) { }

    /// <summary>Atomically overrides public-constructor operations for deterministic tests.</summary>
    /// <param name="openInputDesktop">Opens the current input desktop.</param>
    /// <param name="setThreadDesktop">Assigns the current thread to a desktop.</param>
    /// <param name="closeDesktop">Closes a desktop handle.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideDefaultOperationsForTesting(
        Func<IntPtr> openInputDesktop,
        Func<IntPtr, bool> setThreadDesktop,
        Func<IntPtr, bool> closeDesktop)
    {
        Throw.IfNull(openInputDesktop);
        Throw.IfNull(setThreadDesktop);
        Throw.IfNull(closeDesktop);
        InputDesktopOperations previous = Interlocked.Exchange(
            ref _defaultOperations,
            new(openInputDesktop, setThreadDesktop, closeDesktop));
        return Scope.Create(previous, static value => Volatile.Write(ref _defaultOperations, value));
    }

    /// <summary>Close the desktop.</summary>
    /// <returns>True if this succeeded.</returns>
#if NET462 || NET472 || NET48 || NET481
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
#endif
    protected override bool ReleaseHandle() => _closeDesktop(handle);

    /// <summary>Groups operations used to manage the current input desktop.</summary>
    private sealed class InputDesktopOperations
    {
        /// <summary>Initializes a new instance of the <see cref="InputDesktopOperations"/> class.</summary>
        /// <param name="openInputDesktop">Opens the current input desktop.</param>
        /// <param name="setThreadDesktop">Assigns the current thread to a desktop.</param>
        /// <param name="closeDesktop">Closes a desktop handle.</param>
        internal InputDesktopOperations(
            Func<IntPtr> openInputDesktop,
            Func<IntPtr, bool> setThreadDesktop,
            Func<IntPtr, bool> closeDesktop)
        {
            OpenInputDesktop = openInputDesktop;
            SetThreadDesktop = setThreadDesktop;
            CloseDesktop = closeDesktop;
        }

        /// <summary>Gets the desktop-opening operation.</summary>
        internal Func<IntPtr> OpenInputDesktop { get; }

        /// <summary>Gets the thread-desktop operation.</summary>
        internal Func<IntPtr, bool> SetThreadDesktop { get; }

        /// <summary>Gets the desktop-closing operation.</summary>
        internal Func<IntPtr, bool> CloseDesktop { get; }
    }
}
