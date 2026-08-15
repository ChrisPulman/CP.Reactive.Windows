// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Common Struct Tests behavior.</summary>
public class CommonStructTests
{
    /// <summary>Defines the TestFloat123Point1 test value.</summary>
    private const float TestFloat123Point1 = 123.1F;

    /// <summary>Defines the TestFloat456Point2 test value.</summary>
    private const float TestFloat456Point2 = 456.2F;

    /// <summary>Defines the TestFloat457Point3 test value.</summary>
    private const float TestFloat457Point3 = 457.3F;

    /// <summary>Defines the TestFloat876Point4 test value.</summary>
    private const float TestFloat876Point4 = 876.4F;

    /// <summary>Defines the TestFloat20Point0 test value.</summary>
    private const float TestFloat20Point0 = 20.0F;

    /// <summary>Defines the TestFloat40Point0 test value.</summary>
    private const float TestFloat40Point0 = 40.0F;

    /// <summary>Defines the TestFloat30Point0 test value.</summary>
    private const float TestFloat30Point0 = 30.0F;

    /// <summary>Defines the TestFloat10Point0 test value.</summary>
    private const float TestFloat10Point0 = 10.0F;

    /// <summary>Defines the TestFloat10Point5 test value.</summary>
    private const float TestFloat10Point5 = 10.5F;

    /// <summary>Defines the TestFloat20Point5 test value.</summary>
    private const float TestFloat20Point5 = 20.5F;

    /// <summary>Defines the TestFloat30Point5 test value.</summary>
    private const float TestFloat30Point5 = 30.5F;

    /// <summary>Defines the TestFloat40Point5 test value.</summary>
    private const float TestFloat40Point5 = 40.5F;

    /// <summary>Defines the TestDouble30Point0 test value.</summary>
    private const double TestDouble30Point0 = 30.0;

    /// <summary>Defines the TestDouble40Point0 test value.</summary>
    private const double TestDouble40Point0 = 40.0;

    /// <summary>Defines the TestDouble40Point5 test value.</summary>
    private const double TestDouble40Point5 = 40.5;

    /// <summary>Defines the TestDouble10Point0 test value.</summary>
    private const double TestDouble10Point0 = 10.0;

    /// <summary>Defines the TestDouble10Point5 test value.</summary>
    private const double TestDouble10Point5 = 10.5;

    /// <summary>Defines the TestDouble20Point5 test value.</summary>
    private const double TestDouble20Point5 = 20.5;

    /// <summary>Defines the TestDouble30Point5 test value.</summary>
    private const double TestDouble30Point5 = 30.5;

    /// <summary>Defines the TestDouble20Point0 test value.</summary>
    private const double TestDouble20Point0 = 20.0;

    /// <summary>Defines the TestValue123 test value.</summary>
    private const int TestValue123 = 123;

    /// <summary>Defines the TestValue200 test value.</summary>
    private const int TestValue200 = 200;

    /// <summary>Defines the TestValue150 test value.</summary>
    private const int TestValue150 = 150;

    /// <summary>Defines the TestValue100 test value.</summary>
    private const int TestValue100 = 100;

    /// <summary>Defines the TestValue876 test value.</summary>
    private const int TestValue876 = 876;

    /// <summary>Defines the TestValue457 test value.</summary>
    private const int TestValue457 = 457;

    /// <summary>Defines the TestValue456 test value.</summary>
    private const int TestValue456 = 456;

    /// <summary>Defines the TestValue30 test value.</summary>
    private const int TestValue30 = 30;

    /// <summary>Defines the TestValue20 test value.</summary>
    private const int TestValue20 = 20;

    /// <summary>Defines the TestValue60 test value.</summary>
    private const int TestValue60 = 60;

    /// <summary>Defines the TestValue40 test value.</summary>
    private const int TestValue40 = 40;

    /// <summary>Defines the TestValue10 test value.</summary>
    private const int TestValue10 = 10;

    /// <summary>Test NativeRect Properties.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_NativeRect_PropertiesAsync()
    {
        const int left = 100;
        const int top = TestValue200;
        const int width = 110;
        const int right = left + width;
        const int height = 120;
        const int bottom = top + height;

        var nativeRect = new NativeRect(left, top, new NativeSize(width, height));
        await Assert.That(nativeRect.X).IsEqualTo(left);
        await Assert.That(nativeRect.Y).IsEqualTo(top);
        await Assert.That(nativeRect.Left).IsEqualTo(left);
        await Assert.That(nativeRect.Top).IsEqualTo(top);

        await Assert.That(nativeRect.Width).IsEqualTo(width);
        await Assert.That(nativeRect.Height).IsEqualTo(height);

        await Assert.That(nativeRect.Right).IsEqualTo(right);
        await Assert.That(nativeRect.Bottom).IsEqualTo(bottom);

        await Assert.That(nativeRect.TopLeft).IsEqualTo(new(left, top));
        await Assert.That(nativeRect.BottomLeft).IsEqualTo(new(left, bottom));
        await Assert.That(nativeRect.TopRight).IsEqualTo(new(right, top));
        await Assert.That(nativeRect.BottomRight).IsEqualTo(new(right, bottom));

        await Assert.That(nativeRect.Size).IsEqualTo(new(width, height));
    }

