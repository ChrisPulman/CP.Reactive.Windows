// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel;
using CP.ReactiveUI.Primitives.Windows.Native.Shell;
using CP.ReactiveUI.Primitives.Windows.Native.Shell.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles;
using CP.ReactiveUI.Primitives.Windows.Native.Shell.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif
/// <summary>Helper code for icons.</summary>
public static class IconHelper
{
    /// <summary>Dispatches icon resource loading for identifier and string resource names.</summary>
    private readonly struct IconResourceName
    {
        /// <summary>The integer resource identifier.</summary>
        private readonly IntPtr _identifier;

        /// <summary>The string resource name.</summary>
        private readonly string _name;

        /// <summary>A value indicating whether the string resource name is active.</summary>
        private readonly bool _usesName;

        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.IconHelper.IconResourceName" /> struct.</summary>
        /// <param name="identifier">The integer resource identifier.</param>
        public IconResourceName(IntPtr identifier)
        {
            _identifier = identifier;
            _name = null;
            _usesName = false;
        }

        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.IconHelper.IconResourceName" /> struct.</summary>
        /// <param name="name">The string resource name.</param>
        public IconResourceName(string name)
        {
            _identifier = IntPtr.Zero;
            _name = name;
            _usesName = true;
        }

        /// <summary>Loads an icon with system metrics.</summary>
        /// <param name="instanceHandle">A handle to the module containing the icon resource.</param>
        /// <param name="metricSize">The metric size to use.</param>
        /// <param name="iconHandle">The loaded icon handle.</param>
        /// <returns>The HRESULT from the native loader.</returns>
        public int LoadWithSystemMetrics(IntPtr instanceHandle, IconMetricSize metricSize, out IntPtr iconHandle)
        {
            if (!_usesName)
            {
                return NativeIconMethods.LoadIconMetric(instanceHandle, _identifier, metricSize, out iconHandle);
            }

            return NativeIconMethods.LoadIconMetric(instanceHandle, _name, metricSize, out iconHandle);
        }

        /// <summary>Loads an icon with scale-down semantics.</summary>
        /// <param name="instanceHandle">A handle to the module containing the icon resource.</param>
        /// <param name="width">The desired width in pixels.</param>
        /// <param name="height">The desired height in pixels.</param>
        /// <param name="iconHandle">The loaded icon handle.</param>
        /// <returns>The HRESULT from the native loader.</returns>
        public int LoadWithScaleDown(IntPtr instanceHandle, int width, int height, out IntPtr iconHandle)
        {
            if (!_usesName)
            {
                return NativeIconMethods.LoadIconWithScaleDown(instanceHandle, _identifier, width, height, out iconHandle);
            }

            return NativeIconMethods.LoadIconWithScaleDown(instanceHandle, _name, width, height, out iconHandle);
        }
    }

    /// <summary>The default icon index used when extracting associated icons.</summary>
    private const int DefaultAssociatedIconIndex = 0;

    /// <summary>The default app-logo scale percentage.</summary>
    private const int DefaultLogoScaleValue = 100;

    /// <summary>Gets the default app-logo scale percentage.</summary>
    internal static int DefaultLogoScale => 100;

    /// <summary>Helper method to get the app logo from the applications AppxManifest.</summary>
    /// <typeparam name="TBitmap">The bitmap return type, such as BitmapSource or Bitmap.</typeparam>
    /// <param name="interopWindow">The interop window.</param>
    /// <param name="bitmapType">The bitmap type marker.</param>
    /// <returns>The bitmap instance, or null when no logo is found.</returns>
    public static TBitmap GetAppLogo<TBitmap>(IInteropWindow interopWindow, TBitmap bitmapType)
        where TBitmap : class => GetAppLogo(interopWindow, bitmapType, DefaultLogoScale);

