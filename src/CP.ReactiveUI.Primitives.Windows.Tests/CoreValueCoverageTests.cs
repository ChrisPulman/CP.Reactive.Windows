// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing.Imaging;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Structs.PixelFormats;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Extensive coverage for Common value types and pure managed helpers.</summary>
public sealed class CoreValueCoverageTests
{
    /// <summary>Zero test value.</summary>
    private const int Zero = 0;

    /// <summary>One test value.</summary>
    private const int One = 1;

    /// <summary>Two test value.</summary>
    private const int Two = 2;

    /// <summary>Three test value.</summary>
    private const int Three = 3;

    /// <summary>Four test value.</summary>
    private const int Four = 4;

    /// <summary>Five test value.</summary>
    private const int Five = 5;

    /// <summary>Six test value.</summary>
    private const int Six = 6;

    /// <summary>Seven test value.</summary>
    private const int Seven = 7;

    /// <summary>Eight test value.</summary>
    private const int Eight = 8;

    /// <summary>Nine test value.</summary>
    private const int Nine = 9;

    /// <summary>Ten test value.</summary>
    private const int Ten = 10;

    /// <summary>Eleven test value.</summary>
    private const int Eleven = 11;

    /// <summary>Twelve test value.</summary>
    private const int Twelve = 12;

    /// <summary>Twenty test value.</summary>
    private const int Twenty = 20;

    /// <summary>Twenty-nine test value.</summary>
    private const int TwentyNine = 29;

    /// <summary>Thirty test value.</summary>
    private const int Thirty = 30;

    /// <summary>Thirty-one test value.</summary>
    private const int ThirtyOne = 31;

    /// <summary>Forty test value.</summary>
    private const int Forty = 40;

    /// <summary>Forty-one test value.</summary>
    private const int FortyOne = 41;

    /// <summary>Fifty test value.</summary>
    private const int Fifty = 50;

    /// <summary>Sixty test value.</summary>
    private const int Sixty = 60;

    /// <summary>Sixty-one test value.</summary>
    private const int SixtyOne = 61;

    /// <summary>One hundred test value.</summary>
    private const int OneHundred = 100;

    /// <summary>Opaque channel test value.</summary>
    private const byte Opaque = byte.MaxValue;

    /// <summary>Half alpha test value.</summary>
    private const byte HalfAlpha = 128;

    /// <summary>Mid green test value.</summary>
    private const byte MidGreen = 127;

    /// <summary>Expected blended green test value.</summary>
    private const byte BlendedGreen = 63;

    /// <summary>Three point two five test value.</summary>
    private const float ThreePoint25 = 3.25F;

    /// <summary>Four point seven five test value.</summary>
    private const float FourPoint75 = 4.75F;

    /// <summary>Six point two five test value.</summary>
    private const float SixPoint25 = 6.25F;

    /// <summary>Seven point seven five test value.</summary>
    private const float SevenPoint75 = 7.75F;

    /// <summary>Ten point five test value.</summary>
    private const float TenPointFive = 10.5F;

    /// <summary>Twenty point five test value.</summary>
    private const float TwentyPointFive = 20.5F;

    /// <summary>Thirty point five test value.</summary>
    private const float ThirtyPointFive = 30.5F;

    /// <summary>Forty point five test value.</summary>
    private const float FortyPointFive = 40.5F;

    /// <summary>Described enum for extension coverage.</summary>
    private enum DescribedEnum
    {
        /// <summary>Value with a description attribute.</summary>
        [Description("Stored description")]
        Value,

        /// <summary>Value without a description attribute.</summary>
        NoDescription,
    }

    /// <summary>Exercises NativePoint value semantics, conversions, and extension helpers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativePoint_ValueMembersAndExtensions_ReturnExpectedValuesAsync()
    {
        var point = new NativePoint(Three, Four);
        var same = new NativePoint(Three, Four);
        var different = new NativePoint(Four, Three);
        var (x, y) = point;
        System.Drawing.Point drawingPoint = point;
        NativePoint fromDrawing = drawingPoint;
        NativePoint fromDrawingFloat = new PointF(ThreeAndNineTenthsFloat, FourAndEightTenthsFloat);
        NativePoint fromFloat = new NativePointFloat(ThreeAndNineTenthsFloat, FourAndEightTenthsFloat);
        System.Windows.Point windowsPoint = point;

        await Assert.That(x).IsEqualTo(Three);
        await Assert.That(y).IsEqualTo(Four);
        await Assert.That(point == same).IsTrue();
        await Assert.That(point != different).IsTrue();
        await Assert.That(point.Equals((object)drawingPoint)).IsTrue();
        await Assert.That(point.Equals((object)"not a point")).IsFalse();
        await Assert.That(point.GetHashCode()).IsEqualTo(same.GetHashCode());
        await Assert.That(point.ToString()).IsEqualTo("{X: 3; Y: 4;}");
        await Assert.That(point.ToPoint()).IsEqualTo(windowsPoint);
        await Assert.That(point.ToNativePoint()).IsEqualTo(point);
        await Assert.That(fromDrawing).IsEqualTo(point);
        await Assert.That(fromDrawingFloat).IsEqualTo(point);
        await Assert.That(fromFloat).IsEqualTo(point);
        await Assert.That(point.ChangeX(Ten)).IsEqualTo(new(Ten, Four));
        await Assert.That(point.ChangeY(Eleven)).IsEqualTo(new(Three, Eleven));
        await Assert.That(point.Offset(new NativePoint(Seven, Eight))).IsEqualTo(new(Ten, Twelve));
        await Assert.That(point.Offset()).IsEqualTo(new(0, 0));
        await Assert.That(point.Offset(Nine)).IsEqualTo(new(Twelve, 0));
        await Assert.That(point.Offset(null, Five)).IsEqualTo(new(0, Nine));
    }

