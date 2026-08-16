// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Additional coverage for DWM and desktop messaging components.</summary>
public sealed class DwmMessagingCoverageTests
{
    /// <summary>The first test window handle.</summary>
    private const long TestHandle = 0x1234;

    /// <summary>The test word parameter.</summary>
    private const long TestWordParameter = 0x5678;

    /// <summary>The test long parameter.</summary>
    private const long TestLongParameter = 0x9ABC;

    /// <summary>The test message result.</summary>
    private const ulong TestResult = 0x12345678;

    /// <summary>The blur test region handle.</summary>
    private const int BlurRegionHandle = 42;

    /// <summary>The native true value.</summary>
    private const int NativeTrue = 1;

    /// <summary>The thumbnail opacity test value.</summary>
    private const byte ThumbnailOpacity = 200;

    /// <summary>The different thumbnail opacity test value.</summary>
    private const byte DifferentThumbnailOpacity = 199;

    /// <summary>The invalid-handle thumbnail opacity test value.</summary>
    private const byte InvalidHandleThumbnailOpacity = 128;

    /// <summary>The first rectangle value.</summary>
    private const int RectValue1 = 1;

    /// <summary>The second rectangle value.</summary>
    private const int RectValue2 = 2;

    /// <summary>The third rectangle value.</summary>
    private const int RectValue3 = 3;

    /// <summary>The fourth rectangle value.</summary>
    private const int RectValue4 = 4;

    /// <summary>The fifth rectangle value.</summary>
    private const int RectValue5 = 5;

    /// <summary>The sixth rectangle value.</summary>
    private const int RectValue6 = 6;

    /// <summary>The seventh rectangle value.</summary>
    private const int RectValue7 = 7;

    /// <summary>The eighth rectangle value.</summary>
    private const int RectValue8 = 8;

    /// <summary>The timeout in seconds used when waiting for a hidden window message.</summary>
    private const int WaitTimeoutSeconds = 5;

    /// <summary>The delay in milliseconds between lifecycle checks.</summary>
    private const int PollDelayMilliseconds = 20;

    /// <summary>The timeout used when waiting for a hidden window message.</summary>
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(WaitTimeoutSeconds);

    /// <summary>Verifies DWM blur-behind property tracking and native conversion.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DwmBlurBehindTracksFlagsAndNativeBoolValuesAsync()
    {
        var blurRegion = new IntPtr(BlurRegionHandle);
        var blurBehind = new DwmBlurBehind { Enable = true, TransitionOnMaximized = true };
        blurBehind.SetBlurRegion(blurRegion);

        var same = new DwmBlurBehind { Enable = true, TransitionOnMaximized = true };
        same.SetBlurRegion(blurRegion);

        var different = new DwmBlurBehind { Enable = false };
        var native = blurBehind.ToNative();
        var sameNative = new NativeDwmBlurBehind(
            DwmBlurBehindFlags.Enable | DwmBlurBehindFlags.TransitionMaximized | DwmBlurBehindFlags.BlurRegion,
            NativeTrue,
            blurRegion,
            NativeTrue);

        native.MarkFieldsAsRead();

        await Assert.That(blurBehind.Enable).IsTrue();
        await Assert.That(blurBehind.TransitionOnMaximized).IsTrue();
        await Assert.That(blurBehind.Equals((object)same)).IsTrue();
        await Assert.That(blurBehind == same).IsTrue();
        await Assert.That(blurBehind != different).IsTrue();
        await Assert.That(native).IsEqualTo(sameNative);
        await Assert.That(native.GetHashCode()).IsEqualTo(sameNative.GetHashCode());
        await Assert.That(native.Equals((object)sameNative)).IsTrue();
    }

