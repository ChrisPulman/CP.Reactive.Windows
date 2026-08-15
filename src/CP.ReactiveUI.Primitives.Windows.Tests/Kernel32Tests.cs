// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Kernel;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Kernel32 Tests behavior.</summary>
public class Kernel32Tests
{
    /// <summary>Tests Is Running As Uwp.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_IsRunningAsUwpAsync() => await Assert.That(PackageInfo.IsRunningOnUwp).IsFalse();

    /// <summary>Tests Get Os Version Info Ex.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_GetOsVersionInfoExAsync()
    {
        var versionInfo = OsVersionInfoEx.Create();
        await Assert.That(Kernel32Api.GetVersionEx(ref versionInfo)).IsTrue();
    }
}
