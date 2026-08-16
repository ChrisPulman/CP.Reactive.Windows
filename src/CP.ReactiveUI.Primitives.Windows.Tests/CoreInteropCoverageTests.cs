// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Interop.Com;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.Security;
using CP.ReactiveUI.Primitives.Windows.Native.Security.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Security.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.Shell;
using CP.ReactiveUI.Primitives.Windows.Native.Shell.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles;
using CP.ReactiveUI.Primitives.Windows.Native.Shell.Structs;
using ReactiveUI.Primitives;
using ComFileTime = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Broad coverage for core Windows interop helpers.</summary>
public sealed class CoreInteropCoverageTests
{
    /// <summary>Defines an invalid native session handle.</summary>
    private const int InvalidNativeSessionHandle = -42;

    /// <summary>Defines an invalid process identifier.</summary>
    private const int InvalidProcessId = -1;

    /// <summary>Defines a test process identifier.</summary>
    private const int TestProcessId = 1234;

    /// <summary>Defines the first test FILETIME low value.</summary>
    private const int FileTimeLowValue = 10;

    /// <summary>Defines the first test FILETIME high value.</summary>
    private const int FileTimeHighValue = 20;

    /// <summary>Defines the alternate test FILETIME low value.</summary>
    private const int AlternateFileTimeLowValue = 11;

    /// <summary>Defines a test callback message identifier.</summary>
    private const uint CallbackMessageIdentifier = 500;

    /// <summary>Defines a test terminal-services session identifier.</summary>
    private const uint TerminalServicesSessionId = 7;

    /// <summary>Defines a test shell attribute value.</summary>
    private const uint ShellAttributeValue = 42;

    /// <summary>Defines a test shell icon index.</summary>
    private const int ShellIconIndex = 3;

    /// <summary>Defines an alternate test shell icon index.</summary>
    private const int AlternateShellIconIndex = 4;

    /// <summary>Defines a test appbar left value.</summary>
    private const int AppBarLeft = 1;

    /// <summary>Defines a test appbar top value.</summary>
    private const int AppBarTop = 2;

    /// <summary>Defines a test appbar width value.</summary>
    private const int AppBarWidth = 3;

    /// <summary>Defines a test appbar height value.</summary>
    private const int AppBarHeight = 4;

    /// <summary>Defines a tiny icon width.</summary>
    private const int IconWidth = 1;

    /// <summary>Defines a tiny icon height.</summary>
    private const int IconHeight = 1;

    /// <summary>Defines a test SID attribute flag.</summary>
    private const uint TestSidAttribute = 1;

    /// <summary>Defines the package-name query buffer capacity.</summary>
    private const int PackageNameCapacity = 256;

    /// <summary>Defines the minimum product API Windows major version.</summary>
    private const int VistaMajorVersion = 6;

    /// <summary>Defines the Restart Manager session key length.</summary>
    private const int RestartManagerSessionKeyLength = 32;

    /// <summary>Defines the application restart command-line limit.</summary>
    private const int RestartCommandLineLimit = 1024;

    /// <summary>Defines a test display name.</summary>
    private const string DisplayName = "display";

    /// <summary>Defines a test type name.</summary>
    private const string TypeName = "type";

    /// <summary>Defines a test application name.</summary>
    private const string ApplicationName = "Application";

    /// <summary>Defines a test service name.</summary>
    private const string ServiceName = "Service";

    /// <summary>Tests Kernel32 read-only current-process helpers and invalid-handle branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKernel32_ReadOnlyAndInvalidBranchesAsync()
    {
        using var process = Process.GetCurrentProcess();

        await Assert.That(Kernel32Api.GetCurrentProcessId()).IsEqualTo(process.Id);
        await Assert.That(Kernel32Api.GetCurrentThreadId() > 0).IsTrue();
        await Assert.That(Kernel32Api.GetTickCount64() != 0).IsTrue();
        await Assert.That(Kernel32Api.SystemStartup < TimeProvider.System.GetLocalNow()).IsTrue();

        var processPath = Kernel32Api.GetProcessPath(process.Id);
        await Assert.That(string.IsNullOrWhiteSpace(processPath)).IsFalse();
        await Assert.That(File.Exists(processPath)).IsTrue();
        await Assert.That(process.GetProcessPath()).IsEqualTo(processPath);

        await Assert.That(Kernel32Api.GetProcessPath(InvalidProcessId)).IsNull();
        await Assert.That(Kernel32Api.OpenProcess(ProcessAccessRights.QueryInformation, false, InvalidProcessId)).IsEqualTo(IntPtr.Zero);
        await Assert.That(Kernel32Api.GetModuleHandle("kernel32.dll")).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(Kernel32Api.GetModuleHandle("missing-cp-reactive-windows-module.dll")).IsEqualTo(IntPtr.Zero);
        await Assert.That(Kernel32Api.LocalFree(IntPtr.Zero)).IsEqualTo(IntPtr.Zero);
        await Assert.That(Kernel32Api.CloseHandle(IntPtr.Zero)).IsFalse();
    }

