// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Media.Imaging;
using CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests App Window Tests behavior.</summary>
public class AppWindowTests
{
    /// <summary>Writes diagnostic messages for these tests.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(AppWindowTests));

    /// <summary>All apps we find, should give true when IsApp and have a logo.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestAppWindowListAsync()
    {
        foreach (var interopWindow in AppQueryExtensions.WindowsStoreApps)
        {
            Log.DebugFormat("{0} - {1}", interopWindow.GetCaption(), interopWindow.GetClassname());
            await Assert.That(interopWindow.IsApp()).IsTrue();
            _ = interopWindow.GetIcon(default(BitmapSource));
        }
    }

    /// <summary>Ensures the top-level window query returns only valid native window handles.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestTopLevelWindowsAsync()
    {
        var allHandlesAreValid = true;
        foreach (var window in InteropWindowQueryExtensions.GetTopLevelWindows())
        {
            if (window.Handle == IntPtr.Zero)
            {
                allHandlesAreValid = false;
                break;
            }
        }

        await Assert.That(allHandlesAreValid).IsTrue();
    }
}
