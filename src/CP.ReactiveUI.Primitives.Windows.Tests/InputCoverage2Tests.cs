// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Deterministic coverage for input native composition seams and low-level hook translation.</summary>
public sealed class InputCoverage2Tests
{
    /// <summary>The key-down message id.</summary>
    private const int WmKeyDown = 0x0100;

    /// <summary>The system key-down message id.</summary>
    private const int WmSysKeyDown = 0x0104;

    /// <summary>The offset of KBDLLHOOKSTRUCT.vkCode.</summary>
    private const int KeyboardVirtualKeyCodeOffset = 0;

    /// <summary>The offset of KBDLLHOOKSTRUCT.scanCode.</summary>
    private const int KeyboardScanCodeOffset = 4;

    /// <summary>The offset of KBDLLHOOKSTRUCT.flags.</summary>
    private const int KeyboardFlagsOffset = 8;

    /// <summary>The offset of KBDLLHOOKSTRUCT.time.</summary>
    private const int KeyboardTimestampOffset = 12;

    /// <summary>Tests keyboard hook callback translation, handled return values, state resolution, and disposal.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyboardHook_UsesComposedNativeApiAndTranslatesStateAsync()
    {
        var fake = new FakeNativeHookApi();
        _ = fake.PressedKeys.Add(VirtualKeyCode.RightControl);
        _ = fake.LockKeys.Add(VirtualKeyCode.NumLock);
        var previousApi = NativeHookMethods.SetApiForTesting(fake);

        try
        {
            var observed = new List<KeyboardHookEventArgs>();
            var subscription = KeyboardHook.KeyboardHookEvents.SubscribeOnNext(args =>
            {
                observed.Add(args);
                args.Handled = true;
            });

            try
            {
                var data = AllocateKeyboardHookData(VirtualKeyCode.KeyA, ExtendedKeyFlags.Injected, UIntNinetyNine);
                try
                {
                    var handledResult = fake.Invoke(0, (IntPtr)WmSysKeyDown, data);
                    await Assert.That(handledResult).IsEqualTo((IntPtr)1);
                }
                finally
                {
                    Marshal.FreeHGlobal(data);
                }

                await Assert.That(observed.Count).IsEqualTo(1);
                await Assert.That(observed[0].Key).IsEqualTo(VirtualKeyCode.KeyA);
                await Assert.That(observed[0].IsKeyDown).IsTrue();
                await Assert.That(observed[0].IsRightControl).IsTrue();
                await Assert.That(observed[0].IsLeftAlt).IsTrue();
                await Assert.That(observed[0].IsSystemKey).IsTrue();
                await Assert.That(observed[0].IsNumLockActive).IsTrue();
                await Assert.That(observed[0].IsInjectedByProcess).IsTrue();
                await Assert.That(fake.CallNextHookCount).IsEqualTo(0);

                var forwardedResult = fake.Invoke(-1, (IntPtr)WmKeyDown, IntPtr.Zero);
                await Assert.That(forwardedResult).IsEqualTo(fake.NextResult);
                await Assert.That(fake.CallNextHookCount).IsEqualTo(1);
            }
            finally
            {
                subscription.Dispose();
            }

            await Assert.That(fake.UnhookCount).IsEqualTo(1);
            await Assert.That(fake.UnhookHandle).IsEqualTo(fake.HookHandle);
            await Assert.That(fake.HookType).IsEqualTo(HookTypes.WH_KEYBOARD_LL);
        }
        finally
        {
            _ = NativeHookMethods.SetApiForTesting(previousApi);
        }
    }

    /// <summary>Tests keyboard lock-key state is adjusted for the active hook event.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyboardHook_TogglesLockStateForActiveKeyDownAsync()
    {
        var fake = new FakeNativeHookApi();
        var previousApi = NativeHookMethods.SetApiForTesting(fake);

        try
        {
            var observed = new List<KeyboardHookEventArgs>();
            using var subscription = KeyboardHook.KeyboardHookEvents.SubscribeOnNext(observed.Add);
            var data = AllocateKeyboardHookData(VirtualKeyCode.Capital, ExtendedKeyFlags.None, UIntFortyTwo);
            try
            {
                _ = fake.Invoke(0, (IntPtr)WmKeyDown, data);
            }
            finally
            {
                Marshal.FreeHGlobal(data);
            }

            await Assert.That(observed.Count).IsEqualTo(1);
            await Assert.That(observed[0].IsModifier).IsTrue();
            await Assert.That(observed[0].IsCapsLockActive).IsTrue();
        }
        finally
        {
            _ = NativeHookMethods.SetApiForTesting(previousApi);
        }
    }

