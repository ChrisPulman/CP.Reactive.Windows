// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using Microsoft.Win32.SafeHandles;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles;

/// <summary>Wraps a non-owned cursor handle returned by GetCursorInfo.</summary>
public sealed class SafeCursorReferenceHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles.SafeCursorReferenceHandle" /> class.</summary>
    public SafeCursorReferenceHandle()
        : base(ownsHandle: false)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.UserInterface.SafeHandles.SafeCursorReferenceHandle" /> class.</summary>
    /// <param name="preexistingHandle">The native cursor handle.</param>
    public SafeCursorReferenceHandle(IntPtr preexistingHandle)
        : base(ownsHandle: false)
    {
        SetHandle(preexistingHandle);
    }

    /// <summary>Uses the native cursor handle while this safe handle is reference-counted.</summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="handleAction">The action that uses the native handle.</param>
    /// <returns>The action result.</returns>
    public T UseNativeHandle<T>(Func<IntPtr, T> handleAction)
    {
        Throw.IfNull(handleAction);
        bool handleAcquired = false;
        try
        {
            DangerousAddRef(ref handleAcquired);
            return handleAction(handle);
        }
        finally
        {
            if (handleAcquired)
            {
                DangerousRelease();
            }
        }
    }

    /// <inheritdoc />
    protected override bool ReleaseHandle() => true;
}
