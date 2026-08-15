// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     Contains the raw input from a device.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645562.aspx">RAWINPUT structure</a>
/// </summary>
public readonly record struct RawInput
{
    /// <summary>Gets or sets the RawInput header.</summary>
    public RawInputHeader Header { get; init; }

    /// <summary>Gets or sets the device.</summary>
    public RawDevice Device { get; init; }
}
