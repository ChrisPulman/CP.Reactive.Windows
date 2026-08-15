// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides deterministic final coverage for composed User32 and safe-handle behavior.</summary>
public sealed class CoverageFinalReleaseUser32Tests
{
    /// <summary>Defines the deterministic physical cursor X coordinate.</summary>
    private const int PhysicalCursorX = 10;

    /// <summary>Defines the deterministic physical cursor Y coordinate.</summary>
    private const int PhysicalCursorY = 20;

    /// <summary>Defines the deterministic fallback cursor X coordinate.</summary>
    private const int FallbackCursorX = 30;

    /// <summary>Defines the deterministic fallback cursor Y coordinate.</summary>
    private const int FallbackCursorY = 40;

    /// <summary>Defines the synthetic 32-bit pointer value.</summary>
    private const int PointerWidth32Value = 32;

    /// <summary>Defines the synthetic 64-bit pointer value.</summary>
    private const int PointerWidth64Value = 64;

    /// <summary>Defines the synthetic input handle value.</summary>
    private const int InputHandleValue = 64;

    /// <summary>Defines the deterministic monitor bounds left coordinate.</summary>
    private const int BoundsLeft = 1;

    /// <summary>Defines the deterministic monitor bounds top coordinate.</summary>
    private const int BoundsTop = 2;

    /// <summary>Defines the deterministic monitor bounds right coordinate.</summary>
    private const int BoundsRight = 11;

    /// <summary>Defines the deterministic monitor bounds bottom coordinate.</summary>
    private const int BoundsBottom = 22;

    /// <summary>Defines the monitor information structure size.</summary>
    private const int MonitorInfoSize = 104;

    /// <summary>Defines the synthetic monitor handle value.</summary>
    private const int MonitorHandleValue = 7;

    /// <summary>Defines the synthetic safe monitor handle value.</summary>
    private const int SafeMonitorHandleValue = 9;

    /// <summary>Exercises physical-cursor success, fallback, and unavailable-entry-point paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task User32Api_CursorOperations_ComposedResultsCoverEveryFallbackAsync()
    {
        using (User32Api.OverrideOperationsForTesting(CreateOperations(
            getPhysicalCursorPos: static (out NativePoint point) =>
            {
                point = new(PhysicalCursorX, PhysicalCursorY);
                return true;
            },
            getCursorPosition: static () => new(FallbackCursorX, FallbackCursorY))))
        {
            await Assert.That(User32Api.GetCursorLocation()).IsEqualTo(new(PhysicalCursorX, PhysicalCursorY));
        }

        using (User32Api.OverrideOperationsForTesting(CreateOperations(
            getPhysicalCursorPos: static (out NativePoint point) =>
            {
                point = default;
                return false;
            },
            getCursorPosition: static () => new(FallbackCursorX, FallbackCursorY))))
        {
            await Assert.That(User32Api.GetCursorLocation()).IsEqualTo(new(FallbackCursorX, FallbackCursorY));
        }

        using (User32Api.OverrideOperationsForTesting(CreateOperations(
            getPhysicalCursorPos: ThrowMissingPhysicalCursorPosition,
            getCursorPosition: static () => new(FallbackCursorX, FallbackCursorY))))
        {
            await Assert.That(User32Api.GetCursorLocation()).IsEqualTo(new(FallbackCursorX, FallbackCursorY));
        }
    }

