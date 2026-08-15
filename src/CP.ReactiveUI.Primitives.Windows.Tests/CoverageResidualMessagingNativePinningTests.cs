// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Deterministic coverage tests for native messaging string-pinning boundaries.</summary>
public sealed class CoverageResidualMessagingNativePinningTests
{
    /// <summary>The expected number of hooks added by the handler.</summary>
    private const int ExpectedAddedHookCount = 2;

    /// <summary>The deterministic registered native class atom.</summary>
    private const int NativeClassAtom = 12;

    /// <summary>The deterministic extended window style.</summary>
    private const uint NativeExtendedStyle = 7U;

    /// <summary>The deterministic native window height.</summary>
    private const int NativeHeight = 9;

    /// <summary>The deterministic native window width.</summary>
    private const int NativeWidth = 8;

    /// <summary>The deterministic native class name.</summary>
    private const string NativeClassName = "CP.ReactiveUI.Primitives.Windows.Tests.NativePinning";

    /// <summary>The deterministic native window name.</summary>
    private const string NativeWindowName = "Deterministic native pinning window";

    /// <summary>The deterministic application message window handle.</summary>
    private static readonly IntPtr ApplicationMessageWindowHandle = new(1);

    /// <summary>The deterministic created native window handle.</summary>
    private static readonly IntPtr NativeCreatedWindowHandle = new(34);

    /// <summary>The deterministic native module instance handle.</summary>
    private static readonly IntPtr NativeInstanceHandle = new(56);

    /// <summary>The deterministic long message parameter.</summary>
    private static readonly IntPtr TestLongParameter = new(3);

    /// <summary>The deterministic word message parameter.</summary>
    private static readonly IntPtr TestWordParameter = new(2);

    /// <summary>The unused hook result.</summary>
    private static readonly IntPtr UnusedHookResult = new(1);

    /// <summary>Verifies hook dispatch stops when composition marks the listener as disposed.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinProcListenerHookDispatchUsesLifecycleCompositionAsync()
    {
        var listenerState = new MutableListenerState();
        var firstHookCalls = 0;
        var secondHookCalls = 0;
        HwndSourceHook firstHook = (nint windowHandle, int messageId, nint wordParam, nint longParam, ref bool handled) =>
        {
            _ = windowHandle;
            _ = messageId;
            _ = wordParam;
            _ = longParam;
            firstHookCalls++;
            listenerState.IsDisposed = true;
            return IntPtr.Zero;
        };
        HwndSourceHook secondHook = (nint windowHandle, int messageId, nint wordParam, nint longParam, ref bool handled) =>
        {
            _ = windowHandle;
            _ = messageId;
            _ = wordParam;
            _ = longParam;
            secondHookCalls++;
            handled = true;
            return UnusedHookResult;
        };
        var message = FormsMessage.Create(ApplicationMessageWindowHandle, (int)WindowsMessages.WM_APP, TestWordParameter, TestLongParameter);
        var handled = WinProcListener.ProcessHooksForTesting(listenerState, [firstHook, secondHook], ref message);

        await Assert.That(handled).IsFalse();
        await Assert.That(firstHookCalls).IsEqualTo(1);
        await Assert.That(secondHookCalls).IsEqualTo(0);
        await Assert.That(message.Result).IsEqualTo(IntPtr.Zero);
    }

    /// <summary>Verifies null hook collections return without invoking hooks.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinProcListenerHookDispatchAcceptsNullHookCollectionsAsync()
    {
        var listenerState = new MutableListenerState();
        var message = FormsMessage.Create(ApplicationMessageWindowHandle, (int)WindowsMessages.WM_APP, TestWordParameter, TestLongParameter);

        var handled = WinProcListener.ProcessHooksForTesting(listenerState, null, ref message);

        await Assert.That(handled).IsFalse();
        await Assert.That(message.Result).IsEqualTo(IntPtr.Zero);
    }

    /// <summary>Verifies Forms message publication completes through deterministic lifecycle states.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinProcFormsMessagePublicationCompletesForDestroyAndDisposedStatesAsync()
    {
        var activeState = new MutableListenerState();
        var activeObserver = new RecordingMessageObserver();
        var destroyObserver = new RecordingMessageObserver();
        var disposedState = new MutableListenerState { IsDisposed = true };
        var disposedObserver = new RecordingMessageObserver();

        _ = WinProcFormsExtensions.PublishWindowMessageForTesting(activeState, activeObserver, ApplicationMessageWindowHandle, (int)WindowsMessages.WM_APP, TestWordParameter, TestLongParameter);
        _ = WinProcFormsExtensions.PublishWindowMessageForTesting(activeState, destroyObserver, ApplicationMessageWindowHandle, (int)WindowsMessages.WM_DESTROY, TestWordParameter, TestLongParameter);
        _ = WinProcFormsExtensions.PublishWindowMessageForTesting(disposedState, disposedObserver, ApplicationMessageWindowHandle, (int)WindowsMessages.WM_APP, TestWordParameter, TestLongParameter);

        await Assert.That(activeObserver.Messages.Count).IsEqualTo(1);
        await Assert.That(activeObserver.Completed).IsFalse();
        await Assert.That(destroyObserver.Completed).IsTrue();
        await Assert.That(disposedObserver.Completed).IsTrue();
    }

