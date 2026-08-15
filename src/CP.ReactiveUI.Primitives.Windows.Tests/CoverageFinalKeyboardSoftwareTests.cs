// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Final deterministic coverage for keyboard hook helpers and software registry mapping.</summary>
public sealed class CoverageFinalKeyboardSoftwareTests
{
    /// <summary>The GetKeyNameText flag that marks an extended key.</summary>
    private const uint ExtendedKeyNameFlag = 0x01000000;

    /// <summary>The deterministic installed size value.</summary>
    private const long TestSoftwareSize = 123_456_789L;

    /// <summary>Tests enumerable constructors and the remaining key-comparison branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyHandlers_EnumerableConstructorsAndMenuComparisonAreCoveredAsync()
    {
        IEnumerable<VirtualKeyCode> menuCombination = [VirtualKeyCode.Menu];
        var menuHandler = new KeyCombinationHandler(menuCombination);
        var leftMenuDown = KeyboardHookEventArgs.KeyDown(VirtualKeyCode.LeftMenu);

        await Assert.That(menuHandler.Handle(leftMenuDown)).IsTrue();
        await Assert.That(leftMenuDown.Handled).IsTrue();

        IEnumerable<IKeyboardHookEventHandler> orHandlers =
        [
            new KeyCombinationHandler(VirtualKeyCode.KeyA),
            new KeyCombinationHandler(VirtualKeyCode.KeyB),
        ];
        var orHandler = new KeyOrCombinationHandler(orHandlers);
        await Assert.That(orHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyB))).IsTrue();

        IEnumerable<IKeyboardHookEventHandler> sequenceHandlers =
        [
            new KeyCombinationHandler(VirtualKeyCode.KeyC),
            new KeyCombinationHandler(VirtualKeyCode.KeyD),
        ];
        var sequenceHandler = new KeySequenceHandler(sequenceHandlers) { Timeout = null };
        await Assert.That(sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyC))).IsFalse();
        await Assert.That(sequenceHandler.Handle(KeyboardHookEventArgs.KeyUp(VirtualKeyCode.KeyC))).IsFalse();
        await Assert.That(sequenceHandler.Handle(KeyboardHookEventArgs.KeyDown(VirtualKeyCode.KeyD))).IsTrue();
    }

    /// <summary>Tests keyboard display-name branches through the internal native composition adapter.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyHelper_DisplayNamesUseComposedNativeApiAsync()
    {
        var fakeApi = new FakeKeyboardDisplayApi();
        fakeApi.ScanCodes[(uint)VirtualKeyCode.KeyA] = Seven;
        fakeApi.ScanCodes[(uint)VirtualKeyCode.Return] = Eight;
        fakeApi.ScanCodes[(uint)VirtualKeyCode.Left] = Nine;
        fakeApi.ScanCodes[(uint)VirtualKeyCode.Menu] = Ten;
        fakeApi.Names[Seven] = "A";
        fakeApi.Names[Eight] = "enter";
        fakeApi.Names[Nine] = "left";
        fakeApi.Names[Ten] = "alt";
        var previousApi = KeyHelper.SetDisplayApiForTesting(fakeApi);

        try
        {
            await Assert.That(KeyHelper.VirtualKeyCodeFromString("shift")).IsEqualTo(VirtualKeyCode.Shift);
            var parsedKeys = ToList(KeyHelper.VirtualKeyCodesFromString("a++shift"));
            await Assert.That(parsedKeys.Count).IsEqualTo(Two);
            await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.KeyA, false)).IsEqualTo("A");
            await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.Return, false)).IsEqualTo("Enter");
            await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.Left, false)).IsEqualTo("Left");
            await Assert.That((fakeApi.LastLongParameter & ExtendedKeyNameFlag) != 0).IsTrue();
            await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.LeftMenu, true)).IsEqualTo("Alt");
            await Assert.That(fakeApi.LastMappedVirtualKey).IsEqualTo((uint)VirtualKeyCode.Menu);
            await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.KeyB, false)).IsEqualTo(nameof(VirtualKeyCode.KeyB));

            fakeApi.NumpadDisplayName = "x*";
            await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.Multiply, false)).IsEqualTo("X *");

            fakeApi.NumpadDisplayName = "*";
            await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.Multiply, false)).IsEqualTo(" *");

            fakeApi.NumpadDisplayName = "(numeric keypad)";
            await Assert.That(KeyHelper.VirtualCodeToLocaleDisplayText(VirtualKeyCode.Divide, false)).IsEqualTo("/ (numeric keypad)");
        }
        finally
        {
            _ = KeyHelper.SetDisplayApiForTesting(previousApi);
        }
    }

    /// <summary>Tests keyboard input generator send paths without sending real input.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task KeyboardInputGenerator_SendPathsUseComposedNativeApiAsync()
    {
        var fakeApi = new InputCoverage2Tests.FakeNativeInputApi { SendReturn = UIntTen };
        var previousApi = NativeInput.SetApiForTesting(fakeApi);

        try
        {
            await Assert.That(KeyboardInputGenerator.KeyDown(VirtualKeyCode.KeyA)).IsEqualTo(UIntTen);
            await Assert.That(fakeApi.LastInputs.Length).IsEqualTo(One);
            await Assert.That(fakeApi.LastInputs[0].InputUnion.KeyboardInput.KeyEventFlags).IsEqualTo(KeyEventFlags.None);

            await Assert.That(KeyboardInputGenerator.KeyUp(VirtualKeyCode.KeyB)).IsEqualTo(UIntTen);
            await Assert.That(fakeApi.LastInputs.Length).IsEqualTo(One);
            await Assert.That(fakeApi.LastInputs[0].InputUnion.KeyboardInput.KeyEventFlags).IsEqualTo(KeyEventFlags.KeyUp);

            await Assert.That(KeyboardInputGenerator.KeyPresses(VirtualKeyCode.KeyC)).IsEqualTo(UIntTen);
            await Assert.That(fakeApi.LastInputs.Length).IsEqualTo(Two);
            await Assert.That(fakeApi.LastInputs[0].InputUnion.KeyboardInput.KeyEventFlags).IsEqualTo(KeyEventFlags.None);
            await Assert.That(fakeApi.LastInputs[1].InputUnion.KeyboardInput.KeyEventFlags).IsEqualTo(KeyEventFlags.KeyUp);

            await Assert.That(KeyboardInputGenerator.KeyCombinationPress(VirtualKeyCode.Control, VirtualKeyCode.KeyD)).IsEqualTo(UIntTen);
            await Assert.That(fakeApi.LastInputs.Length).IsEqualTo(Four);
            await Assert.That(fakeApi.SendCalls).IsEqualTo(Four);
        }
        finally
        {
            _ = NativeInput.SetApiForTesting(previousApi);
        }
    }

    /// <summary>Tests installed-software enumeration through the internal registry composition adapter.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task InstalledSoftware_UsesComposedRegistryReaderAsync()
    {
        var registry = new FakeInstalledSoftwareRegistry();
        var previousRegistry = InstallationInformation.SetRegistryForTesting(registry);

        try
        {
            await Assert.That(ToList(InstallationInformation.InstalledSoftware()).Count).IsEqualTo(Zero);

            registry.Root = new(["product", "missing"]);
            var productKey = new FakeInstalledSoftwareRegistryKey([]);
            registry.Root.SubKeys["product"] = productKey;
            _ = productKey
                .WithValue(nameof(SoftwareDetails.DisplayName), "Reactive Windows", RegistryValueKind.String)
                .WithValue(nameof(SoftwareDetails.AuthorizedCDFPrefix), "authorized", RegistryValueKind.String)
                .WithValue(nameof(SoftwareDetails.HelpTelephone), "01234", RegistryValueKind.String)
                .WithValue(nameof(SoftwareDetails.Publisher), null, RegistryValueKind.String)
                .WithValue(nameof(SoftwareDetails.Size), TestSoftwareSize, RegistryValueKind.QWord)
                .WithValue("UnknownValue", "ignored", RegistryValueKind.String);

            var installedSoftware = ToList(InstallationInformation.InstalledSoftware());
            await Assert.That(installedSoftware.Count).IsEqualTo(One);
            await Assert.That(installedSoftware[0].DisplayName).IsEqualTo("Reactive Windows");
            await Assert.That(installedSoftware[0].AuthorizedCDFPrefix).IsEqualTo("authorized");
            await Assert.That(installedSoftware[0].HelpTelephone).IsEqualTo("01234");
            await Assert.That(installedSoftware[0].Publisher).IsNull();
            await Assert.That(installedSoftware[0].Size).IsEqualTo(TestSoftwareSize);
            await Assert.That(registry.Root.DisposeCount).IsEqualTo(One);
        }
        finally
        {
            _ = InstallationInformation.SetRegistryForTesting(previousRegistry);
        }
    }

    /// <summary>Tests registry-value mapping branches that do not require registry access.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SoftwareDetailsMapping_CoversNullInvalidAndBooleanConversionsAsync()
    {
        var values = new Dictionary<string, (object Value, RegistryValueKind Kind)>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(SoftwareDetails.NoModify)] = (1, RegistryValueKind.DWord),
            [nameof(SoftwareDetails.NoRepair)] = (0L, RegistryValueKind.QWord),
            [nameof(SoftwareDetails.Language)] = ("invalid", RegistryValueKind.String),
            [nameof(SoftwareDetails.Readme)] = (null, RegistryValueKind.String),
        };

        var mapped = InstallationInformation.MapFromRegistryValues(
            "not-a-guid",
            values.Keys,
            name => values[name].Value,
            name => values[name].Kind);

        await Assert.That(mapped.DisplayName).IsEqualTo("not-a-guid");
        await Assert.That(mapped.Id).IsNull();
        await Assert.That(mapped.NoModify).IsTrue();
        await Assert.That(mapped.NoRepair).IsFalse();
        await Assert.That(mapped.Language).IsEqualTo(Zero);
        await Assert.That(mapped.Readme).IsNull();
    }

    /// <summary>Copies an enumerable into a list without LINQ.</summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="items">The items to copy.</param>
    /// <returns>The copied items.</returns>
    private static List<T> ToList<T>(IEnumerable<T> items)
    {
        var result = new List<T>();
        foreach (var item in items)
        {
            result.Add(item);
        }

        return result;
    }

    /// <summary>Fake keyboard display API for deterministic display-name tests.</summary>
    private sealed class FakeKeyboardDisplayApi : INativeKeyboardDisplayApi
    {
        /// <summary>The scan code used by the production helper for numpad operator display names.</summary>
        private const uint NumpadOperatorScanCode = 55U;

        /// <summary>Gets the mapped scan codes by virtual key.</summary>
        internal Dictionary<uint, uint> ScanCodes { get; } = [];

        /// <summary>Gets the display names by scan code.</summary>
        internal Dictionary<uint, string> Names { get; } = [];

        /// <summary>Gets or sets the display name returned for numpad operator lookups.</summary>
        internal string NumpadDisplayName { get; set; } = string.Empty;

        /// <summary>Gets the last long parameter passed to GetKeyNameText.</summary>
        internal uint LastLongParameter { get; private set; }

        /// <summary>Gets the last virtual key passed to MapVirtualKeyEx.</summary>
        internal uint LastMappedVirtualKey { get; private set; }

        /// <inheritdoc />
        public IntPtr GetKeyboardLayout(uint threadId) => (IntPtr)One;

        /// <inheritdoc />
        public uint MapVirtualKeyEx(uint code, uint mapType, IntPtr keyboardLayout)
        {
            LastMappedVirtualKey = code;
            return ScanCodes.TryGetValue(code, out var scanCode) ? scanCode : 0;
        }

        /// <inheritdoc />
        public int GetKeyNameText(uint longParameter, Span<char> text)
        {
            LastLongParameter = longParameter;
            var scanCode = (longParameter >> Sixteen) & (uint)TwoHundredFiftyFive;
            var displayName = scanCode == NumpadOperatorScanCode ? NumpadDisplayName : GetDisplayName(scanCode);
            for (var i = 0; i < displayName.Length && i < text.Length; i++)
            {
                text[i] = displayName[i];
            }

            return Math.Min(displayName.Length, text.Length);
        }

        /// <summary>Gets the display name for a scan code.</summary>
        /// <param name="scanCode">The scan code.</param>
        /// <returns>The display name.</returns>
        private string GetDisplayName(uint scanCode) => Names.TryGetValue(scanCode, out var displayName) ? displayName : string.Empty;
    }

    /// <summary>Fake installed-software registry reader.</summary>
    private sealed class FakeInstalledSoftwareRegistry : IInstalledSoftwareRegistry
    {
        /// <summary>Gets or sets the root registry key.</summary>
        internal FakeInstalledSoftwareRegistryKey Root { get; set; }

        /// <inheritdoc />
        public IInstalledSoftwareRegistryKey OpenLocalMachineSubKey(string subkeyName) => Root;
    }

    /// <summary>Fake installed-software registry key.</summary>
    /// <param name="subKeyNames">The child subkey names.</param>
    private sealed class FakeInstalledSoftwareRegistryKey(string[] subKeyNames) : IInstalledSoftwareRegistryKey
    {
        /// <summary>Registry values by name.</summary>
        private readonly Dictionary<string, (object Value, RegistryValueKind Kind)> _values = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Gets child subkeys by name.</summary>
        internal Dictionary<string, FakeInstalledSoftwareRegistryKey> SubKeys { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Gets the dispose count.</summary>
        internal int DisposeCount { get; private set; }

        /// <inheritdoc />
        public void Dispose() => DisposeCount++;

        /// <inheritdoc />
        public string[] GetSubKeyNames() => subKeyNames;

        /// <inheritdoc />
        public IInstalledSoftwareRegistryKey OpenSubKey(string subkeyName) =>
            SubKeys.TryGetValue(subkeyName, out var subKey) ? subKey : null;

        /// <inheritdoc />
        public object GetValue(string valueName) => _values[valueName].Value;

        /// <inheritdoc />
        public RegistryValueKind GetValueKind(string valueName) => _values[valueName].Kind;

        /// <inheritdoc />
        public string[] GetValueNames()
        {
            var valueNames = new string[_values.Count];
            var index = 0;
            foreach (var valueName in _values.Keys)
            {
                valueNames[index] = valueName;
                index++;
            }

            return valueNames;
        }

        /// <summary>Adds a registry value.</summary>
        /// <param name="name">The value name.</param>
        /// <param name="value">The value.</param>
        /// <param name="kind">The value kind.</param>
        /// <returns>The current fake key.</returns>
        internal FakeInstalledSoftwareRegistryKey WithValue(string name, object value, RegistryValueKind kind)
        {
            _values[name] = (value, kind);
            return this;
        }
    }
}
