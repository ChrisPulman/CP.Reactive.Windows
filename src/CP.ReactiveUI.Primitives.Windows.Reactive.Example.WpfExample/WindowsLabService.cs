// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.Enumerations;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi.Wpf;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Keyboard;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Mouse;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;

namespace CP.ReactiveUI.Primitives.Windows.Reactive.Example.WpfExample;

/// <summary>Abstracts the Windows-specific edge so the view model remains independent of WPF controls and native types.</summary>
internal interface IWindowsLabService
{
    IObservable<LabEvent> ObserveLocalWindowEvents();

    IObservable<LabEvent> ObserveDpiChanges();

    IObservable<LabEvent> ObserveDisplayChanges();

    IObservable<LabEvent> ObserveWindowEvents();

    IObservable<LabEvent> ObserveKeyboardEvents();

    IObservable<LabEvent> ObserveMouseEvents();

    IObservable<Unit> CreateWindowGuard(WindowGuardMode mode);

    EnvironmentSnapshot GetEnvironment();

    IReadOnlyList<WindowSnapshot> DiscoverWindows();

    WindowSnapshot GetForegroundWindow();

    CursorSnapshot CaptureCursor();

    void SavePlacement();

    bool RestorePlacement();

    void CenterWindow();

    Task BringToForegroundAsync(WindowSnapshot window);
}

/// <summary>Uses the local Core, Windows, and Reactive assemblies to implement laboratory operations.</summary>
internal sealed class WindowsLabService : IWindowsLabService
{
    private readonly Window _owner;
    private WindowPlacement? _savedPlacement;

    internal WindowsLabService(Window owner) => _owner = owner ?? throw new ArgumentNullException(nameof(owner));

    public IObservable<LabEvent> ObserveLocalWindowEvents() =>
        _owner.ObserveWindowMessages()
            .Where(static message =>
                message.Message == WindowsMessages.WM_ACTIVATE
                || message.Message == WindowsMessages.WM_MOVE
                || message.Message == WindowsMessages.WM_SIZE
                || message.Message == WindowsMessages.WM_WINDOWPOSCHANGED
                || message.Message == WindowsMessages.WM_DISPLAYCHANGE
                || message.Message == WindowsMessages.WM_DPICHANGED)
            .Select(static message => new LabEvent("Window message", $"{message.Message} · HWND 0x{message.Handle:X}"));

    public IObservable<LabEvent> ObserveDpiChanges() => Observable.Create<LabEvent>(observer =>
    {
        var handler = _owner.AttachDpiHandler();
        var subscription = handler.ObserveDpiChanges().Subscribe(
            change => observer.OnNext(new LabEvent("DPI", $"{change.PreviousDpi} → {change.NewDpi} dpi")),
            observer.OnError,
            observer.OnCompleted);
        return new CompositeDisposable(subscription, handler);
    });

    public IObservable<LabEvent> ObserveDisplayChanges() =>
        DisplayTopology.ObserveChanges()
            .Skip(1)
            .Select(static displays => new LabEvent(
                "Display topology",
                $"{displays.Count} display(s) · virtual desktop {FormatRect(DisplayTopology.ScreenBounds)}"));

    public IObservable<LabEvent> ObserveWindowEvents() =>
        WinEventHook.ObserveWindowLifecycleEvents()
            .Merge(WinEventHook.ObserveWindowTitleChanges())
            .Select(info => new LabEvent("WinEvent", DescribeWinEvent(info)));

    public IObservable<LabEvent> ObserveKeyboardEvents() =>
        KeyboardHook.KeyboardHookEvents
            .Where(static args => args.IsKeyDown)
            .Select(static args => new LabEvent("Keyboard", args.ToString()));

    public IObservable<LabEvent> ObserveMouseEvents() =>
        MouseHook.MouseHookEvents
            .Sample(TimeSpan.FromMilliseconds(140))
            .Select(static args => new LabEvent("Mouse", $"{args.WindowsMessage} at {args.Point.X}, {args.Point.Y}"));

    public IObservable<Unit> CreateWindowGuard(WindowGuardMode mode)
    {
        if (mode == WindowGuardMode.None)
        {
            return Observable.Never<Unit>();
        }

        var blockMode = mode == WindowGuardMode.Move
            ? WindowsMoveBlockMode.MoveOnly
            : WindowsMoveBlockMode.MoveAndResize;
        return new WindowsMove(static () => false, blockMode);
    }

