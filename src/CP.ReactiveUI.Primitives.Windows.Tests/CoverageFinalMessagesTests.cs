// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Interop;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Final coverage tests for desktop message-window composition points.</summary>
public sealed class CoverageFinalMessagesTests
{
    /// <summary>The fake message-window handle.</summary>
    private const long WindowHandle = 0x4567;

    /// <summary>The alternate fake message-window handle.</summary>
    private const long AlternateWindowHandle = 0x5678;

    /// <summary>The fake word parameter.</summary>
    private const long WordParameter = 0x12;

    /// <summary>The fake long parameter.</summary>
    private const long LongParameter = 0x34;

    /// <summary>The fake handled message result.</summary>
    private const ulong MessageResult = 0x9876;

    /// <summary>The expected single item count.</summary>
    private const int SingleCount = 1;

    /// <summary>The expected two item count.</summary>
    private const int TwoCount = 2;

    /// <summary>Verifies the shared message-window override drives handle lifecycle and messages.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SharedMessageWindowOverridePublishesLifecycleAndMessagesAsync()
    {
        var messages = new ManualObservable<WindowMessage>();
        var handles = new ManualObservable<nint>();
        var setupHandles = new List<long>();
        var teardownHandles = new List<long>();
        var observedMessages = new List<WindowMessage>();
        using var overrideScope = SharedMessageWindow.OverrideStreamsForTesting(messages, handles, (nint)AlternateWindowHandle);

        using var handleSubscription = SharedMessageWindow.ObserveHandleChanges().Subscribe(setupHandles.Add);
        using var messageSubscription = SharedMessageWindow.ObserveWindowMessages(setupHandles.Add, teardownHandles.Add).Subscribe(observedMessages.Add);
        var message = new WindowMessage((nint)WindowHandle, WindowsMessages.WM_APP, (nint)WordParameter, (nint)LongParameter);

        handles.Publish((nint)WindowHandle);
        messages.Publish(message);
        handles.Publish(0);

        await Assert.That(SharedMessageWindow.Handle).IsEqualTo(AlternateWindowHandle);
        await Assert.That(SharedMessageWindow.NativeHandle).IsEqualTo((nint)AlternateWindowHandle);
        await Assert.That(setupHandles.Contains(WindowHandle)).IsTrue();
        await Assert.That(teardownHandles.Count).IsEqualTo(SingleCount);
        await Assert.That(teardownHandles[0]).IsEqualTo(WindowHandle);
        await Assert.That(observedMessages.Count).IsEqualTo(SingleCount);
        await Assert.That(observedMessages[0]).IsEqualTo(message);
    }

    /// <summary>Verifies the message processor returns observer-supplied handled results.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SharedMessageWindowProcessorReturnsHandledResultAsync()
    {
        var observedMessages = new List<WindowMessage>();
        var observer = new ActionObserver<WindowMessage>(message =>
        {
            observedMessages.Add(message);
            message.Handled = true;
            message.Result = MessageResult;
        });

        var result = SharedMessageWindow.ProcessWindowMessageForTesting(
            observer,
            (nint)WindowHandle,
            WindowsMessages.WM_APP,
            (nint)WordParameter,
            (nint)LongParameter);

        await Assert.That(result).IsEqualTo((nuint)MessageResult);
        await Assert.That(observedMessages.Count).IsEqualTo(SingleCount);
        await Assert.That(observedMessages[0].Hwnd).IsEqualTo(WindowHandle);
        await Assert.That(observedMessages[0].WParam).IsEqualTo(WordParameter);
        await Assert.That(observedMessages[0].LParam).IsEqualTo(LongParameter);
    }

