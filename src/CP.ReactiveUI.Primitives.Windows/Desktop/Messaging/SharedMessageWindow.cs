// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>Provides a shared observable stream of Windows messages using a message-only window on a dedicated thread.</summary>
public static class SharedMessageWindow
{
    /// <summary>The hidden message window title.</summary>
    private const string WindowTitle = "CP.ReactiveUI.Primitives.Windows.MessageWindow";

    /// <summary>The tool window extended style.</summary>
    private const uint ExtendedToolWindowStyle = 128U;

    /// <summary>The application window extended style.</summary>
    private const uint ExtendedAppWindowStyle = 262_144U;

    /// <summary>The popup window style.</summary>
    private const uint PopupWindowStyle = 2_147_483_648U;

    /// <summary>The extended style used for the message-only window.</summary>
    private const uint MessageOnlyWindowExtendedStyle = ExtendedToolWindowStyle | ExtendedAppWindowStyle;

    /// <summary>Synchronizes access to the shared stream.</summary>
#if NET9_0_OR_GREATER
    private static readonly System.Threading.Lock SyncRoot = new();
#else
    private static readonly object SyncRoot = new();
#endif

    /// <summary>Publishes the current message window handle.</summary>
    private static readonly BehaviorSignal<nint> HandleSubject = new((IntPtr)0);

    /// <summary>The optional message stream override used by deterministic tests.</summary>
    private static IObservable<WindowMessage> _messageStreamOverride;

    /// <summary>The optional handle stream override used by deterministic tests.</summary>
    private static IObservable<nint> _handleChangesOverride;

    /// <summary>The optional current handle override used by deterministic tests.</summary>
    private static nint? _handleOverride;

    /// <summary>The optional default-procedure override used by deterministic tests.</summary>
    private static Func<nint, WindowsMessages, nint, nint, nuint> _defaultWindowProcedureOverride;

    /// <summary>The optional quit-message operation override used by deterministic tests.</summary>
    private static Action<int> _postQuitMessageOverride;

    /// <summary>The shared message stream.</summary>
    private static IObservable<WindowMessage> _sharedStream;

    /// <summary>Gets the current handle value of the message window, or zero if no window is currently active.</summary>
    public static long Handle => _handleOverride ?? HandleSubject.Value;

    /// <summary>Gets an observable sequence of all window messages received by the application.</summary>
    /// <remarks>The returned observable is shared among all subscribers. Subscribing to this property allows
    /// monitoring of window messages as they occur. Unsubscribing from all observers will automatically release
    /// resources associated with the stream.</remarks>
    public static IObservable<WindowMessage> WindowMessageEvents
    {
        get
        {
            lock (SyncRoot)
            {
                if (_messageStreamOverride is not null)
                {
                    return _messageStreamOverride;
                }

                _sharedStream ??= CreateBaseStream().ShareLatest();

                return _sharedStream;
            }
        }
    }

    /// <summary>Gets the current native handle of the message window, or zero if no window is currently active.</summary>
    internal static nint NativeHandle => _handleOverride ?? HandleSubject.Value;

    /// <summary>Gets an observable sequence of message-window handle value changes.</summary>
    /// <returns>An observable sequence containing the current handle value and subsequent changes.</returns>
    public static IObservable<long> ObserveHandleChanges() => ObserveNativeHandleChanges().Select(static windowHandle => (long)windowHandle);

    /// <summary>Gets the shared message loop.</summary>
    /// <returns>An observable sequence of Windows messages.</returns>
    public static IObservable<WindowMessage> ObserveWindowMessages() => WindowMessageEvents;

    /// <summary>Subscribes to the shared message loop with lifecycle callbacks.</summary>
    /// <param name="onSetup">Invoked with the window handle value when the window is created or immediately if it already exists.</param>
    /// <param name="onTeardown">
    /// Invoked with the window handle value when the window is destroyed or when the subscription is disposed.
    /// </param>
    /// <returns>An observable sequence of Windows messages.</returns>
    public static IObservable<WindowMessage> ObserveWindowMessages(Action<long> onSetup, Action<long> onTeardown) =>
        ListenCore(windowHandle => onSetup(windowHandle), windowHandle => onTeardown(windowHandle));

    /// <summary>Posts a quit message to the current thread for deterministic message-loop tests.</summary>
    internal static void PostQuitMessageForTesting() => NativeMethods.PostQuitMessage(0);

