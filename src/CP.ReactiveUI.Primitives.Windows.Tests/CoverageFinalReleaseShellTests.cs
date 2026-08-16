// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Exercises shell composition seams without opening Windows user interface.</summary>
public sealed class CoverageFinalReleaseShellTests
{
    /// <summary>A successful HRESULT.</summary>
    private const int Success = 0;

    /// <summary>The icon size requested by the manifest-logo test.</summary>
    private const int RequestedIconSize = 100;

    /// <summary>The byte offset of the cursor handle within <see cref="CursorInfo" />.</summary>
    private const int CursorHandleOffset = sizeof(int) * 2;

    /// <summary>A deterministic native cursor handle.</summary>
    private static readonly IntPtr CursorHandle = new(37);

    /// <summary>Verifies common-dialog activation and shell-item creation use replaceable native operations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ComDialogHelper_UsesDeterministicNativeCompositionOperationsAsync()
    {
        var previousComActivation = ComDialogHelper.CoCreateInstance;
        var previousShellItemCreation = ComDialogHelper.CreateShellItem;
        try
        {
            ComDialogHelper.CoCreateInstance = CreateComDialog;
            ComDialogHelper.CreateShellItem = CreateShellItem;

            using IFileOpenDialog open = ComDialogHelper.CreateDialog<IFileOpenDialog>(ComDialogHelper.ClsidFileOpenDialog);
            using IFileSaveDialog save = ComDialogHelper.CreateDialog<IFileSaveDialog>(ComDialogHelper.ClsidFileSaveDialog);
            using IShellItem item = ComDialogHelper.ShellItemFromPath("C:\\deterministic-shell-item");

            await Assert.That(open).IsNotNull();
            await Assert.That(save).IsNotNull();
            await Assert.That(item).IsNotNull();
            _ = new NativeFileDialogExecutor();
        }
        finally
        {
            ComDialogHelper.CoCreateInstance = previousComActivation;
            ComDialogHelper.CreateShellItem = previousShellItemCreation;
            ComDialogHelper.RestoreNativeFactoriesForTesting();
        }
    }

    /// <summary>Verifies the convenience add-place overload produces a bottom-place request without displaying a dialog.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task FileSaveDialogBuilder_AddPlaceConvenienceOverloadPreservesBottomPlacementAsync()
    {
        var executor = new CapturingExecutor();
        var result = new FileSaveDialogBuilder()
            .AddPlace("C:\\deterministic-place")
            .ShowDialog(IntPtr.Zero, executor);

        await Assert.That(result.WasCancelled).IsTrue();
        await Assert.That(executor.SaveRequest).IsNotNull();
        await Assert.That(executor.SaveRequest.Places.Count).IsEqualTo(1);
        await Assert.That(executor.SaveRequest.Places[0].Path).IsEqualTo("C:\\deterministic-place");
        await Assert.That(executor.SaveRequest.Places[0].AtTop).IsFalse();
    }

