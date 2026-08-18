// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     This struct is passed in the WH_MOUSE_LL hook
///     See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms644970.aspx.
/// </summary>
public readonly record struct MouseLowLevelHookStruct
{
    /// <summary>Gets the x- and y-coordinates of the cursor, in per-monitor-aware screen coordinates.</summary>
    public NativePoint Pt { get; init; }

    /// <summary>
    ///     Gets the mouse data associated with wheel and X-button messages. If the message is WM_MOUSEWHEEL,
    ///     the high-order word of this member is the wheel delta.
    ///     The low-order word is reserved. A positive value indicates that the wheel was rotated forward, away from the user;
    ///     a negative value indicates that the wheel was rotated backward, toward the user.
    ///     One wheel click is defined as WHEEL_DELTA, which is 120.
    ///     If the message is WM_XBUTTONDOWN, WM_XBUTTONUP, WM_XBUTTONDBLCLK, WM_NCXBUTTONDOWN, WM_NCXBUTTONUP, or
    ///     WM_NCXBUTTONDBLCLK,
    ///     the high-order word specifies which X button was pressed or released, and the low-order word is reserved.
    ///     This value can be one or more of the following values.
    ///     Otherwise, mouseData is not used.
    /// </summary>
    internal uint MouseData { get; init; }

    /// <summary>Gets the event-injected flags.</summary>
    internal ExtendedMouseFlags Flags { get; init; }

    /// <summary>Gets the time stamp for this message.</summary>
    internal uint TimeStamp { get; init; }

    /// <summary>Gets additional native information associated with the message.</summary>
    internal UIntPtr ExtraInfo { get; init; }
}
