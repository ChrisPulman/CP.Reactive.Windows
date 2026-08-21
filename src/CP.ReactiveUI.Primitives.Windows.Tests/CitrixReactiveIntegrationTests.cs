// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Enums;
using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Host;
using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Ipc;
using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Lifecycle;
using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Telemetry;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Citrix reactive integration surfaces.</summary>
public partial class CitrixTests
{
    /// <summary>The deterministic lifecycle session id.</summary>
    private const int LifecycleSessionId = 42;

    /// <summary>The deterministic lifecycle window handle.</summary>
    private const int LifecycleWindowHandle = 100;

    /// <summary>The deterministic lifecycle window id.</summary>
    private const int LifecycleWindowId = 7;

    /// <summary>The deterministic primary virtual channel value.</summary>
    private const int IpcChannelValue = 1234;

    /// <summary>The deterministic secondary virtual channel value.</summary>
    private const int IpcOtherChannelValue = 5678;

    /// <summary>The first byte in the IPC data payload.</summary>
    private const byte IpcDataFirstByte = 4;

    /// <summary>The second byte in the IPC data payload.</summary>
    private const byte IpcDataSecondByte = 5;

    /// <summary>The byte used to mutate source buffers after construction.</summary>
    private const byte MutatedByte = 99;

    /// <summary>The first byte of the original write payload.</summary>
    private const byte OriginalPayloadFirstByte = 1;

    /// <summary>The second byte of the original write payload.</summary>
    private const byte OriginalPayloadSecondByte = 2;

    /// <summary>The third byte of the original write payload.</summary>
    private const byte OriginalPayloadThirdByte = 3;

    /// <summary>The first byte of the original metadata payload.</summary>
    private const byte OriginalMetadataFirstByte = 9;

    /// <summary>The second byte of the original metadata payload.</summary>
    private const byte OriginalMetadataSecondByte = 8;

    /// <summary>The expected write payload length.</summary>
    private const int ExpectedPayloadLength = 3;

    /// <summary>The number of adapter close calls needed before the channel is closed.</summary>
    private const int ExpectedCloseAttemptCount = 3;

    /// <summary>The close attempt that returns a failed result.</summary>
    private const int FailedCloseAttempt = 2;

    /// <summary>The deterministic host session id.</summary>
    private const int HostSessionId = 77;

    /// <summary>The deterministic host window handle value.</summary>
    private const int HostWindowHandleValue = 99;

    /// <summary>The deterministic failed host window handle value.</summary>
    private const int HostFailureWindowHandleValue = 100;

    /// <summary>The deterministic failed CCM result code.</summary>
    private const int FailedCcmResultCode = 5;

    /// <summary>The deterministic successful WTS last error value.</summary>
    private const int HostLastError = 123;

    /// <summary>The deterministic failed WTS last error value.</summary>
    private const int HostFailureLastError = 321;

    /// <summary>The telemetry polling interval in milliseconds.</summary>
    private const int TelemetryPollingMilliseconds = 100;

    /// <summary>The telemetry wait timeout in seconds.</summary>
    private const int TelemetryTimeoutSeconds = 5;

    /// <summary>The shared server name used by Citrix tests.</summary>
    private const string ServerName = "server";

