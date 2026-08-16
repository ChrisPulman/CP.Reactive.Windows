// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.TypeConverters;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.TypeConverters;
using Bgr24 = CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats.Bgr24;
using Bgra32 = CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats.Bgra32;
using FileTime = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Exercises deterministic managed branches left by the Core value coverage sweep.</summary>
public sealed class CoverageFinalCoreValueResidualTests
{
    /// <summary>Specifies the bitmap pixel depth used by the coverage fixture.</summary>
    private const int BitmapBitsPerPixel = 32;

    /// <summary>Specifies the bitmap height used by the coverage fixture.</summary>
    private const int BitmapHeight = 6;

    /// <summary>Specifies the bitmap width used by the coverage fixture.</summary>
    private const int BitmapWidth = 8;

    /// <summary>Specifies an alternate blue component for the BGR equality comparison.</summary>
    private const int BgrAlternateBlue = 4;

    /// <summary>Specifies an alternate green component for the BGR equality comparison.</summary>
    private const int BgrAlternateGreen = 5;

    /// <summary>Specifies an alternate red component for the BGR equality comparison.</summary>
    private const int BgrAlternateRed = 6;

    /// <summary>Specifies the baseline blue component for the BGR equality comparison.</summary>
    private const int BgrBlue = 3;

    /// <summary>Specifies the baseline green component for the BGR equality comparison.</summary>
    private const int BgrGreen = 2;

    /// <summary>Specifies the baseline red component for the BGR equality comparison.</summary>
    private const int BgrRed = 1;

    /// <summary>Specifies an alternate alpha component for the BGRA equality comparison.</summary>
    private const int BgraAlternateAlpha = 5;

    /// <summary>Specifies an alternate blue component for the BGRA equality comparison.</summary>
    private const int BgraAlternateBlue = 6;

    /// <summary>Specifies an alternate green component for the BGRA equality comparison.</summary>
    private const int BgraAlternateGreen = 7;

    /// <summary>Specifies an alternate red component for the BGRA equality comparison.</summary>
    private const int BgraAlternateRed = 8;

    /// <summary>Specifies the baseline alpha component for the BGRA equality comparison.</summary>
    private const int BgraAlpha = 4;

    /// <summary>Specifies the baseline blue component for the BGRA equality comparison.</summary>
    private const int BgraBlue = 3;

    /// <summary>Specifies the baseline green component for the BGRA equality comparison.</summary>
    private const int BgraGreen = 2;

    /// <summary>Specifies the baseline red component for the BGRA equality comparison.</summary>
    private const int BgraRed = 1;

    /// <summary>Specifies the expected X coordinate parsed by the point converter.</summary>
    private const int ConvertedPointX = 7;

    /// <summary>Specifies the expected Y coordinate parsed by the point converter.</summary>
    private const int ConvertedPointY = 8;

    /// <summary>Specifies the non-zero cursor handle used for argument-validation coverage.</summary>
    private const int CursorHandleValue = 42;

    /// <summary>Specifies the height of the rectangle docked beside the baseline rectangle.</summary>
    private const int DockedHeight = 4;

    /// <summary>Specifies the left coordinate of the rectangle docked to the baseline rectangle's right side.</summary>
    private const int DockedLeft = -11;

    /// <summary>Specifies the right coordinate of the rectangle docked to the baseline rectangle's left side.</summary>
    private const int DockedRight = 11;

    /// <summary>Specifies the top coordinate of the docked rectangles.</summary>
    private const int DockedTop = 2;

    /// <summary>Specifies the width of the baseline and docked rectangles.</summary>
    private const int DockedWidth = 10;

    /// <summary>Specifies a future Windows major version for negative predicate coverage.</summary>
    private const int FutureWindowsMajorVersion = 9;

    /// <summary>Specifies an invalid title-bar information index for argument-validation coverage.</summary>
    private const int InvalidTitleBarInfoIndexValue = 99;

    /// <summary>Specifies the baseline process identifier for restart-manager equality coverage.</summary>
    private const int ProcessId = 3;

    /// <summary>Specifies the baseline process session identifier for restart-manager equality coverage.</summary>
    private const int ProcessSessionId = 5;

    /// <summary>Specifies a different process identifier for restart-manager inequality coverage.</summary>
    private const int ReplacementProcessId = 4;