    /// <summary>Verifies hook cleanup tolerates subscriptions without optional cleanup disposables.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinProcHandlerCleanupSupportsHooksWithoutOptionalDisposablesAsync()
    {
        var messageWindow = new NoOpMessageHandlerWindow();
        using var factory = WinProcHandler.OverrideMessageWindowFactoryForTesting(() => messageWindow);
        var handler = new WinProcHandler();
        HwndSourceHook hook = static (nint windowHandle, int messageId, nint wordParam, nint longParam, ref bool handled) =>
        {
            _ = windowHandle;
            _ = messageId;
            _ = wordParam;
            _ = longParam;
            _ = handled;
            return IntPtr.Zero;
        };
        using var subscription = handler.Subscribe(new(hook));

        handler.UnsubscribeAllHooks();

        await Assert.That(messageWindow.AddedHooks).IsEqualTo(ExpectedAddedHookCount);
        await Assert.That(messageWindow.RemovedHooks).IsEqualTo(1);
    }

    /// <summary>Verifies individual hook disposal tolerates a missing optional disposable.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinProcHandlerSubscriptionDisposesHookWithoutOptionalDisposableAsync()
    {
        var messageWindow = new NoOpMessageHandlerWindow();
        using var factory = WinProcHandler.OverrideMessageWindowFactoryForTesting(() => messageWindow);
        var handler = new WinProcHandler();
        HwndSourceHook hook = static (nint windowHandle, int messageId, nint wordParam, nint longParam, ref bool handled) =>
        {
            _ = windowHandle;
            _ = messageId;
            _ = wordParam;
            _ = longParam;
            _ = handled;
            return IntPtr.Zero;
        };

        IDisposable subscription = handler.Subscribe(new(hook));
        subscription.Dispose();

        await Assert.That(messageWindow.RemovedHooks).IsEqualTo(1);
    }

    /// <summary>Verifies native string pinning uses composed operations without invoking User32.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeStringPinningOperationsUseDeterministicCallbacksAsync()
    {
        NativePinningCapture capture = new();
        using var operations = capture.CreateOperations();
        WndProc windowProcedure = static (_, _, _, _) => 0U;
        var registeredAtom = SharedMessageWindow.NativeMethods.RegisterClassEx(windowProcedure, NativeInstanceHandle, NativeClassName);
        var unregistered = SharedMessageWindow.NativeMethods.UnregisterClass(NativeClassName, NativeInstanceHandle);
        var createdWindow = SharedMessageWindow.NativeMethods.CreateWindowEx(new()
        {
            ClassName = NativeClassName,
            ExtendedStyle = NativeExtendedStyle,
            Height = NativeHeight,
            InstanceHandle = (long)NativeInstanceHandle,
            Width = NativeWidth,
            WindowName = NativeWindowName,
        });

        await Assert.That(registeredAtom).IsEqualTo((ushort)NativeClassAtom);
        await Assert.That(unregistered).IsTrue();
        await Assert.That(createdWindow).IsEqualTo(NativeCreatedWindowHandle);
        await capture.AssertValuesAsync();
    }

    /// <summary>Verifies native string pinning accepts null class and window names.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeStringPinningOperationsUseNullPointersForNullStringsAsync()
    {
        NativePinningCapture capture = new();
        using var operations = capture.CreateOperations();
        WndProc windowProcedure = static (_, _, _, _) => 0U;

        var registeredAtom = SharedMessageWindow.NativeMethods.RegisterClassEx(windowProcedure, NativeInstanceHandle, null);
        var unregistered = SharedMessageWindow.NativeMethods.UnregisterClass(null, NativeInstanceHandle);
        var createdWindow = SharedMessageWindow.NativeMethods.CreateWindowEx(
            new() { ClassName = null, InstanceHandle = (long)NativeInstanceHandle, WindowName = null });

        await Assert.That(registeredAtom).IsEqualTo((ushort)NativeClassAtom);
        await Assert.That(unregistered).IsTrue();
        await Assert.That(createdWindow).IsEqualTo(NativeCreatedWindowHandle);
        await Assert.That(capture.RegisterPointer).IsNotEqualTo(0L);
        await Assert.That(capture.UnregisterClassNamePointer).IsEqualTo(0L);
        await Assert.That(capture.ClassNamePointer).IsEqualTo(0L);
        await Assert.That(capture.WindowNamePointer).IsEqualTo(0L);
    }

    /// <summary>Captures the managed inputs supplied to native string-pinning operations.</summary>
    private sealed class NativePinningCapture
    {
        /// <summary>Gets the pinned native class-name pointer.</summary>
        public long ClassNamePointer { get; private set; }

        /// <summary>Gets the window extended style.</summary>
        public uint ExtendedStyle { get; private set; }

        /// <summary>Gets the window height.</summary>
        public int Height { get; private set; }

        /// <summary>Gets the native module instance handle.</summary>
        public long InstanceHandle { get; private set; }

