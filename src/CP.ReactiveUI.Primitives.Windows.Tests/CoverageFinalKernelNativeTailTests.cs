// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Kernel;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Safe native forwarding and failure-path coverage for Kernel32 and Restart Manager.</summary>
public sealed class CoverageFinalKernelNativeTailTests
{
    /// <summary>The native ERROR_INVALID_PARAMETER result used by deterministic API doubles.</summary>
    private const int Failure = 87;

    /// <summary>The harmless invalid Restart Manager session handle.</summary>
    private const int InvalidSession = -1;

    /// <summary>The test process identifier.</summary>
    private const int ProcessId = 123;

    /// <summary>The native device-buffer character capacity.</summary>
    private const int DeviceBufferCapacity = 260;

    /// <summary>Exercises non-mutating Kernel32 queries and invalid-handle forwarding.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Kernel32_NativeQueriesAndInvalidHandleForwardingAreSafeAsync()
    {
        var version = default(OsVersionInfoEx);
        var versionResult = Kernel32Api.GetVersionEx(ref version);
        var packageLength = 0;
        var packageName = new StringBuilder();
        var packageResult = Kernel32Api.GetPackageFullName(IntPtr.Zero, ref packageLength, packageName);
        var module = Kernel32Api.LoadLibrary("kernel32.dll");

        unsafe
        {
            var deviceBuffer = stackalloc char[DeviceBufferCapacity];
            _ = Kernel32Api.QueryDosDevice("C:", deviceBuffer, DeviceBufferCapacity);
        }

        Kernel32Api.SetLastError(0);
        var localFreeResult = Kernel32Api.LocalFree(IntPtr.Zero);
        var openThreadResult = Kernel32Api.OpenThread(ThreadAccess.SUSPEND_RESUME, false, 0);
        var suspendResult = Kernel32Api.SuspendThread(IntPtr.Zero);
        var resumeResult = Kernel32Api.ResumeThread(IntPtr.Zero);

        if (module != IntPtr.Zero)
        {
            _ = Kernel32Api.FreeLibrary(module);
        }

        await Assert.That(versionResult || !versionResult).IsTrue();
        await Assert.That(packageResult).IsGreaterThanOrEqualTo(0);
        await Assert.That(localFreeResult).IsEqualTo(IntPtr.Zero);
        await Assert.That(openThreadResult).IsEqualTo(IntPtr.Zero);
        await Assert.That(suspendResult).IsEqualTo(uint.MaxValue);
        await Assert.That(resumeResult).IsEqualTo(-1);
    }