    /// <summary>Verifies DWM thumbnail property flag tracking and native conversion.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DwmThumbnailPropertiesTracksFlagsAndNativeBoolValuesAsync()
    {
        var destination = new NativeRect(RectValue1, RectValue2, RectValue3, RectValue4);
        var source = new NativeRect(RectValue5, RectValue6, RectValue7, RectValue8);
        var thumbnailProperties = new DwmThumbnailProperties { Destination = destination, Source = source, Opacity = ThumbnailOpacity, Visible = true, SourceClientAreaOnly = true };

        var same = new DwmThumbnailProperties { Destination = destination, Source = source, Opacity = ThumbnailOpacity, Visible = true, SourceClientAreaOnly = true };

        var different = new DwmThumbnailProperties { Opacity = DifferentThumbnailOpacity };
        var native = thumbnailProperties.ToNative();
        var sameNative = new NativeDwmThumbnailProperties(
            DwmThumbnailPropertyFlags.Destination
            | DwmThumbnailPropertyFlags.Source
            | DwmThumbnailPropertyFlags.Opacity
            | DwmThumbnailPropertyFlags.Visible
            | DwmThumbnailPropertyFlags.SourceClientAreaOnly,
            destination,
            source,
            ThumbnailOpacity,
            NativeTrue,
            NativeTrue);

        native.MarkFieldsAsRead();

        await Assert.That(thumbnailProperties.Destination).IsEqualTo(destination);
        await Assert.That(thumbnailProperties.Source).IsEqualTo(source);
        await Assert.That(thumbnailProperties.Visible).IsTrue();
        await Assert.That(thumbnailProperties.SourceClientAreaOnly).IsTrue();
        await Assert.That(thumbnailProperties.Equals((object)same)).IsTrue();
        await Assert.That(thumbnailProperties == same).IsTrue();
        await Assert.That(thumbnailProperties != different).IsTrue();
        await Assert.That(native).IsEqualTo(sameNative);
        await Assert.That(native.GetHashCode()).IsEqualTo(sameNative.GetHashCode());
        await Assert.That(native.Equals((object)sameNative)).IsTrue();
    }

    /// <summary>Verifies DWM APIs return stable failure values for invalid handles without changing desktop state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DwmApiInvalidHandleCallsFailSafelyAsync()
    {
        var blurBehind = new DwmBlurBehind { Enable = true };
        var thumbnailProperties = new DwmThumbnailProperties { Visible = true, Opacity = InvalidHandleThumbnailOpacity };
        var sharedFormat = 0U;

        var blurResult = DwmApi.DwmEnableBlurBehindWindow(IntPtr.Zero, ref blurBehind);
        var boundsResult = DwmApi.GetExtendedFrameBounds(IntPtr.Zero, out var bounds);
        var rectangleResult = DwmApi.DwmGetWindowAttribute(
            IntPtr.Zero,
            DwmWindowAttributes.ExtendedFrameBounds,
            out NativeRect rectangle,
            NativeRect.SizeOf);
        var boolResult = DwmApi.DwmGetWindowAttribute(
            IntPtr.Zero,
            DwmWindowAttributes.Cloaked,
            out bool cloaked,
            Marshal.SizeOf<int>());
        var uintResult = DwmApi.DwmGetWindowAttribute(
            IntPtr.Zero,
            DwmWindowAttributes.WindowCornerPreference,
            out uint cornerPreference,
            Marshal.SizeOf<uint>());
        var queryResult = DwmApi.DwmQueryThumbnailSourceSize(IntPtr.Zero, out var thumbnailSize);
        var registerResult = DwmApi.DwmRegisterThumbnail(IntPtr.Zero, IntPtr.Zero, out var thumbnailId);
        var updateResult = DwmApi.DwmUpdateThumbnailProperties(IntPtr.Zero, ref thumbnailProperties);
        var unregisterResult = DwmApi.DwmUnregisterThumbnail(IntPtr.Zero);
        var setResult = DwmApi.DwmSetWindowAttribute(
            IntPtr.Zero,
            DwmWindowAttributes.WindowCornerPreference,
            IntPtr.Zero,
            Marshal.SizeOf<uint>());
        var sharedResult = DwmApi.GetSharedSurface(IntPtr.Zero, 0, 0, 0, ref sharedFormat, out var sharedHandle, 0);
        var updateSharedResult = DwmApi.UpdateWindowShared(IntPtr.Zero, 0, 0, 0, IntPtr.Zero, IntPtr.Zero);

        await Assert.That(blurResult.Succeeded()).IsFalse();
        await Assert.That(boundsResult).IsFalse();
        await Assert.That(bounds).IsEqualTo(NativeRect.Empty);
        await Assert.That(rectangleResult.Succeeded()).IsFalse();
        await Assert.That(rectangle).IsEqualTo(NativeRect.Empty);
        await Assert.That(boolResult.Succeeded()).IsFalse();
        await Assert.That(cloaked).IsFalse();
        await Assert.That(uintResult.Succeeded()).IsFalse();
        await Assert.That(cornerPreference).IsEqualTo(0U);
        await Assert.That(queryResult.Succeeded()).IsFalse();
        await Assert.That(thumbnailSize).IsEqualTo(default);
        await Assert.That(registerResult.Succeeded()).IsFalse();
        await Assert.That(thumbnailId).IsEqualTo(IntPtr.Zero);
        await Assert.That(updateResult.Succeeded()).IsFalse();
        await Assert.That(unregisterResult.Succeeded()).IsFalse();
        await Assert.That(setResult.Succeeded()).IsFalse();
        await Assert.That(sharedResult).IsLessThanOrEqualTo(0);
        await Assert.That(sharedHandle).IsEqualTo(IntPtr.Zero);
        await Assert.That(updateSharedResult).IsLessThanOrEqualTo(0);
    }

