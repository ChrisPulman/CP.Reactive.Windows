// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides deterministic coverage for Core user-interface interop structs.</summary>
public sealed class CoreUserInterfaceStructCoverageTests
{
    /// <summary>The test blur radius.</summary>
    private const float TestBlurRadius = 2.5F;

    /// <summary>The different test blur radius.</summary>
    private const float DifferentBlurRadius = 3.5F;

    /// <summary>Verifies created title-bar values and index branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TitleBarInfoExCoversCreatedValueAndIndexBranchesAsync()
    {
        var value = TitleBarInfoEx.Create();
        var equal = TitleBarInfoEx.Create();
        var defaultValue = default(TitleBarInfoEx);

        await Assert.That(value.Bounds).IsEqualTo(NativeRect.Empty);
        await Assert.That(value.ElementState(TitleBarInfoIndexes.TitleBar)).IsEqualTo(ObjectStates.None);
        await Assert.That(value.ElementState(TitleBarInfoIndexes.Reserved)).IsEqualTo(ObjectStates.None);
        await Assert.That(value.ElementState(TitleBarInfoIndexes.MinimizeButton)).IsEqualTo(ObjectStates.None);
        await Assert.That(value.ElementState(TitleBarInfoIndexes.MaximizeButton)).IsEqualTo(ObjectStates.None);
        await Assert.That(value.ElementState(TitleBarInfoIndexes.HelpButton)).IsEqualTo(ObjectStates.None);
        await Assert.That(value.ElementState(TitleBarInfoIndexes.CloseButton)).IsEqualTo(ObjectStates.None);
        await Assert.That(value.ElementBounds(TitleBarInfoIndexes.TitleBar)).IsEqualTo(NativeRect.Empty);
        await Assert.That(value.ElementBounds(TitleBarInfoIndexes.Reserved)).IsEqualTo(NativeRect.Empty);
        await Assert.That(value.ElementBounds(TitleBarInfoIndexes.MinimizeButton)).IsEqualTo(NativeRect.Empty);
        await Assert.That(value.ElementBounds(TitleBarInfoIndexes.MaximizeButton)).IsEqualTo(NativeRect.Empty);
        await Assert.That(value.ElementBounds(TitleBarInfoIndexes.HelpButton)).IsEqualTo(NativeRect.Empty);
        await Assert.That(value.ElementBounds(TitleBarInfoIndexes.CloseButton)).IsEqualTo(NativeRect.Empty);
        await Assert.That(value.Equals(equal)).IsTrue();
        await Assert.That(value.Equals((object)equal)).IsTrue();
        await Assert.That(value.Equals("title")).IsFalse();
        await Assert.That(value == equal).IsTrue();
        await Assert.That(value != defaultValue).IsTrue();
        await Assert.That(value.GetHashCode()).IsEqualTo(0);
        await Assert.That(() => value.ElementState((TitleBarInfoIndexes)int.MaxValue)).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => value.ElementBounds((TitleBarInfoIndexes)int.MaxValue)).Throws<ArgumentOutOfRangeException>();
    }

    /// <summary>Verifies created scroll-bar values and equality branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ScrollBarInfoCoversCreatedValueAndEqualityBranchesAsync()
    {
        var value = ScrollBarInfo.Create();
        var equal = ScrollBarInfo.Create();
        var defaultValue = default(ScrollBarInfo);

        await Assert.That(value.Bounds).IsEqualTo(default(NativeRect));
        await Assert.That(value.ThumbSize).IsEqualTo(0);
        await Assert.That(value.ThumbBottom).IsEqualTo(0);
        await Assert.That(value.ThumbTop).IsEqualTo(0);
        await Assert.That(value.States).IsEquivalentTo(
        [
            ObjectStates.None,
            ObjectStates.None,
            ObjectStates.None,
            ObjectStates.None,
            ObjectStates.None,
            ObjectStates.None
        ]);
        await Assert.That(value.ToString()).Contains("ThumbSize = 0");
        await Assert.That(value.Equals(equal)).IsTrue();
        await Assert.That(value.Equals((object)equal)).IsTrue();
        await Assert.That(value.Equals("scroll")).IsFalse();
        await Assert.That(value == equal).IsTrue();
        await Assert.That(value != defaultValue).IsTrue();
        await Assert.That(value.GetHashCode()).IsEqualTo(0);
    }

    /// <summary>Verifies animation factory and equality branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AnimationInfoCoversFactoryAndEqualityBranchesAsync()
    {
        var enabled = AnimationInfo.Create();
        var explicitEnabled = AnimationInfo.Create(true);
        var disabled = AnimationInfo.Create(false);

        await Assert.That(enabled.Equals(explicitEnabled)).IsTrue();
        await Assert.That(enabled.Equals((object)explicitEnabled)).IsTrue();
        await Assert.That(enabled.Equals(disabled)).IsFalse();
        await Assert.That(enabled.Equals("animation")).IsFalse();
        await Assert.That(enabled == explicitEnabled).IsTrue();
        await Assert.That(enabled != disabled).IsTrue();
        await Assert.That(enabled.GetHashCode()).IsEqualTo(0);
    }

    /// <summary>Verifies blur parameter factory and equality branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BlurParamsCoversFactoryAndEqualityBranchesAsync()
    {
        var blur = BlurParams.Create(TestBlurRadius, true);
        var equal = BlurParams.Create(TestBlurRadius, true);
        var different = BlurParams.Create(DifferentBlurRadius, false);

        await Assert.That(blur.Equals(equal)).IsTrue();
        await Assert.That(blur.Equals((object)equal)).IsTrue();
        await Assert.That(blur.Equals(different)).IsFalse();
        await Assert.That(blur.Equals("blur")).IsFalse();
        await Assert.That(blur == equal).IsTrue();
        await Assert.That(blur != different).IsTrue();
        await Assert.That(blur.GetHashCode()).IsEqualTo(typeof(BlurParams).GetHashCode());
    }
}
