// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
using CP.ReactiveUI.Primitives.Windows.Desktop.Display;
using CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix;
using CP.ReactiveUI.Primitives.Windows.Native;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace CP.ReactiveUI.Primitives.Windows.Example.ConsoleDemo;

/// <summary>Collects read-only diagnostics through managed and local Windows APIs.</summary>
internal static class DiagnosticsService
{
    private const int DefaultClipboardRetries = 3;

    /// <summary>Captures a dashboard snapshot without blocking the command-loop thread.</summary>
    /// <param name="cancellationToken">Cancels the capture.</param>
    /// <returns>The captured snapshot.</returns>
    public static Task<DiagnosticSnapshot> CaptureSnapshotAsync(CancellationToken cancellationToken) =>
        Task.Run(() => CaptureSnapshot(cancellationToken), cancellationToken);

    /// <summary>Returns processes ordered by descending working-set size.</summary>
    /// <param name="count">The maximum number of rows.</param>
    /// <param name="cancellationToken">Cancels collection.</param>
    /// <returns>The process diagnostics.</returns>
    public static Task<ProcessDiagnostic[]> GetProcessesAsync(int count, CancellationToken cancellationToken) =>
        Task.Run(() => GetProcesses(count, cancellationToken), cancellationToken);

    /// <summary>Returns visible top-level windows from the primary primitives project.</summary>
    /// <param name="count">The maximum number of rows.</param>
    /// <param name="cancellationToken">Cancels collection.</param>
    /// <returns>Formatted window rows.</returns>
    public static Task<List<string>> GetWindowsAsync(int count, CancellationToken cancellationToken) =>
        Task.Run(() => GetWindows(count, cancellationToken), cancellationToken);

    /// <summary>Enumerates window handles through the lean Primitives signal API.</summary>
    /// <param name="count">The maximum number of handles.</param>
    /// <param name="cancellationToken">Cancels enumeration.</param>
    /// <returns>Formatted native handles.</returns>
    public static async Task<IReadOnlyList<string>> GetLeanSignalHandlesAsync(int count, CancellationToken cancellationToken)
    {
        IList<string> handles = await WindowsEnumerator.ObserveWindowHandles()
            .Take(Math.Max(1, count))
            .Map(static handle => $"0x{handle.ToInt64():X}")
            .ToList()
            .ToTask(cancellationToken)
            .ConfigureAwait(false);
        return handles.ToArray();
    }

    /// <summary>Reads a clipboard summary and a bounded text preview when available.</summary>
    /// <param name="includePreview">Whether to include clipboard text.</param>
    /// <returns>The clipboard description.</returns>
    public static string GetClipboardDescription(bool includePreview)
    {
        var sequence = ClipboardNative.SequenceNumber;
        var hasOwner = ClipboardNative.HasOwner;
        var hasText = ClipboardNative.HasFormat((uint)StandardClipboardFormats.UnicodeText);
        var summary = $"sequence {sequence}, owner {(hasOwner ? "yes" : "no")}, Unicode text {(hasText ? "yes" : "no")}";
        if (!includePreview || !hasText)
        {
            return summary;
        }

        using var access = ClipboardNative.Access(
            IntPtr.Zero,
            DefaultClipboardRetries,
            TimeSpan.FromMilliseconds(25),
            TimeSpan.FromMilliseconds(500));
        if (!access.CanAccess)
        {
            return $"{summary}; clipboard is busy";
        }

        var text = access.GetAsUnicodeString();
        text = text.Replace('\r', ' ').Replace('\n', ' ').Trim();
        if (text.Length > 120)
        {
            text = $"{text.Substring(0, 117)}...";
        }

        return string.IsNullOrEmpty(text) ? $"{summary}; text is empty" : $"{summary}; preview: {text}";
    }

    /// <summary>Probes the optional Citrix integration.</summary>
    /// <returns>A non-throwing Citrix session description.</returns>
    public static string GetCitrixDescription()
    {
        if (!WinFrame.IsAvailabe)
        {
            return "Citrix WFAPI is not available (normal outside a Citrix session).";
        }

        return $"Citrix state: {WinFrame.QuerySessionConnectState()}; client: {WinFrame.GetClientName() ?? "unknown"}; address: {WinFrame.GetClientIpAddress() ?? "unknown"}";
    }