    /// <summary>Tests Kernel32 package name and product APIs without changing process state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestKernel32_PackageAndProductQueriesAsync()
    {
        using var process = Process.GetCurrentProcess();
        var packageName = new StringBuilder(PackageNameCapacity);
        var packageNameLength = packageName.Capacity;
        var packageResult = Kernel32Api.GetPackageFullName(process.Handle, ref packageNameLength, packageName);

        await Assert.That(packageResult >= 0).IsTrue();
        await Assert.That(packageNameLength >= 0).IsTrue();

        var productInfoResult = Kernel32Api.GetProductInfo(VistaMajorVersion, 0, 0, 0, out var edition);
        await Assert.That(productInfoResult).IsTrue();
#if NETFRAMEWORK
        await Assert.That(Enum.IsDefined(typeof(WindowsProducts), edition)).IsTrue();
#else
        await Assert.That(Enum.IsDefined(edition)).IsTrue();
#endif
    }

    /// <summary>Tests Advapi32 session and SID helper branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestAdvapi32_SessionAndSidBranchesAsync()
    {
        var currentSessionId = Advapi32Api.CurrentSessionId;
        await Assert.That(string.IsNullOrWhiteSpace(currentSessionId)).IsFalse();
        await Assert.That(currentSessionId).StartsWith("S-");

        var sidAndAttributes = default(SidAndAttributes);
        await Assert.That(sidAndAttributes.HasAttributes(0)).IsTrue();
        await Assert.That(sidAndAttributes.HasAttributes(TestSidAttribute)).IsFalse();
        await Assert.That(sidAndAttributes.ToSidString()).IsEqualTo(string.Empty);
        await Assert.That(sidAndAttributes == default).IsTrue();
        await Assert.That(sidAndAttributes != default).IsFalse();
        await Assert.That(sidAndAttributes.Equals(new object())).IsFalse();
    }

    /// <summary>Tests registry monitor observable construction and invalid open error delivery.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestRegistryMonitor_InvalidKeyReportsErrorAsync()
    {
        var observer = new RecordingObserver<RxVoid>();
        using var subscription = RegistryMonitor
            .ObserveChanges((RegistryHive)int.MinValue, @"Software\CPReactiveWindowsMissing", RegistryNotifyFilter.ChangeName)
            .Subscribe(observer);

        await Assert.That(observer.Error).IsNotNull();
        await Assert.That(observer.Values.Count).IsEqualTo(0);
        await Assert.That(observer.Completed).IsFalse();
        await Assert.That(subscription).IsNotNull();
    }

    /// <summary>Tests COM disposable and ProgID attribute helper branches without requiring a COM server.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestComHelpers_NullPlainObjectAndInheritedAttributeBranchesAsync()
    {
        await Assert.That(DisposableCom.Create<object>(null)).IsNull();

        var instance = new object();
        var disposable = DisposableCom.Create(instance);
        await Assert.That(disposable).IsNotNull();
        await Assert.That(disposable.ComObject).IsEqualTo(instance);
        disposable.Dispose();
        await Assert.That(disposable.ComObject).IsNull();

        var directAttribute = ComProgIdAttribute.GetAttribute(typeof(IAttributedComContract));
        var inheritedAttribute = ComProgIdAttribute.GetAttribute(typeof(IDerivedComContract));
        var missingAttribute = ComProgIdAttribute.GetAttribute(typeof(IUnattributedComContract));

        await Assert.That(directAttribute.Value).IsEqualTo("CP.Reactive.Windows.Test");
        await Assert.That(inheritedAttribute.Value).IsEqualTo(directAttribute.Value);
        await Assert.That(missingAttribute).IsNull();
        await Assert.That(static () => ComProgIdAttribute.GetAttribute(null)).Throws<ArgumentNullException>();
    }

