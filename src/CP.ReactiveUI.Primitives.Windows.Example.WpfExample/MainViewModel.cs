// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Windows.Media;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;
using ReactiveUI.Primitives.Signals;

namespace CP.ReactiveUI.Primitives.Windows.Example.WpfExample;

/// <summary>Reactive presentation model for safe Windows interaction experiments.</summary>
public sealed class MainViewModel : ReactiveObject, IActivatableViewModel
{
    private const int MaximumEventCount = 500;
    private readonly IWindowsLabService _service;
    private readonly ISequencer _mainSequencer;
    private readonly List<LabEvent> _allEvents = new();
    private readonly List<WindowSnapshot> _allWindows = new();
    private readonly ObservableAsPropertyHelper<string> _activitySummary;
    private readonly ObservableAsPropertyHelper<string> _guardSummary;
    private bool _observeKeyboard;
    private bool _observeMouse;
    private bool _observeWindowEvents;
    private string _eventFilter = string.Empty;
    private string _windowFilter = string.Empty;
    private string _windowSummary = "Waiting for HWND initialization…";
    private string _displaySummary = "Querying display topology…";
    private string _foregroundSummary = "Querying the foreground window…";
    private string _statusText = "Ready. Global observations are opt-in and read-only.";
    private string _explorerSummary = "Select Discover to inspect visible top-level windows.";
    private int _eventCount;
    private int _windowResultCount;
    private WindowGuardMode _guardMode;
    private WindowSnapshot _selectedWindow;
    private ImageSource _cursorPreview;
    private ImageSource _dashboardPreview;

