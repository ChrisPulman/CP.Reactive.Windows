// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides deterministic managed coverage for final Core value-object branches.</summary>
public sealed class CoverageFinalCoreValueBranchTests
{
    /// <summary>Defines the number of integer fields in a native point.</summary>
    private const int NativePointIntegerCount = 2;

    /// <summary>Defines the number of integer fields in a native rectangle.</summary>
    private const int NativeRectangleIntegerCount = 4;

    /// <summary>Defines the number of state entries in native title-bar and scrollbar arrays.</summary>
    private const int NativeStateEntryCount = 6;

    /// <summary>Defines the bottom coordinate used by native-layout tests.</summary>
    private const int NativeBottomCoordinate = 6;

    /// <summary>Defines the following coordinate used by title-bar rectangle tests.</summary>
    private const int NativeFollowingCoordinate = 7;

    /// <summary>Verifies every short-circuit outcome of a native message value comparison.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Msg_Equality_DistinguishesEachNativeMemberAsync()
    {
        var value = CreateMessage(One, WindowsMessages.WM_APP, Two, Three, Four, Five, Six);

        await Assert.That(value.Equals(CreateMessage(One, WindowsMessages.WM_APP, Two, Three, Four, Five, Six))).IsTrue();
        await Assert.That(value.Equals(CreateMessage(Two, WindowsMessages.WM_APP, Two, Three, Four, Five, Six))).IsFalse();
        await Assert.That(value.Equals(CreateMessage(One, WindowsMessages.WM_CLOSE, Two, Three, Four, Five, Six))).IsFalse();
        await Assert.That(value.Equals(CreateMessage(One, WindowsMessages.WM_APP, Three, Three, Four, Five, Six))).IsFalse();
        await Assert.That(value.Equals(CreateMessage(One, WindowsMessages.WM_APP, Two, Four, Four, Five, Six))).IsFalse();
        await Assert.That(value.Equals(CreateMessage(One, WindowsMessages.WM_APP, Two, Three, Five, Five, Six))).IsFalse();
        await Assert.That(value.Equals(CreateMessage(One, WindowsMessages.WM_APP, Two, Three, Four, Six, Six))).IsFalse();
    }

    /// <summary>Verifies object equality branch outcomes for value objects with safe native handles.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ObjectEquality_RecognizesMatchingAndDifferentValueTypesAsync()
    {
        var appBar = AppBarData.Create();
        appBar.SetWindowHandle(new(One));
        appBar.CallbackMessageIdentifier = Two;
        var changedAppBar = appBar;
        changedAppBar.CallbackMessageIdentifier = Three;

        var nativeAppBar = appBar.ToNative();
        var changedNativeAppBar = new AppBarData.NativeAppBarData(new(One), Three, default, default, Zero);
        var shellFile = new ShellFileInfo(IntPtr.Zero, One, Two, "display", "type");
        var changedShellFile = new ShellFileInfo(IntPtr.Zero, Three, Two, "display", "type");
        var process = new RmUniqueProcess(One, new() { dwLowDateTime = Two, dwHighDateTime = Three });
        var changedProcess = new RmUniqueProcess(Two, new() { dwLowDateTime = Two, dwHighDateTime = Three });
        var processInfo = new RmProcessInfo(process, "application", "service", default, default, Four, true);
        var changedProcessInfo = new RmProcessInfo(process, "application", "service", default, default, Four, false);

        await Assert.That(appBar.Equals((object)appBar)).IsTrue();
        await Assert.That(appBar.Equals((object)changedAppBar)).IsFalse();
        await Assert.That(nativeAppBar.Equals((object)nativeAppBar)).IsTrue();
        await Assert.That(nativeAppBar.Equals((object)changedNativeAppBar)).IsFalse();
        await Assert.That(shellFile.Equals((object)shellFile)).IsTrue();
        await Assert.That(shellFile.Equals((object)changedShellFile)).IsFalse();
        await Assert.That(process.Equals((object)process)).IsTrue();
        await Assert.That(process.Equals((object)changedProcess)).IsFalse();
        await Assert.That(processInfo.Equals((object)processInfo)).IsTrue();
        await Assert.That(processInfo.Equals((object)changedProcessInfo)).IsFalse();
    }

