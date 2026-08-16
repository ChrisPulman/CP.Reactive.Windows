// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests for the NativeRect struct, and it's extensions.</summary>
public class RectTests
{
    /// <summary>Defines the TestValue100 test value.</summary>
    private const int TestValue100 = 100;

    /// <summary>Defines the TestValue200 test value.</summary>
    private const int TestValue200 = 200;

    /// <summary>Defines the TestValue110 test value.</summary>
    private const int TestValue110 = 110;

    /// <summary>Defines the TestValue120 test value.</summary>
    private const int TestValue120 = 120;

    /// <summary>Defines the TestValue220 test value.</summary>
    private const int TestValue220 = 220;

    /// <summary>Defines the TestValue240 test value.</summary>
    private const int TestValue240 = 240;

    /// <summary>Defines the TestValue10 test value.</summary>
    private const int TestValue10 = 10;

    /// <summary>Defines the TestValue20 test value.</summary>
    private const int TestValue20 = 20;

    /// <summary>Defines the TestValue50 test value.</summary>
    private const int TestValue50 = 50;

    /// <summary>Defines the TestValue90 test value.</summary>
    private const int TestValue90 = 90;

    /// <summary>Tests Ctor L R Native Size.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Ctor_L_R_NativeSizeAsync()
    {
        var rect = new NativeRect(TestValue10, TestValue20, new NativeSize(TestValue100, TestValue200));
        await Assert.That(rect.Left).IsEqualTo(TestValue10);
        await Assert.That(rect.Top).IsEqualTo(TestValue20);
        await Assert.That(rect.Right).IsEqualTo(TestValue100 + TestValue10);
        await Assert.That(rect.Bottom).IsEqualTo(TestValue200 + TestValue20);
        await Assert.That(rect.Width).IsEqualTo(TestValue100);
        await Assert.That(rect.Height).IsEqualTo(TestValue200);
        await Assert.That(rect.Location).IsEqualTo(new(TestValue10, TestValue20));
        await Assert.That(rect.Size).IsEqualTo(new(TestValue100, TestValue200));
    }

    /// <summary>Tests Ctor L R T B.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Ctor_L_R_T_BAsync()
    {
        var rect = new NativeRect(TestValue10, TestValue20, TestValue100, TestValue200);
        await Assert.That(rect.Left).IsEqualTo(TestValue10);
        await Assert.That(rect.Top).IsEqualTo(TestValue20);
        await Assert.That(rect.Right).IsEqualTo(TestValue110);
        await Assert.That(rect.Bottom).IsEqualTo(TestValue220);
        await Assert.That(rect.Width).IsEqualTo(TestValue100);
        await Assert.That(rect.Height).IsEqualTo(TestValue200);
        await Assert.That(rect.Location).IsEqualTo(new(TestValue10, TestValue20));
        await Assert.That(rect.Size).IsEqualTo(new(TestValue100, TestValue200));
    }

    /// <summary>Tests Ctor Native Point Native Size.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Ctor_NativePoint_NativeSizeAsync()
    {
        var rect = new NativeRect(new NativePoint(TestValue10, TestValue20), new NativeSize(TestValue100, TestValue200));
        await Assert.That(rect.Left).IsEqualTo(TestValue10);
        await Assert.That(rect.Top).IsEqualTo(TestValue20);
        await Assert.That(rect.Right).IsEqualTo(TestValue100 + TestValue10);
        await Assert.That(rect.Bottom).IsEqualTo(TestValue200 + TestValue20);
        await Assert.That(rect.Width).IsEqualTo(TestValue100);
        await Assert.That(rect.Height).IsEqualTo(TestValue200);
        await Assert.That(rect.Location).IsEqualTo(new(TestValue10, TestValue20));
        await Assert.That(rect.Size).IsEqualTo(new(TestValue100, TestValue200));
    }

    /// <summary>Tests Native Rect Casts.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeRectCastsAsync()
    {
        var nativeRect = new NativeRect(new NativePoint(TestValue10, TestValue20), new NativeSize(TestValue100, TestValue200));
        Rectangle rectangle = nativeRect;
        await Assert.That((NativeRect)rectangle).IsEqualTo(nativeRect);
        await Assert.That((Rectangle)nativeRect).IsEqualTo(rectangle);

        Int32Rect rect = nativeRect;
        await Assert.That((NativeRect)rect).IsEqualTo(nativeRect);
        await Assert.That((Int32Rect)nativeRect).IsEqualTo(rect);
    }

