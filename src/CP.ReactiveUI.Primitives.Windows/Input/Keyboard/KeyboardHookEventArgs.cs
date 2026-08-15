// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Text;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Keyboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;
#endif
/// <summary>Information on keyboard changes.</summary>
public class KeyboardHookEventArgs : EventArgs
{
    /// <summary>Gets or sets set this to true if the event is handled, other event-handlers in the chain will not be called.</summary>
    public bool Handled { get; set; }

    /// <summary>Gets a value indicating whether this event is for a modifier key.</summary>
    public bool IsModifier { get; internal set; }

    /// <summary>Gets true if Alt key is pressed.</summary>
    public bool IsAlt
    {
        get
        {
            if (!IsLeftAlt)
            {
                return IsRightAlt;
            }

            return true;
        }
    }

    /// <summary>Gets a value indicating whether caps lock is active.</summary>
    public bool IsCapsLockActive { get; internal set; }

    /// <summary>Gets true if control is pressed.</summary>
    public bool IsControl
    {
        get
        {
            if (!IsLeftControl)
            {
                return IsRightControl;
            }

            return true;
        }
    }

    /// <summary>Gets a value indicating whether this is a key-down event.</summary>
    public bool IsKeyDown { get; internal set; }

    /// <summary>Gets a value indicating whether the left alt key is pressed.</summary>
    public bool IsLeftAlt { get; internal set; }

    /// <summary>Gets a value indicating whether the left control key is pressed.</summary>
    public bool IsLeftControl { get; internal set; }

    /// <summary>Gets a value indicating whether the left shift key is pressed.</summary>
    public bool IsLeftShift { get; internal set; }

    /// <summary>Gets a value indicating whether the left Windows key is pressed.</summary>
    public bool IsLeftWindows { get; internal set; }

    /// <summary>Gets a value indicating whether num lock is active.</summary>
    public bool IsNumLockActive { get; internal set; }

    /// <summary>Gets a value indicating whether the right alt key is pressed.</summary>
    public bool IsRightAlt { get; internal set; }

    /// <summary>Gets a value indicating whether the right control key is pressed.</summary>
    public bool IsRightControl { get; internal set; }

    /// <summary>Gets a value indicating whether the right shift key is pressed.</summary>
    public bool IsRightShift { get; internal set; }

    /// <summary>Gets a value indicating whether the right Windows key is pressed.</summary>
    public bool IsRightWindows { get; internal set; }

    /// <summary>Gets a value indicating whether scroll lock is active.</summary>
    public bool IsScrollLockActive { get; internal set; }

    /// <summary>Gets true if shift is pressed.</summary>
    public bool IsShift
    {
        get
        {
            if (!IsLeftShift)
            {
                return IsRightShift;
            }

            return true;
        }
    }

    /// <summary>Gets a value indicating whether this is a system key.</summary>
    public bool IsSystemKey { get; internal set; }

    /// <summary>Gets true if shift is pressed.</summary>
    public bool IsWindows
    {
        get
        {
            if (!IsLeftWindows)
            {
                return IsRightWindows;
            }

            return true;
        }
    }

    /// <summary>Gets the key code itself.</summary>
    public VirtualKeyCode Key { get; internal set; }

    /// <summary>Gets the timestamp of the event.</summary>
    public uint TimeStamp { get; internal set; }

    /// <summary>Gets the DateTimeOffset for this event.</summary>
    public DateTimeOffset EventTime => GetEventTime(TimeProvider.System);

    /// <summary>Gets details about the keyboard event.</summary>
    public ExtendedKeyFlags Flags { get; internal set; }

    /// <summary>Gets a value indicating whether another process injected this event.</summary>
    public bool IsInjectedByProcess => (Flags & ExtendedKeyFlags.Injected) != 0;

    /// <summary>Gets a value indicating whether a lower-integrity process injected this event.</summary>
    public bool IsInjectedByLowerIntegrityLevelProcess
    {
        get
        {
            if ((Flags & ExtendedKeyFlags.Injected) != ExtendedKeyFlags.None)
            {
                return (Flags & ExtendedKeyFlags.LowerIntegretyInjected) != 0;
            }

            return false;
        }
    }