    /// <summary>Exercises NativePointFloat value semantics, conversions, and extension helpers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativePointFloat_ValueMembersAndExtensions_ReturnExpectedValuesAsync()
    {
        var point = new NativePointFloat(ThreeAndQuarterFloat, FourAndThreeQuartersFloat);
        var same = new NativePointFloat(ThreeAndQuarterFloat, FourAndThreeQuartersFloat);
        var (x, y) = point;
        PointF drawingPointF = point;
        System.Drawing.Point drawingPoint = point;
        NativePointFloat fromDrawing = drawingPoint;
        NativePointFloat fromDrawingFloat = drawingPointF;
        NativePointFloat fromNativePoint = new NativePoint(Three, Four);
        System.Windows.Point windowsPoint = point;
        NativePointFloat fromWindowsPoint = windowsPoint;

        await Assert.That(x).IsEqualTo(ThreeAndQuarterFloat);
        await Assert.That(y).IsEqualTo(FourAndThreeQuartersFloat);
        await Assert.That(point == same).IsTrue();
        await Assert.That(point != new NativePointFloat(ThreeAndQuarterFloat, NineFloat)).IsTrue();
        await Assert.That(point.Equals((object)drawingPoint)).IsFalse();
        await Assert.That(fromDrawing).IsEqualTo(new(Three, Four));
        await Assert.That(fromDrawingFloat).IsEqualTo(point);
        await Assert.That(fromNativePoint).IsEqualTo(new(Three, Four));
        await Assert.That(fromWindowsPoint).IsEqualTo(point);
        await Assert.That(point.ToPoint()).IsEqualTo(windowsPoint);
        await Assert.That(point.ToPointF()).IsEqualTo(drawingPointF);
        await Assert.That(point.ToNativePointFloat()).IsEqualTo(point);
        await Assert.That(point.GetHashCode()).IsEqualTo(same.GetHashCode());
        await Assert.That(point.ToString()).IsEqualTo("3.25,4.75");
        await Assert.That(point.ChangeX(NineAndHalfFloat)).IsEqualTo(new(NineAndHalfFloat, FourAndThreeQuartersFloat));
        await Assert.That(point.ChangeY(EightAndHalfFloat)).IsEqualTo(new(ThreeAndQuarterFloat, EightAndHalfFloat));
        await Assert.That(point.Offset()).IsEqualTo(new(0, 0));
        await Assert.That(point.Offset(OneAndHalfFloat)).IsEqualTo(new(FourAndThreeQuartersFloat, 0));
        await Assert.That(point.Offset(null, TwoAndHalfFloat)).IsEqualTo(new(0, SevenAndQuarterFloat));
        await Assert.That(point.Offset(new NativePointFloat(1, Two))).IsEqualTo(new(FourAndQuarterFloat, SixAndThreeQuartersFloat));
        await Assert.That(point.Round()).IsEqualTo(new(Three, Five));
    }

    /// <summary>Exercises NativeSize value semantics, comparisons, conversions, and extension helpers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeSize_ValueMembersAndExtensions_ReturnExpectedValuesAsync()
    {
        var size = new NativeSize(Six, Seven);
        var same = new NativeSize(new Size(Six, Seven));
        var windowsSize = new System.Windows.Size(Six, Seven);
        NativeSize fromWindows = windowsSize;
        Size drawingSize = size;
        var (width, height) = size;

        await Assert.That(width).IsEqualTo(Six);
        await Assert.That(height).IsEqualTo(Seven);
        await Assert.That(NativeSize.Empty.IsEmpty).IsTrue();
        await Assert.That(size.IsEmpty).IsFalse();
        await Assert.That(size == same).IsTrue();
        await Assert.That(size != new NativeSize(Seven, Six)).IsTrue();
        await Assert.That(size.Equals((object)drawingSize)).IsTrue();
        await Assert.That(size.Equals((object)windowsSize)).IsTrue();
        await Assert.That(size.Equals((object)"not a size")).IsFalse();
        await Assert.That(size.CompareTo(new(1, Two))).IsLessThan(0);
        await Assert.That(size.GetHashCode()).IsEqualTo(same.GetHashCode());
        await Assert.That(size.ToString()).IsEqualTo("{Width: 6; Height: 7;}");
        await Assert.That(size.ToNativeSize()).IsEqualTo(size);
        await Assert.That(size.ToSize()).IsEqualTo(windowsSize);
        await Assert.That(fromWindows).IsEqualTo(size);
        await Assert.That((System.Windows.Size)size).IsEqualTo(windowsSize);
        await Assert.That(size.ChangeWidth(Eight)).IsEqualTo(new(Eight, Seven));
        await Assert.That(size.ChangeHeight(Nine)).IsEqualTo(new(Six, Nine));
    }

