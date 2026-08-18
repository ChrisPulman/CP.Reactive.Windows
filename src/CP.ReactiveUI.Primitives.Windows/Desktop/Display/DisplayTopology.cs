// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display;
#endif
/// <summary>Provides current display topology snapshots and change notifications.</summary>
public static class DisplayTopology
{
    /// <summary>Creates the shared display-change message stream.</summary>
    private static Func<IObservable<WindowMessage>> _windowMessages = SharedMessageWindow.ObserveWindowMessages;

    /// <summary>Retrieves the current display snapshot.</summary>
    private static Func<IReadOnlyList<DisplayInfo>> _snapshotProvider = User32Api.EnumDisplays;

    /// <summary>Gets the bounds of the complete virtual desktop.</summary>
    public static NativeRect ScreenBounds => CalculateScreenBounds(GetSnapshot());

    /// <summary>Gets a fresh snapshot of all displays known to Windows.</summary>
    /// <returns>The current display snapshot.</returns>
    public static IReadOnlyList<DisplayInfo> GetSnapshot() => _snapshotProvider();

    /// <summary>Observes display topology, beginning with the current snapshot.</summary>
    /// <returns>A stream containing the current and subsequent display snapshots.</returns>
    public static IObservable<IReadOnlyList<DisplayInfo>> ObserveChanges() =>
        ObserveChangesCore(_windowMessages(), _snapshotProvider);

    /// <summary>Gets the display bounds containing the specified point.</summary>
    /// <param name="point">The virtual-desktop point.</param>
    /// <returns>The containing display bounds, or an empty rectangle when no display contains the point.</returns>
    public static NativeRect GetBounds(NativePoint point)
    {
        DisplayInfo candidate = null;
        foreach (var display in GetSnapshot())
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

        var left = displays[0].Bounds.Left;
        var top = displays[0].Bounds.Top;
        var right = displays[0].Bounds.Right;
        var bottom = displays[0].Bounds.Bottom;
        checked
        {
            for (var index = 1; index < displays.Count; index++)
            {
                var bounds = displays[index].Bounds;
                left = Math.Min(left, bounds.Left);
                top = Math.Min(top, bounds.Top);
                right = Math.Max(right, bounds.Right);
                bottom = Math.Max(bottom, bounds.Bottom);
            }

            return new(left, top, right - left, bottom - top);
        }
    }

    /// <summary>Creates a topology stream from deterministic message and snapshot sources.</summary>
    /// <param name="windowMessages">The source window messages.</param>
    /// <param name="snapshotProvider">The display snapshot provider.</param>
    /// <returns>A stream containing the current and subsequent display snapshots.</returns>
    internal static IObservable<IReadOnlyList<DisplayInfo>> ObserveChangesCore(
        IObservable<WindowMessage> windowMessages,
        Func<IReadOnlyList<DisplayInfo>> snapshotProvider) =>
        (from message in windowMessages
         where message.Msg == WindowsMessages.WM_DISPLAYCHANGE
         select snapshotProvider()).StartWith(snapshotProvider());

    /// <summary>Exchanges topology sources for deterministic tests.</summary>
    /// <param name="windowMessages">The replacement message-source factory.</param>
    /// <param name="snapshotProvider">The replacement snapshot provider.</param>
    /// <returns>A scope that restores the original sources.</returns>
    internal static IDisposable ExchangeSources(
        Func<IObservable<WindowMessage>> windowMessages,
        Func<IReadOnlyList<DisplayInfo>> snapshotProvider)
    {
        Throw.IfNull(windowMessages);
        Throw.IfNull(snapshotProvider);
        var previousWindowMessages = _windowMessages;
        var previousSnapshotProvider = _snapshotProvider;
        _windowMessages = windowMessages;
        _snapshotProvider = snapshotProvider;
        return Scope.Create(
            Tuple.Create(previousWindowMessages, previousSnapshotProvider),
            static previous => (_windowMessages, _snapshotProvider) = (previous.Item1, previous.Item2));
    }
}
