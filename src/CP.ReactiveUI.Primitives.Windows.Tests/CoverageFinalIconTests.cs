// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Media.Imaging;
using CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
using CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Final icon-helper coverage for deterministic decision paths.</summary>
public sealed class CoverageFinalIconTests
{
    /// <summary>A zero test value.</summary>
    private const int Zero = 0;

    /// <summary>An AppX manifest document without a logo element.</summary>
    private const string ManifestWithoutLogo =
        """
        <?xml version="1.0" encoding="utf-8"?>
        <Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10">
          <Properties />
        </Package>
        """;

    /// <summary>A source test window handle.</summary>
    private static readonly IntPtr SourceWindowHandle = new(OneHundredTwenty);

    /// <summary>A sibling process test window handle.</summary>
    private static readonly IntPtr ProcessSiblingWindowHandle = new(OneHundredTwentyFive);

    /// <summary>A top-level test window handle.</summary>
    private static readonly IntPtr TopLevelSiblingWindowHandle = new(OneHundredTwentySeven);

    /// <summary>A wrong-process test window handle.</summary>
    private static readonly IntPtr WrongProcessWindowHandle = new(OneHundredTwentyEight);

    /// <summary>Validates app-logo discovery branches using a known executable path.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AppLogoFromProcessPath_HandlesManifestAndScaleBranchesAsync()
    {
        await Assert.That(Icons.IconHelper.GetAppLogoFromProcessPath(null, default(Bitmap), Hundred)).IsNull();
        await Assert.That(Icons.IconHelper.GetAppLogoFromProcessPath(@"Z:\missing\app.exe", default(Bitmap), Hundred)).IsNull();

        var tempDirectory = CreateTemporaryDirectory("cp-reactive-logo-tests-");
        try
        {
            var exePath = Path.Combine(tempDirectory.FullName, "app.exe");
            File.WriteAllBytes(exePath, []);

            await Assert.That(Icons.IconHelper.GetAppLogoFromProcessPath(exePath, default(Bitmap), Hundred)).IsNull();

            var manifestPath = Path.Combine(tempDirectory.FullName, "AppxManifest.xml");
#if NETFRAMEWORK
            File.WriteAllText(manifestPath, ManifestWithoutLogo);
#else
            await File.WriteAllTextAsync(manifestPath, ManifestWithoutLogo, CancellationToken.None);
#endif
            await Assert.That(Icons.IconHelper.GetAppLogoFromProcessPath(exePath, default(Bitmap), Hundred)).IsNull();

#if NETFRAMEWORK
            File.WriteAllText(manifestPath, CreateManifest(string.Empty));
#else
            await File.WriteAllTextAsync(manifestPath, CreateManifest(string.Empty), CancellationToken.None);
#endif
            await Assert.That(Icons.IconHelper.GetAppLogoFromProcessPath(exePath, default(Bitmap), Hundred)).IsNull();

            var assetsDirectory = Directory.CreateDirectory(Path.Combine(tempDirectory.FullName, "Assets"));
#if NETFRAMEWORK
            File.WriteAllText(manifestPath, CreateManifest(@"Assets\Logo.png"));
#else
            await File.WriteAllTextAsync(manifestPath, CreateManifest(@"Assets\Logo.png"), CancellationToken.None);
#endif
            await Assert.That(Icons.IconHelper.GetAppLogoFromProcessPath(exePath, default(Bitmap), Hundred)).IsNull();

            using (var fallbackLogo = new Bitmap(Sixteen, Sixteen))
            {
                fallbackLogo.SetPixel(0, 0, Color.Red);
                fallbackLogo.Save(Path.Combine(assetsDirectory.FullName, "Logo.png"));
            }

            using (var scaledLogo = new Bitmap(TwentyFour, TwentyFour))
            {
                scaledLogo.SetPixel(0, 0, Color.Blue);
                scaledLogo.Save(Path.Combine(assetsDirectory.FullName, "Logo.scale-200.png"));
            }

            using var fallbackBitmap = Icons.IconHelper.GetAppLogoFromProcessPath(exePath, default(Bitmap), Hundred);
            using var scaledBitmap = Icons.IconHelper.GetAppLogoFromProcessPath(exePath, default(Bitmap), TwoHundred);
            var bitmapSource = Icons.IconHelper.GetAppLogoFromProcessPath(exePath, default(BitmapSource), TwoHundred);
            var unsupported = Icons.IconHelper.GetAppLogoFromProcessPath(exePath, string.Empty, TwoHundred);

            await Assert.That(fallbackBitmap).IsNotNull();
            await Assert.That(fallbackBitmap.Width).IsEqualTo(Sixteen);
            await Assert.That(scaledBitmap).IsNotNull();
            await Assert.That(scaledBitmap.Width).IsEqualTo(TwentyFour);
            await Assert.That(bitmapSource).IsNotNull();
            await Assert.That(bitmapSource.PixelWidth).IsEqualTo(TwentyFour);
            await Assert.That(unsupported).IsNull();
        }
        finally
        {
            Directory.Delete(tempDirectory.FullName, recursive: true);
        }
    }

