// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Mouse;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Mouse;
#endif
/// <summary>This is a utility class to help to generate input for the mouse.</summary>
public static class MouseInputGenerator
{
    /// <summary>Generate mouse button(s) click.</summary>
    /// <param name="mouseButtons">MouseButtons specifying which buttons are pressed.</param>
    /// <returns>number of input events generated.</returns>
    public static uint MouseClick(MouseButtons mouseButtons) => MouseClick(mouseButtons, null, null);

    /// <summary>Generate mouse button(s) click at a specific location.</summary>
    /// <param name="mouseButtons">MouseButtons specifying which buttons are pressed.</param>
    /// <param name="location">NativePoint to specify where the mouse click takes place.</param>
    /// <returns>number of input events generated.</returns>
    public static uint MouseClick(MouseButtons mouseButtons, NativePoint? location) => MouseClick(mouseButtons, location, null);

    /// <summary>Generate mouse button(s) click at a specific location and timestamp.</summary>
    /// <param name="mouseButtons">MouseButtons specifying which buttons are pressed.</param>
    /// <param name="location">NativePoint to specify where the mouse click takes place.</param>
    /// <param name="timestamp">The time stamp for the event.</param>
    /// <returns>number of input events generated.</returns>
    public static uint MouseClick(MouseButtons mouseButtons, NativePoint? location, uint? timestamp) =>
        NativeInput.SendInput(DesktopInput.CreateMouseInputs(
            MouseInput.MouseDown(mouseButtons, location, timestamp),
            MouseInput.MouseUp(mouseButtons, location, timestamp)));

    /// <summary>Generate mouse button(s) down.</summary>
    /// <param name="mouseButtons">MouseButtons specifying which buttons are down.</param>
    /// <returns>number of input events generated.</returns>
    public static uint MouseDown(MouseButtons mouseButtons) => MouseDown(mouseButtons, null, null);

    /// <summary>Generate mouse button(s) down at a specific location.</summary>
    /// <param name="mouseButtons">MouseButtons specifying which buttons are down.</param>
    /// <param name="location">NativePoint to specify where the mouse down takes place.</param>
    /// <returns>number of input events generated.</returns>
    public static uint MouseDown(MouseButtons mouseButtons, NativePoint? location) => MouseDown(mouseButtons, location, null);

    /// <summary>Generate mouse button(s) down at a specific location and timestamp.</summary>
    /// <param name="mouseButtons">MouseButtons specifying which buttons are down.</param>
    /// <param name="location">NativePoint to specify where the mouse down takes place.</param>
    /// <param name="timestamp">The time stamp for the event.</param>
    /// <returns>number of input events generated.</returns>
    public static uint MouseDown(MouseButtons mouseButtons, NativePoint? location, uint? timestamp)
    {
        MouseInput mouseInput = MouseInput.MouseDown(mouseButtons, location, timestamp);
        return NativeInput.SendInput(DesktopInput.CreateMouseInputs(mouseInput));
    }

    /// <summary>Generate mouse button(s) Up.</summary>
    /// <param name="mouseButtons">MouseButtons specifying which buttons are up.</param>
    /// <returns>number of input events generated.</returns>
    public static uint MouseUp(MouseButtons mouseButtons) => MouseUp(mouseButtons, null, null);

    /// <summary>Generate mouse button(s) up at a specific location.</summary>
    /// <param name="mouseButtons">MouseButtons specifying which buttons are up.</param>
    /// <param name="location">NativePoint to specify where the mouse up takes place.</param>
    /// <returns>number of input events generated.</returns>
    public static uint MouseUp(MouseButtons mouseButtons, NativePoint? location) => MouseUp(mouseButtons, location, null);

    /// <summary>Generate mouse button(s) up at a specific location and timestamp.</summary>
    /// <param name="mouseButtons">MouseButtons specifying which buttons are up.</param>
    /// <param name="location">NativePoint to specify where the mouse up takes place.</param>
    /// <param name="timestamp">The time stamp for the event.</param>
    /// <returns>number of input events generated.</returns>
    public static uint MouseUp(MouseButtons mouseButtons, NativePoint? location, uint? timestamp)
    {
        MouseInput mouseInput = MouseInput.MouseUp(mouseButtons, location, timestamp);
        return NativeInput.SendInput(DesktopInput.CreateMouseInputs(mouseInput));
    }

    /// <summary>Generate mouse moves.</summary>
    /// <param name="location">NativePoint to specify where the mouse moves.</param>
    /// <returns>number of input events generated.</returns>
    public static uint MoveMouse(NativePoint location) => MoveMouse(location, null);

    /// <summary>Generate mouse moves at a specific timestamp.</summary>
    /// <param name="location">NativePoint to specify where the mouse moves.</param>
    /// <param name="timestamp">The time stamp for the event.</param>
    /// <returns>number of input events generated.</returns>
    public static uint MoveMouse(NativePoint location, uint? timestamp)
    {
        MouseInput mouseInput = MouseInput.MouseMove(location, timestamp);
        return NativeInput.SendInput(DesktopInput.CreateMouseInputs(mouseInput));
    }

    /// <summary>Generate mouse wheel moves.</summary>
    /// <param name="wheelDelta">The mouse wheel delta.</param>
    /// <returns>number of input events generated.</returns>
    public static uint MoveMouseWheel(int wheelDelta) => MoveMouseWheel(wheelDelta, null, null);

    /// <summary>Generate mouse wheel moves at a specific location.</summary>
    /// <param name="wheelDelta">The mouse wheel delta.</param>
    /// <param name="location">NativePoint to specify where the mouse wheel takes place.</param>
    /// <returns>number of input events generated.</returns>
    public static uint MoveMouseWheel(int wheelDelta, NativePoint? location) => MoveMouseWheel(wheelDelta, location, null);

    /// <summary>Generate mouse wheel moves at a specific location and timestamp.</summary>
    /// <param name="wheelDelta">The mouse wheel delta.</param>
    /// <param name="location">NativePoint to specify where the mouse wheel takes place.</param>
    /// <param name="timestamp">The time stamp for the event.</param>
    /// <returns>number of input events generated.</returns>
    public static uint MoveMouseWheel(int wheelDelta, NativePoint? location, uint? timestamp)
    {
        MouseInput mouseInput = MouseInput.MoveMouseWheel(wheelDelta, location, timestamp);
        return NativeInput.SendInput(DesktopInput.CreateMouseInputs(mouseInput));
    }
}
