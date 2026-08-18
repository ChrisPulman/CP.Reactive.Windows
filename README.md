# CP.ReactiveUI.Primitives.Windows

Composable Windows desktop primitives built on `ReactiveUI.Primitives`.

The library collection exposes Windows operating-system state, callbacks, and commands through small focused packages. One-shot native calls remain ordinary methods; anything that changes over time, is raised by a message, or reports progress is exposed as an `IObservable<T>` using ReactiveUI.Primitives naming and lifetime conventions.

## Packages

Install the package based on your needs, CP.ReactiveUI.Primitives.Windows is the lean desktop package, CP.ReactiveUI.Primitives.Windows.Reactive is the System.Reactive-first variant. The other packages are optional and provide additional native interop, desktop, or integration features.

```powershell
dotnet add package CP.ReactiveUI.Primitives.Windows
dotnet add package CP.ReactiveUI.Primitives.Windows.Core
dotnet add package CP.ReactiveUI.Primitives.Windows.Integrations
dotnet add package CP.ReactiveUI.Primitives.Windows.Reactive
```

| Package | Use when | Main namespaces |
| --- | --- | --- |
| `CP.ReactiveUI.Primitives.Windows.Core` | You need native value types, safe handles, COM contracts, registry monitoring, GDI, Kernel32, Shell32, or User32 helpers. | `CP.ReactiveUI.Primitives.Windows.Native`, `CP.ReactiveUI.Primitives.Windows.Interop` |
| `CP.ReactiveUI.Primitives.Windows` | You need desktop features: clipboard, devices, DPI, dialogs, icons, input, messages, windows, display, power, lifecycle, or multimedia compiled against `ReactiveUI.Primitives`. | `CP.ReactiveUI.Primitives.Windows.Desktop` |
| `CP.ReactiveUI.Primitives.Windows.Integrations` | You need optional Citrix WFAPI helpers or the legacy WinForms `WebBrowser` integration. | `CP.ReactiveUI.Primitives.Windows.Integrations` |
| `CP.ReactiveUI.Primitives.Windows.Reactive` | Your app is System.Reactive-first and wants the same desktop capabilities compiled against `ReactiveUI.Primitives.Reactive`. | `CP.ReactiveUI.Primitives.Windows.Reactive.Desktop` |

The lean and `.Reactive` desktop packages compile the same source. The lean build uses `ReactiveUI.Primitives`; the `.Reactive` build defines `REACTIVE_SHIM`, uses `ReactiveUI.Primitives.Reactive`, and emits the desktop API beneath `CP.ReactiveUI.Primitives.Windows.Reactive.Desktop`. Core interop types remain in `CP.ReactiveUI.Primitives.Windows.Native` and `CP.ReactiveUI.Primitives.Windows.Interop`, so handles and value types keep one identity across both variants.

## Requirements

The packages target `net462`, `net472`, `net48`, `net481`, `net8.0-windows`, `net9.0-windows`, `net10.0-windows`, and `net11.0-windows`. Stable local and Visual Studio builds select the four .NET Framework targets plus `net10.0-windows`; the `net11.0-windows` preview target is enabled explicitly for preview-SDK and CI builds. They are intended for Windows desktop processes and use Windows Forms/WPF-capable TFMs where required by the underlying operating-system feature.

The implementation logs through Apache `log4net`. Libraries never configure appenders on the consumer's behalf. Configure the repository once in the application startup path when diagnostic output is required:

```csharp
using log4net;
using log4net.Config;

BasicConfigurator.Configure();

ILog log = LogManager.GetLogger(typeof(Program));
log.Info("Windows primitives logging is configured.");
```

For production applications, prefer an XML `log4net` configuration with the appenders, levels, and retention policy appropriate to the host. Library trace-level details use the log4net debug level.

Typical consumer imports:

```csharp
using ReactiveUI.Primitives;
using CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
using CP.ReactiveUI.Primitives.Windows.Desktop.Devices;
using CP.ReactiveUI.Primitives.Windows.Desktop.Display;
using CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
using CP.ReactiveUI.Primitives.Windows.Desktop.Input;
using CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
using CP.ReactiveUI.Primitives.Windows.Desktop.Power;
using CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs;
using CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
using CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
using CP.ReactiveUI.Primitives.Windows.Native;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
```

## Reactive Conventions

The package follows the ReactiveUI.Primitives naming model:

| Name shape | Meaning |
| --- | --- |
| `Observe...()` | Creates an observable stream for a changing OS value or event source. Dispose the returned subscription to detach the listener. |
| `...Events`, `...Changes`, or `...Requests` | Shared observable stream backed by a process-wide source, usually a hidden message window or native hook. |
| `Get...()` / properties | Snapshot reads with no subscription. |
| `Set...()` / command verbs | Immediate native command or state change. |
| `Try...()` | Native operation where failure is expected and reported as `false` instead of an exception. |

