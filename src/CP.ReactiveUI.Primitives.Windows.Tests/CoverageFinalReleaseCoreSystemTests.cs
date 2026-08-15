// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides deterministic release coverage for Core system, messaging, shell, and COM composition.</summary>
public sealed class CoverageFinalReleaseCoreSystemTests
{
    /// <summary>Defines the deterministic AppBar window handle.</summary>
    private const int AppBarWindowHandle = 1;

    /// <summary>Defines the deterministic AppBar callback message.</summary>
    private const int AppBarCallbackMessage = 2;

    /// <summary>Defines the initial AppBar bounds left edge.</summary>
    private const int AppBarBoundsLeft = 3;

    /// <summary>Defines the AppBar bounds top edge.</summary>
    private const int AppBarBoundsTop = 4;

    /// <summary>Defines the AppBar bounds right edge.</summary>
    private const int AppBarBoundsRight = 5;

    /// <summary>Defines the AppBar bounds bottom edge.</summary>
    private const int AppBarBoundsBottom = 6;

    /// <summary>Defines the deterministic changed AppBar handle.</summary>
    private const int ChangedAppBarWindowHandle = 7;

    /// <summary>Defines the deterministic AppBar changed callback message.</summary>
    private const int ChangedAppBarCallbackMessage = 8;

    /// <summary>Defines the changed AppBar bounds left edge.</summary>
    private const int ChangedAppBarBoundsLeft = 9;

    /// <summary>Defines the first deterministic process identifier.</summary>
    private const int FirstProcessIdentifier = 1;

    /// <summary>Defines the second deterministic process identifier.</summary>
    private const int SecondProcessIdentifier = 2;

    /// <summary>Defines the third deterministic process identifier.</summary>
    private const int ThirdProcessIdentifier = 3;

    /// <summary>Defines the deterministic shell file attribute count.</summary>
    private const uint ShellFileAttributeCount = 12U;

    /// <summary>Defines the deterministic shell file info buffer size.</summary>
    private const uint ShellFileInfoBufferSize = 24U;

    /// <summary>Defines the larger deterministic shell file attribute count.</summary>
    private const uint LargerShellFileAttributeCount = 36U;

    /// <summary>Defines the StringBuilder capacity that forces the heap session-key path.</summary>
    private const int HeapSessionKeyCapacity = 128;

    /// <summary>Defines the deterministic native failure result.</summary>
    private const int Failure = 87;

    /// <summary>Defines the deterministic session handle.</summary>
    private const int SessionHandle = 73;

    /// <summary>Defines the deterministic DOS device path.</summary>
    private const string DosDevicePath = @"\Device\CoverageVolume";

    /// <summary>Defines the deterministic DOS process image path.</summary>
    private const string DosProcessPath = DosDevicePath + @"\process.exe";

    /// <summary>Defines the deterministic test component name.</summary>
    private const string ComponentName = "CP.Reactive.Coverage";

    /// <summary>Defines the deterministic process handle.</summary>
    private static readonly IntPtr ProcessHandle = new(71);

    /// <summary>Exercises every managed process-path fallback branch without opening a real process.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Kernel32_ProcessPathFallbacksAreFullyComposedAsync()
    {
        string noDosPath;
        string unmatchedDosPath;
        string matchedDosPath;
        unsafe
        {
            using (Kernel32Api.OverrideOperationsForTesting(CreateKernelOperations(WriteDosDevicePath)))
            using (PsApi.OverrideOperationsForTesting(static _ => 0, NoModuleFileName, NoProcessImageFileName))
            {
                noDosPath = Kernel32Api.GetProcessPath(FirstProcessIdentifier);
            }

            using (Kernel32Api.OverrideOperationsForTesting(CreateKernelOperations(WriteDosDevicePath)))
            using (PsApi.OverrideOperationsForTesting(static _ => 0, NoModuleFileName, WriteUnmatchedDosProcessPath))
            {
                unmatchedDosPath = Kernel32Api.GetProcessPath(SecondProcessIdentifier);
            }

            using (Kernel32Api.OverrideOperationsForTesting(CreateKernelOperations(WriteDosDevicePath)))
            using (PsApi.OverrideOperationsForTesting(static _ => 0, NoModuleFileName, WriteMatchedDosProcessPath))
            {
                matchedDosPath = Kernel32Api.GetProcessPath(ThirdProcessIdentifier);
            }
        }

        await Assert.That(noDosPath).IsNull();
        await Assert.That(unmatchedDosPath).IsNull();
        await Assert.That(matchedDosPath).IsEqualTo(@"B:\process.exe");
    }

