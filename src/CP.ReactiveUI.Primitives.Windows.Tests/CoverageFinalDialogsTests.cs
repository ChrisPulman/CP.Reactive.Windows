// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests dialog APIs without displaying native user interface.</summary>
public sealed class CoverageFinalDialogsTests
{
    /// <summary>Defines the first selected path.</summary>
    private const string FirstPath = "c:\\dialog\\one.txt";

    /// <summary>Defines the second selected path.</summary>
    private const string SecondPath = "c:\\dialog\\two.txt";

    /// <summary>Defines the saved path.</summary>
    private const string SavePath = "c:\\dialog\\save.txt";

    /// <summary>Defines the selected folder path.</summary>
    private const string FolderPath = "c:\\dialog\\folder";

    /// <summary>Verifies public convenience methods delegate to the configured executor.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task PublicDialogApisUseConfiguredExecutorAsync()
    {
        var executor = new CapturingFileDialogExecutor
        {
            FolderResult = FileDialogResult.FromPath(FolderPath),
            OpenResult = FileDialogResult.FromPaths([FirstPath, SecondPath]),
            SaveResult = FileDialogResult.FromPath(SavePath),
        };
        DialogFileDialog.DialogExecutor = executor;

        try
        {
            await Assert.That(DialogFileDialog.PickFileToOpen()).IsEqualTo(FirstPath);
            await Assert.That(DialogFileDialog.PickFilesToOpen()).IsEquivalentTo([FirstPath, SecondPath]);
            await Assert.That(DialogFileDialog.PickFileToSave()).IsEqualTo(SavePath);
            await Assert.That(DialogFileDialog.PickFolder()).IsEqualTo(FolderPath);
            await Assert.That(new FileOpenDialogBuilder().ShowDialog()).IsSameReferenceAs(executor.OpenResult);
            await Assert.That(new FileSaveDialogBuilder().ShowDialog()).IsSameReferenceAs(executor.SaveResult);
            await Assert.That(new FolderPickerBuilder().ShowDialog()).IsSameReferenceAs(executor.FolderResult);
        }
        finally
        {
            DialogFileDialog.RestoreExecutorForTesting();
        }
    }

    /// <summary>Verifies the native executor maps successful and cancelled dialogs to results.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeExecutorMapsDialogResultsAsync()
    {
        var openDialog = new FakeOpenDialog();
        var saveDialog = new FakeSaveDialog();
        var executor = new NativeFileDialogExecutor(() => openDialog, () => saveDialog);
        var openResult = executor.ShowOpen(new(IntPtr.Zero.ToInt64(), null, null, null, [], [], true));
        var saveResult = executor.ShowSave(new(IntPtr.Zero.ToInt64(), null, null, null, null, [], []));
        var folderResult = executor.ShowFolder(new(IntPtr.Zero.ToInt64(), null, null));

        openDialog.ShowResultCode = ComDialogHelper.HResultCancelled;
        saveDialog.ShowResultCode = ComDialogHelper.HResultCancelled;
        var cancelledOpen = executor.ShowOpen(new(IntPtr.Zero.ToInt64(), null, null, null, [], [], false));
        var cancelledSave = executor.ShowSave(new(IntPtr.Zero.ToInt64(), null, null, null, null, [], []));

        await Assert.That(openResult.SelectedPaths).IsEquivalentTo([FirstPath, SecondPath]);
        await Assert.That(saveResult.SelectedPath).IsEqualTo(SavePath);
        await Assert.That(folderResult.SelectedPath).IsEqualTo(FirstPath);
        await Assert.That(cancelledOpen.WasCancelled).IsTrue();
        await Assert.That(cancelledSave.WasCancelled).IsTrue();
    }

    /// <summary>Captures requests and supplies deterministic dialog results.</summary>
    private sealed class CapturingFileDialogExecutor : IFileDialogExecutor
    {
        /// <summary>Gets or sets the open result.</summary>
        public FileDialogResult OpenResult { get; set; } = FileDialogResult.Cancelled();

        /// <summary>Gets or sets the save result.</summary>
        public FileDialogResult SaveResult { get; set; } = FileDialogResult.Cancelled();

        /// <summary>Gets or sets the folder result.</summary>
        public FileDialogResult FolderResult { get; set; } = FileDialogResult.Cancelled();

