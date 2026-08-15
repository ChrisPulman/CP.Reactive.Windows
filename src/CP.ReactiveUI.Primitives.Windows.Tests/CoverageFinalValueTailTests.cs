// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.TypeConverters;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.TypeConverters;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tail coverage for deterministic value, converter, and safe-handle branches.</summary>
public sealed class CoverageFinalValueTailTests
{
    /// <summary>Native point conversion text.</summary>
    private const string PointText = "1.5,2.5";

    /// <summary>Native size conversion text.</summary>
    private const string SizeText = "3,4";

    /// <summary>Native size-f conversion text.</summary>
    private const string SizeFloatText = "3.5,4.5";

    /// <summary>Native rect conversion text.</summary>
    private const string RectText = "1,2,3,4";

    /// <summary>Native rect-f conversion text.</summary>
    private const string RectFloatText = "1.5,2.5,3.5,4.5";

    /// <summary>Invalid converter text.</summary>
    private const string InvalidText = "not,a,value";

    /// <summary>Default window placement conversion text.</summary>
    private const string DefaultWindowPlacementText = "Hide|0,0|0,0|0,0,0,0";

    /// <summary>Exercises value alternate constructors and conversion branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ValueAlternateConstructors_ReturnExpectedValuesAsync()
    {
        var windowsSize = new System.Windows.Size(Twelve, Thirteen);
        var nativeSize = new NativeSize(windowsSize);
        var drawingSize = new Size(Twelve, Thirteen);
        var nativeSizeFloatFromDrawing = new NativeSizeFloat(drawingSize);
        var nativeSizeFloatFromWindows = new NativeSizeFloat(windowsSize);
        System.Windows.Size convertedWindowsSize = nativeSizeFloatFromDrawing;

        await Assert.That(nativeSize).IsEqualTo(new(Twelve, Thirteen));
        await Assert.That(nativeSizeFloatFromDrawing).IsEqualTo(new(TwelveFloat, Thirteen));
        await Assert.That(nativeSizeFloatFromWindows).IsEqualTo(new(TwelveFloat, Thirteen));
        await Assert.That(convertedWindowsSize).IsEqualTo(windowsSize);
        await Assert.That(NativePointFloat.Empty).IsEqualTo(default);
    }

