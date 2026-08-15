// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>Extensions for Forms.</summary>
public static class FormsExtensions
{
    /// <summary>Provides Windows Forms extension methods.</summary>
    /// <param name="form">The form to adapt.</param>
    extension(Form form)
    {
        /// <summary>Factory method to create a InteropWindow for the supplied WindowForm.</summary>
        /// <returns>InteropWindow.</returns>
        public InteropWindow AsInteropWindow() => InteropWindowFactory.CreateFor(form.Handle);

        /// <summary>Place the Form.</summary>
        /// <param name="windowPlacement">WindowPlacement.</param>
        /// <returns>InteropWindow.</returns>
        public InteropWindow ApplyPlacement(WindowPlacement windowPlacement)
        {
            InteropWindow interopWindow = form.AsInteropWindow();
            _ = interopWindow.SetPlacement(windowPlacement);
            return interopWindow;
        }

        /// <summary>Returns the WindowPlacement.</summary>
        /// <returns>WindowPlacement.</returns>
        public WindowPlacement RetrievePlacement() => form.AsInteropWindow().GetPlacement();
    }
}
