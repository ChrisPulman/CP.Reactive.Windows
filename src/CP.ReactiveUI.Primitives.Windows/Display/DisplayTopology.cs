// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using CP.ReactiveUI.Primitives.Windows.Native.Extensions;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display;
#endif
/// <summary>Provides current display topology snapshots and change notifications.</summary>
public static class DisplayTopology
{
    /// <summary>Gets the bounds of the complete virtual desktop.</summary>
    public static NativeRect ScreenBounds => CalculateScreenBounds(GetSnapshot());

    /// <summary>Gets a fresh snapshot of all displays known to Windows.</summary>
    /// <returns>The current display snapshot.</returns>
    public static IReadOnlyList<DisplayInfo> GetSnapshot() => User32Api.EnumDisplays();

    /// <summary>Observes display topology, beginning with the current snapshot.</summary>
    /// <returns>A stream containing the current and subsequent display snapshots.</returns>
    public static IObservable<IReadOnlyList<DisplayInfo>> ObserveChanges() => (from _ in SharedMessageWindow.ObserveWindowMessages()
            where _.Msg == WindowsMessages.WM_DISPLAYCHANGE
            select GetSnapshot()).StartWith(GetSnapshot());

    /// <summary>Gets the display bounds containing the specified point.</summary>
    /// <param name="point">The virtual-desktop point.</param>
    /// <returns>The containing display bounds, or an empty rectangle when no display contains the point.</returns>
    public static NativeRect GetBounds(NativePoint point)
    {
        DisplayInfo candidate = null;
        foreach (DisplayInfo display in GetSnapshot())
        {
            if (display.IsPrimary && candidate is null)
            {
                candidate = display;
            }

            if (display.Bounds.Contains(point))
            {
                return display.Bounds;
            }
        }

        return candidate?.Bounds ?? NativeRect.Empty;
    }

    /// <summary>Calculates virtual-desktop bounds for a display snapshot.</summary>
    /// <param name="displays">The display snapshot.</param>
    /// <returns>The virtual-desktop bounds.</returns>
    internal static NativeRect CalculateScreenBounds(IReadOnlyList<DisplayInfo> displays)
    {
        if (displays.Count == 0)
        {
            return NativeRect.Empty;
        }

        int left = displays[0].Bounds.Left;
        int top = displays[0].Bounds.Top;
        int right = displays[0].Bounds.Right;
        int bottom = displays[0].Bounds.Bottom;
        checked
        {
            for (int index = 1; index < displays.Count; index++)
            {
                NativeRect bounds = displays[index].Bounds;
                left = Math.Min(left, bounds.Left);
                top = Math.Min(top, bounds.Top);
                right = Math.Max(right, bounds.Right);
                bottom = Math.Max(bottom, bounds.Bottom);
            }

            return new(left, top, right - left, bottom - top);
        }
    }
}