Reactive sources are ordinary `IObservable<T>` values. Most shared observables are published/ref-counted: the native registration is active while subscribers exist and is released when the last subscription is disposed.

## Lifecycle, Threading, And Errors

Dispose subscriptions and owned objects. Hooks, message windows, clipboard locks, timers, DPI handlers, COM wrappers, and safe handles release native resources through `IDisposable`.

```csharp
IDisposable subscription = ClipboardNative.ClipboardUpdateEvents.Subscribe(update =>
{
    Console.WriteLine(update.Id);
});

subscription.Dispose();
```

Clipboard and common dialogs should be used from an STA desktop thread. UI-bound observers should marshal back to the UI thread using the ReactiveUI.Primitives UI package or the dispatcher/control mechanism used by your application.

Observable setup failures are reported through `OnError`. Native one-shot methods either return `bool`, return nullable data where absence is normal, or throw a platform/native exception for unexpected failures.

## Package API

### Core Native Primitives

Namespace: `CP.ReactiveUI.Primitives.Windows.Native`

Core provides reusable interop building blocks:

| Area | Main APIs |
| --- | --- |
| Geometry and pixels | `NativePoint`, `NativePointFloat`, `NativeSize`, `NativeSizeFloat`, `NativeRect`, `NativeRectFloat`, `Bgr24`, `Bgra32`, `Indexed8`, conversion extensions. |
| Win32 status | `Win32.GetLastErrorCode()`, `Win32.GetHResult()`, `Win32.GetMessage(...)`, `WindowsVersion.IsWindows10OrLater`, `IsWindows11OrLater`, and other version checks. |
| Bitmap access | `BitmapAccessor<TPixel>`, `BitmapAccessorExtensions`, row-span processing for high-performance pixel reads/writes. |
| Handles | Safe handles for GDI, Shell, User32, monitors, cursors, icons, DCs, regions, bitmaps, and selected objects. |
| COM | `ComWrapper`, `DisposableCom`, `IUnknown`, `IDispatch`, `IOleWindow`, `IOleCommandTarget`, `ComProgIdAttribute`. |
| Registry | `RegistryMonitor.ObserveChanges()` and Advapi32 registry access enums. |
| GDI/GDI+ | `Gdi32Api`, `GdiPlusApi`, `GdiExtensions`, bitmap/header structs, raster and device capability enums. |
| Kernel32 | `Kernel32Api`, `PsApi`, native restart-manager helpers, package information, version/product/suite enums. |
| Shell32 | `Shell32Api`, app-bar structs/enums, shell file info, `SafeIconHandle`. |
| User32 | `User32Api`, `DisplayInfo`, window/message/scroll/cursor/monitor structs and enums. |

Core examples:

```csharp
using CP.ReactiveUI.Primitives.Windows.Native;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

bool modernWindows = WindowsVersion.IsWindows10OrLater;
string lastErrorText = Win32.GetMessage(Win32.GetLastErrorCode());

NativeRect bounds = new(left: 0, top: 0, width: 1920, height: 1080);
NativePoint topLeft = bounds.TopLeft;

int screenWidth = User32Api.GetSystemMetrics(SystemMetric.SM_CXSCREEN);
IReadOnlyList<DisplayInfo> displays = User32Api.EnumDisplays();
```

```csharp
using System.Drawing;
using CP.ReactiveUI.Primitives.Windows.Native;
using CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats;

using Bitmap bitmap = new(200, 100);
using BitmapAccessor<Bgra32> accessor = new(bitmap);

accessor.ProcessRows(row =>
{
    for (int x = 0; x < accessor.Width; x++)
    {
        row[x] = new Bgra32(Blue: 0, Green: 0, Red: 255, Alpha: 255);
    }
});
```

```csharp
using CP.ReactiveUI.Primitives.Windows.Native.Security;
using CP.ReactiveUI.Primitives.Windows.Native.Security.Enums;

IDisposable changes = RegistryMonitor.ObserveChanges(
    Microsoft.Win32.RegistryHive.CurrentUser,
    @"Software\MyCompany\MyApp",
    RegistryNotifyFilter.ChangeLastSet)
    .Subscribe(_ =>
{
    Console.WriteLine("Registry value changed.");
});
```

### Desktop Windows

Namespace: `CP.ReactiveUI.Primitives.Windows.Desktop.Windows`

