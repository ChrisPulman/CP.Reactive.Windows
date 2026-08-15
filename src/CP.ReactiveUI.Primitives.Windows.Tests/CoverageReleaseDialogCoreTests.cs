// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Deterministic coverage for the public dialog shortcuts and their COM helper composition seams.</summary>
public sealed class CoverageReleaseDialogCoreTests
{
    /// <summary>The first synthetic input path.</summary>
    private const string InputPath = "c:\\input.txt";

    /// <summary>The synthetic initial source directory.</summary>
    private const string SourceDirectory = "c:\\source";

    /// <summary>The synthetic folder path.</summary>
    private const string FolderPath = "c:\\folder";

    /// <summary>The synthetic shell-item path.</summary>
    private const string ShellItemPath = "c:\\shell-item";

    /// <summary>Verifies every public convenience overload maps its configured values to the executor.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task FileDialogConvenienceOverloadsMapConfiguredRequestsAsync()
    {
        var executor = new CapturingExecutor
        {
            OpenResult = FileDialogResult.FromPaths([InputPath, "c:\\second.txt"]),
            SaveResult = FileDialogResult.FromPath("c:\\output.txt"),
            FolderResult = FileDialogResult.FromPath(FolderPath),
        };
        DialogFileDialog.DialogExecutor = executor;

        try
        {
            var filters = new[] { ("Text", "*.txt"), ("All", "*.*") };
            var owner = new IntPtr(FortyTwo);

            await Assert.That(DialogFileDialog.PickFileToOpen(owner, "Open", SourceDirectory, filters, "txt")).IsEqualTo(InputPath);
            await Assert.That(executor.OpenRequest.Title).IsEqualTo("Open");
            await Assert.That(executor.OpenRequest.InitialDirectory).IsEqualTo(SourceDirectory);
            await Assert.That(executor.OpenRequest.DefaultExtension).IsEqualTo("txt");
            await Assert.That(executor.OpenRequest.Filters).IsEquivalentTo(filters);
            await Assert.That(executor.OpenRequest.AllowMultiSelect).IsFalse();

            await Assert.That(DialogFileDialog.PickFilesToOpen(owner, "Open many", SourceDirectory, filters)).IsEquivalentTo([InputPath, "c:\\second.txt"]);
            await Assert.That(executor.OpenRequest.OwnerHandle).IsEqualTo(owner.ToInt64());
            await Assert.That(executor.OpenRequest.Title).IsEqualTo("Open many");
            await Assert.That(executor.OpenRequest.DefaultExtension).IsNull();
            await Assert.That(executor.OpenRequest.AllowMultiSelect).IsTrue();

            await Assert.That(DialogFileDialog.PickFileToSave(owner, "Save", "c:\\target", "output.txt", filters, "txt")).IsEqualTo("c:\\output.txt");
            await Assert.That(executor.SaveRequest.OwnerHandle).IsEqualTo(owner.ToInt64());
            await Assert.That(executor.SaveRequest.Title).IsEqualTo("Save");
            await Assert.That(executor.SaveRequest.InitialDirectory).IsEqualTo("c:\\target");
            await Assert.That(executor.SaveRequest.SuggestedFileName).IsEqualTo("output.txt");
            await Assert.That(executor.SaveRequest.DefaultExtension).IsEqualTo("txt");
            await Assert.That(executor.SaveRequest.Filters).IsEquivalentTo(filters);

            await Assert.That(DialogFileDialog.PickFolder(owner, "Folder", FolderPath)).IsEqualTo(FolderPath);
            await Assert.That(executor.FolderRequest.OwnerHandle).IsEqualTo(owner.ToInt64());
            await Assert.That(executor.FolderRequest.Title).IsEqualTo("Folder");
            await Assert.That(executor.FolderRequest.InitialDirectory).IsEqualTo(FolderPath);
        }
        finally
        {
            DialogFileDialog.RestoreExecutorForTesting();
        }
    }

    /// <summary>Verifies cancellation consistently maps to null or an empty selection.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task FileDialogConvenienceOverloadsMapCancellationAsync()
    {
        DialogFileDialog.DialogExecutor = new CapturingExecutor();

        try
        {
            await Assert.That(DialogFileDialog.PickFileToOpen()).IsNull();
            await Assert.That(DialogFileDialog.PickFilesToOpen()).IsEmpty();
            await Assert.That(DialogFileDialog.PickFileToSave()).IsNull();
            await Assert.That(DialogFileDialog.PickFolder()).IsNull();
        }
        finally
        {
            DialogFileDialog.RestoreExecutorForTesting();
        }
    }