    /// <summary>Specifies the height of the rectangle that overlaps the baseline rectangle.</summary>
    private const int OverlapHeight = 20;

    /// <summary>Specifies the X coordinate of the rectangle that overlaps the baseline rectangle.</summary>
    private const int OverlapX = 5;

    /// <summary>Specifies the Y coordinate of the rectangle that overlaps the baseline rectangle.</summary>
    private const int OverlapY = -5;

    /// <summary>Specifies the Windows build number used by version-predicate coverage.</summary>
    private const int TestBuildNumber = 21_999;

    /// <summary>Specifies the high FILETIME value used by restart-manager equality coverage.</summary>
    private const int TestFileTimeHighValue = 2;

    /// <summary>Specifies the low FILETIME value used by restart-manager equality coverage.</summary>
    private const int TestFileTimeLowValue = 1;

    /// <summary>Specifies the Windows major version used by positive predicate coverage.</summary>
    private const int TestWindowsMajorVersion = 10;

    /// <summary>Specifies the Windows 7 major version for version-predicate coverage.</summary>
    private const int WindowsSevenMajorVersion = 6;

    /// <summary>Specifies the Windows 7 minor version for version-predicate coverage.</summary>
    private const int WindowsSevenMinorVersion = 1;

    /// <summary>Specifies the Windows XP major version for version-predicate coverage.</summary>
    private const int WindowsXpMajorVersion = 5;

    /// <summary>Specifies the Windows XP minor version that satisfies the XP predicate.</summary>
    private const int WindowsXpMinorVersion = 1;

    /// <summary>Specifies the Windows XP minor version for version-predicate coverage.</summary>
    private const int XpMinorVersion = 0;

    /// <summary>Specifies the first Windows XP minor version recognized by the predicate.</summary>
    private const int WindowsXpSupportedMinorVersion = 1;

    /// <summary>Specifies the baseline restart-manager application name.</summary>
    private const string ApplicationName = "app";

    /// <summary>Specifies a different restart-manager application name for inequality coverage.</summary>
    private const string ChangedApplicationName = "other";

    /// <summary>Specifies the restart-manager service name.</summary>
    private const string ServiceName = "service";

    /// <summary>Exercises all outcomes of the version predicates with an async-local version source.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsVersion_ThresholdAndShortCircuitOutcomes_AreDeterministicAsync()
    {
        using (WindowsVersion.OverrideVersionProviderForTesting(static () => new(FutureWindowsMajorVersion, XpMinorVersion, TestBuildNumber)))
        {
            bool isWindows11OrLater = WindowsVersion.IsWindows11OrLater;
            bool isWindowsVista = WindowsVersion.IsWindowsVista;
            bool isWindowsXp = WindowsVersion.IsWindowsXp;

            await Assert.That(isWindows11OrLater).IsFalse();
            await Assert.That(isWindowsVista).IsFalse();
            await Assert.That(isWindowsXp).IsFalse();
        }

        using (WindowsVersion.OverrideVersionProviderForTesting(static () => new(TestWindowsMajorVersion, XpMinorVersion, TestBuildNumber)))
        {
            bool isWindows11OrLater = WindowsVersion.IsWindows11OrLater;
            bool isWindows7OrLater = WindowsVersion.IsWindows7OrLater;
            bool isWindowsXpOrLater = WindowsVersion.IsWindowsXpOrLater;

            await Assert.That(isWindows11OrLater).IsFalse();
            await Assert.That(isWindows7OrLater).IsTrue();
            await Assert.That(isWindowsXpOrLater).IsTrue();
        }

        using (WindowsVersion.OverrideVersionProviderForTesting(static () => new(WindowsSevenMajorVersion, WindowsSevenMinorVersion)))
        {
            bool isWindows7OrLater = WindowsVersion.IsWindows7OrLater;
            bool isWindowsVista = WindowsVersion.IsWindowsVista;
            bool isWindowsXp = WindowsVersion.IsWindowsXp;

            await Assert.That(isWindows7OrLater).IsTrue();
            await Assert.That(isWindowsVista).IsFalse();
            await Assert.That(isWindowsXp).IsFalse();
        }

        using (WindowsVersion.OverrideVersionProviderForTesting(static () => new(WindowsXpMajorVersion, WindowsXpSupportedMinorVersion)))
        {
            bool isWindowsXp = WindowsVersion.IsWindowsXp;
            bool isWindowsXpOrLater = WindowsVersion.IsWindowsXpOrLater;

            await Assert.That(isWindowsXp).IsTrue();
            await Assert.That(isWindowsXpOrLater).IsTrue();
        }
    }