    /// <summary>Overrides message and handle streams for deterministic tests.</summary>
    /// <param name="messages">The message stream to expose.</param>
    /// <param name="handleChanges">The handle changes to expose.</param>
    /// <param name="currentHandle">The current handle value to expose.</param>
    /// <returns>A scope that restores the native message-window streams.</returns>
    internal static IDisposable OverrideStreamsForTesting(IObservable<WindowMessage> messages, IObservable<nint> handleChanges, nint currentHandle)
    {
        Throw.IfNull(messages);
        Throw.IfNull(handleChanges);
        var previousMessages = _messageStreamOverride;
        IObservable<IntPtr> previousHandleChanges = _handleChangesOverride;
        var previousHandle = _handleOverride;
        lock (SyncRoot)
        {
            _messageStreamOverride = messages;
            _handleChangesOverride = handleChanges;
            _handleOverride = currentHandle;
        }

        return Scope.Create((previousMessages, previousHandleChanges, previousHandle), static previous =>
        {
            lock (SyncRoot)
            {
                (_messageStreamOverride, _handleChangesOverride, _handleOverride) = previous;
            }
        });
    }

    /// <summary>Overrides native message dispatch operations for deterministic tests.</summary>
    /// <param name="defaultWindowProcedure">The replacement default window procedure.</param>
    /// <param name="postQuitMessage">The replacement quit-message operation.</param>
    /// <returns>A scope that restores the production operations.</returns>
    internal static IDisposable OverrideMessageDispatchForTesting(
        Func<nint, WindowsMessages, nint, nint, nuint> defaultWindowProcedure,
        Action<int> postQuitMessage)
    {
        Throw.IfNull(defaultWindowProcedure);
        Throw.IfNull(postQuitMessage);
        (Func<nint, WindowsMessages, nint, nint, nuint> DefaultWindowProcedure, Action<int> PostQuitMessage) previous =
            (_defaultWindowProcedureOverride, _postQuitMessageOverride);
        _defaultWindowProcedureOverride = defaultWindowProcedure;
        _postQuitMessageOverride = postQuitMessage;
        return Scope.Create(previous, static previousOperations =>
        {
            (_defaultWindowProcedureOverride, _postQuitMessageOverride) = previousOperations;
        });
    }

    /// <summary>Processes a message through the production window-message handler for deterministic tests.</summary>
    /// <param name="observer">The observer that receives messages.</param>
    /// <param name="windowHandle">The native window handle.</param>
    /// <param name="message">The Windows message.</param>
    /// <param name="wordParam">The word parameter.</param>
    /// <param name="longParam">The long parameter.</param>
    /// <returns>The message result.</returns>
    internal static nuint ProcessWindowMessageForTesting(
        IObserver<WindowMessage> observer,
        nint windowHandle,
        WindowsMessages message,
        nint wordParam,
        nint longParam) =>
        ProcessWindowMessage(observer, windowHandle, message, wordParam, longParam);

    /// <summary>Gets native handle changes, using an override when one is active.</summary>
    /// <returns>The handle changes.</returns>
    private static IObservable<nint> ObserveNativeHandleChanges() => _handleChangesOverride ?? HandleSubject;

    /// <summary>Creates a lifecycle-aware shared message subscription.</summary>
    /// <param name="onSetup">The setup callback.</param>
    /// <param name="onTeardown">The teardown callback.</param>
    /// <returns>An observable sequence of Windows messages.</returns>
    private static IObservable<WindowMessage> ListenCore(Action<nint> onSetup, Action<nint> onTeardown) => ReactiveSignal.Create<WindowMessage>(observer =>
        {
            ListenState state = new(onSetup, onTeardown);
            var handleSubscription = ObserveNativeHandleChanges().Subscribe(windowHandle =>
            {
                state.UpdateWindowHandle(windowHandle);
            });
            var messageSubscription = WindowMessageEvents.Subscribe(observer);
            state.SetSubscriptions(handleSubscription, messageSubscription);
            return Scope.Create(state, static listenState =>
            {
                listenState.DisposeCore();
            });
        });

    /// <summary>Creates the base observable sequence.</summary>
    /// <returns>An observable sequence of Windows messages.</returns>
    private static IObservable<WindowMessage> CreateBaseStream() => ReactiveSignal.Create((Func<IObserver<WindowMessage>, IDisposable>)CreateMessageSubscription);

