// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Integrations.Browser;
using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix;
using CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Covers the managed behavior around optional Windows integrations.</summary>
public sealed class IntegrationCoverage2Tests
{
    /// <summary>The browser feature-emulation value used for testing.</summary>
    private const int BrowserFeatureVersion = 11_001;

    /// <summary>The expected Citrix client name.</summary>
    private const string ClientName = "citrix-client";

    /// <summary>Verifies the browser version reader and registry writer adapters.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InternetExplorerVersion_ComposesRegistryAccessAsync()
    {
        var registry = new InternetExplorerRegistryProbe { Values = ["7.0", "11.0.26000", "9", "invalid"] };

        await Assert.That(InternetExplorerVersion.GetVersion(registry)).IsEqualTo(Eleven);
        await Assert.That(static () => InternetExplorerVersion.GetVersion(null)).Throws<ArgumentNullException>();

        InternetExplorerVersion.ChangeEmbeddedVersion("integration-coverage", BrowserFeatureVersion, registry);
        await Assert.That(registry.SetValueCount).IsEqualTo(One);
        await Assert.That(registry.KeyName).Contains("FEATURE_BROWSER_EMULATION");
        await Assert.That(registry.ValueName).IsEqualTo("integration-coverage.exe");
        await Assert.That(registry.Value).IsEqualTo(BrowserFeatureVersion);

        var unauthorized = new InternetExplorerRegistryProbe { ExceptionToThrow = new UnauthorizedAccessException() };
        InternetExplorerVersion.ChangeEmbeddedVersion("denied", Eleven, unauthorized);
        var security = new InternetExplorerRegistryProbe { ExceptionToThrow = new System.Security.SecurityException() };
        InternetExplorerVersion.ChangeEmbeddedVersion("denied", Eleven, security);
    }

    /// <summary>Verifies that WinFrame behavior can be composed without installing Citrix.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WinFrame_ComposesNativeApiResultsAndFailuresAsync()
    {
        var available = new WinFrameApiProbe();
        var originalApi = WinFrame.ExchangeApi(available);

        try
        {
            await Assert.That(WinFrame.IsAvailabe).IsTrue();
            await Assert.That(WinFrame.QuerySessionConnectState()).IsEqualTo(ConnectStates.Connected);
            await Assert.That(WinFrame.QuerySessionInformation(InfoClasses.ClientName)).IsEqualTo(ClientName);
            await Assert.That(WinFrame.GetClientName()).IsEqualTo(ClientName);
            await Assert.That(WinFrame.GetClientIpAddress()).IsEqualTo("192.168.0.42");
            await Assert.That(WinFrame.WaitSystemEvent(EventMask.All)).IsEqualTo(EventMask.Logon);
            await Assert.That(available.FreeMemoryCount).IsEqualTo(Five);

            var unavailable = new WinFrameApiProbe { QuerySucceeds = false, WaitSucceeds = false };
            _ = WinFrame.ExchangeApi(unavailable);
            await Assert.That(WinFrame.IsAvailabe).IsTrue();
            await Assert.That(WinFrame.QuerySessionConnectState()).IsNull();
            await Assert.That(WinFrame.QuerySessionInformation(InfoClasses.ClientName)).IsNull();
            await Assert.That(WinFrame.GetClientIpAddress()).IsEqualTo("0.0.0.0");
            await Assert.That(static () => WinFrame.WaitSystemEvent(EventMask.None)).Throws<Win32Exception>();

            _ = WinFrame.ExchangeApi(new WinFrameApiProbe { ThrowOnQuery = true });
            await Assert.That(WinFrame.IsAvailabe).IsFalse();
            await Assert.That(static () => WinFrame.ExchangeApi(null)).Throws<ArgumentNullException>();
        }
        finally
        {
            _ = WinFrame.ExchangeApi(originalApi);
        }
    }

    /// <summary>Provides controlled browser-registry behavior.</summary>
    private sealed class InternetExplorerRegistryProbe : IInternetExplorerRegistry
    {
        /// <summary>Gets or sets the values returned by the reader.</summary>
        public IReadOnlyList<object> Values { get; init; } = [];

        /// <summary>Gets or sets the exception thrown by the writer.</summary>
        public Exception ExceptionToThrow { get; init; }

        /// <summary>Gets the number of write calls.</summary>
        public int SetValueCount { get; private set; }

        /// <summary>Gets the written key name.</summary>
        public string KeyName { get; private set; } = string.Empty;

