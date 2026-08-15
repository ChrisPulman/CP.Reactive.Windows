// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Net.Sockets;
using CP.ReactiveUI.Primitives.Windows.Integrations.Browser;
using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix;
using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Enums;
using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Structs;
using CP.ReactiveUI.Primitives.Windows.Interop.Com;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Coverage for optional Citrix and embedded browser integrations.</summary>
public sealed class IntegrationCoverageTests
{
    /// <summary>Defines the OLE command identifier raised when a script error is displayed.</summary>
    private const int OleCmdDidShowScriptError = 40;

    /// <summary>Defines the successful command result.</summary>
    private const int Ok = 0;

    /// <summary>Defines the unsupported OLE command result.</summary>
    private const int OleCmmdErrENotsupported = -2_147_221_248;

    /// <summary>Defines the browser emulation fallback value.</summary>
    private const int BrowserEmulationFallback = 7000;

    /// <summary>Defines the IE compatibility multiplier.</summary>
    private const int BrowserCompatibilityMultiplier = 1111;

    /// <summary>Defines the IE doctype multiplier.</summary>
    private const int BrowserDoctypeMultiplier = 1000;

    /// <summary>Defines the application name test value.</summary>
    private const string AppInfoApplicationName = "Published App";

    /// <summary>Defines the initial program test value.</summary>
    private const string AppInfoInitialProgram = "program.exe";

    /// <summary>Defines the working directory test value.</summary>
    private const string AppInfoWorkingDirectory = @"C:\Work";

    /// <summary>Defines the client directory test value.</summary>
    private const string ClientInfoDirectory = @"C:\ICA";

    /// <summary>Defines the client name test value.</summary>
    private const string ClientInfoName = "client";

    /// <summary>Defines the service pack test value.</summary>
    private const string ServicePackVersion = "Service Pack Test";

    /// <summary>Defines the connection name test value.</summary>
    private const string UserInfoConnectionName = "ica-tcp";

    /// <summary>Defines the domain name test value.</summary>
    private const string UserInfoDomainName = "domain";

    /// <summary>The document host command handler group.</summary>
    private static readonly Guid CGID_DocHostCommandHandler = new("F38BC242-B950-11D1-8918-00C04FC2C836");

    /// <summary>Tests AppInfo constructor, properties, equality, and string output.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AppInfo_ValueBehavior_IsCoveredAsync()
    {
        var value = new AppInfo(AppInfoInitialProgram, AppInfoWorkingDirectory, AppInfoApplicationName);
        var equal = new AppInfo(AppInfoInitialProgram, AppInfoWorkingDirectory, AppInfoApplicationName);
        var different = new AppInfo("other.exe", AppInfoWorkingDirectory, AppInfoApplicationName);

        await Assert.That(value.InitialProgram).IsEqualTo(AppInfoInitialProgram);
        await Assert.That(value.WorkingDirectory).IsEqualTo(AppInfoWorkingDirectory);
        await Assert.That(value.ApplicationName).IsEqualTo(AppInfoApplicationName);
        await Assert.That(value.Equals((object)equal)).IsTrue();
        await Assert.That(value.Equals("not app info")).IsFalse();
        await Assert.That(value == equal).IsTrue();
        await Assert.That(value != different).IsTrue();
        await Assert.That(value.GetHashCode()).IsEqualTo(equal.GetHashCode());
        await Assert.That(value.ToString()).IsEqualTo(@"Published App|program.exe|C:\Work");
    }

    /// <summary>Tests ClientAddress constructor, properties, equality, and string output.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClientAddress_ValueBehavior_IsCoveredAsync()
    {
        var address = new byte[20];
        address[2] = OneHundredNinetyTwo;
        address[3] = OneHundredSixtyEight;
        address[4] = Ten;
        address[5] = TwentyFive;
        var value = new ClientAddress(AddressFamily.InterNetwork, address);
        var equal = new ClientAddress(AddressFamily.InterNetwork, address);
        address[5] = TwentySix;
        var different = new ClientAddress(AddressFamily.InterNetwork, address);

        await Assert.That(value.AddressFamily).IsEqualTo(AddressFamily.InterNetwork);
        await Assert.That(value.IpAddress).IsEqualTo("192.168.10.25");
        await Assert.That(value.Equals((object)equal)).IsTrue();
        await Assert.That(value.Equals("not address")).IsFalse();
        await Assert.That(value == equal).IsTrue();
        await Assert.That(value != different).IsTrue();
        await Assert.That(value.GetHashCode()).IsEqualTo(equal.GetHashCode());
        await Assert.That(value.ToString()).IsEqualTo("InterNetwork|192.168.10.25");
        await Assert.That(static () => new ClientAddress(AddressFamily.InterNetwork, [1, Two, Three])).Throws<ArgumentException>();
    }

