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
        /// <summary>Blocks interactive movement of the form.</summary>
        /// <returns>An observable whose subscription controls the movement guard lifetime.</returns>
        public IObservable<RxVoid> BlockMove() => form.BlockMove(static () => false);

        /// <summary>Blocks interactive movement of the form while a dynamic condition evaluates to <see langword="false" />.</summary>
        /// <param name="allowMoveCondition">
        /// A condition evaluated for every move request. Returning <see langword="true" /> allows movement;
        /// returning <see langword="false" /> blocks it.
        /// </param>
        /// <returns>An observable whose subscription controls the movement guard lifetime.</returns>
        public IObservable<RxVoid> BlockMove(Func<bool> allowMoveCondition) =>
            CreateMovementGuard(form, allowMoveCondition, WindowsMoveBlockMode.MoveOnly);

        /// <summary>Blocks interactive movement and resizing of the form.</summary>
        /// <returns>An observable whose subscription controls the movement and resize guard lifetime.</returns>
        public IObservable<RxVoid> BlockMoveAndResize() => form.BlockMoveAndResize(static () => false);

        /// <summary>Blocks interactive movement and resizing while a dynamic condition evaluates to <see langword="false" />.</summary>
        /// <param name="allowMoveCondition">
        /// A condition evaluated for every move or resize request. Returning <see langword="true" /> allows the operation;
        /// returning <see langword="false" /> blocks it.
        /// </param>
        /// <returns>An observable whose subscription controls the movement and resize guard lifetime.</returns>
        public IObservable<RxVoid> BlockMoveAndResize(Func<bool> allowMoveCondition) =>
            CreateMovementGuard(form, allowMoveCondition, WindowsMoveBlockMode.MoveAndResize);

        /// <summary>Factory method to create a InteropWindow for the supplied WindowForm.</summary>
        /// <returns>InteropWindow.</returns>
        public InteropWindow AsInteropWindow() => InteropWindowFactory.CreateFor(form.Handle);

        /// <summary>Place the Form.</summary>
        /// <param name="windowPlacement">WindowPlacement.</param>
        /// <returns>InteropWindow.</returns>
        public InteropWindow ApplyPlacement(WindowPlacement windowPlacement)
        {
            var interopWindow = form.AsInteropWindow();
            _ = interopWindow.SetPlacement(windowPlacement);
            return interopWindow;
        }

        /// <summary>Returns the WindowPlacement.</summary>
        /// <returns>WindowPlacement.</returns>
        public WindowPlacement RetrievePlacement() => form.AsInteropWindow().GetPlacement();
    }

    /// <summary>Creates a cold, form-bound movement guard observable.</summary>
    /// <param name="form">The Windows Forms form to protect.</param>
    /// <param name="allowMoveCondition">The dynamic condition that allows protected operations.</param>
    /// <param name="blockMode">The protected interactive operations.</param>
    /// <returns>An observable whose subscription controls the movement guard lifetime.</returns>
    private static IObservable<RxVoid> CreateMovementGuard(
        Form form,
        Func<bool> allowMoveCondition,
        WindowsMoveBlockMode blockMode)
    {
        Throw.IfNull(form);
        Throw.IfNull(allowMoveCondition);
        return ReactiveSignal.CreateWithState<RxVoid, (WindowsMove Guard, Form Form)>(
            (Guard: new WindowsMove(allowMoveCondition, blockMode), Form: form),
            static (state, observer) => state.Guard.Subscribe(observer, state.Form));
    }
}
