// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Kernel;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Final coverage tests for Restart Manager composition paths.</summary>
public sealed class CoverageFinalRestartManagerTests
{
    /// <summary>Defines a successful Win32 result.</summary>
    private const int Success = 0;

    /// <summary>Defines a synthetic Win32 failure result.</summary>
    private const int Failure = 5;

    /// <summary>Defines the native Restart Manager session flags used by tests.</summary>
    private const int SessionFlags = 7;

    /// <summary>Defines the native restart flags used by tests.</summary>
    private const int RestartFlags = 9;

    /// <summary>Defines the synthetic Restart Manager session handle.</summary>
    private const int SessionHandle = 52;

    /// <summary>Defines a synthetic process identifier.</summary>
    private const int ProcessId = 4_321;

    /// <summary>Defines a synthetic Terminal Services session identifier.</summary>
    private const uint TerminalSessionId = 8;

    /// <summary>Defines the native call progress value.</summary>
    private const uint NativeProgress = 35;

    /// <summary>Defines the shutdown progress value.</summary>
    private const uint ShutdownProgress = 45;

    /// <summary>Defines the restart progress value.</summary>
    private const uint RestartProgress = 90;

    /// <summary>Defines an empty count.</summary>
    private const int EmptyCount = 0;

    /// <summary>Defines the first item index.</summary>
    private const int FirstItemIndex = 0;

    /// <summary>Defines a single item count.</summary>
    private const int SingleCount = 1;

    /// <summary>Defines a single native item count.</summary>
    private const uint SingleNativeCount = 1;

    /// <summary>Defines an empty native item count.</summary>
    private const uint EmptyNativeCount = 0;

    /// <summary>Defines an oversized returned process count.</summary>
    private const uint OversizedProcessCount = 2;

    /// <summary>Defines a test session key.</summary>
    private const string SessionKey = "coverage-final-session";

    /// <summary>Defines a test file name.</summary>
    private const string FileName = @"C:\Temp\coverage-final.txt";

    /// <summary>Defines a test service name.</summary>
    private const string ServiceName = "CoverageService";

    /// <summary>Defines a test application name.</summary>
    private const string ApplicationName = "CoverageApp";

    /// <summary>Defines a test service short name.</summary>
    private const string ServiceShortName = "CoverageSvc";

    /// <summary>Tests that empty registration inputs are no-ops and disposal is idempotent.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RestartManager_EmptyRegistrationsAndDisposedGuardsAreCoveredAsync()
    {
        var api = new RestartSessionApiDouble();
        var manager = RestartManager.CreateSession(api);
        string[] missingNames = null;
        RmUniqueProcess[] missingProcesses = null;

        manager.RegisterFiles(missingNames);
        manager.RegisterFiles([]);
        manager.RegisterProcesses(missingProcesses);
        manager.RegisterProcesses([]);
        manager.RegisterServices(missingNames);
        manager.RegisterServices([]);

        await Assert.That(api.RegisterResourcesCount).IsEqualTo(EmptyCount);

        manager.Dispose();
        manager.Dispose();
        var disposedProcess = new RmUniqueProcess(ProcessId, default);

        await Assert.That(api.EndSessionCount).IsEqualTo(SingleCount);
        await Assert.That(() => manager.RegisterFile(FileName)).Throws<ObjectDisposedException>();
        await Assert.That(() => manager.RegisterProcesses(disposedProcess)).Throws<ObjectDisposedException>();
        await Assert.That(() => manager.RegisterServices(ServiceName)).Throws<ObjectDisposedException>();
        await Assert.That(() => manager.GetProcessesUsingResources()).Throws<ObjectDisposedException>();
        await Assert.That(() => manager.Shutdown()).Throws<ObjectDisposedException>();
        await Assert.That(() => manager.Restart()).Throws<ObjectDisposedException>();
    }