    /// <summary>Validates associated and shell icon lookup boundaries.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ShellIconLookups_ExerciseInvalidAndFlagBranchesAsync()
    {
        await Assert.That(static () => Icons.IconHelper.ExtractAssociatedIcon(null, default(Icon))).Throws<ArgumentNullException>();
        await Assert.That(Icons.IconHelper.ExtractAssociatedIcon("missing-file.exe", default(Icon))).IsNull();
        await Assert.That(Icons.IconHelper.ExtractAssociatedIcon("https://example.invalid/app.exe", default(Icon), Zero, useLargeIcon: true)).IsNull();
        await Assert.That(Icons.IconHelper.CountAssociatedIcons("missing-file.exe")).IsGreaterThanOrEqualTo(Zero);

        var tempDirectory = CreateTemporaryDirectory("cp-reactive-empty-icon-tests-");
        try
        {
            var emptyExecutable = Path.Combine(tempDirectory.FullName, "empty.exe");
            File.WriteAllBytes(emptyExecutable, []);
            using var defaultAssociatedIcon = Icons.IconHelper.ExtractAssociatedIcon(emptyExecutable, default(Icon));
            using var smallAssociatedIcon = Icons.IconHelper.ExtractAssociatedIcon(emptyExecutable, default(Icon), Zero, useLargeIcon: false);
            await Assert.That(defaultAssociatedIcon).IsNull();
            await Assert.That(smallAssociatedIcon).IsNull();
        }
        finally
        {
            Directory.Delete(tempDirectory.FullName, recursive: true);
        }

        using var largeExtension = Icons.IconHelper.GetFileExtensionIcon("sample.txt", default(Icon), IconSize.Large, linkOverlay: false);
        using var smallExtension = Icons.IconHelper.GetFileExtensionIcon("sample.lnk", default(Icon), IconSize.Small, linkOverlay: true);
        using var closedFolder = Icons.IconHelper.GetFolderIcon(default(Icon), IconSize.Large, FolderIconType.Closed);
        using var openSmallFolder = Icons.IconHelper.GetFolderIcon(default(Icon), IconSize.Small, FolderIconType.Open);

        await Assert.That(largeExtension is null || largeExtension.Width > Zero).IsTrue();
        await Assert.That(smallExtension is null || smallExtension.Width > Zero).IsTrue();
        await Assert.That(closedFolder is null || closedFolder.Width > Zero).IsTrue();
        await Assert.That(openSmallFolder is null || openSmallFolder.Width > Zero).IsTrue();
    }

    /// <summary>Validates helper icon loader dispatch for string and integer resource names.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task LoadIconHelpers_UseNativeIconApiAndConvertOwnedHandlesAsync()
    {
        var fakeApi = new FakeNativeIconApi();
        var previousApi = Icons.NativeIconMethods.SetApiForTesting(fakeApi);
        try
        {
            using var metricById = Icons.IconHelper.LoadIconWithSystemMetrics(default(Icon), IntPtr.Zero, new IntPtr(One), IconMetricSize.SmallIcon);
            using var metricByName = Icons.IconHelper.LoadIconWithSystemMetrics(default(Bitmap), IntPtr.Zero, "sample", IconMetricSize.StandardIcon);
            using var scaledById = Icons.IconHelper.LoadIconWithScaleDown(default(Icon), IntPtr.Zero, new IntPtr(Two), Sixteen, TwentyFour);
            using var scaledByName = Icons.IconHelper.LoadIconWithScaleDown(default(Bitmap), IntPtr.Zero, "sample", TwentyFour, Sixteen);

            fakeApi.ReturnFailure = true;
            var failed = Icons.IconHelper.LoadIconWithSystemMetrics(default(Icon), IntPtr.Zero, new IntPtr(Three), IconMetricSize.SmallIcon);

            await Assert.That(metricById).IsNotNull();
            await Assert.That(metricByName).IsNotNull();
            await Assert.That(scaledById).IsNotNull();
            await Assert.That(scaledByName).IsNotNull();
            await Assert.That(failed).IsNull();
            await Assert.That(fakeApi.MetricIdCalls).IsEqualTo(Two);
            await Assert.That(fakeApi.MetricNameCalls).IsEqualTo(One);
            await Assert.That(fakeApi.ScaleIdCalls).IsEqualTo(One);
            await Assert.That(fakeApi.ScaleNameCalls).IsEqualTo(One);
        }
        finally
        {
            _ = Icons.NativeIconMethods.SetApiForTesting(previousApi);
        }
    }

