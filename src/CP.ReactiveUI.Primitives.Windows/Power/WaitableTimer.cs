// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using Microsoft.Win32.SafeHandles;
using ReactiveUI.Primitives.Disposables;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Power;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Power;
#endif
/// <summary>
/// A managed wrapper around a Windows waitable timer that can optionally wake the system from sleep or hibernation.
/// See <a href="https://learn.microsoft.com/en-us/windows/win32/sync/waitable-timer-objects">Waitable Timer Objects</a>
/// </summary>
public sealed class WaitableTimer : IDisposable
{
    /// <summary>The native wait result for a signaled object.</summary>
    private const uint WaitObject0 = 0U;

    /// <summary>The native infinite timeout value.</summary>
    private const uint InfiniteTimeout = uint.MaxValue;

    /// <summary>The native wait result for a timeout.</summary>
    private const uint WaitTimeout = 258U;

    /// <summary>The maximum cancellable wait slice.</summary>
    private const uint PollMilliseconds = 100U;

    /// <summary>The wait operation used by this process.</summary>
    private static Func<SafeWaitHandle, uint, uint> _waitForSingleObject = SystemStateApi.WaitForSingleObject;

    /// <summary>The owned waitable timer handle.</summary>
    private SafeWaitHandle _handle;

    /// <summary>A value indicating whether this instance has been disposed.</summary>
    private bool _disposed;

