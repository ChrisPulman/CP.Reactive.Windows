// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using ReactiveUI.Primitives.Disposables;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Lifecycle;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Lifecycle;
#endif
/// <summary>
/// Provides methods for registering and unregistering the current application for automatic restart using Windows
/// Restart Manager, as well as utilities for detecting restart events and handling system shutdown notifications.
/// </summary>
/// <remarks>Use this class to enable your application to be automatically restarted after system updates or
/// shutdowns managed by Windows Restart Manager. It also provides helper methods for detecting restart conditions and
/// responding to session end events, allowing applications to preserve state and handle shutdowns gracefully.</remarks>
#if NETFRAMEWORK
public static class ApplicationRestartManager
#else
public static partial class ApplicationRestartManager
#endif
{
    /// <summary>Cached shared end-session observable.</summary>
    private static IObservable<EndSessionMessage> _endSessionObservable;

    /// <summary>Restart registration operations used by this process.</summary>
    private static ApplicationRestartOperations _restartOperations =
        new(NativeMethods.RegisterApplicationRestart, NativeMethods.UnregisterApplicationRestart);

    /// <summary>Gets the maximum length for the command line arguments, in characters.</summary>
    public static int RestartMaxCmdLine { get; } = 1024;

    /// <summary>Gets the alias for RestartMaxCmdLine for backward compatibility.</summary>
    public static int MaxCommandLineLength => RestartMaxCmdLine;

    /// <summary>Registers the current application for automatic restart.</summary>
    /// <exception cref="T:System.ArgumentException">Thrown when command line arguments exceed maximum length.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when registration fails.</exception>
    public static void RegisterForRestart() => RegisterForRestart(null, ApplicationRestartFlags.None);

    /// <summary>Registers the current application for automatic restart.</summary>
    /// <param name="commandLineArgs">Command-line arguments to pass to the application when it is restarted.</param>
    public static void RegisterForRestart(string commandLineArgs) => RegisterForRestart(commandLineArgs, ApplicationRestartFlags.None);

    /// <summary>
    ///     Registers the current application for automatic restart.
    ///     When the Restart Manager shuts down the application during an update, it will be automatically restarted afterwards.
    /// </summary>
    /// <param name="commandLineArgs">
    ///     Command-line arguments to pass to the application when it is restarted.
    ///     Do not include the executable name - it will be added automatically.
    ///     Maximum length is 1024 characters. Use null or empty string to clear previous registration.
    /// </param>
    /// <param name="flags">
    ///     Flags that control when the application should NOT be restarted.
    ///     Default is None, meaning the application will always be restarted.
    /// </param>
    /// <exception cref="T:System.ArgumentException">Thrown when command line arguments exceed maximum length.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when registration fails.</exception>
    public static void RegisterForRestart(string commandLineArgs, ApplicationRestartFlags flags)
    {
        if (!string.IsNullOrEmpty(commandLineArgs) && commandLineArgs.Length > MaxCommandLineLength)
        {
            throw new ArgumentException($"Command line arguments cannot exceed {MaxCommandLineLength} characters", nameof(commandLineArgs));
        }

        int result = _restartOperations.Register(commandLineArgs, flags);
        if (result != 0)
        {
            throw new Win32Exception(result, "Failed to register application for restart");
        }
    }

    /// <summary>
    ///     Unregisters the current application from automatic restart.
    ///     Call this if you no longer want the application to be restarted by Restart Manager.
    /// </summary>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when unregistration fails.</exception>
    public static void UnregisterForRestart()
    {
        int result = _restartOperations.Unregister();
        if (result != 0)
        {
            throw new Win32Exception(result, "Failed to unregister application from restart");
        }
    }

    /// <summary>
    ///     Checks if the current process was started by Restart Manager.
    ///     This allows the application to detect if it was automatically restarted after an update.
    /// </summary>
    /// <returns>True if the application was restarted by Restart Manager, false otherwise.</returns>
    /// <remarks>Applications should implement their own restart detection when they use custom arguments.</remarks>
    public static bool WasRestartRequested() => WasRestartRequested(Environment.GetCommandLineArgs());

    /// <summary>
    ///     Gets the command-line arguments that were passed to the current process.
    ///     Applications can use this to implement their own restart detection logic
    ///     based on the specific arguments they registered via RegisterForRestart().
    /// </summary>
    /// <returns>Array of command-line arguments, excluding the executable path.</returns>
    public static string[] GetRestartCommandLineArgs() => GetRestartCommandLineArgs(Environment.GetCommandLineArgs());

    /// <summary>
    ///     Creates an observable stream that listens for WM_QUERYENDSESSION and WM_ENDSESSION messages.
    ///     This allows applications to be notified when the system is about to shut down or restart.
    /// </summary>
    /// <returns>
    ///     An observable stream that emits EndSessionMessage values when a session end event occurs.
    ///     Subscribe to this observable to handle shutdown requests gracefully.
    /// </returns>
    public static IObservable<EndSessionMessage> ObserveEndSessionMessages() => ObserveEndSessionMessages(null, null);

    /// <summary>Creates an observable stream that listens for WM_QUERYENDSESSION and WM_ENDSESSION messages.</summary>
    /// <param name="onQuerySession">Handler called when a query end-session message is received.</param>
    /// <returns>An observable stream that emits EndSessionMessage values when a session end event occurs.</returns>
    public static IObservable<EndSessionMessage> ObserveEndSessionMessages(Func<EndSessionReasons, bool> onQuerySession) => ObserveEndSessionMessages(onQuerySession, null);

    /// <summary>
    ///     Creates an observable stream that listens for WM_QUERYENDSESSION and WM_ENDSESSION messages.
    ///     This allows applications to be notified when the system is about to shut down or restart.
    /// </summary>
    /// <param name="onQuerySession">Handler called when a query end-session message is received.</param>
    /// <param name="onEndSession">Handler called when an end-session message is received.</param>
    /// <returns>
    ///     An observable stream that emits EndSessionMessage values when a session end event occurs.
    ///     Subscribe to this observable to handle shutdown requests gracefully.
    /// </returns>
    public static IObservable<EndSessionMessage> ObserveEndSessionMessages(
        Func<EndSessionReasons, bool> onQuerySession,
        Func<EndSessionReasons, bool> onEndSession) =>
        _endSessionObservable ??= ReactiveSignal.CreateWithState(
            new(onQuerySession, onEndSession),
            static (EndSessionHandlers state, IObserver<EndSessionMessage> observer) =>
                new EndSessionMessageObserver(observer, state).Connect()).Share();

    /// <summary>Gets restart command-line arguments from a supplied process argument array.</summary>
    /// <param name="args">The process command-line arguments, including the executable path.</param>
    /// <returns>Array of command-line arguments, excluding the executable path.</returns>
    internal static string[] GetRestartCommandLineArgs(string[] args)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(args);
        if (args.Length <= 1)
        {
            return [];
        }

        string[] restartArgs = new string[checked(args.Length - 1)];
        Array.Copy(args, 1, restartArgs, 0, restartArgs.Length);
        return restartArgs;
    }

    /// <summary>Creates a directly controllable end-session observer for deterministic tests.</summary>
    /// <param name="observer">The observer that receives translated session messages.</param>
    /// <param name="onQuerySession">The optional query-session handler.</param>
    /// <param name="onEndSession">The optional end-session handler.</param>
    /// <param name="connect">A value indicating whether to connect to the shared message stream.</param>
    /// <returns>The message observer and its disposable lifetime.</returns>
    internal static (IObserver<WindowMessage> Observer, IDisposable Lifetime) CreateEndSessionObserverForTesting(
        IObserver<EndSessionMessage> observer,
        Func<EndSessionReasons, bool> onQuerySession,
        Func<EndSessionReasons, bool> onEndSession,
        bool connect)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(observer);
        EndSessionMessageObserver endSessionObserver = new(observer, new EndSessionHandlers(onQuerySession, onEndSession));
        return (Observer: endSessionObserver, Lifetime: connect ? endSessionObserver.Connect() : endSessionObserver);
    }

    /// <summary>Overrides restart registration operations for deterministic tests.</summary>
    /// <param name="register">The replacement registration operation.</param>
    /// <param name="unregister">The replacement unregistration operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideRestartOperationsForTesting(
        Func<string, ApplicationRestartFlags, int> register,
        Func<int> unregister)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(register);
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(unregister);
        ApplicationRestartOperations restartOperations = _restartOperations;
        _restartOperations = new(register, unregister);
        return Scope.Create(restartOperations, static previous =>
        {
            _restartOperations = previous;
        });
    }

    /// <summary>Checks whether supplied process arguments contain a restart marker.</summary>
    /// <param name="args">The process command-line arguments.</param>
    /// <returns>True when one of the arguments requests restart handling; otherwise, false.</returns>
    internal static bool WasRestartRequested(string[] args)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(args);
        return Array.Exists(args, IsRestartArgument);
    }

    /// <summary>Resets the cached end-session observable for deterministic tests.</summary>
    internal static void ResetEndSessionMessagesForTesting() => _endSessionObservable = null;

    /// <summary>Checks whether an argument is a restart marker.</summary>
    /// <param name="arg">Argument to inspect.</param>
    /// <returns>True when the argument indicates a restart; otherwise, false.</returns>
    private static bool IsRestartArgument(string arg) =>
        arg.Equals("/restart", StringComparison.OrdinalIgnoreCase)
        || arg.Equals("-restart", StringComparison.OrdinalIgnoreCase)
        || arg.Equals("--restart", StringComparison.OrdinalIgnoreCase);

    /// <summary>Native kernel32 entry points.</summary>
