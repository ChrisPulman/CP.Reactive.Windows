// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests display behavior.</summary>
public class DisplayTest
{
    /// <summary>Defines the display-change wait duration in milliseconds.</summary>
    private const int DisplayChangeWaitMilliseconds = 100;

    /// <summary>Writes diagnostic messages for these tests.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(DisplayTest));

    /// <summary>Defines the Screenbounds All Screens test value.</summary>
    private readonly NativeRect _screenboundsAllScreens;

    /// <summary>Initializes a new instance of the <see cref="DisplayTest"/> class.</summary>
    public DisplayTest()
    {
        TestLogging.UseConsoleLogger();
        _screenboundsAllScreens = GetScreenBoundsAllScreens();
    }

    /// <summary>Tests All Displays.</summary>
    [Test]
    public void TestAllDisplays()
    {
        foreach (var display in DisplayTopology.GetSnapshot())
        {
            Log.DebugFormat("Index {0} - Primary {3} - Device {1} - Bounds: {2}", display.Index, display.DeviceName, display.Bounds, display.IsPrimary);
        }
    }

    /// <summary>Tests Get Bounds.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestGetBoundsAsync()
    {
        var displayInfo = DisplayTopology.GetSnapshot()[0];
        var displayBounds = DisplayTopology.GetBounds(displayInfo.Bounds.Location);
        await Assert.That(displayBounds).IsEqualTo(displayInfo.Bounds);
    }

    /// <summary>Tests Screenbounds.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestScreenboundsAsync()
    {
        var screenboundsDisplayInfo = DisplayTopology.ScreenBounds;

        await Assert.That(screenboundsDisplayInfo).IsEqualTo(_screenboundsAllScreens);
    }

    /// <summary>Verifies the cached screen bounds remain stable without a display-change notification.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestScreenboundsSubscriptionAsync()
    {
        var screenboundsDisplayInfoBefore = DisplayTopology.ScreenBounds;
        TestAllDisplays();

        await Task.Delay(DisplayChangeWaitMilliseconds);
        var screenboundsDisplayInfoAfter = DisplayTopology.ScreenBounds;
        TestAllDisplays();

        await Assert.That(screenboundsDisplayInfoAfter).IsEqualTo(screenboundsDisplayInfoBefore);
    }

    /// <summary>
    ///     Get the bounds of all screens combined, via build in Screen.AllScreens.
    ///     This has issues when running with alternative DPI settings.
    /// </summary>
    /// <returns>A NativeRect of the bounds of the entire display area.</returns>
    private static NativeRect GetScreenBoundsAllScreens()
    {
        var left = 0;
        var top = 0;
        var bottom = 0;
        var right = 0;
        foreach (var screen in Screen.AllScreens)
        {
            left = Math.Min(left, screen.Bounds.X);
            top = Math.Min(top, screen.Bounds.Y);
            var screenAbsRight = screen.Bounds.X + screen.Bounds.Width;
            var screenAbsBottom = screen.Bounds.Y + screen.Bounds.Height;
            right = Math.Max(right, screenAbsRight);
            bottom = Math.Max(bottom, screenAbsBottom);
        }

        return new(left, top, right + Math.Abs(left), bottom + Math.Abs(top));
    }
}
