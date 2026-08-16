// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Interop.Com;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Apps;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Apps;
#endif
/// <summary>Helper class to support with Windows Store apps.</summary>
public static class AppQueryExtensions
{
    /// <summary>Provides Windows Store app classification operations for an interop window.</summary>
    /// <param name="interopWindow">The window to classify.</param>
    extension(IInteropWindow interopWindow)
    {
        /// <summary>Checks if the window is an App (Win8 or Win10).</summary>
        /// <returns>true when the window is an App.</returns>
        public bool IsApp() =>
            IsApp(WindowsVersion.IsWindows8OrLater, interopWindow.IsWin8App, interopWindow.IsWin10App);

        /// <summary>Tests if this window is for the App-Launcher.</summary>
        /// <returns>true when this window is for the App-Launcher.</returns>
        public bool IsAppLauncher() => AppLauncherClass.Equals(interopWindow.GetClassname());

        /// <summary>Checks if the window is the metro gutter (sizeable separator).</summary>
        /// <returns>true when the window is the metro gutter.</returns>
        public bool IsGutter() => GutterClass.Equals(interopWindow.GetClassname());

        /// <summary>Checks if the window is a Windows 10 App.</summary>
        /// <returns>true when the window is a Windows 10 App.</returns>
        public bool IsWin10App() => IsWin10App(WindowsVersion.IsWindows10OrLater, interopWindow);

        /// <summary>Checks if the window is a background Windows 10 App.</summary>
        /// <returns>true when the window is a background Windows 10 App.</returns>
        public bool IsBackgroundWin10App() =>
            WindowsVersion.IsWindows10OrLater
            && AppFrameWindowClass.Equals(interopWindow.GetClassname())
            && !HasChildClass(interopWindow, AppWindowClass);

        /// <summary>Checks if the window is a Windows 8 App, not Windows 10.</summary>
        /// <returns>true when the window is a Windows 8 App.</returns>
        public bool IsWin8App() => IsWin8App(WindowsVersion.IsWindows8 || WindowsVersion.IsWindows81, interopWindow);
    }

    /// <summary>Used for Windows 8(.1) and Windows 10 (but as child of "ApplicationFrameWindow").</summary>
    public static readonly string AppWindowClass = "Windows.UI.Core.CoreWindow";

    /// <summary>Windows 10 uses ApplicationFrameWindow to host the App.</summary>
    public static readonly string AppFrameWindowClass = "ApplicationFrameWindow";

    /// <summary>Window class name for the immersive launcher.</summary>
    private const string AppLauncherClass = "ImmersiveLauncher";

    /// <summary>Window class name for the immersive gutter.</summary>
    private const string GutterClass = "ImmersiveGutter";

    /// <summary>Window class for the App window; this depends on the Windows version.</summary>
    private static readonly string AppWindowIdentifierClass = GetAppWindowIdentifierClass(WindowsVersion.IsWindows8);

    /// <summary>COM class identifier used by <see cref="IAppVisibility" />.</summary>
    private static readonly Guid CoClassGuidIAppVisibility = new("7E5FE3D9-985F-4908-91F9-EE19F9FD1514");

    /// <summary>Creates the COM app visibility wrapper only when a public app query first needs it.</summary>
    private static readonly Lazy<IDisposableCom<IAppVisibility>> AppVisibility = new(CreateAppVisibility);

    /// <summary>Provides the cached app-launcher lookup delegate.</summary>
    private static readonly Func<IntPtr> FindAppLauncherOperation = FindAppLauncherWindow;

    /// <summary>Provides the cached interop-window factory delegate.</summary>
    private static readonly Func<IntPtr, IInteropWindow> CreateInteropWindowOperation = InteropWindowFactory.CreateFor;

    /// <summary>Provides the current app visibility wrapper.</summary>
    private static Func<IDisposableCom<IAppVisibility>> _appVisibilityProvider = static () => AppVisibility.Value;

    /// <summary>Provides the current display snapshot.</summary>
    private static Func<IReadOnlyList<DisplayInfo>> _displaySnapshotProvider = DisplayTopology.GetSnapshot;