    /// <summary>Verifies lifecycle filters expose the requested typed Citrix event streams.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task LifecycleFilters_ReturnOnlyMatchingEventsAsync()
    {
        using ManualObservable<CitrixLifecycleEvent> events = new();
        var source = new CitrixObservableLifecycleEventSource(events);
        var session = new CitrixSessionInfo(LifecycleSessionId, ConnectStates.Active, "user", "domain", "client", "127.0.0.1");
        var window = new CitrixWindowInfo(LifecycleWindowHandle, LifecycleWindowId, "Published App", "ICASeamless");
        var timestamp = new DateTimeOffset(2026, 8, 21, 8, 0, 0, TimeSpan.Zero);
        var connects = new List<CitrixConnectEvent>();
        var disconnects = new List<CitrixDisconnectEvent>();
        var logins = new List<CitrixLoginEvent>();
        var windowCreates = new List<CitrixWindowCreatedEvent>();
        var windowDestroys = new List<CitrixWindowDestroyedEvent>();
        var windowDistroyAlias = new List<CitrixWindowDestroyedEvent>();
        var icaParses = new List<CitrixICAFileParseEvent>();
        var stateChanges = new List<CitrixSessionStateChangeEvent>();

        using var connectSubscription = source.OnConnect().SubscribeOnNext(connects.Add);
        using var disconnectSubscription = source.OnDisconnect().SubscribeOnNext(disconnects.Add);
        using var loginSubscription = source.OnLogin().SubscribeOnNext(logins.Add);
        using var windowCreateSubscription = source.OnWindowCreated().SubscribeOnNext(windowCreates.Add);
        using var windowDestroySubscription = source.OnWindowDestroyed().SubscribeOnNext(windowDestroys.Add);
        using var windowDistroyAliasSubscription = source.OnWindowDistroyed().SubscribeOnNext(windowDistroyAlias.Add);
        using var icaParseSubscription = source.OnICAFileParse().SubscribeOnNext(icaParses.Add);
        using var stateChangeSubscription = source.OnSessionStateChange().SubscribeOnNext(stateChanges.Add);

        events.OnNext(new CitrixConnectEvent(session, timestamp));
        events.OnNext(new CitrixDisconnectEvent(session, timestamp));
        events.OnNext(new CitrixLoginEvent(session, timestamp));
        events.OnNext(new CitrixWindowCreatedEvent(window, timestamp));
        events.OnNext(new CitrixWindowDestroyedEvent(window, timestamp));
        events.OnNext(new CitrixICAFileParseEvent(new("launch.ica", new Dictionary<string, string> { ["Address"] = ServerName }), timestamp));
        events.OnNext(new CitrixSessionStateChangeEvent(session, ConnectStates.Connected, ConnectStates.Active, timestamp));

        await Assert.That(connects.Count).IsEqualTo(1);
        await Assert.That(connects[0].Session.SessionId).IsEqualTo(LifecycleSessionId);
        await Assert.That(disconnects.Count).IsEqualTo(1);
        await Assert.That(logins.Count).IsEqualTo(1);
        await Assert.That(windowCreates.Count).IsEqualTo(1);
        await Assert.That(windowDestroys.Count).IsEqualTo(1);
        await Assert.That(windowDistroyAlias.Count).IsEqualTo(1);
        await Assert.That(icaParses.Count).IsEqualTo(1);
        await Assert.That(icaParses[0].ICAFile.Properties["Address"]).IsEqualTo(ServerName);
        await Assert.That(stateChanges.Count).IsEqualTo(1);
        await Assert.That(stateChanges[0].CurrentState).IsEqualTo(ConnectStates.Active);
    }