    /// <summary>Exercises NativeSizeFloat value semantics, comparisons, conversions, and extension helpers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeSizeFloat_ValueMembersAndExtensions_ReturnExpectedValuesAsync()
    {
        var size = new NativeSizeFloat(SixAndQuarterFloat, SevenAndThreeQuartersFloat);
        var same = new NativeSizeFloat(SixAndQuarterDouble, SevenAndThreeQuartersDouble);
        var windowsSize = new System.Windows.Size(SixAndQuarterDouble, SevenAndThreeQuartersDouble);
        NativeSizeFloat fromWindows = windowsSize;
        SizeF drawingSizeF = size;
        Size drawingSize = size;
        NativeSizeFloat fromDrawing = drawingSize;
        NativeSizeFloat fromDrawingFloat = drawingSizeF;
        NativeSizeFloat fromNativeSize = new NativeSize(Six, Seven);
        var (width, height) = size;

        await Assert.That(width).IsEqualTo(SixAndQuarterFloat);
        await Assert.That(height).IsEqualTo(SevenAndThreeQuartersFloat);
        await Assert.That(NativeSizeFloat.Empty.IsEmpty).IsTrue();
        await Assert.That(size.IsEmpty).IsFalse();
        await Assert.That(size == same).IsTrue();
        await Assert.That(size != new NativeSizeFloat(SevenAndQuarterFloat, SixAndThreeQuartersFloat)).IsTrue();
        await Assert.That(size.Equals((object)windowsSize)).IsTrue();
        await Assert.That(size.Equals((object)drawingSize)).IsFalse();
        await Assert.That(size.Equals((object)"not a size")).IsFalse();
        await Assert.That(size.CompareTo(new(1, Two))).IsLessThan(0);
        await Assert.That(size.GetHashCode()).IsEqualTo(same.GetHashCode());
        await Assert.That(size.ToString()).IsEqualTo("{Width: 6.25; Height: 7.75;}");
        await Assert.That(size.ToSize()).IsEqualTo(windowsSize);
        await Assert.That(size.ToSizeF()).IsEqualTo(drawingSizeF);
        await Assert.That(size.ToNativeSizeFloat()).IsEqualTo(size);
        await Assert.That(fromWindows).IsEqualTo(size);
        await Assert.That(fromDrawing).IsEqualTo(new(Six, Seven));
        await Assert.That(fromDrawingFloat).IsEqualTo(size);
        await Assert.That(fromNativeSize).IsEqualTo(new(Six, Seven));
        await Assert.That(drawingSize).IsEqualTo(new(Six, Seven));
        await Assert.That(size.ChangeWidth(EightAndHalfFloat)).IsEqualTo(new(EightAndHalfFloat, SevenAndThreeQuartersFloat));
        await Assert.That(size.ChangeHeight(NineAndHalfFloat)).IsEqualTo(new(SixAndQuarterFloat, NineAndHalfFloat));
        await Assert.That(size.Round()).IsEqualTo(new(Six, Eight));
    }

    /// <summary>Exercises NativeRect constructors, conversions, equality, deconstruction, and extensions.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeRect_ValueGeometryAndConversions_ReturnExpectedValuesAsync()
    {
        var rect = new NativeRect(new NativePoint(Ten, Twenty), new NativeSize(Thirty, Forty));
        var same = new NativeRect(Ten, Twenty, Thirty, Forty);
        var fromCorners = new NativeRect(new NativePoint(Ten, Twenty), new NativePoint(Forty, Sixty));
        var (location, size) = rect;
        Rectangle drawingRectangle = rect;
        RectangleF drawingRectangleF = rect;
        System.Windows.Rect windowsRect = rect;
        System.Windows.Int32Rect windowsIntRect = rect;
        NativeRect fromDrawing = drawingRectangle;
        NativeRect fromIntRect = windowsIntRect;

        await Assert.That(NativeRect.Empty.IsEmpty).IsTrue();
        await Assert.That(NativeRect.SizeOf).IsEqualTo(Marshal.SizeOf<NativeRect>());
        await Assert.That(rect == same).IsTrue();
        await Assert.That(rect != new NativeRect(Ten, Twenty, ThirtyOne, Forty)).IsTrue();
        await Assert.That(fromCorners).IsEqualTo(rect);
        await Assert.That(location).IsEqualTo(new(Ten, Twenty));
        await Assert.That(size).IsEqualTo(new(Thirty, Forty));
        await Assert.That(rect.Equals((object)drawingRectangle)).IsTrue();
        await Assert.That(rect.Equals((object)"not a rect")).IsFalse();
        await Assert.That(rect.GetHashCode()).IsEqualTo(same.GetHashCode());
        await Assert.That(rect.ToString()).IsEqualTo("{Left: 10; Top: 20; Width: 30; Height: 40;}");
        await Assert.That(rect.ToRectangle()).IsEqualTo(drawingRectangle);
        await Assert.That(rect.ToRectangleF()).IsEqualTo(drawingRectangleF);
        await Assert.That(rect.ToRect()).IsEqualTo(windowsRect);
        await Assert.That(rect.ToInt32Rect()).IsEqualTo(windowsIntRect);
        await Assert.That(rect.ToNativeRect()).IsEqualTo(rect);
        await Assert.That(fromDrawing).IsEqualTo(rect);
        await Assert.That(fromIntRect).IsEqualTo(rect);
        await Assert.That(rect.ChangeX(Eleven)).IsEqualTo(new(Eleven, Twenty, Thirty, Forty));
        await Assert.That(rect.ChangeY(TwentyOne)).IsEqualTo(new(Ten, TwentyOne, Thirty, Forty));
        await Assert.That(rect.ChangeWidth(ThirtyOne)).IsEqualTo(new(Ten, Twenty, ThirtyOne, Forty));
        await Assert.That(rect.ChangeHeight(FortyOne)).IsEqualTo(new(Ten, Twenty, Thirty, FortyOne));
        await Assert.That(rect.Offset()).IsEqualTo(rect);
        await Assert.That(rect.Offset(Five)).IsEqualTo(new(Fifteen, Twenty, Thirty, Forty));
        await Assert.That(rect.Offset(null, Five)).IsEqualTo(new(Ten, TwentyFive, Thirty, Forty));
        await Assert.That(rect.MoveTo()).IsEqualTo(rect);
        await Assert.That(rect.MoveTo(1)).IsEqualTo(new(1, Twenty, Thirty, Forty));
        await Assert.That(rect.MoveTo(null, Two)).IsEqualTo(new(Ten, Two, Thirty, Forty));
        await Assert.That(rect.Resize()).IsEqualTo(rect);
        await Assert.That(rect.Resize(1)).IsEqualTo(new(Ten, Twenty, 1, Forty));
        await Assert.That(rect.Resize(null, Two)).IsEqualTo(new(Ten, Twenty, Thirty, Two));
        await Assert.That(rect.Normalize()).IsEqualTo(rect);
        await Assert.That(new NativeRect(Forty, Sixty, -Thirty, -Forty).Normalize()).IsEqualTo(rect);
    }

