// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Interop;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>Extensions for WPF Windows.</summary>
public static class WindowsExtensions
{
    /// <summary>Provides WPF window extension methods.</summary>
    /// <param name="window">The window to adapt.</param>
    extension(Window window)
    {
        /// <summary>Gets the native handle of a Window.</summary>
        public IntPtr Handle => new WindowInteropHelper(window).Handle;

        /// <summary>Factory method to create a InteropWindow for the supplied Window.</summary>
        /// <returns>InteropWindow.</returns>
        public InteropWindow AsInteropWindow() => InteropWindowFactory.CreateFor(get_Handle(window));

        /// <summary>Place the window.</summary>
        /// <param name="windowPlacement">WindowPlacement.</param>
        /// <returns>InteropWindow.</returns>
        public InteropWindow ApplyPlacement(WindowPlacement windowPlacement)
        {
            InteropWindow interopWindow = window.AsInteropWindow();
            _ = interopWindow.SetPlacement(windowPlacement);
            return interopWindow;
        }

        /// <summary>Returns the WindowPlacement.</summary>
        /// <returns>WindowPlacement.</returns>
        public WindowPlacement RetrievePlacement() => window.AsInteropWindow().GetPlacement();
    }
}
