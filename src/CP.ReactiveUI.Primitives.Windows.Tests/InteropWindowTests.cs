// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Interop Window Tests behavior.</summary>
public class InteropWindowTests
{
    /// <summary>The test form height.</summary>
    private const int TestFormHeight = 240;

    /// <summary>The test form width.</summary>
    private const int TestFormWidth = 320;

    /// <summary>Test some of the InteropWindowQuery logic by finding the taskbar and the clock on it.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestTaskbarInfoAsync()
    {
        IInteropWindow systray = null;
        foreach (var window in InteropWindowQueryExtensions.GetTopWindows())
        {
            if (window.GetClassname() == "Shell_TrayWnd")
            {
                systray = window;
                break;
            }
        }

        await Assert.That(systray).IsNotNull();
        var taskbarInfo = systray.GetInfo();
        await Assert.That(taskbarInfo.ClientBounds.Width * taskbarInfo.ClientBounds.Height > 0).IsTrue();

        // The clock is not a direct taskbar child on every supported Explorer version.
        IInteropWindow clock = null;
        foreach (var child in systray.GetChildren())
        {
            if (child.GetClassname() == "TrayClockWClass")
            {
                clock = child;
                break;
            }
        }

        if (clock is not null)
        {
            var info = clock.GetInfo();
            await Assert.That(info.ClientBounds.Width * info.ClientBounds.Height > 0).IsTrue();
        }
    }

    /// <summary>Tests Get Info With Parent Crop.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_GetInfo_WithParentCropAsync()
    {
        using var form = new Form { Width = TestFormWidth, Height = TestFormHeight, };
        form.Show();
        System.Windows.Forms.Application.DoEvents();

        var testWindow = InteropWindowFactory.CreateFor(form.Handle);
        var info = testWindow.GetInfo();
        await Assert.That(info.Bounds.Width > 0).IsTrue();
        await Assert.That(info.Bounds.Height > 0).IsTrue();
    }
}
