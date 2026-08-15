// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using CP.ReactiveUI.Primitives.Windows.Native.Security.Enums;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.Primitives.Signals;

namespace CP.ReactiveUI.Primitives.Windows.Native.Security;

/// <summary>Monitors registry keys for changes.</summary>
public static class RegistryMonitor
{
    /// <summary>Create an observable to monitor for registry changes.</summary>
    /// <param name="hive">RegistryHive</param>
    /// <param name="subKey">string</param>
    /// <returns>Observable change notifications.</returns>
    public static IObservable<RxVoid> ObserveChanges(RegistryHive hive, string subKey) => ObserveChanges(hive, subKey, null, RegistryNotifyFilter.ChangeLastSet);

    /// <summary>Create an observable to monitor for registry changes.</summary>
    /// <param name="hive">RegistryHive</param>
    /// <param name="subKey">string</param>
    /// <param name="registrationScheduler">Scheduler used to register the native wait callback.</param>
    /// <returns>Observable change notifications.</returns>
    public static IObservable<RxVoid> ObserveChanges(RegistryHive hive, string subKey, ISequencer registrationScheduler) => ObserveChanges(hive, subKey, registrationScheduler, RegistryNotifyFilter.ChangeLastSet);

    /// <summary>Create an observable to monitor for registry changes.</summary>
    /// <param name="hive">RegistryHive</param>
    /// <param name="subKey">string</param>
    /// <param name="filter">Registry change filter.</param>
    /// <returns>Observable change notifications.</returns>
    public static IObservable<RxVoid> ObserveChanges(RegistryHive hive, string subKey, RegistryNotifyFilter filter) => ObserveChanges(hive, subKey, null, filter);

    /// <summary>Create an observable to monitor for registry changes.</summary>
    /// <param name="hive">RegistryHive</param>
    /// <param name="subKey">string</param>
    /// <param name="registrationScheduler">Scheduler used to register the native wait callback.</param>
    /// <param name="filter">Registry change filter.</param>
    /// <returns>Observable change notifications.</returns>
    public static IObservable<RxVoid> ObserveChanges(RegistryHive hive, string subKey, ISequencer registrationScheduler, RegistryNotifyFilter filter) => ObserveChanges(new IntPtr((int)hive), subKey, registrationScheduler, filter);

    /// <summary>Create an observable to monitor for registry changes.</summary>
    /// <param name="key">IntPtr</param>
    /// <param name="subKey">string</param>
    /// <param name="registrationScheduler">Scheduler used to register the native wait callback.</param>
    /// <param name="filter">Registry change filter.</param>
    /// <returns>Observable change notifications.</returns>
    internal static IObservable<RxVoid> ObserveChanges(IntPtr key, string subKey, ISequencer registrationScheduler, RegistryNotifyFilter filter) => Signal.CreateWithState(new(key, subKey, registrationScheduler, filter), (RegistryObservationState state, IObserver<RxVoid> observer) => state.Subscribe(observer));

    /// <summary>State for a registry observation subscription.</summary>
    private sealed class RegistryObservationState
    {
        /// <summary>The registration scheduler.</summary>
        private readonly ISequencer _registrationScheduler;

        /// <summary>The registry hive key handle.</summary>
        private readonly IntPtr _key;

        /// <summary>The registry subkey.</summary>
        private readonly string _subKey;

        /// <summary>The registry change filter.</summary>
        private readonly RegistryNotifyFilter _filter;

        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Security.RegistryMonitor.RegistryObservationState" /> class.</summary>
        /// <param name="key">The registry hive key handle.</param>
        /// <param name="subKey">The registry subkey.</param>
        /// <param name="registrationScheduler">The registration scheduler.</param>
        /// <param name="filter">The registry change filter.</param>
        internal RegistryObservationState(IntPtr key, string subKey, ISequencer registrationScheduler, RegistryNotifyFilter filter)
        {
            _key = key;
            _subKey = subKey;
            _registrationScheduler = registrationScheduler;
            _filter = filter;
        }

        /// <summary>Subscribes an observer to registry changes.</summary>
        /// <param name="observer">The observer.</param>
        /// <returns>The subscription.</returns>
        internal IDisposable Subscribe(IObserver<RxVoid> observer)
        {
            try
            {
                if (Advapi32Api.RegOpenKeyEx(_key, _subKey, RegistryOpenOptions.None, RegistryKeySecurityAccessRights.QueryValue | RegistryKeySecurityAccessRights.EnumerateSubKeys | RegistryKeySecurityAccessRights.Notify | RegistryKeySecurityAccessRights.ReadControl, out var registryKey) != 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                IDisposable subscription = CreateKeyValuesChangedObservable(registryKey, _filter).SubscribeOn(_registrationScheduler ?? Sequencer.CurrentThread).Subscribe(observer);
                return Scope.Create(new(registryKey, subscription), delegate(RegistrySubscription state)
                {
                    state.Dispose();
                });
            }
            catch (Win32Exception error)
            {
                observer.OnError(error);
                return Scope.Create(delegate
                {
                });
            }
        }

        /// <summary>Internal method to create the value changed observable.</summary>
        /// <param name="key">Registry key handle.</param>
        /// <param name="filter">Registry change filter.</param>
        /// <returns>Observable change notifications.</returns>
        private static IObservable<RxVoid> CreateKeyValuesChangedObservable(SafeRegistryHandle key, RegistryNotifyFilter filter) => Signal.CreateWithState(new(key, filter), (KeyChangeState state, IObserver<RxVoid> observer) => state.Subscribe(observer)).Repeat();
    }

