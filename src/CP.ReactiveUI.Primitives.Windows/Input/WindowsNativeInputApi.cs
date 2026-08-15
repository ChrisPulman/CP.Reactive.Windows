// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using ReactiveUI.Primitives.Disposables;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Production Win32 input API implementation.</summary>
internal sealed class WindowsNativeInputApi : INativeInputApi
{
    /// <summary>Defines a last-input native operation.</summary>
    /// <param name="lastInputInfo">The last-input info to fill.</param>
    /// <returns><see langword="true" /> when the operation succeeds.</returns>
    internal delegate bool GetLastInputInfoOperation(ref LastInputInfo lastInputInfo);

    /// <summary>Defines a send-input native operation.</summary>
    /// <param name="numberOfInputs">The number of inputs.</param>
    /// <param name="inputs">The input records.</param>
    /// <param name="inputSize">The native input record size.</param>
    /// <returns>The number of input records sent.</returns>
    internal delegate uint SendInputOperation(uint numberOfInputs, DesktopInput[] inputs, int inputSize);

    /// <summary>Contains native input entry points.</summary>
    private static class NativeMethods
    {
        /// <summary>Gets the last input information.</summary>
        /// <param name="lastInputInfo">Last input information.</param>
        /// <returns><see langword="true" /> when the native call succeeds.</returns>
        [DllImport("User32")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetLastInputInfo(ref LastInputInfo lastInputInfo);

        /// <summary>Synthesizes keyboard and mouse input events.</summary>
        /// <param name="numberOfInputs">The number of input records.</param>
        /// <param name="inputs">The input records.</param>
        /// <param name="inputSize">The native input record size.</param>
        /// <returns>The number of input records successfully inserted into the input stream.</returns>
        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern uint SendInput(uint numberOfInputs, [In][MarshalAs(UnmanagedType.LPArray)] DesktopInput[] inputs, int inputSize);
    }

    /// <summary>Stores native input operations.</summary>
    /// <param name="getLastInputInfo">The last-input operation.</param>
    /// <param name="sendInput">The send-input operation.</param>
    private sealed class NativeInputOperations(GetLastInputInfoOperation getLastInputInfo, SendInputOperation sendInput)
    {
        /// <summary>Gets the last input information.</summary>
        /// <param name="lastInputInfo">Last input information.</param>
        /// <returns><see langword="true" /> when the native call succeeds.</returns>
        public bool GetLastInputInfo(ref LastInputInfo lastInputInfo) => getLastInputInfo(ref lastInputInfo);

        /// <summary>Synthesizes keyboard and mouse input events.</summary>
        /// <param name="numberOfInputs">The number of input records.</param>
        /// <param name="inputs">The input records.</param>
        /// <param name="inputSize">The native input record size.</param>
        /// <returns>The number of input records successfully inserted into the input stream.</returns>
        public uint SendInput(uint numberOfInputs, DesktopInput[] inputs, int inputSize) => sendInput(numberOfInputs, inputs, inputSize);
    }

    /// <summary>The singleton instance.</summary>
    internal static readonly WindowsNativeInputApi Instance = new();

    /// <summary>The active native operations.</summary>
    private static NativeInputOperations _operations = new(NativeMethods.GetLastInputInfo, NativeMethods.SendInput);

    /// <inheritdoc />
    public bool GetLastInputInfo(ref LastInputInfo lastInputInfo) => _operations.GetLastInputInfo(ref lastInputInfo);

    /// <inheritdoc />
    public uint SendInput(DesktopInput[] inputs) => _operations.SendInput(checked((uint)inputs.Length), inputs, DesktopInput.Size);

    /// <summary>Overrides the native input operations for deterministic tests.</summary>
    /// <param name="getLastInputInfo">The replacement last-input operation.</param>
    /// <param name="sendInput">The replacement send-input operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(GetLastInputInfoOperation getLastInputInfo, SendInputOperation sendInput)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(getLastInputInfo);
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(sendInput);
        NativeInputOperations operations = _operations;
        _operations = new(getLastInputInfo, sendInput);
        return Scope.Create(operations, delegate(NativeInputOperations operations2)
        {
            _operations = operations2;
        });
    }
}
