// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Extensions;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using ReactiveUI.Primitives.Disposables;
using log4net;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
#endif
/// <summary>Provides managed wrappers for native DPI-related Win32 API methods.</summary>
public static class NativeDpiMethods
{
    /// <summary>Native DPI entry points.</summary>
    internal static class NativeMethods
    {
        /// <summary>Lazy-loaded Shcore module handle.</summary>
        private static readonly Lazy<IntPtr> ShcoreModule = new(() => System.Runtime.InteropServices.NativeLibrary.Load("shcore.dll", typeof(NativeMethods).Assembly, DllImportSearchPath.System32));

        /// <summary>Lazy-loaded User32 module handle.</summary>
        private static readonly Lazy<IntPtr> User32Module = new(() => System.Runtime.InteropServices.NativeLibrary.Load("user32.dll", typeof(NativeMethods).Assembly, DllImportSearchPath.System32));

        /// <summary>Provides Shcore export pointers.</summary>
        private static Func<string, IntPtr> _shcoreExportProvider = (exportName) => System.Runtime.InteropServices.NativeLibrary.GetExport(ShcoreModule.Value, exportName);

        /// <summary>Provides User32 export pointers.</summary>
        private static Func<string, IntPtr> _user32ExportProvider = (exportName) => System.Runtime.InteropServices.NativeLibrary.GetExport(User32Module.Value, exportName);

        /// <summary>Retrieves the DPI awareness for a process.</summary>
        /// <param name="processHandle">The process handle to query, or zero for the current process.</param>
        /// <param name="value">The process DPI awareness value.</param>
        /// <returns>The HRESULT returned by the native API.</returns>
        internal static unsafe HResult GetProcessDpiAwareness(IntPtr processHandle, out DpiAwareness value)
        {
            fixed (DpiAwareness* valuePointer = &value)
            {
                return (HResult)((delegate* unmanaged[Stdcall]<IntPtr, DpiAwareness*, uint>)(void*)GetShcoreExport("GetProcessDpiAwareness"))(processHandle, valuePointer);
            }
        }

        /// <summary>Sets the process DPI awareness.</summary>
        /// <param name="dpiAwareness">The DPI awareness value.</param>
        /// <returns>The HRESULT returned by the native API.</returns>
        internal static unsafe HResult SetProcessDpiAwareness(DpiAwareness dpiAwareness) => ((delegate* unmanaged[Stdcall]<DpiAwareness, HResult>)(void*)GetShcoreExport("SetProcessDpiAwareness"))(dpiAwareness);

        /// <summary>Sets the process DPI awareness context.</summary>
        /// <param name="dpiAwarenessContext">The DPI awareness context.</param>
        /// <returns><see langword="true" /> when the native call succeeds.</returns>
        internal static unsafe bool SetProcessDpiAwarenessContext(DpiAwarenessContext dpiAwarenessContext) => ((delegate* unmanaged[Stdcall]<IntPtr, int>)(void*)GetUser32Export("SetProcessDpiAwarenessContext"))(ToDpiAwarenessContextHandle(dpiAwarenessContext)) != 0;

        /// <summary>Retrieves the DPI for a window.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <returns>The window DPI.</returns>
        internal static unsafe uint GetDpiForWindow(IntPtr windowHandle) => ((delegate* unmanaged[Stdcall]<IntPtr, uint>)(void*)GetUser32Export("GetDpiForWindow"))(windowHandle);

        /// <summary>Retrieves monitor DPI information.</summary>
        /// <param name="monitorHandle">The monitor handle.</param>
        /// <param name="dpiType">The DPI type to query.</param>
        /// <param name="dpiX">The horizontal DPI.</param>
        /// <param name="dpiY">The vertical DPI.</param>
        /// <returns>The HRESULT returned by the native API.</returns>
        internal static unsafe HResult GetDpiForMonitor(IntPtr monitorHandle, MonitorDpiType dpiType, out uint dpiX, out uint dpiY)
        {
            fixed (uint* dpiXPointer = &dpiX)
            {
                fixed (uint* dpiYPointer = &dpiY)
                {
                    return (HResult)((delegate* unmanaged[Stdcall]<IntPtr, MonitorDpiType, uint*, uint*, uint>)(void*)GetShcoreExport("GetDpiForMonitor"))(monitorHandle, dpiType, dpiXPointer, dpiYPointer);
                }
            }
        }

        /// <summary>Enables non-client DPI scaling for a top-level window.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <returns><see langword="true" /> when the native call succeeds.</returns>
        internal static unsafe bool EnableNonClientDpiScaling(IntPtr windowHandle) => ((delegate* unmanaged[Stdcall]<IntPtr, int>)(void*)GetUser32Export("EnableNonClientDpiScaling"))(windowHandle) != 0;

