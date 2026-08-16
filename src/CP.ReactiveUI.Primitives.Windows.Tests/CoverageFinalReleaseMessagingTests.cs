// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Final deterministic coverage tests for the Windows messaging composition boundaries.</summary>
public sealed class CoverageFinalReleaseMessagingTests
{
    /// <summary>Defines the active shared-window handle.</summary>
    private const int ActiveSharedWindowHandle = 42;

    /// <summary>Defines the default dispatch long parameter.</summary>
    private const int DefaultDispatchLongParameter = 3;

    /// <summary>Defines the default dispatch window handle.</summary>
    private const int DefaultDispatchWindowHandle = 1;

    /// <summary>Defines the default dispatch word parameter.</summary>
    private const int DefaultDispatchWordParameter = 2;

    /// <summary>Defines the handled message result.</summary>
    private const int HandledMessageResult = 99;

    /// <summary>Defines the hook long parameter.</summary>
    private const int HookLongParameter = 34;

    /// <summary>Defines the hook word parameter.</summary>
    private const int HookWordParameter = 12;

    /// <summary>Defines the deterministic registered-message identifier.</summary>
    private const int RegisteredMessageId = 0xC001;

    /// <summary>Defines the session registration flags.</summary>
    private const int SessionRegistrationFlags = 6;

    /// <summary>Defines the session registration window handle.</summary>
    private const int SessionRegistrationWindowHandle = 5;

    /// <summary>Defines the session unregistration window handle.</summary>
    private const int SessionUnregistrationWindowHandle = 7;

    /// <summary>Defines the shared-window override handle.</summary>
    private const int SharedOverrideWindowHandle = 77;

    /// <summary>Defines the deterministic registered-message name.</summary>
    private const string RegisteredMessageName = "CP.ReactiveUI.Primitives.Windows.Tests.Message";