    /// <summary>Tests mouse hook callback translation, handled and forwarded return values, and disposal.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MouseHook_UsesComposedNativeApiAndTranslatesMessagesAsync()
    {
        var fake = new FakeNativeHookApi();
        var previousApi = NativeHookMethods.SetApiForTesting(fake);

        try
        {
            var observed = new List<MouseHookEventArgs>();
            var subscription = MouseHook.MouseHookEvents.SubscribeOnNext(args =>
            {
                observed.Add(args);
                args.Handled = observed.Count == 1;
            });

            try
            {
                var data = AllocateMouseHookData(new(Thirty, Forty));
                try
                {
                    var handledResult = fake.Invoke(0, (IntPtr)WindowsMessages.WM_LBUTTONDOWN, data);
                    await Assert.That(handledResult).IsEqualTo((IntPtr)1);

                    var forwardedResult = fake.Invoke(0, (IntPtr)WindowsMessages.WM_MOUSEMOVE, data);
                    await Assert.That(forwardedResult).IsEqualTo(fake.NextResult);
                }
                finally
                {
                    Marshal.FreeHGlobal(data);
                }

                await Assert.That(observed.Count).IsEqualTo(Two);
                await Assert.That(observed[0].WindowsMessage).IsEqualTo(WindowsMessages.WM_LBUTTONDOWN);
                await Assert.That(observed[0].Point).IsEqualTo(new(Thirty, Forty));
                await Assert.That(observed[1].WindowsMessage).IsEqualTo(WindowsMessages.WM_MOUSEMOVE);
                await Assert.That(fake.CallNextHookCount).IsEqualTo(1);
            }
            finally
            {
                subscription.Dispose();
            }

            await Assert.That(fake.UnhookCount).IsEqualTo(1);
            await Assert.That(fake.HookType).IsEqualTo(HookTypes.WH_MOUSE_LL);
        }
        finally
        {
            _ = NativeHookMethods.SetApiForTesting(previousApi);
        }
    }

