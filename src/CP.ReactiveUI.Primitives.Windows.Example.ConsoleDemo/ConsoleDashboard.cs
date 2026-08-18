// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using CP.ReactiveUI.Primitives.Windows.Desktop.Display;
using ReactiveUI.Primitives;

namespace CP.ReactiveUI.Primitives.Windows.Example.ConsoleDemo;

/// <summary>Renders and operates the interactive diagnostics command loop.</summary>
internal sealed class ConsoleDashboard : IDisposable
{
    private const int DefaultListCount = 10;
    private static readonly object ConsoleSync = new();
    private readonly DiagnosticsViewModel _viewModel;
    private readonly DiagnosticLog _log;
    private readonly List<string> _history = new();
    private readonly IDisposable _liveLogSubscription;
    private CancellationTokenSource _activeCommand;
    private bool _exitRequested;
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="ConsoleDashboard"/> class.</summary>
    /// <param name="viewModel">The reactive dashboard state.</param>
    /// <param name="log">The event log.</param>
    public ConsoleDashboard(DiagnosticsViewModel viewModel, DiagnosticLog log)
    {
        _viewModel = viewModel;
        _log = log;
        _liveLogSubscription = log.Entries.Subscribe(RenderLiveEvent);
    }

    /// <summary>Runs the command loop until the user exits.</summary>
    /// <returns>The process exit code.</returns>
    public async Task<int> RunAsync()
    {
        ConsoleCancelEventHandler cancelHandler = OnCancelKeyPress;
        Console.CancelKeyPress += cancelHandler;
        try
        {
            RenderBanner();
            RenderHelp(compact: true);
            try
            {
                await RefreshAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _log.Warning($"Initial snapshot is unavailable: {exception.Message}");
            }

            while (!_exitRequested)
            {
                WritePrompt();
                var input = Console.ReadLine();
                if (input is null)
                {
                    break;
                }

                input = input.Trim();
                if (input.Length == 0)
                {
                    continue;
                }

                _history.Add(input);
                var cancellation = new CancellationTokenSource();
                _activeCommand = cancellation;
                try
                {
                    await DispatchAsync(input, cancellation.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    WriteLine("Command cancelled.", ConsoleColor.Yellow);
                }
                catch (Exception exception)
                {
                    _log.Error(exception.Message);
                    WriteLine($"Command failed: {exception.Message}", ConsoleColor.Red);
                }
                finally
                {
                    _activeCommand = null;
                    cancellation.Dispose();
                }
            }

            WriteLine("Dashboard stopped cleanly.", ConsoleColor.Cyan);
            return 0;
        }
        finally
        {
            Console.CancelKeyPress -= cancelHandler;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _activeCommand?.Cancel();
        _activeCommand?.Dispose();
        _activeCommand = null;
        _liveLogSubscription.Dispose();
    }

    private async Task DispatchAsync(string input, CancellationToken cancellationToken)
    {
        var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].ToLowerInvariant();
        switch (command)
        {
            case "help":
            case "?":
                RenderHelp(compact: false);
                break;
            case "status":
                RenderSnapshot(_viewModel.LastSnapshot);
                break;
            case "refresh":
                await RefreshAsync(cancellationToken).ConfigureAwait(false);
                break;
            case "processes":
                await RenderProcessesAsync(ParseCount(parts), cancellationToken).ConfigureAwait(false);
                break;
            case "windows":
                await RenderWindowsAsync(ParseCount(parts), cancellationToken).ConfigureAwait(false);
                break;
            case "reactive":
            case "signals":
                await RenderLeanSignalsAsync(ParseCount(parts), cancellationToken).ConfigureAwait(false);
                break;
            case "displays":
                RenderDisplays();
                break;
            case "clipboard":
                WriteLine(DiagnosticsService.GetClipboardDescription(includePreview: true), ConsoleColor.White);
                break;
            case "environment":
            case "env":
                RenderEnvironment();
                break;
            case "cursor":
                WriteLine($"Cursor (live 500 ms sample): {_viewModel.Cursor}", ConsoleColor.White);
                break;
            case "citrix":
                WriteLine(DiagnosticsService.GetCitrixDescription(), ConsoleColor.White);
                break;
            case "watch":
                await SetWatchAsync(parts, cancellationToken).ConfigureAwait(false);
                break;
            case "awake":
                await SetAwakeAsync(parts, cancellationToken).ConfigureAwait(false);
                break;
            case "wait":
                await WaitAsync(parts, cancellationToken).ConfigureAwait(false);
                break;
            case "history":
                RenderHistory(ParseCount(parts));
                break;
            case "events":
            case "logs":
                RenderLog(ParseCount(parts));
                break;
            case "about":
                RenderAbout();
                break;
            case "clear":
            case "cls":
                TryClear();
                RenderBanner();
                break;
            case "quit":
            case "exit":
                _exitRequested = true;
                break;
            default:
                WriteLine($"Unknown command '{parts[0]}'. Type 'help' for the command list.", ConsoleColor.Yellow);
                break;
        }
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var snapshot = await _viewModel.Refresh.Execute().ToTask(cancellationToken).ConfigureAwait(false);
        RenderSnapshot(snapshot);
    }

    private async Task RenderProcessesAsync(int count, CancellationToken cancellationToken)
    {
        var rows = await DiagnosticsService.GetProcessesAsync(count, cancellationToken).ConfigureAwait(false);
        WriteLine("PID     WORKING SET  PROCESS                   WINDOW", ConsoleColor.Cyan);
        foreach (var row in rows)
        {
            WriteLine($"{row.Id,6}  {DiagnosticsService.FormatBytes(row.WorkingSet),11}  {Trim(row.Name, 24),-24}  {Trim(row.WindowTitle, 60)}");
        }
    }

    private async Task RenderWindowsAsync(int count, CancellationToken cancellationToken)
    {
        var rows = await DiagnosticsService.GetWindowsAsync(count, cancellationToken).ConfigureAwait(false);
        WriteLine($"Visible top-level windows ({rows.Count} shown)", ConsoleColor.Cyan);
        foreach (var row in rows)
        {
            WriteLine(row);
        }
    }

    private async Task RenderLeanSignalsAsync(int count, CancellationToken cancellationToken)
    {
        var rows = await DiagnosticsService.GetLeanSignalHandlesAsync(count, cancellationToken).ConfigureAwait(false);
        WriteLine("Handles streamed by CP.ReactiveUI.Primitives.Windows and Signal<T>:", ConsoleColor.Cyan);
        WriteLine(string.Join("  ", rows));
    }

    private async Task SetWatchAsync(string[] parts, CancellationToken cancellationToken)
    {
        var enabled = ParseToggle(parts, _viewModel.IsWatching);
        _ = await _viewModel.SetWatching.Execute(enabled).ToTask(cancellationToken).ConfigureAwait(false);
        WriteLine($"Automatic three-second refresh is {(enabled ? "on" : "off")}.", ConsoleColor.Green);
    }

    private async Task SetAwakeAsync(string[] parts, CancellationToken cancellationToken)
    {
        var enabled = ParseToggle(parts, _viewModel.IsKeepingAwake);
        _ = await _viewModel.SetKeepingAwake.Execute(enabled).ToTask(cancellationToken).ConfigureAwait(false);
        WriteLine($"Safe system-sleep request is {(enabled ? "active" : "released")}.", ConsoleColor.Green);
    }

    private async Task WaitAsync(string[] parts, CancellationToken cancellationToken)
    {
        if (parts.Length < 2 || !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) || seconds < 0 || seconds > 300)
        {
            WriteLine("Usage: wait <seconds>, from 0 through 300. Press Ctrl+C to cancel.", ConsoleColor.Yellow);
            return;
        }

        var result = await _viewModel.Wait.Execute(TimeSpan.FromSeconds(seconds)).ToTask(cancellationToken).ConfigureAwait(false);
        WriteLine(result, ConsoleColor.Green);
    }

