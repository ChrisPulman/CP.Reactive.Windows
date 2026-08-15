// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;

/// <summary>Provides the active composed GDI operations for safe handles.</summary>
internal static class GdiSafeHandleApi
{
    /// <summary>Stores the active GDI implementation.</summary>
    private static IGdiSafeHandleApi _current = new GdiSafeHandleOperations(
        Gdi32Api.DeleteObject,
        Gdi32Api.SelectObjectHandle,
        Gdi32Api.RestoreObjectHandle);

    /// <summary>Gets the active GDI implementation.</summary>
    internal static IGdiSafeHandleApi Current => Volatile.Read(ref _current);

    /// <summary>Atomically replaces the active GDI implementation.</summary>
    /// <param name="replacement">The replacement implementation.</param>
    /// <returns>The previous implementation.</returns>
    internal static IGdiSafeHandleApi Swap(IGdiSafeHandleApi replacement)
    {
        Throw.IfNull(replacement);
        return Interlocked.Exchange(ref _current, replacement);
    }
}
