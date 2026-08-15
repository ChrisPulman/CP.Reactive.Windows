// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Keyboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;
#endif
/// <summary>Some code to help with hotkeys.</summary>
public static class KeyHelper
{
    /// <summary>Native keyboard display-name methods.</summary>
    private static class NativeMethods
    {
        /// <summary>Maps a virtual key code to a scan code.</summary>
        /// <param name="code">The virtual key code.</param>
        /// <param name="mapType">The mapping type.</param>
        /// <param name="keyboardLayout">The keyboard layout handle.</param>
        /// <returns>The mapped scan code.</returns>
        [DllImport("user32.dll", EntryPoint = "MapVirtualKeyExW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern uint MapVirtualKeyEx(uint code, uint mapType, IntPtr keyboardLayout);

        /// <summary>Gets the active input locale identifier for the supplied thread.</summary>
        /// <param name="threadId">The thread id, or 0 for the current thread.</param>
        /// <returns>The keyboard layout handle.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetKeyboardLayout(uint threadId);

        /// <summary>Gets the localized name of a key from its scan code.</summary>
        /// <param name="longParameter">The scan code and modifier parameter.</param>
        /// <param name="text">The destination text buffer.</param>
        /// <param name="size">The destination buffer size.</param>
        /// <returns>The number of copied characters.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetKeyNameTextW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern unsafe int GetKeyNameText(uint longParameter, char* text, int size);
    }

    /// <summary>Production adapter for keyboard display-name methods.</summary>
    private sealed class WindowsNativeKeyboardDisplayApi : INativeKeyboardDisplayApi
    {
        /// <summary>The shared production adapter.</summary>
        internal static readonly WindowsNativeKeyboardDisplayApi Instance = new();

        /// <inheritdoc />
        public IntPtr GetKeyboardLayout(uint threadId) => NativeMethods.GetKeyboardLayout(threadId);

        /// <inheritdoc />
        public uint MapVirtualKeyEx(uint code, uint mapType, IntPtr keyboardLayout) => NativeMethods.MapVirtualKeyEx(code, mapType, keyboardLayout);

        /// <inheritdoc />
        public unsafe int GetKeyNameText(uint longParameter, Span<char> text)
        {
            fixed (char* buffer = text)
            {
                return NativeMethods.GetKeyNameText(longParameter, buffer, text.Length);
            }
        }
    }

    /// <summary>The GetKeyNameText flag that ignores left and right key variants.</summary>
    private const uint DoNotCareLeftRight = 33_554_432U;

    /// <summary>The GetKeyNameText flag that marks an extended key.</summary>
    private const uint Extended = 16_777_216U;

    /// <summary>The maximum display-name buffer size passed to GetKeyNameText.</summary>
    private const int KeyNameCapacity = 100;

    /// <summary>The scan code shift used by GetKeyNameText.</summary>
    private const int KeyNameShift = 16;

    /// <summary>The scan code for the numeric keypad multiply key.</summary>
    private const uint NumpadScanCode = 55U;

    /// <summary>The scan code used by Windows for Print Screen.</summary>
    private const uint PrintScreenScanCode = 311U;

    /// <summary>The scan code used by Windows for Pause.</summary>
    private const uint PauseScanCode = 69U;

    /// <summary>The keys whose mapped scan codes require the extended-key flag.</summary>
    private static readonly VirtualKeyCode[] ExtendedKeys = new VirtualKeyCode[11]
    {
        VirtualKeyCode.Left,
        VirtualKeyCode.Up,
        VirtualKeyCode.Right,
        VirtualKeyCode.Down,
        VirtualKeyCode.Prior,
        VirtualKeyCode.Next,
        VirtualKeyCode.End,
        VirtualKeyCode.Home,
        VirtualKeyCode.Insert,
        VirtualKeyCode.Delete,
        VirtualKeyCode.NumLock
    };

    /// <summary>The keyboard display-name API override used by deterministic tests.</summary>
    private static INativeKeyboardDisplayApi _displayApiOverride;

    /// <summary>Get the name of a key, in the keyboard locale.</summary>
    /// <param name="givenKey">VirtualKeyCode.</param>
    /// <returns>string.</returns>
    public static string VirtualCodeToLocaleDisplayText(VirtualKeyCode givenKey) => VirtualCodeToLocaleDisplayText(givenKey, doNotCare: true);

    /// <summary>Get the name of a key, in the keyboard locale.</summary>
    /// <param name="givenKey">VirtualKeyCode.</param>
    /// <param name="doNotCare">bool, default true.</param>
    /// <returns>string.</returns>
    public static string VirtualCodeToLocaleDisplayText(VirtualKeyCode givenKey, bool doNotCare)
    {
        Span<char> keyName = stackalloc char[100];
        uint scanCodeModifier = (doNotCare ? 33_554_432U : 0U);
        VirtualKeyCode virtualKey = NormalizeVirtualKey(givenKey, doNotCare);
        INativeKeyboardDisplayApi displayApi = GetDisplayApi();
        if (TryGetNumpadOperatorDisplayText(virtualKey, keyName, displayApi, out var numpadDisplayText))
        {
            return numpadDisplayText;
        }

        IntPtr keyboardLayout = displayApi.GetKeyboardLayout(0U);
        uint scanCode = GetDisplayTextScanCode(virtualKey, keyboardLayout, displayApi);
        if (scanCode != 0)
        {
            return GetDisplayText(scanCode | scanCodeModifier, keyName, givenKey, displayApi);
        }

        return givenKey.ToString();
    }

    /// <summary>Get the VirtualKeyCodes from a key combination description.</summary>
    /// <param name="keyDescription">string with the key combination.</param>
    /// <returns>IEnumerable with VirtualKeyCodes.</returns>
    public static IEnumerable<VirtualKeyCode> VirtualKeyCodesFromString(string keyDescription) => GetVirtualKeyCodesFromString(keyDescription);

    /// <summary>Get a VirtualKeyCodes from a string.</summary>
    /// <param name="keyDescription">string with the key.</param>
    /// <returns>VirtualKeyCodes.</returns>
    public static VirtualKeyCode VirtualKeyCodeFromString(string keyDescription)
    {
        if (string.IsNullOrEmpty(keyDescription))
        {
            return VirtualKeyCode.None;
        }

        if (keyDescription.Length == 1)
        {
            keyDescription = $"KEY{keyDescription}";
        }

        if (Enum.TryParse<VirtualKeyCode>(keyDescription, ignoreCase: true, out var result))
        {
            return result;
        }

        keyDescription = keyDescription.ToLowerInvariant();
        return keyDescription switch
        {
            "alt" => VirtualKeyCode.Menu,
            "ctrl" => VirtualKeyCode.Control,
            "win" => VirtualKeyCode.LeftWin,
            "shift" => VirtualKeyCode.Shift,
            _ => VirtualKeyCode.None,
        };
    }

    /// <summary>Replaces the keyboard display-name API for deterministic tests.</summary>
    /// <param name="api">The replacement keyboard display API.</param>
    /// <returns>The previous keyboard display API.</returns>
    internal static INativeKeyboardDisplayApi SetDisplayApiForTesting(INativeKeyboardDisplayApi api)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(api);
        INativeKeyboardDisplayApi displayApi = GetDisplayApi();
        _displayApiOverride = ((api == WindowsNativeKeyboardDisplayApi.Instance) ? null : api);
        return displayApi;
    }