    /// <summary>Verifies the defensive null-pointer conversion fallback without resolving a real SID.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SidAndAttributes_NullNativeText_UsesEmptyStringFallbackAsync()
    {
        using var operations = Advapi32Api.OverrideOperationsForTesting(
            ConvertSidToNullText,
            static (_, _, _, _, _) => Zero,
            DoNotOpenRegistry,
            DoNotGetTokenInformation);

        var value = new SidAndAttributes(new(One), Zero);

        await Assert.That(value.ToSidString()).IsEqualTo(string.Empty);
    }

    /// <summary>Verifies every state-comparison short-circuit outcome of a scrollbar value.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ScrollBarInfo_Equality_DistinguishesEachStateSlotAsync()
    {
        var value = CreateScrollBarInfo();

        await Assert.That(value.Equals(CreateScrollBarInfo())).IsTrue();
        for (var index = Zero; index < Six; index++)
        {
            await Assert.That(value.Equals(CreateScrollBarInfo(index))).IsFalse();
        }
    }

    /// <summary>Verifies every rectangle-comparison short-circuit outcome of a title-bar value.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TitleBarInfoEx_Equality_DistinguishesEachRectangleSlotAsync()
    {
        var value = CreateTitleBarInfoEx();

        await Assert.That(value.Equals(CreateTitleBarInfoEx())).IsTrue();
        for (var index = Zero; index < Six; index++)
        {
            await Assert.That(value.Equals(CreateTitleBarInfoEx(index))).IsFalse();
        }
    }

    /// <summary>Verifies all null converter fallbacks exposed by the placement converter.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowPlacementTypeConverter_NullComponentConversions_UseEmptyValuesAsync()
    {
        var pointProvider = TypeDescriptor.AddAttributes(
            typeof(NativePoint),
            new TypeConverterAttribute(typeof(NullNativePointTypeConverter)));
        var rectangleProvider = TypeDescriptor.AddAttributes(
            typeof(NativeRect),
            new TypeConverterAttribute(typeof(NullNativeRectTypeConverter)));
        try
        {
            var converter = new WindowPlacementTypeConverter();
            var nullMinimum = (WindowPlacement)converter.ConvertFromInvariantString("Normal|null-point|3,4|5,6,7,8");
            var nullMaximum = (WindowPlacement)converter.ConvertFromInvariantString("Normal|1,2|null-point|5,6,7,8");
            var nullNormal = (WindowPlacement)converter.ConvertFromInvariantString("Normal|1,2|3,4|null-rectangle");

            await Assert.That(nullMinimum.MinPosition).IsEqualTo(NativePoint.Empty);
            await Assert.That(nullMaximum.MaxPosition).IsEqualTo(NativePoint.Empty);
            await Assert.That(nullNormal.NormalPosition).IsEqualTo(NativeRect.Empty);
        }
        finally
        {
            TypeDescriptor.RemoveProvider(rectangleProvider, typeof(NativeRect));
            TypeDescriptor.RemoveProvider(pointProvider, typeof(NativePoint));
        }
    }

    /// <summary>Creates a message value from managed native-layout bytes.</summary>
    /// <param name="handle">The window handle.</param>
    /// <param name="message">The message identifier.</param>
    /// <param name="wordParameter">The word parameter.</param>
    /// <param name="longParameter">The long parameter.</param>
    /// <param name="time">The message tick count.</param>
    /// <param name="cursorX">The cursor X coordinate.</param>
    /// <param name="cursorY">The cursor Y coordinate.</param>
    /// <returns>The populated message value.</returns>
    private static Msg CreateMessage(
        int handle,
        WindowsMessages message,
        int wordParameter,
        int longParameter,
        int time,
        int cursorX,
        int cursorY) =>
        ReadStructure<Msg>(
            buffer =>
            {
                var pointerSize = IntPtr.Size;
                var messageOffset = pointerSize;
                var wordParameterOffset = AlignToPointer(messageOffset + sizeof(uint));
                var longParameterOffset = wordParameterOffset + pointerSize;
                var timeOffset = longParameterOffset + pointerSize;
                var cursorOffset = timeOffset + sizeof(uint);

                Marshal.WriteIntPtr(buffer, Zero, new(handle));
                Marshal.WriteInt32(buffer, messageOffset, unchecked((int)message));
                Marshal.WriteIntPtr(buffer, wordParameterOffset, new(wordParameter));
                Marshal.WriteIntPtr(buffer, longParameterOffset, new(longParameter));
                Marshal.WriteInt32(buffer, timeOffset, time);
                Marshal.WriteInt32(buffer, cursorOffset, cursorX);
                Marshal.WriteInt32(buffer, cursorOffset + sizeof(int), cursorY);
            });