        /// <summary>Retrieves the system DPI.</summary>
        /// <returns>The system DPI.</returns>
        internal static unsafe uint GetDpiForSystem() => ((delegate* unmanaged[Stdcall]<uint>)(void*)GetUser32Export("GetDpiForSystem"))();

        /// <summary>Retrieves a system parameter for the supplied DPI.</summary>
        /// <param name="action">The system parameter action.</param>
        /// <param name="uiParameter">The action-specific unsigned integer parameter.</param>
        /// <param name="parameter">A pointer to the action-specific parameter buffer.</param>
        /// <param name="updateProfileFlags">Flags that control profile update behavior.</param>
        /// <param name="dpi">The DPI value.</param>
        /// <returns><see langword="true" /> when the native call succeeds.</returns>
        internal static unsafe bool SystemParametersInfoForDpi(SystemParametersInfoActions action, uint uiParameter, IntPtr parameter, SystemParametersInfoBehaviors updateProfileFlags, uint dpi) => ((delegate* unmanaged[Stdcall]<SystemParametersInfoActions, uint, IntPtr, SystemParametersInfoBehaviors, uint, int>)(void*)GetUser32Export("SystemParametersInfoForDpi"))(action, uiParameter, parameter, updateProfileFlags, dpi) != 0;

        /// <summary>Retrieves the current thread DPI awareness context.</summary>
        /// <returns>The current thread DPI awareness context.</returns>
        internal static unsafe DpiAwarenessContext GetThreadDpiAwarenessContext() => FromDpiAwarenessContextHandle(((delegate* unmanaged[Stdcall]<IntPtr>)(void*)GetUser32Export("GetThreadDpiAwarenessContext"))());

        /// <summary>Sets the current thread DPI awareness context.</summary>
        /// <param name="dpiAwarenessContext">The new DPI awareness context.</param>
        /// <returns>The previous DPI awareness context.</returns>
        internal static unsafe DpiAwarenessContext SetThreadDpiAwarenessContext(DpiAwarenessContext dpiAwarenessContext) => FromDpiAwarenessContextHandle(((delegate* unmanaged[Stdcall]<IntPtr, IntPtr>)(void*)GetUser32Export("SetThreadDpiAwarenessContext"))(ToDpiAwarenessContextHandle(dpiAwarenessContext)));

        /// <summary>Retrieves the DPI awareness value from a DPI awareness context.</summary>
        /// <param name="dpiAwarenessContext">The DPI awareness context.</param>
        /// <returns>The DPI awareness value.</returns>
        internal static unsafe DpiAwareness GetAwarenessFromDpiAwarenessContext(DpiAwarenessContext dpiAwarenessContext) => ((delegate* unmanaged[Stdcall]<IntPtr, DpiAwareness>)(void*)GetUser32Export("GetAwarenessFromDpiAwarenessContext"))(ToDpiAwarenessContextHandle(dpiAwarenessContext));

        /// <summary>Retrieves the DPI from a DPI awareness context.</summary>
        /// <param name="dpiAwarenessContext">The DPI awareness context.</param>
        /// <returns>The DPI value.</returns>
        internal static unsafe uint GetDpiFromDpiAwarenessContext(DpiAwarenessContext dpiAwarenessContext) => ((delegate* unmanaged[Stdcall]<IntPtr, uint>)(void*)GetUser32Export("GetDpiFromDpiAwarenessContext"))(ToDpiAwarenessContextHandle(dpiAwarenessContext));

        /// <summary>Determines whether a DPI awareness context is valid.</summary>
        /// <param name="dpiAwarenessContext">The DPI awareness context.</param>
        /// <returns><see langword="true" /> when the context is valid.</returns>
        internal static unsafe bool IsValidDpiAwarenessContext(DpiAwarenessContext dpiAwarenessContext) => ((delegate* unmanaged[Stdcall]<IntPtr, int>)(void*)GetUser32Export("IsValidDpiAwarenessContext"))(ToDpiAwarenessContextHandle(dpiAwarenessContext)) != 0;

        /// <summary>Retrieves the DPI hosting behavior for a window.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <returns>The DPI hosting behavior.</returns>
        internal static unsafe DpiHostingBehavior GetWindowDpiHostingBehavior(IntPtr windowHandle) => ((delegate* unmanaged[Stdcall]<IntPtr, DpiHostingBehavior>)(void*)GetUser32Export("GetWindowDpiHostingBehavior"))(windowHandle);

