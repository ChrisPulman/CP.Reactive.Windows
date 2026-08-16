// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>Factory for InteropWindows.</summary>
public static class InteropWindowFactory
{
    /// <summary>Factory method to create a InteropWindow for the supplied handle.</summary>
    /// <param name="handle">int.</param>
    /// <returns>InteropWindow.</returns>
    public static InteropWindow CreateFor(int handle) => CreateFor(new IntPtr(handle));

    /// <summary>Factory method to create a InteropWindow for the supplied handle.</summary>
    /// <param name="handle">IntPtr.</param>
    /// <returns>InteropWindow.</returns>
    public static InteropWindow CreateFor(IntPtr handle) => new(handle);
}