    /// <summary>Provides native class-name window lookup.</summary>
    private static Func<string, string, IntPtr> _findWindowOperation = User32Api.FindWindow;

    /// <summary>Gets the windowHandle for the AppLauncer.</summary>
    public static IntPtr AppLauncher =>
        GetAppLauncherHandle(_appVisibilityProvider() is not null, FindAppLauncherOperation);

    /// <summary>Gets a value indicating whether the app-launcher is visible.</summary>
    public static bool IsLauncherVisible => GetLauncherVisibility(_appVisibilityProvider()?.ComObject.IsLauncherVisible());

    /// <summary>Gets handles of all Windows store apps.</summary>
    public static IEnumerable<IInteropWindow> WindowsStoreApps => EnumerateWindowsStoreApps();

    /// <summary>Check if a Windows Store App (WinRT) is visible.</summary>
    /// <param name="windowBounds">NativeRect.</param>
    /// <returns>true if an app, covering the supplied rect, is visible.</returns>
    public static bool AppVisible(NativeRect windowBounds) =>
        AppVisible(windowBounds, _appVisibilityProvider()?.ComObject, _displaySnapshotProvider(), GetMonitor);

    /// <summary>Get the AppLauncher.</summary>
    /// <returns>IInteropWindow.</returns>
    public static IInteropWindow GetAppLauncher() =>
        GetAppLauncher(IsLauncherVisible, FindAppLauncherOperation, CreateInteropWindowOperation);

    /// <summary>Determines whether an app is supported from supplied operating-system observations.</summary>
    /// <param name="isWindows8OrLater">Whether the current system supports Windows Store apps.</param>
    /// <param name="isWin8App">Determines whether the window is a Windows 8 app.</param>
    /// <param name="isWin10App">Determines whether the window is a Windows 10 app.</param>
    /// <returns>true when the window is a supported Windows Store app.</returns>
    internal static bool IsApp(bool isWindows8OrLater, Func<bool> isWin8App, Func<bool> isWin10App)
    {
        Throw.IfNull(isWin8App);
        Throw.IfNull(isWin10App);

        return isWindows8OrLater && (isWin8App() || isWin10App());
    }

    /// <summary>Determines whether a window is a Windows 10 app from supplied operating-system observations.</summary>
    /// <param name="isWindows10OrLater">Whether Windows 10 app hosting is available.</param>
    /// <param name="interopWindow">The window to classify.</param>
    /// <returns>true when the window is a Windows 10 app.</returns>
    internal static bool IsWin10App(bool isWindows10OrLater, IInteropWindow interopWindow)
    {
        Throw.IfNull(interopWindow);

        return isWindows10OrLater
            && (AppWindowClass.Equals(interopWindow.GetClassname()) || HasChildClass(interopWindow, AppWindowClass));
    }

    /// <summary>Determines whether a window is a Windows 8 app from supplied operating-system observations.</summary>
    /// <param name="isWindows8Or81">Whether Windows 8 or Windows 8.1 is active.</param>
    /// <param name="interopWindow">The window to classify.</param>
    /// <returns>true when the window is a Windows 8 app.</returns>
    internal static bool IsWin8App(bool isWindows8Or81, IInteropWindow interopWindow)
    {
        Throw.IfNull(interopWindow);

        return isWindows8Or81 && AppWindowClass.Equals(interopWindow.GetClassname());
    }

    /// <summary>Gets the class used to enumerate Windows Store app windows.</summary>
    /// <param name="isWindows8">Whether the current operating system is Windows 8.</param>
    /// <returns>The app-window class for the operating system.</returns>
    internal static string GetAppWindowIdentifierClass(bool isWindows8) =>
        isWindows8 ? AppWindowClass : AppFrameWindowClass;

    /// <summary>Gets an app-launcher handle when app visibility support is available.</summary>
    /// <param name="isAppVisibilityAvailable">Whether app visibility support is available.</param>
    /// <param name="findWindow">The native app-launcher lookup.</param>
    /// <returns>The launcher handle, or zero when unsupported.</returns>
    internal static IntPtr GetAppLauncherHandle(bool isAppVisibilityAvailable, Func<IntPtr> findWindow)
    {
        Throw.IfNull(findWindow);

        return isAppVisibilityAvailable ? findWindow() : IntPtr.Zero;
    }