    /// <summary>State for a single registry key change wait.</summary>
    private sealed class KeyChangeState
    {
        /// <summary>The registry key handle.</summary>
        private readonly SafeRegistryHandle _key;

        /// <summary>The registry change filter.</summary>
        private readonly RegistryNotifyFilter _filter;

        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Security.RegistryMonitor.KeyChangeState" /> class.</summary>
        /// <param name="key">The registry key handle.</param>
        /// <param name="filter">The registry change filter.</param>
        internal KeyChangeState(SafeRegistryHandle key, RegistryNotifyFilter filter)
        {
            _key = key;
            _filter = filter;
        }

        /// <summary>Subscribes an observer to a single registry key change.</summary>
        /// <param name="observer">The observer.</param>
        /// <returns>The subscription.</returns>
        internal IDisposable Subscribe(IObserver<RxVoid> observer)
        {
            AutoResetEvent eventNotify = new(initialState: false);
            if (RegisterKeyChangeNotification(_key, _filter, eventNotify) != 0)
            {
                eventNotify.Dispose();
                observer.OnError(new Win32Exception(Marshal.GetLastWin32Error()));
                return Scope.Create(delegate
                {
                });
            }

            IDisposable callback = SetCallbackWhenSignalled(eventNotify, new RegistryNotification(observer).Notify);
            return Scope.Create(new(eventNotify, callback), delegate(KeyChangeSubscription state)
            {
                state.Dispose();
            });
        }

        /// <summary>Helper method to call the action when the WaitHandle is signaled.</summary>
        /// <param name="waitObject">WaitHandle</param>
        /// <param name="action">Action</param>
        /// <returns>IDisposable.</returns>
        private static IDisposable SetCallbackWhenSignalled(WaitHandle waitObject, Action action) => Scope.Create(
            ThreadPool.RegisterWaitForSingleObject(waitObject, delegate(object state, bool timedOut)
            {
                ((Action)state)();
            }, action, -1, executeOnlyOnce: true),
            delegate (RegisteredWaitHandle wait)
            {
                _ = wait.Unregister(null);
            });

        /// <summary>Registers the native key change notification.</summary>
        /// <param name="key">Registry key handle.</param>
        /// <param name="filter">Registry change filter.</param>
        /// <param name="eventNotify">Event to signal when the key changes.</param>
        /// <returns>Win32 result code.</returns>
        private static int RegisterKeyChangeNotification(SafeRegistryHandle key, RegistryNotifyFilter filter, AutoResetEvent eventNotify) => Advapi32Api.RegNotifyChangeKeyValue(key, watchSubtree: true, filter, eventNotify.SafeWaitHandle, asynchronous: true);
    }

    /// <summary>Registry notification callback state.</summary>
    private sealed class RegistryNotification
    {
        /// <summary>The observer.</summary>
        private readonly IObserver<RxVoid> _observer;

        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Security.RegistryMonitor.RegistryNotification" /> class.</summary>
        /// <param name="observer">The observer.</param>
        internal RegistryNotification(IObserver<RxVoid> observer)
        {
            _observer = observer;
        }

        /// <summary>Sends the registry change notification.</summary>
        internal void Notify()
        {
            _observer.OnNext(RxVoid.Default);
            _observer.OnCompleted();
        }
    }

    /// <summary>Registry subscription state.</summary>
    private sealed class RegistrySubscription : IDisposable
    {
        /// <summary>The registry key handle.</summary>
        private readonly SafeRegistryHandle _registryKey;

        /// <summary>The observable subscription.</summary>
        private readonly IDisposable _subscription;

        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Security.RegistryMonitor.RegistrySubscription" /> class.</summary>
        /// <param name="registryKey">The registry key handle.</param>
        /// <param name="subscription">The observable subscription.</param>
        internal RegistrySubscription(SafeRegistryHandle registryKey, IDisposable subscription)
        {
            _registryKey = registryKey;
            _subscription = subscription;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _subscription.Dispose();
            _registryKey.Dispose();
        }
    }

    /// <summary>Key change subscription state.</summary>
    private sealed class KeyChangeSubscription : IDisposable
    {
        /// <summary>The event to signal when the key changes.</summary>
        private readonly AutoResetEvent _eventNotify;

        /// <summary>The registered wait callback.</summary>
        private readonly IDisposable _callback;

        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Security.RegistryMonitor.KeyChangeSubscription" /> class.</summary>
        /// <param name="eventNotify">The event to signal when the key changes.</param>
        /// <param name="callback">The registered wait callback.</param>
        internal KeyChangeSubscription(AutoResetEvent eventNotify, IDisposable callback)
        {
            _eventNotify = eventNotify;
            _callback = callback;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _callback.Dispose();
            _eventNotify.Dispose();
        }
    }
}