| API | Purpose |
| --- | --- |
| `InteropWindowFactory.CreateFor(...)` | Wrap a native `HWND` as an `InteropWindow`. |
| `InteropWindowQueryExtensions.GetForegroundWindow()`, `GetDesktopWindow()`, `GetTopLevelWindows(...)`, `GetTopWindows(...)`, `GetWindowsForProcess(...)` | Query common desktop windows. |
| `InteropWindowExtensions` | Fill/query cached window data, get caption/text/class/children/parent/process/placement/region/scroll info, move, restore, minimize, maximize, print, set style, bring to foreground. |
| `WindowsEnumerator.EnumerateWindowHandles(...)`, `EnumerateWindows(...)` | Enumerate windows as `IEnumerable<T>` or `IObservable<T>`. |
| `WinEventHook.ObserveWinEvents(...)`, `ObserveWindowTitleChanges()`, `ObserveWindowLifecycleEvents()` | Observe WinEvent callbacks. |
| `EnvironmentMonitor.EnvironmentChangeEvents` | Observe `WM_SETTINGCHANGE` environment and system parameter updates. |
| `WindowScroller` | Scroll a target window by message, mouse wheel, or keyboard page keys. |
| `WindowsExtensions` / `FormsExtensions` | Convert WPF/WinForms windows to `InteropWindow`; apply/retrieve placement. |

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Windows;

IInteropWindow foreground = InteropWindowQueryExtensions.GetForegroundWindow();
foreground.Fill();

Console.WriteLine(foreground.GetCaption());
Console.WriteLine(foreground.GetClassname());
Console.WriteLine(foreground.GetPlacement().ShowCmd);

await foreground.ToForegroundAsync();
foreground.MoveTo(new(100, 100));
foreground.Restore();
```

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Windows;

foreach (IInteropWindow window in InteropWindowQueryExtensions.GetTopLevelWindows())
{
    Console.WriteLine($"{window.Handle}: {window.GetCaption()}");
}

IDisposable enumeration = WindowsEnumerator.EnumerateWindows().Subscribe(window =>
{
    Console.WriteLine(window.GetCaption());
});
```

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Windows;

IDisposable titleChanges = WinEventHook.ObserveWindowTitleChanges().Subscribe(info =>
{
    Console.WriteLine($"{info.Window.GetCaption()} changed title");
});

IDisposable environment = EnvironmentMonitor.EnvironmentChangeEvents.Subscribe(change =>
{
    Console.WriteLine($"{change.Area}: {change.SystemParametersInfoAction}");
});
```

### Store App Visibility

Namespace: `CP.ReactiveUI.Primitives.Windows.Desktop.Apps`

`AppQueryExtensions` identifies Windows Store app host windows and launcher windows.

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Apps;
using CP.ReactiveUI.Primitives.Windows.Desktop.Windows;

foreach (IInteropWindow appWindow in AppQueryExtensions.WindowsStoreApps)
{
    Console.WriteLine(appWindow.GetCaption());
}

bool launcherVisible = AppQueryExtensions.IsLauncherVisible;
IInteropWindow launcher = AppQueryExtensions.GetAppLauncher();
```

### Display And DPI

Namespaces:

- `CP.ReactiveUI.Primitives.Windows.Desktop.Display`
- `CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi`
- `CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Forms`
- `CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Wpf`

| API | Purpose |
| --- | --- |
| `DisplayTopology.ScreenBounds` | Virtual desktop bounds. |
| `DisplayTopology.GetSnapshot()` | Current monitor snapshot. |
| `DisplayTopology.GetBounds(NativePoint)` | Monitor bounds containing a point. |
| `DisplayTopology.ObserveChanges()` | Current display snapshot plus later topology changes. |
| `DpiApi` / `NativeDpiMethods` | DPI awareness and per-window/monitor DPI functions. |
| `DpiCalculator` | Pure scale/unscale helpers. |
| `DpiHandler` | Per-window DPI message handling and `ObserveDpiChanges()`. |
| `BitmapScaleHandler` | Cache bitmaps by DPI. |
| `FormsDpiExtensions` / `WindowDpiExtensions` | Attach DPI handling to WinForms and WPF windows. |

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Display;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

NativeRect virtualDesktop = DisplayTopology.ScreenBounds;
NativeRect activeMonitor = DisplayTopology.GetBounds(new NativePoint(10, 10));

IDisposable displayChanges = DisplayTopology.ObserveChanges().Subscribe(displays =>
{
    foreach (var display in displays)
    {
        Console.WriteLine($"{display.DeviceName}: {display.Bounds}");
    }
});
```

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

using DpiHandler dpi = new();

IDisposable dpiChanges = dpi.ObserveDpiChanges().Subscribe(change =>
{
    Console.WriteLine($"DPI changed from {change.PreviousDpi} to {change.NewDpi}");
});

int scaled = dpi.ScaleWithCurrentDpi(24);
NativeSize scaledSize = dpi.ScaleWithCurrentDpi(new NativeSize(200, 100));
NativeSize originalSize = dpi.UnscaleWithCurrentDpi(scaledSize);
```