    /// <summary>Test NativePoint TypeConverter.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_NativePoint_TypeConverterAsync()
    {
        var nativePoint = new NativePoint(TestValue123, TestValue456);

        var typeConverter = TypeDescriptor.GetConverter(typeof(NativePoint));
        await Assert.That(typeConverter).IsNotNull();
        var stringRepresentation = typeConverter.ConvertToInvariantString(nativePoint);
        await Assert.That(stringRepresentation).IsEqualTo("123,456");
        var nativePointResult = (NativePoint?)typeConverter.ConvertFromInvariantString(stringRepresentation);
        await Assert.That(nativePointResult.HasValue).IsTrue();
        if (nativePointResult.HasValue)
        {
            await Assert.That(nativePointResult.Value).IsEqualTo(nativePoint);
        }
    }

    /// <summary>Test NativeSize operators.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_NativeSize_OperatorsAsync()
    {
        var nativeSize1 = new NativeSize(TestValue123, TestValue456);
        var nativeSize2 = new NativeSize(TestValue123, TestValue456);
        var drawingSize = new System.Drawing.Size(TestValue123, TestValue456);
        var windowsSize = new System.Windows.Size(TestValue123, TestValue456);
        var drawingSizeNotEqual = new System.Drawing.Size(TestValue456, TestValue123);
        var windowsSizeNotEqual = new System.Windows.Size(TestValue456, TestValue123);
        await Assert.That(nativeSize1 == nativeSize2).IsTrue();
        await Assert.That(nativeSize1 == drawingSize).IsTrue();
        await Assert.That(drawingSize == nativeSize1).IsTrue();
        await Assert.That(nativeSize1 != drawingSize).IsFalse();
        await Assert.That(drawingSize != nativeSize1).IsFalse();
        await Assert.That(drawingSizeNotEqual != nativeSize1).IsTrue();

        await Assert.That(nativeSize1 == windowsSize).IsTrue();
        await Assert.That(windowsSize == nativeSize1).IsTrue();
        await Assert.That(nativeSize1 != windowsSize).IsFalse();
        await Assert.That(windowsSize != nativeSize1).IsFalse();
        await Assert.That(windowsSizeNotEqual != nativeSize1).IsTrue();
    }

    /// <summary>Test NativeSize operators.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_NativeSizeFloat_OperatorsAsync()
    {
        var nativeSize1 = new NativeSizeFloat(TestValue123, TestValue456);
        var nativeSize2 = new NativeSizeFloat(TestValue123, TestValue456);
        var drawingSize = new System.Drawing.Size(TestValue123, TestValue456);
        var windowsSize = new System.Windows.Size(TestValue123, TestValue456);
        var drawingSizeNotEqual = new System.Drawing.Size(TestValue456, TestValue123);
        var windowsSizeNotEqual = new System.Windows.Size(TestValue456, TestValue123);
        await Assert.That(nativeSize1 == nativeSize2).IsTrue();
        await Assert.That(nativeSize1 == drawingSize).IsTrue();
        await Assert.That(drawingSize == nativeSize1).IsTrue();
        await Assert.That(nativeSize1 != drawingSize).IsFalse();
        await Assert.That(drawingSize != nativeSize1).IsFalse();
        await Assert.That(drawingSizeNotEqual != nativeSize1).IsTrue();

        await Assert.That(nativeSize1 == windowsSize).IsTrue();
        await Assert.That(windowsSize == nativeSize1).IsTrue();
        await Assert.That(nativeSize1 != windowsSize).IsFalse();
        await Assert.That(windowsSize != nativeSize1).IsFalse();
        await Assert.That(windowsSizeNotEqual != nativeSize1).IsTrue();
    }