    public EnvironmentSnapshot GetEnvironment()
    {
        var handle = GetOwnerHandle();
        var dpi = handle == IntPtr.Zero ? checked((int)NativeDpiMethods.GetDpiForSystem()) : NativeDpiMethods.GetDpi(handle);
        var displays = User32Api.EnumDisplays()
            .Select(static display => new DisplaySnapshot(
                string.IsNullOrWhiteSpace(display.DeviceName) ? "Display" : display.DeviceName,
                $"{FormatRect(display.Bounds)} · work {FormatRect(display.WorkingArea)}",
                display.IsPrimary ? "PRIMARY" : "SECONDARY"))
            .ToArray();
        var placement = handle == IntPtr.Zero
            ? default(WindowPlacement?)
            : InteropWindowFactory.CreateFor(handle).GetPlacement(forceUpdate: true);

        return new EnvironmentSnapshot
        {
            WindowSummary = handle == IntPtr.Zero
                ? "HWND is created when the window source initializes."
                : $"HWND 0x{handle.ToInt64():X} · {dpi} dpi · {FormatRect(placement.Value.NormalPosition)} · {placement.Value.ShowCmd}",
            DisplaySummary = $"{displays.Length} display(s) · virtual desktop {FormatRect(DisplayTopology.ScreenBounds)}",
            ForegroundSummary = DescribeWindow(GetForegroundWindow()),
            Displays = displays,
        };
    }

    public IReadOnlyList<WindowSnapshot> DiscoverWindows()
    {
        List<WindowSnapshot> snapshots = new();
        foreach (var window in InteropWindowQueryExtensions.GetTopLevelWindows(ignoreKnownClasses: true).Take(160))
        {
            try
            {
                snapshots.Add(CreateSnapshot(window));
            }
            catch (Exception)
            {
                // A foreign window can disappear between enumeration and inspection.
            }
        }

        return snapshots;
    }

    public WindowSnapshot GetForegroundWindow()
    {
        var handle = User32Api.GetForegroundWindow();
        return handle == IntPtr.Zero ? null : CreateSnapshot(InteropWindowFactory.CreateFor(handle));
    }

    public CursorSnapshot CaptureCursor()
    {
        if (!CursorHelper.TryGetCurrentCursor(out var cursor))
        {
            return new CursorSnapshot { Description = "Windows did not expose a cursor image." };
        }

        using (cursor)
        {
            var layer = cursor.ColorLayer ?? cursor.MaskLayer;
            if (layer is null)
            {
                return new CursorSnapshot { Description = "The cursor has no drawable layer." };
            }

            var bitmapHandle = layer.GetHbitmap();
            try
            {
                var source = Imaging.CreateBitmapSourceFromHBitmap(
                    bitmapHandle,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                source.Freeze();
                return new CursorSnapshot
                {
                    Image = source,
                    Description = $"Cursor {cursor.Size.Width}×{cursor.Size.Height} · hot spot {cursor.HotSpot.X}, {cursor.HotSpot.Y}",
                };
            }
            finally
            {
                _ = Gdi32Api.DeleteObject(bitmapHandle);
            }
        }
    }

    public void SavePlacement()
    {
        var handle = GetOwnerHandle();
        if (handle != IntPtr.Zero)
        {
            _savedPlacement = InteropWindowFactory.CreateFor(handle).GetPlacement(forceUpdate: true);
        }
    }

    public bool RestorePlacement()
    {
        var handle = GetOwnerHandle();
        if (handle == IntPtr.Zero || !_savedPlacement.HasValue)
        {
            return false;
        }

        _ = InteropWindowFactory.CreateFor(handle).SetPlacement(_savedPlacement.Value);
        return true;
    }

    public void CenterWindow()
    {
        var workArea = SystemParameters.WorkArea;
        _owner.Left = workArea.Left + Math.Max(0, (workArea.Width - _owner.ActualWidth) / 2);
        _owner.Top = workArea.Top + Math.Max(0, (workArea.Height - _owner.ActualHeight) / 2);
    }

    public async Task BringToForegroundAsync(WindowSnapshot window)
    {
        if (window is null)
        {
            return;
        }

        await InteropWindowFactory.CreateFor(window.Handle).ToForegroundAsync();
    }

    private IntPtr GetOwnerHandle() => new WindowInteropHelper(_owner).Handle;

    private WindowSnapshot CreateSnapshot(IInteropWindow window)
    {
        var ownerHandle = GetOwnerHandle();
        var caption = window.Handle == ownerHandle ? _owner.Title : window.GetCaption(forceUpdate: true);
        var info = window.GetInfo(forceUpdate: true);
        return new WindowSnapshot(
            window.Handle,
            string.IsNullOrWhiteSpace(caption) ? "(untitled)" : caption,
            window.GetClassname(forceUpdate: true),
            window.GetProcessId(forceUpdate: true),
            FormatRect(info.Bounds));
    }

    private static string DescribeWindow(WindowSnapshot window) => window is null
        ? "No foreground window was reported."
        : $"{window.Caption} · PID {window.ProcessId} · {window.HandleText}";

    private static string DescribeWinEvent(WinEventInfo info)
    {
        var handle = info.Window?.Handle ?? IntPtr.Zero;
        string caption;
        try
        {
            caption = handle == IntPtr.Zero ? "(no window)" : info.Window.GetCaption(forceUpdate: true);
        }
        catch (Exception)
        {
            caption = "(window closed)";
        }

        return $"{info.WinEvent} · 0x{handle.ToInt64():X} · {caption}";
    }

    private static string FormatRect(NativeRect rect) => $"{rect.Width}×{rect.Height} @ {rect.Left},{rect.Top}";
}