    /// <summary>Validates native icon forwarding through the exchangeable API.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeIconMethods_UseNativeIconApiForAllForwardingWrappersAsync()
    {
        var fakeApi = new FakeNativeIconApi();
        var previousApi = Icons.NativeIconMethods.SetApiForTesting(fakeApi);
        try
        {
            await Assert.That(static () => Icons.NativeIconMethods.SetApiForTesting(null)).Throws<ArgumentNullException>();

            using var sourceHandle = new CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle(IntPtr.Zero);
            using var copiedSafeHandle = Icons.NativeIconMethods.CopyIcon(sourceHandle);
            using var copiedRawHandle = Icons.NativeIconMethods.CopyIcon(new IntPtr(Four));
            var iconInfo = default(CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.Structs.IconInfo);
            var iconInfoResult = Icons.NativeIconMethods.GetIconInfo(sourceHandle, out iconInfo);
            var iconInfoEx = default(CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.Structs.IconInfoEx);
            var iconInfoExResult = Icons.NativeIconMethods.GetIconInfoEx(new(Five), ref iconInfoEx);
            var createdHandle = Icons.NativeIconMethods.CreateIconIndirect(ref iconInfo);
            var drawArguments = new Icons.NativeIconMethods.DrawIconArguments(
                new(Six),
                (Seven, Eight),
                new(Nine),
                (Ten, Eleven),
                Twelve,
                new(Thirteen),
                DrawIconExFlags.DI_NORMAL);
            var drawResult = Icons.NativeIconMethods.DrawIconEx(in drawArguments);

            await Assert.That(copiedSafeHandle).IsNotNull();
            await Assert.That(copiedRawHandle).IsNotNull();
            await Assert.That(iconInfoResult).IsTrue();
            await Assert.That(iconInfoExResult).IsTrue();
            await Assert.That(createdHandle).IsEqualTo(new(Fourteen));
            await Assert.That(drawResult).IsTrue();
            await Assert.That(fakeApi.CopySafeCalls).IsEqualTo(One);
            await Assert.That(fakeApi.CopyRawCalls).IsEqualTo(One);
            await Assert.That(fakeApi.GetIconInfoCalls).IsEqualTo(One);
            await Assert.That(fakeApi.GetIconInfoExCalls).IsEqualTo(One);
            await Assert.That(fakeApi.CreateIconIndirectCalls).IsEqualTo(One);
            await Assert.That(fakeApi.DrawIconExCalls).IsEqualTo(One);
            await Assert.That(fakeApi.LastDrawArguments).IsEqualTo(drawArguments);
        }
        finally
        {
            _ = Icons.NativeIconMethods.SetApiForTesting(previousApi);
        }
    }

    /// <summary>Validates icon conversion, write helper, and metric-size branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IconHelper_ConvertsHandlesWritesIconAndValidatesMetricSizeAsync()
    {
        await Assert.That(Icons.IconHelper.IconHandleTo(IntPtr.Zero, default(Icon))).IsNull();
        await Assert.That(Icons.IconHelper.IconHandleTo(default(CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle), default(Icon))).IsNull();
        await Assert.That(static () => Icons.IconHelper.GetSystemIconSize((IconMetricSize)(-One))).Throws<ArgumentOutOfRangeException>();

        var iconHandle = CreateNativeIconHandle(Color.Yellow);
        var bitmapHandle = CreateNativeIconHandle(Color.Orange);
        var unsupportedHandle = CreateNativeIconHandle(Color.Black);
        using var iconHandleCleanup = new CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle(iconHandle);
        using var bitmapHandleCleanup = new CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle(bitmapHandle);
        using var unsupportedHandleCleanup = new CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle(unsupportedHandle);
        using var icon = Icons.IconHelper.IconHandleTo(iconHandle, default(Icon));
        using var bitmap = Icons.IconHelper.IconHandleTo(bitmapHandle, default(Bitmap));
        var unsupported = Icons.IconHelper.IconHandleTo(unsupportedHandle, string.Empty);
        using var safeIconHandle = new CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle(CreateNativeIconHandle(Color.Purple));
        using var safeIcon = Icons.IconHelper.IconHandleTo(safeIconHandle, default(Icon));
#if NETFRAMEWORK
        using var stream = new MemoryStream();
#else
        await using var stream = new MemoryStream();
#endif
        using var sourceBitmap = new Bitmap(Sixteen, Sixteen);

        Icons.IconHelper.WriteIcon(stream, [sourceBitmap]);

        await Assert.That(icon).IsNotNull();
        await Assert.That(bitmap).IsNotNull();
        await Assert.That(unsupported).IsNull();
        await Assert.That(safeIcon).IsNotNull();
        await Assert.That(Icons.IconHelper.GetSystemIconSize(IconMetricSize.SmallIcon).Width).IsGreaterThan(Zero);
        await Assert.That(Icons.IconHelper.GetSystemIconSize(IconMetricSize.StandardIcon).Height).IsGreaterThan(Zero);
        await Assert.That(stream.Length).IsGreaterThan(Zero);
    }