    /// <summary>Exercises both stack and heap session-key paths without starting a real Restart Manager session.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RestartManagerApi_StartSessionUsesConfiguredOperationAsync()
    {
        int failedResult;
        int successfulResult;
        int heapResult;
        int failedSession;
        int successfulSession;
        int heapSession;
        var successfulKey = new StringBuilder();
        var heapKey = new StringBuilder(HeapSessionKeyCapacity);
        unsafe
        {
            using (RestartManagerApi.OverrideStartSessionOperationForTesting(FailStartSession))
            {
                failedResult = RestartManagerApi.RmStartSession(out failedSession, 0, new());
            }

            using (RestartManagerApi.OverrideStartSessionOperationForTesting(WriteStartSession))
            {
                successfulResult = RestartManagerApi.RmStartSession(out successfulSession, 0, successfulKey);
                heapResult = RestartManagerApi.RmStartSession(out heapSession, 0, heapKey);
            }
        }

        await Assert.That(failedResult).IsEqualTo(Failure);
        await Assert.That(failedSession).IsEqualTo(-1);
        await Assert.That(successfulResult).IsEqualTo(0);
        await Assert.That(successfulSession).IsEqualTo(SessionHandle);
        await Assert.That(successfulKey.ToString()).IsEqualTo("coverage-session");
        await Assert.That(heapResult).IsEqualTo(0);
        await Assert.That(heapSession).IsEqualTo(SessionHandle);
        await Assert.That(heapKey.ToString()).IsEqualTo("coverage-session");
    }

    /// <summary>Exercises successful and unsuccessful OLE32 identifier conversions through injected operations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Ole32_IdentifierConversionsUseConfiguredOperationsAsync()
    {
        var identifier = Guid.Parse("6D49E246-0C4D-44B2-9B55-FECED39C1EB3");
        using var scope = Ole32Api.OverrideOperationsForTesting(
            (string _, out Guid classId) =>
            {
                classId = identifier;
                return HResult.Ok;
            },
            (ref Guid classId, out string programId) =>
            {
                programId = classId == identifier ? ComponentName : null;
                return classId == identifier ? HResult.Ok : HResult.Fail;
            });

        await Assert.That(Ole32Api.ClassIdFromProgId(ComponentName)).IsEqualTo(identifier);
        await Assert.That(Ole32Api.ProgIdFromClassId(identifier)).IsEqualTo(ComponentName);
        await Assert.That(Ole32Api.ProgIdFromClassId(Guid.Empty)).IsNull();
    }

