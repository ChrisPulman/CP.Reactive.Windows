// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>Blocks selected user-initiated window operations while a supplied condition evaluates to <see langword="false" />.</summary>
public sealed class WindowsMove : IObservable<RxVoid>
{
    /// <summary>The mask that removes the low-order system-command source bits.</summary>
    private const long SystemCommandMask = 0xFFF0;

    /// <summary>The system command that starts an interactive window move.</summary>
    private const long MoveSystemCommand = 0xF010;

    /// <summary>The system command that starts an interactive window resize.</summary>
    private const long ResizeSystemCommand = 0xF000;

    /// <summary>The window message that carries system commands.</summary>
    private const int SystemCommandMessage = 0x0112;

    /// <summary>The error reported when a WPF window source cannot be resolved.</summary>
    private const string SourceUnavailableMessage = "Unable to get an HwndSource for the target window.";

    /// <summary>The condition evaluated for every interactive move request.</summary>
    private readonly Func<bool> _allowMoveCondition;

    /// <summary>The interactive window operations protected by this guard.</summary>
    private readonly WindowsMoveBlockMode _blockMode;

    /// <summary>Initializes a new instance of the <see cref="WindowsMove" /> class that protects movement and resizing.</summary>
    /// <param name="allowMoveCondition">
    /// A condition evaluated for every protected request. Returning <see langword="true" /> allows the operation;
    /// returning <see langword="false" /> blocks it.
    /// </param>
    public WindowsMove(Func<bool> allowMoveCondition)
        : this(allowMoveCondition, WindowsMoveBlockMode.MoveAndResize)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="WindowsMove" /> class for the selected operations.</summary>
    /// <param name="allowMoveCondition">
    /// A condition evaluated for every protected request. Returning <see langword="true" /> allows the operation;
    /// returning <see langword="false" /> blocks it.
    /// </param>
    /// <param name="blockMode">The interactive operations protected by the guard.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="blockMode" /> is not a defined mode.</exception>
    public WindowsMove(Func<bool> allowMoveCondition, WindowsMoveBlockMode blockMode)
    {
        Throw.IfNull(allowMoveCondition);
        if (blockMode is < WindowsMoveBlockMode.MoveOnly or > WindowsMoveBlockMode.MoveAndResize)
        {
            throw new ArgumentOutOfRangeException(nameof(blockMode), blockMode, "The window movement block mode is not defined.");
        }

        _allowMoveCondition = allowMoveCondition;
        _blockMode = blockMode;
    }

    /// <summary>Installs the movement guard on the current process main WPF window.</summary>
    /// <param name="observer">The observer that receives a setup error when the main WPF window cannot be resolved.</param>
    /// <returns>A disposable that removes the window-message hook.</returns>
    public IDisposable Subscribe(IObserver<RxVoid> observer)
    {
        Throw.IfNull(observer);
        using var process = Process.GetCurrentProcess();
        var source = HwndSource.FromHwnd(process.MainWindowHandle);
        if (source is not null)
        {
            return Subscribe(observer, source);
        }

        observer.OnError(new InvalidOperationException(SourceUnavailableMessage));
        return Scope.Empty;
    }

    /// <summary>Installs the movement guard on an HWND source.</summary>
    /// <param name="observer">The observer associated with the hook lifetime.</param>
    /// <param name="hwndSource">The HWND source on which to install the hook.</param>
    /// <returns>A disposable that removes the window-message hook.</returns>
    public IDisposable Subscribe(IObserver<RxVoid> observer, HwndSource hwndSource)
    {
        Throw.IfNull(observer);
        Throw.IfNull(hwndSource);
        return AttachHook(hwndSource.AddHook, hwndSource.RemoveHook);
    }

    /// <summary>Installs the movement guard on a WPF window when its HWND source becomes available.</summary>
    /// <param name="observer">The observer that receives a setup error when the window source cannot be resolved.</param>
    /// <param name="window">The WPF window on which to install the hook.</param>
    /// <returns>A disposable that cancels deferred setup or removes the window-message hook.</returns>
    public IDisposable Subscribe(IObserver<RxVoid> observer, Window window)
    {
        Throw.IfNull(observer);
        Throw.IfNull(window);
        return new WindowHookSubscription(this, observer, window);
    }

    /// <summary>Installs the movement guard on a Windows Forms form and follows native handle recreation.</summary>
    /// <param name="observer">The observer associated with the hook lifetime.</param>
    /// <param name="form">The Windows Forms form on which to install the hook.</param>
    /// <returns>A disposable that cancels deferred setup or removes the window-message hook.</returns>
    public IDisposable Subscribe(IObserver<RxVoid> observer, Form form)
    {
        Throw.IfNull(observer);
        Throw.IfNull(form);
        if (form.IsDisposed)
        {
            observer.OnError(new ObjectDisposedException(nameof(form)));
            return Scope.Empty;
        }

        var listener = new WinProcListener(form);
        IDisposable hookSubscription = AttachHook(listener.AddHook, listener.RemoveHook);
        return Scope.Create(
            (HookSubscription: hookSubscription, Listener: listener),
            static state =>
            {
                state.HookSubscription.Dispose();
                state.Listener.Dispose();
            });
    }

    /// <summary>Installs the movement guard on the HWND source that presents a visual.</summary>
    /// <param name="observer">The observer that receives a setup error when the visual has no WPF window source.</param>
    /// <param name="visual">The visual whose HWND source receives the hook.</param>
    /// <returns>A disposable that removes the window-message hook.</returns>
    public IDisposable Subscribe(IObserver<RxVoid> observer, Visual visual)
    {
        Throw.IfNull(observer);
        Throw.IfNull(visual);
        if (PresentationSource.FromVisual(visual) is HwndSource source)
        {
            return Subscribe(observer, source);
        }

        observer.OnError(new InvalidOperationException(SourceUnavailableMessage));
        return Scope.Empty;
    }