    /// <summary>Verifies COM factories and shell item helpers use injected, non-native operations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ComDialogHelperUsesInjectedFactoriesAndCollectsPathsAsync()
    {
        Guid observedDialogClass = Guid.Empty;
        Guid observedDialogInterface = Guid.Empty;
        Guid observedShellInterface = Guid.Empty;
        ComDialogHelper.CreateDialogInstance = (classId, interfaceId) =>
        {
            observedDialogClass = classId;
            observedDialogInterface = interfaceId;
            return (0, IntPtr.Zero);
        };
        ComDialogHelper.CreateShellItemInstance = (_, interfaceId) =>
        {
            observedShellInterface = interfaceId;
            return (0, IntPtr.Zero);
        };
        ComDialogHelper.WrapShellItem = static _ => new TestShellItem(ShellItemPath);

        try
        {
            using var open = ComDialogHelper.CreateDialog<IFileOpenDialog>(ComDialogHelper.ClsidFileOpenDialog);
            using var save = ComDialogHelper.CreateDialog<IFileSaveDialog>(ComDialogHelper.ClsidFileSaveDialog);
            using var item = ComDialogHelper.ShellItemFromPath(ShellItemPath);
            IReadOnlyList<string> paths = ComDialogHelper.CollectPaths(new TestShellItemArray(["c:\\one", "c:\\two"]));

            await Assert.That(open.Handle).IsEqualTo(IntPtr.Zero);
            await Assert.That(save.Handle).IsEqualTo(IntPtr.Zero);
            await Assert.That(observedDialogClass).IsEqualTo(ComDialogHelper.ClsidFileSaveDialog);
            await Assert.That(observedDialogInterface).IsEqualTo(IFileSaveDialog.InterfaceId);
            await Assert.That(observedShellInterface).IsNotEqualTo(Guid.Empty);
            await Assert.That(ComDialogHelper.GetFileSysPath(item)).IsEqualTo(ShellItemPath);
            await Assert.That(paths).IsEquivalentTo(["c:\\one", "c:\\two"]);

            ComDialogHelper.RestoreNativeFactoriesForTesting();
            ComDialogHelper.CreateShellItemInstance = static (_, _) => (0, IntPtr.Zero);
            using IShellItem coreWrappedItem = ComDialogHelper.ShellItemFromPath("c:\\core-wrapper");
            await Assert.That(coreWrappedItem.Handle).IsEqualTo(IntPtr.Zero);
        }
        finally
        {
            ComDialogHelper.RestoreNativeFactoriesForTesting();
        }
    }

    /// <summary>Verifies HRESULT failures and invalid wrappers do not invoke native dialog UI.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ComDialogHelperMapsInjectedComFailuresAsync()
    {
        ComDialogHelper.CreateDialogInstance = static (_, _) => (unchecked((int)0x80004005), IntPtr.Zero);
        ComDialogHelper.CreateShellItemInstance = static (_, _) => (unchecked((int)0x80004005), IntPtr.Zero);

        try
        {
            await Assert.That(static () => ComDialogHelper.CreateDialog<IFileOpenDialog>(ComDialogHelper.ClsidFileOpenDialog))
                .Throws<COMException>();
            await Assert.That(static () => ComDialogHelper.ShellItemFromPath("c:\\missing"))
                .Throws<COMException>();
        }
        finally
        {
            ComDialogHelper.RestoreNativeFactoriesForTesting();
        }
    }

    /// <summary>Verifies the production factory composition reaches low-level injected operations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ComDialogHelperCoreFactoriesUseInjectedNativeOperationsAsync()
    {
        var dialogCalls = 0;
        var shellCalls = 0;
        ComDialogHelper.RestoreNativeFactoriesForTesting();
        ComDialogHelper.NativeDialogActivation = (_, _) =>
        {
            dialogCalls++;
            return (0, IntPtr.Zero);
        };
        ComDialogHelper.NativeShellItemCreation = (_, _) =>
        {
            shellCalls++;
            return (0, IntPtr.Zero);
        };

        try
        {
            using var dialog = ComDialogHelper.CreateDialog<IFileOpenDialog>(ComDialogHelper.ClsidFileOpenDialog);
            using IShellItem item = ComDialogHelper.ShellItemFromPath("c:\\composed-item");

            await Assert.That(dialogCalls).IsEqualTo(1);
            await Assert.That(shellCalls).IsEqualTo(1);
            await Assert.That(dialog.Handle).IsEqualTo(IntPtr.Zero);
            await Assert.That(item.Handle).IsEqualTo(IntPtr.Zero);
        }
        finally
        {
            ComDialogHelper.RestoreNativeFactoriesForTesting();
        }
    }

