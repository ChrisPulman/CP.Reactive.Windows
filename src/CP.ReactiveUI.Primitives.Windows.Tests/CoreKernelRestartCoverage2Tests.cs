// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Buffers.Binary;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Second-wave coverage for Kernel32 and Restart Manager behavior.</summary>
public sealed class CoreKernelRestartCoverage2Tests
{
    /// <summary>Defines a synthetic Win32 failure result.</summary>
    private const int TestFailure = 5;

    /// <summary>Defines the native OSVERSIONINFOEX byte size.</summary>
    private const int NativeOsVersionInfoSize = 284;

    /// <summary>Defines the native OSVERSIONINFOEX major-version offset.</summary>
    private const int MajorVersionOffset = 4;

    /// <summary>Defines the native OSVERSIONINFOEX minor-version offset.</summary>
    private const int MinorVersionOffset = 8;

    /// <summary>Defines the native OSVERSIONINFOEX build-number offset.</summary>
    private const int BuildNumberOffset = 12;

    /// <summary>Defines the native OSVERSIONINFOEX platform-id offset.</summary>
    private const int PlatformIdOffset = 16;

    /// <summary>Defines the native OSVERSIONINFOEX service-pack text offset.</summary>
    private const int ServicePackVersionOffset = 20;

    /// <summary>Defines the native OSVERSIONINFOEX service-pack-major offset.</summary>
    private const int ServicePackMajorOffset = 276;

    /// <summary>Defines the native OSVERSIONINFOEX service-pack-minor offset.</summary>
    private const int ServicePackMinorOffset = 278;

    /// <summary>Defines the native OSVERSIONINFOEX suite-mask offset.</summary>
    private const int SuiteMaskOffset = 280;

    /// <summary>Defines the native OSVERSIONINFOEX product-type offset.</summary>
    private const int ProductTypeOffset = 282;

    /// <summary>Defines the first native process record index.</summary>
    private const int FirstRecordIndex = 0;

    /// <summary>Defines the second native process record index.</summary>
    private const int SecondRecordIndex = 1;

    /// <summary>Defines the copied process record count.</summary>
    private const int CopiedRecordCount = 1;

    /// <summary>Defines the native process record count.</summary>
    private const int NativeRecordCount = 2;

    /// <summary>Defines the synthetic Restart Manager session handle.</summary>
    private const int SessionHandle = 48;

    /// <summary>Defines a synthetic process identifier.</summary>
    private const int ProcessId = 3210;

    /// <summary>Defines a second synthetic process identifier.</summary>
    private const int SecondProcessId = 3211;

    /// <summary>Defines a synthetic process start-time low value.</summary>
    private const int ProcessStartLow = 765;

    /// <summary>Defines a synthetic process start-time high value.</summary>
    private const int ProcessStartHigh = 987;

    /// <summary>Defines a synthetic Terminal Services session identifier.</summary>
    private const uint TerminalSessionId = 3;

    /// <summary>Defines the shutdown progress value.</summary>
    private const uint ShutdownProgress = 25;

    /// <summary>Defines the restart progress value.</summary>
    private const uint RestartProgress = 75;

    /// <summary>Defines the Windows major version.</summary>
    private const int MajorVersion = 10;

    /// <summary>Defines the Windows minor version.</summary>
    private const int MinorVersion = 1;

    /// <summary>Defines the Windows build number.</summary>
    private const int BuildNumber = 26_000;

    /// <summary>Defines the Windows platform identifier.</summary>
    private const int PlatformId = 2;

    /// <summary>Defines the service pack major version.</summary>
    private const short ServicePackMajor = 4;

    /// <summary>Defines the service pack minor version.</summary>
    private const short ServicePackMinor = 2;

    /// <summary>Defines a test session key.</summary>
    private const string SessionKey = "cp-reactive-session";

    /// <summary>Defines a test file name.</summary>
    private const string FileName = @"C:\Temp\sample.txt";

    /// <summary>Defines a test service name.</summary>
    private const string ServiceName = "SampleService";

    /// <summary>Defines a test application name.</summary>
    private const string ApplicationName = "SampleApp";