    /// <summary>Tests Native Rect Extensions.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeRectExtensionsAsync()
    {
        var nativeRect = new NativeRect(new NativePoint(TestValue10, TestValue20), new NativeSize(TestValue100, TestValue200));
        Rectangle rectangle = nativeRect;

        var rectangleWithXyOffset = rectangle;
        rectangleWithXyOffset.Offset(TestValue10, TestValue10);
        await Assert.That((Rectangle)nativeRect.Offset(TestValue10, TestValue10)).IsEqualTo(rectangleWithXyOffset);

        var rectangleWithChangedSize = rectangle;
        rectangleWithChangedSize.Size = new(TestValue50, TestValue50);
        await Assert.That((Rectangle)nativeRect.Resize(TestValue50, TestValue50)).IsEqualTo(rectangleWithChangedSize);

        var pointOffset = new Point(TestValue10, TestValue10);
        var rectangleWithPointOffset = rectangle;
        rectangleWithPointOffset.Offset(pointOffset);
        await Assert.That((Rectangle)nativeRect.Offset(pointOffset)).IsEqualTo(rectangleWithPointOffset);

        var rectangleInflated = rectangle;
        rectangleInflated.Inflate(TestValue10, TestValue10);
        await Assert.That((Rectangle)nativeRect.Inflate(TestValue10, TestValue10)).IsEqualTo(rectangleInflated);

        var rectangleChangedX = rectangle;
        rectangleChangedX.X = TestValue110;
        await Assert.That((Rectangle)nativeRect.ChangeX(TestValue110)).IsEqualTo(rectangleChangedX);

        var rectangleChangedY = rectangle;
        rectangleChangedY.Y = TestValue110;
        await Assert.That((Rectangle)nativeRect.ChangeY(TestValue110)).IsEqualTo(rectangleChangedY);

        var rectangleChangedWidth = rectangle;
        rectangleChangedWidth.Width = TestValue110;
        await Assert.That((Rectangle)nativeRect.ChangeWidth(TestValue110)).IsEqualTo(rectangleChangedWidth);

        var rectangleChangedHeight = rectangle;
        rectangleChangedHeight.Height = TestValue110;
        await Assert.That((Rectangle)nativeRect.ChangeHeight(TestValue110)).IsEqualTo(rectangleChangedHeight);
    }

    /// <summary>Tests Is Adjacent.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IsAdjacentAsync()
    {
        const int width = TestValue100;
        const int height = TestValue100;

        const int left = TestValue200;
        const int top = TestValue200;

        var rect1 = new NativeRect(new NativePoint(left, top), new NativeSize(width, height));
        var rect2 = new NativeRect(new NativePoint(left - width, top), new NativeSize(width, height));
        await Assert.That(rect1.IsAdjacent(rect2)).IsEqualTo(AdjacentTo.Left);

        rect2 = new(new NativePoint(left + width, top), new NativeSize(width, height));
        await Assert.That(rect1.IsAdjacent(rect2)).IsEqualTo(AdjacentTo.Right);

        rect2 = new(new NativePoint(left, top + height), new NativeSize(width, height));
        await Assert.That(rect1.IsAdjacent(rect2)).IsEqualTo(AdjacentTo.Bottom);

        rect2 = new(new NativePoint(left, top - height), new NativeSize(width, height));
        await Assert.That(rect1.IsAdjacent(rect2)).IsEqualTo(AdjacentTo.Top);
    }

    /// <summary>Tests Is Docked.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IsDockedAsync()
    {
        const int width = TestValue100;
        const int height = TestValue100;

        const int left = TestValue200;
        const int top = TestValue200;

        var rect1 = new NativeRect(new NativePoint(left, top), new NativeSize(width, height));
        var rect2 = new NativeRect(new NativePoint(left - width - 1, top), new NativeSize(width, height));
        await Assert.That(rect2.IsDockedToLeftOf(rect1)).IsTrue();
        await Assert.That(rect2.IsDockedToRightOf(rect1)).IsFalse();

        rect2 = new(new NativePoint(left + width + 1, top), new NativeSize(width, height));
        await Assert.That(rect2.IsDockedToRightOf(rect1)).IsTrue();
        await Assert.That(rect2.IsDockedToLeftOf(rect1)).IsFalse();
    }

    /// <summary>Tests Normalize.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NormalizeAsync()
    {
        const int width = -TestValue100;
        const int height = -TestValue100;

        const int left = TestValue200;
        const int top = TestValue200;

        var unnormalized = new NativeRect(new NativePoint(left, top), new NativeSize(width, height));
        Rectangle normalized = new(TestValue100, TestValue100, TestValue100, TestValue100);
        await Assert.That((Rectangle)unnormalized.Normalize()).IsEqualTo(normalized);
    }

