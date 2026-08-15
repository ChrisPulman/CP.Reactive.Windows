// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Simple observable used for deterministic test streams.</summary>
/// <typeparam name="T">The published value type.</typeparam>
/// <param name="initialValue">The optional initial value.</param>
/// <param name="emitInitial">A value indicating whether to emit the initial value on subscribe.</param>
internal sealed class ManualObservable<T>(T initialValue = default, bool emitInitial = false) : IObservable<T>, IDisposable
{
    /// <summary>Synchronizes observer access.</summary>
    private readonly Lock _syncRoot = new();

    /// <summary>The current observers.</summary>
    private readonly List<IObserver<T>> _observers = new();

    /// <inheritdoc />
    public IDisposable Subscribe(IObserver<T> observer)
    {
        lock (_syncRoot)
        {
            _observers.Add(observer);
        }

        if (emitInitial)
        {
            observer.OnNext(initialValue);
        }

        return new Subscription(this, observer);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        List<IObserver<T>> observers;
        lock (_syncRoot)
        {
            observers = new(_observers);
            _observers.Clear();
        }

        foreach (IObserver<T> observer in observers)
        {
            observer.OnCompleted();
        }
    }

    /// <summary>Publishes a value to current observers.</summary>
    /// <param name="value">The value to publish.</param>
    internal void OnNext(T value)
    {
        List<IObserver<T>> observers;
        lock (_syncRoot)
        {
            observers = new(_observers);
        }

        foreach (IObserver<T> observer in observers)
        {
            observer.OnNext(value);
        }
    }

    /// <summary>Removes an observer.</summary>
    /// <param name="observer">The observer to remove.</param>
    private void Unsubscribe(IObserver<T> observer)
    {
        lock (_syncRoot)
        {
            _ = _observers.Remove(observer);
        }
    }

    /// <summary>Represents a deterministic observable subscription.</summary>
    /// <param name="owner">The owner observable.</param>
    /// <param name="observer">The subscribed observer.</param>
    private sealed class Subscription(ManualObservable<T> owner, IObserver<T> observer) : IDisposable
    {
        /// <summary>The subscribed observer.</summary>
        private IObserver<T> _observer = observer;

        /// <inheritdoc />
        public void Dispose()
        {
            IObserver<T> currentObserver = Interlocked.Exchange(ref _observer, null);
            if (currentObserver is not null)
            {
                owner.Unsubscribe(currentObserver);
            }
        }
    }
}