    /// <summary>Helper method to get the app logo from the applications AppxManifest.</summary>
    /// <typeparam name="TBitmap">The bitmap return type, such as BitmapSource or Bitmap.</typeparam>
    /// <param name="interopWindow">The interop window.</param>
    /// <param name="bitmapType">The bitmap type marker.</param>
    /// <param name="scale">The requested logo scale, where 100 is the default.</param>
    /// <returns>The bitmap instance, or null when no logo is found.</returns>
    public static TBitmap GetAppLogo<TBitmap>(IInteropWindow interopWindow, TBitmap bitmapType, int scale)
        where TBitmap : class => GetAppLogoFromProcessPath(GetAppProcessPath(interopWindow), bitmapType, scale);

    /// <summary>Writes the images to the stream as an icon.</summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="images">The images to write.</param>
    public static void WriteIcon(Stream stream, IEnumerable<Image> images) => IconFileWriter.WriteIconFile(stream, images);

    /// <summary>Extracts an associated icon from an executable or DLL file.</summary>
    /// <typeparam name="TIcon">The icon return type, such as Icon, Bitmap, or BitmapSource.</typeparam>
    /// <param name="filePath">The file path.</param>
    /// <param name="iconType">The icon type marker.</param>
    /// <returns>The icon, or null when no icon is available.</returns>
    public static TIcon ExtractAssociatedIcon<TIcon>(string filePath, TIcon iconType)
        where TIcon : class => ExtractAssociatedIcon(filePath, iconType, 0, useLargeIcon: true);

    /// <summary>Extracts an associated icon from an executable or DLL file.</summary>
    /// <typeparam name="TIcon">The icon return type, such as Icon, Bitmap, or BitmapSource.</typeparam>
    /// <param name="filePath">The file path.</param>
    /// <param name="iconType">The icon type marker.</param>
    /// <param name="index">The icon index.</param>
    /// <param name="useLargeIcon">A value indicating whether the large icon is preferred.</param>
    /// <returns>The icon, or null when no icon is available.</returns>
    public static TIcon ExtractAssociatedIcon<TIcon>(string filePath, TIcon iconType, int index, bool useLargeIcon)
        where TIcon : class
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(filePath);
        if (!Uri.TryCreate(filePath, UriKind.Absolute, out var uri))
        {
            filePath = Path.GetFullPath(filePath);
            uri = new(filePath);
        }

        if (!uri.IsFile || !File.Exists(filePath))
        {
            return null;
        }

        _ = Shell32Api.ExtractIconEx(filePath, index, out var large, out var small, 1);
        using SafeIconHandle largeIcon = new(large);
        using SafeIconHandle smallIcon = new(small);
        if (useLargeIcon && !largeIcon.IsInvalid)
        {
            return IconHandleTo(largeIcon, iconType);
        }

        if (!smallIcon.IsInvalid)
        {
            return IconHandleTo(smallIcon, iconType);
        }