    /// <summary>Verifies DWM convenience APIs return deterministic values for invalid handles.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DwmConvenienceMethodsHandleInvalidWindowsAsync()
    {
        var systemColor = DwmApi.ColorizationSystemDrawingColor;
        var drawingColor = DwmApi.ColorizationDrawingColor;
        var mediaColor = DwmApi.ColorizationColor;
        var isCloaked = DwmApi.IsWindowCloaked(IntPtr.Zero);
        var cornerPreference = DwmApi.GetWindowCornerPreference(IntPtr.Zero);
        var setCornerPreference = DwmApi.SetWindowCornerPreference(IntPtr.Zero, DwmWindowCornerPreference.Round);

        await Assert.That(systemColor.A != 0).IsTrue();
        await Assert.That(drawingColor.ToArgb()).IsEqualTo(systemColor.ToArgb());
        await Assert.That(mediaColor.A).IsEqualTo(systemColor.A);
        await Assert.That(mediaColor.R).IsEqualTo(systemColor.R);
        await Assert.That(mediaColor.G).IsEqualTo(systemColor.G);
        await Assert.That(mediaColor.B).IsEqualTo(systemColor.B);
        await Assert.That(isCloaked).IsFalse();
        await Assert.That(cornerPreference).IsEqualTo(DwmWindowCornerPreference.Default);
        await Assert.That(setCornerPreference).IsFalse();
    }

