// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Net.Sockets;
using CP.ReactiveUI.Primitives.Windows.Integrations.Browser;
using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Covers deterministic branch edges in optional Windows integrations.</summary>
public sealed class CoverageFinalIntegrationBranchTests
{
    /// <summary>The first string branch value.</summary>
    private const string FirstText = "first";

    /// <summary>The second string branch value.</summary>
    private const string SecondText = "second";

    /// <summary>The third string branch value.</summary>
    private const string ThirdText = "third";

    /// <summary>A different string value for false equality branches.</summary>
    private const string DifferentText = "different";

    /// <summary>An unsigned eleven value.</summary>
    private const uint UIntElevenValue = 11U;

    /// <summary>An unsigned twenty-one value.</summary>
    private const uint UIntTwentyOneValue = 21U;

    /// <summary>Verifies the registry reader fallback branch without reading the Windows Registry.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsInternetExplorerRegistry_ReadValue_ReturnsFallbackForMissingKeyAsync() =>
        await Assert.That(InternetExplorerVersion.WindowsInternetExplorerRegistry.ReadValue(null, "missing")).IsEqualTo("0");

    /// <summary>Verifies AppInfo equality and null-backed hash branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AppInfo_BranchEdges_AreCoveredAsync()
    {
        var value = new AppInfo(FirstText, SecondText, ThirdText);

        await Assert.That(value.Equals(new(DifferentText, SecondText, ThirdText))).IsFalse();
        await Assert.That(value.Equals(new(FirstText, DifferentText, ThirdText))).IsFalse();
        await Assert.That(value.Equals(new(FirstText, SecondText, DifferentText))).IsFalse();
        await Assert.That(new AppInfo(null, null, null).GetHashCode()).IsEqualTo(new AppInfo(null, null, null).GetHashCode());
    }

    /// <summary>Verifies ClientAddress equality short-circuit branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClientAddress_BranchEdges_AreCoveredAsync()
    {
        var address = CreateAddressBytes();
        var value = new ClientAddress(AddressFamily.InterNetwork, address);

        await Assert.That(value.Equals(new(AddressFamily.InterNetworkV6, address))).IsFalse();
        address[Five] = TwentyTwo;
        await Assert.That(value.Equals(new(AddressFamily.InterNetwork, address))).IsFalse();
    }

    /// <summary>Verifies ClientDisplay equality short-circuit branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClientDisplay_BranchEdges_AreCoveredAsync()
    {
        var value = new ClientDisplay(UIntTen, UIntTwenty, UIntThirty);

        await Assert.That(value.Equals(new(UIntElevenValue, UIntTwenty, UIntThirty))).IsFalse();
        await Assert.That(value.Equals(new(UIntTen, UIntTwentyOneValue, UIntThirty))).IsFalse();
        await Assert.That(value.Equals(new(UIntTen, UIntTwenty, UIntThirtyTwo))).IsFalse();
    }

    /// <summary>Verifies ClientLatency equality short-circuit branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClientLatency_BranchEdges_AreCoveredAsync()
    {
        var value = new ClientLatency(UIntTen, UIntTwenty, UIntThirty);

        await Assert.That(value.Equals(new(UIntElevenValue, UIntTwenty, UIntThirty))).IsFalse();
        await Assert.That(value.Equals(new(UIntTen, UIntTwentyOneValue, UIntThirty))).IsFalse();
        await Assert.That(value.Equals(new(UIntTen, UIntTwenty, UIntThirtyTwo))).IsFalse();
    }

    /// <summary>Verifies OsVersionInfo service pack null branch.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task OsVersionInfo_BranchEdges_AreCoveredAsync()
    {
        var value = new OsVersionInfo(One, Two, Three, Four, Five, null);

        await Assert.That(value.ServicePackVersion).IsEqualTo(string.Empty);
        await Assert.That(value.ToString()).IsEqualTo("2.3.4|5|");
    }

    /// <summary>Verifies SessionTime equality short-circuit branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SessionTime_BranchEdges_AreCoveredAsync()
    {
        var value = new SessionTime(OneAndHalfDouble, TwoAndHalfDouble, ThreeAndHalfDouble, FourAndHalfDouble, FiveAndHalfDouble);

        await Assert.That(value.Equals(new(TwoAndHalfDouble, TwoAndHalfDouble, ThreeAndHalfDouble, FourAndHalfDouble, FiveAndHalfDouble))).IsFalse();
        await Assert.That(value.Equals(new(OneAndHalfDouble, ThreeAndHalfDouble, ThreeAndHalfDouble, FourAndHalfDouble, FiveAndHalfDouble))).IsFalse();
        await Assert.That(value.Equals(new(OneAndHalfDouble, TwoAndHalfDouble, FourAndHalfDouble, FourAndHalfDouble, FiveAndHalfDouble))).IsFalse();
        await Assert.That(value.Equals(new(OneAndHalfDouble, TwoAndHalfDouble, ThreeAndHalfDouble, FiveAndHalfDouble, FiveAndHalfDouble))).IsFalse();
        await Assert.That(value.Equals(new(OneAndHalfDouble, TwoAndHalfDouble, ThreeAndHalfDouble, FourAndHalfDouble, SixAndHalfDouble))).IsFalse();
    }

    /// <summary>Verifies UserInfo equality and null-backed hash branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task UserInfo_BranchEdges_AreCoveredAsync()
    {
        var value = new UserInfo(FirstText, SecondText, ThirdText);

        await Assert.That(value.Equals(new(DifferentText, SecondText, ThirdText))).IsFalse();
        await Assert.That(value.Equals(new(FirstText, DifferentText, ThirdText))).IsFalse();
        await Assert.That(value.Equals(new(FirstText, SecondText, DifferentText))).IsFalse();
        await Assert.That(new UserInfo(null, null, null).GetHashCode()).IsEqualTo(new UserInfo(null, null, null).GetHashCode());
    }

    /// <summary>Creates a Citrix client address buffer.</summary>
    /// <returns>The address bytes.</returns>
    private static byte[] CreateAddressBytes()
    {
        var address = new byte[Twenty];
        address[Two] = OneHundredNinetyTwo;
        address[Three] = OneHundredSixtyEight;
        address[Four] = Ten;
        address[Five] = TwentyOne;
        return address;
    }
}
