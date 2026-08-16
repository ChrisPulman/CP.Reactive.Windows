// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs;
#endif
/// <summary>Shared internal helper that handles all COM interactions for the file and folder dialog builders.</summary>
#if NETFRAMEWORK
internal static class ComDialogHelper
#else
internal static partial class ComDialogHelper
#endif
{
    /// <summary>HRESULT returned when the user dismisses the dialog via Cancel or Escape.</summary>
    internal const int HResultCancelled = -2_147_023_673;

    /// <summary>The FileOpenDialog COM class identifier.</summary>
    internal static readonly Guid ClsidFileOpenDialog = new("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7");

    /// <summary>The FileSaveDialog COM class identifier.</summary>
    internal static readonly Guid ClsidFileSaveDialog = new("C0B4E2F3-BA21-4773-8DBA-335EC946EB8B");

    /// <summary>The IShellItem COM interface identifier.</summary>
    private static readonly Guid IidIShellItem = new("43826D1E-E718-42EE-BC55-A1E261C37BFE");

    /// <summary>Gets or sets the native COM instance factory.</summary>
    internal static Func<Guid, Guid, (int ResultCode, IntPtr Dialog)> CreateDialogInstance { get; set; } = CreateDialogInstanceCore;

    /// <summary>Gets or sets the native shell item factory.</summary>
    internal static Func<string, Guid, (int ResultCode, IntPtr Item)> CreateShellItemInstance { get; set; } = CreateShellItemInstanceCore;

    /// <summary>Gets or sets the shell item wrapper factory.</summary>
    internal static Func<IntPtr, IShellItem> WrapShellItem { get; set; } = WrapShellItemCore;

    /// <summary>Gets or sets the low-level COM dialog activation operation.</summary>
    internal static Func<Guid, Guid, (int ResultCode, IntPtr Dialog)> NativeDialogActivation { get; set; } = ActivateNativeDialog;

    /// <summary>Gets or sets the low-level shell item creation operation.</summary>
    internal static Func<string, Guid, (int ResultCode, IntPtr Item)> NativeShellItemCreation { get; set; } = CreateNativeShellItem;

    /// <summary>Gets or sets the platform COM activation operation.</summary>
    internal static CoCreateInstanceOperation CoCreateInstance { get; set; } = NativeMethods.CoCreateInstance;

    /// <summary>Gets or sets the platform shell-item creation operation.</summary>
    internal static CreateShellItemOperation CreateShellItem { get; set; } = NativeMethods.SHCreateItemFromParsingName;

    /// <summary>Creates a COM dialog coclass instance and casts it to <typeparamref name="T" />.</summary>
    /// <exception cref="T:System.PlatformNotSupportedException">Called on a non-Windows platform.</exception>
    /// <exception cref="T:System.InvalidOperationException">The COM object could not be instantiated.</exception>
    /// <typeparam name="T">The COM dialog interface type.</typeparam>
    /// <param name="clsid">The clsid value.</param>
    /// <returns>The current builder or result value.</returns>
    internal static T CreateDialog<T>(Guid clsid)
        where T : ComObject
    {
        Guid interfaceId = GetInterfaceId<T>();
        var (resultCode, dialog) = CreateDialogInstance(clsid, interfaceId);
        Marshal.ThrowExceptionForHR(resultCode);
        return typeof(T) == typeof(IFileOpenDialog)
            ? (T)(ComObject)new IFileOpenDialog(dialog)
            : (T)(ComObject)new IFileSaveDialog(dialog);
    }

    /// <summary>
    /// Converts filter tuples to
    /// <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop.FilterSpec" />
    /// structs and applies them to the dialog.
    /// </summary>
    /// <param name="setFileTypes">The method used to apply the filter specifications.</param>
    /// <param name="setFileTypeIndex">The method used to select the active filter index.</param>
    /// <param name="filters">The configured filters.</param>
    internal static void ApplyFilters(Action<FilterSpec[]> setFileTypes, Action<uint> setFileTypeIndex, IReadOnlyList<(string Name, string Pattern)> filters)
    {
        if (filters is null || filters.Count == 0)
        {
            return;
        }

        FilterSpec[] specs = new FilterSpec[filters.Count];
        for (int i = 0; i < filters.Count; i = checked(i + 1))
        {
            specs[i] = new(filters[i].Name, filters[i].Pattern);
        }

        setFileTypes(specs);
        setFileTypeIndex(1U);
    }

    /// <summary>Sets the initial folder on the dialog.</summary>
    /// <param name="setFolder">The method used to set the dialog folder.</param>
    /// <param name="path">The configured initial directory path.</param>
    internal static void ApplyInitialDirectory(Action<IShellItem> setFolder, string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return;
        }

        try
        {
            using IShellItem shellItem = ShellItemFromPath(path);
            setFolder(shellItem);
        }
        catch (COMException)
        {
        }
    }

    /// <summary>Adds custom places to the dialog's navigation sidebar.</summary>
    /// <param name="addPlace">The method used to add a custom place.</param>
    /// <param name="places">The configured custom places.</param>
    internal static void ApplyPlaces(Action<IShellItem, FileDialogAddPlaceFlags> addPlace, IReadOnlyList<(string Path, bool AtTop)> places)
    {
        if (places is null || places.Count == 0)
        {
            return;
        }

        foreach (var (path, atTop) in places)
        {
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                try
                {
                    using IShellItem shellItem = ShellItemFromPath(path);
                    addPlace(shellItem, atTop ? FileDialogAddPlaceFlags.Top : FileDialogAddPlaceFlags.Bottom);
                }
                catch (COMException)
                {
                }
            }
        }
    }

    /// <summary>
    /// Returns the file-system path string for the given
    /// <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop.IShellItem" />.
    /// </summary>
    /// <param name="item">The shell item.</param>
    /// <returns>The file-system path.</returns>
    internal static string GetFileSysPath(IShellItem item) => item.GetDisplayName(ShellItemDisplayName.FileSysPath);

    /// <summary>
    /// Collects file-system paths from all items in an
    /// <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop.IShellItemArray" />.
    /// </summary>
    /// <param name="items">The shell item array.</param>
    /// <returns>The collected file-system paths.</returns>
    internal static IReadOnlyList<string> CollectPaths(IShellItemArray items)
    {
        using (items)
        {
            uint count = items.GetCount();
            checked
            {
                List<string> result = new((int)count);
                for (uint i = 0U; i < count; i++)
                {
                    using IShellItem item = items.GetItemAt(i);
                    result.Add(GetFileSysPath(item));
                }

                return result;
            }
        }
    }

    /// <summary>Creates an IShellItem from a file-system path.</summary>
    /// <param name="path">The file-system path.</param>
    /// <returns>The created shell item.</returns>
    internal static IShellItem ShellItemFromPath(string path)
    {
        Guid iid = IidIShellItem;
        var (resultCode, item) = CreateShellItemInstance(path, iid);
        Marshal.ThrowExceptionForHR(resultCode);
        return WrapShellItem(item);
    }

    /// <summary>Restores native helper delegates after deterministic tests.</summary>
    internal static void RestoreNativeFactoriesForTesting()
    {
        CreateDialogInstance = CreateDialogInstanceCore;
        CreateShellItemInstance = CreateShellItemInstanceCore;
        WrapShellItem = WrapShellItemCore;
        NativeDialogActivation = ActivateNativeDialog;
        NativeShellItemCreation = CreateNativeShellItem;
        CoCreateInstance = NativeMethods.CoCreateInstance;
        CreateShellItem = NativeMethods.SHCreateItemFromParsingName;
    }

    /// <summary>Gets the COM interface identifier for a wrapper type.</summary>
    /// <typeparam name="T">The dialog wrapper type.</typeparam>
    /// <returns>The COM interface identifier.</returns>
    private static Guid GetInterfaceId<T>()
        where T : ComObject
    {
        if (typeof(T) == typeof(IFileOpenDialog))
        {
            return IFileOpenDialog.InterfaceId;
        }

        if (typeof(T) == typeof(IFileSaveDialog))
        {
            return IFileSaveDialog.InterfaceId;
        }

        throw new PlatformNotSupportedException("The Windows Common Item Dialog is only available on Windows Vista or later.");
    }

    /// <summary>Creates a native COM instance.</summary>
    /// <param name="clsid">The class identifier.</param>
    /// <param name="interfaceId">The requested interface identifier.</param>
    /// <returns>The native result and interface pointer.</returns>
    private static (int ResultCode, IntPtr Dialog) CreateDialogInstanceCore(Guid clsid, Guid interfaceId) =>
        NativeDialogActivation(clsid, interfaceId);

    /// <summary>Activates a native COM dialog using the Windows COM runtime.</summary>
    /// <param name="clsid">The class identifier.</param>
    /// <param name="interfaceId">The requested interface identifier.</param>
    /// <returns>The native result and interface pointer.</returns>
    private static (int ResultCode, IntPtr Dialog) ActivateNativeDialog(Guid clsid, Guid interfaceId)
    {
        int resultCode = CoCreateInstance(
            ref clsid,
            IntPtr.Zero,
            NativeMethods.ClsctxInprocServer,
            ref interfaceId,
            out var dialog);
        return (ResultCode: resultCode, Dialog: dialog);
    }

    /// <summary>Creates a native shell item instance.</summary>
    /// <param name="path">The file-system path.</param>
    /// <param name="interfaceId">The shell item interface identifier.</param>
    /// <returns>The native result and shell item pointer.</returns>
    private static (int ResultCode, IntPtr Item) CreateShellItemInstanceCore(string path, Guid interfaceId) =>
        NativeShellItemCreation(path, interfaceId);

    /// <summary>Creates a native shell item using the Windows shell runtime.</summary>
    /// <param name="path">The file-system path.</param>
    /// <param name="interfaceId">The requested interface identifier.</param>
    /// <returns>The native result and shell item pointer.</returns>
    private static (int ResultCode, IntPtr Item) CreateNativeShellItem(string path, Guid interfaceId)
    {
        Guid iid = interfaceId;
        int resultCode = CreateShellItem(path, IntPtr.Zero, ref iid, out var item);
        return (ResultCode: resultCode, Item: item);
    }

    /// <summary>Wraps a shell item pointer.</summary>
    /// <param name="handle">The shell item handle.</param>
    /// <returns>The shell item wrapper.</returns>
    private static IShellItem WrapShellItemCore(IntPtr handle) => new(handle);

    /// <summary>Native shell helpers.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>In-process COM server class context.</summary>
        internal const uint ClsctxInprocServer = 1U;

        /// <summary>Creates a COM object instance.</summary>
        /// <param name="classId">The COM class identifier.</param>
        /// <param name="outerUnknown">The controlling unknown for aggregation.</param>
        /// <param name="classContext">The class context.</param>
        /// <param name="interfaceId">The requested interface identifier.</param>
        /// <param name="instance">The created COM interface pointer.</param>
        /// <returns>The native HRESULT.</returns>
#if NETFRAMEWORK
        [DllImport("ole32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int CoCreateInstance(
            ref Guid classId,
            IntPtr outerUnknown,
            uint classContext,
            ref Guid interfaceId,
            out IntPtr instance);
#else
        [LibraryImport("ole32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int CoCreateInstance(
            ref Guid classId,
            IntPtr outerUnknown,
            uint classContext,
            ref Guid interfaceId,
            out IntPtr instance);
#endif

        /// <summary>Creates a shell item from a file-system parsing name.</summary>
        /// <param name="path">The file-system path.</param>
        /// <param name="bindContext">The optional bind context.</param>
        /// <param name="interfaceId">The requested COM interface identifier.</param>
        /// <param name="shellItem">The created shell item.</param>
        /// <returns>The native HRESULT.</returns>
#if NETFRAMEWORK
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int SHCreateItemFromParsingName(
            string path,
            IntPtr bindContext,
            ref Guid interfaceId,
            out IntPtr shellItem);
#else
        [LibraryImport("shell32.dll", EntryPoint = "SHCreateItemFromParsingName", StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int SHCreateItemFromParsingName(
            string path,
            IntPtr bindContext,
            ref Guid interfaceId,
            out IntPtr shellItem);
#endif
    }
}
