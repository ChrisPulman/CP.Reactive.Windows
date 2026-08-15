// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Extensions;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;
using log4net;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
#endif
/// <summary>
/// Handles DPI changes. See
/// <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/dn469266.aspx">Writing DPI-Aware Desktop and Win32 Applications</a>.
/// </summary>
public sealed class DpiHandler : IDisposable
{
    /// <summary>The DPI message processing result.</summary>
    /// <param name="DpiChanged">A value indicating whether DPI data was found.</param>
    /// <param name="CurrentDpi">The current DPI value.</param>
    /// <param name="Handled">A value indicating whether the native message was handled.</param>
    private readonly record struct DpiMessageResult(bool DpiChanged, int CurrentDpi, bool Handled);

    /// <summary>The low-word mask used to extract the X-axis DPI from WM_DPICHANGED.</summary>
    private const int LowWordMask = 65_535;

    /// <summary>The logger for DPI handling.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(DpiHandler));

    /// <summary>The DPI change signal.</summary>
    private readonly Signal<DpiChangeInfo> _onDpiChanged = new();

    /// <summary>The scoped thread DPI-awareness context.</summary>
    private readonly IDisposable _scopedThreadDpiAwarenessContext;

    /// <summary>Stores whether the handler is running via a listener workaround.</summary>
    private bool _needsListenerWorkaround;

    /// <summary>Gets the current DPI for the UI element related to this handler.</summary>
    public int CurrentDpi { get; private set; }

    /// <summary>Gets or sets the message handler that must be disposed with this instance.</summary>
    internal IDisposable MessageHandler { get; set; }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.DpiHandler" /> class.</summary>
    public DpiHandler()
        : this(needsListenerWorkaround: false)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.DpiHandler" /> class.</summary>
    /// <param name="needsListenerWorkaround">A value indicating whether the listener workaround should be used.</param>
    public DpiHandler(bool needsListenerWorkaround)
    {
        _needsListenerWorkaround = needsListenerWorkaround;
        _scopedThreadDpiAwarenessContext = NativeDpiMethods.DefaultScopedThreadDpiAwarenessContext();
    }

    /// <summary>Enables non-client DPI scaling when the operating system supports it.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns><see langword="true" /> when non-client DPI scaling was enabled.</returns>
    public static bool TryEnableNonClientDpiScaling(IntPtr windowHandle)
    {
        if (!WindowsVersion.IsWindows10OrLater)
        {
            return false;
        }

        if (NativeDpiMethods.EnableNonClientDpiScaling(windowHandle).Succeeded())
        {
            return true;
        }

        Win32Error error = Win32.GetLastErrorCode();
        if (Log.IsDebugEnabled)
        {
            Log.DebugFormat("Error enabling non client dpi scaling : {0}", Win32.GetMessage(error));
        }

        return false;
    }

    /// <summary>Observes DPI changes for the related UI element.</summary>
    /// <returns>An observable sequence of DPI changes.</returns>
    public IObservable<DpiChangeInfo> ObserveDpiChanges() => _onDpiChanged;

    /// <summary>Scales the supplied number to the current DPI.</summary>
    /// <param name="someNumber">The number to scale.</param>
    /// <returns>The scaled number.</returns>
    public double ScaleWithCurrentDpi(double someNumber) => ScaleWithCurrentDpi(someNumber, null);

    /// <summary>Scales the supplied number to the current DPI.</summary>
    /// <param name="someNumber">The number to scale.</param>
    /// <param name="scaleModifier">A function that can modify the scale factor.</param>
    /// <returns>The scaled number.</returns>
    public double ScaleWithCurrentDpi(double someNumber, Func<float, float> scaleModifier) => DpiCalculator.ScaleWithDpi(someNumber, CurrentDpi, scaleModifier);

    /// <summary>Scales the supplied number to the current DPI.</summary>
    /// <param name="someNumber">The number to scale.</param>
    /// <returns>The scaled number.</returns>
    public int ScaleWithCurrentDpi(int someNumber) => ScaleWithCurrentDpi(someNumber, null);

    /// <summary>Scales the supplied number to the current DPI.</summary>
    /// <param name="someNumber">The number to scale.</param>
    /// <param name="scaleModifier">A function that can modify the scale factor.</param>
    /// <returns>The scaled number.</returns>
    public int ScaleWithCurrentDpi(int someNumber, Func<float, float> scaleModifier) => DpiCalculator.ScaleWithDpi(someNumber, CurrentDpi, scaleModifier);