    /// <summary>Verifies only WFAPI-backed mappings are exposed by the static default lifecycle surface.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task LifecycleDefaults_ExposeOnlySupportedMappingsAsync()
    {
        var parameterlessMethods = new HashSet<string>(StringComparer.Ordinal);
        foreach (var method in typeof(CitrixLifecycleObservables).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
        {
            if (method.GetParameters().Length == 0)
            {
                _ = parameterlessMethods.Add(method.Name);
            }
        }

        var sourceMethods = new HashSet<string>(StringComparer.Ordinal);
        foreach (var method in typeof(CitrixLifecycleExtensions).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
        {
            _ = sourceMethods.Add(method.Name);
        }

        await Assert.That(parameterlessMethods.Contains(nameof(CitrixLifecycleObservables.OnConnect))).IsTrue();
        await Assert.That(parameterlessMethods.Contains(nameof(CitrixLifecycleObservables.OnDisconnect))).IsTrue();
        await Assert.That(parameterlessMethods.Contains(nameof(CitrixLifecycleObservables.OnLogin))).IsTrue();
        await Assert.That(parameterlessMethods.Contains(nameof(CitrixLifecycleObservables.OnSessionStateChange))).IsTrue();
        await Assert.That(parameterlessMethods.Contains("OnWindowCreated")).IsFalse();
        await Assert.That(parameterlessMethods.Contains("OnWindowDestroyed")).IsFalse();
        await Assert.That(parameterlessMethods.Contains("OnWindowDistroyed")).IsFalse();
        await Assert.That(parameterlessMethods.Contains("OnICAFileParse")).IsFalse();
        await Assert.That(sourceMethods.Contains("OnWindowCreated")).IsTrue();
        await Assert.That(sourceMethods.Contains("OnWindowDestroyed")).IsTrue();
        await Assert.That(sourceMethods.Contains("OnWindowDistroyed")).IsTrue();
        await Assert.That(sourceMethods.Contains("OnICAFileParse")).IsTrue();
    }

    /// <summary>Verifies IPC observables call injected virtual-driver callbacks and close sessions once.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task VirtualChannelIpc_UsesInjectedAdapterAndFiltersByChannelAsync()
    {
        using var adapter = new TestVirtualDriverAdapter();
        var channel = new CitrixVirtualChannelHandle(IpcChannelValue);
        var otherChannel = new CitrixVirtualChannelHandle(IpcOtherChannelValue);
        var data = new List<CitrixVirtualChannelData>();
        var events = new List<CitrixVirtualChannelEvent>();
        var sessionSource = new TaskCompletionSource<CitrixVirtualChannelSession>(TaskCreationOptions.RunContinuationsAsynchronously);
        var writeSource = new TaskCompletionSource<CitrixVirtualChannelWriteResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var featureSource = new TaskCompletionSource<CitrixVirtualChannelFeatureRegistration>(TaskCreationOptions.RunContinuationsAsynchronously);
        byte[] payload = [OriginalPayloadFirstByte, OriginalPayloadSecondByte, OriginalPayloadThirdByte];
        byte[] metadata = [OriginalMetadataFirstByte, OriginalMetadataSecondByte];

        using var openSubscription = CitrixVirtualChannelIpc
            .DriverOpen(adapter, new("CTXTEST"))
            .Subscribe(new TaskCompletionObserver<CitrixVirtualChannelSession>(sessionSource));
        using var dataSubscription = CitrixVirtualChannelIpc
            .ObserveIncomingData(adapter, channel)
            .SubscribeOnNext(data.Add);
        using var eventSubscription = CitrixVirtualChannelIpc
            .ObserveEvents(adapter, channel)
            .SubscribeOnNext(events.Add);
        using var writeSubscription = CitrixVirtualChannelIpc
            .DriverWrite(adapter, new(channel, payload))
            .Subscribe(new TaskCompletionObserver<CitrixVirtualChannelWriteResult>(writeSource));
        using var featureSubscription = CitrixVirtualChannelIpc
            .VdRegisterFeature(adapter, new("feature", new(1, 2), metadata))
            .Subscribe(new TaskCompletionObserver<CitrixVirtualChannelFeatureRegistration>(featureSource));

        adapter.PublishData(new(otherChannel, [0]));
        adapter.PublishData(new(channel, [IpcDataFirstByte, IpcDataSecondByte]));
        adapter.PublishEvent(new(CitrixVirtualChannelEventKind.DataReceived, otherChannel, CitrixVirtualChannelStatus.Success));
        adapter.PublishEvent(new(CitrixVirtualChannelEventKind.DataReceived, channel, CitrixVirtualChannelStatus.Success));

        var session = await AwaitWithTimeoutAsync(sessionSource.Task, TimeSpan.FromSeconds(TelemetryTimeoutSeconds));
        var writeResult = await AwaitWithTimeoutAsync(writeSource.Task, TimeSpan.FromSeconds(TelemetryTimeoutSeconds));
        var featureRegistration = await AwaitWithTimeoutAsync(featureSource.Task, TimeSpan.FromSeconds(TelemetryTimeoutSeconds));
        payload[0] = MutatedByte;
        metadata[0] = MutatedByte;
        session.Dispose();
        session.Dispose();

        await Assert.That(adapter.OpenCount).IsEqualTo(1);
        await Assert.That(session.Channel).IsEqualTo(channel);
        await Assert.That(session.IsOpen).IsFalse();
        await Assert.That(adapter.CloseCount).IsEqualTo(1);
        await Assert.That(writeResult.BytesWritten).IsEqualTo(ExpectedPayloadLength);
        await Assert.That(adapter.LastWriteRequest.CopyPayload()[0]).IsEqualTo(OriginalPayloadFirstByte);
        await Assert.That(featureRegistration.FeatureName).IsEqualTo("feature");
        await Assert.That(adapter.LastFeature.CopyMetadata()[0]).IsEqualTo(OriginalMetadataFirstByte);
        await Assert.That(data.Count).IsEqualTo(1);
        await Assert.That(data[0].CopyPayload()[0]).IsEqualTo(IpcDataFirstByte);
        await Assert.That(events.Count).IsEqualTo(1);
        await Assert.That(events[0].Channel).IsEqualTo(channel);
    }

    /// <summary>Verifies disposing a one-shot IPC subscription cancels an in-flight adapter operation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task VirtualChannelIpc_DisposeCancelsInFlightOperationAsync()
    {
        using var adapter = new BlockingVirtualDriverAdapter();
        var observer = new RecordingObserver<CitrixVirtualChannelWriteResult>();
        var subscription = CitrixVirtualChannelIpc
            .DriverWrite(adapter, new(new(IpcChannelValue), [OriginalPayloadFirstByte]))
            .Subscribe(observer);

        await AwaitWithTimeoutAsync(adapter.Started.Task, TimeSpan.FromSeconds(TelemetryTimeoutSeconds));
        subscription.Dispose();
        await AwaitWithTimeoutAsync(adapter.Finished.Task, TimeSpan.FromSeconds(TelemetryTimeoutSeconds));

        await Assert.That(adapter.CancellationObserved).IsTrue();
        await Assert.That(observer.NextCount).IsEqualTo(0);
        await Assert.That(observer.ErrorCount).IsEqualTo(0);
        await Assert.That(observer.CompletedCount).IsEqualTo(0);
    }

    /// <summary>Verifies channel close retries remain available after exceptions and unsuccessful results.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task VirtualChannelSession_CloseRetriesUntilSuccessfulAsync()
    {
        using var adapter = new RetryingCloseVirtualDriverAdapter();
        var session = new CitrixVirtualChannelSession(
            adapter,
            new(new(IpcChannelValue), CitrixVirtualChannelStatus.Success));
        InvalidOperationException closeException = null;

        try
        {
            try
            {
                _ = session.Close();
            }
            catch (InvalidOperationException error)
            {
                closeException = error;
            }

            var failedResult = session.Close();
            var successfulResult = session.Close();
            var alreadyClosedResult = session.Close();

            await Assert.That(closeException).IsNotNull();
            await Assert.That(failedResult.Status).IsEqualTo(CitrixVirtualChannelStatus.Failed);
            await Assert.That(session.IsOpen).IsFalse();
            await Assert.That(successfulResult.Status).IsEqualTo(CitrixVirtualChannelStatus.Success);
            await Assert.That(alreadyClosedResult.Status).IsEqualTo(CitrixVirtualChannelStatus.Closed);
            await Assert.That(adapter.CloseCount).IsEqualTo(ExpectedCloseAttemptCount);
        }
        finally
        {
            session.Dispose();
        }
    }

    /// <summary>Verifies CCM wrappers and WTS registration lifetimes are observable and injectable.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task HostSessionManagement_UsesInjectedCcmAndWtsAdaptersAsync()
    {
        CcmHostSessionInformationResult infoResult = default;
        CcmHostOperationResult disconnectResult = default;
        CcmHostOperationResult logoffResult = default;
        WtsSessionNotificationRegistration registration = default;
        var unregisterCount = 0;
        var ccmApi = new DelegatingCitrixCcmHostSessionApi(
            static sessionId => new(sessionId, 0, CreateSessionInformation(sessionId), HasSessionInformation: true),
            static sessionId => new(sessionId, 0),
            static sessionId => new(sessionId, FailedCcmResultCode));
        var wtsApi = new TestWtsSessionNotificationApi(true, HostLastError, () => unregisterCount++);

        using var informationSubscription = CitrixHostSessionManagement
            .CCMGetSessionInformation(HostSessionId, ccmApi)
            .SubscribeOnNext(value => infoResult = value);
        using var disconnectSubscription = CitrixHostSessionManagement
            .CCMDisconnectSession(HostSessionId, ccmApi)
            .SubscribeOnNext(value => disconnectResult = value);
        using var logoffSubscription = CitrixHostSessionManagement
            .CCMLogoffSession(HostSessionId, ccmApi)
            .SubscribeOnNext(value => logoffResult = value);
        using (CitrixHostSessionManagement
            .WTSRegisterSessionNotification(new(HostWindowHandleValue), WtsSessionNotificationScope.AllSessions, wtsApi)
            .SubscribeOnNext(value => registration = value))
        {
            await Assert.That(registration.Succeeded).IsTrue();
            await Assert.That(registration.WindowHandleValue).IsEqualTo(HostWindowHandleValue);
        }

        await Assert.That(infoResult.Succeeded).IsTrue();
        await Assert.That(infoResult.HasSessionInformation).IsTrue();
        await Assert.That(infoResult.SessionInformation.SessionId).IsEqualTo(HostSessionId);
        await Assert.That(disconnectResult.Succeeded).IsTrue();
        await Assert.That(logoffResult.Succeeded).IsFalse();
        await Assert.That(unregisterCount).IsEqualTo(1);
    }

    /// <summary>Verifies WTS registration failures emit the last Win32 error and do not unregister.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task HostSessionManagement_WtsFailureEmitsLastErrorAsync()
    {
        WtsSessionNotificationRegistration registration = default;
        var unregisterCount = 0;
        var wtsApi = new TestWtsSessionNotificationApi(false, HostFailureLastError, () => unregisterCount++);

        using var subscription = CitrixHostSessionManagement
            .WTSRegisterSessionNotification(new(HostFailureWindowHandleValue), WtsSessionNotificationScope.ThisSession, wtsApi)
            .SubscribeOnNext(value => registration = value);

        await Assert.That(registration.Succeeded).IsFalse();
        await Assert.That(registration.LastError).IsEqualTo(HostFailureLastError);
        await Assert.That(unregisterCount).IsEqualTo(0);
    }

    /// <summary>Verifies the native CCM adapter reports a deterministic missing SDK path without requiring Citrix.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task HostSessionManagement_NativeAdapterReportsMissingSdkPathAsync()
    {
        var missingSdkPath = Path.Combine(
            Path.GetTempPath(),
            $"citrix-tests-{Guid.NewGuid():N}",
            Environment.Is64BitProcess ? "CCMSDK64.dll" : "CCMSDK.dll");
        FileNotFoundException exception = null;

        try
        {
            using var adapter = new NativeCitrixCcmHostSessionApi(missingSdkPath, "test-connection");
        }
        catch (FileNotFoundException error)
        {
            exception = error;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.FileName).IsEqualTo(Path.GetFullPath(missingSdkPath));
    }

    /// <summary>Verifies no-adapter CCM observables report a missing Workspace SDK through their error channel.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task HostSessionManagement_NoAdapterOverloadsReportMissingSdkAsync()
    {
        var informationErrorSource = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        var disconnectErrorSource = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        var logoffErrorSource = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var informationSubscription = CitrixHostSessionManagement
            .CCMGetSessionInformation(HostSessionId)
            .Subscribe(new ErrorCompletionObserver<CcmHostSessionInformationResult>(informationErrorSource));
        using var disconnectSubscription = CitrixHostSessionManagement
            .CCMDisconnectSession(HostSessionId)
            .Subscribe(new ErrorCompletionObserver<CcmHostOperationResult>(disconnectErrorSource));
        using var logoffSubscription = CitrixHostSessionManagement
            .CCMLogoffSession(HostSessionId)
            .Subscribe(new ErrorCompletionObserver<CcmHostOperationResult>(logoffErrorSource));

        var informationError = await AwaitWithTimeoutAsync(informationErrorSource.Task, TimeSpan.FromSeconds(TelemetryTimeoutSeconds));
        var disconnectError = await AwaitWithTimeoutAsync(disconnectErrorSource.Task, TimeSpan.FromSeconds(TelemetryTimeoutSeconds));
        var logoffError = await AwaitWithTimeoutAsync(logoffErrorSource.Task, TimeSpan.FromSeconds(TelemetryTimeoutSeconds));

        await Assert.That(informationError is FileNotFoundException).IsTrue();
        await Assert.That(disconnectError is FileNotFoundException).IsTrue();
        await Assert.That(logoffError is FileNotFoundException).IsTrue();
    }

    /// <summary>Verifies telemetry request mapping, URI building, and injected polling transport.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MonitorTelemetry_BuildsRequestsAndPollsInjectedTransportAsync()
    {
        var observedAt = new DateTimeOffset(2026, 8, 21, 9, 0, 0, TimeSpan.Zero);
        var options = new CitrixMonitorTelemetryOptions
        {
            PollingInterval = TimeSpan.FromMilliseconds(TelemetryPollingMilliseconds),
            SessionsQuery = "$top=1",
            MachineResourceUtilizationEntity = CitrixMachineResourceUtilizationTelemetryEntity.ResourceUtilization,
            ApplicationFailureEntity = CitrixApplicationFailureTelemetryEntity.ApplicationFaults,
            ApplicationFailureLogsQuery = "?$top=2",
            TimeProvider = new FixedTimeProvider(observedAt),
        };
        var requests = new List<CitrixMonitorTelemetryRequest>();
        var transport = new DelegateCitrixMonitorTelemetryTransport((request, cancellationToken) =>
        {
            requests.Add(request);
            return Task.FromResult("{\"value\":[]}");
        });
        var telemetry = new CitrixMonitorTelemetry(transport, options);
        var snapshotSource = new TaskCompletionSource<CitrixMonitorTelemetrySnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var subscription = telemetry.Sessions.Subscribe(new TaskCompletionObserver<CitrixMonitorTelemetrySnapshot>(snapshotSource));
        var sessionSnapshot = await AwaitWithTimeoutAsync(snapshotSource.Task, TimeSpan.FromSeconds(TelemetryTimeoutSeconds));

        var builder = new CitrixMonitorTelemetryUriBuilder(new($"https://{ServerName}/Citrix/Monitor/OData/v4/Data"));
        var applicationRequest = CitrixMonitorTelemetry.CreateApplicationFailureLogsRequest(options);
        var utilizationRequest = CitrixMonitorTelemetry.CreateMachineResourceUtilizationsRequest(options);
        var applicationUri = builder.CreateRequestUri(applicationRequest);

        await Assert.That(sessionSnapshot.Name).IsEqualTo(CitrixMonitorTelemetry.SessionsName);
        await Assert.That(sessionSnapshot.Entity).IsEqualTo(CitrixMonitorTelemetry.SessionsEntity);
        await Assert.That(sessionSnapshot.Query).IsEqualTo("$top=1");
        await Assert.That(sessionSnapshot.Json).IsEqualTo("{\"value\":[]}");
        await Assert.That(sessionSnapshot.ObservedAt).IsEqualTo(observedAt);
        await Assert.That(requests.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(utilizationRequest.Entity).IsEqualTo(CitrixMonitorTelemetry.ResourceUtilizationEntity);
        await Assert.That(applicationRequest.Entity).IsEqualTo(CitrixMonitorTelemetry.ApplicationFaultsEntity);
        await Assert.That(applicationUri.AbsoluteUri).IsEqualTo($"https://{ServerName}/Citrix/Monitor/OData/v4/Data/ApplicationFaults?$top=2");
    }

    /// <summary>Creates deterministic CCM session information for tests.</summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>The session information.</returns>
    private static CcmHostSessionInformation CreateSessionInformation(int sessionId) =>
        new(
            sessionId,
            ConnectionId: 1,
            FriendlyName: "Friendly",
            NonSeamlessAppTitle: "App",
            IsFullScreen: false,
            Ssl: true,
            EncryptionLevel: "Basic",
            EngineVersion: "1.0",
            ServerName: ServerName,
            UserName: "user",
            DomainName: "domain",
            RxFrameCount: 2,
            TxFrameCount: 3,
            RxByteCount: 4,
            TxByteCount: 5,
            RxFrameErrorCount: 0,
            TxFrameErrorCount: 0,
            SeamlessMode: true,
            ZlMode: false,
            Cgp: true,
            SpeedBrowseEnabled: false,
            LastLatency: 6,
            AverageLatency: 7,
            RoundTripDeviation: 8,
            HorizontalResolution: 1920,
            VerticalResolution: 1080,
            ColorDepth: 32,
            AudioEnabled: true,
            PdaEnabled: false,
            TwnEnabled: false,
            PnpEnabled: true);

    /// <summary>Waits for a task to complete within a deterministic timeout.</summary>
    /// <typeparam name="T">The task result type.</typeparam>
    /// <param name="task">The task to await.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>The task result.</returns>
    private static async Task<T> AwaitWithTimeoutAsync<T>(Task<T> task, TimeSpan timeout)
    {
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout));
        if (!ReferenceEquals(completedTask, task))
        {
            throw new TimeoutException("The Citrix telemetry stream did not emit before the test timeout.");
        }

        return await task;
    }

    /// <summary>Deterministic virtual-driver adapter for IPC tests.</summary>
    private sealed class TestVirtualDriverAdapter : ICitrixVirtualDriverAdapter, IDisposable
    {
        /// <summary>The incoming data stream.</summary>
        private readonly ManualObservable<CitrixVirtualChannelData> _incomingData = new();

        /// <summary>The driver event stream.</summary>
        private readonly ManualObservable<CitrixVirtualChannelEvent> _events = new();

        /// <inheritdoc />
        public IObservable<CitrixVirtualChannelData> IncomingData => _incomingData;

        /// <inheritdoc />
        public IObservable<CitrixVirtualChannelEvent> Events => _events;

        /// <summary>Gets the number of open calls.</summary>
        internal int OpenCount { get; private set; }

        /// <summary>Gets the number of close calls.</summary>
        internal int CloseCount { get; private set; }

        /// <summary>Gets the last write request.</summary>
        internal CitrixVirtualChannelWriteRequest LastWriteRequest { get; private set; }

        /// <summary>Gets the last feature registration request.</summary>
        internal CitrixVirtualChannelFeature LastFeature { get; private set; }

        /// <inheritdoc />
        public CitrixVirtualChannelOpenResult DriverOpen(CitrixVirtualChannelOpenRequest request, CancellationToken cancellationToken)
        {
            OpenCount++;
            return new(new(IpcChannelValue), CitrixVirtualChannelStatus.Success);
        }

        /// <inheritdoc />
        public CitrixVirtualChannelCloseResult DriverClose(CitrixVirtualChannelHandle channel, CancellationToken cancellationToken)
        {
            CloseCount++;
            return new(channel, CitrixVirtualChannelStatus.Success);
        }

        /// <inheritdoc />
        public CitrixVirtualChannelWriteResult DriverWrite(CitrixVirtualChannelWriteRequest request, CancellationToken cancellationToken)
        {
            LastWriteRequest = request;
            return new(request.Channel, CitrixVirtualChannelStatus.Success, request.PayloadLength);
        }

        /// <inheritdoc />
        public CitrixVirtualChannelFeatureRegistration VdRegisterFeature(CitrixVirtualChannelFeature feature, CancellationToken cancellationToken)
        {
            LastFeature = feature;
            return new(feature.Name, CitrixVirtualChannelStatus.Success);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _incomingData.Dispose();
            _events.Dispose();
        }

        /// <summary>Publishes incoming data.</summary>
        /// <param name="data">The data to publish.</param>
        internal void PublishData(CitrixVirtualChannelData data) => _incomingData.OnNext(data);

        /// <summary>Publishes a driver event.</summary>
        /// <param name="driverEvent">The event to publish.</param>
        internal void PublishEvent(CitrixVirtualChannelEvent driverEvent) => _events.OnNext(driverEvent);
    }

    /// <summary>Virtual-driver adapter whose write blocks until its cancellation token is signaled.</summary>
    private sealed class BlockingVirtualDriverAdapter : ICitrixVirtualDriverAdapter, IDisposable
    {
        /// <summary>The incoming data stream.</summary>
        private readonly ManualObservable<CitrixVirtualChannelData> _incomingData = new();

        /// <summary>The driver event stream.</summary>
        private readonly ManualObservable<CitrixVirtualChannelEvent> _events = new();

        /// <inheritdoc />
        public IObservable<CitrixVirtualChannelData> IncomingData => _incomingData;

        /// <inheritdoc />
        public IObservable<CitrixVirtualChannelEvent> Events => _events;

        /// <summary>Gets a completion source signaled when the write begins.</summary>
        internal TaskCompletionSource<bool> Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Gets a completion source signaled when the write observes cancellation.</summary>
        internal TaskCompletionSource<bool> Finished { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Gets a value indicating whether the operation token was canceled.</summary>
        internal bool CancellationObserved { get; private set; }

        /// <inheritdoc />
        public CitrixVirtualChannelOpenResult DriverOpen(CitrixVirtualChannelOpenRequest request, CancellationToken cancellationToken) =>
            new(new(IpcChannelValue), CitrixVirtualChannelStatus.Success);

        /// <inheritdoc />
        public CitrixVirtualChannelCloseResult DriverClose(CitrixVirtualChannelHandle channel, CancellationToken cancellationToken) =>
            new(channel, CitrixVirtualChannelStatus.Success);

        /// <inheritdoc />
        public CitrixVirtualChannelWriteResult DriverWrite(CitrixVirtualChannelWriteRequest request, CancellationToken cancellationToken)
        {
            _ = Started.TrySetResult(true);
            _ = cancellationToken.WaitHandle.WaitOne();
            CancellationObserved = cancellationToken.IsCancellationRequested;
            _ = Finished.TrySetResult(true);
            cancellationToken.ThrowIfCancellationRequested();
            return new(request.Channel, CitrixVirtualChannelStatus.Success, request.PayloadLength);
        }

        /// <inheritdoc />
        public CitrixVirtualChannelFeatureRegistration VdRegisterFeature(CitrixVirtualChannelFeature feature, CancellationToken cancellationToken) =>
            new(feature.Name, CitrixVirtualChannelStatus.Success);

        /// <inheritdoc />
        public void Dispose()
        {
            _incomingData.Dispose();
            _events.Dispose();
        }
    }

    /// <summary>Virtual-driver adapter that throws, fails, then succeeds on successive close attempts.</summary>
    private sealed class RetryingCloseVirtualDriverAdapter : ICitrixVirtualDriverAdapter, IDisposable
    {
        /// <summary>The incoming data stream.</summary>
        private readonly ManualObservable<CitrixVirtualChannelData> _incomingData = new();

        /// <summary>The driver event stream.</summary>
        private readonly ManualObservable<CitrixVirtualChannelEvent> _events = new();

        /// <inheritdoc />
        public IObservable<CitrixVirtualChannelData> IncomingData => _incomingData;

        /// <inheritdoc />
        public IObservable<CitrixVirtualChannelEvent> Events => _events;

        /// <summary>Gets the number of close attempts.</summary>
        internal int CloseCount { get; private set; }

        /// <inheritdoc />
        public CitrixVirtualChannelOpenResult DriverOpen(CitrixVirtualChannelOpenRequest request, CancellationToken cancellationToken) =>
            new(new(IpcChannelValue), CitrixVirtualChannelStatus.Success);

        /// <inheritdoc />
        public CitrixVirtualChannelCloseResult DriverClose(CitrixVirtualChannelHandle channel, CancellationToken cancellationToken)
        {
            CloseCount++;
            return CloseCount switch
            {
                1 => throw new InvalidOperationException("Deterministic close failure."),
                FailedCloseAttempt => new(channel, CitrixVirtualChannelStatus.Failed),
                _ => new(channel, CitrixVirtualChannelStatus.Success),
            };
        }

        /// <inheritdoc />
        public CitrixVirtualChannelWriteResult DriverWrite(CitrixVirtualChannelWriteRequest request, CancellationToken cancellationToken) =>
            new(request.Channel, CitrixVirtualChannelStatus.Success, request.PayloadLength);

        /// <inheritdoc />
        public CitrixVirtualChannelFeatureRegistration VdRegisterFeature(CitrixVirtualChannelFeature feature, CancellationToken cancellationToken) =>
            new(feature.Name, CitrixVirtualChannelStatus.Success);

        /// <inheritdoc />
        public void Dispose()
        {
            _incomingData.Dispose();
            _events.Dispose();
        }
    }

    /// <summary>Thread-safe observer that records notification counts.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    private sealed class RecordingObserver<T> : IObserver<T>
    {
        /// <summary>The completed notification count.</summary>
        private int _completedCount;

        /// <summary>The error notification count.</summary>
        private int _errorCount;

        /// <summary>The next notification count.</summary>
        private int _nextCount;

        /// <summary>Gets the completed notification count.</summary>
        internal int CompletedCount => Volatile.Read(ref _completedCount);

        /// <summary>Gets the error notification count.</summary>
        internal int ErrorCount => Volatile.Read(ref _errorCount);

        /// <summary>Gets the next notification count.</summary>
        internal int NextCount => Volatile.Read(ref _nextCount);

        /// <inheritdoc />
        public void OnCompleted() => Interlocked.Increment(ref _completedCount);

        /// <inheritdoc />
        public void OnError(Exception error) => Interlocked.Increment(ref _errorCount);

        /// <inheritdoc />
        public void OnNext(T value) => Interlocked.Increment(ref _nextCount);
    }

    /// <summary>Observer that completes a task with the source error.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    /// <param name="errorSource">The source completed by an error notification.</param>
    private sealed class ErrorCompletionObserver<T>(TaskCompletionSource<Exception> errorSource) : IObserver<T>
    {
        /// <inheritdoc />
        public void OnCompleted() =>
            _ = errorSource.TrySetException(new InvalidOperationException("The native CCM observable completed without reporting the missing SDK."));

        /// <inheritdoc />
        public void OnError(Exception error) => _ = errorSource.TrySetResult(error);

        /// <inheritdoc />
        public void OnNext(T value) =>
            _ = errorSource.TrySetException(new InvalidOperationException("The native CCM observable emitted a value without an installed SDK."));
    }

    /// <summary>Deterministic WTS notification API for host tests.</summary>
    /// <param name="registerResult">The registration result.</param>
    /// <param name="lastError">The last Win32 error.</param>
    /// <param name="onUnregister">The unregistration callback.</param>
    private sealed class TestWtsSessionNotificationApi(bool registerResult, int lastError, Action onUnregister) : IWtsSessionNotificationApi
    {
        /// <inheritdoc />
        public bool WTSRegisterSessionNotification(IntPtr windowHandle, WtsSessionNotificationScope scope) => registerResult;

        /// <inheritdoc />
        public bool WTSUnRegisterSessionNotification(IntPtr windowHandle)
        {
            onUnregister();
            return true;
        }

        /// <inheritdoc />
        public int GetLastError() => lastError;
    }

    /// <summary>Completes a task from the first observable value.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    /// <param name="completionSource">The task completion source.</param>
    private sealed class TaskCompletionObserver<T>(TaskCompletionSource<T> completionSource) : IObserver<T>
    {
        /// <inheritdoc />
        public void OnCompleted()
        {
        }

        /// <inheritdoc />
        public void OnError(Exception error) => completionSource.TrySetException(error);

        /// <inheritdoc />
        public void OnNext(T value) => completionSource.TrySetResult(value);
    }

    /// <summary>Fixed UTC time provider for telemetry tests.</summary>
    /// <param name="utcNow">The fixed UTC timestamp.</param>
    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        /// <inheritdoc />
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