### Messaging And Session State

Namespace: `CP.ReactiveUI.Primitives.Windows.Desktop.Messaging`

| API | Purpose |
| --- | --- |
| `SharedMessageWindow.WindowMessageEvents` | Shared hidden-window message stream. |
| `SharedMessageWindow.ObserveWindowMessages(...)` | Observe messages with setup/teardown callbacks tied to the message-window handle. |
| `SharedMessageWindow.ObserveHandleChanges()` | Observe hidden-window handle creation/destruction. |
| `MessageLoop` | Run and control the message loop backing the shared window. |
| `WinProcListener`, `WinProcHandler`, `WinProcHandlerHook` | Attach to WPF/WinForms/native window procedures. |
| `WinProcWindowsExtensions`, `WinProcFormsExtensions` | Convenience attachment for WPF and WinForms. |
| `WindowsSessionListener` | Observe lock/unlock, logon/logoff, and other WTS session changes. |
| `WindowsMessage`, `WindowMessageInfo`, `WindowMessage`, `Msg` | Message data and helper structures. |

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
using CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.Enumerations;

IDisposable messages = SharedMessageWindow.WindowMessageEvents
    .Where(message => message.Msg == WindowsMessages.WM_SETTINGCHANGE)
    .Subscribe(message => Console.WriteLine(message.Msg));

IDisposable handleChanges = SharedMessageWindow.ObserveHandleChanges()
    .Subscribe(hwnd => Console.WriteLine($"Message window: {hwnd}"));
```

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;

using WindowsSessionListener listener = new();

IDisposable lockChanges = listener.ObserveSessionLockChanges().Subscribe(session =>
{
    Console.WriteLine($"{session.EventType} for session {session.SessionId}");
});

listener.Start();
listener.Pause();
listener.Resume();
listener.Stop();
```

### Clipboard

Namespace: `CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard`

| API | Purpose |
| --- | --- |
| `ClipboardNative.HasOwner`, `SequenceNumber`, `HasFormat(...)` | Snapshot clipboard state. |
| `ClipboardNative.Access(...)`, `AccessAsync(...)` | Open and lock the clipboard. Dispose the returned token. |
| `ClipboardNative.ClipboardUpdateEvents` | Observe clipboard sequence/content updates. |
| `ClipboardNative.ClipboardRenderFormatRequests` | Observe delayed-render requests. |
| `ClipboardFormatExtensions` | Register, map, enumerate, and display clipboard formats. |
| `ClipboardStringExtensions` | Read/write Unicode string data. |
| `ClipboardByteExtensions` | Read/write byte data. |
| `ClipboardStreamExtensions` | Read/write stream data. |
| `ClipboardFileExtensions` | Read/write file-drop lists. |
| `ClipboardCloudExtensions` | Set cloud/history/monitor-processing flags. |
| `ClipboardMiscExtensions` | Clear contents and set delayed-render content. |

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;

using IClipboardAccessToken clipboard = ClipboardNative.Access();

clipboard.ClearContents();
clipboard.SetAsUnicodeString("Hello from CP.ReactiveUI.Primitives.Windows");

string text = clipboard.GetAsUnicodeString();
IEnumerable<string> formats = clipboard.AvailableFormats();
```

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;

IDisposable updates = ClipboardNative.ClipboardUpdateEvents.Subscribe(update =>
{
    Console.WriteLine($"Clipboard #{update.Id} at {update.Timestamp}");
    Console.WriteLine(string.Join(", ", update.Formats));
});

IDisposable delayedRender = ClipboardNative.ClipboardRenderFormatRequests.Subscribe(request =>
{
    if (request.RenderAllFormats)
    {
        request.AccessToken.SetAsUnicodeString("Rendered on demand");
    }
});
```

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;

using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(2));
using IClipboardAccessToken clipboard = await ClipboardNative.AccessAsync(cancellation.Token);