        /// <summary>Sets the current thread DPI hosting behavior.</summary>
        /// <param name="dpiHostingBehavior">The new DPI hosting behavior.</param>
        /// <returns>The previous DPI hosting behavior.</returns>
        internal static unsafe DpiHostingBehavior SetThreadDpiHostingBehavior(DpiHostingBehavior dpiHostingBehavior) => ((delegate* unmanaged[Stdcall]<DpiHostingBehavior, DpiHostingBehavior>)(void*)GetUser32Export("SetThreadDpiHostingBehavior"))(dpiHostingBehavior);

        /// <summary>Retrieves the current thread DPI hosting behavior.</summary>
        /// <returns>The current thread DPI hosting behavior.</returns>
        internal static unsafe DpiHostingBehavior GetThreadDpiHostingBehavior() => ((delegate* unmanaged[Stdcall]<DpiHostingBehavior>)(void*)GetUser32Export("GetThreadDpiHostingBehavior"))();

        /// <summary>Sets dialog control DPI change behavior.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="mask">A mask specifying the subset of flags to change.</param>
        /// <param name="values">The desired value for the specified subset of flags.</param>
        /// <returns><see langword="true" /> when the native call succeeds.</returns>
        internal static unsafe bool SetDialogControlDpiChangeBehavior(IntPtr windowHandle, DialogScalingBehaviors mask, DialogScalingBehaviors values) => ((delegate* unmanaged[Stdcall]<IntPtr, DialogScalingBehaviors, DialogScalingBehaviors, int>)(void*)GetUser32Export("SetDialogControlDpiChangeBehavior"))(windowHandle, mask, values) != 0;

        /// <summary>Retrieves dialog control DPI change behavior.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <returns>The dialog scaling behavior.</returns>
        internal static unsafe DialogScalingBehaviors GetDialogControlDpiChangeBehavior(IntPtr windowHandle) => ((delegate* unmanaged[Stdcall]<IntPtr, DialogScalingBehaviors>)(void*)GetUser32Export("GetDialogControlDpiChangeBehavior"))(windowHandle);

        /// <summary>Retrieves a system metric for a supplied DPI.</summary>
        /// <param name="index">The system metric or configuration setting to retrieve.</param>
        /// <param name="dpi">The DPI to use for scaling.</param>
        /// <returns>The requested system metric or configuration setting.</returns>
        internal static unsafe int GetSystemMetricsForDpi(SystemMetric index, uint dpi) => ((delegate* unmanaged[Stdcall]<SystemMetric, uint, int>)(void*)GetUser32Export("GetSystemMetricsForDpi"))(index, dpi);

        /// <summary>Adjusts a window rectangle for the provided DPI.</summary>
        /// <param name="rect">The desired client rectangle.</param>
        /// <param name="style">The window style.</param>
        /// <param name="hasMenu">A value indicating whether the window has a menu.</param>
        /// <param name="extendedStyle">The extended window style.</param>
        /// <param name="dpi">The DPI to use for scaling.</param>
        /// <returns><see langword="true" /> when the native call succeeds.</returns>
        internal static unsafe bool AdjustWindowRectExForDpi(NativeRect* rect, WindowStyleFlags style, bool hasMenu, ExtendedWindowStyleFlags extendedStyle, uint dpi) => ((delegate* unmanaged[Stdcall]<NativeRect*, WindowStyleFlags, int, ExtendedWindowStyleFlags, uint, int>)(void*)GetUser32Export("AdjustWindowRectExForDpi"))(rect, style, hasMenu ? 1 : 0, extendedStyle, dpi) != 0;

        /// <summary>Converts a point from logical coordinates to physical coordinates.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="point">The point to convert.</param>
        /// <returns><see langword="true" /> when the native call succeeds.</returns>
        internal static unsafe bool LogicalToPhysicalPointForPerMonitorDPI(IntPtr windowHandle, NativePoint* point) => ((delegate* unmanaged[Stdcall]<IntPtr, NativePoint*, int>)(void*)GetUser32Export("LogicalToPhysicalPointForPerMonitorDPI"))(windowHandle, point) != 0;

        /// <summary>Converts a point from physical coordinates to logical coordinates.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="point">The point to convert.</param>
        /// <returns><see langword="true" /> when the native call succeeds.</returns>
        internal static unsafe bool PhysicalToLogicalPointForPerMonitorDPI(IntPtr windowHandle, NativePoint* point) => ((delegate* unmanaged[Stdcall]<IntPtr, NativePoint*, int>)(void*)GetUser32Export("PhysicalToLogicalPointForPerMonitorDPI"))(windowHandle, point) != 0;

