// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Mouse;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Mouse;
#endif
/// <summary>Information on mouse changes TODO: Make the information a lot clearer, than processing WindowsMessages.</summary>
public class MouseHookEventArgs : EventArgs
{
    /// <summary>Gets or sets set this to true if the event is handled, other event-handlers in the chain will not be called.</summary>
    public bool Handled { get; set; }

    /// <summary>Gets or sets the x- and y-coordinates of the cursor, in per-monitor-aware screen coordinates.</summary>
    public NativePoint Point { get; set; }

    /// <summary>Gets or sets the mouse message.</summary>
    public WindowsMessages WindowsMessage { get; set; }
}
