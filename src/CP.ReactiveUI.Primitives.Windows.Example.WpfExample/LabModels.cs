// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Windows.Media;

namespace CP.ReactiveUI.Primitives.Windows.Example.WpfExample;

/// <summary>Controls the activation-scoped guard installed on the laboratory window.</summary>
public enum WindowGuardMode
{
    /// <summary>Allows normal interactive movement and resizing.</summary>
    None,

    /// <summary>Blocks interactive movement only.</summary>
    Move,

    /// <summary>Blocks interactive movement and resizing.</summary>
    MoveAndResize,
}

/// <summary>One timestamped item displayed by the reactive event console.</summary>
public sealed class LabEvent
{
    /// <summary>Initializes a new event item.</summary>
    /// <param name="source">Short event source.</param>
    /// <param name="message">Human-readable event details.</param>
    public LabEvent(string source, string message)
    {
        Timestamp = DateTimeOffset.Now;
        Source = source;
        Message = message;
    }

    /// <summary>Gets the event timestamp.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Gets the timestamp formatted for the dashboard.</summary>
    public string Time => Timestamp.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);

    /// <summary>Gets the event source.</summary>
    public string Source { get; }

    /// <summary>Gets the event details.</summary>
    public string Message { get; }
}

/// <summary>Read-only native top-level window snapshot.</summary>
public sealed class WindowSnapshot
{
    /// <summary>Initializes a native window snapshot.</summary>
    /// <param name="handle">Native window handle.</param>
    /// <param name="caption">Window caption.</param>
    /// <param name="className">Native class name.</param>
    /// <param name="processId">Owning process identifier.</param>
    /// <param name="bounds">Formatted native bounds.</param>
    public WindowSnapshot(IntPtr handle, string caption, string className, int processId, string bounds)
    {
        Handle = handle;
        Caption = caption;
        ClassName = className;
        ProcessId = processId;
        Bounds = bounds;
    }

    /// <summary>Gets the native window handle.</summary>
    public IntPtr Handle { get; }

    /// <summary>Gets the hexadecimal handle label.</summary>
    public string HandleText => $"0x{Handle.ToInt64():X}";

    /// <summary>Gets the caption.</summary>
    public string Caption { get; }

    /// <summary>Gets the native class name.</summary>
    public string ClassName { get; }

    /// <summary>Gets the owning process identifier.</summary>
    public int ProcessId { get; }

    /// <summary>Gets formatted bounds.</summary>
    public string Bounds { get; }
}

/// <summary>Display topology item projected from the local Core display primitive.</summary>
public sealed class DisplaySnapshot
{
    /// <summary>Initializes a display snapshot.</summary>
    /// <param name="name">Display device name.</param>
    /// <param name="bounds">Display and working-area description.</param>
    /// <param name="role">Primary or secondary role.</param>
    public DisplaySnapshot(string name, string bounds, string role)
    {
        Name = name;
        Bounds = bounds;
        Role = role;
    }

    /// <summary>Gets the display device name.</summary>
    public string Name { get; }

    /// <summary>Gets the formatted display bounds.</summary>
    public string Bounds { get; }

    /// <summary>Gets the display role.</summary>
    public string Role { get; }
}

/// <summary>Current laboratory window and display metrics.</summary>
public sealed class EnvironmentSnapshot
{
    /// <summary>Gets or sets the window summary.</summary>
    public string WindowSummary { get; set; }

    /// <summary>Gets or sets the display summary.</summary>
    public string DisplaySummary { get; set; }

    /// <summary>Gets or sets the foreground summary.</summary>
    public string ForegroundSummary { get; set; }

    /// <summary>Gets or sets current displays.</summary>
    public IReadOnlyList<DisplaySnapshot> Displays { get; set; }
}

/// <summary>Result returned by the local cursor visual helper.</summary>
public sealed class CursorSnapshot
{
    /// <summary>Gets or sets the WPF cursor image.</summary>
    public ImageSource Image { get; set; }

    /// <summary>Gets or sets cursor metadata.</summary>
    public string Description { get; set; }
}
