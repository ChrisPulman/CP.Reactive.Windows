// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.TypeConverters;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides deterministic native-layout coverage for user-interface value structures.</summary>
public sealed class CoverageReleaseCoreUserInterfaceValueTests
{
    /// <summary>Verifies all cursor-value branches without reading or changing the desktop cursor.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CursorInfo_NativeValueBranchesAsync()
    {
        var value = CreateCursorInfo(CursorInfoFlags.Showing, new(Eleven), new(Two, Three), ThirtyTwo);
        var same = CreateCursorInfo(CursorInfoFlags.Showing, new(Eleven), new(Two, Three), ThirtyTwo);
        var differentSize = CreateCursorInfo(CursorInfoFlags.Showing, new(Eleven), new(Two, Three), ThirtyOne);
        var differentFlags = CreateCursorInfo(default, new(Eleven), new(Two, Three), ThirtyTwo);
        var differentHandle = CreateCursorInfo(CursorInfoFlags.Showing, new(Twelve), new(Two, Three), ThirtyTwo);
        var differentLocation = CreateCursorInfo(CursorInfoFlags.Showing, new(Eleven), new(Four, Three), ThirtyTwo);
        var hidden = CreateCursorInfo(default, new(Eleven), default, ThirtyTwo);

        using var cursorHandle = value.CursorHandle;
        await Assert.That(value.Flags).IsEqualTo(CursorInfoFlags.Showing);
        await Assert.That(cursorHandle.UseNativeHandle(static handle => handle)).IsEqualTo(new(Eleven));
        await Assert.That(value.Location).IsEqualTo(new(Two, Three));
        await Assert.That(value.IsShowing).IsTrue();
        await Assert.That(hidden.IsShowing).IsFalse();
        await Assert.That(default(CursorInfo).IsShowing).IsFalse();
        await Assert.That(value == same).IsTrue();
        await Assert.That(value != same).IsFalse();
        await Assert.That(value.Equals(differentSize)).IsFalse();
        await Assert.That(value.Equals(differentFlags)).IsFalse();
        await Assert.That(value.Equals(differentHandle)).IsFalse();
        await Assert.That(value.Equals(differentLocation)).IsFalse();
        await Assert.That(value.Equals((object)value)).IsTrue();
        await Assert.That(value.Equals(new object())).IsFalse();
        await Assert.That(value.GetHashCode()).IsEqualTo(Zero);
    }

