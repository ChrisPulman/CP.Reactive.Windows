// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     Contains information about a low-level keyboard input event.
/// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644967(v=vs.85).aspx">KBDLLHOOKSTRUCT structure</a>
/// </summary>
public readonly record struct KeyboardLowLevelHookStruct
{
    /// <summary>Stores the raw virtual-key code.</summary>
    private readonly uint _virtualKeyCode;

    /// <summary>Initializes a new instance of the <see cref="KeyboardLowLevelHookStruct"/> struct.</summary>
    /// <param name="virtualKeyCode">The raw virtual-key code.</param>
    /// <param name="scanCode">The hardware scan code.</param>
    /// <param name="flags">The extended key flags.</param>
    /// <param name="timeStamp">The message timestamp.</param>
    /// <param name="extraInfo">The additional native information.</param>
    internal KeyboardLowLevelHookStruct(uint virtualKeyCode, uint scanCode, ExtendedKeyFlags flags, uint timeStamp, UIntPtr extraInfo)
    {
        _virtualKeyCode = virtualKeyCode;
        ScanCode = scanCode;
        Flags = flags;
        TimeStamp = timeStamp;
        ExtraInfo = extraInfo;
    }

    /// <summary>Gets a virtual-key code. The code must be a value in the range 1 to 254.</summary>
    public VirtualKeyCode VirtualKeyCode => (VirtualKeyCode)checked((int)_virtualKeyCode);

    /// <summary>Gets a hardware scan code for the key.</summary>
    public uint ScanCode { get; }

    /// <summary>
    ///     Gets or sets the extended-key flag, event-injected flags, context code, and transition-state flag.
    ///     This member is specified as follows. An application can use the following values to test the keystroke flags.
    ///     Testing LLKHF_INJECTED (bit 4) will tell you whether the event was injected.
    ///     If it was, then testing LLKHF_LOWER_IL_INJECTED (bit 1) will tell you whether or not the event
    ///     was injected from a process running at lower integrity level.
    /// </summary>
    public ExtendedKeyFlags Flags { get; init; }

    /// <summary>Gets or sets the time stamp for this message, equivalent to what GetMessageTime would return for this message.</summary>
    public uint TimeStamp { get; init; }

    /// <summary>Gets additional native information associated with the message.</summary>
    internal UIntPtr ExtraInfo { get; }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_virtualKeyCode, ScanCode, Flags, TimeStamp, ExtraInfo);

    /// <inheritdoc/>
    public bool Equals(KeyboardLowLevelHookStruct other) =>
        EqualityComparer<uint>.Default.Equals(_virtualKeyCode, other._virtualKeyCode)
        && EqualityComparer<uint>.Default.Equals(ScanCode, other.ScanCode)
        && EqualityComparer<ExtendedKeyFlags>.Default.Equals(Flags, other.Flags)
        && EqualityComparer<uint>.Default.Equals(TimeStamp, other.TimeStamp)
        && EqualityComparer<UIntPtr>.Default.Equals(ExtraInfo, other.ExtraInfo);
}