    /// <summary>Tests zero-process reboot state branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RestartManager_ZeroProcessQueriesReturnEmptyAndNoRebootAsync()
    {
        var api = new RestartSessionApiDouble { FirstGetListResult = Success };

        using var manager = RestartManager.CreateSession(api);

        var processes = manager.GetProcessesUsingResources();
        var processesWithReason = manager.GetProcessesUsingResources(out var rebootReason);

        await Assert.That(processes.Count).IsEqualTo(EmptyCount);
        await Assert.That(processesWithReason.Count).IsEqualTo(EmptyCount);
        await Assert.That(rebootReason).IsEqualTo(RmRebootReason.None);
        await Assert.That(manager.IsRebootRequired()).IsFalse();
        await Assert.That(manager.GetRebootReason()).IsEqualTo(RmRebootReason.None);
    }

    /// <summary>Tests that Restart Manager truncates native process counts to the managed buffer length.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RestartManager_ProcessListCreationTruncatesOversizedNativeCountAsync()
    {
        var api = new RestartSessionApiDouble { ReturnedProcessCount = OversizedProcessCount };
        api.Processes.Add(CreateProcessInfo());

        using var manager = RestartManager.CreateSession(api);

        var processes = manager.GetProcessesUsingResources(out var rebootReason);

        await Assert.That(processes.Count).IsEqualTo(SingleCount);
        await Assert.That(processes[FirstItemIndex].Process.ProcessId).IsEqualTo(ProcessId);
        await Assert.That(rebootReason).IsEqualTo(RmRebootReason.None);

        var emptyReturnedApi = new RestartSessionApiDouble { ReturnedProcessCount = EmptyNativeCount };
        emptyReturnedApi.Processes.Add(CreateProcessInfo());

        using var emptyReturnedManager = RestartManager.CreateSession(emptyReturnedApi);

        var emptyReturnedProcesses = emptyReturnedManager.GetProcessesUsingResources();

        await Assert.That(emptyReturnedProcesses.Count).IsEqualTo(EmptyCount);
    }

    /// <summary>Tests observable success and error delivery for Restart Manager progress APIs.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RestartManager_ProgressObservablesCompleteAndForwardErrorsAsync()
    {
        var successApi = new RestartSessionApiDouble();

        using var successManager = RestartManager.CreateSession(successApi);
        var shutdownObserver = new CollectingObserver<uint>();
        var restartObserver = new CollectingObserver<uint>();

        using var shutdownSubscription = successManager.ObserveShutdownProgress().Subscribe(shutdownObserver);
        using var restartSubscription = successManager.ObserveRestartProgress().Subscribe(restartObserver);

        await Assert.That(shutdownObserver.Values).Contains(ShutdownProgress);
        await Assert.That(restartObserver.Values).Contains(RestartProgress);
        await Assert.That(shutdownObserver.Completed).IsTrue();
        await Assert.That(restartObserver.Completed).IsTrue();
        await Assert.That(shutdownObserver.Error).IsNull();
        await Assert.That(restartObserver.Error).IsNull();

        var failureApi = new RestartSessionApiDouble { ShutdownResult = Failure, RestartResult = Failure };

        using var failureManager = RestartManager.CreateSession(failureApi);
        var shutdownErrorObserver = new CollectingObserver<uint>();
        var restartErrorObserver = new CollectingObserver<uint>();

        using var shutdownErrorSubscription = failureManager.ObserveShutdownProgress().Subscribe(shutdownErrorObserver);
        using var restartErrorSubscription = failureManager.ObserveRestartProgress().Subscribe(restartErrorObserver);

        await Assert.That(shutdownErrorObserver.Error).IsNotNull();
        await Assert.That(restartErrorObserver.Error).IsNotNull();
        await Assert.That(shutdownErrorObserver.Completed).IsFalse();
        await Assert.That(restartErrorObserver.Completed).IsFalse();
    }

