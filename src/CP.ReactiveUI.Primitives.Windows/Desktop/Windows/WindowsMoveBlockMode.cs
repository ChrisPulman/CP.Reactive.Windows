// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>Specifies which interactive window operations a <see cref="WindowsMove" /> guard blocks.</summary>
public enum WindowsMoveBlockMode
{
    /// <summary>Blocks interactive movement while leaving resizing available.</summary>
    MoveOnly,

    /// <summary>Blocks both interactive movement and resizing.</summary>
    MoveAndResize,
}
