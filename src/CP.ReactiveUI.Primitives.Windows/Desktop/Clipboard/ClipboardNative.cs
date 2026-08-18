// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
#endif
/// <summary>Provides low level access to the Windows clipboard.</summary>
public static class ClipboardNative
{
    /// <summary>The global clipboard lock provider.</summary>
    private static readonly ClipboardSemaphore ClipboardLockProvider = new();

    /// <summary>Native clipboard operations used by this type.</summary>
    private static ClipboardNativeOperations _operations = new(
        NativeMethods.GetClipboardOwner,
        NativeMethods.GetClipboardSequenceNumber,
        NativeMethods.IsClipboardFormatAvailable);

    /// <summary>The last observed clipboard sequence number.</summary>
    private static uint _previousSequence;

    /// <summary>The shared clipboard update observable.</summary>
    private static IObservable<ClipboardUpdateInformation> _clipboardUpdateEvents;

    /// <summary>The shared render-format request observable.</summary>
    private static IObservable<ClipboardRenderFormatRequest> _clipboardRenderFormatRequests;

    /// <summary>
    /// Gets an observable that publishes the current clipboard contents after every paste action.
    /// Best to use SubscribeOn with the UI SynchronizationContext.
    /// </summary>
    public static IObservable<ClipboardUpdateInformation> ClipboardUpdateEvents =>
        _clipboardUpdateEvents ??= ReactiveSignal.Create<ClipboardUpdateInformation>(observer =>
            SharedMessageWindow.ObserveWindowMessages(
                hwnd =>
                {
                    var windowHandle = (nint)hwnd;
                    if (!NativeMethods.AddClipboardFormatListener(windowHandle))
                    {
                        observer.OnError(new Win32Exception());
                        return;
                    }

                    observer.OnNext(ClipboardUpdateInformation.Create(windowHandle));
                },
                static hwnd => _ = NativeMethods.RemoveClipboardFormatListener((nint)hwnd))
            .Where(static message => message.Msg == WindowsMessages.WM_CLIPBOARDUPDATE)
            .Subscribe(
                message =>
                {
                    ClipboardUpdateInformation information =
                        ClipboardUpdateInformation.Create((nint)message.Hwnd);
                    if (information.Id <= _previousSequence)
                    {
                        return;
                    }

                    _previousSequence = information.Id;
                    observer.OnNext(information);
                },
                observer.OnError,
                observer.OnCompleted)).Publish().RefCount();

    /// <summary>
    /// Gets an observable that can be subscribed to be informed if a certain clipboard format is requested.
    /// Best to use SubscribeOn with the UI SynchronizationContext.
    /// </summary>
    public static IObservable<ClipboardRenderFormatRequest> ClipboardRenderFormatRequests =>
        _clipboardRenderFormatRequests ??= ReactiveSignal.Create<ClipboardRenderFormatRequest>(observer =>
            SharedMessageWindow.ObserveWindowMessages()
            .Where(static message =>
                message.Msg is WindowsMessages.WM_RENDERALLFORMATS
                    or WindowsMessages.WM_RENDERFORMAT
                    or WindowsMessages.WM_DESTROYCLIPBOARD)
            .Subscribe(
                message => observer.OnNext(CreateRenderFormatRequest(message)),
                observer.OnError,
                observer.OnCompleted)).Publish().RefCount();

    /// <summary>Gets whether the clipboard currently has an owner window.</summary>
    public static bool HasOwner => _operations.GetOwner() != IntPtr.Zero;

    /// <summary>
    /// Gets the current clipboard sequence number via GetClipboardSequenceNumber.
    /// This returns 0 if there is no WINSTA_ACCESSCLIPBOARD.
    /// </summary>
    public static uint SequenceNumber => _operations.GetSequenceNumber();

    /// <summary>Get access, a global lock, to the clipboard.</summary>
    /// <returns>IClipboard, which will unlock when Dispose is called.</returns>
    public static IClipboardAccessToken Access() => ClipboardLockProvider.Lock();

    /// <summary>Get access, a global lock, to the clipboard.</summary>
    /// <param name="windowHandle">IntPtr with the windows handle.</param>
    /// <returns>IClipboard, which will unlock when Dispose is called.</returns>
    public static IClipboardAccessToken Access(IntPtr windowHandle) => ClipboardLockProvider.Lock(windowHandle);