    private void RenderSnapshot(DiagnosticSnapshot snapshot)
    {
        if (snapshot is null)
        {
            WriteLine("No snapshot has been captured yet. Use 'refresh'.", ConsoleColor.Yellow);
            return;
        }

        WriteLine($"\n  WINDOWS DIAGNOSTIC SNAPSHOT  {snapshot.CapturedAt:yyyy-MM-dd HH:mm:ss zzz}", ConsoleColor.Cyan);
        WriteKeyValue("Reactive state", _viewModel.ActivityText);
        WriteKeyValue("Windows", snapshot.WindowsVersion);
        WriteKeyValue("CLR", snapshot.RuntimeVersion);
        WriteKeyValue("Identity", snapshot.Identity);
        WriteKeyValue("Process", snapshot.CurrentProcess);
        WriteKeyValue("Uptime", snapshot.ProcessUptime.ToString("g", CultureInfo.InvariantCulture));
        WriteKeyValue("Inventory", $"{snapshot.ProcessCount} processes | {snapshot.WindowCount} top-level windows | {snapshot.DisplayCount} displays");
        WriteKeyValue("Desktop", snapshot.DesktopBounds);
        WriteKeyValue("Cursor", snapshot.Cursor);
        WriteKeyValue("Foreground", snapshot.ForegroundWindow);
        WriteKeyValue("Clipboard", snapshot.Clipboard);
        WriteKeyValue("Native events", _viewModel.NativeEventCount.ToString(CultureInfo.InvariantCulture));
    }

