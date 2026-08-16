// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles;

/// <summary>Wraps a non-owned monitor handle returned by User32 monitor enumeration.</summary>
public sealed class SafeMonitorHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles.SafeMonitorHandle" /> class.</summary>
    public SafeMonitorHandle()
        : base(ownsHandle: false) { }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles.SafeMonitorHandle" /> class.</summary>
    /// <param name="preexistingHandle">The native monitor handle.</param>
    public SafeMonitorHandle(IntPtr preexistingHandle)
        : base(ownsHandle: false)
    {
        SetHandle(preexistingHandle);
    }

    /// <summary>Invokes the non-owning release path for deterministic verification.</summary>
    /// <returns>True because monitor handles are not owned by this wrapper.</returns>
    internal bool ReleaseForTesting() => ReleaseHandle();

    /// <inheritdoc />
    protected override bool ReleaseHandle() => true;
}