    /// <summary>Validates deterministic window-handle icon fallback branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IconExtensions_OperationSeamCoversWindowHandleFallbacksAsync()
    {
        var previousOperations = SetOperationsWithResourceIcon(new(Two), Color.Red);
        try
        {
            await Assert.That(static () => Icons.IconExtensions.SetOperationsForTesting(null)).Throws<ArgumentNullException>();

            using var defaultOverloadIcon = Icons.IconExtensions.GetIconForWindowHandle(new(Five), default(Icon));
            using var secondarySmallIcon = Icons.IconExtensions.GetIconForWindowHandle(new(One), default(Icon), useLargeIcons: false);

            _ = SetOperationsWithResourceIcon(IntPtr.Zero, Color.Salmon);
            using var primarySmallIcon = Icons.IconExtensions.GetIconForWindowHandle(new(Six), default(Icon), useLargeIcons: false);

            _ = SetOperationsWithResourceIcon(new(One), Color.Brown);
            using var primaryLargeIcon = Icons.IconExtensions.GetIconForWindowHandle(new(Seven), default(Icon), useLargeIcons: true);

            _ = SetOperationsWithClassIcon(ClassLongIndex.SmallIconHandle, Color.Blue);
            using var classSmallIcon = Icons.IconExtensions.GetIconForWindowHandle(new(Two), default(Icon), useLargeIcons: false);

            _ = SetOperationsWithClassIcon(ClassLongIndex.IconHandle, Color.Green);
            using var classLargeIcon = Icons.IconExtensions.GetIconForWindowHandle(new(Three), default(Icon), useLargeIcons: true);

            _ = SetOperationsWithoutIcons();
            var missingIcon = Icons.IconExtensions.GetIconForWindowHandle(new(Four), default(Icon), useLargeIcons: true);

            await Assert.That(secondarySmallIcon).IsNotNull();
            await Assert.That(defaultOverloadIcon).IsNotNull();
            await Assert.That(primarySmallIcon).IsNotNull();
            await Assert.That(primaryLargeIcon).IsNotNull();
            await Assert.That(classSmallIcon).IsNotNull();
            await Assert.That(classLargeIcon).IsNotNull();
            await Assert.That(missingIcon).IsNull();
        }
        finally
        {
            _ = Icons.IconExtensions.SetOperationsForTesting(previousOperations);
        }
    }

    /// <summary>Validates high-level interop-window icon lookup branches without real UI side effects.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IconExtensions_GetIconCoversAppProcessAndTopLevelFallbacksAsync()
    {
        var sourceWindow = new InteropWindow(SourceWindowHandle) { Classname = "SourceWindow", ProcessId = GetCurrentProcessId() };
        var previousOperations = Icons.IconExtensions.SetOperationsForTesting(CreateHighLevelOperations(
            static _ => true,
            static _ => IntPtr.Zero,
            static () => []));
        try
        {
            var appIcon = sourceWindow.GetIcon(default(Icon), useLargeIcons: false);

            _ = Icons.IconExtensions.SetOperationsForTesting(CreateHighLevelOperations(
                static _ => false,
                static handle => handle == SourceWindowHandle ? CreateNativeIconHandle(Color.Aqua) : IntPtr.Zero,
                static () => []));
            using var immediateIcon = sourceWindow.GetIcon(default(Icon), useLargeIcons: false);
            using var immediateIconDefaultOverload = sourceWindow.GetIcon(default(Icon));
            using var windowIconDefaultOverload = sourceWindow.GetIconFromWindow(default(Icon));

            _ = Icons.IconExtensions.SetOperationsForTesting(CreateHighLevelOperations(
                static _ => false,
                static handle => handle == ProcessSiblingWindowHandle ? CreateNativeIconHandle(Color.Azure) : IntPtr.Zero,
                static () => [],
                static _ => [Process.GetCurrentProcess()],
                static _ => new InteropWindow(ProcessSiblingWindowHandle)));
            using var processSiblingIcon = sourceWindow.GetIcon(default(Icon), useLargeIcons: false);

            _ = Icons.IconExtensions.SetOperationsForTesting(CreateHighLevelOperations(
                static _ => false,
                static handle => handle == TopLevelSiblingWindowHandle ? CreateNativeIconHandle(Color.Beige) : IntPtr.Zero,
                static () =>
                [
                    new InteropWindow(SourceWindowHandle) { ProcessId = GetCurrentProcessId() },
                    new InteropWindow(WrongProcessWindowHandle) { ProcessId = GetCurrentProcessId() + One },
                    new InteropWindow(TopLevelSiblingWindowHandle) { ProcessId = GetCurrentProcessId() },
                ]));
            using var topLevelSiblingIcon = sourceWindow.GetIcon(default(Icon), useLargeIcons: false);

            await Assert.That(appIcon).IsNull();
            await Assert.That(immediateIcon).IsNotNull();
            await Assert.That(immediateIconDefaultOverload).IsNotNull();
            await Assert.That(windowIconDefaultOverload).IsNotNull();
            await Assert.That(processSiblingIcon).IsNotNull();
            await Assert.That(topLevelSiblingIcon).IsNotNull();
        }
        finally
        {
            _ = Icons.IconExtensions.SetOperationsForTesting(previousOperations);
        }
    }