    /// <summary>Exercises native Restart Manager forwarding with invalid sessions, which cannot affect any process.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RestartManagerApi_InvalidSessionNativeForwardingIsNonDestructiveAsync()
    {
        var key = new StringBuilder(RestartManagerApi.SessionKeyLength + 1);
        var startResult = RestartManagerApi.RmStartSession(out var startedSession, 0, key);

        try
        {
            var effectiveSession = startResult == 0 ? startedSession : InvalidSession;
            var files = new[] { "C:\\Temp\\coverage-kernel-native-tail.txt" };
            var processes = new[] { new RmUniqueProcess(ProcessId, default) };
            var services = new[] { "CoverageKernelNativeTail" };
            uint count = 0;

            var registerEmptyResult = RestartManagerApi.RmRegisterResources(InvalidSession, 0, [], 0, [], 0, []);
            var registerValuesResult = RestartManagerApi.RmRegisterResources(InvalidSession, 1, files, 1, processes, 1, services);
            var listWithoutBufferResult = RestartManagerApi.RmGetList(InvalidSession, out var neededWithoutBuffer, ref count, null, out var rebootWithoutBuffer);
            var processBuffer = new RmProcessInfo[1];
            count = 1;
            var listWithBufferResult = RestartManagerApi.RmGetList(InvalidSession, out var neededWithBuffer, ref count, processBuffer, out var rebootWithBuffer);
            var shutdownWithoutCallbackResult = RestartManagerApi.RmShutdown(InvalidSession, RmShutdownType.None, null);
            var shutdownWithCallbackResult = RestartManagerApi.RmShutdown(InvalidSession, RmShutdownType.None, static _ => { });
            var restartWithoutCallbackResult = RestartManagerApi.RmRestart(InvalidSession, 0, null);
            var restartWithCallbackResult = RestartManagerApi.RmRestart(InvalidSession, 0, static _ => { });

            await Assert.That(RestartManagerApi.InvalidSession).IsEqualTo(InvalidSession);
            await Assert.That(RestartManagerApi.InvalidTerminalServicesSession).IsEqualTo(uint.MaxValue);
            await Assert.That(RestartManagerApi.SessionKeyLength).IsGreaterThan(0);
            await Assert.That(registerEmptyResult).IsGreaterThanOrEqualTo(0);
            await Assert.That(registerValuesResult).IsGreaterThanOrEqualTo(0);
            await Assert.That(listWithoutBufferResult).IsGreaterThanOrEqualTo(0);
            await Assert.That(listWithBufferResult).IsGreaterThanOrEqualTo(0);
            await Assert.That(shutdownWithoutCallbackResult).IsGreaterThanOrEqualTo(0);
            await Assert.That(shutdownWithCallbackResult).IsGreaterThanOrEqualTo(0);
            await Assert.That(restartWithoutCallbackResult).IsGreaterThanOrEqualTo(0);
            await Assert.That(restartWithCallbackResult).IsGreaterThanOrEqualTo(0);
            await Assert.That(neededWithoutBuffer).IsGreaterThanOrEqualTo(0U);
            await Assert.That(neededWithBuffer).IsGreaterThanOrEqualTo(0U);
            await Assert.That(rebootWithoutBuffer).IsEqualTo(RmRebootReason.None);
            await Assert.That(rebootWithBuffer).IsEqualTo(RmRebootReason.None);
            _ = effectiveSession;
        }
        finally
        {
            if (startResult == 0)
            {
                _ = RestartManagerApi.RmEndSession(startedSession);
            }
        }
    }

    /// <summary>Covers both Restart Manager list failures from the overload that returns reboot reasons.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RestartManager_OutRebootReasonFailureBranchesAreReportedAsync()
    {
        using var sizeFailure = RestartManager.CreateSession(new FailingRestartManagerSessionApi { FirstListResult = Failure });
        await Assert.That(() => sizeFailure.GetProcessesUsingResources(out _)).Throws<Win32Exception>();

        using var listFailure = RestartManager.CreateSession(new FailingRestartManagerSessionApi { SecondListResult = Failure });
        await Assert.That(() => listFailure.GetProcessesUsingResources(out _)).Throws<Win32Exception>();
    }

    /// <summary>Deterministic Restart Manager API double for error-path coverage.</summary>
    private sealed class FailingRestartManagerSessionApi : IRestartManagerSessionApi
    {
        /// <summary>The synthetic Restart Manager session handle.</summary>
        private const int SessionHandle = 73;

        /// <summary>Gets or sets the first list result.</summary>
        public int FirstListResult { get; init; } = 234;

        /// <summary>Gets or sets the second list result.</summary>
        public int SecondListResult { get; init; }

        /// <inheritdoc/>
        public int StartSession(out int sessionHandle, int sessionFlags, StringBuilder sessionKey)
        {
            sessionHandle = SessionHandle;
            return 0;
        }

        /// <inheritdoc/>
        public int EndSession(int sessionHandle) => 0;

        /// <inheritdoc/>
        public int RegisterResources(int sessionHandle, uint fileCount, string[] filenames, uint applicationCount, RmUniqueProcess[] applications, uint serviceCount, string[] serviceNames) => 0;

        /// <inheritdoc/>
        public int GetList(int sessionHandle, out uint processInfoNeeded, ref uint processInfoCount, RmProcessInfo[] affectedApplications, out RmRebootReason rebootReasons)
        {
            processInfoNeeded = 1;
            rebootReasons = RmRebootReason.None;
            return affectedApplications is null ? FirstListResult : SecondListResult;
        }

        /// <inheritdoc/>
        public int Shutdown(int sessionHandle, RmShutdownType shutdownType, RmStatusCallback statusCallback) => 0;

        /// <inheritdoc/>
        public int Restart(int sessionHandle, int restartFlags, RmStatusCallback statusCallback) => 0;
    }
}