    /// <summary>Tests shell structs, safe icon handles, and read-only shell queries.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestShell32_StructsSafeHandlesAndQueriesAsync()
    {
        var bounds = new NativeRect(AppBarLeft, AppBarTop, AppBarWidth, AppBarHeight);
        var appBarData = AppBarData.Create();
        appBarData.SetWindowHandle(new(IconWidth));
        appBarData.CallbackMessageIdentifier = CallbackMessageIdentifier;
        appBarData.AppBarEdge = AppBarEdges.Bottom;
        appBarData.Bounds = bounds;
        appBarData.AutoHide = true;

        var matchingAppBarData = AppBarData.Create();
        matchingAppBarData.Apply(appBarData.ToNative());

        await Assert.That(matchingAppBarData).IsEqualTo(appBarData);
        await Assert.That(matchingAppBarData == appBarData).IsTrue();
        await Assert.That(matchingAppBarData != appBarData).IsFalse();
        await Assert.That(matchingAppBarData.Equals(new object())).IsFalse();
        await Assert.That(matchingAppBarData.GetHashCode()).IsEqualTo(0);

        matchingAppBarData.State = AppBarStates.AllwaysOnTop;
        await Assert.That(matchingAppBarData.AutoHide).IsTrue();
        await Assert.That(matchingAppBarData.State).IsEqualTo(AppBarStates.AllwaysOnTop);

        var shellInfo = new ShellFileInfo(IntPtr.Zero, ShellIconIndex, ShellAttributeValue, DisplayName, TypeName);
        var sameShellInfo = new ShellFileInfo(IntPtr.Zero, ShellIconIndex, ShellAttributeValue, DisplayName, TypeName);
        var differentShellInfo = new ShellFileInfo(IntPtr.Zero, AlternateShellIconIndex, ShellAttributeValue, DisplayName, TypeName);
        await Assert.That(shellInfo).IsEqualTo(sameShellInfo);
        await Assert.That(shellInfo == sameShellInfo).IsTrue();
        await Assert.That(shellInfo != differentShellInfo).IsTrue();
        await Assert.That(shellInfo.Equals(new object())).IsFalse();
        await Assert.That(shellInfo.IconHandle.IsInvalid).IsTrue();

        using var invalidIconHandle = new SafeIconHandle();
        await Assert.That(invalidIconHandle.IsInvalid).IsTrue();
        await Assert.That(invalidIconHandle.UseNativeHandle(static handle => handle)).IsEqualTo(IntPtr.Zero);
        await Assert.That(() => invalidIconHandle.UseNativeHandle<int>(null)).Throws<ArgumentNullException>();

        using var bitmap = new Bitmap(IconWidth, IconHeight);
        using var iconHandle = new SafeIconHandle(bitmap);
        await Assert.That(iconHandle.IsInvalid).IsFalse();
        await Assert.That(iconHandle.UseNativeHandle(static handle => handle != IntPtr.Zero)).IsTrue();

        var fileInfo = default(ShellFileInfo);
        var result = Shell32Api.SHGetFileInfo(
            Process.GetCurrentProcess().MainModule?.FileName,
            ShellFileAttributeFlags.Normal,
            ref fileInfo,
            0,
            ShellGetFileInfoFlags.DisplayName | ShellGetFileInfoFlags.TypeName);

        await Assert.That(result).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(string.IsNullOrWhiteSpace(fileInfo.DisplayName)).IsFalse();
        await Assert.That(string.IsNullOrWhiteSpace(fileInfo.TypeName)).IsFalse();
    }

    /// <summary>Tests Restart Manager structs and safe non-destructive APIs.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestRestartManager_StructsAndSafeApisAsync()
    {
        var fileTime = new ComFileTime { dwLowDateTime = FileTimeLowValue, dwHighDateTime = FileTimeHighValue };
        var alternateFileTime = new ComFileTime { dwLowDateTime = AlternateFileTimeLowValue, dwHighDateTime = FileTimeHighValue };
        var process = new RmUniqueProcess(TestProcessId, fileTime);
        var sameProcess = new RmUniqueProcess(TestProcessId, fileTime);
        var differentProcess = new RmUniqueProcess(TestProcessId, alternateFileTime);

        await Assert.That(process).IsEqualTo(sameProcess);
        await Assert.That(process == sameProcess).IsTrue();
        await Assert.That(process != differentProcess).IsTrue();
        await Assert.That(process.Equals(new object())).IsFalse();
        await Assert.That(process.GetHashCode()).IsEqualTo(sameProcess.GetHashCode());

        var processInfo = new RmProcessInfo(
            process,
            ApplicationName,
            ServiceName,
            RmAppType.RmConsole,
            RmAppStatus.RmStatusRunning,
            TerminalServicesSessionId,
            restartable: true);
        var sameProcessInfo = new RmProcessInfo(
            process,
            ApplicationName,
            ServiceName,
            RmAppType.RmConsole,
            RmAppStatus.RmStatusRunning,
            TerminalServicesSessionId,
            restartable: true);
        var differentProcessInfo = new RmProcessInfo(
            process,
            ApplicationName,
            ServiceName,
            RmAppType.RmService,
            RmAppStatus.RmStatusStopped,
            TerminalServicesSessionId,
            restartable: false);

        await Assert.That(RestartManagerApi.SessionKeyLength).IsEqualTo(RestartManagerSessionKeyLength);
        await Assert.That(RestartManagerApi.InvalidSession).IsEqualTo(-1);
        await Assert.That(RestartManagerApi.InvalidTerminalServicesSession).IsEqualTo(uint.MaxValue);
        await Assert.That(static () => RestartManagerApi.RmStartSession(out _, 0, null)).Throws<ArgumentNullException>();

        await Assert.That(processInfo).IsEqualTo(sameProcessInfo);
        await Assert.That(processInfo == sameProcessInfo).IsTrue();
        await Assert.That(processInfo != differentProcessInfo).IsTrue();
        await Assert.That(processInfo.Equals(new object())).IsFalse();
        await Assert.That(processInfo.GetHashCode()).IsEqualTo(sameProcessInfo.GetHashCode());
    }