    /// <summary>Generate KeyboardHookEventArgs for a key down.</summary>
    /// <param name="virtualKeyCode">VirtualKeyCode.</param>
    /// <returns>KeyboardHookEventArgs.</returns>
    public static KeyboardHookEventArgs KeyDown(VirtualKeyCode virtualKeyCode) => new KeyboardHookEventArgs { Key = virtualKeyCode, IsKeyDown = true, IsModifier = virtualKeyCode.IsModifier() };

    /// <summary>Generate KeyboardHookEventArgs for a key up.</summary>
    /// <param name="virtualKeyCode">VirtualKeyCode.</param>
    /// <returns>KeyboardHookEventArgs.</returns>
    public static KeyboardHookEventArgs KeyUp(VirtualKeyCode virtualKeyCode) => new KeyboardHookEventArgs { Key = virtualKeyCode, IsKeyDown = false, IsModifier = virtualKeyCode.IsModifier() };

    /// <summary>Gets the event time using the supplied time provider.</summary>
    /// <param name="timeProvider">The time provider.</param>
    /// <returns>The calculated event time.</returns>
    public DateTimeOffset GetEventTime(TimeProvider timeProvider)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(timeProvider);
        TimeSpan runningTimeSpan = TimeSpan.FromMilliseconds(checked(Environment.TickCount - TimeStamp));
        return timeProvider.GetLocalNow().Subtract(runningTimeSpan);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder dump = new();
        if (!IsModifier)
        {
            AppendPressedModifiers(dump);
        }

        _ = dump.Append(Key).Append(IsKeyDown ? " down" : " up");
        _ = dump.Append(Handled ? " (" : " (not ").Append("handled)");
        AppendLockState(dump);
        return dump.ToString();
    }

    /// <summary>Appends a side-aware modifier label when a modifier is pressed.</summary>
    /// <param name="dump">The destination string builder.</param>
    /// <param name="isPressed">A value indicating whether either side of the modifier is pressed.</param>
    /// <param name="isLeft">A value indicating whether the left-side modifier is pressed.</param>
    /// <param name="isRight">A value indicating whether the right-side modifier is pressed.</param>
    /// <param name="label">The modifier label to append.</param>
    private static void AppendSideAwareModifier(StringBuilder dump, bool isPressed, bool isLeft, bool isRight, string label)
    {
        if (isPressed)
        {
            AppendSimpleModifier(dump, isLeft, "left ");
            AppendSimpleModifier(dump, isRight, "right ");
            _ = dump.Append(label);
        }
    }

    /// <summary>Appends text when the supplied condition is true.</summary>
    /// <param name="dump">The destination string builder.</param>
    /// <param name="shouldAppend">A value indicating whether the text should be appended.</param>
    /// <param name="text">The text to append.</param>
    private static void AppendSimpleModifier(StringBuilder dump, bool shouldAppend, string text)
    {
        if (shouldAppend)
        {
            _ = dump.Append(text);
        }
    }

    /// <summary>Appends the pressed modifier keys to the supplied string builder.</summary>
    /// <param name="dump">The destination string builder.</param>
    private void AppendPressedModifiers(StringBuilder dump)
    {
        AppendSideAwareModifier(dump, IsShift, IsLeftShift, IsRightShift, "shift +");
        AppendSideAwareModifier(dump, IsControl, IsLeftControl, IsRightControl, "control +");
        AppendSimpleModifier(dump, IsLeftAlt, " with left-alt");
        AppendSimpleModifier(dump, IsRightAlt, " with right-alt");
        AppendSimpleModifier(dump, IsLeftWindows, " with left-windows");
        AppendSimpleModifier(dump, IsRightWindows, " with right-windows");
    }

    /// <summary>Appends active lock-key state to the supplied string builder.</summary>
    /// <param name="dump">The destination string builder.</param>
    private void AppendLockState(StringBuilder dump)
    {
        AppendSimpleModifier(dump, IsScrollLockActive, " ScrollLocked");
        AppendSimpleModifier(dump, IsNumLockActive, " NumLock active");
        AppendSimpleModifier(dump, IsCapsLockActive, " CapsLock active");
    }
}