    /// <summary>Initializes the view model with a Windows boundary service.</summary>
    /// <param name="service">Windows laboratory operations.</param>
    internal MainViewModel(IWindowsLabService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _mainSequencer = new SynchronizationContextSequencer(
            SynchronizationContext.Current ?? throw new InvalidOperationException("The view model must be created on the WPF UI thread."));

        RefreshEnvironmentCommand = ReactiveCommand.Create(RefreshEnvironment);
        RefreshForegroundCommand = ReactiveCommand.Create(RefreshForeground);
        SavePlacementCommand = ReactiveCommand.Create(SavePlacement);
        RestorePlacementCommand = ReactiveCommand.Create(RestorePlacement);
        CenterWindowCommand = ReactiveCommand.Create(CenterWindow);
        DisableGuardCommand = ReactiveCommand.Create(() => { GuardMode = WindowGuardMode.None; });
        GuardMoveCommand = ReactiveCommand.Create(() => { GuardMode = WindowGuardMode.Move; });
        GuardMoveResizeCommand = ReactiveCommand.Create(() => { GuardMode = WindowGuardMode.MoveAndResize; });
        DiscoverWindowsCommand = ReactiveCommand.CreateFromTask(DiscoverWindowsAsync);
        var canTargetWindow = this.WhenAnyValue(static viewModel => viewModel.SelectedWindow).Select(static window => window is not null);
        BringSelectedToForegroundCommand = ReactiveCommand.CreateFromTask(BringSelectedToForegroundAsync, canTargetWindow);
        ClearEventsCommand = ReactiveCommand.Create(ClearEvents);
        CaptureCursorCommand = ReactiveCommand.Create(CaptureCursor);
        CaptureDashboardCommand = ReactiveCommand.CreateFromTask(CaptureDashboardAsync);

        _guardSummary = this.WhenAnyValue(static viewModel => viewModel.GuardMode)
            .Select(static mode => mode switch
            {
                WindowGuardMode.Move => "MOVE GUARD ACTIVE · resize remains available",
                WindowGuardMode.MoveAndResize => "MOVE + RESIZE GUARD ACTIVE",
                _ => "No interaction guard",
            })
            .ToProperty(this, static viewModel => viewModel.GuardSummary);

        _activitySummary = this.WhenAnyValue(
                static viewModel => viewModel.EventCount,
                static viewModel => viewModel.ObserveKeyboard,
                static viewModel => viewModel.ObserveMouse,
                static viewModel => viewModel.ObserveWindowEvents,
                static (count, keyboard, mouse, windows) =>
                    $"{count} events · {(keyboard || mouse || windows ? "opt-in streams active" : "local streams only")}")
            .ToProperty(this, static viewModel => viewModel.ActivitySummary);

        this.WhenActivated((Action<IDisposable> disposables) =>
        {
            Signal.Merge(
                    SafeStream(_service.ObserveLocalWindowEvents(), "Window messages"),
                    SafeStream(_service.ObserveDpiChanges(), "DPI"),
                    SafeStream(_service.ObserveDisplayChanges(), "Display topology"))
                .ObserveOn(_mainSequencer)
                .Subscribe(AddEvent)
                .TrackWith(disposables);

            ObserveOptIn(
                    this.WhenAnyValue(static viewModel => viewModel.ObserveWindowEvents),
                    _service.ObserveWindowEvents,
                    "WinEvent")
                .Subscribe(AddEvent)
                .TrackWith(disposables);
            ObserveOptIn(
                    this.WhenAnyValue(static viewModel => viewModel.ObserveKeyboard),
                    _service.ObserveKeyboardEvents,
                    "Keyboard hook")
                .Subscribe(AddEvent)
                .TrackWith(disposables);
            ObserveOptIn(
                    this.WhenAnyValue(static viewModel => viewModel.ObserveMouse),
                    _service.ObserveMouseEvents,
                    "Mouse hook")
                .Subscribe(AddEvent)
                .TrackWith(disposables);

            this.WhenAnyValue(static viewModel => viewModel.EventFilter)
                .Throttle(TimeSpan.FromMilliseconds(160), Sequencer.Default)
                .ObserveOn(_mainSequencer)
                .Subscribe(_ => ApplyEventFilter())
                .TrackWith(disposables);
            this.WhenAnyValue(static viewModel => viewModel.WindowFilter)
                .Throttle(TimeSpan.FromMilliseconds(160), Sequencer.Default)
                .ObserveOn(_mainSequencer)
                .Subscribe(_ => ApplyWindowFilter())
                .TrackWith(disposables);

            Signal.Merge(
                RefreshEnvironmentCommand.ThrownExceptions,
                RefreshForegroundCommand.ThrownExceptions,
                SavePlacementCommand.ThrownExceptions,
                RestorePlacementCommand.ThrownExceptions,
                CenterWindowCommand.ThrownExceptions,
                DiscoverWindowsCommand.ThrownExceptions,
                BringSelectedToForegroundCommand.ThrownExceptions,
                CaptureCursorCommand.ThrownExceptions,
                CaptureDashboardCommand.ThrownExceptions)
                .ObserveOn(_mainSequencer)
                .Subscribe(exception => ReportFailure("Command", exception))
                .TrackWith(disposables);

            RefreshEnvironment();
            AddEvent(new LabEvent("Activation", "Subscriptions attached; disposal is tied to view activation."));
        });
    }

    /// <summary>Gets the activation controller used by ReactiveWindow.</summary>
    public ViewModelActivator Activator { get; } = new();

    /// <summary>Gets the filtered event collection.</summary>
    public ObservableCollection<LabEvent> FilteredEvents { get; } = new();

    /// <summary>Gets the filtered top-level window collection.</summary>
    public ObservableCollection<WindowSnapshot> FilteredWindows { get; } = new();

    /// <summary>Gets current display snapshots.</summary>
    public ObservableCollection<DisplaySnapshot> Displays { get; } = new();

    /// <summary>Gets the view interaction that renders the dashboard preview.</summary>
    public Interaction<RxVoid, ImageSource> CaptureDashboard { get; } = new();

    /// <summary>Gets the command that refreshes local HWND, DPI, display and foreground metrics.</summary>
    public ReactiveCommand<RxVoid, RxVoid> RefreshEnvironmentCommand { get; }

    /// <summary>Gets the command that refreshes only the foreground window.</summary>
    public ReactiveCommand<RxVoid, RxVoid> RefreshForegroundCommand { get; }

    /// <summary>Gets the command that stores the current native placement in memory.</summary>
    public ReactiveCommand<RxVoid, RxVoid> SavePlacementCommand { get; }

