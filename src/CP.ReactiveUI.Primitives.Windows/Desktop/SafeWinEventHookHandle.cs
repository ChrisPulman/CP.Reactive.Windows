// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Win32.SafeHandles;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>Represents a non-owning safe wrapper around a native WinEvent hook handle.</summary>
public sealed class SafeWinEventHookHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Windows.SafeWinEventHookHandle" /> class.</summary>
    private SafeWinEventHookHandle()
        : base(ownsHandle: false)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Windows.SafeWinEventHookHandle" /> class.</summary>
    /// <param name="hookHandle">The native WinEvent hook handle.</param>
    private SafeWinEventHookHandle(IntPtr hookHandle)
        : base(ownsHandle: false)
    {
        SetHandle(hookHandle);
    }

    /// <summary>Creates a non-owning safe handle for an externally-owned WinEvent hook.</summary>
    /// <param name="hookHandle">The native WinEvent hook handle.</param>
    /// <returns>The safe handle wrapper.</returns>
    public static SafeWinEventHookHandle FromUnowned(IntPtr hookHandle) => new(hookHandle);

    /// <summary>Creates an invalid non-owning safe handle.</summary>
    /// <returns>The invalid safe handle wrapper.</returns>
    internal static SafeWinEventHookHandle CreateInvalid() => new();

    /// <summary>Invokes the non-owning release path.</summary>
    /// <returns>true because the wrapper never owns the native hook handle.</returns>
    internal bool TryRelease() => ReleaseHandle();

    /// <inheritdoc />
    protected override bool ReleaseHandle() => true;
}