uint customFormat = ClipboardFormatExtensions.RegisterFormat("com.example.payload");
clipboard.SetAsBytes(new byte[] { 1, 2, 3 }, customFormat);
byte[] payload = clipboard.GetAsBytes(customFormat);
```

### Devices And Raw Input

Namespaces:

- `CP.ReactiveUI.Primitives.Windows.Desktop.Devices`
- `CP.ReactiveUI.Primitives.Windows.Desktop.Input`

| API | Purpose |
| --- | --- |
| `DeviceNotification.OnNotification` | Shared device notification stream. |
| `DeviceNotification.ObserveDeviceNotifications(...)` | Create a notification stream for all interfaces or one device class. |
| `ObserveDeviceArrivals(...)`, `ObserveDeviceRemovals(...)` | Filter device-interface arrivals/removals. |
| `ObserveVolumeChanges(...)`, `ObserveVolumeAdditions(...)`, `ObserveVolumeRemovals(...)` | Filter volume device changes. |
| `RawInputApi` | Register raw input and query raw-input devices/data. |
| `RawInputMonitor.ObserveRawInput(...)` | Observe `WM_INPUT` raw input events. |
| `RawInputDeviceMonitor.GetDevicesSnapshot()` | Current raw-input devices by handle. |
| `RawInputDeviceMonitor.ObserveDeviceChanges(...)` | Observe raw-input device arrival/removal. |
| `NativeInput` | Send native keyboard/mouse input. |

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Devices;

IDisposable arrivals = DeviceNotification.ObserveDeviceArrivals().Subscribe(change =>
{
    Console.WriteLine($"Arrived: {change.Device.DisplayName}");
});

IDisposable removals = DeviceNotification.ObserveDeviceRemovals().Subscribe(change =>
{
    Console.WriteLine($"Removed: {change.Device.DisplayName}");
});

IDisposable volumes = DeviceNotification.ObserveVolumeAdditions().Subscribe(volume =>
{
    Console.WriteLine(volume.Volume.Drives);
});
```

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Input;
using CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;

IReadOnlyDictionary<IntPtr, RawInputDeviceInformation> devices =
    RawInputDeviceMonitor.GetDevicesSnapshot();

IDisposable rawInput = RawInputMonitor.ObserveRawInput(RawInputDevices.Keyboard, RawInputDevices.Mouse)
    .Subscribe(input => Console.WriteLine(input.RawInput.Header.Type));

IDisposable rawDevices = RawInputDeviceMonitor.ObserveDeviceChanges(RawInputDevices.Keyboard)
    .Subscribe(change => Console.WriteLine(change.Added));
```

### Keyboard And Mouse Hooks

Namespaces:

- `CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard`
- `CP.ReactiveUI.Primitives.Windows.Desktop.Input.Mouse`

| API | Purpose |
| --- | --- |
| `KeyboardHook.KeyboardHookEvents` | Global low-level keyboard hook stream. |
| `KeyboardHookExtensions` | Filter and compose keyboard hook events. |
| `KeyboardInputGenerator` | Send synthetic keyboard input. |
| `KeyHelper`, `VirtualKeyCodeExtensions` | Virtual-key classification and conversion helpers. |
| `KeyCombinationHandler`, `KeyOrCombinationHandler`, `KeySequenceHandler` | Detect key combinations and sequences. |
| `MouseHook.MouseHookEvents` | Global low-level mouse hook stream. |
| `MouseInputGenerator` | Send synthetic mouse input. |

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
using CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;

IDisposable keyboard = KeyboardHook.KeyboardHookEvents.Subscribe(args =>
{
    if (args.IsKeyDown && args.Key == VirtualKeyCode.F12)
    {
        args.Handled = true;
        Console.WriteLine("F12 handled.");
    }
});

KeyboardInputGenerator.KeyPresses(VirtualKeyCode.Return);
```

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Input.Mouse;

IDisposable mouse = MouseHook.MouseHookEvents.Subscribe(args =>
{
    Console.WriteLine($"{args.WindowsMessage} at {args.Point.X}, {args.Point.Y}");
});

MouseInputGenerator.MoveMouse(new(200, 200));
```

### Shell Dialogs

Namespace: `CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs`

| API | Purpose |
| --- | --- |
| `FileDialog.PickFileToOpen(...)` | Convenience open-file dialog. |
| `FileDialog.PickFilesToOpen(...)` | Convenience multi-select open-file dialog. |
| `FileDialog.PickFileToSave(...)` | Convenience save-file dialog. |
| `FileDialog.PickFolder(...)` | Convenience folder-picker dialog. |
| `FileOpenDialogBuilder` | Fluent open-file dialog configuration. |
| `FileSaveDialogBuilder` | Fluent save-file dialog configuration. |
| `FolderPickerBuilder` | Fluent folder picker configuration. |
| `FileDialogResult` | Cancellation and selection result. |

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs;

string path = FileDialog.PickFileToOpen(
    ownerHandle: IntPtr.Zero,
    title: "Open image",
    initialDirectory: Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
    filters: new[] { ("Images", "*.png;*.jpg;*.bmp"), ("All files", "*.*") },
    defaultExtension: "png");

if (path is not null)
{
    Console.WriteLine(path);
}
```

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs;

FileDialogResult result = new FileOpenDialogBuilder()
    .WithTitle("Import")
    .WithInitialDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
    .AddFilter("JSON", "*.json")
    .AddFilter("All files", "*.*")
    .AddPlace(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), atTop: true)
    .AllowMultipleSelection()
    .ShowDialog();