    /// <summary>Gets the launcher-visibility result from a nullable native observation.</summary>
    /// <param name="isVisible">The native visibility result, if app visibility support is available.</param>
    /// <returns>true when the launcher is visible.</returns>
    internal static bool GetLauncherVisibility(bool? isVisible) => isVisible ?? false;

    /// <summary>Determines app visibility using supplied operating-system observations.</summary>
    /// <param name="windowBounds">Bounds of the window to check.</param>
    /// <param name="appVisibility">App visibility COM wrapper, or null when unavailable.</param>
    /// <param name="displays">Display snapshot to inspect.</param>
    /// <param name="monitorFromBounds">Function returning the monitor for fullscreen bounds.</param>
    /// <returns>true when the app should be treated as visible.</returns>
    internal static bool AppVisible(NativeRect windowBounds, IAppVisibility appVisibility, IReadOnlyList<DisplayInfo> displays, Func<NativeRect, IntPtr> monitorFromBounds) =>
        AppVisibleFromObservations(
            windowBounds,
            GetVisibilityOnMonitor(appVisibility),
            displays,
            monitorFromBounds);

    /// <summary>Gets the monitor-visibility operation from an app visibility wrapper.</summary>
    /// <param name="appVisibility">The app visibility wrapper, if available.</param>
    /// <returns>The monitor-visibility operation, or null.</returns>
    internal static Func<IntPtr, MonitorAppVisibility> GetVisibilityOnMonitor(IAppVisibility appVisibility) =>
        appVisibility is null ? null : appVisibility.GetAppVisibilityOnMonitor;

    /// <summary>Determines app visibility using supplied display and monitor-visibility observations.</summary>
    /// <param name="windowBounds">Bounds of the window to check.</param>
    /// <param name="getVisibilityOnMonitor">Function returning the app visibility for a monitor, or null when unavailable.</param>
    /// <param name="displays">Display snapshot to inspect.</param>
    /// <param name="monitorFromBounds">Function returning the monitor for fullscreen bounds.</param>
    /// <returns>true when the app should be treated as visible.</returns>
    internal static bool AppVisibleFromObservations(
        NativeRect windowBounds,
        Func<IntPtr, MonitorAppVisibility> getVisibilityOnMonitor,
        IReadOnlyList<DisplayInfo> displays,
        Func<NativeRect, IntPtr> monitorFromBounds)
    {
        if (getVisibilityOnMonitor is null)
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
                if (monitor != IntPtr.Zero && getVisibilityOnMonitor(monitor) == MonitorAppVisibility.MAV_APP_VISIBLE)
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
        IntPtr appLauncher = isLauncherVisible ? IntPtr.Zero : findLauncher();
        return appLauncher == IntPtr.Zero ? null : createWindow(appLauncher);
    }

    /// <summary>Enumerates visible Windows Store app windows from supplied operating-system observations.</summary>
    /// <param name="appVisibility">App visibility COM wrapper, or null when unavailable.</param>
    /// <param name="appWindows">Candidate app windows.</param>
    /// <param name="findGutter">Function returning the gutter handle.</param>
    /// <param name="createWindow">Function creating a window wrapper.</param>
    /// <returns>The visible Windows Store app windows.</returns>
    internal static IEnumerable<IInteropWindow> EnumerateWindowsStoreApps(
        IAppVisibility appVisibility,
        IEnumerable<IInteropWindow> appWindows,
        Func<IntPtr> findGutter,
        Func<IntPtr, IInteropWindow> createWindow) =>
        EnumerateWindowsStoreApps(appVisibility is not null, appWindows, findGutter, createWindow);

