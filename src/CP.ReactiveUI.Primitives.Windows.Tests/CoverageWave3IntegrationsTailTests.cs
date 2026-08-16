// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Integrations.Browser;
using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix;
using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Covers deterministic integration adapters without changing operating-system state.</summary>
public sealed class CoverageWave3IntegrationsTailTests
{
    /// <summary>A reusable registry value name used by adapter tests.</summary>
    private const string RegistryValueName = "value";

    /// <summary>The expected IE 11 emulation value which ignores doctype.</summary>
    private const int ElevenThousandOne = 11_001;

    /// <summary>The expected IE 11 emulation value which respects doctype.</summary>
    private const int ElevenThousand = 11_000;

    /// <summary>The expected IE 7 fallback emulation value.</summary>
    private const int SevenThousand = 7000;

    /// <summary>The expected IE 8 compatibility emulation value.</summary>
    private const int EightThousandEightHundredEightyEight = 8888;

    /// <summary>The expected IE 9 compatibility emulation value.</summary>
    private const int NineThousandNineHundredNinetyNine = 9999;

    /// <summary>An explicit emulation version used for a write-capture test.</summary>
    private const int TenThousandOne = 10_001;

    /// <summary>An explicit Windows registry adapter write-capture value.</summary>
    private const int TwelveThousandOne = 12_001;

    /// <summary>Verifies browser registry composition and all browser-version calculations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InternetExplorerVersion_UsesInjectedRegistryWithoutWritingWindowsRegistryAsync()
    {
        var registry = new InternetExplorerRegistryProbe(["8.0", "9.0", "11.0", "invalid"]);
        var originalRegistry = InternetExplorerVersion.ExchangeRegistry(registry);

        try
        {
            await Assert.That(InternetExplorerVersion.Version).IsEqualTo(Eleven);
            await Assert.That(InternetExplorerVersion.GetEmbVersion()).IsEqualTo(ElevenThousandOne);
            await Assert.That(InternetExplorerVersion.GetEmbVersion(false)).IsEqualTo(ElevenThousand);
            await Assert.That(InternetExplorerVersion.GetEmbVersion(true, Seven)).IsEqualTo(SevenThousand);
            await Assert.That(InternetExplorerVersion.GetEmbVersion(false, Eight)).IsEqualTo(EightThousandEightHundredEightyEight);
            await Assert.That(InternetExplorerVersion.GetEmbVersion(true, Nine)).IsEqualTo(NineThousandNineHundredNinetyNine);

            InternetExplorerVersion.ChangeEmbeddedVersion();
            InternetExplorerVersion.ChangeEmbeddedVersion(false);
            InternetExplorerVersion.ChangeEmbeddedVersion("coverage-browser");
            InternetExplorerVersion.ChangeEmbeddedVersion("coverage-browser-doctype", false);
            InternetExplorerVersion.ChangeEmbeddedVersion("coverage-browser-explicit", TenThousandOne);

            await Assert.That(registry.Writes.Count).IsEqualTo(Five);
            await Assert.That(registry.Writes[0].KeyName).Contains("FEATURE_BROWSER_EMULATION");
            await Assert.That(registry.Writes[2].ValueName).IsEqualTo("coverage-browser.exe");
            await Assert.That(registry.Writes[3].Value).IsEqualTo(ElevenThousand);
            await Assert.That(registry.Writes[4].Value).IsEqualTo(TenThousandOne);
            await Assert.That(static () => InternetExplorerVersion.ExchangeRegistry(null)).Throws<ArgumentNullException>();
        }
        finally
        {
            _ = InternetExplorerVersion.ExchangeRegistry(originalRegistry);
        }
    }