        /// <summary>Exchanges native export providers for deterministic testing.</summary>
        /// <param name="shcoreExportProvider">The replacement Shcore export provider.</param>
        /// <param name="user32ExportProvider">The replacement User32 export provider.</param>
        /// <returns>A disposable scope that restores the previous export providers.</returns>
        internal static IDisposable ExchangeExportProviders(Func<string, IntPtr> shcoreExportProvider, Func<string, IntPtr> user32ExportProvider)
        {
            CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(shcoreExportProvider);
            CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(user32ExportProvider);
            Func<string, IntPtr> shcoreExportProvider2 = _shcoreExportProvider;
            Func<string, IntPtr> previousUser32ExportProvider = _user32ExportProvider;
            _shcoreExportProvider = shcoreExportProvider;
            _user32ExportProvider = user32ExportProvider;
            return Scope.Create((shcoreExportProvider2, previousUser32ExportProvider), delegate((Func<string, IntPtr> previousShcoreExportProvider, Func<string, IntPtr> previousUser32ExportProvider) previous)
            {
                (_shcoreExportProvider, _user32ExportProvider) = previous;
            });
        }

        /// <summary>Converts a native DPI awareness context handle to the managed context value.</summary>
        /// <param name="dpiAwarenessContext">The native DPI awareness context handle.</param>
        /// <returns>The managed DPI awareness context value.</returns>
        private static DpiAwarenessContext FromDpiAwarenessContextHandle(IntPtr dpiAwarenessContext) => (DpiAwarenessContext)dpiAwarenessContext.ToInt64();

        /// <summary>Retrieves a Shcore export pointer.</summary>
        /// <param name="exportName">The export name.</param>
        /// <returns>The export pointer.</returns>
        private static IntPtr GetShcoreExport(string exportName) => _shcoreExportProvider(exportName);

        /// <summary>Retrieves a User32 export pointer.</summary>
        /// <param name="exportName">The export name.</param>
        /// <returns>The export pointer.</returns>
        private static IntPtr GetUser32Export(string exportName) => _user32ExportProvider(exportName);

        /// <summary>Converts a managed DPI awareness context to a native pointer-sized handle.</summary>
        /// <param name="dpiAwarenessContext">The managed DPI awareness context value.</param>
        /// <returns>The native DPI awareness context handle.</returns>
        private static IntPtr ToDpiAwarenessContextHandle(DpiAwarenessContext dpiAwarenessContext) => (IntPtr)(int)dpiAwarenessContext;
    }

    /// <summary>The first Windows 10 build that supports thread DPI awareness contexts.</summary>
    private const int Windows10AnniversaryUpdateBuild = 14_393;

    /// <summary>The first Windows 10 build that supports per-monitor V2 process awareness.</summary>
    private const int Windows10CreatorsUpdateBuild = 15_063;

    /// <summary>The logger for native DPI operations.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(NativeDpiMethods));

    /// <summary>The current native DPI API implementation.</summary>
    private static INativeDpiApi _api = WindowsNativeDpiApi.Instance;

    /// <summary>Gets whether Windows 8.1 or later APIs are available.</summary>
    private static Func<bool> _isWindows81OrLater = () => WindowsVersion.IsWindows81OrLater;

    /// <summary>Gets whether Windows 10 or later APIs are available.</summary>
    private static Func<bool> _isWindows10OrLater = () => WindowsVersion.IsWindows10OrLater;

    /// <summary>Gets whether a specific Windows 10 build or later is available.</summary>
    private static Func<int, bool> _isWindows10BuildOrLater = WindowsVersion.IsWindows10BuildOrLater;

