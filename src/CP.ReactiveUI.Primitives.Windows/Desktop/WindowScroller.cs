// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using CP.ReactiveUI.Primitives.Windows.Native;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using Microsoft.Win32;
using log4net;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>The is a container class to help to scroll a window.</summary>
public class WindowScroller
{
    /// <summary>Restores overridden scroll operations.</summary>
    internal sealed class OperationsOverride : IDisposable
    {
        /// <summary>Previous operations to restore.</summary>
        private readonly WindowScrollerOperations _previous;

        /// <summary>Tracks whether this scope has already restored the previous operations.</summary>
        private int _disposed;

        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Windows.WindowScroller.OperationsOverride" /> class.</summary>
        /// <param name="previous">Previous operations to restore.</param>
        internal OperationsOverride(WindowScrollerOperations previous)
        {
            _previous = previous;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _ = Interlocked.Exchange(ref _operations, _previous);
            }
        }
    }

    /// <summary>Operations used by <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Windows.WindowScroller" /> to interact with native scroll state and input.</summary>
    internal class WindowScrollerOperations
    {
        /// <summary>Retrieves native scroll position information.</summary>
        /// <param name="windowHandle">The target window handle.</param>
        /// <param name="scrollBar">The target scroll bar.</param>
        /// <param name="scrollInfo">The scroll information to populate.</param>
        /// <returns><see langword="true" /> when scroll information was retrieved.</returns>
        internal virtual bool GetScrollInfo(IntPtr windowHandle, ScrollBarTypes scrollBar, ref ScrollInfo scrollInfo) => User32Api.GetScrollInfo(windowHandle, scrollBar, ref scrollInfo);

        /// <summary>Applies native scroll position information.</summary>
        /// <param name="windowHandle">The target window handle.</param>
        /// <param name="scrollBar">The target scroll bar.</param>
        /// <param name="scrollInfo">The scroll information to apply.</param>
        /// <param name="redraw">Whether the target should redraw.</param>
        /// <returns>The native result.</returns>
        internal virtual int SetScrollInfo(IntPtr windowHandle, ScrollBarTypes scrollBar, ref ScrollInfo scrollInfo, bool redraw) => User32Api.SetScrollInfo(windowHandle, scrollBar, ref scrollInfo, redraw);

        /// <summary>Sends a scroll command message.</summary>
        /// <param name="windowHandle">The target window handle.</param>
        /// <param name="message">The Windows message.</param>
        /// <param name="scrollBarCommand">The scroll command.</param>
        /// <param name="parameter">The message parameter.</param>
        /// <returns>The native result.</returns>
        internal virtual int SendCommandMessage(IntPtr windowHandle, WindowsMessages message, ScrollBarCommands scrollBarCommand, int parameter) => User32Api.SendMessage(windowHandle, message, scrollBarCommand, parameter);

        /// <summary>Sends an integer scroll position message.</summary>
        /// <param name="windowHandle">The target window handle.</param>
        /// <param name="message">The Windows message.</param>
        /// <param name="wordParameter">The word parameter.</param>
        /// <param name="parameter">The message parameter.</param>
        /// <returns>The native result.</returns>
        internal virtual IntPtr SendIntegerMessage(IntPtr windowHandle, WindowsMessages message, int wordParameter, int parameter) => User32Api.SendMessage(windowHandle, message, wordParameter, parameter);

        /// <summary>Retrieves native scroll bar information.</summary>
        /// <param name="windowHandle">The target window handle.</param>
        /// <param name="objectId">The native object identifier.</param>
        /// <param name="scrollBarInfo">The scroll bar information to populate.</param>
        /// <returns><see langword="true" /> when scroll bar information was retrieved.</returns>
        internal virtual bool GetScrollBarInfo(IntPtr windowHandle, ObjectIdentifiers objectId, ref ScrollBarInfo scrollBarInfo) => User32Api.GetScrollBarInfo(windowHandle, objectId, ref scrollBarInfo);

        /// <summary>Gets the configured scroll wheel line count text.</summary>
        /// <returns>The registry value text, or <see langword="null" /> when unavailable.</returns>
        internal virtual string GetScrollWheelLines()
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", writable: false);
            return key?.GetValue("WheelScrollLines") as string;
        }

        /// <summary>Generates key down input.</summary>
        /// <param name="keycodes">The key codes to generate.</param>
        /// <returns>The generated input count.</returns>
        internal virtual uint KeyDown(params VirtualKeyCode[] keycodes) => KeyboardInputGenerator.KeyDown(keycodes);

        /// <summary>Generates key press input.</summary>
        /// <param name="keycodes">The key codes to generate.</param>
        /// <returns>The generated input count.</returns>
        internal virtual uint KeyPresses(params VirtualKeyCode[] keycodes) => KeyboardInputGenerator.KeyPresses(keycodes);

        /// <summary>Generates key up input.</summary>
        /// <param name="keycodes">The key codes to generate.</param>
        /// <returns>The generated input count.</returns>
        internal virtual uint KeyUp(params VirtualKeyCode[] keycodes) => KeyboardInputGenerator.KeyUp(keycodes);

        /// <summary>Generates mouse-wheel input.</summary>
        /// <param name="wheelDelta">The wheel delta.</param>
        /// <param name="location">The target location.</param>
        /// <returns>The generated input count.</returns>
        internal virtual uint MoveMouseWheel(int wheelDelta, NativePoint? location)
        {
            if (wheelDelta != 0 || location.HasValue)
            {
                return MouseInputGenerator.MoveMouseWheel(wheelDelta, location);
            }

            return 0U;
        }
    }

    /// <summary>Default number of lines scrolled for one mouse wheel detent.</summary>
    private const int DefaultScrollWheelLines = 3;

    /// <summary>Divisor used to calculate a rectangle center point.</summary>
    private const int CoordinateCenterDivisor = 2;

    /// <summary>Number of key events generated by a successful page key press.</summary>
    private const uint KeyboardPageKeyPressCount = 2U;

    /// <summary>Number of mouse-wheel events generated by a successful wheel move.</summary>
    private const int MouseWheelMoveCount = 1;

    /// <summary>Scale factor used by WM_HSCROLL thumb positioning.</summary>
    private const int HorizontalThumbPositionScale = 65_536;

    /// <summary>Base value used by WM_HSCROLL thumb positioning.</summary>
    private const int HorizontalThumbPositionBase = 4;

    /// <summary>Bit shift used by WM_VSCROLL thumb positioning.</summary>
    private const int VerticalThumbPositionShift = 16;

    /// <summary>Offset from the last visible scroll position.</summary>
    private const int LastVisibleScrollItemOffset = 1;

    /// <summary>Error message used for unsupported scroll modes.</summary>
    private const string UnsupportedScrollModeMessage = "Unsupported scroll mode.";

    /// <summary>Logger for scroll operations.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(WindowScroller));

    /// <summary>Native operations used by this scroller.</summary>
    private static WindowScrollerOperations _operations = new();

    /// <summary>Gets the scroll-lines value from the registry.</summary>
    public static int ScrollWheelLinesFromRegistry
    {
        get
        {
            string wheelScrollLines = Volatile.Read(ref _operations).GetScrollWheelLines();
            if (wheelScrollLines is null)
            {
                return 3;
            }

            if (!int.TryParse(wheelScrollLines, out var scrollLines))
            {
                return 3;
            }

            return scrollLines;
        }
    }

    /// <summary>Gets the initial scroll information used for reset and bounds checks.</summary>
    public ScrollInfo InitialScrollInfo { get; internal set; }

    /// <summary>Gets a value indicating whether the scroller is at the end.</summary>
    public bool IsAtEnd
    {
        get
        {
            if (!GetPosition(out var scrollInfo))
            {
                return false;
            }

            checked
            {
                long currentEnd = Math.Max(scrollInfo.Position, scrollInfo.TrackingPosition) + scrollInfo.PageSize;
                return (KeepInitialBounds ? InitialScrollInfo.Maximum : scrollInfo.Maximum) <= currentEnd - 1;
            }
        }
    }

    /// <summary>Gets a value indicating whether the scroller is at the start.</summary>
    public bool IsAtStart
    {
        get
        {
            if (!GetPosition(out var scrollInfo))
            {
                return false;
            }

            int currentStart = Math.Max(scrollInfo.Position, scrollInfo.TrackingPosition);
            return (KeepInitialBounds ? InitialScrollInfo.Minimum : scrollInfo.Minimum) >= currentStart;
        }
    }

    /// <summary>Gets or sets a value indicating whether scrolling stays within the initial bounds.</summary>
    public bool KeepInitialBounds { get; set; } = true;

    /// <summary>Gets the information on the used scrollbar, if any.</summary>
    public ScrollBarInfo? ScrollBar { get; internal set; }

    /// <summary>Gets the scrollbar type to use.</summary>
    public ScrollBarTypes ScrollBarType { get; internal set; } = ScrollBarTypes.Vertical;

    /// <summary>Gets or sets the area of the scrollbar, which can be the WindowToScroll.</summary>
    public IInteropWindow ScrollBarWindow { get; set; }

    /// <summary>Gets or sets the area which is scrolling, which can be the WindowToScroll.</summary>
    public IInteropWindow ScrollingWindow { get; set; }

    /// <summary>Gets or sets the scroll mode to use.</summary>
    public ScrollModes ScrollMode { get; set; } = ScrollModes.WindowsMessage;

    /// <summary>Gets or sets a value indicating whether the scrollbar should represent changes.</summary>
    public bool ShowChanges { get; set; } = true;

    /// <summary>Gets or sets the scroll-wheel delta.</summary>
    public int WheelDelta { get; set; }

    /// <summary>Move to the end.</summary>
    /// <returns>bool if this worked.</returns>
    public bool End() => ScrollMode switch
        {
            ScrollModes.KeyboardPageUpDown => SendControlKey(VirtualKeyCode.End),
            ScrollModes.WindowsMessage => SendScrollMessage(ScrollBarCommands.SB_BOTTOM),
            ScrollModes.AbsoluteWindowMessage => MoveToAbsoluteBoundary(end: true),
            ScrollModes.MouseWheel => MoveToBoundaryWithMouseWheel(end: true),
            _ => throw new ArgumentOutOfRangeException("ScrollMode", ScrollMode, "Unsupported scroll mode."),
        };

    /// <summary>Get current position.</summary>
    /// <param name="scrollInfo">ScrollInfo out.</param>
    /// <returns>SCROLLINFO.</returns>
    public bool GetPosition(out ScrollInfo scrollInfo)
    {
        scrollInfo = ScrollInfo.Create(ScrollInfoMask.All);
        return Volatile.Read(ref _operations).GetScrollInfo(ScrollBarWindow.Handle, ScrollBarType, ref scrollInfo);
    }

    /// <summary>Method to set the ScrollbarInfo, if we can get it.</summary>
    /// <returns>ScrollBarInfo?</returns>
    public ScrollBarInfo? GetScrollbarInfo() => GetScrollbarInfo(forceUpdate: false);

    /// <summary>Method to set the ScrollbarInfo, if we can get it.</summary>
    /// <param name="forceUpdate">set to true to force an update.</param>
    /// <returns>ScrollBarInfo?</returns>
    public ScrollBarInfo? GetScrollbarInfo(bool forceUpdate)
    {
        if (ScrollBar.HasValue && !forceUpdate)
        {
            return ScrollBar;
        }

        ObjectIdentifiers objectId = GetObjectIdentifier();
        ScrollBarInfo scrollbarInfo = ScrollBarInfo.Create();
        if (!Volatile.Read(ref _operations).GetScrollBarInfo(ScrollBarWindow.Handle, objectId, ref scrollbarInfo))
        {
            Win32Error error = Win32.GetLastErrorCode();
            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("Error retrieving Scrollbar info : {0}", Win32.GetMessage(error));
            }

            return null;
        }

        ScrollBar = scrollbarInfo;
        return scrollbarInfo;
    }

    /// <summary>Returns true if the window needs focus to scroll.</summary>
    /// <returns>true if focus is needed.</returns>
    public bool NeedsFocus() => ScrollMode == ScrollModes.KeyboardPageUpDown;

    /// <summary>Go to the next "page".</summary>
    /// <returns>bool if this worked.</returns>
    public bool Next() => ScrollMode switch
        {
            ScrollModes.KeyboardPageUpDown => Volatile.Read(ref _operations).KeyPresses(VirtualKeyCode.Next) == 2,
            ScrollModes.WindowsMessage => SendScrollMessage(ScrollBarCommands.SB_PAGEDOWN),
            ScrollModes.AbsoluteWindowMessage => MoveByPage(forward: true),
            ScrollModes.MouseWheel => MoveMouseWheel(forward: true),
            _ => throw new ArgumentOutOfRangeException("ScrollMode", ScrollMode, "Unsupported scroll mode."),
        };

    /// <summary>Go to the previous "page".</summary>
    /// <returns>bool if this worked.</returns>
    public bool Previous() => ScrollMode switch
        {
            ScrollModes.KeyboardPageUpDown => Volatile.Read(ref _operations).KeyPresses(VirtualKeyCode.Prior) == 2,
            ScrollModes.WindowsMessage => SendScrollMessage(ScrollBarCommands.SB_PAGEUP),
            ScrollModes.AbsoluteWindowMessage => MoveByPage(forward: false),
            ScrollModes.MouseWheel => MoveMouseWheel(forward: false),
            _ => throw new ArgumentOutOfRangeException("ScrollMode", ScrollMode, "Unsupported scroll mode."),
        };

    /// <summary>Set the position back to the original, only works for windows which support ScrollModes.WindowsMessage.</summary>
    /// <returns>true if this worked.</returns>
    public bool Reset()
    {
        ScrollInfo initialScrollInfo = InitialScrollInfo;
        return ApplyPosition(ref initialScrollInfo);
    }

    /// <summary>Move to the start.</summary>
    /// <returns>bool if this worked.</returns>
    public bool Start() => ScrollMode switch
        {
            ScrollModes.KeyboardPageUpDown => SendControlKey(VirtualKeyCode.Home),
            ScrollModes.WindowsMessage => SendScrollMessage(ScrollBarCommands.SB_TOP),
            ScrollModes.AbsoluteWindowMessage => MoveToAbsoluteBoundary(end: false),
            ScrollModes.MouseWheel => MoveToBoundaryWithMouseWheel(end: false),
            _ => throw new ArgumentOutOfRangeException("ScrollMode", ScrollMode, "Unsupported scroll mode."),
        };

    /// <summary>Overrides native scroll operations while the returned scope is alive.</summary>
    /// <param name="operations">The replacement operations.</param>
    /// <returns>A scope that restores the previous operations when disposed.</returns>
    internal static IDisposable OverrideOperationsForTesting(WindowScrollerOperations operations)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(operations);
        return new OperationsOverride(Interlocked.Exchange(ref _operations, operations));
    }

    /// <summary>Sends a control-modified navigation key.</summary>
    /// <param name="key">The navigation key to send.</param>
    /// <returns><see langword="true" /> after the key sequence is sent.</returns>
    private static bool SendControlKey(VirtualKeyCode key)
    {
        WindowScrollerOperations operations = Volatile.Read(ref _operations);
        _ = operations.KeyDown(VirtualKeyCode.Control);
        _ = operations.KeyPresses(key);
        _ = operations.KeyUp(VirtualKeyCode.Control);
        return true;
    }

    /// <summary>Apply position from the scrollInfo.</summary>
    /// <param name="scrollInfo">SCROLLINFO ref.</param>
    /// <returns>bool.</returns>
    private bool ApplyPosition(ref ScrollInfo scrollInfo)
    {
        if (ShowChanges)
        {
            _ = Volatile.Read(ref _operations).SetScrollInfo(ScrollBarWindow.Handle, ScrollBarType, ref scrollInfo, redraw: true);
        }

        checked
        {
            if (ScrollBarType == ScrollBarTypes.Horizontal)
            {
                _ = Volatile.Read(ref _operations).SendIntegerMessage(ScrollingWindow.Handle, WindowsMessages.WM_HSCROLL, 4 + (65_536 * scrollInfo.Position), 0);
                return true;
            }

            ScrollBarTypes scrollBarType = ScrollBarType;
            if (unchecked((uint)(scrollBarType - 1)) <= 1U)
            {
                _ = Volatile.Read(ref _operations).SendIntegerMessage(ScrollingWindow.Handle, WindowsMessages.WM_VSCROLL, (int)(4L + unchecked((long)(scrollInfo.Position << 16))), 0);
                return true;
            }

            if (ScrollBarType != ScrollBarTypes.Both)
            {
                throw new ArgumentOutOfRangeException("ScrollBarType", ScrollBarType, "Unsupported scroll bar type.");
            }

            return true;
        }
    }

    /// <summary>Gets the scroll bar object identifier.</summary>
    /// <returns>The matching object identifier.</returns>
    private ObjectIdentifiers GetObjectIdentifier()
    {
        return ScrollBarType switch
        {
            ScrollBarTypes.Control or ScrollBarTypes.Both => ObjectIdentifiers.Client,
            ScrollBarTypes.Vertical => ObjectIdentifiers.VerticalScrollbar,
            ScrollBarTypes.Horizontal => ObjectIdentifiers.HorizontalScrollbar,
            _ => throw new ArgumentOutOfRangeException("ScrollBarType", ScrollBarType, "Unsupported scroll bar type.")
        };
    }

    /// <summary>Gets the middle point of the scrolling window.</summary>
    /// <returns>The middle point.</returns>
    private NativePoint GetScrollMiddlePoint()
    {
        NativeRect bounds = ScrollingWindow.GetInfo().Bounds;
        checked
        {
            return new(bounds.X + unchecked(bounds.Width / 2), bounds.Y + unchecked(bounds.Height / 2));
        }
    }

    /// <summary>Moves by a single page in the requested direction.</summary>
    /// <param name="forward">Whether to move toward the end of the scrollbar.</param>
    /// <returns><see langword="true" /> when the position could be applied.</returns>
    private bool MoveByPage(bool forward)
    {
        if (!TryRetrievePosition(out var scrollInfo))
        {
            return false;
        }

        scrollInfo.Position = checked(forward ? Math.Min(scrollInfo.Maximum, scrollInfo.Position + (int)scrollInfo.PageSize) : Math.Max(scrollInfo.Minimum, scrollInfo.Position - (int)scrollInfo.PageSize));
        return ApplyPosition(ref scrollInfo);
    }

    /// <summary>Moves to an absolute scrollbar boundary.</summary>
    /// <param name="end">Whether to move to the end rather than the start.</param>
    /// <returns><see langword="true" /> when the position could be applied.</returns>
    private bool MoveToAbsoluteBoundary(bool end)
    {
        if (!TryRetrievePosition(out var scrollInfo))
        {
            return false;
        }

        scrollInfo.Position = (end ? scrollInfo.Maximum : scrollInfo.Minimum);
        return ApplyPosition(ref scrollInfo);
    }

    /// <summary>Moves a mouse-wheel scroller until it reaches the requested boundary.</summary>
    /// <param name="end">Whether to move toward the end rather than the start.</param>
    /// <returns><see langword="true" /> after movement stops.</returns>
    private bool MoveToBoundaryWithMouseWheel(bool end)
    {
        while ((end ? (!IsAtEnd) : (!IsAtStart)) && !(end ? (!Next()) : (!Previous())))
        {
        }

        return true;
    }

    /// <summary>Moves the mouse wheel by one configured increment.</summary>
    /// <param name="forward">Whether to move toward the end of the scrollbar.</param>
    /// <returns><see langword="true" /> when the wheel event was generated.</returns>
    private bool MoveMouseWheel(bool forward) => Volatile.Read(ref _operations).MoveMouseWheel(forward ? checked(-WheelDelta) : WheelDelta, GetScrollMiddlePoint()) == 1;

    /// <summary>Helper method to send the right message.</summary>
    /// <param name="scrollBarCommand">ScrollBarCommands enum to specify where to scroll.</param>
    /// <returns>true if this was possible.</returns>
    private bool SendScrollMessage(ScrollBarCommands scrollBarCommand)
    {
        ScrollBarTypes scrollBarType = ScrollBarType;
        if ((uint)scrollBarType > 1U)
        {
            return false;
        }

        WindowsMessages message = ((ScrollBarType == ScrollBarTypes.Horizontal) ? WindowsMessages.WM_HSCROLL : WindowsMessages.WM_VSCROLL);
        _ = Volatile.Read(ref _operations).SendCommandMessage(ScrollingWindow.Handle, message, scrollBarCommand, 0);
        return true;
    }

    /// <summary>Retrieve position from the scrollInfo.</summary>
    /// <param name="scrollInfo">ScrollInfo out.</param>
    /// <returns>bool.</returns>
    private bool TryRetrievePosition(out ScrollInfo scrollInfo)
    {
        bool hasScrollInfo = GetPosition(out scrollInfo);
        if (Log.IsDebugEnabled)
        {
            if (hasScrollInfo)
            {
                Log.DebugFormat("Retrieved ScrollInfo: {0}", scrollInfo);
            }
            else
            {
                Log.Debug("Couldn't get scrollinfo.");
            }
        }

        return hasScrollInfo;
    }
}