    /// <summary>Creates a subscription that owns the message-loop thread.</summary>
    /// <param name="observer">The observer that receives messages.</param>
    /// <returns>A disposable that stops the message loop.</returns>
    private static IDisposable CreateMessageSubscription(IObserver<WindowMessage> observer)
    {
        MessageSubscriptionState state = new(observer, $"MsgLoop_{Guid.NewGuid():N}");
        Thread thread = new(static threadState =>
        {
            RunMessageLoop((MessageSubscriptionState)threadState);
        });
        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start(state);
        return Scope.Create(state, static subscriptionState =>
        {
            subscriptionState.DisposeCore();
        });
    }

    /// <summary>Runs the message loop on the owning thread.</summary>
    /// <param name="state">The message subscription state.</param>
    private static void RunMessageLoop(MessageSubscriptionState state)
    {
        var observer = state.Observer;
        var className = state.ClassName;
        WndProc windowProcedure = (windowHandle2, message2, wordParam, longParam) => ProcessWindowMessage(observer, windowHandle2, message2, wordParam, longParam);
        nint instanceHandle = Kernel32Api.GetModuleHandle(null);
        _ = NativeMethods.RegisterClassEx(windowProcedure, instanceHandle, className);
        var windowHandle = NativeMethods.CreateWindowEx(new()
        {
            ExtendedStyle = MessageOnlyWindowExtendedStyle,
            ClassName = className,
            WindowName = WindowTitle,
            Style = PopupWindowStyle,
            X = 0,
            Y = 0,
            Width = 0,
            Height = 0,
            ParentWindowHandle = 0,
            MenuHandle = 0,
            InstanceHandle = (long)instanceHandle,
            Parameter = 0,
        });
        if (windowHandle != 0)
        {
            state.SetWindowHandle(windowHandle);
            HandleSubject.OnNext(windowHandle);
        }
        while (MessageLoop.TryGetMessage(out var message))
        {
            MessageLoop.Dispatch(ref message);
        }

        if (windowHandle != 0)
        {
            HandleSubject.OnNext((IntPtr)0);
            state.SetWindowHandle(0);
        }

        _ = NativeMethods.UnregisterClass(className, instanceHandle);
        GC.KeepAlive(windowProcedure);
    }

    /// <summary>Processes a native window message.</summary>
    /// <param name="observer">The observer that receives messages.</param>
    /// <param name="windowHandle">The native window handle.</param>
    /// <param name="message">The Windows message.</param>
    /// <param name="wordParam">The word parameter.</param>
    /// <param name="longParam">The long parameter.</param>
    /// <returns>The message result.</returns>
    private static nuint ProcessWindowMessage(IObserver<WindowMessage> observer, nint windowHandle, WindowsMessages message, nint wordParam, nint longParam)
    {
        if (message == WindowsMessages.WM_DESTROY)
        {
            if (_postQuitMessageOverride is null)
            {
                NativeMethods.PostQuitMessage(0);
            }
            else
            {
                _postQuitMessageOverride(0);
            }

            return 0U;
        }

        WindowMessage windowMessage = new(windowHandle, message, wordParam, longParam);
        observer.OnNext(windowMessage);
        if (windowMessage.Handled)
        {
            return checked((nuint)windowMessage.Result);
        }

        return _defaultWindowProcedureOverride is null
            ? NativeMethods.DefWindowProc(windowHandle, message, wordParam, longParam)
            : _defaultWindowProcedureOverride(windowHandle, message, wordParam, longParam);
    }