    /// <summary>Tests deterministic forwarding in the composable native Restart Manager session API.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeRestartManagerSessionApi_ForwardsAllOperationsAsync()
    {
        var recorder = new NativeCallRecorder();
        var api = new NativeRestartManagerSessionApi(
            recorder.StartSession,
            recorder.EndSession,
            recorder.RegisterResources,
            recorder.GetList,
            recorder.Shutdown,
            recorder.Restart);
        var sessionKey = new StringBuilder();
        var filenames = new[] { FileName };
        var applications = new[] { new RmUniqueProcess(ProcessId, default) };
        var services = new[] { ServiceName };
        var affectedApplications = new RmProcessInfo[SingleCount];
        uint processInfoCount = SingleNativeCount;

        var startResult = api.StartSession(out var sessionHandle, SessionFlags, sessionKey);
        var registerResult = api.RegisterResources(
            sessionHandle,
            (uint)filenames.Length,
            filenames,
            (uint)applications.Length,
            applications,
            (uint)services.Length,
            services);
        var listResult = api.GetList(sessionHandle, out var processInfoNeeded, ref processInfoCount, affectedApplications, out var rebootReasons);
        var shutdownResult = api.Shutdown(sessionHandle, RmShutdownType.RmShutdownOnlyRegistered, recorder.RecordNativeProgress);
        var restartResult = api.Restart(sessionHandle, RestartFlags, recorder.RecordNativeProgress);
        var endResult = api.EndSession(sessionHandle);

        await Assert.That(startResult).IsEqualTo(Success);
        await Assert.That(registerResult).IsEqualTo(Success);
        await Assert.That(listResult).IsEqualTo(Success);
        await Assert.That(shutdownResult).IsEqualTo(Success);
        await Assert.That(restartResult).IsEqualTo(Success);
        await Assert.That(endResult).IsEqualTo(Success);
        await Assert.That(sessionHandle).IsEqualTo(SessionHandle);
        await Assert.That(sessionKey.ToString()).IsEqualTo(SessionKey);
        await Assert.That(processInfoNeeded).IsEqualTo(SingleNativeCount);
        await Assert.That(processInfoCount).IsEqualTo(SingleNativeCount);
        await Assert.That(rebootReasons).IsEqualTo(RmRebootReason.RmRebootReasonDetectedSelf);
        await Assert.That(affectedApplications[FirstItemIndex].ApplicationName).IsEqualTo(ApplicationName);
        await Assert.That(recorder.NativeProgressValues).Contains(NativeProgress);
        await Assert.That(recorder.StartSessionCount).IsEqualTo(SingleCount);
        await Assert.That(recorder.RegisterResourcesCount).IsEqualTo(SingleCount);
        await Assert.That(recorder.GetListCount).IsEqualTo(SingleCount);
        await Assert.That(recorder.ShutdownCount).IsEqualTo(SingleCount);
        await Assert.That(recorder.RestartCount).IsEqualTo(SingleCount);
        await Assert.That(recorder.EndSessionCount).IsEqualTo(SingleCount);
    }

    /// <summary>Creates synthetic Restart Manager process info.</summary>
    /// <returns>The process information.</returns>
    private static RmProcessInfo CreateProcessInfo() =>
        new(
            new(ProcessId, default),
            ApplicationName,
            ServiceShortName,
            RmAppType.RmConsole,
            RmAppStatus.RmStatusRunning,
            TerminalSessionId,
            restartable: true);

    /// <summary>Collects observable notifications for assertion.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    private sealed class CollectingObserver<T> : IObserver<T>
    {
        /// <summary>Gets the observed values.</summary>
        public List<T> Values { get; } = [];

        /// <summary>Gets the observed terminal error.</summary>
        public Exception Error { get; private set; }

        /// <summary>Gets a value indicating whether completion was observed.</summary>
        public bool Completed { get; private set; }

        /// <inheritdoc/>
        public void OnCompleted() => Completed = true;

        /// <inheritdoc/>
        public void OnError(Exception error) => Error = error;

        /// <inheritdoc/>
        public void OnNext(T value) => Values.Add(value);
    }

    /// <summary>Records native wrapper calls.</summary>
    private sealed class NativeCallRecorder
    {
        /// <summary>Gets the native progress values.</summary>
        public List<uint> NativeProgressValues { get; } = [];

