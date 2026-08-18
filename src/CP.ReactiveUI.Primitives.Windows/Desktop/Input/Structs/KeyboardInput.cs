// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     This struct contains information about a simulated keyboard event.
///     See
///     <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646271.aspx">KEYBDINPUT structure</a>
/// </summary>
public readonly record struct KeyboardInput
{
    /// <summary>Stores the native virtual-key code.</summary>
    private readonly ushort _virtualKeyCode;

    /// <summary>Stores the native hardware scan code.</summary>
    private readonly ushort _scanCode;

    /// <summary>Stores the native key event flags.</summary>
    private readonly KeyEventFlags _keyEventFlags;

    /// <summary>Stores the event timestamp.</summary>
    private readonly uint _timestamp;

    /// <summary>Stores native extra information associated with the keystroke.</summary>
    private readonly UIntPtr _extraInfo;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs.KeyboardInput" /> struct.</summary>
    /// <param name="virtualKeyCode">The virtual key code.</param>
    /// <param name="scanCode">The hardware scan code.</param>
    /// <param name="keyEventFlags">The key event flags.</param>
    /// <param name="timestamp">The input timestamp.</param>
    private KeyboardInput(VirtualKeyCode virtualKeyCode, ushort scanCode, KeyEventFlags keyEventFlags, uint timestamp)
    {
        _virtualKeyCode = checked((ushort)virtualKeyCode);
        _scanCode = scanCode;
        _keyEventFlags = keyEventFlags;
        _timestamp = timestamp;
        _extraInfo = UIntPtr.Zero;
    }

    /// <summary>
    ///     Gets a virtual-key code. The code must be a value in the range 1 to 254.
    ///     If the flags member specifies KEYEVENTF_UNICODE, wVk must be 0.
    /// </summary>
    public VirtualKeyCode VirtualKeyCode => (VirtualKeyCode)_virtualKeyCode;

    /// <summary>
    ///     Gets a hardware scan code for the key. If KeyEventFlags specifies Unicode, ScanCode specifies a Unicode character which
    ///     is to be sent to the foreground application.
    /// </summary>
    public ushort ScanCode => _scanCode;

    /// <summary>Gets various aspects of a keystroke. This member can be certain combinations of the following values.</summary>
    public KeyEventFlags KeyEventFlags => _keyEventFlags;

    /// <summary>Gets the time stamp for the event, in milliseconds. If this parameter is zero, the system will provide its own time stamp.</summary>
    public uint Timestamp => _timestamp;

    /// <summary>Gets native extra information associated with the keystroke.</summary>
    internal UIntPtr ExtraInfo => _extraInfo;

    /// <summary>Create a KeyboardInput for a key press (up / down).</summary>
    /// <param name="virtualKeyCode">Value from VirtualKeyCodes.</param>
    /// <returns>KeyboardInput[].</returns>
    public static KeyboardInput[] ForKeyPress(VirtualKeyCode virtualKeyCode) => ForKeyPress(virtualKeyCode, null);

    /// <summary>Create a KeyboardInput for a key press (up / down).</summary>
    /// <param name="virtualKeyCode">Value from VirtualKeyCodes.</param>
    /// <param name="timestamp">The optional timestamp.</param>
    /// <returns>KeyboardInput[].</returns>
    public static KeyboardInput[] ForKeyPress(VirtualKeyCode virtualKeyCode, uint? timestamp) =>
        [
            ForKeyDown(virtualKeyCode, timestamp),
            ForKeyUp(virtualKeyCode, timestamp),
        ];

    /// <summary>Create a KeyboardInput for a key down.</summary>
    /// <param name="virtualKeyCode">Value from VirtualKeyCodes.</param>
    /// <returns>KeyboardInput.</returns>
    public static KeyboardInput ForKeyDown(VirtualKeyCode virtualKeyCode) => ForKeyDown(virtualKeyCode, null);

    /// <summary>Create a KeyboardInput for a key down.</summary>
    /// <param name="virtualKeyCode">Value from VirtualKeyCodes.</param>
    /// <param name="timestamp">The optional timestamp.</param>
    /// <returns>KeyboardInput.</returns>
    public static KeyboardInput ForKeyDown(VirtualKeyCode virtualKeyCode, uint? timestamp)
    {
        var messageTime = timestamp ?? unchecked((uint)Environment.TickCount);
        return new(virtualKeyCode, 0, KeyEventFlags.None, messageTime);
    }

    /// <summary>Create a KeyboardInput for a key up.</summary>
    /// <param name="virtualKeyCode">Value from VirtualKeyCodes.</param>
    /// <returns>KeyboardInput.</returns>
    public static KeyboardInput ForKeyUp(VirtualKeyCode virtualKeyCode) => ForKeyUp(virtualKeyCode, null);

    /// <summary>Create a KeyboardInput for a key up.</summary>
    /// <param name="virtualKeyCode">Value from VirtualKeyCodes.</param>
    /// <param name="timestamp">The optional timestamp.</param>
    /// <returns>KeyboardInput.</returns>
    public static KeyboardInput ForKeyUp(VirtualKeyCode virtualKeyCode, uint? timestamp)
    {
        var messageTime = timestamp ?? unchecked((uint)Environment.TickCount);
        return new(virtualKeyCode, 0, KeyEventFlags.KeyUp, messageTime);
    }
}