    /// <summary>Exercises message, appbar, and message-membership equality branches without Windows calls.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MessagingAndAppBarValueMembersCoverEqualityBranchesAsync()
    {
        var data = AppBarData.Create();
        data.SetWindowHandle(new(AppBarWindowHandle));
        data.AutoHide = true;
        data.CallbackMessageIdentifier = AppBarCallbackMessage;
        data.AppBarEdge = AppBarEdges.Left;
        data.Bounds = new(AppBarBoundsLeft, AppBarBoundsTop, AppBarBoundsRight, AppBarBoundsBottom);

        var same = data;
        var differentHandle = data;
        differentHandle.SetWindowHandle(new(ChangedAppBarWindowHandle));
        var differentParameter = data;
        differentParameter.AutoHide = false;
        var differentCallback = data;
        differentCallback.CallbackMessageIdentifier = ChangedAppBarCallbackMessage;
        var differentEdge = data;
        differentEdge.AppBarEdge = AppBarEdges.Right;
        var differentBounds = data;
        differentBounds.Bounds = new(ChangedAppBarBoundsLeft, AppBarBoundsTop, AppBarBoundsRight, AppBarBoundsBottom);
        Msg message = default;

        await Assert.That(data.Equals(same)).IsTrue();
        await Assert.That(data.Equals(differentHandle)).IsFalse();
        await Assert.That(data.Equals(differentParameter)).IsFalse();
        await Assert.That(data.Equals(differentCallback)).IsFalse();
        await Assert.That(data.Equals(differentEdge)).IsFalse();
        await Assert.That(data.Equals(differentBounds)).IsFalse();
        await Assert.That(data.Equals(new object())).IsFalse();
        await Assert.That(message.WindowHandle).IsEqualTo(IntPtr.Zero);
        await Assert.That(message.Equals(default(Msg))).IsTrue();
        await Assert.That(message.Equals(new object())).IsFalse();
        await Assert.That(WindowsMessages.WM_APP.IsIn(WindowsMessages.WM_NULL, WindowsMessages.WM_APP)).IsTrue();
        await Assert.That(WindowsMessages.WM_APP.IsIn(WindowsMessages.WM_NULL)).IsFalse();
        await Assert.That(Shell32Api.GetShellFileInfoBufferSize(ShellFileAttributeCount, ShellFileInfoBufferSize)).IsEqualTo(ShellFileInfoBufferSize);
        await Assert.That(Shell32Api.GetShellFileInfoBufferSize(LargerShellFileAttributeCount, ShellFileInfoBufferSize)).IsEqualTo(LargerShellFileAttributeCount);
    }

    /// <summary>Creates deterministic Kernel32 operations for process-path tests.</summary>
    /// <param name="queryDosDevice">The replacement DOS-device operation.</param>
    /// <returns>The configured operations.</returns>
    private static unsafe Kernel32Operations CreateKernelOperations(QueryDosDeviceOperation queryDosDevice)
    {
        var openCount = 0;
        return new()
        {
            GetLogicalDrives = static () => ["A:\\", "B:\\"],
            SetDefaultDllDirectories = static _ => true,
            SetDllDirectory = static _ => true,
            AllocConsole = static () => true,
            AttachConsole = static _ => true,
            CloseHandle = static _ => true,
            OpenProcess = (_, _, _) =>
            {
                openCount++;
                return openCount == FirstProcessIdentifier ? IntPtr.Zero : ProcessHandle;
            },
            QueryDosDevice = queryDosDevice,
            QueryFullProcessImageName = NoProcessImageName,
            GetVersionEx = static _ => false,
            GetPackageFullName = static (IntPtr _, ref int _, char* _) => Failure,
        };
    }

    /// <summary>Returns a failed process-image-name query.</summary>
    /// <param name="processHandle">The process handle.</param>
    /// <param name="flags">The query flags.</param>
    /// <param name="path">The output path buffer.</param>
    /// <param name="length">The path buffer length.</param>
    /// <returns><see langword="false"/>.</returns>
    private static unsafe bool NoProcessImageName(IntPtr processHandle, uint flags, char* path, ref int length)
    {
        GC.KeepAlive(processHandle);
        GC.KeepAlive(flags);
        GC.KeepAlive((IntPtr)path);
        length = 0;
        return false;
    }

    /// <summary>Returns no module file name.</summary>
    /// <param name="processHandle">The process handle.</param>
    /// <param name="moduleHandle">The module handle.</param>
    /// <param name="path">The output path buffer.</param>
    /// <param name="size">The path buffer size.</param>
    /// <returns>Zero.</returns>
    private static unsafe int NoModuleFileName(nint processHandle, nint moduleHandle, char* path, int size)
    {
        GC.KeepAlive(processHandle);
        GC.KeepAlive(moduleHandle);
        GC.KeepAlive((IntPtr)path);
        GC.KeepAlive(size);
        return 0;
    }