        /// <summary>Gets the start-session call count.</summary>
        public int StartSessionCount { get; private set; }

        /// <summary>Gets the register-resources call count.</summary>
        public int RegisterResourcesCount { get; private set; }

        /// <summary>Gets the get-list call count.</summary>
        public int GetListCount { get; private set; }

        /// <summary>Gets the shutdown call count.</summary>
        public int ShutdownCount { get; private set; }

        /// <summary>Gets the restart call count.</summary>
        public int RestartCount { get; private set; }

        /// <summary>Gets the end-session call count.</summary>
        public int EndSessionCount { get; private set; }

        /// <summary>Starts a Restart Manager session.</summary>
        /// <param name="sessionHandle">Restart Manager session handle.</param>
        /// <param name="sessionFlags">Reserved session flags.</param>
        /// <param name="sessionKey">Session key output buffer.</param>
        /// <returns>Win32 result code.</returns>
        public int StartSession(out int sessionHandle, int sessionFlags, StringBuilder sessionKey)
        {
            _ = sessionFlags;
            StartSessionCount++;
            sessionHandle = SessionHandle;
            _ = sessionKey.Append(SessionKey);
            return Success;
        }

        /// <summary>Ends a Restart Manager session.</summary>
        /// <param name="sessionHandle">Restart Manager session handle.</param>
        /// <returns>Win32 result code.</returns>
        public int EndSession(int sessionHandle)
        {
            _ = sessionHandle;
            EndSessionCount++;
            return Success;
        }

        /// <summary>Registers resources with a Restart Manager session.</summary>
        /// <param name="sessionHandle">Restart Manager session handle.</param>
        /// <param name="fileCount">Number of file names.</param>
        /// <param name="filenames">File names.</param>
        /// <param name="applicationCount">Number of process identities.</param>
        /// <param name="applications">Process identities.</param>
        /// <param name="serviceCount">Number of service names.</param>
        /// <param name="serviceNames">Service names.</param>
        /// <returns>Win32 result code.</returns>
        public int RegisterResources(
            int sessionHandle,
            uint fileCount,
            string[] filenames,
            uint applicationCount,
            RmUniqueProcess[] applications,
            uint serviceCount,
            string[] serviceNames)
        {
            _ = sessionHandle;
            _ = fileCount;
            _ = filenames;
            _ = applicationCount;
            _ = applications;
            _ = serviceCount;
            _ = serviceNames;
            RegisterResourcesCount++;
            return Success;
        }

        /// <summary>Gets affected applications for a Restart Manager session.</summary>
        /// <param name="sessionHandle">Restart Manager session handle.</param>
        /// <param name="processInfoNeeded">Required process-info count.</param>
        /// <param name="processInfoCount">Supplied process-info count.</param>
        /// <param name="affectedApplications">Affected applications buffer.</param>
        /// <param name="rebootReasons">Reboot reason flags.</param>
        /// <returns>Win32 result code.</returns>
        public int GetList(
            int sessionHandle,
            out uint processInfoNeeded,
            ref uint processInfoCount,
            RmProcessInfo[] affectedApplications,
            out RmRebootReason rebootReasons)
        {
            _ = sessionHandle;
            GetListCount++;
            processInfoNeeded = SingleNativeCount;
            processInfoCount = SingleNativeCount;
            rebootReasons = RmRebootReason.RmRebootReasonDetectedSelf;
            affectedApplications[FirstItemIndex] = CreateProcessInfo();
            return Success;
        }

        /// <summary>Shuts down affected applications.</summary>
        /// <param name="sessionHandle">Restart Manager session handle.</param>
        /// <param name="shutdownType">Shutdown type.</param>
        /// <param name="statusCallback">Status callback.</param>
        /// <returns>Win32 result code.</returns>
        public int Shutdown(int sessionHandle, RmShutdownType shutdownType, RmStatusCallback statusCallback)
        {
            _ = sessionHandle;
            _ = shutdownType;
            ShutdownCount++;
            statusCallback(NativeProgress);
            return Success;
        }

