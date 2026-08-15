// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Interop;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests User32 Tests behavior.</summary>
public class User32Tests
{
    /// <summary>Defines the TestValue100 test value.</summary>
    private const int TestValue100 = 100;

    /// <summary>Defines the TestValue200 test value.</summary>
    private const int TestValue200 = 200;

    /// <summary>Defines the TestValue10 test value.</summary>
    private const int TestValue10 = 10;

    /// <summary>Writes diagnostic messages for these tests.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(User32Tests));

    /// <summary>Tests Get Classname.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestGetClassnameAsync()
    {
        var desktopHandle = InteropWindowQueryExtensions.GetDesktopWindow();

        var classname = User32Api.GetClassname(desktopHandle.Handle);
        await Assert.That(classname).IsEqualTo("#32769");
    }

    /// <summary>Test GetWindow.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestGetTopLevelWindowsAsync()
    {
        var foundWindow = false;
        foreach (var window in InteropWindowQueryExtensions.GetTopWindows())
        {
            if (window.IsVisible())
            {
                foundWindow = true;
                Log.DebugFormat("{0}", window.Dump());
            }
        }

        await Assert.That(foundWindow).IsTrue();
    }

    /// <summary>Test WindowPlacement_TypeConverter.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestWindowPlacement_TypeConverterAsync()
    {
        var windowPlacement = WindowPlacement.Create();
        windowPlacement.MinPosition = new(TestValue10, TestValue10);
        windowPlacement.MaxPosition = new(TestValue100, TestValue100);
        windowPlacement.NormalPosition = new(TestValue100, TestValue100, TestValue200, TestValue200);
        windowPlacement.ShowCmd = ShowWindowCommands.Normal;

        var typeConverter = TypeDescriptor.GetConverter(typeof(WindowPlacement));
        await Assert.That(typeConverter).IsNotNull();
        var stringRepresentation = typeConverter.ConvertToInvariantString(windowPlacement);
        await Assert.That(stringRepresentation).IsEqualTo("Normal|10,10|100,100|100,100,200,200");
        var windowPlacementResult = (WindowPlacement?)typeConverter.ConvertFromInvariantString(stringRepresentation);
        await Assert.That(windowPlacementResult.HasValue).IsTrue();
        if (windowPlacementResult.HasValue)
        {
            await Assert.That(windowPlacementResult.Value).IsEqualTo(windowPlacement);
        }
    }

    /// <summary>Test GetTextFromWindow.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_GetTextFromWindowAsync()
    {
        const string title = "1234567890";
        var window = new Window { Title = title, };

        string text;
        try
        {
            window.Show();
            var handle = new WindowInteropHelper(window).Handle;
            text = User32Api.GetTextFromWindow(handle);
        }
        finally
        {
            window.Close();
        }

        await Assert.That(text).IsEqualTo(title);
    }
}
