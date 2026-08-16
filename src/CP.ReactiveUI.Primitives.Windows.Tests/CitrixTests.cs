// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Citrix Tests behavior.</summary>
public class CitrixTests
{
    /// <summary>Assume that we are not running on Citrix.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestCitrix_NotAvailableAsync() => await Assert.That(WinFrame.IsAvailabe).IsFalse();
}