    /// <summary>Exercises both pointer-width wrapper paths without native window manipulation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task User32Api_PointerWidthWrappers_ComposedOperationsRoundTripAsync()
    {
        IntPtr expected32 = new(PointerWidth32Value);
        IntPtr expected64 = new(PointerWidth64Value);

        using (User32Api.OverrideOperationsForTesting(CreateOperations(is64BitProcess: static () => false)))
        {
            await Assert.That(User32Api.GetClassLongWrapper(IntPtr.Zero, default)).IsEqualTo(expected32);
            await Assert.That(User32Api.GetWindowLongWrapper(IntPtr.Zero, default)).IsEqualTo(expected32);
            await Assert.That(User32Api.SetWindowLongWrapper(IntPtr.Zero, default, expected32)).IsEqualTo(expected32);
        }

        using (User32Api.OverrideOperationsForTesting(CreateOperations(is64BitProcess: static () => true)))
        {
            await Assert.That(User32Api.GetClassLongWrapper(IntPtr.Zero, default)).IsEqualTo(expected64);
            await Assert.That(User32Api.GetWindowLongWrapper(IntPtr.Zero, default)).IsEqualTo(expected64);
            await Assert.That(User32Api.SetWindowLongWrapper(IntPtr.Zero, default, expected64)).IsEqualTo(expected64);
        }
    }

    /// <summary>Exercises input and parameter wrappers without changing desktop state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task User32Api_ComposedInputAndParameters_AvoidNativeDesktopMutationAsync()
    {
        const int BufferCapacity = 4;
        IntPtr expected = new(InputHandleValue);
        var nullTerminated = new StringBuilder(BufferCapacity);
        var full = new StringBuilder(BufferCapacity);
        var failed = new StringBuilder(BufferCapacity);

        using (User32Api.OverrideOperationsForTesting(CreateOperations(
            systemParametersInfoBuffer: WriteNullTerminatedBuffer)))
        {
            await Assert.That(User32Api.SetFocus(IntPtr.Zero)).IsEqualTo(expected);
            await Assert.That(User32Api.SetCapture(IntPtr.Zero)).IsEqualTo(expected);
            await Assert.That(User32Api.ReleaseCapture()).IsTrue();
            await Assert.That(User32Api.LockWorkStation()).IsTrue();
            await Assert.That(User32Api.SystemParametersInfo(default, 0, nullTerminated, default)).IsTrue();
            await Assert.That(nullTerminated.ToString()).IsEqualTo("ab");
        }

        using (User32Api.OverrideOperationsForTesting(CreateOperations(
            systemParametersInfoBuffer: WriteFullBuffer)))
        {
            await Assert.That(User32Api.SystemParametersInfo(default, 0, full, default)).IsTrue();
            await Assert.That(full.ToString()).IsEqualTo("abcd");
        }

        using (User32Api.OverrideOperationsForTesting(CreateOperations(
            systemParametersInfoBuffer: static (_, _, _, _) => false)))
        {
            await Assert.That(User32Api.SystemParametersInfo(default, 0, failed, default)).IsFalse();
        }
    }

    /// <summary>Exercises the empty text-buffer pinning path without invoking User32.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task User32Api_SystemParametersInfo_EmptyComposedBufferSucceedsAsync()
    {
        var empty = new StringBuilder(0);

        using (User32Api.OverrideOperationsForTesting(CreateOperations(
            systemParametersInfoBuffer: static (_, _, _, _) => true)))
        {
            await Assert.That(User32Api.SystemParametersInfo(default, 0, empty, default)).IsTrue();
        }

        await Assert.That(empty).IsEmpty();
    }

    /// <summary>Exercises the public input-desktop constructor through composed User32 operations only.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SafeCurrentInputDesktopHandle_DefaultOperations_AreComposedWithoutNativeCallsAsync()
    {
        const int DesktopHandleValue = 77;
        var closeCount = 0;
        var desktopOperations = new DesktopOperationOverrides(
            static (_, _, _) => new(DesktopHandleValue),
            static _ => true,
            _ =>
            {
                closeCount++;
                return true;
            });

        using (User32Api.OverrideOperationsForTesting(CreateOperations(
            desktopOperations: desktopOperations)))
        using (var handle = new SafeCurrentInputDesktopHandle())
        {
            await Assert.That(handle.IsInvalid).IsFalse();
        }

        await Assert.That(closeCount).IsEqualTo(1);
    }

