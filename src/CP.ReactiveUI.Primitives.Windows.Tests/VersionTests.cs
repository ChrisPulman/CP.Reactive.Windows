// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Kernel;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Version Tests behavior.</summary>
public class VersionTests
{
    /// <summary>Defines the TestValue6 test value.</summary>
    private const int TestValue6 = 6;

    /// <summary>Test GetVersionEx.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestOsVersionInfoExAsync()
    {
        var versionInfo = OsVersionInfoEx.Create();
        await Assert.That(Kernel32Api.GetVersionEx(ref versionInfo)).IsTrue();
        await Assert.That(versionInfo.MajorVersion >= TestValue6).IsTrue();
    }
}