    /// <summary>Gets the command that restores the in-memory native placement.</summary>
    public ReactiveCommand<RxVoid, RxVoid> RestorePlacementCommand { get; }

    /// <summary>Gets the safe command that centers this window.</summary>
    public ReactiveCommand<RxVoid, RxVoid> CenterWindowCommand { get; }

    /// <summary>Gets the command that removes interaction guards.</summary>
    public ReactiveCommand<RxVoid, RxVoid> DisableGuardCommand { get; }

    /// <summary>Gets the command that blocks movement only.</summary>
    public ReactiveCommand<RxVoid, RxVoid> GuardMoveCommand { get; }

    /// <summary>Gets the command that blocks movement and resizing.</summary>
    public ReactiveCommand<RxVoid, RxVoid> GuardMoveResizeCommand { get; }

    /// <summary>Gets the command that discovers visible top-level windows.</summary>
    public ReactiveCommand<RxVoid, RxVoid> DiscoverWindowsCommand { get; }

    /// <summary>Gets the explicitly invoked command that requests foreground for the selected window.</summary>
    public ReactiveCommand<RxVoid, RxVoid> BringSelectedToForegroundCommand { get; }

    /// <summary>Gets the command that clears the local event history.</summary>
    public ReactiveCommand<RxVoid, RxVoid> ClearEventsCommand { get; }

    /// <summary>Gets the command that captures the current cursor visual.</summary>
    public ReactiveCommand<RxVoid, RxVoid> CaptureCursorCommand { get; }

    /// <summary>Gets the command that renders this dashboard to memory.</summary>
    public ReactiveCommand<RxVoid, RxVoid> CaptureDashboardCommand { get; }

    /// <summary>Gets or sets whether read-only global keyboard events are observed.</summary>
    public bool ObserveKeyboard
    {
        get => _observeKeyboard;
        set => this.RaiseAndSetIfChanged(ref _observeKeyboard, value);
    }

    /// <summary>Gets or sets whether sampled read-only global mouse events are observed.</summary>
    public bool ObserveMouse
    {
        get => _observeMouse;
        set => this.RaiseAndSetIfChanged(ref _observeMouse, value);
    }

    /// <summary>Gets or sets whether global window lifecycle and title events are observed.</summary>
    public bool ObserveWindowEvents
    {
        get => _observeWindowEvents;
        set => this.RaiseAndSetIfChanged(ref _observeWindowEvents, value);
    }

    /// <summary>Gets or sets the reactive event filter.</summary>
    public string EventFilter
    {
        get => _eventFilter;
        set => this.RaiseAndSetIfChanged(ref _eventFilter, value ?? string.Empty);
    }

    /// <summary>Gets or sets the reactive top-level window filter.</summary>
    public string WindowFilter
    {
        get => _windowFilter;
        set => this.RaiseAndSetIfChanged(ref _windowFilter, value ?? string.Empty);
    }

    /// <summary>Gets or sets the active local interaction guard.</summary>
    public WindowGuardMode GuardMode
    {
        get => _guardMode;
        set => this.RaiseAndSetIfChanged(ref _guardMode, value);
    }

    /// <summary>Gets or sets the selected native window snapshot.</summary>
    public WindowSnapshot SelectedWindow
    {
        get => _selectedWindow;
        set => this.RaiseAndSetIfChanged(ref _selectedWindow, value);
    }

    /// <summary>Gets or sets the current cursor preview.</summary>
    public ImageSource CursorPreview
    {
        get => _cursorPreview;
        private set => this.RaiseAndSetIfChanged(ref _cursorPreview, value);
    }

    /// <summary>Gets or sets the in-memory dashboard preview.</summary>
    public ImageSource DashboardPreview
    {
        get => _dashboardPreview;
        private set => this.RaiseAndSetIfChanged(ref _dashboardPreview, value);
    }

    /// <summary>Gets or sets the local window summary.</summary>
    public string WindowSummary
    {
        get => _windowSummary;
        private set => this.RaiseAndSetIfChanged(ref _windowSummary, value);
    }