    /// <summary>Creates a managed native scrollbar value with an optional altered state slot.</summary>
    /// <param name="changedStateIndex">The state slot to alter, or a negative value for no alteration.</param>
    /// <returns>The populated scrollbar value.</returns>
    private static ScrollBarInfo CreateScrollBarInfo(int changedStateIndex = -1) =>
        ReadStructure<ScrollBarInfo>(
            buffer =>
            {
                const int BoundsOffset = sizeof(uint);
                const int ScrollBarRectangleIntegerCount = 8;
                const int StatesOffset = BoundsOffset + (sizeof(int) * ScrollBarRectangleIntegerCount);
                Marshal.WriteInt32(buffer, Zero, Marshal.SizeOf<ScrollBarInfo>());
                WriteNativeRect(buffer, BoundsOffset, One, Two, Four, NativeBottomCoordinate);
                Marshal.WriteInt32(buffer, BoundsOffset + (sizeof(int) * NativeRectangleIntegerCount), One);
                Marshal.WriteInt32(buffer, BoundsOffset + (sizeof(int) * (NativeRectangleIntegerCount + One)), Two);
                Marshal.WriteInt32(buffer, BoundsOffset + (sizeof(int) * NativeStateEntryCount), Three);
                Marshal.WriteInt32(buffer, BoundsOffset + (sizeof(int) * NativeFollowingCoordinate), Four);
                for (var index = Zero; index < NativeStateEntryCount; index++)
                {
                    Marshal.WriteInt32(buffer, StatesOffset + (index * sizeof(uint)), index == changedStateIndex ? Two : One);
                }
            });

    /// <summary>Creates a managed native title-bar value with an optional altered rectangle slot.</summary>
    /// <param name="changedRectangleIndex">The rectangle slot to alter, or a negative value for no alteration.</param>
    /// <returns>The populated title-bar value.</returns>
    private static TitleBarInfoEx CreateTitleBarInfoEx(int changedRectangleIndex = -1) =>
        ReadStructure<TitleBarInfoEx>(
            buffer =>
            {
                const int BoundsOffset = sizeof(uint);
                const int StatesOffset = BoundsOffset + (sizeof(int) * NativeRectangleIntegerCount);
                const int RectanglesOffset = StatesOffset + (sizeof(uint) * NativeStateEntryCount);
                Marshal.WriteInt32(buffer, Zero, Marshal.SizeOf<TitleBarInfoEx>());
                WriteNativeRect(buffer, BoundsOffset, One, Two, Four, NativeBottomCoordinate);
                for (var index = Zero; index < NativeStateEntryCount; index++)
                {
                    Marshal.WriteInt32(buffer, StatesOffset + (index * sizeof(uint)), One);
                    var rectangleOffset = RectanglesOffset + (index * (sizeof(int) * NativeRectangleIntegerCount));
                    WriteNativeRect(buffer, rectangleOffset, index, index + One, index + Three + (index == changedRectangleIndex ? One : Zero), index + Four);
                }
            });

