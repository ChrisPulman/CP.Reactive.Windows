// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides deterministic coverage for Core, Kernel32, and User32 value surfaces.</summary>
public sealed class CoverageWave3CoreKernelUser32Tests
{
    /// <summary>Verifies representative Windows system-color index values.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SystemColorIndex_StaticValues_MapToNativeIndexesAsync()
    {
        const int ScrollBarColorIndex = 0;
        const int InfoBackgroundColorIndex = 24;
        const int HotlightColorIndex = 26;
        const int MenuBarColorIndex = 30;

        await Assert.That(SystemColorIndex.ScrollBar.Value).IsEqualTo(ScrollBarColorIndex);
        await Assert.That(SystemColorIndex.InfoBackground.Value).IsEqualTo(InfoBackgroundColorIndex);
        await Assert.That(SystemColorIndex.Hotlight.Value).IsEqualTo(HotlightColorIndex);
        await Assert.That(SystemColorIndex.MenuBar.Value).IsEqualTo(MenuBarColorIndex);
    }

    /// <summary>Exercises title-bar element accessors over default native slots.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TitleBarInfoEx_ElementAccessors_ExposeDefaultSlotsAsync()
    {
        const TitleBarInfoIndexes InvalidTitleBarInfoIndex = (TitleBarInfoIndexes)99;

        var titleBar = TitleBarInfoEx.Create();
        var copy = TitleBarInfoEx.Create();

        await Assert.That(titleBar.Bounds).IsEqualTo(NativeRect.Empty);
        await Assert.That(titleBar.ElementState(TitleBarInfoIndexes.TitleBar)).IsEqualTo(ObjectStates.None);
        await Assert.That(titleBar.ElementState(TitleBarInfoIndexes.Reserved)).IsEqualTo(ObjectStates.None);
        await Assert.That(titleBar.ElementState(TitleBarInfoIndexes.MinimizeButton)).IsEqualTo(ObjectStates.None);
        await Assert.That(titleBar.ElementState(TitleBarInfoIndexes.MaximizeButton)).IsEqualTo(ObjectStates.None);
        await Assert.That(titleBar.ElementState(TitleBarInfoIndexes.HelpButton)).IsEqualTo(ObjectStates.None);
        await Assert.That(titleBar.ElementState(TitleBarInfoIndexes.CloseButton)).IsEqualTo(ObjectStates.None);
        await Assert.That(titleBar.ElementBounds(TitleBarInfoIndexes.TitleBar)).IsEqualTo(NativeRect.Empty);
        await Assert.That(titleBar.ElementBounds(TitleBarInfoIndexes.Reserved)).IsEqualTo(NativeRect.Empty);
        await Assert.That(titleBar.ElementBounds(TitleBarInfoIndexes.MinimizeButton)).IsEqualTo(NativeRect.Empty);
        await Assert.That(titleBar.ElementBounds(TitleBarInfoIndexes.MaximizeButton)).IsEqualTo(NativeRect.Empty);
        await Assert.That(titleBar.ElementBounds(TitleBarInfoIndexes.HelpButton)).IsEqualTo(NativeRect.Empty);
        await Assert.That(titleBar.ElementBounds(TitleBarInfoIndexes.CloseButton)).IsEqualTo(NativeRect.Empty);
        await Assert.That(titleBar.Equals(copy)).IsTrue();
        await Assert.That(titleBar.Equals((object)copy)).IsTrue();
        await Assert.That(titleBar == copy).IsTrue();
        await Assert.That(titleBar != copy).IsFalse();
        await Assert.That(static () => TitleBarInfoEx.Create().ElementState(InvalidTitleBarInfoIndex)).Throws<ArgumentOutOfRangeException>();
        await Assert.That(static () => TitleBarInfoEx.Create().ElementBounds(InvalidTitleBarInfoIndex)).Throws<ArgumentOutOfRangeException>();
    }

    /// <summary>Exercises scroll and animation structs without native calls.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ScrollAndAnimationStructs_RoundTripStateAndEqualityAsync()
    {
        const int Position = 123;
        const int TrackingPosition = 456;
        const int ScrollStateCount = 6;

        var scrollBar = ScrollBarInfo.Create();
        var scrollInfo = ScrollInfo.Create(ScrollInfoMask.All);
        scrollInfo.Position = Position;
        scrollInfo.TrackingPosition = TrackingPosition;
        var enabled = AnimationInfo.Create();
        var enabledCopy = AnimationInfo.Create(true);
        var disabled = AnimationInfo.Create(false);

        await Assert.That(scrollBar.Bounds).IsEqualTo(default(NativeRect));
        await Assert.That(scrollBar.ThumbSize).IsEqualTo(0);
        await Assert.That(scrollBar.ThumbBottom).IsEqualTo(0);
        await Assert.That(scrollBar.ThumbTop).IsEqualTo(0);
        await Assert.That(scrollBar.States.Length).IsEqualTo(ScrollStateCount);
        await Assert.That(scrollBar.States[0]).IsEqualTo(ObjectStates.None);
        await Assert.That(scrollBar.ToString()).Contains("ThumbSize = 0");
        await Assert.That(scrollBar.Equals((object)ScrollBarInfo.Create())).IsTrue();
        await Assert.That(scrollInfo.Minimum).IsEqualTo(0);
        await Assert.That(scrollInfo.Maximum).IsEqualTo(0);
        await Assert.That(scrollInfo.PageSize).IsEqualTo(0U);
        await Assert.That(scrollInfo.Position).IsEqualTo(Position);
        await Assert.That(scrollInfo.TrackingPosition).IsEqualTo(TrackingPosition);
        await Assert.That(scrollInfo.ToString()).Contains("Position = 123");
        await Assert.That(scrollInfo.Equals((object)scrollInfo)).IsTrue();
        await Assert.That(enabled == enabledCopy).IsTrue();
        await Assert.That(enabled != disabled).IsTrue();
        await Assert.That(enabled.GetHashCode()).IsEqualTo(enabledCopy.GetHashCode());
    }