    /// <summary>Validates app-logo window path, icon stream failures, structs, and native wrapper fallbacks.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NonCursorIconGaps_CoverStreamsStructsAndNativeWrappersAsync()
    {
        var plainWindow = new InteropWindow(IntPtr.Zero) { Classname = "PlainWindow", ProcessId = Zero };
        var appFrameWithChild = CreateAppFrameWindow(new(Eight), AppQueryExtensions.AppWindowClass);
        var appFrameWithoutChild = CreateAppFrameWindow(new(Nine), "OtherChild");

        using var defaultLogo = Icons.IconHelper.GetAppLogo(plainWindow, default(Bitmap));
        using var childLogo = Icons.IconHelper.GetAppLogo(appFrameWithChild, default(Bitmap), Hundred);
        using var missingChildLogo = Icons.IconHelper.GetAppLogo(appFrameWithoutChild, default(Bitmap), Hundred);
#if NETFRAMEWORK
        using var throwingStream = new ThrowingIconStream();
#else
        await using var throwingStream = new ThrowingIconStream();
#endif
        using var extracted = throwingStream.ExtractVistaIcon();

        var iconInfo = default(Icons.Structs.IconInfo);
        iconInfo.IsIcon = true;
        iconInfo.Hotspot = new(Three, Four);
        using var bitmaskBitmapHandle = iconInfo.BitmaskBitmapHandle;
        using var colorBitmapHandle = iconInfo.ColorBitmapHandle;
        var iconInfoEx = Icons.Structs.IconInfoEx.Create();
        iconInfoEx.IsIcon = true;
        iconInfoEx.Hotspot = new(Five, Six);
        var unterminatedIconInfoEx = Icons.Structs.IconInfoEx.CreateWithUnterminatedModuleNameForTesting('x');

        var nativeApi = Icons.WindowsNativeIconApi.Instance;
        using var copiedIcon = nativeApi.CopyIcon(IntPtr.Zero);
        var indirectInfo = default(Icons.Structs.IconInfo);
        var indirectHandle = nativeApi.CreateIconIndirect(ref indirectInfo);
        var extendedInfo = Icons.Structs.IconInfoEx.Create();
        var extendedResult = nativeApi.GetIconInfoEx(IntPtr.Zero, ref extendedInfo);
        var metricLoaderIsAvailable = TryLoadMissingMetricIcon(nativeApi, out var metricResult, out var metricHandle);
        using var metricSafeHandle = new CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle(metricHandle);
        var scaleDownLoaderIsAvailable = TryLoadMissingScaleDownIcon(nativeApi, out var scaledResult, out var scaledHandle);
        using var scaledSafeHandle = new CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle(scaledHandle);
        var nativeOperations = Icons.IconWindowOperations.CreateNative();
        var nativeProcessId = nativeOperations.GetProcessId(new InteropWindow(IntPtr.Zero) { ProcessId = GetCurrentProcessId() });

        await Assert.That(defaultLogo).IsNull();
        await Assert.That(childLogo).IsNull();
        await Assert.That(missingChildLogo).IsNull();
        await Assert.That(extracted).IsNull();
        await Assert.That(iconInfo.IsIcon).IsTrue();
        await Assert.That(iconInfo.Hotspot).IsEqualTo(new(Three, Four));
        await Assert.That(bitmaskBitmapHandle.IsInvalid).IsTrue();
        await Assert.That(colorBitmapHandle.IsInvalid).IsTrue();
        await Assert.That(iconInfoEx.IsIcon).IsTrue();
        await Assert.That(iconInfoEx.Hotspot).IsEqualTo(new(Five, Six));
        await Assert.That(unterminatedIconInfoEx.ModuleName.Length).IsGreaterThan(Zero);
        await Assert.That(copiedIcon).IsNotNull();
        await Assert.That(indirectHandle).IsEqualTo(IntPtr.Zero);
        await Assert.That(extendedResult).IsFalse();
        await Assert.That(metricLoaderIsAvailable ? metricResult != Zero : metricHandle == IntPtr.Zero).IsTrue();
        await Assert.That(scaleDownLoaderIsAvailable ? scaledResult != Zero : scaledHandle == IntPtr.Zero).IsTrue();
        await Assert.That(nativeProcessId).IsEqualTo(GetCurrentProcessId());
    }

    /// <summary>Creates an AppX manifest document containing the supplied logo path.</summary>
    /// <param name="logoPath">The logo path to write.</param>
    /// <returns>The manifest XML.</returns>
    private static string CreateManifest(string logoPath) =>
        $$"""
        <?xml version="1.0" encoding="utf-8"?>
        <Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10">
          <Properties>
            <Logo>{{logoPath}}</Logo>
          </Properties>
        </Package>
        """;