    /// <summary>Tests Intersect.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IntersectAsync()
    {
        var rect1 = new NativeRect(TestValue100, TestValue100, new NativeSize(TestValue100, TestValue100));
        var rect2 = new NativeRect(TestValue90, TestValue90, new NativeSize(TestValue20, TestValue20));
        var rect3 = rect1.Intersect(rect2);

        await Assert.That(rect1.IntersectsWith(rect2)).IsTrue();
        var expected = new NativeRect(TestValue100, TestValue100, new NativeSize(TestValue10, TestValue10));
        await Assert.That(rect3).IsEqualTo(expected);
    }

    /// <summary>Tests Union.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task UnionAsync()
    {
        var rect1 = new NativeRect(TestValue100, TestValue100, new NativeSize(TestValue100, TestValue100));
        var rect2 = new NativeRect(TestValue90, TestValue90, new NativeSize(TestValue20, TestValue20));
        var rect3 = rect1.Union(rect2);

        var expected = new NativeRect(TestValue90, TestValue90, new NativeSize(TestValue110, TestValue110));
        await Assert.That(rect3).IsEqualTo(expected);
    }

    /// <summary>Verifies the integer and floating-point surfaces delegate to equivalent shared geometry.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SharedGeometryAlgorithmsAsync()
    {
        var integerRectangle = new NativeRect(TestValue10, TestValue20, TestValue100, TestValue200);
        var floatingRectangle = new NativeRectFloat(TestValue10, TestValue20, TestValue100, TestValue200);
        var integerAdjacent = new NativeRect(TestValue110, TestValue20, TestValue10, TestValue200);
        var floatingAdjacent = new NativeRectFloat(TestValue110, TestValue20, TestValue10, TestValue200);
        var integerOverlapping = new NativeRect(TestValue90, TestValue90, TestValue20, TestValue20);
        var floatingOverlapping = new NativeRectFloat(TestValue90, TestValue90, TestValue20, TestValue20);

        await Assert.That(integerRectangle.Contains(TestValue10, TestValue20)).IsTrue();
        await Assert.That(floatingRectangle.Contains(TestValue10, TestValue20)).IsTrue();
        await Assert.That(integerRectangle.Contains(integerRectangle.Right, integerRectangle.Top)).IsFalse();
        await Assert.That(floatingRectangle.Contains(floatingRectangle.Right, floatingRectangle.Top)).IsFalse();
        await Assert.That(integerRectangle.IsAdjacent(integerAdjacent)).IsEqualTo(AdjacentTo.Right);
        await Assert.That(floatingRectangle.IsAdjacent(floatingAdjacent)).IsEqualTo(AdjacentTo.Right);
        await Assert.That(integerRectangle.HasOverlap(integerAdjacent)).IsTrue();
        await Assert.That(floatingRectangle.HasOverlap(floatingAdjacent)).IsTrue();
        await Assert.That(integerRectangle.IntersectsWith(integerOverlapping)).IsTrue();
        await Assert.That(floatingRectangle.IntersectsWith(floatingOverlapping)).IsTrue();
        await Assert.That(integerRectangle.Union(integerOverlapping)).IsEqualTo(new(TestValue10, TestValue20, TestValue100, TestValue200));
        await Assert.That(floatingRectangle.Union(floatingOverlapping)).IsEqualTo(new(TestValue10, TestValue20, TestValue100, TestValue200));
        await Assert.That(integerRectangle.Inflate(TestValue10, TestValue20)).IsEqualTo(new(0, 0, TestValue120, TestValue240));
        await Assert.That(floatingRectangle.Inflate(TestValue10, TestValue20)).IsEqualTo(new(0, 0, TestValue120, TestValue240));
        await Assert.That(integerRectangle.ToRect()).IsEqualTo(new(TestValue10, TestValue20, TestValue100, TestValue200));
        await Assert.That(floatingRectangle.ToRect()).IsEqualTo(new(TestValue10, TestValue20, TestValue100, TestValue200));
    }

    /// <summary>Verifies both rectangle surfaces retain their two-corner matrix transformation behavior.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SharedGeometryTransformAsync()
    {
        var matrix = System.Windows.Media.Matrix.Identity;
        matrix.Translate(TestValue10, TestValue20);

        await Assert.That(new NativeRect(0, 0, TestValue100, TestValue200).Transform(matrix)).IsEqualTo(new(TestValue10, TestValue20, TestValue100, TestValue200));
        await Assert.That(new NativeRectFloat(0, 0, TestValue100, TestValue200).Transform(matrix)).IsEqualTo(new(TestValue10, TestValue20, TestValue100, TestValue200));
    }
}