    private static void RenderAbout()
    {
        WriteLine("ReactiveUI 24 console architecture", ConsoleColor.Cyan);
        WriteLine("  ReactiveObject + RaiseAndSetIfChanged reactive state");
        WriteLine("  ReactiveCommand sync/async commands, IsExecuting, ThrownExceptions, cancellation");
        WriteLine("  IActivatableViewModel + ViewModelActivator + WhenActivated/DisposeWith lifetimes");
        WriteLine("  WhenAnyValue + ObservableAsPropertyHelper derived status");
        WriteLine("  Lean Signal<T>, Signal.Every, Map, Keep, Unique, FlatMap, Recover, and ToTask pipelines");
        WriteLine("  Local Windows, Core, and Integrations APIs with no compatibility distribution");
    }

    private static void RenderDisplays()
    {
        var displays = DisplayTopology.GetSnapshot();
        WriteLine($"Displays ({displays.Count}); virtual desktop {DisplayTopology.ScreenBounds}", ConsoleColor.Cyan);
        foreach (var display in displays)
        {
            WriteLine($"  #{display.Index} {(display.IsPrimary ? "primary" : "secondary"),-9} {display.DeviceName,-18} bounds {display.Bounds} work {display.WorkingArea}");
        }
    }

    private static void RenderEnvironment()
    {
        WriteLine("Read-only environment diagnostics", ConsoleColor.Cyan);
        WriteKeyValue("Current directory", Environment.CurrentDirectory);
        WriteKeyValue("System directory", Environment.SystemDirectory);
        WriteKeyValue("Command line", Environment.CommandLine);
        WriteKeyValue("Processors", Environment.ProcessorCount.ToString(CultureInfo.InvariantCulture));
        WriteKeyValue("64-bit OS/process", $"{Environment.Is64BitOperatingSystem}/{Environment.Is64BitProcess}");
        WriteKeyValue("Interactive", Environment.UserInteractive.ToString(CultureInfo.InvariantCulture));
    }

    private void RenderHistory(int count)
    {
        var start = Math.Max(0, _history.Count - count);
        for (var index = start; index < _history.Count; index++)
        {
            WriteLine($"{index + 1,4}  {_history[index]}");
        }
    }

    private void RenderLog(int count)
    {
        foreach (var entry in _log.Snapshot(count))
        {
            RenderEvent(entry);
        }
    }

