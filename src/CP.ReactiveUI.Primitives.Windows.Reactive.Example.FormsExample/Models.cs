// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Reactive.Example.FormsExample;

internal sealed class WindowSnapshot
{
    public WindowSnapshot(long handle, int processId, string processName, string caption, string className, string bounds, bool visible, bool minimized, bool maximized)
    {
        Handle = handle;
        ProcessId = processId;
        ProcessName = processName;
        Caption = caption;
        ClassName = className;
        Bounds = bounds;
        Visible = visible;
        Minimized = minimized;
        Maximized = maximized;
    }

    public long Handle { get; }

    public string HandleHex => "0x" + Handle.ToString("X");

    public int ProcessId { get; }

    public string ProcessName { get; }

    public string Caption { get; }

    public string ClassName { get; }

    public string Bounds { get; }

    public bool Visible { get; }

    public bool Minimized { get; }

    public bool Maximized { get; }
}

internal sealed class ProcessSnapshot
{
    public ProcessSnapshot(int id, string name, long workingSet, int threadCount, string mainWindowTitle)
    {
        Id = id;
        Name = name;
        WorkingSet = workingSet;
        ThreadCount = threadCount;
        MainWindowTitle = mainWindowTitle;
    }

    public int Id { get; }

    public string Name { get; }

    public string WorkingSetText => (WorkingSet / 1_048_576D).ToString("N1") + " MB";

    public long WorkingSet { get; }

    public int ThreadCount { get; }

    public string MainWindowTitle { get; }
}

internal sealed class DisplaySnapshot
{
    public DisplaySnapshot(string name, string bounds, string workingArea, string resolution, bool primary)
    {
        Name = name;
        Bounds = bounds;
        WorkingArea = workingArea;
        Resolution = resolution;
        Primary = primary;
    }

    public string Name { get; }

    public string Bounds { get; }

    public string WorkingArea { get; }

    public string Resolution { get; }

    public bool Primary { get; }
}

internal sealed class CapabilitySnapshot
{
    public CapabilitySnapshot(string capability, string state, string detail)
    {
        Capability = capability;
        State = state;
        Detail = detail;
    }

    public string Capability { get; }

    public string State { get; }

    public string Detail { get; }
}

internal sealed class OperationsEvent
{
    public OperationsEvent(DateTimeOffset timestamp, string category, string severity, string message)
    {
        Timestamp = timestamp;
        Category = category;
        Severity = severity;
        Message = message;
    }

    public string Time => Timestamp.ToLocalTime().ToString("HH:mm:ss.fff");

    public DateTimeOffset Timestamp { get; }

    public string Category { get; }

    public string Severity { get; }

    public string Message { get; }
}