    /// <summary>Exercises monitor callback continuation and rejection without monitor enumeration.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task User32Api_MonitorCallback_ComposedStateBranchesAreDeterministicAsync()
    {
        var displays = new List<DisplayInfo>();
        NativeRect bounds = new(BoundsLeft, BoundsTop, BoundsRight, BoundsBottom);

        using (User32Api.OverrideOperationsForTesting(CreateOperations(
            getMonitorInfo: (IntPtr _, ref MonitorInfoEx monitorInfo) =>
            {
                monitorInfo = new(MonitorInfoSize, bounds, bounds, MonitorInfoFlags.Primary, "test");
                return true;
            })))
        {
        await Assert.That(User32Api.AddDisplayMonitorForTesting(new(MonitorHandleValue), displays)).IsEqualTo(1);
        }

        await Assert.That(User32Api.AddDisplayMonitorForTesting(IntPtr.Zero, new())).IsEqualTo(0);
        await Assert.That(displays.Count).IsEqualTo(1);
        await Assert.That(displays[0].DeviceName).IsEqualTo("test");
    }

    /// <summary>Exercises the explicitly non-owning monitor release path.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SafeMonitorHandle_NonOwningReleasePath_ReturnsTrueAsync()
    {
        using var monitor = new SafeMonitorHandle(new IntPtr(SafeMonitorHandleValue));

        await Assert.That(monitor.ReleaseForTesting()).IsTrue();
    }

    /// <summary>Builds a complete deterministic User32 operation set.</summary>
    /// <param name="getPhysicalCursorPos">The physical cursor operation.</param>
    /// <param name="getCursorPosition">The fallback cursor operation.</param>
    /// <param name="getMonitorInfo">The monitor-information operation.</param>
    /// <param name="desktopOperations">The optional desktop operation overrides.</param>
    /// <param name="systemParametersInfoBuffer">The parameter buffer operation.</param>
    /// <param name="is64BitProcess">The pointer-width predicate.</param>
    /// <returns>A composed operation set.</returns>
    private static User32Operations CreateOperations(
        GetPhysicalCursorPosOperation getPhysicalCursorPos = null,
        Func<NativePoint> getCursorPosition = null,
        GetMonitorInfoOperation getMonitorInfo = null,
        DesktopOperationOverrides desktopOperations = null,
        SystemParametersInfoBufferOperation systemParametersInfoBuffer = null,
        Func<bool> is64BitProcess = null) => new()
        {
            GetPhysicalCursorPos = getPhysicalCursorPos ?? (static (out NativePoint point) =>
            {
                point = default;
                return false;
            }),
            GetCursorPosition = getCursorPosition ?? (static () => default),
            GetMonitorInfo = getMonitorInfo ?? (static (IntPtr _, ref MonitorInfoEx monitorInfo) => false),
            OpenInputDesktop = GetOpenInputDesktop(desktopOperations),
            SetThreadDesktop = GetSetThreadDesktop(desktopOperations),
            CloseDesktop = GetCloseDesktop(desktopOperations),
            GetClassLong = static (_, _) => new IntPtr(PointerWidth32Value),
            GetClassLongPtr = static (_, _) => new IntPtr(PointerWidth64Value),
            GetWindowLong = static (_, _) => new IntPtr(PointerWidth32Value),
            GetWindowLongPtr = static (_, _) => new IntPtr(PointerWidth64Value),
            SetWindowLong = static (_, _, value) => value,
            SetWindowLongPtr = static (_, _, value) => value,
            SetFocus = static _ => new IntPtr(InputHandleValue),
            SetCapture = static _ => new IntPtr(InputHandleValue),
            ReleaseCapture = static () => true,
            SystemParametersInfoBuffer = systemParametersInfoBuffer ?? (static (_, _, _, _) => true),
            LockWorkStation = static () => true,
            Is64BitProcess = is64BitProcess ?? (static () => true),
        };

