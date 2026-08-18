// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif
/// <summary>Win32 native methods for icons.</summary>
#if NETFRAMEWORK
public static class NativeIconMethods
#else
public static partial class NativeIconMethods
#endif
{
    /// <summary>The native icon API used by this process.</summary>
    private static INativeIconApi _api = WindowsNativeIconApi.Instance;

    /// <summary>Copies an icon handle to a new icon handle.</summary>
    /// <param name="iconHandle">The icon handle to copy.</param>
    /// <returns>The copied icon handle.</returns>
    public static SafeIconHandle CopyIcon(SafeIconHandle iconHandle) => _api.CopyIcon(iconHandle);

    /// <summary>Copies an icon handle to a new icon handle.</summary>
    /// <param name="iconHandle">The icon handle to copy.</param>
    /// <returns>The copied icon handle.</returns>
    public static SafeIconHandle CopyIcon(IntPtr iconHandle) => _api.CopyIcon(iconHandle);

    /// <summary>
    /// Retrieves information about the specified icon or cursor.
    /// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms648070(v=vs.85).aspx">GetIconInfo function</a>
    /// This also describes how to get more information about standard icons and cursors.
    /// </summary>
    /// <param name="iconHandle">A handle to the icon or cursor.</param>
    /// <param name="iconInfo">A pointer to an ICONINFO structure. The function fills in the structure's members.</param>
    /// <returns>A value indicating whether the function succeeded.</returns>
    public static bool GetIconInfo(SafeIconHandle iconHandle, out IconInfo iconInfo) => _api.GetIconInfo(iconHandle, out iconInfo);

    /// <summary>
    /// Retrieves information about the specified icon or cursor.
    /// See <a href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-geticoninfoexw">GetIconInfoEx function</a>
    /// This also describes how to get more information about standard icons and cursors.
    /// </summary>
    /// <param name="iconOrCursorHandle">A IntPtr handle to the icon or cursor.</param>
    /// <param name="iconInfoEx">A pointer to an ICONINFOEX structure. The function fills in the structure's members.</param>
    /// <returns>A value indicating whether the function succeeded.</returns>
    public static bool GetIconInfoEx(IntPtr iconOrCursorHandle, ref IconInfoEx iconInfoEx) => _api.GetIconInfoEx(iconOrCursorHandle, ref iconInfoEx);

    /// <summary>See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms648062(v=vs.85).aspx">CreateIconIndirect function</a>.</summary>
    /// <param name="icon">Icon information.</param>
    /// <returns>The created icon handle.</returns>
    public static IntPtr CreateIconIndirect(ref IconInfo icon) => _api.CreateIconIndirect(ref icon);

    /// <summary>
    /// Loads an icon with the specified dimensions from an icon resource.
    /// The icon resource can be from an application instance or loaded from a file.
    /// This function automatically scales down a larger image to the requested size.
    /// See <a href="https://docs.microsoft.com/en-us/windows/win32/api/commctrl/nf-commctrl-loadiconmetric">LoadIconMetric function</a>
    /// </summary>
    /// <param name="instanceHandle">
    /// A handle to the module of either a DLL or executable (.exe) that contains the icon to be loaded.
    /// To load a stock system icon, set this parameter to IntPtr.Zero.
    /// </param>
    /// <param name="iconName">
    /// The icon name. To load a stock system icon, use one of the IDI_* constants.
    /// To load from resources, use MAKEINTRESOURCE macro result or the string name.
    /// </param>
    /// <param name="lims">
    /// The size of the icon to load. This can be SmallIcon (SM_CXSMICON) or StandardIcon (SM_CXICON).
    /// </param>
    /// <param name="iconHandle">
    /// When this function returns, contains a handle to the loaded icon.
    /// </param>
    /// <returns>
    /// If the function succeeds, it returns S_OK (0). Otherwise, it returns an HRESULT error code.
    /// </returns>
    public static int LoadIconMetric(
        IntPtr instanceHandle,
        IntPtr iconName,
        IconMetricSize lims,
        out IntPtr iconHandle) =>
        LoadIconMetric(instanceHandle, new NativeIconResourceName(iconName), lims, out iconHandle);

    /// <summary>
    /// Loads an icon with the specified dimensions from an icon resource.
    /// The icon resource can be from an application instance or loaded from a file.
    /// This function automatically scales down a larger image to the requested size.
    /// See <a href="https://docs.microsoft.com/en-us/windows/win32/api/commctrl/nf-commctrl-loadiconmetric">LoadIconMetric function</a>
    /// </summary>
    /// <param name="instanceHandle">
    /// A handle to the module of either a DLL or executable (.exe) that contains the icon to be loaded.
    /// To load a stock system icon, set this parameter to IntPtr.Zero.
    /// </param>
    /// <param name="iconName">
    /// The icon name as a string.
    /// </param>
    /// <param name="lims">
    /// The size of the icon to load. This can be SmallIcon (SM_CXSMICON) or StandardIcon (SM_CXICON).
    /// </param>
    /// <param name="iconHandle">
    /// When this function returns, contains a handle to the loaded icon.
    /// </param>
    /// <returns>
    /// If the function succeeds, it returns S_OK (0). Otherwise, it returns an HRESULT error code.
    /// </returns>
    public static int LoadIconMetric(
        IntPtr instanceHandle,
        string iconName,
        IconMetricSize lims,
        out IntPtr iconHandle) =>
        LoadIconMetric(instanceHandle, new NativeIconResourceName(iconName), lims, out iconHandle);

    /// <summary>
    /// Loads an icon. If the icon is larger than the requested size, this function scales down the icon to the requested size.
    /// See <a href="https://docs.microsoft.com/en-us/windows/win32/api/commctrl/nf-commctrl-loadiconwithscaledown">LoadIconWithScaleDown function</a>
    /// </summary>
    /// <param name="instanceHandle">
    /// A handle to the module of either a DLL or executable (.exe) that contains the icon to be loaded.
    /// To load a stock system icon, set this parameter to IntPtr.Zero.
    /// </param>
    /// <param name="iconName">
    /// The icon name. To load a stock system icon, use one of the IDI_* constants.
    /// To load from resources, use MAKEINTRESOURCE macro result or the string name.
    /// </param>
    /// <param name="cx">The desired width, in pixels, of the icon.</param>
    /// <param name="cy">The desired height, in pixels, of the icon.</param>
    /// <param name="iconHandle">
    /// When this function returns, contains a handle to the loaded icon.
    /// </param>
    /// <returns>
    /// If the function succeeds, it returns S_OK (0). Otherwise, it returns an HRESULT error code.
    /// </returns>
    public static int LoadIconWithScaleDown(
        IntPtr instanceHandle,
        IntPtr iconName,
        int cx,
        int cy,
        out IntPtr iconHandle) =>
        LoadIconWithScaleDown(instanceHandle, new NativeIconResourceName(iconName), cx, cy, out iconHandle);

    /// <summary>
    /// Loads an icon. If the icon is larger than the requested size, this function scales down the icon to the requested size.
    /// See <a href="https://docs.microsoft.com/en-us/windows/win32/api/commctrl/nf-commctrl-loadiconwithscaledown">LoadIconWithScaleDown function</a>
    /// </summary>
    /// <param name="instanceHandle">
    /// A handle to the module of either a DLL or executable (.exe) that contains the icon to be loaded.
    /// To load a stock system icon, set this parameter to IntPtr.Zero.
    /// </param>
    /// <param name="iconName">
    /// The icon name as a string.
    /// </param>
    /// <param name="cx">The desired width, in pixels, of the icon.</param>
    /// <param name="cy">The desired height, in pixels, of the icon.</param>
    /// <param name="iconHandle">
    /// When this function returns, contains a handle to the loaded icon.
    /// </param>
    /// <returns>
    /// If the function succeeds, it returns S_OK (0). Otherwise, it returns an HRESULT error code.
    /// </returns>
    public static int LoadIconWithScaleDown(
        IntPtr instanceHandle,
        string iconName,
        int cx,
        int cy,
        out IntPtr iconHandle) =>
        LoadIconWithScaleDown(instanceHandle, new NativeIconResourceName(iconName), cx, cy, out iconHandle);

    /// <summary>Draws an icon or cursor with DrawIconEx.</summary>
    /// <param name="arguments">The DrawIconEx arguments.</param>
    /// <returns>A value indicating whether drawing succeeded.</returns>
    internal static bool DrawIconEx(in DrawIconArguments arguments) => _api.DrawIconEx(in arguments);

    /// <summary>Replaces the native icon API for deterministic tests.</summary>
    /// <param name="api">The replacement icon API.</param>
    /// <returns>The previous icon API.</returns>
    internal static INativeIconApi SetApiForTesting(INativeIconApi api)
    {
        Throw.IfNull(api);
        var api2 = _api;
        _api = api;
        return api2;
    }

    /// <summary>Loads an icon metric resource through the resource-name dispatcher.</summary>
    /// <param name="instanceHandle">The module instance handle.</param>
    /// <param name="iconName">The icon resource name.</param>
    /// <param name="lims">The icon metric size.</param>
    /// <param name="iconHandle">The loaded icon handle.</param>
    /// <returns>The HRESULT from the native loader.</returns>
    private static int LoadIconMetric(
        IntPtr instanceHandle,
        NativeIconResourceName iconName,
        IconMetricSize lims,
        out IntPtr iconHandle) =>
        iconName.LoadMetric(instanceHandle, lims, out iconHandle);

    /// <summary>Loads a scale-down icon resource through the resource-name dispatcher.</summary>
    /// <param name="instanceHandle">The module instance handle.</param>
    /// <param name="iconName">The icon resource name.</param>
    /// <param name="cx">The desired icon width.</param>
    /// <param name="cy">The desired icon height.</param>
    /// <param name="iconHandle">The loaded icon handle.</param>
    /// <returns>The HRESULT from the native loader.</returns>
    private static int LoadIconWithScaleDown(
        IntPtr instanceHandle,
        NativeIconResourceName iconName,
        int cx,
        int cy,
        out IntPtr iconHandle) =>
        iconName.LoadWithScaleDown(instanceHandle, cx, cy, out iconHandle);

    /// <summary>Contains DrawIconEx arguments.</summary>
    internal readonly record struct DrawIconArguments
    {
        /// <summary>The target device context.</summary>
        private readonly IntPtr _deviceContext;

        /// <summary>The icon handle.</summary>
        private readonly IntPtr _iconHandle;

        /// <summary>The flicker-free brush handle.</summary>
        private readonly IntPtr _flickerFreeBrush;

        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.NativeIconMethods.DrawIconArguments" /> struct.</summary>
        /// <param name="deviceContext">The target device context.</param>
        /// <param name="location">The drawing location.</param>
        /// <param name="iconHandle">The icon handle.</param>
        /// <param name="size">The destination size.</param>
        /// <param name="animationStep">The animation step.</param>
        /// <param name="flickerFreeBrush">The flicker-free brush handle.</param>
        /// <param name="flags">The draw flags.</param>
        public DrawIconArguments(
            IntPtr deviceContext,
            (int Left, int Top) location,
            IntPtr iconHandle,
            (int Width, int Height) size,
            int animationStep,
            IntPtr flickerFreeBrush,
            DrawIconExFlags flags)
        {
            _deviceContext = deviceContext;
            Left = location.Left;
            Top = location.Top;
            _iconHandle = iconHandle;
            Width = size.Width;
            Height = size.Height;
            AnimationStep = animationStep;
            _flickerFreeBrush = flickerFreeBrush;
            Flags = flags;
        }

        /// <summary>Gets the target device context.</summary>
        internal IntPtr DeviceContext => _deviceContext;

        /// <summary>Gets the left coordinate.</summary>
        internal int Left { get; }

        /// <summary>Gets the top coordinate.</summary>
        internal int Top { get; }

        /// <summary>Gets the icon handle.</summary>
        internal IntPtr IconHandle => _iconHandle;

        /// <summary>Gets the destination width.</summary>
        internal int Width { get; }

        /// <summary>Gets the destination height.</summary>
        internal int Height { get; }

        /// <summary>Gets the animation step.</summary>
        internal int AnimationStep { get; }

        /// <summary>Gets the flicker-free brush handle.</summary>
        internal IntPtr FlickerFreeBrush => _flickerFreeBrush;

        /// <summary>Gets the draw flags.</summary>
        internal DrawIconExFlags Flags { get; }
    }

    /// <summary>Dispatches native icon resource loading for identifier and string names.</summary>
    private readonly struct NativeIconResourceName
    {
        /// <summary>The integer resource identifier.</summary>
        private readonly IntPtr _identifier;

        /// <summary>The string resource name.</summary>
        private readonly string _name;

        /// <summary>A value indicating whether the string resource name is active.</summary>
        private readonly bool _usesName;

        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.NativeIconMethods.NativeIconResourceName" /> struct.</summary>
        /// <param name="identifier">The integer resource identifier.</param>
        public NativeIconResourceName(IntPtr identifier)
        {
            _identifier = identifier;
            _name = null;
            _usesName = false;
        }

        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.NativeIconMethods.NativeIconResourceName" /> struct.</summary>
        /// <param name="name">The string resource name.</param>
        public NativeIconResourceName(string name)
        {
            _identifier = IntPtr.Zero;
            _name = name;
            _usesName = true;
        }

        /// <summary>Loads a metric-sized icon.</summary>
        /// <param name="instanceHandle">The module instance handle.</param>
        /// <param name="lims">The icon metric size.</param>
        /// <param name="iconHandle">The loaded icon handle.</param>
        /// <returns>The HRESULT from the native loader.</returns>
        public int LoadMetric(IntPtr instanceHandle, IconMetricSize lims, out IntPtr iconHandle) =>
            !_usesName
                ? _api.LoadIconMetric(instanceHandle, _identifier, lims, out iconHandle)
                : _api.LoadIconMetric(instanceHandle, _name, lims, out iconHandle);

        /// <summary>Loads a scale-down icon.</summary>
        /// <param name="instanceHandle">The module instance handle.</param>
        /// <param name="cx">The desired icon width.</param>
        /// <param name="cy">The desired icon height.</param>
        /// <param name="iconHandle">The loaded icon handle.</param>
        /// <returns>The HRESULT from the native loader.</returns>
        public int LoadWithScaleDown(IntPtr instanceHandle, int cx, int cy, out IntPtr iconHandle) =>
            !_usesName
                ? _api.LoadIconWithScaleDown(instanceHandle, _identifier, cx, cy, out iconHandle)
                : _api.LoadIconWithScaleDown(instanceHandle, _name, cx, cy, out iconHandle);
    }

    /// <summary>Native icon entry points.</summary>
#if NETFRAMEWORK
    internal static class NativeMethods
#else
    internal static partial class NativeMethods
#endif
    {
        /// <summary>The loaded user32 module.</summary>
        private static readonly IntPtr User32Module = NativeLibrary.Load(
            "user32.dll",
            typeof(NativeMethods).Assembly,
            DllImportSearchPath.System32);

        /// <summary>The GetIconInfoExW export pointer.</summary>
        private static readonly IntPtr GetIconInfoExExport = NativeLibrary.GetExport(
            User32Module,
            "GetIconInfoExW");

        /// <summary>Invokes the native <c>CopyIcon</c> entry point.</summary>
        /// <param name="iconHandle">The native <paramref name="iconHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern SafeIconHandle CopyIcon(SafeIconHandle iconHandle);
#else
        [LibraryImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial SafeIconHandle CopyIcon(SafeIconHandle iconHandle);
#endif

        /// <summary>Invokes the native <c>CopyIcon</c> entry point.</summary>
        /// <param name="iconHandle">The native <paramref name="iconHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern SafeIconHandle CopyIcon(IntPtr iconHandle);
#else
        [LibraryImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial SafeIconHandle CopyIcon(IntPtr iconHandle);
#endif

        /// <summary>Invokes the native <c>GetIconInfo</c> entry point.</summary>
        /// <param name="iconHandle">The native <paramref name="iconHandle" /> value.</param>
        /// <param name="iconInfo">The native <paramref name="iconInfo" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetIconInfo(SafeIconHandle iconHandle, out IconInfo iconInfo);
#else
        [LibraryImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetIconInfo(SafeIconHandle iconHandle, out IconInfo iconInfo);
#endif

        /// <summary>Gets extended icon information from a raw icon or cursor handle.</summary>
        /// <param name="iconOrCursorHandle">The icon or cursor handle.</param>
        /// <param name="iconInfoEx">The extended icon information.</param>
        /// <returns><see langword="true" /> when the call succeeds.</returns>
        internal static unsafe bool GetIconInfoEx(IntPtr iconOrCursorHandle, ref IconInfoEx iconInfoEx)
        {
            fixed (IconInfoEx* iconInfoExPointer = &iconInfoEx)
            {
                return ((delegate* unmanaged[Stdcall]<IntPtr, IconInfoEx*, int>)(void*)GetIconInfoExExport)(
                    iconOrCursorHandle,
                    iconInfoExPointer) != 0;
            }
        }

        /// <summary>Invokes the native <c>CreateIconIndirect</c> entry point.</summary>
        /// <param name="icon">The native <paramref name="icon" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr CreateIconIndirect(ref IconInfo icon);
#else
        [LibraryImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial IntPtr CreateIconIndirect(ref IconInfo icon);
#endif

        /// <summary>Invokes the native <c>LoadIconMetric</c> entry point.</summary>
        /// <param name="instanceHandle">The native <paramref name="instanceHandle" /> value.</param>
        /// <param name="iconName">The native <paramref name="iconName" /> value.</param>
        /// <param name="lims">The native <paramref name="lims" /> value.</param>
        /// <param name="iconHandle">The native <paramref name="iconHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("comctl32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int LoadIconMetric(IntPtr instanceHandle, IntPtr iconName, IconMetricSize lims, out IntPtr iconHandle);
#else
        [LibraryImport("comctl32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int LoadIconMetric(IntPtr instanceHandle, IntPtr iconName, IconMetricSize lims, out IntPtr iconHandle);
#endif

        /// <summary>Invokes the native <c>LoadIconMetric</c> entry point.</summary>
        /// <param name="instanceHandle">The native <paramref name="instanceHandle" /> value.</param>
        /// <param name="iconName">The native <paramref name="iconName" /> value.</param>
        /// <param name="lims">The native <paramref name="lims" /> value.</param>
        /// <param name="iconHandle">The native <paramref name="iconHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("comctl32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int LoadIconMetric(IntPtr instanceHandle, string iconName, IconMetricSize lims, out IntPtr iconHandle);
#else
        [LibraryImport("comctl32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int LoadIconMetric(IntPtr instanceHandle, string iconName, IconMetricSize lims, out IntPtr iconHandle);
#endif

        /// <summary>Invokes the native <c>LoadIconWithScaleDown</c> entry point.</summary>
        /// <param name="instanceHandle">The native <paramref name="instanceHandle" /> value.</param>
        /// <param name="iconName">The native <paramref name="iconName" /> value.</param>
        /// <param name="cx">The native <paramref name="cx" /> value.</param>
        /// <param name="cy">The native <paramref name="cy" /> value.</param>
        /// <param name="iconHandle">The native <paramref name="iconHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("comctl32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int LoadIconWithScaleDown(IntPtr instanceHandle, IntPtr iconName, int cx, int cy, out IntPtr iconHandle);
#else
        [LibraryImport("comctl32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int LoadIconWithScaleDown(IntPtr instanceHandle, IntPtr iconName, int cx, int cy, out IntPtr iconHandle);
#endif

        /// <summary>Invokes the native <c>LoadIconWithScaleDown</c> entry point.</summary>
        /// <param name="instanceHandle">The native <paramref name="instanceHandle" /> value.</param>
        /// <param name="iconName">The native <paramref name="iconName" /> value.</param>
        /// <param name="cx">The native <paramref name="cx" /> value.</param>
        /// <param name="cy">The native <paramref name="cy" /> value.</param>
        /// <param name="iconHandle">The native <paramref name="iconHandle" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("comctl32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int LoadIconWithScaleDown(IntPtr instanceHandle, string iconName, int cx, int cy, out IntPtr iconHandle);
#else
        [LibraryImport("comctl32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int LoadIconWithScaleDown(IntPtr instanceHandle, string iconName, int cx, int cy, out IntPtr iconHandle);
#endif

        /// <summary>Invokes the native <c>DrawIconEx</c> entry point.</summary>
        /// <param name="deviceContext">The native <paramref name="deviceContext" /> value.</param>
        /// <param name="left">The native <paramref name="left" /> value.</param>
        /// <param name="top">The native <paramref name="top" /> value.</param>
        /// <param name="iconHandle">The native <paramref name="iconHandle" /> value.</param>
        /// <param name="width">The native <paramref name="width" /> value.</param>
        /// <param name="height">The native <paramref name="height" /> value.</param>
        /// <param name="animationStep">The native <paramref name="animationStep" /> value.</param>
        /// <param name="flickerFreeBrush">The native <paramref name="flickerFreeBrush" /> value.</param>
        /// <param name="flags">The native <paramref name="flags" /> value.</param>
        /// <returns>The native result.</returns>
#if NETFRAMEWORK
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DrawIconEx(
            IntPtr deviceContext,
            int left,
            int top,
            IntPtr iconHandle,
            int width,
            int height,
            int animationStep,
            IntPtr flickerFreeBrush,
            DrawIconExFlags flags);
#else
        [LibraryImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool DrawIconEx(
            IntPtr deviceContext,
            int left,
            int top,
            IntPtr iconHandle,
            int width,
            int height,
            int animationStep,
            IntPtr flickerFreeBrush,
            DrawIconExFlags flags);
#endif
    }
}
