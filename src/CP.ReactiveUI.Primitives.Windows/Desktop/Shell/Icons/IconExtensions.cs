// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif
/// <summary>Extension code for icons.</summary>
public static class IconExtensions
{
    /// <summary>Provides extension members for the target instance.</summary>
    /// <param name="window">The extended instance.</param>
    extension(IInteropWindow window)
    {
        /// <summary>Gets the icon for a window.</summary>
        /// <typeparam name="TIcon">The return type for the icon, such as Icon, Bitmap, or BitmapSource.</typeparam>
        /// <param name="iconType">The icon type marker.</param>
        /// <returns>The icon for the window, or the default value if no icon is found.</returns>
        public TIcon GetIcon<TIcon>(TIcon iconType)
            where TIcon : class => window.GetIcon(iconType, useLargeIcons: false);

        /// <summary>Gets the icon for a window.</summary>
        /// <typeparam name="TIcon">The return type for the icon, such as Icon, Bitmap, or BitmapSource.</typeparam>
        /// <param name="iconType">The icon type marker.</param>
        /// <param name="useLargeIcons">A value indicating whether large icons should be preferred.</param>
        /// <returns>The icon for the window, or the default value if no icon is found.</returns>
        public TIcon GetIcon<TIcon>(TIcon iconType, bool useLargeIcons)
            where TIcon : class
        {
            if (_operations.IsApp(window))
            {
                return IconHelper.GetAppLogo(window, iconType, IconHelper.DefaultLogoScale);
            }

            var icon = window.GetIconFromWindow(iconType, useLargeIcons);
            if (icon is not null)
            {
                return icon;
            }

            var processId = _operations.GetProcessId(window);
            var processPath = _operations.GetProcessPath(processId);
            if (processPath is not null)
            {
                return IconHelper.ExtractAssociatedIcon(processPath, iconType, 0, useLargeIcons);
            }

            icon = GetIconFromSiblingProcessWindow(processId, useLargeIcons, iconType);
            return icon ?? GetIconFromSiblingTopLevelWindow(window, processId, useLargeIcons, iconType);
        }

        /// <summary>Gets the icon for an interop window.</summary>
        /// <typeparam name="TIcon">The return type for the icon, such as Icon, Bitmap, or BitmapSource.</typeparam>
        /// <param name="iconType">The icon type marker.</param>
        /// <returns>The icon for the window, or the default value if no icon is found.</returns>
        public TIcon GetIconFromWindow<TIcon>(TIcon iconType)
            where TIcon : class => window.GetIconFromWindow(iconType, useLargeIcons: false);

        /// <summary>Gets the icon for an interop window.</summary>
        /// <typeparam name="TIcon">The return type for the icon, such as Icon, Bitmap, or BitmapSource.</typeparam>
        /// <param name="iconType">The icon type marker.</param>
        /// <param name="useLargeIcons">A value indicating whether large icons should be preferred.</param>
        /// <returns>The icon for the window, or the default value if no icon is found.</returns>
        public TIcon GetIconFromWindow<TIcon>(TIcon iconType, bool useLargeIcons)
            where TIcon : class => GetIconForWindowHandle(window.Handle, iconType, useLargeIcons);
    }