    /// <summary>Get access, a global lock, to the clipboard.</summary>
    /// <param name="windowHandle">IntPtr with the windows handle.</param>
    /// <param name="retries">int with the amount of lock attempts are made.</param>
    /// <returns>IClipboard, which will unlock when Dispose is called.</returns>
    public static IClipboardAccessToken Access(IntPtr windowHandle, int retries) => ClipboardLockProvider.Lock(windowHandle, retries);

    /// <summary>Get access, a global lock, to the clipboard.</summary>
    /// <param name="windowHandle">IntPtr with the windows handle.</param>
    /// <param name="retries">int with the amount of lock attempts are made.</param>
    /// <param name="retryInterval">Timespan between retries.</param>
    /// <returns>IClipboard, which will unlock when Dispose is called.</returns>
    public static IClipboardAccessToken Access(IntPtr windowHandle, int retries, TimeSpan retryInterval) => ClipboardLockProvider.Lock(windowHandle, retries, retryInterval);

    /// <summary>Get access, a global lock, to the clipboard.</summary>
    /// <param name="windowHandle">IntPtr with the windows handle.</param>
    /// <param name="retries">int with the amount of lock attempts are made.</param>
    /// <param name="retryInterval">Timespan between retries.</param>
    /// <param name="timeout">Timeout for getting the lock.</param>
    /// <returns>IClipboard, which will unlock when Dispose is called.</returns>
    public static IClipboardAccessToken Access(
        IntPtr windowHandle,
        int retries,
        TimeSpan retryInterval,
        TimeSpan timeout) =>
        ClipboardLockProvider.Lock(windowHandle, retries, retryInterval, timeout);

    /// <summary>Get access, a global lock, to the clipboard.</summary>
    /// <returns>IClipboard in a Task, which will unlock when Dispose is called.</returns>
    public static ValueTask<IClipboardAccessToken> AccessAsync() => ClipboardLockProvider.LockAsync();

    /// <summary>Get access, a global lock, to the clipboard.</summary>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>IClipboard in a Task, which will unlock when Dispose is called.</returns>
    public static ValueTask<IClipboardAccessToken> AccessAsync(CancellationToken cancellationToken) => ClipboardLockProvider.LockAsync(cancellationToken);

    /// <summary>Get access, a global lock, to the clipboard.</summary>
    /// <param name="windowHandle">IntPtr with the windows handle.</param>
    /// <returns>IClipboard in a Task, which will unlock when Dispose is called.</returns>
    public static ValueTask<IClipboardAccessToken> AccessAsync(IntPtr windowHandle) => ClipboardLockProvider.LockAsync(windowHandle);

    /// <summary>Get access, a global lock, to the clipboard.</summary>
    /// <param name="windowHandle">IntPtr with the windows handle.</param>
    /// <param name="retries">int with the amount of lock attempts are made.</param>
    /// <returns>IClipboard in a Task, which will unlock when Dispose is called.</returns>
    public static ValueTask<IClipboardAccessToken> AccessAsync(IntPtr windowHandle, int retries) => ClipboardLockProvider.LockAsync(windowHandle, retries);

    /// <summary>Get access, a global lock, to the clipboard.</summary>
    /// <param name="windowHandle">IntPtr with the windows handle.</param>
    /// <param name="retries">int with the amount of lock attempts are made.</param>
    /// <param name="retryInterval">Timespan between retries.</param>
    /// <returns>IClipboard in a Task, which will unlock when Dispose is called.</returns>
    public static ValueTask<IClipboardAccessToken> AccessAsync(IntPtr windowHandle, int retries, TimeSpan retryInterval) => ClipboardLockProvider.LockAsync(windowHandle, retries, retryInterval);

    /// <summary>Get access, a global lock, to the clipboard.</summary>
    /// <param name="windowHandle">IntPtr with the windows handle.</param>
    /// <param name="retries">int with the amount of lock attempts are made.</param>
    /// <param name="retryInterval">Timespan between retries.</param>
    /// <param name="timeout">Timespan to wait for a lock.</param>
    /// <returns>IClipboard in a Task, which will unlock when Dispose is called.</returns>
    public static ValueTask<IClipboardAccessToken> AccessAsync(
        IntPtr windowHandle,
        int retries,
        TimeSpan retryInterval,
        TimeSpan timeout) =>
        ClipboardLockProvider.LockAsync(windowHandle, retries, retryInterval, timeout);