    /// <summary>Gets virtual key codes from a key combination description.</summary>
    /// <param name="keyDescription">The key combination description.</param>
    /// <returns>The virtual key codes parsed from the description.</returns>
    private static IEnumerable<VirtualKeyCode> GetVirtualKeyCodesFromString(string keyDescription)
    {
        if (string.IsNullOrEmpty(keyDescription))
        {
            yield break;
        }

        string[] array = keyDescription.Split('+');
        for (int i = 0; i < array.Length; i++)
        {
            string trimmed = array[i].Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                VirtualKeyCode virtualKeyCode = VirtualKeyCodeFromString(trimmed);
                if (virtualKeyCode != VirtualKeyCode.None)
                {
                    yield return virtualKeyCode;
                }
            }
        }
    }

    /// <summary>Normalizes left and right modifier keys when a side-specific name is not required.</summary>
    /// <param name="givenKey">The virtual key to normalize.</param>
    /// <param name="doNotCare">A value indicating whether left/right variants should be ignored.</param>
    /// <returns>The normalized virtual key.</returns>
    private static VirtualKeyCode NormalizeVirtualKey(VirtualKeyCode givenKey, bool doNotCare)
    {
        if (doNotCare)
        {
            VirtualKeyCode result;
            switch (givenKey)
            {
            case VirtualKeyCode.LeftMenu or VirtualKeyCode.RightMenu:
            {
                result = VirtualKeyCode.Menu;
                break;
            }

            case VirtualKeyCode.LeftControl or VirtualKeyCode.RightControl:
            {
                result = VirtualKeyCode.Control;
                break;
            }

            case VirtualKeyCode.LeftShift or VirtualKeyCode.RightShift:
            {
                result = VirtualKeyCode.Shift;
                break;
            }

            default:
            {
                result = givenKey;
                break;
            }
            }
            return result;
        }

        return givenKey;
    }

