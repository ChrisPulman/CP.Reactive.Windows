// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CP.ReactiveUI.Primitives.Windows.Integrations.Browser;
using CP.ReactiveUI.Primitives.Windows.Native;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Composition;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Keyboard;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Mouse;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Media;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Media.Enums;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Power;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Reactive.Example.FormsExample;

internal sealed class WindowsOperationsService : IDisposable
{
    private readonly ISubject<OperationsEvent> _events = Subject.Synchronize(new Subject<OperationsEvent>());
    private bool _disposed;

    public IObservable<OperationsEvent> Events => _events.AsObservable();

    public IDisposable StartPassiveMonitoring()
    {
        var subscriptions = new CompositeDisposable();
        Subscribe("Message window", SharedMessageWindow.ObserveHandleChanges, handle =>
            Publish("Lifecycle", "Info", "Shared native message window ready: 0x" + handle.ToString("X")), subscriptions);
        Subscribe("Display topology", DisplayTopology.ObserveChanges, displays =>
            Publish("Display", "Info", "Display topology changed; " + displays.Count + " monitor(s) detected."), subscriptions);
#if NETFRAMEWORK
        Subscribe("Clipboard listener", () => ClipboardNative.ClipboardUpdateEvents, update =>
            Publish("Clipboard", "Info", "Clipboard #" + update.Id + ": " + string.Join(", ", update.Formats)), subscriptions);
#else
        Publish("Capability", "Unavailable", "Reactive clipboard notifications are disabled on modern targets because the local source-generated listener entry point is unavailable; clipboard inspection and actions remain active.");
#endif
        Subscribe("Environment listener", () => EnvironmentMonitor.EnvironmentChangeEvents, change =>
            Publish("Environment", "Info", (change.Area ?? "System settings") + " changed (" + change.SystemParametersInfoAction + ")."), subscriptions);
        Subscribe("Power listener", () => PowerBroadcastListener.PowerBroadcastEvents, powerEvent =>
            Publish("Power", "Info", "Power broadcast: " + powerEvent), subscriptions);
        Subscribe("Window lifecycle hook", WinEventHook.ObserveWindowLifecycleEvents, windowEvent =>
            Publish("Window", "Trace", windowEvent.WinEvent + " on 0x" + windowEvent.Window.Handle.ToInt64().ToString("X")), subscriptions);
        Subscribe("Window title hook", WinEventHook.ObserveWindowTitleChanges, windowEvent =>
            Publish("Window", "Trace", "Title changed on 0x" + windowEvent.Window.Handle.ToInt64().ToString("X")), subscriptions);

        TryStartSessionListener(subscriptions);
        Publish("Application", "Success", "Passive Windows diagnostics are active.");
        return subscriptions;
    }

    public IDisposable StartInputCapture()
    {
        var subscriptions = new CompositeDisposable();
        Subscribe("Keyboard hook", () => KeyboardHook.KeyboardHookEvents, key =>
            Publish("Input", "Trace", key.ToString()), subscriptions);
        Subscribe("Mouse hook", () => MouseHook.MouseHookEvents.Sample(TimeSpan.FromMilliseconds(250D)), mouse =>
            Publish("Input", "Trace", mouse.WindowsMessage + " at " + mouse.Point.X + ", " + mouse.Point.Y), subscriptions);
        Publish("Input", "Success", "Opt-in global input diagnostics enabled; events are observed but never blocked.");
        return Disposable.Create(() =>
        {
            subscriptions.Dispose();
            Publish("Input", "Info", "Global input diagnostics disabled and hooks released.");
        });
    }

    public IReadOnlyList<WindowSnapshot> GetWindows()
    {
        var results = new List<WindowSnapshot>();
        foreach (var window in InteropWindowQueryExtensions.GetTopLevelWindows(false).Take(300))
        {
            try
            {
                _ = window.Fill(
                    InteropWindowRetrieveSettings.Info
                    | InteropWindowRetrieveSettings.Caption
                    | InteropWindowRetrieveSettings.Classname
                    | InteropWindowRetrieveSettings.ProcessId
                    | InteropWindowRetrieveSettings.Visible
                    | InteropWindowRetrieveSettings.Minimized
                    | InteropWindowRetrieveSettings.Maximized
                    | InteropWindowRetrieveSettings.ForceUpdate);
                var info = window.Info;
                var processId = window.ProcessId ?? 0;
                results.Add(new WindowSnapshot(
                    window.Handle.ToInt64(),
                    processId,
                    GetProcessName(processId),
                    window.Caption ?? string.Empty,
                    window.Classname ?? string.Empty,
                    info.HasValue ? FormatRectangle(info.Value.Bounds) : "Unavailable",
                    window.IsVisible ?? false,
                    window.IsMinimized ?? false,
                    window.IsMaximized ?? false));
            }
            catch (Exception exception) when (IsRecoverable(exception))
            {
                Publish("Window", "Warning", "Could not inspect window 0x" + window.Handle.ToInt64().ToString("X") + ": " + exception.Message);
            }
        }

        return results.OrderBy(item => item.ProcessName).ThenBy(item => item.Caption).ToArray();
    }

