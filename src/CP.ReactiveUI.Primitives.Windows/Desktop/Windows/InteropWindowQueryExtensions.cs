// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>Query for native windows.</summary>
public static class InteropWindowQueryExtensions
{
    /// <summary>Provides query predicates for a native window.</summary>
    /// <param name="interopWindow">The native window to inspect.</param>
    extension(IInteropWindow interopWindow)
    {
        /// <summary>Check the Classname of the IInteropWindow against a list of know classes which can be ignored.</summary>
        /// <returns>bool.</returns>
        public bool CanIgnoreClass()
        {
            var className = interopWindow.GetClassname();
            foreach (var ignoreClass in IgnoreClasses)
            {
                if (string.Equals(ignoreClass, className, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Is the specified window a visible popup.</summary>
        /// <returns>true if the IInteropWindow is a popup.</returns>
        public bool IsPopup() => interopWindow.IsPopup(ignoreKnowClasses: true);

        /// <summary>Is the specified window a visible popup.</summary>
        /// <param name="ignoreKnowClasses">true to ignore some known internal windows classes.</param>
        /// <returns>true if the IInteropWindow is a popup.</returns>
        public bool IsPopup(bool ignoreKnowClasses)
        {
            if (ignoreKnowClasses && interopWindow.CanIgnoreClass())
            {
                return false;
            }

            if (interopWindow.GetInfo().Bounds.IsEmpty)
            {
                return false;
            }

            if (interopWindow.GetParent() != IntPtr.Zero)
            {
                return false;
            }

            var windowInfo = interopWindow.GetInfo();
            var windowStyle = windowInfo.Style;
            if ((windowStyle & WindowStyleFlags.WS_POPUP) == 0)
            {
                return false;
            }

            var extendedWindowStyle = windowInfo.ExtendedStyle;
            return (interopWindow.IsWin8App()
                    || (extendedWindowStyle & ExtendedWindowStyleFlags.WS_EX_NOREDIRECTIONBITMAP) == 0)
                && !interopWindow.IsBackgroundWin10App()
                && (windowStyle & WindowStyleFlags.WS_VISIBLE) != WindowStyleFlags.None
                && !interopWindow.IsMinimized();
        }

        /// <summary>Check if the window is a top level window.</summary>
        /// <returns>bool.</returns>
        public bool IsTopLevel() => interopWindow.IsTopLevel(ignoreKnowClasses: true);

        /// <summary>Check if the window is a top level window.</summary>
        /// <param name="ignoreKnowClasses">true to ignore classes from the IgnoreClasses list.</param>
        /// <returns>bool.</returns>
        public bool IsTopLevel(bool ignoreKnowClasses)
        {
            if (ignoreKnowClasses && interopWindow.CanIgnoreClass())
            {
                return false;
            }

            var info = interopWindow.GetInfo();
            if (info.Bounds.IsEmpty)
            {
                return false;
            }

            if (interopWindow.GetParent() != IntPtr.Zero)
            {
                return false;
            }

            var extendedWindowStyle = info.ExtendedStyle;
            return HasTopLevelStyle(interopWindow, info, extendedWindowStyle);
        }
    }

    /// <summary>Default window classes which can be ignored.</summary>
    private static readonly string[] DefaultIgnoreClasses = ["Progman", "Button", "Dwm"];

    /// <summary>Gets window classes which can be ignored.</summary>
    public static ConcurrentBag<string> IgnoreClasses { get; } = new(DefaultIgnoreClasses);

    /// <summary>Get the window with which the user is currently working.</summary>
    /// <returns>IInteropWindow.</returns>
    public static IInteropWindow GetForegroundWindow() => InteropWindowFactory.CreateFor(User32Api.GetForegroundWindow());

    /// <summary>Gets the Desktop window.</summary>
    /// <returns>IInteropWindow for the desktop window.</returns>
    public static IInteropWindow GetDesktopWindow() => InteropWindowFactory.CreateFor(User32Api.GetDesktopWindow());

    /// <summary>Find windows belonging to the same process (thread) as the process ID.</summary>
    /// <param name="processId">int with process Id.</param>
    /// <returns>IEnumerable with IInteropWindow.</returns>
    public static IEnumerable<IInteropWindow> GetWindowsForProcess(int processId)
    {
        using Process process = Process.GetProcessById(processId);
        foreach (ProcessThread thread in process.Threads)
        {
            var handles = User32Api.EnumThreadWindows(thread.Id);
            thread.Dispose();
            foreach (var handle in handles)
            {
                yield return InteropWindowFactory.CreateFor(handle);
            }
        }
    }

    /// <summary>Iterate the Top level windows, from top to bottom.</summary>
    /// <returns>IEnumerable with all the top level windows.</returns>
    public static IEnumerable<IInteropWindow> GetTopLevelWindows() => GetTopLevelWindows(ignoreKnownClasses: true);

    /// <summary>Iterate the Top level windows, from top to bottom.</summary>
    /// <param name="ignoreKnownClasses">true to ignore windows with certain known classes.</param>
    /// <returns>IEnumerable with all the top level windows.</returns>
    public static IEnumerable<IInteropWindow> GetTopLevelWindows(bool ignoreKnownClasses)
    {
        foreach (var possibleTopLevel in GetTopWindows())
        {
            if (possibleTopLevel.IsTopLevel(ignoreKnownClasses))
            {
                yield return possibleTopLevel;
            }
        }
    }

    /// <summary>Iterate the windows, from top to bottom.</summary>
    /// <returns>IEnumerable with all the top level windows.</returns>
    public static IEnumerable<IInteropWindow> GetTopWindows() => GetTopWindows(null);

    /// <summary>Iterate the windows, from top to bottom.</summary>
    /// <param name="parent">InteropWindow as the parent, to iterate over the children, or null for all.</param>
    /// <returns>IEnumerable with all the top level windows.</returns>
    public static IEnumerable<IInteropWindow> GetTopWindows(IInteropWindow parent)
    {
        var windowPtr = parent is null
            ? User32Api.GetTopWindow(IntPtr.Zero)
            : User32Api.GetWindow(parent.Handle, GetWindowCommands.GW_CHILD);
        while (windowPtr != IntPtr.Zero)
        {
            yield return InteropWindowFactory.CreateFor(windowPtr);
            windowPtr = User32Api.GetWindow(windowPtr, GetWindowCommands.GW_HWNDNEXT);
        }
    }

    /// <summary>Checks style flags that determine whether a window is top-level.</summary>
    /// <param name="interopWindow">Window to inspect.</param>
    /// <param name="info">Window information.</param>
    /// <param name="extendedWindowStyle">Extended window style.</param>
    /// <returns>true when the style flags describe a top-level window.</returns>
    private static bool HasTopLevelStyle(IInteropWindow interopWindow, WindowInfo info, ExtendedWindowStyleFlags extendedWindowStyle) =>
        (extendedWindowStyle & ExtendedWindowStyleFlags.WS_EX_TOOLWINDOW) == 0
            && (interopWindow.IsWin8App()
                || (extendedWindowStyle & ExtendedWindowStyleFlags.WS_EX_NOREDIRECTIONBITMAP) == 0)
            && !interopWindow.IsBackgroundWin10App()
            && (info.Style & WindowStyleFlags.WS_VISIBLE) != WindowStyleFlags.None
            && interopWindow.GetCaption().Length != 0
            && !interopWindow.IsMinimized();
}
