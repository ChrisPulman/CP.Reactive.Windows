// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>
///     An enum specifying the type of input event used for the SendInput call.
///     This specifies which structure type of the union supplied to SendInput is used.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646270(v=vs.85).aspx">INPUT structure</a>
/// </summary>
public enum InputTypes : uint
{
    /// <summary>The event is a mouse event.</summary>
    Mouse,
    /// <summary>The event is a keyboard event.</summary>
    Keyboard,
    /// <summary>The event is a hardware event.</summary>
    Hardware,
}
