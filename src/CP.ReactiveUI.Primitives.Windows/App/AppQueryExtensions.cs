// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using CP.ReactiveUI.Primitives.Windows.Interop.Com;
using CP.ReactiveUI.Primitives.Windows.Native;
using CP.ReactiveUI.Primitives.Windows.Native.Extensions;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Apps;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Apps;
#endif
/// <summary>Helper class to support with Windows Store apps.</summary>
public static class AppQueryExtensions
{
    extension(IInteropWindow interopWindow)
    {
        /// <summary>Checks if the window is an App (Win8 or Win10).</summary>
        /// <returns>true when the window is an App.</returns>
        public bool IsApp()
        {
            if (WindowsVersion.IsWindows8OrLater)
            {
                if (!interopWindow.IsWin8App())
                {
                    return interopWindow.IsWin10App();
                }

                return true;
            }

            return false;
        }

        /// <summary>Tests if this window is for the App-Launcher.</summary>
        /// <returns>true when this window is for the App-Launcher.</returns>
        public bool IsAppLauncher() => "ImmersiveLauncher".Equals(interopWindow.GetClassname());

        /// <summary>Checks if the window is the metro gutter (sizeable separator).</summary>
        /// <returns>true when the window is the metro gutter.</returns>
        public bool IsGutter() => "ImmersiveGutter".Equals(interopWindow.GetClassname());

        /// <summary>Checks if the window is a Windows 10 App.</summary>
        /// <returns>true when the window is a Windows 10 App.</returns>
        public bool IsWin10App()
        {
            if (WindowsVersion.IsWindows10OrLater)
            {
                if (!AppWindowClass.Equals(interopWindow.GetClassname()))
                {
                    return HasChildClass(interopWindow, AppWindowClass);
                }

                return true;
            }

            return false;
        }

        /// <summary>Checks if the window is a background Windows 10 App.</summary>
        /// <returns>true when the window is a background Windows 10 App.</returns>
        public bool IsBackgroundWin10App()
        {
            if (WindowsVersion.IsWindows10OrLater && AppFrameWindowClass.Equals(interopWindow.GetClassname()))
            {
                return !HasChildClass(interopWindow, AppWindowClass);
            }

            return false;
        }

        /// <summary>Checks if the window is a Windows 8 App, not Windows 10.</summary>
        /// <returns>true when the window is a Windows 8 App.</returns>
        public bool IsWin8App()
        {
            if (WindowsVersion.IsWindows8 || WindowsVersion.IsWindows81)
            {
                return AppWindowClass.Equals(interopWindow.GetClassname());
            }

            return false;
        }
    }

    /// <summary>Used for Windows 8(.1) and Windows 10 (but as child of "ApplicationFrameWindow").</summary>
    public static readonly string AppWindowClass;

    /// <summary>Windows 10 uses ApplicationFrameWindow to host the App.</summary>
    public static readonly string AppFrameWindowClass;

    /// <summary>Window class name for the immersive launcher.</summary>
    private const string ApplauncherClass = "ImmersiveLauncher";

    /// <summary>Window class name for the immersive gutter.</summary>
    private const string GutterClass = "ImmersiveGutter";

    /// <summary>Window class for the App window; this depends on the Windows version.</summary>
    private static readonly string AppWindowIdentifierClass;

    /// <summary>COM class identifier for <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Apps.IAppVisibility" />.</summary>
    private static readonly Guid CoClassGuidIAppVisibility;

    /// <summary>COM app visibility wrapper, when available on the current Windows version.</summary>
    private static readonly IDisposableCom<IAppVisibility> AppVisibility;

    /// <summary>Gets the windowHandle for the AppLauncer.</summary>
    public static IntPtr AppLauncher
    {
        get
        {
            if (AppVisibility is not null)
            {
                return User32Api.FindWindow("ImmersiveLauncher", null);
            }

            return IntPtr.Zero;
        }
    }

    /// <summary>Gets a value indicating whether the app-launcher is visible.</summary>
    public static bool IsLauncherVisible => AppVisibility?.ComObject.IsLauncherVisible() ?? false;

    /// <summary>Gets handles of all Windows store apps.</summary>
    public static IEnumerable<IInteropWindow> WindowsStoreApps => EnumerateWindowsStoreApps();

    /// <summary>Initializes static members of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Apps.AppQueryExtensions" /> class.</summary>
    static AppQueryExtensions()
    {
        AppWindowClass = "Windows.UI.Core.CoreWindow";
        AppFrameWindowClass = "ApplicationFrameWindow";
        CoClassGuidIAppVisibility = new("7E5FE3D9-985F-4908-91F9-EE19F9FD1514");
        AppWindowIdentifierClass = (WindowsVersion.IsWindows8 ? AppWindowClass : AppFrameWindowClass);
        if (!WindowsVersion.IsWindows8OrLater)
        {
            AppVisibility = null;
            return;
        }

        try
        {
            AppVisibility = DisposableCom.Create((IAppVisibility)Activator.CreateInstance(Type.GetTypeFromCLSID(CoClassGuidIAppVisibility)));
        }
        catch
        {
            AppVisibility = null;
        }
    }

