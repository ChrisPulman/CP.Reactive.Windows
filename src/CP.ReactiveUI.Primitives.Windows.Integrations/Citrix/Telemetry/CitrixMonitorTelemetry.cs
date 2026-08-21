// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using ReactiveUI.Primitives.Disposables;
using ReactiveSignal = ReactiveUI.Primitives.Signals.Signal;

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Telemetry;

/// <summary>Reactive Citrix Monitor Service telemetry streams.</summary>
public sealed class CitrixMonitorTelemetry
{
    /// <summary>The public sessions stream name.</summary>
    public static readonly string SessionsName = "Sessions";

    /// <summary>The public machine resource utilization stream name.</summary>
    public static readonly string MachineResourceUtilizationsName = "MachineResourceUtilizations";

    /// <summary>The public connection failure log stream name.</summary>
    public static readonly string ConnectionFailureLogsName = "ConnectionFailureLogs";

    /// <summary>The public application failure log stream name.</summary>
    public static readonly string ApplicationFailureLogsName = "ApplicationFailureLogs";

    /// <summary>The Citrix Monitor Service Sessions entity.</summary>
    public static readonly string SessionsEntity = "Sessions";

    /// <summary>The Citrix Monitor Service ResourceUtilizationSummary entity.</summary>
    public static readonly string ResourceUtilizationSummaryEntity = "ResourceUtilizationSummary";

    /// <summary>The Citrix Monitor Service ResourceUtilization entity.</summary>
    public static readonly string ResourceUtilizationEntity = "ResourceUtilization";

    /// <summary>The Citrix Monitor Service ConnectionFailureLogs entity.</summary>
    public static readonly string ConnectionFailureLogsEntity = "ConnectionFailureLogs";

    /// <summary>The Citrix Monitor Service ApplicationErrors entity.</summary>
    public static readonly string ApplicationErrorsEntity = "ApplicationErrors";

    /// <summary>The Citrix Monitor Service ApplicationFaults entity.</summary>
    public static readonly string ApplicationFaultsEntity = "ApplicationFaults";

    /// <summary>The transport used to collect raw JSON telemetry.</summary>
    private readonly ICitrixMonitorTelemetryTransport _transport;

    /// <summary>The options used to configure telemetry polling.</summary>
    private readonly CitrixMonitorTelemetryOptions _options;

    /// <summary>Initializes a new instance of the <see cref="CitrixMonitorTelemetry"/> class.</summary>
    /// <param name="transport">The telemetry transport.</param>
    public CitrixMonitorTelemetry(ICitrixMonitorTelemetryTransport transport)
        : this(transport, new())
    {
    }

    /// <summary>Initializes a new instance of the <see cref="CitrixMonitorTelemetry"/> class.</summary>
    /// <param name="transport">The telemetry transport.</param>
    /// <param name="options">The telemetry options.</param>
    public CitrixMonitorTelemetry(ICitrixMonitorTelemetryTransport transport, CitrixMonitorTelemetryOptions options)
    {
        Throw.IfNull(transport);
        Throw.IfNull(options);
        Throw.IfNull(options.TimeProvider);
        ValidatePollingInterval(options.PollingInterval);
        _transport = transport;
        _options = options.Clone();
        Sessions = CreatePollingStream(CreateSessionsRequest(_options));
        MachineResourceUtilizations = CreatePollingStream(CreateMachineResourceUtilizationsRequest(_options));
        ConnectionFailureLogs = CreatePollingStream(CreateConnectionFailureLogsRequest(_options));
        ApplicationFailureLogs = CreatePollingStream(CreateApplicationFailureLogsRequest(_options));
    }

    /// <summary>Gets raw JSON snapshots from the Citrix Monitor Service <c>Sessions</c> entity.</summary>
    public IObservable<CitrixMonitorTelemetrySnapshot> Sessions { get; }

    /// <summary>Gets raw JSON snapshots for machine resource utilization.</summary>
    public IObservable<CitrixMonitorTelemetrySnapshot> MachineResourceUtilizations { get; }

    /// <summary>Gets raw JSON snapshots from the Citrix Monitor Service <c>ConnectionFailureLogs</c> entity.</summary>
    public IObservable<CitrixMonitorTelemetrySnapshot> ConnectionFailureLogs { get; }

    /// <summary>Gets raw JSON snapshots for application failures.</summary>
    public IObservable<CitrixMonitorTelemetrySnapshot> ApplicationFailureLogs { get; }

    /// <summary>Creates a raw JSON sessions telemetry request.</summary>
    /// <param name="options">The telemetry options.</param>
    /// <returns>The telemetry request.</returns>
    public static CitrixMonitorTelemetryRequest CreateSessionsRequest(CitrixMonitorTelemetryOptions options)
    {
        Throw.IfNull(options);
        return new(SessionsName, SessionsEntity, options.SessionsQuery);
    }