    /// <summary>Installs the movement guard on an abstract message source for deterministic validation.</summary>
    /// <param name="observer">The observer associated with the hook lifetime.</param>
    /// <param name="hookSource">The message source on which to install the hook.</param>
    /// <returns>A disposable that removes the window-message hook.</returns>
    internal IDisposable Subscribe(IObserver<RxVoid> observer, IWindowMessageHookSource hookSource)
    {
        Throw.IfNull(observer);
        Throw.IfNull(hookSource);
        return AttachHook(hookSource.AddHook, hookSource.RemoveHook);
    }

    /// <summary>Processes a window message without requiring a native window.</summary>
    /// <param name="message">The window message identifier.</param>
    /// <param name="wordParameter">The message word parameter.</param>
    /// <param name="handled">A value indicating whether message processing is complete.</param>
    /// <returns>The message result.</returns>
    internal IntPtr ProcessWindowMessageForTesting(int message, IntPtr wordParameter, ref bool handled) =>
        ProcessWindowMessage(message, wordParameter, ref handled);

    /// <summary>Installs a hook and creates its removal lifetime.</summary>
    /// <param name="addHook">The operation that installs the hook.</param>
    /// <param name="removeHook">The operation that removes the hook.</param>
    /// <returns>A disposable that removes the installed hook.</returns>
    private IDisposable AttachHook(Action<HwndSourceHook> addHook, Action<HwndSourceHook> removeHook)
    {
        HwndSourceHook hook = WndProc;
        addHook(hook);
        return Scope.Create(
            (RemoveHook: removeHook, Hook: hook),
            static state => state.RemoveHook(state.Hook));
    }

    /// <summary>Processes the messages relevant to interactive window movement.</summary>
    /// <param name="message">The window message identifier.</param>
    /// <param name="wordParameter">The message word parameter.</param>
    /// <param name="handled">A value indicating whether message processing is complete.</param>
    /// <returns>The message result.</returns>
    private IntPtr ProcessWindowMessage(int message, IntPtr wordParameter, ref bool handled)
    {
        var systemCommand = wordParameter.ToInt64() & SystemCommandMask;
        var protectsCommand = systemCommand == MoveSystemCommand
            || (_blockMode == WindowsMoveBlockMode.MoveAndResize && systemCommand == ResizeSystemCommand);
        if (message == SystemCommandMessage && protectsCommand && !_allowMoveCondition())
        {
            handled = true;
        }

        return IntPtr.Zero;
    }

    /// <summary>Receives messages from the hooked WPF window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="message">The window message identifier.</param>
    /// <param name="wordParameter">The message word parameter.</param>
    /// <param name="longParameter">The message long parameter.</param>
    /// <param name="handled">A value indicating whether message processing is complete.</param>
    /// <returns>The message result.</returns>
    private IntPtr WndProc(
        IntPtr windowHandle,
        int message,
        IntPtr wordParameter,
        IntPtr longParameter,
        ref bool handled)
    {
        _ = windowHandle;
        _ = longParameter;
        return ProcessWindowMessage(message, wordParameter, ref handled);
    }

    /// <summary>Manages deferred WPF window-source initialization and the installed hook lifetime.</summary>
    private sealed class WindowHookSubscription : IDisposable
    {
        /// <summary>The movement guard installed when the source becomes available.</summary>
        private readonly WindowsMove _guard;

        /// <summary>The observer associated with the hook lifetime.</summary>
        private readonly IObserver<RxVoid> _observer;

        /// <summary>The target WPF window.</summary>
        private readonly Window _window;

        /// <summary>The active hook lifetime.</summary>
        private IDisposable _hookSubscription;

        /// <summary>Indicates whether the source-initialized handler remains registered.</summary>
        private bool _sourceInitializedRegistered;

        /// <summary>Indicates whether this lifetime has been disposed.</summary>
        private bool _isDisposed;

        /// <summary>Initializes a new instance of the <see cref="WindowHookSubscription" /> class.</summary>
        /// <param name="guard">The movement guard to install.</param>
        /// <param name="observer">The observer associated with the hook lifetime.</param>
        /// <param name="window">The target WPF window.</param>
        internal WindowHookSubscription(WindowsMove guard, IObserver<RxVoid> observer, Window window)
        {
            _guard = guard;
            _observer = observer;
            _window = window;

            if (PresentationSource.FromVisual(window) is HwndSource source)
            {
                _hookSubscription = guard.Subscribe(observer, source);
                return;
            }

            window.SourceInitialized += OnSourceInitialized;
            _sourceInitializedRegistered = true;
        }

        /// <summary>Cancels deferred initialization or removes the installed hook.</summary>
        void IDisposable.Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            if (_sourceInitializedRegistered)
            {
                _window.SourceInitialized -= OnSourceInitialized;
                _sourceInitializedRegistered = false;
            }

            _hookSubscription?.Dispose();
            _hookSubscription = null;
        }

        /// <summary>Installs the movement guard after WPF creates the window source.</summary>
        /// <param name="sender">The event source.</param>
        /// <param name="args">The event arguments.</param>
        private void OnSourceInitialized(object sender, EventArgs args)
        {
            _ = sender;
            _ = args;
            _window.SourceInitialized -= OnSourceInitialized;
            _sourceInitializedRegistered = false;
            if (PresentationSource.FromVisual(_window) is HwndSource source)
            {
                _hookSubscription = _guard.Subscribe(_observer, source);
                return;
            }

            _observer.OnError(new InvalidOperationException(SourceUnavailableMessage));
        }
    }
}
