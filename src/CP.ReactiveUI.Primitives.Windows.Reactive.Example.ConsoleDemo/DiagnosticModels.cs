// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Reactive.Example.ConsoleDemo;

/// <summary>Contains a point-in-time diagnostic overview.</summary>
internal sealed class DiagnosticSnapshot
{
    /// <summary>Gets or sets when the snapshot was captured.</summary>
    public DateTimeOffset CapturedAt { get; set; }

    /// <summary>Gets or sets the Windows version.</summary>
    public string WindowsVersion { get; set; }

    /// <summary>Gets or sets the CLR version.</summary>
    public string RuntimeVersion { get; set; }

    /// <summary>Gets or sets the current process description.</summary>
    public string CurrentProcess { get; set; }

    /// <summary>Gets or sets the machine and user description.</summary>
    public string Identity { get; set; }

    /// <summary>Gets or sets the process count.</summary>
    public int ProcessCount { get; set; }

    /// <summary>Gets or sets the top-level window count.</summary>
    public int WindowCount { get; set; }

    /// <summary>Gets or sets the display count.</summary>
    public int DisplayCount { get; set; }

    /// <summary>Gets or sets the virtual desktop bounds.</summary>
    public string DesktopBounds { get; set; }

    /// <summary>Gets or sets the cursor description.</summary>
    public string Cursor { get; set; }

    /// <summary>Gets or sets the foreground-window description.</summary>
    public string ForegroundWindow { get; set; }

    /// <summary>Gets or sets the clipboard description.</summary>
    public string Clipboard { get; set; }

    /// <summary>Gets or sets the process uptime.</summary>
    public TimeSpan ProcessUptime { get; set; }
}

/// <summary>Contains safe process-list data copied from a process handle.</summary>
internal sealed class ProcessDiagnostic
{
    /// <summary>Gets or sets the process identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the process name.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets the working set.</summary>
    public long WorkingSet { get; set; }

    /// <summary>Gets or sets the main-window title.</summary>
    public string WindowTitle { get; set; }
}