if (!result.WasCancelled)
{
    foreach (string selectedPath in result.SelectedPaths)
    {
        Console.WriteLine(selectedPath);
    }
}
```

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs;

FileDialogResult save = new FileSaveDialogBuilder()
    .WithTitle("Save report")
    .WithSuggestedFileName("report.txt")
    .WithDefaultExtension("txt")
    .AddFilter("Text", "*.txt")
    .ShowDialog();

FileDialogResult folder = new FolderPickerBuilder()
    .WithTitle("Output folder")
    .ShowDialog();
```

### Icons, Cursors, And Shell Images

Namespace: `CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons`

| API | Purpose |
| --- | --- |
| `IconHelper.GetAppLogo<TBitmap>(...)` | Load a Store app logo as `Bitmap`, `Icon`, or WPF `BitmapSource` where supported. |
| `IconHelper.ExtractAssociatedIcon<TIcon>(...)` | Extract an icon from an executable or DLL. |
| `IconHelper.CountAssociatedIcons(...)` | Count associated icons in a file. |
| `IconHelper.IconHandleTo<TIcon>(...)` | Convert native/safe icon handles. |
| `IconHelper.GetFileExtensionIcon<TIcon>(...)` | Resolve shell icon for an extension. |
| `IconHelper.GetFolderIcon<TIcon>(...)` | Resolve system folder icons. |
| `IconHelper.GetSystemIconSize(...)` and metric methods | Query recommended icon sizes. |
| `IconHelper.LoadIconWithSystemMetrics(...)`, `LoadIconWithScaleDown(...)` | Load resources at DPI-aware sizes. |
| `IconLoader` and `IconStreamExtensions` | Load icons from files/streams. |
| `IconFileWriter.WriteIconFile(...)`, `IconHelper.WriteIcon(...)` | Create `.ico` files from images. |
| `CursorHelper.TryGetCurrentCursor(...)`, `DrawCursorOnGraphics(...)`, `DrawCursorOnBitmap(...)` | Capture and render current cursor layers. |

```csharp
using System.Drawing;
using CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
using CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.Enums;

Icon notepadIcon = IconHelper.ExtractAssociatedIcon(
    @"C:\Windows\System32\notepad.exe",
    iconType: (Icon)null);

Bitmap txtIcon = IconHelper.GetFileExtensionIcon(
    "example.txt",
    iconType: (Bitmap)null,
    size: IconSize.Small,
    linkOverlay: false);

Size smallIconSize = IconHelper.GetSystemIconSize(IconMetricSize.SmallIcon);
```

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;

if (CursorHelper.TryGetCurrentCursor(out CapturedCursor cursor)
    && cursor.ColorLayer is not null)
{
    cursor.ColorLayer.Save("cursor.png");
}
```

### Desktop Composition

Namespace: `CP.ReactiveUI.Primitives.Windows.Desktop.Composition`

`DwmApi` wraps Desktop Window Manager calls for thumbnails, iconic previews, window attributes, composition, blur, and corner preferences. `DwmBlurBehind` and `DwmThumbnailProperties` are managed structures for the native DWM values.

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Composition;
using CP.ReactiveUI.Primitives.Windows.Desktop.Composition.Enums;
using CP.ReactiveUI.Primitives.Windows.Desktop.Windows;

IntPtr hwnd = InteropWindowQueryExtensions.GetForegroundWindow().Handle.DangerousGetHandle();

bool applied = DwmApi.SetWindowCornerPreference(
    hwnd,
    DwmWindowCornerPreference.Round);
```

### Power, Timers, And Multimedia

Namespaces:

- `CP.ReactiveUI.Primitives.Windows.Desktop.Power`
- `CP.ReactiveUI.Primitives.Windows.Desktop.Media`

| API | Purpose |
| --- | --- |
| `PowerBroadcastListener.PowerBroadcastEvents` | Observe all `WM_POWERBROADCAST` events. |
| `SystemSuspendingEvents`, `SystemResumedFromSuspendEvents`, `SystemAutomaticResumeEvents`, `PowerStatusChanges` | Filter common power events. |
| `PowerManagementApi.Sleep(...)`, `Hibernate(...)`, `Shutdown(...)`, `Restart(...)`, `LogOff(...)`, `ExitWindowsEx(...)`, `SetSuspendState(...)` | Initiate system power/session commands. |
| `WaitableTimer` | Managed waitable timer with one-shot, absolute, periodic, cancellation, and observable signal support. |
| `SystemStateApi` | Execution-state and waitable-timer native helpers. |
| `WinMm.PlaySystemSound(...)`, `Play(...)`, `StopPlaying()` | Play system sounds, resources, files/memory, and stop playback. |

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Power;

IDisposable suspending = PowerBroadcastListener.Suspending.Subscribe(_ =>
{
    Console.WriteLine("System is suspending.");
});