    /// <summary>Verifies message value objects expose native and managed values consistently.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MessageStructsExposeValuesAndEqualityAsync()
    {
        var windowMessage = new WindowMessage((nint)TestHandle, WindowsMessages.WM_APP, (nint)TestWordParameter, (nint)TestLongParameter) { Handled = true, Result = TestResult };
        var messageInfo = WindowMessageInfo.Create(TestHandle, (int)WindowsMessages.WM_APP, TestWordParameter, TestLongParameter);
        var defaultMessage = default(Msg);
        var sameDefaultMessage = default(Msg);

        await Assert.That(windowMessage.Hwnd).IsEqualTo(TestHandle);
        await Assert.That(windowMessage.Msg).IsEqualTo(WindowsMessages.WM_APP);
        await Assert.That(windowMessage.WParam).IsEqualTo(TestWordParameter);
        await Assert.That(windowMessage.LParam).IsEqualTo(TestLongParameter);
        await Assert.That(windowMessage.Handled).IsTrue();
        await Assert.That(windowMessage.Result).IsEqualTo(TestResult);
        await Assert.That(windowMessage.WindowHandle).IsEqualTo((nint)TestHandle);
        await Assert.That(windowMessage.WordParameter).IsEqualTo((nint)TestWordParameter);
        await Assert.That(windowMessage.LongParameter).IsEqualTo((nint)TestLongParameter);
        await Assert.That(messageInfo.Handle).IsEqualTo(TestHandle);
        await Assert.That(messageInfo.Message).IsEqualTo(WindowsMessages.WM_APP);
        await Assert.That(messageInfo.WordParam).IsEqualTo(TestWordParameter);
        await Assert.That(messageInfo.LongParam).IsEqualTo(TestLongParameter);
        await Assert.That(defaultMessage == sameDefaultMessage).IsTrue();
        await Assert.That(defaultMessage != sameDefaultMessage).IsFalse();
        await Assert.That(defaultMessage.Equals((object)sameDefaultMessage)).IsTrue();
        await Assert.That(defaultMessage.GetHashCode()).IsEqualTo(sameDefaultMessage.GetHashCode());
        await Assert.That(defaultMessage.Handle).IsEqualTo(0L);
        await Assert.That(defaultMessage.Message).IsEqualTo(WindowsMessages.WM_NULL);
        await Assert.That(defaultMessage.WParam).IsEqualTo(0UL);
        await Assert.That(defaultMessage.LParam).IsEqualTo(0UL);
        await Assert.That(defaultMessage.CursorPosition).IsEqualTo(default);
        await Assert.That(defaultMessage.Time <= TimeProvider.System.GetLocalNow()).IsTrue();
    }

    /// <summary>Verifies Windows message registration, name lookup, and membership helpers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsMessageHelpersResolveBuiltInAndRegisteredMessagesAsync()
    {
        var registeredName = $"CP.Reactive.Windows.Tests.{Guid.NewGuid():N}";
        var registeredMessage = WindowsMessage.RegisterWindowsMessage(registeredName);

        await Assert.That(WindowsMessage.GetWindowsMessage((uint)WindowsMessages.WM_DESTROY)).IsEqualTo(nameof(WindowsMessages.WM_DESTROY));
        await Assert.That(registeredMessage).IsGreaterThanOrEqualTo((uint)WindowsMessages.WM_APPLICATION_STRING);
        await Assert.That(WindowsMessage.GetWindowsMessage(registeredMessage)).IsEqualTo(registeredName);
        await Assert.That(WindowsMessage.GetWindowsMessage(uint.MaxValue)).IsNull();
    }

    /// <summary>Verifies the shared message window creates a hidden handle, receives posted messages, and tears down.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SharedMessageWindowReceivesPostedMessagesAndReportsLifecycleAsync()
    {
        var setupSource = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
        var teardownSource = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
        var messageSource = new TaskCompletionSource<WindowMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        long setupHandle = 0;

        var subscription = SharedMessageWindow
            .ObserveWindowMessages(
                handle =>
                {
                    setupHandle = handle;
                    _ = setupSource.TrySetResult(handle);
                },
                handle => _ = teardownSource.TrySetResult(handle))
            .Where(static message => message.Msg == WindowsMessages.WM_APP && message.WParam == TestWordParameter)
            .Subscribe(message =>
            {
                message.Handled = true;
                message.Result = TestResult;
                _ = messageSource.TrySetResult(message);
            });

        setupHandle = await WaitForTaskAsync(setupSource.Task);
        var messageResult = User32.User32Api.SendMessage((nint)setupHandle, WindowsMessages.WM_APP, (nint)TestWordParameter, (nint)TestLongParameter);
        var received = await WaitForTaskAsync(messageSource.Task);

        subscription.Dispose();
        var teardownHandle = await WaitForTaskAsync(teardownSource.Task);

        await Assert.That(messageResult).IsEqualTo((nint)TestResult);
        await Assert.That(setupHandle).IsNotEqualTo(0L);
        await Assert.That(received.Hwnd).IsEqualTo(setupHandle);
        await Assert.That(received.Msg).IsEqualTo(WindowsMessages.WM_APP);
        await Assert.That(received.WParam).IsEqualTo(TestWordParameter);
        await Assert.That(received.LParam).IsEqualTo(TestLongParameter);
        await Assert.That(received.Handled).IsTrue();
        await Assert.That(received.Result).IsEqualTo(TestResult);
        await Assert.That(teardownHandle).IsEqualTo(setupHandle);
    }