    /// <summary>Tests ClientDisplay constructor, properties, equality, and color-depth mapping.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClientDisplay_ValueBehaviorAndColorDepthMapping_IsCoveredAsync()
    {
        var value = new ClientDisplay(OneThousandTwentyFour, SevenHundredSixtyEight, Sixteen);
        var equal = new ClientDisplay(OneThousandTwentyFour, SevenHundredSixtyEight, Sixteen);
        var different = new ClientDisplay(OneThousandTwentyFour, SevenHundredSixtyEight, Eight);

        await Assert.That(value.ClientSize).IsEqualTo(new(OneThousandTwentyFour, SevenHundredSixtyEight));
        await Assert.That(value.ColorDepth).IsEqualTo(UIntThirtyTwo);
        await Assert.That(new ClientDisplay(1, 1, 1).ColorDepth).IsEqualTo(UIntFour);
        await Assert.That(new ClientDisplay(1, 1, Two).ColorDepth).IsEqualTo(UIntEight);
        await Assert.That(new ClientDisplay(1, 1, Four).ColorDepth).IsEqualTo(UIntSixteen);
        await Assert.That(new ClientDisplay(1, 1, Eight).ColorDepth).IsEqualTo(UIntTwentyFour);
        await Assert.That(new ClientDisplay(1, 1, NinetyNine).ColorDepth).IsEqualTo(UIntNinetyNine);
        await Assert.That(value.Equals((object)equal)).IsTrue();
        await Assert.That(value.Equals("not display")).IsFalse();
        await Assert.That(value == equal).IsTrue();
        await Assert.That(value != different).IsTrue();
        await Assert.That(value.GetHashCode()).IsEqualTo(equal.GetHashCode());
        await Assert.That(value.ToString()).Contains("32");
    }

    /// <summary>Tests ClientLatency constructor, properties, equality, and string output.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClientLatency_ValueBehavior_IsCoveredAsync()
    {
        var value = new ClientLatency(Ten, Twenty, Thirty);
        var equal = new ClientLatency(Ten, Twenty, Thirty);
        var different = new ClientLatency(Ten, Twenty, ThirtyOne);

        await Assert.That(value.Avarage).IsEqualTo(UIntTen);
        await Assert.That(value.Last).IsEqualTo(UIntTwenty);
        await Assert.That(value.Derivation).IsEqualTo(UIntThirty);
        await Assert.That(value.Equals((object)equal)).IsTrue();
        await Assert.That(value.Equals("not latency")).IsFalse();
        await Assert.That(value == equal).IsTrue();
        await Assert.That(value != different).IsTrue();
        await Assert.That(value.GetHashCode()).IsEqualTo(equal.GetHashCode());
        await Assert.That(value.ToString()).IsEqualTo("10|20|30");
    }

    /// <summary>Tests ClientInfo constructor, properties, equality, and string output.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClientInfo_ValueBehavior_IsCoveredAsync()
    {
        var address = new ClientAddress(AddressFamily.InterNetwork, CreateAddressBytes(Ten, Eleven, Twelve, Thirteen));
        var value = new ClientInfo(ClientInfoName, ClientInfoDirectory, Hundred, TwoHundred, ThreeHundred, address);
        var equal = new ClientInfo(ClientInfoName, ClientInfoDirectory, Hundred, TwoHundred, ThreeHundred, address);
        var different = new ClientInfo("other", ClientInfoDirectory, Hundred, TwoHundred, ThreeHundred, address);

        await Assert.That(value.Name).IsEqualTo(ClientInfoName);
        await Assert.That(value.Directory).IsEqualTo(ClientInfoDirectory);
        await Assert.That(value.BuildNumber).IsEqualTo(Hundred);
        await Assert.That(value.ProductId).IsEqualTo(TwoHundred);
        await Assert.That(value.HardwareId).IsEqualTo(ThreeHundred);
        await Assert.That(value.Address).IsEqualTo(address);
        await Assert.That(value.Equals((object)equal)).IsTrue();
        await Assert.That(value.Equals("not client info")).IsFalse();
        await Assert.That(value == equal).IsTrue();
        await Assert.That(value != different).IsTrue();
        await Assert.That(value.GetHashCode()).IsEqualTo(equal.GetHashCode());
        await Assert.That(value.ToString()).IsEqualTo(@"client|C:\ICA|100|200|300|InterNetwork|10.11.12.13");
    }