    /// <summary>Exercises NativeRect relation helpers for contains, overlap, adjacency, and intersections.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeRect_RelationHelpers_ReturnExpectedValuesAsync()
    {
        var rect = new NativeRect(Ten, Ten, Twenty, Twenty);
        var contained = new NativeRect(Twelve, Twelve, Four, Four);
        var overlapping = new NativeRect(Twenty, Twenty, Twenty, Twenty);
        var containing = new NativeRect(0, 0, Forty, Forty);
        var rightAdjacent = new NativeRect(Thirty, Twelve, Ten, Ten);
        var bottomAdjacent = new NativeRect(Twelve, Thirty, Ten, Ten);
        var dockedLeft = new NativeRect(0, Ten, Nine, Twenty);
        var dockedRight = new NativeRect(ThirtyOne, Ten, Nine, Twenty);

        await Assert.That(rect.Contains(new NativePoint(Ten, Ten))).IsTrue();
        await Assert.That(rect.Contains(TwentyNine, TwentyNine)).IsTrue();
        await Assert.That(rect.Contains(Thirty, Thirty)).IsFalse();
        await Assert.That(rect.Contains(contained)).IsTrue();
        await Assert.That(rect.HasOverlap(overlapping)).IsTrue();
        await Assert.That(rect.HasOverlap(containing)).IsFalse();
        await Assert.That(rect.IsAdjacent(rightAdjacent)).IsEqualTo(AdjacentTo.Right);
        await Assert.That(rect.IsAdjacent(bottomAdjacent)).IsEqualTo(AdjacentTo.Bottom);
        await Assert.That(rect.IsAdjacent(new(Hundred, Hundred, 1, 1))).IsEqualTo(AdjacentTo.None);
        await Assert.That(dockedLeft.IsDockedToLeftOf(rect)).IsTrue();
        await Assert.That(dockedRight.IsDockedToRightOf(rect)).IsTrue();
        await Assert.That(rect.Union(new(0, 0, Five, Five))).IsEqualTo(new(0, 0, Thirty, Thirty));
        await Assert.That(rect.Intersect(overlapping)).IsEqualTo(new(Twenty, Twenty, Ten, Ten));
        await Assert.That(rect.Intersect(new(Hundred, Hundred, 1, 1))).IsEqualTo(NativeRect.Empty);
        await Assert.That(rect.Intersect2(overlapping)).IsEqualTo(new(Twenty, Ten, Ten, Thirty));
        await Assert.That(rect.IntersectsWith(overlapping)).IsTrue();
        await Assert.That(rect.IntersectsWith(new(Hundred, Hundred, 1, 1))).IsFalse();
        await Assert.That(rect.Inflate(Two, Three)).IsEqualTo(new(Eight, Seven, TwentyFour, TwentySix));
    }