    /// <summary>Gets or sets the display topology summary.</summary>
    public string DisplaySummary
    {
        get => _displaySummary;
        private set => this.RaiseAndSetIfChanged(ref _displaySummary, value);
    }

    /// <summary>Gets or sets the foreground window summary.</summary>
    public string ForegroundSummary
    {
        get => _foregroundSummary;
        private set => this.RaiseAndSetIfChanged(ref _foregroundSummary, value);
    }

    /// <summary>Gets or sets the status bar text.</summary>
    public string StatusText
    {
        get => _statusText;
        private set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    /// <summary>Gets or sets the explorer result summary.</summary>
    public string ExplorerSummary
    {
        get => _explorerSummary;
        private set => this.RaiseAndSetIfChanged(ref _explorerSummary, value);
    }

    /// <summary>Gets the derived guard state label.</summary>
    public string GuardSummary => _guardSummary.Value;

    /// <summary>Gets the derived activity label.</summary>
    public string ActivitySummary => _activitySummary.Value;

    /// <summary>Gets a framework summary compatible with all target frameworks.</summary>
    public string RuntimeSummary => $"CLR {Environment.Version} · ReactiveUI 24";

    /// <summary>Gets or sets the retained event count.</summary>
    public int EventCount
    {
        get => _eventCount;
        private set => this.RaiseAndSetIfChanged(ref _eventCount, value);
    }

    /// <summary>Gets or sets the filtered discovery count.</summary>
    public int WindowResultCount
    {
        get => _windowResultCount;
        private set => this.RaiseAndSetIfChanged(ref _windowResultCount, value);
    }

    internal IObservable<RxVoid> CreateWindowGuard(WindowGuardMode mode) => _service.CreateWindowGuard(mode);

    internal void ReportGuardFailure(Exception exception) => ReportFailure("Window guard", exception);

    private IObservable<LabEvent> ObserveOptIn(
        IObservable<bool> enabled,
        Func<IObservable<LabEvent>> sourceFactory,
        string sourceName) =>
        enabled.DistinctUntilChanged()
            .Select(isEnabled => isEnabled
                ? SafeStream(sourceFactory(), sourceName)
                : Signal.Empty<LabEvent>())
            .Switch()
            .ObserveOn(_mainSequencer);

    private static IObservable<LabEvent> SafeStream(IObservable<LabEvent> source, string sourceName) =>
        source.Catch<LabEvent, Exception>(exception =>
            Signal.Return(new LabEvent(sourceName, $"Unavailable: {exception.Message}")));

    private void RefreshEnvironment()
    {
        try
        {
            var snapshot = _service.GetEnvironment();
            WindowSummary = snapshot.WindowSummary;
            DisplaySummary = snapshot.DisplaySummary;
            ForegroundSummary = snapshot.ForegroundSummary;
            Displays.Clear();
            foreach (var display in snapshot.Displays)
            {
                Displays.Add(display);
            }

            StatusText = "Environment refreshed through Core and lean Windows APIs.";
            AddEvent(new LabEvent("Environment", DisplaySummary));
        }
        catch (Exception exception)
        {
            ReportFailure("Environment", exception);
        }
    }

    private void RefreshForeground()
    {
        try
        {
            var foreground = _service.GetForegroundWindow();
            ForegroundSummary = foreground is null
                ? "No foreground window was reported."
                : $"{foreground.Caption} · PID {foreground.ProcessId} · {foreground.HandleText}";
            StatusText = "Foreground snapshot refreshed without changing focus.";
        }
        catch (Exception exception)
        {
            ReportFailure("Foreground", exception);
        }
    }

    private void SavePlacement()
    {
        try
        {
            _service.SavePlacement();
            StatusText = "Native WindowPlacement saved in memory.";
            AddEvent(new LabEvent("Placement", "Current position and show state saved."));
        }
        catch (Exception exception)
        {
            ReportFailure("Save placement", exception);
        }
    }

    private void RestorePlacement()
    {
        try
        {
            StatusText = _service.RestorePlacement()
                ? "Saved native placement restored."
                : "Save a placement before restoring it.";
            AddEvent(new LabEvent("Placement", StatusText));
        }
        catch (Exception exception)
        {
            ReportFailure("Restore placement", exception);
        }
    }

    private void CenterWindow()
    {
        try
        {
            _service.CenterWindow();
            StatusText = "Window centered inside the current work area.";
        }
        catch (Exception exception)
        {
            ReportFailure("Center window", exception);
        }
    }

    private async Task DiscoverWindowsAsync()
    {
        try
        {
            var snapshots = await Task.Run(_service.DiscoverWindows);
            _allWindows.Clear();
            _allWindows.AddRange(snapshots);
            ApplyWindowFilter();
            StatusText = $"Discovered {_allWindows.Count} visible top-level window(s).";
            AddEvent(new LabEvent("Discovery", StatusText));
        }
        catch (Exception exception)
        {
            ReportFailure("Window discovery", exception);
        }
    }

    private async Task BringSelectedToForegroundAsync()
    {
        try
        {
            var selected = SelectedWindow;
            await _service.BringToForegroundAsync(selected);
            StatusText = selected is null ? "Select a window first." : $"Foreground requested for {selected.Caption}.";
            AddEvent(new LabEvent("Foreground", StatusText));
        }
        catch (Exception exception)
        {
            ReportFailure("Foreground request", exception);
        }
    }

    private void CaptureCursor()
    {
        try
        {
            var snapshot = _service.CaptureCursor();
            CursorPreview = snapshot.Image;
            StatusText = snapshot.Description;
            AddEvent(new LabEvent("Cursor helper", StatusText));
        }
        catch (Exception exception)
        {
            ReportFailure("Cursor capture", exception);
        }
    }

    private async Task CaptureDashboardAsync()
    {
        try
        {
            DashboardPreview = await CaptureDashboard.Handle(RxVoid.Default);
            StatusText = "Dashboard rendered to an in-memory WPF BitmapSource.";
            AddEvent(new LabEvent("Visual", StatusText));
        }
        catch (Exception exception)
        {
            ReportFailure("Dashboard preview", exception);
        }
    }

    private void AddEvent(LabEvent item)
    {
        _allEvents.Insert(0, item);
        if (_allEvents.Count > MaximumEventCount)
        {
            _allEvents.RemoveAt(_allEvents.Count - 1);
        }

        EventCount = _allEvents.Count;
        ApplyEventFilter();
    }

    private void ClearEvents()
    {
        _allEvents.Clear();
        FilteredEvents.Clear();
        EventCount = 0;
        StatusText = "Local event history cleared; active streams remain subscribed.";
    }

    private void ApplyEventFilter()
    {
        FilteredEvents.Clear();
        foreach (var item in _allEvents.Where(item => Matches(item.Source, EventFilter) || Matches(item.Message, EventFilter)))
        {
            FilteredEvents.Add(item);
        }
    }

    private void ApplyWindowFilter()
    {
        FilteredWindows.Clear();
        foreach (var item in _allWindows.Where(item =>
                     Matches(item.Caption, WindowFilter)
                     || Matches(item.ClassName, WindowFilter)
                     || Matches(item.ProcessId.ToString(), WindowFilter)
                     || Matches(item.HandleText, WindowFilter)))
        {
            FilteredWindows.Add(item);
        }

        WindowResultCount = FilteredWindows.Count;
        ExplorerSummary = $"{WindowResultCount} of {_allWindows.Count} window(s) match · selection is a read-only snapshot.";
    }

    private void ReportFailure(string source, Exception exception)
    {
        StatusText = $"{source}: {exception.Message}";
        AddEvent(new LabEvent(source, $"Error · {exception.Message}"));
    }

    private static bool Matches(string value, string filter)
    {
#if NETFRAMEWORK
        return string.IsNullOrWhiteSpace(filter)
            || (!string.IsNullOrEmpty(value) && value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
#else
        return string.IsNullOrWhiteSpace(filter)
            || (!string.IsNullOrEmpty(value) && value.Contains(filter, StringComparison.OrdinalIgnoreCase));
#endif
    }
}
