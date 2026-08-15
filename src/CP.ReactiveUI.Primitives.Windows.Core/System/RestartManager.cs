// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.Primitives.Signals;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>
///     High-level helper class for using the Windows Restart Manager.
///     The Restart Manager helps minimize application downtime during software installations and updates
///     by identifying which applications are using locked files and then shutting them down and restarting them.
/// </summary>
/// <example>
///     Example usage to find processes locking a file:
///     <code>
///     using (var session = RestartManager.CreateSession())
///     {
///         session.RegisterFile(@"C:\path\to\locked\file.dll");
///         var processes = session.GetProcessesUsingResources();
///
///         foreach (var process in processes)
///         {
///             Console.WriteLine($"Process {process.strAppName} (PID: {process.Process.processId}) is using the file");
///         }
///     }
///     </code>
/// </example>
public sealed class RestartManager : IDisposable
{
    /// <summary>The Win32 error code returned when more data is available.</summary>
    private const int ErrorMoreData = 234;

    /// <summary>The Restart Manager session key.</summary>
    private readonly string _sessionKey;

    /// <summary>The Restart Manager API implementation.</summary>
    private readonly IRestartManagerSessionApi _sessionApi;

    /// <summary>The Restart Manager session handle.</summary>
    private int _sessionHandle = RestartManagerApi.InvalidSession;

    /// <summary>The value indicating whether this instance has been disposed.</summary>
    private bool _disposed;

    /// <summary>Initializes a new instance of the RestartManager class.</summary>
    /// <param name="sessionHandle">Restart Manager session handle.</param>
    /// <param name="sessionKey">Restart Manager session key.</param>
    /// <param name="sessionApi">Restart Manager API implementation.</param>
    internal RestartManager(int sessionHandle, string sessionKey, IRestartManagerSessionApi sessionApi)
    {
        Throw.IfNull(sessionApi);
        _sessionHandle = sessionHandle;
        _sessionKey = sessionKey;
        _sessionApi = sessionApi;
    }

    /// <summary>Gets the session key for this Restart Manager session.</summary>
    public string SessionKey => _sessionKey;

    /// <summary>Creates a new Restart Manager session.</summary>
    /// <returns>A new RestartManager instance.</returns>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the session could not be started.</exception>
    public static RestartManager CreateSession() => CreateSession(NativeRestartManagerSessionApi.Instance);

    /// <summary>Ends the Restart Manager session and releases resources.</summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_sessionHandle != RestartManagerApi.InvalidSession)
            {
                _ = _sessionApi.EndSession(_sessionHandle);
                _sessionHandle = RestartManagerApi.InvalidSession;
            }

