// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Ipc;

/// <summary>Provides composition-first Citrix virtual-channel IPC helpers over injected driver callbacks.</summary>
public static class CitrixVirtualChannelIpc
{
    /// <summary>Composes a DriverOpen call as a single-value observable.</summary>
    /// <param name="adapter">The injected virtual-driver adapter.</param>
    /// <param name="request">The open request.</param>
    /// <returns>An observable that emits an explicit channel lifetime and completes.</returns>
    public static IObservable<CitrixVirtualChannelSession> DriverOpen(
        ICitrixVirtualDriverAdapter adapter,
        CitrixVirtualChannelOpenRequest request)
    {
        Throw.IfNull(adapter);
        Throw.IfNull(request);
        return new OneShotObservable<CitrixVirtualChannelSession>(cancellationToken =>
        {
            var result = adapter.DriverOpen(request, cancellationToken)
                ?? throw new InvalidOperationException("The Citrix virtual-driver adapter returned no DriverOpen result.");
            return new CitrixVirtualChannelSession(adapter, result);
        });
    }

    /// <summary>Composes a DriverClose call as a single-value observable.</summary>
    /// <param name="adapter">The injected virtual-driver adapter.</param>
    /// <param name="channel">The channel to close.</param>
    /// <returns>An observable that emits the close result and completes.</returns>
    public static IObservable<CitrixVirtualChannelCloseResult> DriverClose(
        ICitrixVirtualDriverAdapter adapter,
        CitrixVirtualChannelHandle channel)
    {
        Throw.IfNull(adapter);
        return new OneShotObservable<CitrixVirtualChannelCloseResult>(cancellationToken =>
            adapter.DriverClose(channel, cancellationToken)
            ?? throw new InvalidOperationException("The Citrix virtual-driver adapter returned no DriverClose result."));
    }

    /// <summary>Composes a DriverWrite call as a single-value observable.</summary>
    /// <param name="adapter">The injected virtual-driver adapter.</param>
    /// <param name="request">The write request.</param>
    /// <returns>An observable that emits the write result and completes.</returns>
    public static IObservable<CitrixVirtualChannelWriteResult> DriverWrite(
        ICitrixVirtualDriverAdapter adapter,
        CitrixVirtualChannelWriteRequest request)
    {
        Throw.IfNull(adapter);
        Throw.IfNull(request);
        return new OneShotObservable<CitrixVirtualChannelWriteResult>(cancellationToken =>
            adapter.DriverWrite(request, cancellationToken)
            ?? throw new InvalidOperationException("The Citrix virtual-driver adapter returned no DriverWrite result."));
    }

    /// <summary>Composes a VdRegisterFeature call as a single-value observable.</summary>
    /// <param name="adapter">The injected virtual-driver adapter.</param>
    /// <param name="feature">The feature to register.</param>
    /// <returns>An observable that emits the feature registration result and completes.</returns>
    public static IObservable<CitrixVirtualChannelFeatureRegistration> VdRegisterFeature(
        ICitrixVirtualDriverAdapter adapter,
        CitrixVirtualChannelFeature feature)
    {
        Throw.IfNull(adapter);
        Throw.IfNull(feature);
        return new OneShotObservable<CitrixVirtualChannelFeatureRegistration>(cancellationToken =>
            adapter.VdRegisterFeature(feature, cancellationToken)
            ?? throw new InvalidOperationException("The Citrix virtual-driver adapter returned no VdRegisterFeature result."));
    }

    /// <summary>Gets incoming virtual-channel data from the injected adapter.</summary>
    /// <param name="adapter">The injected virtual-driver adapter.</param>
    /// <returns>The incoming data stream.</returns>
    public static IObservable<CitrixVirtualChannelData> ObserveIncomingData(ICitrixVirtualDriverAdapter adapter)
    {
        Throw.IfNull(adapter);
        return adapter.IncomingData ?? throw new InvalidOperationException("The Citrix virtual-driver adapter returned no incoming data stream.");
    }

    /// <summary>Gets incoming virtual-channel data for a specific channel from the injected adapter.</summary>
    /// <param name="adapter">The injected virtual-driver adapter.</param>
    /// <param name="channel">The channel to observe.</param>
    /// <returns>The incoming data stream for the channel.</returns>
    public static IObservable<CitrixVirtualChannelData> ObserveIncomingData(
        ICitrixVirtualDriverAdapter adapter,
        CitrixVirtualChannelHandle channel) =>
        new FilteredObservable<CitrixVirtualChannelData>(
            ObserveIncomingData(adapter),
            data => data.Channel == channel);

    /// <summary>Gets virtual-channel lifecycle and diagnostic events from the injected adapter.</summary>
    /// <param name="adapter">The injected virtual-driver adapter.</param>
    /// <returns>The event stream.</returns>
    public static IObservable<CitrixVirtualChannelEvent> ObserveEvents(ICitrixVirtualDriverAdapter adapter)
    {
        Throw.IfNull(adapter);
        return adapter.Events ?? throw new InvalidOperationException("The Citrix virtual-driver adapter returned no event stream.");
    }