    /// <summary>Verifies a manifest without a logo is handled without loading any native icon resource.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IconHelper_ManifestWithoutLogoReturnsNullWithoutNativeIconAccessAsync()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"cp-reactive-shell-{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(directory);
        try
        {
            string manifestPath = Path.Combine(directory, "AppxManifest.xml");
            const string manifest = "<Package xmlns=\"http://schemas.microsoft.com/appx/manifest/foundation/windows10\"><Properties /></Package>";
#if NETFRAMEWORK
            File.WriteAllText(manifestPath, manifest);
#else
            await File.WriteAllTextAsync(manifestPath, manifest);
#endif

            using var bitmapType = new Bitmap(1, 1);
            Bitmap logo = IconHelper.GetAppLogoFromProcessPath(Path.Combine(directory, "application.exe"), bitmapType, RequestedIconSize);

            await Assert.That(logo).IsNull();
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>Verifies current-cursor capture branches use deterministic cursor information providers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CursorHelper_UsesDeterministicCaptureProvidersAsync()
    {
        CursorInfoProvider previousCursorInfo = CursorHelper.SetCursorInfoProviderForTesting(PopulateShowingCursorInfo);
        CursorIconInfoProvider previousIconInfo = CursorHelper.SetCursorIconInfoProviderForTesting(PopulateIconInfo);
        CursorCaptureOperation previousCapture = CursorHelper.SetCursorCaptureOperationForTesting(CaptureCursor);
        try
        {
            bool captured = CursorHelper.TryGetCurrentCursor(out CapturedCursor result);
            using (result)
            {
                await Assert.That(captured).IsTrue();
                NativeSize expectedSize = new(1, 1);
                await Assert.That(result.Size).IsEqualTo(expectedSize);
            }

            _ = CursorHelper.SetCursorInfoProviderForTesting(DoNotPopulateCursorInfo);
            await Assert.That(CursorHelper.TryGetCurrentCursor(out CapturedCursor hiddenResult)).IsFalse();
            hiddenResult.Dispose();

            _ = CursorHelper.SetCursorInfoProviderForTesting(PopulateShowingCursorInfo);
            _ = CursorHelper.SetCursorIconInfoProviderForTesting(DoNotPopulateIconInfo);
            await Assert.That(CursorHelper.TryGetCurrentCursor(out CapturedCursor unavailableResult)).IsFalse();
            unavailableResult.Dispose();

            await Assert.That(static () => CursorHelper.SetCursorInfoProviderForTesting(null)).Throws<ArgumentNullException>();
            await Assert.That(static () => CursorHelper.SetCursorIconInfoProviderForTesting(null)).Throws<ArgumentNullException>();
            await Assert.That(static () => CursorHelper.SetCursorCaptureOperationForTesting(null)).Throws<ArgumentNullException>();
        }
        finally
        {
            _ = CursorHelper.SetCursorCaptureOperationForTesting(previousCapture);
            _ = CursorHelper.SetCursorIconInfoProviderForTesting(previousIconInfo);
            _ = CursorHelper.SetCursorInfoProviderForTesting(previousCursorInfo);
        }
    }

    /// <summary>Creates a deterministic COM dialog pointer.</summary>
    /// <param name="classId">The requested class identifier.</param>
    /// <param name="outerUnknown">The aggregation controller.</param>
    /// <param name="classContext">The COM context.</param>
    /// <param name="interfaceId">The requested interface identifier.</param>
    /// <param name="instance">The created interface pointer.</param>
    /// <returns>A successful HRESULT.</returns>
    private static int CreateComDialog(ref Guid classId, IntPtr outerUnknown, uint classContext, ref Guid interfaceId, out IntPtr instance)
    {
        _ = classId;
        _ = outerUnknown;
        _ = classContext;
        _ = interfaceId;
        instance = IntPtr.Zero;
        return Success;
    }

    /// <summary>Creates a deterministic shell-item pointer.</summary>
    /// <param name="path">The shell parsing path.</param>
    /// <param name="bindContext">The optional bind context.</param>
    /// <param name="interfaceId">The requested interface identifier.</param>
    /// <param name="shellItem">The created shell-item pointer.</param>
    /// <returns>A successful HRESULT.</returns>
    private static int CreateShellItem(string path, IntPtr bindContext, ref Guid interfaceId, out IntPtr shellItem)
    {
        _ = path;
        _ = bindContext;
        _ = interfaceId;
        shellItem = IntPtr.Zero;
        return Success;
    }

    /// <summary>Populates visible cursor information without querying Windows.</summary>
    /// <param name="cursorInfo">The cursor information to populate.</param>
    /// <returns><see langword="true" />.</returns>
    private static bool PopulateShowingCursorInfo(ref CursorInfo cursorInfo)
    {
        cursorInfo = CreateShowingCursorInfo();
        return true;
    }

    /// <summary>Does not populate cursor information.</summary>
    /// <param name="cursorInfo">The cursor information to leave unchanged.</param>
    /// <returns><see langword="false" />.</returns>
    private static bool DoNotPopulateCursorInfo(ref CursorInfo cursorInfo)
    {
        _ = cursorInfo;
        return false;
    }

    /// <summary>Populates empty icon information without querying Windows.</summary>
    /// <param name="cursorHandle">The cursor handle.</param>
    /// <param name="iconInfo">The icon information to populate.</param>
    /// <returns><see langword="true" />.</returns>
    private static bool PopulateIconInfo(IntPtr cursorHandle, ref IconInfoEx iconInfo)
    {
        _ = cursorHandle;
        iconInfo = IconInfoEx.Create();
        return true;
    }

    /// <summary>Does not populate icon information.</summary>
    /// <param name="cursorHandle">The cursor handle.</param>
    /// <param name="iconInfo">The icon information to leave unchanged.</param>
    /// <returns><see langword="false" />.</returns>
    private static bool DoNotPopulateIconInfo(IntPtr cursorHandle, ref IconInfoEx iconInfo)
    {
        _ = cursorHandle;
        _ = iconInfo;
        return false;
    }

    /// <summary>Captures a deterministic cursor shape.</summary>
    /// <param name="result">The cursor capture to populate.</param>
    /// <param name="cursorHandle">The cursor handle.</param>
    /// <param name="iconInfo">The cursor icon information.</param>
    private static void CaptureCursor(CapturedCursor result, IntPtr cursorHandle, in IconInfoEx iconInfo)
    {
        _ = cursorHandle;
        _ = iconInfo;
        result.Size = new(1, 1);
    }

    /// <summary>Creates an in-memory visible cursor information structure.</summary>
    /// <returns>The visible cursor information.</returns>
    private static CursorInfo CreateShowingCursorInfo()
    {
        CursorInfo cursorInfo = CursorInfo.Create();
        IntPtr buffer = Marshal.AllocHGlobal(Marshal.SizeOf<CursorInfo>());
        try
        {
            Marshal.StructureToPtr(cursorInfo, buffer, fDeleteOld: false);
            Marshal.WriteInt32(buffer, sizeof(int), (int)CursorInfoFlags.Showing);
            Marshal.WriteIntPtr(buffer, CursorHandleOffset, CursorHandle);
            return Marshal.PtrToStructure<CursorInfo>(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>Captures requests without displaying a dialog.</summary>
    private sealed class CapturingExecutor : IFileDialogExecutor
    {
        /// <summary>Gets the captured save request.</summary>
        public FileSaveDialogRequest SaveRequest { get; private set; }

        /// <inheritdoc />
        public FileDialogResult ShowOpen(FileOpenDialogRequest request)
        {
            _ = request;
            return FileDialogResult.Cancelled();
        }

        /// <inheritdoc />
        public FileDialogResult ShowSave(FileSaveDialogRequest request)
        {
            SaveRequest = request;
            return FileDialogResult.Cancelled();
        }

        /// <inheritdoc />
        public FileDialogResult ShowFolder(FolderPickerDialogRequest request)
        {
            _ = request;
            return FileDialogResult.Cancelled();
        }
    }
}