    /// <summary>Gets a value indicating whether the current process is DPI aware.</summary>
    public static bool IsDpiAware
    {
        get
        {
            if (!_isWindows81OrLater())
            {
                Log.Debug("An application can only be DPI aware starting with Window 8.1 and later.");
                return false;
            }

            using Process process = Process.GetCurrentProcess();
            _ = GetProcessDpiAwareness(process.Handle, out var dpiAwareness);
            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("Process {0} has a Dpi awareness {1}", process.ProcessName, dpiAwareness);
            }

            return dpiAwareness is not DpiAwareness.Unaware and not DpiAwareness.Invalid;
        }
    }

    /// <summary>Make the current process DPI aware.</summary>
    /// <returns><see langword="true" /> if it was possible to change the DPI awareness.</returns>
    public static bool EnableDpiAware()
    {
        if (!_isWindows81OrLater())
        {
            Log.Debug("An application can only be DPI aware starting with Window 8.1 and later.");
            return false;
        }

        if (_isWindows10BuildOrLater(15_063))
        {
            _ = SetProcessDpiAwarenessContext(DpiAwarenessContext.PerMonitorAwareV2);
            return true;
        }

        return SetProcessDpiAwareness(DpiAwareness.PerMonitorAware).Succeeded();
    }

    /// <summary>Retrieve the DPI value for the supplied window handle.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The DPI value.</returns>
    public static int GetDpi(IntPtr windowHandle) => _api.GetDpi(windowHandle);

    /// <summary>Return the DPI for the screen which the location is located on.</summary>
    /// <param name="location">The location to inspect.</param>
    /// <returns>The DPI value.</returns>
    public static int GetDpi(NativePoint location) => _api.GetDpi(location);

    /// <summary>Create a scope which enables the default DPI-aware context.</summary>
    /// <returns>A disposable scope that restores the previous context.</returns>
    public static IDisposable DefaultScopedThreadDpiAwarenessContext() => _api.DefaultScopedThreadDpiAwarenessContext();

    /// <summary>Create a scope for the DpiAwarenessContext.</summary>
    /// <param name="dpiAwarenessContext">The primary DPI awareness context.</param>
    /// <returns>A disposable scope that restores the previous context.</returns>
    public static IDisposable ScopedThreadDpiAwarenessContext(DpiAwarenessContext dpiAwarenessContext) => ScopedThreadDpiAwarenessContext(dpiAwarenessContext, null);

    /// <summary>Create a scope for the DpiAwarenessContext.</summary>
    /// <param name="dpiAwarenessContext">The primary DPI awareness context.</param>
    /// <param name="alternativeAwarenessContext">The fallback DPI awareness context.</param>
    /// <returns>A disposable scope that restores the previous context.</returns>
    public static IDisposable ScopedThreadDpiAwarenessContext(DpiAwarenessContext dpiAwarenessContext, DpiAwarenessContext? alternativeAwarenessContext)
    {
        if (!_isWindows10BuildOrLater(14_393))
        {
            return Scope.Empty;
        }

        DpiAwarenessContext? previousDpiAwarenessContext = null;
        if (IsValidDpiAwarenessContext(dpiAwarenessContext))
        {
            previousDpiAwarenessContext = SetThreadDpiAwarenessContext(dpiAwarenessContext);
        }
        else if (alternativeAwarenessContext.HasValue && IsValidDpiAwarenessContext(alternativeAwarenessContext.Value))
        {
            previousDpiAwarenessContext = SetThreadDpiAwarenessContext(alternativeAwarenessContext.Value);
        }

        if (!previousDpiAwarenessContext.HasValue)
        {
            return Scope.Empty;
        }

        return Scope.Create(previousDpiAwarenessContext.Value, delegate(DpiAwarenessContext context)
        {
            _ = SetThreadDpiAwarenessContext(context);
        });
    }

    /// <summary>Retrieves the DPI awareness of the specified process.</summary>
    /// <param name="processHandle">The process handle to query, or zero for the current process.</param>
    /// <param name="value">The process DPI awareness value.</param>
    /// <returns>The HRESULT returned by the Win32 API.</returns>
    public static HResult GetProcessDpiAwareness(IntPtr processHandle, out DpiAwareness value) => NativeMethods.GetProcessDpiAwareness(processHandle, out value);

    /// <summary>Sets the current process to a specified dots per inch awareness level.</summary>
    /// <param name="dpiAwareness">The DPI awareness value.</param>
    /// <returns>The HRESULT returned by the Win32 API.</returns>
    public static HResult SetProcessDpiAwareness(DpiAwareness dpiAwareness) => NativeMethods.SetProcessDpiAwareness(dpiAwareness);

    /// <summary>Sets the current process to a specified DPI awareness context.</summary>
    /// <param name="dpiAwarenessContext">The DPI awareness context.</param>
    /// <returns><see langword="true" /> when the function succeeds.</returns>
    public static bool SetProcessDpiAwarenessContext(DpiAwarenessContext dpiAwarenessContext) => NativeMethods.SetProcessDpiAwarenessContext(dpiAwarenessContext);

    /// <summary>Returns the DPI value for the associated window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The DPI value.</returns>
    public static uint GetDpiForWindow(IntPtr windowHandle) => NativeMethods.GetDpiForWindow(windowHandle);

    /// <summary>Queries the dots per inch of a display.</summary>
    /// <param name="monitorHandle">The monitor handle.</param>
    /// <param name="dpiType">The DPI type to query.</param>
    /// <param name="dpiX">The horizontal DPI.</param>
    /// <param name="dpiY">The vertical DPI.</param>
    /// <returns>The HRESULT returned by the Win32 API.</returns>
    public static HResult GetDpiForMonitor(IntPtr monitorHandle, MonitorDpiType dpiType, out uint dpiX, out uint dpiY) => NativeMethods.GetDpiForMonitor(monitorHandle, dpiType, out dpiX, out dpiY);

    /// <summary>Enables automatic DPI scaling for the non-client area of the specified top-level window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns><see cref="F:CP.ReactiveUI.Primitives.Windows.Native.Enums.HResult.Ok" /> when the function succeeds; otherwise <see cref="F:CP.ReactiveUI.Primitives.Windows.Native.Enums.HResult.Fail" />.</returns>
    public static HResult EnableNonClientDpiScaling(IntPtr windowHandle) => _api.EnableNonClientDpiScaling(windowHandle);

    /// <summary>Returns the system DPI.</summary>
    /// <returns>The system DPI.</returns>
    public static uint GetDpiForSystem() => _api.GetDpiForSystem();

    /// <summary>Converts a point in a window from logical coordinates into physical coordinates.</summary>
    /// <param name="windowHandle">The handle to the window whose transform is used for the conversion.</param>
    /// <param name="point">The point to convert.</param>
    /// <returns><see langword="true" /> when the function succeeds.</returns>
    public static unsafe bool LogicalToPhysicalPointForPerMonitorDPI(IntPtr windowHandle, ref NativePoint point)
    {
        fixed (NativePoint* pointPointer = &point)
        {
            return NativeMethods.LogicalToPhysicalPointForPerMonitorDPI(windowHandle, pointPointer);
        }
    }

    /// <summary>Converts a point in a window from physical coordinates into logical coordinates.</summary>
    /// <param name="windowHandle">The handle to the window whose transform is used for the conversion.</param>
    /// <param name="point">The point to convert.</param>
    /// <returns><see langword="true" /> when the function succeeds.</returns>
    public static unsafe bool PhysicalToLogicalPointForPerMonitorDPI(IntPtr windowHandle, ref NativePoint point)
    {
        fixed (NativePoint* pointPointer = &point)
        {
            return NativeMethods.PhysicalToLogicalPointForPerMonitorDPI(windowHandle, pointPointer);
        }
    }

    /// <summary>Retrieves the value of one of the system-wide parameters for the supplied DPI.</summary>
    /// <param name="action">The system-wide parameter to retrieve.</param>
    /// <param name="uiParameter">An action-specific unsigned integer parameter.</param>
    /// <param name="parameter">A pointer to the action-specific parameter buffer.</param>
    /// <param name="updateProfileFlags">Flags that control profile update behavior.</param>
    /// <param name="dpi">The DPI value.</param>
    /// <returns><see langword="true" /> when the function succeeds.</returns>
    public static bool SystemParametersInfoForDpi(SystemParametersInfoActions action, uint uiParameter, IntPtr parameter, SystemParametersInfoBehaviors updateProfileFlags, uint dpi) => _api.SystemParametersInfoForDpi(action, uiParameter, parameter, updateProfileFlags, dpi);

    /// <summary>Gets the DPI awareness context for the current thread.</summary>
    /// <returns>The DPI awareness context.</returns>
    public static DpiAwarenessContext GetThreadDpiAwarenessContext() => NativeMethods.GetThreadDpiAwarenessContext();

    /// <summary>Set the DPI awareness for the current thread to the provided value.</summary>
    /// <param name="dpiAwarenessContext">The new value for the current thread.</param>
    /// <returns>The previous DPI awareness context.</returns>
    public static DpiAwarenessContext SetThreadDpiAwarenessContext(DpiAwarenessContext dpiAwarenessContext) => NativeMethods.SetThreadDpiAwarenessContext(dpiAwarenessContext);

    /// <summary>Retrieves the DpiAwareness value from a DpiAwarenessContext.</summary>
    /// <param name="dpiAwarenessContext">The DPI awareness context.</param>
    /// <returns>The DPI awareness value.</returns>
    public static DpiAwareness GetAwarenessFromDpiAwarenessContext(DpiAwarenessContext dpiAwarenessContext) => NativeMethods.GetAwarenessFromDpiAwarenessContext(dpiAwarenessContext);

    /// <summary>Retrieves the DPI from a given DPI awareness context handle.</summary>
    /// <param name="dpiAwarenessContext">The DPI awareness context.</param>
    /// <returns>The DPI value.</returns>
    public static uint GetDpiFromDpiAwarenessContext(DpiAwarenessContext dpiAwarenessContext) => NativeMethods.GetDpiFromDpiAwarenessContext(dpiAwarenessContext);

    /// <summary>Determines if a specified DPI awareness context is valid and supported.</summary>
    /// <param name="dpiAwarenessContext">The context to test.</param>
    /// <returns><see langword="true" /> when the context is supported.</returns>
    public static bool IsValidDpiAwarenessContext(DpiAwarenessContext dpiAwarenessContext) => NativeMethods.IsValidDpiAwarenessContext(dpiAwarenessContext);

    /// <summary>Returns the DPI hosting behavior of the specified window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The DPI hosting behavior.</returns>
    public static DpiHostingBehavior GetWindowDpiHostingBehavior(IntPtr windowHandle) => NativeMethods.GetWindowDpiHostingBehavior(windowHandle);

    /// <summary>Sets the thread's DPI hosting behavior.</summary>
    /// <param name="dpiHostingBehavior">The new DPI hosting behavior.</param>
    /// <returns>The previous DPI hosting behavior.</returns>
    public static DpiHostingBehavior SetThreadDpiHostingBehavior(DpiHostingBehavior dpiHostingBehavior) => NativeMethods.SetThreadDpiHostingBehavior(dpiHostingBehavior);

    /// <summary>Retrieves the DPI hosting behavior from the current thread.</summary>
    /// <returns>The DPI hosting behavior.</returns>
    public static DpiHostingBehavior GetThreadDpiHostingBehavior() => NativeMethods.GetThreadDpiHostingBehavior();

    /// <summary>Overrides the per-monitor DPI scaling behavior of a child window in a dialog.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="mask">A mask specifying the subset of flags to change.</param>
    /// <param name="values">The desired value for the specified subset of flags.</param>
    /// <returns><see langword="true" /> when the function succeeds.</returns>
    public static bool SetDialogControlDpiChangeBehavior(IntPtr windowHandle, DialogScalingBehaviors mask, DialogScalingBehaviors values) => NativeMethods.SetDialogControlDpiChangeBehavior(windowHandle, mask, values);

    /// <summary>Retrieves per-monitor DPI scaling behavior overrides of a child window in a dialog.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The dialog scaling behavior.</returns>
    public static DialogScalingBehaviors GetDialogControlDpiChangeBehavior(IntPtr windowHandle) => NativeMethods.GetDialogControlDpiChangeBehavior(windowHandle);

    /// <summary>Retrieves the value of one of the system metrics for the supplied DPI.</summary>
    /// <param name="index">The system metric or configuration setting to retrieve.</param>
    /// <param name="dpi">The DPI to use for scaling.</param>
    /// <returns>The requested system metric or configuration setting.</returns>
    public static int GetSystemMetricsForDpi(SystemMetric index, uint dpi) => _api.GetSystemMetricsForDpi(index, dpi);

    /// <summary>Calculates the required size of a window rectangle for the provided DPI.</summary>
    /// <param name="rect">The desired client rectangle.</param>
    /// <param name="style">The window style.</param>
    /// <param name="hasMenu">A value indicating whether the window has a menu.</param>
    /// <param name="extendedStyle">The extended window style.</param>
    /// <param name="dpi">The DPI to use for scaling.</param>
    /// <returns><see langword="true" /> when the function succeeds.</returns>
    public static bool AdjustWindowRectExForDpi(ref NativeRect rect, WindowStyleFlags style, bool hasMenu, ExtendedWindowStyleFlags extendedStyle, uint dpi) => _api.AdjustWindowRectExForDpi(ref rect, style, hasMenu, extendedStyle, dpi);

    /// <summary>Exchanges the native DPI API for testing.</summary>
    /// <param name="api">The replacement native DPI API.</param>
    /// <returns>The previous native DPI API.</returns>
    internal static INativeDpiApi ExchangeApi(INativeDpiApi api)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(api);
        INativeDpiApi api2 = _api;
        _api = api;
        return api2;
    }

    /// <summary>Exchanges platform-version checks for deterministic testing.</summary>
    /// <param name="isWindows81OrLater">A function that returns whether Windows 8.1 or later APIs are available.</param>
    /// <param name="isWindows10OrLater">A function that returns whether Windows 10 or later APIs are available.</param>
    /// <param name="isWindows10BuildOrLater">A function that returns whether the supplied Windows 10 build or later is available.</param>
    /// <returns>A disposable scope that restores the previous platform checks.</returns>
    internal static IDisposable ExchangePlatform(Func<bool> isWindows81OrLater, Func<bool> isWindows10OrLater, Func<int, bool> isWindows10BuildOrLater)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(isWindows81OrLater);
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(isWindows10OrLater);
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(isWindows10BuildOrLater);
        Func<bool> isWindows81OrLater2 = _isWindows81OrLater;
        Func<bool> previousIsWindows10OrLater = _isWindows10OrLater;
        Func<int, bool> previousIsWindows10BuildOrLater = _isWindows10BuildOrLater;
        _isWindows81OrLater = isWindows81OrLater;
        _isWindows10OrLater = isWindows10OrLater;
        _isWindows10BuildOrLater = isWindows10BuildOrLater;
        return Scope.Create((isWindows81OrLater2, previousIsWindows10OrLater, previousIsWindows10BuildOrLater), delegate((Func<bool> previousIsWindows81OrLater, Func<bool> previousIsWindows10OrLater, Func<int, bool> previousIsWindows10BuildOrLater) previous)
        {
            (_isWindows81OrLater, _isWindows10OrLater, _isWindows10BuildOrLater) = previous;
        });
    }

    /// <summary>Retrieve the DPI value for the supplied window handle.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The DPI value.</returns>
    internal static int GetDpiCore(IntPtr windowHandle)
    {
        if (!User32Api.IsWindow(windowHandle))
        {
            return DpiCalculator.DefaultScreenDpi;
        }

        checked
        {
            if (_isWindows10OrLater())
            {
                return (int)GetDpiForWindow(windowHandle);
            }

            if (_isWindows81OrLater() && GetDpiForMonitor(User32Api.MonitorFromWindow(windowHandle, MonitorFrom.None), MonitorDpiType.None, out var dpiX, out var _).Succeeded())
            {
                return (int)dpiX;
            }

            using SafeWindowDcHandle deviceContextHandle = SafeWindowDcHandle.FromWindow(windowHandle);
            return (deviceContextHandle is null) ? DpiCalculator.DefaultScreenDpi : Gdi32Api.GetDeviceCaps(deviceContextHandle, DeviceCaps.LOGPIXELSX);
        }
    }

    /// <summary>Return the DPI for the screen which the location is located on.</summary>
    /// <param name="location">The location to inspect.</param>
    /// <returns>The DPI value.</returns>
    internal static int GetDpiCore(NativePoint location)
    {
        if (!_isWindows81OrLater())
        {
            return DpiCalculator.DefaultScreenDpi;
        }

        NativeRect rect = new(location.X, location.Y, 1, 1);
        if (!GetDpiForMonitor(User32Api.MonitorFromRect(ref rect, MonitorFrom.None), MonitorDpiType.None, out var dpiX, out var _).Succeeded())
        {
            return DpiCalculator.DefaultScreenDpi;
        }

        return checked((int)dpiX);
    }

    /// <summary>Create a scope which enables the default DPI-aware context.</summary>
    /// <returns>A disposable scope that restores the previous context.</returns>
    internal static IDisposable DefaultScopedThreadDpiAwarenessContextCore() => ScopedThreadDpiAwarenessContext(DpiAwarenessContext.PerMonitorAwareV2, DpiAwarenessContext.PerMonitorAware);

    /// <summary>Enables automatic DPI scaling for the non-client area of the specified top-level window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns><see cref="F:CP.ReactiveUI.Primitives.Windows.Native.Enums.HResult.Ok" /> when the function succeeds; otherwise <see cref="F:CP.ReactiveUI.Primitives.Windows.Native.Enums.HResult.Fail" />.</returns>
    internal static HResult EnableNonClientDpiScalingCore(IntPtr windowHandle)
    {
        if (!NativeMethods.EnableNonClientDpiScaling(windowHandle))
        {
            return HResult.Fail;
        }

        return HResult.Ok;
    }

    /// <summary>Calculates the required size of a window rectangle for the provided DPI.</summary>
    /// <param name="rect">The desired client rectangle.</param>
    /// <param name="style">The window style.</param>
    /// <param name="hasMenu">A value indicating whether the window has a menu.</param>
    /// <param name="extendedStyle">The extended window style.</param>
    /// <param name="dpi">The DPI to use for scaling.</param>
    /// <returns><see langword="true" /> when the function succeeds.</returns>
    internal static unsafe bool AdjustWindowRectExForDpiCore(ref NativeRect rect, WindowStyleFlags style, bool hasMenu, ExtendedWindowStyleFlags extendedStyle, uint dpi)
    {
        fixed (NativeRect* rect2 = &rect)
        {
            return NativeMethods.AdjustWindowRectExForDpi(rect2, style, hasMenu, extendedStyle, dpi);
        }
    }
}