        return (!largeIcon.IsInvalid) ? IconHandleTo(largeIcon, iconType) : null;
    }

    /// <summary>Gets the number of icons in the file.</summary>
    /// <param name="location">The executable or DLL location.</param>
    /// <returns>The number of icons in the file.</returns>
    public static int CountAssociatedIcons(string location)
    {
        return Shell32Api.ExtractIconEx(location, -1, out _, out _, 0);
    }

    /// <summary>Creates a typed icon object from the specified icon handle.</summary>
    /// <typeparam name="TIcon">The icon return type, such as Icon, Bitmap, or BitmapSource.</typeparam>
    /// <param name="iconHandle">The icon handle.</param>
    /// <param name="iconType">The icon type marker.</param>
    /// <returns>The typed icon object, or null.</returns>
    public static TIcon IconHandleTo<TIcon>(IntPtr iconHandle, TIcon iconType)
        where TIcon : class
    {
        if (iconHandle == IntPtr.Zero)
        {
            return null;
        }

        using Icon icon = Icon.FromHandle(iconHandle);
        if (typeof(TIcon) == typeof(Icon))
        {
            return icon.Clone() as TIcon;
        }

        using Bitmap bitmap = icon.ToBitmap();
        if (typeof(TIcon) == typeof(Bitmap))
        {
            return bitmap.Clone() as TIcon;
        }

        return (typeof(TIcon) == typeof(BitmapSource)) ? (bitmap.ToBitmapSource() as TIcon) : null;
    }

    /// <summary>Creates a typed icon object from the specified safe icon handle.</summary>
    /// <typeparam name="TIcon">The icon return type, such as Icon, Bitmap, or BitmapSource.</typeparam>
    /// <param name="iconHandle">The safe icon handle.</param>
    /// <param name="iconType">The icon type marker.</param>
    /// <returns>The typed icon object, or null.</returns>
    public static TIcon IconHandleTo<TIcon>(SafeIconHandle iconHandle, TIcon iconType)
        where TIcon : class
    {
        _ = iconType;
        if (iconHandle is not null && !iconHandle.IsInvalid)
        {
            return iconHandle.UseNativeHandle((nativeIconHandle) => IconHandleTo(nativeIconHandle, iconType));
        }

        return null;
    }

    /// <summary>Gets an icon for a file extension.</summary>
    /// <typeparam name="TIcon">The icon return type, such as Icon, Bitmap, or BitmapSource.</typeparam>
    /// <param name="filename">The file name.</param>
    /// <param name="iconType">The icon type marker.</param>
    /// <param name="size">The requested icon size.</param>
    /// <param name="linkOverlay">A value indicating whether to include the link icon.</param>
    /// <returns>The icon, or null when no shell icon is available.</returns>
    public static TIcon GetFileExtensionIcon<TIcon>(string filename, TIcon iconType, IconSize size, bool linkOverlay)
        where TIcon : class
    {
        ShellFileInfo shellFileInfo = default;
        ShellGetFileInfoFlags flags = ShellGetFileInfoFlags.Icon | ShellGetFileInfoFlags.UseFileAttributes;
        if (linkOverlay)
        {
            flags |= ShellGetFileInfoFlags.LinkOverlay;
        }

        flags = (ShellGetFileInfoFlags)((int)flags | ((IconSize.Small == size) ? 1 : 0));
        _ = Shell32Api.SHGetFileInfo(Path.GetFileName(filename), ShellFileAttributeFlags.Normal, ref shellFileInfo, checked((uint)Marshal.SizeOf<ShellFileInfo>()), flags);
        using SafeIconHandle iconHandle = shellFileInfo.IconHandle;
        return IconHandleTo(iconHandle, iconType);
    }

    /// <summary>Gets a system folder icon.</summary>
    /// <typeparam name="TIcon">The icon return type, such as Icon, Bitmap, or BitmapSource.</typeparam>
    /// <param name="iconType">The icon type marker.</param>
    /// <param name="size">The requested icon size.</param>
    /// <param name="folderIconType">The folder icon type.</param>
    /// <returns>The icon, or null when no shell icon is available.</returns>
    public static TIcon GetFolderIcon<TIcon>(TIcon iconType, IconSize size, FolderIconType folderIconType)
        where TIcon : class
    {
        ShellGetFileInfoFlags flags = ShellGetFileInfoFlags.Icon | ShellGetFileInfoFlags.UseFileAttributes;
        if (folderIconType == FolderIconType.Open)
        {
            flags |= ShellGetFileInfoFlags.OpenIcon;
        }

        flags = (ShellGetFileInfoFlags)((int)flags | ((IconSize.Small == size) ? 1 : 0));
        ShellFileInfo shellFileInfo = default;
        _ = Shell32Api.SHGetFileInfo(null, ShellFileAttributeFlags.Directory, ref shellFileInfo, checked((uint)Marshal.SizeOf<ShellFileInfo>()), flags);
        using SafeIconHandle iconHandle = shellFileInfo.IconHandle;
        return IconHandleTo(iconHandle, iconType);
    }

    /// <summary>Gets the system-preferred width for small icons.</summary>
    /// <returns>The recommended width of a small icon, in pixels.</returns>
    public static int GetSmallIconWidth() => User32Api.GetSystemMetrics(SystemMetric.SM_CXSMICON);

    /// <summary>Gets the system-preferred height for small icons.</summary>
    /// <returns>The recommended height of a small icon, in pixels.</returns>
    public static int GetSmallIconHeight() => User32Api.GetSystemMetrics(SystemMetric.SM_CYSMICON);

    /// <summary>Gets the system-preferred width for large or standard icons.</summary>
    /// <returns>The default width of an icon, in pixels.</returns>
    public static int GetStandardIconWidth() => User32Api.GetSystemMetrics(SystemMetric.SM_CXICON);

    /// <summary>Gets the system-preferred height for large or standard icons.</summary>
    /// <returns>The default height of an icon, in pixels.</returns>
    public static int GetStandardIconHeight() => User32Api.GetSystemMetrics(SystemMetric.SM_CYICON);

    /// <summary>Gets the system-preferred width for icon grid spacing.</summary>
    /// <returns>The width of a grid cell for items in large icon view, in pixels.</returns>
    public static int GetIconSpacingWidth() => User32Api.GetSystemMetrics(SystemMetric.SM_CXICONSPACING);

    /// <summary>Gets the system-preferred height for icon grid spacing.</summary>
    /// <returns>The height of a grid cell for items in large icon view, in pixels.</returns>
    public static int GetIconSpacingHeight() => User32Api.GetSystemMetrics(SystemMetric.SM_CYICONSPACING);

    /// <summary>Gets the system-preferred size for icons based on the metric size.</summary>
    /// <param name="metricSize">The metric size.</param>
    /// <returns>A size structure containing the width and height in pixels.</returns>
    public static Size GetSystemIconSize(IconMetricSize metricSize) => metricSize switch
        {
            IconMetricSize.SmallIcon => new Size(GetSmallIconWidth(), GetSmallIconHeight()),
            IconMetricSize.StandardIcon => new Size(GetStandardIconWidth(), GetStandardIconHeight()),
            _ => throw new ArgumentOutOfRangeException(nameof(metricSize)),
        };

    /// <summary>Loads an icon at the system-preferred size using LoadIconMetric.</summary>
    /// <typeparam name="TIcon">The icon return type, such as Icon, Bitmap, or BitmapSource.</typeparam>
    /// <param name="iconType">The icon type marker.</param>
    /// <param name="instanceHandle">A handle to the module containing the icon resource.</param>
    /// <param name="iconName">The icon resource identifier.</param>
    /// <param name="metricSize">The metric size to use.</param>
    /// <returns>The loaded icon, or null if loading failed.</returns>
    public static TIcon LoadIconWithSystemMetrics<TIcon>(TIcon iconType, IntPtr instanceHandle, IntPtr iconName, IconMetricSize metricSize)
        where TIcon : class => LoadIconWithSystemMetrics(iconType, instanceHandle, new IconResourceName(iconName), metricSize);

    /// <summary>Loads an icon at the system-preferred size using LoadIconMetric.</summary>
    /// <typeparam name="TIcon">The icon return type, such as Icon, Bitmap, or BitmapSource.</typeparam>
    /// <param name="iconType">The icon type marker.</param>
    /// <param name="instanceHandle">A handle to the module containing the icon resource.</param>
    /// <param name="iconName">The icon resource name.</param>
    /// <param name="metricSize">The metric size to use.</param>
    /// <returns>The loaded icon, or null if loading failed.</returns>
    public static TIcon LoadIconWithSystemMetrics<TIcon>(TIcon iconType, IntPtr instanceHandle, string iconName, IconMetricSize metricSize)
        where TIcon : class => LoadIconWithSystemMetrics(iconType, instanceHandle, new IconResourceName(iconName), metricSize);

    /// <summary>Loads an icon with automatic scaling using LoadIconWithScaleDown.</summary>
    /// <typeparam name="TIcon">The icon return type, such as Icon, Bitmap, or BitmapSource.</typeparam>
    /// <param name="iconType">The icon type marker.</param>
    /// <param name="instanceHandle">A handle to the module containing the icon resource.</param>
    /// <param name="iconName">The icon resource identifier.</param>
    /// <param name="width">The desired width of the icon in pixels.</param>
    /// <param name="height">The desired height of the icon in pixels.</param>
    /// <returns>The loaded icon, or null if loading failed.</returns>
    public static TIcon LoadIconWithScaleDown<TIcon>(TIcon iconType, IntPtr instanceHandle, IntPtr iconName, int width, int height)
        where TIcon : class => LoadIconWithScaleDown(iconType, instanceHandle, new IconResourceName(iconName), width, height);

    /// <summary>Loads an icon with automatic scaling using LoadIconWithScaleDown.</summary>
    /// <typeparam name="TIcon">The icon return type, such as Icon, Bitmap, or BitmapSource.</typeparam>
    /// <param name="iconType">The icon type marker.</param>
    /// <param name="instanceHandle">A handle to the module containing the icon resource.</param>
    /// <param name="iconName">The icon resource name.</param>
    /// <param name="width">The desired width of the icon in pixels.</param>
    /// <param name="height">The desired height of the icon in pixels.</param>
    /// <returns>The loaded icon, or null if loading failed.</returns>
    public static TIcon LoadIconWithScaleDown<TIcon>(TIcon iconType, IntPtr instanceHandle, string iconName, int width, int height)
        where TIcon : class => LoadIconWithScaleDown(iconType, instanceHandle, new IconResourceName(iconName), width, height);

    /// <summary>Helper method to get the app logo from a known process path.</summary>
    /// <typeparam name="TBitmap">The bitmap return type, such as BitmapSource or Bitmap.</typeparam>
    /// <param name="exePath">The executable path.</param>
    /// <param name="bitmapType">The bitmap type marker.</param>
    /// <param name="scale">The requested logo scale, where 100 is the default.</param>
    /// <returns>The bitmap instance, or null when no logo is found.</returns>
    internal static TBitmap GetAppLogoFromProcessPath<TBitmap>(string exePath, TBitmap bitmapType, int scale)
        where TBitmap : class
    {
        if (exePath is null)
        {
            return null;
        }

        string directory = Path.GetDirectoryName(exePath);
        if (!Directory.Exists(directory))
        {
            return null;
        }

        string manifestPath = Path.Combine(directory, "AppxManifest.xml");
        if (!File.Exists(manifestPath))
        {
            return null;
        }

        string pathToLogo = ReadLogoPath(manifestPath);
        if (pathToLogo is null)
        {
            return null;
        }

        string finalLogoPath = FindLogoPath(directory, pathToLogo, scale);
        if (finalLogoPath is null || !File.Exists(finalLogoPath))
        {
            return null;
        }

        using FileStream fileStream = File.OpenRead(finalLogoPath);
        if (typeof(BitmapSource).IsAssignableFrom(typeof(TBitmap)))
        {
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = fileStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage as TBitmap;
        }

        if (typeof(Bitmap) != typeof(TBitmap))
        {
            return null;
        }

        using Image bitmap = Image.FromStream(fileStream);
        return bitmap.Clone() as TBitmap;
    }

    /// <summary>Loads an icon at the system-preferred size using a resource name dispatcher.</summary>
    /// <typeparam name="TIcon">The icon return type, such as Icon, Bitmap, or BitmapSource.</typeparam>
    /// <param name="iconType">The icon type marker.</param>
    /// <param name="instanceHandle">A handle to the module containing the icon resource.</param>
    /// <param name="iconName">The icon resource name.</param>
    /// <param name="metricSize">The metric size to use.</param>
    /// <returns>The loaded icon, or null if loading failed.</returns>
    private static TIcon LoadIconWithSystemMetrics<TIcon>(TIcon iconType, IntPtr instanceHandle, IconResourceName iconName, IconMetricSize metricSize)
        where TIcon : class
    {
        return LoadOwnedIcon(iconType, LoadIcon);
        int LoadIcon(out IntPtr iconHandle) => iconName.LoadWithSystemMetrics(instanceHandle, metricSize, out iconHandle);
    }

    /// <summary>Loads an icon with automatic scaling using a resource name dispatcher.</summary>
    /// <typeparam name="TIcon">The icon return type, such as Icon, Bitmap, or BitmapSource.</typeparam>
    /// <param name="iconType">The icon type marker.</param>
    /// <param name="instanceHandle">A handle to the module containing the icon resource.</param>
    /// <param name="iconName">The icon resource name.</param>
    /// <param name="width">The desired width of the icon in pixels.</param>
    /// <param name="height">The desired height of the icon in pixels.</param>
    /// <returns>The loaded icon, or null if loading failed.</returns>
    private static TIcon LoadIconWithScaleDown<TIcon>(TIcon iconType, IntPtr instanceHandle, IconResourceName iconName, int width, int height)
        where TIcon : class
    {
        return LoadOwnedIcon(iconType, LoadIcon);
        int LoadIcon(out IntPtr iconHandle) => iconName.LoadWithScaleDown(instanceHandle, width, height, out iconHandle);
    }

    /// <summary>Finds the logo path matching the requested scale.</summary>
    /// <param name="directory">The app directory.</param>
    /// <param name="pathToLogo">The manifest logo path.</param>
    /// <param name="scale">The requested scale.</param>
    /// <returns>The logo path, or null.</returns>
    private static string FindLogoPath(string directory, string pathToLogo, int scale)
    {
        if (string.IsNullOrEmpty(pathToLogo))
        {
            return null;
        }

        string logoDirectoryName = Path.GetDirectoryName(pathToLogo);
        if (logoDirectoryName is null)
        {
            return null;
        }

        string path = Path.Combine(directory, logoDirectoryName);
        string logoExtension = Path.GetExtension(pathToLogo);
        string logoName = Path.GetFileNameWithoutExtension(pathToLogo);
        string scaleSuffix = $".scale-{scale}{logoExtension}";
        string firstLogoPath = null;
        string[] files = Directory.GetFiles(path, $"{logoName}*{logoExtension}");
        foreach (string logoFile in files)
        {
            firstLogoPath ??= logoFile;

            if (logoFile.EndsWith(scaleSuffix, StringComparison.Ordinal))
            {
                return logoFile;
            }
        }

        return firstLogoPath;
    }

    /// <summary>Gets the path for the real modern app process belonging to the window.</summary>
    /// <param name="interopWindow">The interop window.</param>
    /// <returns>The process path, or null.</returns>
    private static string GetAppProcessPath(IInteropWindow interopWindow)
    {
        _ = User32Api.GetWindowThreadProcessId(interopWindow.Handle, out var processId);
        if (string.Equals(interopWindow.GetClassname(), AppQueryExtensions.AppFrameWindowClass, StringComparison.Ordinal))
        {
            processId = GetAppChildProcessId(interopWindow);
        }

        if (processId > 0)
        {
            return Kernel32Api.GetProcessPath(processId);
        }

        return null;
    }

    /// <summary>Gets the process id for the child app window.</summary>
    /// <param name="interopWindow">The app frame window.</param>
    /// <returns>The process id, or zero.</returns>
    private static int GetAppChildProcessId(IInteropWindow interopWindow)
    {
        foreach (IInteropWindow child in interopWindow.GetChildren())
        {
            if (string.Equals(AppQueryExtensions.AppWindowClass, child.GetClassname(), StringComparison.Ordinal))
            {
                return child.GetProcessId();
            }
        }

        return 0;
    }

    /// <summary>Loads and converts an owned icon handle.</summary>
    /// <typeparam name="TIcon">The icon return type.</typeparam>
    /// <param name="iconType">The icon type marker.</param>
    /// <param name="iconLoader">The native icon loader.</param>
    /// <returns>The icon, or null.</returns>
    private static TIcon LoadOwnedIcon<TIcon>(TIcon iconType, IconLoader iconLoader)
        where TIcon : class
    {
        int result = iconLoader(out var iconHandle);
        using SafeIconHandle safeIconHandle = new(iconHandle);
        return (result != 0 || safeIconHandle.IsInvalid) ? null : IconHandleTo(safeIconHandle, iconType);
    }

    /// <summary>Reads the manifest logo path.</summary>
    /// <param name="manifestPath">The manifest path.</param>
    /// <returns>The logo path, or null.</returns>
    private static string ReadLogoPath(string manifestPath)
    {
        using FileStream fileStream = File.OpenRead(manifestPath);
        XDocument xDocument = XDocument.Load((Stream)fileStream);
        XName propertiesNamespace = XName.Get("Properties", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");
        XName logoNamespace = XName.Get("Logo", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");
        return xDocument.Root?.Element(propertiesNamespace)?.Element(logoNamespace)?.Value;
    }
}
