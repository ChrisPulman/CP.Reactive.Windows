// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Exercises dialog COM wrappers through deterministic, unmanaged in-memory vtables.</summary>
public sealed class CoverageReleaseDialogInteropTests
{
    /// <summary>The successful HRESULT.</summary>
    private const int Success = 0;

    /// <summary>The generic COM failure HRESULT.</summary>
    private const int EFail = unchecked((int)0x80004005);

    /// <summary>The deterministic selected path.</summary>
    private const string SelectedPath = "c:\\selected.txt";

    /// <summary>The first deterministic result path.</summary>
    private const string ResultPath = "c:\\result.txt";

    /// <summary>The second deterministic result path.</summary>
    private const string SecondResultPath = "c:\\second.txt";

    /// <summary>The alternate deterministic result path.</summary>
    private const string OtherResultPath = "c:\\other.txt";

    /// <summary>The text filter name.</summary>
    private const string TextFilterName = "Text";

    /// <summary>The text filter pattern.</summary>
    private const string TextFilterPattern = "*.txt";

    /// <summary>The text file extension.</summary>
    private const string TextExtension = "txt";

    /// <summary>The open dialog title.</summary>
    private const string OpenDialogTitle = "Open result";

    /// <summary>The save file name.</summary>
    private const string SaveFileName = "save.txt";

    /// <summary>The expected number of selected items and configured filters.</summary>
    private const uint ExpectedItemCount = 2U;

    /// <summary>Releases a COM interface reference.</summary>
    /// <param name="instance">The COM interface pointer.</param>
    /// <returns>The remaining reference count.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate uint ReleaseDelegate(IntPtr instance);

    /// <summary>Displays a COM dialog.</summary>
    /// <param name="instance">The COM interface pointer.</param>
    /// <param name="owner">The owner window handle.</param>
    /// <returns>The HRESULT.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int ShowDelegate(IntPtr instance, IntPtr owner);

    /// <summary>Sets COM file-type filters.</summary>
    /// <param name="instance">The COM interface pointer.</param>
    /// <param name="count">The number of filters.</param>
    /// <param name="filters">The native filter buffer.</param>
    /// <returns>The HRESULT.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int SetFileTypesDelegate(IntPtr instance, uint count, IntPtr filters);

    /// <summary>Sets a COM file-type index.</summary>
    /// <param name="instance">The COM interface pointer.</param>
    /// <param name="index">The selected filter index.</param>
    /// <returns>The HRESULT.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int SetFileTypeIndexDelegate(IntPtr instance, uint index);

    /// <summary>Sets COM dialog options.</summary>
    /// <param name="instance">The COM interface pointer.</param>
    /// <param name="options">The dialog options.</param>
    /// <returns>The HRESULT.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int SetOptionsDelegate(IntPtr instance, FileOpenOptions options);

    /// <summary>Sets a COM dialog folder.</summary>
    /// <param name="instance">The COM interface pointer.</param>
    /// <param name="folder">The shell item pointer.</param>
    /// <returns>The HRESULT.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int SetFolderDelegate(IntPtr instance, IntPtr folder);

    /// <summary>Sets a COM dialog string value.</summary>
    /// <param name="instance">The COM interface pointer.</param>
    /// <param name="value">The native string pointer.</param>
    /// <returns>The HRESULT.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int SetStringDelegate(IntPtr instance, IntPtr value);

    /// <summary>Gets a COM dialog result.</summary>
    /// <param name="instance">The COM interface pointer.</param>
    /// <param name="item">The returned shell item pointer.</param>
    /// <returns>The HRESULT.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int GetResultDelegate(IntPtr instance, out IntPtr item);

    /// <summary>Adds a COM dialog navigation place.</summary>
    /// <param name="instance">The COM interface pointer.</param>
    /// <param name="place">The shell item pointer.</param>
    /// <param name="flags">The placement flags.</param>
    /// <returns>The HRESULT.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int AddPlaceDelegate(IntPtr instance, IntPtr place, FileDialogAddPlaceFlags flags);

    /// <summary>Gets the selected COM dialog items.</summary>
    /// <param name="instance">The COM interface pointer.</param>
    /// <param name="items">The returned shell item array pointer.</param>
    /// <returns>The HRESULT.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int GetResultsDelegate(IntPtr instance, out IntPtr items);