    /// <summary>Verifies abstract hook sources support absent lifecycle callbacks and completion notification.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task HookSourceSupportsOptionalCallbacksAndDisposedEventAsync()
    {
        var source = new DeterministicHookSource();
        var values = new List<WindowMessageInfo>();
        var completed = false;

        using (WinProcWindowsExtensions
            .ObserveWindowMessages<long>(source, null, null)
            .Subscribe(values.Add, static _ => { }, () => completed = true))
        {
            source.Publish(WindowsMessages.WM_APP, HookWordParameter, HookLongParameter);
            source.PublishDisposed();
        }

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0].Message).IsEqualTo(WindowsMessages.WM_APP);
        await Assert.That(completed).IsTrue();
        await Assert.That(source.RemovedHooks).IsEqualTo(1);
    }

    /// <summary>Verifies shared-message lifecycle handles inactive and active transitions without a native window.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SharedMessageWindowLifecycleHandlesAllDeterministicTransitionsAsync()
    {
        var messages = new DeterministicObservable<WindowMessage>();
        var handles = new DeterministicObservable<nint>();
        var setup = new List<long>();
        var teardown = new List<long>();

        await Assert.That(SharedMessageWindow.Handle).IsGreaterThanOrEqualTo(0L);
        using (SharedMessageWindow.OverrideStreamsForTesting(messages, handles, SharedOverrideWindowHandle))
        {
            await Assert.That(SharedMessageWindow.Handle).IsEqualTo((long)SharedOverrideWindowHandle);
            using (SharedMessageWindow.ObserveWindowMessages(setup.Add, teardown.Add).Subscribe(static _ => { }))
            {
                handles.Publish(0);
                handles.Publish((nint)ActiveSharedWindowHandle);
                handles.Publish(0);
            }
        }

        await Assert.That(setup.Count).IsEqualTo(1);
        await Assert.That(setup[0]).IsEqualTo((long)ActiveSharedWindowHandle);
        await Assert.That(teardown.Count).IsEqualTo(1);
        await Assert.That(teardown[0]).IsEqualTo((long)ActiveSharedWindowHandle);
    }

    /// <summary>Verifies handled, default, and destroy message dispatch use composed operations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SharedMessageWindowDispatchUsesDeterministicOperationsAsync()
    {
        var defaultCalls = 0;
        var quitCode = -1;
        using var operations = SharedMessageWindow.OverrideMessageDispatchForTesting(
            (windowHandle, message, wordParameter, longParameter) =>
            {
                defaultCalls++;
                return (nuint)(windowHandle + (nint)message + wordParameter + longParameter);
            },
            exitCode => quitCode = exitCode);
        var handledObserver = new DelegatingObserver<WindowMessage>(static message =>
        {
            message.Handled = true;
            message.Result = HandledMessageResult;
        });
        var unhandledObserver = new DelegatingObserver<WindowMessage>(static _ => { });

        var handled = SharedMessageWindow.ProcessWindowMessageForTesting(
            handledObserver,
            DefaultDispatchWindowHandle,
            WindowsMessages.WM_APP,
            DefaultDispatchWordParameter,
            DefaultDispatchLongParameter);
        var unhandled = SharedMessageWindow.ProcessWindowMessageForTesting(
            unhandledObserver,
            DefaultDispatchWindowHandle,
            WindowsMessages.WM_APP,
            DefaultDispatchWordParameter,
            DefaultDispatchLongParameter);
        var destroyed = SharedMessageWindow.ProcessWindowMessageForTesting(
            unhandledObserver,
            DefaultDispatchWindowHandle,
            WindowsMessages.WM_DESTROY,
            0,
            0);

        await Assert.That(handled).IsEqualTo((nuint)HandledMessageResult);
        await Assert.That(unhandled)
            .IsEqualTo((nuint)(
                DefaultDispatchWindowHandle
                + (int)WindowsMessages.WM_APP
                + DefaultDispatchWordParameter
                + DefaultDispatchLongParameter));
        await Assert.That(destroyed).IsEqualTo(0U);
        await Assert.That(defaultCalls).IsEqualTo(1);
        await Assert.That(quitCode).IsEqualTo(0);
    }

    /// <summary>Verifies registration boundaries use deterministic composed native operations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MessageRegistrationBoundariesUseComposedOperationsAsync()
    {
        var registeredName = string.Empty;
        var registeredHandle = IntPtr.Zero;
        var registeredFlags = -1;
        var unregisteredHandle = IntPtr.Zero;
        using var messageRegistration = WindowsMessage.OverrideRegistrationForTesting(name =>
        {
            registeredName = name;
            return RegisteredMessageId;
        });
        using var sessionRegistration = WindowsSessionListenerNativeMethods.OverrideForTesting(
            (windowHandle, flags) =>
            {
                registeredHandle = windowHandle;
                registeredFlags = flags;
                return true;
            },
            windowHandle =>
            {
                unregisteredHandle = windowHandle;
                return false;
            });

        var registeredMessage = WindowsMessage.RegisterWindowsMessage(RegisteredMessageName);
        var registrationResult = WindowsSessionListenerNativeMethods.WtsRegisterSessionNotification(
            (nint)SessionRegistrationWindowHandle,
            SessionRegistrationFlags);
        var unregistrationResult = WindowsSessionListenerNativeMethods.WtsUnRegisterSessionNotification(
            (nint)SessionUnregistrationWindowHandle);

        await Assert.That(registeredMessage).IsEqualTo((uint)RegisteredMessageId);
        await Assert.That(registeredName).IsEqualTo(RegisteredMessageName);
        await Assert.That(registrationResult).IsTrue();
        await Assert.That(registeredHandle).IsEqualTo((nint)SessionRegistrationWindowHandle);
        await Assert.That(registeredFlags).IsEqualTo(SessionRegistrationFlags);
        await Assert.That(unregistrationResult).IsFalse();
        await Assert.That(unregisteredHandle).IsEqualTo((nint)SessionUnregistrationWindowHandle);
    }

    /// <summary>Observable that exposes deterministic values to subscribers.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    private sealed class DeterministicObservable<T> : IObservable<T>
    {
        /// <summary>The subscribed observers.</summary>
        private readonly List<IObserver<T>> _observers = [];

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observers.Add(observer);
            return new ReleaseSubscription<T>(_observers, observer);
        }

        /// <summary>Publishes a deterministic value.</summary>
        /// <param name="value">The value to publish.</param>
        public void Publish(T value)
        {
            foreach (IObserver<T> observer in _observers.ToArray())
            {
                observer.OnNext(value);
            }
        }
    }

    /// <summary>Deterministic WPF hook source without a native window.</summary>
    private sealed class DeterministicHookSource : IWindowMessageHookSource
    {
        /// <summary>Defines the deterministic hook window handle.</summary>
        private const long DeterministicHookWindowHandle = 71L;

        /// <summary>The installed hook.</summary>
        private HwndSourceHook _hook;

        /// <inheritdoc/>
        public event EventHandler Disposed;

        /// <inheritdoc/>
        public long Handle => DeterministicHookWindowHandle;

        /// <inheritdoc/>
        public bool IsDisposed => false;

        /// <summary>Gets the number of removed hooks.</summary>
        public int RemovedHooks { get; private set; }

        /// <inheritdoc/>
        public void AddHook(HwndSourceHook hook) => _hook = hook;

        /// <summary>Publishes a message through the installed hook.</summary>
        /// <param name="message">The message type.</param>
        /// <param name="wordParameter">The word parameter.</param>
        /// <param name="longParameter">The long parameter.</param>
        public void Publish(WindowsMessages message, long wordParameter, long longParameter)
        {
            var handled = false;
            _ = _hook((nint)Handle, (int)message, (nint)wordParameter, (nint)longParameter, ref handled);
        }

        /// <summary>Publishes the deterministic disposal event.</summary>
        public void PublishDisposed() => Disposed?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public void RemoveHook(HwndSourceHook hook)
        {
            RemovedHooks++;
            if (_hook == hook)
            {
                _hook = null;
            }
        }
    }

    /// <summary>Observer that delegates notifications to a callback.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    /// <param name="onNext">The value callback.</param>
    private sealed class DelegatingObserver<T>(Action<T> onNext) : IObserver<T>
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

    /// <summary>Subscription that removes one observer.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    /// <param name="observers">The observer collection to update.</param>
    /// <param name="observer">The observer to remove when disposed.</param>
    private sealed class ReleaseSubscription<T>(List<IObserver<T>> observers, IObserver<T> observer) : IDisposable
    {
        /// <summary>The observer to remove when disposed.</summary>
        private readonly IObserver<T> _observer = observer;

        /// <summary>The observer collection to update.</summary>
        private readonly List<IObserver<T>> _observers = observers;

        /// <inheritdoc/>
        public void Dispose() => _ = _observers.Remove(_observer);
    }
}
