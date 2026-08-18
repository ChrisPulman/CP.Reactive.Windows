// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Disposables;
using System.Reactive.Linq;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Power;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
using ReactiveUI.Reactive;
using RxVoid = System.Reactive.Unit;

namespace CP.ReactiveUI.Primitives.Windows.Reactive.Example.ConsoleDemo;

/// <summary>Owns the dashboard's reactive state, commands, activation, and native subscriptions.</summary>
internal sealed class DiagnosticsViewModel : ReactiveObject, IActivatableViewModel, IDisposable
{
    private readonly DiagnosticLog _log;
    private readonly ObservableAsPropertyHelper<string> _activityText;
    private DiagnosticSnapshot _lastSnapshot;
    private bool _isBusy;
    private bool _isWatching;
    private bool _isKeepingAwake;
    private int _displayCount;
    private int _nativeEventCount;
    private string _cursor = "unknown";
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="DiagnosticsViewModel"/> class.</summary>
    /// <param name="log">The event log.</param>
    public DiagnosticsViewModel(DiagnosticLog log)
    {
        _log = log;
        Activator = new();
        Refresh = ReactiveCommand.CreateFromTask<DiagnosticSnapshot>(DiagnosticsService.CaptureSnapshotAsync);
        Wait = ReactiveCommand.CreateFromTask<TimeSpan, string>(WaitAsync);
        SetWatching = ReactiveCommand.Create<bool, bool>(enabled => IsWatching = enabled);
        SetKeepingAwake = ReactiveCommand.Create<bool, bool>(SetAwake);

        _activityText = this.WhenAnyValue(
                static model => model.IsBusy,
                static model => model.IsWatching,
                static model => model.IsKeepingAwake,
                static (busy, watching, awake) => busy ? "BUSY" : $"READY | watch {(watching ? "on" : "off")} | awake {(awake ? "on" : "off")}")
            .ToProperty(this, static model => model.ActivityText, initialValue: "STARTING");

        this.WhenActivated(disposables =>
        {
            ObserveCommandState(disposables);
            ObserveWindowsEvents(disposables);
            ObserveCursor(disposables);
            ObserveAutomaticRefresh(disposables);
            _log.Info("Reactive activation started; native subscriptions are live.");
            disposables(Disposable.Create(() => _log.Info("Reactive activation stopped; subscriptions disposed.")));
        });
    }

    /// <inheritdoc />
    public ViewModelActivator Activator { get; }

    /// <summary>Gets the command that captures a diagnostic snapshot.</summary>
    public ReactiveCommand<RxVoid, DiagnosticSnapshot> Refresh { get; }

    /// <summary>Gets the cancellable wait command.</summary>
    public ReactiveCommand<TimeSpan, string> Wait { get; }

    /// <summary>Gets the command that controls automatic snapshots.</summary>
    public ReactiveCommand<bool, bool> SetWatching { get; }

    /// <summary>Gets the command that controls the Windows execution-state request.</summary>
    public ReactiveCommand<bool, bool> SetKeepingAwake { get; }

    /// <summary>Gets a derived status produced by <c>WhenAnyValue</c> and <c>ToProperty</c>.</summary>
    public string ActivityText => _activityText.Value;

    /// <summary>Gets the most recent snapshot.</summary>
    public DiagnosticSnapshot LastSnapshot
    {
        get => _lastSnapshot;
        private set => this.RaiseAndSetIfChanged(ref _lastSnapshot, value);
    }