    /// <summary>Returns no process image file name.</summary>
    /// <param name="processHandle">The process handle.</param>
    /// <param name="path">The output path buffer.</param>
    /// <param name="size">The path buffer size.</param>
    /// <returns>Zero.</returns>
    private static unsafe int NoProcessImageFileName(nint processHandle, char* path, int size) =>
        WriteProcessImagePath(processHandle, path, size, string.Empty);

    /// <summary>Writes an unmatched DOS process image path.</summary>
    /// <param name="processHandle">The process handle.</param>
    /// <param name="path">The output path buffer.</param>
    /// <param name="size">The path buffer size.</param>
    /// <returns>The number of written characters.</returns>
    private static unsafe int WriteUnmatchedDosProcessPath(nint processHandle, char* path, int size) =>
        WriteProcessImagePath(processHandle, path, size, @"\Device\OtherVolume\process.exe");

    /// <summary>Writes a matching DOS process image path.</summary>
    /// <param name="processHandle">The process handle.</param>
    /// <param name="path">The output path buffer.</param>
    /// <param name="size">The path buffer size.</param>
    /// <returns>The number of written characters.</returns>
    private static unsafe int WriteMatchedDosProcessPath(nint processHandle, char* path, int size) =>
        WriteProcessImagePath(processHandle, path, size, DosProcessPath);

    /// <summary>Writes a deterministic process image path.</summary>
    /// <param name="processHandle">The process handle.</param>
    /// <param name="path">The output path buffer.</param>
    /// <param name="size">The path buffer size.</param>
    /// <param name="value">The path value.</param>
    /// <returns>The number of written characters.</returns>
    private static unsafe int WriteProcessImagePath(nint processHandle, char* path, int size, string value)
    {
        GC.KeepAlive(processHandle);
        int length = Math.Min(size, value.Length);
        value.AsSpan(0, length).CopyTo(new(path, length));
        return length;
    }

    /// <summary>Writes a deterministic DOS device path for only the second logical drive.</summary>
    /// <param name="deviceName">The logical drive name.</param>
    /// <param name="path">The output path buffer.</param>
    /// <param name="size">The path buffer size.</param>
    /// <returns>The number of written characters.</returns>
    private static unsafe int WriteDosDevicePath(string deviceName, char* path, int size) =>
        deviceName == "B:"
            ? WriteProcessImagePath(0, path, size, DosDevicePath)
            : 0;

    /// <summary>Returns a deterministic failed Restart Manager session start.</summary>
    /// <param name="sessionHandle">Receives the session handle.</param>
    /// <param name="sessionFlags">The session flags.</param>
    /// <param name="sessionKey">The session key buffer.</param>
    /// <returns>The deterministic failure result.</returns>
    private static unsafe int FailStartSession(out int sessionHandle, int sessionFlags, char* sessionKey)
    {
        sessionHandle = -1;
        GC.KeepAlive(sessionFlags);
        GC.KeepAlive((IntPtr)sessionKey);
        return Failure;
    }

    /// <summary>Writes a deterministic Restart Manager session key.</summary>
    /// <param name="sessionHandle">Receives the session handle.</param>
    /// <param name="sessionFlags">The session flags.</param>
    /// <param name="sessionKey">The session key buffer.</param>
    /// <returns>Zero.</returns>
    private static unsafe int WriteStartSession(out int sessionHandle, int sessionFlags, char* sessionKey)
    {
        const string Value = "coverage-session";
        sessionHandle = SessionHandle;
        GC.KeepAlive(sessionFlags);
        Value.AsSpan().CopyTo(new(sessionKey, Value.Length));
        sessionKey[Value.Length] = '\0';
        return 0;
    }
}