    /// <summary>Gets a shell item display name.</summary>
    /// <param name="instance">The COM interface pointer.</param>
    /// <param name="displayName">The requested display-name shape.</param>
    /// <param name="name">The returned task-memory string pointer.</param>
    /// <returns>The HRESULT.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int GetDisplayNameDelegate(IntPtr instance, ShellItemDisplayName displayName, out IntPtr name);

    /// <summary>Gets the number of shell items.</summary>
    /// <param name="instance">The COM interface pointer.</param>
    /// <param name="count">The returned number of items.</param>
    /// <returns>The HRESULT.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int GetCountDelegate(IntPtr instance, out uint count);

    /// <summary>Gets a shell item by index.</summary>
    /// <param name="instance">The COM interface pointer.</param>
    /// <param name="index">The item index.</param>
    /// <param name="item">The returned shell item pointer.</param>
    /// <returns>The HRESULT.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int GetItemAtDelegate(IntPtr instance, uint index, out IntPtr item);

    /// <summary>Verifies every FileDialog COM wrapper forwards to its documented vtable slot.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DialogComWrappersUseInMemoryVtablesAsync()
    {
        using var firstItem = new NativeShellItem(ResultPath);
        using var secondItem = new NativeShellItem(SecondResultPath);
        using var results = new NativeShellItemArray([firstItem.Pointer, secondItem.Pointer]);
        using var nativeDialog = new NativeDialog(firstItem.Pointer, results.Pointer);
        using var open = new IFileOpenDialog(nativeDialog.Pointer);
        using var save = new IFileSaveDialog(nativeDialog.Pointer);

        await Assert.That(open.Show(new(FortyTwo))).IsEqualTo(Success);
        open.SetFileTypes([new(TextFilterName, TextFilterPattern), new("All", "*.*")]);
        open.SetFileTypeIndex(ExpectedItemCount);
        open.SetOptions(FileOpenOptions.FileMustExist | FileOpenOptions.AllowMultiSelect);
        using (var folder = new IShellItem(firstItem.Pointer))
        {
            open.SetFolder(folder);
            open.AddPlace(folder, FileDialogAddPlaceFlags.Top);
        }

        open.SetFileName(ResultPath);
        open.SetTitle(OpenDialogTitle);
        open.SetDefaultExtension(TextExtension);
        using (IShellItem result = open.GetResult())
        {
            await Assert.That(result.GetDisplayName(ShellItemDisplayName.FileSysPath)).IsEqualTo(ResultPath);
        }

        using (IShellItemArray selected = open.GetResults())
        {
            await Assert.That(selected.GetCount()).IsEqualTo(ExpectedItemCount);
            using IShellItem selectedItem = selected.GetItemAt(One);
            await Assert.That(selectedItem.GetDisplayName(ShellItemDisplayName.FileSysPath)).IsEqualTo(SecondResultPath);
        }

        save.SetFileName(SaveFileName);
        await Assert.That(nativeDialog.LastOwner).IsEqualTo(new(FortyTwo));
        await Assert.That(nativeDialog.FilterCount).IsEqualTo(ExpectedItemCount);
        await Assert.That(nativeDialog.FirstFilterName).IsEqualTo(TextFilterName);
        await Assert.That(nativeDialog.FileTypeIndex).IsEqualTo(ExpectedItemCount);
        await Assert.That(nativeDialog.Options).IsEqualTo(FileOpenOptions.FileMustExist | FileOpenOptions.AllowMultiSelect);
        await Assert.That(nativeDialog.Folder).IsEqualTo(firstItem.Pointer);
        await Assert.That(nativeDialog.PlaceFlags).IsEqualTo(FileDialogAddPlaceFlags.Top);
        await Assert.That(nativeDialog.FileName).IsEqualTo(SaveFileName);
        await Assert.That(nativeDialog.Title).IsEqualTo(OpenDialogTitle);
        await Assert.That(nativeDialog.DefaultExtension).IsEqualTo(TextExtension);

        const string nullValue = null!;
        open.SetFileName(nullValue);
        open.SetTitle(nullValue);
        open.SetDefaultExtension(nullValue);
        await Assert.That(nativeDialog.FileName).IsNull();
        await Assert.That(nativeDialog.Title).IsNull();
        await Assert.That(nativeDialog.DefaultExtension).IsNull();

        nativeDialog.OperationResult = EFail;
        await Assert.That(() => open.SetOptions(FileOpenOptions.PickFolders)).Throws<COMException>();
    }