    /// <summary>Verifies session listeners filter event types and honor pause and resume.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsSessionListenerFiltersSessionEventsAndHonorsPauseAsync()
    {
        var sessionSource = new FakeSessionMessageSource();
        var registrationCalls = new List<IntPtr>();
        var unregistrationCalls = new List<IntPtr>();
        using var operations = WindowsSessionListener.OverrideOperationsForTesting(
            sessionSource.Listen,
            (handle, _) =>
            {
                registrationCalls.Add(handle);
                return true;
            },
            handle =>
            {
                unregistrationCalls.Add(handle);
                return true;
            });

        using var listener = new WindowsSessionListener();
        var allEvents = new List<WtsSessionChangeEvents>();
        var lockEvents = new List<WtsSessionChangeEvents>();
        var logonEvents = new List<WtsSessionChangeEvents>();

        using (listener.ObserveSessionChanges().Subscribe(args => allEvents.Add(args.EventType)))
        {
            using (listener.ObserveSessionLockChanges().Subscribe(args => lockEvents.Add(args.EventType)))
            {
                using (listener.ObserveSessionLogonChanges().Subscribe(args => logonEvents.Add(args.EventType)))
                {
                    listener.Start();
                    sessionSource.Publish(WtsSessionChangeEvents.WTS_SESSION_LOCK);
                    sessionSource.Publish(WtsSessionChangeEvents.WTS_REMOTE_CONNECT);
                    sessionSource.Publish(WtsSessionChangeEvents.WTS_SESSION_LOGON);
                    listener.Start();
                    listener.Pause();
                    sessionSource.Publish(WtsSessionChangeEvents.WTS_SESSION_UNLOCK);
                    listener.Resume();
                    sessionSource.Publish(WtsSessionChangeEvents.WTS_SESSION_LOGOFF);
                    listener.Stop();
                }
            }
        }

        await Assert.That(registrationCalls.Count).IsEqualTo(SingleCount);
        await Assert.That(registrationCalls[0]).IsEqualTo((IntPtr)WindowHandle);
        await Assert.That(unregistrationCalls.Count).IsEqualTo(SingleCount);
        await Assert.That(unregistrationCalls[0]).IsEqualTo((IntPtr)WindowHandle);
        await Assert.That(allEvents.Contains(WtsSessionChangeEvents.WTS_SESSION_LOCK)).IsTrue();
        await Assert.That(allEvents.Contains(WtsSessionChangeEvents.WTS_REMOTE_CONNECT)).IsTrue();
        await Assert.That(allEvents.Contains(WtsSessionChangeEvents.WTS_SESSION_LOGON)).IsTrue();
        await Assert.That(allEvents.Contains(WtsSessionChangeEvents.WTS_SESSION_UNLOCK)).IsFalse();
        await Assert.That(allEvents.Contains(WtsSessionChangeEvents.WTS_SESSION_LOGOFF)).IsTrue();
        await Assert.That(lockEvents.Count).IsEqualTo(SingleCount);
        await Assert.That(lockEvents[0]).IsEqualTo(WtsSessionChangeEvents.WTS_SESSION_LOCK);
        await Assert.That(logonEvents.Count).IsEqualTo(TwoCount);
        await Assert.That(logonEvents[0]).IsEqualTo(WtsSessionChangeEvents.WTS_SESSION_LOGON);
        await Assert.That(logonEvents[1]).IsEqualTo(WtsSessionChangeEvents.WTS_SESSION_LOGOFF);
    }

    /// <summary>Verifies session registration failures surface during subscription.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsSessionListenerThrowsWhenRegistrationFailsAsync()
    {
        var sessionSource = new FakeSessionMessageSource();
        using var operations = WindowsSessionListener.OverrideOperationsForTesting(
            sessionSource.Listen,
            static (_, _) => false,
            static _ => true);
        using var listener = new WindowsSessionListener();

        await Assert.That(() => listener.ObserveSessionChanges().Subscribe(static _ => { })).Throws<InvalidOperationException>();
    }

