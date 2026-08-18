// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard.Internals;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.Internals;
#endif
/// <summary>This can be used to get a lock to the clipboard, and free it again.</summary>
#if NETFRAMEWORK
internal sealed class ClipboardSemaphore : IDisposable
#else
internal sealed partial class ClipboardSemaphore : IDisposable
#endif
{
    /// <summary>The default number of retries when opening the clipboard.</summary>
    private const int DefaultRetries = 5;

    /// <summary>The default clipboard wait timeout.</summary>
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMilliseconds(200.0);

    /// <summary>The default retry interval between clipboard open attempts.</summary>
    private static readonly TimeSpan DefaultRetryInterval = TimeSpan.FromMilliseconds(100.0);

    /// <summary>Keeps the default clipboard-owner window alive for process-lifetime clipboard ownership.</summary>
    private static Func<IDisposable> _messageWindowLifetimeFactory =
        static () => SharedMessageWindow.ObserveWindowMessages().Subscribe(GC.KeepAlive);

    /// <summary>Keeps the deferred message-window lifetime.</summary>
    private static Lazy<IDisposable> _messageWindowLifetime = CreateMessageWindowLifetime();

    /// <summary>Native clipboard semaphore operations used by this type.</summary>
    private static ClipboardSemaphoreOperations _operations =
        CreateDefaultOperations();

    /// <summary>Shared message-window operations used by the default acquisition path.</summary>
    private static SharedMessageWindowOperations _sharedMessageWindowOperations =
        CreateDefaultSharedMessageWindowOperations();

    /// <summary>The semaphore that serializes in-process clipboard access.</summary>
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    /// <summary>Implementation of the IDisposable.</summary>
    public void Dispose() => _semaphoreSlim.Dispose();

    /// <summary>Overrides clipboard semaphore operations for deterministic tests.</summary>
    /// <param name="openClipboard">The replacement open operation.</param>
    /// <param name="closeClipboard">The replacement close operation.</param>
    /// <param name="tryAcquireMessageWindow">The replacement owner-window acquisition operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(Func<IntPtr, bool> openClipboard, Func<bool> closeClipboard, Func<int, TimeSpan, IntPtr?> tryAcquireMessageWindow)
    {
        Throw.IfNull(openClipboard);
        Throw.IfNull(closeClipboard);
        Throw.IfNull(tryAcquireMessageWindow);
        var operations = _operations;
        _operations = new(openClipboard, closeClipboard, tryAcquireMessageWindow);
        return Scope.Create(operations, static previous => _operations = previous);
    }

    /// <summary>Overrides shared message-window operations for deterministic tests.</summary>
    /// <param name="keepAlive">The replacement operation that keeps the message window alive.</param>
    /// <param name="getNativeHandle">The replacement operation that gets the native window handle.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideSharedMessageWindowOperationsForTesting(Func<IDisposable> keepAlive, Func<IntPtr> getNativeHandle)
    {
        Throw.IfNull(keepAlive);
        Throw.IfNull(getNativeHandle);
        var sharedMessageWindowOperations = _sharedMessageWindowOperations;
        _sharedMessageWindowOperations = new(keepAlive, getNativeHandle);
        return Scope.Create(sharedMessageWindowOperations, static previous => _sharedMessageWindowOperations = previous);
    }

    /// <summary>Overrides the deferred default message-window lifetime factory for deterministic tests.</summary>
    /// <param name="messageWindowLifetimeFactory">The replacement message-window lifetime factory.</param>
    /// <returns>A scope that restores the previous factory and deferred lifetime.</returns>
    internal static IDisposable OverrideMessageWindowLifetimeFactoryForTesting(Func<IDisposable> messageWindowLifetimeFactory)
    {
        Throw.IfNull(messageWindowLifetimeFactory);
        (Func<IDisposable> Factory, Lazy<IDisposable> Lifetime) previous = (_messageWindowLifetimeFactory, _messageWindowLifetime);
        _messageWindowLifetimeFactory = messageWindowLifetimeFactory;
        _messageWindowLifetime = CreateMessageWindowLifetime();
        return Scope.Create(
            previous,
            static state =>
            {
                _messageWindowLifetimeFactory = state.Factory;
                _messageWindowLifetime = state.Lifetime;
            });
    }

    /// <summary>Gets the deferred default message-window lifetime through its deterministic test seam.</summary>
    /// <returns>The initialized message-window lifetime subscription.</returns>
    internal static IDisposable GetMessageWindowLifetimeForTesting() => _messageWindowLifetime.Value;

    /// <summary>Uses the default native clipboard operation composition for deterministic construction coverage.</summary>
    /// <returns>A scope that restores the previous operations without invoking a native entry point.</returns>
    internal static IDisposable UseDefaultOperationsForTesting()
    {
        var operations = _operations;
        _operations = CreateDefaultOperations();
        return Scope.Create(operations, static previous => _operations = previous);
    }

    /// <summary>Uses the default shared message-window operation composition for deterministic construction coverage.</summary>
    /// <returns>A scope that restores the previous operations without accessing the shared window.</returns>
    internal static IDisposable UseDefaultSharedMessageWindowOperationsForTesting()
    {
        var operations = _sharedMessageWindowOperations;
        _sharedMessageWindowOperations = CreateDefaultSharedMessageWindowOperations();
        return Scope.Create(operations, static previous => _sharedMessageWindowOperations = previous);
    }

    /// <summary>Acquires the shared message-window handle using the default acquisition path.</summary>
    /// <param name="retries">The number of handle lookup retries.</param>
    /// <param name="retryInterval">The delay between handle lookups.</param>
    /// <returns>The acquired window handle, or <see langword="null" />.</returns>
    internal static IntPtr? TryAcquireSharedMessageWindowForTesting(int retries, TimeSpan retryInterval) => TryAcquireSharedMessageWindow(retries, retryInterval);

    /// <summary>Get a lock to the clipboard.</summary>
    /// <returns>IClipboardLock.</returns>
    internal IClipboardAccessToken Lock() => Lock((IntPtr)0, DefaultRetries, DefaultRetryInterval, DefaultTimeout);

    /// <summary>Get a lock to the clipboard.</summary>
    /// <param name="windowHandle">IntPtr with a window handle for the potential new owner.</param>
    /// <returns>IClipboardLock.</returns>
    internal IClipboardAccessToken Lock(IntPtr windowHandle) =>
        Lock(windowHandle, DefaultRetries, DefaultRetryInterval, DefaultTimeout);

    /// <summary>Get a lock to the clipboard.</summary>
    /// <param name="windowHandle">IntPtr with a window handle for the potential new owner.</param>
    /// <param name="retries">int with number of retries.</param>
    /// <returns>IClipboardLock.</returns>
    internal IClipboardAccessToken Lock(IntPtr windowHandle, int retries) => Lock(windowHandle, retries, DefaultRetryInterval, DefaultTimeout);

    /// <summary>Get a lock to the clipboard.</summary>
    /// <param name="windowHandle">IntPtr with a window handle for the potential new owner.</param>
    /// <param name="retries">int with number of retries.</param>
    /// <param name="retryInterval">TimeSpan for the time between retries.</param>
    /// <returns>IClipboardLock.</returns>
    internal IClipboardAccessToken Lock(IntPtr windowHandle, int retries, TimeSpan retryInterval) => Lock(windowHandle, retries, retryInterval, DefaultTimeout);

    /// <summary>Get a lock to the clipboard.</summary>
    /// <param name="windowHandle">IntPtr with a window handle for the potential new owner.</param>
    /// <param name="retries">int with number of retries.</param>
    /// <param name="retryInterval">TimeSpan for the time between retries.</param>
    /// <param name="timeout">TimeSpan for the timeout.</param>
    /// <returns>IClipboardLock.</returns>
    internal IClipboardAccessToken Lock(IntPtr windowHandle, int retries, TimeSpan retryInterval, TimeSpan timeout)
    {
        if (!_semaphoreSlim.Wait(timeout))
        {
            return new ClipboardAccessToken { CanAccess = false, IsLockTimeout = true };
        }

        if (windowHandle == IntPtr.Zero && !TryAcquireMessageWindow(retries, retryInterval, out windowHandle))
        {
            _ = _semaphoreSlim.Release();
            return new ClipboardAccessToken { CanAccess = false, IsOpenTimeout = true };
        }

        var isOpened = false;
        do
        {
            if (OpenClipboard(windowHandle))
            {
                isOpened = true;
                break;
            }

            retries = checked(retries - 1);
            if (retries >= 0)
            {
                Thread.Sleep(retryInterval);
            }
        }
        while (retries >= 0);
        if (!isOpened)
        {
            _ = _semaphoreSlim.Release();
            return new ClipboardAccessToken { CanAccess = false, IsOpenTimeout = true };
        }

        return CreateAccessToken();
    }

    /// <summary>Lock the clipboard, return a disposable which can free this again.</summary>
    /// <returns>Task with IClipboardLock.</returns>
    internal ValueTask<IClipboardAccessToken> LockAsync() =>
        LockAsync((IntPtr)0, DefaultRetries, DefaultRetryInterval, DefaultTimeout, CancellationToken.None);

    /// <summary>Lock the clipboard, return a disposable which can free this again.</summary>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>Task with IClipboardLock.</returns>
    internal ValueTask<IClipboardAccessToken> LockAsync(CancellationToken cancellationToken) =>
        LockAsync((IntPtr)0, DefaultRetries, DefaultRetryInterval, DefaultTimeout, cancellationToken);

    /// <summary>Lock the clipboard, return a disposable which can free this again.</summary>
    /// <param name="windowHandle">IntPtr with the window handle of the potential new owner.</param>
    /// <returns>Task with IClipboardLock.</returns>
    internal ValueTask<IClipboardAccessToken> LockAsync(IntPtr windowHandle) =>
        LockAsync(windowHandle, DefaultRetries, DefaultRetryInterval, DefaultTimeout, CancellationToken.None);

    /// <summary>Lock the clipboard, return a disposable which can free this again.</summary>
    /// <param name="windowHandle">IntPtr with the window handle of the potential new owner.</param>
    /// <param name="retries">int with the number of retries.</param>
    /// <returns>Task with IClipboardLock.</returns>
    internal ValueTask<IClipboardAccessToken> LockAsync(IntPtr windowHandle, int retries) => LockAsync(windowHandle, retries, DefaultRetryInterval, DefaultTimeout, CancellationToken.None);

    /// <summary>Lock the clipboard, return a disposable which can free this again.</summary>
    /// <param name="windowHandle">IntPtr with the window handle of the potential new owner.</param>
    /// <param name="retries">int with the number of retries.</param>
    /// <param name="retryInterval">TimeSpan for the time between retries.</param>
    /// <returns>Task with IClipboardLock.</returns>
    internal ValueTask<IClipboardAccessToken> LockAsync(
        IntPtr windowHandle,
        int retries,
        TimeSpan retryInterval) =>
        LockAsync(windowHandle, retries, retryInterval, DefaultTimeout, CancellationToken.None);

    /// <summary>Lock the clipboard, return a disposable which can free this again.</summary>
    /// <param name="windowHandle">IntPtr with the window handle of the potential new owner.</param>
    /// <param name="retries">int with the number of retries.</param>
    /// <param name="retryInterval">TimeSpan for the time between retries.</param>
    /// <param name="timeout">TimeSpan for the timeout.</param>
    /// <returns>Task with IClipboardLock.</returns>
    internal ValueTask<IClipboardAccessToken> LockAsync(
        IntPtr windowHandle,
        int retries,
        TimeSpan retryInterval,
        TimeSpan timeout) =>
        LockAsync(windowHandle, retries, retryInterval, timeout, CancellationToken.None);

    /// <summary>Lock the clipboard, return a disposable which can free this again.</summary>
    /// <param name="windowHandle">IntPtr with the window handle of the potential new owner.</param>
    /// <param name="retries">int with the number of retries.</param>
    /// <param name="retryInterval">TimeSpan for the time between retries.</param>
    /// <param name="timeout">TimeSpan for the timeout.</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>Task with IClipboardLock.</returns>
    internal ValueTask<IClipboardAccessToken> LockAsync(
        IntPtr windowHandle,
        int retries,
        TimeSpan retryInterval,
        TimeSpan timeout,
        CancellationToken cancellationToken) =>
        !cancellationToken.IsCancellationRequested
            ? new(Lock(windowHandle, retries, retryInterval, timeout))
            : new(Task.FromCanceled<IClipboardAccessToken>(cancellationToken));

    /// <summary>Composes the default native clipboard operations without invoking them.</summary>
    /// <returns>The default native clipboard operations.</returns>
    private static ClipboardSemaphoreOperations CreateDefaultOperations() =>
        new(NativeMethods.OpenClipboard, NativeMethods.CloseClipboard, TryAcquireSharedMessageWindow);

    /// <summary>Composes the default shared message-window operations without invoking them.</summary>
    /// <returns>The default shared message-window operations.</returns>
    private static SharedMessageWindowOperations CreateDefaultSharedMessageWindowOperations() =>
        new(static () => _messageWindowLifetime.Value, static () => SharedMessageWindow.NativeHandle);

    /// <summary>Creates the message-window lifetime subscription without initializing it.</summary>
    /// <returns>A deferred message-window lifetime subscription.</returns>
    private static Lazy<IDisposable> CreateMessageWindowLifetime() =>
        new(static () => _messageWindowLifetimeFactory());

    /// <summary>
    /// Opens the clipboard for examination and prevents other applications from modifying the clipboard content.
    /// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms649048(v=vs.85).aspx">OpenClipboard</a>.
    /// </summary>
    /// <param name="newOwnerWindowHandle">
    /// IntPtr with the window handle of the new owner. If this parameter is NULL,
    /// the open clipboard is associated with the current task.
    /// </param>
    /// <returns>true if the clipboard is opened.</returns>
    private static bool OpenClipboard(IntPtr newOwnerWindowHandle) => _operations.OpenClipboard(newOwnerWindowHandle);

    /// <summary>
    /// Closes the clipboard after examination.
    /// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms649048(v=vs.85).aspx">CloseClipboard</a>.
    /// </summary>
    /// <returns>true if the clipboard is closed.</returns>
    private static bool CloseClipboard() => _operations.CloseClipboard();

    /// <summary>Acquires a lease that keeps the default clipboard-owner window alive.</summary>
    /// <param name="retries">The number of handle lookup retries.</param>
    /// <param name="retryInterval">The delay between handle lookups.</param>
    /// <param name="windowHandle">The acquired window handle.</param>
    /// <returns><see langword="true" /> when a window handle was acquired.</returns>
    private static bool TryAcquireMessageWindow(int retries, TimeSpan retryInterval, out IntPtr windowHandle) =>
        _operations.TryAcquireMessageWindow(retries, retryInterval, out windowHandle);

    /// <summary>Gets a shared message-window handle for zero-owner clipboard access.</summary>
    /// <param name="retries">The number of handle lookup retries.</param>
    /// <param name="retryInterval">The delay between handle lookups.</param>
    /// <returns><see langword="true" /> when a handle was acquired.</returns>
    private static IntPtr? TryAcquireSharedMessageWindow(int retries, TimeSpan retryInterval)
    {
        _ = _sharedMessageWindowOperations.KeepAlive();
        checked
        {
            var remainingHandleAttempts = retries + 1;
            do
            {
                var windowHandle = _sharedMessageWindowOperations.GetNativeHandle();
                if (windowHandle != IntPtr.Zero)
                {
                    return windowHandle;
                }

                Thread.Sleep(retryInterval);
                remainingHandleAttempts--;
            }
            while (remainingHandleAttempts > 0);
            return null;
        }
    }

    /// <summary>Creates a token that owns the opened clipboard and semaphore lease.</summary>
    /// <returns>The access token.</returns>
    private ClipboardAccessToken CreateAccessToken() => new(() =>
    {
        _ = CloseClipboard();
        _ = _semaphoreSlim.Release();
    });

    /// <summary>Contains the native clipboard entry points.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>Opens the clipboard.</summary>
        /// <param name="newOwnerWindowHandle">The new owner window handle.</param>
        /// <returns><see langword="true" /> when the clipboard is opened.</returns>
#if NETFRAMEWORK
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OpenClipboard(IntPtr newOwnerWindowHandle);
#else
        [LibraryImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool OpenClipboard(IntPtr newOwnerWindowHandle);
#endif

        /// <summary>Closes the clipboard.</summary>
        /// <returns><see langword="true" /> when the clipboard is closed.</returns>
#if NETFRAMEWORK
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseClipboard();
#else
        [LibraryImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool CloseClipboard();
#endif
    }

    /// <summary>Composes clipboard semaphore operations without invoking them during construction.</summary>
    /// <param name="openClipboard">The clipboard open operation.</param>
    /// <param name="closeClipboard">The clipboard close operation.</param>
    /// <param name="tryAcquireMessageWindow">The owner-window acquisition operation.</param>
    private sealed class ClipboardSemaphoreOperations(
        Func<IntPtr, bool> openClipboard,
        Func<bool> closeClipboard,
        Func<int, TimeSpan, IntPtr?> tryAcquireMessageWindow)
    {
        /// <summary>Closes the clipboard.</summary>
        /// <returns><see langword="true" /> when the clipboard closed.</returns>
        public bool CloseClipboard() => closeClipboard();

        /// <summary>Opens the clipboard.</summary>
        /// <param name="newOwnerWindowHandle">The new owner window handle.</param>
        /// <returns><see langword="true" /> when the clipboard opened.</returns>
        public bool OpenClipboard(IntPtr newOwnerWindowHandle) => openClipboard(newOwnerWindowHandle);

        /// <summary>Acquires a message-window handle.</summary>
        /// <param name="retries">The number of handle lookup retries.</param>
        /// <param name="retryInterval">The delay between handle lookups.</param>
        /// <param name="windowHandle">The acquired window handle.</param>
        /// <returns><see langword="true" /> when a handle was acquired.</returns>
        public bool TryAcquireMessageWindow(int retries, TimeSpan retryInterval, out IntPtr windowHandle)
        {
            var acquiredHandle = tryAcquireMessageWindow(retries, retryInterval);
            windowHandle = acquiredHandle.GetValueOrDefault();
            return acquiredHandle.HasValue;
        }
    }

    /// <summary>Composes shared message-window operations without invoking them during construction.</summary>
    /// <param name="keepAlive">The message-window lifetime operation.</param>
    /// <param name="getNativeHandle">The native handle query operation.</param>
    private sealed class SharedMessageWindowOperations(Func<IDisposable> keepAlive, Func<IntPtr> getNativeHandle)
    {
        /// <summary>Gets the native message-window handle.</summary>
        /// <returns>The native message-window handle.</returns>
        public IntPtr GetNativeHandle() => getNativeHandle();

        /// <summary>Keeps the shared message window alive.</summary>
        /// <returns>The message-window lifetime subscription.</returns>
        public IDisposable KeepAlive() => keepAlive();
    }
}
