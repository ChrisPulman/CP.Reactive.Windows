// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Deterministic coverage for shared Windows app, environment, software, and device decisions.</summary>
#if NETFRAMEWORK
public sealed class CoverageFinalAppEnvironmentSoftwareTests
#else
public sealed partial class CoverageFinalAppEnvironmentSoftwareTests
#endif
{
    /// <summary>Defines a non-zero synthetic app-launcher handle.</summary>
    private static readonly IntPtr AppLauncherHandle = new(0x1234);

    /// <summary>Defines a non-zero synthetic monitor handle.</summary>
    private static readonly IntPtr MonitorHandle = new(0x2345);

    /// <summary>Exercises app classification decisions independently of the host operating-system version.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AppClassification_DecisionSeamsCoverOperatingSystemAndWindowOutcomesAsync()
    {
        var coreWindow = CreateWindow(AppQueryExtensions.AppWindowClass);
        var childCoreWindow = CreateWindow(AppQueryExtensions.AppWindowClass);
        var frameWithChild = CreateWindow(AppQueryExtensions.AppFrameWindowClass, childCoreWindow);
        var otherWindow = CreateWindow("CoverageOtherWindow");
        var frameWithoutChild = CreateWindow(AppQueryExtensions.AppFrameWindowClass, otherWindow);
        var win8Calls = 0;
        var win10Calls = 0;
        bool IsWin8App()
        {
            win8Calls++;
            return true;
        }

        bool IsWin10App()
        {
            win10Calls++;
            return true;
        }

        await Assert.That(AppQueryExtensions.IsApp(false, IsWin8App, IsWin10App)).IsFalse();
        await Assert.That(win8Calls).IsEqualTo(Zero);
        await Assert.That(win10Calls).IsEqualTo(Zero);
        await Assert.That(AppQueryExtensions.IsApp(true, static () => true, static () => false)).IsTrue();
        await Assert.That(AppQueryExtensions.IsApp(true, static () => false, static () => true)).IsTrue();
        await Assert.That(AppQueryExtensions.IsApp(true, static () => false, static () => false)).IsFalse();

        await Assert.That(AppQueryExtensions.IsWin10App(false, coreWindow)).IsFalse();
        await Assert.That(AppQueryExtensions.IsWin10App(true, coreWindow)).IsTrue();
        await Assert.That(AppQueryExtensions.IsWin10App(true, frameWithChild)).IsTrue();
        await Assert.That(AppQueryExtensions.IsWin10App(true, frameWithoutChild)).IsFalse();
        await Assert.That(AppQueryExtensions.IsWin8App(false, coreWindow)).IsFalse();
        await Assert.That(AppQueryExtensions.IsWin8App(true, coreWindow)).IsTrue();
        await Assert.That(AppQueryExtensions.IsWin8App(true, otherWindow)).IsFalse();
        await Assert.That(AppQueryExtensions.GetAppWindowIdentifierClass(true)).IsEqualTo(AppQueryExtensions.AppWindowClass);
        await Assert.That(AppQueryExtensions.GetAppWindowIdentifierClass(false)).IsEqualTo(AppQueryExtensions.AppFrameWindowClass);
    }

    /// <summary>Exercises app visibility and launcher composition without native window or COM activation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AppVisibility_CompositionSeamsCoverNullAndAvailableObservationsAsync()
    {
        var findWindowCalls = 0;
        var appVisibility = new FakeAppVisibility(true, MonitorAppVisibility.MAV_APP_VISIBLE);
        using var disposableAppVisibility = new FakeDisposableAppVisibility(appVisibility);

        await Assert.That(AppQueryExtensions.GetAppLauncherHandle(false, () =>
        {
            findWindowCalls++;
            return AppLauncherHandle;
        })).IsEqualTo(IntPtr.Zero);
        await Assert.That(findWindowCalls).IsEqualTo(Zero);
        await Assert.That(AppQueryExtensions.GetAppLauncherHandle(true, () =>
        {
            findWindowCalls++;
            return AppLauncherHandle;
        })).IsEqualTo(AppLauncherHandle);
        await Assert.That(findWindowCalls).IsEqualTo(One);
        await Assert.That(AppQueryExtensions.GetLauncherVisibility(null)).IsFalse();
        await Assert.That(AppQueryExtensions.GetLauncherVisibility(false)).IsFalse();
        await Assert.That(AppQueryExtensions.GetLauncherVisibility(true)).IsTrue();
        await Assert.That(AppQueryExtensions.GetVisibilityOnMonitor(null)).IsNull();

        var getVisibility = AppQueryExtensions.GetVisibilityOnMonitor(appVisibility);
        await Assert.That(getVisibility).IsNotNull();
        await Assert.That(getVisibility!(MonitorHandle)).IsEqualTo(MonitorAppVisibility.MAV_APP_VISIBLE);
        await Assert.That(AppQueryExtensions.GetAppVisibility(null)).IsNull();
        await Assert.That(AppQueryExtensions.GetAppVisibility(disposableAppVisibility)).IsSameReferenceAs(appVisibility);
    }

    /// <summary>Exercises public app query entry points and app-window enumeration without changing operating-system state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AppQuery_PublicEntryPointsReturnTypedResultsWithoutSystemMutationAsync()
    {
        using (AppQueryExtensions.OverridePublicOperationsForTesting(
                   static () => null,
                   static () => Array.Empty<DisplayInfo>(),
                   static (_, _) => IntPtr.Zero))
        {
            var appVisibility = AppQueryExtensions.AppVisible(NativeRect.Empty);
            var launcher = AppQueryExtensions.GetAppLauncher();
            var sawStoreAppsEnumerable = false;
            foreach (IInteropWindow storeApp in AppQueryExtensions.WindowsStoreApps)
            {
                _ = storeApp;
                sawStoreAppsEnumerable = true;
            }

            await Assert.That(appVisibility).IsTrue();
            await Assert.That(launcher).IsNull();
            await Assert.That(sawStoreAppsEnumerable).IsFalse();
        }
    }

    /// <summary>Exercises the environment-message filter with a composed message stream.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task EnvironmentMonitor_ComposedMessageStreamFiltersAndProjectsEnvironmentChangesAsync()
    {
        using var messages = new ManualObservable<WindowMessage>();
        var observer = new CoreInteropCoverageTests.RecordingObserver<EnvironmentChangedEventArgs>();
        using var observedSubscription = EnvironmentMonitor.CreateEnvironmentChangeEvents(messages).Subscribe(observer);

        messages.OnNext(new(0, WindowsMessages.WM_NULL, 0, IntPtr.Zero));
        messages.OnNext(new(0, WindowsMessages.WM_WININICHANGE, (nint)SystemParametersInfoActions.SPI_SETWORKAREA, IntPtr.Zero));

        await Assert.That(observer.Values.Count).IsEqualTo(One);
        await Assert.That(observer.Values[Zero].SystemParametersInfoAction).IsEqualTo(SystemParametersInfoActions.SPI_SETWORKAREA);
    }

    /// <summary>Exercises production registry-wrapper null paths through read-only keys.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InstallationInformation_ProductionRegistryWrapperHandlesMissingChildWithoutMutationAsync()
    {
        var replacement = new EmptyInstalledSoftwareRegistry();
        IInstalledSoftwareRegistry productionRegistry = InstallationInformation.SetRegistryForTesting(replacement);
        try
        {
            using IInstalledSoftwareRegistryKey softwareKey = productionRegistry.OpenLocalMachineSubKey("SOFTWARE");
            await Assert.That(softwareKey).IsNotNull();

            using IInstalledSoftwareRegistryKey missingKey = softwareKey!.OpenSubKey($"CP.ReactiveUI.Primitives.Windows.{Guid.NewGuid():N}");
            await Assert.That(missingKey).IsNull();
        }
        finally
        {
            _ = InstallationInformation.SetRegistryForTesting(productionRegistry);
        }
    }

    /// <summary>Exercises the device registry-value null path without opening a registry key.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DevBroadcastDeviceInterface_MalformedNameUsesSafeRegistryValueFallbackAsync()
    {
        var device = DevBroadcastDeviceInterface.Test("CoverageMalformedDevice");

        await Assert.That(device.FriendlyDeviceName).IsEqualTo("CoverageMalformedDevice");
    }

    /// <summary>Creates an in-memory interop window with the supplied class and children.</summary>
    /// <param name="className">The class name to expose.</param>
    /// <param name="children">The child windows to expose.</param>
    /// <returns>The configured interop window.</returns>
    private static InteropWindow CreateWindow(string className, params IInteropWindow[] children) =>
        new(IntPtr.Zero) { Children = children, Classname = className };

    /// <summary>Provides a deterministic in-memory app visibility implementation.</summary>
    /// <param name="isLauncherVisible">Whether the launcher is visible.</param>
    /// <param name="monitorVisibility">The visibility value to return for every monitor.</param>
#if NET
    [System.Runtime.InteropServices.Marshalling.GeneratedComClass]
#endif
#if NETFRAMEWORK
    private sealed class FakeAppVisibility(bool isLauncherVisible, MonitorAppVisibility monitorVisibility) : IAppVisibility
#else
    private sealed partial class FakeAppVisibility(bool isLauncherVisible, MonitorAppVisibility monitorVisibility) : IAppVisibility
#endif
    {
        /// <inheritdoc />
        public MonitorAppVisibility GetAppVisibilityOnMonitor(IntPtr monitorHandle) => monitorVisibility;

        /// <inheritdoc />
        public bool IsLauncherVisible() => isLauncherVisible;
    }

    /// <summary>Provides a deterministic disposable app-visibility wrapper without COM marshalling.</summary>
    /// <param name="appVisibility">The wrapped visibility implementation.</param>
    private sealed class FakeDisposableAppVisibility(IAppVisibility appVisibility) : IDisposableCom<IAppVisibility>
    {
        /// <inheritdoc />
        public IAppVisibility ComObject => appVisibility;

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }

    /// <summary>Provides a registry reader that is never invoked by the production-wrapper test.</summary>
    private sealed class EmptyInstalledSoftwareRegistry : IInstalledSoftwareRegistry
    {
        /// <inheritdoc />
        public IInstalledSoftwareRegistryKey OpenLocalMachineSubKey(string subkeyName) => null;
    }
}