    /// <summary>Gets a value indicating whether a command is executing.</summary>
    public bool IsBusy
    {
        get => _isBusy;
        private set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    /// <summary>Gets a value indicating whether periodic refresh is active.</summary>
    public bool IsWatching
    {
        get => _isWatching;
        private set => this.RaiseAndSetIfChanged(ref _isWatching, value);
    }

    /// <summary>Gets a value indicating whether Windows has been asked to keep the system awake.</summary>
    public bool IsKeepingAwake
    {
        get => _isKeepingAwake;
        private set => this.RaiseAndSetIfChanged(ref _isKeepingAwake, value);
    }

    /// <summary>Gets the latest display count reported by the topology observable.</summary>
    public int DisplayCount
    {
        get => _displayCount;
        private set => this.RaiseAndSetIfChanged(ref _displayCount, value);
    }

    /// <summary>Gets the latest sampled cursor location.</summary>
    public string Cursor
    {
        get => _cursor;
        private set => this.RaiseAndSetIfChanged(ref _cursor, value);
    }

    /// <summary>Gets the number of native events seen during this activation.</summary>
    public int NativeEventCount
    {
        get => _nativeEventCount;
        private set => this.RaiseAndSetIfChanged(ref _nativeEventCount, value);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (IsKeepingAwake)
        {
            _ = SystemStateApi.AllowSleep();
            IsKeepingAwake = false;
        }

        Refresh.Dispose();
        Wait.Dispose();
        SetWatching.Dispose();
        SetKeepingAwake.Dispose();
        _activityText.Dispose();
    }

    private static async Task<string> WaitAsync(TimeSpan duration, CancellationToken cancellationToken)
    {
        await Task.Delay(duration, cancellationToken).ConfigureAwait(false);
        return $"Waited {duration.TotalSeconds:0.###} seconds without blocking the dashboard thread.";
    }

    private bool SetAwake(bool enabled)
    {
        _ = enabled ? SystemStateApi.PreventSystemSleep() : SystemStateApi.AllowSleep();
        IsKeepingAwake = enabled;
        _log.Info(enabled ? "System-sleep prevention enabled." : "System-sleep prevention released.");
        return enabled;
    }

    private void ObserveCommandState(Action<IDisposable> register)
    {
        register(Refresh.IsExecuting.Subscribe(executing => IsBusy = executing));
        register(Refresh.Subscribe(snapshot =>
        {
            LastSnapshot = snapshot;
            DisplayCount = snapshot.DisplayCount;
        }));
        register(Refresh.ThrownExceptions.Subscribe(exception => _log.Error($"Refresh failed: {exception.Message}")));
        register(Wait.ThrownExceptions
            .Where(static exception => exception is not OperationCanceledException)
            .Subscribe(exception => _log.Error($"Wait failed: {exception.Message}")));
        register(SetKeepingAwake.ThrownExceptions.Subscribe(exception => _log.Error($"Power request failed: {exception.Message}")));
    }

    private void ObserveWindowsEvents(Action<IDisposable> register)
    {
        register(ObserveSafely(
            DisplayTopology.ObserveChanges,
            displays =>
            {
                DisplayCount = displays.Count;
                RecordNativeEvent($"Display topology changed: {displays.Count} display(s).");
            },
            "display topology"));
        register(Observable.Interval(TimeSpan.FromSeconds(1))
            .Select(static _ => ClipboardNative.SequenceNumber)
            .DistinctUntilChanged()
            .Skip(1)
            .Subscribe(
                sequence => RecordNativeEvent($"Clipboard sequence changed to {sequence}."),
                exception => _log.Warning($"Clipboard polling stopped: {exception.Message}")));
        register(ObserveSafely(
            () => EnvironmentMonitor.EnvironmentChangeEvents,
            change => RecordNativeEvent($"Environment changed: {change.Area ?? "unspecified"} ({change.SystemParametersInfoAction})."),
            "environment"));

        WindowsSessionListener sessionListener = new();
        register(sessionListener);
        register(ObserveSafely(
            sessionListener.ObserveSessionChanges,
            change => RecordNativeEvent($"Session {change.SessionId}: {change.EventType}."),
            "session"));
    }

    private void ObserveCursor(Action<IDisposable> register) =>
        register(Observable.Interval(TimeSpan.FromMilliseconds(500))
            .Select(static _ => User32Api.GetCursorLocation())
            .DistinctUntilChanged()
            .Subscribe(
                point => Cursor = $"X={point.X}, Y={point.Y}",
                exception => _log.Warning($"Cursor sampling stopped: {exception.Message}")));

    private void ObserveAutomaticRefresh(Action<IDisposable> register) =>
        register(Observable.Interval(TimeSpan.FromSeconds(3))
            .Where(_ => IsWatching && !IsBusy)
            .SelectMany(_ => Refresh.Execute().Catch<DiagnosticSnapshot, Exception>(exception =>
            {
                _log.Warning($"Automatic refresh skipped: {exception.Message}");
                return Observable.Empty<DiagnosticSnapshot>();
            }))
            .Subscribe());

    private IDisposable ObserveSafely<T>(Func<IObservable<T>> sourceFactory, Action<T> onNext, string name)
    {
        try
        {
            return sourceFactory().Subscribe(onNext, exception => _log.Warning($"The {name} stream is unavailable: {exception.Message}"));
        }
        catch (Exception exception)
        {
            _log.Warning($"The {name} stream could not start: {exception.Message}");
            return Disposable.Empty;
        }
    }

    private void RecordNativeEvent(string message)
    {
        NativeEventCount++;
        _log.Info(message);
    }
}
