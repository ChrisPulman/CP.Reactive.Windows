// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Additional deterministic coverage for dialog value objects and helpers.</summary>
public sealed class CoverageWave3DialogsTests
{
    /// <summary>The zero test value.</summary>
    private const int Zero = 0;

    /// <summary>The selected text file path.</summary>
    private const string SelectedTextPath = "c:\\selected.txt";

    /// <summary>The text filter display name.</summary>
    private const string TextFilterName = "Text";

    /// <summary>The image filter display name.</summary>
    private const string ImageFilterName = "Image";

    /// <summary>The all-files filter display name.</summary>
    private const string AllFilterName = "All";

    /// <summary>The all-files filter pattern.</summary>
    private const string AllFilePattern = "*.*";

    /// <summary>The first selected text file path.</summary>
    private const string FirstTextPath = "c:\\one.txt";

    /// <summary>The second selected text file path.</summary>
    private const string SecondTextPath = "c:\\two.txt";

    /// <summary>The text-file filter pattern.</summary>
    private const string TextFilePattern = "*.txt";

    /// <summary>The image-file filter pattern.</summary>
    private const string ImageFilePattern = "*.png";

    /// <summary>The dialog open title.</summary>
    private const string OpenTitle = "Open";

    /// <summary>The dialog save title.</summary>
    private const string SaveTitle = "Save";

    /// <summary>The dialog folder title.</summary>
    private const string FolderTitle = "Folder";

    /// <summary>The input directory path.</summary>
    private const string InputDirectory = "c:\\in";

    /// <summary>The output directory path.</summary>
    private const string OutputDirectory = "c:\\out";

    /// <summary>The folder directory path.</summary>
    private const string FolderDirectory = "c:\\folder";

    /// <summary>The places directory path.</summary>
    private const string PlacesDirectory = "c:\\places";

    /// <summary>The suggested save file name.</summary>
    private const string SuggestedFileName = "name.txt";

    /// <summary>The default extension.</summary>
    private const string TextExtension = "txt";

    /// <summary>The file-must-exist flag value.</summary>
    private const uint FileMustExistFlag = 0x00001000U;

    /// <summary>The combined file-must-exist and multi-select flag value.</summary>
    private const uint MultiSelectMustExistFlag = 0x00001200U;

    /// <summary>The shell file-system path display-name flag value.</summary>
    private const uint FileSystemPathFlag = 0x80058000U;

    /// <summary>The zero unsigned integer value.</summary>
    private const uint UIntZero = 0U;

    /// <summary>The one unsigned integer value.</summary>
    private const uint UIntOne = 1U;

    /// <summary>Verifies result factories expose consistent cancellation and selection state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task FileDialogResultFactoriesExposeExpectedStateAsync()
    {
        var cancelled = FileDialogResult.Cancelled();
        var single = FileDialogResult.FromPath(SelectedTextPath);
        var multiple = FileDialogResult.FromPaths([FirstTextPath, SecondTextPath]);
        var emptyMultiple = FileDialogResult.FromPaths([]);

        await Assert.That(cancelled.WasCancelled).IsTrue();
        await Assert.That(cancelled.SelectedPath).IsNull();
        await Assert.That(cancelled.SelectedPaths).IsNull();
        await Assert.That(single.WasCancelled).IsFalse();
        await Assert.That(single.SelectedPath).IsEqualTo(SelectedTextPath);
        await Assert.That(single.SelectedPaths).IsNull();
        await Assert.That(multiple.SelectedPath).IsEqualTo(FirstTextPath);
        await Assert.That(multiple.SelectedPaths).IsEquivalentTo([FirstTextPath, SecondTextPath]);
        await Assert.That(emptyMultiple.WasCancelled).IsFalse();
        await Assert.That(emptyMultiple.SelectedPath).IsNull();
        await Assert.That(emptyMultiple.SelectedPaths).IsEmpty();
    }

    /// <summary>Verifies request records retain value semantics for deterministic executor tests.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DialogRequestsSupportValueEqualityAsync()
    {
        var filters = new[] { (TextFilterName, TextFilePattern) };
        var places = new[] { (PlacesDirectory, true) };
        var open = new FileOpenDialogRequest(One, OpenTitle, InputDirectory, TextExtension, filters, places, true);
        var sameOpen = open with { };
        var changedOpen = open with { AllowMultiSelect = false };
        var save = new FileSaveDialogRequest(Two, SaveTitle, OutputDirectory, SuggestedFileName, TextExtension, filters, places);
        var sameSave = save with { };
        var folder = new FolderPickerDialogRequest(Three, FolderTitle, FolderDirectory);
        var sameFolder = folder with { };

        await Assert.That(open).IsEqualTo(sameOpen);
        await Assert.That(open).IsNotEqualTo(changedOpen);
        await Assert.That(open.OwnerHandle).IsEqualTo(One);
        await Assert.That(save).IsEqualTo(sameSave);
        await Assert.That(save.SuggestedFileName).IsEqualTo(SuggestedFileName);
        await Assert.That(folder).IsEqualTo(sameFolder);
        await Assert.That(folder.InitialDirectory).IsEqualTo(FolderDirectory);
    }

