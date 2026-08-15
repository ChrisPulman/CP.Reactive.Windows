// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>Enumeration containing flags for raw keyboard input.</summary>
[Flags]
public enum RawKeyboardFlags
{
    /// <summary>The key is down with no extra raw keyboard flags.</summary>
    None = 0,
    /// <summary>The key is up.</summary>
    Break = 1,
    /// <summary>The scan code has the E0 prefix.</summary>
    E0 = 2,
    /// <summary>The scan code has the E1 prefix.</summary>
    E1 = 4,
    /// <summary>No clue.</summary>
    TerminalServerSetLED = 8,
    /// <summary>No clue.</summary>
    TerminalServerShadow = 0x10,
    /// <summary>No clue.</summary>
    TerminalServerVkPacket = 0x20
}
