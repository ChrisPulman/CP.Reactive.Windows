// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.Security.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.Shell.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Shell.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides deterministic coverage for Core native value structures.</summary>
public sealed class CoverageReleaseCoreStructTests
{
    /// <summary>Defines the native monitor-information structure size.</summary>
    private const int MonitorInfoSize = 104;

    /// <summary>Defines a monitor device name.</summary>
    private const string DeviceName = "DISPLAY1";

    /// <summary>Defines an alternate monitor device name.</summary>
    private const string AlternateDeviceName = "DISPLAY2";

    /// <summary>Defines a shell display name.</summary>
    private const string DisplayName = "display";

    /// <summary>Defines a shell type name.</summary>
    private const string TypeName = "type";

    /// <summary>Verifies appbar native conversion, equality and every differing value member.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AppBarData_NativeValueAndEqualityMembersAsync()
    {
        var data = AppBarData.Create();
        data.SetWindowHandle(new(One));
        data.CallbackMessageIdentifier = Two;
        data.AppBarEdge = AppBarEdges.Left;
        data.Bounds = new(Three, Four, Five, Six);
        data.AutoHide = true;
        var emptyData = AppBarData.Create();

        await Assert.That(emptyData.AutoHide).IsFalse();
        emptyData.AutoHide = false;
        await Assert.That(emptyData.AutoHide).IsFalse();

        var native = data.ToNative();
        var same = new AppBarData.NativeAppBarData(new(One), Two, AppBarEdges.Left, new(Three, Four, Five, Six), One);
        var differentSize = new AppBarData.NativeAppBarData(new(Two), Two, AppBarEdges.Left, new(Three, Four, Five, Six), One);
        var differentCallback = new AppBarData.NativeAppBarData(new(One), Seven, AppBarEdges.Left, new(Three, Four, Five, Six), One);
        var differentEdge = new AppBarData.NativeAppBarData(new(One), Two, AppBarEdges.Right, new(Three, Four, Five, Six), One);
        var differentBounds = new AppBarData.NativeAppBarData(new(One), Two, AppBarEdges.Left, new(Seven, Four, Five, Six), One);
        var differentParameter = new AppBarData.NativeAppBarData(new(One), Two, AppBarEdges.Left, new(Three, Four, Five, Six), Zero);

        await Assert.That(native).IsEqualTo(same);
        await Assert.That(native.Equals(new object())).IsFalse();
        await Assert.That(native.Equals(differentSize)).IsFalse();
        await Assert.That(native.Equals(differentCallback)).IsFalse();
        await Assert.That(native.Equals(differentEdge)).IsFalse();
        await Assert.That(native.Equals(differentBounds)).IsFalse();
        await Assert.That(native.Equals(differentParameter)).IsFalse();
        await Assert.That(native.Equals(WithNativeSize(in native, Zero))).IsFalse();
        await Assert.That(native.GetHashCode()).IsEqualTo(same.GetHashCode());

        var applied = AppBarData.Create();
        applied.Apply(native);
        await Assert.That(applied).IsEqualTo(data);
        applied.State = AppBarStates.AllwaysOnTop;
        await Assert.That(applied.AutoHide).IsTrue();
        await Assert.That(applied.State).IsEqualTo(AppBarStates.AllwaysOnTop);
    }

    /// <summary>Verifies monitor information construction, null-name normalization, and equality branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task MonitorInfoEx_PropertiesAndEqualityBranchesAsync()
    {
        var created = MonitorInfoEx.Create();
        var monitor = new NativeRect(One, Two, Three, Four);
        var workArea = new NativeRect(Five, Six, Seven, Eight);
        var value = new MonitorInfoEx(MonitorInfoSize, monitor, workArea, MonitorInfoFlags.Primary, DeviceName);
        var same = new MonitorInfoEx(MonitorInfoSize, monitor, workArea, MonitorInfoFlags.Primary, DeviceName);
        var nullName = new MonitorInfoEx(MonitorInfoSize, monitor, workArea, MonitorInfoFlags.Primary, null);
        var differentSize = new MonitorInfoEx(MonitorInfoSize + One, monitor, workArea, MonitorInfoFlags.Primary, DeviceName);
        var differentMonitor = new MonitorInfoEx(MonitorInfoSize, new(Seven, Two, Three, Four), workArea, MonitorInfoFlags.Primary, DeviceName);
        var differentWorkArea = new MonitorInfoEx(MonitorInfoSize, monitor, new(Seven, Six, Seven, Eight), MonitorInfoFlags.Primary, DeviceName);
        var differentFlags = new MonitorInfoEx(MonitorInfoSize, monitor, workArea, MonitorInfoFlags.None, DeviceName);
        var differentName = new MonitorInfoEx(MonitorInfoSize, monitor, workArea, MonitorInfoFlags.Primary, AlternateDeviceName);

        await Assert.That(created.Size).IsEqualTo(MonitorInfoSize);
        await Assert.That(created.DeviceName).IsEqualTo(string.Empty);
        await Assert.That(default(MonitorInfoEx).DeviceName).IsEqualTo(string.Empty);
        await Assert.That(value.Size).IsEqualTo(MonitorInfoSize);
        await Assert.That(value.Monitor).IsEqualTo(monitor);
        await Assert.That(value.WorkArea).IsEqualTo(workArea);
        await Assert.That(value.Flags).IsEqualTo(MonitorInfoFlags.Primary);
        await Assert.That(value.DeviceName).IsEqualTo(DeviceName);
        await Assert.That(nullName.DeviceName).IsEqualTo(string.Empty);
        await Assert.That(value).IsEqualTo(same);
        await Assert.That(value == same).IsTrue();
        await Assert.That(value != created).IsTrue();
        await Assert.That(value.Equals(differentSize)).IsFalse();
        await Assert.That(value.Equals(differentMonitor)).IsFalse();
        await Assert.That(value.Equals(differentWorkArea)).IsFalse();
        await Assert.That(value.Equals(differentFlags)).IsFalse();
        await Assert.That(value.Equals(differentName)).IsFalse();
        await Assert.That(value.Equals((object)value)).IsTrue();
        await Assert.That(value.Equals(new object())).IsFalse();
        await Assert.That(value.GetHashCode()).IsEqualTo(typeof(MonitorInfoEx).GetHashCode());
    }