        /// <summary>Gets the registered class structure pointer.</summary>
        public long RegisterPointer { get; private set; }

        /// <summary>Gets the class-name pointer supplied for unregistration.</summary>
        public long UnregisterClassNamePointer { get; private set; }

        /// <summary>Gets the module instance handle supplied for unregistration.</summary>
        public long UnregisterInstanceHandle { get; private set; }

        /// <summary>Gets the window width.</summary>
        public int Width { get; private set; }

        /// <summary>Gets the pinned native window-name pointer.</summary>
        public long WindowNamePointer { get; private set; }

        /// <summary>Asynchronously verifies all captured native operation values.</summary>
        /// <returns>A task representing the assertions.</returns>
        public async Task AssertValuesAsync()
        {
            await Assert.That(RegisterPointer).IsNotEqualTo(0L);
            await Assert.That(UnregisterClassNamePointer).IsNotEqualTo(0L);
            await Assert.That(UnregisterInstanceHandle).IsEqualTo((long)NativeInstanceHandle);
            await Assert.That(ClassNamePointer).IsNotEqualTo(0L);
            await Assert.That(ExtendedStyle).IsEqualTo(NativeExtendedStyle);
            await Assert.That(Height).IsEqualTo(NativeHeight);
            await Assert.That(InstanceHandle).IsEqualTo((long)NativeInstanceHandle);
            await Assert.That(Width).IsEqualTo(NativeWidth);
            await Assert.That(WindowNamePointer).IsNotEqualTo(0L);
        }

        /// <summary>Creates an override scope that records deterministic native operations.</summary>
        /// <returns>A scope that restores the native operations.</returns>
        public IDisposable CreateOperations() => SharedMessageWindow.NativeMethods.OverrideStringPinningOperationsForTesting(RegisterClass, UnregisterClass, CreateWindow);

        /// <summary>Records native window-creation arguments.</summary>
        /// <param name="arguments">The native window-creation arguments.</param>
        /// <returns>The deterministic native window handle.</returns>
        private nint CreateWindow(SharedMessageWindow.NativeMethods.CreateWindowArguments arguments)
        {
            ExtendedStyle = arguments.ExtendedStyle;
            ClassNamePointer = arguments.ClassNamePointer;
            WindowNamePointer = arguments.WindowNamePointer;
            Width = arguments.Width;
            Height = arguments.Height;
            InstanceHandle = arguments.InstanceHandle;
            return NativeCreatedWindowHandle;
        }

        /// <summary>Records the native class-registration pointer.</summary>
        /// <param name="pointer">The native class-registration pointer.</param>
        /// <returns>The deterministic registered class atom.</returns>
        private ushort RegisterClass(nint pointer)
        {
            RegisterPointer = (long)pointer;
            return NativeClassAtom;
        }

        /// <summary>Records the native class-unregistration arguments.</summary>
        /// <param name="pointer">The native class-name pointer.</param>
        /// <param name="handle">The native module instance handle.</param>
        /// <returns><c>true</c> to emulate successful unregistration.</returns>
        private bool UnregisterClass(nint pointer, nint handle)
        {
            UnregisterClassNamePointer = (long)pointer;
            UnregisterInstanceHandle = (long)handle;
            return true;
        }
    }

    /// <summary>Provides mutable lifecycle state for deterministic hook tests.</summary>
    private sealed class MutableListenerState : IWinProcListenerState
    {
        /// <inheritdoc />
        public bool IsDisposed { get; set; }
    }

    /// <summary>Records messages and completion notifications.</summary>
    private sealed class RecordingMessageObserver : IObserver<WindowMessageInfo>
    {
        /// <summary>Gets the messages received by the observer.</summary>
        public List<WindowMessageInfo> Messages { get; } = [];

        /// <summary>Gets a value indicating whether completion was received.</summary>
        public bool Completed { get; private set; }

        /// <inheritdoc />
        public void OnCompleted() => Completed = true;

        /// <inheritdoc />
        public void OnError(Exception error) => GC.KeepAlive(error);

        /// <inheritdoc />
        public void OnNext(WindowMessageInfo value) => Messages.Add(value);
    }

    /// <summary>Provides a deterministic message-handler window without a native HWND.</summary>
    private sealed class NoOpMessageHandlerWindow : IMessageHandlerWindow
    {
        /// <inheritdoc />
        public event EventHandler Disposed
        {
            add
            {
                _ = value;
            }

            remove
            {
                _ = value;
            }
        }

        /// <summary>Gets the number of hooks added to the window.</summary>
        public int AddedHooks { get; private set; }

        /// <summary>Gets the number of hooks removed from the window.</summary>
        public int RemovedHooks { get; private set; }

        /// <inheritdoc />
        public long Handle => 1L;

        /// <inheritdoc />
        public bool IsDisposed => false;

        /// <inheritdoc />
        public HwndSource Source => null;

        /// <inheritdoc />
        public void AddHook(HwndSourceHook hook)
        {
            _ = hook;
            AddedHooks++;
        }

        /// <inheritdoc />
        public void RemoveHook(HwndSourceHook hook)
        {
            _ = hook;
            RemovedHooks++;
        }
    }
}
