// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Kernel;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Exercises User32 wrappers with read-only or invalid-handle native operations.</summary>
public sealed class CoverageWave4User32NativeWrapperTests
{
    /// <summary>Exercises cursor, display, text, and process-resource read operations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task User32Api_ReadOnlyDesktopAndTextOperations_AreCallableAsync()
    {
        var noWindow = IntPtr.Zero;
        var exception = User32Api.CreateWin32Exception(nameof(User32Api.CreateWin32Exception));
        var cursorLocation = User32Api.GetCursorLocation();
        NativePoint expectedCursorLocation = new(cursorLocation.X, cursorLocation.Y);
        var inputDesktop = User32Api.OpenInputDesktop(0, false, DesktopAccessRight.DESKTOP_READOBJECTS);
        var deviceContext = User32Api.GetDC(noWindow);

        _ = User32Api.SetThreadDesktop(noWindow);
        _ = User32Api.GetClassLongWrapper(noWindow, default);
        _ = User32Api.GetGuiResourcesGdiCount();
        _ = User32Api.GetGuiResourcesUserCount();
        _ = User32Api.GetTitleBarInfoEx(noWindow);
        _ = User32Api.GetWindowLongWrapper(noWindow, default);
        _ = User32Api.SetWindowLongWrapper(noWindow, default, noWindow);
        _ = User32Api.SetExtendedWindowStyle(noWindow, default);
        _ = User32Api.SetWindowStyle(noWindow, default);

        if (inputDesktop != noWindow)
        {
            _ = User32Api.CloseDesktop(inputDesktop);
        }

        if (deviceContext != noWindow)
        {
            _ = User32Api.ReleaseDC(noWindow, deviceContext);
        }

        await Assert.That(exception.Data["Method"]).IsEqualTo(nameof(User32Api.CreateWin32Exception));
        await Assert.That(cursorLocation).IsEqualTo(expectedCursorLocation);
        await Assert.That(User32Api.GetDisplayInfo(noWindow, 0)).IsNull();
        await Assert.That(User32Api.EnumDisplays().Count).IsGreaterThanOrEqualTo(0);
        await Assert.That(User32Api.EnumThreadWindows(int.MaxValue)).IsEmpty();
        await Assert.That(User32Api.GetClassname(noWindow)).IsEmpty();
        await Assert.That(User32Api.GetText(noWindow)).IsEmpty();
        await Assert.That(User32Api.GetTextFromWindow(noWindow)).IsNull();
        await Assert.That(User32Api.GetWindowTextLength(noWindow)).IsEqualTo(0);
    }