    /// <summary>Test NativeRect TypeConverter.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_NativeRect_TypeConverterAsync()
    {
        var nativeRect = new NativeRect(TestValue123, TestValue456, TestValue457, TestValue876);

        var typeConverter = TypeDescriptor.GetConverter(typeof(NativeRect));
        await Assert.That(typeConverter).IsNotNull();
        var stringRepresentation = typeConverter.ConvertToInvariantString(nativeRect);
        await Assert.That(stringRepresentation).IsEqualTo("123,456,457,876");
        var nativePointResult = (NativeRect?)typeConverter.ConvertFromInvariantString(stringRepresentation);
        await Assert.That(nativePointResult.HasValue).IsTrue();
        if (nativePointResult.HasValue)
        {
            await Assert.That(nativePointResult.Value).IsEqualTo(nativeRect);
        }
    }

    /// <summary>Test NativeRectFloat TypeConverter.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_NativeRectFloat_TypeConverterAsync()
    {
        var nativeRect = new NativeRectFloat(TestFloat123Point1, TestFloat456Point2, TestFloat457Point3, TestFloat876Point4);

        var typeConverter = TypeDescriptor.GetConverter(typeof(NativeRectFloat));
        await Assert.That(typeConverter).IsNotNull();
        var stringRepresentation = typeConverter.ConvertToInvariantString(nativeRect);
        await Assert.That(stringRepresentation).IsEqualTo("123.1,456.2,457.3,876.4");
        var nativePointResult = (NativeRectFloat?)typeConverter.ConvertFromInvariantString(stringRepresentation);
        await Assert.That(nativePointResult.HasValue).IsTrue();
        if (nativePointResult.HasValue)
        {
            await Assert.That(nativePointResult.Value).IsEqualTo(nativeRect);
        }
    }

    /// <summary>Test NativeRect Transform.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_NativeRect_TransformAsync()
    {
        const int offsetX = TestValue20;
        const int offsetY = TestValue30;
        var nativeRectBefore = new NativeRect(0, 0, new NativeSize(TestValue200, TestValue60));
        var nativeRectAfter = new NativeRect(offsetX, offsetY, new NativeSize(TestValue60, TestValue200));
        var myMatrix = new System.Windows.Media.Matrix(0, 1, 1, 0, offsetX, offsetY);

        await Assert.That(nativeRectBefore.Transform(myMatrix)).IsEqualTo(nativeRectAfter);
    }

    /// <summary>Test implicit conversion for NativeRectFloat.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_NativeRectFloat_ImplicitConversionAsync()
    {
        var nativeRectFloat = new NativeRectFloat(TestFloat10Point5, TestFloat20Point5, TestFloat30Point5, TestFloat40Point5);

        var nativeRect = new NativeRect(TestValue10, TestValue20, TestValue30, TestValue40);
        var nativeRectFloatExpected1 = new NativeRectFloat(TestValue10, TestValue20, TestValue30, TestValue40);
        NativeRectFloat nativeRectFloatConverted1 = nativeRect;
        await Assert.That(nativeRectFloatConverted1).IsEqualTo(nativeRectFloatExpected1);

        var nativeRectExpected = new NativeRect(TestValue10, TestValue20, TestValue30, TestValue40);
        NativeRect nativeRectConverted = nativeRectFloat;
        await Assert.That(nativeRectConverted).IsEqualTo(nativeRectExpected);

#if !NETSTANDARD2_0
        var rect = new Rect(TestDouble10Point0, TestDouble20Point0, TestDouble30Point0, TestDouble40Point0);
        var nativeRectFloatExpected2 = new NativeRectFloat(TestFloat10Point0, TestFloat20Point0, TestFloat30Point0, TestFloat40Point0);
        NativeRectFloat nativeRectFloatConverted2 = rect;
        await Assert.That(nativeRectFloatConverted2).IsEqualTo(nativeRectFloatExpected2);

        var rectExpected = new Rect(TestDouble10Point5, TestDouble20Point5, TestDouble30Point5, TestDouble40Point5);
        Rect rectConverted = nativeRectFloat;
        await Assert.That(rectConverted).IsEqualTo(rectExpected);

        var int32Rect = new Int32Rect(TestValue10, TestValue20, TestValue30, TestValue40);
        var nativeRectFloatExpected3 = new NativeRectFloat(TestValue10, TestValue20, TestValue30, TestValue40);
        NativeRectFloat nativeRectFloatConverted3 = int32Rect;
        await Assert.That(nativeRectFloatConverted3).IsEqualTo(nativeRectFloatExpected3);

        var int32RectExpected = new Int32Rect(TestValue10, TestValue20, TestValue30, TestValue40);
        Int32Rect int32RectConverted = nativeRectFloat;
        await Assert.That(int32RectConverted).IsEqualTo(int32RectExpected);
#endif

        var rectangle = new Rectangle(TestValue10, TestValue20, TestValue30, TestValue40);
        var nativeRectFloatExpected4 = new NativeRectFloat(TestValue10, TestValue20, TestValue30, TestValue40);
        NativeRectFloat nativeRectFloatConverted4 = rectangle;
        await Assert.That(nativeRectFloatConverted4).IsEqualTo(nativeRectFloatExpected4);

        var rectangleExpected = new Rectangle(TestValue10, TestValue20, TestValue30, TestValue40);
        Rectangle rectangleConverted = nativeRectFloat;
        await Assert.That(rectangleConverted).IsEqualTo(rectangleExpected);

        var rectangleF = new RectangleF(TestFloat10Point5, TestFloat20Point5, TestFloat30Point5, TestFloat40Point5);
        var nativeRectFloatExpected5 = new NativeRectFloat(TestFloat10Point5, TestFloat20Point5, TestFloat30Point5, TestFloat40Point5);
        NativeRectFloat nativeRectFloatConverted5 = rectangleF;
        await Assert.That(nativeRectFloatConverted5).IsEqualTo(nativeRectFloatExpected5);

        var rectangleFExpected = new RectangleF(TestFloat10Point5, TestFloat20Point5, TestFloat30Point5, TestFloat40Point5);
        RectangleF rectangleFConverted = nativeRectFloat;
        await Assert.That(rectangleFConverted).IsEqualTo(rectangleFExpected);
    }