    /// <summary>Represents a native window class registration structure.</summary>
    private readonly unsafe struct WindowClassEx
    {
        /// <summary>The structure size.</summary>
        private readonly uint _size;

        /// <summary>The class style.</summary>
        private readonly uint _style;

        /// <summary>The window procedure.</summary>
        private readonly nint _windowProcedure;

        /// <summary>The extra class byte count.</summary>
        private readonly int _classExtraBytes;

        /// <summary>The extra window byte count.</summary>
        private readonly int _windowExtraBytes;

        /// <summary>The instance handle.</summary>
        private readonly nint _instanceHandle;

        /// <summary>The icon handle.</summary>
        private readonly nint _iconHandle;

        /// <summary>The cursor handle.</summary>
        private readonly nint _cursorHandle;

        /// <summary>The background brush handle.</summary>
        private readonly nint _backgroundBrushHandle;

        /// <summary>The menu name.</summary>
        private readonly unsafe char* _menuName;

        /// <summary>The class name.</summary>
        private readonly unsafe char* _className;

        /// <summary>The small icon handle.</summary>
        private readonly nint _smallIconHandle;

        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.SharedMessageWindow.WindowClassEx" /> struct.</summary>
        /// <param name="windowProcedure">The window procedure pointer.</param>
        /// <param name="instanceHandle">The instance handle.</param>
        /// <param name="className">The window class name.</param>
        public unsafe WindowClassEx(nint windowProcedure, nint instanceHandle, char* className)
        {
            _size = checked((uint)Marshal.SizeOf<WindowClassEx>());
            _style = 0U;
            _windowProcedure = windowProcedure;
            _classExtraBytes = 0;
            _windowExtraBytes = 0;
            _instanceHandle = instanceHandle;
            _iconHandle = 0;
            _cursorHandle = 0;
            _backgroundBrushHandle = 0;
            _menuName = null;
            _className = className;
            _smallIconHandle = 0;
        }

        /// <summary>Reads fields so source analyzers do not treat ABI-only values as dead writes.</summary>
        public void MarkFieldsAsRead()
        {
            _ = _size;
            _ = _style;
            _ = _windowProcedure;
            _ = _classExtraBytes;
            _ = _windowExtraBytes;
            _ = _instanceHandle;
            _ = _iconHandle;
            _ = _cursorHandle;
            _ = _backgroundBrushHandle;
            _ = _menuName;
            _ = _className;
            _ = _smallIconHandle;
        }
    }

    /// <summary>Contains native methods used by the shared message window.</summary>
    internal static class NativeMethods
    {
        /// <summary>The User32 library name.</summary>
        private const string User32Dll = "user32.dll";

        /// <summary>The User32 module handle.</summary>
        private static readonly Lazy<nint> User32Module = new(static () => NativeLibrary.Load(Path.Combine(Environment.SystemDirectory, "user32.dll")));

        /// <summary>The RegisterClassExW export pointer.</summary>
        private static readonly Lazy<nint> RegisterClassExExport = new(static () => NativeLibrary.GetExport(User32Module.Value, "RegisterClassExW"));

        /// <summary>The UnregisterClassW export pointer.</summary>
        private static readonly Lazy<nint> UnregisterClassExport = new(static () => NativeLibrary.GetExport(User32Module.Value, "UnregisterClassW"));

        /// <summary>The CreateWindowExW export pointer.</summary>
        private static readonly Lazy<nint> CreateWindowExExport = new(static () => NativeLibrary.GetExport(User32Module.Value, "CreateWindowExW"));

        /// <summary>The DefWindowProcW export pointer.</summary>
        private static readonly Lazy<nint> DefWindowProcExport = new(static () => NativeLibrary.GetExport(User32Module.Value, "DefWindowProcW"));

        /// <summary>The PostQuitMessage export pointer.</summary>
        private static readonly Lazy<nint> PostQuitMessageExport = new(static () => NativeLibrary.GetExport(User32Module.Value, nameof(PostQuitMessage)));

        /// <summary>The PostMessageW export pointer.</summary>
        private static readonly Lazy<nint> PostMessageExport = new(static () => NativeLibrary.GetExport(User32Module.Value, "PostMessageW"));

        /// <summary>The class-registration operation.</summary>
        private static Func<nint, ushort> _registerClassExOperation = InvokeRegisterClassEx;

        /// <summary>The class-unregistration operation.</summary>
        private static Func<nint, nint, bool> _unregisterClassOperation = InvokeUnregisterClass;

        /// <summary>The window-creation operation.</summary>
        private static Func<CreateWindowArguments, nint> _createWindowExOperation = InvokeCreateWindowEx;

