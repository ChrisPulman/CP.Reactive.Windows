// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>The different known HID usage pages.</summary>
[Flags]
public enum HidUsagePages
{
    /// <summary>No HID usage page.</summary>
    None = 0,
    /// <summary>Generic desktop controls.</summary>
    Generic = 1,
    /// <summary>Simulation controls.</summary>
    Simulation = 2,
    /// <summary>Virtual reality controls.</summary>
    VR = Generic | Simulation,
    /// <summary>Sports controls.</summary>
    Sport = 4,
    /// <summary>Games controls.</summary>
    Game = Generic | Sport,
    /// <summary>Keyboard controls.</summary>
    Keyboard = VR | Sport,
    /// <summary>LED controls.</summary>
    LED = 8,
    /// <summary>Button controls.</summary>
    Button = Generic | LED,
    /// <summary>Ordinal controls.</summary>
    Ordinal = Simulation | LED,
    /// <summary>Telephony controls.</summary>
    Telephony = VR | LED,
    /// <summary>Consumer controls.</summary>
    Consumer = Sport | LED,
    /// <summary>Digitizer controls.</summary>
    Digitizer = Game | LED,
    /// <summary>Physical interface device controls.</summary>
    PID = Keyboard | LED,
    /// <summary>Unicode usage page.</summary>
    Unicode = 0x10,
    /// <summary>Alphanumeric display.</summary>
    AlphaNumeric = Sport | Unicode,
    /// <summary>Medical instruments.</summary>
    Medical = 0x40,
    /// <summary>Monitor page 0.</summary>
    MonitorPage0 = 0x80,
    /// <summary>Monitor page 1.</summary>
    MonitorPage1 = Generic | MonitorPage0,
    /// <summary>Monitor page 2.</summary>
    MonitorPage2 = Simulation | MonitorPage0,
    /// <summary>Monitor page 3.</summary>
    MonitorPage3 = VR | MonitorPage0,
    /// <summary>Power page 0.</summary>
    PowerPage0 = Sport | MonitorPage0,
    /// <summary>Power page 1.</summary>
    PowerPage1 = Game | MonitorPage0,
    /// <summary>Power page 2.</summary>
    PowerPage2 = MonitorPage2 | Sport,
    /// <summary>Power page 3.</summary>
    PowerPage3 = Keyboard | MonitorPage0,
    /// <summary>Bar code scanner.</summary>
    BarCode = Consumer | MonitorPage0,
    /// <summary>Scale page.</summary>
    Scale = Digitizer | MonitorPage0,
    /// <summary>Magnetic strip reading devices.</summary>
    MSR = PowerPage2 | LED,
    /// <summary>Camera controls.</summary>
    Camera = Unicode | MonitorPage0,
    /// <summary>Arcade controls.</summary>
    Arcade = MonitorPage1 | Unicode,
}