    /// <summary>Exercises NativeRectFloat constructors, conversions, equality, deconstruction, and extensions.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeRectFloat_ValueGeometryAndConversions_ReturnExpectedValuesAsync()
    {
        var rect = new NativeRectFloat(new NativePointFloat(TenAndHalfFloat, TwentyAndHalfFloat), new NativeSizeFloat(ThirtyAndHalfFloat, FortyAndHalfFloat));
        var same = new NativeRectFloat(TenAndHalfFloat, TwentyAndHalfFloat, ThirtyAndHalfFloat, FortyAndHalfFloat);
        var fromCorners = new NativeRectFloat(new NativePointFloat(TenAndHalfFloat, TwentyAndHalfFloat), new NativePointFloat(FortyOneFloat, SixtyOneFloat));
        var (location, size) = rect;
        RectangleF drawingRectangleF = rect;
        Rectangle drawingRectangle = rect;
        System.Windows.Rect windowsRect = rect;
        System.Windows.Int32Rect windowsIntRect = rect;
        NativeRectFloat fromDrawingF = drawingRectangleF;
        NativeRectFloat fromDrawing = drawingRectangle;
        NativeRectFloat fromNativeRect = new NativeRect(Ten, Twenty, Thirty, Forty);
        NativeRectFloat fromWindowsRect = windowsRect;
        NativeRectFloat fromWindowsIntRect = windowsIntRect;

        await Assert.That(NativeRectFloat.Empty.IsEmpty).IsTrue();
        await Assert.That(rect == same).IsTrue();
        await Assert.That(rect != new NativeRectFloat(TenAndHalfFloat, TwentyAndHalfFloat, ThirtyOneAndHalfFloat, FortyAndHalfFloat)).IsTrue();
        await Assert.That(fromCorners).IsEqualTo(rect);
        await Assert.That(location).IsEqualTo(new(TenAndHalfFloat, TwentyAndHalfFloat));
        await Assert.That(size).IsEqualTo(new(ThirtyAndHalfFloat, FortyAndHalfFloat));
        await Assert.That(rect.Equals((object)drawingRectangleF)).IsTrue();
        await Assert.That(rect.Equals((object)windowsRect)).IsTrue();
        await Assert.That(rect.Equals((object)"not a rect")).IsFalse();
        await Assert.That(rect.GetHashCode()).IsEqualTo(same.GetHashCode());
        await Assert.That(rect.ToString()).IsEqualTo("{Left: 10.5; Top: 20.5; Width: 30.5; Height: 40.5;}");
        await Assert.That(rect.ToRectangleF()).IsEqualTo(drawingRectangleF);
        await Assert.That(rect.ToRectangle()).IsEqualTo(drawingRectangle);
        await Assert.That(rect.ToRect()).IsEqualTo(windowsRect);
        await Assert.That(rect.ToInt32Rect()).IsEqualTo(windowsIntRect);
        await Assert.That(rect.ToNativeRectFloat()).IsEqualTo(rect);
        await Assert.That(rect.ToNativeRect()).IsEqualTo(new(Ten, Twenty, Thirty, Forty));
        await Assert.That(fromDrawingF).IsEqualTo(rect);
        await Assert.That(fromDrawing).IsEqualTo(new(Ten, Twenty, Thirty, Forty));
        await Assert.That(fromNativeRect).IsEqualTo(new(Ten, Twenty, Thirty, Forty));
        await Assert.That(fromWindowsRect).IsEqualTo(rect);
        await Assert.That(fromWindowsIntRect).IsEqualTo(new(Ten, Twenty, Thirty, Forty));
        await Assert.That(rect.Contains(new(Eleven, TwentyOne))).IsTrue();
        await Assert.That(rect.ChangeX(ElevenAndHalfFloat)).IsEqualTo(new(ElevenAndHalfFloat, TwentyAndHalfFloat, ThirtyAndHalfFloat, FortyAndHalfFloat));
        await Assert.That(rect.ChangeY(TwentyOneAndHalfFloat)).IsEqualTo(new(TenAndHalfFloat, TwentyOneAndHalfFloat, ThirtyAndHalfFloat, FortyAndHalfFloat));
        await Assert.That(rect.ChangeWidth(ThirtyOneAndHalfFloat)).IsEqualTo(new(TenAndHalfFloat, TwentyAndHalfFloat, ThirtyOneAndHalfFloat, FortyAndHalfFloat));
        await Assert.That(rect.ChangeHeight(FortyOneAndHalfFloat)).IsEqualTo(new(TenAndHalfFloat, TwentyAndHalfFloat, ThirtyAndHalfFloat, FortyOneAndHalfFloat));
        await Assert.That(rect.Offset()).IsEqualTo(rect);
        await Assert.That(rect.Offset(OneAndQuarterFloat)).IsEqualTo(new(ElevenAndThreeQuartersFloat, TwentyAndHalfFloat, ThirtyAndHalfFloat, FortyAndHalfFloat));
        await Assert.That(rect.Offset(null, OneAndQuarterFloat)).IsEqualTo(new(TenAndHalfFloat, TwentyOneAndThreeQuartersFloat, ThirtyAndHalfFloat, FortyAndHalfFloat));
        await Assert.That(rect.MoveTo()).IsEqualTo(rect);
        await Assert.That(rect.MoveTo(OneAndQuarterFloat)).IsEqualTo(new(OneAndQuarterFloat, TwentyAndHalfFloat, ThirtyAndHalfFloat, FortyAndHalfFloat));
        await Assert.That(rect.MoveTo(null, TwoAndQuarterFloat)).IsEqualTo(new(TenAndHalfFloat, TwoAndQuarterFloat, ThirtyAndHalfFloat, FortyAndHalfFloat));
        await Assert.That(rect.Resize()).IsEqualTo(rect);
        await Assert.That(rect.Resize(OneAndQuarterFloat)).IsEqualTo(new(TenAndHalfFloat, TwentyAndHalfFloat, OneAndQuarterFloat, FortyAndHalfFloat));
        await Assert.That(rect.Resize(null, TwoAndQuarterFloat)).IsEqualTo(new(TenAndHalfFloat, TwentyAndHalfFloat, ThirtyAndHalfFloat, TwoAndQuarterFloat));
        await Assert.That(rect.Round()).IsEqualTo(new(Ten, Twenty, Thirty, Forty));
        await Assert.That(new NativeRectFloat(FortyOneFloat, SixtyOneFloat, -ThirtyAndHalfFloat, -FortyAndHalfFloat).Normalize()).IsEqualTo(rect);
    }

