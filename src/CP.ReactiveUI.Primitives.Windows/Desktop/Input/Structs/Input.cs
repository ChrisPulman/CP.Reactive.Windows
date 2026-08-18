// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     A struct used by SendInput to store information for synthesizing input events such as keystrokes, mouse movement,
///     and mouse clicks.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646270(v=vs.85).aspx">INPUT structure</a>
/// </summary>
public readonly record struct Input
{
    /// <summary>Gets used as the Size in the SendInput call.</summary>
    public static int Size => Marshal.SizeOf<Input>();

    /// <summary>Gets the type of the input event. This member can be one of the following values.</summary>
    public InputTypes InputType { get; init; }

    /// <summary>Gets a union which contains the MouseInput, KeyboardInput or HardwareInput.</summary>
    public InputUnion InputUnion { get; init; }

    /// <summary>A factory method to simplify creating mouse input.</summary>
    /// <param name="mouseInputs">The mouse input records.</param>
    /// <returns>Array of Input structs.</returns>
    public static Input[] CreateMouseInputs(params MouseInput[] mouseInputs)
    {
        Input[] result = new Input[mouseInputs.Length];
        var index = 0;
        foreach (var mouseInput in mouseInputs)
        {
            result[index] = new Input { InputType = InputTypes.Mouse, InputUnion = new InputUnion { MouseInput = mouseInput } };
            index = checked(index + 1);
        }

        return result;
    }

    /// <summary>A factory method to simplify creating input.</summary>
    /// <param name="keyboardInputs">The keyboard input records.</param>
    /// <returns>Array of Input structs.</returns>
    public static Input[] CreateKeyboardInputs(params KeyboardInput[] keyboardInputs)
    {
        Input[] result = new Input[keyboardInputs.Length];
        var index = 0;
        foreach (var keyboardInput in keyboardInputs)
        {
            result[index] = new Input { InputType = InputTypes.Keyboard, InputUnion = new InputUnion { KeyboardInput = keyboardInput } };
            index = checked(index + 1);
        }

        return result;
    }
}
