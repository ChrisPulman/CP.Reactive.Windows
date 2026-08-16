// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides deterministic final coverage for Core COM and Shell interop composition.</summary>
public sealed class CoverageFinalCoreInteropTailTests
{
    /// <summary>Defines a deterministic shell operation result.</summary>
    private const int ShellOperationResult = 23;

    /// <summary>Exercises both inherited COM-attribute lookup outcomes.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ComProgIdAttribute_InheritedLookup_HandlesPresentAndMissingAttributesAsync()
    {
        var present = ComProgIdAttribute.GetAttribute(typeof(IFinalAttributedComContractChild));
        var missing = ComProgIdAttribute.GetAttribute(typeof(IFinalUnattributedComContractChild));

        await Assert.That(present.Value).IsEqualTo("CP.Reactive.Windows.Inherited");
        await Assert.That(missing).IsNull();
    }

    /// <summary>Exercises null and non-null path pinning through a composed shell operation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Shell32Api_ShellFileInfo_PinsBothPathShapesWithoutNativeCallsAsync()
    {
        var paths = new List<IntPtr>();
        IntPtr expectedResult = new(ShellOperationResult);
        using var scope = Shell32Api.OverrideShellFileInfoOperationForTesting(
            (path, _, _, _, _) =>
            {
                paths.Add(path);
                return expectedResult;
            });

        var nullPathInfo = default(ShellFileInfo);
        var populatedPathInfo = default(ShellFileInfo);
        IntPtr nullPathResult = Shell32Api.SHGetFileInfo(
            null!,
            default,
            ref nullPathInfo,
            0U,
            default);
        IntPtr populatedPathResult = Shell32Api.SHGetFileInfo(
            "deterministic.txt",
            default,
            ref populatedPathInfo,
            0U,
            default);

        await Assert.That(nullPathResult).IsEqualTo(expectedResult);
        await Assert.That(populatedPathResult).IsEqualTo(expectedResult);
        await Assert.That(paths[0]).IsEqualTo(IntPtr.Zero);
        await Assert.That(paths[1]).IsNotEqualTo(IntPtr.Zero);
    }
}