    /// <summary>Verifies existing folders and COM failures are handled by the reusable directory helpers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ComDialogHelperAppliesExistingFoldersAndPlacesAsync()
    {
        var setFolderCalls = 0;
        var addPlaceCalls = 0;
        var flags = new List<FileDialogAddPlaceFlags>();
        var path = Path.GetTempPath();
        ComDialogHelper.CreateShellItemInstance = static (_, _) => (0, IntPtr.Zero);
        ComDialogHelper.WrapShellItem = _ => new TestShellItem(path);

        try
        {
            ComDialogHelper.ApplyInitialDirectory(_ => setFolderCalls++, path);
            ComDialogHelper.ApplyPlaces(
                (_, flag) =>
                {
                    addPlaceCalls++;
                    flags.Add(flag);
                },
                [(path, true), (path, false)]);

            ComDialogHelper.CreateShellItemInstance = static (_, _) => (unchecked((int)0x80004005), IntPtr.Zero);
            ComDialogHelper.ApplyInitialDirectory(_ => setFolderCalls++, path);
            ComDialogHelper.ApplyPlaces((_, _) => addPlaceCalls++, [(path, true)]);

            await Assert.That(setFolderCalls).IsEqualTo(1);
            await Assert.That(addPlaceCalls).IsEqualTo(Two);
            await Assert.That(flags).IsEquivalentTo([FileDialogAddPlaceFlags.Top, FileDialogAddPlaceFlags.Bottom]);
        }
        finally
        {
            ComDialogHelper.RestoreNativeFactoriesForTesting();
        }
    }

    /// <summary>Captures requests from the public shortcut APIs.</summary>
    private sealed class CapturingExecutor : IFileDialogExecutor
    {
        /// <summary>Gets or sets the open result.</summary>
        public FileDialogResult OpenResult { get; set; } = FileDialogResult.Cancelled();

        /// <summary>Gets or sets the save result.</summary>
        public FileDialogResult SaveResult { get; set; } = FileDialogResult.Cancelled();

        /// <summary>Gets or sets the folder result.</summary>
        public FileDialogResult FolderResult { get; set; } = FileDialogResult.Cancelled();

        /// <summary>Gets the last open request.</summary>
        public FileOpenDialogRequest OpenRequest { get; private set; }

        /// <summary>Gets the last save request.</summary>
        public FileSaveDialogRequest SaveRequest { get; private set; }

        /// <summary>Gets the last folder request.</summary>
        public FolderPickerDialogRequest FolderRequest { get; private set; }

        /// <inheritdoc />
        public FileDialogResult ShowOpen(FileOpenDialogRequest request)
        {
            OpenRequest = request;
            return OpenResult;
        }

        /// <inheritdoc />
        public FileDialogResult ShowSave(FileSaveDialogRequest request)
        {
            SaveRequest = request;
            return SaveResult;
        }

        /// <inheritdoc />
        public FileDialogResult ShowFolder(FolderPickerDialogRequest request)
        {
            FolderRequest = request;
            return FolderResult;
        }
    }

    /// <summary>Provides deterministic file-system paths without a COM vtable.</summary>
    /// <param name="path">The path returned by the item.</param>
    private sealed class TestShellItem(string path) : IShellItem(IntPtr.Zero)
    {
        /// <inheritdoc />
        internal override string GetDisplayName(ShellItemDisplayName displayName)
        {
            _ = displayName;
            return path;
        }
    }

    /// <summary>Provides deterministic shell items without a COM vtable.</summary>
    /// <param name="paths">The paths returned by the array.</param>
    private sealed class TestShellItemArray(IReadOnlyList<string> paths) : IShellItemArray(IntPtr.Zero)
    {
        /// <inheritdoc />
        internal override uint GetCount() => (uint)paths.Count;

        /// <inheritdoc />
        internal override IShellItem GetItemAt(uint index) => new TestShellItem(paths[(int)index]);
    }
}