    /// <summary>Gets virtual-channel lifecycle and diagnostic events for a specific channel from the injected adapter.</summary>
    /// <param name="adapter">The injected virtual-driver adapter.</param>
    /// <param name="channel">The channel to observe.</param>
    /// <returns>The event stream for the channel.</returns>
    public static IObservable<CitrixVirtualChannelEvent> ObserveEvents(
        ICitrixVirtualDriverAdapter adapter,
        CitrixVirtualChannelHandle channel) =>
        new FilteredObservable<CitrixVirtualChannelEvent>(
            ObserveEvents(adapter),
            data => data.Channel == channel);

    /// <summary>Represents a cancellable single-value observable.</summary>
    /// <typeparam name="T">The emitted value type.</typeparam>
    private sealed class OneShotObservable<T> : IObservable<T>
    {
        /// <summary>The operation to invoke on subscribe.</summary>
        private readonly Func<CancellationToken, T> _operation;

        /// <summary>Initializes a new instance of the <see cref="OneShotObservable{T}"/> class.</summary>
        /// <param name="operation">The operation to invoke on subscribe.</param>
        internal OneShotObservable(Func<CancellationToken, T> operation)
        {
            Throw.IfNull(operation);
            _operation = operation;
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<T> observer)
        {
            Throw.IfNull(observer);
            var subscription = new OneShotSubscription();
            _ = Task.Run(
                () => Execute(observer, subscription),
                CancellationToken.None);
            return subscription;
        }

        /// <summary>Executes the operation away from the subscribing thread.</summary>
        /// <param name="observer">The observer receiving the result.</param>
        /// <param name="subscription">The cancellable subscription.</param>
        private void Execute(IObserver<T> observer, OneShotSubscription subscription)
        {
            try
            {
                var value = _operation(subscription.Token);
                if (!subscription.IsCancellationRequested)
                {
                    observer.OnNext(value);
                    observer.OnCompleted();
                }
            }
            catch (OperationCanceledException) when (subscription.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                if (!subscription.IsCancellationRequested)
                {
                    observer.OnError(ex);
                }
            }
            finally
            {
                subscription.Complete();
            }
        }

        /// <summary>Owns cancellation for one operation.</summary>
        private sealed class OneShotSubscription : IDisposable
        {
            /// <summary>The cancellation source.</summary>
            private readonly CancellationTokenSource _cancellation = new();

            /// <summary>Serializes cancellation and completion.</summary>
            private readonly Lock _gate = new();

            /// <summary>Tracks whether the operation has completed.</summary>
            private int _completed;

            /// <summary>Tracks whether the subscription was disposed.</summary>
            private int _disposed;

            /// <summary>Gets the operation token.</summary>
            internal CancellationToken Token => _cancellation.Token;

            /// <summary>Gets a value indicating whether cancellation was requested.</summary>
            internal bool IsCancellationRequested => _cancellation.IsCancellationRequested;

            /// <inheritdoc />
            public void Dispose()
            {
                lock (_gate)
                {
                    if (_disposed != 0)
                    {
                        return;
                    }

                    _disposed = 1;
                    if (_completed == 0)
                    {
                        _cancellation.Cancel();
                        return;
                    }

                    _cancellation.Dispose();
                }
            }

            /// <summary>Marks the operation complete and releases its cancellation source.</summary>
            internal void Complete()
            {
                lock (_gate)
                {
                    if (_completed == 0)
                    {
                        _completed = 1;
                        _cancellation.Dispose();
                    }
                }
            }
        }
    }

    /// <summary>Filters an observable without requiring System.Reactive.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    private sealed class FilteredObservable<T> : IObservable<T>
    {
        /// <summary>The source observable.</summary>
        private readonly IObservable<T> _source;

        /// <summary>The predicate used to pass values.</summary>
        private readonly Func<T, bool> _predicate;

        /// <summary>Initializes a new instance of the <see cref="FilteredObservable{T}"/> class.</summary>
        /// <param name="source">The source observable.</param>
        /// <param name="predicate">The predicate used to pass values.</param>
        internal FilteredObservable(IObservable<T> source, Func<T, bool> predicate)
        {
            Throw.IfNull(source);
            Throw.IfNull(predicate);
            _source = source;
            _predicate = predicate;
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<T> observer)
        {
            Throw.IfNull(observer);
            return _source.Subscribe(new FilteringObserver(observer, _predicate));
        }

        /// <summary>Forwards only values accepted by a predicate.</summary>
        private sealed class FilteringObserver : IObserver<T>
        {
            /// <summary>The downstream observer.</summary>
            private readonly IObserver<T> _observer;

            /// <summary>The predicate used to pass values.</summary>
            private readonly Func<T, bool> _predicate;

            /// <summary>Initializes a new instance of the <see cref="FilteringObserver"/> class.</summary>
            /// <param name="observer">The downstream observer.</param>
            /// <param name="predicate">The predicate used to pass values.</param>
            internal FilteringObserver(IObserver<T> observer, Func<T, bool> predicate)
            {
                _observer = observer;
                _predicate = predicate;
            }

            /// <inheritdoc />
            public void OnCompleted() => _observer.OnCompleted();

            /// <inheritdoc />
            public void OnError(Exception error) => _observer.OnError(error);

            /// <inheritdoc />
            public void OnNext(T value)
            {
                if (_predicate(value))
                {
                    _observer.OnNext(value);
                }
            }
        }
    }
}