    /// <summary>Provides extension members for the target instance.</summary>
    /// <param name="icon">The extended instance.</param>
    extension(Icon icon)
    {
        /// <summary>Converts an icon to an image source.</summary>
        /// <returns>The converted bitmap source.</returns>
        public BitmapSource ToBitmapSource()
        {
            using Bitmap bitmap = icon.ToBitmap();
            var nativeBitmapHandle = bitmap.GetHbitmap();
            using (new SafeHBitmapHandle(nativeBitmapHandle))
            {
                return Imaging.CreateBitmapSourceFromHBitmap(nativeBitmapHandle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }
    }

    /// <summary>The WM_GETICON parameter for the large icon.</summary>
    private static readonly IntPtr BigIconMessageParameter = new(1);

    /// <summary>The WM_GETICON parameter for the small icon.</summary>
    private static readonly IntPtr SmallIconMessageParameter = new(0);

    /// <summary>The WM_GETICON parameter for the secondary small icon.</summary>
    private static readonly IntPtr SmallIconSecondMessageParameter = new(2);

    /// <summary>The icon operations used by this process.</summary>
    private static IconWindowOperations _operations = IconWindowOperations.CreateNative();

    /// <summary>Gets the icon for a window handle.</summary>
    /// <typeparam name="TIcon">The return type for the icon, such as Icon, Bitmap, or BitmapSource.</typeparam>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="iconType">The icon type marker.</param>
    /// <returns>The icon for the window handle, or the default value if no icon is found.</returns>
    public static TIcon GetIconForWindowHandle<TIcon>(IntPtr windowHandle, TIcon iconType)
        where TIcon : class => GetIconForWindowHandle(windowHandle, iconType, useLargeIcons: false);

    /// <summary>Gets the icon for a window handle.</summary>
    /// <typeparam name="TIcon">The return type for the icon, such as Icon, Bitmap, or BitmapSource.</typeparam>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="iconType">The icon type marker.</param>
    /// <param name="useLargeIcons">A value indicating whether large icons should be preferred.</param>
    /// <returns>The icon for the window handle, or the default value if no icon is found.</returns>
    public static TIcon GetIconForWindowHandle<TIcon>(IntPtr windowHandle, TIcon iconType, bool useLargeIcons)
        where TIcon : class => IconHelper.IconHandleTo(GetIconHandle(windowHandle, useLargeIcons), iconType);

    /// <summary>Replaces the window operations for deterministic tests.</summary>
    /// <param name="operations">The replacement operations.</param>
    /// <returns>The previous operations.</returns>
    internal static IconWindowOperations SetOperationsForTesting(IconWindowOperations operations)
    {
        Throw.IfNull(operations);
        var operations2 = _operations;
        _operations = operations;
        return operations2;
    }

    /// <summary>Gets an icon from a sibling process window.</summary>
    /// <typeparam name="TIcon">The return icon type.</typeparam>
    /// <param name="processId">The target process id.</param>
    /// <param name="useLargeIcons">A value indicating whether large icons should be preferred.</param>
    /// <param name="iconType">The icon type marker.</param>
    /// <returns>The matching icon, or the default value.</returns>
    private static TIcon GetIconFromSiblingProcessWindow<TIcon>(int processId, bool useLargeIcons, TIcon iconType)
        where TIcon : class
    {
        using var process = _operations.GetProcessById(processId);
        var processName = process.ProcessName;
        var array = _operations.GetProcessesByName(processName);
        foreach (var possibleParentProcess in array)
        {
            using (possibleParentProcess)
            {
                var icon = _operations.CreateWindow(possibleParentProcess.MainWindowHandle).GetIconFromWindow(iconType, useLargeIcons);
                if (icon is not null)
                {
                    return icon;
                }
            }
        }

        return null;
    }

    /// <summary>Gets an icon from another top-level window in the same process.</summary>
    /// <typeparam name="TIcon">The return icon type.</typeparam>
    /// <param name="window">The source window.</param>
    /// <param name="processId">The target process id.</param>
    /// <param name="useLargeIcons">A value indicating whether large icons should be preferred.</param>
    /// <param name="iconType">The icon type marker.</param>
    /// <returns>The matching icon, or the default value.</returns>
    private static TIcon GetIconFromSiblingTopLevelWindow<TIcon>(IInteropWindow window, int processId, bool useLargeIcons, TIcon iconType)
        where TIcon : class
    {
        foreach (var otherWindow in _operations.GetTopWindows())
        {
            if (otherWindow.Handle != window.Handle && _operations.GetProcessId(otherWindow) == processId)
            {
                var icon = otherWindow.GetIconFromWindow(iconType, useLargeIcons);
                if (icon is not null)
                {
                    return icon;
                }
            }
        }

        return null;
    }

    /// <summary>Gets the best icon handle for a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="useLargeIcons">A value indicating whether large icons should be preferred.</param>
    /// <returns>The icon handle, or zero.</returns>
    private static IntPtr GetIconHandle(IntPtr windowHandle, bool useLargeIcons)
    {
        var iconHandle = IntPtr.Zero;
        if (useLargeIcons)
        {
            iconHandle = GetLargeIconHandle(windowHandle);
        }
        else if (!_operations.TrySendMessage(windowHandle, WindowsMessages.WM_GETICON, SmallIconSecondMessageParameter, out iconHandle))
        {
            iconHandle = _operations.GetClassLong(windowHandle, ClassLongIndex.SmallIconHandle);
        }

        if (iconHandle == IntPtr.Zero)
        {
            iconHandle = GetSmallIconHandle(windowHandle);
        }

        if (iconHandle == IntPtr.Zero)
        {
            iconHandle = GetLargeIconHandle(windowHandle);
        }

        return iconHandle;
    }

    /// <summary>Gets a small icon handle from a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The icon handle, or zero.</returns>
    private static IntPtr GetSmallIconHandle(IntPtr windowHandle) =>
        _operations.TrySendMessage(
            windowHandle,
            WindowsMessages.WM_GETICON,
            SmallIconMessageParameter,
            out var iconHandle)
            ? iconHandle
            : _operations.GetClassLong(windowHandle, ClassLongIndex.SmallIconHandle);

    /// <summary>Gets a large icon handle from a window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The icon handle, or zero.</returns>
    private static IntPtr GetLargeIconHandle(IntPtr windowHandle) =>
        _operations.TrySendMessage(
            windowHandle,
            WindowsMessages.WM_GETICON,
            BigIconMessageParameter,
            out var iconHandle)
            ? iconHandle
            : _operations.GetClassLong(windowHandle, ClassLongIndex.IconHandle);
}