    /// <summary>Tests OsVersionInfo factory, constructor, properties, equality, and string output.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task OsVersionInfo_ValueBehavior_IsCoveredAsync()
    {
        var created = OsVersionInfo.Create();
        var value = new OsVersionInfo(OneHundredFiftySix, Ten, 0, TwentySixThousandOneHundred, Two, ServicePackVersion);
        var equal = new OsVersionInfo(OneHundredFiftySix, Ten, 0, TwentySixThousandOneHundred, Two, ServicePackVersion);
        var different = new OsVersionInfo(OneHundredFiftySix, Eleven, 0, TwentySixThousandOneHundred, Two, ServicePackVersion);

        await Assert.That(created.ServicePackVersion).IsEqualTo(string.Empty);
        await Assert.That(value.MajorVersion).IsEqualTo(Ten);
        await Assert.That(value.MinorVersion).IsEqualTo(0);
        await Assert.That(value.BuildNumber).IsEqualTo(TwentySixThousandOneHundred);
        await Assert.That(value.PlatformId).IsEqualTo(Two);
        await Assert.That(value.ServicePackVersion).IsEqualTo(ServicePackVersion);
        await Assert.That(value.Equals((object)equal)).IsTrue();
        await Assert.That(value.Equals("not os version")).IsFalse();
        await Assert.That(value == equal).IsTrue();
        await Assert.That(value != different).IsTrue();
        await Assert.That(value.GetHashCode()).IsEqualTo(equal.GetHashCode());
        await Assert.That(value.ToString()).IsEqualTo("10.0.26100|2|Service Pack Test");
    }

    /// <summary>Tests SessionTime constructor, properties, equality, and string output.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SessionTime_ValueBehavior_IsCoveredAsync()
    {
        var value = new SessionTime(OneAndHalfDouble, TwoAndHalfDouble, ThreeAndHalfDouble, FourAndHalfDouble, FiveAndHalfDouble);
        var equal = new SessionTime(OneAndHalfDouble, TwoAndHalfDouble, ThreeAndHalfDouble, FourAndHalfDouble, FiveAndHalfDouble);
        var different = new SessionTime(OneAndHalfDouble, TwoAndHalfDouble, ThreeAndHalfDouble, FourAndHalfDouble, SixAndHalfDouble);

        await Assert.That(value.ConnectTime).IsEqualTo(OneAndHalfDouble);
        await Assert.That(value.DisconnectTime).IsEqualTo(TwoAndHalfDouble);
        await Assert.That(value.LastInputTime).IsEqualTo(ThreeAndHalfDouble);
        await Assert.That(value.LogonTime).IsEqualTo(FourAndHalfDouble);
        await Assert.That(value.CurrentTime).IsEqualTo(FiveAndHalfDouble);
        await Assert.That(value.Equals((object)equal)).IsTrue();
        await Assert.That(value.Equals("not session time")).IsFalse();
        await Assert.That(value == equal).IsTrue();
        await Assert.That(value != different).IsTrue();
        await Assert.That(value.GetHashCode()).IsEqualTo(equal.GetHashCode());
        await Assert.That(value.ToString()).IsEqualTo("1.5|2.5|3.5|4.5|5.5");
    }

    /// <summary>Tests UserInfo constructor, properties, equality, and string output.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task UserInfo_ValueBehavior_IsCoveredAsync()
    {
        var value = new UserInfo("user", UserInfoDomainName, UserInfoConnectionName);
        var equal = new UserInfo("user", UserInfoDomainName, UserInfoConnectionName);
        var different = new UserInfo("other", UserInfoDomainName, UserInfoConnectionName);

        await Assert.That(value.Username).IsEqualTo("user");
        await Assert.That(value.Domainname).IsEqualTo(UserInfoDomainName);
        await Assert.That(value.ConnectionName).IsEqualTo(UserInfoConnectionName);
        await Assert.That(value.Equals((object)equal)).IsTrue();
        await Assert.That(value.Equals("not user info")).IsFalse();
        await Assert.That(value == equal).IsTrue();
        await Assert.That(value != different).IsTrue();
        await Assert.That(value.GetHashCode()).IsEqualTo(equal.GetHashCode());
        await Assert.That(value.ToString()).IsEqualTo("user|domain|ica-tcp");
    }

    /// <summary>Tests the Citrix WinFrame API failure paths on machines without WFAPI.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinFrame_WhenWfApiIsUnavailable_ReturnsFalseAndThrowsForDirectCallsAsync()
    {
        await Assert.That(WinFrame.IsAvailabe).IsFalse();
        await Assert.That(static () => WinFrame.QuerySessionConnectState()).Throws<Exception>();
        await Assert.That(static () => WinFrame.QuerySessionInformation(InfoClasses.ClientName)).Throws<Exception>();
        await Assert.That(static () => WinFrame.GetClientName()).Throws<Exception>();
        await Assert.That(static () => WinFrame.GetClientIpAddress()).Throws<Exception>();
        await Assert.That(static () => WinFrame.WaitSystemEvent(EventMask.None)).Throws<Exception>();
    }

