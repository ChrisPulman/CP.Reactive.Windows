// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using ReactiveUI.Primitives.Disposables;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard.Internals;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.Internals;
#endif
/// <summary>Native clipboard methods.</summary>
internal static class NativeMethods
{
    /// <summary>Lazy shell32 module handle.</summary>
    private static readonly Lazy<IntPtr> Shell32Module = new(() => System.Runtime.InteropServices.NativeLibrary.Load("shell32.dll"));

    /// <summary>Clipboard data placement operation used by this type.</summary>
    private static Func<uint, IntPtr, IntPtr> _setClipboardData = SetClipboardData;

    /// <summary>Enumerates the data formats currently available on the clipboard.</summary>
    /// <param name="format">The previous clipboard format, or zero to begin enumeration.</param>
    /// <returns>The next available clipboard format.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern uint EnumClipboardFormats(uint format);

    /// <summary>Determines whether the clipboard contains data in the specified format.</summary>
    /// <param name="format">The clipboard format.</param>
    /// <returns><see langword="true" /> if the format is available.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IsClipboardFormatAvailable(uint format);

    /// <summary>Empties the clipboard and frees handles to data in the clipboard.</summary>
    /// <returns><see langword="true" /> if the clipboard was emptied.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool EmptyClipboard();

    /// <summary>Retrieves data from the clipboard in a specified format.</summary>
    /// <param name="format">The clipboard format.</param>
    /// <returns>A handle to the clipboard data.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern IntPtr GetClipboardData(uint format);

    /// <summary>Registers a new clipboard format.</summary>
    /// <param name="format">The format name.</param>
    /// <returns>The registered clipboard format identifier.</returns>
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegisterClipboardFormatW", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern uint RegisterClipboardFormat(string format);

    /// <summary>Gets the current clipboard owner window handle.</summary>
    /// <returns>The owner window handle.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern IntPtr GetClipboardOwner();

    /// <summary>Retrieves the name of a registered clipboard format.</summary>
    /// <param name="format">The clipboard format identifier.</param>
    /// <param name="formatName">The output character buffer.</param>
    /// <param name="capacity">The capacity of <paramref name="formatName" /> in characters.</param>
    /// <returns>The copied character count, or zero when no registered name is available.</returns>
    [DllImport("user32.dll", EntryPoint = "GetClipboardFormatNameW", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern unsafe int GetClipboardFormatName(uint format, char* formatName, int capacity);

    /// <summary>Gets the current clipboard sequence number.</summary>
    /// <returns>The clipboard sequence number.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern uint GetClipboardSequenceNumber();

    /// <summary>Retrieves dropped file names from an HDROP handle.</summary>
    /// <param name="dropHandle">The HDROP handle.</param>
    /// <param name="fileIndex">The file index, or uint.MaxValue for the count.</param>
    /// <param name="fileName">The output filename buffer.</param>
    /// <param name="characterCount">The output buffer character count.</param>
    /// <returns>The copied character count or file count.</returns>
    internal static unsafe int DragQueryFile(IntPtr dropHandle, uint fileIndex, char* fileName, int characterCount) => DragQueryFileNative(dropHandle, fileIndex, fileName, characterCount);

    /// <summary>Adds a window as a clipboard format listener.</summary>
    /// <param name="windowHandle">The listener window handle.</param>
    /// <returns><see langword="true" /> if the listener was added.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool AddClipboardFormatListener(IntPtr windowHandle);

    /// <summary>Removes a window as a clipboard format listener.</summary>
    /// <param name="windowHandle">The listener window handle.</param>
    /// <returns><see langword="true" /> if the listener was removed.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool RemoveClipboardFormatListener(IntPtr windowHandle);

    /// <summary>Places data on the clipboard in the specified format.</summary>
    /// <param name="format">The clipboard format.</param>
    /// <param name="memory">The global memory handle.</param>
    internal static void SetClipboardDataWithErrorHandling(uint format, IntPtr memory)
    {
        if (_setClipboardData(format, memory) == IntPtr.Zero && memory != IntPtr.Zero)
        {
            throw new Win32Exception();
        }
    }

    /// <summary>Overrides clipboard data placement for deterministic tests.</summary>
    /// <param name="setClipboardData">The replacement clipboard data placement operation.</param>
    /// <returns>A scope that restores the previous operation.</returns>
    internal static IDisposable OverrideSetClipboardDataForTesting(Func<uint, IntPtr, IntPtr> setClipboardData)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(setClipboardData);
        Func<uint, IntPtr, IntPtr> setClipboardData2 = _setClipboardData;
        _setClipboardData = setClipboardData;
        return Scope.Create(setClipboardData2, delegate(Func<uint, IntPtr, IntPtr> previous)
        {
            _setClipboardData = previous;
        });
    }

    /// <summary>Places data on the clipboard in the specified format.</summary>
    /// <param name="format">The clipboard format.</param>
    /// <param name="memory">The global memory handle.</param>
    /// <returns>The clipboard data handle.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern IntPtr SetClipboardData(uint format, IntPtr memory);

    /// <summary>Retrieves dropped file names from an HDROP handle.</summary>
    /// <param name="dropHandle">The HDROP handle.</param>
    /// <param name="fileIndex">The file index, or uint.MaxValue for the count.</param>
    /// <param name="fileName">The output filename buffer.</param>
    /// <param name="characterCount">The output buffer character count.</param>
    /// <returns>The copied character count or file count.</returns>
    private static unsafe int DragQueryFileNative(IntPtr dropHandle, uint fileIndex, char* fileName, int characterCount) => ((delegate* unmanaged[Stdcall]<IntPtr, uint, char*, int, int>)(void*)System.Runtime.InteropServices.NativeLibrary.GetExport(Shell32Module.Value, "DragQueryFileW"))(dropHandle, fileIndex, fileName, characterCount);
}