    /// <summary>Verifies the native executor maps all result branches without invoking Windows UI.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeFileDialogExecutorUsesDeterministicComDialogsAsync()
    {
        using var item = new NativeShellItem(SelectedPath);
        using var otherItem = new NativeShellItem(OtherResultPath);
        using var items = new NativeShellItemArray([item.Pointer, otherItem.Pointer]);
        using var openDialog = new NativeDialog(item.Pointer, items.Pointer);
        using var saveDialog = new NativeDialog(item.Pointer, items.Pointer);
        var executor = new NativeFileDialogExecutor(
            () => new IFileOpenDialog(openDialog.Pointer),
            () => new IFileSaveDialog(saveDialog.Pointer));
        var existingDirectory = Path.GetTempPath();
        ComDialogHelper.CreateShellItemInstance = (_, _) => (Success, item.Pointer);
        ComDialogHelper.WrapShellItem = static handle => new IShellItem(handle);
        var defaultDialogActivation = new DialogActivation(openDialog.Pointer);

        try
        {
            var filters = new[] { (TextFilterName, TextFilterPattern) };
            var places = new[] { (existingDirectory, true), (existingDirectory, false) };
            var open = executor.ShowOpen(new(One, "Open", existingDirectory, TextExtension, filters, places, false));
            var multiple = executor.ShowOpen(new(One, null, null, null, [], [], true));
            var saved = executor.ShowSave(new(Two, "Save", existingDirectory, SaveFileName, TextExtension, filters, places));
            var folder = executor.ShowFolder(new(Three, "Folder", existingDirectory));

            await Assert.That(open.SelectedPath).IsEqualTo(SelectedPath);
            await Assert.That(multiple.SelectedPaths).IsEquivalentTo([SelectedPath, OtherResultPath]);
            await Assert.That(saved.SelectedPath).IsEqualTo(SelectedPath);
            await Assert.That(folder.SelectedPath).IsEqualTo(SelectedPath);
            await Assert.That(openDialog.Options).IsEqualTo(FileOpenOptions.PickFolders);
            await Assert.That(saveDialog.Options).IsEqualTo(FileOpenOptions.OverwritePrompt);

            ComDialogHelper.CoCreateInstance = defaultDialogActivation.Create;
            var defaultExecutor = new NativeFileDialogExecutor();
            var defaultOpen = defaultExecutor.ShowOpen(new(0L, null, null, null, [], [], false));
            var defaultSave = defaultExecutor.ShowSave(new(0L, null, null, null, null, [], []));
            await Assert.That(defaultOpen.SelectedPath).IsEqualTo(SelectedPath);
            await Assert.That(defaultSave.SelectedPath).IsEqualTo(SelectedPath);

            openDialog.ShowResult = ComDialogHelper.HResultCancelled;
            saveDialog.ShowResult = ComDialogHelper.HResultCancelled;
            await Assert.That(executor.ShowOpen(new(0L, null, null, null, [], [], false)).WasCancelled).IsTrue();
            await Assert.That(executor.ShowSave(new(0L, null, null, null, null, [], [])).WasCancelled).IsTrue();
            await Assert.That(executor.ShowFolder(new(0L, null, null)).WasCancelled).IsTrue();

            openDialog.ShowResult = EFail;
            await Assert.That(() => executor.ShowOpen(new(0L, null, null, null, [], [], false))).Throws<COMException>();
        }
        finally
        {
            ComDialogHelper.RestoreNativeFactoriesForTesting();
        }
    }

    /// <summary>Verifies executor constructor guards and the public save builder delegation path.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeExecutorGuardsAndSaveBuilderDefaultOverloadAsync()
    {
        Func<IFileOpenDialog> noOpenFactory = null;
        Func<IFileSaveDialog> noSaveFactory = null;
        await Assert.That(() => new NativeFileDialogExecutor(noOpenFactory, static () => new IFileSaveDialog(IntPtr.Zero)))
            .Throws<ArgumentNullException>();
        await Assert.That(() => new NativeFileDialogExecutor(static () => new IFileOpenDialog(IntPtr.Zero), noSaveFactory))
            .Throws<ArgumentNullException>();

        var executor = new SaveOnlyExecutor();
        DialogFileDialog.DialogExecutor = executor;
        try
        {
            FileDialogResult result = new FileSaveDialogBuilder().ShowDialog();
            await Assert.That(result.WasCancelled).IsTrue();
            await Assert.That(executor.SaveCalls).IsEqualTo(1);
        }
        finally
        {
            DialogFileDialog.RestoreExecutorForTesting();
        }
    }