    private static void RenderHelp(bool compact)
    {
        if (compact)
        {
            WriteLine("Type 'help' for commands; 'refresh' captures now; 'quit' exits. Ctrl+C cancels work.", ConsoleColor.DarkGray);
            return;
        }

        WriteLine("\nCOMMANDS", ConsoleColor.Cyan);
        WriteLine("  status | refresh              Show the current/capture a new snapshot");
        WriteLine("  processes [n] | windows [n]   Inspect safe process/window metadata");
        WriteLine("  signals [n] | reactive [n]    Stream handles through lean Primitives signals");
        WriteLine("  displays | clipboard | env    Inspect display, clipboard, environment state");
        WriteLine("  cursor | citrix               Inspect input position or optional WFAPI");
        WriteLine("  watch [on|off]                Toggle periodic reactive snapshots");
        WriteLine("  awake [on|off]                Toggle a reversible system-sleep request");
        WriteLine("  wait <seconds>                Run a cancellable ReactiveCommand");
        WriteLine("  history [n] | events [n]      Show command history or bounded live log");
        WriteLine("  about | clear | quit          Architecture, screen, and lifecycle");
    }

    private static void RenderBanner()
    {
        WriteLine("╔══════════════════════════════════════════════════════════════════════╗", ConsoleColor.Cyan);
        WriteLine("║  CP.REACTIVEUI.PRIMITIVES.WINDOWS  •  DIAGNOSTICS & AUTOMATION     ║", ConsoleColor.Cyan);
        WriteLine("╚══════════════════════════════════════════════════════════════════════╝", ConsoleColor.Cyan);
    }

    private void RenderLiveEvent(DiagnosticLogEntry entry)
    {
        if (_disposed)
        {
            return;
        }

        RenderEvent(entry);
    }

    private static void RenderEvent(DiagnosticLogEntry entry)
    {
        var color = entry.Level == "ERROR" ? ConsoleColor.Red : entry.Level == "WARN" ? ConsoleColor.Yellow : ConsoleColor.DarkGray;
        WriteLine($"[{entry.Timestamp:HH:mm:ss}] {entry.Level,-5} {entry.Message}", color);
    }

    private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs args)
    {
        args.Cancel = true;
        var command = _activeCommand;
        if (command is not null)
        {
            command.Cancel();
            return;
        }

        _exitRequested = true;
        WriteLine("Exit requested; press Enter if the prompt is waiting.", ConsoleColor.Yellow);
    }

    private static bool ParseToggle(string[] parts, bool current)
    {
        if (parts.Length < 2)
        {
            return !current;
        }

        if (string.Equals(parts[1], "on", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(parts[1], "off", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        throw new ArgumentException("Expected 'on' or 'off'.");
    }

    private static int ParseCount(string[] parts)
    {
        if (parts.Length < 2 || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var count))
        {
            return DefaultListCount;
        }

        return Math.Max(1, Math.Min(50, count));
    }

    private static string Trim(string value, int length)
    {
        value ??= string.Empty;
        return value.Length <= length ? value : $"{value.Substring(0, length - 3)}...";
    }

    private static void WriteKeyValue(string key, string value) => WriteLine($"  {key,-14} {value}");

    private static void WritePrompt()
    {
        lock (ConsoleSync)
        {
            TrySetColor(ConsoleColor.Green);
            Console.Write("\ndiag> ");
            TryResetColor();
        }
    }

    private static void WriteLine(string value, ConsoleColor? color = null)
    {
        lock (ConsoleSync)
        {
            if (color.HasValue)
            {
                TrySetColor(color.Value);
            }

            Console.WriteLine(value);
            if (color.HasValue)
            {
                TryResetColor();
            }
        }
    }

    private static void TrySetColor(ConsoleColor color)
    {
        try
        {
            Console.ForegroundColor = color;
        }
        catch (IOException)
        {
        }
    }

    private static void TryResetColor()
    {
        try
        {
            Console.ResetColor();
        }
        catch (IOException)
        {
        }
    }

    private static void TryClear()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
        }
    }
}