        /// <inheritdoc/>
        public FileDialogResult ShowOpen(FileOpenDialogRequest request)
        {
            _ = request;
            return OpenResult;
        }

        /// <inheritdoc/>
        public FileDialogResult ShowSave(FileSaveDialogRequest request)
        {
            _ = request;
            return SaveResult;
        }

        /// <inheritdoc/>
        public FileDialogResult ShowFolder(FolderPickerDialogRequest request)
        {
            _ = request;
            return FolderResult;
        }
    }

    /// <summary>Provides a deterministic open dialog implementation.</summary>
    private sealed class FakeOpenDialog : IFileOpenDialog
    {
        /// <summary>Initializes a new instance of the <see cref="FakeOpenDialog"/> class.</summary>
        public FakeOpenDialog()
            : base(IntPtr.Zero)
        {
        }

        /// <summary>Gets or sets the HRESULT returned from <see cref="Show"/>.</summary>
        public int ShowResultCode { get; set; }

        /// <inheritdoc/>
        internal override int Show(IntPtr ownerHandle)
        {
            _ = ownerHandle;
            return ShowResultCode;
        }

        /// <inheritdoc/>
        internal override void SetFileTypes(FilterSpec[] filterSpecs) => _ = filterSpecs;

        /// <inheritdoc/>
        internal override void SetFileTypeIndex(uint fileTypeIndex) => _ = fileTypeIndex;

        /// <inheritdoc/>
        internal override void SetOptions(FileOpenOptions options) => _ = options;

        /// <inheritdoc/>
        internal override void SetTitle(string title) => _ = title;

        /// <inheritdoc/>
        internal override void SetDefaultExtension(string defaultExtension) => _ = defaultExtension;

        /// <inheritdoc/>
        internal override IShellItem GetResult() => new FakeShellItem(FirstPath);

        /// <inheritdoc/>
        internal override IShellItemArray GetResults() => new FakeShellItemArray([FirstPath, SecondPath]);
    }

    /// <summary>Provides a deterministic save dialog implementation.</summary>
    private sealed class FakeSaveDialog : IFileSaveDialog
    {
        /// <summary>Initializes a new instance of the <see cref="FakeSaveDialog"/> class.</summary>
        public FakeSaveDialog()
            : base(IntPtr.Zero)
        {
        }

        /// <summary>Gets or sets the HRESULT returned from <see cref="Show"/>.</summary>
        public int ShowResultCode { get; set; }

        /// <inheritdoc/>
        internal override int Show(IntPtr ownerHandle)
        {
            _ = ownerHandle;
            return ShowResultCode;
        }

        /// <inheritdoc/>
        internal override void SetFileTypes(FilterSpec[] filterSpecs) => _ = filterSpecs;

        /// <inheritdoc/>
        internal override void SetFileTypeIndex(uint fileTypeIndex) => _ = fileTypeIndex;

        /// <inheritdoc/>
        internal override void SetOptions(FileOpenOptions options) => _ = options;

        /// <inheritdoc/>
        internal override void SetTitle(string title) => _ = title;

        /// <inheritdoc/>
        internal override void SetFileName(string name) => _ = name;

        /// <inheritdoc/>
        internal override void SetDefaultExtension(string defaultExtension) => _ = defaultExtension;

        /// <inheritdoc/>
        internal override IShellItem GetResult() => new FakeShellItem(SavePath);
    }

    /// <summary>Provides a deterministic shell item implementation.</summary>
    /// <param name="path">The filesystem path returned by the item.</param>
    private sealed class FakeShellItem(string path) : IShellItem(IntPtr.Zero)
    {
        /// <inheritdoc/>
        internal override string GetDisplayName(ShellItemDisplayName displayName)
        {
            _ = displayName;
            return path;
        }
    }

    /// <summary>Provides a deterministic shell item array implementation.</summary>
    /// <param name="paths">The filesystem paths returned by the array.</param>
    private sealed class FakeShellItemArray(IReadOnlyList<string> paths) : IShellItemArray(IntPtr.Zero)
    {
        /// <inheritdoc/>
        internal override uint GetCount() => (uint)paths.Count;

        /// <inheritdoc/>
        internal override IShellItem GetItemAt(uint index) => new FakeShellItem(paths[(int)index]);
    }
}
