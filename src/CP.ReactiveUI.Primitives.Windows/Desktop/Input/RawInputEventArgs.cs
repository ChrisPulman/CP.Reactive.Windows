// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Raw Input information.</summary>
public class RawInputEventArgs : EventArgs
{
    /// <summary>Gets or sets if true the application was in the foreground.</summary>
    public bool IsForeground { get; set; }

    /// <summary>Gets or sets the actual raw input.</summary>
    public RawInput RawInput { get; set; }
}