#if NETFRAMEWORK
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>Registers the active instance of an application for restart.</summary>
        /// <param name="commandLine">Command line to pass to the restarted application.</param>
        /// <param name="flags">Application restart flags.</param>
        /// <returns>S_OK on success; otherwise, an error value.</returns>
#if NETFRAMEWORK
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int RegisterApplicationRestart(string commandLine, ApplicationRestartFlags flags);
#else
        [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int RegisterApplicationRestart(string commandLine, ApplicationRestartFlags flags);
#endif

        /// <summary>Removes the active instance of an application from the restart list.</summary>
        /// <returns>S_OK on success; otherwise, an error value.</returns>
#if NETFRAMEWORK
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int UnregisterApplicationRestart();
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int UnregisterApplicationRestart();
#endif
    }

    /// <summary>Composes restart registration operations without invoking them during construction.</summary>
    /// <param name="register">The registration operation.</param>
    /// <param name="unregister">The unregistration operation.</param>
    private sealed class ApplicationRestartOperations(Func<string, ApplicationRestartFlags, int> register, Func<int> unregister)
    {
        /// <summary>Registers the application for restart.</summary>
        /// <param name="commandLine">The restart command line.</param>
        /// <param name="flags">The restart flags.</param>
        /// <returns>The native result.</returns>
        public int Register(string commandLine, ApplicationRestartFlags flags) => register(commandLine, flags);

        /// <summary>Unregisters the application from restart.</summary>
        /// <returns>The native result.</returns>
        public int Unregister() => unregister();
    }

    /// <summary>Forwards native window session messages to the end-session observable.</summary>
    /// <param name="observer">Observer that receives end-session messages.</param>
    /// <param name="handlers">Handlers used to pre-handle end-session messages.</param>
    private sealed class EndSessionMessageObserver(IObserver<EndSessionMessage> observer, EndSessionHandlers handlers) : IObserver<WindowMessage>, IDisposable
    {
        /// <summary>Shared message subscription.</summary>
        private IDisposable _subscription;

        /// <summary>Connects to the shared message window.</summary>
        /// <returns>The connected observer.</returns>
        public EndSessionMessageObserver Connect()
        {
            _subscription = SharedMessageWindow.WindowMessageEvents.Subscribe(this);
            return this;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _endSessionObservable = null;
            _subscription?.Dispose();
        }

        /// <inheritdoc />
        public void OnCompleted() => observer.OnCompleted();

        /// <inheritdoc />
        public void OnError(Exception error) => observer.OnError(error);

        /// <inheritdoc />
        public void OnNext(WindowMessage value)
        {
            if (value.Msg == WindowsMessages.WM_QUERYENDSESSION || value.Msg == WindowsMessages.WM_ENDSESSION)
            {
                WindowMessage message = value;
                EndSessionReasons endSessionReason = (EndSessionReasons)checked((uint)message.LParam);
                if (message.Msg == WindowsMessages.WM_QUERYENDSESSION)
                {
                    message = TryHandle(in message, endSessionReason, handlers.OnQuerySession);
                }
                else if (message.Msg == WindowsMessages.WM_ENDSESSION)
                {
                    message = TryHandle(in message, endSessionReason, handlers.OnEndSession);
                }

                if (!message.Handled)
                {
                    EndSessionMessage endSessionMessage = new(message.Msg, endSessionReason);
                    observer.OnNext(endSessionMessage);
                }
            }
        }

        /// <summary>Handles a message through a supplied session handler.</summary>
        /// <param name="message">Window message to update.</param>
        /// <param name="reason">End-session reason.</param>
        /// <param name="handler">Handler to invoke.</param>
        /// <returns>The original message, or a handled copy when a handler processed it.</returns>
        private static WindowMessage TryHandle(in WindowMessage message, EndSessionReasons reason, Func<EndSessionReasons, bool> handler)
        {
            if (handler is null)
            {
                return message;
            }

            bool canEndSession = handler(reason);
            message.Result = (ulong)((!canEndSession) ? 1 : 0);
            message.Handled = true;
            return message;
        }
    }

    /// <summary>Stores end-session handlers for a shared message subscription.</summary>
    /// <param name="OnQuerySession">Handler invoked for query end-session messages.</param>
    /// <param name="OnEndSession">Handler invoked for end-session messages.</param>
    private sealed record EndSessionHandlers(Func<EndSessionReasons, bool> OnQuerySession, Func<EndSessionReasons, bool> OnEndSession);
}