    /// <summary>Verifies shared handle observation includes setup and teardown values.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SharedMessageWindowHandleObserverPublishesCurrentAndLifecycleValuesAsync()
    {
        var observedHandles = new List<long>();
        var nonZeroHandleSource = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var handleSubscription = SharedMessageWindow.ObserveHandleChanges().Subscribe(handle =>
        {
            observedHandles.Add(handle);
            if (handle != 0)
            {
                _ = nonZeroHandleSource.TrySetResult(handle);
            }
        });

        var messageSubscription = SharedMessageWindow.ObserveWindowMessages().Subscribe(static _ => { });
        var nonZeroHandle = await WaitForTaskAsync(nonZeroHandleSource.Task);

        messageSubscription.Dispose();

        await Assert.That(nonZeroHandle).IsNotEqualTo(0L);
        await Assert.That(observedHandles.Exists(static handle => handle != 0)).IsTrue();
    }

    /// <summary>Verifies the WinForms native-window listener dispatches and removes hooks.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinProcListenerDispatchesAndRemovesHooksAsync()
    {
        using var control = new System.Windows.Forms.Control();
        control.CreateControl();

        var listener = new WinProcListener(control);
        var callCount = 0;
        nint hook(IntPtr windowHandle, int message, IntPtr wordParam, IntPtr longParam, ref bool handled)
        {
            GC.KeepAlive(windowHandle);
            GC.KeepAlive(wordParam);
            GC.KeepAlive(longParam);
            if ((WindowsMessages)message != WindowsMessages.WM_APP)
            {
                return IntPtr.Zero;
            }

            callCount++;
            handled = true;
            return (nint)TestResult;
        }

        listener.AddHook(hook);
        var result = User32.User32Api.SendMessage(control.Handle, WindowsMessages.WM_APP, (nint)TestWordParameter, (nint)TestLongParameter);
        listener.RemoveHook(hook);
        _ = User32.User32Api.SendMessage(control.Handle, WindowsMessages.WM_APP, (nint)TestWordParameter, (nint)TestLongParameter);

        await Assert.That(result).IsEqualTo((nint)TestResult);
        await Assert.That(callCount).IsEqualTo(1);
        await Assert.That(listener.IsDisposed).IsFalse();

        listener.Dispose();
        await Assert.That(listener.IsDisposed).IsTrue();
    }

    /// <summary>Verifies the WinForms observable extension forwards window messages.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinProcFormsMessagesPublishesControlMessagesAsync()
    {
        using var control = new System.Windows.Forms.Control();
        control.CreateControl();

        var observedMessages = new List<WindowMessageInfo>();
        using var subscription = control.ObserveWindowMessages()
            .Where(static message => message.Message == WindowsMessages.WM_APP)
            .Subscribe(observedMessages.Add);

        _ = User32.User32Api.SendMessage(control.Handle, WindowsMessages.WM_APP, (nint)TestWordParameter, (nint)TestLongParameter);

        await Assert.That(observedMessages.Count).IsEqualTo(1);
        await Assert.That(observedMessages[0].Handle).IsEqualTo(control.Handle.ToInt64());
        await Assert.That(observedMessages[0].WordParam).IsEqualTo(TestWordParameter);
        await Assert.That(observedMessages[0].LongParam).IsEqualTo(TestLongParameter);
    }

