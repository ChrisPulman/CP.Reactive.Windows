// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using ReactiveUI.Primitives.Disposables;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Reactive access to RawInput.</summary>
public static class RawInputMonitor
{
    /// <summary>Raw-input monitor infrastructure shared by the raw-input monitor types.</summary>
    internal static class Infrastructure
    {
        /// <summary>The default shared message source.</summary>
        private static readonly IRawInputMessageSource DefaultMessageSource = new SharedRawInputMessageSource();

        /// <summary>The active message source.</summary>
        private static IRawInputMessageSource _messageSource = DefaultMessageSource;

        /// <summary>Gets the active raw-input message source.</summary>
        internal static IRawInputMessageSource MessageSource => _messageSource;

        /// <summary>Overrides the raw-input message source for deterministic tests.</summary>
        /// <param name="messageSource">The replacement source.</param>
        /// <returns>A scope that restores the previous source.</returns>
        internal static IDisposable OverrideMessageSourceForTesting(IRawInputMessageSource messageSource)
        {
            CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(messageSource);
            IRawInputMessageSource messageSource2 = _messageSource;
            _messageSource = messageSource;
            ResetMonitorCaches();
            return Scope.Create(messageSource2, delegate(IRawInputMessageSource source)
            {
                _messageSource = source;
                ResetMonitorCaches();
            });
        }

        /// <summary>Resets raw-input monitor caches.</summary>
        private static void ResetMonitorCaches()
        {
            ResetForTesting();
            RawInputDeviceMonitor.ResetForTesting();
        }
    }

    /// <summary>Production raw-input message source backed by the shared message window.</summary>
    private sealed class SharedRawInputMessageSource : IRawInputMessageSource
    {
        /// <inheritdoc />
        public IObservable<WindowMessage> Messages => SharedMessageWindow.WindowMessageEvents;

        /// <inheritdoc />
        public IObservable<long> ObserveHandleChanges() => SharedMessageWindow.ObserveHandleChanges();
    }

    /// <summary>The cached shared raw-input observable.</summary>
    private static IObservable<RawInputEventArgs> _rawInputObservable;

    /// <summary>
    /// Gets the shared observable for Raw Input.
    /// Multiple subscribers will share the same underlying hook, and the hook
    /// is automatically disposed when the subscriber count reaches zero.
    /// </summary>
    /// <param name="devices">The raw input device types to register.</param>
    /// <returns>A shared stream of raw input events.</returns>
    public static IObservable<RawInputEventArgs> ObserveRawInput(params RawInputDevices[] devices)
    {
        if (_rawInputObservable is not null)
        {
            return _rawInputObservable;
        }

        _rawInputObservable = ReactiveSignal.CreateSafe(delegate(IObserver<RawInputEventArgs> observer)
        {
            IDisposable messageSubscription = Infrastructure.MessageSource.Messages.Where((windowsMessage) => windowsMessage.Msg == WindowsMessages.WM_INPUT).Subscribe(delegate(WindowMessage windowsMessage)
            {
                windowsMessage.Handled = true;
                int dataSize = Marshal.SizeOf<RawInput>();
                if (RawInputApi.GetRawInputData((nint)windowsMessage.LParam, RawInputDataCommands.Input, out var data, ref dataSize, Marshal.SizeOf<RawInputHeader>()) != -1)
                {
                    observer.OnNext(new RawInputEventArgs { IsForeground = (checked((int)windowsMessage.WParam) == 0), RawInput = data });
                }
            });
            IDisposable registrationSubscription = (from windowHandle in Infrastructure.MessageSource.ObserveHandleChanges()
                where windowHandle != 0
                select windowHandle).Take(1).Subscribe(delegate(long windowHandle)
            {
                RawInputApi.RegisterRawInput((nint)windowHandle, RawInputDeviceFlags.InputSink | RawInputDeviceFlags.DeviceNotify, devices);
            });
            return new ActionDisposable(delegate
            {
                _rawInputObservable = null;
                registrationSubscription.Dispose();
                messageSubscription.Dispose();
            });
        }).Publish().RefCount();
        return _rawInputObservable;
    }

    /// <summary>Resets the cached raw-input observable for deterministic tests.</summary>
    internal static void ResetForTesting() => _rawInputObservable = null;
}
