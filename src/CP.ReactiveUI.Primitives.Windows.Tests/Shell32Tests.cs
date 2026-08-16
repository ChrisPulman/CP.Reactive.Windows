// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Shell;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Shell32 Tests behavior.</summary>
public class Shell32Tests
{
    /// <summary>Test AppBarr.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestAppBarAsync()
    {
        var appBarData = Shell32Api.TaskbarPosition;
        await Assert.That(appBarData.Bounds.IsEmpty).IsFalse();
    }
}