    /// <summary>Exercises invalid-handle window and system-parameter operations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task User32Api_InvalidHandleOperations_AreCallableAsync()
    {
        const int ParameterBufferCapacity = 4;
        var noWindow = IntPtr.Zero;
        var placement = WindowPlacement.Create();
        var windowInfo = WindowInfo.Create();
        var scrollInfo = ScrollInfo.Create(ScrollInfoMask.All);
        var scrollBarInfo = ScrollBarInfo.Create();
        var cursorInfo = CursorInfo.Create();
        NativeRect rectangle = default;
        NativePoint point = default;
        var animationInfo = AnimationInfo.Create();
        var parameterBuffer = new StringBuilder(ParameterBufferCapacity);

        _ = User32Api.TrySendMessage(noWindow, WindowsMessages.WM_NULL, noWindow, out _);
        _ = User32Api.SetParent(noWindow, noWindow);
        _ = User32Api.SetWindowText(noWindow, string.Empty);
        _ = User32Api.GetSysColor(SystemColorIndex.ScrollBar);
        _ = User32Api.GetWindowPlacement(noWindow, ref placement);
        _ = User32Api.SetWindowPlacement(noWindow, ref placement);
        _ = User32Api.PrintWindow(noWindow, noWindow, default);
        _ = User32Api.SendMessage(noWindow, WindowsMessages.WM_SYSCOMMAND, SysCommands.None, noWindow);
        _ = User32Api.SendMessage(noWindow, WindowsMessages.WM_NULL, noWindow, string.Empty);
        _ = User32Api.MonitorFromWindow(noWindow, MonitorFrom.None);
        _ = User32Api.MonitorFromRect(ref rectangle, MonitorFrom.None);
        _ = User32Api.GetWindowInfo(noWindow, ref windowInfo);
        _ = User32Api.EnumWindows(static (_, _) => false, noWindow);
        _ = User32Api.GetScrollInfo(noWindow, ScrollBarTypes.Control, ref scrollInfo);
        _ = User32Api.SetScrollInfo(noWindow, ScrollBarTypes.Control, ref scrollInfo, false);
        _ = User32Api.ShowScrollBar(noWindow, ScrollBarTypes.Control, false);
        _ = User32Api.GetScrollBarInfo(noWindow, default, ref scrollBarInfo);
        _ = User32Api.SetWindowDisplayAffinity(noWindow, default);
        _ = User32Api.GetWindowDisplayAffinity(noWindow, out _);
        _ = User32Api.MapWindowPoints(noWindow, noWindow, ref point, 1);
        _ = User32Api.SystemParametersInfo(SystemParametersInfoActions.SPI_NONE, 0, string.Empty, SystemParametersInfoBehaviors.None);
        _ = User32Api.SystemParametersInfo(SystemParametersInfoActions.SPI_GETBEEP, 0, parameterBuffer, SystemParametersInfoBehaviors.None);
        _ = User32Api.SystemParametersInfo(SystemParametersInfoActions.SPI_GETANIMATION, 0, ref animationInfo, SystemParametersInfoBehaviors.None);
        _ = User32Api.GetCursorInfo(ref cursorInfo);
        _ = User32Api.DestroyCursor(noWindow);
        _ = User32Api.FillRect(noWindow, ref rectangle, noWindow);

        await Assert.That(User32Api.GetParent(noWindow)).IsEqualTo(noWindow);
        await Assert.That(User32Api.GetWindow(noWindow, default)).IsEqualTo(noWindow);
        await Assert.That(User32Api.IsWindow(noWindow)).IsFalse();
        await Assert.That(User32Api.IsWindowVisible(noWindow)).IsFalse();
        await Assert.That(User32Api.IsIconic(noWindow)).IsFalse();
        await Assert.That(User32Api.IsZoomed(noWindow)).IsFalse();
    }

    /// <summary>Exercises the large-text branch against a private, unshown managed window.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task User32Api_GetTextFromWindow_LargeManagedCaption_RoundTripsAsync()
    {
        const int LargeCaptionLength = 300;
        var expectedText = new string('x', LargeCaptionLength);
        using var window = new Form { Text = expectedText };

        var actualText = User32Api.GetTextFromWindow(window.Handle);

        await Assert.That(actualText).IsEqualTo(expectedText);
    }

    /// <summary>Exercises read-only Kernel32 process and operating-system wrappers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Kernel32Api_CurrentProcessReadOperations_AreCallableAsync()
    {
        const int WindowsMajorVersion = 10;
        using var process = Process.GetCurrentProcess();
        var processHandle = Kernel32Api.OpenProcess(ProcessAccessRights.QueryInformation, false, process.Id);
        var version = OsVersionInfoEx.Create();

        try
        {
            _ = Kernel32Api.GetProductInfo(WindowsMajorVersion, 0, 0, 0, out _);
            _ = Kernel32Api.GetVersionEx(ref version);
            _ = Kernel32Api.GetTickCount64();
            _ = Kernel32Api.SystemStartup;
            _ = Kernel32Api.FreeLibrary(IntPtr.Zero);
            _ = Kernel32Api.GetModuleHandle("kernel32.dll");
            _ = PsApi.GetModuleFilename(processHandle, IntPtr.Zero);
            _ = PsApi.GetProcessImageFileName(processHandle);
        }
        finally
        {
            if (processHandle != IntPtr.Zero)
            {
                _ = Kernel32Api.CloseHandle(processHandle);
            }
        }

        await Assert.That(Kernel32Api.GetCurrentProcessId()).IsEqualTo(process.Id);
        await Assert.That(Kernel32Api.GetCurrentThreadId()).IsGreaterThan(0);
        await Assert.That(Kernel32Api.GetProcessPath(process.Id)).IsNotNull();
    }