    /// <summary>Reads a structure from a temporary, zero-initialized managed native-layout buffer.</summary>
    /// <typeparam name="T">The structure type.</typeparam>
    /// <param name="populate">Writes the managed layout values into the buffer.</param>
    /// <returns>The populated structure.</returns>
    private static T ReadStructure<T>(Action<IntPtr> populate)
        where T : struct
    {
        Throw.IfNull(populate);
        var size = Marshal.SizeOf<T>();
        var buffer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(new byte[size], Zero, buffer, size);
            populate(buffer);
            return Marshal.PtrToStructure<T>(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>Writes a native RECT field in its raw left, top, right, bottom layout.</summary>
    /// <param name="buffer">The target buffer.</param>
    /// <param name="offset">The rectangle offset.</param>
    /// <param name="left">The left coordinate.</param>
    /// <param name="top">The top coordinate.</param>
    /// <param name="right">The right coordinate.</param>
    /// <param name="bottom">The bottom coordinate.</param>
    private static void WriteNativeRect(IntPtr buffer, int offset, int left, int top, int right, int bottom)
    {
        Marshal.WriteInt32(buffer, offset, left);
        Marshal.WriteInt32(buffer, offset + sizeof(int), top);
        Marshal.WriteInt32(buffer, offset + (sizeof(int) * NativePointIntegerCount), right);
        Marshal.WriteInt32(buffer, offset + (sizeof(int) * Three), bottom);
    }

    /// <summary>Aligns a byte offset for a native pointer field.</summary>
    /// <param name="offset">The unaligned offset.</param>
    /// <returns>The pointer-aligned offset.</returns>
    private static int AlignToPointer(int offset) =>
        (offset + IntPtr.Size - One) & ~(IntPtr.Size - One);

    /// <summary>Returns successful SID conversion with a null result pointer.</summary>
    /// <param name="sid">The source SID pointer.</param>
    /// <param name="sidString">The converted SID pointer.</param>
    /// <returns><see langword="true" />.</returns>
    private static bool ConvertSidToNullText(IntPtr sid, out IntPtr sidString)
    {
        GC.KeepAlive(sid);
        sidString = IntPtr.Zero;
        return true;
    }

    /// <summary>Provides a deterministic unused registry-open operation.</summary>
    /// <param name="key">The parent key.</param>
    /// <param name="subKey">The subkey.</param>
    /// <param name="options">The registry options.</param>
    /// <param name="desiredAccess">The desired access rights.</param>
    /// <param name="openedKey">The opened registry key.</param>
    /// <returns>A failing Win32 result.</returns>
    private static int DoNotOpenRegistry(
        IntPtr key,
        string subKey,
        RegistryOpenOptions options,
        RegistryKeySecurityAccessRights desiredAccess,
        out Microsoft.Win32.SafeHandles.SafeRegistryHandle openedKey)
    {
        GC.KeepAlive(key);
        GC.KeepAlive(subKey);
        GC.KeepAlive(options);
        GC.KeepAlive(desiredAccess);
        openedKey = null!;
        return One;
    }

    /// <summary>Provides a deterministic unused token-information operation.</summary>
    /// <param name="tokenHandle">The access token handle.</param>
    /// <param name="informationClass">The requested information class.</param>
    /// <param name="information">The information buffer.</param>
    /// <param name="informationLength">The information-buffer length.</param>
    /// <param name="returnLength">The returned length.</param>
    /// <returns><see langword="false" />.</returns>
    private static bool DoNotGetTokenInformation(
        IntPtr tokenHandle,
        TokenInformationClasses informationClass,
        IntPtr information,
        int informationLength,
        out int returnLength)
    {
        GC.KeepAlive(tokenHandle);
        GC.KeepAlive(informationClass);
        GC.KeepAlive(information);
        GC.KeepAlive(informationLength);
        returnLength = Zero;
        return false;
    }

    /// <summary>Converts selected point text to a null native value for defensive fallback coverage.</summary>
    public sealed class NullNativePointTypeConverter : NativePointTypeConverter
    {
        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
            value is string pointText && string.Equals(pointText, "null-point", StringComparison.Ordinal)
                ? null!
                : base.ConvertFrom(context, culture, value);
    }

    /// <summary>Converts selected rectangle text to a null native value for defensive fallback coverage.</summary>
    public sealed class NullNativeRectTypeConverter : NativeRectTypeConverter
    {
        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
            value is string rectangleText && string.Equals(rectangleText, "null-rectangle", StringComparison.Ordinal)
                ? null!
                : base.ConvertFrom(context, culture, value);
    }
}