        /// <summary>Overrides string-pinning native operations for deterministic tests.</summary>
        /// <param name="registerClassEx">The class-registration operation.</param>
        /// <param name="unregisterClass">The class-unregistration operation.</param>
        /// <param name="createWindowEx">The window-creation operation.</param>
        /// <returns>A scope that restores the production operations.</returns>
        internal static IDisposable OverrideStringPinningOperationsForTesting(
            Func<nint, ushort> registerClassEx,
            Func<nint, nint, bool> unregisterClass,
            Func<CreateWindowArguments, nint> createWindowEx)
        {
            Throw.IfNull(registerClassEx);
            Throw.IfNull(unregisterClass);
            Throw.IfNull(createWindowEx);
            (Func<nint, ushort> RegisterClassEx, Func<nint, nint, bool> UnregisterClass, Func<CreateWindowArguments, nint> CreateWindowEx) previous =
                (_registerClassExOperation, _unregisterClassOperation, _createWindowExOperation);
            (_registerClassExOperation, _unregisterClassOperation, _createWindowExOperation) = (registerClassEx, unregisterClass, createWindowEx);
            return Scope.Create(previous, static previousOperations =>
            {
                (_registerClassExOperation, _unregisterClassOperation, _createWindowExOperation) = previousOperations;
            });
        }

        /// <summary>Registers a window class.</summary>
        /// <param name="windowProcedure">The window procedure.</param>
        /// <param name="instanceHandle">The instance handle.</param>
        /// <param name="className">The window class name.</param>
        /// <returns>The registered class atom.</returns>
        internal static unsafe ushort RegisterClassEx(WndProc windowProcedure, nint instanceHandle, string className)
        {
            fixed (char* classNamePointer = className)
            {
                WindowClassEx windowClass = new(Marshal.GetFunctionPointerForDelegate(windowProcedure), instanceHandle, classNamePointer);
                windowClass.MarkFieldsAsRead();
                return _registerClassExOperation((nint)(&windowClass));
            }
        }

        /// <summary>Unregisters a window class.</summary>
        /// <param name="className">The window class name.</param>
        /// <param name="instanceHandle">The instance handle.</param>
        /// <returns><c>true</c> when the class was unregistered; otherwise <c>false</c>.</returns>
        internal static unsafe bool UnregisterClass(string className, nint instanceHandle)
        {
            fixed (char* classNamePointer = className)
            {
                return _unregisterClassOperation((nint)classNamePointer, instanceHandle);
            }
        }

        /// <summary>Creates a window.</summary>
        /// <param name="request">The window creation request.</param>
        /// <returns>The created window handle.</returns>
        internal static unsafe nint CreateWindowEx(CreateWindowRequest request)
        {
            fixed (char* classNamePointer = request.ClassName)
            {
                fixed (char* windowNamePointer = request.WindowName)
                {
                    CreateWindowArguments arguments = new()
                    {
                        ExtendedStyle = request.ExtendedStyle,
                        ClassNamePointer = (long)(nint)classNamePointer,
                        WindowNamePointer = (long)(nint)windowNamePointer,
                        Style = request.Style,
                        X = request.X,
                        Y = request.Y,
                        Width = request.Width,
                        Height = request.Height,
                        ParentWindowHandle = request.ParentWindowHandle,
                        MenuHandle = request.MenuHandle,
                        InstanceHandle = request.InstanceHandle,
                        Parameter = request.Parameter,
                    };

                    return _createWindowExOperation(arguments);
                }
            }
        }

        /// <summary>Calls the default window procedure.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="message">The message.</param>
        /// <param name="wordParam">The word parameter.</param>
        /// <param name="longParam">The long parameter.</param>
        /// <returns>The message result.</returns>
        internal static unsafe nuint DefWindowProc(nint windowHandle, WindowsMessages message, nint wordParam, nint longParam) =>
            ((delegate* unmanaged[Stdcall]<IntPtr, WindowsMessages, IntPtr, IntPtr, UIntPtr>)checked((nuint)DefWindowProcExport.Value))(
                windowHandle,
                message,
                wordParam,
                longParam);

        /// <summary>Posts a quit message.</summary>
        /// <param name="exitCode">The exit code.</param>
        internal static unsafe void PostQuitMessage(int exitCode) => ((delegate* unmanaged[Stdcall]<int, void>)checked((nuint)PostQuitMessageExport.Value))(exitCode);

        /// <summary>Posts a message to a window.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="message">The message.</param>
        /// <param name="wordParam">The word parameter.</param>
        /// <param name="longParam">The long parameter.</param>
        /// <returns><c>true</c> when the message was posted; otherwise <c>false</c>.</returns>
        internal static unsafe bool PostMessage(nint windowHandle, uint message, nint wordParam, nint longParam) =>
            ((delegate* unmanaged[Stdcall]<nint, uint, nint, nint, int>)checked((nuint)PostMessageExport.Value))(windowHandle, message, wordParam, longParam) != 0;