    /// <summary>Exercises all composed input-desktop safe-handle outcomes without native desktop changes.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SafeCurrentInputDesktopHandle_ComposedOperations_CoverSuccessAndFailureAsync()
    {
        const int SuccessfulHandleValue = 1;
        const int UnsuccessfulSetHandleValue = 2;
        var success = new DesktopOperationProbe((IntPtr)SuccessfulHandleValue, true);
        var unsuccessfulSet = new DesktopOperationProbe((IntPtr)UnsuccessfulSetHandleValue, false);
        var failedOpen = new DesktopOperationProbe(IntPtr.Zero, true);

        using (var successfulHandle = new TestInputDesktopHandle(success.Open, success.Set, success.Close))
        {
            await Assert.That(successfulHandle.IsInvalid).IsFalse();
        }

        using (var unsuccessfulSetHandle = new TestInputDesktopHandle(unsuccessfulSet.Open, unsuccessfulSet.Set, unsuccessfulSet.Close))
        {
            await Assert.That(unsuccessfulSetHandle.IsInvalid).IsFalse();
        }

        using (var failedOpenHandle = new TestInputDesktopHandle(failedOpen.Open, failedOpen.Set, failedOpen.Close))
        {
            await Assert.That(failedOpenHandle.IsInvalid).IsTrue();
        }

        await Assert.That(success.CloseCount).IsEqualTo(1);
        await Assert.That(unsuccessfulSet.CloseCount).IsEqualTo(1);
        await Assert.That(failedOpen.CloseCount).IsEqualTo(0);
    }

    /// <summary>Exercises the public input-desktop constructor with composed operations only.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SafeCurrentInputDesktopHandle_DefaultConstructor_UsesOverriddenOperationsAsync()
    {
        const int DesktopHandleValue = 3;
        var operations = new DesktopOperationProbe((IntPtr)DesktopHandleValue, true);

        using (SafeCurrentInputDesktopHandle.OverrideDefaultOperationsForTesting(
            operations.Open,
            operations.Set,
            operations.Close))
        using (var handle = new SafeCurrentInputDesktopHandle())
        {
            await Assert.That(handle.IsInvalid).IsFalse();
        }

        await Assert.That(operations.CloseCount).IsEqualTo(1);
    }

    /// <summary>Provides deterministic input-desktop operation results.</summary>
    private sealed class DesktopOperationProbe
    {
        /// <summary>Initializes a new instance of the <see cref="DesktopOperationProbe"/> class.</summary>
        /// <param name="desktopHandle">The handle returned when opening.</param>
        /// <param name="setResult">The result returned when setting the thread desktop.</param>
        internal DesktopOperationProbe(IntPtr desktopHandle, bool setResult)
        {
            DesktopHandle = desktopHandle;
            SetResult = setResult;
        }

        /// <summary>Gets the number of close invocations.</summary>
        internal int CloseCount { get; private set; }

        /// <summary>Gets the desktop handle returned by <see cref="Open"/>.</summary>
        private IntPtr DesktopHandle { get; }

        /// <summary>Gets the result returned by <see cref="Set"/>.</summary>
        private bool SetResult { get; }

        /// <summary>Closes a desktop handle.</summary>
        /// <param name="desktopHandle">The desktop handle.</param>
        /// <returns>True.</returns>
        internal bool Close(IntPtr desktopHandle)
        {
            _ = desktopHandle;
            CloseCount++;
            return true;
        }

        /// <summary>Opens the configured desktop handle.</summary>
        /// <returns>The configured desktop handle.</returns>
        internal IntPtr Open() => DesktopHandle;

        /// <summary>Sets the thread desktop.</summary>
        /// <param name="desktopHandle">The desktop handle.</param>
        /// <returns>The configured result.</returns>
        internal bool Set(IntPtr desktopHandle)
        {
            _ = desktopHandle;
            return SetResult;
        }
    }

    /// <summary>Provides a public test facade over the composed safe-handle constructor.</summary>
    private sealed class TestInputDesktopHandle : SafeCurrentInputDesktopHandle
    {
        /// <summary>Initializes a new instance of the <see cref="TestInputDesktopHandle"/> class.</summary>
        /// <param name="openInputDesktop">Opens the input desktop.</param>
        /// <param name="setThreadDesktop">Sets the thread desktop.</param>
        /// <param name="closeDesktop">Closes the desktop.</param>
        internal TestInputDesktopHandle(
            Func<IntPtr> openInputDesktop,
            Func<IntPtr, bool> setThreadDesktop,
            Func<IntPtr, bool> closeDesktop)
            : base(openInputDesktop, setThreadDesktop, closeDesktop)
        {
        }
    }
}