    /// <summary>Defines a second test application name.</summary>
    private const string SecondApplicationName = "Second";

    /// <summary>Defines a test service short name.</summary>
    private const string ServiceShortName = "Svc";

    /// <summary>Defines a second test service short name.</summary>
    private const string SecondServiceShortName = "Svc2";

    /// <summary>Defines a test service pack string.</summary>
    private const string ServicePackVersion = "Service Pack";

    /// <summary>Tests deterministic parsing of native OSVERSIONINFOEX bytes.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKernel32_CreateOsVersionInfoParsesNativeBufferAsync()
    {
        Span<byte> buffer = stackalloc byte[NativeOsVersionInfoSize];
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(MajorVersionOffset), MajorVersion);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(MinorVersionOffset), MinorVersion);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(BuildNumberOffset), BuildNumber);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(PlatformIdOffset), PlatformId);
        WriteUtf16(buffer.Slice(ServicePackVersionOffset), ServicePackVersion);
        BinaryPrimitives.WriteInt16LittleEndian(buffer.Slice(ServicePackMajorOffset), ServicePackMajor);
        BinaryPrimitives.WriteInt16LittleEndian(buffer.Slice(ServicePackMinorOffset), ServicePackMinor);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(SuiteMaskOffset), (ushort)WindowsSuites.TerminalServer);
        buffer[ProductTypeOffset] = (byte)WindowsProductTypes.VER_NT_WORKSTATION;

        var versionInfo = Kernel32Api.CreateOsVersionInfo(buffer);

        await Assert.That(versionInfo.MajorVersion).IsEqualTo(MajorVersion);
        await Assert.That(versionInfo.MinorVersion).IsEqualTo(MinorVersion);
        await Assert.That(versionInfo.BuildNumber).IsEqualTo(BuildNumber);
        await Assert.That(versionInfo.PlatformId).IsEqualTo(PlatformId);
        await Assert.That(versionInfo.ServicePackVersion).IsEqualTo(ServicePackVersion);
        await Assert.That(versionInfo.ServicePackMajor).IsEqualTo(ServicePackMajor);
        await Assert.That(versionInfo.ServicePackMinor).IsEqualTo(ServicePackMinor);
        await Assert.That(versionInfo.SuiteMask).IsEqualTo(WindowsSuites.TerminalServer);
        await Assert.That(versionInfo.ProductType).IsEqualTo(WindowsProductTypes.VER_NT_WORKSTATION);
    }

    /// <summary>Tests deterministic parsing and copying of native Restart Manager process records.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestRestartManagerApi_ReadsAndCopiesNativeProcessRecordsAsync()
    {
        var nativeRecords = new byte[RestartManagerApi.NativeProcessInfoSize * NativeRecordCount];
        WriteNativeProcessRecord(nativeRecords.AsSpan(FirstRecordIndex, RestartManagerApi.NativeProcessInfoSize), ProcessId, ApplicationName, ServiceShortName, restartable: true);
        WriteNativeProcessRecord(
            nativeRecords.AsSpan(SecondRecordIndex * RestartManagerApi.NativeProcessInfoSize, RestartManagerApi.NativeProcessInfoSize),
            SecondProcessId,
            SecondApplicationName,
            SecondServiceShortName,
            restartable: false);

        var firstProcess = RestartManagerApi.ReadNativeProcessInfo(nativeRecords.AsSpan(FirstRecordIndex, RestartManagerApi.NativeProcessInfoSize));
        var copied = new RmProcessInfo[CopiedRecordCount];
        RestartManagerApi.CopyAffectedApps(nativeRecords, copied, NativeRecordCount);
        RestartManagerApi.CopyAffectedApps(null, copied, CopiedRecordCount);
        RestartManagerApi.CopyAffectedApps(nativeRecords, null, CopiedRecordCount);

        await Assert.That(firstProcess.Process.ProcessId).IsEqualTo(ProcessId);
        await Assert.That(firstProcess.Process.ProcessStartTime.dwLowDateTime).IsEqualTo(ProcessStartLow);
        await Assert.That(firstProcess.Process.ProcessStartTime.dwHighDateTime).IsEqualTo(ProcessStartHigh);
        await Assert.That(firstProcess.ApplicationName).IsEqualTo(ApplicationName);
        await Assert.That(firstProcess.ServiceShortName).IsEqualTo(ServiceShortName);
        await Assert.That(firstProcess.ApplicationType).IsEqualTo(RmAppType.RmConsole);
        await Assert.That(firstProcess.ApplicationStatus).IsEqualTo(RmAppStatus.RmStatusRunning | RmAppStatus.RmStatusRestarted);
        await Assert.That(firstProcess.TerminalServicesSessionId).IsEqualTo(TerminalSessionId);
        await Assert.That(firstProcess.Restartable).IsTrue();
        await Assert.That(copied[FirstRecordIndex]).IsEqualTo(firstProcess);
    }

    /// <summary>Tests high-level Restart Manager branches through a composable session API.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestRestartManager_UsesComposableApiForSuccessBranchesAsync()
    {
        var api = new FakeRestartManagerSessionApi();
        api.Processes.Add(CreateProcessInfo(ProcessId, restartable: true));
        api.RebootReason = RmRebootReason.RmRebootReasonDetectedSelf;

        using var manager = RestartManager.CreateSession(api);
        var process = new RmUniqueProcess(ProcessId, default);
        manager.RegisterFile(FileName);
        await Assert.That(api.RegisteredFileCount).IsEqualTo(1U);

        manager.RegisterProcesses(process);
        await Assert.That(api.RegisteredApplicationCount).IsEqualTo((uint)CopiedRecordCount);

        manager.RegisterServices(ServiceName);
        await Assert.That(api.RegisteredServiceCount).IsEqualTo((uint)CopiedRecordCount);

        var processes = manager.GetProcessesUsingResources();
        var processesWithReason = manager.GetProcessesUsingResources(out var rebootReason);
        manager.Shutdown();
        manager.Shutdown(RmShutdownType.None);
        manager.Shutdown(static _ => { });
        manager.Restart();
        manager.Restart(static _ => { });

        var shutdownProgress = new List<uint>();
        using var shutdownSubscription = manager.ObserveShutdownProgress(RmShutdownType.RmShutdownOnlyRegistered).SubscribeOnNext(shutdownProgress.Add);
        var restartProgress = new List<uint>();
        using var restartSubscription = manager.ObserveRestartProgress().SubscribeOnNext(restartProgress.Add);

        await Assert.That(manager.SessionKey).IsEqualTo(SessionKey);
        await Assert.That(processes.Count).IsEqualTo(CopiedRecordCount);
        await Assert.That(processesWithReason.Count).IsEqualTo(CopiedRecordCount);
        await Assert.That(rebootReason).IsEqualTo(RmRebootReason.RmRebootReasonDetectedSelf);
        await Assert.That(manager.IsRebootRequired()).IsTrue();
        await Assert.That(manager.GetRebootReason()).IsEqualTo(RmRebootReason.RmRebootReasonDetectedSelf);
        await Assert.That(api.ShutdownCallbackValues).Contains(ShutdownProgress);
        await Assert.That(api.RestartCallbackValues).Contains(RestartProgress);
        await Assert.That(shutdownProgress).Contains(ShutdownProgress);
        await Assert.That(restartProgress).Contains(RestartProgress);
    }

    /// <summary>Tests high-level Restart Manager failure branches through a composable session API.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestRestartManager_UsesComposableApiForFailureBranchesAsync()
    {
        var failingStartApi = new FakeRestartManagerSessionApi { StartResult = TestFailure };
        await Assert.That(() => RestartManager.CreateSession(failingStartApi)).Throws<Win32Exception>();
        await Assert.That(static () => RestartManager.CreateSession(null)).Throws<ArgumentNullException>();
        await Assert.That(static () => new RestartManager(SessionHandle, SessionKey, null)).Throws<ArgumentNullException>();

        var registerFailureApi = new FakeRestartManagerSessionApi { RegisterResult = TestFailure };
        using var registerFailure = RestartManager.CreateSession(registerFailureApi);
        await Assert.That(() => registerFailure.RegisterFile(FileName)).Throws<Win32Exception>();
        RmUniqueProcess[] registeredProcesses = [new(ProcessId, default)];
        await Assert.That(() => registerFailure.RegisterProcesses(registeredProcesses)).Throws<Win32Exception>();
        await Assert.That(() => registerFailure.RegisterServices(ServiceName)).Throws<Win32Exception>();

        var sizeFailureApi = new FakeRestartManagerSessionApi { FirstGetListResult = TestFailure };
        using var sizeFailure = RestartManager.CreateSession(sizeFailureApi);
        await Assert.That(() => sizeFailure.GetProcessesUsingResources()).Throws<Win32Exception>();

        var listFailureApi = new FakeRestartManagerSessionApi { SecondGetListResult = TestFailure };
        listFailureApi.Processes.Add(CreateProcessInfo(ProcessId, restartable: false));
        using var listFailure = RestartManager.CreateSession(listFailureApi);
        await Assert.That(() => listFailure.GetProcessesUsingResources()).Throws<Win32Exception>();

        var actionFailureApi = new FakeRestartManagerSessionApi { ShutdownResult = TestFailure, RestartResult = TestFailure };
        using var actionFailure = RestartManager.CreateSession(actionFailureApi);
        await Assert.That(() => actionFailure.Shutdown()).Throws<Win32Exception>();
        await Assert.That(() => actionFailure.Restart()).Throws<Win32Exception>();

        using var shutdownErrorSubscription = actionFailure.ObserveShutdownProgress().SubscribeOnNext(static _ => { });
        using var restartErrorSubscription = actionFailure.ObserveRestartProgress().SubscribeOnNext(static _ => { });
        await Assert.That(shutdownErrorSubscription).IsNotNull();
        await Assert.That(restartErrorSubscription).IsNotNull();
    }

    /// <summary>Writes a native Restart Manager process record.</summary>
    /// <param name="destination">The destination bytes.</param>
    /// <param name="processId">The process identifier.</param>
    /// <param name="applicationName">The application name.</param>
    /// <param name="serviceShortName">The service short name.</param>
    /// <param name="restartable">A value indicating whether restart is supported.</param>
    private static void WriteNativeProcessRecord(Span<byte> destination, int processId, string applicationName, string serviceShortName, bool restartable)
    {
        BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(RestartManagerApi.ProcessIdOffset), processId);
        BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(RestartManagerApi.ProcessStartTimeLowOffset), ProcessStartLow);
        BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(RestartManagerApi.ProcessStartTimeHighOffset), ProcessStartHigh);
        WriteUtf16(destination.Slice(RestartManagerApi.ApplicationNameOffset), applicationName);
        WriteUtf16(destination.Slice(RestartManagerApi.ServiceShortNameOffset), serviceShortName);
        BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(RestartManagerApi.ApplicationTypeOffset), (int)RmAppType.RmConsole);
        BinaryPrimitives.WriteUInt32LittleEndian(
            destination.Slice(RestartManagerApi.ApplicationStatusOffset),
            (uint)(RmAppStatus.RmStatusRunning | RmAppStatus.RmStatusRestarted));
        BinaryPrimitives.WriteUInt32LittleEndian(destination.Slice(RestartManagerApi.TerminalServicesSessionIdOffset), TerminalSessionId);
        BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(RestartManagerApi.RestartableOffset), restartable ? CopiedRecordCount : FirstRecordIndex);
    }

    /// <summary>Writes a null-terminated UTF-16 string into a native byte span.</summary>
    /// <param name="destination">The destination bytes.</param>
    /// <param name="value">The string value.</param>
    private static void WriteUtf16(Span<byte> destination, string value) =>
        Encoding.Unicode.GetBytes(value).AsSpan().CopyTo(destination);

    /// <summary>Creates synthetic Restart Manager process info.</summary>
    /// <param name="processId">The process identifier.</param>
    /// <param name="restartable">A value indicating whether restart is supported.</param>
    /// <returns>The process information.</returns>
    private static RmProcessInfo CreateProcessInfo(int processId, bool restartable) =>
        new(
            new(processId, default),
            ApplicationName,
            ServiceShortName,
            RmAppType.RmConsole,
            RmAppStatus.RmStatusRunning,
            TerminalSessionId,
            restartable);

    /// <summary>Fake Restart Manager API for deterministic high-level tests.</summary>
    private sealed class FakeRestartManagerSessionApi : IRestartManagerSessionApi
    {
        /// <summary>Defines a successful Win32 result.</summary>
        private const int Success = 0;

        /// <summary>Defines the Restart Manager ERROR_MORE_DATA result.</summary>
        private const int ErrorMoreData = 234;

        /// <summary>Gets or sets the start-session result.</summary>
        public int StartResult { get; init; }

        /// <summary>Gets or sets the register result.</summary>
        public int RegisterResult { get; init; }

        /// <summary>Gets or sets the first get-list result.</summary>
        public int FirstGetListResult { get; init; } = ErrorMoreData;

        /// <summary>Gets or sets the second get-list result.</summary>
        public int SecondGetListResult { get; init; }

        /// <summary>Gets or sets the shutdown result.</summary>
        public int ShutdownResult { get; init; }

        /// <summary>Gets or sets the restart result.</summary>
        public int RestartResult { get; init; }

        /// <summary>Gets or sets the reboot reason returned by get-list calls.</summary>
        public RmRebootReason RebootReason { get; set; }

        /// <summary>Gets synthetic processes.</summary>
        public List<RmProcessInfo> Processes { get; } = [];

        /// <summary>Gets shutdown callback values.</summary>
        public List<uint> ShutdownCallbackValues { get; } = [];

        /// <summary>Gets restart callback values.</summary>
        public List<uint> RestartCallbackValues { get; } = [];

        /// <summary>Gets the most recent file registration count.</summary>
        public uint RegisteredFileCount { get; private set; }

        /// <summary>Gets the most recent application registration count.</summary>
        public uint RegisteredApplicationCount { get; private set; }

        /// <summary>Gets the most recent service registration count.</summary>
        public uint RegisteredServiceCount { get; private set; }

        /// <inheritdoc/>
        public int StartSession(out int sessionHandle, int sessionFlags, StringBuilder sessionKey)
        {
            sessionHandle = SessionHandle;
            _ = sessionKey.Append(SessionKey);
            return StartResult;
        }

        /// <inheritdoc/>
        public int EndSession(int sessionHandle) => Success;

        /// <inheritdoc/>
        public int RegisterResources(
            int sessionHandle,
            uint fileCount,
            string[] filenames,
            uint applicationCount,
            RmUniqueProcess[] applications,
            uint serviceCount,
            string[] serviceNames)
        {
            if (fileCount != 0)
            {
                RegisteredFileCount = fileCount;
            }

            if (applicationCount != 0)
            {
                RegisteredApplicationCount = applicationCount;
            }

            if (serviceCount != 0)
            {
                RegisteredServiceCount = serviceCount;
            }

            return RegisterResult;
        }

        /// <inheritdoc/>
        public int GetList(
            int sessionHandle,
            out uint processInfoNeeded,
            ref uint processInfoCount,
            RmProcessInfo[] affectedApplications,
            out RmRebootReason rebootReasons)
        {
            rebootReasons = RebootReason;
            processInfoNeeded = (uint)Processes.Count;

            if (affectedApplications is null)
            {
                return FirstGetListResult;
            }

            for (var i = 0; i < affectedApplications.Length && i < Processes.Count; i++)
            {
                affectedApplications[i] = Processes[i];
            }

            processInfoCount = (uint)Math.Min(affectedApplications.Length, Processes.Count);
            return SecondGetListResult;
        }

        /// <inheritdoc/>
        public int Shutdown(int sessionHandle, RmShutdownType shutdownType, RmStatusCallback statusCallback)
        {
            statusCallback?.Invoke(ShutdownProgress);
            ShutdownCallbackValues.Add(ShutdownProgress);
            return ShutdownResult;
        }

        /// <inheritdoc/>
        public int Restart(int sessionHandle, int restartFlags, RmStatusCallback statusCallback)
        {
            statusCallback?.Invoke(RestartProgress);
            RestartCallbackValues.Add(RestartProgress);
            return RestartResult;
        }
    }
}