    /// <summary>Exercises window-placement, window-info, cursor-info, and safe cursor reference helpers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task User32WindowAndCursorValues_RoundTripManagedFieldsAsync()
    {
        const int CursorHandleValue = 42;
        const int FirstCoordinate = 1;
        const int SecondCoordinate = 2;
        const int ThirdCoordinate = 3;
        const int FourthCoordinate = 4;
        const int FifthCoordinate = 5;
        const int SixthCoordinate = 6;
        const int SeventhCoordinate = 7;
        const int EighthCoordinate = 8;
        const int NinthCoordinate = 9;
        const int TenthCoordinate = 10;
        const int EleventhCoordinate = 11;
        const int TwelfthCoordinate = 12;

        NativePoint minPosition = new(FirstCoordinate, SecondCoordinate);
        NativePoint maxPosition = new(ThirdCoordinate, FourthCoordinate);
        NativeRect normalPosition = new(FifthCoordinate, SixthCoordinate, SeventhCoordinate, EighthCoordinate);
        NativeRect windowBounds = new(NinthCoordinate, TenthCoordinate, EleventhCoordinate, TwelfthCoordinate);
        NativeRect clientBounds = new(FirstCoordinate, ThirdCoordinate, FifthCoordinate, SeventhCoordinate);
        var placement = WindowPlacement.Create();
        placement.Flags = WindowPlacementFlags.AsyncWindowPlacement;
        placement.ShowCmd = ShowWindowCommands.Maximize;
        placement.MinPosition = minPosition;
        placement.MaxPosition = maxPosition;
        placement.NormalPosition = normalPosition;
        var window = WindowInfo.Create();
        window.Bounds = windowBounds;
        window.ClientBounds = clientBounds;
        var cursor = CursorInfo.Create();
        using var cursorHandle = new SafeCursorReferenceHandle((IntPtr)CursorHandleValue);
        using var emptyCursorHandle = new SafeCursorReferenceHandle();

        await Assert.That(placement.Flags).IsEqualTo(WindowPlacementFlags.AsyncWindowPlacement);
        await Assert.That(placement.ShowCmd).IsEqualTo(ShowWindowCommands.Maximize);
        await Assert.That(placement.MinPosition).IsEqualTo(minPosition);
        await Assert.That(placement.MaxPosition).IsEqualTo(maxPosition);
        await Assert.That(placement.NormalPosition).IsEqualTo(normalPosition);
        await Assert.That(placement.ToString()).Contains(nameof(ShowWindowCommands.Maximize));
        await Assert.That(placement.Equals((object)placement)).IsTrue();
        await Assert.That(placement != WindowPlacement.Create()).IsTrue();
        await Assert.That(window.Bounds).IsEqualTo(windowBounds);
        await Assert.That(window.ClientBounds).IsEqualTo(clientBounds);
        await Assert.That(window.IsActive).IsFalse();
        await Assert.That(window.BorderSize).IsEqualTo(default(NativeSize));
        await Assert.That(window.ToString()).Contains(nameof(WindowInfo.ClientBounds));
        await Assert.That(window.Equals((object)window)).IsTrue();
        await Assert.That(window != WindowInfo.Create()).IsTrue();
        await Assert.That(cursor.Flags).IsEqualTo(default(CursorInfoFlags));
        await Assert.That(cursor.Location).IsEqualTo(default(NativePoint));
        await Assert.That(cursor.IsShowing).IsFalse();
        using var createdCursorHandle = cursor.CursorHandle;
        await Assert.That(createdCursorHandle.IsInvalid).IsTrue();
        await Assert.That(cursor.Equals((object)cursor)).IsTrue();
        await Assert.That(cursorHandle.UseNativeHandle(static handle => handle)).IsEqualTo((IntPtr)CursorHandleValue);
        await Assert.That(() => emptyCursorHandle.UseNativeHandle<int>(null!)).Throws<ArgumentNullException>();
    }

    /// <summary>Exercises OS version value equality and hash code components.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task OsVersionInfoEx_Equality_UsesEveryPublicComponentAsync()
    {
        const int MajorVersion = 10;
        const int MinorVersion = 1;
        const int BuildNumber = 22_631;
        const int PlatformId = 2;
        const short ServicePackMajor = 1;
        const short ServicePackMinor = 2;

        var version = new OsVersionInfoEx
        {
            MajorVersion = MajorVersion,
            MinorVersion = MinorVersion,
            BuildNumber = BuildNumber,
            PlatformId = PlatformId,
            ServicePackVersion = "SP1",
            ServicePackMajor = ServicePackMajor,
            ServicePackMinor = ServicePackMinor,
            SuiteMask = WindowsSuites.Enterprise,
            ProductType = WindowsProductTypes.VER_NT_WORKSTATION,
        };
        var same = version;
        var different = new OsVersionInfoEx { ServicePackVersion = "SP2" };

        await Assert.That(OsVersionInfoEx.Create().ServicePackVersion).IsEqualTo(string.Empty);
        await Assert.That(version.Equals(same)).IsTrue();
        await Assert.That(version.Equals((object)same)).IsTrue();
        await Assert.That(version == same).IsTrue();
        await Assert.That(version != different).IsTrue();
        await Assert.That(version.Equals("not a version")).IsFalse();
        await Assert.That(version.GetHashCode()).IsEqualTo(same.GetHashCode());
    }
}