    /// <summary>Captures the public builder's save request.</summary>
    private sealed class SaveOnlyExecutor : IFileDialogExecutor
    {
        /// <summary>Gets the number of save calls.</summary>
        public int SaveCalls { get; private set; }

        /// <inheritdoc />
        public FileDialogResult ShowOpen(FileOpenDialogRequest request) => FileDialogResult.Cancelled();

        /// <inheritdoc />
        public FileDialogResult ShowSave(FileSaveDialogRequest request)
        {
            SaveCalls++;
            return FileDialogResult.Cancelled();
        }

        /// <inheritdoc />
        public FileDialogResult ShowFolder(FolderPickerDialogRequest request) => FileDialogResult.Cancelled();
    }

    /// <summary>Provides a deterministic Common Item Dialog activation operation.</summary>
    /// <param name="pointer">The in-memory COM dialog pointer to return.</param>
    private sealed class DialogActivation(IntPtr pointer)
    {
        /// <summary>Returns the configured in-memory dialog for a requested COM interface.</summary>
        /// <param name="classId">The requested COM class identifier.</param>
        /// <param name="outerUnknown">The optional outer unknown pointer.</param>
        /// <param name="classContext">The requested COM activation context.</param>
        /// <param name="interfaceId">The requested COM interface identifier.</param>
        /// <param name="instance">The activated COM interface pointer.</param>
        /// <returns>A successful HRESULT.</returns>
        internal int Create(ref Guid classId, IntPtr outerUnknown, uint classContext, ref Guid interfaceId, out IntPtr instance)
        {
            _ = classId;
            _ = outerUnknown;
            _ = classContext;
            _ = interfaceId;
            instance = pointer;
            return Success;
        }
    }

    /// <summary>Owns a small unmanaged COM-compatible object and its rooted vtable delegates.</summary>
    private sealed class ComVtable : IDisposable
    {
        /// <summary>The first vtable slot.</summary>
        private const int FirstVtableSlot = 0;

        /// <summary>The IUnknown release vtable slot.</summary>
        private const int ReleaseVtableSlot = 2;

        /// <summary>The rooted vtable callbacks.</summary>
        private readonly List<Delegate> _callbacks = [];

        /// <summary>The allocated vtable memory.</summary>
        private readonly IntPtr _vtable;

        /// <summary>Indicates whether the unmanaged allocations were released.</summary>
        private int _isDisposed;

        /// <summary>Initializes a new instance of the <see cref="ComVtable"/> class.</summary>
        /// <param name="slotCount">The number of vtable slots.</param>
        internal ComVtable(int slotCount)
        {
            _vtable = Marshal.AllocHGlobal(checked(IntPtr.Size * slotCount));
            Pointer = Marshal.AllocHGlobal(IntPtr.Size);
            for (int slot = FirstVtableSlot; slot < slotCount; slot++)
            {
                Marshal.WriteIntPtr(_vtable, checked(slot * IntPtr.Size), IntPtr.Zero);
            }

            Marshal.WriteIntPtr(Pointer, _vtable);
            Set(ReleaseVtableSlot, new ReleaseDelegate(static _ => 0U));
        }

        /// <summary>Finalizes an instance of the <see cref="ComVtable"/> class.</summary>
        ~ComVtable()
        {
            Dispose(disposing: false);
        }

        /// <summary>Gets the COM interface pointer.</summary>
        internal IntPtr Pointer { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Adds a callback at a vtable slot.</summary>
        /// <param name="slot">The zero-based vtable slot.</param>
        /// <param name="callback">The callback to root and expose.</param>
        internal void Set(int slot, Delegate callback)
        {
            _callbacks.Add(callback);
            Marshal.WriteIntPtr(_vtable, checked(slot * IntPtr.Size), Marshal.GetFunctionPointerForDelegate(callback));
        }

        /// <summary>Releases unmanaged resources owned by this instance.</summary>
        /// <param name="disposing">Whether managed resources should also be released.</param>
        private void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref _isDisposed, One) != 0)
            {
                return;
            }

