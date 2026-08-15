// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Dwm Test behavior.</summary>
public class DwmTest
{
    /// <summary>Test is Dwm is Enabled.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestDwmEnabledAsync() => await Assert.That(DwmApi.IsDwmEnabled).IsTrue();
}