    /// <summary>Scales the supplied size to the current DPI.</summary>
    /// <param name="size">The size to scale.</param>
    /// <returns>The scaled size.</returns>
    public NativeSize ScaleWithCurrentDpi(NativeSize size) => ScaleWithCurrentDpi(size, null);

    /// <summary>Scales the supplied size to the current DPI.</summary>
    /// <param name="size">The size to scale.</param>
    /// <param name="scaleModifier">A function that can modify the scale factor.</param>
    /// <returns>The scaled size.</returns>
    public NativeSize ScaleWithCurrentDpi(NativeSize size, Func<float, float> scaleModifier) => DpiCalculator.ScaleWithDpi(size, CurrentDpi, scaleModifier);

    /// <summary>Scales the supplied floating-point size to the current DPI.</summary>
    /// <param name="size">The size to scale.</param>
    /// <returns>The scaled size.</returns>
    public NativeSizeFloat ScaleWithCurrentDpi(NativeSizeFloat size) => ScaleWithCurrentDpi(size, null);

    /// <summary>Scales the supplied floating-point size to the current DPI.</summary>
    /// <param name="size">The size to scale.</param>
    /// <param name="scaleModifier">A function that can modify the scale factor.</param>
    /// <returns>The scaled size.</returns>
    public NativeSizeFloat ScaleWithCurrentDpi(NativeSizeFloat size, Func<float, float> scaleModifier) => DpiCalculator.ScaleWithDpi(size, CurrentDpi, scaleModifier);

    /// <summary>Scales the supplied point to the current DPI.</summary>
    /// <param name="point">The point to scale.</param>
    /// <returns>The scaled point.</returns>
    public NativePoint ScaleWithCurrentDpi(NativePoint point) => ScaleWithCurrentDpi(point, null);

    /// <summary>Scales the supplied point to the current DPI.</summary>
    /// <param name="point">The point to scale.</param>
    /// <param name="scaleModifier">A function that can modify the scale factor.</param>
    /// <returns>The scaled point.</returns>
    public NativePoint ScaleWithCurrentDpi(NativePoint point, Func<float, float> scaleModifier) => DpiCalculator.ScaleWithDpi(point, CurrentDpi, scaleModifier);

    /// <summary>Scales the supplied floating-point coordinates to the current DPI.</summary>
    /// <param name="point">The point to scale.</param>
    /// <returns>The scaled point.</returns>
    public NativePointFloat ScaleWithCurrentDpi(NativePointFloat point) => ScaleWithCurrentDpi(point, null);

    /// <summary>Scales the supplied floating-point coordinates to the current DPI.</summary>
    /// <param name="point">The point to scale.</param>
    /// <param name="scaleModifier">A function that can modify the scale factor.</param>
    /// <returns>The scaled point.</returns>
    public NativePointFloat ScaleWithCurrentDpi(NativePointFloat point, Func<float, float> scaleModifier) => DpiCalculator.ScaleWithDpi(point, CurrentDpi, scaleModifier);

    /// <summary>Unscales the supplied number from the current DPI.</summary>
    /// <param name="someNumber">The number to unscale.</param>
    /// <returns>The unscaled number.</returns>
    public double UnscaleWithCurrentDpi(double someNumber) => UnscaleWithCurrentDpi(someNumber, null);

    /// <summary>Unscales the supplied number from the current DPI.</summary>
    /// <param name="someNumber">The number to unscale.</param>
    /// <param name="scaleModifier">A function that can modify the scale factor.</param>
    /// <returns>The unscaled number.</returns>
    public double UnscaleWithCurrentDpi(double someNumber, Func<float, float> scaleModifier) => DpiCalculator.UnscaleWithDpi(someNumber, CurrentDpi, scaleModifier);

    /// <summary>Unscales the supplied number from the current DPI.</summary>
    /// <param name="someNumber">The number to unscale.</param>
    /// <returns>The unscaled number.</returns>
    public int UnscaleWithCurrentDpi(int someNumber) => UnscaleWithCurrentDpi(someNumber, null);

    /// <summary>Unscales the supplied number from the current DPI.</summary>
    /// <param name="someNumber">The number to unscale.</param>
    /// <param name="scaleModifier">A function that can modify the scale factor.</param>
    /// <returns>The unscaled number.</returns>
    public int UnscaleWithCurrentDpi(int someNumber, Func<float, float> scaleModifier) => DpiCalculator.UnscaleWithDpi(someNumber, CurrentDpi, scaleModifier);