        /// <summary>Restarts affected applications.</summary>
        /// <param name="sessionHandle">Restart Manager session handle.</param>
        /// <param name="restartFlags">Reserved restart flags.</param>
        /// <param name="statusCallback">Status callback.</param>
        /// <returns>Win32 result code.</returns>
        public int Restart(int sessionHandle, int restartFlags, RmStatusCallback statusCallback)
        {
            _ = sessionHandle;
            _ = restartFlags;
            RestartCount++;
            statusCallback(NativeProgress);
            return Success;
        }

        /// <summary>Records native progress.</summary>
        /// <param name="progress">The native progress value.</param>
        public void RecordNativeProgress(uint progress) => NativeProgressValues.Add(progress);
    }

    /// <summary>Fake Restart Manager API for deterministic high-level tests.</summary>
    private sealed class RestartSessionApiDouble : IRestartManagerSessionApi
    {
        /// <summary>Defines the Restart Manager ERROR_MORE_DATA result.</summary>
        private const int ErrorMoreData = 234;

        /// <summary>Gets or sets the first get-list result.</summary>
        public int FirstGetListResult { get; init; } = ErrorMoreData;

        /// <summary>Gets or sets the second get-list result.</summary>
        public int SecondGetListResult { get; init; }

        /// <summary>Gets or sets the shutdown result.</summary>
        public int ShutdownResult { get; init; }

        /// <summary>Gets or sets the restart result.</summary>
        public int RestartResult { get; init; }

        /// <summary>Gets or sets the returned process count.</summary>
        public uint? ReturnedProcessCount { get; init; }

        /// <summary>Gets synthetic processes.</summary>
        public List<RmProcessInfo> Processes { get; } = [];

        /// <summary>Gets the register-resources call count.</summary>
        public int RegisterResourcesCount { get; private set; }

        /// <summary>Gets the end-session call count.</summary>
        public int EndSessionCount { get; private set; }

        /// <inheritdoc/>
        public int StartSession(out int sessionHandle, int sessionFlags, StringBuilder sessionKey)
        {
            sessionHandle = SessionHandle;
            _ = sessionKey.Append(SessionKey);
            return Success;
        }

        /// <inheritdoc/>
        public int EndSession(int sessionHandle)
        {
            EndSessionCount++;
            return Success;
        }

        /// <inheritdoc/>
        public int RegisterResources(
            int sessionHandle,
            uint fileCount,
            string[] filenames,
            uint applicationCount,
            RmUniqueProcess[] applications,
            uint serviceCount,
            string[] serviceNames)
        {
            RegisterResourcesCount++;
            return Success;
        }

        /// <inheritdoc/>
        public int GetList(
            int sessionHandle,
            out uint processInfoNeeded,
            ref uint processInfoCount,
            RmProcessInfo[] affectedApplications,
            out RmRebootReason rebootReasons)
        {
            rebootReasons = RmRebootReason.None;
            processInfoNeeded = (uint)Processes.Count;

            if (affectedApplications is null)
            {
                return FirstGetListResult;
            }

            for (var i = 0; i < affectedApplications.Length && i < Processes.Count; i++)
            {
                affectedApplications[i] = Processes[i];
            }

            processInfoCount = ReturnedProcessCount ?? (uint)Math.Min(affectedApplications.Length, Processes.Count);
            return SecondGetListResult;
        }

        /// <inheritdoc/>
        public int Shutdown(int sessionHandle, RmShutdownType shutdownType, RmStatusCallback statusCallback)
        {
            statusCallback?.Invoke(ShutdownProgress);
            return ShutdownResult;
        }

        /// <inheritdoc/>
        public int Restart(int sessionHandle, int restartFlags, RmStatusCallback statusCallback)
        {
            statusCallback?.Invoke(RestartProgress);
            return RestartResult;
        }
    }
}