    /// <summary>Exercises NativeRectFloat relation helpers for contains, overlap, adjacency, and intersections.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeRectFloat_RelationHelpers_ReturnExpectedValuesAsync()
    {
        var rect = new NativeRectFloat(Ten, Ten, Twenty, Twenty);
        var contained = new NativeRectFloat(Twelve, Twelve, Four, Four);
        var overlapping = new NativeRectFloat(Twenty, Twenty, Twenty, Twenty);
        var containing = new NativeRectFloat(0, 0, Forty, Forty);
        var leftAdjacent = new NativeRectFloat(-Ten, Twelve, Twenty, Ten);
        var topAdjacent = new NativeRectFloat(Twelve, -Ten, Ten, Twenty);
        var dockedLeft = new NativeRectFloat(0, Ten, Nine, Twenty);
        var dockedRight = new NativeRectFloat(ThirtyOne, Ten, Nine, Twenty);

        await Assert.That(rect.Contains(new NativePointFloat(Ten, Ten))).IsTrue();
        await Assert.That(rect.Contains(TwentyNineAndHalfFloat, TwentyNineAndHalfFloat)).IsTrue();
        await Assert.That(rect.Contains(Thirty, Thirty)).IsFalse();
        await Assert.That(rect.Contains(contained)).IsTrue();
        await Assert.That(rect.HasOverlap(overlapping)).IsTrue();
        await Assert.That(rect.HasOverlap(containing)).IsFalse();
        await Assert.That(rect.IsAdjacent(leftAdjacent)).IsEqualTo(AdjacentTo.Left);
        await Assert.That(rect.IsAdjacent(topAdjacent)).IsEqualTo(AdjacentTo.Top);
        await Assert.That(rect.IsAdjacent(new(Hundred, Hundred, 1, 1))).IsEqualTo(AdjacentTo.None);
        await Assert.That(dockedLeft.IsDockedToLeftOf(rect)).IsTrue();
        await Assert.That(dockedRight.IsDockedToRightOf(rect)).IsTrue();
        await Assert.That(rect.Union(new(0, 0, Five, Five))).IsEqualTo(new(0, 0, Thirty, Thirty));
        await Assert.That(rect.Intersect(overlapping)).IsEqualTo(new(Twenty, Twenty, Ten, Ten));
        await Assert.That(rect.IntersectsWith(overlapping)).IsTrue();
        await Assert.That(rect.IntersectsWith(new(Hundred, Hundred, 1, 1))).IsFalse();
        await Assert.That(rect.Inflate(Two, Three)).IsEqualTo(new(Eight, Seven, TwentyFour, TwentySix));
        await Assert.That(rect.Inflate(TwoAndHalfFloat, ThreeAndHalfFloat)).IsEqualTo(new(SevenAndHalfFloat, SixAndHalfFloat, TwentyFive, TwentySeven));
        await Assert.That(rect.Inflate(new(OneAndHalfFloat, TwoAndHalfFloat))).IsEqualTo(new(EightAndHalfFloat, SevenAndHalfFloat, TwentyThree, TwentyFive));
    }

    /// <summary>Exercises Common type converters for successful and unsupported conversion paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TypeConverters_SupportedAndUnsupportedInputs_ReturnExpectedValuesAsync()
    {
        var nativePointConverter = TypeDescriptor.GetConverter(typeof(NativePoint));
        var nativePointFloatConverter = TypeDescriptor.GetConverter(typeof(NativePointFloat));
        var nativeSizeConverter = TypeDescriptor.GetConverter(typeof(NativeSize));
        var nativeSizeFloatConverter = TypeDescriptor.GetConverter(typeof(NativeSizeFloat));
        var nativeRectConverter = TypeDescriptor.GetConverter(typeof(NativeRect));
        var nativeRectFloatConverter = TypeDescriptor.GetConverter(typeof(NativeRectFloat));

        await Assert.That(nativePointConverter.CanConvertFrom(typeof(string))).IsTrue();
        await Assert.That(nativePointConverter.CanConvertTo(typeof(string))).IsTrue();
        await Assert.That(nativePointConverter.ConvertFromInvariantString("1,2")).IsEqualTo(new NativePoint(1, Two));
        await Assert.That(nativePointConverter.ConvertTo(null, CultureInfo.InvariantCulture, new NativePoint(1, Two), typeof(string))).IsEqualTo("1,2");
        await Assert.That(() => nativePointConverter.ConvertFromInvariantString("bad")).Throws<NotSupportedException>();
        await Assert.That(nativePointConverter.ConvertTo(null, CultureInfo.InvariantCulture, new(), typeof(string))).IsEqualTo("System.Object");

        await Assert.That(nativePointFloatConverter.ConvertFromInvariantString("1.5,2.5")).IsEqualTo(new NativePointFloat(OneAndHalfFloat, TwoAndHalfFloat));
        await Assert.That(nativePointFloatConverter.ConvertToInvariantString(new NativePointFloat(OneAndHalfFloat, TwoAndHalfFloat))).IsEqualTo("1.5,2.5");
        await Assert.That(() => nativePointFloatConverter.ConvertFromInvariantString("1.5,broken")).Throws<NotSupportedException>();

        await Assert.That(nativeSizeConverter.ConvertFromInvariantString("3,4")).IsEqualTo(new NativeSize(Three, Four));
        await Assert.That(nativeSizeConverter.ConvertToInvariantString(new NativeSize(Three, Four))).IsEqualTo("4,3");
        await Assert.That(() => nativeSizeConverter.ConvertFromInvariantString("3")).Throws<NotSupportedException>();

        await Assert.That(nativeSizeFloatConverter.ConvertFromInvariantString("3.5,4.5")).IsEqualTo(new NativeSizeFloat(ThreeAndHalfFloat, FourAndHalfFloat));
        await Assert.That(nativeSizeFloatConverter.ConvertToInvariantString(new NativeSizeFloat(ThreeAndHalfFloat, FourAndHalfFloat))).IsEqualTo("3.5,4.5");
        await Assert.That(() => nativeSizeFloatConverter.ConvertFromInvariantString("3.5,broken")).Throws<NotSupportedException>();

        await Assert.That(nativeRectConverter.ConvertFromInvariantString("1,2,3,4")).IsEqualTo(new NativeRect(1, Two, Three, Four));
        await Assert.That(nativeRectConverter.ConvertToInvariantString(new NativeRect(1, Two, Three, Four))).IsEqualTo("1,2,3,4");
        await Assert.That(() => nativeRectConverter.ConvertFromInvariantString("1,2,3")).Throws<NotSupportedException>();

        await Assert.That(nativeRectFloatConverter.ConvertFromInvariantString("1.5,2.5,3.5,4.5")).IsEqualTo(new NativeRectFloat(OneAndHalfFloat, TwoAndHalfFloat, ThreeAndHalfFloat, FourAndHalfFloat));
        await Assert.That(nativeRectFloatConverter.ConvertToInvariantString(new NativeRectFloat(OneAndHalfFloat, TwoAndHalfFloat, ThreeAndHalfFloat, FourAndHalfFloat))).IsEqualTo("1.5,2.5,3.5,4.5");
        await Assert.That(() => nativeRectFloatConverter.ConvertFromInvariantString("1.5,2.5,broken,4.5")).Throws<NotSupportedException>();
    }

