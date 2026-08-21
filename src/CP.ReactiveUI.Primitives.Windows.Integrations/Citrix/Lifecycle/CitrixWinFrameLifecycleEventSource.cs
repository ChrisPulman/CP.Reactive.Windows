// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Lifecycle;

/// <summary>Observes Citrix WFAPI session lifecycle events through <see cref="WinFrame" />.</summary>
public sealed class CitrixWinFrameLifecycleEventSource : ICitrixLifecycleEventSource
{
    /// <summary>The shared production source.</summary>
    private static readonly Lazy<CitrixWinFrameLifecycleEventSource> Singleton = new(static () => new CitrixWinFrameLifecycleEventSource());

    /// <summary>The blocking WFAPI wait operation.</summary>
    private readonly Func<EventMask, EventMask> _waitSystemEvent;

    /// <summary>The Citrix connection-state query operation.</summary>
    private readonly Func<ConnectStates?> _querySessionConnectState;

    /// <summary>The timestamp provider.</summary>
    private readonly TimeProvider _timeProvider;

    /// <summary>Initializes a new instance of the <see cref="CitrixWinFrameLifecycleEventSource" /> class.</summary>
    public CitrixWinFrameLifecycleEventSource()
        : this(WinFrame.WaitSystemEvent, WinFrame.QuerySessionConnectState)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="CitrixWinFrameLifecycleEventSource" /> class.</summary>
    /// <param name="waitSystemEvent">The blocking WFAPI wait operation.</param>
    /// <param name="querySessionConnectState">The Citrix connection-state query operation.</param>
    public CitrixWinFrameLifecycleEventSource(
        Func<EventMask, EventMask> waitSystemEvent,
        Func<ConnectStates?> querySessionConnectState)
        : this(waitSystemEvent, querySessionConnectState, TimeProvider.System)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="CitrixWinFrameLifecycleEventSource" /> class.</summary>
    /// <param name="waitSystemEvent">The blocking WFAPI wait operation.</param>
    /// <param name="querySessionConnectState">The Citrix connection-state query operation.</param>
    /// <param name="timeProvider">The timestamp provider.</param>
    internal CitrixWinFrameLifecycleEventSource(
        Func<EventMask, EventMask> waitSystemEvent,
        Func<ConnectStates?> querySessionConnectState,
        TimeProvider timeProvider)
    {
        Throw.IfNull(waitSystemEvent);
        Throw.IfNull(querySessionConnectState);
        Throw.IfNull(timeProvider);
        _waitSystemEvent = waitSystemEvent;
        _querySessionConnectState = querySessionConnectState;
        _timeProvider = timeProvider;
    }

    /// <summary>Gets the shared production source.</summary>
    public static CitrixWinFrameLifecycleEventSource Instance => Singleton.Value;

    /// <inheritdoc />
    public IObservable<CitrixLifecycleEvent> ObserveLifecycleEvents() => new WinFrameLifecycleObservable(_waitSystemEvent, _querySessionConnectState, _timeProvider);