    /// <summary>Tests Restart Manager invalid native handle branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestRestartManager_InvalidNativeSessionApisAsync()
    {
        uint processInfoCount = 0;
        var getListResult = RestartManagerApi.RmGetList(
            InvalidNativeSessionHandle,
            out var processInfoNeeded,
            ref processInfoCount,
            null,
            out var rebootReason);
        await Assert.That(getListResult).IsNotEqualTo(0);
        await Assert.That(processInfoNeeded).IsEqualTo(0U);
        await Assert.That(rebootReason).IsEqualTo(RmRebootReason.None);
        await Assert.That(RestartManagerApi.RmEndSession(InvalidNativeSessionHandle)).IsNotEqualTo(0);
    }

    /// <summary>Tests Restart Manager session APIs that do not stop or restart applications.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestRestartManager_SafeSessionApisAsync()
    {
        var manager = RestartManager.CreateSession();
        await Assert.That(string.IsNullOrWhiteSpace(manager.SessionKey)).IsFalse();
        manager.RegisterFiles(null);
        manager.RegisterFiles([]);
        manager.RegisterProcesses(null);
        manager.RegisterProcesses([]);
        manager.RegisterServices(null);
        manager.RegisterServices([]);
        await Assert.That(manager.GetProcessesUsingResources()).IsNotNull();
        await Assert.That(manager.GetRebootReason()).IsEqualTo(RmRebootReason.None);
        await Assert.That(manager.IsRebootRequired()).IsFalse();

        manager.Dispose();
        manager.Dispose();
        await Assert.That(() => manager.RegisterFile(Process.GetCurrentProcess().MainModule?.FileName)).Throws<ObjectDisposedException>();
    }

    /// <summary>Tests application restart pure guard/value behavior without registering the process.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestApplicationRestartManager_PureBranchesAsync()
    {
        var tooLongCommandLine = new string('x', ApplicationRestartManager.MaxCommandLineLength + 1);

        await Assert.That(ApplicationRestartManager.RestartMaxCmdLine).IsEqualTo(RestartCommandLineLimit);
        await Assert.That(ApplicationRestartManager.MaxCommandLineLength).IsEqualTo(ApplicationRestartManager.RestartMaxCmdLine);
        await Assert.That(ApplicationRestartManager.WasRestartRequested()).IsFalse();
        await Assert.That(ApplicationRestartManager.GetRestartCommandLineArgs()).IsNotNull();
        await Assert.That(() => RegisterForRestart(tooLongCommandLine)).Throws<ArgumentException>();

        var endSessionMessage = new EndSessionMessage(WindowsMessages.WM_QUERYENDSESSION, EndSessionReasons.ENDSESSION_LOGOFF) { Handled = true, Result = 1 };
        var sameEndSessionMessage = endSessionMessage with { };

        await Assert.That(endSessionMessage).IsEqualTo(sameEndSessionMessage);
        await Assert.That(endSessionMessage.Handled).IsTrue();
        await Assert.That(endSessionMessage.Result).IsEqualTo(1);
    }

    /// <summary>Registers too-long restart arguments to exercise the managed guard branch.</summary>
    /// <param name="commandLine">The command line to register.</param>
    private static void RegisterForRestart(string commandLine) =>
        ApplicationRestartManager.RegisterForRestart(commandLine, ApplicationRestartFlags.None);

    /// <summary>Records observable notifications for synchronous branch tests.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    internal sealed class RecordingObserver<T> : IObserver<T>
    {
        /// <summary>Gets observed values.</summary>
        public List<T> Values { get; } = [];

        /// <summary>Gets the observed error.</summary>
        public Exception Error { get; private set; }

        /// <summary>Gets a value indicating whether completion was observed.</summary>
        public bool Completed { get; private set; }

        /// <inheritdoc/>
        public void OnCompleted() => Completed = true;

        /// <inheritdoc/>
        public void OnError(Exception error) => Error = error;

        /// <inheritdoc/>
        public void OnNext(T value) => Values.Add(value);
    }
}