    /// <summary>Gets an open-input-desktop callback from optional overrides.</summary>
    /// <param name="desktopOperations">The optional desktop callbacks.</param>
    /// <returns>The selected callback.</returns>
    private static Func<uint, bool, DesktopAccessRight, IntPtr> GetOpenInputDesktop(DesktopOperationOverrides desktopOperations) =>
        desktopOperations?.OpenInputDesktop ?? (static (_, _, _) => IntPtr.Zero);

    /// <summary>Gets a set-thread-desktop callback from optional overrides.</summary>
    /// <param name="desktopOperations">The optional desktop callbacks.</param>
    /// <returns>The selected callback.</returns>
    private static Func<IntPtr, bool> GetSetThreadDesktop(DesktopOperationOverrides desktopOperations) =>
        desktopOperations?.SetThreadDesktop ?? (static _ => false);

    /// <summary>Gets a close-desktop callback from optional overrides.</summary>
    /// <param name="desktopOperations">The optional desktop callbacks.</param>
    /// <returns>The selected callback.</returns>
    private static Func<IntPtr, bool> GetCloseDesktop(DesktopOperationOverrides desktopOperations) =>
        desktopOperations?.CloseDesktop ?? (static _ => true);

    /// <summary>Throws a deterministic missing-entry-point exception.</summary>
    /// <param name="point">The unused output point.</param>
    /// <returns>Never returns.</returns>
    private static bool ThrowMissingPhysicalCursorPosition(out NativePoint point)
    {
        point = default;
        throw new EntryPointNotFoundException();
    }

    /// <summary>Writes a null-terminated deterministic UTF-16 response.</summary>
    /// <param name="action">The ignored action.</param>
    /// <param name="parameterValue">The ignored parameter value.</param>
    /// <param name="value">The response buffer.</param>
    /// <param name="behavior">The ignored behavior.</param>
    /// <returns>True.</returns>
    private static unsafe bool WriteNullTerminatedBuffer(
        SystemParametersInfoActions action,
        uint parameterValue,
        IntPtr value,
        SystemParametersInfoBehaviors behavior)
    {
        _ = action;
        _ = parameterValue;
        _ = behavior;
        var pointer = (char*)value;
        pointer[0] = 'a';
        pointer[1] = 'b';
        pointer[2] = '\0';
        return true;
    }

    /// <summary>Writes a full deterministic UTF-16 response.</summary>
    /// <param name="action">The ignored action.</param>
    /// <param name="parameterValue">The ignored parameter value.</param>
    /// <param name="value">The response buffer.</param>
    /// <param name="behavior">The ignored behavior.</param>
    /// <returns>True.</returns>
    private static unsafe bool WriteFullBuffer(
        SystemParametersInfoActions action,
        uint parameterValue,
        IntPtr value,
        SystemParametersInfoBehaviors behavior)
    {
        _ = action;
        _ = parameterValue;
        _ = behavior;
        var pointer = (char*)value;
        pointer[0] = 'a';
        pointer[1] = 'b';
        pointer[2] = 'c';
        pointer[3] = 'd';
        return true;
    }

    /// <summary>Groups desktop operation callbacks for User32 operation construction.</summary>
    /// <param name="openInputDesktop">The open-input-desktop callback.</param>
    /// <param name="setThreadDesktop">The set-thread-desktop callback.</param>
    /// <param name="closeDesktop">The close-desktop callback.</param>
    private sealed class DesktopOperationOverrides(
        Func<uint, bool, DesktopAccessRight, IntPtr> openInputDesktop,
        Func<IntPtr, bool> setThreadDesktop,
        Func<IntPtr, bool> closeDesktop)
    {
        /// <summary>Gets the open-input-desktop callback.</summary>
        internal Func<uint, bool, DesktopAccessRight, IntPtr> OpenInputDesktop { get; } = openInputDesktop;

        /// <summary>Gets the set-thread-desktop callback.</summary>
        internal Func<IntPtr, bool> SetThreadDesktop { get; } = setThreadDesktop;

        /// <summary>Gets the close-desktop callback.</summary>
        internal Func<IntPtr, bool> CloseDesktop { get; } = closeDesktop;
    }
}