        /// <summary>Invokes the native class-registration operation.</summary>
        /// <param name="windowClassPointer">The native class structure pointer.</param>
        /// <returns>The registered class atom.</returns>
        private static unsafe ushort InvokeRegisterClassEx(nint windowClassPointer) =>
            ((delegate* unmanaged[Stdcall]<WindowClassEx*, ushort>)checked((nuint)RegisterClassExExport.Value))((WindowClassEx*)windowClassPointer);

        /// <summary>Invokes the native class-unregistration operation.</summary>
        /// <param name="classNamePointer">The class name pointer.</param>
        /// <param name="instanceHandle">The module instance handle.</param>
        /// <returns><c>true</c> when the class was unregistered.</returns>
        private static unsafe bool InvokeUnregisterClass(nint classNamePointer, nint instanceHandle) =>
            ((delegate* unmanaged[Stdcall]<char*, nint, int>)checked((nuint)UnregisterClassExport.Value))((char*)classNamePointer, instanceHandle) != 0;

        /// <summary>Invokes the native window-creation operation.</summary>
        /// <param name="arguments">The pinned native window-creation arguments.</param>
        /// <returns>The created window handle.</returns>
        private static unsafe nint InvokeCreateWindowEx(CreateWindowArguments arguments) =>
            ((delegate* unmanaged[Stdcall]<uint, char*, char*, uint, int, int, int, int, nint, nint, nint, nint, nint>)checked((nuint)CreateWindowExExport.Value))(
                arguments.ExtendedStyle,
                (char*)(nint)arguments.ClassNamePointer,
                (char*)(nint)arguments.WindowNamePointer,
                arguments.Style,
                arguments.X,
                arguments.Y,
                arguments.Width,
                arguments.Height,
                (nint)arguments.ParentWindowHandle,
                (nint)arguments.MenuHandle,
                (nint)arguments.InstanceHandle,
                (nint)arguments.Parameter);

        /// <summary>Stores CreateWindowEx request values before string pinning.</summary>
        internal sealed class CreateWindowRequest
        {
            /// <summary>Gets the extended window style.</summary>
            public uint ExtendedStyle { get; init; }

            /// <summary>Gets the window class name.</summary>
            public string ClassName { get; init; }

            /// <summary>Gets the window name.</summary>
            public string WindowName { get; init; }

            /// <summary>Gets the window style.</summary>
            public uint Style { get; init; }

            /// <summary>Gets the x-coordinate.</summary>
            public int X { get; init; }

            /// <summary>Gets the y-coordinate.</summary>
            public int Y { get; init; }

            /// <summary>Gets the window width.</summary>
            public int Width { get; init; }

            /// <summary>Gets the window height.</summary>
            public int Height { get; init; }

            /// <summary>Gets the parent window handle.</summary>
            public long ParentWindowHandle { get; init; }

            /// <summary>Gets the menu handle.</summary>
            public long MenuHandle { get; init; }

            /// <summary>Gets the instance handle.</summary>
            public long InstanceHandle { get; init; }

            /// <summary>Gets the creation parameter.</summary>
            public long Parameter { get; init; }
        }

        /// <summary>Stores pinned CreateWindowEx argument values.</summary>
        internal sealed class CreateWindowArguments
        {
            /// <summary>Gets the extended window style.</summary>
            public uint ExtendedStyle { get; init; }

            /// <summary>Gets the window class-name pointer snapshot.</summary>
            public long ClassNamePointer { get; init; }

            /// <summary>Gets the window-name pointer snapshot.</summary>
            public long WindowNamePointer { get; init; }

            /// <summary>Gets the window style.</summary>
            public uint Style { get; init; }

            /// <summary>Gets the x-coordinate.</summary>
            public int X { get; init; }

            /// <summary>Gets the y-coordinate.</summary>
            public int Y { get; init; }

            /// <summary>Gets the window width.</summary>
            public int Width { get; init; }

            /// <summary>Gets the window height.</summary>
            public int Height { get; init; }

            /// <summary>Gets the parent window handle.</summary>
            public long ParentWindowHandle { get; init; }