    /// <summary>Tests keyboard and mouse input generators compose native SendInput records without real OS input.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InputGenerators_UseComposedNativeInputApiAsync()
    {
        var fake = new FakeNativeInputApi { SendReturn = UIntTen };
        var previousApi = NativeInput.SetApiForTesting(fake);

        try
        {
            await Assert.That(KeyboardInputGenerator.KeyDown(VirtualKeyCode.KeyA, VirtualKeyCode.KeyB)).IsEqualTo(UIntTen);
            await Assert.That(fake.LastInputs.Length).IsEqualTo(Two);
            await Assert.That(fake.LastInputs[0].InputUnion.KeyboardInput.VirtualKeyCode).IsEqualTo(VirtualKeyCode.KeyA);
            await Assert.That(fake.LastInputs[1].InputUnion.KeyboardInput.KeyEventFlags).IsEqualTo(KeyEventFlags.None);

            await Assert.That(KeyboardInputGenerator.KeyUp(VirtualKeyCode.KeyC)).IsEqualTo(UIntTen);
            await Assert.That(fake.LastInputs[0].InputUnion.KeyboardInput.KeyEventFlags).IsEqualTo(KeyEventFlags.KeyUp);

            await Assert.That(KeyboardInputGenerator.KeyPresses(VirtualKeyCode.KeyD, VirtualKeyCode.KeyE)).IsEqualTo(UIntTen);
            await Assert.That(fake.LastInputs.Length).IsEqualTo(Four);
            await Assert.That(fake.LastInputs[1].InputUnion.KeyboardInput.KeyEventFlags).IsEqualTo(KeyEventFlags.KeyUp);

            await Assert.That(KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Control, VirtualKeyCode.KeyF)).IsEqualTo(UIntTen);
            await Assert.That(fake.LastInputs.Length).IsEqualTo(Four);
            await Assert.That(fake.LastInputs[0].InputUnion.KeyboardInput.KeyEventFlags).IsEqualTo(KeyEventFlags.None);
            await Assert.That(fake.LastInputs[Two].InputUnion.KeyboardInput.KeyEventFlags).IsEqualTo(KeyEventFlags.KeyUp);

            await Assert.That(MouseInputGenerator.MouseClick(MouseButtons.Left | MouseButtons.Right, new(Ten, Twenty), UIntNinetyNine)).IsEqualTo(UIntTen);
            await Assert.That(fake.LastInputs.Length).IsEqualTo(Two);
            await Assert.That(fake.LastInputs[0].InputUnion.MouseInput.MouseEventFlags).HasFlag(MouseEventFlags.LeftDown);
            await Assert.That(fake.LastInputs[1].InputUnion.MouseInput.MouseEventFlags).HasFlag(MouseEventFlags.RightUp);

            await Assert.That(MouseInputGenerator.MoveMouse(new(Thirty, Forty), UIntFortyTwo)).IsEqualTo(UIntTen);
            await Assert.That(fake.LastInputs[0].InputUnion.MouseInput.MouseEventFlags).HasFlag(MouseEventFlags.Move);

            await Assert.That(MouseInputGenerator.MoveMouseWheel(-OneHundredTwenty, null, UIntFortyTwo)).IsEqualTo(UIntTen);
            await Assert.That(fake.LastInputs[0].InputUnion.MouseInput.MouseData).IsEqualTo(-OneHundredTwenty);
            await Assert.That(fake.SendCalls).IsEqualTo(Seven);
        }
        finally
        {
            _ = NativeInput.SetApiForTesting(previousApi);
        }
    }

    /// <summary>Tests last-input fallbacks use the composed native input API failure branch.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeInput_LastInputFallbacksUseComposedApiAsync()
    {
        var fake = new FakeNativeInputApi { LastInputResult = false };
        var previousApi = NativeInput.SetApiForTesting(fake);

        try
        {
            await Assert.That(NativeInput.LastInputDateTime).IsEqualTo(DateTimeOffset.MinValue);
            await Assert.That(NativeInput.LastInputTimeSpan).IsEqualTo(TimeSpan.MaxValue);
            await Assert.That(fake.LastInputCalls).IsEqualTo(Two);
        }
        finally
        {
            _ = NativeInput.SetApiForTesting(previousApi);
        }
    }

    /// <summary>Allocates a low-level keyboard hook payload.</summary>
    /// <param name="key">The virtual key code.</param>
    /// <param name="flags">The low-level hook flags.</param>
    /// <param name="timestamp">The hook timestamp.</param>
    /// <returns>An unmanaged pointer containing a keyboard hook payload.</returns>
    private static IntPtr AllocateKeyboardHookData(VirtualKeyCode key, ExtendedKeyFlags flags, uint timestamp)
    {
        var data = Marshal.AllocHGlobal(Marshal.SizeOf<KeyboardLowLevelHookStruct>());
        Marshal.WriteInt32(data, KeyboardVirtualKeyCodeOffset, (int)key);
        Marshal.WriteInt32(data, KeyboardScanCodeOffset, 0);
        Marshal.WriteInt32(data, KeyboardFlagsOffset, unchecked((int)flags));
        Marshal.WriteInt32(data, KeyboardTimestampOffset, unchecked((int)timestamp));
        return data;
    }

    /// <summary>Allocates a low-level mouse hook payload.</summary>
    /// <param name="point">The hook point.</param>
    /// <returns>An unmanaged pointer containing a mouse hook payload.</returns>
    private static IntPtr AllocateMouseHookData(NativePoint point)
    {
        var data = Marshal.AllocHGlobal(Marshal.SizeOf<MouseLowLevelHookStruct>());
        var hookStruct = new MouseLowLevelHookStruct { Pt = point };
        Marshal.StructureToPtr(hookStruct, data, false);
        return data;
    }

    /// <summary>Deterministic native hook API for callback tests.</summary>
    internal sealed class FakeNativeHookApi : INativeHookApi
    {
        /// <summary>The pressed-key state returned by GetAsyncKeyState.</summary>
        private const short PressedKeyState = unchecked((short)0x8000);

        /// <summary>The active lock-key state returned by GetKeyState.</summary>
        private const short ActiveLockKeyState = 1;

        /// <summary>Stores the active hook callback.</summary>
        private LowLevelHookProc _callback;

        /// <summary>Gets the simulated pressed keys.</summary>
        internal HashSet<VirtualKeyCode> PressedKeys { get; } = [];

        /// <summary>Gets the simulated active lock keys.</summary>
        internal HashSet<VirtualKeyCode> LockKeys { get; } = [];

        /// <summary>Gets the hook handle returned by hook registration.</summary>
        internal IntPtr HookHandle { get; } = new(OneThousandTwoHundredThirtyFour);

        /// <summary>Gets the result returned when a hook forwards to the next hook.</summary>
        internal IntPtr NextResult { get; } = new(FourThousandThreeHundredTwentyOne);

        /// <summary>Gets the registered hook type.</summary>
        internal HookTypes HookType { get; private set; }

        /// <summary>Gets the number of forwarded hook calls.</summary>
        internal int CallNextHookCount { get; private set; }

        /// <summary>Gets the number of unhook calls.</summary>
        internal int UnhookCount { get; private set; }

        /// <summary>Gets the last unhooked handle.</summary>
        internal IntPtr UnhookHandle { get; private set; }

        /// <inheritdoc />
        public IntPtr CallNextHookEx(IntPtr hookHandle, int code, IntPtr parameter, IntPtr data)
        {
            CallNextHookCount++;
            return NextResult;
        }

        /// <inheritdoc />
        public short GetAsyncKeyState(VirtualKeyCode keyCode) => PressedKeys.Contains(keyCode) ? PressedKeyState : (short)0;

        /// <inheritdoc />
        public short GetKeyState(VirtualKeyCode keyCode) => LockKeys.Contains(keyCode) ? ActiveLockKeyState : (short)0;

        /// <inheritdoc />
        public IntPtr SetWindowsHookEx(HookTypes hookType, LowLevelHookProc callback, IntPtr moduleHandle, uint threadId)
        {
            HookType = hookType;
            _callback = callback;
            return HookHandle;
        }

        /// <inheritdoc />
        public bool UnhookWindowsHookEx(IntPtr hookHandle)
        {
            UnhookCount++;
            UnhookHandle = hookHandle;
            return true;
        }

        /// <summary>Invokes the captured hook callback.</summary>
        /// <param name="code">The hook code.</param>
        /// <param name="parameter">The hook parameter.</param>
        /// <param name="data">The hook payload pointer.</param>
        /// <returns>The callback result.</returns>
        internal IntPtr Invoke(int code, IntPtr parameter, IntPtr data) => _callback(code, parameter, data);
    }

    /// <summary>Deterministic native input API for generator tests.</summary>
    internal sealed class FakeNativeInputApi : INativeInputApi
    {
        /// <summary>Gets or sets the return value for SendInput.</summary>
        internal uint SendReturn { get; set; }

        /// <summary>Gets or sets a value indicating whether last-input queries succeed.</summary>
        internal bool LastInputResult { get; set; }

        /// <summary>Gets the last input records sent.</summary>
        internal Input[] LastInputs { get; private set; } = [];

        /// <summary>Gets the number of SendInput calls.</summary>
        internal int SendCalls { get; private set; }

        /// <summary>Gets the number of last-input calls.</summary>
        internal int LastInputCalls { get; private set; }

        /// <inheritdoc />
        public bool GetLastInputInfo(ref LastInputInfo lastInputInfo)
        {
            LastInputCalls++;
            return LastInputResult;
        }

        /// <inheritdoc />
        public uint SendInput(Input[] inputs)
        {
            SendCalls++;
            LastInputs = inputs;
            return SendReturn;
        }
    }
}