    /// <summary>Gets a value indicating whether the timer has been created successfully.</summary>
    public bool IsValid
    {
        get
        {
            if (_handle is not null)
            {
                return !_handle.IsInvalid;
            }

            return false;
        }
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Power.WaitableTimer" /> class.</summary>
    public WaitableTimer()
        : this(manualReset: false)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Power.WaitableTimer" /> class.</summary>
    /// <param name="manualReset">
    /// If <c>true</c>, creates a manual-reset notification timer.
    /// If <c>false</c>, creates a synchronization timer.
    /// </param>
    public WaitableTimer(bool manualReset)
    {
        _handle = SystemStateApi.CreateWaitableTimer(IntPtr.Zero, manualReset, null);
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Power.WaitableTimer" /> class.</summary>
    /// <param name="name">The name of the timer.</param>
    public WaitableTimer(string name)
        : this(name, manualReset: false)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Power.WaitableTimer" /> class.</summary>
    /// <param name="name">The name of the timer.</param>
    /// <param name="manualReset">
    /// If <c>true</c>, creates a manual-reset notification timer.
    /// If <c>false</c>, creates a synchronization timer.
    /// </param>
    public WaitableTimer(string name, bool manualReset)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNullOrEmpty(name);
        _handle = SystemStateApi.CreateWaitableTimer(IntPtr.Zero, manualReset, name);
    }

    /// <summary>Sets the timer to fire once after the specified delay.</summary>
    /// <param name="delay">The delay before the timer fires.</param>
    /// <returns><c>true</c> if the timer was set successfully.</returns>
    public bool SetOnce(TimeSpan delay) => SetOnce(delay, wakeSystem: false);

    /// <summary>Sets the timer to fire once after the specified delay.</summary>
    /// <param name="delay">The delay before the timer fires.</param>
    /// <param name="wakeSystem">
    /// If <c>true</c>, the system will be woken from sleep or hibernation when the timer fires.
    /// Requires the SE_SYSTEMTIME_NAME privilege.
    /// </param>
    /// <returns><c>true</c> if the timer was set successfully.</returns>
    public bool SetOnce(TimeSpan delay, bool wakeSystem)
    {
        ThrowIfDisposed();
        long dueTime = ToRelativeFileTime(delay);
        return SystemStateApi.SetWaitableTimer(_handle, ref dueTime, 0, wakeSystem);
    }

    /// <summary>Sets the timer to fire at the specified absolute UTC time.</summary>
    /// <param name="dueTime">The UTC time at which the timer should fire.</param>
    /// <returns><c>true</c> if the timer was set successfully.</returns>
    public bool SetAt(DateTimeOffset dueTime) => SetAt(dueTime, wakeSystem: false);

    /// <summary>Sets the timer to fire at the specified absolute UTC time.</summary>
    /// <param name="dueTime">The UTC time at which the timer should fire.</param>
    /// <param name="wakeSystem">
    /// If <c>true</c>, the system will be woken from sleep or hibernation when the timer fires.
    /// Requires the SE_SYSTEMTIME_NAME privilege.
    /// </param>
    /// <returns><c>true</c> if the timer was set successfully.</returns>
    public bool SetAt(DateTimeOffset dueTime, bool wakeSystem)
    {
        ThrowIfDisposed();
        long fileTime = dueTime.ToFileTime();
        return SystemStateApi.SetWaitableTimer(_handle, ref fileTime, 0, wakeSystem);
    }

    /// <summary>Sets the timer to fire periodically.</summary>
    /// <param name="initialDelay">The delay before the first firing.</param>
    /// <param name="period">The period between subsequent firings, in milliseconds.</param>
    /// <returns><c>true</c> if the timer was set successfully.</returns>
    public bool SetPeriodic(TimeSpan initialDelay, int period) => SetPeriodic(initialDelay, period, wakeSystem: false);

    /// <summary>Sets the timer to fire periodically.</summary>
    /// <param name="initialDelay">The delay before the first firing.</param>
    /// <param name="period">The period between subsequent firings, in milliseconds.</param>
    /// <param name="wakeSystem">
    /// If <c>true</c>, the system will be woken from sleep or hibernation on the first firing.
    /// Requires the SE_SYSTEMTIME_NAME privilege.
    /// </param>
    /// <returns><c>true</c> if the timer was set successfully.</returns>
    public bool SetPeriodic(TimeSpan initialDelay, int period, bool wakeSystem)
    {
        ThrowIfDisposed();
        long dueTime = ToRelativeFileTime(initialDelay);
        return SystemStateApi.SetWaitableTimer(_handle, ref dueTime, period, wakeSystem);
    }

    /// <summary>Cancels the timer so it no longer fires.</summary>
    /// <returns><c>true</c> if the cancel was successful.</returns>
    public bool Cancel()
    {
        ThrowIfDisposed();
        return SystemStateApi.CancelWaitableTimer(_handle);
    }

    /// <summary>Waits for the timer to be signaled.</summary>
    /// <param name="timeout">Maximum time to wait. Use <see cref="F:System.Threading.Timeout.InfiniteTimeSpan" /> to wait indefinitely.</param>
    /// <returns><c>true</c> if the timer was signaled; <c>false</c> if the wait timed out.</returns>
    public bool Wait(TimeSpan timeout)
    {
        ThrowIfDisposed();
        return WaitCore(timeout, CancellationToken.None);
    }

    /// <summary>Waits indefinitely for the timer to be signaled.</summary>
    public void Wait() => Wait(Timeout.InfiniteTimeSpan);

    /// <summary>Asynchronously waits indefinitely for the timer to be signaled.</summary>
    /// <returns>A task that yields <see langword="true" /> when the timer is signaled.</returns>
    public ValueTask<bool> WaitAsync() => WaitAsync(Timeout.InfiniteTimeSpan, CancellationToken.None);

    /// <summary>Asynchronously waits indefinitely for the timer to be signaled.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that yields <see langword="true" /> when the timer is signaled.</returns>
    public ValueTask<bool> WaitAsync(CancellationToken cancellationToken) => WaitAsync(Timeout.InfiniteTimeSpan, cancellationToken);

    /// <summary>Asynchronously waits for the timer to be signaled.</summary>
    /// <param name="timeout">Maximum time to wait. Use <see cref="F:System.Threading.Timeout.InfiniteTimeSpan" /> to wait indefinitely.</param>
    /// <returns>A task that yields <see langword="true" /> when the timer is signaled; otherwise <see langword="false" />.</returns>
    public ValueTask<bool> WaitAsync(TimeSpan timeout) => WaitAsync(timeout, CancellationToken.None);

    /// <summary>Asynchronously waits for the timer to be signaled.</summary>
    /// <param name="timeout">Maximum time to wait. Use <see cref="F:System.Threading.Timeout.InfiniteTimeSpan" /> to wait indefinitely.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that yields <see langword="true" /> when the timer is signaled; otherwise <see langword="false" />.</returns>
    public ValueTask<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return new(Task.Run(() => WaitCore(timeout, cancellationToken), cancellationToken));
    }