IDisposable resumed = PowerBroadcastListener.SystemResumedFromSuspendEvents.Subscribe(_ =>
{
    Console.WriteLine("System resumed.");
});
```

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Power;

using WaitableTimer timer = new();
timer.SetOnce(TimeSpan.FromSeconds(10));

using CancellationTokenSource cts = new(TimeSpan.FromMinutes(1));
await timer.WaitAsync(cts.Token);

IDisposable signals = timer.ObserveSignals().Subscribe(_ =>
{
    Console.WriteLine("Timer signaled.");
});
```

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Media;
using CP.ReactiveUI.Primitives.Windows.Desktop.Media.Enums;
using CP.ReactiveUI.Primitives.Windows.Desktop.Power;

WinMm.PlaySystemSound(SystemSounds.Asterisk);
WinMm.StopPlaying();

// These are real OS commands. Use only for deliberate user actions.
// PowerManagementApi.Sleep();
// PowerManagementApi.Restart(force: false);
```

### Application Restart And End Session

Namespace: `CP.ReactiveUI.Primitives.Windows.Desktop.Lifecycle`

| API | Purpose |
| --- | --- |
| `ApplicationRestartManager.RegisterForRestart(...)` | Register the current process with Windows Application Restart. |
| `UnregisterForRestart()` | Remove restart registration. |
| `WasRestartRequested()` | Check common restart command-line markers. |
| `GetRestartCommandLineArgs()` | Return restart arguments excluding the executable path. |
| `ObserveEndSessionMessages(...)` | Observe `WM_QUERYENDSESSION` and `WM_ENDSESSION`. |

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Lifecycle;
using CP.ReactiveUI.Primitives.Windows.Desktop.Lifecycle.Enums;

ApplicationRestartManager.RegisterForRestart(
    commandLineArgs: "--restart",
    flags: ApplicationRestartFlags.None);

bool restarted = ApplicationRestartManager.WasRestartRequested();
string[] restartArgs = ApplicationRestartManager.GetRestartCommandLineArgs();

IDisposable endSession = ApplicationRestartManager.ObserveEndSessionMessages(
    onQuerySession: reason =>
    {
        SaveDocuments();
        return true;
    },
    onEndSession: reason =>
    {
        FlushTelemetry();
        return true;
    })
    .Subscribe(message => Console.WriteLine(message.EndSessionReason));
```

### Software Inventory

Namespace: `CP.ReactiveUI.Primitives.Windows.Desktop.Software`

`InstallationInformation.InstalledSoftware()` reads uninstall registry entries and returns `SoftwareDetails`.

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Software;

foreach (SoftwareDetails software in InstallationInformation.InstalledSoftware())
{
    Console.WriteLine($"{software.DisplayName} {software.DisplayVersion}");
}
```

### Shell Dialog, Window, And UI Framework Extensions

WinForms:

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Windows;

IInteropWindow interop = myForm.AsInteropWindow();
WindowPlacement placement = myForm.RetrievePlacement();
myForm.ApplyPlacement(placement);
```

WPF:

```csharp
using CP.ReactiveUI.Primitives.Windows.Desktop.Windows;

IInteropWindow interop = myWindow.AsInteropWindow();
WindowPlacement placement = myWindow.RetrievePlacement();
myWindow.ApplyPlacement(placement);
```

### Optional Integrations

Namespace: `CP.ReactiveUI.Primitives.Windows.Integrations.Citrix`

`WinFrame` wraps Citrix WFAPI session information:

| API | Purpose |
| --- | --- |
| `WinFrame.IsAvailabe` | Returns whether WFAPI can be queried in the current process. |
| `GetClientIpAddress()` | Citrix client IP address. |
| `GetClientName()` | Citrix client name. |
| `QuerySessionConnectState()` | Current Citrix connection state. |
| `QuerySessionInformation(InfoClasses)` | Raw string query by Citrix info class. |
| `WaitSystemEvent(EventMask)` | Block until a Citrix system event occurs. |

```csharp
using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix;

if (WinFrame.IsAvailabe)
{
    Console.WriteLine(WinFrame.GetClientName());
    Console.WriteLine(WinFrame.GetClientIpAddress());
    Console.WriteLine(WinFrame.QuerySessionConnectState());
}
```

Namespace: `CP.ReactiveUI.Primitives.Windows.Integrations.Browser`

`InternetExplorerVersion` configures the legacy WinForms `WebBrowser` emulation mode. `ExtendedWebBrowser` hosts a `WebBrowser` control that suppresses script-error command handling through `IOleCommandTarget`.

