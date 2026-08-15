// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;

/// <summary>Provides a safe handle for a DIB section.</summary>
public class SafeDibSectionHandle : SafeObjectHandle
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeDibSectionHandle" /> class.</summary>
    public SafeDibSectionHandle()
        : base(ownsHandle: true)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeDibSectionHandle" /> class from an existing handle.</summary>
    /// <param name="preexistingHandle">The existing DIB section handle.</param>
    public SafeDibSectionHandle(IntPtr preexistingHandle)
        : base(ownsHandle: true)
    {
        SetHandle(preexistingHandle);
    }
}