    /// <summary>Exercises enum description and HRESULT success/failure helpers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task EnumAndHResultHelpers_ReturnExpectedValuesAsync()
    {
        await Assert.That(DescribedEnum.Value.GetEnumDescription()).IsEqualTo("Stored description");
        await Assert.That(DescribedEnum.NoDescription.GetEnumDescription()).IsNull();
        var attribute = (DescriptionAttribute)DescribedEnum.Value.GetAttributeOfType(typeof(DescriptionAttribute));
        await Assert.That(attribute.Description).IsEqualTo("Stored description");
        await Assert.That(HResult.Ok.Succeeded()).IsTrue();
        await Assert.That(HResult.False.Succeeded()).IsTrue();
        await Assert.That(HResult.Fail.Failed()).IsTrue();
        await Assert.That(HResult.AccessDenied.Failed()).IsTrue();
        HResult.Ok.ThrowOnFailure();
        await Assert.That(static () => HResult.NotImplemented.ThrowOnFailure()).Throws<Exception>();
    }

    /// <summary>Exercises pixel value semantics and alpha blending branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task PixelStructs_EqualityToStringAndBlendBranches_ReturnExpectedValuesAsync()
    {
        var bgr = new Bgr24(Ten, Twenty, Thirty);
        var sameBgr = new Bgr24(Ten, Twenty, Thirty);
        var differentBgr = new Bgr24(Eleven, Twenty, Thirty);
        var bgra = new Bgra32(Ten, Twenty, Thirty, Forty);
        var sameBgra = new Bgra32(Ten, Twenty, Thirty, Forty);
        var indexed = new Indexed8(Nine);

        await Assert.That(bgr == sameBgr).IsTrue();
        await Assert.That(bgr != differentBgr).IsTrue();
        await Assert.That(bgr.Equals((object)sameBgr)).IsTrue();
        await Assert.That(bgr.Equals((object)bgra)).IsFalse();
        await Assert.That(bgr.GetHashCode()).IsEqualTo(sameBgr.GetHashCode());
        await Assert.That(bgr.ToString()).IsEqualTo("Bgr24(10, 20, 30)");
        await Assert.That(bgra == sameBgra).IsTrue();
        await Assert.That(bgra != new Bgra32(Ten, Twenty, Thirty, FortyOne)).IsTrue();
        await Assert.That(bgra.Equals((object)sameBgra)).IsTrue();
        await Assert.That(bgra.Equals((object)bgr)).IsFalse();
        await Assert.That(bgra.GetHashCode()).IsEqualTo(sameBgra.GetHashCode());
        await Assert.That(bgra.ToString()).IsEqualTo("Bgra32(10, 20, 30, 40)");
        await Assert.That(indexed == new Indexed8(Nine)).IsTrue();
        await Assert.That(indexed != new Indexed8(Ten)).IsTrue();
        await Assert.That(indexed.Equals((object)new Indexed8(Nine))).IsTrue();
        await Assert.That(indexed.Equals((object)bgr)).IsFalse();
        await Assert.That(indexed.GetHashCode()).IsEqualTo(new Indexed8(Nine).GetHashCode());
        await Assert.That(indexed.ToString()).IsEqualTo("Indexed8(9)");

        var transparentTargetBgr = bgr;
        Bgr24.AlphaBlend(ref transparentTargetBgr, new(TwoHundred, Hundred, Fifty, 0));
        await Assert.That(transparentTargetBgr).IsEqualTo(bgr);
        var opaqueTargetBgr = bgr;
        Bgr24.AlphaBlend(ref opaqueTargetBgr, new(TwoHundred, Hundred, Fifty));
        await Assert.That(opaqueTargetBgr).IsEqualTo(new(TwoHundred, Hundred, Fifty));
        var blendedTargetBgr = new Bgr24(0, 0, 0);
        Bgr24.AlphaBlend(ref blendedTargetBgr, new(TwoHundredFiftyFive, OneHundredTwentySeven, 0, OneHundredTwentyEight));
        await Assert.That(blendedTargetBgr).IsEqualTo(new(OneHundredTwentyEight, SixtyThree, 0));

        var transparentTargetBgra = bgra;
        Bgra32.AlphaBlend(ref transparentTargetBgra, new(TwoHundred, Hundred, Fifty, 0));
        await Assert.That(transparentTargetBgra).IsEqualTo(bgra);
        var opaqueTargetBgra = bgra;
        var opaqueSource = new Bgra32(TwoHundred, Hundred, Fifty);
        Bgra32.AlphaBlend(ref opaqueTargetBgra, opaqueSource);
        await Assert.That(opaqueTargetBgra).IsEqualTo(opaqueSource);
        var blendedTargetBgra = new Bgra32(0, 0, 0, Ten);
        Bgra32.AlphaBlend(ref blendedTargetBgra, new(TwoHundredFiftyFive, OneHundredTwentySeven, 0, OneHundredTwentyEight));
        await Assert.That(blendedTargetBgra).IsEqualTo(new(OneHundredTwentyEight, SixtyThree, 0));
    }