    /// <summary>Exercises valid and invalid managed type-converter paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TypeConverters_ValidAndInvalidTextPaths_ReturnExpectedValuesAsync()
    {
        NativePointTypeConverter pointConverter = new();
        WindowPlacementTypeConverter placementConverter = new();

        NativePoint convertedPoint = new(ConvertedPointX, ConvertedPointY);
        await Assert.That(pointConverter.ConvertFromInvariantString("7,8")).IsEqualTo(convertedPoint);
        await Assert.That(() => pointConverter.ConvertFromInvariantString("x,8")).Throws<NotSupportedException>();
        await Assert.That(() => pointConverter.ConvertFromInvariantString("7,x")).Throws<NotSupportedException>();

        var placement = (WindowPlacement)placementConverter.ConvertFromInvariantString("Normal|1,2|3,4|5,6,7,8");
        await Assert.That(placement.ShowCmd).IsEqualTo(ShowWindowCommands.Normal);
        await Assert.That(placement.MinPosition).IsEqualTo(new(BgrRed, BgrGreen));
        await Assert.That(placement.MaxPosition).IsEqualTo(new(BgrBlue, BgrAlternateBlue));
        await Assert.That(placement.NormalPosition).IsEqualTo(new(BgrAlternateGreen, BgrAlternateRed, BgraAlternateGreen, BgraAlternateRed));
        await Assert.That(placementConverter.ConvertToInvariantString(placement)).IsEqualTo("Normal|1,2|3,4|5,6,7,8");
        await Assert.That(() => placementConverter.ConvertFromInvariantString("Normal|1,2|3,4")).Throws<NotSupportedException>();
        await Assert.That(placementConverter.CanConvertTo(null, typeof(DateTime))).IsFalse();
    }

    /// <summary>Exercises value equality paths that distinguish each logical component.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ValueEquality_ComponentDifferences_AreObservedAsync()
    {
        Bgr24 bgr = new(BgrRed, BgrGreen, BgrBlue);
        await Assert.That(bgr.Equals(new(BgrRed, BgrGreen, BgrAlternateBlue))).IsFalse();
        await Assert.That(bgr.Equals(new(BgrRed, BgrAlternateGreen, BgrBlue))).IsFalse();
        await Assert.That(bgr.Equals(new(BgrAlternateRed, BgrGreen, BgrBlue))).IsFalse();

        Bgra32 bgra = new(BgraRed, BgraGreen, BgraBlue, BgraAlpha);
        await Assert.That(bgra.Equals(new(BgraRed, BgraGreen, BgraBlue, BgraAlternateAlpha))).IsFalse();
        await Assert.That(bgra.Equals(new(BgraRed, BgraGreen, BgraAlternateBlue, BgraAlpha))).IsFalse();
        await Assert.That(bgra.Equals(new(BgraRed, BgraAlternateGreen, BgraBlue, BgraAlpha))).IsFalse();
        await Assert.That(bgra.Equals(new(BgraAlternateRed, BgraGreen, BgraBlue, BgraAlpha))).IsFalse();

        NativeRect first = new(XpMinorVersion, XpMinorVersion, DockedWidth, DockedWidth);
        NativeRect crossing = new(OverlapX, OverlapY, DockedWidth, OverlapHeight);
        NativeRect dockedOnRight = new(first.Right + 1, DockedTop, DockedWidth, DockedHeight);
        NativeRect dockedOnLeft = new(first.Left - DockedWidth - 1, DockedTop, DockedWidth, DockedHeight);
        await Assert.That(first.HasOverlap(crossing)).IsTrue();
        await Assert.That(first.IsDockedToLeftOf(dockedOnRight)).IsTrue();
        await Assert.That(first.IsDockedToRightOf(dockedOnLeft)).IsTrue();
    }

