// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
#endif
/// <summary>Stores information about a DPI change.</summary>
public class DpiChangeInfo
{
    /// <summary>Gets the DPI from before the change.</summary>
    public int PreviousDpi { get; }

    /// <summary>Gets the new DPI.</summary>
    public int NewDpi { get; }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.DpiChangeInfo" /> class.</summary>
    /// <param name="previousDpi">The DPI before the change.</param>
    /// <param name="newDpi">The DPI after the change.</param>
    public DpiChangeInfo(int previousDpi, int newDpi)
    {
        PreviousDpi = previousDpi;
        NewDpi = newDpi;
    }
}