    /// <summary>Verifies abstract WPF hook-source messages publish and clean up hooks.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinProcWindowsHookSourcePublishesCompletesAndDisposesAsync()
    {
        var hookSource = new FakeWindowMessageHookSource();
        var observedMessages = new List<WindowMessageInfo>();
        var completed = false;
        var disposedState = 0L;

        using (WinProcWindowsExtensions
            .ObserveWindowMessages(hookSource, static handle => handle, handle => disposedState = handle)
            .Subscribe(
                observedMessages.Add,
                static _ => { },
                () => completed = true))
        {
            hookSource.Dispatch(WindowsMessages.WM_APP, WordParameter, LongParameter);
            hookSource.IsDisposed = true;
            hookSource.Dispatch(WindowsMessages.WM_DESTROY, 0, 0);
        }

        await Assert.That(observedMessages.Count).IsEqualTo(TwoCount);
        await Assert.That(observedMessages[0].Handle).IsEqualTo(WindowHandle);
        await Assert.That(observedMessages[0].Message).IsEqualTo(WindowsMessages.WM_APP);
        await Assert.That(observedMessages[0].WordParam).IsEqualTo(WordParameter);
        await Assert.That(observedMessages[0].LongParam).IsEqualTo(LongParameter);
        await Assert.That(completed).IsTrue();
        await Assert.That(disposedState).IsEqualTo(WindowHandle);
        await Assert.That(hookSource.AddHookCount).IsEqualTo(SingleCount);
        await Assert.That(hookSource.RemoveHookCount).IsEqualTo(SingleCount);
    }

    /// <summary>Verifies WPF window-message extension wrappers validate and defer missing HWND sources.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinProcWindowsExtensionWrappersValidateAndDeferWindowSourcesAsync()
    {
        HwndSource hwndSource = null;
        Window window = null;
        var hiddenWindow = new Window();

        await Assert.That(() => hwndSource.ObserveWindowMessages()).Throws<NotSupportedException>();
        await Assert.That(() => window.ObserveWindowMessages()).Throws<NotSupportedException>();

        using var subscription = hiddenWindow.ObserveWindowMessages().Subscribe(static _ => { });
        hiddenWindow.Close();

        await Assert.That(subscription).IsNotNull();
    }

    /// <summary>Verifies WinProcHandler manages duplicate subscriptions and teardown paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinProcHandlerUsesFactoryAndCleansHooksAsync()
    {
        var fakeWindow = new FakeMessageHandlerWindow();
        using var factory = WinProcHandler.OverrideMessageWindowFactoryForTesting(() => fakeWindow);
        var handler = new WinProcHandler();
        var callbackCount = 0;
        var disposableCount = 0;
        var hook = new WinProcHandlerHook((IntPtr windowHandle, int message, IntPtr wordParam, IntPtr longParam, ref bool handled) =>
        {
            if ((WindowsMessages)message != WindowsMessages.WM_APP)
            {
                return IntPtr.Zero;
            }

            callbackCount++;
            handled = true;
            return (nint)MessageResult;
        }) { Disposable = new CallbackDisposable(() => disposableCount++) };

        var subscription = handler.Subscribe(hook);
        var duplicateSubscription = handler.Subscribe(hook);
        var result = fakeWindow.Dispatch(WindowsMessages.WM_APP, WordParameter, LongParameter);
        duplicateSubscription.Dispose();
        subscription.Dispose();
        _ = fakeWindow.Dispatch(WindowsMessages.WM_APP, WordParameter, LongParameter);

        await Assert.That(handler.Handle).IsEqualTo(WindowHandle);
        await Assert.That(handler.MessageHandlerWindow).IsNull();
        await Assert.That(result).IsEqualTo((nint)MessageResult);
        await Assert.That(callbackCount).IsEqualTo(SingleCount);
        await Assert.That(disposableCount).IsEqualTo(SingleCount);
        await Assert.That(fakeWindow.AddHookCount).IsEqualTo(TwoCount);
        await Assert.That(fakeWindow.RemoveHookCount).IsEqualTo(SingleCount);

        var secondSubscription = handler.Subscribe(hook);
        _ = fakeWindow.Dispatch(WindowsMessages.WM_NCDESTROY, 0, 0);
        secondSubscription.Dispose();

        await Assert.That(fakeWindow.RemoveHookCount).IsEqualTo(TwoCount);
    }