    /// <summary>Observes each waitable timer signal until the subscription is disposed.</summary>
    /// <returns>An observable sequence of signal timestamps.</returns>
    public IObservable<DateTimeOffset> ObserveSignals() => ObserveSignals(Timeout.InfiniteTimeSpan);

    /// <summary>Observes each waitable timer signal until the subscription is disposed or a wait times out.</summary>
    /// <param name="timeout">Maximum time to wait between timer signals.</param>
    /// <returns>An observable sequence of signal timestamps.</returns>
    public IObservable<DateTimeOffset> ObserveSignals(TimeSpan timeout) => ReactiveSignal.Create(delegate(IObserver<DateTimeOffset> observer)
        {
            CancellationTokenSource cancellationTokenSource = new();
            _ = Task.Run(
                async delegate
            {
                try
                {
                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        if (!(await WaitAsync(timeout, cancellationTokenSource.Token).ConfigureAwait(continueOnCapturedContext: false)))
                        {
                            observer.OnCompleted();
                            break;
                        }

                        observer.OnNext(TimeProvider.System.GetUtcNow());
                    }
                }
                catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
                {
                }
                catch (Exception error)
                {
                    observer.OnError(error);
                }
            },
                CancellationToken.None);
            return cancellationTokenSource;
        });

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _handle?.Dispose();
            _handle = null;
        }
    }

    /// <summary>Overrides the wait operation for deterministic tests.</summary>
    /// <param name="waitForSingleObject">The replacement wait operation.</param>
    /// <returns>A scope that restores the previous wait operation.</returns>
    internal static IDisposable OverrideWaitForSingleObjectForTesting(Func<SafeWaitHandle, uint, uint> waitForSingleObject)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(waitForSingleObject);
        Func<SafeWaitHandle, uint, uint> waitForSingleObject2 = _waitForSingleObject;
        _waitForSingleObject = waitForSingleObject;
        return Scope.Create(waitForSingleObject2, delegate(Func<SafeWaitHandle, uint, uint> previous)
        {
            _waitForSingleObject = previous;
        });
    }

    /// <summary>Converts a relative delay into a native waitable timer due time.</summary>
    /// <param name="delay">The relative delay.</param>
    /// <returns>The native due time.</returns>
    private static long ToRelativeFileTime(TimeSpan delay)
    {
        if (delay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), delay, "The delay must be zero or positive.");
        }

        return checked(-delay.Ticks);
    }

    /// <summary>Converts a timeout to milliseconds.</summary>
    /// <param name="timeout">The timeout value.</param>
    /// <returns>The timeout in milliseconds.</returns>
    private static uint ToMilliseconds(TimeSpan timeout)
    {
        if (timeout == Timeout.InfiniteTimeSpan)
        {
            return uint.MaxValue;
        }

        double totalMilliseconds = timeout.TotalMilliseconds;
        if (totalMilliseconds < 0.0 || totalMilliseconds > 4294967295.0)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "The timeout must be infinite or between zero and UInt32.MaxValue milliseconds.");
        }

        return checked((uint)totalMilliseconds);
    }

    /// <summary>Throws when this instance has been disposed.</summary>
    private void ThrowIfDisposed() => CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfDisposed(_disposed, this);

    /// <summary>Waits for the timer with cancellation support.</summary>
    /// <param name="timeout">The timeout.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true" /> when signaled; otherwise <see langword="false" />.</returns>
    private bool WaitCore(TimeSpan timeout, CancellationToken cancellationToken)
    {
        uint remainingMilliseconds = ToMilliseconds(timeout);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            uint waitMilliseconds = ((remainingMilliseconds == uint.MaxValue) ? 100U : Math.Min(remainingMilliseconds, 100U));
            uint waitResult = _waitForSingleObject(_handle, waitMilliseconds);
            switch (waitResult)
            {
            case 0U:
                return true;
            case 258U:
            {
                if (remainingMilliseconds != uint.MaxValue)
                {
                    if (remainingMilliseconds <= waitMilliseconds)
                    {
                        return false;
                    }

                    remainingMilliseconds = checked(remainingMilliseconds - waitMilliseconds);
                }

                break;
            }

            default:
                throw new InvalidOperationException($"Waitable timer wait failed with native result 0x{waitResult:X8}.");
            }
        }
    }
}