    /// <summary>Verifies all window-information properties and equality branches without a native window.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowInfo_NativeValueBranchesAsync()
    {
        var value = CreateWindowInfo((
            Sixty, new(One, Two, Three, Four), new(Five, Six, Three, Four), WindowStyleFlags.WS_VISIBLE,
            ExtendedWindowStyleFlags.WS_EX_TOPMOST, One, new(Seven, Eight), Nine, Ten));
        var same = CreateWindowInfo((
            Sixty, new(One, Two, Three, Four), new(Five, Six, Three, Four), WindowStyleFlags.WS_VISIBLE,
            ExtendedWindowStyleFlags.WS_EX_TOPMOST, One, new(Seven, Eight), Nine, Ten));
        var differentSize = CreateWindowInfo((
            Fifty, new(One, Two, Three, Four), new(Five, Six, Three, Four), WindowStyleFlags.WS_VISIBLE,
            ExtendedWindowStyleFlags.WS_EX_TOPMOST, One, new(Seven, Eight), Nine, Ten));
        var differentBounds = CreateWindowInfo((
            Sixty, new(Eleven, Two, Three, Four), new(Five, Six, Three, Four), WindowStyleFlags.WS_VISIBLE,
            ExtendedWindowStyleFlags.WS_EX_TOPMOST, One, new(Seven, Eight), Nine, Ten));
        var differentClientBounds = CreateWindowInfo((
            Sixty, new(One, Two, Three, Four), new(Fifteen, Six, Three, Four), WindowStyleFlags.WS_VISIBLE,
            ExtendedWindowStyleFlags.WS_EX_TOPMOST, One, new(Seven, Eight), Nine, Ten));
        var differentStyle = CreateWindowInfo((Sixty, new(One, Two, Three, Four), new(Five, Six, Three, Four), default, ExtendedWindowStyleFlags.WS_EX_TOPMOST, One, new(Seven, Eight), Nine, Ten));
        var differentExtendedStyle = CreateWindowInfo((Sixty, new(One, Two, Three, Four), new(Five, Six, Three, Four), WindowStyleFlags.WS_VISIBLE, default, One, new(Seven, Eight), Nine, Ten));
        var differentStatus = CreateWindowInfo((
            Sixty, new(One, Two, Three, Four), new(Five, Six, Three, Four), WindowStyleFlags.WS_VISIBLE,
            ExtendedWindowStyleFlags.WS_EX_TOPMOST, Zero, new(Seven, Eight), Nine, Ten));
        var differentWidth = CreateWindowInfo((
            Sixty, new(One, Two, Three, Four), new(Five, Six, Three, Four), WindowStyleFlags.WS_VISIBLE,
            ExtendedWindowStyleFlags.WS_EX_TOPMOST, One, new(Seventeen, Eight), Nine, Ten));
        var differentHeight = CreateWindowInfo((
            Sixty, new(One, Two, Three, Four), new(Five, Six, Three, Four), WindowStyleFlags.WS_VISIBLE,
            ExtendedWindowStyleFlags.WS_EX_TOPMOST, One, new(Seven, Eighteen), Nine, Ten));
        var differentAtom = CreateWindowInfo((
            Sixty, new(One, Two, Three, Four), new(Five, Six, Three, Four), WindowStyleFlags.WS_VISIBLE,
            ExtendedWindowStyleFlags.WS_EX_TOPMOST, One, new(Seven, Eight), Twenty, Ten));
        var differentVersion = CreateWindowInfo((
            Sixty, new(One, Two, Three, Four), new(Five, Six, Three, Four), WindowStyleFlags.WS_VISIBLE,
            ExtendedWindowStyleFlags.WS_EX_TOPMOST, One, new(Seven, Eight), Nine, Twenty));

        await Assert.That(value.IsActive).IsTrue();
        await Assert.That(value.Bounds).IsEqualTo(new(One, Two, Three, Four));
        await Assert.That(value.ClientBounds).IsEqualTo(new(Five, Six, Three, Four));
        await Assert.That(value.Style).IsEqualTo(WindowStyleFlags.WS_VISIBLE);
        await Assert.That(value.ExtendedStyle).IsEqualTo(ExtendedWindowStyleFlags.WS_EX_TOPMOST);
        await Assert.That(value.BorderSize).IsEqualTo(new(Seven, Eight));
        await Assert.That(value.AtomWindowType).IsEqualTo((ushort)Nine);
        await Assert.That(value.CreatorVersion).IsEqualTo((ushort)Ten);
        await Assert.That(value == same).IsTrue();
        await Assert.That(value != same).IsFalse();
        await Assert.That(value.Equals(differentSize)).IsFalse();
        await Assert.That(value.Equals(differentBounds)).IsFalse();
        await Assert.That(value.Equals(differentClientBounds)).IsFalse();
        await Assert.That(value.Equals(differentStyle)).IsFalse();
        await Assert.That(value.Equals(differentExtendedStyle)).IsFalse();
        await Assert.That(value.Equals(differentStatus)).IsFalse();
        await Assert.That(value.Equals(differentWidth)).IsFalse();
        await Assert.That(value.Equals(differentHeight)).IsFalse();
        await Assert.That(value.Equals(differentAtom)).IsFalse();
        await Assert.That(value.Equals(differentVersion)).IsFalse();
        await Assert.That(value.Equals((object)value)).IsTrue();
        await Assert.That(value.Equals(new object())).IsFalse();
        await Assert.That(value.GetHashCode()).IsEqualTo(Zero);
    }

    /// <summary>Verifies all window-placement equality and type-conversion branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowPlacement_ValueAndConverterBranchesAsync()
    {
        var value = WindowPlacement.Create();
        value.Flags = WindowPlacementFlags.AsyncWindowPlacement;
        value.ShowCmd = ShowWindowCommands.Maximize;
        value.MinPosition = new(One, Two);
        value.MaxPosition = new(Three, Four);
        value.NormalPosition = new(Five, Six, Seven, Eight);
        var same = value;
        var differentFlags = value;
        differentFlags.Flags = default;
        var differentShowCommand = value;
        differentShowCommand.ShowCmd = default;
        var differentMinimum = value;
        differentMinimum.MinPosition = default;
        var differentMaximum = value;
        differentMaximum.MaxPosition = default;
        var differentNormal = value;
        differentNormal.NormalPosition = default;
        var converter = new WindowPlacementTypeConverter();

        await Assert.That(value == same).IsTrue();
        await Assert.That(value != same).IsFalse();
        await Assert.That(value.Equals(default)).IsFalse();
        await Assert.That(value.Equals(differentFlags)).IsFalse();
        await Assert.That(value.Equals(differentShowCommand)).IsFalse();
        await Assert.That(value.Equals(differentMinimum)).IsFalse();
        await Assert.That(value.Equals(differentMaximum)).IsFalse();
        await Assert.That(value.Equals(differentNormal)).IsFalse();
        await Assert.That(value.Equals((object)value)).IsTrue();
        await Assert.That(value.Equals(new object())).IsFalse();
        await Assert.That(value.GetHashCode()).IsEqualTo(Zero);
        await Assert.That(converter.ConvertToInvariantString(value)).IsEqualTo("Maximize|1,2|3,4|5,6,7,8");
        await Assert.That(converter.ConvertFromInvariantString("Maximize|1,2|3,4|5,6,7,8")).IsEqualTo(differentFlags);
        await Assert.That(() => converter.ConvertFrom(null, CultureInfo.InvariantCulture, new())).Throws<NotSupportedException>();
        await Assert.That(converter.ConvertTo(null, CultureInfo.InvariantCulture, new(), typeof(string))).IsNotNull();
    }