    /// <summary>Verifies WinProcHandler disposes subscriptions after the hook list has been cleared.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinProcHandlerSubscriptionAfterClearRemovesNativeHookOnlyAsync()
    {
        var fakeWindow = new FakeMessageHandlerWindow();
        using var factory = WinProcHandler.OverrideMessageWindowFactoryForTesting(() => fakeWindow);
        var handler = new WinProcHandler();
        var disposableCount = 0;
        var hook = new WinProcHandlerHook(static (IntPtr windowHandle, int message, IntPtr wordParam, IntPtr longParam, ref bool handled) =>
        {
            GC.KeepAlive(windowHandle);
            GC.KeepAlive(message);
            GC.KeepAlive(wordParam);
            GC.KeepAlive(longParam);
            GC.KeepAlive(handled);
            return IntPtr.Zero;
        }) { Disposable = new CallbackDisposable(() => disposableCount++) };

        var subscription = handler.Subscribe(hook);
        handler.UnsubscribeAllHooks();
        WinProcHandler.Instance.UnsubscribeAllHooks();
        var removeCountAfterClear = fakeWindow.RemoveHookCount;

        subscription.Dispose();

        await Assert.That(fakeWindow.RemoveHookCount).IsEqualTo(removeCountAfterClear + SingleCount);
        await Assert.That(disposableCount).IsEqualTo(SingleCount);
    }

    /// <summary>Simple observable that publishes values manually.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    private sealed class ManualObservable<T> : IObservable<T>
    {
        /// <summary>The active observers.</summary>
        private readonly List<IObserver<T>> _observers = [];

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observers.Add(observer);
            return new CallbackDisposable(() => _ = _observers.Remove(observer));
        }

