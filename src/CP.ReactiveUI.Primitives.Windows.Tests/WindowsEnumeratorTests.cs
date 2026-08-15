// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Windows Enumerator Tests behavior.</summary>
public class WindowsEnumeratorTests
{
    /// <summary>Defines the TestValue400 test value.</summary>
    private const int MaxFindAttempts = 10;

    /// <summary>Defines the TestValue10 test value.</summary>
    private const int TestValue10 = 10;

    /// <summary>Tests Enumerate Windows Sync.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task EnumerateWindowsSyncAsync()
    {
        var count = 0;
        foreach (var unused in WindowsEnumerator.EnumerateWindows((IInteropWindow)null))
        {
            count++;
        }

        await Assert.That(count > 0).IsTrue();
    }

    /// <summary>Tests Enumerate Window Handles Sync.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task EnumerateWindowHandlesSyncAsync()
    {
        var count = 0;
        foreach (var unused in WindowsEnumerator.EnumerateWindowHandles((IInteropWindow)null))
        {
            count++;
        }

        await Assert.That(count > 0).IsTrue();
    }

    /// <summary>Tests Enumerate Windows Take10.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task EnumerateWindows_Take10Async()
    {
        var count = 0;
        foreach (var unused in WindowsEnumerator.EnumerateWindows((IInteropWindow)null))
        {
            count++;
            if (count == TestValue10)
            {
                break;
            }
        }

        await Assert.That(count == TestValue10).IsTrue();
    }

    /// <summary>Tests Enumerate Windows.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task EnumerateWindowsAsync()
    {
        var windows = WindowsEnumerator.ObserveWindows();
        await Assert.That(windows).IsNotNull();
    }

    /// <summary>Tests Enumerate Window Handles.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task EnumerateWindowHandlesAsync()
    {
        var windows = WindowsEnumerator.ObserveWindowHandles();
        await Assert.That(windows).IsNotNull();
    }

    /// <summary>Tests Enumerate Windows Async Find.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task EnumerateWindowsAsync_FindAsync()
    {
        using var form = new Form { Text = Guid.NewGuid().ToString(), TopLevel = true, };
        form.Show();

        // Important, otherwise Windows doesn't have time to display the window!
        Application.DoEvents();

        var windowFound = false;
        for (var attempt = 0; attempt < MaxFindAttempts && !windowFound; attempt++)
        {
            Application.DoEvents();
            foreach (var info in WindowsEnumerator.EnumerateWindows((IInteropWindow)null))
            {
                if (info.Handle == form.Handle)
                {
                    windowFound = true;
                    break;
                }
            }
        }

        await Assert.That(windowFound).IsTrue();
    }

    /// <summary>Tests Enumerate Windows Async Take10.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public Task EnumerateWindowsAsync_Take10Async() => EnumerateWindows_Take10Async();
}