    /// <summary>Get access, a global lock, to the clipboard.</summary>
    /// <param name="windowHandle">IntPtr with the windows handle.</param>
    /// <param name="retries">int with the amount of lock attempts are made.</param>
    /// <param name="retryInterval">Timespan between retries.</param>
    /// <param name="timeout">Timespan to wait for a lock.</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>IClipboard in a Task, which will unlock when Dispose is called.</returns>
    public static ValueTask<IClipboardAccessToken> AccessAsync(
        IntPtr windowHandle,
        int retries,
        TimeSpan retryInterval,
        TimeSpan timeout,
        CancellationToken cancellationToken) =>
        ClipboardLockProvider.LockAsync(windowHandle, retries, retryInterval, timeout, cancellationToken);

    /// <summary>Test if the specified format is available on the clipboard.</summary>
    /// <param name="formatId">uint.</param>
    /// <returns>bool.</returns>
    public static bool HasFormat(uint formatId) => _operations.IsFormatAvailable(formatId);

    /// <summary>Test if the specified format is available on the clipboard.</summary>
    /// <param name="format">string.</param>
    /// <returns>bool.</returns>
    public static bool HasFormat(string format) => _operations.IsFormatAvailable(ClipboardFormatExtensions.MapFormatToId(format));

    /// <summary>Overrides native clipboard operations for deterministic tests.</summary>
    /// <param name="getOwner">The replacement owner query.</param>
    /// <param name="getSequenceNumber">The replacement sequence-number query.</param>
    /// <param name="isFormatAvailable">The replacement format availability query.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(Func<IntPtr> getOwner, Func<uint> getSequenceNumber, Func<uint, bool> isFormatAvailable)
    {
        Throw.IfNull(getOwner);
        Throw.IfNull(getSequenceNumber);
        Throw.IfNull(isFormatAvailable);
        var operations = _operations;
        _operations = new(getOwner, getSequenceNumber, isFormatAvailable);
        return Scope.Create(operations, static previous => _operations = previous);
    }

    /// <summary>Resets cached clipboard observables for deterministic tests.</summary>
    internal static void ResetObservablesForTesting()
    {
        _clipboardUpdateEvents = null;
        _clipboardRenderFormatRequests = null;
        _previousSequence = 0U;
    }

    /// <summary>Creates a render-format request from a clipboard window message for deterministic coverage tests.</summary>
    /// <param name="message">The source window message.</param>
    /// <returns>The render-format request.</returns>
    internal static ClipboardRenderFormatRequest CreateRenderFormatRequestForTesting(WindowMessage message) =>
        CreateRenderFormatRequest(message);

    /// <summary>Creates a render-format request from a clipboard window message.</summary>
    /// <param name="message">The source window message.</param>
    /// <returns>The render-format request.</returns>
    private static ClipboardRenderFormatRequest CreateRenderFormatRequest(WindowMessage message) =>
        message.Msg switch
        {
            WindowsMessages.WM_RENDERALLFORMATS => new ClipboardRenderFormatRequest { AccessToken = Access() },
            WindowsMessages.WM_RENDERFORMAT => new ClipboardRenderFormatRequest
            {
                RequestedFormatId = checked((uint)message.WParam),
                AccessToken = new ClipboardAccessToken { CanAccess = true, IsLockTimeout = true },
            },
            WindowsMessages.WM_DESTROYCLIPBOARD => new ClipboardRenderFormatRequest { IsDestroyClipboard = true },
            _ => throw new InvalidOperationException($"Unsupported clipboard message {message.Msg}."),
        };

    /// <summary>Composes native clipboard operations without invoking them during construction.</summary>
    /// <param name="getOwner">The owner query.</param>
    /// <param name="getSequenceNumber">The sequence-number query.</param>
    /// <param name="isFormatAvailable">The format availability query.</param>
    private sealed class ClipboardNativeOperations(
        Func<IntPtr> getOwner,
        Func<uint> getSequenceNumber,
        Func<uint, bool> isFormatAvailable)
    {
        /// <summary>Gets the current clipboard owner handle.</summary>
        /// <returns>The owner handle.</returns>
        public IntPtr GetOwner() => getOwner();

        /// <summary>Gets the current clipboard sequence number.</summary>
        /// <returns>The sequence number.</returns>
        public uint GetSequenceNumber() => getSequenceNumber();

        /// <summary>Determines whether a clipboard format is available.</summary>
        /// <param name="formatId">The clipboard format identifier.</param>
        /// <returns><see langword="true" /> when the format is available.</returns>
        public bool IsFormatAvailable(uint formatId) => isFormatAvailable(formatId);
    }
}