        /// <summary>Publishes a value to observers.</summary>
        /// <param name="value">The value to publish.</param>
        public void Publish(T value)
        {
            foreach (var observer in _observers.ToArray())
            {
                observer.OnNext(value);
            }
        }
    }

    /// <summary>Observer that forwards values to a callback.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    /// <param name="onNext">The value callback.</param>
    private sealed class ActionObserver<T>(Action<T> onNext) : IObserver<T>
    {
        /// <inheritdoc/>
        public void OnCompleted()
        {
        }

        /// <inheritdoc/>
        public void OnError(Exception error) => GC.KeepAlive(error);

        /// <inheritdoc/>
        public void OnNext(T value) => onNext(value);
    }

    /// <summary>Fake message source for session-listener tests.</summary>
    private sealed class FakeSessionMessageSource
    {
        /// <summary>The fake session id.</summary>
        private const int SessionId = 42;

        /// <summary>The active observers.</summary>
        private readonly List<IObserver<WindowMessage>> _observers = [];

        /// <summary>Creates a session-message stream.</summary>
        /// <param name="onSetup">The setup callback.</param>
        /// <param name="onTeardown">The teardown callback.</param>
        /// <returns>The fake stream.</returns>
        public IObservable<WindowMessage> Listen(Action<long> onSetup, Action<long> onTeardown) =>
            new SessionObservable(this, onSetup, onTeardown);

        /// <summary>Publishes a session event.</summary>
        /// <param name="eventType">The event type.</param>
        public void Publish(WtsSessionChangeEvents eventType)
        {
            var message = new WindowMessage(
                (nint)WindowHandle,
                WindowsMessages.WM_WTSSESSION_CHANGE,
                (nint)eventType,
                (nint)SessionId);
            foreach (var observer in _observers.ToArray())
            {
                observer.OnNext(message);
            }
        }

        /// <summary>Observable that owns one fake session registration.</summary>
        /// <param name="owner">The owner source.</param>
        /// <param name="onSetup">The setup callback.</param>
        /// <param name="onTeardown">The teardown callback.</param>
        private sealed class SessionObservable(FakeSessionMessageSource owner, Action<long> onSetup, Action<long> onTeardown) : IObservable<WindowMessage>
        {
            /// <inheritdoc/>
            public IDisposable Subscribe(IObserver<WindowMessage> observer)
            {
                onSetup(WindowHandle);
                owner._observers.Add(observer);
                return new CallbackDisposable(() =>
                {
                    _ = owner._observers.Remove(observer);
                    onTeardown(WindowHandle);
                });
            }
        }
    }

    /// <summary>Fake WPF hook source.</summary>
    private sealed class FakeWindowMessageHookSource : IWindowMessageHookSource
    {
        /// <summary>The registered hook.</summary>
        private HwndSourceHook _hook;

        /// <inheritdoc/>
        public event EventHandler Disposed;

        /// <inheritdoc/>
        public long Handle => WindowHandle;

        /// <inheritdoc/>
        public bool IsDisposed { get; set; }

        /// <summary>Gets the add-hook call count.</summary>
        public int AddHookCount { get; private set; }

        /// <summary>Gets the remove-hook call count.</summary>
        public int RemoveHookCount { get; private set; }

        /// <inheritdoc/>
        public void AddHook(HwndSourceHook hook)
        {
            AddHookCount++;
            _hook = hook;
        }

        /// <summary>Dispatches a message through the registered hook.</summary>
        /// <param name="message">The message.</param>
        /// <param name="wordParameter">The word parameter.</param>
        /// <param name="longParameter">The long parameter.</param>
        public void Dispatch(WindowsMessages message, long wordParameter, long longParameter)
        {
            var handled = false;
            _ = _hook((IntPtr)WindowHandle, (int)message, (IntPtr)wordParameter, (IntPtr)longParameter, ref handled);
            if (IsDisposed)
            {
                Disposed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc/>
        public void RemoveHook(HwndSourceHook hook)
        {
            RemoveHookCount++;
            if (_hook == hook)
            {
                _hook = null;
            }
        }
    }

    /// <summary>Fake message-handler window.</summary>
    private sealed class FakeMessageHandlerWindow : IMessageHandlerWindow
    {
        /// <summary>The registered hooks.</summary>
        private readonly List<HwndSourceHook> _hooks = [];

        /// <inheritdoc/>
        public event EventHandler Disposed;

        /// <inheritdoc/>
        public long Handle => WindowHandle;

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <inheritdoc/>
        public HwndSource Source => null;

        /// <summary>Gets the add-hook call count.</summary>
        public int AddHookCount { get; private set; }

        /// <summary>Gets the remove-hook call count.</summary>
        public int RemoveHookCount { get; private set; }

        /// <inheritdoc/>
        public void AddHook(HwndSourceHook hook)
        {
            AddHookCount++;
            _hooks.Add(hook);
        }

        /// <summary>Dispatches a message through registered hooks.</summary>
        /// <param name="message">The message.</param>
        /// <param name="wordParameter">The word parameter.</param>
        /// <param name="longParameter">The long parameter.</param>
        /// <returns>The first handled result, or zero.</returns>
        public IntPtr Dispatch(WindowsMessages message, long wordParameter, long longParameter)
        {
            foreach (var hook in _hooks.ToArray())
            {
                var handled = false;
                var result = hook((IntPtr)WindowHandle, (int)message, (IntPtr)wordParameter, (IntPtr)longParameter, ref handled);
                if (message == WindowsMessages.WM_NCDESTROY)
                {
                    IsDisposed = true;
                    Disposed?.Invoke(this, EventArgs.Empty);
                }

                if (handled)
                {
                    return result;
                }
            }

            return IntPtr.Zero;
        }

        /// <inheritdoc/>
        public void RemoveHook(HwndSourceHook hook)
        {
            RemoveHookCount++;
            _ = _hooks.Remove(hook);
        }
    }

    /// <summary>Disposable that invokes a callback once.</summary>
    /// <param name="callback">The callback to invoke.</param>
    private sealed class CallbackDisposable(Action callback) : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose() => callback();
    }
}