    /// <summary>Verifies the Windows registry adapter can be composed without calling the Windows Registry.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsInternetExplorerRegistry_UsesSuppliedDelegatesAsync()
    {
        var writtenKey = string.Empty;
        var writtenValueName = string.Empty;
        var writtenValue = new object();
        var registry = new InternetExplorerVersion.WindowsInternetExplorerRegistry(
            static (_, _) => "12.0",
            (keyName, valueName, value) =>
            {
                writtenKey = keyName;
                writtenValueName = valueName;
                writtenValue = value;
            });

        await Assert.That(registry.GetValue("key", RegistryValueName)).IsEqualTo("12.0");
        registry.SetValue("key", RegistryValueName, TwelveThousandOne);

        await Assert.That(writtenKey).IsEqualTo("key");
        await Assert.That(writtenValueName).IsEqualTo(RegistryValueName);
        await Assert.That(writtenValue).IsEqualTo(TwelveThousandOne);
        await Assert.That(static () => new InternetExplorerVersion.WindowsInternetExplorerRegistry(null, static (_, _, _) => { })).Throws<ArgumentNullException>();
        await Assert.That(static () => new InternetExplorerVersion.WindowsInternetExplorerRegistry(static (_, _) => null, null)).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies the WFAPI adapter invokes injected native-call delegates only.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeWinFrameApi_UsesInjectedNativeCallsAsync()
    {
        var freedMemory = IntPtr.Zero;
        var api = new WinFrame.NativeWinFrameApi(QuerySessionInformation, WaitSystemEvent, memory => freedMemory = memory);

        var queried = api.QuerySessionInformation(IntPtr.Zero, -1, InfoClasses.ClientName, out var buffer, out var bytesReturned);
        var waited = api.WaitSystemEvent(IntPtr.Zero, EventMask.All, out var eventFlags);
        api.FreeMemory(new(FortyTwo));

        await Assert.That(queried).IsTrue();
        await Assert.That(buffer).IsEqualTo(new(One));
        await Assert.That(bytesReturned).IsEqualTo(One);
        await Assert.That(waited).IsTrue();
        await Assert.That(eventFlags).IsEqualTo(EventMask.Logon);
        await Assert.That(freedMemory).IsEqualTo(new(FortyTwo));
        await Assert.That(static () => new WinFrame.NativeWinFrameApi(null, WaitSystemEvent, static _ => { })).Throws<ArgumentNullException>();
        await Assert.That(static () => new WinFrame.NativeWinFrameApi(QuerySessionInformation, null, static _ => { })).Throws<ArgumentNullException>();
        await Assert.That(static () => new WinFrame.NativeWinFrameApi(QuerySessionInformation, WaitSystemEvent, null)).Throws<ArgumentNullException>();
    }

    /// <summary>Returns a deterministic WFAPI session-information response.</summary>
    /// <param name="serverHandle">The ignored server handle.</param>
    /// <param name="sessionId">The ignored session identifier.</param>
    /// <param name="infoType">The ignored information type.</param>
    /// <param name="buffer">The output buffer.</param>
    /// <param name="bytesReturned">The output byte count.</param>
    /// <returns><see langword="true" />.</returns>
    private static bool QuerySessionInformation(IntPtr serverHandle, int sessionId, InfoClasses infoType, out IntPtr buffer, out int bytesReturned)
    {
        GC.KeepAlive(serverHandle);
        GC.KeepAlive(sessionId);
        GC.KeepAlive(infoType);
        buffer = new(One);
        bytesReturned = One;
        return true;
    }

    /// <summary>Returns a deterministic WFAPI session event.</summary>
    /// <param name="serverHandle">The ignored server handle.</param>
    /// <param name="eventMask">The ignored requested event mask.</param>
    /// <param name="eventFlags">The output event mask.</param>
    /// <returns><see langword="true" />.</returns>
    private static bool WaitSystemEvent(IntPtr serverHandle, EventMask eventMask, out EventMask eventFlags)
    {
        GC.KeepAlive(serverHandle);
        GC.KeepAlive(eventMask);
        eventFlags = EventMask.Logon;
        return true;
    }

    /// <summary>Captures in-memory browser registry operations.</summary>
    /// <param name="values">The registry values to return in sequence.</param>
    private sealed class InternetExplorerRegistryProbe(IReadOnlyList<object> values) : IInternetExplorerRegistry
    {
        /// <summary>Gets the captured registry writes.</summary>
        public List<(string KeyName, string ValueName, object Value)> Writes { get; } = [];

        /// <summary>Gets or sets the next registry value index.</summary>
        private int ReadIndex { get; set; }

        /// <inheritdoc />
        public object GetValue(string keyName, string valueName)
        {
            var value = values[ReadIndex % values.Count];
            ReadIndex++;
            return value;
        }

        /// <inheritdoc />
        public void SetValue(string keyName, string valueName, object value) => Writes.Add((keyName, valueName, value));
    }
}
