// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Media;

#if REACTIVE_TEST_SHIM
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
using TestRxVoid = System.Reactive.Unit;
#else
using CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
using TestRxVoid = ReactiveUI.Primitives.RxVoid;
#endif

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests the dynamic window-movement guard.</summary>
public sealed class WindowsMoveTests
{
    /// <summary>The system command that begins interactive resizing.</summary>
    private const int ResizeSystemCommand = 0xF000;

    /// <summary>The system command that begins interactive movement.</summary>
    private const int MoveSystemCommand = 0xF010;

    /// <summary>A reserved low-order system-command source bit.</summary>
    private const int SystemCommandSourceBit = 0x0002;

    /// <summary>The expected number of move-condition evaluations.</summary>
    private const int ExpectedMoveEvaluationCount = 2;

    /// <summary>An undefined block-mode value.</summary>
    private const int UndefinedBlockMode = -1;

    /// <summary>The window message that carries system commands.</summary>
    private const int SystemCommandMessage = 0x0112;

    /// <summary>An unrelated window message.</summary>
    private const int UnrelatedMessage = 0x0111;

    /// <summary>Verifies the allow condition is evaluated again for every move request.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DynamicConditionIsEvaluatedForEveryMoveRequestAsync()
    {
        var allowMove = false;
        var evaluationCount = 0;
        var movementGuard = new WindowsMove(() =>
        {
            evaluationCount++;
            return allowMove;
        });
        var handled = false;

        IntPtr blockedResult = movementGuard.ProcessWindowMessageForTesting(
            SystemCommandMessage,
            (nint)MoveSystemCommand,
            ref handled);

        await Assert.That(handled).IsTrue();
        await Assert.That(blockedResult).IsEqualTo(IntPtr.Zero);

        allowMove = true;
        handled = false;
        IntPtr allowedResult = movementGuard.ProcessWindowMessageForTesting(
            SystemCommandMessage,
            (nint)MoveSystemCommand,
            ref handled);

        await Assert.That(handled).IsFalse();
        await Assert.That(allowedResult).IsEqualTo(IntPtr.Zero);
        await Assert.That(evaluationCount).IsEqualTo(ExpectedMoveEvaluationCount);
    }

    /// <summary>Verifies low-order source bits do not prevent move-command recognition.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MoveCommandSourceBitsAreMaskedAsync()
    {
        var movementGuard = new WindowsMove(static () => false);
        var handled = false;

        _ = movementGuard.ProcessWindowMessageForTesting(
            SystemCommandMessage,
            (nint)(MoveSystemCommand | SystemCommandSourceBit),
            ref handled);

        await Assert.That(handled).IsTrue();
    }

    /// <summary>Verifies the default mode dynamically protects resize requests as well as move requests.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MoveAndResizeModeUsesDynamicConditionForResizeAsync()
    {
        var allowOperation = false;
        var movementGuard = new WindowsMove(() => allowOperation);
        var handled = false;

        _ = movementGuard.ProcessWindowMessageForTesting(
            SystemCommandMessage,
            (nint)ResizeSystemCommand,
            ref handled);

        await Assert.That(handled).IsTrue();

        allowOperation = true;
        handled = false;
        _ = movementGuard.ProcessWindowMessageForTesting(
            SystemCommandMessage,
            (nint)ResizeSystemCommand,
            ref handled);

        await Assert.That(handled).IsFalse();
    }

    /// <summary>Verifies move-only mode and unrelated messages do not evaluate the condition or change handled state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MoveOnlyModeLeavesResizeAndUnrelatedMessagesUntouchedAsync()
    {
        var evaluationCount = 0;
        var movementGuard = new WindowsMove(
            () =>
            {
                evaluationCount++;
                return false;
            },
            WindowsMoveBlockMode.MoveOnly);
        var resizeHandled = false;
        var unrelatedHandled = true;

        _ = movementGuard.ProcessWindowMessageForTesting(
            SystemCommandMessage,
            (nint)ResizeSystemCommand,
            ref resizeHandled);
        _ = movementGuard.ProcessWindowMessageForTesting(
            UnrelatedMessage,
            (nint)MoveSystemCommand,
            ref unrelatedHandled);

        await Assert.That(resizeHandled).IsFalse();
        await Assert.That(unrelatedHandled).IsTrue();
        await Assert.That(evaluationCount).IsEqualTo(0);
    }

    /// <summary>Verifies an allow condition is required.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ConstructorRejectsMissingConditionAsync() =>
        await Assert.That(static () => new WindowsMove(null)).Throws<ArgumentNullException>();