    /// <summary>Test inflate for NativeRect.</summary>
    /// <param name="x">The x test value.</param>
    /// <param name="y">The y test value.</param>
    /// <param name="width">The width test value.</param>
    /// <param name="height">The height test value.</param>
    /// <param name="inflateX">The inflate x test value.</param>
    /// <param name="inflateY">The inflate y test value.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    [Arguments(TestValue10, TestValue10, TestValue40, TestValue40, TestValue10, TestValue10)]
    [Arguments(TestValue200, TestValue100, -TestValue40, -TestValue20, TestValue10, TestValue20)]
    [Arguments(TestValue100, TestValue200, -TestValue20, -TestValue40, TestValue20, TestValue10)]
    [Arguments(TestValue100, TestValue100, TestValue40, TestValue40, -TestValue10, TestValue10)]
    [Arguments(TestValue100, TestValue100, TestValue40, TestValue40, TestValue10, -TestValue10)]
    public async Task Test_NativeRect_InflateAsync(int x, int y, int width, int height, int inflateX, int inflateY)
    {
        var nativeRect = new NativeRect(x, y, width, height);
        var nativeSize = new NativeSize(inflateX, inflateY);
        Rectangle rectangle = nativeRect;
        var nativeRectInflated = nativeRect.Inflate(inflateX, inflateY);
        var nativeRectInflatedWithSize = nativeRect.Inflate(nativeSize);
        await Assert.That(nativeRectInflatedWithSize).IsEqualTo(nativeRectInflated);
        rectangle.Inflate(inflateX, inflateY);
        await Assert.That(nativeRectInflated).IsNotEqualTo(nativeRect);
        await Assert.That((Rectangle)nativeRectInflated).IsEqualTo(rectangle);
    }

    /// <summary>Test union for NativeRect.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_NativeRect_UnionAsync()
    {
        var testCases = new[]
        {
            (First: new NativeRect(TestValue10, TestValue10, TestValue40, TestValue40), Second: new NativeRect(TestValue20, TestValue20, TestValue10, TestValue10)),
            (First: new NativeRect(TestValue150, TestValue150, TestValue100, TestValue100), Second: new NativeRect(TestValue100, TestValue100, TestValue100, TestValue100)),
        };

        foreach (var (First, Second) in testCases)
        {
            Rectangle unionNativeRect = First.Union(Second);
            var unionRectangle = Rectangle.Union(First, Second);

            await Assert.That(unionNativeRect).IsEqualTo(unionRectangle);
        }
    }

    /// <summary>Test union for NativeRect.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_NativeRect_IntersectAsync()
    {
        var testCases = new[]
        {
            (First: new NativeRect(TestValue10, TestValue10, TestValue40, TestValue40), Second: new NativeRect(TestValue20, TestValue20, TestValue10, TestValue10)),
            (First: new NativeRect(TestValue150, TestValue150, TestValue100, TestValue100), Second: new NativeRect(TestValue100, TestValue100, TestValue100, TestValue100)),
        };

        foreach (var (First, Second) in testCases)
        {
            Rectangle intersectNativeRect = First.Intersect(Second);
            var intersectRectangle = Rectangle.Intersect(First, Second);

            await Assert.That(intersectNativeRect).IsEqualTo(intersectRectangle);
        }
    }
}