    /// <summary>Check if a Windows Store App (WinRT) is visible.</summary>
    /// <param name="windowBounds">NativeRect.</param>
    /// <returns>true if an app, covering the supplied rect, is visible.</returns>
    public static bool AppVisible(NativeRect windowBounds) => AppVisible(windowBounds, AppVisibility?.ComObject, DisplayTopology.GetSnapshot(), delegate(NativeRect bounds)
        {
            NativeRect rectangle = bounds;
            return User32Api.MonitorFromRect(ref rectangle, MonitorFrom.None);
        });

    /// <summary>Get the AppLauncher.</summary>
    /// <returns>IInteropWindow.</returns>
    public static IInteropWindow GetAppLauncher() => GetAppLauncher(IsLauncherVisible, () => User32Api.FindWindow("ImmersiveLauncher", null), InteropWindowFactory.CreateFor);

    /// <summary>Determines app visibility using supplied operating-system observations.</summary>
    /// <param name="windowBounds">Bounds of the window to check.</param>
    /// <param name="appVisibility">App visibility COM wrapper, or null when unavailable.</param>
    /// <param name="displays">Display snapshot to inspect.</param>
    /// <param name="monitorFromBounds">Function returning the monitor for fullscreen bounds.</param>
    /// <returns>true when the app should be treated as visible.</returns>
    internal static bool AppVisible(NativeRect windowBounds, IAppVisibility appVisibility, IReadOnlyList<DisplayInfo> displays, Func<NativeRect, IntPtr> monitorFromBounds)
    {
        if (appVisibility is null)
        {
            return true;
        }

        foreach (DisplayInfo screen in displays)
        {
            if (screen.Bounds.Contains(windowBounds))
            {
                if (!windowBounds.Equals(screen.Bounds))
                {
                    return true;
                }

                IntPtr monitor = monitorFromBounds(screen.Bounds);
                if (monitor != IntPtr.Zero && appVisibility.GetAppVisibilityOnMonitor(monitor) == MonitorAppVisibility.MAV_APP_VISIBLE)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Gets the app launcher from supplied operating-system observations.</summary>
    /// <param name="isLauncherVisible">true when the launcher is already visible.</param>
    /// <param name="findLauncher">Function returning the launcher handle.</param>
    /// <param name="createWindow">Function creating a window wrapper.</param>
    /// <returns>The app launcher window, or null.</returns>
    internal static IInteropWindow GetAppLauncher(bool isLauncherVisible, Func<IntPtr> findLauncher, Func<IntPtr, IInteropWindow> createWindow)
    {
        if (isLauncherVisible)
        {
            return null;
        }

        IntPtr appLauncher = findLauncher();
        if (appLauncher != IntPtr.Zero)
        {
            return createWindow(appLauncher);
        }

        return null;
    }

    /// <summary>Enumerates visible Windows Store app windows from supplied operating-system observations.</summary>
    /// <param name="appVisibility">App visibility COM wrapper, or null when unavailable.</param>
    /// <param name="appWindows">Candidate app windows.</param>
    /// <param name="findGutter">Function returning the gutter handle.</param>
    /// <param name="createWindow">Function creating a window wrapper.</param>
    /// <returns>The visible Windows Store app windows.</returns>
    internal static IEnumerable<IInteropWindow> EnumerateWindowsStoreApps(IAppVisibility appVisibility, IEnumerable<IInteropWindow> appWindows, Func<IntPtr> findGutter, Func<IntPtr, IInteropWindow> createWindow)
    {
        if (appVisibility is null)
        {
            yield break;
        }

        foreach (IInteropWindow currentAppWindow in appWindows)
        {
            if (currentAppWindow.IsApp())
            {
                yield return currentAppWindow;
            }
        }

        IntPtr gutterHandle = findGutter();
        if (gutterHandle != IntPtr.Zero)
        {
            yield return createWindow(gutterHandle);
        }
    }

    /// <summary>Enumerates candidate app windows by class name.</summary>
    /// <returns>The candidate app windows.</returns>
    private static IEnumerable<IInteropWindow> EnumerateAppWindows()
    {
        IntPtr nextHandle = User32Api.FindWindow(AppWindowIdentifierClass, null);
        while (nextHandle != IntPtr.Zero)
        {
            yield return InteropWindowFactory.CreateFor(nextHandle);
            nextHandle = User32Api.FindWindowEx(IntPtr.Zero, nextHandle, AppWindowIdentifierClass, null);
        }
    }

    /// <summary>Enumerates visible Windows Store app windows.</summary>
    /// <returns>The visible Windows Store app windows.</returns>
    private static IEnumerable<IInteropWindow> EnumerateWindowsStoreApps() => EnumerateWindowsStoreApps(AppVisibility?.ComObject, EnumerateAppWindows(), () => User32Api.FindWindow("ImmersiveGutter", null), InteropWindowFactory.CreateFor);

    /// <summary>Checks whether any child window has the specified class name.</summary>
    /// <param name="interopWindow">Window to inspect.</param>
    /// <param name="className">Class name to find.</param>
    /// <returns>true when a child with the class name is found.</returns>
    private static bool HasChildClass(IInteropWindow interopWindow, string className)
    {
        foreach (IInteropWindow child in interopWindow.GetChildren())
        {
            if (string.Equals(child.GetClassname(), className, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