            Marshal.FreeHGlobal(Pointer);
            Marshal.FreeHGlobal(_vtable);
            if (disposing)
            {
                _callbacks.Clear();
            }
        }
    }

    /// <summary>Provides an in-memory shell item vtable.</summary>
    private sealed class NativeShellItem : IDisposable
    {
        /// <summary>The IShellItem GetDisplayName vtable slot.</summary>
        private const int GetDisplayNameVtableSlot = 5;

        /// <summary>The IShellItem vtable size.</summary>
        private const int ShellItemVtableSlotCount = 6;

        /// <summary>The path returned by GetDisplayName.</summary>
        private readonly string _path;

        /// <summary>The COM object allocation.</summary>
        private readonly ComVtable _vtable = new(ShellItemVtableSlotCount);

        /// <summary>Initializes a new instance of the <see cref="NativeShellItem"/> class.</summary>
        /// <param name="path">The path returned by GetDisplayName.</param>
        internal NativeShellItem(string path)
        {
            _path = path;
            _vtable.Set(GetDisplayNameVtableSlot, new GetDisplayNameDelegate(GetDisplayName));
        }

        /// <summary>Gets the shell-item pointer.</summary>
        internal IntPtr Pointer => _vtable.Pointer;

        /// <inheritdoc />
        public void Dispose() => _vtable.Dispose();

        /// <summary>Returns a COM task-memory display name for the requested shape.</summary>
        /// <param name="_">The shell item pointer.</param>
        /// <param name="displayName">The requested display-name shape.</param>
        /// <param name="name">The allocated result string.</param>
        /// <returns>The HRESULT.</returns>
        private int GetDisplayName(IntPtr _, ShellItemDisplayName displayName, out IntPtr name)
        {
            name = Marshal.StringToCoTaskMemUni(_path);
            return Success;
        }
    }

    /// <summary>Provides an in-memory IShellItemArray vtable.</summary>
    private sealed class NativeShellItemArray : IDisposable
    {
        /// <summary>The IShellItemArray GetCount vtable slot.</summary>
        private const int GetCountVtableSlot = 7;

        /// <summary>The IShellItemArray GetItemAt vtable slot.</summary>
        private const int GetItemAtVtableSlot = 8;

        /// <summary>The IShellItemArray vtable size.</summary>
        private const int ShellItemArrayVtableSlotCount = 9;

        /// <summary>The shell item pointers returned by the array.</summary>
        private readonly IntPtr[] _items;

        /// <summary>The COM object allocation.</summary>
        private readonly ComVtable _vtable = new(ShellItemArrayVtableSlotCount);

        /// <summary>Initializes a new instance of the <see cref="NativeShellItemArray"/> class.</summary>
        /// <param name="items">The shell item pointers returned by the array.</param>
        internal NativeShellItemArray(IntPtr[] items)
        {
            _items = items;
            _vtable.Set(GetCountVtableSlot, new GetCountDelegate(GetCount));
            _vtable.Set(GetItemAtVtableSlot, new GetItemAtDelegate(GetItemAt));
        }

        /// <summary>Gets the shell item array pointer.</summary>
        internal IntPtr Pointer => _vtable.Pointer;

        /// <inheritdoc />
        public void Dispose() => _vtable.Dispose();

        /// <summary>Gets the number of item pointers.</summary>
        /// <param name="_">The array pointer.</param>
        /// <param name="count">The returned item count.</param>
        /// <returns>The HRESULT.</returns>
        private int GetCount(IntPtr _, out uint count)
        {
            count = checked((uint)_items.Length);
            return Success;
        }

        /// <summary>Gets one item pointer.</summary>
        /// <param name="_">The array pointer.</param>
        /// <param name="index">The requested item index.</param>
        /// <param name="item">The returned shell item pointer.</param>
        /// <returns>The HRESULT.</returns>
        private int GetItemAt(IntPtr _, uint index, out IntPtr item)
        {
            item = _items[checked((int)index)];
            return Success;
        }
    }

    /// <summary>Provides an in-memory IFileDialog/IFileOpenDialog vtable and call recording.</summary>
    private sealed class NativeDialog : IDisposable
    {
        /// <summary>The Show vtable slot.</summary>
        private const int ShowVtableSlot = 3;

        /// <summary>The SetFileTypes vtable slot.</summary>
        private const int SetFileTypesVtableSlot = 4;

        /// <summary>The SetFileTypeIndex vtable slot.</summary>
        private const int SetFileTypeIndexVtableSlot = 5;

        /// <summary>The SetOptions vtable slot.</summary>
        private const int SetOptionsVtableSlot = 9;

        /// <summary>The SetFolder vtable slot.</summary>
        private const int SetFolderVtableSlot = 12;

        /// <summary>The SetFileName vtable slot.</summary>
        private const int SetFileNameVtableSlot = 15;

        /// <summary>The SetTitle vtable slot.</summary>
        private const int SetTitleVtableSlot = 17;

        /// <summary>The GetResult vtable slot.</summary>
        private const int GetResultVtableSlot = 20;

        /// <summary>The AddPlace vtable slot.</summary>
        private const int AddPlaceVtableSlot = 21;

        /// <summary>The SetDefaultExtension vtable slot.</summary>
        private const int SetDefaultExtensionVtableSlot = 22;

        /// <summary>The GetResults vtable slot.</summary>
        private const int GetResultsVtableSlot = 27;

        /// <summary>The IFileOpenDialog vtable size.</summary>
        private const int FileOpenDialogVtableSlotCount = 28;

        /// <summary>The shell item returned from GetResult.</summary>
        private readonly IntPtr _resultItem;

        /// <summary>The shell item array returned from GetResults.</summary>
        private readonly IntPtr _results;

        /// <summary>The COM object allocation.</summary>
        private readonly ComVtable _vtable = new(FileOpenDialogVtableSlotCount);

        /// <summary>Initializes a new instance of the <see cref="NativeDialog"/> class.</summary>
        /// <param name="resultItem">The shell item returned from GetResult.</param>
        /// <param name="results">The shell item array returned from GetResults.</param>
        internal NativeDialog(IntPtr resultItem, IntPtr results)
        {
            _resultItem = resultItem;
            _results = results;
            _vtable.Set(ShowVtableSlot, new ShowDelegate(Show));
            _vtable.Set(SetFileTypesVtableSlot, new SetFileTypesDelegate(SetFileTypes));
            _vtable.Set(SetFileTypeIndexVtableSlot, new SetFileTypeIndexDelegate(SetFileTypeIndex));
            _vtable.Set(SetOptionsVtableSlot, new SetOptionsDelegate(SetOptions));
            _vtable.Set(SetFolderVtableSlot, new SetFolderDelegate(SetFolder));
            _vtable.Set(SetFileNameVtableSlot, new SetStringDelegate(SetFileName));
            _vtable.Set(SetTitleVtableSlot, new SetStringDelegate(SetTitle));
            _vtable.Set(GetResultVtableSlot, new GetResultDelegate(GetResult));
            _vtable.Set(AddPlaceVtableSlot, new AddPlaceDelegate(AddPlace));
            _vtable.Set(SetDefaultExtensionVtableSlot, new SetStringDelegate(SetDefaultExtension));
            _vtable.Set(GetResultsVtableSlot, new GetResultsDelegate(GetResults));
        }

        /// <summary>Gets or sets the dialog Show HRESULT.</summary>
        internal int ShowResult { get; set; }

        /// <summary>Gets or sets the operation HRESULT.</summary>
        internal int OperationResult { get; set; }

        /// <summary>Gets the in-memory dialog pointer.</summary>
        internal IntPtr Pointer => _vtable.Pointer;

        /// <summary>Gets the most recent owner handle.</summary>
        internal IntPtr LastOwner { get; private set; }

        /// <summary>Gets the transmitted native filter count.</summary>
        internal uint FilterCount { get; private set; }

        /// <summary>Gets the first filter name.</summary>
        internal string FirstFilterName { get; private set; }

        /// <summary>Gets the selected file type index.</summary>
        internal uint FileTypeIndex { get; private set; }

        /// <summary>Gets the options supplied by the wrapper.</summary>
        internal FileOpenOptions Options { get; private set; }

        /// <summary>Gets the folder pointer supplied by the wrapper.</summary>
        internal IntPtr Folder { get; private set; }

        /// <summary>Gets the place flags supplied by the wrapper.</summary>
        internal FileDialogAddPlaceFlags PlaceFlags { get; private set; }

        /// <summary>Gets the supplied file name.</summary>
        internal string FileName { get; private set; }

        /// <summary>Gets the supplied title.</summary>
        internal string Title { get; private set; }

        /// <summary>Gets the supplied default extension.</summary>
        internal string DefaultExtension { get; private set; }

        /// <inheritdoc />
        public void Dispose() => _vtable.Dispose();

        /// <summary>Records Show calls.</summary>
        /// <param name="_">The COM interface pointer.</param>
        /// <param name="owner">The owner window handle.</param>
        /// <returns>The HRESULT.</returns>
        private int Show(IntPtr _, IntPtr owner)
        {
            LastOwner = owner;
            return ShowResult;
        }

        /// <summary>Records file filter buffers.</summary>
        /// <param name="_">The COM interface pointer.</param>
        /// <param name="count">The number of filters.</param>
        /// <param name="filters">The native filter buffer.</param>
        /// <returns>The HRESULT.</returns>
        private int SetFileTypes(IntPtr _, uint count, IntPtr filters)
        {
            FilterCount = count;
            IntPtr name = Marshal.ReadIntPtr(filters);
            FirstFilterName = Marshal.PtrToStringUni(name);
            return OperationResult;
        }

        /// <summary>Records the selected filter index.</summary>
        /// <param name="_">The COM interface pointer.</param>
        /// <param name="index">The selected filter index.</param>
        /// <returns>The HRESULT.</returns>
        private int SetFileTypeIndex(IntPtr _, uint index)
        {
            FileTypeIndex = index;
            return OperationResult;
        }

        /// <summary>Records dialog options.</summary>
        /// <param name="_">The COM interface pointer.</param>
        /// <param name="options">The dialog options.</param>
        /// <returns>The HRESULT.</returns>
        private int SetOptions(IntPtr _, FileOpenOptions options)
        {
            Options = options;
            return OperationResult;
        }

        /// <summary>Records the selected folder.</summary>
        /// <param name="_">The COM interface pointer.</param>
        /// <param name="folder">The shell item pointer.</param>
        /// <returns>The HRESULT.</returns>
        private int SetFolder(IntPtr _, IntPtr folder)
        {
            Folder = folder;
            return OperationResult;
        }

        /// <summary>Records file names, titles, and extensions.</summary>
        /// <param name="_">The COM interface pointer.</param>
        /// <param name="value">The native string pointer.</param>
        /// <returns>The HRESULT.</returns>
        private int SetFileName(IntPtr _, IntPtr value)
        {
            FileName = Marshal.PtrToStringUni(value);
            return OperationResult;
        }

        /// <summary>Records dialog titles.</summary>
        /// <param name="_">The COM interface pointer.</param>
        /// <param name="value">The native string pointer.</param>
        /// <returns>The HRESULT.</returns>
        private int SetTitle(IntPtr _, IntPtr value)
        {
            Title = Marshal.PtrToStringUni(value);
            return OperationResult;
        }

        /// <summary>Returns the deterministic selected item.</summary>
        /// <param name="_">The COM interface pointer.</param>
        /// <param name="item">The returned shell item pointer.</param>
        /// <returns>The HRESULT.</returns>
        private int GetResult(IntPtr _, out IntPtr item)
        {
            item = _resultItem;
            return OperationResult;
        }

        /// <summary>Records navigation place flags.</summary>
        /// <param name="_">The COM interface pointer.</param>
        /// <param name="place">The shell item pointer.</param>
        /// <param name="flags">The placement flags.</param>
        /// <returns>The HRESULT.</returns>
        private int AddPlace(IntPtr _, IntPtr place, FileDialogAddPlaceFlags flags)
        {
            Folder = place;
            PlaceFlags = flags;
            return OperationResult;
        }

        /// <summary>Records default extensions.</summary>
        /// <param name="_">The COM interface pointer.</param>
        /// <param name="value">The native string pointer.</param>
        /// <returns>The HRESULT.</returns>
        private int SetDefaultExtension(IntPtr _, IntPtr value)
        {
            DefaultExtension = Marshal.PtrToStringUni(value);
            return OperationResult;
        }

        /// <summary>Returns the deterministic selected item array.</summary>
        /// <param name="_">The COM interface pointer.</param>
        /// <param name="selected">The returned shell item array pointer.</param>
        /// <returns>The HRESULT.</returns>
        private int GetResults(IntPtr _, out IntPtr selected)
        {
            selected = _results;
            return OperationResult;
        }
    }
}