    /// <summary>Enumerates candidate app windows when app visibility support is available.</summary>
    /// <param name="isAppVisibilityAvailable">Whether the operating system exposes app visibility support.</param>
    /// <param name="appWindows">Candidate app windows.</param>
    /// <param name="findGutter">Function returning the gutter handle.</param>
    /// <param name="createWindow">Function creating a window wrapper.</param>
    /// <returns>The visible Windows Store app windows.</returns>
    internal static IEnumerable<IInteropWindow> EnumerateWindowsStoreApps(
        bool isAppVisibilityAvailable,
        IEnumerable<IInteropWindow> appWindows,
        Func<IntPtr> findGutter,
        Func<IntPtr, IInteropWindow> createWindow)
    {
        if (!isAppVisibilityAvailable)
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

    /// <summary>Creates the app visibility COM wrapper from a supplied activation operation.</summary>
    /// <param name="createInstance">The operation that activates the app visibility COM object.</param>
    /// <returns>The app visibility wrapper, or null when it is unavailable.</returns>
    internal static IDisposableCom<IAppVisibility> CreateAppVisibility(Func<Guid, object> createInstance)
    {
        Throw.IfNull(createInstance);

        if (!WindowsVersion.IsWindows8OrLater)
        {
            return null;
        }

        try
        {
            return DisposableCom.Create(
                (IAppVisibility)createInstance(CoClassGuidIAppVisibility));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Overrides public app-query dependencies while the returned scope is alive.</summary>
    /// <param name="appVisibilityProvider">Provides the app visibility wrapper.</param>
    /// <param name="displaySnapshotProvider">Provides the display snapshot.</param>
    /// <param name="findWindowOperation">Provides native class-name window lookup.</param>
    /// <returns>A scope that restores the previous dependencies.</returns>
    internal static IDisposable OverridePublicOperationsForTesting(
        Func<IDisposableCom<IAppVisibility>> appVisibilityProvider,
        Func<IReadOnlyList<DisplayInfo>> displaySnapshotProvider,
        Func<string, string, IntPtr> findWindowOperation)
    {
        Throw.IfNull(appVisibilityProvider);
        Throw.IfNull(displaySnapshotProvider);
        Throw.IfNull(findWindowOperation);
        var previous = (
            AppVisibilityProvider: _appVisibilityProvider,
            DisplaySnapshotProvider: _displaySnapshotProvider,
            FindWindowOperation: _findWindowOperation);
        _appVisibilityProvider = appVisibilityProvider;
        _displaySnapshotProvider = displaySnapshotProvider;
        _findWindowOperation = findWindowOperation;
        return Scope.Create(previous, static operations =>
        {
            _appVisibilityProvider = operations.AppVisibilityProvider;
            _displaySnapshotProvider = operations.DisplaySnapshotProvider;
            _findWindowOperation = operations.FindWindowOperation;
        });
    }

    /// <summary>Gets the monitor containing the supplied bounds.</summary>
    /// <param name="bounds">Bounds to locate.</param>
    /// <returns>The containing monitor handle.</returns>
    internal static IntPtr GetMonitor(NativeRect bounds) => User32Api.MonitorFromRect(ref bounds, MonitorFrom.None);

    /// <summary>Gets an app visibility COM object from its disposable wrapper.</summary>
    /// <param name="appVisibility">The disposable app visibility wrapper, if available.</param>
    /// <returns>The wrapped COM object, or null.</returns>
    internal static IAppVisibility GetAppVisibility(IDisposableCom<IAppVisibility> appVisibility) => appVisibility?.ComObject;

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
    private static IEnumerable<IInteropWindow> EnumerateWindowsStoreApps() =>
        EnumerateWindowsStoreApps(
            GetAppVisibility(_appVisibilityProvider()),
            EnumerateAppWindows(),
            static () => _findWindowOperation(GutterClass, null),
            InteropWindowFactory.CreateFor);

    /// <summary>Creates the app visibility COM wrapper when the OS supports it.</summary>
    /// <returns>The COM wrapper, or null when it is unavailable.</returns>
    private static IDisposableCom<IAppVisibility> CreateAppVisibility() =>
        CreateAppVisibility(static classIdentifier => Activator.CreateInstance(Type.GetTypeFromCLSID(classIdentifier)));

    /// <summary>Finds the immersive app-launcher window.</summary>
    /// <returns>The app-launcher window handle.</returns>
    private static IntPtr FindAppLauncherWindow() => _findWindowOperation(AppLauncherClass, null);

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