    /// <summary>Tries to build the localized display text for keypad operator keys.</summary>
    /// <param name="virtualKey">The virtual key.</param>
    /// <param name="keyName">The key-name buffer.</param>
    /// <param name="displayApi">The keyboard display-name API.</param>
    /// <param name="displayText">The localized display text.</param>
    /// <returns><see langword="true" /> when the key is a keypad operator.</returns>
    private static bool TryGetNumpadOperatorDisplayText(VirtualKeyCode virtualKey, Span<char> keyName, INativeKeyboardDisplayApi displayApi, out string displayText)
    {
        displayText = virtualKey switch
        {
            VirtualKeyCode.Multiply => GetNumpadOperatorDisplayText("*", keyName, displayApi),
            VirtualKeyCode.Divide => GetNumpadOperatorDisplayText("/", keyName, displayApi),
            _ => null,
        };
        return displayText is not null;
    }

    /// <summary>Builds the localized display text for a keypad operator.</summary>
    /// <param name="symbol">The operator symbol.</param>
    /// <param name="keyName">The key-name buffer.</param>
    /// <param name="displayApi">The keyboard display-name API.</param>
    /// <returns>The localized display text.</returns>
    private static string GetNumpadOperatorDisplayText(string symbol, Span<char> keyName, INativeKeyboardDisplayApi displayApi)
    {
        int characters = displayApi.GetKeyNameText(3_604_480U, keyName);
        string keyString = CP.ReactiveUI.Primitives.Windows.PolyFills.SpanText.Create(keyName.Slice(0, characters)).Replace("*", string.Empty).Trim()
            .ToLowerInvariant();
        if (!CP.ReactiveUI.Primitives.Windows.PolyFills.SpanText.Contains(keyString, '('))
        {
            return $"{CapitalizeFirstCharacter(keyString)} {symbol}";
        }

        return $"{symbol} {keyString}";
    }

    /// <summary>Gets the scan code value to pass to GetKeyNameText.</summary>
    /// <param name="virtualKey">The virtual key.</param>
    /// <param name="keyboardLayout">The keyboard layout handle.</param>
    /// <param name="displayApi">The keyboard display-name API.</param>
    /// <returns>The scan code value, or 0 when one cannot be resolved.</returns>
    private static uint GetDisplayTextScanCode(VirtualKeyCode virtualKey, IntPtr keyboardLayout, INativeKeyboardDisplayApi displayApi)
    {
        uint scanCode = virtualKey switch
        {
            VirtualKeyCode.Print => 311U,
            VirtualKeyCode.Pause => 69U,
            _ => displayApi.MapVirtualKeyEx(checked((uint)virtualKey), 4U, keyboardLayout),
        };
        if (scanCode != 0)
        {
            return (scanCode << 16) | GetExtendedKeyModifier(virtualKey);
        }

        return 0U;
    }

    /// <summary>Gets the GetKeyNameText extended-key modifier for keys whose mapped scan code omits it.</summary>
    /// <param name="virtualKey">The virtual key.</param>
    /// <returns>The extended-key modifier, or 0 when it is not required.</returns>
    private static uint GetExtendedKeyModifier(VirtualKeyCode virtualKey)
    {
        if (!IsExtendedKey(virtualKey))
        {
            return 0U;
        }

        return 16_777_216U;
    }

    /// <summary>Returns whether a mapped scan code requires the extended-key flag.</summary>
    /// <param name="virtualKey">The virtual key.</param>
    /// <returns><see langword="true" /> when the extended-key flag is required.</returns>
    private static bool IsExtendedKey(VirtualKeyCode virtualKey) => Array.IndexOf(ExtendedKeys, virtualKey) >= 0;

    /// <summary>Gets the display text for a resolved scan code.</summary>
    /// <param name="scanCode">The scan code value.</param>
    /// <param name="keyName">The key-name buffer.</param>
    /// <param name="fallbackKey">The fallback virtual key.</param>
    /// <param name="displayApi">The keyboard display-name API.</param>
    /// <returns>The display text.</returns>
    private static string GetDisplayText(uint scanCode, Span<char> keyName, VirtualKeyCode fallbackKey, INativeKeyboardDisplayApi displayApi)
    {
        int characters = displayApi.GetKeyNameText(scanCode, keyName);
        if (characters == 0)
        {
            return fallbackKey.ToString();
        }

        return CapitalizeFirstCharacter(CP.ReactiveUI.Primitives.Windows.PolyFills.SpanText.Create(keyName.Slice(0, characters)));
    }

    /// <summary>Capitalizes the first character and lowers the rest of a display string.</summary>
    /// <param name="value">The value to capitalize.</param>
    /// <returns>The capitalized value.</returns>
    private static string CapitalizeFirstCharacter(string value) => value.Length switch
        {
            0 => value,
            1 => value.ToUpperInvariant(),
            _ => $"{char.ToUpperInvariant(value[0])}{CP.ReactiveUI.Primitives.Windows.PolyFills.SpanText.Slice(value, 1).ToLowerInvariant()}",
        };

    /// <summary>Gets the active keyboard display-name API.</summary>
    /// <returns>The active keyboard display-name API.</returns>
    private static INativeKeyboardDisplayApi GetDisplayApi() => _displayApiOverride ?? WindowsNativeKeyboardDisplayApi.Instance;
}