            _disposed = true;
        }
    }

    /// <summary>Registers one or more files with the Restart Manager session.</summary>
    /// <param name="filenames">Full paths to the files to register.</param>
    /// <exception cref="T:System.ObjectDisposedException">Thrown when the session has been disposed.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the resources could not be registered.</exception>
    public void RegisterFiles(params string[] filenames)
    {
        ThrowIfDisposed();
        if (filenames is not null && filenames.Length != 0)
        {
            int result = _sessionApi.RegisterResources(_sessionHandle, checked((uint)filenames.Length), filenames, 0U, null, 0U, null);
            if (result != 0)
            {
                throw new Win32Exception(result, "Failed to register files with Restart Manager");
            }
        }
    }

    /// <summary>Registers a single file with the Restart Manager session.</summary>
    /// <param name="filename">Full path to the file to register.</param>
    /// <exception cref="T:System.ObjectDisposedException">Thrown when the session has been disposed.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the resource could not be registered.</exception>
    public void RegisterFile(string filename) => RegisterFiles(filename);

    /// <summary>Registers one or more processes with the Restart Manager session.</summary>
    /// <param name="processes">Array of RmUniqueProcess structures identifying the processes.</param>
    /// <exception cref="T:System.ObjectDisposedException">Thrown when the session has been disposed.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the resources could not be registered.</exception>
    public void RegisterProcesses(params RmUniqueProcess[] processes)
    {
        ThrowIfDisposed();
        if (processes is not null && processes.Length != 0)
        {
            int result = _sessionApi.RegisterResources(_sessionHandle, 0U, null, checked((uint)processes.Length), processes, 0U, null);
            if (result != 0)
            {
                throw new Win32Exception(result, "Failed to register processes with Restart Manager");
            }
        }
    }

    /// <summary>Registers one or more Windows services with the Restart Manager session.</summary>
    /// <param name="serviceNames">Short names of the services to register.</param>
    /// <exception cref="T:System.ObjectDisposedException">Thrown when the session has been disposed.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the resources could not be registered.</exception>
    public void RegisterServices(params string[] serviceNames)
    {
        ThrowIfDisposed();
        if (serviceNames is not null && serviceNames.Length != 0)
        {
            int result = _sessionApi.RegisterResources(_sessionHandle, 0U, null, 0U, null, checked((uint)serviceNames.Length), serviceNames);
            if (result != 0)
            {
                throw new Win32Exception(result, "Failed to register services with Restart Manager");
            }
        }
    }

    /// <summary>Gets a list of all applications and services using the registered resources.</summary>
    /// <returns>A list of RmProcessInfo structures describing the affected applications.</returns>
    /// <exception cref="T:System.ObjectDisposedException">Thrown when the session has been disposed.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the list could not be retrieved.</exception>
    public IReadOnlyList<RmProcessInfo> GetProcessesUsingResources()
    {
        ThrowIfDisposed();
        uint processInfoCount = 0U;
        int result = _sessionApi.GetList(_sessionHandle, out var processInfoNeeded, ref processInfoCount, null, out var rebootReasons);
        if (result is not 0 and not 234)
        {
            throw new Win32Exception(result, "Failed to get list size from Restart Manager");
        }

        if (processInfoNeeded == 0)
        {
            return [];
        }

        RmProcessInfo[] processInfo = new RmProcessInfo[processInfoNeeded];
        processInfoCount = processInfoNeeded;
        result = _sessionApi.GetList(_sessionHandle, out processInfoNeeded, ref processInfoCount, processInfo, out rebootReasons);
        if (result != 0)
        {
            throw new Win32Exception(result, "Failed to get process list from Restart Manager");
        }

        return CreateProcessInfoList(processInfo, processInfoCount);
    }

    /// <summary>Gets a list of all applications and services using the registered resources, along with the reboot reason.</summary>
    /// <param name="rebootReason">Receives flags indicating why a reboot might be necessary.</param>
    /// <returns>A list of RmProcessInfo structures describing the affected applications.</returns>
    /// <exception cref="T:System.ObjectDisposedException">Thrown when the session has been disposed.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the list could not be retrieved.</exception>
    public IReadOnlyList<RmProcessInfo> GetProcessesUsingResources(out RmRebootReason rebootReason)
    {
        ThrowIfDisposed();
        uint processInfoCount = 0U;
        int result = _sessionApi.GetList(_sessionHandle, out var processInfoNeeded, ref processInfoCount, null, out rebootReason);
        if (result is not 0 and not 234)
        {
            throw new Win32Exception(result, "Failed to get list size from Restart Manager");
        }

        if (processInfoNeeded == 0)
        {
            return [];
        }

        RmProcessInfo[] processInfo = new RmProcessInfo[processInfoNeeded];
        processInfoCount = processInfoNeeded;
        result = _sessionApi.GetList(_sessionHandle, out processInfoNeeded, ref processInfoCount, processInfo, out rebootReason);
        if (result != 0)
        {
            throw new Win32Exception(result, "Failed to get process list from Restart Manager");
        }

        return CreateProcessInfoList(processInfo, processInfoCount);
    }

    /// <summary>Shuts down the applications and services using the registered resources.</summary>
    /// <exception cref="T:System.ObjectDisposedException">Thrown when the session has been disposed.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the shutdown failed.</exception>
    public void Shutdown() => Shutdown(RmShutdownType.RmForceShutdown, null);

    /// <summary>Shuts down the applications and services using the registered resources.</summary>
    /// <param name="shutdownType">Flags controlling the shutdown behavior.</param>
    /// <exception cref="T:System.ObjectDisposedException">Thrown when the session has been disposed.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the shutdown failed.</exception>
    public void Shutdown(RmShutdownType shutdownType) => Shutdown(shutdownType, null);

    /// <summary>Shuts down the applications and services using the registered resources.</summary>
    /// <param name="statusCallback">Callback to receive progress updates (0-100).</param>
    /// <exception cref="T:System.ObjectDisposedException">Thrown when the session has been disposed.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the shutdown failed.</exception>
    public void Shutdown(Action<uint> statusCallback) => Shutdown(RmShutdownType.RmForceShutdown, statusCallback);

    /// <summary>Shuts down the applications and services using the registered resources.</summary>
    /// <param name="shutdownType">Flags controlling the shutdown behavior.</param>
    /// <param name="statusCallback">Callback to receive progress updates (0-100).</param>
    /// <exception cref="T:System.ObjectDisposedException">Thrown when the session has been disposed.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the shutdown failed.</exception>
    public void Shutdown(RmShutdownType shutdownType, Action<uint> statusCallback)
    {
        ThrowIfDisposed();
        RmStatusCallback callback = null;
        if (statusCallback is not null)
        {
            callback = statusCallback.Invoke;
        }

        int result = _sessionApi.Shutdown(_sessionHandle, shutdownType, callback);
        if (result != 0)
        {
            throw new Win32Exception(result, "Failed to shutdown applications via Restart Manager");
        }
    }

    /// <summary>Observes shutdown progress while shutting down applications and services using the registered resources.</summary>
    /// <returns>An observable sequence of native Restart Manager progress values from 0 to 100.</returns>
    public IObservable<uint> ObserveShutdownProgress() => ObserveShutdownProgress(RmShutdownType.RmForceShutdown);

    /// <summary>Observes shutdown progress while shutting down applications and services using the registered resources.</summary>
    /// <param name="shutdownType">Flags controlling the shutdown behavior.</param>
    /// <returns>An observable sequence of native Restart Manager progress values from 0 to 100.</returns>
    public IObservable<uint> ObserveShutdownProgress(RmShutdownType shutdownType) => Signal.Create(delegate(IObserver<uint> observer)
        {
            try
            {
                Shutdown(shutdownType, observer.OnNext);
                observer.OnCompleted();
            }
            catch (Exception error)
            {
                observer.OnError(error);
            }

            return EmptyDisposable.Instance;
        });

    /// <summary>Restarts applications and services that were shut down by the Shutdown method.</summary>
    /// <exception cref="T:System.ObjectDisposedException">Thrown when the session has been disposed.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the restart failed.</exception>
    public void Restart() => Restart(null);

    /// <summary>Restarts applications and services that were shut down by the Shutdown method.</summary>
    /// <param name="statusCallback">Callback to receive progress updates (0-100).</param>
    /// <exception cref="T:System.ObjectDisposedException">Thrown when the session has been disposed.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the restart failed.</exception>
    public void Restart(Action<uint> statusCallback)
    {
        ThrowIfDisposed();
        RmStatusCallback callback = null;
        if (statusCallback is not null)
        {
            callback = statusCallback.Invoke;
        }

        int result = _sessionApi.Restart(_sessionHandle, 0, callback);
        if (result != 0)
        {
            throw new Win32Exception(result, "Failed to restart applications via Restart Manager");
        }
    }

    /// <summary>Observes restart progress while restarting applications and services that were shut down by Restart Manager.</summary>
    /// <returns>An observable sequence of native Restart Manager progress values from 0 to 100.</returns>
    public IObservable<uint> ObserveRestartProgress() => Signal.Create(delegate(IObserver<uint> observer)
        {
            try
            {
                Restart(observer.OnNext);
                observer.OnCompleted();
            }
            catch (Exception error)
            {
                observer.OnError(error);
            }

            return EmptyDisposable.Instance;
        });

    /// <summary>Checks if a reboot would be required to complete the operation.</summary>
    /// <returns>True if a reboot is required, false otherwise.</returns>
    /// <exception cref="T:System.ObjectDisposedException">Thrown when the session has been disposed.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the check failed.</exception>
    public bool IsRebootRequired()
    {
        _ = GetProcessesUsingResources(out var rebootReason);
        return rebootReason != RmRebootReason.None;
    }

    /// <summary>Gets the reason why a reboot would be required.</summary>
    /// <returns>Flags indicating the reboot reason.</returns>
    /// <exception cref="T:System.ObjectDisposedException">Thrown when the session has been disposed.</exception>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the check failed.</exception>
    public RmRebootReason GetRebootReason()
    {
        _ = GetProcessesUsingResources(out var rebootReason);
        return rebootReason;
    }

    /// <summary>Creates a new Restart Manager session.</summary>
    /// <param name="sessionApi">Restart Manager API implementation.</param>
    /// <returns>A new RestartManager instance.</returns>
    /// <exception cref="T:System.ComponentModel.Win32Exception">Thrown when the session could not be started.</exception>
    internal static RestartManager CreateSession(IRestartManagerSessionApi sessionApi)
    {
        Throw.IfNull(sessionApi);
        StringBuilder sessionKey = new(checked(RestartManagerApi.SessionKeyLength + 1));
        int result = sessionApi.StartSession(out var sessionHandle, 0, sessionKey);
        if (result != 0)
        {
            throw new Win32Exception(result, "Failed to start Restart Manager session");
        }

        return new(sessionHandle, sessionKey.ToString(), sessionApi);
    }

    /// <summary>Creates a process-info list from the returned native records.</summary>
    /// <param name="processInfo">Returned process information.</param>
    /// <param name="count">Number of valid process records.</param>
    /// <returns>The copied process information.</returns>
    private static List<RmProcessInfo> CreateProcessInfoList(RmProcessInfo[] processInfo, uint count)
    {
        checked
        {
            int length = Math.Min((int)count, processInfo.Length);
            List<RmProcessInfo> processes = new(length);
            for (int i = 0; i < length; i++)
            {
                processes.Add(processInfo[i]);
            }

            return processes;
        }
    }

    /// <summary>Throws if this instance has been disposed.</summary>
    private void ThrowIfDisposed() => Throw.IfDisposed(_disposed, this);
}