    /// <summary>Creates high-level icon-window operations for interop-window extension tests.</summary>
    /// <param name="isApp">The app predicate.</param>
    /// <param name="tryIconHandle">The icon handle lookup.</param>
    /// <param name="getTopWindows">The top-level window lookup.</param>
    /// <param name="getProcessesByName">The optional process-name lookup.</param>
    /// <param name="createWindow">The optional window factory.</param>
    /// <returns>The deterministic operations.</returns>
    private static IconWindowOperations CreateHighLevelOperations(
        Func<IInteropWindow, bool> isApp,
        Func<IntPtr, IntPtr> tryIconHandle,
        Func<IEnumerable<IInteropWindow>> getTopWindows,
        Func<string, Process[]> getProcessesByName = null,
        Func<IntPtr, IInteropWindow> createWindow = null) =>
        new()
        {
            CreateWindow = createWindow ?? (static handle => new InteropWindow(handle)),
            GetClassLong = static (_, _) => IntPtr.Zero,
            GetProcessById = static _ => Process.GetCurrentProcess(),
            GetProcessId = static window => window.ProcessId ?? GetCurrentProcessId(),
            GetProcessPath = static _ => null,
            GetProcessesByName = getProcessesByName ?? (static _ => []),
            GetTopWindows = getTopWindows,
            IsApp = isApp,
            TrySendMessage = (IntPtr windowHandle, WindowsMessages _, IntPtr _, out IntPtr result) =>
            {
                result = tryIconHandle(windowHandle);
                return result != IntPtr.Zero;
            },
        };

    /// <summary>Creates deterministic icon-window operations for fallback tests.</summary>
    /// <param name="trySendMessage">The SendMessage probe.</param>
    /// <param name="getClassLong">The class-long lookup probe.</param>
    /// <returns>The deterministic operations.</returns>
    private static IconWindowOperations CreateWindowOperations(
        TrySendIconMessage trySendMessage,
        Func<IntPtr, ClassLongIndex, IntPtr> getClassLong) =>
        new()
        {
            CreateWindow = static _ => throw new InvalidOperationException("Window creation was not expected."),
            GetClassLong = getClassLong,
            GetProcessById = static _ => throw new InvalidOperationException("Process lookup was not expected."),
            GetProcessId = static _ => Zero,
            GetProcessPath = static _ => null,
            GetProcessesByName = static _ => [],
            GetTopWindows = static () => [],
            IsApp = static _ => false,
            TrySendMessage = trySendMessage,
        };

    /// <summary>Creates an app-frame window with a deterministic child window.</summary>
    /// <param name="childHandle">The child window handle.</param>
    /// <param name="childClassname">The child window class name.</param>
    /// <returns>The configured app-frame window.</returns>
    private static InteropWindow CreateAppFrameWindow(IntPtr childHandle, string childClassname) =>
        new(IntPtr.Zero) { Classname = AppQueryExtensions.AppFrameWindowClass, Children = [CreateAppFrameChild(childHandle, childClassname)] };

    /// <summary>Creates a deterministic child window for an app-frame window.</summary>
    /// <param name="childHandle">The child window handle.</param>
    /// <param name="childClassname">The child window class name.</param>
    /// <returns>The configured child window.</returns>
    private static InteropWindow CreateAppFrameChild(IntPtr childHandle, string childClassname) =>
        new(childHandle) { Classname = childClassname, ProcessId = GetCurrentProcessId() };

    /// <summary>Gets the identifier of the current process.</summary>
    /// <returns>The current process identifier.</returns>
    private static int GetCurrentProcessId()
    {
        using var process = Process.GetCurrentProcess();
        return process.Id;
    }

