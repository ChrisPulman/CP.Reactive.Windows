// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Deterministic coverage for file-dialog builders without opening native UI.</summary>
public sealed class FileDialogCoverage2Tests
{
    /// <summary>The text-file filter pattern.</summary>
    private const string TextFilePattern = "*.txt";

    /// <summary>The test owner window handle.</summary>
    private static readonly IntPtr OwnerHandle = new(0x1234);

    /// <summary>Tests open-file builder request mapping and multi-selection result propagation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task FileOpenBuilderMapsRequestAndReturnsExecutorResultAsync()
    {
        var expected = FileDialogResult.FromPaths(["c:\\one.txt", "c:\\two.txt"]);
        var executor = new CapturingFileDialogExecutor { OpenResult = expected };
        var result = new FileOpenDialogBuilder()
            .WithTitle("Open title")
            .WithInitialDirectory("c:\\source")
            .WithDefaultExtension("txt")
            .AddFilter("Text", TextFilePattern)
            .AddFilter("All", "*.*")
            .AddPlace("c:\\places\\top", atTop: true)
            .AddPlace("c:\\places\\bottom")
            .AllowMultipleSelection()
            .ShowDialog(OwnerHandle, executor);

        await Assert.That(result).IsSameReferenceAs(expected);
        await Assert.That(executor.OpenRequest).IsNotNull();
        await Assert.That(executor.OpenRequest.OwnerHandle).IsEqualTo(OwnerHandle.ToInt64());
        await Assert.That(executor.OpenRequest.Title).IsEqualTo("Open title");
        await Assert.That(executor.OpenRequest.InitialDirectory).IsEqualTo("c:\\source");
        await Assert.That(executor.OpenRequest.DefaultExtension).IsEqualTo("txt");
        await Assert.That(executor.OpenRequest.AllowMultiSelect).IsTrue();
        await Assert.That(executor.OpenRequest.Filters.Count).IsEqualTo(Two);
        await Assert.That(executor.OpenRequest.Filters[0].Name).IsEqualTo("Text");
        await Assert.That(executor.OpenRequest.Filters[1].Pattern).IsEqualTo("*.*");
        await Assert.That(executor.OpenRequest.Places.Count).IsEqualTo(Two);
        await Assert.That(executor.OpenRequest.Places[0].AtTop).IsTrue();
        await Assert.That(executor.OpenRequest.Places[1].AtTop).IsFalse();
    }

    /// <summary>Tests save-file builder request mapping and cancellation propagation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task FileSaveBuilderMapsRequestAndReturnsCancellationAsync()
    {
        var expected = FileDialogResult.Cancelled();
        var executor = new CapturingFileDialogExecutor { SaveResult = expected };
        var result = new FileSaveDialogBuilder()
            .WithTitle("Save title")
            .WithInitialDirectory("c:\\target")
            .WithSuggestedFileName("document.txt")
            .WithDefaultExtension("txt")
            .AddFilter("Text", TextFilePattern)
            .AddPlace("c:\\places", atTop: true)
            .ShowDialog(OwnerHandle, executor);

        await Assert.That(result).IsSameReferenceAs(expected);
        await Assert.That(result.WasCancelled).IsTrue();
        await Assert.That(executor.SaveRequest).IsNotNull();
        await Assert.That(executor.SaveRequest.OwnerHandle).IsEqualTo(OwnerHandle.ToInt64());
        await Assert.That(executor.SaveRequest.Title).IsEqualTo("Save title");
        await Assert.That(executor.SaveRequest.InitialDirectory).IsEqualTo("c:\\target");
        await Assert.That(executor.SaveRequest.SuggestedFileName).IsEqualTo("document.txt");
        await Assert.That(executor.SaveRequest.DefaultExtension).IsEqualTo("txt");
        await Assert.That(executor.SaveRequest.Filters[0].Pattern).IsEqualTo(TextFilePattern);
        await Assert.That(executor.SaveRequest.Places[0].Path).IsEqualTo("c:\\places");
    }

    /// <summary>Tests folder-picker builder request mapping.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task FolderPickerBuilderMapsRequestAsync()
    {
        var expected = FileDialogResult.FromPath("c:\\folder");
        var executor = new CapturingFileDialogExecutor { FolderResult = expected };
        var result = new FolderPickerBuilder()
            .WithTitle("Folder title")
            .WithInitialDirectory("c:\\initial")
            .ShowDialog(OwnerHandle, executor);

        await Assert.That(result).IsSameReferenceAs(expected);
        await Assert.That(result.SelectedPath).IsEqualTo("c:\\folder");
        await Assert.That(executor.FolderRequest).IsNotNull();
        await Assert.That(executor.FolderRequest.OwnerHandle).IsEqualTo(OwnerHandle.ToInt64());
        await Assert.That(executor.FolderRequest.Title).IsEqualTo("Folder title");
        await Assert.That(executor.FolderRequest.InitialDirectory).IsEqualTo("c:\\initial");
    }

    /// <summary>Captures dialog requests and returns configured results.</summary>
    private sealed class CapturingFileDialogExecutor : IFileDialogExecutor
    {
        /// <summary>Gets or sets the open dialog result.</summary>
        public FileDialogResult OpenResult { get; set; } = FileDialogResult.Cancelled();

        /// <summary>Gets or sets the save dialog result.</summary>
        public FileDialogResult SaveResult { get; set; } = FileDialogResult.Cancelled();

        /// <summary>Gets or sets the folder picker result.</summary>
        public FileDialogResult FolderResult { get; set; } = FileDialogResult.Cancelled();

        /// <summary>Gets the captured open request.</summary>
        public FileOpenDialogRequest OpenRequest { get; private set; }

        /// <summary>Gets the captured save request.</summary>
        public FileSaveDialogRequest SaveRequest { get; private set; }

        /// <summary>Gets the captured folder request.</summary>
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
}
