// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Win32;
using RegistryValueReader = CP.ReactiveUI.Primitives.Windows.Desktop.RegistryValueReader;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Exercises the final deterministic branches reported by the release coverage gate.</summary>
#if NETFRAMEWORK
public sealed class CoverageFinalExactResidualTests
#else
public sealed partial class CoverageFinalExactResidualTests
#endif
{
    /// <summary>Defines a non-zero source handle captured at subscription time.</summary>
    private const long InitialSourceHandle = 1L;

    /// <summary>Defines the active source handle used after deferred initialization.</summary>
    private const long ActiveSourceHandle = 2L;

    /// <summary>Exercises UTF-16 continuation and shared registry-value outcomes.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeUtf16AndRegistryValueReaderCoverEveryShortCircuitAsync()
    {
        await Assert.That(NativeUtf16String.ReadNullTerminated([0, 1])).IsEqualTo("Ā");
        await Assert.That(NativeUtf16String.ReadNullTerminated([1, 0])).IsEqualTo("\u0001");
        await Assert.That(NativeUtf16String.ReadNullTerminated([0, 0])).IsEqualTo(string.Empty);
        await Assert.That(NativeUtf16String.ReadNullTerminated([1])).IsEqualTo(string.Empty);
        await Assert.That(RegistryValueReader.GetValue(null, "CoverageMissingValue")).IsNull();
        await Assert.That(RegistryValueReader.GetValue(Registry.CurrentUser, $"CP.Reactive.{Guid.NewGuid():N}")).IsNull();
    }

    /// <summary>Exercises public app-query branches through composed dependencies only.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AppQueryPublicOperationsCoverWin8AndAvailableVisibilityBranchesAsync()
    {
        var coreWindow = new InteropWindow(IntPtr.Zero) { Children = [], Classname = AppQueryExtensions.AppWindowClass };

        using (WindowsVersion.OverrideVersionProviderForTesting(static () => new(6, 2)))
        {
            await Assert.That(coreWindow.IsWin8App()).IsTrue();
        }

        using (WindowsVersion.OverrideVersionProviderForTesting(static () => new(6, 3)))
        {
            await Assert.That(coreWindow.IsWin8App()).IsTrue();
        }

        using var appVisibility = new FakeDisposableAppVisibility(new FakeAppVisibility(isLauncherVisible: true));
        using var operations = AppQueryExtensions.OverridePublicOperationsForTesting(
            () => appVisibility,
            static () => Array.Empty<DisplayInfo>(),
            static (_, _) => IntPtr.Zero);

        await Assert.That(AppQueryExtensions.AppVisible(NativeRect.Empty)).IsFalse();
        await Assert.That(AppQueryExtensions.GetAppLauncher()).IsNull();
        await Assert.That(AppQueryExtensions.AppLauncher).IsEqualTo(IntPtr.Zero);

        using var hiddenAppVisibility = new FakeDisposableAppVisibility(new FakeAppVisibility(isLauncherVisible: false));
        using var hiddenOperations = AppQueryExtensions.OverridePublicOperationsForTesting(
            () => hiddenAppVisibility,
            static () => Array.Empty<DisplayInfo>(),
            static (_, _) => new(1));
        await Assert.That(AppQueryExtensions.GetAppLauncher()).IsNotNull();
    }

    /// <summary>Exercises nullable parent and production registry lookup outcomes without mutation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowParentAndProductionRegistryCoverNullOutcomesAsync()
    {
        var window = new InteropWindow(IntPtr.Zero);
        await Assert.That(window.HasParent).IsFalse();
        window.Parent = IntPtr.Zero;
        await Assert.That(window.HasParent).IsFalse();
        window.Parent = new(1);
        await Assert.That(window.HasParent).IsTrue();

        var replacement = new EmptyInstalledSoftwareRegistry();
        IInstalledSoftwareRegistry productionRegistry = InstallationInformation.SetRegistryForTesting(replacement);
        try
        {
            using IInstalledSoftwareRegistryKey missingKey = productionRegistry.OpenLocalMachineSubKey(
                $"SOFTWARE\\CP.Reactive.{Guid.NewGuid():N}");
            await Assert.That(missingKey).IsNull();
        }
        finally
        {
            _ = InstallationInformation.SetRegistryForTesting(productionRegistry);
        }
    }

    /// <summary>Exercises both outcomes of the window-lifecycle event projection.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowLifecycleProjectionFiltersNonWindowObjectsAsync()
    {
        using var source = new ManualObservable<WinEventInfo>();
        var observer = new CoreInteropCoverageTests.RecordingObserver<WinEventInfo>();
        using var subscription = WinEventHook.ObserveWindowObjects(source).Subscribe(observer);

        source.OnNext(WinEventInfo.Create(IntPtr.Zero, WinEvents.EVENT_OBJECT_CREATE, IntPtr.Zero, ObjectIdentifiers.Client, 0, 0, 0));
        source.OnNext(WinEventInfo.Create(IntPtr.Zero, WinEvents.EVENT_OBJECT_DESTROY, IntPtr.Zero, ObjectIdentifiers.Window, 0, 0, 0));

        await Assert.That(observer.Values.Count).IsEqualTo(1);
        await Assert.That(observer.Values[0].ObjectIdentifier).IsEqualTo(ObjectIdentifiers.Window);

        await Assert.That(WinProcWindowsExtensions.SelectSourceHandle(InitialSourceHandle, ActiveSourceHandle)).IsEqualTo(InitialSourceHandle);
        await Assert.That(WinProcWindowsExtensions.SelectSourceHandle(0L, ActiveSourceHandle)).IsEqualTo(ActiveSourceHandle);
    }

    /// <summary>Provides a deterministic app-visibility implementation.</summary>
    /// <param name="isLauncherVisible">Whether the synthetic app launcher is visible.</param>
#if NET
    [System.Runtime.InteropServices.Marshalling.GeneratedComClass]
#endif
#if NETFRAMEWORK
    private sealed class FakeAppVisibility(bool isLauncherVisible) : IAppVisibility
#else
    private sealed partial class FakeAppVisibility(bool isLauncherVisible) : IAppVisibility
#endif
    {
        /// <inheritdoc />
        public MonitorAppVisibility GetAppVisibilityOnMonitor(IntPtr monitorHandle) => MonitorAppVisibility.MAV_APP_VISIBLE;

        /// <inheritdoc />
        public bool IsLauncherVisible() => isLauncherVisible;
    }

    /// <summary>Provides a deterministic disposable app-visibility wrapper.</summary>
    /// <param name="appVisibility">The wrapped app visibility object.</param>
    private sealed class FakeDisposableAppVisibility(IAppVisibility appVisibility) : IDisposableCom<IAppVisibility>
    {
        /// <inheritdoc />
        public IAppVisibility ComObject => appVisibility;

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }

    /// <summary>Provides a registry implementation used only to reveal the production reader.</summary>
    private sealed class EmptyInstalledSoftwareRegistry : IInstalledSoftwareRegistry
    {
        /// <inheritdoc />
        public IInstalledSoftwareRegistryKey OpenLocalMachineSubKey(string subkeyName) => null;
    }
}