    /// <summary>Creates a unique temporary directory.</summary>
    /// <param name="prefix">The directory name prefix.</param>
    /// <returns>The created directory.</returns>
    private static DirectoryInfo CreateTemporaryDirectory(string prefix) =>
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("N")));

    /// <summary>Sets operations that return an icon for a matching resource parameter.</summary>
    /// <param name="expectedResourceParameter">The resource parameter that produces an icon.</param>
    /// <param name="color">The returned icon color.</param>
    /// <returns>The previous icon-window operations.</returns>
    private static IconWindowOperations SetOperationsWithResourceIcon(IntPtr expectedResourceParameter, Color color) =>
        Icons.IconExtensions.SetOperationsForTesting(CreateWindowOperations(
            (IntPtr _, WindowsMessages _, IntPtr resourceParameter, out IntPtr result) =>
                TryCreateIconHandle(resourceParameter, expectedResourceParameter, color, out result),
            static (_, _) => IntPtr.Zero));

    /// <summary>Sets operations that return an icon for a matching class-long index.</summary>
    /// <param name="expectedClassLongIndex">The class-long index that produces an icon.</param>
    /// <param name="color">The returned icon color.</param>
    /// <returns>The previous icon-window operations.</returns>
    private static IconWindowOperations SetOperationsWithClassIcon(ClassLongIndex expectedClassLongIndex, Color color) =>
        Icons.IconExtensions.SetOperationsForTesting(CreateWindowOperations(
            TryReturnNoIcon,
            (_, classLongIndex) => classLongIndex == expectedClassLongIndex ? CreateNativeIconHandle(color) : IntPtr.Zero));

    /// <summary>Sets operations that do not return icons.</summary>
    /// <returns>The previous icon-window operations.</returns>
    private static IconWindowOperations SetOperationsWithoutIcons() =>
        Icons.IconExtensions.SetOperationsForTesting(CreateWindowOperations(TryReturnNoIcon, static (_, _) => IntPtr.Zero));

    /// <summary>Returns an icon handle when a resource parameter matches the configured value.</summary>
    /// <param name="resourceParameter">The resource parameter to inspect.</param>
    /// <param name="expectedResourceParameter">The resource parameter that produces an icon.</param>
    /// <param name="color">The returned icon color.</param>
    /// <param name="result">The returned icon handle.</param>
    /// <returns><see langword="true" /> when an icon handle was created; otherwise, <see langword="false" />.</returns>
    private static bool TryCreateIconHandle(IntPtr resourceParameter, IntPtr expectedResourceParameter, Color color, out IntPtr result)
    {
        result = resourceParameter == expectedResourceParameter ? CreateNativeIconHandle(color) : IntPtr.Zero;
        return result != IntPtr.Zero;
    }

    /// <summary>Returns no icon from a deterministic icon-message probe.</summary>
    /// <param name="windowHandle">The target window handle.</param>
    /// <param name="message">The requested window message.</param>
    /// <param name="resourceParameter">The icon resource parameter.</param>
    /// <param name="result">The icon handle result.</param>
    /// <returns><see langword="false" />.</returns>
    private static bool TryReturnNoIcon(IntPtr windowHandle, WindowsMessages message, IntPtr resourceParameter, out IntPtr result)
    {
        _ = windowHandle;
        _ = message;
        _ = resourceParameter;
        result = IntPtr.Zero;
        return false;
    }

    /// <summary>Attempts to load a missing icon through the metric-sized native wrapper.</summary>
    /// <param name="nativeApi">The native icon API wrapper.</param>
    /// <param name="result">The native loader result.</param>
    /// <param name="iconHandle">The loaded icon handle.</param>
    /// <returns><see langword="true" /> when the native export is available; otherwise, <see langword="false" />.</returns>
    private static bool TryLoadMissingMetricIcon(WindowsNativeIconApi nativeApi, out int result, out IntPtr iconHandle)
    {
        try
        {
            result = nativeApi.LoadIconMetric(IntPtr.Zero, "missing-cp-reactive-icon", IconMetricSize.SmallIcon, out iconHandle);
            return true;
        }
        catch (EntryPointNotFoundException)
        {
            result = Zero;
            iconHandle = IntPtr.Zero;
            return false;
        }
    }

    /// <summary>Attempts to load a missing icon through the scale-down native wrapper.</summary>
    /// <param name="nativeApi">The native icon API wrapper.</param>
    /// <param name="result">The native loader result.</param>
    /// <param name="iconHandle">The loaded icon handle.</param>
    /// <returns><see langword="true" /> when the native export is available; otherwise, <see langword="false" />.</returns>
    private static bool TryLoadMissingScaleDownIcon(WindowsNativeIconApi nativeApi, out int result, out IntPtr iconHandle)
    {
        try
        {
            result = nativeApi.LoadIconWithScaleDown(IntPtr.Zero, "missing-cp-reactive-icon", Sixteen, Sixteen, out iconHandle);
            return true;
        }
        catch (EntryPointNotFoundException)
        {
            result = Zero;
            iconHandle = IntPtr.Zero;
            return false;
        }
    }

    /// <summary>Creates an owned native icon handle from a test bitmap color.</summary>
    /// <param name="color">The bitmap color.</param>
    /// <returns>The native icon handle.</returns>
    private static IntPtr CreateNativeIconHandle(Color color)
    {
        using var bitmap = new Bitmap(Sixteen, Sixteen);
        bitmap.SetPixel(0, 0, color);
        return bitmap.GetHicon();
    }

    /// <summary>A stream that throws an IOException from icon extraction reads.</summary>
    private sealed class ThrowingIconStream : MemoryStream
    {
        /// <inheritdoc />
        public override long Length => throw new IOException("The test stream cannot report length.");
    }

    /// <summary>Fake native icon API used to verify forwarding and conversion behavior.</summary>
    private sealed class FakeNativeIconApi : INativeIconApi
    {
        /// <summary>Gets or sets a value indicating whether loader calls should fail.</summary>
        public bool ReturnFailure { get; set; }

        /// <summary>Gets the metric-by-id call count.</summary>
        public int MetricIdCalls { get; private set; }

        /// <summary>Gets the metric-by-name call count.</summary>
        public int MetricNameCalls { get; private set; }

        /// <summary>Gets the scale-by-id call count.</summary>
        public int ScaleIdCalls { get; private set; }

        /// <summary>Gets the scale-by-name call count.</summary>
        public int ScaleNameCalls { get; private set; }

        /// <summary>Gets the safe-copy call count.</summary>
        public int CopySafeCalls { get; private set; }

        /// <summary>Gets the raw-copy call count.</summary>
        public int CopyRawCalls { get; private set; }

        /// <summary>Gets the create-indirect call count.</summary>
        public int CreateIconIndirectCalls { get; private set; }

        /// <summary>Gets the draw-icon call count.</summary>
        public int DrawIconExCalls { get; private set; }

        /// <summary>Gets the get-icon-info call count.</summary>
        public int GetIconInfoCalls { get; private set; }

        /// <summary>Gets the get-icon-info-ex call count.</summary>
        public int GetIconInfoExCalls { get; private set; }

        /// <summary>Gets the last draw arguments received by the fake.</summary>
        public Icons.NativeIconMethods.DrawIconArguments LastDrawArguments { get; private set; }

        /// <inheritdoc />
        public CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle CopyIcon(CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle iconHandle) =>
            CopyIconCore();

        /// <inheritdoc />
        public CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle CopyIcon(IntPtr iconHandle) =>
            CopyIconRawCore();

        /// <inheritdoc />
        public IntPtr CreateIconIndirect(ref CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.Structs.IconInfo icon)
        {
            _ = icon;
            CreateIconIndirectCalls++;
            return new(Fourteen);
        }

        /// <inheritdoc />
        public bool DrawIconEx(in Icons.NativeIconMethods.DrawIconArguments arguments)
        {
            LastDrawArguments = arguments;
            DrawIconExCalls++;
            return true;
        }

        /// <inheritdoc />
        public bool GetIconInfo(
            CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle iconHandle,
            out CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.Structs.IconInfo iconInfo)
        {
            _ = iconHandle;
            iconInfo = default;
            GetIconInfoCalls++;
            return true;
        }

        /// <inheritdoc />
        public bool GetIconInfoEx(IntPtr iconOrCursorHandle, ref CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.Structs.IconInfoEx iconInfoEx)
        {
            _ = iconOrCursorHandle;
            _ = iconInfoEx;
            GetIconInfoExCalls++;
            return true;
        }

        /// <inheritdoc />
        public int LoadIconMetric(IntPtr instanceHandle, IntPtr iconName, IconMetricSize lims, out IntPtr iconHandle)
        {
            _ = instanceHandle;
            _ = iconName;
            _ = lims;
            MetricIdCalls++;
            return LoadIcon(out iconHandle);
        }

        /// <inheritdoc />
        public int LoadIconMetric(IntPtr instanceHandle, string iconName, IconMetricSize lims, out IntPtr iconHandle)
        {
            _ = instanceHandle;
            _ = iconName;
            _ = lims;
            MetricNameCalls++;
            return LoadIcon(out iconHandle);
        }

        /// <inheritdoc />
        public int LoadIconWithScaleDown(IntPtr instanceHandle, IntPtr iconName, int cx, int cy, out IntPtr iconHandle)
        {
            _ = instanceHandle;
            _ = iconName;
            _ = cx;
            _ = cy;
            ScaleIdCalls++;
            return LoadIcon(out iconHandle);
        }

        /// <inheritdoc />
        public int LoadIconWithScaleDown(IntPtr instanceHandle, string iconName, int cx, int cy, out IntPtr iconHandle)
        {
            _ = instanceHandle;
            _ = iconName;
            _ = cx;
            _ = cy;
            ScaleNameCalls++;
            return LoadIcon(out iconHandle);
        }

        /// <summary>Loads or fails a deterministic native icon handle.</summary>
        /// <param name="iconHandle">The loaded icon handle.</param>
        /// <returns>The fake HRESULT.</returns>
        private int LoadIcon(out IntPtr iconHandle)
        {
            if (ReturnFailure)
            {
                iconHandle = IntPtr.Zero;
                return One;
            }

            using var bitmap = new Bitmap(Sixteen, Sixteen);
            bitmap.SetPixel(0, 0, Color.Green);
            iconHandle = bitmap.GetHicon();
            return Zero;
        }

        /// <summary>Records and returns a safe-copy result.</summary>
        /// <returns>The fake copied icon handle.</returns>
        private CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle CopyIconCore()
        {
            CopySafeCalls++;
            return new(IntPtr.Zero);
        }

        /// <summary>Records and returns a raw-copy result.</summary>
        /// <returns>The fake copied icon handle.</returns>
        private CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles.SafeIconHandle CopyIconRawCore()
        {
            CopyRawCalls++;
            return new(IntPtr.Zero);
        }
    }
}