    /// <summary>Unscales the supplied size from the current DPI.</summary>
    /// <param name="size">The size to unscale.</param>
    /// <returns>The unscaled size.</returns>
    public NativeSize UnscaleWithCurrentDpi(NativeSize size) => UnscaleWithCurrentDpi(size, null);

    /// <summary>Unscales the supplied size from the current DPI.</summary>
    /// <param name="size">The size to unscale.</param>
    /// <param name="scaleModifier">A function that can modify the scale factor.</param>
    /// <returns>The unscaled size.</returns>
    public NativeSize UnscaleWithCurrentDpi(NativeSize size, Func<float, float> scaleModifier) => DpiCalculator.UnscaleWithDpi(size, CurrentDpi, scaleModifier);

    /// <summary>Unscales the supplied floating-point size from the current DPI.</summary>
    /// <param name="size">The size to unscale.</param>
    /// <returns>The unscaled size.</returns>
    public NativeSizeFloat UnscaleWithCurrentDpi(NativeSizeFloat size) => UnscaleWithCurrentDpi(size, null);

    /// <summary>Unscales the supplied floating-point size from the current DPI.</summary>
    /// <param name="size">The size to unscale.</param>
    /// <param name="scaleModifier">A function that can modify the scale factor.</param>
    /// <returns>The unscaled size.</returns>
    public NativeSizeFloat UnscaleWithCurrentDpi(NativeSizeFloat size, Func<float, float> scaleModifier) => DpiCalculator.UnscaleWithDpi(size, CurrentDpi, scaleModifier);

    /// <summary>Unscales the supplied point from the current DPI.</summary>
    /// <param name="point">The point to unscale.</param>
    /// <returns>The unscaled point.</returns>
    public NativePoint UnscaleWithCurrentDpi(NativePoint point) => UnscaleWithCurrentDpi(point, null);

    /// <summary>Unscales the supplied point from the current DPI.</summary>
    /// <param name="point">The point to unscale.</param>
    /// <param name="scaleModifier">A function that can modify the scale factor.</param>
    /// <returns>The unscaled point.</returns>
    public NativePoint UnscaleWithCurrentDpi(NativePoint point, Func<float, float> scaleModifier) => DpiCalculator.UnscaleWithDpi(point, CurrentDpi, scaleModifier);

    /// <summary>Unscales the supplied floating-point coordinates from the current DPI.</summary>
    /// <param name="point">The point to unscale.</param>
    /// <returns>The unscaled point.</returns>
    public NativePointFloat UnscaleWithCurrentDpi(NativePointFloat point) => UnscaleWithCurrentDpi(point, null);

    /// <summary>Unscales the supplied floating-point coordinates from the current DPI.</summary>
    /// <param name="point">The point to unscale.</param>
    /// <param name="scaleModifier">A function that can modify the scale factor.</param>
    /// <returns>The unscaled point.</returns>
    public NativePointFloat UnscaleWithCurrentDpi(NativePointFloat point, Func<float, float> scaleModifier) => DpiCalculator.UnscaleWithDpi(point, CurrentDpi, scaleModifier);

    /// <inheritdoc />
    public void Dispose()
    {
        MessageHandler?.Dispose();
        MessageHandler = null;
        _scopedThreadDpiAwarenessContext.Dispose();
        _onDpiChanged.Dispose();
    }

    /// <summary>Handles WPF and WinForms window DPI messages.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="msg">The Windows message.</param>
    /// <param name="wordParameter">The word parameter.</param>
    /// <param name="longParameter">The long parameter.</param>
    /// <param name="handled">A value indicating whether the message was handled.</param>
    /// <returns>Zero.</returns>
    internal IntPtr HandleWindowMessages(IntPtr windowHandle, int msg, IntPtr wordParameter, IntPtr longParameter, ref bool handled)
    {
        if (HandleWindowMessages(WindowMessageInfo.Create(windowHandle.ToInt64(), msg, wordParameter.ToInt64(), longParameter.ToInt64())))
        {
            handled = true;
        }

        return IntPtr.Zero;
    }

    /// <summary>Handles DPI-aware window messages.</summary>
    /// <param name="windowMessageInfo">The window message information.</param>
    /// <returns><see langword="true" /> when the message was handled.</returns>
    internal bool HandleWindowMessages(WindowMessageInfo windowMessageInfo)
    {
        DpiMessageResult result = ProcessWindowMessage(windowMessageInfo);
        return ApplyDpiChange(result.DpiChanged, result.CurrentDpi, result.Handled);
    }

