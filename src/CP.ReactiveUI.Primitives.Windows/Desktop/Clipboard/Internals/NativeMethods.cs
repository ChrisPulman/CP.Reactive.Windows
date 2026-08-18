// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard.Internals;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.Internals;
#endif
/// <summary>Native clipboard methods.</summary>
#if NETFRAMEWORK
internal static unsafe class NativeMethods
#else
internal static unsafe partial class NativeMethods
#endif
{
    /// <summary>Lazy shell32 module handle.</summary>
    private static readonly Lazy<IntPtr> Shell32Module = new(static () => NativeLibrary.Load("shell32.dll"));

    /// <summary>Clipboard empty operation used by this type.</summary>
    private static Func<bool> _emptyClipboard = EmptyClipboardNative;

    /// <summary>Clipboard listener registration operation used by this type.</summary>
    private static Func<IntPtr, bool> _addClipboardFormatListener = AddClipboardFormatListenerNative;

    /// <summary>Clipboard listener removal operation used by this type.</summary>
    private static Func<IntPtr, bool> _removeClipboardFormatListener = RemoveClipboardFormatListenerNative;

    /// <summary>Dropped-file enumeration operation used by this type.</summary>
    private static DragQueryFileOperation _dragQueryFile = DragQueryFileNative;

    /// <summary>Clipboard data placement operation used by this type.</summary>
    private static Func<uint, IntPtr, IntPtr> _setClipboardData = SetClipboardData;

    /// <summary>Enumerates the data formats currently available on the clipboard.</summary>
    /// <param name="format">The previous clipboard format, or zero to begin enumeration.</param>
    /// <returns>The next available clipboard format.</returns>
#if NETFRAMEWORK
    [DllImport("user32.dll", EntryPoint = "EnumClipboardFormats", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern uint EnumClipboardFormats(uint format);
#else
    [LibraryImport("user32.dll", EntryPoint = "EnumClipboardFormats", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial uint EnumClipboardFormats(uint format);
#endif

    /// <summary>Determines whether the clipboard contains data in the specified format.</summary>
    /// <param name="format">The clipboard format.</param>
    /// <returns><see langword="true" /> if the format is available.</returns>
#if NETFRAMEWORK
    [DllImport("user32.dll", EntryPoint = "IsClipboardFormatAvailable", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IsClipboardFormatAvailable(uint format);
#else
    [LibraryImport("user32.dll", EntryPoint = "IsClipboardFormatAvailable", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool IsClipboardFormatAvailable(uint format);
#endif

    /// <summary>Retrieves data from the clipboard in a specified format.</summary>
    /// <param name="format">The clipboard format.</param>
    /// <returns>A handle to the clipboard data.</returns>
#if NETFRAMEWORK
    [DllImport("user32.dll", EntryPoint = "GetClipboardData", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern IntPtr GetClipboardData(uint format);
#else
    [LibraryImport("user32.dll", EntryPoint = "GetClipboardData", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial IntPtr GetClipboardData(uint format);
#endif

    /// <summary>Registers a new clipboard format.</summary>
    /// <param name="format">The format name.</param>
    /// <returns>The registered clipboard format identifier.</returns>
#if NETFRAMEWORK
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegisterClipboardFormatW", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern uint RegisterClipboardFormat(string format);
#else
    [LibraryImport("user32.dll", EntryPoint = "RegisterClipboardFormatW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial uint RegisterClipboardFormat(string format);
#endif

    /// <summary>Gets the current clipboard owner window handle.</summary>
    /// <returns>The owner window handle.</returns>
#if NETFRAMEWORK
    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern IntPtr GetClipboardOwner();
#else
    [LibraryImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial IntPtr GetClipboardOwner();
#endif

    /// <summary>Retrieves the name of a registered clipboard format.</summary>
    /// <param name="format">The clipboard format identifier.</param>
    /// <param name="formatName">The output character buffer.</param>
    /// <param name="capacity">The capacity of <paramref name="formatName" /> in characters.</param>
    /// <returns>The copied character count, or zero when no registered name is available.</returns>
#if NETFRAMEWORK
    [DllImport("user32.dll", EntryPoint = "GetClipboardFormatNameW", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern int GetClipboardFormatName(uint format, char* formatName, int capacity);
#else
    [LibraryImport("user32.dll", EntryPoint = "GetClipboardFormatNameW", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial int GetClipboardFormatName(uint format, char* formatName, int capacity);
#endif

    /// <summary>Gets the current clipboard sequence number.</summary>
    /// <returns>The clipboard sequence number.</returns>
#if NETFRAMEWORK
    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern uint GetClipboardSequenceNumber();
#else
    [LibraryImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial uint GetClipboardSequenceNumber();
#endif

    /// <summary>Retrieves dropped file names from an HDROP handle.</summary>
    /// <param name="dropHandle">The HDROP handle.</param>
    /// <param name="fileIndex">The file index, or uint.MaxValue for the count.</param>
    /// <param name="fileName">The output filename buffer.</param>
    /// <param name="characterCount">The output buffer character count.</param>
    /// <returns>The copied character count or file count.</returns>
    internal static int DragQueryFile(IntPtr dropHandle, uint fileIndex, char* fileName, int characterCount) =>
        _dragQueryFile(dropHandle, fileIndex, fileName, characterCount);

    /// <summary>Frees global memory that was not transferred to the clipboard.</summary>
    /// <param name="memoryHandle">The global memory handle.</param>
    /// <returns><see cref="F:System.IntPtr.Zero" /> when the memory was released.</returns>
#if NETFRAMEWORK
    [DllImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern IntPtr GlobalFree(IntPtr memoryHandle);
#else
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial IntPtr GlobalFree(IntPtr memoryHandle);
#endif

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

    /// <summary>Empties the clipboard and frees handles to data in the clipboard.</summary>
    /// <returns><see langword="true" /> if the clipboard was emptied.</returns>
    internal static bool EmptyClipboard() => _emptyClipboard();

    /// <summary>Adds a window as a clipboard format listener.</summary>
    /// <param name="windowHandle">The listener window handle.</param>
    /// <returns><see langword="true" /> if the listener was added.</returns>
    internal static bool AddClipboardFormatListener(IntPtr windowHandle) => _addClipboardFormatListener(windowHandle);

    /// <summary>Removes a window as a clipboard format listener.</summary>
    /// <param name="windowHandle">The listener window handle.</param>
    /// <returns><see langword="true" /> if the listener was removed.</returns>
    internal static bool RemoveClipboardFormatListener(IntPtr windowHandle) => _removeClipboardFormatListener(windowHandle);

    /// <summary>Overrides clipboard listener operations for deterministic tests.</summary>
    /// <param name="addClipboardFormatListener">The replacement listener registration operation.</param>
    /// <param name="removeClipboardFormatListener">The replacement listener removal operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideClipboardListenerOperationsForTesting(
        Func<IntPtr, bool> addClipboardFormatListener,
        Func<IntPtr, bool> removeClipboardFormatListener)
    {
        Throw.IfNull(addClipboardFormatListener);
        Throw.IfNull(removeClipboardFormatListener);
        var previousAdd = _addClipboardFormatListener;
        var previousRemove = _removeClipboardFormatListener;
        _addClipboardFormatListener = addClipboardFormatListener;
        _removeClipboardFormatListener = removeClipboardFormatListener;
        return Scope.Create((previousAdd, previousRemove), static previous => (_addClipboardFormatListener, _removeClipboardFormatListener) = previous);
    }

    /// <summary>Overrides clipboard emptying for deterministic tests.</summary>
    /// <param name="emptyClipboard">The replacement clipboard empty operation.</param>
    /// <returns>A scope that restores the previous operation.</returns>
    internal static IDisposable OverrideEmptyClipboardForTesting(Func<bool> emptyClipboard)
    {
        Throw.IfNull(emptyClipboard);
        var previous = _emptyClipboard;
        _emptyClipboard = emptyClipboard;
        return Scope.Create(previous, static previousOperation => _emptyClipboard = previousOperation);
    }

    /// <summary>Overrides dropped-file enumeration for deterministic tests.</summary>
    /// <param name="dragQueryFile">The replacement dropped-file enumeration operation.</param>
    /// <returns>A scope that restores the previous operation.</returns>
    internal static IDisposable OverrideDragQueryFileForTesting(DragQueryFileOperation dragQueryFile)
    {
        Throw.IfNull(dragQueryFile);
        var previous = _dragQueryFile;
        _dragQueryFile = dragQueryFile;
        return Scope.Create(previous, static previousOperation => _dragQueryFile = previousOperation);
    }

    /// <summary>Overrides dropped-file enumeration with an operation that does not access the output buffer.</summary>
    /// <param name="dragQueryFile">The replacement dropped-file enumeration operation.</param>
    /// <returns>A scope that restores the previous operation.</returns>
    internal static IDisposable OverrideDragQueryFileForTesting(Func<IntPtr, uint, int> dragQueryFile)
    {
        Throw.IfNull(dragQueryFile);
        return OverrideDragQueryFileForTesting((dropHandle, fileIndex, _, _) => dragQueryFile(dropHandle, fileIndex));
    }

    /// <summary>Overrides clipboard data placement for deterministic tests.</summary>
    /// <param name="setClipboardData">The replacement clipboard data placement operation.</param>
    /// <returns>A scope that restores the previous operation.</returns>
    internal static IDisposable OverrideSetClipboardDataForTesting(Func<uint, IntPtr, IntPtr> setClipboardData)
    {
        Throw.IfNull(setClipboardData);
        var setClipboardData2 = _setClipboardData;
        _setClipboardData = setClipboardData;
        return Scope.Create(setClipboardData2, static previous => _setClipboardData = previous);
    }

    /// <summary>Empties the clipboard and frees handles to data in the clipboard.</summary>
    /// <returns><see langword="true" /> if the clipboard was emptied.</returns>
#if NETFRAMEWORK
    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EmptyClipboardNative();
#else
    [LibraryImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EmptyClipboardNative();
#endif

    /// <summary>Adds a window as a clipboard format listener.</summary>
    /// <param name="windowHandle">The listener window handle.</param>
    /// <returns><see langword="true" /> if the listener was added.</returns>
#if NETFRAMEWORK
    [DllImport("user32.dll", EntryPoint = "AddClipboardFormatListener", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AddClipboardFormatListenerNative(IntPtr windowHandle);
#else
    [LibraryImport("user32.dll", EntryPoint = "AddClipboardFormatListener", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AddClipboardFormatListenerNative(IntPtr windowHandle);
#endif

    /// <summary>Removes a window as a clipboard format listener.</summary>
    /// <param name="windowHandle">The listener window handle.</param>
    /// <returns><see langword="true" /> if the listener was removed.</returns>
#if NETFRAMEWORK
    [DllImport("user32.dll", EntryPoint = "RemoveClipboardFormatListener", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveClipboardFormatListenerNative(IntPtr windowHandle);
#else
    [LibraryImport("user32.dll", EntryPoint = "RemoveClipboardFormatListener", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RemoveClipboardFormatListenerNative(IntPtr windowHandle);
#endif

    /// <summary>Places data on the clipboard in the specified format.</summary>
    /// <param name="format">The clipboard format.</param>
    /// <param name="memory">The global memory handle.</param>
    /// <returns>The clipboard data handle.</returns>
#if NETFRAMEWORK
    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern IntPtr SetClipboardData(uint format, IntPtr memory);
#else
    [LibraryImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial IntPtr SetClipboardData(uint format, IntPtr memory);
#endif

    /// <summary>Retrieves dropped file names from an HDROP handle.</summary>
    /// <param name="dropHandle">The HDROP handle.</param>
    /// <param name="fileIndex">The file index, or uint.MaxValue for the count.</param>
    /// <param name="fileName">The output filename buffer.</param>
    /// <param name="characterCount">The output buffer character count.</param>
    /// <returns>The copied character count or file count.</returns>
    private static int DragQueryFileNative(
        IntPtr dropHandle,
        uint fileIndex,
        char* fileName,
        int characterCount)
    {
        var export = NativeLibrary.GetExport(Shell32Module.Value, "DragQueryFileW");
        delegate* unmanaged[Stdcall]<IntPtr, uint, char*, int, int> dragQueryFile =
            (delegate* unmanaged[Stdcall]<IntPtr, uint, char*, int, int>)(void*)export;
        return dragQueryFile(dropHandle, fileIndex, fileName, characterCount);
    }
}
