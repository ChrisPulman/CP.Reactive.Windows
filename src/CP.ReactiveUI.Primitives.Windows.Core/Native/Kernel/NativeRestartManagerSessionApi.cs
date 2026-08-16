// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Native Restart Manager session operations.</summary>
internal sealed class NativeRestartManagerSessionApi : IRestartManagerSessionApi
{
    /// <summary>Ends a Restart Manager session.</summary>
    private readonly Func<int, int> _endSession;

    /// <summary>Gets affected applications for a Restart Manager session.</summary>
    private readonly GetListOperation _getList;

    /// <summary>Registers resources with a Restart Manager session.</summary>
    private readonly RegisterResourcesOperation _registerResources;

    /// <summary>Restarts affected applications.</summary>
    private readonly RestartOperation _restart;

    /// <summary>Shuts down affected applications.</summary>
    private readonly ShutdownOperation _shutdown;

    /// <summary>Starts a Restart Manager session.</summary>
    private readonly StartSessionOperation _startSession;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Kernel.NativeRestartManagerSessionApi" /> class.</summary>
    /// <param name="startSession">Starts a Restart Manager session.</param>
    /// <param name="endSession">Ends a Restart Manager session.</param>
    /// <param name="registerResources">Registers resources with a Restart Manager session.</param>
    /// <param name="getList">Gets affected applications for a Restart Manager session.</param>
    /// <param name="shutdown">Shuts down affected applications.</param>
    /// <param name="restart">Restarts affected applications.</param>
    internal NativeRestartManagerSessionApi(
        StartSessionOperation startSession,
        Func<int, int> endSession,
        RegisterResourcesOperation registerResources,
        GetListOperation getList,
        ShutdownOperation shutdown,
        RestartOperation restart)
    {
        _startSession = startSession;
        _endSession = endSession;
        _registerResources = registerResources;
        _getList = getList;
        _shutdown = shutdown;
        _restart = restart;
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Kernel.NativeRestartManagerSessionApi" /> class.</summary>
    private NativeRestartManagerSessionApi()
        : this(
            RestartManagerApi.RmStartSession,
            RestartManagerApi.RmEndSession,
            RestartManagerApi.RmRegisterResources,
            RestartManagerApi.RmGetList,
            RestartManagerApi.RmShutdown,
            RestartManagerApi.RmRestart) { }

    /// <summary>Gets the singleton native Restart Manager API implementation.</summary>
    internal static NativeRestartManagerSessionApi Instance { get; } = new();

    /// <inheritdoc />
    public int StartSession(out int sessionHandle, int sessionFlags, StringBuilder sessionKey) =>
        _startSession(out sessionHandle, sessionFlags, sessionKey);

    /// <inheritdoc />
    public int EndSession(int sessionHandle) => _endSession(sessionHandle);

    /// <inheritdoc />
    public int RegisterResources(
        int sessionHandle,
        uint fileCount,
        string[] filenames,
        uint applicationCount,
        RmUniqueProcess[] applications,
        uint serviceCount,
        string[] serviceNames) =>
        _registerResources(
            sessionHandle,
            fileCount,
            filenames,
            applicationCount,
            applications,
            serviceCount,
            serviceNames);

    /// <inheritdoc />
    public int GetList(
        int sessionHandle,
        out uint processInfoNeeded,
        ref uint processInfoCount,
        RmProcessInfo[] affectedApplications,
        out RmRebootReason rebootReasons) =>
        _getList(
            sessionHandle,
            out processInfoNeeded,
            ref processInfoCount,
            affectedApplications,
            out rebootReasons);

    /// <inheritdoc />
    public int Shutdown(
        int sessionHandle,
        RmShutdownType shutdownType,
        RmStatusCallback statusCallback) => _shutdown(sessionHandle, shutdownType, statusCallback);

    /// <inheritdoc />
    public int Restart(int sessionHandle, int restartFlags, RmStatusCallback statusCallback) =>
        _restart(sessionHandle, restartFlags, statusCallback);
}