    /// <summary>Handles DPI-aware context-menu messages.</summary>
    /// <param name="windowMessageInfo">The window message information.</param>
    /// <returns>Zero.</returns>
    internal IntPtr HandleContextMenuMessages(WindowMessageInfo windowMessageInfo)
    {
        DpiMessageResult result = ProcessContextMenuMessage(windowMessageInfo);
        _ = ApplyDpiChange(result.DpiChanged, result.CurrentDpi, handled: false);
        return IntPtr.Zero;
    }

    /// <summary>Processes a window message into a DPI change result.</summary>
    /// <param name="windowMessageInfo">The window message information.</param>
    /// <returns>The DPI change result.</returns>
    private DpiMessageResult ProcessWindowMessage(WindowMessageInfo windowMessageInfo) => windowMessageInfo.Message switch
        {
            WindowsMessages.WM_NCCREATE => ProcessNonClientCreate(windowMessageInfo),
            WindowsMessages.WM_CREATE => ProcessWindowCreate(windowMessageInfo),
            WindowsMessages.WM_DPICHANGED => ProcessWindowDpiChanged(windowMessageInfo),
            WindowsMessages.WM_PAINT => ProcessWindowPaint(windowMessageInfo),
            WindowsMessages.WM_SETICON => ProcessWindowSetIcon(windowMessageInfo),
            WindowsMessages.WM_DPICHANGED_BEFOREPARENT => ProcessParentDpiMessage(windowMessageInfo, "before"),
            WindowsMessages.WM_DPICHANGED_AFTERPARENT => ProcessParentDpiMessage(windowMessageInfo, "after"),
            WindowsMessages.WM_DESTROY => ProcessWindowDestroy(windowMessageInfo),
            _ => default(DpiMessageResult),
        };

    /// <summary>Processes a context-menu message into a DPI change result.</summary>
    /// <param name="windowMessageInfo">The window message information.</param>
    /// <returns>The DPI change result.</returns>
    private DpiMessageResult ProcessContextMenuMessage(WindowMessageInfo windowMessageInfo) => windowMessageInfo.Message switch
        {
            WindowsMessages.WM_SHOWWINDOW => ProcessContextMenuShow(windowMessageInfo),
            WindowsMessages.WM_DESTROY => ProcessContextMenuDestroy(),
            _ => default(DpiMessageResult),
        };

    /// <summary>Processes a non-client create message.</summary>
    /// <param name="windowMessageInfo">The window message information.</param>
    /// <returns>The DPI change result.</returns>
    private DpiMessageResult ProcessNonClientCreate(WindowMessageInfo windowMessageInfo)
    {
        nint windowHandle = (nint)windowMessageInfo.Handle;
        LogVerbose("Processing {0} event, enabling DPI scaling for window {1}", windowMessageInfo.Message, windowHandle);
        _ = TryEnableNonClientDpiScaling(windowHandle);
        return default;
    }

    /// <summary>Processes a window create message.</summary>
    /// <param name="windowMessageInfo">The window message information.</param>
    /// <returns>The DPI change result.</returns>
    private DpiMessageResult ProcessWindowCreate(WindowMessageInfo windowMessageInfo)
    {
        nint windowHandle = (nint)windowMessageInfo.Handle;
        LogVerbose("Processing {0} event, retrieving DPI for window {1}", windowMessageInfo.Message, windowHandle);
        _scopedThreadDpiAwarenessContext.Dispose();
        return new(DpiChanged: true, NativeDpiMethods.GetDpi(windowHandle), Handled: false);
    }

    /// <summary>Processes a DPI changed message.</summary>
    /// <param name="windowMessageInfo">The window message information.</param>
    /// <returns>The DPI change result.</returns>
    private DpiMessageResult ProcessWindowDpiChanged(WindowMessageInfo windowMessageInfo)
    {
        nint windowHandle = (nint)windowMessageInfo.Handle;
        LogVerbose("Processing {0} event, resizing / positioning window {1}", windowMessageInfo.Message, windowHandle);
        NativeRect advisedRectangle = Marshal.PtrToStructure<NativeRect>((nint)windowMessageInfo.LongParam);
        _ = User32Api.SetWindowPos(windowHandle, IntPtr.Zero, advisedRectangle.Left, advisedRectangle.Top, advisedRectangle.Width, advisedRectangle.Height, WindowPos.SWP_NOACTIVATE | WindowPos.SWP_NOOWNERZORDER | WindowPos.SWP_NOZORDER);
        checked
        {
            int currentDpi = (int)unchecked((nint)windowMessageInfo.WordParam) & 0xFFFF;
            return new(DpiChanged: true, currentDpi, Handled: true);
        }
    }