            /// <summary>Gets the menu handle.</summary>
            public long MenuHandle { get; init; }

            /// <summary>Gets the instance handle.</summary>
            public long InstanceHandle { get; init; }

            /// <summary>Gets the creation parameter.</summary>
            public long Parameter { get; init; }
        }
    }

    /// <summary>Stores per-listener state without closure captures.</summary>
    /// <param name="onSetup">The setup callback.</param>
    /// <param name="onTeardown">The teardown callback.</param>
    private sealed class ListenState(Action<nint> onSetup, Action<nint> onTeardown)
    {
        /// <summary>Synchronizes active window handle changes.</summary>
#if NET9_0_OR_GREATER
        private readonly System.Threading.Lock _handleLock = new();
#else
        private readonly object _handleLock = new();
#endif

        /// <summary>Stores the handle subscription.</summary>
        private IDisposable _handleSubscription;

        /// <summary>Stores the message subscription.</summary>
        private IDisposable _messageSubscription;

        /// <summary>Stores the active window handle.</summary>
        private nint _activeWindowHandle;

        /// <summary>Disposes the subscriptions and invokes teardown if a handle is active.</summary>
        public void DisposeCore()
        {
            lock (_handleLock)
            {
                if (_activeWindowHandle != 0)
                {
                    onTeardown(_activeWindowHandle);
                    _activeWindowHandle = 0;
                }
            }

            _handleSubscription.Dispose();
            _messageSubscription.Dispose();
        }

        /// <summary>Sets the subscriptions that this state owns.</summary>
        /// <param name="handleSubscription">The handle subscription.</param>
        /// <param name="messageSubscription">The message subscription.</param>
        public void SetSubscriptions(IDisposable handleSubscription, IDisposable messageSubscription)
        {
            _handleSubscription = handleSubscription;
            _messageSubscription = messageSubscription;
        }

        /// <summary>Updates the active window handle and invokes setup or teardown callbacks.</summary>
        /// <param name="windowHandle">The new window handle.</param>
        public void UpdateWindowHandle(nint windowHandle)
        {
            lock (_handleLock)
            {
                if (windowHandle != 0)
                {
                    _activeWindowHandle = windowHandle;
                    onSetup(_activeWindowHandle);
                }
                else if (_activeWindowHandle != 0)
                {
                    onTeardown(_activeWindowHandle);
                    _activeWindowHandle = 0;
                }
            }
        }
    }

    /// <summary>Stores message-loop subscription state without closure captures.</summary>
    /// <param name="observer">The observer that receives messages.</param>
    /// <param name="className">The window class name.</param>
    private sealed class MessageSubscriptionState(IObserver<WindowMessage> observer, string className)
    {
        /// <summary>The message used to destroy the message-only window.</summary>
        private const uint DestroyMessage = 2U;

        /// <summary>Synchronizes message-window lifecycle state.</summary>
#if NET9_0_OR_GREATER
        private readonly System.Threading.Lock _windowHandleLock = new();
#else
        private readonly object _windowHandleLock = new();
#endif

        /// <summary>Tracks a disposal request that arrived before window creation completed.</summary>
        private bool _disposeRequested;

        /// <summary>Stores the created window handle.</summary>
        private nint _windowHandle;

        /// <summary>Gets the observer that receives messages.</summary>
        public IObserver<WindowMessage> Observer { get; } = observer;

        /// <summary>Gets the window class name.</summary>
        public string ClassName { get; } = className;

        /// <summary>Posts the destroy message to the owned message window.</summary>
        public void DisposeCore()
        {
            nint windowHandle;
            lock (_windowHandleLock)
            {
                _disposeRequested = true;
                windowHandle = _windowHandle;
            }

            if (windowHandle != 0)
            {
                _ = NativeMethods.PostMessage(windowHandle, DestroyMessage, 0, 0);
            }
        }

        /// <summary>Sets the created window handle.</summary>
        /// <param name="windowHandle">The created window handle.</param>
        public void SetWindowHandle(nint windowHandle)
        {
            bool shouldDestroy;
            lock (_windowHandleLock)
            {
                _windowHandle = windowHandle;
                shouldDestroy = _disposeRequested && windowHandle != 0;
            }

            if (shouldDestroy)
            {
                _ = NativeMethods.PostMessage(windowHandle, DestroyMessage, 0, 0);
            }
        }
    }
}