    /// <summary>Creates a cursor-information value from deterministic native-layout data.</summary>
    /// <param name="flags">The cursor visibility flags.</param>
    /// <param name="cursorHandle">The native cursor handle.</param>
    /// <param name="location">The cursor location.</param>
    /// <param name="size">The reported native structure size.</param>
    /// <returns>The cursor-information value.</returns>
    private static CursorInfo CreateCursorInfo(CursorInfoFlags flags, IntPtr cursorHandle, NativePoint location, int size)
    {
        var nativeSize = Marshal.SizeOf<CursorInfo>();
        var buffer = Marshal.AllocHGlobal(nativeSize);
        try
        {
            Marshal.Copy(new byte[nativeSize], Zero, buffer, nativeSize);
            var handleOffset = IntPtr.Size == sizeof(long) ? Eight : Four;
            var pointOffset = handleOffset + IntPtr.Size;
            Marshal.WriteInt32(buffer, Zero, size);
            Marshal.WriteInt32(buffer, sizeof(int), (int)flags);
            Marshal.WriteIntPtr(buffer, handleOffset, cursorHandle);
            Marshal.WriteInt32(buffer, pointOffset, location.X);
            Marshal.WriteInt32(buffer, pointOffset + sizeof(int), location.Y);
            return Marshal.PtrToStructure<CursorInfo>(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>Creates a window-information value from deterministic native-layout data.</summary>
    /// <param name="values">The native window-information values.</param>
    /// <returns>The window-information value.</returns>
    private static WindowInfo CreateWindowInfo((
        int Size,
        NativeRect Bounds,
        NativeRect ClientBounds,
        WindowStyleFlags Style,
        ExtendedWindowStyleFlags ExtendedStyle,
        int Status,
        NativeSize BorderSize,
        int Atom,
        int CreatorVersion) values)
    {
        const int BoundsOffset = 4;
        const int ClientBoundsOffset = 20;
        const int StyleOffset = 36;
        const int ExtendedStyleOffset = 40;
        const int StatusOffset = 44;
        const int BorderWidthOffset = 48;
        const int BorderHeightOffset = 52;
        const int AtomOffset = 56;
        const int CreatorVersionOffset = 58;
        var nativeSize = Marshal.SizeOf<WindowInfo>();
        var buffer = Marshal.AllocHGlobal(nativeSize);
        try
        {
            Marshal.Copy(new byte[nativeSize], Zero, buffer, nativeSize);
            Marshal.WriteInt32(buffer, Zero, values.Size);
            WriteNativeRect(buffer, BoundsOffset, values.Bounds);
            WriteNativeRect(buffer, ClientBoundsOffset, values.ClientBounds);
            Marshal.WriteInt32(buffer, StyleOffset, unchecked((int)values.Style));
            Marshal.WriteInt32(buffer, ExtendedStyleOffset, (int)values.ExtendedStyle);
            Marshal.WriteInt32(buffer, StatusOffset, values.Status);
            Marshal.WriteInt32(buffer, BorderWidthOffset, values.BorderSize.Width);
            Marshal.WriteInt32(buffer, BorderHeightOffset, values.BorderSize.Height);
            Marshal.WriteInt16(buffer, AtomOffset, unchecked((short)values.Atom));
            Marshal.WriteInt16(buffer, CreatorVersionOffset, unchecked((short)values.CreatorVersion));
            return Marshal.PtrToStructure<WindowInfo>(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>Writes a native rectangle into an unmanaged test buffer.</summary>
    /// <param name="buffer">The unmanaged test buffer.</param>
    /// <param name="offset">The rectangle offset in bytes.</param>
    /// <param name="value">The rectangle value.</param>
    private static void WriteNativeRect(IntPtr buffer, int offset, NativeRect value)
    {
        Marshal.WriteInt32(buffer, offset, value.Left);
        Marshal.WriteInt32(buffer, offset + sizeof(int), value.Top);
        Marshal.WriteInt32(buffer, offset + (sizeof(int) * Two), value.Right);
        Marshal.WriteInt32(buffer, offset + (sizeof(int) * Three), value.Bottom);
    }
}