    /// <summary>Exercises BitmapAccessor managed guard and extension branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapAccessor_ErrorBranchesAndExtensions_ReturnExpectedValuesAsync()
    {
        using var target = new Bitmap(Three, Two, PixelFormat.Format32bppArgb);
        using var source = new Bitmap(Three, Two, PixelFormat.Format32bppArgb);
        using var wrongSize = new Bitmap(Two, Two, PixelFormat.Format32bppArgb);
        var calledPair = false;
        var calledSingle = false;

        target.ProcessPixelRows<Bgra32>(source, (targetAccessor, sourceAccessor) =>
        {
            calledPair = true;
            targetAccessor.GetRowSpan(0)[0] = new(1, Two, Three, Four);
            _ = sourceAccessor.GetRowSpan(0)[0];
        });
        target.ProcessPixelRows<Bgra32>(accessor =>
        {
            calledSingle = true;
            accessor.ProcessRows(static (rowIndex, row) => row[Zero] = new((byte)rowIndex, Two, Three));
        });

        await Assert.That(calledPair).IsTrue();
        await Assert.That(calledSingle).IsTrue();
        await Assert.That(target.GetPixel(Zero, One).R).IsEqualTo((byte)One);
        await Assert.That(() => target.ProcessPixelRows<Bgra32>(wrongSize, static (_, _) => { })).Throws<ArgumentException>();
        await Assert.That(() => target.ProcessPixelRows((Action<BitmapAccessor<Bgra32>>)null)).Throws<ArgumentNullException>();
        await Assert.That(static () => ((Bitmap)null).ProcessPixelRows<Bgra32>(static _ => { })).Throws<ArgumentNullException>();
        await Assert.That(static () => new BitmapAccessor<Bgra32>(null)).Throws<ArgumentNullException>();
        await Assert.That(() => new BitmapAccessor<Indexed8>(target)).Throws<NotSupportedException>();

        using var accessor = new BitmapAccessor<Bgra32>(target, readOnly: true);
        await Assert.That(accessor.Width).IsEqualTo(Three);
        await Assert.That(accessor.Height).IsEqualTo(Two);
        await Assert.That(accessor.Stride).IsGreaterThanOrEqualTo(Twelve);
        await Assert.That(accessor.PixelFormat).IsEqualTo(PixelFormat.Format32bppArgb);
        await Assert.That(() => accessor.GetRowSpan(-1)).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => accessor.GetRowSpan(Two)).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => accessor.ProcessRows(null)).Throws<ArgumentNullException>();
        await Assert.That(() => _ = accessor.PaletteSpan).Throws<NotSupportedException>();
    }

    /// <summary>Exercises BitmapAccessor indexed palette read/write branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapAccessor_IndexedPalette_ReadWriteAndReadOnlyBranchesReturnExpectedValuesAsync()
    {
        using var writable = new Bitmap(Two, Two, PixelFormat.Format8bppIndexed);
        var writablePalette = writable.Palette;
        writablePalette.Entries[1] = Color.FromArgb(TwoHundredFiftyFive, 1, Two, Three);
        writable.Palette = writablePalette;

        Bgra32 originalPaletteValue;
        using (var accessor = new BitmapAccessor<Indexed8>(writable, readOnly: false))
        {
            var palette = accessor.PaletteSpan;
            originalPaletteValue = palette[One];
            palette[One] = new(Nine, Eight, Seven, Six);
            accessor.GetRowSpan(Zero)[Zero] = new(One);
        }

        await Assert.That(originalPaletteValue).IsEqualTo(new(One, Two, Three));
        await Assert.That(writable.Palette.Entries[One]).IsEqualTo(Color.FromArgb(Six, Nine, Eight, Seven));

        using var readOnly = new Bitmap(Two, Two, PixelFormat.Format8bppIndexed);
        var readOnlyPalette = readOnly.Palette;
        readOnlyPalette.Entries[1] = Color.FromArgb(TwoHundredFiftyFive, 1, Two, Three);
        readOnly.Palette = readOnlyPalette;

        using (var accessor = new BitmapAccessor<Indexed8>(readOnly, readOnly: true))
        {
            accessor.PaletteSpan[One] = new(Nine, Eight, Seven, Six);
        }

        await Assert.That(readOnly.Palette.Entries[One]).IsEqualTo(Color.FromArgb(Opaque, One, Two, Three));
    }
}
