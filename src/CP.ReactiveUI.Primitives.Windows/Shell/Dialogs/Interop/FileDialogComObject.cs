// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs.Interop;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop;
#endif
/// <summary>Base wrapper for native IFileDialog-compatible interfaces.</summary>
internal class FileDialogComObject : ComObject
{
    /// <summary>The Show vtable slot.</summary>
    private const int ShowSlot = 3;

    /// <summary>The SetFileTypes vtable slot.</summary>
    private const int SetFileTypesSlot = 4;

    /// <summary>The SetFileTypeIndex vtable slot.</summary>
    private const int SetFileTypeIndexSlot = 5;

    /// <summary>The SetOptions vtable slot.</summary>
    private const int SetOptionsSlot = 9;

    /// <summary>The SetFolder vtable slot.</summary>
    private const int SetFolderSlot = 12;

    /// <summary>The SetFileName vtable slot.</summary>
    private const int SetFileNameSlot = 15;

    /// <summary>The SetTitle vtable slot.</summary>
    private const int SetTitleSlot = 17;

    /// <summary>The GetResult vtable slot.</summary>
    private const int GetResultSlot = 20;

    /// <summary>The AddPlace vtable slot.</summary>
    private const int AddPlaceSlot = 21;

    /// <summary>The SetDefaultExtension vtable slot.</summary>
    private const int SetDefaultExtensionSlot = 22;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop.FileDialogComObject" /> class.</summary>
    /// <param name="handle">The owned COM interface pointer.</param>
    protected FileDialogComObject(IntPtr handle)
        : base(handle)
    {
    }

    /// <summary>Shows the dialog.</summary>
    /// <param name="ownerHandle">The owner window handle.</param>
    /// <returns>The HRESULT returned by the dialog.</returns>
    internal virtual unsafe int Show(IntPtr ownerHandle) => ((delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int>)(void*)GetMethod(3))(base.Handle, ownerHandle);

    /// <summary>Sets the file filters.</summary>
    /// <param name="filterSpecs">The filter specifications.</param>
    internal virtual unsafe void SetFileTypes(FilterSpec[] filterSpecs)
    {
        using NativeFilterSpecs nativeFilters = new(filterSpecs);
        delegate* unmanaged[Stdcall]<IntPtr, uint, IntPtr, int> method = (delegate* unmanaged[Stdcall]<IntPtr, uint, IntPtr, int>)(void*)GetMethod(4);
        ComObject.ThrowIfFailed(nativeFilters.SetFileTypes(method, base.Handle));
    }

    /// <summary>Sets the selected file type index.</summary>
    /// <param name="fileTypeIndex">The one-based file type index.</param>
    internal virtual unsafe void SetFileTypeIndex(uint fileTypeIndex) => ComObject.ThrowIfFailed(((delegate* unmanaged[Stdcall]<IntPtr, uint, int>)(void*)GetMethod(5))(base.Handle, fileTypeIndex));

    /// <summary>Sets the dialog options.</summary>
    /// <param name="options">The options.</param>
    internal virtual unsafe void SetOptions(FileOpenOptions options) => ComObject.ThrowIfFailed(((delegate* unmanaged[Stdcall]<IntPtr, FileOpenOptions, int>)(void*)GetMethod(9))(base.Handle, options));

    /// <summary>Sets the initial folder.</summary>
    /// <param name="shellItem">The shell item.</param>
    internal virtual unsafe void SetFolder(IShellItem shellItem) => ComObject.ThrowIfFailed(((delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int>)(void*)GetMethod(12))(base.Handle, shellItem.Handle));

    /// <summary>Sets the file name.</summary>
    /// <param name="name">The file name.</param>
    internal virtual unsafe void SetFileName(string name)
    {
        delegate* unmanaged[Stdcall]<IntPtr, char*, int> method = (delegate* unmanaged[Stdcall]<IntPtr, char*, int>)(void*)GetMethod(15);
        fixed (char* namePointer = name)
        {
            ComObject.ThrowIfFailed(method(base.Handle, namePointer));
        }
    }

    /// <summary>Sets the title.</summary>
    /// <param name="title">The title.</param>
    internal virtual unsafe void SetTitle(string title)
    {
        delegate* unmanaged[Stdcall]<IntPtr, char*, int> method = (delegate* unmanaged[Stdcall]<IntPtr, char*, int>)(void*)GetMethod(17);
        fixed (char* titlePointer = title)
        {
            ComObject.ThrowIfFailed(method(base.Handle, titlePointer));
        }
    }

    /// <summary>Gets the selected item.</summary>
    /// <returns>The selected item.</returns>
    internal virtual unsafe IShellItem GetResult()
    {
        IntPtr item = default;
        ComObject.ThrowIfFailed(((delegate* unmanaged[Stdcall]<IntPtr, out IntPtr, int>)(void*)GetMethod(20))(base.Handle, out item));
        return new(item);
    }

    /// <summary>Adds a place to the dialog navigation list.</summary>
    /// <param name="shellItem">The shell item.</param>
    /// <param name="addPlaceFlags">The add-place flags.</param>
    internal virtual unsafe void AddPlace(IShellItem shellItem, FileDialogAddPlaceFlags addPlaceFlags) => ComObject.ThrowIfFailed(((delegate* unmanaged[Stdcall]<IntPtr, IntPtr, FileDialogAddPlaceFlags, int>)(void*)GetMethod(21))(base.Handle, shellItem.Handle, addPlaceFlags));

    /// <summary>Sets the default extension.</summary>
    /// <param name="defaultExtension">The default extension.</param>
    internal virtual unsafe void SetDefaultExtension(string defaultExtension)
    {
        delegate* unmanaged[Stdcall]<IntPtr, char*, int> method = (delegate* unmanaged[Stdcall]<IntPtr, char*, int>)(void*)GetMethod(22);
        fixed (char* extensionPointer = defaultExtension)
        {
            ComObject.ThrowIfFailed(method(base.Handle, extensionPointer));
        }
    }
}