    /// <summary>Verifies undefined block modes are rejected.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ConstructorRejectsUndefinedBlockModeAsync() =>
        await Assert.That(static () => new WindowsMove(static () => false, (WindowsMoveBlockMode)UndefinedBlockMode))
            .Throws<ArgumentOutOfRangeException>();

    /// <summary>Verifies subscription installs the real hook path and disposal removes the same hook.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SubscriptionUsesDynamicConditionAndRemovesHookOnDisposalAsync()
    {
        var allowOperation = false;
        var movementGuard = new WindowsMove(() => allowOperation);
        var hookSource = new DeterministicHookSource();
        var observer = new RecordingObserver<TestRxVoid>();

        IDisposable subscription = movementGuard.Subscribe(observer, hookSource);
        (IntPtr blockedResult, bool blockedHandled) = hookSource.Publish(SystemCommandMessage, MoveSystemCommand);
        allowOperation = true;
        (IntPtr allowedResult, bool allowedHandled) = hookSource.Publish(SystemCommandMessage, ResizeSystemCommand);
        subscription.Dispose();

        await Assert.That(blockedHandled).IsTrue();
        await Assert.That(blockedResult).IsEqualTo(IntPtr.Zero);
        await Assert.That(allowedHandled).IsFalse();
        await Assert.That(allowedResult).IsEqualTo(IntPtr.Zero);
        await Assert.That(hookSource.AddedHooks).IsEqualTo(1);
        await Assert.That(hookSource.RemovedHooks).IsEqualTo(1);
        await Assert.That(observer.Error).IsNull();
    }

    /// <summary>Verifies a disconnected visual reports setup failure through the observer.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DisconnectedVisualReportsSourceUnavailableAsync()
    {
        var movementGuard = new WindowsMove(static () => false);
        var observer = new RecordingObserver<TestRxVoid>();

        using IDisposable subscription = movementGuard.Subscribe(observer, new DrawingVisual());

        await Assert.That(observer.Error).IsTypeOf<InvalidOperationException>();
    }