    private static DiagnosticSnapshot CaptureSnapshot(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Process[] processes = Process.GetProcesses();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var currentProcess = Process.GetCurrentProcess();
            var displays = DisplayTopology.GetSnapshot();
            var windows = WindowsEnumerator.EnumerateWindows((IInteropWindow)null);
            var windowCount = windows.Count();
            var foreground = DescribeForegroundWindow();
            var cursor = User32Api.GetCursorLocation();
            return new()
            {
                CapturedAt = TimeProvider.System.GetLocalNow(),
                WindowsVersion = $"{WindowsVersion.WinVersion} (Windows 11+ {WindowsVersion.IsWindows11OrLater})",
                RuntimeVersion = Environment.Version.ToString(),
                CurrentProcess = $"{currentProcess.ProcessName} ({currentProcess.Id}), {FormatBytes(currentProcess.WorkingSet64)} working set",
                Identity = $"{Environment.UserDomainName}\\{Environment.UserName} on {Environment.MachineName}",
                ProcessCount = processes.Length,
                WindowCount = windowCount,
                DisplayCount = displays.Count,
                DesktopBounds = DisplayTopology.ScreenBounds.ToString(),
                Cursor = $"X={cursor.X}, Y={cursor.Y}",
                ForegroundWindow = foreground,
                Clipboard = GetClipboardDescription(includePreview: false),
                ProcessUptime = TimeProvider.System.GetLocalNow().DateTime - currentProcess.StartTime,
            };
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }
    }

    private static ProcessDiagnostic[] GetProcesses(int count, CancellationToken cancellationToken)
    {
        List<ProcessDiagnostic> rows = new();
        Process[] processes = Process.GetProcesses();
        try
        {
            foreach (var process in processes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    rows.Add(new()
                    {
                        Id = process.Id,
                        Name = process.ProcessName,
                        WorkingSet = process.WorkingSet64,
                        WindowTitle = process.MainWindowTitle,
                    });
                }
                catch (InvalidOperationException)
                {
                }
                catch (System.ComponentModel.Win32Exception)
                {
                }
            }
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }

        return rows.OrderByDescending(static row => row.WorkingSet).Take(Math.Max(1, count)).ToArray();
    }

    private static List<string> GetWindows(int count, CancellationToken cancellationToken)
    {
        List<string> rows = new();
        foreach (var window in WindowsEnumerator.EnumerateWindows((IInteropWindow)null))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (!window.IsVisible(forceUpdate: true))
                {
                    continue;
                }

                var caption = window.GetCaption(forceUpdate: true);
                if (string.IsNullOrWhiteSpace(caption))
                {
                    continue;
                }

                rows.Add($"0x{window.Handle.ToInt64():X}  PID {window.GetProcessId(forceUpdate: true),6}  {caption}");
                if (rows.Count >= Math.Max(1, count))
                {
                    break;
                }
            }
            catch (InvalidOperationException)
            {
            }
            catch (System.ComponentModel.Win32Exception)
            {
            }
        }

        return rows;
    }

    private static string DescribeForegroundWindow()
    {
        var handle = User32Api.GetForegroundWindow();
        if (handle == IntPtr.Zero)
        {
            return "none";
        }

        var window = InteropWindowFactory.CreateFor(handle);
        var caption = window.GetCaption(forceUpdate: true);
        var processId = window.GetProcessId(forceUpdate: true);
        return $"0x{handle.ToInt64():X}, PID {processId}, {caption}";
    }

    /// <summary>Formats a byte count for dashboard output.</summary>
    /// <param name="bytes">The byte count.</param>
    /// <returns>A compact size.</returns>
    public static string FormatBytes(long bytes)
    {
        const double Megabyte = 1024D * 1024D;
        const double Gigabyte = Megabyte * 1024D;
        return bytes >= Gigabyte
            ? $"{bytes / Gigabyte:0.0} GiB"
            : $"{bytes / Megabyte:0.0} MiB";
    }
}