    /// <summary>Exercises converter capability and fallback branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TypeConverters_CapabilityAndFallbackBranches_ReturnExpectedValuesAsync()
    {
        var pointConverter = new NativePointFloatTypeConverter();
        var sizeConverter = new NativeSizeTypeConverter();
        var sizeFloatConverter = new NativeSizeFloatTypeConverter();
        var rectConverter = new NativeRectTypeConverter();
        var rectFloatConverter = new NativeRectFloatTypeConverter();
        var windowPlacementConverter = new WindowPlacementTypeConverter();

        await Assert.That(pointConverter.CanConvertFrom(typeof(string))).IsTrue();
        await Assert.That(pointConverter.CanConvertFrom(typeof(int))).IsFalse();
        await Assert.That(pointConverter.CanConvertTo(typeof(string))).IsTrue();
        await Assert.That(pointConverter.CanConvertTo(typeof(Size))).IsFalse();
        await Assert.That(pointConverter.ConvertFromInvariantString(PointText)).IsEqualTo(new NativePointFloat(OneAndHalfFloat, TwoAndHalfFloat));
        await Assert.That(pointConverter.ConvertToInvariantString(new NativePointFloat(OneAndHalfFloat, TwoAndHalfFloat))).IsEqualTo(PointText);
        await Assert.That(() => pointConverter.ConvertFromInvariantString(InvalidText)).Throws<NotSupportedException>();
        await Assert.That(() => pointConverter.ConvertTo(null, CultureInfo.InvariantCulture, new(), typeof(Size))).Throws<NotSupportedException>();

        await Assert.That(sizeConverter.CanConvertFrom(typeof(string))).IsTrue();
        await Assert.That(sizeConverter.CanConvertFrom(typeof(int))).IsFalse();
        await Assert.That(sizeConverter.CanConvertTo(typeof(string))).IsTrue();
        await Assert.That(sizeConverter.CanConvertTo(typeof(Size))).IsFalse();
        await Assert.That(sizeConverter.ConvertFromInvariantString(SizeText)).IsEqualTo(new NativeSize(Three, Four));
        await Assert.That(sizeConverter.ConvertToInvariantString(new NativeSize(Three, Four))).IsEqualTo("4,3");

        await Assert.That(sizeFloatConverter.CanConvertFrom(typeof(string))).IsTrue();
        await Assert.That(sizeFloatConverter.CanConvertFrom(typeof(int))).IsFalse();
        await Assert.That(sizeFloatConverter.CanConvertTo(typeof(string))).IsTrue();
        await Assert.That(sizeFloatConverter.CanConvertTo(typeof(Size))).IsFalse();
        await Assert.That(sizeFloatConverter.ConvertFromInvariantString(SizeFloatText)).IsEqualTo(new NativeSizeFloat(ThreeAndHalfFloat, FourAndHalfFloat));
        await Assert.That(sizeFloatConverter.ConvertToInvariantString(new NativeSizeFloat(ThreeAndHalfFloat, FourAndHalfFloat))).IsEqualTo(SizeFloatText);

        await Assert.That(rectConverter.CanConvertFrom(typeof(string))).IsTrue();
        await Assert.That(rectConverter.CanConvertFrom(typeof(int))).IsFalse();
        await Assert.That(rectConverter.CanConvertTo(typeof(string))).IsTrue();
        await Assert.That(rectConverter.CanConvertTo(typeof(Size))).IsFalse();
        await Assert.That(rectConverter.ConvertFromInvariantString(RectText)).IsEqualTo(new NativeRect(One, Two, Three, Four));
        await Assert.That(rectConverter.ConvertToInvariantString(new NativeRect(One, Two, Three, Four))).IsEqualTo(RectText);

        await Assert.That(rectFloatConverter.CanConvertFrom(typeof(string))).IsTrue();
        await Assert.That(rectFloatConverter.CanConvertFrom(typeof(int))).IsFalse();
        await Assert.That(rectFloatConverter.CanConvertTo(typeof(string))).IsTrue();
        await Assert.That(rectFloatConverter.CanConvertTo(typeof(Size))).IsFalse();
        await Assert.That(rectFloatConverter.ConvertFromInvariantString(RectFloatText)).IsEqualTo(new NativeRectFloat(OneAndHalfFloat, TwoAndHalfFloat, ThreeAndHalfFloat, FourAndHalfFloat));
        await Assert.That(rectFloatConverter.ConvertToInvariantString(new NativeRectFloat(OneAndHalfFloat, TwoAndHalfFloat, ThreeAndHalfFloat, FourAndHalfFloat))).IsEqualTo(RectFloatText);

        await Assert.That(windowPlacementConverter.CanConvertFrom(typeof(string))).IsTrue();
        await Assert.That(windowPlacementConverter.CanConvertFrom(typeof(int))).IsFalse();
        await Assert.That(windowPlacementConverter.CanConvertTo(typeof(string))).IsTrue();
        await Assert.That(windowPlacementConverter.CanConvertTo(typeof(Size))).IsFalse();
        await Assert.That(windowPlacementConverter.ConvertToInvariantString(WindowPlacement.Create())).IsEqualTo(DefaultWindowPlacementText);
        await Assert.That(() => windowPlacementConverter.ConvertFromInvariantString(InvalidText)).Throws<NotSupportedException>();
    }

    /// <summary>Exercises invalid and non-owned safe-handle branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SafeHandles_InvalidAndReferenceBranches_ReturnExpectedValuesAsync()
    {
        using var selectObjectHandle = new SafeSelectObjectHandle();
        using var compatibleDcHandle = new SafeCompatibleDcHandle();
        using var preexistingCompatibleDcHandle = new SafeCompatibleDcHandle(new IntPtr(FortyTwo));
        using var dibSectionHandle = new SafeDibSectionHandle();
        using var preexistingDibSectionHandle = new SafeDibSectionHandle(new IntPtr(FortyTwo));
        using var regionHandle = new SafeRegionHandle();
        using var preexistingRegionHandle = new SafeRegionHandle(new IntPtr(FortyTwo));
        using var cursorHandle = new SafeCursorReferenceHandle(new IntPtr(FortyTwo));
        using var defaultCursorHandle = new SafeCursorReferenceHandle();
        using var monitorHandle = new SafeMonitorHandle(new IntPtr(NinetyNine));
        using var defaultMonitorHandle = new SafeMonitorHandle();

        preexistingCompatibleDcHandle.SetHandleAsInvalid();
        preexistingDibSectionHandle.SetHandleAsInvalid();
        preexistingRegionHandle.SetHandleAsInvalid();

        await Assert.That(selectObjectHandle.IsInvalid).IsTrue();
        await Assert.That(compatibleDcHandle.IsInvalid).IsTrue();
        await Assert.That(dibSectionHandle.IsInvalid).IsTrue();
        await Assert.That(regionHandle.IsInvalid).IsTrue();
        await Assert.That(defaultCursorHandle.IsInvalid).IsTrue();
        await Assert.That(cursorHandle.UseNativeHandle(static handle => handle)).IsEqualTo(new(FortyTwo));
        await Assert.That(monitorHandle.IsInvalid).IsFalse();
        await Assert.That(defaultMonitorHandle.IsInvalid).IsTrue();
    }
}
