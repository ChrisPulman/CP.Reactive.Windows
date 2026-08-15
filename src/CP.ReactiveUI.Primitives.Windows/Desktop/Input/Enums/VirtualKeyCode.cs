// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>
///     Symbolic constant names, hexadecimal values, and mouse or keyboard equivalents for the virtual-key codes used by
///     the system.
///     The codes are listed in numeric order.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/dd375731.aspx">Virtual-Key Codes</a>
/// </summary>
public enum VirtualKeyCode
{
    /// <summary>Not a key.</summary>
    None = 0,
    /// <summary>Left mouse button.</summary>
    Lbutton = 1,
    /// <summary>Right mouse button.</summary>
    Rbutton = 2,
    /// <summary>Control-break processing.</summary>
    Cancel = 3,
    /// <summary>Middle mouse button (three-button mouse).</summary>
    Mbutton = 4,
    /// <summary>Windows 2000/XP: X1 mouse button.</summary>
    Xbutton1 = 5,
    /// <summary>Windows 2000/XP: X2 mouse button.</summary>
    Xbutton2 = 6,
    /// <summary>BACKSPACE key.</summary>
    Back = 8,
    /// <summary>TAB key.</summary>
    Tab = 9,
    /// <summary>CLEAR key.</summary>
    Clear = 12,
    /// <summary>ENTER key.</summary>
    Return = 13,
    /// <summary>SHIFT key.</summary>
    Shift = 16,
    /// <summary>CTRL key.</summary>
    Control = 17,
    /// <summary>ALT key.</summary>
    Menu = 18,
    /// <summary>PAUSE key.</summary>
    Pause = 19,
    /// <summary>CAPS LOCK key.</summary>
    Capital = 20,
    /// <summary>Input Method Editor (IME) Kana mode.</summary>
    Kana = 21,
    /// <summary>IME Junja mode.</summary>
    Junja = 23,
    /// <summary>IME final mode.</summary>
    Final = 24,
    /// <summary>IME Hanja mode.</summary>
    Hanja = 25,
    /// <summary>ESC key.</summary>
    Escape = 27,
    /// <summary>IME convert.</summary>
    Convert = 28,
    /// <summary>IME nonconvert.</summary>
    Nonconvert = 29,
    /// <summary>IME accept key.</summary>
    Accept = 30,
    /// <summary>IME mode change request.</summary>
    Modechange = 31,
    /// <summary>SPACEBAR key.</summary>
    Space = 32,
    /// <summary>PAGE UP key.</summary>
    Prior = 33,
    /// <summary>PAGE DOWN key.</summary>
    Next = 34,
    /// <summary>END key.</summary>
    End = 35,
    /// <summary>HOME key.</summary>
    Home = 36,
    /// <summary>LEFT ARROW key.</summary>
    Left = 37,
    /// <summary>UP ARROW key.</summary>
    Up = 38,
    /// <summary>RIGHT ARROW key.</summary>
    Right = 39,
    /// <summary>DOWN ARROW key.</summary>
    Down = 40,
    /// <summary>SELECT key.</summary>
    Select = 41,
    /// <summary>PRINT key.</summary>
    Print = 42,
    /// <summary>EXECUTE key.</summary>
    Execute = 43,
    /// <summary>This is the PrintScreen key, which is also called Snapshot.</summary>
    PrintScreen = 44,
    /// <summary>INS key.</summary>
    Insert = 45,
    /// <summary>DEL key.</summary>
    Delete = 46,
    /// <summary>HELP key.</summary>
    Help = 47,
    /// <summary>0 key.</summary>
    Key0 = 48,
    /// <summary>1 key.</summary>
    Key1 = 49,
    /// <summary>2 key.</summary>
    Key2 = 50,
    /// <summary>3 key.</summary>
    Key3 = 51,
    /// <summary>4 key.</summary>
    Key4 = 52,
    /// <summary>5 key.</summary>
    Key5 = 53,
    /// <summary>6 key.</summary>
    Key6 = 54,
    /// <summary>7 key.</summary>
    Key7 = 55,
    /// <summary>8 key.</summary>
    Key8 = 56,
    /// <summary>9 key.</summary>
    Key9 = 57,
    /// <summary>A key.</summary>
    KeyA = 65,
    /// <summary>B key.</summary>
    KeyB = 66,
    /// <summary>C key.</summary>
    KeyC = 67,
    /// <summary>D key.</summary>
    KeyD = 68,
    /// <summary>E key.</summary>
    KeyE = 69,
    /// <summary>F key.</summary>
    KeyF = 70,
    /// <summary>G key.</summary>
    KeyG = 71,
    /// <summary>H key.</summary>
    KeyH = 72,
    /// <summary>I key.</summary>
    KeyI = 73,
    /// <summary>J key.</summary>
    KeyJ = 74,
    /// <summary>K key.</summary>
    KeyK = 75,
    /// <summary>L key.</summary>
    KeyL = 76,
    /// <summary>M key.</summary>
    KeyM = 77,
    /// <summary>N key.</summary>
    KeyN = 78,
    /// <summary>O key.</summary>
    KeyO = 79,
    /// <summary>P key.</summary>
    KeyP = 80,
    /// <summary>Q key.</summary>
    KeyQ = 81,
    /// <summary>R key.</summary>
    KeyR = 82,
    /// <summary>S key.</summary>
    KeyS = 83,
    /// <summary>T key.</summary>
    KeyT = 84,
    /// <summary>U key.</summary>
    KeyU = 85,
    /// <summary>V key.</summary>
    KeyV = 86,
    /// <summary>W key.</summary>
    KeyW = 87,
    /// <summary>X key.</summary>
    KeyX = 88,
    /// <summary>Y key.</summary>
    KeyY = 89,
    /// <summary>Z key.</summary>
    KeyZ = 90,
    /// <summary>Left Windows key (Microsoft Natural keyboard).</summary>
    LeftWin = 91,
    /// <summary>Right Windows key (Natural keyboard).</summary>
    RightWin = 92,
    /// <summary>Applications key (Natural keyboard).</summary>
    Apps = 93,
    /// <summary>Computer Sleep key.</summary>
    Sleep = 95,
    /// <summary>Numeric keypad 0 key.</summary>
    Numpad0 = 96,
    /// <summary>Numeric keypad 1 key.</summary>
    Numpad1 = 97,
    /// <summary>Numeric keypad 2 key.</summary>
    Numpad2 = 98,
    /// <summary>Numeric keypad 3 key.</summary>
    Numpad3 = 99,
    /// <summary>Numeric keypad 4 key.</summary>
    Numpad4 = 100,
    /// <summary>Numeric keypad 5 key.</summary>
    Numpad5 = 101,
    /// <summary>Numeric keypad 6 key.</summary>
    Numpad6 = 102,
    /// <summary>Numeric keypad 7 key.</summary>
    Numpad7 = 103,
    /// <summary>Numeric keypad 8 key.</summary>
    Numpad8 = 104,
    /// <summary>Numeric keypad 9 key.</summary>
    Numpad9 = 105,
    /// <summary>Multiply key.</summary>
    Multiply = 106,
    /// <summary>Add key.</summary>
    Add = 107,
    /// <summary>Separator key.</summary>
    Separator = 108,
    /// <summary>Subtract key.</summary>
    Subtract = 109,
    /// <summary>Decimal key.</summary>
    Decimal = 110,
    /// <summary>Divide key.</summary>
    Divide = 111,
    /// <summary>F1 key.</summary>
    F1 = 112,
    /// <summary>F2 key.</summary>
    F2 = 113,
    /// <summary>F3 key.</summary>
    F3 = 114,
    /// <summary>F4 key.</summary>
    F4 = 115,
    /// <summary>F5 key.</summary>
    F5 = 116,
    /// <summary>F6 key.</summary>
    F6 = 117,
    /// <summary>F7 key.</summary>
    F7 = 118,
    /// <summary>F8 key.</summary>
    F8 = 119,
    /// <summary>F9 key.</summary>
    F9 = 120,
    /// <summary>F10 key.</summary>
    F10 = 121,
    /// <summary>F11 key.</summary>
    F11 = 122,
    /// <summary>F12 key.</summary>
    F12 = 123,
    /// <summary>F13 key.</summary>
    F13 = 124,
    /// <summary>F14 key.</summary>
    F14 = 125,
    /// <summary>F15 key.</summary>
    F15 = 126,
    /// <summary>F16 key.</summary>
    F16 = 127,
    /// <summary>F17 key.</summary>
    F17 = 128,
    /// <summary>F18 key.</summary>
    F18 = 129,
    /// <summary>F19 key.</summary>
    F19 = 130,
    /// <summary>F20 key.</summary>
    F20 = 131,
    /// <summary>F21 key.</summary>
    F21 = 132,
    /// <summary>F22 key, (PPC only) Key used to lock device.</summary>
    F22 = 133,
    /// <summary>F23 key.</summary>
    F23 = 134,
    /// <summary>F24 key.</summary>
    F24 = 135,
    /// <summary>NUM LOCK key.</summary>
    NumLock = 144,
    /// <summary>SCROLL LOCK key.</summary>
    Scroll = 145,
    /// <summary>Left SHIFT key.</summary>
    LeftShift = 160,
    /// <summary>Right SHIFT key.</summary>
    RightShift = 161,
    /// <summary>Left CONTROL key.</summary>
    LeftControl = 162,
    /// <summary>Right CONTROL key.</summary>
    RightControl = 163,
    /// <summary>Left MENU key.</summary>
    LeftMenu = 164,
    /// <summary>Right MENU key.</summary>
    RightMenu = 165,
    /// <summary>Windows 2000/XP: Browser Back key.</summary>
    BrowserBack = 166,
    /// <summary>Windows 2000/XP: Browser Forward key.</summary>
    BrowserForward = 167,
    /// <summary>Windows 2000/XP: Browser Refresh key.</summary>
    BrowserRefresh = 168,
    /// <summary>Windows 2000/XP: Browser Stop key.</summary>
    BrowserStop = 169,
    /// <summary>Windows 2000/XP: Browser Search key.</summary>
    BrowserSearch = 170,
    /// <summary>Windows 2000/XP: Browser Favorites key.</summary>
    BrowserFavorites = 171,
    /// <summary>Windows 2000/XP: Browser Start and Home key.</summary>
    BrowserHome = 172,
    /// <summary>Windows 2000/XP: Volume Mute key.</summary>
    VolumeMute = 173,
    /// <summary>Windows 2000/XP: Volume Down key.</summary>
    VolumeDown = 174,
    /// <summary>Windows 2000/XP: Volume Up key.</summary>
    VolumeUp = 175,
    /// <summary>Windows 2000/XP: Next Track key.</summary>
    MediaNextTrack = 176,
    /// <summary>Windows 2000/XP: Previous Track key.</summary>
    MediaPrevTrack = 177,
    /// <summary>Windows 2000/XP: Stop Media key.</summary>
    MediaStop = 178,
    /// <summary>Windows 2000/XP: Play/Pause Media key.</summary>
    MediaPlayPause = 179,
    /// <summary>Windows 2000/XP: Start Mail key.</summary>
    LaunchMail = 180,
    /// <summary>Windows 2000/XP: Select Media key.</summary>
    LaunchMediaSelect = 181,
    /// <summary>Windows 2000/XP: Start Application 1 key.</summary>
    LaunchApp1 = 182,
    /// <summary>Windows 2000/XP: Start Application 2 key.</summary>
    LaunchApp2 = 183,
    /// <summary>Used for miscellaneous characters; it can vary by keyboard.</summary>
    Oem1 = 186,
    /// <summary>Windows 2000/XP: For any country/region, the '+' key.</summary>
    OemPlus = 187,
    /// <summary>Windows 2000/XP: For any country/region, the ',' key.</summary>
    OemComma = 188,
    /// <summary>Windows 2000/XP: For any country/region, the '-' key.</summary>
    OemMinus = 189,
    /// <summary>Windows 2000/XP: For any country/region, the '.' key.</summary>
    OemPeriod = 190,
    /// <summary>Used for miscellaneous characters; it can vary by keyboard.</summary>
    Oem2 = 191,
    /// <summary>Used for miscellaneous characters; it can vary by keyboard.</summary>
    Oem3 = 192,
    /// <summary>Used for miscellaneous characters; it can vary by keyboard.</summary>
    Oem4 = 219,
    /// <summary>Used for miscellaneous characters; it can vary by keyboard.</summary>
    Oem5 = 220,
    /// <summary>Used for miscellaneous characters; it can vary by keyboard.</summary>
    Oem6 = 221,
    /// <summary>Used for miscellaneous characters; it can vary by keyboard.</summary>
    Oem7 = 222,
    /// <summary>Used for miscellaneous characters; it can vary by keyboard.</summary>
    Oem8 = 223,
    /// <summary>Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard.</summary>
    Oem102 = 226,
    /// <summary>Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key.</summary>
    Processkey = 229,
    /// <summary>
    ///     Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes.
    ///     The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more
    ///     information,
    ///     see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP.
    /// </summary>
    Packet = 231,
    /// <summary>Attn key.</summary>
    Attn = 246,
    /// <summary>CrSel key.</summary>
    Crsel = 247,
    /// <summary>ExSel key.</summary>
    Exsel = 248,
    /// <summary>Erase EOF key.</summary>
    Ereof = 249,
    /// <summary>Play key.</summary>
    Play = 250,
    /// <summary>Zoom key.</summary>
    Zoom = 251,
    /// <summary>Reserved virtual key.</summary>
    Noname = 252,
    /// <summary>PA1 key.</summary>
    Pa1 = 253,
    /// <summary>Clear key.</summary>
    OemClear = 254,
}