    /// <summary>Converts blocking WFAPI waits into an observable sequence.</summary>
    /// <param name="waitSystemEvent">The blocking WFAPI wait operation.</param>
    /// <param name="querySessionConnectState">The Citrix connection-state query operation.</param>
    /// <param name="timeProvider">The timestamp provider.</param>
    private sealed class WinFrameLifecycleObservable(
        Func<EventMask, EventMask> waitSystemEvent,
        Func<ConnectStates?> querySessionConnectState,
        TimeProvider timeProvider) : IObservable<CitrixLifecycleEvent>
    {
        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<CitrixLifecycleEvent> observer)
        {
            Throw.IfNull(observer);
            WinFrameLifecycleSubscription subscription = new(waitSystemEvent, querySessionConnectState, timeProvider, observer);
            subscription.Start();
            return subscription;
        }
    }

    /// <summary>Owns one background WFAPI wait loop.</summary>
    /// <param name="waitSystemEvent">The blocking WFAPI wait operation.</param>
    /// <param name="querySessionConnectState">The Citrix connection-state query operation.</param>
    /// <param name="timeProvider">The timestamp provider.</param>
    /// <param name="observer">The observer that receives lifecycle events.</param>
    private sealed class WinFrameLifecycleSubscription(
        Func<EventMask, EventMask> waitSystemEvent,
        Func<ConnectStates?> querySessionConnectState,
        TimeProvider timeProvider,
        IObserver<CitrixLifecycleEvent> observer) : IDisposable
    {
        /// <summary>WF_EVENT_FLUSH, used to release blocking WFWaitSystemEvent calls.</summary>
        private const EventMask FlushEvent = (EventMask)0x80000000;

        /// <summary>The current Citrix session identifier used by <see cref="WinFrame" />.</summary>
        private const int CurrentSession = -1;

        /// <summary>The last observed connection state.</summary>
        private ConnectStates? _lastConnectState;

        /// <summary>A value indicating whether disposal has been requested.</summary>
        private volatile bool _isDisposed;

        /// <summary>Stops the wait loop.</summary>
        public void Dispose()
        {
            _isDisposed = true;
            try
            {
                _ = waitSystemEvent(FlushEvent);
            }
            catch (Win32Exception)
            {
                // Disposal is best-effort because WFAPI may already be unavailable or shutting down.
            }
            catch (DllNotFoundException)
            {
                // Disposal is best-effort because WFAPI may already be unavailable or shutting down.
            }
            catch (EntryPointNotFoundException)
            {
                // Disposal is best-effort because WFAPI may already be unavailable or shutting down.
            }
            catch (BadImageFormatException)
            {
                // Disposal is best-effort because WFAPI may already be unavailable or shutting down.
            }
        }

        /// <summary>Starts the wait loop.</summary>
        public void Start()
        {
            Thread thread = new(Run) { IsBackground = true, Name = $"CitrixLifecycle_{Guid.NewGuid():N}" };
            thread.Start();
        }

        /// <summary>Runs the blocking wait loop.</summary>
        private void Run()
        {
            while (!_isDisposed)
            {
                EventMask eventMask;
                try
                {
                    eventMask = waitSystemEvent(EventMask.All);
                }
                catch (Exception ex)
                {
                    if (!_isDisposed)
                    {
                        observer.OnError(ex);
                    }

                    return;
                }

                if (_isDisposed || eventMask == EventMask.None || eventMask == FlushEvent)
                {
                    continue;
                }

                PublishEvents(eventMask, timeProvider.GetUtcNow());
            }
        }

        /// <summary>Publishes typed lifecycle events for the WFAPI mask.</summary>
        /// <param name="eventMask">The WFAPI event mask.</param>
        /// <param name="timestamp">The event timestamp.</param>
        private void PublishEvents(EventMask eventMask, DateTimeOffset timestamp)
        {
            ConnectStates? previousState = _lastConnectState;
            ConnectStates? currentState = TryQuerySessionConnectState();
            _lastConnectState = currentState;
            CitrixSessionInfo session = new(CurrentSession, currentState, null, null, null, null);

            if ((eventMask & EventMask.Connect) == EventMask.Connect)
            {
                observer.OnNext(new CitrixConnectEvent(session, timestamp));
            }

            if ((eventMask & EventMask.Disconnect) == EventMask.Disconnect)
            {
                observer.OnNext(new CitrixDisconnectEvent(session, timestamp));
            }

            if ((eventMask & EventMask.Logon) == EventMask.Logon)
            {
                observer.OnNext(new CitrixLoginEvent(session, timestamp));
            }

            if ((eventMask & EventMask.StateChange) == EventMask.StateChange)
            {
                observer.OnNext(new CitrixSessionStateChangeEvent(session, previousState, currentState, timestamp));
            }
        }

        /// <summary>Queries the current connection state without tearing down the lifecycle stream when the state is unavailable.</summary>
        /// <returns>The current state, when available.</returns>
        private ConnectStates? TryQuerySessionConnectState()
        {
            try
            {
                return querySessionConnectState();
            }
            catch (Win32Exception)
            {
                return null;
            }
            catch (DllNotFoundException)
            {
                return null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }
            catch (BadImageFormatException)
            {
                return null;
            }
        }
    }
}