    /// <summary>Verifies scroll information defaults, mutable positions and equality paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ScrollInfo_PropertiesFormattingAndEqualityAsync()
    {
        var value = ScrollInfo.Create(ScrollInfoMask.All);
        var same = ScrollInfo.Create(ScrollInfoMask.All);
        var differentMask = ScrollInfo.Create(ScrollInfoMask.Range);

        value.Position = Nine;
        value.TrackingPosition = Ten;

        await Assert.That(value.Minimum).IsEqualTo(0);
        await Assert.That(value.Maximum).IsEqualTo(0);
        await Assert.That(value.PageSize).IsEqualTo(0U);
        await Assert.That(value.Position).IsEqualTo(Nine);
        await Assert.That(value.TrackingPosition).IsEqualTo(Ten);
        await Assert.That(value.ToString()).Contains($"Position = {Nine}");
        await Assert.That(value != same).IsTrue();
        await Assert.That(same == ScrollInfo.Create(ScrollInfoMask.All)).IsTrue();
        await Assert.That(same.Equals(differentMask)).IsFalse();
        await Assert.That(same.Equals(default)).IsFalse();
        await Assert.That(same.Equals((object)same)).IsTrue();
        await Assert.That(same.Equals(new object())).IsFalse();
        await Assert.That(same.GetHashCode()).IsEqualTo(0);
    }

    /// <summary>Verifies default GDI bitmap value semantics without allocating native resources.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task GdiBitmap_DefaultValueSemanticsAsync()
    {
        var value = new GdiBitmap();
        var same = default(GdiBitmap);

        await Assert.That(value.Width).IsEqualTo(0);
        await Assert.That(value.Height).IsEqualTo(0);
        await Assert.That(value).IsEqualTo(same);
        await Assert.That(value == same).IsTrue();
        await Assert.That(value != same).IsFalse();
        await Assert.That(value.Equals((object)same)).IsTrue();
        await Assert.That(value.Equals(new object())).IsFalse();
        await Assert.That(value.GetHashCode()).IsEqualTo(typeof(GdiBitmap).GetHashCode());
    }

    /// <summary>Verifies SID value equality and requested-attribute branches without resolving a native SID.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SidAndAttributes_ValueAndAttributeBranchesAsync()
    {
        var value = new SidAndAttributes(new(One), Three);
        var same = new SidAndAttributes(new(One), Three);
        var different = new SidAndAttributes(new(Two), One);
        var differentAttributes = new SidAndAttributes(new(One), Two);

        await Assert.That(value).IsEqualTo(same);
        await Assert.That(value == same).IsTrue();
        await Assert.That(value != different).IsTrue();
        await Assert.That(value.Equals(differentAttributes)).IsFalse();
        await Assert.That(value.Equals((object)value)).IsTrue();
        await Assert.That(value.Equals(new object())).IsFalse();
        await Assert.That(value.GetHashCode()).IsEqualTo(same.GetHashCode());
        await Assert.That(value.HasAttributes(0)).IsTrue();
        await Assert.That(value.HasAttributes(One)).IsTrue();
        await Assert.That(value.HasAttributes(Two)).IsTrue();
        await Assert.That(value.HasAttributes(Four)).IsFalse();
    }

    /// <summary>Verifies shell file information equality and its null-safe default value behavior.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ShellFileInfo_ValueEqualityAndHashCodeAsync()
    {
        var value = new ShellFileInfo(IntPtr.Zero, One, Two, DisplayName, TypeName);
        var same = new ShellFileInfo(IntPtr.Zero, One, Two, DisplayName, TypeName);
        var different = new ShellFileInfo(IntPtr.Zero, Three, Two, DisplayName, TypeName);

        await Assert.That(value).IsEqualTo(same);
        await Assert.That(value == same).IsTrue();
        await Assert.That(value != different).IsTrue();
        await Assert.That(value.Equals(new object())).IsFalse();
        await Assert.That(value.GetHashCode()).IsEqualTo(same.GetHashCode());
        await Assert.That(value.IconIndex).IsEqualTo(One);
        await Assert.That(value.Attributes).IsEqualTo((uint)Two);
        await Assert.That(value.DisplayName).IsEqualTo(DisplayName);
        await Assert.That(value.TypeName).IsEqualTo(TypeName);
    }

    /// <summary>Creates an app-bar native value with a substituted structure size.</summary>
    /// <param name="value">The source app-bar native value.</param>
    /// <param name="size">The replacement structure size.</param>
    /// <returns>The app-bar native value with the replacement size.</returns>
    private static AppBarData.NativeAppBarData WithNativeSize(in AppBarData.NativeAppBarData value, int size)
    {
        var nativeSize = Marshal.SizeOf<AppBarData.NativeAppBarData>();
        var buffer = Marshal.AllocHGlobal(nativeSize);
        try
        {
            Marshal.StructureToPtr(value, buffer, false);
            Marshal.WriteInt32(buffer, size);
            return Marshal.PtrToStructure<AppBarData.NativeAppBarData>(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