    public IReadOnlyList<ProcessSnapshot> GetProcesses()
    {
        var results = new List<ProcessSnapshot>();
        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                try
                {
                    results.Add(new ProcessSnapshot(
                        process.Id,
                        process.ProcessName,
                        process.WorkingSet64,
                        process.Threads.Count,
                        process.MainWindowTitle ?? string.Empty));
                }
                catch (Exception exception) when (IsRecoverable(exception))
                {
                    Publish("Process", "Warning", "Process details unavailable: " + exception.Message);
                }
            }
        }

        return results.OrderByDescending(item => item.WorkingSet).ToArray();
    }

    public IReadOnlyList<DisplaySnapshot> GetDisplays()
    {
        return DisplayTopology.GetSnapshot()
            .Select(display => new DisplaySnapshot(
                display.DeviceName ?? "Display",
                FormatRectangle(display.Bounds),
                FormatRectangle(display.WorkingArea),
                display.ScreenWidth + " × " + display.ScreenHeight,
                display.IsPrimary))
            .ToArray();
    }

    public IReadOnlyList<CapabilitySnapshot> GetCapabilities()
    {
        var capabilities = new List<CapabilitySnapshot>();
        AddCapability(capabilities, "Windows version", () => WindowsVersion.WinVersion + (WindowsVersion.IsWindows11OrLater ? " (Windows 11+)" : string.Empty));
        AddCapability(capabilities, "Desktop composition", () => DwmApi.IsDwmEnabled ? "Enabled; accent " + DwmApi.ColorizationDrawingColor : "Disabled");
        AddCapability(capabilities, "Display topology", () => DisplayTopology.GetSnapshot().Count + " monitor(s); virtual bounds " + FormatRectangle(DisplayTopology.ScreenBounds));
        AddCapability(capabilities, "Clipboard", () => "Sequence " + ClipboardNative.SequenceNumber + "; owner " + (ClipboardNative.HasOwner ? "present" : "none"));
        AddCapability(capabilities, "Native input", () => "Last input " + FormatDuration(NativeInput.LastInputTimeSpan) + " ago");
        AddCapability(capabilities, "DPI primitives", () => "96→144 scale factor " + DpiCalculator.DpiScaleFactor(96, 144).ToString("0.00"));
        AddCapability(capabilities, "Embedded browser integration", () => "IE engine " + InternetExplorerVersion.Version + "; emulation " + InternetExplorerVersion.GetEmbVersion());
        AddCapability(capabilities, "Reactive message pump", () => "Handle 0x" + SharedMessageWindow.Handle.ToString("X"));
        return capabilities;
    }

    public string ReadClipboardSummary()
    {
        var formats = new List<string>();
        if (ClipboardNative.HasFormat((uint)StandardClipboardFormats.UnicodeText))
        {
            formats.Add("Unicode text");
        }

        if (ClipboardNative.HasFormat((uint)StandardClipboardFormats.Drop))
        {
            formats.Add("File drop");
        }

        var preview = string.Empty;
        if (ClipboardNative.HasFormat((uint)StandardClipboardFormats.UnicodeText))
        {
            using var access = ClipboardNative.Access();
            if (access.CanAccess)
            {
                preview = access.GetAsUnicodeString();
                if (preview.Length > 500)
                {
                    preview = preview.Substring(0, 500) + "…";
                }
            }
        }

        return "Sequence: " + ClipboardNative.SequenceNumber
            + Environment.NewLine + "Owner: " + (ClipboardNative.HasOwner ? "Yes" : "No")
            + Environment.NewLine + "Known formats: " + (formats.Count == 0 ? "Other / none" : string.Join(", ", formats))
            + Environment.NewLine + Environment.NewLine + (string.IsNullOrEmpty(preview) ? "No Unicode text preview available." : preview);
    }

    public void WriteClipboardText(string text)
    {
        using var access = ClipboardNative.Access();
        access.ThrowWhenNoAccess();
        access.SetAsUnicodeString(text ?? string.Empty);
        Publish("Clipboard", "Success", "User-provided text written through ClipboardNative.Access().");
    }

    public void BringWindowToFront(WindowSnapshot snapshot)
    {
        var window = InteropWindowFactory.CreateFor(new IntPtr(snapshot.Handle));
        if (!window.Exists())
        {
            throw new InvalidOperationException("The selected window no longer exists.");
        }

        _ = window.ToForegroundAsync();
        Publish("Action", "Success", "Foreground requested for " + snapshot.HandleHex + ".");
    }

    public void RestoreWindow(WindowSnapshot snapshot)
    {
        var window = InteropWindowFactory.CreateFor(new IntPtr(snapshot.Handle));
        if (!window.Exists())
        {
            throw new InvalidOperationException("The selected window no longer exists.");
        }

        _ = window.Restore();
        Publish("Action", "Success", "Restore requested for " + snapshot.HandleHex + ".");
    }

    public void SetKeepAwake(bool enabled)
    {
        var previous = enabled ? SystemStateApi.PreventSystemSleep() : SystemStateApi.AllowSleep();
        Publish("Power", previous == 0 ? "Warning" : "Success", enabled
            ? "Temporary system-sleep prevention enabled; display policy is unchanged."
            : "Normal system sleep policy restored.");
    }

    public void PlayNotification()
    {
        WinMm.PlaySystemSound(SystemSounds.SystemAsterisk);
        Publish("Media", "Success", "SystemAsterisk played through WinMm.");
    }

    public string BuildDiagnosticsReport(int windowCount, int processCount, int displayCount)
    {
        return "Windows Operations Center" + Environment.NewLine
            + "Captured: " + DateTimeOffset.Now.ToString("O") + Environment.NewLine
            + "OS: " + Environment.OSVersion + Environment.NewLine
            + "Windows API version: " + WindowsVersion.WinVersion + Environment.NewLine
            + "Runtime: " + Environment.Version + Environment.NewLine
            + "Machine: " + Environment.MachineName + Environment.NewLine
            + "Process architecture: " + (Environment.Is64BitProcess ? "64-bit" : "32-bit") + Environment.NewLine
            + "Windows: " + windowCount + Environment.NewLine
            + "Processes: " + processCount + Environment.NewLine
            + "Displays: " + displayCount + Environment.NewLine
            + "Clipboard sequence: " + ClipboardNative.SequenceNumber + Environment.NewLine
            + "DWM: " + (DwmApi.IsDwmEnabled ? "enabled" : "disabled") + Environment.NewLine
            + "Last input idle: " + FormatDuration(NativeInput.LastInputTimeSpan);
    }

    public void Publish(string category, string severity, string message)
    {
        if (!_disposed)
        {
            _events.OnNext(new OperationsEvent(DateTimeOffset.Now, category, severity, message));
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _events.OnCompleted();
        (_events as IDisposable)?.Dispose();
    }

    private static void AddCapability(ICollection<CapabilitySnapshot> capabilities, string name, Func<string> read)
    {
        try
        {
            capabilities.Add(new CapabilitySnapshot(name, "Available", read()));
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            capabilities.Add(new CapabilitySnapshot(name, "Unavailable", exception.Message));
        }
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration == TimeSpan.MaxValue)
        {
            return "unknown";
        }

        return duration.TotalHours >= 1D
            ? duration.TotalHours.ToString("0.0") + " h"
            : duration.TotalMinutes >= 1D
                ? duration.TotalMinutes.ToString("0.0") + " min"
                : duration.TotalSeconds.ToString("0") + " s";
    }

    private static string FormatRectangle(CP.ReactiveUI.Primitives.Windows.Native.Structs.NativeRect rectangle) =>
        rectangle.Left + ", " + rectangle.Top + "  " + rectangle.Width + " × " + rectangle.Height;

    private static string GetProcessName(int processId)
    {
        if (processId <= 0)
        {
            return "System";
        }

        try
        {
            using var process = Process.GetProcessById(processId);
            return process.ProcessName;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            return "PID " + processId;
        }
    }

    private static bool IsRecoverable(Exception exception) =>
        exception is Win32Exception
        || exception is InvalidOperationException
        || exception is NotSupportedException
        || exception is UnauthorizedAccessException
        || exception is System.Security.SecurityException
        || exception is System.Runtime.InteropServices.ExternalException
        || exception is ArgumentException;

    private void Subscribe<T>(string capability, Func<IObservable<T>> sourceFactory, Action<T> onNext, CompositeDisposable subscriptions)
    {
        try
        {
            subscriptions.Add(sourceFactory().Subscribe(onNext, exception =>
                Publish("Capability", "Unavailable", capability + ": " + exception.Message)));
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            Publish("Capability", "Unavailable", capability + ": " + exception.Message);
        }
    }

    private void TryStartSessionListener(CompositeDisposable subscriptions)
    {
        try
        {
            var listener = new WindowsSessionListener();
            subscriptions.Add(listener);
            subscriptions.Add(listener.ObserveSessionChanges().Subscribe(change =>
                Publish("Session", "Info", change.EventType + " for session " + change.SessionId), exception =>
                    Publish("Capability", "Unavailable", "Session listener: " + exception.Message)));
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            Publish("Capability", "Unavailable", "Session listener: " + exception.Message);
        }
    }
}
