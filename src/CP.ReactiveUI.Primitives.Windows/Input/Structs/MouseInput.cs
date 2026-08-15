// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     Contains information about a simulated mouse event.
///     See
///     <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646273(v=vs.85).aspx">MOUSEINPUT structure</a>
/// </summary>
public readonly record struct MouseInput
{
    /// <summary>The native mouse button input data.</summary>
    /// <param name="Flags">The mouse event flags.</param>
    /// <param name="MouseData">The mouse data.</param>
    private readonly record struct MouseButtonInputData(MouseEventFlags Flags, uint MouseData);

    /// <summary>Gets the x coordinate or movement delta.</summary>
    public int Dx => _dx;

    /// <summary>Gets the y coordinate or movement delta.</summary>
    public int Dy => _dy;

    /// <summary>Gets the mouse button or wheel data.</summary>
    public int MouseData => (int)_mouseData;

    /// <summary>Gets the mouse event flags.</summary>
    public MouseEventFlags MouseEventFlags => _mouseEventFlags;

    /// <summary>Gets the mouse event timestamp.</summary>
    public uint Timestamp => _timestamp;

    /// <summary>Gets native extra information associated with the mouse event.</summary>
    internal UIntPtr ExtraInfo => _extraInfo;

    /// <summary>The mouse data value for the first extended button.</summary>
    private const int XButton1Data = 1;

    /// <summary>The mouse data value for the second extended button.</summary>
    private const int XButton2Data = 2;

    /// <summary>The maximum absolute mouse coordinate.</summary>
    private const int AbsoluteCoordinateMaximum = 65_535;

    /// <summary>Gets the flags used when a mouse location is supplied.</summary>
    private const MouseEventFlags MouseMoveMouseEventFlags = MouseEventFlags.Move | MouseEventFlags.Virtualdesk | MouseEventFlags.Absolute;

    /// <summary>Stores the x coordinate or movement delta.</summary>
    private readonly int _dx;

    /// <summary>Stores the y coordinate or movement delta.</summary>
    private readonly int _dy;

    /// <summary>Stores the mouse button or wheel data.</summary>
    private readonly uint _mouseData;

    /// <summary>Stores the mouse event flags.</summary>
    private readonly MouseEventFlags _mouseEventFlags;

    /// <summary>Stores the mouse event timestamp.</summary>
    private readonly uint _timestamp;

    /// <summary>Stores native extra information associated with the mouse event.</summary>
    private readonly UIntPtr _extraInfo;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs.MouseInput" /> struct.</summary>
    /// <param name="dx">The x coordinate or movement delta.</param>
    /// <param name="dy">The y coordinate or movement delta.</param>
    /// <param name="mouseData">The mouse button or wheel data.</param>
    /// <param name="mouseEventFlags">The mouse event flags.</param>
    /// <param name="timestamp">The mouse event timestamp.</param>
    private MouseInput(int dx, int dy, uint mouseData, MouseEventFlags mouseEventFlags, uint timestamp)
        : this(dx, dy, mouseData, mouseEventFlags, timestamp, UIntPtr.Zero)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs.MouseInput" /> struct.</summary>
    /// <param name="dx">The x coordinate or movement delta.</param>
    /// <param name="dy">The y coordinate or movement delta.</param>
    /// <param name="mouseData">The mouse button or wheel data.</param>
    /// <param name="mouseEventFlags">The mouse event flags.</param>
    /// <param name="timestamp">The mouse event timestamp.</param>
    /// <param name="extraInfo">The native extra information associated with the mouse event.</param>
    private MouseInput(int dx, int dy, uint mouseData, MouseEventFlags mouseEventFlags, uint timestamp, UIntPtr extraInfo)
    {
        _dx = dx;
        _dy = dy;
        _mouseData = mouseData;
        _mouseEventFlags = mouseEventFlags;
        _timestamp = timestamp;
        _extraInfo = extraInfo;
    }

    /// <summary>Create a MouseInput struct for a wheel move.</summary>
    /// <param name="wheelDelta">How much does the wheel move.</param>
    /// <returns>MouseInput.</returns>
    public static MouseInput MoveMouseWheel(int wheelDelta) => MoveMouseWheel(wheelDelta, null, null);

    /// <summary>Create a MouseInput struct for a wheel move at a specific location.</summary>
    /// <param name="wheelDelta">How much does the wheel move.</param>
    /// <param name="location">Location of the event.</param>
    /// <returns>MouseInput.</returns>
    public static MouseInput MoveMouseWheel(int wheelDelta, NativePoint? location) => MoveMouseWheel(wheelDelta, location, null);

    /// <summary>Create a MouseInput struct for a wheel move.</summary>
    /// <param name="wheelDelta">How much does the wheel move.</param>
    /// <param name="location">Location of the event.</param>
    /// <param name="timestamp">The time stamp for the event.</param>
    /// <returns>MouseInput.</returns>
    public static MouseInput MoveMouseWheel(int wheelDelta, NativePoint? location, uint? timestamp)
    {
        location = RemapLocation(location);
        MouseEventFlags mouseEventFlags = (location.HasValue ? (MouseEventFlags.Move | MouseEventFlags.Virtualdesk | MouseEventFlags.Absolute) : MouseEventFlags.None);
        uint messageTime = timestamp ?? checked((uint)Environment.TickCount);
        return new(location?.X ?? 0, location?.Y ?? 0, (uint)wheelDelta, mouseEventFlags | MouseEventFlags.Wheel, messageTime);
    }

    /// <summary>Create a MouseInput struct for a mouse move.</summary>
    /// <param name="location">Where is the click located.</param>
    /// <returns>MouseInput.</returns>
    public static MouseInput MouseMove(NativePoint location) => MouseMove(location, null);

    /// <summary>Create a MouseInput struct for a mouse move.</summary>
    /// <param name="location">Where is the click located.</param>
    /// <param name="timestamp">The time stamp for the event.</param>
    /// <returns>MouseInput.</returns>
    public static MouseInput MouseMove(NativePoint location, uint? timestamp)
    {
        location = RemapLocation(location);
        uint messageTime = timestamp ?? checked((uint)Environment.TickCount);
        return new(location.X, location.Y, 0U, MouseEventFlags.Move | MouseEventFlags.Virtualdesk | MouseEventFlags.Absolute, messageTime);
    }

    /// <summary>Create a MouseInput struct for a mouse button down.</summary>
    /// <param name="mouseButtons">MouseButtons to specify which mouse buttons.</param>
    /// <returns>MouseInput.</returns>
    public static MouseInput MouseDown(MouseButtons mouseButtons) => MouseDown(mouseButtons, null, null);

    /// <summary>Create a MouseInput struct for a mouse button down at a specific location.</summary>
    /// <param name="mouseButtons">MouseButtons to specify which mouse buttons.</param>
    /// <param name="location">Where is the click located.</param>
    /// <returns>MouseInput.</returns>
    public static MouseInput MouseDown(MouseButtons mouseButtons, NativePoint? location) => MouseDown(mouseButtons, location, null);

    /// <summary>Create a MouseInput struct for a mouse button down.</summary>
    /// <param name="mouseButtons">MouseButtons to specify which mouse buttons.</param>
    /// <param name="location">Where is the click located.</param>
    /// <param name="timestamp">The time stamp for the event.</param>
    /// <returns>MouseInput.</returns>
    public static MouseInput MouseDown(MouseButtons mouseButtons, NativePoint? location, uint? timestamp)
    {
        location = RemapLocation(location);
        MouseEventFlags mouseEventFlags = (location.HasValue ? (MouseEventFlags.Move | MouseEventFlags.Virtualdesk | MouseEventFlags.Absolute) : MouseEventFlags.None);
        MouseButtonInputData buttonData = GetButtonDownData(mouseButtons);
        uint messageTime = timestamp ?? checked((uint)Environment.TickCount);
        return new(location?.X ?? 0, location?.Y ?? 0, buttonData.MouseData, mouseEventFlags | buttonData.Flags, messageTime);
    }

    /// <summary>Create a MouseInput struct for a mouse button up.</summary>
    /// <param name="mouseButtons">MouseButtons to specify which mouse buttons.</param>
    /// <returns>MouseInput.</returns>
    public static MouseInput MouseUp(MouseButtons mouseButtons) => MouseUp(mouseButtons, null, null);

    /// <summary>Create a MouseInput struct for a mouse button up at a specific location.</summary>
    /// <param name="mouseButtons">MouseButtons to specify which mouse buttons.</param>
    /// <param name="location">Where is the click located.</param>
    /// <returns>MouseInput.</returns>
    public static MouseInput MouseUp(MouseButtons mouseButtons, NativePoint? location) => MouseUp(mouseButtons, location, null);

    /// <summary>Create a MouseInput struct for a mouse button up.</summary>
    /// <param name="mouseButtons">MouseButtons to specify which mouse buttons.</param>
    /// <param name="location">Where is the click located.</param>
    /// <param name="timestamp">The time stamp for the event.</param>
    /// <returns>MouseInput.</returns>
    public static MouseInput MouseUp(MouseButtons mouseButtons, NativePoint? location, uint? timestamp)
    {
        location = RemapLocation(location);
        MouseEventFlags mouseEventFlags = (location.HasValue ? (MouseEventFlags.Move | MouseEventFlags.Virtualdesk | MouseEventFlags.Absolute) : MouseEventFlags.None);
        MouseButtonInputData buttonData = GetButtonUpData(mouseButtons);
        uint messageTime = timestamp ?? checked((uint)Environment.TickCount);
        return new(location?.X ?? 0, location?.Y ?? 0, buttonData.MouseData, mouseEventFlags | buttonData.Flags, messageTime);
    }

    /// <summary>The coordinates need to be mapped from 0-65535 where 0 is left and 65535 is right.</summary>
    /// <param name="location">The native screen coordinate.</param>
    /// <returns>The remapped absolute mouse coordinate.</returns>
    private static NativePoint RemapLocation(NativePoint location)
    {
        NativeRect bounds = DisplayTopology.ScreenBounds;
        checked
        {
            if (bounds.Width * bounds.Height != 0)
            {
                return new(location.X * unchecked(65_535 / bounds.Width), location.Y * unchecked(65_535 / bounds.Height));
            }

            return location;
        }
    }

    /// <summary>Maps nullable coordinates to absolute input coordinates when a location is present.</summary>
    /// <param name="location">The nullable location to map.</param>
    /// <returns>The mapped nullable location.</returns>
    private static NativePoint? RemapLocation(NativePoint? location)
    {
        if (!location.HasValue)
        {
            return null;
        }

        return RemapLocation(location.Value);
    }

    /// <summary>Gets the native mouse button data for a button-down event.</summary>
    /// <param name="mouseButtons">The mouse buttons to press.</param>
    /// <returns>The native button data.</returns>
    private static MouseButtonInputData GetButtonDownData(MouseButtons mouseButtons)
    {
        MouseEventFlags mouseEventFlags = MouseEventFlags.None;
        uint mouseData = 0U;
        AddButtonData(mouseButtons, MouseButtons.Left, MouseEventFlags.LeftDown, 0U, ref mouseEventFlags, ref mouseData);
        AddButtonData(mouseButtons, MouseButtons.Right, MouseEventFlags.RightDown, 0U, ref mouseEventFlags, ref mouseData);
        AddButtonData(mouseButtons, MouseButtons.Middle, MouseEventFlags.MiddleDown, 0U, ref mouseEventFlags, ref mouseData);
        AddButtonData(mouseButtons, MouseButtons.XButton1, MouseEventFlags.XDown, 1U, ref mouseEventFlags, ref mouseData);
        AddButtonData(mouseButtons, MouseButtons.XButton2, MouseEventFlags.XDown, 2U, ref mouseEventFlags, ref mouseData);
        return new(mouseEventFlags, mouseData);
    }

    /// <summary>Gets the native mouse button data for a button-up event.</summary>
    /// <param name="mouseButtons">The mouse buttons to release.</param>
    /// <returns>The native button data.</returns>
    private static MouseButtonInputData GetButtonUpData(MouseButtons mouseButtons)
    {
        MouseEventFlags mouseEventFlags = MouseEventFlags.None;
        uint mouseData = 0U;
        AddButtonData(mouseButtons, MouseButtons.Left, MouseEventFlags.LeftUp, 0U, ref mouseEventFlags, ref mouseData);
        AddButtonData(mouseButtons, MouseButtons.Right, MouseEventFlags.RightUp, 0U, ref mouseEventFlags, ref mouseData);
        AddButtonData(mouseButtons, MouseButtons.Middle, MouseEventFlags.MiddleUp, 0U, ref mouseEventFlags, ref mouseData);
        AddButtonData(mouseButtons, MouseButtons.XButton1, MouseEventFlags.XUp, 1U, ref mouseEventFlags, ref mouseData);
        AddButtonData(mouseButtons, MouseButtons.XButton2, MouseEventFlags.XUp, 2U, ref mouseEventFlags, ref mouseData);
        return new(mouseEventFlags, mouseData);
    }

    /// <summary>Adds the native data for a selected mouse button.</summary>
    /// <param name="mouseButtons">The selected mouse buttons.</param>
    /// <param name="button">The button to test.</param>
    /// <param name="flag">The native flag to add when selected.</param>
    /// <param name="data">The native mouse data to add when selected.</param>
    /// <param name="mouseEventFlags">The accumulated native flags.</param>
    /// <param name="mouseData">The accumulated native mouse data.</param>
    private static void AddButtonData(MouseButtons mouseButtons, MouseButtons button, MouseEventFlags flag, uint data, ref MouseEventFlags mouseEventFlags, ref uint mouseData)
    {
        if ((mouseButtons & button) != MouseButtons.None)
        {
            mouseEventFlags |= flag;
            mouseData |= data;
        }
    }
}
