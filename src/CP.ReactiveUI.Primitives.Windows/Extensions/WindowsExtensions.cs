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

        /// <summary>Blocks interactive movement of the window.</summary>
        /// <returns>An observable whose subscription controls the movement guard lifetime.</returns>
        public IObservable<RxVoid> BlockMove() => window.BlockMove(static () => false);

        /// <summary>Blocks interactive movement of the window while a dynamic condition evaluates to <see langword="false" />.</summary>
        /// <param name="allowMoveCondition">
        /// A condition evaluated for every move request. Returning <see langword="true" /> allows movement;
        /// returning <see langword="false" /> blocks it.
        /// </param>
        /// <returns>An observable whose subscription controls the movement guard lifetime.</returns>
        public IObservable<RxVoid> BlockMove(Func<bool> allowMoveCondition) =>
            CreateMovementGuard(window, allowMoveCondition, WindowsMoveBlockMode.MoveOnly);

        /// <summary>Blocks interactive movement and resizing of the window.</summary>
        /// <returns>An observable whose subscription controls the movement and resize guard lifetime.</returns>
        public IObservable<RxVoid> BlockMoveAndResize() => window.BlockMoveAndResize(static () => false);

        /// <summary>Blocks interactive movement and resizing while a dynamic condition evaluates to <see langword="false" />.</summary>
        /// <param name="allowMoveCondition">
        /// A condition evaluated for every move or resize request. Returning <see langword="true" /> allows the operation;
        /// returning <see langword="false" /> blocks it.
        /// </param>
        /// <returns>An observable whose subscription controls the movement and resize guard lifetime.</returns>
        public IObservable<RxVoid> BlockMoveAndResize(Func<bool> allowMoveCondition) =>
            CreateMovementGuard(window, allowMoveCondition, WindowsMoveBlockMode.MoveAndResize);

        /// <summary>Factory method to create a InteropWindow for the supplied Window.</summary>
        /// <returns>InteropWindow.</returns>
        public InteropWindow AsInteropWindow() => InteropWindowFactory.CreateFor(get_Handle(window));

        /// <summary>Place the window.</summary>
        /// <param name="windowPlacement">WindowPlacement.</param>
        /// <returns>InteropWindow.</returns>
        public InteropWindow ApplyPlacement(WindowPlacement windowPlacement)
        {
            var interopWindow = window.AsInteropWindow();
            _ = interopWindow.SetPlacement(windowPlacement);
            return interopWindow;
        }

        /// <summary>Returns the WindowPlacement.</summary>
        /// <returns>WindowPlacement.</returns>
        public WindowPlacement RetrievePlacement() => window.AsInteropWindow().GetPlacement();
    }

    /// <summary>Creates a cold, window-bound movement guard observable.</summary>
    /// <param name="window">The WPF window to protect.</param>
    /// <param name="allowMoveCondition">The dynamic condition that allows protected operations.</param>
    /// <param name="blockMode">The protected interactive operations.</param>
    /// <returns>An observable whose subscription controls the movement guard lifetime.</returns>
    private static IObservable<RxVoid> CreateMovementGuard(
        Window window,
        Func<bool> allowMoveCondition,
        WindowsMoveBlockMode blockMode)
    {
        Throw.IfNull(window);
        Throw.IfNull(allowMoveCondition);
        return ReactiveSignal.CreateWithState<RxVoid, (WindowsMove Guard, Window Window)>(
            (Guard: new WindowsMove(allowMoveCondition, blockMode), Window: window),
            static (state, observer) => state.Guard.Subscribe(observer, state.Window));
    }
}