    /// <summary>Verifies the shared WinProcHandler creates a WPF message window and manages hook subscriptions.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinProcHandlerCreatesWindowAndUnsubscribesHooksAsync()
    {
        var handler = WinProcHandler.Instance;
        handler.UnsubscribeAllHooks();
        var callCount = 0;
        var disposableCallCount = 0;
        var hook = new WinProcHandlerHook((windowHandle, message, wordParam, longParam, ref handled) =>
        {
            if ((WindowsMessages)message != WindowsMessages.WM_APP)
            {
                return IntPtr.Zero;
            }

            callCount++;
            handled = true;
            return (nint)TestResult;
        })
        { Disposable = new CallbackDisposable(() => disposableCallCount++) };

        var subscription = handler.Subscribe(hook);
        var duplicateSubscription = handler.Subscribe(hook);
        var result = User32.User32Api.SendMessage((nint)handler.Handle, WindowsMessages.WM_APP, (nint)TestWordParameter, (nint)TestLongParameter);
        duplicateSubscription.Dispose();

        await Assert.That(handler.MessageHandlerWindow).IsNotNull();
        await Assert.That(handler.Handle).IsNotEqualTo(0L);
        await Assert.That(result).IsEqualTo((nint)TestResult);
        await Assert.That(callCount).IsEqualTo(1);
        await Assert.That(disposableCallCount).IsEqualTo(0);

        subscription.Dispose();
        _ = User32.User32Api.SendMessage((nint)handler.Handle, WindowsMessages.WM_APP, (nint)TestWordParameter, (nint)TestLongParameter);

        await Assert.That(callCount).IsEqualTo(1);
        await Assert.That(disposableCallCount).IsEqualTo(1);

        handler.UnsubscribeAllHooks();
    }

    /// <summary>Verifies the WPF HwndSource observable extension publishes messages and disposes setup state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinProcMessagesPublishesHwndSourceMessagesAndDisposesStateAsync()
    {
        using var hwndSource = WinProcHandler.CreateMessageWindow(0, $"CP.Reactive.Tests.{Guid.NewGuid():N}");
        await WaitUntilAsync(() => hwndSource.Handle != IntPtr.Zero);
        var observedMessages = new List<WindowMessageInfo>();
        var setupHandle = 0L;
        var disposedHandle = 0L;

        using (var subscription = WinProcWindowsExtensions.ObserveWindowMessages(
            null,
            hwndSource,
            handle =>
            {
                setupHandle = handle;
                return handle;
            },
            handle => disposedHandle = handle).Subscribe(observedMessages.Add))
        {
            _ = User32.User32Api.SendMessage(hwndSource.Handle, WindowsMessages.WM_APP, (nint)TestWordParameter, (nint)TestLongParameter);
        }

        await Assert.That(setupHandle).IsEqualTo(hwndSource.Handle.ToInt64());
        await Assert.That(observedMessages.Exists(static message => message.Message == WindowsMessages.WM_APP)).IsTrue();
        await Assert.That(disposedHandle).IsEqualTo(setupHandle);
    }

    /// <summary>Verifies WPF message observation rejects missing targets.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinProcMessagesThrowsWhenNoWindowOrSourceIsSuppliedAsync() =>
        await Assert.That(static () => WinProcWindowsExtensions.ObserveWindowMessages<object>(null, null, null, null)).Throws<NotSupportedException>();

    /// <summary>Waits for a task with a bounded timeout.</summary>
    /// <typeparam name="T">The task result type.</typeparam>
    /// <param name="task">The task to wait for.</param>
    /// <returns>The completed task result.</returns>
    private static async Task<T> WaitForTaskAsync<T>(Task<T> task)
    {
        var completedTask = await Task.WhenAny(task, Task.Delay(WaitTimeout));
        if (completedTask != task)
        {
            throw new TimeoutException("The expected Windows message was not observed.");
        }

        return await task;
    }

    /// <summary>Waits until a condition becomes true.</summary>
    /// <param name="condition">The condition to wait for.</param>
    /// <returns>A task representing the asynchronous wait.</returns>
    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var clock = TimeProvider.System;
        var stopAt = clock.GetUtcNow() + WaitTimeout;
#if !NETFRAMEWORK
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(PollDelayMilliseconds));
#endif
        while (!condition())
        {
            if (clock.GetUtcNow() >= stopAt)
            {
                throw new TimeoutException("The expected Windows message-window lifecycle state was not observed.");
            }

#if NETFRAMEWORK
            await Task.Delay(PollDelayMilliseconds);
#else
            _ = await timer.WaitForNextTickAsync();
#endif
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