    /// <summary>Tests Internet Explorer version calculations and private parser edge cases.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InternetExplorerVersion_CalculatesEmbeddedVersionFromInstalledVersionAsync()
    {
        var installedVersion = InternetExplorerVersion.Version;
        var expectedIgnoreDoctype = ExpectedEmbeddedVersion(installedVersion, true);
        var expectedRespectDoctype = ExpectedEmbeddedVersion(installedVersion, false);

        await Assert.That(installedVersion >= 0).IsTrue();
        await Assert.That(InternetExplorerVersion.GetEmbVersion()).IsEqualTo(expectedIgnoreDoctype);
        await Assert.That(InternetExplorerVersion.GetEmbVersion(true)).IsEqualTo(expectedIgnoreDoctype);
        await Assert.That(InternetExplorerVersion.GetEmbVersion(false)).IsEqualTo(expectedRespectDoctype);
        await Assert.That(InternetExplorerVersion.GetMajorVersion("11.0.9600.19597")).IsEqualTo(Eleven);
        await Assert.That(InternetExplorerVersion.GetMajorVersion("9")).IsEqualTo(Nine);
        await Assert.That(InternetExplorerVersion.GetMajorVersion("not-a-version")).IsEqualTo(0);
        await Assert.That(InternetExplorerVersion.GetMajorVersion(string.Empty)).IsEqualTo(0);
    }

    /// <summary>Tests the extended WebBrowser command target that suppresses script error dialogs.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ExtendedWebBrowserSite_QueryStatusAndExec_AreCoveredAsync()
    {
        using var browser = new BrowserProbe();
        var commandTarget = browser.CreateCommandTarget();

        await Assert.That(commandTarget.QueryStatus(Guid.Empty, 0, IntPtr.Zero, IntPtr.Zero)).IsEqualTo(OleCmmdErrENotsupported);
        await Assert.That(commandTarget.Exec(CGID_DocHostCommandHandler, OleCmdDidShowScriptError, 0, IntPtr.Zero, IntPtr.Zero)).IsEqualTo(Ok);
        await Assert.That(commandTarget.Exec(Guid.Empty, OleCmdDidShowScriptError, 0, IntPtr.Zero, IntPtr.Zero)).IsEqualTo(OleCmmdErrENotsupported);
        await Assert.That(commandTarget.Exec(CGID_DocHostCommandHandler, OleCmdDidShowScriptError + 1, 0, IntPtr.Zero, IntPtr.Zero)).IsEqualTo(OleCmmdErrENotsupported);
    }

    /// <summary>Creates a Citrix client address buffer.</summary>
    /// <param name="first">The first displayed IP address byte.</param>
    /// <param name="second">The second displayed IP address byte.</param>
    /// <param name="third">The third displayed IP address byte.</param>
    /// <param name="fourth">The fourth displayed IP address byte.</param>
    /// <returns>The 20-byte client address buffer.</returns>
    private static byte[] CreateAddressBytes(byte first, byte second, byte third, byte fourth)
    {
        var address = new byte[20];
        address[2] = first;
        address[3] = second;
        address[4] = third;
        address[5] = fourth;
        return address;
    }

    /// <summary>Calculates the expected embedded browser emulation version.</summary>
    /// <param name="installedVersion">The installed browser major version.</param>
    /// <param name="ignoreDoctype">A value indicating whether doctype should be ignored.</param>
    /// <returns>The expected embedded browser emulation version.</returns>
    private static int ExpectedEmbeddedVersion(int installedVersion, bool ignoreDoctype)
    {
        const int internetExplorerNine = 9;
        const int internetExplorerSeven = 7;

        if (installedVersion > internetExplorerNine)
        {
            return (installedVersion * BrowserDoctypeMultiplier) + (ignoreDoctype ? 1 : 0);
        }

        return installedVersion > internetExplorerSeven
            ? installedVersion * BrowserCompatibilityMultiplier
            : BrowserEmulationFallback;
    }

    /// <summary>Exposes the protected browser site factory.</summary>
    private sealed class BrowserProbe : ExtendedWebBrowser
    {
        /// <summary>Creates the browser command target.</summary>
        /// <returns>The OLE command target.</returns>
        public IOleCommandTarget CreateCommandTarget() => (IOleCommandTarget)CreateWebBrowserSiteBase();
    }
}
