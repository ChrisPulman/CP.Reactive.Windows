// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using ReactiveUI.Reactive;
using RxVoid = System.Reactive.Unit;

namespace CP.ReactiveUI.Primitives.Windows.Reactive.Example.FormsExample;

internal sealed class OperationsCenterViewModel : ReactiveObject, IDisposable
{
    private const int MaximumEventCount = 1_500;
    private readonly WindowsOperationsService _service;
    private readonly List<OperationsEvent> _allEvents = new List<OperationsEvent>();
    private readonly CompositeDisposable _lifetime = new CompositeDisposable();
    private string _clipboardDraft = "Windows Operations Center diagnostic marker";
    private string _clipboardSummary = "Clipboard data has not been sampled.";
    private string _eventCategory = "All";
    private string _eventSearch = string.Empty;
    private string _inputIdle = "Unknown";
    private bool _inputCaptureEnabled;
    private bool _eventCapturePaused;
    private bool _keepAwake;
    private WindowSnapshot _selectedWindow;
    private string _status = "Ready";
    private string _lastRefresh = "Not refreshed";
    private bool _disposed;

    public OperationsCenterViewModel(WindowsOperationsService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));

        var hasWindow = this.WhenAnyValue(viewModel => viewModel.SelectedWindow).Select(window => window != null);
        RefreshAllCommand = ReactiveCommand.Create(RefreshAll);
        RefreshWindowsCommand = ReactiveCommand.Create(RefreshWindows);
        RefreshProcessesCommand = ReactiveCommand.Create(RefreshProcesses);
        RefreshDisplaysCommand = ReactiveCommand.Create(RefreshDisplays);
        RefreshClipboardCommand = ReactiveCommand.Create(RefreshClipboard);
        BringToFrontCommand = ReactiveCommand.Create(BringSelectedToFront, hasWindow);
        RestoreWindowCommand = ReactiveCommand.Create(RestoreSelectedWindow, hasWindow);
        WriteClipboardCommand = ReactiveCommand.Create(WriteClipboard);
        CopyReportCommand = ReactiveCommand.Create(CopyReport);
        ClearEventsCommand = ReactiveCommand.Create(ClearEvents);
        PlayNotificationCommand = ReactiveCommand.Create(PlayNotification);

        _lifetime.Add(this.WhenAnyValue(viewModel => viewModel.EventSearch, viewModel => viewModel.EventCategory)
            .Subscribe(_ => RebuildFilteredEvents()));
    }

    public BindingList<WindowSnapshot> Windows { get; } = new BindingList<WindowSnapshot>();

    public BindingList<ProcessSnapshot> Processes { get; } = new BindingList<ProcessSnapshot>();

    public BindingList<DisplaySnapshot> Displays { get; } = new BindingList<DisplaySnapshot>();

    public BindingList<CapabilitySnapshot> Capabilities { get; } = new BindingList<CapabilitySnapshot>();

    public BindingList<OperationsEvent> FilteredEvents { get; } = new BindingList<OperationsEvent>();

    public string[] EventCategories { get; } = { "All", "Application", "Capability", "Clipboard", "Display", "Environment", "Input", "Lifecycle", "Media", "Power", "Process", "Session", "Window", "Action" };

    public ReactiveCommand<RxVoid, RxVoid> RefreshAllCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> RefreshWindowsCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> RefreshProcessesCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> RefreshDisplaysCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> RefreshClipboardCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> BringToFrontCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> RestoreWindowCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> WriteClipboardCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> CopyReportCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> ClearEventsCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> PlayNotificationCommand { get; }

    public string Status
    {
        get => _status;
        private set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public string LastRefresh
    {
        get => _lastRefresh;
        private set => this.RaiseAndSetIfChanged(ref _lastRefresh, value);
    }

    public string EventSearch
    {
        get => _eventSearch;
        set => this.RaiseAndSetIfChanged(ref _eventSearch, value);
    }

    public string EventCategory
    {
        get => _eventCategory;
        set => this.RaiseAndSetIfChanged(ref _eventCategory, value);
    }

    public bool EventCapturePaused
    {
        get => _eventCapturePaused;
        set => this.RaiseAndSetIfChanged(ref _eventCapturePaused, value);
    }

    public bool InputCaptureEnabled
    {
        get => _inputCaptureEnabled;
        set => this.RaiseAndSetIfChanged(ref _inputCaptureEnabled, value);
    }

    public bool KeepAwake
    {
        get => _keepAwake;
        set => this.RaiseAndSetIfChanged(ref _keepAwake, value);
    }

    public string ClipboardDraft
    {
        get => _clipboardDraft;
        set => this.RaiseAndSetIfChanged(ref _clipboardDraft, value);
    }

    public string ClipboardSummary
    {
        get => _clipboardSummary;
        private set => this.RaiseAndSetIfChanged(ref _clipboardSummary, value);
    }

    public string InputIdle
    {
        get => _inputIdle;
        private set => this.RaiseAndSetIfChanged(ref _inputIdle, value);
    }

    public WindowSnapshot SelectedWindow
    {
        get => _selectedWindow;
        set => this.RaiseAndSetIfChanged(ref _selectedWindow, value);
    }

    public int WindowCount => Windows.Count;

    public int ProcessCount => Processes.Count;

    public int DisplayCount => Displays.Count;

    public int EventCount => _allEvents.Count;

    public IDisposable Activate()
    {
        var activation = new CompositeDisposable();
        var inputSubscription = new SerialDisposable();
        activation.Add(inputSubscription);
        var keepAwakeWasEnabled = false;
        var synchronizationContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
        var uiScheduler = new SynchronizationContextScheduler(synchronizationContext);

        activation.Add(_service.StartPassiveMonitoring());
        activation.Add(_service.Events
            .ObserveOn(uiScheduler)
            .Subscribe(AddEvent, exception => Status = "Event stream error: " + exception.Message));

        activation.Add(this.WhenAnyValue(viewModel => viewModel.InputCaptureEnabled)
            .DistinctUntilChanged()
            .Subscribe(enabled =>
            {
                inputSubscription.Disposable = enabled ? _service.StartInputCapture() : Disposable.Empty;
            }));

        activation.Add(this.WhenAnyValue(viewModel => viewModel.KeepAwake)
            .DistinctUntilChanged()
            .Subscribe(enabled =>
            {
                ExecuteSafely("Power policy", () => _service.SetKeepAwake(enabled));
                keepAwakeWasEnabled = enabled;
            }));

        activation.Add(Observable.Interval(TimeSpan.FromSeconds(1D), uiScheduler)
            .StartWith(0L)
            .Subscribe(_ => UpdateInputIdle()));

        activation.Add(Disposable.Create(() =>
        {
            if (keepAwakeWasEnabled)
            {
                ExecuteSafely("Power policy", () => _service.SetKeepAwake(false));
            }
        }));

        RefreshAll();
        return activation;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _lifetime.Dispose();
    }

    private static void ReplaceItems<T>(BindingList<T> target, IEnumerable<T> items)
    {
        target.RaiseListChangedEvents = false;
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }

        target.RaiseListChangedEvents = true;
        target.ResetBindings();
    }

    private void AddEvent(OperationsEvent item)
    {
        if (EventCapturePaused)
        {
            return;
        }

        _allEvents.Insert(0, item);
        if (_allEvents.Count > MaximumEventCount)
        {
            _allEvents.RemoveAt(_allEvents.Count - 1);
        }

        RebuildFilteredEvents();
        this.RaisePropertyChanged(nameof(EventCount));
        Status = item.Severity == "Unavailable" || item.Severity == "Warning" ? item.Message : "Monitoring Windows activity";
    }

    private void BringSelectedToFront()
    {
        if (SelectedWindow != null)
        {
            ExecuteSafely("Foreground window", () => _service.BringWindowToFront(SelectedWindow));
        }
    }

    private void ClearEvents()
    {
        _allEvents.Clear();
        FilteredEvents.Clear();
        this.RaisePropertyChanged(nameof(EventCount));
        Status = "Event stream cleared";
    }

    private void CopyReport()
    {
        ExecuteSafely("Diagnostics copy", () =>
        {
            _service.WriteClipboardText(_service.BuildDiagnosticsReport(WindowCount, ProcessCount, DisplayCount));
            RefreshClipboard();
        });
    }

    private void ExecuteSafely(string operation, Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            Status = operation + " unavailable: " + exception.Message;
            _service.Publish("Capability", "Unavailable", Status);
        }
    }

    private void PlayNotification() => ExecuteSafely("System sound", _service.PlayNotification);

    private void RebuildFilteredEvents()
    {
        var category = EventCategory;
        var search = EventSearch ?? string.Empty;
        var filtered = _allEvents.Where(item =>
                (string.Equals(category, "All", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(category, item.Category, StringComparison.OrdinalIgnoreCase))
                && (search.Length == 0
                    || item.Message.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                    || item.Severity.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0))
            .Take(500)
            .ToArray();
        ReplaceItems(FilteredEvents, filtered);
    }

    private void RefreshAll()
    {
        Status = "Refreshing local diagnostics…";
        RefreshWindows();
        RefreshProcesses();
        RefreshDisplays();
        RefreshClipboard();
        ExecuteSafely("Capability scan", () => ReplaceItems(Capabilities, _service.GetCapabilities()));
        LastRefresh = "Updated " + DateTime.Now.ToString("T");
        Status = "Live diagnostics online";
    }

    private void RefreshClipboard()
    {
        ExecuteSafely("Clipboard read", () => ClipboardSummary = _service.ReadClipboardSummary());
    }

    private void RefreshDisplays()
    {
        ExecuteSafely("Display scan", () =>
        {
            ReplaceItems(Displays, _service.GetDisplays());
            this.RaisePropertyChanged(nameof(DisplayCount));
        });
    }

    private void RefreshProcesses()
    {
        ExecuteSafely("Process scan", () =>
        {
            ReplaceItems(Processes, _service.GetProcesses());
            this.RaisePropertyChanged(nameof(ProcessCount));
        });
    }

    private void RefreshWindows()
    {
        ExecuteSafely("Window scan", () =>
        {
            ReplaceItems(Windows, _service.GetWindows());
            this.RaisePropertyChanged(nameof(WindowCount));
        });
    }

    private void RestoreSelectedWindow()
    {
        if (SelectedWindow != null)
        {
            ExecuteSafely("Restore window", () => _service.RestoreWindow(SelectedWindow));
        }
    }

    private void UpdateInputIdle()
    {
        ExecuteSafely("Last input query", () =>
        {
            var idle = CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.NativeInput.LastInputTimeSpan;
            InputIdle = idle == TimeSpan.MaxValue ? "Unavailable" : idle.TotalSeconds.ToString("0") + " seconds";
        });
    }

    private void WriteClipboard()
    {
        ExecuteSafely("Clipboard write", () =>
        {
            _service.WriteClipboardText(ClipboardDraft);
            RefreshClipboard();
        });
    }
}
