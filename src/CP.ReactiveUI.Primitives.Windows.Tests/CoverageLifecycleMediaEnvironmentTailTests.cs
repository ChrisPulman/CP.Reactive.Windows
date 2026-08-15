// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Coverage for lifecycle, media, and environment tail branches.</summary>
public sealed class CoverageLifecycleMediaEnvironmentTailTests
{
    /// <summary>Executable argument value.</summary>
    private const string ExecutableArgument = "app.exe";

    /// <summary>Restart argument value.</summary>
    private const string RestartArgument = "/restart";

    /// <summary>Single-dash restart argument value.</summary>
    private const string SingleDashRestartArgument = "-restart";

    /// <summary>Double-dash restart argument value.</summary>
    private const string DoubleDashRestartArgument = "--restart";

    /// <summary>Non-restart argument value.</summary>
    private const string OtherArgument = "--other";

    /// <summary>Environment change area value.</summary>
    private const string EnvironmentArea = "Environment";

    /// <summary>Exercises restart argument helpers without native restart registration.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RestartArgumentHelpersCoverEmptyCopyAndMarkerBranchesAsync()
    {
        var noArguments = Array.Empty<string>();
        var executableOnly = new[] { ExecutableArgument };
        var copiedArguments = ApplicationRestartManager.GetRestartCommandLineArgs([ExecutableArgument, OtherArgument, DoubleDashRestartArgument]);

        await Assert.That(ApplicationRestartManager.GetRestartCommandLineArgs(noArguments)).IsEmpty();
        await Assert.That(ApplicationRestartManager.GetRestartCommandLineArgs(executableOnly)).IsEmpty();
        await Assert.That(copiedArguments.Length).IsEqualTo(Two);
        await Assert.That(copiedArguments[0]).IsEqualTo(OtherArgument);
        await Assert.That(copiedArguments[1]).IsEqualTo(DoubleDashRestartArgument);
        await Assert.That(ApplicationRestartManager.WasRestartRequested([ExecutableArgument, OtherArgument])).IsFalse();
        await Assert.That(ApplicationRestartManager.WasRestartRequested([ExecutableArgument, RestartArgument])).IsTrue();
        await Assert.That(ApplicationRestartManager.WasRestartRequested([ExecutableArgument, SingleDashRestartArgument])).IsTrue();
        await Assert.That(ApplicationRestartManager.WasRestartRequested([ExecutableArgument, DoubleDashRestartArgument])).IsTrue();
    }

    /// <summary>Exercises renamed end-session observable construction without publishing shutdown messages.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ObserveEndSessionMessagesCreatesSharedObservableOnSubscriptionAsync()
    {
        ApplicationRestartManager.ResetEndSessionMessagesForTesting();
        var observer = new CoreInteropCoverageTests.RecordingObserver<EndSessionMessage>();

        using (ApplicationRestartManager.ObserveEndSessionMessages().Subscribe(observer))
        {
            await Assert.That(observer.Values).IsEmpty();
        }

        ApplicationRestartManager.ResetEndSessionMessagesForTesting();
        await Assert.That(ApplicationRestartManager.ObserveEndSessionMessages(static _ => true)).IsNotNull();
        await Assert.That(ApplicationRestartManager.ObserveEndSessionMessages(static _ => true, static _ => false)).IsNotNull();
    }

    /// <summary>Exercises environment message mapping without broadcasting a system setting change.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task EnvironmentChangeMessageMappingReturnsActionAndAreaAsync()
    {
        var areaPointer = Marshal.StringToHGlobalAuto(EnvironmentArea);
        try
        {
            var message = new WindowMessage(
                0,
                WindowsMessages.WM_SETTINGCHANGE,
                (nint)SystemParametersInfoActions.SPI_SETWORKAREA,
                areaPointer);
            var args = EnvironmentMonitor.CreateChangedEventArgs(message);

            await Assert.That(args.SystemParametersInfoAction).IsEqualTo(SystemParametersInfoActions.SPI_SETWORKAREA);
            await Assert.That(args.Area).IsEqualTo(EnvironmentArea);
            await Assert.That(EnvironmentMonitor.EnvironmentChangeEvents).IsNotNull();
        }
        finally
        {
            Marshal.FreeHGlobal(areaPointer);
        }
    }
}