    /// <summary>Processes a paint message.</summary>
    /// <param name="windowMessageInfo">The window message information.</param>
    /// <returns>The DPI change result.</returns>
    private DpiMessageResult ProcessWindowPaint(WindowMessageInfo windowMessageInfo)
    {
        if (CurrentDpi != 0)
        {
            return default;
        }

        return new(DpiChanged: true, NativeDpiMethods.GetDpi((nint)windowMessageInfo.Handle), Handled: false);
    }

    /// <summary>Processes a set-icon message.</summary>
    /// <param name="windowMessageInfo">The window message information.</param>
    /// <returns>The DPI change result.</returns>
    private DpiMessageResult ProcessWindowSetIcon(WindowMessageInfo windowMessageInfo)
    {
        if (!_needsListenerWorkaround)
        {
            return default;
        }

        _needsListenerWorkaround = false;
        return new(DpiChanged: true, NativeDpiMethods.GetDpi((nint)windowMessageInfo.Handle), Handled: false);
    }

    /// <summary>Processes a parent DPI notification message.</summary>
    /// <param name="windowMessageInfo">The window message information.</param>
    /// <param name="position">The parent notification position.</param>
    /// <returns>The DPI change result.</returns>
    private DpiMessageResult ProcessParentDpiMessage(WindowMessageInfo windowMessageInfo, string position)
    {
        LogVerbose("Dpi changed on {0} {1} parent", (nint)windowMessageInfo.Handle, position);
        return default;
    }

    /// <summary>Processes a window destroy message.</summary>
    /// <param name="windowMessageInfo">The window message information.</param>
    /// <returns>The DPI change result.</returns>
    private DpiMessageResult ProcessWindowDestroy(WindowMessageInfo windowMessageInfo)
    {
        LogVerbose("Completing the observable for {0}", (nint)windowMessageInfo.Handle);
        _onDpiChanged.OnCompleted();
        Dispose();
        return default;
    }

    /// <summary>Processes a context-menu show message.</summary>
    /// <param name="windowMessageInfo">The window message information.</param>
    /// <returns>The DPI change result.</returns>
    private DpiMessageResult ProcessContextMenuShow(WindowMessageInfo windowMessageInfo)
    {
        nint windowHandle = (nint)windowMessageInfo.Handle;
        LogVerbose("Processing {0} event, retrieving DPI for ContextMenuStrip {1}", windowMessageInfo.Message, windowHandle);
        return new(DpiChanged: true, NativeDpiMethods.GetDpi(windowHandle), Handled: false);
    }

    /// <summary>Processes a context-menu destroy message.</summary>
    /// <returns>The DPI change result.</returns>
    private DpiMessageResult ProcessContextMenuDestroy()
    {
        _onDpiChanged.OnCompleted();
        return default;
    }

    /// <summary>Applies a DPI change to the current handler state.</summary>
    /// <param name="isDpiMessage">A value indicating whether the message carries DPI data.</param>
    /// <param name="currentDpi">The current DPI.</param>
    /// <param name="handled">A value indicating whether the original message was handled.</param>
    /// <returns><paramref name="handled" /> when the message carries DPI data; otherwise, <see langword="false" />.</returns>
    private bool ApplyDpiChange(bool isDpiMessage, int currentDpi, bool handled)
    {
        if (!isDpiMessage)
        {
            return false;
        }

        if (CurrentDpi != currentDpi)
        {
            PublishDpiChange(currentDpi);
        }
        else
        {
            LogVerbose("DPI was unchanged from {0}", CurrentDpi);
        }

        return handled;
    }

    /// <summary>Publishes a changed DPI value.</summary>
    /// <param name="currentDpi">The current DPI.</param>
    private void PublishDpiChange(int currentDpi)
    {
        int beforeDpi = CurrentDpi;
        LogVerbose("Changing DPI from {0} to {1}", beforeDpi, currentDpi);
        CurrentDpi = currentDpi;
        _onDpiChanged.OnNext(new(beforeDpi, currentDpi));
    }

    /// <summary>Writes a verbose log message when verbose logging is enabled.</summary>
    /// <param name="message">The message template.</param>
    /// <param name="arguments">The message arguments.</param>
    private void LogVerbose(string message, params object[] arguments)
    {
        GC.KeepAlive(MessageHandler);
        if (Log.IsDebugEnabled)
        {
            Log.DebugFormat(message, arguments);
        }
    }
}