    /// <summary>Verifies every window extension overload validates its required arguments eagerly.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowExtensionOverloadsRejectMissingArgumentsAsync()
    {
        Window missingWindow = null;
        Func<bool> missingCondition = null;
        var window = new Window();

        await Assert.That(() => missingWindow.BlockMove()).Throws<ArgumentNullException>();
        await Assert.That(() => missingWindow.BlockMove(static () => false)).Throws<ArgumentNullException>();
        await Assert.That(() => missingWindow.BlockMoveAndResize()).Throws<ArgumentNullException>();
        await Assert.That(() => missingWindow.BlockMoveAndResize(static () => false)).Throws<ArgumentNullException>();
        await Assert.That(() => window.BlockMove(missingCondition)).Throws<ArgumentNullException>();
        await Assert.That(() => window.BlockMoveAndResize(missingCondition)).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies all extension overloads defer installation until the WPF window source is initialized.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowExtensionOverloadsAttachWhenWindowSourceInitializesAsync()
    {
        var window = new Window { ShowInTaskbar = false, WindowStyle = WindowStyle.ToolWindow };
        var moveObserver = new RecordingObserver<TestRxVoid>();
        var conditionalMoveObserver = new RecordingObserver<TestRxVoid>();
        var resizeObserver = new RecordingObserver<TestRxVoid>();
        var conditionalResizeObserver = new RecordingObserver<TestRxVoid>();
        IDisposable moveSubscription = null;
        IDisposable conditionalMoveSubscription = null;
        IDisposable resizeSubscription = null;
        IDisposable conditionalResizeSubscription = null;

        try
        {
            moveSubscription = window.BlockMove().Subscribe(moveObserver);
            conditionalMoveSubscription = window.BlockMove(static () => false).Subscribe(conditionalMoveObserver);
            resizeSubscription = window.BlockMoveAndResize().Subscribe(resizeObserver);
            conditionalResizeSubscription = window.BlockMoveAndResize(static () => true).Subscribe(conditionalResizeObserver);

            window.Show();

            await Assert.That(PresentationSource.FromVisual(window)).IsTypeOf<HwndSource>();
            await Assert.That(moveObserver.Error).IsNull();
            await Assert.That(conditionalMoveObserver.Error).IsNull();
            await Assert.That(resizeObserver.Error).IsNull();
            await Assert.That(conditionalResizeObserver.Error).IsNull();
        }
        finally
        {
            conditionalResizeSubscription?.Dispose();
            resizeSubscription?.Dispose();
            conditionalMoveSubscription?.Dispose();
            moveSubscription?.Dispose();
            window.Close();
        }
    }

    /// <summary>Verifies disposing before source initialization prevents deferred hook installation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowExtensionDisposalCancelsDeferredInstallationAsync()
    {
        var window = new Window { ShowInTaskbar = false, WindowStyle = WindowStyle.ToolWindow };
        var observer = new RecordingObserver<TestRxVoid>();
        IDisposable subscription = window.BlockMove().Subscribe(observer);

        subscription.Dispose();
        subscription.Dispose();
        try
        {
            window.Show();

            await Assert.That(PresentationSource.FromVisual(window)).IsTypeOf<HwndSource>();
            await Assert.That(observer.Error).IsNull();
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Verifies an extension subscription can attach immediately to an initialized WPF source.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowExtensionAttachesImmediatelyToInitializedSourceAsync()
    {
        var window = new Window { ShowInTaskbar = false, WindowStyle = WindowStyle.ToolWindow };
        var observer = new RecordingObserver<TestRxVoid>();
        IDisposable subscription = null;

        try
        {
            window.Show();
            subscription = window.BlockMoveAndResize(static () => false).Subscribe(observer);

            await Assert.That(PresentationSource.FromVisual(window)).IsTypeOf<HwndSource>();
            await Assert.That(observer.Error).IsNull();
        }
        finally
        {
            subscription?.Dispose();
            window.Close();
        }
    }

    /// <summary>Verifies every Windows Forms extension overload validates its required arguments eagerly.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task FormExtensionOverloadsRejectMissingArgumentsAsync()
    {
        Form missingForm = null;
        Func<bool> missingCondition = null;
        using var form = new Form();

        await Assert.That(() => missingForm.BlockMove()).Throws<ArgumentNullException>();
        await Assert.That(() => missingForm.BlockMove(static () => false)).Throws<ArgumentNullException>();
        await Assert.That(() => missingForm.BlockMoveAndResize()).Throws<ArgumentNullException>();
        await Assert.That(() => missingForm.BlockMoveAndResize(static () => false)).Throws<ArgumentNullException>();
        await Assert.That(() => form.BlockMove(missingCondition)).Throws<ArgumentNullException>();
        await Assert.That(() => form.BlockMoveAndResize(missingCondition)).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies Windows Forms extensions attach before handle creation and retain the guard across handle recreation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task FormExtensionRetainsMoveGuardAcrossHandleRecreationAsync()
    {
        using var form = new RecreatingForm();
        var evaluationCount = 0;
        var observer = new RecordingObserver<TestRxVoid>();
        using IDisposable subscription = form.BlockMove(() =>
        {
            evaluationCount++;
            return false;
        }).Subscribe(observer);

        await Assert.That(form.IsHandleCreated).IsFalse();

        _ = User32.User32Api.SendMessage(
            form.Handle,
            WindowsMessages.WM_SYSCOMMAND,
            (nint)MoveSystemCommand,
            IntPtr.Zero);
        form.RecreateHandleForTesting();
        _ = User32.User32Api.SendMessage(
            form.Handle,
            WindowsMessages.WM_SYSCOMMAND,
            (nint)MoveSystemCommand,
            IntPtr.Zero);

        await Assert.That(evaluationCount).IsEqualTo(ExpectedMoveEvaluationCount);
        await Assert.That(observer.Error).IsNull();
    }

    /// <summary>Verifies the move-and-resize extension attaches immediately to an existing Windows Forms handle.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task FormExtensionBlocksResizeOnInitializedHandleAsync()
    {
        using var form = new Form();
        var evaluationCount = 0;
        var observer = new RecordingObserver<TestRxVoid>();
        _ = form.Handle;
        using IDisposable subscription = form.BlockMoveAndResize(() =>
        {
            evaluationCount++;
            return false;
        }).Subscribe(observer);

        _ = User32.User32Api.SendMessage(
            form.Handle,
            WindowsMessages.WM_SYSCOMMAND,
            (nint)ResizeSystemCommand,
            IntPtr.Zero);

        await Assert.That(evaluationCount).IsEqualTo(1);
        await Assert.That(observer.Error).IsNull();
    }

    /// <summary>Verifies unconditional Windows Forms guards can be cancelled before a native handle is created.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task FormExtensionDisposalCancelsDeferredInstallationAsync()
    {
        using var moveForm = new Form();
        using var resizeForm = new Form();
        var moveObserver = new RecordingObserver<TestRxVoid>();
        var resizeObserver = new RecordingObserver<TestRxVoid>();
        IDisposable moveSubscription = moveForm.BlockMove().Subscribe(moveObserver);
        IDisposable resizeSubscription = resizeForm.BlockMoveAndResize().Subscribe(resizeObserver);

        moveSubscription.Dispose();
        moveSubscription.Dispose();
        resizeSubscription.Dispose();
        _ = moveForm.Handle;
        _ = resizeForm.Handle;

        await Assert.That(moveObserver.Error).IsNull();
        await Assert.That(resizeObserver.Error).IsNull();
    }

    /// <summary>Verifies subscribing a guard to a disposed Windows Forms form reports an observable error.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DisposedFormReportsSetupFailureAsync()
    {
        var form = new Form();
        form.Dispose();
        var movementGuard = new WindowsMove(static () => false);
        var observer = new RecordingObserver<TestRxVoid>();

        using IDisposable subscription = movementGuard.Subscribe(observer, form);

        await Assert.That(observer.Error).IsTypeOf<ObjectDisposedException>();
    }

    /// <summary>Verifies the native listener keeps hooks when a Windows Forms handle is recreated.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinProcListenerRetainsHooksAcrossHandleRecreationAsync()
    {
        using var form = new RecreatingForm();
        var listener = new WinProcListener(form);
        var callCount = 0;
        nint hook(IntPtr windowHandle, int message, IntPtr wordParameter, IntPtr longParameter, ref bool handled)
        {
            _ = windowHandle;
            _ = wordParameter;
            _ = longParameter;
            if ((WindowsMessages)message == WindowsMessages.WM_APP)
            {
                callCount++;
                handled = true;
            }

            return IntPtr.Zero;
        }

        listener.AddHook(hook);
        _ = User32.User32Api.SendMessage(form.Handle, WindowsMessages.WM_APP, IntPtr.Zero, IntPtr.Zero);
        form.RecreateHandleForTesting();
        _ = User32.User32Api.SendMessage(form.Handle, WindowsMessages.WM_APP, IntPtr.Zero, IntPtr.Zero);
        listener.Dispose();
        listener.Dispose();
        listener.AddHook(hook);
        listener.RemoveHook(hook);
        var disposalForm = new Form();
        using var disposalListener = new WinProcListener(disposalForm);
        disposalForm.Dispose();

        await Assert.That(callCount).IsEqualTo(ExpectedMoveEvaluationCount);
        await Assert.That(listener.IsDisposed).IsTrue();
        await Assert.That(disposalListener.IsDisposed).IsTrue();
    }

    /// <summary>A deterministic message hook source that does not require a native window.</summary>
    private sealed class DeterministicHookSource : IWindowMessageHookSource
    {
        /// <summary>The installed message hook.</summary>
        private HwndSourceHook _hook;

        /// <inheritdoc />
        public event EventHandler Disposed;

        /// <inheritdoc />
        public long Handle => 1L;

        /// <inheritdoc />
        public bool IsDisposed => false;

        /// <summary>Gets the number of installed hooks.</summary>
        public int AddedHooks { get; private set; }

        /// <summary>Gets the number of removed hooks.</summary>
        public int RemovedHooks { get; private set; }

        /// <inheritdoc />
        public void AddHook(HwndSourceHook hook)
        {
            AddedHooks++;
            _hook = hook;
        }

        /// <summary>Publishes one deterministic window message.</summary>
        /// <param name="message">The message identifier.</param>
        /// <param name="wordParameter">The message word parameter.</param>
        /// <returns>The hook result and handled state.</returns>
        public (IntPtr Result, bool Handled) Publish(int message, int wordParameter)
        {
            GC.KeepAlive(Disposed);
            var handled = false;
            IntPtr result = _hook(IntPtr.Zero, message, (nint)wordParameter, IntPtr.Zero, ref handled);
            return (result, handled);
        }

        /// <inheritdoc />
        public void RemoveHook(HwndSourceHook hook)
        {
            if (_hook == hook)
            {
                RemovedHooks++;
                _hook = null;
            }
        }
    }

    /// <summary>A form that exposes native handle recreation for lifecycle tests.</summary>
    private sealed class RecreatingForm : Form
    {
        /// <summary>Destroys and recreates the native form handle.</summary>
        internal void RecreateHandleForTesting() => RecreateHandle();
    }

    /// <summary>An observer that records setup errors.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    private sealed class RecordingObserver<T> : IObserver<T>
    {
        /// <summary>Gets the observed error.</summary>
        public Exception Error { get; private set; }

        /// <inheritdoc />
        public void OnCompleted()
        {
        }

        /// <inheritdoc />
        public void OnError(Exception error) => Error = error;

        /// <inheritdoc />
        public void OnNext(T value) => GC.KeepAlive(value);
    }
}
