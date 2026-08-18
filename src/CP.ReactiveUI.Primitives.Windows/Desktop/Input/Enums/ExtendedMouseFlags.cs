// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>
///     The event-injected flags. An application can use the following values to test the flags.
///     Testing LLMHF_INJECTED (bit 0) will tell you whether the event was injected.
///     If it was, then testing LLMHF_LOWER_IL_INJECTED (bit 1) will tell you whether or not
///     the event was injected from a process running at lower integrity level.
/// </summary>
[Flags]
public enum ExtendedMouseFlags : uint
{
    /// <summary>Test the event-injected (from any process) flag.</summary>
    None = 0U,
    /// <summary>Test the event-injected (from any process) flag.</summary>
    Injected = 1U,
    /// <summary>Test the event-injected (from a process running at lower integrity level) flag.</summary>
    LowerIntegretyInjected = 2U,
}