    /// <summary>Creates a raw JSON machine resource utilization telemetry request.</summary>
    /// <param name="options">The telemetry options.</param>
    /// <returns>The telemetry request.</returns>
    public static CitrixMonitorTelemetryRequest CreateMachineResourceUtilizationsRequest(CitrixMonitorTelemetryOptions options)
    {
        Throw.IfNull(options);
        return new(
            MachineResourceUtilizationsName,
            GetMachineResourceUtilizationEntity(options.MachineResourceUtilizationEntity),
            options.MachineResourceUtilizationsQuery);
    }

    /// <summary>Creates a raw JSON connection failure log telemetry request.</summary>
    /// <param name="options">The telemetry options.</param>
    /// <returns>The telemetry request.</returns>
    public static CitrixMonitorTelemetryRequest CreateConnectionFailureLogsRequest(CitrixMonitorTelemetryOptions options)
    {
        Throw.IfNull(options);
        return new(ConnectionFailureLogsName, ConnectionFailureLogsEntity, options.ConnectionFailureLogsQuery);
    }

    /// <summary>Creates a raw JSON application failure log telemetry request.</summary>
    /// <param name="options">The telemetry options.</param>
    /// <returns>The telemetry request.</returns>
    public static CitrixMonitorTelemetryRequest CreateApplicationFailureLogsRequest(CitrixMonitorTelemetryOptions options)
    {
        Throw.IfNull(options);
        return new(
            ApplicationFailureLogsName,
            GetApplicationFailureEntity(options.ApplicationFailureEntity),
            options.ApplicationFailureLogsQuery);
    }

    /// <summary>Gets the Monitor Service entity used for machine resource utilization telemetry.</summary>
    /// <param name="entity">The configured entity.</param>
    /// <returns>The Monitor Service entity name.</returns>
    public static string GetMachineResourceUtilizationEntity(CitrixMachineResourceUtilizationTelemetryEntity entity) =>
        entity switch
        {
            CitrixMachineResourceUtilizationTelemetryEntity.ResourceUtilizationSummary => ResourceUtilizationSummaryEntity,
            CitrixMachineResourceUtilizationTelemetryEntity.ResourceUtilization => ResourceUtilizationEntity,
            _ => throw new ArgumentOutOfRangeException(nameof(entity), entity, "The machine resource utilization entity is not supported."),
        };

    /// <summary>Gets the Monitor Service entity used for application failure telemetry.</summary>
    /// <param name="entity">The configured entity.</param>
    /// <returns>The Monitor Service entity name.</returns>
    public static string GetApplicationFailureEntity(CitrixApplicationFailureTelemetryEntity entity) =>
        entity switch
        {
            CitrixApplicationFailureTelemetryEntity.ApplicationErrors => ApplicationErrorsEntity,
            CitrixApplicationFailureTelemetryEntity.ApplicationFaults => ApplicationFaultsEntity,
            _ => throw new ArgumentOutOfRangeException(nameof(entity), entity, "The application failure entity is not supported."),
        };

    /// <summary>Validates a polling interval.</summary>
    /// <param name="pollingInterval">The polling interval.</param>
    private static void ValidatePollingInterval(TimeSpan pollingInterval)
    {
        if (pollingInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(pollingInterval), pollingInterval, "The polling interval must be greater than zero.");
        }
    }

    /// <summary>Creates a polling stream for one telemetry request.</summary>
    /// <param name="request">The telemetry request.</param>
    /// <returns>The polling stream.</returns>
    private IObservable<CitrixMonitorTelemetrySnapshot> CreatePollingStream(CitrixMonitorTelemetryRequest request) =>
        ReactiveSignal.CreateSafe<CitrixMonitorTelemetrySnapshot>(observer =>
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            _ = Task.Run(
                async () =>
                {
                    try
                    {
                        if (!_options.PollImmediately)
                        {
                            await DelayPollingIntervalAsync(cancellationToken).ConfigureAwait(false);
                        }

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var json = await _transport.GetJsonAsync(request, cancellationToken).ConfigureAwait(false);
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            observer.OnNext(new(request, json, _options.TimeProvider.GetUtcNow()));
                            await DelayPollingIntervalAsync(cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                    }
                    catch (Exception error)
                    {
                        observer.OnError(error);
                    }
                },
                CancellationToken.None);
            return Scope.Create(
                cancellationTokenSource,
                static source =>
                {
                    source.Cancel();
                    source.Dispose();
                });
        });

    /// <summary>Waits for the configured polling interval.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The delay task.</returns>
    private Task DelayPollingIntervalAsync(CancellationToken cancellationToken) => Task.Delay(_options.PollingInterval, cancellationToken);
}
