// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>
///     The extended-key flag, event-injected flags, context code, and transition-state flag.
///     This member is specified as follows.
///     An application can use the following values to test the keystroke flags.
///     Testing LLKHF_INJECTED (bit 4) will tell you whether the event was injected.
///     If it was, then testing LLKHF_LOWER_IL_INJECTED (bit 1) will tell you whether or not the event was injected from a
///     process running at lower integrity level.
/// </summary>
[Flags]
public enum ExtendedKeyFlags : uint
{
    /// <summary>No flags.</summary>
    None = 0U,
    /// <summary>Test the extended-key flag.</summary>
    Extended = 1U,
    /// <summary>Test the event-injected (from a process running at lower integrity level) flag.</summary>
    LowerIntegretyInjected = 2U,
    /// <summary>Test the event-injected (from any process) flag.</summary>
    Injected = 0x10U,
    /// <summary>Test the context code.</summary>
    AltDown = 0x20U,
    /// <summary>Test the transition-state flag.</summary>
    Up = 0x80U
}