```csharp
using CP.ReactiveUI.Primitives.Windows.Integrations.Browser;

int installedMajor = InternetExplorerVersion.Version;
int emulationVersion = InternetExplorerVersion.GetEmbVersion(ignoreDoctype: true);

InternetExplorerVersion.ChangeEmbeddedVersion(
    applicationName: "MyDesktopApp",
    browserVersion: emulationVersion);

var browser = new ExtendedWebBrowser
{
    Dock = DockStyle.Fill,
    ScriptErrorsSuppressed = true
};
```

## Reactive Package Variant

Use `CP.ReactiveUI.Primitives.Windows.Reactive` when your consuming project already uses System.Reactive-flavoured ReactiveUI.Primitives packages.

```powershell
dotnet add package CP.ReactiveUI.Primitives.Windows.Reactive
```

The package recompiles the `CP.ReactiveUI.Primitives.Windows` desktop source with `REACTIVE_SHIM`; it does not maintain a second implementation. Project-level aliases bind shared factory and scheduler/unit names to the System.Reactive-flavoured ReactiveUI.Primitives surface. Only the desktop namespace changes:

| Lean package | System.Reactive package |
|---|---|
| `CP.ReactiveUI.Primitives.Windows.Desktop.*` | `CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.*` |
| `ReactiveUI.Primitives` | `ReactiveUI.Primitives.Reactive` |
| `ReactiveUI.Primitives.RxVoid` | `System.Reactive.Unit` |
| `ReactiveUI.Primitives.Concurrency.ISequencer` | `System.Reactive.Concurrency.IScheduler` |

Core types are referenced, not recompiled, by both variants:

```csharp
using CP.ReactiveUI.Primitives.Windows.Native;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Power;
using System.Reactive.Linq;

// WaitableTimer is emitted by the .Reactive desktop assembly.
// NativeRect and the other Win32 value types retain their Core identity.
using var timer = new WaitableTimer();
using var subscription = timer
    .ObserveSignals()
    .Take(1)
    .Select(static elapsedAt => $"Timer elapsed at {elapsedAt:O}")
    .Subscribe(Console.WriteLine);

if (!timer.SetOnce(TimeSpan.FromSeconds(1)))
{
    throw new InvalidOperationException("The Windows waitable timer could not be armed.");
}
```

The shared source declares its package namespace with the following boundary:

```csharp
#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Power;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Power;
#endif
```

Choose one desktop variant per consumer project. Reference `CP.ReactiveUI.Primitives.Windows` for lean pipelines, or `CP.ReactiveUI.Primitives.Windows.Reactive` when the consumer standardizes on System.Reactive operators and scheduler/unit conventions.

## Quality Gates For Contributors

The repository solution is `src\CP.ReactiveUI.Primitives.Windows.slnx`.

Use NUKE for the standard local build. It uses only SDK targets supported by a stable Visual Studio installation:

```powershell
.\build.cmd Compile --Configuration Release
```

The equivalent direct solution build is:

```powershell
dotnet build .\src\CP.ReactiveUI.Primitives.Windows.slnx -c Release `
  -p:TreatWarningsAsErrors=true -p:WarningsAsErrors=true
```

To validate and package the complete target-framework surface, install a .NET 11 preview SDK (and use a preview-capable Visual Studio configuration), then opt in explicitly:

```powershell
.\build.cmd Compile --Configuration Release `
  --EnableDotNet11PreviewTargetFrameworks true
```

The BuildOnly CI workflow installs both .NET 10 and the .NET 11 preview SDK, enables the preview target automatically, runs the NUKE compile target, and executes the lean and Reactive shared-source TUnit hosts sequentially through Microsoft Testing Platform with Cobertura coverage. It also runs the .NET 11 preview test host. A stable Visual Studio installation does not select `net11.0-windows`, preventing `NETSDK1045`; the preview target remains part of CI and release validation.

Run the strict direct build with warnings as errors when diagnosing MSBuild behavior:

```powershell
dotnet build .\src\CP.ReactiveUI.Primitives.Windows.slnx -c Release --no-restore `
  -p:TreatWarningsAsErrors=true -p:WarningsAsErrors=true `
  -p:CodeAnalysisTreatWarningsAsErrors=true
```

Run TUnit tests through Microsoft Testing Platform with the repository coverage configuration:

```powershell
dotnet .\src\CP.ReactiveUI.Primitives.Windows.Tests\bin\Release\net10.0-windows\CP.ReactiveUI.Primitives.Windows.Tests.dll `
  --minimum-expected-tests 1 --config-file .\testconfig.json --coverage `
  --coverage-output .\.codex-diagnostics\coverage-final.cobertura.xml `
  --coverage-output-format cobertura
```

`testconfig.json` contains the generated-code exclusions. Do not pass `--coverage-settings` together with `--config-file`; this Microsoft Testing Platform version accepts one configuration source per run.

All projects use central analyzer package versions, project-level usings/aliases, and no diagnostic suppressions. Tests are TUnit tests and should use TUnit assertions only.