        /// <summary>Gets the written value name.</summary>
        public string ValueName { get; private set; } = string.Empty;

        /// <summary>Gets the written value.</summary>
        public object Value { get; private set; } = string.Empty;

        /// <summary>Gets or sets the number of read calls.</summary>
        private int ReadValueCount { get; set; }

        /// <inheritdoc />
        public object GetValue(string keyName, string valueName)
        {
            var value = Values[ReadValueCount % Values.Count];
            ReadValueCount++;
            return value;
        }

        /// <inheritdoc />
        public void SetValue(string keyName, string valueName, object value)
        {
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            KeyName = keyName;
            ValueName = valueName;
            Value = value;
            SetValueCount++;
        }
    }

    /// <summary>Provides in-memory Citrix native buffers.</summary>
    private sealed class WinFrameApiProbe : IWinFrameApi
    {
        /// <summary>The unmanaged ClientAddress size.</summary>
        private const int ClientAddressSize = 24;

        /// <summary>The first displayed address offset.</summary>
        private const int FirstAddressOffset = 6;

        /// <summary>The first displayed address byte.</summary>
        private const byte FirstAddressByte = 192;

        /// <summary>The second displayed address offset.</summary>
        private const int SecondAddressOffset = 7;

        /// <summary>The second displayed address byte.</summary>
        private const byte SecondAddressByte = 168;

        /// <summary>The third displayed address offset.</summary>
        private const int ThirdAddressOffset = 8;

        /// <summary>The fourth displayed address offset.</summary>
        private const int FourthAddressOffset = 9;

        /// <summary>The fourth displayed address byte.</summary>
        private const byte FourthAddressByte = 42;

        /// <summary>Gets or sets a value indicating whether session queries succeed.</summary>
        public bool QuerySucceeds { get; init; } = true;

        /// <summary>Gets or sets a value indicating whether event waits succeed.</summary>
        public bool WaitSucceeds { get; init; } = true;

        /// <summary>Gets or sets a value indicating whether queries throw.</summary>
        public bool ThrowOnQuery { get; init; }

        /// <summary>Gets the number of freed native buffers.</summary>
        public int FreeMemoryCount { get; private set; }

        /// <inheritdoc />
        public bool QuerySessionInformation(IntPtr serverHandle, int sessionId, InfoClasses infoType, out IntPtr buffer, out int bytesReturned)
        {
            if (ThrowOnQuery)
            {
                throw new DllNotFoundException();
            }

            if (!QuerySucceeds)
            {
                buffer = IntPtr.Zero;
                bytesReturned = 0;
                return false;
            }

            buffer = infoType switch
            {
                InfoClasses.ConnectState => CreateConnectState(),
                InfoClasses.ClientAddress => CreateClientAddress(),
                _ => Marshal.StringToHGlobalUni(ClientName),
            };
            bytesReturned = 1;
            return true;
        }

        /// <inheritdoc />
        public bool WaitSystemEvent(IntPtr serverHandle, EventMask eventMask, out EventMask eventFlags)
        {
            eventFlags = EventMask.Logon;
            return WaitSucceeds;
        }

        /// <inheritdoc />
        public void FreeMemory(IntPtr memory)
        {
            if (memory.ToInt64() > FourThousandThreeHundredTwentyOne)
            {
                Marshal.FreeHGlobal(memory);
            }

            FreeMemoryCount++;
        }

        /// <summary>Creates a connection-state buffer.</summary>
        /// <returns>A native connection-state buffer.</returns>
        private static IntPtr CreateConnectState()
        {
            var buffer = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(buffer, (int)ConnectStates.Connected);
            return buffer;
        }

        /// <summary>Creates a client-address buffer.</summary>
        /// <returns>A native client-address buffer.</returns>
        private static IntPtr CreateClientAddress()
        {
            var buffer = Marshal.AllocHGlobal(ClientAddressSize);
            Marshal.WriteInt32(buffer, (int)System.Net.Sockets.AddressFamily.InterNetwork);
            Marshal.WriteByte(buffer, FirstAddressOffset, FirstAddressByte);
            Marshal.WriteByte(buffer, SecondAddressOffset, SecondAddressByte);
            Marshal.WriteByte(buffer, ThirdAddressOffset, 0);
            Marshal.WriteByte(buffer, FourthAddressOffset, FourthAddressByte);
            return buffer;
        }
    }
}
