// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace CP.ReactiveUI.Primitives.Windows.Example.ConsoleDemo;

/// <summary>Represents a timestamped dashboard event.</summary>
internal sealed class DiagnosticLogEntry
{
    /// <summary>Gets or sets the timestamp.</summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>Gets or sets the severity.</summary>
    public string Level { get; set; }

    /// <summary>Gets or sets the message.</summary>
    public string Message { get; set; }
}

/// <summary>Provides a bounded, signal-backed, in-memory event log.</summary>
internal sealed class DiagnosticLog : IDisposable
{
    private const int Capacity = 200;
    private readonly object _syncRoot = new();
    private readonly Queue<DiagnosticLogEntry> _history = new();
    private readonly Signal<DiagnosticLogEntry> _entries = new();
    private bool _disposed;

    /// <summary>Gets the live event stream.</summary>
    public IObservable<DiagnosticLogEntry> Entries => _entries;

    /// <summary>Adds an informational event.</summary>
    /// <param name="message">The message.</param>
    public void Info(string message) => Write("INFO", message);

    /// <summary>Adds a warning event.</summary>
    /// <param name="message">The message.</param>
    public void Warning(string message) => Write("WARN", message);

    /// <summary>Adds an error event.</summary>
    /// <param name="message">The message.</param>
    public void Error(string message) => Write("ERROR", message);

    /// <summary>Returns the newest log entries.</summary>
    /// <param name="count">The maximum number of entries.</param>
    /// <returns>A stable snapshot ordered from oldest to newest.</returns>
    public IReadOnlyList<DiagnosticLogEntry> Snapshot(int count)
    {
        lock (_syncRoot)
        {
            var skip = Math.Max(0, _history.Count - Math.Max(1, count));
            return _history.Skip(skip).ToArray();
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
        _entries.OnCompleted();
        _entries.Dispose();
    }

    private void Write(string level, string message)
    {
        if (_disposed)
        {
            return;
        }

        DiagnosticLogEntry entry = new()
        {
            Timestamp = TimeProvider.System.GetLocalNow(),
            Level = level,
            Message = message,
        };
        lock (_syncRoot)
        {
            _history.Enqueue(entry);
            while (_history.Count > Capacity)
            {
                _ = _history.Dequeue();
            }
        }

        _entries.OnNext(entry);
    }
}