    /// <summary>Verifies filter application skips empty input and selects the first real filter.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ComDialogHelperApplyFiltersSkipsEmptyInputAndSelectsFirstFilterAsync()
    {
        var setFileTypesCalls = Zero;
        var setFileTypeIndexCalls = Zero;
        FilterSpec[] capturedFilters = [];
        uint selectedIndex = Zero;

        ComDialogHelper.ApplyFilters(
            filters =>
            {
                setFileTypesCalls++;
                capturedFilters = filters;
            },
            index =>
            {
                setFileTypeIndexCalls++;
                selectedIndex = index;
            },
            null);
        ComDialogHelper.ApplyFilters(
            _ => setFileTypesCalls++,
            _ => setFileTypeIndexCalls++,
            []);
        ComDialogHelper.ApplyFilters(
            filters =>
            {
                setFileTypesCalls++;
                capturedFilters = filters;
            },
            index =>
            {
                setFileTypeIndexCalls++;
                selectedIndex = index;
            },
            [(TextFilterName, TextFilePattern), (AllFilterName, AllFilePattern)]);

        await Assert.That(setFileTypesCalls).IsEqualTo(One);
        await Assert.That(setFileTypeIndexCalls).IsEqualTo(One);
        await Assert.That(selectedIndex).IsEqualTo(UIntOne);
        await Assert.That(capturedFilters[0]).IsEqualTo(new(TextFilterName, TextFilePattern));
        await Assert.That(capturedFilters[1].Name).IsEqualTo(AllFilterName);
        await Assert.That(capturedFilters[1].Spec).IsEqualTo(AllFilePattern);
    }

    /// <summary>Verifies missing initial directories and places are ignored before native shell calls.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ComDialogHelperSkipsMissingInitialDirectoriesAndPlacesAsync()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var setFolderCalls = Zero;
        var addPlaceCalls = Zero;

        ComDialogHelper.ApplyInitialDirectory(_ => setFolderCalls++, null);
        ComDialogHelper.ApplyInitialDirectory(_ => setFolderCalls++, string.Empty);
        ComDialogHelper.ApplyInitialDirectory(_ => setFolderCalls++, missingPath);
        ComDialogHelper.ApplyPlaces((_, _) => addPlaceCalls++, null);
        ComDialogHelper.ApplyPlaces((_, _) => addPlaceCalls++, []);
        ComDialogHelper.ApplyPlaces((_, _) => addPlaceCalls++, [(missingPath, true), (string.Empty, false)]);

        await Assert.That(setFolderCalls).IsEqualTo(Zero);
        await Assert.That(addPlaceCalls).IsEqualTo(Zero);
    }

    /// <summary>Verifies unsupported COM wrappers are rejected before native activation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ComDialogHelperRejectsUnsupportedComWrapperBeforeNativeActivationAsync() =>
        await Assert.That(static () => ComDialogHelper.CreateDialog<UnsupportedComObject>(Guid.Empty))
            .Throws<PlatformNotSupportedException>();

    /// <summary>Verifies dialog interop value types and native filter storage expose expected values.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DialogInteropValuesExposeExpectedSemanticsAsync()
    {
        var text = new FilterSpec(TextFilterName, TextFilePattern);
        var sameText = new FilterSpec(TextFilterName, TextFilePattern);
        var image = new FilterSpec(ImageFilterName, ImageFilePattern);
        using var nativeFilters = new NativeFilterSpecs([text, image]);
        var bottomPlace = ToUInt32(FileDialogAddPlaceFlags.Bottom);
        var topPlace = ToUInt32(FileDialogAddPlaceFlags.Top);
        var fileMustExist = ToUInt32(FileOpenOptions.FileMustExist);
        var multiSelectMustExist = ToUInt32(FileOpenOptions.FileMustExist | FileOpenOptions.AllowMultiSelect);
        var fileSystemPath = ToUInt32(ShellItemDisplayName.FileSysPath);

        await Assert.That(text == sameText).IsTrue();
        await Assert.That(text != image).IsTrue();
        await Assert.That(text.Equals((object)sameText)).IsTrue();
        await Assert.That(text.Equals((object)TextFilterName)).IsFalse();
        await Assert.That(text.GetHashCode()).IsEqualTo(sameText.GetHashCode());
        await Assert.That(bottomPlace).IsEqualTo(UIntZero);
        await Assert.That(topPlace).IsEqualTo(UIntOne);
        await Assert.That(fileMustExist).IsEqualTo(FileMustExistFlag);
        await Assert.That(multiSelectMustExist).IsEqualTo(MultiSelectMustExistFlag);
        await Assert.That(fileSystemPath).IsEqualTo(FileSystemPathFlag);
        await Assert.That(nativeFilters.Count).IsEqualTo(Two);
    }

    /// <summary>Converts an enum value to an unsigned integer for runtime value assertions.</summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value.</param>
    /// <returns>The unsigned integer representation.</returns>
    private static uint ToUInt32<T>(T value)
        where T : struct, Enum =>
        Convert.ToUInt32(value, CultureInfo.InvariantCulture);

    /// <summary>Unsupported COM wrapper used to cover helper type validation.</summary>
    private sealed class UnsupportedComObject : ComObject
    {
        /// <summary>Initializes a new instance of the <see cref="UnsupportedComObject"/> class.</summary>
        /// <param name="handle">The owned COM interface pointer.</param>
        internal UnsupportedComObject(IntPtr handle)
            : base(handle)
        {
        }
    }
}