    /// <summary>Exercises Bitmap V5 equality groups and safe-handle argument validation without native mutation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapAndSafeHandle_ManagedBranches_ReturnExpectedValuesAsync()
    {
        var value = BitmapV5Header.Create(BitmapWidth, BitmapHeight, BitmapBitsPerPixel);
        var bitmapInfoDifference = value;
        bitmapInfoDifference.SizeImage++;
        var maskDifference = value;
        maskDifference.RedMask++;
        var colorSpaceDifference = value;
        colorSpaceDifference.GammaBlue++;
        var profileDifference = value;
        profileDifference.ProfileSize++;

        await Assert.That(value.Equals(bitmapInfoDifference)).IsFalse();
        await Assert.That(value.Equals(maskDifference)).IsFalse();
        await Assert.That(value.Equals(colorSpaceDifference)).IsFalse();
        await Assert.That(value.Equals(profileDifference)).IsFalse();

        IntPtr cursorHandle = new(CursorHandleValue);
        using SafeCursorReferenceHandle handle = new(cursorHandle);
        await Assert.That(() => handle.UseNativeHandle<int>(null)).Throws<ArgumentNullException>();
        await Assert.That(handle.UseNativeHandle(static nativeHandle => nativeHandle)).IsEqualTo(cursorHandle);
    }

    /// <summary>Exercises user-interface and restart-manager value comparisons without invoking Windows APIs.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeStructs_EqualityAndSelectionBranches_AreDeterministicAsync()
    {
        var animationEnabled = AnimationInfo.Create();
        var animationDisabled = AnimationInfo.Create(enableAnimations: false);
        await Assert.That(animationEnabled.Equals(AnimationInfo.Create())).IsTrue();
        await Assert.That(animationEnabled.Equals(animationDisabled)).IsFalse();
        await Assert.That(animationEnabled.Equals(new())).IsFalse();

        var scrollBar = ScrollBarInfo.Create();
        await Assert.That(scrollBar.Equals(ScrollBarInfo.Create())).IsTrue();
        await Assert.That(scrollBar.Equals(default(ScrollBarInfo))).IsFalse();
        await Assert.That(scrollBar.Equals(new())).IsFalse();

        var titleBar = TitleBarInfoEx.Create();
        await Assert.That(titleBar.Equals(TitleBarInfoEx.Create())).IsTrue();
        await Assert.That(titleBar.Equals(default(TitleBarInfoEx))).IsFalse();
        await Assert.That(titleBar.ElementState(TitleBarInfoIndexes.TitleBar)).IsEqualTo(ObjectStates.None);
        await Assert.That(titleBar.ElementBounds(TitleBarInfoIndexes.CloseButton)).IsEqualTo(NativeRect.Empty);
        const TitleBarInfoIndexes invalidIndex = (TitleBarInfoIndexes)InvalidTitleBarInfoIndexValue;
        await Assert.That(() => titleBar.ElementState(invalidIndex)).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => titleBar.ElementBounds(invalidIndex)).Throws<ArgumentOutOfRangeException>();

        FileTime time = new() { dwLowDateTime = TestFileTimeLowValue, dwHighDateTime = TestFileTimeHighValue };
        var process = new RmUniqueProcess(ProcessId, time);
        var sameProcess = new RmUniqueProcess(ProcessId, time);
        var changedProcess = new RmUniqueProcess(ReplacementProcessId, time);
        await Assert.That(process.Equals(sameProcess)).IsTrue();
        await Assert.That(process.Equals(changedProcess)).IsFalse();

        var processInfo = new RmProcessInfo(process, ApplicationName, ServiceName, RmAppType.RmMainWindow, RmAppStatus.RmStatusRunning, ProcessSessionId, true);
        var sameProcessInfo = new RmProcessInfo(process, ApplicationName, ServiceName, RmAppType.RmMainWindow, RmAppStatus.RmStatusRunning, ProcessSessionId, true);
        var changedProcessInfo = new RmProcessInfo(process, ChangedApplicationName, ServiceName, RmAppType.RmMainWindow, RmAppStatus.RmStatusRunning, ProcessSessionId, true);
        await Assert.That(processInfo.Equals(sameProcessInfo)).IsTrue();
        await Assert.That(processInfo.Equals(changedProcessInfo)).IsFalse();
        await Assert.That(processInfo.Equals(new())).IsFalse();
    }
}
